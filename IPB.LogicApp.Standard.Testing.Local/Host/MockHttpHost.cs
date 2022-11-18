namespace IPB.LogicApp.Standard.Testing.Local.Host
{
    using System;
    using WireMock.Logging;
    using WireMock.Server;
    using WireMock.Settings;

    /// <summary>
    /// The mock HTTP host.
    /// </summary>
    public class MockHttpHost : IDisposable
    {
        private const int DefaultPort = 7075;

        public WireMockServer Server { get; set; }

        public string HostUri { get; }


        public MockHttpHost() : this(DefaultPort)
        {
            
        }

        public MockHttpHost(int port)
        {
            var settings = new WireMockServerSettings();
            settings.Port = port;
            settings.MaxRequestLogCount = 100;
            settings.Logger = new WireMockConsoleLogger();
            Server = WireMockServer.Start(settings);

            HostUri = (new UriBuilder(Uri.UriSchemeHttp, Environment.MachineName, port).Uri.ToString()).TrimEnd('/');
        }

        /// <summary>
        /// Disposes the resources.
        /// </summary>
        public void Dispose()
        {
            Server.Stop();
        }

    }
}