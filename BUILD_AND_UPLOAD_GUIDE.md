# Room Management Plugin - Build and Upload Guide

## Overview
This is a RockRMS plugin built with .NET Framework 4.7.2. The build process automatically creates a `.plugin` package file that can be uploaded to the Rock Shop.

## Prerequisites

### Windows (Recommended)
- Visual Studio 2019 or later
- .NET Framework 4.7.2 SDK
- MSBuild (included with Visual Studio)

### macOS/Linux (Alternative)
- Mono Framework
- MSBuild (via Mono)
- Note: You may need to adjust paths for cross-platform compatibility

## Build Process

### Step 1: Update Version Number
Edit the version file:
```
com.bemaservices.RoomManagement/builds/.version
```

Current format: `MAJOR.MINOR.BUILD.REVISION` (e.g., `2.6.4.16`)

**Important**: Update the Major.Minor.Build numbers manually. The Revision number is auto-incremented during build.

### Step 2: Build the Plugin

#### Option A: Using Visual Studio (Windows)
1. Open the solution file (if available) or the `.csproj` file in Visual Studio
2. Set the **Configuration** to **Release** (not Debug)
3. Right-click the project → **Build** or press `Ctrl+Shift+B`
4. The build process will:
   - Increment the revision number
   - Copy DLLs and plugin files to `builds/tmp/`
   - Create a `.plugin` file in `builds/X.Y.Z/` folder (e.g., `builds/2.6.4/`)

#### Option B: Using MSBuild Command Line (Windows)
```bash
cd "com.bemaservices.RoomManagement"
msbuild com.bemaservices.RoomManagement.csproj /p:Configuration=Release /p:Platform="Any CPU"
```

#### Option C: Using MSBuild on macOS/Linux (Mono)
```bash
cd "com.bemaservices.RoomManagement"
msbuild com.bemaservices.RoomManagement.csproj /p:Configuration=Release /p:Platform="Any CPU"
```

**Note**: You may need to adjust project references if building outside the full Rock solution.

### Step 3: Locate the Built Plugin
After a successful Release build, find your plugin file at:
```
builds/X.Y.Z/ResourceReservation-vX.Y.Z.REVISION.plugin
```

Example: `builds/2.6.4/ResourceReservation-v2.6.4.17.plugin`

### Step 4: Update Release Documentation
Before uploading, update:
- **Description.html** - Add release notes for the new version
- **PostInstallInstructions.html** - Update if needed
- Package metadata (screenshots, icons if changed)

To generate release notes from git commits:
```bash
git log --pretty=oneline <previous-commit>..<current-commit> | grep " + " | cut -c 42- | grep '\[RM\]'
```

## Upload to Rock Shop

### Step 5: Upload the Plugin
1. Log into the Rock Shop (https://www.rockrms.com/Shop)
2. Navigate to your plugin listing
3. Upload the `.plugin` file from `builds/X.Y.Z/`
4. Fill in:
   - Version number
   - Release notes (from Description.html)
   - Documentation URL
   - Post-install instructions

## Build Process Details

The build process (defined in `com.bemaservices.RoomManagement.csproj`) automatically:

1. **BeforeBuild** (`UpdateAssemblyVersion` target):
   - Reads version from `builds/.version`
   - Increments the Build number
   - Updates `Properties/AssemblyInfo.cs` with new version

2. **AfterBuild** (`BuildPackageZip` target):
   - Copies files to `builds/tmp/`:
     - DLLs: `com.bemaservices.RoomManagement.dll`, `com.centralaz.RoomManagement.dll`, `itextsharp.dll`
     - XML documentation: `com.bemaservices.RoomManagement.xml`
     - Plugin files: All files from `Plugins/com_bemaservices/RoomManagement/`
     - Webhook: `Webhooks/GetReservationCalendarFeed.ashx`
   - Creates a zip file named `ResourceReservation-vX.Y.Z.REVISION.plugin`
   - Places it in `builds/X.Y.Z/` folder

## Troubleshooting

### Build Fails - Missing References
The project references other Rock projects. You may need:
- Full Rock solution structure
- Or adjust project references to point to compiled DLLs

### Build Fails - MSBuild Tasks Not Found
Ensure MSBuild Community Tasks is installed:
```
packages/MSBuildTasks.1.5.0.235/tools
```

### Version Not Incrementing
- Ensure you're building in **Release** configuration
- Check that `builds/.version` file exists and is readable
- Verify `Build.tasks` file is present

### Plugin File Not Created
- Check build output for errors
- Ensure `builds/tmp/` directory is writable
- Verify all source files exist (Plugins, Webhooks folders)

## Quick Reference

**Current Version**: Check `builds/.version` (currently: `2.6.4.16`)

**Build Output Location**: `builds/X.Y.Z/ResourceReservation-vX.Y.Z.REVISION.plugin`

**Release Notes Template**: See `builds/Description.html`

**Install Instructions**: See `builds/PostInstallInstructions.html`


















