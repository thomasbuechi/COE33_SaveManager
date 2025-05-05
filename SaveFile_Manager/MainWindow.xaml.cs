using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace SaveFile_Manager {
    public partial class MainWindow : Window {
        private readonly string _baseSaveFilePath;
        private string SaveFilePath => Path.Combine(_baseSaveFilePath, "Sandfall", "Saved", "SaveGames");
        private string CustomSaveFilesPath => Path.Combine(SaveFilePath, "CustomSaveFiles");

        public MainWindow()
        {
            InitializeComponent();
            _baseSaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCustomSaveDataList(notifyUser: true);
        }

        private void refreshData_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SaveFilePath))
            {
                RefreshCustomSaveDataList(notifyUser: true);
            }
            else
            {
                ShowDialogMessage("Invalid base folder.");
            }
        }

        private void loadBackup_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolderName = backedUpFiles.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedFolderName))
            {
                ShowDialogMessage("No backup selected.");
                return;
            }

            if (!ShowConfirmationDialog("This will replace your current save folder. Do you want to continue?"))
                return;

            try
            {
                EnsureCustomSaveFilesFolder();
                string sourceBackupFolder = Path.Combine(CustomSaveFilesPath, selectedFolderName);
                string steamIdFolder = GetSteamIdFolder();

                if (string.IsNullOrEmpty(steamIdFolder))
                {
                    ShowDialogMessage("Error: Steam ID folder not found. Unable to load backup.");
                    return;
                }

                string destSteamIdFolder = Path.Combine(SaveFilePath, Path.GetFileName(steamIdFolder));

                if (Directory.Exists(destSteamIdFolder))
                {
                    Directory.Delete(destSteamIdFolder, true);
                }

                CopyDirectory(sourceBackupFolder, destSteamIdFolder);

                ShowDialogMessage("Backup loaded successfully!");
                RefreshCustomSaveDataList();
            }
            catch (Exception ex)
            {
                ShowDialogMessage($"Error during loading backup:\n{ex.Message}");
            }
        }

        private void saveBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SaveFilePath) || !Directory.Exists(SaveFilePath))
            {
                ShowDialogMessage("Invalid save folder.");
                return;
            }

            var inputDialog = new InputDialog("") { Owner = this };
            if (inputDialog.ShowDialog() != true)
                return;

            string backupName = inputDialog.InputText.Trim();
            if (string.IsNullOrEmpty(backupName))
            {
                ShowDialogMessage("Backup name cannot be empty.");
                return;
            }

            string steamIdFolder = GetSteamIdFolder();
            if (string.IsNullOrEmpty(steamIdFolder))
            {
                ShowDialogMessage("Steam ID folder not found.");
                return;
            }

            EnsureCustomSaveFilesFolder();
            string targetFolder = Path.Combine(CustomSaveFilesPath, backupName);

            try
            {
                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                }

                DirectoryCopy(steamIdFolder, targetFolder, true);
                RefreshCustomSaveDataList();
                ShowDialogMessage("Backup saved successfully!");
            }
            catch (Exception ex)
            {
                ShowDialogMessage($"Error saving backup:\n{ex.Message}");
            }
        }

        private void deleteSelectedBackupFile_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolderName = backedUpFiles.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedFolderName))
            {
                ShowDialogMessage("No backup selected.");
                return;
            }

            if (!ShowConfirmationDialog($"Delete '{selectedFolderName}'?"))
                return;

            try
            {
                EnsureCustomSaveFilesFolder();
                string folderToDelete = Path.Combine(CustomSaveFilesPath, selectedFolderName);

                if (Directory.Exists(folderToDelete))
                {
                    Directory.Delete(folderToDelete, true);
                    RefreshCustomSaveDataList();
                    ShowDialogMessage($"Backup '{selectedFolderName}' deleted successfully.");
                }
                else
                {
                    ShowDialogMessage("Error: Selected backup folder not found.");
                }
            }
            catch (Exception ex)
            {
                ShowDialogMessage($"Error during deletion:\n{ex.Message}");
            }
        }

        private void openExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SaveFilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SaveFilePath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                ShowDialogMessage("SaveGames folder not found.");
            }
        }

        private void RefreshCustomSaveDataList(bool notifyUser = false)
        {
            backedUpFiles.Items.Clear();
            if (!Directory.Exists(CustomSaveFilesPath))
            {
                Directory.CreateDirectory(CustomSaveFilesPath);
                if (notifyUser)
                {
                    ShowDialogMessage("The custom backup folder was not found, so a new one has been created.");
                }
            }

            if (Directory.Exists(CustomSaveFilesPath))
            {
                foreach (string dir in Directory.GetDirectories(CustomSaveFilesPath))
                {
                    backedUpFiles.Items.Add(Path.GetFileName(dir));
                }
            }
        }

        private void EnsureCustomSaveFilesFolder()
        {
            if (!Directory.Exists(CustomSaveFilesPath))
            {
                Directory.CreateDirectory(CustomSaveFilesPath);
            }
        }

        private string GetSteamIdFolder()
        {
            if (!Directory.Exists(SaveFilePath))
            {
                return null;
            }
            return Directory.GetDirectories(SaveFilePath)
                            .FirstOrDefault(d => Path.GetFileName(d).StartsWith("765"));
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            foreach (FileInfo file in dir.GetFiles())
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void ShowDialogMessage(string message)
        {
            ShowDialog(message, isConfirmation: false);
        }

        private bool ShowConfirmationDialog(string message)
        {
            return ShowDialog(message, isConfirmation: true) == MessageBoxResult.Yes;
        }

        private MessageBoxResult ShowDialog(string message, bool isConfirmation)
        {
            var dialog = new ConfirmDialog(message, isConfirmation)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}