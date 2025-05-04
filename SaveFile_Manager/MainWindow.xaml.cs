using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SaveFile_Manager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string saveFilePath = @"";
        private string settingsFile = "config.txt"; // You can change this to a more specific path
        

        private const string CustomSaveFilesFolderName = "CustomSaveFiles";
        private const string MainSaveFolderName = "76561197984330337"; // or whatever the folder name is under SaveGames


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
            string currentFolder = dataPath.Text.Trim(); // This is the original game save folder (e.g., ...\76561197984330337)
            if (string.IsNullOrEmpty(currentFolder) || !Directory.Exists(currentFolder))
            {
                MessageBox.Show("Invalid base folder.");
                return;
            }

            string selectedFile = backedUpFiles.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("No file selected.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Do you want to load this file?",
                "Load Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Use saveFilePath to determine the CustomSaveFiles path (one level up from original folder)
                    string customSaveFilesPath = Path.Combine(Path.GetDirectoryName(saveFilePath), "CustomSaveFiles");

                    string sourceFile = Path.Combine(customSaveFilesPath, selectedFile);
                    if (!File.Exists(sourceFile))
                    {
                        MessageBox.Show("Selected file does not exist in CustomSaveFiles.");
                        return;
                    }

                    // Destination is the original save directory (currentFolder)
                    string destFile = Path.Combine(currentFolder, RemovePrefix(selectedFile)); // Strip prefix

                    File.Copy(sourceFile, destFile, true);

                    RefreshLists(expeditionComboBox.SelectedItem as string);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading file: " + ex.Message);
                }
            }
            else
            {
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
                // Use saveFilePath to determine the target folder, and change the ending to "CustomSaveFiles"
                string targetFolder = Path.Combine(Path.GetDirectoryName(saveFilePath), "CustomSaveFiles"); // Use the user-defined path
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
                string expeditionFilter = expeditionComboBox.SelectedItem as string; // Get current combo box selection
                RefreshLists(expeditionFilter); // Pass the filter to make sure lists are updated with the right files
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
        }

        string GetSteamIdFolder(string saveGamesPath)
        {
            var steamIdDir = Directory.GetDirectories(saveGamesPath)
                                      .FirstOrDefault(d => Path.GetFileName(d).StartsWith("765"));
            return steamIdDir;
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


            private void RefreshLists()
            {
                string expeditionFilter = expeditionComboBox.SelectedItem as string ?? "All Expeditions";
                RefreshLists(expeditionFilter);
            }
 

        private void PopulateExpeditionComboBox()
        {
            // Clear any existing items in the ComboBox
            expeditionComboBox.Items.Clear();

            // Add a default "All Expeditions" option
            expeditionComboBox.Items.Add("All Expeditions");

            // Ensure the user-defined path is valid
            if (Directory.Exists(saveFilePath))
            {
                // A HashSet is used to avoid duplicates in the ComboBox
                HashSet<string> expeditions = new HashSet<string>();

                // Search for all files matching the pattern "EXPEDITION_*.sav" in the user-defined path
                foreach (string filePath in Directory.GetFiles(saveFilePath, "EXPEDITION_*.sav", SearchOption.TopDirectoryOnly))
                {
                    // Get just the file name, e.g., "EXPEDITION_0.sav"
                    string fileName = Path.GetFileName(filePath);

                    // Extract the expedition number from the file name (e.g., "0" from "EXPEDITION_0.sav")
                    string expeditionNumber = fileName.Split('_')[1].Split('.')[0]; // Get the number after "EXPEDITION_"
                    expeditions.Add(expeditionNumber); // Add the expedition number to the set
                }

                // Add all unique expedition numbers to the ComboBox
                foreach (var expedition in expeditions.OrderBy(e => e))
                {
                    expeditionComboBox.Items.Add(expedition);
                }

                // Set the default option to "All Expeditions"
                expeditionComboBox.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("The specified directory does not exist.");
            }
        }





        private void RefreshLists(string expeditionFilter)
        {
            currentSaveFiles.Items.Clear();
            backedUpFiles.Items.Clear();

            // === Refresh current save files ===
            if (Directory.Exists(saveFilePath))
            {
                foreach (string filePath in Directory.GetFiles(saveFilePath, "EXPEDITION_*.sav"))
                {
                    string fileName = Path.GetFileName(filePath);

                    if (expeditionFilter == "All Expeditions" || fileName.Contains($"EXPEDITION_{expeditionFilter}.sav"))
                    {
                        currentSaveFiles.Items.Add(fileName);
                    }
                }
            }

            // === Refresh backed-up files ===
            string customSaveFilesPath = Path.Combine(Path.GetDirectoryName(saveFilePath), "CustomSaveFiles");

            if (Directory.Exists(customSaveFilesPath))
            {
                foreach (string filePath in Directory.GetFiles(customSaveFilesPath, "*_EXPEDITION_?.sav"))
                {
                    string fileName = Path.GetFileName(filePath);

                    if (expeditionFilter == "All Expeditions" ||
                        Regex.IsMatch(fileName, @$"_EXPEDITION_{expeditionFilter}\.sav$", RegexOptions.IgnoreCase))
                    {
                        backedUpFiles.Items.Add(fileName);
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
