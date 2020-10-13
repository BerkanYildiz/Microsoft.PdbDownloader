namespace Microsoft.PdbDownloader
{
    using System.Diagnostics;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            Debug.WriteLine("[*] The application has initialized.");
        }
    }
}
