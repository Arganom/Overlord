using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Net;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;

namespace FS_Launcher
{
    public partial class MainWindow : Window
    {
        string launcherVersion = "1.1.1";

        // Paths
        private string rootPath;
        private string tempPath;
        private string fsTemp;

        private string gamePath;

        private string dlcPath;
        private string dlcTemp;
        private string dlcSavePath;

        // Files
        private string fsLauncher;
        private string versionFile;
        private string updater;

        private string grfsExe;

        private string firewallBatCreate;
        private string firewallBatDelete;

        private string dlcBin;
        private string dlcSave;
        private string dlcSaveTemp;
        private string dlcSaveBak;

        // Bools
        bool updateAvailable;
        bool firstRun;
        bool firstRunMessages = true;
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Other Stuff
        string dlc1;
        string dlc2;
        string dlc3;
        string strFirewall;
        string strDLC;

        public MainWindow()
        {
            InitializeComponent();

            DelTemp();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            fsTemp = Path.Combine(tempPath, "FS Launcher");

            fsLauncher = Path.Combine(rootPath, "FS-Launcher.exe");
            versionFile = Path.Combine(rootPath, "version.txt");
            updater = Path.Combine(rootPath, "updater.exe");

            dlcBin = Path.Combine(fsTemp, "dlc.bin");
            dlcTemp = Path.Combine(fsTemp, "DLC");
            dlcSaveTemp = Path.Combine(fsTemp, "1.save");

            firewallBatCreate = Path.Combine(rootPath, "files", "firewall", "windows", "create_firewall_rule_windows.bat");
            firewallBatDelete = Path.Combine(rootPath, "files", "firewall", "windows", "delete_firewall_rule_windows.bat");

            VersionText.Text = $"v{launcherVersion}";

            CreateReg();

            FirstRun();
            CheckShiftKey();
            GetCFG();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void DumpVersion()
        {
            File.WriteAllText(versionFile, launcherVersion);

            RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
            keyFSL.SetValue("Version", $"{launcherVersion}");
            keyFSL.Close();
        }

        private void CreateTemp()
        {
            Directory.CreateDirectory(fsTemp);
        }

        private void DelTemp()
        {
            if (Directory.Exists(fsTemp))
            {
                Directory.Delete(fsTemp, true);
            }
        }

        private void RestartFSL()
        {
            Process.Start(fsLauncher);
            Application.Current.Shutdown();
        }

        private void CreateReg()
        {
            RegistryKey key1 = Registry.CurrentUser.OpenSubKey(@"Software", true);
            key1.CreateSubKey("FS Launcher");

            key1.Close();
        }

        private void GetCFG()
        {
            using (RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher"))
            {
                if (keyFSL != null)
                {
                    // Game Path
                    Object obGRFSPath = keyFSL.GetValue("GRFSPath");
                    if (obGRFSPath != null)
                    {
                        gamePath = (obGRFSPath as String);
                        grfsExe = Path.Combine(gamePath, "Future Soldier.exe");
                    }

                    // DLC
                    Object obDLC = keyFSL.GetValue("DLCSavePath");
                    if (obDLC != null)
                    {
                        strDLC = (obDLC as String);

                        dlcPath = Path.Combine(gamePath, "DLC");
                        dlcSave = Path.Combine(strDLC, "1.save");
                        dlcSaveBak = Path.Combine(strDLC, "1.save.bak");
                    }

                    keyFSL.Close();
                }
            }
        }

        public void CheckShiftKey()
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                Object obFirstRun = keyFSL.GetValue("FirstRun");
                if (obFirstRun != null)
                {
                    string strFirstRun = (obFirstRun as String);
                    if (strFirstRun == "1")
                    {
                        MessageBoxResult resetSoftware = MessageBox.Show("Are you sure you want to reset FS Launcher?", "Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (resetSoftware == MessageBoxResult.Yes)
                        {
                            try
                            {
                                keyFSL.SetValue("FirstRun", "0");
                                keyFSL.Close();

                                RestartFSL();
                            }
                            catch
                            {
                                SystemSounds.Exclamation.Play();
                                MessageBox.Show("Error resetting FS Launcher.");
                                RestartFSL();
                            }
                        }
                    }
                }
            }
        }

        private void FirstRun()
        {
            RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
            Object obFirstRun = keyFSL.GetValue("FirstRun");

            if (obFirstRun == null)
            {
                firstRun = true;
            }

            if (obFirstRun != null)
            {
                string strFirstRun = (obFirstRun as String);
                if (strFirstRun == "0")
                {
                    firstRun = true;
                }
            }

            if (firstRun == true)
            {
                if (firstRunMessages == true)
                {
                    MessageBox.Show("Welcome to FS Launcher!", "Future Soldier Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageBox.Show("Most buttons have a left and right click function, if you would like to undo something you have previously done, try right clicking the button!", "Future Soldier Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                }

                WinForms.FolderBrowserDialog grfsInstallPathDialog = new WinForms.FolderBrowserDialog();
                grfsInstallPathDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                grfsInstallPathDialog.Description = "Please select your Ghost Recon: Future Soldier install folder.\n(Location with Future Soldier.exe)";
                grfsInstallPathDialog.ShowNewFolderButton = false;
                WinForms.DialogResult grfsResult = grfsInstallPathDialog.ShowDialog();

                if (grfsResult == WinForms.DialogResult.OK)
                {
                    gamePath = Path.Combine(grfsInstallPathDialog.SelectedPath);
                    grfsExe = Path.Combine(gamePath, "Future Soldier.exe");

                    if (!File.Exists(grfsExe))
                    {
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show("Please select the location with Future Soldier.exe");
                        firstRunMessages = false;
                        FirstRun();
                    }
                    if (File.Exists(grfsExe))
                    {
                        MessageBox.Show("You can change your install location at any time by holding shift on startup!", "Future Soldier Launcher", MessageBoxButton.OK, MessageBoxImage.Information);

                        keyFSL.SetValue("GRFSPath", gamePath);
                        keyFSL.SetValue("FirstRun", "1");
                        keyFSL.Close();
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            keyFSL.Close();
        }

        private void CheckForUpdates()
        {
            DumpVersion();

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://pastebin.com/raw/rg2GbmNL"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        updateAvailable = true;
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
                        updateAvailable = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for updates:\n{ex}", "Error");
                }
            }
            else
            {
                updateAvailable = true;
                InstallUpdate(false, Version.zero);
            }
        }
        private void InstallUpdate(bool isUpdate, Version _onlineVersion)
        {
            try
            {
                LauncherUpdate updateWindow = new LauncherUpdate();
                updateWindow.Show();
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show($"Error: {ex}");
            }
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(grfsExe);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching Ghost Recon: Future Soldier:\n\n{ex}");
            }
        }

        private void FirewallButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult firewallMessageBox1 = System.Windows.MessageBox.Show("Are you sure you want to setup the Firewall Rule in Windows?", "Firewall", System.Windows.MessageBoxButton.YesNo);
            if (firewallMessageBox1 == MessageBoxResult.Yes)
            {
                strFirewall = "0";
                RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                Object obFirewall = keyFSL.GetValue("Firewall");

                if (obFirewall == null)
                {
                    strFirewall = "0";
                }

                if (obFirewall != null)
                {
                    strFirewall = (obFirewall as String);
                }

                if (strFirewall != "1")
                {
                    keyFSL.SetValue("Firewall", "1");
                    keyFSL.Close();
                    ExecuteAsAdmin(firewallBatCreate);
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Firewall Rule successfully created!", "Firewall");
                }
                else
                {
                    keyFSL.Close();
                    MessageBox.Show("The firewall rule seems to already exist. Try deleting it first.");
                    return;
                }
            }
        }

        private void FirewallButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult firewallMessageBox2 = System.Windows.MessageBox.Show("Are you sure you want to delete the Firewall Rule in Windows?", "Firewall", System.Windows.MessageBoxButton.YesNo);
            if (firewallMessageBox2 == MessageBoxResult.Yes)
            {
                strFirewall = "0";
                RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                Object obFirewall = keyFSL.GetValue("Firewall");

                if (obFirewall == null)
                {
                    strFirewall = "0";
                }

                if (obFirewall != null)
                {
                    strFirewall = (obFirewall as String);
                }

                if (strFirewall != "0")
                {
                    keyFSL.SetValue("Firewall", "0");
                    keyFSL.Close();
                    ExecuteAsAdmin(firewallBatDelete);
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Firewall Rule successfully deleted!", "Firewall");
                }
                else
                {
                    keyFSL.Close();
                    MessageBox.Show("The firewall rule does not seem to exist. Try creating it first.");
                    return;
                }
            }
        }

        private void UnlockDLCButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult unlockDLC = MessageBox.Show("Are you sure you want to download and unlock the DLC?\n\nDownload Size: 1.76 GB\nExtracted Size: 2.43 GB", "Unlock DLC", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (unlockDLC == MessageBoxResult.Yes)
            {
                RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                Object obDLC = keyFSL.GetValue("DLC");

                if (obDLC == null)
                {
                    strDLC = "0";
                }

                if (obDLC != null)
                {
                    strDLC = (obDLC as String);
                }

                if (strDLC == "1")
                {
                    MessageBox.Show("DLC seems to already be unlocked.", "DLC Unlocker");
                    return;
                }

                string ubiInstallDir = "C:\\Program Files(x86)\\Ubisoft\\Ubisoft Game Launcher\\";

                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Ubisoft\\Launcher"))
                {
                    if (regKey != null)
                    {
                        Object obUbiPath = regKey.GetValue("InstallDir");
                        if (obUbiPath != null)
                        {
                            ubiInstallDir = (obUbiPath as String);
                        }
                    }
                }

                MessageBox.Show($"Please select the following folder:\n{ubiInstallDir}savegames\\USER-ID\n\nIt should be a folder named with random characters.", "DLC Unlocker");

                WinForms.FolderBrowserDialog ubisoftIDDialog = new WinForms.FolderBrowserDialog();
                ubisoftIDDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                ubisoftIDDialog.Description = "Please select the following folder:\nC:\\Program Files(x86)\\Ubisoft\\Ubisoft Game Launcher\\savegames\\USER-ID\n\nIt should be a folder named with random characters.";
                ubisoftIDDialog.ShowNewFolderButton = false;
                WinForms.DialogResult ubiIDResult = ubisoftIDDialog.ShowDialog();

                if (ubiIDResult == WinForms.DialogResult.OK)
                {
                    dlcSavePath = Path.Combine(ubisoftIDDialog.SelectedPath);
                    DirectoryInfo dir_info = new DirectoryInfo(dlcSavePath);
                    string directory = dir_info.Name;

                    if (directory != "53")
                    {
                        dlcSavePath = Path.Combine(ubisoftIDDialog.SelectedPath, "53");

                        if (!Directory.Exists(dlcSavePath))
                        {
                            MessageBox.Show("Please select the correct folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    dlcPath = Path.Combine(gamePath, "DLC");
                    dlcSave = Path.Combine(dlcSavePath, "1.save");
                    dlcSaveBak = Path.Combine(dlcSavePath, "1.save.bak");
                    keyFSL.SetValue("DLCSavePath", $"{dlcSavePath}");
                    keyFSL.Close();

                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("The download will now start, please do not close FS Launcher until it finishes.", "DLC Unlocker");

                    CloseButton.IsEnabled = false;
                    LaunchButton.IsEnabled = false;
                    FirewallButton.IsEnabled = false;
                    UnlockDLCButton.IsEnabled = false;
                    PunkbusterButton.IsEnabled = false;
                    RPCS3Button.IsEnabled = false;
                    pb.Visibility = Visibility.Visible;

                    CreateTemp();
                    Directory.CreateDirectory(dlcPath);

                    WebClient webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DLCDownloadCompletedCallback);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/rdmjk53ozbm8agn/dlc.bin?dl=1"), dlcBin);
                }
            }
        }

        private void DLCDownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory(dlcBin, fsTemp);
                File.Delete(dlcBin);

                foreach (string dirPath in Directory.GetDirectories(dlcTemp, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(dlcTemp, dlcPath));
                }

                foreach (string newPath in Directory.GetFiles(dlcTemp, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(dlcTemp, dlcPath), true);
                }

                Directory.Delete(dlcTemp, true);

                File.Copy(dlcSave, dlcSaveBak);
                File.Delete(dlcSave);
                File.Copy(dlcSaveTemp, dlcSave);

                RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
                keyFSL.SetValue("DLC", "1");
                keyFSL.Close();

                CloseButton.IsEnabled = true;
                LaunchButton.IsEnabled = true;
                FirewallButton.IsEnabled = true;
                UnlockDLCButton.IsEnabled = true;
                PunkbusterButton.IsEnabled = true;
                RPCS3Button.IsEnabled = true;
                pb.Visibility = Visibility.Hidden;

                SystemSounds.Exclamation.Play();
                MessageBox.Show("DLC Unlocked!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n\n{ex}");
            }
        }

        private void UnlockDLCButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RegistryKey keyFSL = Registry.CurrentUser.OpenSubKey(@"Software\FS Launcher", true);
            Object obDLC = keyFSL.GetValue("DLC");

            if (obDLC == null)
            {
                strDLC = "0";
            }

            if (obDLC != null)
            {
                strDLC = (obDLC as String);
            }

            if (strDLC != "1")
            {
                return;
            }

            MessageBoxResult deleteDLC = MessageBox.Show("Are you sure you want to delete the DLC?", "DLC", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (deleteDLC == MessageBoxResult.Yes)
            {
                File.Delete(dlcSave);
                File.Copy(dlcSaveBak, dlcSave);
                File.Delete(dlcSaveBak);

                dlc1 = Path.Combine(dlcPath, "dlc1");
                dlc2 = Path.Combine(dlcPath, "dlc2");
                dlc3 = Path.Combine(dlcPath, "dlc3");

                Directory.Delete(dlc1, true);
                Directory.Delete(dlc2, true);
                Directory.Delete(dlc3, true);

                keyFSL.SetValue("DLC", "0");
                keyFSL.Close();

                SystemSounds.Exclamation.Play();
                MessageBox.Show("DLC deleted!");
            }
        }

        private void PunkbusterButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAdministrator())
            {
                Punkbuster pnkbstrWindow = new Punkbuster();
                this.Close();
                pnkbstrWindow.Show();
            }
            else
            {
                MessageBox.Show("Please run FS Launcher as Admin before running Punkbuster.", "Punkbuster", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void RPCS3Button_Click(object sender, RoutedEventArgs e)
        {
            RPCS3 rpcs3Window = new RPCS3();
            this.Close();
            rpcs3Window.Show();
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }

        private void GitHubLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/KilLo445/FS-Launcher");
        }

       
        

        private void VersionText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult checkUpdateLMB = MessageBox.Show("Do you want to check for updates?", "Check for updates", MessageBoxButton.YesNo);
            if (checkUpdateLMB == MessageBoxResult.Yes)
            {
                CheckForUpdates();

                if (updateAvailable == false)
                {
                    MessageBox.Show("No update is available.", "Check for updates");
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DelTemp();
        }

        public void ExecuteAsAdmin(string adminFileName)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = adminFileName;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
            catch
            {
                MessageBox.Show($"Error launching as admin.\nPlease accept admin prompt.\n\nFile: {adminFileName}", "Error");
                return;
            }
        }

        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }
    }
}