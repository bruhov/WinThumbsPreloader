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
            badArguments = (arguments.Length == 0 || arguments.Length > 2);
            if (badArguments) return;

            bool optionsProvided = (arguments.Length == 2);
            string rawOptions = (optionsProvided ? arguments[0] : "");
            path = arguments[optionsProvided ? 1 : 0];

            badArguments = !Directory.Exists(path);
            if (badArguments) return;

            includeNestedDirectories = rawOptions.Contains("r");
            silentMode = rawOptions.Contains("s");
        }
    }
}
