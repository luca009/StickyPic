using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace StickyPic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double imageAspectRatio = 0.5625f;
        bool windowTransparencyEnabled = false;
        DispatcherTimer pollTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
        double previousImageSeed = 0;

        public MainWindow()
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource bitmap = Clipboard.GetImage();
                previousImageSeed = bitmap.Height * bitmap.Width;
            }

            pollTimer.Tick += pollTimer_Tick;
            pollTimer.Start();

            InitializeComponent();

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk"))
                cboxPinSuggestions.IsChecked = true;
            this.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 50;

            string[] arguments = GetArguments();
            if (arguments.Length > 1)
            {
                if (File.Exists(arguments[1]))
                {
                    var uri = new Uri(arguments[1]);
                    var bitmap = new BitmapImage(uri);
                    OpenImage(imageMain, bitmap);
                }
                else if (arguments[1] == "--launch-hidden")
                {
                    this.Visibility = Visibility.Hidden;
                }
            }
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource image = Clipboard.GetImage();
                if (image.Width * image.Height != previousImageSeed) //Super hacky way to detect clipboard changes
                {
                    previousImageSeed = image.Width * image.Height;
                    PinSuggestion pinSuggestion = new PinSuggestion();
                    if (pinSuggestion.ShowDialog() == true)
                    {
                        OpenImage(imageMain, Clipboard.GetImage());
                        this.Show();
                    }
                }
            }
        }

        private string[] GetArguments()
        {
            string[] arguments = null;
            arguments = Environment.GetCommandLineArgs(); //Load command line arguments into array
            if (arguments != null)
                return arguments;
            else
                return null;
        }

        private void OpenImage(Image image, ImageSource bitmap)
        {
            image.Source = bitmap; //Set the image source to the bitmap
            imageAspectRatio = bitmap.Height / bitmap.Width; //Calculate aspect ratio
            this.MinHeight = (this.Width * imageAspectRatio);
            this.Height = this.MinHeight; //Set window size according to aspect ratio
            this.MinWidth = 100f; //Make minimum size smaller
            bBack.Visibility = Visibility.Visible; //Show back button
            gridHome.Visibility = Visibility.Hidden; //Hide the home screen elements
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); //Get array of dropped objects
                try
                {
                    var uri = new Uri(files[0]);
                    var bitmap = new BitmapImage(uri);
                    OpenImage(imageMain, bitmap); //Open the image
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Did you drag the wrong file?\nError Message: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Height = (this.Width * imageAspectRatio) + 6f; //Set window size according to aspect ratio
            this.MinHeight = this.Height; //Avoid glitching
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal; //Prevent maximizing
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Allow window to get dragged anywhere
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); //Close application
        }

        private void bMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; //Minimize
        }

        private void bBack_Click(object sender, RoutedEventArgs e)
        {
            imageMain.Source = null;
            this.MinWidth = 320f; //Reset minimum size
            bBack.Visibility = Visibility.Hidden; //Hide button
            gridHome.Visibility = Visibility.Visible; //Show the home screen elements
        }

        private void bFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
                OpenImage(imageMain, Clipboard.GetImage()); //Get the image from the Clipboard and open it
            else
                MessageBox.Show("Nothing in Clipboard!", "Clipboard Empty", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void cboxShowUI_Click(object sender, RoutedEventArgs e)
        {
            if (cboxShowUI.IsChecked == true)
            {
                //Make UI elements visible
                bBack.Visibility = Visibility.Visible;
                bClose.Visibility = Visibility.Visible;
                bMinimize.Visibility = Visibility.Visible;
            }
            else
            {
                //Hide UI elements
                bBack.Visibility = Visibility.Hidden;
                bClose.Visibility = Visibility.Hidden;
                bMinimize.Visibility = Visibility.Hidden;
            }
        }

        private void cboxEnableTransparency_Click(object sender, RoutedEventArgs e)
        {
            windowTransparencyEnabled = (bool)cboxEnableTransparency.IsChecked;
            if (!windowTransparencyEnabled)
                this.BeginAnimation(OpacityProperty, new DoubleAnimation(1f, new Duration(TimeSpan.FromSeconds(0.25f))));
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (windowTransparencyEnabled)
                this.BeginAnimation(OpacityProperty, new DoubleAnimation(1f, new Duration(TimeSpan.FromSeconds(0.25f)))); //Make window opaque
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (windowTransparencyEnabled)
                this.BeginAnimation(OpacityProperty, new DoubleAnimation(0.3f, new Duration(TimeSpan.FromSeconds(1f)))); //Make window transparent
        }

        private void cboxPinSuggestions_Click(object sender, RoutedEventArgs e)
        {
            if (cboxPinSuggestions.IsChecked == true)
            {
                WshShell shell = new WshShell();
                string shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "Starts StickyPic without UI";
                shortcut.TargetPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                shortcut.Arguments = "--launch-hidden";
                shortcut.Save();

                Process p = new Process();
                p.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                p.StartInfo.Arguments = "--launch-hidden";
                p.Start();
                p.Dispose();
            }
            else
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk"))
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk");
            }
        }
    }
}
