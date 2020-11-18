using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace AudioSelector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ProcessStartInfo psi;
        public MainWindow()
        {
            psi = new ProcessStartInfo(Environment.CurrentDirectory + "\\nircmdc.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };


            InitializeComponent();
            this.Hide();
            LoadConfig();
            
            DataContext = this;
        }

        private void LoadConfig()
        {
            AS audioselector = null;
            try
            {
                using (StreamReader sr = new StreamReader("config.xml"))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(AS));
                    audioselector = xml.Deserialize(sr) as AS;
                    sr.Close();
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Eror reading config.xml!" + Environment.NewLine + ex.ToString());
                this.Close();
            }

            if (audioselector == null)
            {
                MessageBox.Show("Config.xml could not be read!");
                this.Close();
            }
            

            ContextMenu cm = new ContextMenu();
            {
                MenuItem mi = new MenuItem() { Header = "Anzeigen" };
                mi.Click += (x, y) =>
                {
                    if (!Application.Current.MainWindow.IsVisible)
                        Application.Current.MainWindow.Show();
                };
                cm.Items.Add(mi);
                cm.Items.Add(new Separator());
            }
            foreach(Option o in audioselector.Options)
            {
                MenuItem mi = new MenuItem() { Header = o.Name };
                mi.Click += (x, y) =>
                {
                    psi.Arguments = "setdefaultsounddevice \""+ o.DefaultMultimediaOut +"\" 1";
                    Process.Start(psi);
                    psi.Arguments = "setdefaultsounddevice \"" + o.DefaultCommunicationsOut + "\" 2";
                    Process.Start(psi);
                    psi.Arguments = "setdefaultsounddevice \""+o.DefaultMultimediaIn+"\" 1";
                    Process.Start(psi);
                    psi.Arguments = "setdefaultsounddevice \"" + o.DefaultCommunicationsIn + "\" 2";
                    Process.Start(psi);
                };
                cm.Items.Add(mi);
            }
            {
                cm.Items.Add(new Separator());
                MenuItem mi = new MenuItem() { Header = "Beenden" };
                mi.Click += (x, y) => this.Close();
                cm.Items.Add(mi);
            }

            myNotifyIcon.ContextMenu = cm;
        }

        private void Btn_Shutdown_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_HideWnd_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Btn_ReloadConfig(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }
    }
}
