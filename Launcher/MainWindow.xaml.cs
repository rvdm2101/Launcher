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
using System.Runtime.InteropServices;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Browse browse = null;
        WindowFunctions WF = null;
        public System.Windows.Media.Brush brushFull = null;
        public System.Windows.Media.Brush brushNormal = null;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public SqlCeConnection con = new SqlCeConnection("Data Source=..\\..\\Files\\Launcher.sdf");
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Button button = new Button();
        public GradientStop NewColor = new GradientStop();
        public object exists = false;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowFunctions.WindowProc));
            };
            brushFull = (DrawingBrush)FindResource("FullScreenButton");
            brushNormal = (DrawingBrush)FindResource("NormalScreenButton");

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 2);
            list();
            Resize();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundChanger();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            list();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //Listbox.Items.Clear();
            //NameExe.Content = "";
            //IconExe.Source = null;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Thread.Sleep(1);
            Resize();
            if (WindowState == WindowState.Maximized)
            {
                ScreenSize.Fill = brushFull;
                Main.Height = MainBack.Height;
                Main.Width = MainBack.Width;
            }
            else if (WindowState == WindowState.Normal)
            {
                ScreenSize.Fill = brushNormal;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonSize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                ScreenSize.Fill = brushFull;
            }
            else
            {
                WindowState = WindowState.Normal;
                ScreenSize.Fill = brushNormal;
            }
            Resize();
        }

        private void ButtonHide_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void BackgroundChanger()
        {
            LinearGradientBrush gradient = new LinearGradientBrush();

            gradient.StartPoint = new System.Windows.Point(0.5, 0);
            gradient.EndPoint = new System.Windows.Point(0.5, 1);

            var primaire = "";

            try
            {
                SqlCeCommand cmd = new SqlCeCommand("SELECT PrimaireColor FROM Apps WHERE Name='" + NameExe.Content.ToString() + "';");
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed) { con.Open(); }
                SqlCeDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetValue(0).ToString() != "")
                    {
                        primaire = reader.GetValue(0).ToString();
                    }
                }
            }
            finally
            {
                con.Close();
            }
            
            var PictoColor = "#FF85C1F5";
            if (primaire.ToString() != "")
            {
                PictoColor = "#" + primaire;
            }
            System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(PictoColor.ToString());
            System.Windows.Media.Color colorEnd = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#003D3D3D");

            GradientStop Start = new GradientStop();
            Start.Color = color;
            Start.Offset = 0;
            GradientStop End = new GradientStop();
            End.Color = colorEnd;
            End.Offset = 0.75;

            gradient.GradientStops.Add(Start);
            gradient.GradientStops.Add(End);

            Main.Background = gradient;
            Storyboard sb = (Storyboard)FindResource("NewGradientColor");
            sb.Begin(this);
        }

        private void ExpanderButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;

            button = (Button)sender;
            LogicalTreeHelper.FindLogicalNode(button, "ExpanderButton");
            button.IsEnabled = false;
            dispatcherTimer.Start();

            if (NameExe.Content.ToString() != "")
            {
                if (ExpanderGrid.Height == 0)
                {
                    Storyboard sbe = (Storyboard)FindResource("ExpanderOpen");
                    sbe.Begin(this);

                    Storyboard sbi = button.Resources["ExpIconOpen"] as Storyboard;
                    sbi.Begin();
                }
                else
                {
                    Storyboard sbe = (Storyboard)FindResource("ExpanderClose");
                    sbe.Begin(this);

                    Storyboard sbi = button.Resources["ExpIconClose"] as Storyboard;
                    sbi.Begin();
                }
            }
        }

        public void list()
        {
            NameExe.Content = "";
            IconExe.Source = null;
            Listbox.Items.Clear();
            int selected = 0;
            SqlCeCommand cmd = new SqlCeCommand("SELECT DISTINCT Class FROM Apps ORDER BY Class ASC");
            cmd.Connection = con;
            if (con.State == ConnectionState.Closed) { con.Open(); }
            SqlCeDataReader reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Expander expander = new Expander();
                    expander.Header = reader.GetString(0);
                    expander.Style = FindResource("ExpanderBackground") as Style;
                    expander.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFFFF");
                    expander.FontWeight = FontWeights.Bold;
                    expander.IsExpanded = true;
                    Grid ItemGrid = new Grid();
                    Grid BackGrid = new Grid();
                    BackGrid.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF3D3D3D");
                    BackGrid.VerticalAlignment = VerticalAlignment.Top;
                    BackGrid.Height = 24;
                    ItemGrid.Children.Add(BackGrid);
                    ItemGrid.Children.Add(expander);
                    Listbox.Items.Add(ItemGrid);

                    ItemsControl itemsControl = new ItemsControl();
                    itemsControl.Margin = new Thickness(0, 2, 0, 2);
                    itemsControl.BorderThickness = new Thickness(0);
                    itemsControl.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#00FFFFFF");
                    expander.Content = itemsControl;

                    SqlCeCommand cmdA = new SqlCeCommand("SELECT * FROM Apps WHERE Class = '" + reader.GetString(0) + "' ORDER BY Name ASC");
                    cmdA.Connection = con;
                    SqlCeDataReader readerA = cmdA.ExecuteReader();
                    while (readerA.Read())
                    {
                        Button button = new Button();
                        button.Name = "A" + readerA.GetInt32(0).ToString();
                        button.Style = FindResource("ListButton") as Style;
                        button.Click += Button_Click;
                        button.MouseDoubleClick += Start_Click;
                        button.PreviewMouseRightButtonDown += ListItem_PreviewMouseRightButtonDown;
                        Grid grid = new Grid();
                        System.Windows.Controls.Image Icon = new System.Windows.Controls.Image();
                        Icon.HorizontalAlignment = HorizontalAlignment.Left;
                        Icon.Height = 20;
                        Icon.Width = 20;
                        grid.Children.Add(Icon);
                        TextBlock Name = new TextBlock();
                        Name.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFFFF");
                        Name.FontWeight = FontWeights.DemiBold;
                        Name.VerticalAlignment = VerticalAlignment.Center;
                        Name.Margin = new Thickness(26,0,55,0);
                        grid.Children.Add(Name);

                        TextBlock status = new TextBlock();
                        status.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFFFF");
                        status.FontWeight = FontWeights.DemiBold;
                        status.VerticalAlignment = VerticalAlignment.Center;
                        status.HorizontalAlignment = HorizontalAlignment.Right;
                        status.Margin = new Thickness(0, 0, 5, 0);
                        grid.Children.Add(status);

                        Icon icon = null;
                        icon = System.Drawing.Icon.ExtractAssociatedIcon(readerA.GetString(2));
                        Bitmap bitmap = icon.ToBitmap();
                        IntPtr hBitmap = bitmap.GetHbitmap();

                        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        int indexStart = readerA.GetString(2).LastIndexOf('\\');
                        int indexStop = readerA.GetString(2).LastIndexOf('.');
                        string Namexe = readerA.GetString(2).Substring(indexStart + 1, indexStop - indexStart - 1);

                        Process[] processlist = Process.GetProcesses();
                        foreach (Process theprocess in processlist)
                        {
                            if (theprocess.ProcessName == Namexe && theprocess.MainWindowHandle.ToInt64() != 0)
                            {
                                status.Text = "Running";
                            }
                        }

                        Name.Text = readerA.GetString(1);
                        Icon.Source = wpfBitmap;
                        button.Content = grid;
                        button.Margin = new Thickness(0,2,0,2);
                        itemsControl.Items.Add(button);
                        if(NameExe.Content.ToString() == "")
                        {
                            NameExe.Content = readerA.GetString(1);
                            IconExe.Source = wpfBitmap;
                        }
                        if(selected == 0)
                        {
                            selected++;
                        }
                    }
                }
            }
            finally
            {
                con.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string idt = (sender as Button).Name.ToString().Substring(1);
            string testname = "";
            SqlCeCommand cmdt = new SqlCeCommand("SELECT * FROM Apps WHERE [Id] = '" + idt + "';");
            cmdt.Connection = con;
            if (con.State == ConnectionState.Closed) { con.Open(); }
            SqlCeDataReader readert = cmdt.ExecuteReader();

            while (readert.Read()) { testname = readert.GetString(1); }
            if (testname != NameExe.Content.ToString())
            {
                if (ExpanderGrid.Height > 0)
                {
                    Storyboard sbe = (Storyboard)FindResource("ExpanderClose");
                    sbe.Begin(this);

                    Storyboard sbi = button.Resources["ExpIconClose"] as Storyboard;
                    sbi.Begin();
                }

                try
                {
                    string id = (sender as Button).Name.ToString().Substring(1);

                    SqlCeCommand cmd = new SqlCeCommand("SELECT * FROM Apps WHERE [Id] = '" + id + "';");
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
                        IconExe.Source = wpfBitmap;
                        NameExe.Content = reader.GetString(1);
                    }
                }
                catch (Exception ex)
                {
                    if (Listbox.SelectedIndex < 0)
                    {
                    }
                    else
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                finally
                {
                    con.Close();
                }
                BackgroundChanger();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (NameExe.Content.ToString() != "")
            {
                SqlCeCommand cmd = new SqlCeCommand();
                if (Start.ClickMode == ClickMode.Press)
                {
                    string id = (sender as Button).Name.ToString().Substring(1);
                    cmd = new SqlCeCommand("SELECT * FROM Apps WHERE [Id] = '" + id + "';");
                }
                else
                {
                    cmd = new SqlCeCommand("SELECT * FROM Apps WHERE [Name] = '" + NameExe.Content.ToString() + "';");
                }
                cmd.Connection = con;
                try
                {
                    if (con.State == ConnectionState.Closed) { con.Open(); }
                    SqlCeDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int indexStart = reader.GetString(2).LastIndexOf('\\');
                        int indexStop = reader.GetString(2).LastIndexOf('.');
                        string Namexe = reader.GetString(2).Substring(indexStart + 1, indexStop - indexStart - 1);
                        int running = 0;
                        
                        Process[] processlist = Process.GetProcesses();
                        foreach (Process theprocess in processlist)
                        {
                            if (theprocess.ProcessName == Namexe && theprocess.MainWindowHandle.ToInt64() != 0)
                            {
                                running++;
                            }
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo(reader.GetString(2));
                        startInfo.WindowStyle = ProcessWindowStyle.Maximized;

                        if (running == 0)
                        {
                            Process.Start(startInfo);
                        }
                        else
                        {
                            SetForegroundWindow(Process.GetProcessesByName(Namexe).FirstOrDefault().MainWindowHandle);
                            if (this.IsActive == true)
                            {
                                Process.Start(startInfo);
                            }
                        }
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
            }
        }

        public void Resize()
        {
            Main.Height = MainBack.ActualHeight;
            Main.Width = MainBack.ActualWidth;
            Listbox.Width = Main.Width / 3;
            Listbox.Height = Main.Height;
            Listbox.VerticalAlignment = VerticalAlignment.Top;

            var ma = Margin;
            ma.Top = 12;
            ma.Left = Logo.Width + 25;
            Add.Margin = ma;

            var mt = Margin;
            mt.Top = 15;
            mt.Left = Listbox.Width + 5;
            TitleExe.Margin = mt;

            var ms = Margin;
            ms.Top = (TitleExe.Height + TitleExe.Margin.Top) + 5;
            ms.Left = Listbox.Width + 5;
            StartExp.Margin = ms;

            var mexp = Margin;
            mexp.Top = (StartExp.Height + StartExp.Margin.Top) + 5;
            mexp.Left = Listbox.Width + 5;
            Settings_exp.Margin = mexp;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            browse = new Browse();
            browse.Start();
            if (browse.Name != "")
            {
                try
                {
                    SqlCeCommand cmd = new SqlCeCommand("SELECT * FROM Apps WHERE Path='" + browse.Path + "';");
                    cmd.Connection = con;
                    if (con.State == ConnectionState.Closed) { con.Open(); }
                    SqlCeDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if(reader.GetValue(2).ToString() != "")
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
                    Icon icon = null;
                    icon = System.Drawing.Icon.ExtractAssociatedIcon(browse.Path);
                    Bitmap bm = icon.ToBitmap();
                    
                    // Store the histogram in a dictionary          
                    Dictionary<System.Drawing.Color, int> histo = new Dictionary<System.Drawing.Color, int>();
                    for (int x = 0; x < bm.Width; x++)
                    {
                        for (int y = 0; y < bm.Height; y++)
                        {
                            // Get pixel color 
                            System.Drawing.Color c = bm.GetPixel(x, y);
                            // If it exists in our 'histogram' increment the corresponding value, or add new
                            if ((c.R.ToString() != "00" || c.G.ToString() != "00" || c.B.ToString() != "00") && c.A.ToString() == "255")
                            {
                                if (histo.ContainsKey(c))
                                    histo[c] = histo[c] + 1;
                                else
                                    histo.Add(c, 1);
                            }
                        }
                    }
                    var items = from pair in histo orderby pair.Value descending select pair;
                    var colors = items.Take(1);
                    var color = "";

                    foreach (KeyValuePair<System.Drawing.Color, int> pair in colors)
                    {
                        color = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", pair.Key.A, pair.Key.R, pair.Key.G, pair.Key.B);
                    }

                    try
                    {
                        SqlCeCommand cmd = new SqlCeCommand("INSERT INTO Apps (Name, Path, PrimaireColor) VALUES('" + browse.Name + "', '" + browse.Path + "' , '" + color + "');");
                        cmd.Connection = con;
                        if (con.State == ConnectionState.Closed) { con.Open(); }
                        if (browse.Name != "")
                        {
                            cmd.ExecuteNonQuery();
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
                    list();
                }
                else
                {
                    MessageBox.Show("This item is already added");
                    exists = false;
                }
            }
        }

        private void DelAll_Click(object sender, RoutedEventArgs e)
        {
            if (Listbox.Items.Count > 0)
            {
                DeleteMessage DelMes = new DeleteMessage();
                try
                {
                    DelMes.Owner = this;
                    DelMes.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                if ((bool)DelMes.Del == true)
                {
                    SqlCeCommand cmd = new SqlCeCommand("DELETE FROM Apps;");
                    cmd.Connection = con;
                    try
                    {
                        if (con.State == ConnectionState.Closed) { con.Open(); }
                        SqlCeDataReader reader = cmd.ExecuteReader();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        DelMes.Del = false;
                        con.Close();
                    }
                    list();
                }
            }
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanOutput scanOutput = new ScanOutput();
            scanOutput.Owner = this;
            scanOutput.ShowDialog();

            //List<string> files = new List<string>();
            ////string[] files = Directory.GetFiles("c://Windows//", "*.exe", SearchOption.AllDirectories);
            
            //foreach (var item in Directory.GetDirectories("D://"))
            //{
            //    try
            //    {
            //        foreach(var app in Directory.GetFiles(item, "*.exe", SearchOption.AllDirectories))
            //        {
            //            try
            //            {
            //                files.Add(app);
            //            }
            //            catch (UnauthorizedAccessException)
            //            {

            //            }
            //        }
            //    }
            //    catch (UnauthorizedAccessException)
            //    {

            //    }
            //}

            // Display all the files.
            //if (files != null)
            //{
            //    foreach (string file in files)
            //    {
            //        MessageBox.Show(file.ToString());
            //    }
            //}
        }

        private void ListItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            button.IsEnabled = true;
            dispatcherTimer.Stop();
        }

        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsItem.Opacity != 0 && NameExe.Content.ToString() != "")
            {
                try
                {
                    SqlCeCommand cmd = new SqlCeCommand("SELECT Id FROM Apps WHERE Name='" + NameExe.Content.ToString() + "';");
                    cmd.Connection = con;
                    if (con.State == ConnectionState.Closed) { con.Open(); }
                    SqlCeDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader.GetValue(0).ToString() != "")
                        {
                            Properties.Settings.Default.ExId = reader.GetInt32(0);
                        }
                    }
                }
                finally
                {
                    con.Close();
                }
                try
                {
                    Properties.Settings.Default.ExNa = NameExe.Content.ToString();
                    Properties.Settings.Default.Save();
                    Settings settings = new Settings();
                    settings.Owner = this;
                    settings.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    if (Properties.Settings.Default.Saved == true)
                    {
                        NameExe.Content = "";
                        IconExe.Source = null;
                        list();
                        Button button = new Button();
                        button.Name = "A" + Properties.Settings.Default.ExId.ToString();
                        button.Click += Button_Click;
                        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    Properties.Settings.Default.Saved = false;
                    Properties.Settings.Default.ExId = 0;
                    Properties.Settings.Default.ExNa = "";
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteItem.Opacity != 0)
            {
                SqlCeCommand cmd = new SqlCeCommand("DELETE FROM Apps WHERE [Name] = '" + NameExe.Content.ToString() + "';");
                cmd.Connection = con;
                try
                {
                    if (con.State == ConnectionState.Closed) { con.Open(); }
                    SqlCeDataReader reader = cmd.ExecuteReader();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
                Storyboard sbe = (Storyboard)FindResource("ExpanderClose");
                sbe.Begin(this);

                Storyboard sbi = button.Resources["ExpIconClose"] as Storyboard;
                sbi.Begin();

                NameExe.Content = "";
                IconExe.Source = null;
                list();
            }
        }
    }
}
