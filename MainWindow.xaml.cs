using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StickyPic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double imageAspectRatio = 0.5625f;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenImage(Image image, ImageSource bitmap)
        {
            image.Source = bitmap; //Open the first object in the array
            imageAspectRatio = bitmap.Height / bitmap.Width; //Calculate aspect ratio
            this.Width = 480f;
            this.Height = (this.Width * imageAspectRatio); //Set window size according to aspect ratio
            this.MinWidth = 100f; //Make minimum size smaller
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
                    bBack.Visibility = Visibility.Visible; //Show back button
                    canvasHome.Visibility = Visibility.Hidden; //Hide the home screen elements
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
            canvasHome.Visibility = Visibility.Visible; //Show the home screen elements
        }

        private void bFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
                OpenImage(imageMain, Clipboard.GetImage()); //Get the image from the Clipboard and open it
            bBack.Visibility = Visibility.Visible; //Show back button
            canvasHome.Visibility = Visibility.Hidden; //Hide the home screen elements
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
    }
}
