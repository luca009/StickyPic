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
    /// <summary>
    /// Interaction logic for PaintColorPaletteControl.xaml
    /// </summary>
    public partial class PaintColorPaletteControl : UserControl
    {
        private List<Color> colorPalette = new List<Color>();
        public List<Color> ColorPalette
        {
            get
            {
                return colorPalette;
            }
            set
            {
                stackpanelColors.Children.Clear();
                foreach (Color color in value)
                {
                    PaintColorControl paintColorControl = new PaintColorControl() { Color = new SolidColorBrush(color) };
                    paintColorControl.SelectionChanged += PaintColorControl_SelectionChanged;
                    stackpanelColors.Children.Add(paintColorControl);
                }

                PaintColorControl highlighterControl = new PaintColorControl() { Color = new SolidColorBrush(Colors.Yellow), Type = PenType.Highlighter };
                highlighterControl.SelectionChanged += PaintColorControl_SelectionChanged;
                stackpanelColors.Children.Add(highlighterControl);

                PaintColorControl eraserControl = new PaintColorControl() { Color = new SolidColorBrush(Colors.Transparent), Type = PenType.Eraser };
                eraserControl.SelectionChanged += PaintColorControl_SelectionChanged;
                stackpanelColors.Children.Add(eraserControl);

                colorPalette = value;
            }
        }
        public PenType SelectedType
        {
            get
            {
                foreach (PaintColorControl paintColorControl in stackpanelColors.Children)
                {
                    if (paintColorControl.IsSelected)
                        return paintColorControl.Type;
                }

                return PenType.Pen;
            }
        }
        public Color SelectedColor
        {
            get
            {
                foreach (PaintColorControl paintColorControl in stackpanelColors.Children)
                {
                    if (paintColorControl.IsSelected)
                        return paintColorControl.Color.Color;
                }

                return Colors.Black;
            }
        }
        public delegate void SelectionChangedDelegate(object sender);
        public event SelectionChangedDelegate SelectionChanged;

        public PaintColorPaletteControl()
        {
            InitializeComponent();
        }

        public void SelectIndex(int index)
        {
            ((PaintColorControl)stackpanelColors.Children[index]).SimulateClick();
        }

        private void PaintColorControl_SelectionChanged(object sender)
        {
            foreach (PaintColorControl paintColorControl in stackpanelColors.Children)
            {
                if (paintColorControl.Equals(sender))
                    continue;

                paintColorControl.IsSelected = false;
            }

            if (SelectionChanged != null)
                SelectionChanged(this);
        }
    }
}
