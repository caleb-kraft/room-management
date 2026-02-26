# Visual Studio Build Guide - Calendar Management Plugin

## Step-by-Step Instructions

### Prerequisites
- Visual Studio 2019 or later installed on Windows
- Full Rock solution (RockRMS) available
- This plugin project should be part of or reference the Rock solution

---

## Step 1: Open the Rock Solution in Visual Studio

1. **Launch Visual Studio**
   - Open Visual Studio from the Start menu or desktop shortcut

2. **Open the Solution**
   - Click **File** → **Open** → **Project/Solution** (or press `Ctrl+Shift+O`)
   - Navigate to your Rock solution directory
   - Select the `.sln` file (e.g., `Rock.sln` or `BEMA Software Services.sln`)
   - Click **Open**

   **Alternative**: If you have the solution file in your recent projects, you can click on it from the Start Page.

---

## Step 2: Locate the Calendar Management Project

1. **Find the Project in Solution Explorer**
   - Look in the **Solution Explorer** panel (usually on the right side)
   - Expand the solution tree if needed
   - Find the project: `com.bemaservices.RoomManagement`
   - If you don't see it, you may need to:
     - Right-click the solution → **Add** → **Existing Project**
     - Navigate to: `com.bemaservices.RoomManagement\com.bemaservices.RoomManagement.csproj`
     - Click **Open**

---

## Step 3: Set Build Configuration to Release

1. **Change Configuration**
   - Look at the toolbar near the top of Visual Studio
   - Find the dropdown that says **Debug** (next to the green play button)
   - Click the dropdown and select **Release**
   
   **Important**: The plugin package is only created when building in **Release** mode. Debug builds won't create the `.plugin` file.

   **Alternative Method**:
   - Right-click the solution in Solution Explorer
   - Select **Configuration Manager**
   - Under **Active solution configuration**, change from **Debug** to **Release**
   - Click **Close**

---

## Step 4: Verify Project Dependencies

1. **Check Project References**
   - Right-click `com.bemaservices.RoomManagement` in Solution Explorer
   - Select **Properties** (or press `Alt+Enter`)
   - Go to the **References** section
   - Verify that all Rock project references are present:
     - Rock
     - Rock.Common
     - Rock.Enums
     - Rock.Lava
     - Rock.Rest
     - etc.

2. **Restore NuGet Packages** (if needed)
   - Right-click the solution in Solution Explorer
   - Select **Restore NuGet Packages**
   - Wait for the restore to complete

---

## Step 5: Build the Project

### Option A: Build Single Project
1. **Right-click** on `com.bemaservices.RoomManagement` in Solution Explorer
2. Select **Build** (or press `Ctrl+Shift+B`)
3. Watch the **Output** window at the bottom for build progress

### Option B: Build Entire Solution
1. Click **Build** in the menu bar
2. Select **Build Solution** (or press `Ctrl+Shift+B`)
3. Watch the **Output** window for build progress

### What to Look For:
- **Build succeeded** message in the Output window
- No errors (warnings are usually okay)
- The build process will show messages like:
  ```
  Version: 2.7.0.1
  Copying files...
  Creating plugin package...
  ```

---

## Step 6: Verify the Build Output

1. **Check Build Output Location**
   - Navigate to: `com.bemaservices.RoomManagement\builds\2.7.0\`
   - You should see a file named: `CalendarManagement-v2.7.0.1.plugin`
   - (The revision number will increment each time you build)

2. **Verify File Size**
   - The `.plugin` file should be several MB in size (not empty)
   - Right-click → **Properties** to check file size

---

## Step 7: Locate Your Plugin File

The plugin file will be located at:
```
com.bemaservices.RoomManagement\builds\2.7.0\CalendarManagement-v2.7.0.1.plugin
```

**Note**: The revision number (the last number) will increment automatically each time you build in Release mode:
- First build: `CalendarManagement-v2.7.0.1.plugin`
- Second build: `CalendarManagement-v2.7.0.2.plugin`
- And so on...

---

## Troubleshooting

### Build Fails with "MSBuild Community Tasks" Error
- **Solution**: Ensure the Rock solution includes the MSBuild Community Tasks package
- Check: `packages\MSBuildTasks.1.5.0.235\tools\MSBuild.Community.Tasks.Targets` exists

### Build Fails with Missing References
- **Solution**: Ensure all Rock projects are included in the solution
- Right-click solution → **Restore NuGet Packages**
- Rebuild the solution

### No `.plugin` File Created
- **Check**: Are you building in **Release** mode? (Not Debug)
- **Check**: Look in the `builds\2.7.0\` folder (version folder matches your version)
- **Check**: Check the Output window for errors during the `BuildPackageZip` target

### Version Not Incrementing
- **Check**: Ensure `builds\.version` file exists and is readable
- **Check**: The file should contain: `2.7.0.0` (or your current version)
- **Note**: Only the revision (last number) increments automatically

### "Cannot find version file" Error
- **Solution**: Ensure `builds\.version` file exists in the project directory
- The file should contain: `2.7.0.0` (format: `MAJOR.MINOR.BUILD.REVISION`)

---

## Quick Reference

| Step | Action | Location |
|------|--------|----------|
| 1 | Open Solution | File → Open → Project/Solution |
| 2 | Find Project | Solution Explorer → `com.bemaservices.RoomManagement` |
| 3 | Set to Release | Toolbar dropdown: Debug → Release |
| 4 | Build | Right-click project → Build |
| 5 | Find Plugin | `builds\2.7.0\CalendarManagement-v2.7.0.X.plugin` |

---

## Next Steps After Building

Once you have the `.plugin` file:

1. **Install in Rock**:
   - Log into your Rock instance
   - Go to **Admin Tools** → **Plugins** → **Plugin Manager**
   - Click **Upload Plugin**
   - Select your `.plugin` file
   - Follow the installation wizard

2. **Verify Installation**:
   - Check that the plugin appears in the plugin list
   - Verify the version shows as `2.7.0.X`
   - Test the functionality

---

## Build Output Details

When building successfully, you'll see output like:
```
Build started...
UpdateAssemblyVersion:
  Reading version from builds\.version
  Incrementing build number...
  Updating AssemblyInfo.cs...
CopyFiles:
  Copying DLLs...
  Copying plugin files...
  Copying webhooks...
BuildPackageZip:
  Version: 2.7.0.1
  Creating zip: CalendarManagement-v2.7.0.1.plugin
Build succeeded.
```

---

## Tips

- **Always build in Release mode** for production plugins
- **Keep the version file updated** manually for major/minor version changes
- **Test the plugin** in a development environment before deploying to production
- **Keep backups** of previous plugin versions
- The revision number increments automatically, so you don't need to manually update it


















