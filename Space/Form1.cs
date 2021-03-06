﻿using System;
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
        private Bitmap LastBitmap { get; set; }
        public Form1()
        {
            InitializeComponent();
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.MediumPurple;
            this.pictureBox1.BackColor = Color.MediumPurple;
            LastBitmap = Properties.Resources.blackheart60x60;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = Properties.Resources.sparklingheart60x60;
            LastBitmap = Properties.Resources.sparklingheart60x60;

            FreedSpace = new BinSize().GetBinSize();
            // empty bin
            SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlag.SHERB_NOSOUND | RecycleFlag.SHERB_NOCONFIRMATION);

            FreedSpace += GetDirectorySize(KnownFolders.GetPath(KnownFolder.Downloads));
            // empty downloads
            Empty(new DirectoryInfo(KnownFolders.GetPath(KnownFolder.Downloads)));

            if (checkBox1.Checked)
            {
                var folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    RemoveBinAndObjFolders(folderBrowserDialog1.SelectedPath);
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

        public void RemoveBinAndObjFolders(string basePath)
        {
            foreach (var dir in Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories).Where(x => x.EndsWith("\\obj") || x.EndsWith("\\bin")))
            {
                FreedSpace += GetDirectorySize(dir);

                Yellow(() => Console.Write($"{ dir } --> {  GetDirectorySize(dir) } Bytes"));
                try
                {
                    Directory.Delete(dir, true);
                    Green(() => Console.Write(" OK !"));
                }
                catch (Exception ex)
                {
                    Red(() => Console.Write($" FEHLER ! \r\n {ex.Message}"));
                    //Error = true;
                    break;
                }
                Console.WriteLine();
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

        private void pictureBox1_MouseHover(object sender, EventArgs e) => pictureBox1.Image = Properties.Resources.redheart60x60;
        
        private void pictureBox1_MouseLeave(object sender, EventArgs e) => pictureBox1.Image = LastBitmap;

        public static void Yellow(Action action) => Colored(ConsoleColor.Yellow, action);

        public static void Red(Action action)
        {
            Colored(ConsoleColor.Red, action);
        }

        public static void Green(Action action)
        {
            Colored(ConsoleColor.Green, action);
        }

        public static void Colored(ConsoleColor color, Action action)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            action();
            Console.ForegroundColor = old;
        }
    }
}
