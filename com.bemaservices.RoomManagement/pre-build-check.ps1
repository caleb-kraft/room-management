# Pre-Build Check Script for Rock Plugin
# Run this before building to catch common compilation errors

Write-Host "Running pre-build checks..." -ForegroundColor Cyan
Write-Host ""

$errors = @()
$warnings = @()
$pluginPath = "Plugins\com_bemaservices\RoomManagement"

# Check 1: SetLinqDataSource without AsQueryable
Write-Host "Checking SetLinqDataSource calls..." -ForegroundColor Yellow
$setLinqIssues = Get-ChildItem -Path $pluginPath -Filter "*.ascx.cs" -Recurse | 
    Select-String -Pattern "SetLinqDataSource\([^)]*\)" | 
    Where-Object { 
        $_.Line -notmatch "AsQueryable" -and 
        $_.Line -notmatch "Queryable\(\)" -and
        $_.Line -notmatch "SetLinqDataSource<\w+>" # Explicit type specification is OK
    }

if ($setLinqIssues) {
    $errors += "`n[ERROR] Found SetLinqDataSource calls without AsQueryable() or explicit type:"
    $setLinqIssues | ForEach-Object { 
        $errors += "  $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" 
    }
}

# Check 2: SaveAttributeValues with 3 arguments (may be incorrect for entities)
Write-Host "Checking SaveAttributeValues calls..." -ForegroundColor Yellow
$saveAttrIssues = Get-ChildItem -Path $pluginPath -Filter "*.ascx.cs" -Recurse | 
    Select-String -Pattern "SaveAttributeValues\([^)]*,[^)]*,[^)]*\)"

if ($saveAttrIssues) {
    $warnings += "`n[WARNING] Found SaveAttributeValues calls with 3 arguments (verify these are correct):"
    $saveAttrIssues | ForEach-Object { 
        $warnings += "  $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" 
    }
}

# Check 3: Variables used before declaration (basic pattern check)
Write-Host "Checking for potential variable scope issues..." -ForegroundColor Yellow
# This is a simplified check - looks for common patterns
$varPattern = "var\s+(\w+)\s*="
$files = Get-ChildItem -Path $pluginPath -Filter "*.ascx.cs" -Recurse
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $lines = Get-Content $file.FullName
    $varDeclarations = [regex]::Matches($content, $varPattern)
    
    foreach ($match in $varDeclarations) {
        $varName = $match.Groups[1].Value
        $lineNum = ($content.Substring(0, $match.Index) -split "`n").Count
        
        # Check if variable is used before this declaration in the same method
        $beforeText = $content.Substring(0, $match.Index)
        $methodStart = $beforeText.LastIndexOf("void ") 
        if ($methodStart -gt 0) {
            $methodText = $content.Substring($methodStart, $match.Index - $methodStart)
            $usesBefore = [regex]::Matches($methodText, "\b$varName\b")
            if ($usesBefore.Count -gt 0) {
                $warnings += "`n[WARNING] Potential variable used before declaration: $varName in $($file.Name):$lineNum"
            }
        }
    }
}

# Check 4: Missing null checks before using .Get() results
Write-Host "Checking for potential null reference issues..." -ForegroundColor Yellow
$nullCheckIssues = Get-ChildItem -Path $pluginPath -Filter "*.ascx.cs" -Recurse | 
    Select-String -Pattern "\.Get\([^)]+\)\.[A-Z]" | 
    Where-Object { 
        $prevLine = (Get-Content $_.Path)[$_.LineNumber - 2]
        $prevLine -notmatch "if.*!= null" -and $prevLine -notmatch "if.*== null"
    }

if ($nullCheckIssues.Count -gt 0) {
    $warnings += "`n[WARNING] Found potential null reference issues (verify null checks):"
    $nullCheckIssues | Select-Object -First 10 | ForEach-Object { 
        $warnings += "  $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" 
    }
}

# Display results
Write-Host "`n=== CHECK RESULTS ===" -ForegroundColor Cyan

if ($errors.Count -gt 0) {
    Write-Host "`n❌ ERRORS FOUND:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    Write-Host "`nPlease fix these errors before building." -ForegroundColor Red
    $hasErrors = $true
} else {
    Write-Host "`n✅ No critical errors found!" -ForegroundColor Green
    $hasErrors = $false
}

if ($warnings.Count -gt 0) {
    Write-Host "`n⚠️  WARNINGS:" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
    Write-Host "`nPlease review these warnings." -ForegroundColor Yellow
}

if (-not $hasErrors) {
    Write-Host "`n✅ Pre-build checks passed! Safe to build." -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ Pre-build checks failed. Fix errors before building." -ForegroundColor Red
    exit 1
}

