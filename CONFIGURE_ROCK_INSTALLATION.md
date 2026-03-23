# Configure Project to Use Rock Installation DLLs

## Step-by-Step Instructions

### Step 1: Find Your Rock Installation Path

Your Rock installation is typically located at one of these locations:

**Common paths:**
- `C:\inetpub\wwwroot\RockWeb\`
- `C:\Rock\RockWeb\`
- `D:\Rock\RockWeb\`
- `C:\Program Files\RockRMS\RockWeb\`

**How to find it:**
1. Open **File Explorer**
2. Look for a folder named `RockWeb`
3. Inside should be a `Bin` folder with DLLs like `Rock.dll`, `Rock.Common.dll`, etc.

**Note the full path** to the `Bin` folder (e.g., `C:\inetpub\wwwroot\RockWeb\Bin`)

---

### Step 2: Add Reference Paths in Visual Studio

1. **In Visual Studio**, right-click on `com.bemaservices.RoomManagement` project
2. Select **Properties**
3. Go to the **Reference Paths** tab (under **Build** on the left)
4. Click the **...** button or type the path
5. **Add the path** to your Rock `Bin` folder:
   - Example: `C:\inetpub\wwwroot\RockWeb\Bin`
6. Click **Add Folder**
7. Click **OK**

This tells Visual Studio where to look for the Rock DLLs.

---

### Step 3: Verify/Fix Individual References

Some references might still show errors. Let's check them:

1. **In Solution Explorer**, expand `com.bemaservices.RoomManagement`
2. Expand **References**
3. Look for references with yellow warning icons

**For each missing reference:**

#### System References (Should be automatic)
- `System`, `System.Core`, `System.Data`, etc. - These should resolve automatically if .NET Framework 4.7.2 is installed
- If not, right-click project → **Properties** → **Application** → Ensure Target framework is `.NET Framework 4.7.2`

#### Rock References (Need to point to Bin folder)
- `Rock.dll`
- `Rock.Common.dll`
- `Rock.Enums.dll`
- `Rock.Lava.dll`
- `Rock.Rest.dll`
- `Rock.Lava.Shared.dll`
- `DotLiquid.dll`

**To fix a Rock reference:**
1. Right-click the reference → **Properties**
2. Check the **Path** - it should point to your Rock `Bin` folder
3. If it's wrong, delete the reference and re-add it:
   - Right-click **References** → **Add Reference**
   - Click **Browse**
   - Navigate to your Rock `Bin` folder
   - Select the DLL
   - Click **OK**

#### NuGet Package References
These should restore automatically, but if not:

1. **Right-click solution** → **Restore NuGet Packages**
2. Or use Package Manager Console:
   - **Tools** → **NuGet Package Manager** → **Package Manager Console**
   - Run: `Update-Package -reinstall`

#### Special Cases

**itextsharp.dll:**
- Should be in: `com.bemaservices.RoomManagement\libs\itextsharp.dll`
- If missing, check if it exists in your Rock `Bin` folder and copy it

**com.centralaz.RoomManagement:**
- This is another project in your repo
- Add it to the solution:
  - Right-click solution → **Add** → **Existing Project**
  - Navigate to: `com.centralaz.RoomManagement\com.centralaz.RoomManagement.csproj`

---

### Step 4: Verify All References

After adding the reference path:

1. **Build the project** (right-click → Build)
2. Check the **Error List** window
3. Any remaining errors will show which references are still missing

---

### Step 5: Common Issues and Fixes

#### "Could not find file" errors
- **Solution**: Ensure the DLL actually exists in your Rock `Bin` folder
- Check the file exists: `RockWeb\Bin\Rock.dll` (or whatever DLL is missing)

#### "The type or namespace name could not be found"
- **Solution**: The reference path might be wrong
- Re-check Step 2 - ensure the path is correct
- Try rebuilding: **Build** → **Rebuild Solution**

#### NuGet packages still missing
- **Solution**: 
  - Close Visual Studio
  - Delete the `packages` folder in your project directory
  - Reopen Visual Studio
  - Restore NuGet packages again

#### System references missing
- **Solution**: Install .NET Framework 4.7.2 Developer Pack
  - Download from: https://dotnet.microsoft.com/download/dotnet-framework/net472
  - Install the Developer Pack (not just the runtime)

---

## Quick Checklist

- [ ] Found Rock installation `Bin` folder path
- [ ] Added reference path in project properties
- [ ] Restored NuGet packages
- [ ] Added `com.centralaz.RoomManagement` project to solution
- [ ] Verified `.NET Framework 4.7.2` is target framework
- [ ] Built project successfully

---

## After Configuration

Once all references are resolved:

1. **Set Configuration to Release** (toolbar dropdown)
2. **Build the project**
3. Find your plugin at: `builds\2.7.0\CalendarManagement-v2.7.0.1.plugin`

---

## Need Help?

If you're still seeing errors:
1. Note which specific DLLs are missing
2. Check if they exist in your Rock `Bin` folder
3. Share the error messages and I can help troubleshoot


















