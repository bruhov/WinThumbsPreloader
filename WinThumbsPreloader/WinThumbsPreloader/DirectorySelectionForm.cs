
namespace WinThumbsPreloader
{
    public partial class DirectorySelectionForm : Form
    {
        public DirectorySelectionForm()
        {
            InitializeComponent();
        }

        private void selectDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            string folderName = folderBrowserDialog1.SelectedPath;

            string lastFolderName = Path.GetFullPath(folderName);
            listBox1.Items.Add(lastFolderName);
        }
    }
}
