# Rock Plugin Build Checklist

## Pre-Build Verification Steps

### 1. Compilation Error Patterns to Check

#### Type Inference Issues (CS0411)
**Pattern:** `SetLinqDataSource` with collections that aren't `IQueryable<T>`
```csharp
// ❌ WRONG - List<T> can't infer type
var items = someService.GetItems().ToList();
grid.SetLinqDataSource(items);

// ✅ CORRECT - Convert to IQueryable
var items = someService.GetItems().ToList();
grid.SetLinqDataSource(items.AsQueryable());

// ✅ CORRECT - Or use IQueryable directly
var items = someService.GetItems();
grid.SetLinqDataSource(items);
```

**Search Pattern:**
```powershell
# Find potential issues
Select-String -Path "*.ascx.cs" -Pattern "SetLinqDataSource\([^)]*\)" | Where-Object { $_.Line -notmatch "AsQueryable" }
```

#### Variable Scope Conflicts (CS0136)
**Pattern:** Same variable name declared in nested scopes
```csharp
// ❌ WRONG
var reservationType = ...; // outer scope
if (condition) {
    var reservationType = ...; // nested scope - ERROR!
}

// ✅ CORRECT
var reservationType = ...; // declare once at method level
if (condition) {
    // use reservationType
}
```

**Search Pattern:**
```powershell
# Find duplicate variable declarations
Select-String -Path "*.ascx.cs" -Pattern "var (\w+)\s*=" -AllMatches | 
    Group-Object { $_.Matches.Groups[1].Value } | 
    Where-Object { $_.Count -gt 1 }
```

#### Method Signature Mismatches (CS1501, CS1503)
**Pattern:** Wrong number/type of arguments
```csharp
// ❌ WRONG - SaveAttributeValues doesn't have 3-arg overload for entities
Rock.Attribute.Helper.SaveAttributeValues(sourceEntity, targetEntity, rockContext);

// ✅ CORRECT
entity.LoadAttributes(rockContext);
Rock.Attribute.Helper.GetEditValues(phAttributes, entity);
entity.SaveAttributeValues(rockContext);
```

**Search Pattern:**
```powershell
# Find SaveAttributeValues calls
Select-String -Path "*.ascx.cs" -Pattern "SaveAttributeValues\([^)]*,[^)]*,[^)]*\)"
```

#### Variables Used Before Declaration (CS0841)
**Pattern:** Variable used before it's declared
```csharp
// ❌ WRONG
var result = SomeMethod(reservationType); // used here
var reservationType = GetReservationType(); // declared here

// ✅ CORRECT
var reservationType = GetReservationType(); // declare first
var result = SomeMethod(reservationType); // use after
```

### 2. Common Rock Plugin Patterns

#### Attribute Handling
```csharp
// Standard pattern:
entity.LoadAttributes(rockContext);
Rock.Attribute.Helper.GetEditValues(phAttributes, entity);
entity.SaveAttributeValues(rockContext);
```

#### RockContext Usage
```csharp
// Always use 'using' for proper disposal
using (var rockContext = new RockContext()) {
    // work with context
}
```

#### Grid Data Binding
```csharp
// Always use AsQueryable() for SetLinqDataSource
grid.SetLinqDataSource(items.AsQueryable());
grid.DataBind();
```

### 3. Automated Pre-Build Checks

Create a PowerShell script `pre-build-check.ps1`:

```powershell
# Pre-Build Check Script
$errors = @()

# Check 1: SetLinqDataSource without AsQueryable
$setLinqIssues = Select-String -Path "Plugins\**\*.ascx.cs" -Pattern "SetLinqDataSource\([^)]*\)" | 
    Where-Object { $_.Line -notmatch "AsQueryable" -and $_.Line -notmatch "Queryable\(\)" }
if ($setLinqIssues) {
    $errors += "Found SetLinqDataSource calls without AsQueryable():"
    $setLinqIssues | ForEach-Object { $errors += "  $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" }
}

# Check 2: SaveAttributeValues with 3 arguments
$saveAttrIssues = Select-String -Path "Plugins\**\*.ascx.cs" -Pattern "SaveAttributeValues\([^)]*,[^)]*,[^)]*\)"
if ($saveAttrIssues) {
    $errors += "Found SaveAttributeValues calls with 3 arguments (may be incorrect):"
    $saveAttrIssues | ForEach-Object { $errors += "  $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" }
}

# Check 3: Variables declared multiple times (basic check)
# This is a simplified check - full analysis requires parsing

if ($errors.Count -gt 0) {
    Write-Host "`n=== PRE-BUILD CHECK FAILED ===" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
    Write-Host "`nPlease fix these issues before building." -ForegroundColor Red
    exit 1
} else {
    Write-Host "Pre-build checks passed!" -ForegroundColor Green
    exit 0
}
```

### 4. Build Process

1. **Run Pre-Build Checks**
   ```powershell
   .\pre-build-check.ps1
   ```

2. **Build Locally** (if Rock solution is available)
   ```powershell
   $msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
   & $msbuild com.bemaservices.RoomManagement.csproj /t:Build /p:Configuration=Debug /p:SolutionDir="C:\path\to\Rock\" /v:minimal
   ```

3. **Check for Errors**
   - Review build output for CS errors
   - Check for warnings that might indicate issues

4. **Deploy and Test**
   - Copy DLLs to RockWeb\bin
   - Copy .ascx/.ascx.cs files to RockWeb\Plugins
   - Clear ASP.NET temp files or restart app pool
   - Test in browser

### 5. Common Error Codes Reference

- **CS0411**: Type arguments cannot be inferred
- **CS0136**: Variable name conflict in scope
- **CS0841**: Variable used before declaration
- **CS1501**: No overload takes X arguments
- **CS1503**: Argument type mismatch

### 6. IDE Integration

**Visual Studio:**
- Enable "Treat warnings as errors" for common issues
- Use Code Analysis (FxCop/SonarAnalyzer)
- Enable nullable reference types for better null checking

**VS Code/Cursor:**
- Install C# extension (OmniSharp)
- Enable real-time error checking
- Use C# Dev Kit for better IntelliSense

