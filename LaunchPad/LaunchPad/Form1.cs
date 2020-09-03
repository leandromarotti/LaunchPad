using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace LaunchPad
{

    public partial class FrmMain : Form
    {

        public FrmMain()
        {
            InitializeComponent();
        }

        List<string> fileList = new List<string>();
        List<string> folderList = new List<string>();

        public void PathFileToListAndListBox(string pathFile, ListBox lb, List<string> ls)
        {
            StreamReader sr = new StreamReader(pathFile);
            lb.Items.Clear();
            ls.Clear();
            while (sr.Peek() > -1)
            {
                ls.Add(sr.ReadLine());
            }
            lb.Items.AddRange(ls.ToArray());
            sr.Close();
        }

        public void RefreshListBox(ListBox lb, List<string> list)
        {
            lb.Items.Clear();
            lb.Items.AddRange(list.ToArray());
        }

        public void UpdatePathFile(List<string> list, string pathFile)
        {
            StreamWriter sw = new StreamWriter(pathFile, false);
            foreach (string line in list)
            {
                sw.WriteLine(line);
            }
            sw.Close();
        }

        public void AddPathToList(List<string> lst, string item)
        {
            // avoid duplicate items
            if (lst.Contains(item))
            {
                lst.Remove(item);
            }
            lst.Insert(0, item); // item is the most recent path
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            string specialFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "LaunchPadFiles");
            string MRUfilesPath = Path.Combine(specialFolder, "MRUfiles.txt");
            string MRUfoldersPath = Path.Combine(specialFolder, "MRUfolders.txt");
            if (!Directory.Exists(specialFolder))
            {
                Directory.CreateDirectory(specialFolder);
            }
            if (!File.Exists(MRUfilesPath))
            {
                File.CreateText(MRUfilesPath);
            }
            if (!File.Exists(MRUfoldersPath))
            {
                File.CreateText(MRUfoldersPath);
            }
            PathFileToListAndListBox(MRUfoldersPath, lstFolders, folderList);
            PathFileToListAndListBox(MRUfilesPath, lstFiles, fileList);
            if (!Size.Equals(Properties.Settings.Default.appSize, new Size(0, 0)))
            {
                this.Size = Properties.Settings.Default.appSize;
            }
            if (!Point.Equals(Properties.Settings.Default.appLocation, new Point(0, 0)))
            {
                this.Location = Properties.Settings.Default.appLocation;
            }

        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            string specialFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), "LaunchPadFiles");
            string MRUfilesPath = Path.Combine(specialFolder, "MRUFiles.txt");
            string MRUfoldersPath = Path.Combine(specialFolder, "MRUfolders.txt");
            UpdatePathFile(fileList, MRUfilesPath);
            UpdatePathFile(folderList, MRUfoldersPath);
            Properties.Settings.Default.appSize = this.Size;
            Properties.Settings.Default.appLocation = this.Location;
            Properties.Settings.Default.Save();
        }

        private void btnNewFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFile = new OpenFileDialog();
            oFile.Title = "Choose the file to be launched.";
            oFile.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            DialogResult dr = oFile.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string newFile = oFile.FileName;
                AddPathToList(fileList, newFile);
                RefreshListBox(lstFiles, fileList);
                string newFolder = Path.GetDirectoryName(newFile);
                AddPathToList(folderList, newFolder);
                RefreshListBox(lstFolders, folderList);
                //launch the selected file with a default program
                Process proc = new Process();
                proc.StartInfo.FileName = newFile;
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
            }
        }

        private void btnNewFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog oFolder = new FolderBrowserDialog();
            oFolder.Description = "Choose the folder to be accessed.";
            oFolder.RootFolder = Environment.SpecialFolder.MyComputer;
            oFolder.ShowNewFolderButton = true;
            DialogResult dr = oFolder.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string newFolder = oFolder.SelectedPath;
                AddPathToList(folderList, newFolder);
                RefreshListBox(lstFolders, folderList);
                // open the user selected folder
                Process proc = new Process();
                proc.StartInfo.FileName = newFolder;
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
            }
        }

        private void lstFolders_DoubleClick(object sender, EventArgs e)
        {
            if (lstFolders.SelectedItem != null)

                if (Directory.Exists(lstFolders.SelectedItem.ToString()))
                {
                    // Open folder in Windows Explorer
                    Process.Start(@lstFolders.SelectedItem.ToString());
                }
                else
                {
                    folderList.Remove(lstFolders.SelectedItem.ToString());
                    RefreshListBox(lstFolders, folderList);
                }

        }

        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItem != null)

                if (File.Exists(lstFiles.SelectedItem.ToString()))
                {
                    // launch the selected file
                    Process proc = new Process();
                    proc.StartInfo.FileName = lstFiles.SelectedItem.ToString();
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                    AddPathToList(fileList, lstFiles.SelectedItem.ToString());
                    RefreshListBox(lstFiles, fileList);
                } 
                else
                {
                    fileList.Remove(lstFiles.SelectedItem.ToString());
                    RefreshListBox(lstFiles, fileList);
                }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
