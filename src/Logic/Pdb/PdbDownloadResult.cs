namespace Microsoft.PdbDownloader.Logic.Pdb
{
    using System.Net;

    internal class PdbDownloadResult
    {
        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        internal HttpStatusCode ResponseStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the response buffer.
        /// </summary>
        internal byte[] ResponseBuffer
        {
            get;
            set;
        }
    }
}
