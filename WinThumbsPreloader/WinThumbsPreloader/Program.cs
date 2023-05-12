namespace WinThumbsPreloader
{
    static class Program
    {

        [STAThread]
        static void Main(string[] arguments)
        {

            Options options = new Options(arguments);
            if (options.badArguments)
            {
                Application.EnableVisualStyles();
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AboutForm());
            }
            else
            {
                new ThumbnailsPreloader(options.path, options.includeNestedDirectories, options.silentMode);
                Application.Run();
            }
        }
    }
}
