using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using IWshRuntimeLibrary;

namespace FS_Launcher
{
    public partial class RPCS3 : Window
    {
        string rpcs3Site = "https://rpcs3.net/";

        string dlRPCS3 = "https://www.dropbox.com/s/cz9p5fi9lg0p6hh/rpcs3.zip?dl=1";
        string dlPS3 = "http://dus01.ps3.update.playstation.net/update/ps3/image/us/2022_0510_95307e1b51d3bcc33a274db91488d29f/PS3UPDAT.PUP";

        // Paths
        private string rootPath;
        private string tempPath;
        private string localAppData;
        private string fsTemp;

        private string defaultRPCS3Path;
        private string rpcs3Path;

        // Files
        private string iconFile;

        private string rpcs3Zip;
        private string rpcs3Exe;

        private string ps3Software;

        // Bools
        bool rpcs3Installed;

        // Other Stuff
        private string strRPCS3;

        public RPCS3()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            fsTemp = Path.Combine(tempPath, "FS Launcher");

            defaultRPCS3Path = Path.Combine(localAppData, "Programs", "rpcs3");

            iconFile = Path.Combine(rootPath, "RPCS3.ico");
            this.Icon = new BitmapImage(new Uri(iconFile, UriKind.Relative));

            rpcs3Zip = Path.Combine(fsTemp, "rpcs3.zip");

            GetCFG();

            if (rpcs3Installed == true)
            {
                MainButton.Content = "Launch";
            }
            else
            {
                MainButton.Content = "Download";
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void RPCS3Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(rpcs3Site);
        }

        private void CloseButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow mwindow = new MainWindow();
            mwindow.Show();
            this.Close();
        }

        private void GetCFG()
        {
            using (RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true))
            {
                if (keyFSL != null)
                {
                    // RPCS3 Installed
                    Object obRPCS3 = keyFSL.GetValue("RPCS3");
                    if (obRPCS3 != null)
                    {
                        strRPCS3 = (obRPCS3 as String);
                    }
                    if (obRPCS3 == null)
                    {
                        strRPCS3 = "0";
                    }

                    if (strRPCS3 == "1")
                    {
                        rpcs3Installed = true;
                    }
                    if (strRPCS3 != "1")
                    {
                        rpcs3Installed = false;
                    }

                    // RPCS3 Path
                    Object obRPCS3Path = keyFSL.GetValue("RPCS3Path");
                    if (obRPCS3Path != null)
                    {
                        rpcs3Path = (obRPCS3Path as String);
                        rpcs3Exe = Path.Combine(rpcs3Path, "rpcs3.exe");
                    }

                    keyFSL.Close();
                }
            }
        }

        private void MinimizeButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);

            if (keyFSL != null)
            {
                Object obRPCS3 = keyFSL.GetValue("RPCS3");
                if (obRPCS3 != null)
                {
                    strRPCS3 = (obRPCS3 as String);
                }
                if (obRPCS3 == null)
                {
                    strRPCS3 = "0";
                }

                if (strRPCS3 == "1")
                {
                    rpcs3Installed = true;
                }
                if (strRPCS3 != "1")
                {
                    rpcs3Installed = false;
                }
            }

            if (rpcs3Installed == true)
            {
                LaunchRPCS3();
            }

            if (rpcs3Installed == false)
            {
                MessageBoxResult rpcs3InstallConfirm = System.Windows.MessageBox.Show("Are you sure you want to download RPCS3?", "RPCS3", System.Windows.MessageBoxButton.YesNo);
                if (rpcs3InstallConfirm == MessageBoxResult.Yes)
                {
                    MessageBox.Show("This will most likely download an outdated version, please update RPCS3 if asked.", "RPCS3", MessageBoxButton.OK, MessageBoxImage.Information);

                    MessageBoxResult rpcs3InstallLocation = System.Windows.MessageBox.Show($"Would you like to install RPCS3 at the recommended location?\n\nRecommended Path: {defaultRPCS3Path}", "RPCS3", System.Windows.MessageBoxButton.YesNo);
                    if (rpcs3InstallLocation == MessageBoxResult.Yes)
                    {
                        rpcs3Path = defaultRPCS3Path;
                        rpcs3Exe = Path.Combine(rpcs3Path, "rpcs3.exe");

                        keyFSL.SetValue("RPCS3Path", $"{rpcs3Path}");
                        keyFSL.Close();

                        DownloadRPCS3();
                    }
                    if (rpcs3InstallLocation == MessageBoxResult.No)
                    {
                        WinForms.FolderBrowserDialog rpcs3InstallPathDialog = new WinForms.FolderBrowserDialog();
                        rpcs3InstallPathDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                        rpcs3InstallPathDialog.Description = "Please select where you would like to install RPCS3.\n\nA folder called \"rpcs3\" will be created.";
                        rpcs3InstallPathDialog.ShowNewFolderButton = true;
                        WinForms.DialogResult rpcs3Result = rpcs3InstallPathDialog.ShowDialog();

                        if (rpcs3Result == WinForms.DialogResult.OK)
                        {
                            rpcs3Path = Path.Combine(rpcs3InstallPathDialog.SelectedPath, "rpcs3");
                            rpcs3Exe = Path.Combine(rpcs3Path, "rpcs3.exe");

                            keyFSL.SetValue("RPCS3Path", $"{rpcs3Path}");
                            keyFSL.Close();

                            DownloadRPCS3();
                        }
                    }
                }
            }
        }

        private void LaunchRPCS3()
        {
            RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);

            if (keyFSL != null)
            {
                Object obRPCS3 = keyFSL.GetValue("RPCS3");
                if (obRPCS3 != null)
                {
                    strRPCS3 = (obRPCS3 as String);
                }
                if (obRPCS3 == null)
                {
                    strRPCS3 = "0";
                }

                if (strRPCS3 == "1")
                {
                    rpcs3Installed = true;
                }
                if (strRPCS3 != "1")
                {
                    rpcs3Installed = false;
                }

                Object obRPCS3Path = keyFSL.GetValue("RPCS3Path");
                if (obRPCS3Path != null)
                {
                    rpcs3Path = (obRPCS3Path as String);
                    rpcs3Exe = Path.Combine(rpcs3Path, "rpcs3.exe");
                }
            }

            if (rpcs3Installed == true)
            {
                if (Directory.Exists(rpcs3Path))
                {
                    try
                    {
                        Process.Start(rpcs3Exe);
                        keyFSL.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        keyFSL.Close();
                        return;
                    }
                }
                else
                {
                    keyFSL.SetValue("RPCS3", "0");
                    MessageBox.Show("RPCS3 does not seem to be installed.\n\nPlease restart FS Launcher.", "RPCS3");
                    keyFSL.Close();
                }
            }
        }

        private void MainButton_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (rpcs3Installed == true)
            {
                MessageBoxResult uninstallRPCS3 = System.Windows.MessageBox.Show("Are you sure you want to uninstall RPCS3?", "RPCS3", System.Windows.MessageBoxButton.YesNo);
                if (uninstallRPCS3 == MessageBoxResult.Yes)
                {
                    if (Directory.Exists(rpcs3Path))
                    {
                        try
                        {
                            Directory.Delete(rpcs3Path, true);

                            RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                            keyFSL.SetValue("RPCS3", "0");
                            keyFSL.Close();

                            MainButton.Content = "Download";
                            MessageBox.Show("Uninstall complete!", "RPCS3", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        catch
                        {
                            MessageBox.Show($"", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void DownloadRPCS3()
        {
            try
            {
                MainButton.IsEnabled = false;
                CloseButton.IsEnabled = false;
                pb.Visibility = Visibility.Visible;
                DLStatus.Visibility = Visibility.Visible;
                DLStatus.Text = "Downloading RPCS3";

                Directory.CreateDirectory(fsTemp);

                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(InstallRPCS3);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri(dlRPCS3), rpcs3Zip);
            }
            catch (Exception ex)
            {
                MainButton.IsEnabled = true;
                CloseButton.IsEnabled = true;
                pb.Visibility = Visibility.Hidden;
                DLStatus.Visibility = Visibility.Hidden;
                DLStatus.Text = "";
                MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void InstallRPCS3(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory(rpcs3Zip, rpcs3Path);
                System.IO.File.Delete(rpcs3Zip);

                pb.Visibility = Visibility.Hidden;
                DLStatus.Visibility = Visibility.Hidden;

                RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                keyFSL.SetValue("RPCS3", "1");
                keyFSL.Close();

                MessageBox.Show("Install complete!", "RPCS3", MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBoxResult rpcs3DesktopShortcut = System.Windows.MessageBox.Show("Would you like to create a desktop shortcut?", "RPCS3", System.Windows.MessageBoxButton.YesNo);
                if (rpcs3DesktopShortcut == MessageBoxResult.Yes)
                {
                    object shDesktop = (object)"Desktop";
                    WshShell shell = new WshShell();
                    string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\RPCS3.lnk";
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                    shortcut.TargetPath = rpcs3Path + "\\rpcs3.exe";
                    shortcut.Save();
                }

                MessageBoxResult dlPS3Software = System.Windows.MessageBox.Show("Would you like to download the PS3 System Software?", "RPCS3", System.Windows.MessageBoxButton.YesNo);
                if (dlPS3Software == MessageBoxResult.Yes)
                {
                    DownloadPS3Software();
                }
                if (dlPS3Software == MessageBoxResult.No)
                {
                    MainButton.IsEnabled = true;
                    MainButton.Content = "Launch";
                    CloseButton.IsEnabled = true;
                    pb.Visibility = Visibility.Hidden;
                    DLStatus.Visibility = Visibility.Hidden;
                    DLStatus.Text = "";
                } 
            }
            catch (Exception ex)
            {
                MainButton.IsEnabled = true;
                CloseButton.IsEnabled = true;
                pb.Visibility = Visibility.Hidden;
                DLStatus.Visibility = Visibility.Hidden;
                DLStatus.Text = "";
                MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void DownloadPS3Software()
        {
            try
            {
                MainButton.IsEnabled = false;
                CloseButton.IsEnabled = false;

                MessageBox.Show("Please select where you would like to save the PS3 System Software.\n\nSelect somewhere you will remember, you will need it later.", "PS3 System Software", MessageBoxButton.OK, MessageBoxImage.Information);

                WinForms.FolderBrowserDialog ps3SoftwarePathDialog = new WinForms.FolderBrowserDialog();
                ps3SoftwarePathDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                ps3SoftwarePathDialog.Description = "Please select where you would like to save the PS3 System Software.";
                ps3SoftwarePathDialog.ShowNewFolderButton = true;
                WinForms.DialogResult ps3SoftwareResult = ps3SoftwarePathDialog.ShowDialog();
                if (ps3SoftwareResult == WinForms.DialogResult.OK)
                {
                    ps3Software = Path.Combine(ps3SoftwarePathDialog.SelectedPath, "PS3UPDAT.PUP");

                    pb.Visibility = Visibility.Visible;
                    DLStatus.Visibility = Visibility.Visible;
                    DLStatus.Text = "Downloading PS3 System Software";

                    WebClient webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadPS3SoftwareCallback);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri(dlPS3), ps3Software);
                }
            }
            catch (Exception ex)
            {
                MainButton.IsEnabled = true;
                CloseButton.IsEnabled = true;
                pb.Visibility = Visibility.Hidden;
                DLStatus.Visibility = Visibility.Hidden;
                DLStatus.Text = "";
                MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void DownloadPS3SoftwareCallback(object sender, AsyncCompletedEventArgs e)
        {
            MainButton.IsEnabled = true;
            MainButton.Content = "Launch";
            CloseButton.IsEnabled = true;
            pb.Visibility = Visibility.Hidden;
            DLStatus.Visibility = Visibility.Hidden;
            DLStatus.Text = "";

            MessageBox.Show("Download complete!", "PS3 System Software", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }
    }
}
