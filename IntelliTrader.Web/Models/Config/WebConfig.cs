using IntelliTrader.Core;

namespace IntelliTrader.Web
{
    internal class WebConfig : IWebConfig
    {
        /// <summary>
        /// Enable / disable debug mode
        /// </summary>
        public bool DebugMode { get; set; } = true;

        /// <summary>
        /// Enable / disable web interface
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Port on which to host the web interface
        /// </summary>
        public int Port { get; set; } = 9696;

        /// <summary>
        /// Enable / disable read only mode
        /// </summary>
        public bool ReadOnlyMode { get; set; } = false;

        /// <summary>
        /// Certificate password
        /// </summary>
        public string SSLCertPassword { get; set; } = "certpass";

        /// <summary>
        /// Path to the SSL certificate
        /// </summary>
        public string SSLCertPath { get; set; } = "data/cert.pfx";

        /// <summary>
        /// Enable SSL for web interface
        /// </summary>
        public bool SSLEnabled { get; set; } = false;
    }
}