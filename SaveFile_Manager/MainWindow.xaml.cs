using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace SaveFile_Manager {
    public partial class MainWindow : Window {
        private string saveFilePath = @"";

        private const string CustomSaveFilesFolderName = "CustomSaveFiles";

        public MainWindow()
        {
            InitializeComponent();
            string appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            saveFilePath = Path.Combine(appDataLocal, "Sandfall", "Saved", "SaveGames");
            RefreshCustomSaveDataList();
        }

        private void refreshData_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(saveFilePath))
            {
                RefreshCustomSaveDataList();
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
                string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
                string sourceBackupFolder = Path.Combine(customSaveFilesPath, selectedFolderName);
                string destSteamIdFolder = Path.Combine(saveFilePath, Path.GetFileName(GetSteamIdFolder()));

                if (string.IsNullOrEmpty(GetSteamIdFolder()))
                {
                    ShowDialogMessage("Error: Steam ID folder not found. Unable to load backup.");
                    return;
                }

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
                ShowDialogMessage("Error during loading backup:\n" + ex.Message);
            }
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

        string GetSteamIdFolder()
        {
            string appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string saveGamesPath = Path.Combine(appDataLocal, "Sandfall", "Saved", "SaveGames");

            return Directory.GetDirectories(saveGamesPath)
                            .FirstOrDefault(d => Path.GetFileName(d).StartsWith("765"));
        }

        private void saveBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(saveFilePath) || !Directory.Exists(saveFilePath))
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

            string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
            string targetFolder = Path.Combine(customSaveFilesPath, backupName);

            try
            {
                if (!Directory.Exists(customSaveFilesPath))
                {
                    Directory.CreateDirectory(customSaveFilesPath);
                }
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
                ShowDialogMessage("Error saving backup:\n" + ex.Message);
            }
        }

        private void RefreshCustomSaveDataList()
        {
            backedUpFiles.Items.Clear();
            string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
            if (Directory.Exists(customSaveFilesPath))
            {
                foreach (string dir in Directory.GetDirectories(customSaveFilesPath))
                {
                    backedUpFiles.Items.Add(Path.GetFileName(dir));
                }
            }
            else
            {
                ShowDialogMessage("CustomSaveFiles folder not found.");
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

        private void openExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(saveFilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = saveFilePath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                ShowDialogMessage("SaveGames folder not found.");
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
                string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
                string folderToDelete = Path.Combine(customSaveFilesPath, selectedFolderName);

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
                ShowDialogMessage("Error during deletion:\n" + ex.Message);
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
