// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace IPB.LogicApp.Standard.Testing.Local.Host
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The function test host.
    /// </summary>
    public class WorkflowTestHost : IDisposable
    {

        private TextWriterTraceListener _traceListener;

        /// <summary>
        /// If this is set to true then we will add the output from func.exe
        /// to the trace output so it will show in your test output.  This could be handy
        /// for troubleshooting on a build agent but it could be quite a bit of info
        /// so you might want to set it to false and just use the log written in the test folder
        /// so it doesnt give you big verbose test results
        /// </summary>
        public bool WriteFuncOutputToDebugTrace { get; set; }
        /// <summary>
        /// Get or sets the Output Data.
        /// </summary>
        public List<string> OutputData { get; private set; }

        /// <summary>
        /// Gets or sets the error data.
        /// </summary>
        public List<string> ErrorData { get; private set; }

        /// <summary>
        /// Gets or sets the Function process.
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        /// Gets or sets the Working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTestHost"/> class.
        /// </summary>
        public WorkflowTestHost(WorkflowTestInput[] inputs = null, string localSettings = null, string parameters = null, string connectionDetails = null, string host = null, DirectoryInfo artifactsDirectory = null, bool writeFuncOutputToDebugTrace = false)
        {
            var baseTestDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Temp-LogicApp-Tests");
            if (Directory.Exists(baseTestDirectory) == false)
                Directory.CreateDirectory(baseTestDirectory);

            WorkingDirectory = Path.Combine(baseTestDirectory, Guid.NewGuid().ToString());

            OutputData = new List<string>();
            ErrorData = new List<string>();

            _traceListener = new TextWriterTraceListener($"{WorkingDirectory}\\func.log.txt");
            WriteFuncOutputToDebugTrace = writeFuncOutputToDebugTrace;

            StartFunctionRuntime(inputs, localSettings, parameters, connectionDetails, host, artifactsDirectory);
        }

        /// <summary>
        /// Starts the function runtime.
        /// </summary>
        protected void StartFunctionRuntime(WorkflowTestInput[] inputs, string localSettings, string parameters, string connectionDetails, string host, DirectoryInfo artifactsDirectory)
        {
            try
            {
                var processes = Process.GetProcessesByName("func");
                foreach (var process in processes)
                {
                    process.Kill();
                }

                Directory.CreateDirectory(WorkingDirectory);

                if (inputs != null && inputs.Length > 0)
                {
                    foreach (var input in inputs)
                    {
                        if (!string.IsNullOrEmpty(input.FunctionName))
                        {
                            Directory.CreateDirectory(Path.Combine(WorkingDirectory, input.FunctionName));
                            File.WriteAllText(Path.Combine(WorkingDirectory, input.FunctionName, input.Filename), input.FlowDefinition);
                        }
                    }
                }

                if (artifactsDirectory != null)
                {
                    if (!artifactsDirectory.Exists)
                    {
                        throw new DirectoryNotFoundException(artifactsDirectory.FullName);
                    }

                    var artifactsWorkingDirectory = Path.Combine(WorkingDirectory, "Artifacts");
                    Directory.CreateDirectory(artifactsWorkingDirectory);
                    CopyDirectory(source: artifactsDirectory, destination: new DirectoryInfo(artifactsWorkingDirectory));
                }

                if (!string.IsNullOrEmpty(parameters))
                {
                    File.WriteAllText(Path.Combine(WorkingDirectory, "parameters.json"), parameters);
                }

                if (!string.IsNullOrEmpty(connectionDetails))
                {
                    File.WriteAllText(Path.Combine(WorkingDirectory, "connections.json"), connectionDetails);
                }

                if (!string.IsNullOrEmpty(localSettings))
                {
                    File.WriteAllText(Path.Combine(WorkingDirectory, "local.settings.json"), localSettings);
                }
                else
                {
                    File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestFiles\\local.settings.json"), Path.Combine(WorkingDirectory, "local.settings.json"));
                }

                if (!string.IsNullOrEmpty(host))
                {
                    File.WriteAllText(Path.Combine(WorkingDirectory, "host.json"), host);
                }
                else
                {
                    File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestFiles\\host.json"), Path.Combine(WorkingDirectory, "host.json"));
                }

                ListDirectoryContents(WorkingDirectory);

                var funcExePath = FuncHelper.GetFuncPath();
                Console.WriteLine($"Starting Function Runtime: {funcExePath}");
                Process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = WorkingDirectory,
                        FileName = funcExePath,
                        Arguments = "start --verbose",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Normal
                    }
                };

                var processStarted = new TaskCompletionSource<bool>();

                Process.OutputDataReceived += (sender, args) =>
                {
                    var outputData = args.Data;
                    if (outputData != null && outputData.Contains("Host started") && !processStarted.Task.IsCompleted)
                    {
                        processStarted.SetResult(true);
                    }

                    lock (this)
                    {
                        //Write to the debug so it will show in the test results
                        if (WriteFuncOutputToDebugTrace)
                            Debug.WriteLine(args.Data);

                        //This will write to the log file in the temp test folder
                        _traceListener.WriteLine(args.Data);
                        OutputData.Add(args.Data);
                    }
                };

                var errorData = string.Empty;
                Process.ErrorDataReceived += (sender, args) =>
                {
                    errorData = args.Data;

                    lock (this)
                    {
                        //Write to the debug so it will show in the test results
                        if (WriteFuncOutputToDebugTrace)
                            Debug.WriteLine(args.Data);

                        //This will write to the log file in the temp test folder
                        _traceListener.WriteLine(args.Data);
                        ErrorData.Add(args.Data);
                    }
                };

                Process.Start();

                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();

                var result = Task.WhenAny(processStarted.Task, Task.Delay(TimeSpan.FromMinutes(2))).Result;

                if (result != processStarted.Task)
                {
                    throw new InvalidOperationException("Runtime did not start properly. Please make sure you have the latest Azure Functions Core Tools installed and available on your PATH environment variable, and that Azurite is up and running.");
                }

                if (Process.HasExited)
                {
                    throw new InvalidOperationException($"Runtime did not start properly. The error is '{errorData}'. Please make sure you have the latest Azure Functions Core Tools installed and available on your PATH environment variable, and that Azurite is up and running.");
                }

            }
            catch (Exception ex)
            {
                _traceListener.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
                Directory.Delete(WorkingDirectory, recursive: true);

                throw;
            }
        }

        /// <summary>
        /// Copies the directory.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        protected static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            if (!destination.Exists)
            {
                destination.Create();
            }

            // Copy all files.
            var files = source.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name));
            }

            // Process subdirectories.
            var dirs = source.GetDirectories();
            foreach (var dir in dirs)
            {
                // Get destination directory.
                var destinationDir = Path.Combine(destination.FullName, dir.Name);

                // Call CopyDirectory() recursively.
                CopyDirectory(dir, new DirectoryInfo(destinationDir));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_traceListener != null)
                    _traceListener.Flush();

                Process?.Close();
            }
            finally
            {
                var i = 0;
                while (i < 5)
                {
                    try
                    {
                        //Note: Ill review this and see if it makes sense to clean up these folders
                        //VS clean should do it anyway and leaving files there will help troubleshooting
                        //Directory.Delete(WorkingDirectory, recursive: true);
                        break;
                    }
                    catch
                    {
                        i++;
                        Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                    }
                }
            }
        }


        public static void ListDirectoryContents(string path)
        {
            Console.WriteLine("Listing files in directory");
            Console.WriteLine($"Directory: {path}");
            var filePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var filePath in filePaths)
            {
                Console.WriteLine(filePath);
            }
        }
    }
}
