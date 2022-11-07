using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FS_Launcher
{
    public partial class Punkbuster : Window
    {
        // Paths
        private string rootPath;
        private string tempPath;
        private string fsTemp;
        private string sys32;
        private string gamePath;
        private string pnkbstrBin;

        // Files
        private string iconFile;
        
        private string pbA;
        private string pbB;
        private string pbAsys;
        private string pbBsys;

        private string pbsvcGame;
        private string pbsvc;

        // Bools
        bool pbAInst;
        bool pbBInst;

        public Punkbuster()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            fsTemp = Path.Combine(tempPath, "FS Launcher");
            sys32 = Environment.SystemDirectory;

            pnkbstrBin = Path.Combine(rootPath, "files", "punkbuster");

            pbA = Path.Combine(pnkbstrBin, "PnkBstrA.exe");
            pbB = Path.Combine(pnkbstrBin, "PnkBstrB.exe");
            pbAsys = Path.Combine(sys32, "PnkBstrA.exe");
            pbBsys = Path.Combine(sys32, "PnkBstrB.exe");

            GetGamePath();
            pbsvcGame = Path.Combine(gamePath, "pbsvc.exe");
            pbsvc = Path.Combine(pnkbstrBin, "pbsvc.exe");

            iconFile = Path.Combine(rootPath, "PnkBstr.ico");
            this.Icon = new BitmapImage(new Uri(iconFile, UriKind.Relative));

            pbAInst = false;
            pbBInst = false;
        }

        private void GetGamePath()
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
                    }

                    keyFSL.Close();
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }

        private void PnkBstrLogo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://www.evenbalance.com/");
        }

        private void CheckSys32_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult checkAB = MessageBox.Show("Are you sure you want to check the installation of PnkBstrA and PnkBstrB?", "Punkbuster", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (checkAB == MessageBoxResult.Yes)
            {
                pbAInst = false;
                pbBInst = false;

                if (File.Exists(pbAsys))
                {
                    pbAInst = true;
                }

                if (File.Exists(pbBsys))
                {
                    pbBInst = true;
                }

                if (pbAInst == true && pbBInst == true)
                {
                    MessageBox.Show("PnkBstrA.exe and PnkBstrB.exe seem to be installed correctly!");
                    return;
                }
                else
                {
                    if (pbAInst == false || pbBInst == false)
                    {
                        MessageBoxResult installPnkBstrAB = MessageBox.Show("One or more of the files have not been found, would you like to install them?", "Punkbuster", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (installPnkBstrAB == MessageBoxResult.Yes)
                        {
                            if (pbAInst == false)
                            {
                                try
                                {
                                    File.Copy(pbA, pbAsys);
                                    pbAInst = true;
                                }
                                catch { }
                            }
                            if (pbBInst == false)
                            {
                                try
                                {
                                    File.Copy(pbB, pbBsys);
                                    pbBInst = true;
                                }
                                catch { }
                            }
                            if (pbAInst == true && pbBInst == true)
                            {
                                MessageBox.Show("Both files sucsessfully installed!", "Punkbuster");
                                return;
                            }
                            else
                            {
                                MessageBox.Show("One or more of the files failed to install, please try again.", "Punkbuster");
                                return;
                            }
                        }
                    }
                }
            }
        }
        private void CheckSys32_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBoxResult forcePBinstall = MessageBox.Show("Are you sure you want to force the installation of PnkBstrA and PnkBstrB?\n\nPlease only do this if the main install fails.", "Punkbuster", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (forcePBinstall == MessageBoxResult.Yes)
            {
                pbAInst = false;
                pbBInst = false;

                if (File.Exists(pbAsys))
                {
                    try
                    {
                        File.Delete(pbAsys);
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error:\n\n{ex}");
                    }
                }

                if (File.Exists(pbBsys))
                {
                    try
                    {
                        File.Delete(pbBsys);
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error:\n\n{ex}");
                    }
                }

                try
                {
                    File.Copy(pbA, pbAsys);
                    pbAInst = true;
                }
                catch { }

                try
                {
                    File.Copy(pbB, pbBsys);
                    pbBInst = true;
                }
                catch { }

                if (pbAInst == true && pbBInst == true)
                {
                    MessageBox.Show("Both files sucsessfully installed!", "Punkbuster");
                    return;
                }
                else
                {
                    MessageBox.Show("One or more of the files failed to install, please try again.", "Punkbuster");
                    return;
                }
            }  
        }

        private void TestInstall_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(pbsvcGame))
            {
                try
                {
                    Process.Start(pbsvcGame);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error\n{ex}");
                }
            }
            else
            {
                try
                {
                    Process.Start(pbsvc);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error\n{ex}");
                }
            }
        }
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mwindow = new MainWindow();
            mwindow.Show();
            this.Close();
        }
    }
}
