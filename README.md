![Demo Picture of Application](https://github.com/thomasbuechi/COE33_SaveManager/blob/main/demoPicture.png)

# TLDR
**Steam Only so far**
1. Download Application
2. Unzip Application
3. Start Application
4. Press Open SaveGames button
5. BACKUP UR OWN FILES (765... Folder, which is your SteamID)
6. Make sure only one Folder with 765... exists.
7. ???
8. Profit
   
## Installation
**Steam Only so far**
1.  **Download:** Download the latest release ZIP file from the [Releases](link/to/your/releases) page.
2.  **Unzip:** Extract the contents of the downloaded ZIP file to a custom folder of your choice on your computer.
3.  **Backup Your Save File (Important!):** **It's highly recommended that you create a backup of your current save file before proceeding.** While I've done my best to ensure everything works smoothly, data loss can unfortunately happen, and I won't be able to recover it for you.

    Your save files for Clair Obscure: Expedition 33 are located within a folder named with your SteamID, which starts with `765...`, in the following directory:

    ```
    C:\Users\Your PC's name\AppData\Local\Sandfall\Saved\SaveGames\765...
    ```

    This `765...` folder contains your save slots (e.g., `Expedition0`, `Expedition1`, up to `Expedition9`). If you have multiple save states, **backing up this entire `765...` folder will copy all of them.**

    **For a smoother experience, especially if you intend to load a specific backed-up save state later, it's recommended to start with only one desired save state present in your `765...` folder before using this application.**

    Alternatively, you can use the **"Open SaveGames"** button within the application itself to be directly navigated to your SaveGames folder.

---

**Important Requirement:** Please ensure that there is **only one** folder within the `SaveGames` directory that starts with the prefix `765...`. This folder contains your game's save files. The application will typically display a message if it detects zero or more than one such folder.

## Saving Your Game State:

To save your current game state using this application:

1.  Hit the **"Save"** button within the application.
2.  Provide a name for your save.

The created save file (which is a copy of your `765...` SteamID folder) will be stored in the following location:
```
C:\Users\Your PC's name\AppData\Local\Sandfall\Saved\SaveGames\CustomSaveFiles
```

## **Loading a Backed-Up Save File:**

If you choose to load a save file that you've previously backed up using this application, here's what happens:

1.  **Deletion of Current Save:** The application will move your current `765...` folder from the `SaveGames` directory to the Recycle Bin.
2.  **Restoration of Backup:** Your chosen backed-up `765...` folder will then be pasted into the `SaveGames` directory.

**To load this data in the game:**

1.  Back out to the **Main Menu**.
2.  Select the **"Continue"** option. 
3.  Choose the save you want to load and then select **"Continue"**. You'll be able to pick up your game from where you last saved in that backed-up state.

**A Small Visual Note:** The "Continue" save state in the game might not immediately update to reflect the newly loaded save state. This is a visual quirk and usually resolves after a game restart or by loading another save. Rest assured, the correct save data will have been loaded.


# Additional Application Functions:

* **Delete Button:** When you select a saved game file in the application's list and click the "Delete" button, that specific saved game folder will be moved to your computer's Recycle Bin.
* **Refresh Button:** If you make any manual changes to the files or folders within the application's working directories (like adding or removing backups directly), clicking the "Refresh" button will update the application's view to reflect these changes.
