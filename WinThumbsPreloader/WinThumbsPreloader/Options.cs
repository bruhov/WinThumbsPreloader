using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinThumbsPreloader
{
    class Options
    {
        public bool badArguments;
        public bool includeNestedDirectories;
        public bool silentMode;
        public string path;

        public Options(string[] arguments)
        {
            foreach (var arg in arguments)
            {
                switch (arg)
                {
                    case "-r":
                        includeNestedDirectories = true;
                        break;

                    case "-s":
                        silentMode = true;
                        break;

                    default:
                        path = arg;
                        continue;
                }
            }

            badArguments = !Directory.Exists(path);
            if (badArguments) return;
        }
    }
}
