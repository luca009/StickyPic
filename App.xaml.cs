using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StickyPic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Logger.Log("Unhandled Exception. Message: " + e.Exception.Message, Classes.LogSeverity.Error);
#if DEBUG
            MessageBox.Show("Exception. Message: " + e.Exception.Message);
            e.Handled = true;
#endif
        }
    }
}
