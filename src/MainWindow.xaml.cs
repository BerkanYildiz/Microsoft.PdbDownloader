namespace Microsoft.PdbDownloader
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using Microsoft.PdbDownloader.Logic.Pdb;
    using Microsoft.PdbDownloader.Logic.Pe;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void OnDragEnter(object Sender, DragEventArgs Args)
        {
            // Debug.WriteLine("[*] OnDragEnter(...) has been called.");
            // Debug.WriteLine("[*] OnDragEnter->Formats: " + string.Join(", ", Args.Data.GetFormats()));

            this.DragAndDropBorder.Opacity = 1.0d;
        }

        private void OnDragLeave(object Sender, DragEventArgs Args)
        {
            // Debug.WriteLine("[*] OnDragLeave(...) has been called.");
            // Debug.WriteLine("[*] OnDragLeave->Formats: " + string.Join(", ", Args.Data.GetFormats()));

            this.DragAndDropBorder.Opacity = 0.0d;
        }

        private void OnDrop(object Sender, DragEventArgs Args)
        {
            Debug.WriteLine("[*] OnDrop(...) has been called.");
            Debug.WriteLine("[*] OnDrop->Formats: " + string.Join(", ", Args.Data.GetFormats()));
            this.DragAndDropBorder.Opacity = 0.0d;

            // 
            // Verify if this is a file being dropped.
            // 

            if (!Args.Data.GetDataPresent("FileDrop"))
            {
                return;
            }

            // 
            // Retrieve the full path of the file.
            // 

            var FilePath = (string) null;

            try
            {
                var Metadatas = (string[]) Args.Data.GetData("FileNameW");

                if (Metadatas != null && Metadatas.Length > 0)
                {
                    FilePath = Metadatas[0];

                    if (string.IsNullOrEmpty(FilePath))
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            // 
            // Make sure the file exist on disk.
            // 

            if (!File.Exists(FilePath))
            {
                return;
            }

            FilePath = FilePath.ToLower();

            // 
            // Make sure this is an executable file.
            // 

            var AuthorizedFormats = new []
            {
                ".exe", ".dll", ".sys"
            };

            if (!AuthorizedFormats.Any(FilePath.Contains))
            {
                Debug.WriteLine("[*] The file is not an executable.");
                return;
            }

            // 
            // Start a new task to avoid freezing the UI.
            // 

            Task.Run(async () =>
            {
                // 
                // Read the file content.
                // 

                var FileBuffer = (byte[]) null;

                using (var FileStream = File.OpenRead(FilePath))
                {
                    FileBuffer = new byte[FileStream.Length];

                    try
                    {
                        // 
                        // Read the entire file at once, I can't be bothered.
                        // 

                        var NumberOfBytesRead = await FileStream.ReadAsync(FileBuffer, 0, FileBuffer.Length);

                        if (NumberOfBytesRead != FileBuffer.Length)
                        {
                            FileBuffer = null;
                        }
                    }
                    catch (Exception)
                    {
                        FileBuffer = null;
                    }
                }

                // 
                // Verify if the file has been read.
                // 

                if (FileBuffer == null)
                {
                    Debug.WriteLine("[*] Failed to read the file.");
                    return;
                }

                // 
                // Parse the PDB metadatas from the file buffer.
                // 

                if (!PeUtils.TryGetPdbMetadata(FileBuffer, out var PdbName, out var PdbHash))
                {
                    Debug.WriteLine("[*] Failed to parse the PDB metadatas from the file buffer.");
                    return;
                }

                Debug.WriteLine("[*] PdbName: " + PdbName);
                Debug.WriteLine("[*] PdbHash: " + PdbHash);

                // 
                // Try to download the PDB using those informations.
                // 

                var PdbResult = (PdbDownloadResult) null;

                try
                {
                    PdbResult = await PdbDownloader.TryDownloadPdbFile(PdbName, PdbHash);
                }
                catch (Exception)
                {
                    // ..
                }

                // 
                // Verify if we correctly received the data.
                // 

                if (PdbResult == null || PdbResult.ResponseBuffer == null)
                {
                    Debug.WriteLine("[*] Failed to download the PDB file.");
                    return;
                }

                // 
                // We've downloaded the PDB file, save it somewhere.
                // 

                var InputDirectoryPath = Path.GetDirectoryName(FilePath);
                var PdbFilePath = Path.Combine(InputDirectoryPath, PdbName);

                try
                {
                    File.WriteAllBytes(PdbFilePath, PdbResult.ResponseBuffer);
                }
                catch (Exception)
                {
                    return;
                }

                // 
                // Success!
                // 

                Debug.WriteLine("[*] We've downloaded and saved the PDB file!");
            });
        }
    }
}
