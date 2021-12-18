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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StickyPic
{
    /// <summary>
    /// Interaction logic for PinSuggestion.xaml
    /// </summary>
    public partial class PinSuggestion : Window
    {
        DispatcherTimer timeoutTimer = new DispatcherTimer();
        public PinSuggestion()
        {
            InitializeComponent();
            timeoutTimer.Interval = TimeSpan.FromSeconds(6);
            timeoutTimer.Tick += timeoutTimer_Tick;
            timeoutTimer.Start();

            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = 0;
        }

        private void timeoutTimer_Tick(object sender, EventArgs e)
        {
            DoubleAnimation opacityDecreaseAnimation = new DoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(2)))
                { EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn } };
            opacityDecreaseAnimation.Completed += opacityDecreaseAnimation_Completed;
            this.BeginAnimation(Window.OpacityProperty, opacityDecreaseAnimation);
        }

        private void opacityDecreaseAnimation_Completed(object sender, EventArgs e)
        {
            this.DialogResult = false;
            timeoutTimer.Stop();
            this.Close();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            timeoutTimer.Stop();
            DoubleAnimation opacityIncreaseAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(0.1)))
                { EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut } };
            this.BeginAnimation(Window.OpacityProperty, opacityIncreaseAnimation);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            timeoutTimer.Start();
            DoubleAnimation opacityDecreaseAnimation = new DoubleAnimation(0.9, new Duration(TimeSpan.FromSeconds(0.1)))
                { EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut } };
            this.BeginAnimation(Window.OpacityProperty, opacityDecreaseAnimation);
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            timeoutTimer.Stop();
            this.Close();
        }

        private void bPin_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            timeoutTimer.Stop();
            this.Close();
        }
    }
}
