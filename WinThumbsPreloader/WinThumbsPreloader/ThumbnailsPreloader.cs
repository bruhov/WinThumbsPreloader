namespace WinThumbsPreloader
{
    public enum ThumbnailsPreloaderState
    {
        New,
        GettingNumberOfItems,
        Processing,
        Canceled,
        Done
    }

    //Preload all thumbnails, show progress dialog
    class ThumbnailsPreloader
    {
        private DirectoryScanner directoryScanner;
        private ProgressDialog progressDialog;
        private Timer progressDialogUpdateTimer;

        public ThumbnailsPreloaderState state = ThumbnailsPreloaderState.GettingNumberOfItems;
        public ThumbnailsPreloaderState prevState = ThumbnailsPreloaderState.New;
        public int totalItemsCount = 0;
        public int processedItemsCount = 0;
        public string currentFile = "";

        public ThumbnailsPreloader(string path, bool includeNestedDirectories, bool silentMode)
        {
            directoryScanner = new DirectoryScanner(path, includeNestedDirectories);
            if (!silentMode)
            {
                InitProgressDialog();
                InitProgressDialogUpdateTimer();
            }
            Run();
        }

        private void InitProgressDialog()
        {
            progressDialog = new ProgressDialog();
            progressDialog.AutoClose = false;
            progressDialog.ShowTimeRemaining = false;
            progressDialog.Title = "WinThumbsPreloader";
            progressDialog.CancelMessage = Resources.ThumbnailsPreloader_CancelMessage;
            progressDialog.Maximum = 100;
            progressDialog.Value = 0;
            progressDialog.Show();
            UpdateProgressDialog(null, null);
        }

        private void InitProgressDialogUpdateTimer()
        {
            progressDialogUpdateTimer = new System.Windows.Forms.Timer();
            progressDialogUpdateTimer.Interval = 250;
            progressDialogUpdateTimer.Tick += new EventHandler(UpdateProgressDialog);
            progressDialogUpdateTimer.Start();
        }

        private void UpdateProgressDialog(object sender, EventArgs e)
        {
            if (progressDialog.HasUserCancelled)
            {
                state = ThumbnailsPreloaderState.Canceled;
            }
            else if (state == ThumbnailsPreloaderState.GettingNumberOfItems)
            {
                if (prevState != state)
                {
                    prevState = state;
                    progressDialog.Line1 = Resources.ThumbnailsPreloader_PreloadingThumbnails;
                    progressDialog.Line3 = Resources.ThumbnailsPreloader_CalculatingNumberOfItems;
                    progressDialog.Marquee = true;
                }
                progressDialog.Line2 = String.Format(Resources.ThumbnailsPreloader_Discovered0Items, totalItemsCount);
            }
            else if (state == ThumbnailsPreloaderState.Processing)
            {
                if (prevState != state)
                {
                    prevState = state;
                    progressDialog.Line1 = String.Format(Resources.ThumbnailsPreloader_PreloadingThumbnailsFor0Items, totalItemsCount);
                    progressDialog.Maximum = totalItemsCount;
                    progressDialog.Marquee = false;
                }
                progressDialog.Title = String.Format(Resources.ThumbnailsPreloader_Processing, (processedItemsCount * 100) / totalItemsCount);
                progressDialog.Line2 = Resources.ThumbnailsPreloader_Name + ": " + Path.GetFileName(currentFile);
                progressDialog.Line3 = String.Format(Resources.ThumbnailsPreloader_ItemsRemaining, totalItemsCount - processedItemsCount);
                progressDialog.Value = processedItemsCount;
            }
        }

        private async void Run()
        {
            await Task.Run(() =>
            {
                //Get total items count
                state = ThumbnailsPreloaderState.GettingNumberOfItems;
                foreach (int itemsCount in directoryScanner.GetItemsCount())
                {
                    totalItemsCount += itemsCount;
                    if (state == ThumbnailsPreloaderState.Canceled) return;
                }
                if (totalItemsCount == 0)
                {
                    state = ThumbnailsPreloaderState.Done;
                    return;
                }
                //Start processing
                state = ThumbnailsPreloaderState.Processing;
                ThumbnailPreloader thumbnailPreloader = new ThumbnailPreloader();
                foreach (string item in directoryScanner.GetItems())
                {
                    currentFile = item;
                    thumbnailPreloader.PreloadThumbnail(item);
                    processedItemsCount++;
                    if (processedItemsCount == totalItemsCount) state = ThumbnailsPreloaderState.Done;
                    if (state == ThumbnailsPreloaderState.Canceled) return;
                }
            });
            Application.Exit();
        }
    }
}
