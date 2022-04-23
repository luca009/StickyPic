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

namespace StickyPic.Controls
{
    public enum PenType
    {
        Pen, Highlighter, Eraser
    }

    /// <summary>
    /// Interaction logic for PaintColorControl.xaml
    /// </summary>
    public partial class PaintColorControl : UserControl
    {
        public Thickness Stroke
        {
            get
            {
                return ellipseColor.Margin;
            }
            set
            {
                ellipseColor.Margin = value;
            }
        }
        public SolidColorBrush Color
        {
            get
            {
                return ellipseColor.Fill as SolidColorBrush;
            }
            set
            {
                ellipseColor.Fill = value;
            }
        }
        private bool isSelected = false;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;

                if (isSelected)
                    this.Background = Resources["SelectedBGColor"] as SolidColorBrush;
                else
                    this.Background = null;
            }
        }
        private PenType type = PenType.Pen;
        public PenType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                if (value < PenType.Highlighter)
                {
                    gridEllipses.Visibility = Visibility.Visible;
                    contentIcon.Visibility = Visibility.Hidden;
                }
                else
                {
                    gridEllipses.Visibility = Visibility.Hidden;
                    contentIcon.Visibility = Visibility.Visible;
                }

                switch (type)
                {
                    case PenType.Highlighter:
                        contentIcon.Content = Resources["iconHighlighter"];
                        break;
                    case PenType.Eraser:
                        contentIcon.Content = Resources["iconEraser"];
                        break;
                }
            }
        }
        public delegate void SelectionChangedDelegate(object sender);
        public event SelectionChangedDelegate SelectionChanged;

        public PaintColorControl()
        {
            InitializeComponent();
            Stroke = new Thickness(1);
            Color = new SolidColorBrush(Colors.Red);
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        public void SimulateClick()
        {
            Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsSelected = !IsSelected;

            if (SelectionChanged != null)
                SelectionChanged(this);
        }
    }
}
