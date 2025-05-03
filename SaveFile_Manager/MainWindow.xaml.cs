using System;
using System.IO;
using System.Text;
using System.Windows;

namespace SaveFile_Manager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string saveFilePath = @"";
        private string settingsFile = "config.txt"; // You can change this to a more specific path


        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(settingsFile))
            {
                saveFilePath = File.ReadAllText(settingsFile).Trim();
            }
            else
            {
                saveFilePath = ""; // Or set to some default
            }

            dataPath.Text = saveFilePath;
            RefreshLists();
            PopulateExpeditionComboBox();
        }

        private void setPath_Click(object sender, RoutedEventArgs e)
        {
            saveFilePath = dataPath.Text;
            if (Directory.Exists(saveFilePath))
            {
                File.WriteAllText(settingsFile, saveFilePath); // Save to settings file
                RefreshLists();
            }
            else
            {
                MessageBox.Show("Invalid base folder.");
            }
        }

        private void refreshData_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the base folder exists
            if (Directory.Exists(saveFilePath))
            {
                // Get the selected expedition filter from the ComboBox
                string expeditionFilter = expeditionComboBox.SelectedItem as string;

                // Call RefreshLists() with the selected expedition filter to refresh both folders
                RefreshLists(expeditionFilter);
            }
            else
            {
                MessageBox.Show("Invalid base folder.");
            }
        }

        private void loadBackup_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the base path is valid
            string currentFolder = dataPath.Text.Trim();
            if (string.IsNullOrEmpty(currentFolder) || !Directory.Exists(currentFolder))
            {
                MessageBox.Show("Invalid base folder.");
                return;
            }

            // Get the selected (focused) file from the backedUpFiles ListBox
            string selectedFile = backedUpFiles.SelectedItem as string;

            // Check if a file is selected
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("No file selected.");
                return;
            }

            // Ask for confirmation before proceeding with the load action
            MessageBoxResult result = MessageBox.Show(
                "Do you want to load this file?",
                "Load Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            // If the user clicked Yes, proceed with loading the file
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Go up one level from the current folder to get to the parent directory
                    string parentFolder = Directory.GetParent(currentFolder).FullName;

                    // Construct the path to CustomSaveFiles
                    string customSaveFilesPath = Path.Combine(parentFolder, "CustomSaveFiles");

                    // Ensure the file exists in the CustomSaveFiles folder
                    string sourceFile = Path.Combine(customSaveFilesPath, selectedFile);
                    if (!File.Exists(sourceFile))
                    {
                        MessageBox.Show("Selected file does not exist in CustomSaveFiles.");
                        return;
                    }

                    // Define the target folder for backup (same level as CustomSaveFiles)
                    string targetFolder = Path.Combine(parentFolder, "Backup");
                    Directory.CreateDirectory(targetFolder);

                    // Remove the prefix added during the save process
                    string cleanedFileName = RemovePrefix(selectedFile);

                    // Define the destination path for the file
                    string destFile = Path.Combine(targetFolder, cleanedFileName);

                    // Copy the file to the destination folder
                    File.Copy(sourceFile, destFile, true);

                    RefreshLists();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading file: " + ex.Message);
                }
            }
            else
            {
                // If the user clicked No, do nothing and return
                MessageBox.Show("Load operation cancelled.");
            }
        }



        private void saveBackup_Click(object sender, RoutedEventArgs e)
        {
            string currentFolder = dataPath.Text.Trim();

            if (string.IsNullOrEmpty(currentFolder) || !Directory.Exists(currentFolder))
            {
                MessageBox.Show("Invalid base folder.");
                return;
            }

            string selectedFile = currentSaveFiles.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("No file selected.");
                return;
            }

            string sourceFile = Path.Combine(currentFolder, selectedFile);
            if (!File.Exists(sourceFile))
            {
                MessageBox.Show("Selected file does not exist.");
                return;
            }

            // Show input popup — no extra button in the main UI
            var inputDialog = new InputDialog("") { Owner = this };
            if (inputDialog.ShowDialog() != true)
                return;

            string prefix = inputDialog.InputText.Trim();

            try
            {
                string parentFolder = Directory.GetParent(currentFolder).FullName;
                string targetFolder = Path.Combine(parentFolder, "CustomSaveFiles");
                Directory.CreateDirectory(targetFolder);

                string fileName = Path.GetFileName(selectedFile);

                // Clean the prefix by removing any trailing underscores
                string cleanedPrefix = CleanPrefix(prefix);

                // Combine cleaned prefix with the file name
                string targetFileName = cleanedPrefix + "_" + fileName;

                string destFile = Path.Combine(targetFolder, targetFileName);

                // Copy the file to the destination folder
                File.Copy(sourceFile, destFile, true);


                // Refresh the lists after saving a backup
                RefreshLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
        }


        private string CleanPrefix(string prefix)
        {
            // Trim whitespace
            prefix = prefix.Trim();

            // Remove only the underscore if the prefix ends with one and is followed by EXPEDITION later
            if (prefix.EndsWith("_"))
            {
                prefix = prefix.TrimEnd('_');
            }

            return prefix;
        }

        private string RemovePrefix(string fileName)
        {
            fileName = fileName.Trim();

            int index = fileName.IndexOf("EXPEDITION_", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return fileName.Substring(index); // Keep "EXPEDITION_..." onward
            }

            // If "EXPEDITION_" isn't found, fallback to original behavior
            int underscoreIndex = fileName.IndexOf("_");
            if (underscoreIndex > 0)
            {
                return fileName.Substring(underscoreIndex + 1);
            }

            return fileName;
        }


        private void refreshDataBackedUp_Click(object sender, RoutedEventArgs e)
        {
            string customSaveFilesPath = @"C:\Users\balli\AppData\Local\Sandfall\Saved\SaveGames\76561197984330337\CustomSaveFiles";
            string keyword = "CustomSaveFiles";
            int index = customSaveFilesPath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return;
            }

            int start = index + keyword.Length;
            string trimmedPath = customSaveFilesPath.Length > start + 1 ? customSaveFilesPath.Substring(start + 1) : string.Empty;

            if (Directory.Exists(customSaveFilesPath))
            {
                foreach (string filePath in Directory.GetFiles(customSaveFilesPath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = filePath.Substring(index + keyword.Length + 1);
                    backedUpFiles.Items.Add(relativePath);
                }
            }
            RefreshLists();
        }

        // Method to refresh both lists (currentSaveFiles and backedUpFiles)
        private void RefreshLists()
        {
            currentSaveFiles.Items.Clear();
            backedUpFiles.Items.Clear();

            // Refresh current save files
            if (Directory.Exists(saveFilePath))
            {
                string keyword = "Backup";
                int index = saveFilePath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    foreach (string filePath in Directory.GetFiles(saveFilePath, "*", SearchOption.AllDirectories))
                    {
                        string relativePath = filePath.Substring(index + keyword.Length + 1);
                        currentSaveFiles.Items.Add(relativePath);
                    }
                }
            }

            // Refresh backed-up files
            string customSaveFilesPath = @"C:\Users\balli\AppData\Local\Sandfall\Saved\SaveGames\76561197984330337\CustomSaveFiles";
            string keywordBackedUp = "CustomSaveFiles";
            int indexBackedUp = customSaveFilesPath.IndexOf(keywordBackedUp, StringComparison.OrdinalIgnoreCase);
            if (indexBackedUp >= 0)
            {
                foreach (string filePath in Directory.GetFiles(customSaveFilesPath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = filePath.Substring(indexBackedUp + keywordBackedUp.Length + 1);
                    backedUpFiles.Items.Add(relativePath);
                }
            }
        }


        private void PopulateExpeditionComboBox()
        {
            // Populate the ComboBox with available expeditions based on the files in the "Backup" directory
            expeditionComboBox.Items.Clear();

            // Add a default "All Expeditions" option
            expeditionComboBox.Items.Add("All Expeditions");

            // Get all unique expedition numbers from the save files
            HashSet<string> expeditions = new HashSet<string>();
            if (Directory.Exists(saveFilePath))
            {
                string keyword = "Backup";
                int index = saveFilePath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    foreach (string filePath in Directory.GetFiles(saveFilePath, "*", SearchOption.AllDirectories))
                    {
                        string relativePath = filePath.Substring(index + keyword.Length + 1);

                        // Check if the file follows the "EXPEDITION_X" format
                        if (relativePath.Contains("EXPEDITION_"))
                        {
                            string[] parts = relativePath.Split('_');
                            if (parts.Length > 1)
                            {
                                string expeditionNumber = parts[1];
                                expeditions.Add(expeditionNumber); // Add the expedition number to the set
                            }
                        }
                    }
                }
            }

            // Add all unique expedition numbers to the ComboBox
            foreach (var expedition in expeditions.OrderBy(e => e))
            {
                expeditionComboBox.Items.Add(expedition);
            }

            // Select the "All Expeditions" option by default
            expeditionComboBox.SelectedIndex = 0;
        }


        private void RefreshLists(string expeditionFilter)
        {
            // Clear both lists
            currentSaveFiles.Items.Clear();
            backedUpFiles.Items.Clear();

            // Refresh current save files (Backup directory)
            if (Directory.Exists(saveFilePath))
            {
                string keyword = "Backup";
                int index = saveFilePath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    foreach (string filePath in Directory.GetFiles(saveFilePath, "*", SearchOption.AllDirectories))
                    {
                        string relativePath = filePath.Substring(index + keyword.Length + 1);

                        // Apply the filter if it is "All Expeditions" or the selected expedition
                        if (expeditionFilter == "All Expeditions" || relativePath.Contains($"EXPEDITION_{expeditionFilter}_"))
                        {
                            currentSaveFiles.Items.Add(relativePath);
                        }
                    }
                }
            }

            // Refresh backed-up files (CustomSaveFiles directory)
            string customSaveFilesPath = @"C:\Users\balli\AppData\Local\Sandfall\Saved\SaveGames\76561197984330337\CustomSaveFiles";
            string keywordBackedUp = "CustomSaveFiles";
            int indexBackedUp = customSaveFilesPath.IndexOf(keywordBackedUp, StringComparison.OrdinalIgnoreCase);
            if (indexBackedUp >= 0)
            {
                foreach (string filePath in Directory.GetFiles(customSaveFilesPath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = filePath.Substring(indexBackedUp + keywordBackedUp.Length + 1);

                    // Apply the filter if it is "All Expeditions" or the selected expedition
                    if (expeditionFilter == "All Expeditions" || relativePath.Contains($"EXPEDITION_{expeditionFilter}_"))
                    {
                        backedUpFiles.Items.Add(relativePath);
                    }
                }
            }
        }


        private void expeditionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get the selected expedition filter from the ComboBox
            string expeditionFilter = expeditionComboBox.SelectedItem as string;

            // Call RefreshLists() to automatically refresh both lists with the selected filter applied
            RefreshLists(expeditionFilter);
        }

    }
}
