using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Externals;
using Vannatech.CoreAudio.Interfaces;

namespace AudioSelector_Core
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

            if (audioselector.Startup != null)
            foreach(DeviceVolume dv in audioselector.Startup.DeviceVolumes)
            {
                string s = GetDeviceIdByName(dv.DeviceName);
                SetMasterVolume(s, dv.Volume);
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

                if (audioselector.Startup?.DefaultOption == o.Name)
                {
                    psi.Arguments = "setdefaultsounddevice \"" + o.DefaultMultimediaOut + "\" 1";
                    Process.Start(psi);
                    psi.Arguments = "setdefaultsounddevice \"" + o.DefaultCommunicationsOut + "\" 2";
                    Process.Start(psi);
                    psi.Arguments = "setdefaultsounddevice \"" + o.DefaultMultimediaIn + "\" 1";
                    Process.Start(psi);
                    psi.Arguments = "setdefaultsounddevice \"" + o.DefaultCommunicationsIn + "\" 2";
                    Process.Start(psi);
                }

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

        private static string GetDeviceIdByName(string name)
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDeviceCollection deviceCollection = null;
            IPropertyStore deviceProperties = null;
            IMMDevice device = null;

            try
            {
                // get the speakers (1st render + multimedia) device
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());

                deviceEnumerator.EnumAudioEndpoints(EDataFlow.eAll, 0xF, out deviceCollection);
                deviceCollection.GetCount(out uint count);

                for (uint i = 0; i < count; ++i)
                {
                    try
                    {
                        deviceCollection.Item(i, out device);
                        device.OpenPropertyStore(STGM.STGM_READ, out deviceProperties);

                        PROPERTYKEY propkey = new PROPERTYKEY()
                        {
                            fmtid = Vannatech.CoreAudio.Constants.PropertyKeys.PKEY_DeviceInterface_FriendlyName,
                            pid = 2
                        };

                        deviceProperties.GetValue(ref propkey, out PROPVARIANT variant);
                        string deviceName = Marshal.PtrToStringUni(variant.Data.AsStringPtr);
                        if (deviceName.ToLowerInvariant() == name.ToLowerInvariant())
                        {
                            device.GetId(out string r);
                            return r;
                        }
                    }
                    finally
                    {
                        if (device != null) Marshal.ReleaseComObject(device);
                        if (deviceProperties != null) Marshal.ReleaseComObject(deviceProperties);
                    }
                }
            }
            finally
            {
                if (device != null) Marshal.ReleaseComObject(device);
                if (deviceProperties != null) Marshal.ReleaseComObject(deviceProperties);
                if (deviceCollection != null) Marshal.ReleaseComObject(deviceCollection);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }

            return "";
        }

        #region Master Volume Manipulation

        /// <summary>
        /// Gets the current master volume in scalar values (percentage)
        /// </summary>
        /// <returns>-1 in case of an error, if successful the value will be between 0 and 100</returns>
        private static float GetMasterVolume(string endpointId)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject(endpointId);
                if (masterVol == null)
                    return -1;

                masterVol.GetMasterVolumeLevelScalar(out float volumeLevel);
                return volumeLevel * 100;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Gets the mute state of the master volume. 
        /// While the volume can be muted the <see cref="GetMasterVolume"/> will still return the pre-muted volume value.
        /// </summary>
        /// <returns>false if not muted, true if volume is muted</returns>
        private static bool GetMasterVolumeMute(string endpointId)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject(endpointId);
                if (masterVol == null)
                    return false;

                masterVol.GetMute(out bool isMuted);
                return isMuted;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Sets the master volume to a specific level
        /// </summary>
        /// <param name="newLevel">Value between 0 and 100 indicating the desired scalar value of the volume</param>
        private static void SetMasterVolume(string endpointId, float newLevel)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject(endpointId);
                if (masterVol == null)
                    return;

                masterVol.SetMasterVolumeLevelScalar(newLevel / 100, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Increments or decrements the current volume level by the <see cref="stepAmount"/>.
        /// </summary>
        /// <param name="stepAmount">Value between -100 and 100 indicating the desired step amount. Use negative numbers to decrease
        /// the volume and positive numbers to increase it.</param>
        /// <returns>the new volume level assigned</returns>
        private static float StepMasterVolume(string endpointId, float stepAmount)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject(endpointId);
                if (masterVol == null)
                    return -1;

                float stepAmountScaled = stepAmount / 100;

                // Get the level
                masterVol.GetMasterVolumeLevelScalar(out float volumeLevel);

                // Calculate the new level
                float newLevel = volumeLevel + stepAmountScaled;
                newLevel = Math.Min(1, newLevel);
                newLevel = Math.Max(0, newLevel);

                masterVol.SetMasterVolumeLevelScalar(newLevel, Guid.Empty);

                // Return the new volume level that was set
                return newLevel * 100;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Mute or unmute the master volume
        /// </summary>
        /// <param name="isMuted">true to mute the master volume, false to unmute</param>
        private static void SetMasterVolumeMute(string endpointId, bool isMuted)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject(endpointId);
                if (masterVol == null)
                    return;

                masterVol.SetMute(isMuted, Guid.Empty);
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        /// <summary>
        /// Switches between the master volume mute states depending on the current state
        /// </summary>
        /// <returns>the current mute state, true if the volume was muted, false if unmuted</returns>
        private static bool ToggleMasterVolumeMute(string endpointId)
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject(endpointId);
                if (masterVol == null)
                    return false;

                masterVol.GetMute(out bool isMuted);
                masterVol.SetMute(!isMuted, Guid.Empty);

                return !isMuted;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        private static IAudioEndpointVolume GetMasterVolumeObject(string endpointId)
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDevice device = null;
            try
            {
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                deviceEnumerator.GetDevice(endpointId, out device);

                Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                device.Activate(IID_IAudioEndpointVolume, 0, IntPtr.Zero, out object o);
                IAudioEndpointVolume masterVol = (IAudioEndpointVolume)o;

                return masterVol;
            }
            finally
            {
                if (device != null) Marshal.ReleaseComObject(device);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        #endregion
    }

    // #region Abstracted COM interfaces from Windows CoreAudio API
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator
    {
    }
}
