namespace Microsoft.PdbDownloader.Logic.Pdb
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static class PdbDownloader
    {
        /// <summary>
        /// The URL to the Microsoft Symbols Server, where the PDB files are hosted.
        /// </summary>
        internal const string MicrosoftSymbolsUrl = "https://msdl.microsoft.com/download/symbols";

        /// <summary>
        /// Tries to retrieve and download the PDB file for the given module.
        /// </summary>
        /// <param name="FileName">The name of the file.</param>
        /// <param name="Hash">The debug hash of the file.</param>
        internal static async Task<PdbDownloadResult> TryDownloadPdbFile(string FileName, string Hash)
        {
            // 
            // Verify the passed parameters.
            // 

            if (string.IsNullOrEmpty(FileName))
            {
                throw new ArgumentNullException(nameof(FileName), "The filename is null or empty.");
            }

            if (string.IsNullOrEmpty(Hash))
            {
                throw new ArgumentNullException(nameof(Hash), "The hash is null or empty.");
            }

            // 
            // Format the parameters correctly.
            // 

            if (FileName.Contains("."))
            {
                FileName = FileName.Split('.')[0];
            }

            if (Hash.Length == 32)
            {
                // 
                // The hash must contain the "age" of the file at the end.
                // 

                Hash += "1";
            }

            // 
            // Initialize the returned structure.
            // 

            var ReturnedResult = new PdbDownloadResult();

            // 
            // Try to download the PDB file from the Microsoft Symbols Server.
            // 

            using (var HttpClient = new HttpClient())
            {
                var HttpResponse = await HttpClient.GetAsync($"{MicrosoftSymbolsUrl}/{FileName}.pdb/{Hash}/{FileName}.pdb");

                using (HttpResponse)
                {
                    // 
                    // Set the result to the returned structure.
                    // 

                    ReturnedResult.ResponseStatus = HttpResponse.StatusCode;

                    if (HttpResponse.IsSuccessStatusCode)
                    {
                        ReturnedResult.ResponseBuffer = await HttpResponse.Content.ReadAsByteArrayAsync();
                    }
                }
            }

            return ReturnedResult;
        }
    }
}
