using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Threading;
using Launcher.Files;
using System.Drawing;
using Microsoft.Win32;
using System.Windows.Interop;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Data;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        MainWindow main = new MainWindow();
        public object restoreIfMove = false;
        public object exists = false;
        public string ExNa = Properties.Settings.Default.ExNa;
        public int ExId = Properties.Settings.Default.ExId;
        public SqlCeConnection con = new SqlCeConnection("Data Source=..\\..\\Files\\Launcher.sdf");

        public Settings()
        {
            InitializeComponent();
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowFunctions.WindowProc));
            };
            Start();
        }

        private void Start()
        {
            Icon.Source = null;
            Name.Content = null;
            Class.Items.Clear();
            try
            {
                SqlCeCommand cmd = new SqlCeCommand("SELECT DISTINCT Class FROM Apps");
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed) { con.Open(); }
                SqlCeDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Class.Items.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                con.Close();
            }

            try
            {
                string ItemName = ExNa;

                SqlCeCommand cmd = new SqlCeCommand("SELECT * FROM Apps WHERE [Name] = '" + ItemName + "';");
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed) { con.Open(); }
                SqlCeDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Icon icon = null;
                    icon = System.Drawing.Icon.ExtractAssociatedIcon(reader.GetString(2));
                    Bitmap bitmap = icon.ToBitmap();
                    IntPtr hBitmap = bitmap.GetHbitmap();

                    ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Icon.Source = wpfBitmap;
                    Name.Content = ItemName;
                    NewName.Text = ItemName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                con.Close();
            }

            try
            {
                string ItemName = ExNa;
                SqlCeCommand cmd = new SqlCeCommand("SELECT Class FROM Apps WHERE [Name] = '" + ItemName + "';");
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed) { con.Open(); }
                SqlCeDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Class.SelectedItem = reader.GetString(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                con.Close();
            }

            LinearGradientBrush gradient = new LinearGradientBrush();

            gradient.StartPoint = new System.Windows.Point(0.5, 0);
            gradient.EndPoint = new System.Windows.Point(0.5, 1);

            System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3D3D3D");
            System.Windows.Media.Color colorEnd = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00EDEDED");

            GradientStop Start = new GradientStop();
            Start.Color = color;
            Start.Offset = 0;
            GradientStop End = new GradientStop();
            End.Color = colorEnd;
            End.Offset = 1;

            gradient.GradientStops.Add(Start);
            gradient.GradientStops.Add(End);

            Main.Background = gradient;
            Storyboard sb = (Storyboard)FindResource("NewGradientColor");
            sb.Begin(this);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCeCommand cmd = new SqlCeCommand("SELECT * FROM Apps WHERE Name = '" + NewName.Text + "';");
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed) { con.Open(); }
                SqlCeDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetValue(2).ToString() != "")
                    {
                        exists = true;
                    }
                }
            }
            finally
            {
                con.Close();
            }
            if ((bool)exists == false || NewName.Text == ExNa)
            {
                try
                {
                    SqlCeCommand cmd = new SqlCeCommand("UPDATE Apps SET Name = '" + NewName.Text.ToString() + "', Class = '" + Class.SelectedItem.ToString() + "' WHERE Id = '" + ExId + "';");
                    cmd.Connection = con;
                    if (con.State == ConnectionState.Closed) { con.Open(); }
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
                Properties.Settings.Default.Saved = true;
                Properties.Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("This name is already used. Please choose a different.");
            }
            exists = false;
        }

        private void ClAd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCeCommand cmd = new SqlCeCommand("SELECT DISTINCT Class FROM Apps WHERE Class = '" + ClNa.Text + "';");
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed) { con.Open(); }
                SqlCeDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetValue(0).ToString() != "")
                    {
                        exists = true;
                    }
                }
            }
            finally
            {
                con.Close();
            }
            if ((bool)exists == false)
            {
                try
                {
                    SqlCeCommand cmd = new SqlCeCommand("UPDATE Apps SET Class = '" + ClNa.Text + "' WHERE Id = '" + ExId + "';");
                    cmd.Connection = con;
                    if (con.State == ConnectionState.Closed) { con.Open(); }
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                    Start();
                    ClNa.Text = "";
                }
                Properties.Settings.Default.Saved = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("This class already exists. Please type a different name.");
                exists = false;
            }
        }
    }
}
