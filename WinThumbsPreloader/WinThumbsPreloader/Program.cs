using System;
using System.IO;
using System.Windows.Forms;

namespace WinThumbsPreloader
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] arguments)
        {
            /*
            //Test culture
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            */
            Options options = new Options(arguments);
            if (options.badArguments)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AboutForm());
            }
            else
            {
                new ThumbnailsPreloader(options.path, options.includeNestedDirectories, options.silentMode, options.multithreaded);
                Application.Run();
            }
        }
    }
}
