# Fix Missing References - Step by Step

## The Problem

You're seeing errors because:
1. NuGet packages haven't been restored
2. The project references other Rock projects that aren't in the solution
3. Some DLLs are expected to be in the Rock solution's `RockWeb\Bin` folder

## Solution Steps

### Step 1: Create/Save a Solution File

1. **In Visual Studio**:
   - Go to **File** → **Save All** (or `Ctrl+Shift+S`)
   - If prompted to save the solution, click **Yes**
   - Save it as: `RoomManagement.sln` in the `room-management` folder

   **OR** if you want to create a new solution:
   - **File** → **New** → **Project**
   - Select **Blank Solution**
   - Name: `RoomManagement`
   - Location: `C:\Users\lgazureadmin\source\repos\caleb-kraft\room-management\`
   - Click **OK**
   - Right-click solution → **Add** → **Existing Project**
   - Add: `com.bemaservices.RoomManagement\com.bemaservices.RoomManagement.csproj`
   - Add: `com.centralaz.RoomManagement\com.centralaz.RoomManagement.csproj` (if it exists)

### Step 2: Restore NuGet Packages

1. **Right-click the solution** in Solution Explorer
2. Select **Restore NuGet Packages**
3. Wait for it to complete

   **OR** use Package Manager Console:
   - **Tools** → **NuGet Package Manager** → **Package Manager Console**
   - Run: `Update-Package -reinstall`

### Step 3: Fix Missing Rock Project References

The project expects to be part of a Rock solution. You have two options:

#### Option A: Add Rock Projects to Solution (If You Have Them)

If you have the Rock solution elsewhere:

1. **Find your Rock solution** (e.g., `Rock.sln` or `BemaRockV14.sln`)
2. **Open that solution** instead
3. **Add this plugin project** to it:
   - Right-click solution → **Add** → **Existing Project**
   - Navigate to: `com.bemaservices.RoomManagement.csproj`

#### Option B: Reference Rock DLLs Directly (If You Have Rock Installed)

If you have Rock installed/running, you can reference the compiled DLLs:

1. **Find your Rock installation** (usually `C:\inetpub\wwwroot\RockWeb\` or similar)
2. **In Visual Studio**, for each missing Rock reference:
   - Right-click **References** → **Add Reference**
   - Click **Browse**
   - Navigate to: `RockWeb\Bin\` folder
   - Select the DLL (e.g., `Rock.dll`, `Rock.Common.dll`, etc.)
   - Click **OK**

   **Common Rock DLLs to add**:
   - `Rock.dll`
   - `Rock.Common.dll`
   - `Rock.Enums.dll`
   - `Rock.Lava.dll`
   - `Rock.Rest.dll`
   - `Rock.Lava.Shared.dll`
   - `DotLiquid.dll`
   - `Ical.Net.dll`
   - `Ical.Net.Collections.dll`
   - `Newtonsoft.Json.dll`
   - `EntityFramework.dll`
   - `EntityFramework.SqlServer.dll`
   - `PuppeteerSharp.dll`
   - `Quartz.dll`
   - `TimeZoneConverter.dll`
   - `System.Net.Http.Formatting.dll`
   - `System.Web.Http.dll`

### Step 4: Fix System References

Some System references might need the .NET Framework targeting pack:

1. **Right-click the project** → **Properties**
2. Go to **Application** tab
3. Ensure **Target framework** is: `.NET Framework 4.7.2`
4. If it's different, change it and save

### Step 5: Fix itextsharp Reference

The `itextsharp.dll` should be in the project's `libs` folder:

1. **Check if it exists**: `com.bemaservices.RoomManagement\libs\itextsharp.dll`
2. If missing, you may need to download it or copy it from another Rock installation
3. The project reference should point to: `libs\itextsharp.dll`

### Step 6: Fix com.centralaz.RoomManagement Reference

This is another project in your repository:

1. **Add it to the solution**:
   - Right-click solution → **Add** → **Existing Project**
   - Navigate to: `com.centralaz.RoomManagement\com.centralaz.RoomManagement.csproj`
   - Add it

2. **Verify the reference**:
   - Right-click `com.bemaservices.RoomManagement` → **Properties** → **References**
   - Ensure `com.centralaz.RoomManagement` is listed

## Quick Fix: If You Have Rock Running

The easiest solution if you have Rock installed:

1. **Find your RockWeb folder** (where Rock is installed)
2. **Copy the path** to `RockWeb\Bin\`
3. **In Visual Studio**:
   - Right-click project → **Properties**
   - Go to **Reference Paths** (under Build)
   - Add the path: `C:\inetpub\wwwroot\RockWeb\Bin\` (or wherever your Rock is)
   - Click **OK**

This will help Visual Studio find the DLLs automatically.

## Alternative: Build Without Full Solution

If you just need to build the plugin and have the Rock DLLs available:

1. **Copy all required DLLs** to a folder
2. **Update project references** to point to those DLLs
3. **Build in Release mode**

However, this is more complex and error-prone.

## Recommended Approach

**Best option**: Open the full Rock solution (if you have it) and add this plugin project to it. This ensures all references are correct and the build environment matches production.

If you don't have the Rock solution, you'll need access to the compiled Rock DLLs from a Rock installation.


















