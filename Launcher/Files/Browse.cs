using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Launcher.Files
{
    class Browse
    {
        //MainWindow _form = Application.Current.Windows[0] as MainWindow;

        public bool? DialogResult { get; set; }

        public string Name = "";
        //public Icon icon = null;
        public string Path = "";
        public void Start()
        {
            //foreach (Window window in Application.Current.Windows)
            //{
            //    if (window.GetType() == typeof(MainWindow))
            //    {
            //        var main = window as MainWindow;
            //        if (main != null)
            //        {
            //        }
            //    }
            //}

            BrowseFile();
        }

        public void BrowseFile()
        {
            OpenFileDialog oFG = new OpenFileDialog();
            oFG.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            oFG.Title = "Browse executables";
            oFG.CheckFileExists = true;
            oFG.CheckPathExists = true;
            oFG.DefaultExt = "executable";
            oFG.Filter = "executables (*.exe)|*.exe";
            oFG.FilterIndex = 1;
            oFG.RestoreDirectory = true;
            oFG.ReadOnlyChecked = true;
            oFG.ShowReadOnly = true;
            if (oFG.ShowDialog() == DialogResult != true)
            {
                if (oFG.SafeFileName.Length != 0)
                {
                    int check = oFG.SafeFileName.LastIndexOf('.');
                    if (oFG.SafeFileName.Substring(check, oFG.SafeFileName.Length - check) == ".exe")
                    {
                        Name = oFG.SafeFileName.Substring(0, check);
                        Path = oFG.FileName;
                    }
                    else
                    {
                        MessageBox.Show("Please select an executable", "Wrong type",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}
