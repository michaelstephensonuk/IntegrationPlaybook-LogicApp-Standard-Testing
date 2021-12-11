using System;
using System.Collections.Generic;
using System.Text;

namespace IPB.LogicApp.Standard.Testing.Helpers
{
    public class ApiSettings
    {
        public const string BaseUrl = "https://management.azure.com";

        /// <summary>
        /// If the management api changes version this would need changing
        /// </summary>
        public const string ApiVersion = "2018-11-01";
    }
}
