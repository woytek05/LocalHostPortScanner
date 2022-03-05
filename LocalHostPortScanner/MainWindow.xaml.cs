using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace LocalHostPortScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener server;
        private static readonly Regex rgx = new Regex("[0-9.-]+");
        Thread thread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private static bool IsTextAllowed(string text)
        {
            return rgx.IsMatch(text);
        }

        private void textBoxFrom_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void textBoxTo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        public void Scan()
        {
            int from = 0, to = 0;
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                from = Convert.ToInt32(textBoxFrom.Text);
                to = Convert.ToInt32(textBoxTo.Text);
            }));

            if (from > to)
            {
                MessageBox.Show("Invalid port range.");
                return;
            }
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                listBoxHosts.Items.Clear();
                listBoxHosts.Items.Add("Start a scan...");
            }));
            
            for (int i = from; i <= to; i++)
            {

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    labelHost.Content = "Currently, the port scan: " + i;
                }));
                try
                {
                    server = new TcpListener(IPAddress.Loopback, i);
                    server.Start();
                    server.Stop();
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        listBoxHosts.Items.Add("Port: " + i + " is busy");
                    }));
                }
            }
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                listBoxHosts.Items.Add("Scan completed!");
            }));
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (thread == null)
                thread = new Thread(Scan);

            if (thread.IsAlive == false)
            {
                thread.Start();
                buttonStop.IsEnabled = true;
                buttonStart.Content = "PAUSE";
            }
            else if (thread.ThreadState != ThreadState.Suspended)
            {
                thread.Suspend();
                buttonStop.IsEnabled = false;
                buttonStart.Content = "RESUME";
            }
            else if (thread.ThreadState == ThreadState.Suspended)
            {
                thread.Resume();
                buttonStart.Content = "PAUSE";
                buttonStop.IsEnabled = true;
            }

        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            thread.Abort();
            thread = new Thread(Scan);
            buttonStop.IsEnabled = false;
            buttonStart.Content = "START";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (thread == null)
                thread = new Thread(Scan);

            if (thread.IsAlive == true)
            {
                if (thread.ThreadState == ThreadState.Suspended)
                {
                    thread.Resume();
                    thread.Abort();
                }
                else
                {
                    thread.Abort();
                }
            }
        }
    }
}
