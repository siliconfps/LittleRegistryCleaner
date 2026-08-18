# Build & Package Script para Little Registry Cleaner
# Este script compila a solucao em Release e gera o executavel instalador (Setup.exe)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  Compilando e Gerando Instalador: Little Registry Cleaner" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Localizar MSBuild
$msbuildCandidates = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
)

$msbuildPath = $null
foreach ($path in $msbuildCandidates) {
    if (Test-Path $path) {
        $msbuildPath = $path
        break
    }
}

if (-not $msbuildPath) {
    $cmd = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($cmd) {
        $msbuildPath = $cmd.Source
    }
}

if (-not $msbuildPath) {
    Write-Error "MSBuild nao foi encontrado no sistema. Certifique-se de que o .NET Framework 4.0 ou Visual Studio esteja instalado."
    exit 1
}

Write-Host "`n[1/3] Usando MSBuild: $msbuildPath" -ForegroundColor Green

# 2. Localizar Inno Setup (ISCC.exe)
$isccCandidates = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 5\ISCC.exe",
    "C:\Program Files\Inno Setup 5\ISCC.exe"
)

$isccPath = $null
foreach ($path in $isccCandidates) {
    if (Test-Path $path) {
        $isccPath = $path
        break
    }
}

if (-not $isccPath) {
    $cmd = Get-Command iscc -ErrorAction SilentlyContinue
    if ($cmd) {
        $isccPath = $cmd.Source
    }
}

if (-not $isccPath) {
    Write-Error "Compilador do Inno Setup (ISCC.exe) nao foi encontrado. Instale com 'winget install JRSoftware.InnoSetup'."
    exit 1
}

Write-Host "[2/3] Usando Inno Setup Compiler: $isccPath" -ForegroundColor Green

# 3. Compilar a Solucao em modo Release
Write-Host "`n[+] Compilando Little Registry Cleaner (Release)..." -ForegroundColor Yellow
$slnPath = Join-Path $PSScriptRoot "Little Registry Cleaner.sln"
& $msbuildPath $slnPath /p:Configuration=Release /v:minimal /nologo

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha na compilacao da solucao com MSBuild."
    exit $LASTEXITCODE
}

Write-Host "[OK] Binarios compilados com sucesso!" -ForegroundColor Green

# 4. Gerar assemblies satelites de recursos/idiomas (Localizacao)
Write-Host "`n[+] Compilando bibliotecas de idiomas (20 idiomas)..." -ForegroundColor Yellow

Add-Type -AssemblyName System.Windows.Forms

$cscPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if (!(Test-Path $cscPath)) {
    $cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
}

$baseOut = Join-Path $PSScriptRoot "Little Registry Cleaner\bin\Release"
$langs = @('ar', 'de', 'el', 'es', 'fa', 'fr', 'hu', 'it', 'ja', 'lt', 'nl', 'pl', 'pt', 'ru', 'sv-SE', 'th', 'tr', 'vi', 'zh-CHS', 'zh-CHT')

function Compile-SatelliteAssembly {
    param(
        [string]$AssemblyBaseName,
        [string]$Culture,
        [hashtable[]]$Resources,
        [string]$OutDir
    )

    $targetDir = Join-Path $OutDir $Culture
    if (!(Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }
    $outDll = Join-Path $targetDir "$AssemblyBaseName.resources.dll"

    # Obter a versao exata do assembly principal
    $mainBinPath = Join-Path $OutDir "$AssemblyBaseName.exe"
    if (!(Test-Path $mainBinPath)) {
        $mainBinPath = Join-Path $OutDir "$AssemblyBaseName.dll"
    }
    $asmVer = "2.0.0.0"
    if (Test-Path $mainBinPath) {
        $asmVer = [System.Reflection.AssemblyName]::GetAssemblyName($mainBinPath).Version.ToString()
    }

    $tempResFiles = @()
    $resArgs = @()

    foreach ($item in $Resources) {
        $resxFile = Join-Path $PSScriptRoot $item.Resx
        if (Test-Path $resxFile) {
            $tempFile = [System.IO.Path]::GetTempFileName()
            $tempResFiles += $tempFile
            
            $reader = New-Object System.Resources.ResXResourceReader($resxFile)
            $writer = New-Object System.Resources.ResourceWriter($tempFile)
            foreach ($d in $reader) {
                $writer.AddResource($d.Key, $d.Value)
            }
            $writer.Close()
            $reader.Close()
            
            $resInternalName = "$($item.BaseName).$Culture.resources"
            $resArgs += "/resource:`"$tempFile`",`"$resInternalName`""
        }
    }

    if ($resArgs.Count -gt 0) {
        $csTemp = [System.IO.Path]::GetTempFileName() + ".cs"
        "[assembly: System.Reflection.AssemblyCulture(`"$Culture`")] [assembly: System.Reflection.AssemblyVersion(`"$asmVer`")]" | Set-Content $csTemp

        $cmdArgs = @("/target:library", "/nologo", "/nowarn:1607", "/out:`"$outDll`"", "`"$csTemp`"") + $resArgs
        & $cscPath $cmdArgs | Out-Null

        Remove-Item $csTemp -ErrorAction SilentlyContinue
    }

    foreach ($f in $tempResFiles) { Remove-Item $f -ErrorAction SilentlyContinue }
}

foreach ($l in $langs) {
    # 1. Little Registry Cleaner
    Compile-SatelliteAssembly -AssemblyBaseName "Little Registry Cleaner" -Culture $l -OutDir $baseOut -Resources @(
        @{ Resx = "Little Registry Cleaner\About.$l.resx"; BaseName = "Little_Registry_Cleaner.About" },
        @{ Resx = "Little Registry Cleaner\CrashReporter.$l.resx"; BaseName = "Little_Registry_Cleaner.CrashReporter" },
        @{ Resx = "Little Registry Cleaner\Main.$l.resx"; BaseName = "Little_Registry_Cleaner.Main" },
        @{ Resx = "Little Registry Cleaner\Restore.$l.resx"; BaseName = "Little_Registry_Cleaner.Restore" },
        @{ Resx = "Little Registry Cleaner\ScanDlg.$l.resx"; BaseName = "Little_Registry_Cleaner.ScanDlg" },
        @{ Resx = "Little Registry Cleaner\Properties\Resources.$l.resx"; BaseName = "Little_Registry_Cleaner.Properties.Resources" },
        @{ Resx = "Little Registry Cleaner\Scanners\Strings.$l.resx"; BaseName = "Little_Registry_Cleaner.Scanners.Strings" }
    )

    # 2. Little Startup Manager
    Compile-SatelliteAssembly -AssemblyBaseName "Little Startup Manager" -Culture $l -OutDir $baseOut -Resources @(
        @{ Resx = "Little Startup Manager\EditRunItem.$l.resx"; BaseName = "Little_Startup_Manager.EditRunItem" },
        @{ Resx = "Little Startup Manager\NewRunItem.$l.resx"; BaseName = "Little_Startup_Manager.NewRunItem" },
        @{ Resx = "Little Startup Manager\StartupManager.$l.resx"; BaseName = "Little_Startup_Manager.StartupManager" },
        @{ Resx = "Little Startup Manager\Properties\Resources.$l.resx"; BaseName = "Little_Startup_Manager.Properties.Resources" }
    )

    # 3. Little Uninstall Manager
    Compile-SatelliteAssembly -AssemblyBaseName "Little Uninstall Manager" -Culture $l -OutDir $baseOut -Resources @(
        @{ Resx = "Little Uninstall Manager\UninstallManager.$l.resx"; BaseName = "Little_Uninstall_Manager.UninstallManager" },
        @{ Resx = "Little Uninstall Manager\Properties\Resources.$l.resx"; BaseName = "Little_Uninstall_Manager.Properties.Resources" }
    )

    # 4. AutoUpdater.NET
    Compile-SatelliteAssembly -AssemblyBaseName "AutoUpdater.NET" -Culture $l -OutDir $baseOut -Resources @(
        @{ Resx = "Little Registry Cleaner\AutoUpdater.NET\DownloadUpdateDialog.$l.resx"; BaseName = "AutoUpdaterDotNET.DownloadUpdateDialog" },
        @{ Resx = "Little Registry Cleaner\AutoUpdater.NET\RemindLaterForm.$l.resx"; BaseName = "AutoUpdaterDotNET.RemindLaterForm" },
        @{ Resx = "Little Registry Cleaner\AutoUpdater.NET\UpdateForm.$l.resx"; BaseName = "AutoUpdaterDotNET.UpdateForm" },
        @{ Resx = "Little Registry Cleaner\AutoUpdater.NET\Properties\Resources.$l.resx"; BaseName = "AutoUpdaterDotNET.Properties.Resources" }
    )
}

Write-Host "[OK] Assemblies de idiomas gerados com sucesso para $($langs.Count) culturas!" -ForegroundColor Green

# 5. Compilar o Instalador Inno Setup
Write-Host "`n[3/3] Gerando executavel instalador..." -ForegroundColor Yellow
$issPath = Join-Path $PSScriptRoot "installer.iss"
& $isccPath $issPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha ao gerar o instalador com Inno Setup."
    exit $LASTEXITCODE
}

# 5. Localizar arquivo final gerado
$outputDir = Join-Path $PSScriptRoot "Output"
$setupFile = Get-ChildItem -Path $outputDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($setupFile) {
    $sizeMb = [math]::Round($setupFile.Length / 1MB, 2)
    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host " [SUCESSO] Instalador gerado com exito!" -ForegroundColor Green
    Write-Host " Arquivo: $($setupFile.FullName)" -ForegroundColor Cyan
    Write-Host " Tamanho: $sizeMb MB" -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Green
} else {
    Write-Host "`nInstalador compilado no diretorio: $outputDir" -ForegroundColor Green
}
