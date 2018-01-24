using Launcher.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for ScanOutput.xaml
    /// </summary>
    public partial class ScanOutput : Window
    {
        public System.Windows.Media.Brush brushFull = null;
        public System.Windows.Media.Brush brushNormal = null;

        BackgroundWorker Worker = new BackgroundWorker();

        public ScanOutput()
        {
            InitializeComponent();
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowFunctions.WindowProc));
            };
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            CheckAll_cb.Checked += (s, e) => { SelectAll(true); };
            CheckAll_cb.Unchecked += (s, e) => { SelectAll(false); };

            brushFull = (DrawingBrush)FindResource("FullScreenButton");
            brushNormal = (DrawingBrush)FindResource("NormalScreenButton");
            listBox.Items.Clear();

            Start();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int count = 0;
            string folder = "";
            Dispatcher.BeginInvoke(new Action(() => { folder = SelectedFolder_lb.Content.ToString(); }), DispatcherPriority.Background);
            for (int i = 0; i < 1;)
            {
                if (folder == "")
                {
                    Thread.Sleep(10);
                }
                else
                {
                    i = 1;
                }
            }
            foreach (var item in Directory.GetDirectories(folder.ToString()))
            {
                try
                {
                    foreach (var app in Directory.GetFiles(item, "*.exe", SearchOption.AllDirectories))
                    {
                        if (app.Contains("nstall") || app.Contains("etup") || app.Contains("et-Up") || app.Contains("et-up") || app.Contains("pdate") || app.Contains("elper")) { }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    Grid grid = new Grid();

                                    ItemsControl itemsControl = new ItemsControl();
                                    itemsControl.Margin = new Thickness(0, 2, 0, 2);
                                    itemsControl.BorderThickness = new Thickness(0);
                                    itemsControl.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#00FFFFFF");
                                    grid.Children.Add(itemsControl);

                                    ToggleButton toggle = new ToggleButton();
                                    toggle.Name = "A" + count.ToString();
                                    toggle.Style = FindResource("ListButton") as Style;
                                    toggle.Height = 20;

                                    Grid innerGrid = new Grid();

                                    System.Windows.Controls.Image Icon = new System.Windows.Controls.Image();
                                    Icon.HorizontalAlignment = HorizontalAlignment.Left;
                                    Icon.Height = 20;
                                    Icon.Width = 20;
                                    Icon.Margin = new Thickness(25, 0, 0, 0);
                                    Icon icon = null;
                                    icon = System.Drawing.Icon.ExtractAssociatedIcon(app);
                                    Bitmap bitmap = icon.ToBitmap();
                                    IntPtr hBitmap = bitmap.GetHbitmap();
                                    ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                    Icon.Source = wpfBitmap;
                                    innerGrid.Children.Add(Icon);

                                    int chars = app.LastIndexOf("\\");
                                    string exeName = app.Substring(chars + 1, app.Length - chars - 5);
                                    int length = 25;
                                    TextBlock name = new TextBlock();
                                    name.HorizontalAlignment = HorizontalAlignment.Left;
                                    name.VerticalAlignment = VerticalAlignment.Center;
                                    name.Margin = new Thickness(45, 0, 0, 0);
                                    if (exeName.Length > length)
                                    {
                                        name.Text = exeName.Substring(0, length) + " ...";
                                        name.ToolTip = exeName;
                                    }
                                    else
                                    {
                                        name.Text = exeName;
                                    }
                                    innerGrid.Children.Add(name);

                                    TextBlock path = new TextBlock();
                                    path.HorizontalAlignment = HorizontalAlignment.Left;
                                    path.VerticalAlignment = VerticalAlignment.Center;
                                    path.Margin = new Thickness(200, 0, 0, 0);
                                    path.Text = app.ToString();
                                    innerGrid.Children.Add(path);

                                    CheckBox checkBox = new CheckBox();
                                    checkBox.Name = "A" + count.ToString();
                                    checkBox.SetBinding(ToggleButton.IsCheckedProperty, (toggle.Name = checkBox.Name));
                                    checkBox.VerticalAlignment = VerticalAlignment.Center;
                                    innerGrid.Children.Add(checkBox);

                                    toggle.Content = innerGrid;
                                    toggle.Margin = new Thickness(0, 2, 0, 2);
                                    toggle.IsChecked = true;
                                    //itemsControl.Items.Add(toggle);
                                    //listBox.Items.Add(grid);
                                    listBox.Items.Add(toggle);
                                }
                                catch (UnauthorizedAccessException)
                                {

                                }
                            }), DispatcherPriority.Background);
                        }
                        count++;
                    }
                }
                catch (UnauthorizedAccessException)
                {

                }
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        }

        public void Start()
        {
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

        private void SelectFolder_b_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();
            var FBD = new System.Windows.Forms.FolderBrowserDialog();
            FBD.Description = "select the text folder";
            FBD.RootFolder = Environment.SpecialFolder.MyComputer;
            System.Windows.Forms.DialogResult result = FBD.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SelectedFolder_lb.Content = FBD.SelectedPath;

                Mouse.OverrideCursor = Cursors.AppStarting;
                Worker.RunWorkerAsync();
            }
        }

        private void SelectAll(bool select)
        {
            MessageBox.Show(select.ToString());
        }
    }
}
