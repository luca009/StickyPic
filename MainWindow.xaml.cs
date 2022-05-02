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
using System.Collections.Generic;
using System.Windows.Ink;
using StickyPic.Classes;

namespace StickyPic
{
    public enum UIMode
    {
        Hidden, Home, View, Paint
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double imageAspectRatio = 0.5625f;
        double maxHeight = SystemParameters.PrimaryScreenHeight;
        bool windowTransparencyEnabled = false;
        bool processTerminating = false;
        UIMode uiMode;
        public Logger Logger = new Logger(false);
        DispatcherTimer pollTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
        double previousImageSeed = 0;

        public MainWindow()
        {
            if (Clipboard.ContainsImage())
                previousImageSeed = Clipboard.GetImage().Height * Clipboard.GetImage().Width;

            pollTimer.Tick += pollTimer_Tick;

            InitializeComponent();

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk")) //check if Pin Suggestions are on
                cboxPinSuggestions.IsChecked = true;

            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; i++)
            {
                if (File.Exists(arguments[i]))
                {
                    var uri = new Uri(arguments[1]);
                    var bitmap = new BitmapImage(uri);
                    OpenImage(imageMain, bitmap);
                }
                else
                {
                    switch (arguments[i])
                    {
                        case "--launch-hidden":
                            Logger.FlushlessLog("Running as background process.", LogSeverity.Message);
                            this.Visibility = Visibility.Hidden;
                            pollTimer.Start();
                            break;
                        case "--logging":
                            Logger.Enabled = true;
                            Logger.Log("Initialized logging.", LogSeverity.Message);
                            break;
                        default:
                            break;
                    }
                }
            }

            Logger.Log("\"Pin Suggestions are located at " +
                Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk""",
                LogSeverity.Message);

            List<Color> colorPalette = new List<Color>
            {
                Colors.Black, Colors.White, Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow
            };
            palettecontrolPalette.ColorPalette = colorPalette;

            SetUIMode(UIMode.Home);

            Logger.Log("Started successfully.", LogSeverity.Message);
        }

        private void SetUIMode(UIMode mode)
        {
            uiMode = mode;

            switch (mode)
            {
                case UIMode.View:
                    bBack.Visibility = Visibility.Visible; //Show back button
                    gridHome.Visibility = Visibility.Hidden; //Hide the home screen elements
                    viewboxInkCanvas.Visibility = Visibility.Visible;
                    bModeSwitch.Visibility = Visibility.Visible;
                    gridTitleBar.Visibility = Visibility.Visible;

                    viewboxInkCanvas.IsHitTestVisible = false;
                    bModeSwitch.Content = Resources["iconPen"];

                    gridTitleBar.Visibility = Visibility.Visible;
                    break;

                case UIMode.Paint:
                    gridTitleBar.Visibility = Visibility.Visible;

                    viewboxInkCanvas.IsHitTestVisible = true;
                    bModeSwitch.Content = Resources["iconImage"];

                    palettecontrolPalette.SelectIndex(0);
                    break;

                case UIMode.Home:
                    bBack.Visibility = Visibility.Hidden; //Hide back button
                    gridHome.Visibility = Visibility.Visible; //Show the home screen elements
                    viewboxInkCanvas.Visibility = Visibility.Hidden;
                    bModeSwitch.Visibility = Visibility.Collapsed;

                    gridTitleBar.Visibility = Visibility.Visible;

                    inkcanvasCanvas.Strokes.Clear();
                    break;

                case UIMode.Hidden:
                    gridTitleBar.Visibility = Visibility.Collapsed;
                    break;

                default:
                    break;
            }

            if (uiMode < UIMode.Paint)
            {
                gridToolsView.Visibility = Visibility.Visible;
                gridToolsPaint.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridToolsView.Visibility = Visibility.Collapsed;
                gridToolsPaint.Visibility = Visibility.Visible;
            }

            UpdateWindowSize();
        }

        private void UpdateWindowSize()
        {
            double newHeight = this.Width * imageAspectRatio;

            if (uiMode > 0)
                newHeight += gridTitleBar.Height;

            if (newHeight > (maxHeight - this.Top))
            {
                newHeight = (maxHeight - this.Top);

                if (uiMode > 0)
                    this.Width = (newHeight - gridTitleBar.Height) / imageAspectRatio;
                else
                    this.Width = newHeight / imageAspectRatio;
            }

            this.MinHeight = newHeight; //Avoid glitching
            this.Height = newHeight; //Set window size according to aspect ratio
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal; //Prevent maximizing
        }

        private void ShowErrorGrid()
        {
            gridToolsError.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(2500);
                Dispatcher.Invoke(() => { gridToolsError.Visibility = Visibility.Hidden; });
            });
        }

        private void OpenImage(Image image, ImageSource bitmap)
        {
            Logger.Log("Opening image.", LogSeverity.Message);
            try
            {
                image.Source = bitmap; //Set the image source to the bitmap
                imageAspectRatio = bitmap.Height / bitmap.Width; //Calculate aspect ratio
                this.MinHeight = (this.Width * imageAspectRatio);
                this.Height = this.MinHeight; //Set window size according to aspect ratio
                this.MinWidth = 100f; //Make minimum size smaller
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to open image. Message: " + ex.Message, LogSeverity.Error);
                ShowErrorGrid();
            }

            SetUIMode(UIMode.View);
        }

        private void EnableBackgroundProcess()
        {
            Logger.Log("Enabling background process.", LogSeverity.Message);

            Process[] processes = Process.GetProcessesByName("StickyPic");

            if (processes.Length <= 0)
            {
                Logger.Log("Other StickyPic processes detected.", LogSeverity.Warning);
                return;
            }

            Process p = new Process();
            p.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
            p.StartInfo.Arguments = "--launch-hidden";
            if (Logger.Enabled) p.StartInfo.Arguments += " --logging";

            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to enable background process. Message: " + ex.Message, LogSeverity.Error);
            }
            p.Dispose();
        }

        private bool GetIfOtherStickyPicInstancesRunning()
        {
            Process[] processes = Process.GetProcessesByName("StickyPic");
            return processes.Length > 1;
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            if (GetIfOtherStickyPicInstancesRunning())
            {
                Logger.Log("Other instances of StickyPic detected. Shutting down.", LogSeverity.Warning);

                processTerminating = true;
                Application.Current.Shutdown();
            }

            if (Clipboard.ContainsImage())
            {
                BitmapSource image = Clipboard.GetImage();
                if (image.Width * image.Height != previousImageSeed) //Super hacky way to detect clipboard changes
                {
                    Logger.Log("Change in clipboard image detected.", LogSeverity.Message);

                    previousImageSeed = image.Width * image.Height;
                    PinSuggestion pinSuggestion = new PinSuggestion();
                    if (pinSuggestion.ShowDialog() == true)
                    {
                        OpenImage(imageMain, Clipboard.GetImage());
                        this.Show();
                        pollTimer.Stop();
                    }
                }
            }
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
                catch (Exception)
                {
                    ShowErrorGrid();
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWindowSize();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Allow window to get dragged anywhere
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (windowTransparencyEnabled)
                this.BeginAnimation(OpacityProperty, new DoubleAnimation(1f, new Duration(TimeSpan.FromSeconds(0.25f)))); //Make window opaque
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (windowTransparencyEnabled)
                this.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5f, new Duration(TimeSpan.FromSeconds(1f)))); //Make window transparent
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cboxPinSuggestions.IsChecked == true && GetIfOtherStickyPicInstancesRunning() == false && processTerminating == false)
            {
                EnableBackgroundProcess();
            }
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

            SetUIMode(UIMode.Home);
        }

        private void cboxEnableTransparency_Click(object sender, RoutedEventArgs e)
        {
            windowTransparencyEnabled = (bool)cboxEnableTransparency.IsChecked;
            if (!windowTransparencyEnabled)
                this.BeginAnimation(OpacityProperty, new DoubleAnimation(1f, new Duration(TimeSpan.FromSeconds(0.25f))));
        }

        private void bFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            throw new Exception("test");

            if (Clipboard.ContainsImage())
                OpenImage(imageMain, Clipboard.GetImage()); //Get the image from the Clipboard and open it
            else
                MessageBox.Show("Nothing in Clipboard!", "Clipboard Empty", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void cboxShowUI_Click(object sender, RoutedEventArgs e)
        {
            if (cboxShowUI.IsChecked == true)
                SetUIMode(UIMode.View);
            else
                SetUIMode(UIMode.Hidden);
        }

        private void cboxPinSuggestions_Click(object sender, RoutedEventArgs e)
        {
            if (cboxPinSuggestions.IsChecked == true)
            {
                //Create a shortcut in the Startup directory for Pin Suggestions
                WshShell shell = new WshShell();
                string shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "Starts StickyPic without UI";
                shortcut.TargetPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                shortcut.Arguments = "--launch-hidden";
                shortcut.Save();
            }
            else
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk"))
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\StickyPic Pin Suggestions.lnk");
            }
        }

        private void bModeSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (uiMode == UIMode.View)
            {
                SetUIMode(UIMode.Paint);
            }
            else
            {
                SetUIMode(UIMode.View);
            }
        }

        private void palettecontrolPalette_SelectionChanged(object sender)
        {
            DrawingAttributes attributes = new DrawingAttributes();
            attributes.Color = palettecontrolPalette.SelectedColor;

            switch (palettecontrolPalette.SelectedType)
            {
                case Controls.PenType.Pen:
                    attributes.StylusTip = StylusTip.Ellipse;
                    attributes.Width = attributes.Height = sldBrushSize.Value;
                    attributes.IsHighlighter = false;
                    inkcanvasCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    inkcanvasCanvas.UseCustomCursor = true;
                    break;
                case Controls.PenType.Highlighter:
                    attributes.StylusTip = StylusTip.Rectangle;
                    attributes.Width = 1;
                    attributes.Height = sldBrushSize.Value;
                    attributes.IsHighlighter = true;
                    inkcanvasCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    inkcanvasCanvas.UseCustomCursor = true;
                    break;
                case Controls.PenType.Eraser:
                    attributes.StylusTip = StylusTip.Rectangle;
                    attributes.IsHighlighter = false;
                    inkcanvasCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    inkcanvasCanvas.EraserShape = new RectangleStylusShape(sldBrushSize.Value * 3, sldBrushSize.Value * 3);
                    inkcanvasCanvas.UseCustomCursor = false;
                    break;
            }

            inkcanvasCanvas.DefaultDrawingAttributes = attributes;
        }

        private void sldBrushSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkcanvasCanvas == null) return;

            inkcanvasCanvas.DefaultDrawingAttributes.Height = sldBrushSize.Value;
            inkcanvasCanvas.EraserShape = new RectangleStylusShape(sldBrushSize.Value * 3, sldBrushSize.Value * 3);

            if (!inkcanvasCanvas.DefaultDrawingAttributes.IsHighlighter)
                inkcanvasCanvas.DefaultDrawingAttributes.Width = sldBrushSize.Value;
        }

        private void miCopyImage_Click(object sender, RoutedEventArgs e)
        {
            pollTimer.Stop();

            double width = imageMain.Source.Width;
            double height = imageMain.Source.Height;

            //Bunch of UI stuff, basically recreate what's in the Window
            Image image = new Image() { Width = width, Height = height, Source = imageMain.Source };
            InkCanvas canvas = new InkCanvas() { Width = inkcanvasCanvas.ActualWidth, Height = inkcanvasCanvas.ActualHeight, Background = null };
            Viewbox canvasViewbox = new Viewbox() { Stretch = Stretch.UniformToFill, Child = canvas };
            canvas.Strokes = inkcanvasCanvas.Strokes;

            Grid grid = new Grid() { Background = null };
            grid.Children.Add(image);
            grid.Children.Add(canvasViewbox);

            //Force rendering using a Viewbox
            Viewbox box = new Viewbox() { Child = grid };
            box.Measure(new System.Windows.Size(width, height));
            box.Arrange(new Rect(new System.Windows.Size(width, height)));

            //Render everything and copy it to the clipboard, making sure not to make pin suggestions fire
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(grid);

            //Copy the image
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                var bitmap = System.Drawing.Bitmap.FromStream(ms);
                Clipboard.SetDataObject(bitmap);
            }

            previousImageSeed = imageMain.Source.Width * imageMain.Source.Height;
            pollTimer.Start();
        }

        private void miClearDrawings_Click(object sender, RoutedEventArgs e)
        {
            inkcanvasCanvas.Strokes.Clear();
        }
    }
}
