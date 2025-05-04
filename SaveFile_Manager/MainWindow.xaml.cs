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
            string appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            saveFilePath = Path.Combine(appDataLocal, "Sandfall", "Saved", "SaveGames");
            InitializeComponent();
            RefreshLists();
        }


        private void refreshData_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(saveFilePath))
            {
                RefreshLists();
            }
            else
            {
                MessageBox.Show("Invalid base folder.");
            }
        }

        private void loadBackup_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolderName = backedUpFiles.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedFolderName))
            {
                MessageBox.Show("No backup selected.");
                return;
            }

            // Create the custom confirmation dialog for loading
            var confirmDialog = new ConfirmDialog($"This will replace your current save folder. Do you want to continue?");
            confirmDialog.Owner = this; // Set the parent window (MainWindow) for positioning
            confirmDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner; // Position it relative to MainWindow
            confirmDialog.ShowDialog(); // Show the dialog

            // Proceed based on the user's response
            if (confirmDialog.Result == MessageBoxResult.Yes)
            {
                try
                {
                    // Define the path for CustomSaveFiles
                    string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
                    string sourceBackupFolder = Path.Combine(customSaveFilesPath, selectedFolderName);
                    string destSteamIdFolder = Path.Combine(saveFilePath, Path.GetFileName(GetSteamIdFolder()));

                    // Check if the Steam ID folder exists
                    if (string.IsNullOrEmpty(GetSteamIdFolder()))
                    {
                        // Show error if the Steam ID folder does not exist
                        MessageBox.Show("Error: Steam ID folder not found. Unable to load backup.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Stop the backup loading process if the folder doesn't exist
                    }

                    // Delete existing save folder
                    if (Directory.Exists(destSteamIdFolder))
                    {
                        Directory.Delete(destSteamIdFolder, true);
                    }

                    // Copy the selected backup folder to the destination
                    CopyDirectory(sourceBackupFolder, destSteamIdFolder);

                    // Show the confirmation dialog instead of MessageBox
                    var confirmDialogSuccess = new ConfirmDialog("Backup loaded successfully!");
                    confirmDialogSuccess.Owner = this;
                    confirmDialogSuccess.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    confirmDialogSuccess.ShowDialog(); // Show the dialog after success

                    RefreshLists(); // Refresh the list of backups after loading
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during loading backup: " + ex.Message);
                }
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
                MessageBox.Show("Invalid save folder.");
                return;
            }

            var inputDialog = new InputDialog("") { Owner = this };
            if (inputDialog.ShowDialog() != true)
                return;

            string backupName = inputDialog.InputText.Trim();
            if (string.IsNullOrEmpty(backupName))
            {
                MessageBox.Show("Backup name cannot be empty.");
                return;
            }

            // Get the Steam ID folder
            string steamIdFolder = GetSteamIdFolder();
            if (string.IsNullOrEmpty(steamIdFolder))
            {
                MessageBox.Show("Steam ID folder not found.");
                return;
            }

            string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
            string targetFolder = Path.Combine(customSaveFilesPath, backupName);

            try
            {
                // Ensure the CustomSaveFiles directory exists
                if (!Directory.Exists(customSaveFilesPath))
                {
                    Directory.CreateDirectory(customSaveFilesPath);
                }

                // Delete the target folder if it exists
                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                }

                // Copy only the Steam ID folder
                DirectoryCopy(steamIdFolder, targetFolder, true);

                // Refresh the backup list
                RefreshLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving backup: " + ex.Message);
            }
        }


        private void RefreshLists()
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
                MessageBox.Show("CustomSaveFiles folder not found.");
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
                MessageBox.Show("SaveGames folder not found.");
            }
        }

        private void deleteSelectedBackupFile_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolderName = backedUpFiles.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedFolderName))
            {
                MessageBox.Show("No backup selected.");
                return;
            }

            // Create the custom confirmation dialog for deletion
            var confirmDialog = new ConfirmDialog($"Delete '{selectedFolderName}'?");
            confirmDialog.Owner = this; // Set the parent window (MainWindow) for positioning
            confirmDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner; // Position it relative to MainWindow
            confirmDialog.ShowDialog(); // Show the dialog

            // Proceed based on the user's response
            if (confirmDialog.Result == MessageBoxResult.Yes)
            {
                try
                {
                    // Perform the delete operation as usual
                    string customSaveFilesPath = Path.Combine(saveFilePath, "CustomSaveFiles");
                    string folderToDelete = Path.Combine(customSaveFilesPath, selectedFolderName);

                    if (Directory.Exists(folderToDelete))
                    {
                        Directory.Delete(folderToDelete, true);
                        RefreshLists(); // Update the ListBox

                        // Show the success confirmation dialog
                        var confirmDialogSuccess = new ConfirmDialog($"Backup '{selectedFolderName}' deleted successfully.");
                        confirmDialogSuccess.Owner = this;
                        confirmDialogSuccess.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        confirmDialogSuccess.ShowDialog(); // Show the dialog after success
                    }
                    else
                    {
                        MessageBox.Show("Error: Selected backup folder not found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during deletion: " + ex.Message);
                }
            }
        }



    }
}
