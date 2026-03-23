# Finding the Rock Solution File (.sln)

## Where to Look for the Solution File

The `.sln` (solution) file is typically located in the **main Rock solution directory**. Here's where to find it:

### Common Locations:

1. **In your Rock installation directory**:
   - Look for a folder named:
     - `Rock` or `RockRMS`
     - `BemaRockV14` (based on your project references)
     - `RockWeb` (the web application folder)
   - The `.sln` file will be in the **root** of that directory

2. **Typical paths**:
   ```
   C:\Rock\
   C:\RockRMS\
   C:\Projects\Rock\
   C:\Projects\BemaRockV14\
   D:\Rock\
   ```

3. **Look for files named**:
   - `Rock.sln`
   - `RockRMS.sln`
   - `BEMA Software Services.sln`
   - `BemaRockV14.sln`

### How to Find It:

#### Method 1: File Explorer Search
1. Open **File Explorer** (Windows Explorer)
2. Navigate to where you typically store projects (e.g., `C:\Projects\` or `D:\Rock\`)
3. In the search box, type: `*.sln`
4. Look for solution files related to Rock

#### Method 2: Check Recent Files in Visual Studio
1. Open **Visual Studio**
2. Look at the **Start Page** or **File** → **Recent Projects**
3. If you've opened the Rock solution before, it will be listed there

#### Method 3: Check the Project References
Your plugin project references show it expects to be in a solution structure like:
```
Solution Root/
├── Rock.sln (or similar)
├── Rock/
├── Rock.Common/
├── Rock.Enums/
├── Rock.Lava/
├── Rock.Rest/
├── BemaRockV14/
│   └── Rock.Lava/
└── room-management/
    └── com.bemaservices.RoomManagement/
```

The solution file should be at the **same level** as these Rock project folders.

### If You Can't Find the Solution File:

#### Option 1: Open the Project File Directly
You can open just the plugin project file:
1. In Visual Studio: **File** → **Open** → **Project/Solution**
2. Navigate to: `room-management\com.bemaservices.RoomManagement\`
3. Select: `com.bemaservices.RoomManagement.csproj`
4. Click **Open**

**Note**: This will open the project, but you may need to fix project references manually.

#### Option 2: Create a New Solution
1. In Visual Studio: **File** → **New** → **Project**
2. Select **Blank Solution**
3. Name it (e.g., "Rock Plugins")
4. Right-click the solution → **Add** → **Existing Project**
5. Navigate to: `com.bemaservices.RoomManagement.csproj`
6. You'll need to add references to Rock projects manually

#### Option 3: Ask Your Team
- Check with your team where the Rock solution is located
- It might be on a shared drive or server
- Check your organization's documentation or wiki

### What the Solution File Looks Like:

A `.sln` file is a text file that Visual Studio uses. It typically starts with:
```
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Rock", "Rock\Rock.csproj", ...
```

### Quick Check:

Based on your project structure, the solution is likely:
- **Two levels up** from `room-management` folder
- In a directory that contains both `Rock` projects and your `room-management` folder
- Named something like `Rock.sln` or `BemaRockV14.sln`

### Example Structure:
```
C:\Projects\
├── Rock.sln                    ← THE SOLUTION FILE
├── Rock\
├── Rock.Common\
├── Rock.Enums\
├── BemaRockV14\
│   └── Rock.Lava\
└── room-management\             ← Your plugin is here
    └── com.bemaservices.RoomManagement\
```

---

## Once You Find It:

1. **Double-click** the `.sln` file to open it in Visual Studio
2. Or use **File** → **Open** → **Project/Solution** in Visual Studio
3. Navigate to and select the `.sln` file

Then follow the [Visual Studio Build Guide](./VISUAL_STUDIO_BUILD_GUIDE.md) to build your plugin.


















