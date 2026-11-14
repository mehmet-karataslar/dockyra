[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$errors = New-Object System.Collections.Generic.List[string]

Write-Host "=== Dockyra Planning & Agent Governance Validator ===" -ForegroundColor Cyan

# 1. Verify required core files exist
$requiredFiles = @(
    "AGENTS.md",
    "README.md",
    "ARCHITECTURE.md",
    "ROADMAP.md",
    "TASKS.md",
    "CHANGELOG.md",
    "Dockyra.sln",
    "planning/README.md",
    "planning/PROJECT-OVERVIEW.md",
    "planning/CURRENT-STATE.md",
    "planning/ROADMAP.md",
    "planning/CHANGELOG.md",
    "planning/modules/module-registry.yaml"
)

foreach ($rel in $requiredFiles) {
    $full = Join-Path $repoRoot $rel
    if (-not (Test-Path $full)) {
        $errors.Add("Missing required file: $rel")
    }
}

# 2. Validate module-registry.yaml IDs and paths
$registryFile = Join-Path $repoRoot "planning/modules/module-registry.yaml"
if (Test-Path $registryFile) {
    $regContent = Get-Content $registryFile -Raw
    $modIds = [regex]::Matches($regContent, "(?m)^\s*-\s*id:\s*(MOD-[A-Z0-9-]+)") | ForEach-Object { $_.Groups[1].Value }
    $seenModIds = @{}
    foreach ($id in $modIds) {
        if ($seenModIds.ContainsKey($id)) {
            $errors.Add("Duplicate module ID in module-registry.yaml: $id")
        }
        $seenModIds[$id] = $true
    }

    $regPaths = [regex]::Matches($regContent, "(?m)^\s*-\s*((?:src|tests|planning)/[^\r\n]+)") | ForEach-Object { $_.Groups[1].Value.Trim() }
    foreach ($p in $regPaths) {
        $fullPath = Join-Path $repoRoot ($p -replace '/', [IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path $fullPath)) {
            $errors.Add("Non-existent path in module-registry.yaml: $p")
        }
    }
}

# 3. Validate unique TASK-XXXX IDs across planning/tasks/*.md
$taskFiles = @(
    "planning/tasks/completed.md",
    "planning/tasks/in-progress.md",
    "planning/tasks/backlog.md",
    "planning/tasks/blocked.md"
)
$seenTasks = @{}
foreach ($tf in $taskFiles) {
    $fullTf = Join-Path $repoRoot $tf
    if (Test-Path $fullTf) {
        $content = Get-Content $fullTf -Raw
        $taskMatches = [regex]::Matches($content, "(?m)^\s*-\s*id:\s*(TASK-\d{4})")
        foreach ($m in $taskMatches) {
            $tid = $m.Groups[1].Value
            if ($seenTasks.ContainsKey($tid)) {
                $errors.Add("Duplicate task ID '$tid' found in $tf and $($seenTasks[$tid])")
            }
            $seenTasks[$tid] = $tf
        }
    }
}

# 4. Validate relative markdown links in planning/ and root docs
$mdFiles = Get-ChildItem -Path (Join-Path $repoRoot "planning") -Filter "*.md" -Recurse
foreach ($md in $mdFiles) {
    $text = Get-Content $md.FullName -Raw
    $linkMatches = [regex]::Matches($text, "\[[^\]]+\]\(([^)#]+)(?:#[^)]*)?\)")
    foreach ($lm in $linkMatches) {
        $target = $lm.Groups[1].Value.Trim()
        if ($target -match "^(https?|file|customization|conversation)://") {
            continue
        }
        $resolved = Join-Path $md.DirectoryName ($target -replace '/', [IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path $resolved)) {
            $relMd = [IO.Path]::GetRelativePath($repoRoot, $md.FullName)
            $errors.Add("Broken markdown link '$target' in $relMd")
        }
    }
}

if ($errors.Count -gt 0) {
    Write-Host "Validation FAILED with $($errors.Count) error(s):" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    exit 1
}

Write-Host "Validation PASSED: $($seenModIds.Count) modules, $($seenTasks.Count) tasks, and $($mdFiles.Count) planning documents verified." -ForegroundColor Green
exit 0

