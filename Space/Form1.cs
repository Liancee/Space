using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Space
{
    public partial class Form1 : Form
    {
        private double FreedSpace { get; set; }
        public Form1()
        {
            InitializeComponent();
            this.CenterToScreen();
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.MediumPurple;
            this.pictureBox1.BackColor = Color.MediumPurple;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.sparklingheart60x60;

            FreedSpace = new BinSize().GetBinSize();
            // empty bin
            SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlag.SHERB_NOSOUND | RecycleFlag.SHERB_NOCONFIRMATION);

            FreedSpace += GetDirectorySize(KnownFolders.GetPath(KnownFolder.Downloads));
            // empty downloads
            //Empty(new DirectoryInfo(KnownFolders.GetPath(KnownFolder.Downloads)));

            if (checkBox1.Checked)
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    FindVsSolutions(drive.Name);
                }
                ShowResultAndExit();
            }
            else ShowResultAndExit();
        }

        [DllImport("Shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlag dwFlags);

        enum RecycleFlag : int
        {
            SHERB_NOCONFIRMATION = 0x00000001, // No confirmation, when emptying
            SHERB_NOPROGRESSUI = 0x00000001, // No progress tracking window during the emptying of the recycle bin
            SHERB_NOSOUND = 0x00000004 // No sound when the emptying of the recycle bin is complete
        }

        private static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        public void Empty(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private void FindVsSolutions(string driveName)
        {
            var dirInfo = new DirectoryInfo(driveName);
            foreach (var file in dirInfo.GetFiles())
            {
                if (file.Extension != ".sln")
                    return;
                DeleteBinObj(file);
            }
        }

        private void DeleteBinObj(FileInfo file)
        {
            foreach (var dir in file.Directory.GetDirectories())
            {
                if (dir.Name != "bin" || dir.Name != "obj")
                    return;
                FreedSpace += GetDirectorySize(dir.FullName);
                dir.Delete(true);
            }
        }

        private void ShowResultAndExit()
        {
            MessageBox.Show(this, $"{SizeSuffix((Int64)FreedSpace),2} were deleted.", "Freed disk space", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        public static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.redheart60x60;
        }
        

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.blackheart60x60;
        }
    }
}
