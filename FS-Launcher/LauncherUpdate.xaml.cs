using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace FS_Launcher
{
    public partial class LauncherUpdate : Window
    {
        // Directories
        private string rootPath;

        // Files
        private string updater;
        private string verFile;

        // Versions
        private string installedVer;
        private string updateVer;

        // Bools
        bool regVer;

        public LauncherUpdate()
        {
            InitializeComponent();

            this.Topmost = true;

            rootPath = Directory.GetCurrentDirectory();

            verFile = Path.Combine(rootPath, "version.txt");
            updater = Path.Combine(rootPath, "updater.exe");

            GetVersion();
        }

        private void GetVersion()
        {
            WebClient webClient = new WebClient();
            updateVer = webClient.DownloadString("https://pastebin.com/raw/rg2GbmNL");

            using (RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher"))
            {
                if (keyFSL != null)
                {
                    Object obVer = keyFSL.GetValue("Version");
                    if (obVer != null)
                    {
                        installedVer = (obVer as String);
                        regVer = true;
                    }
                    else
                    {
                        regVer = false;
                    }

                    keyFSL.Close();
                }
                else
                {
                    regVer = false;
                }
            }

            if (regVer == true)
            {
                InstalledVersion.Text = $"Installed Version: v{installedVer}";
            }

            if (regVer == false)
            {
                if (File.Exists(verFile))
                {
                    installedVer = File.ReadAllText(verFile);
                    InstalledVersion.Text = $"Installed Version: v{installedVer}";
                }
                else
                {
                    InstalledVersion.Text = $"Installed Version: Unknown";
                }
            }

            UpdateVersion.Text = $"Latest Version: v{updateVer}";
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(updater))
            {
                Process.Start(updater);
                Application.Current.Shutdown();
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
