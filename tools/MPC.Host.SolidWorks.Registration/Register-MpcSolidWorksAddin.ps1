[CmdletBinding()]
param(
    [string] $Configuration = "Debug",

    [ValidateSet("x64")]
    [string] $Platform = "x64",

    [switch] $DisableStartup
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AddinGuid =
    "{9A54C5C5-5457-4B41-9147-184FF399B057}"

$AddinTitle =
    "MPC Manufacturing Intelligence"

$AddinDescription =
    "MPC STEP canonical import add-in"

function Assert-Administrator
{
    $Identity =
        [Security.Principal.WindowsIdentity]::GetCurrent()

    $Principal =
        [Security.Principal.WindowsPrincipal]::new(
            $Identity)

    $AdministratorRole =
        [Security.Principal.WindowsBuiltInRole]::Administrator

    if (!$Principal.IsInRole($AdministratorRole))
    {
        throw `
            "Registration requires an elevated PowerShell session."
    }
}

Assert-Administrator

$RepositoryRoot =
    [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot "..\.."))

$HostAssemblyPath =
    Join-Path `
        $RepositoryRoot `
        "src\MPC.Host.SolidWorks\bin\$Platform\$Configuration\net48\MPC.Host.SolidWorks.dll"

$RegAsmPath =
    Join-Path `
        ([System.Environment]::GetFolderPath(
            [System.Environment+SpecialFolder]::Windows)) `
        "Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"

if (!(Test-Path $HostAssemblyPath -PathType Leaf))
{
    throw `
        "Host assembly was not found: $HostAssemblyPath"
}

if (!(Test-Path $RegAsmPath -PathType Leaf))
{
    throw `
        "64-bit RegAsm was not found: $RegAsmPath"
}

Write-Host "===== MPC SOLIDWORKS COM REGISTRATION ====="
Write-Host "Assembly: $HostAssemblyPath"
Write-Host "RegAsm:   $RegAsmPath"

& $RegAsmPath `
    $HostAssemblyPath `
    /codebase

if ($LASTEXITCODE -ne 0)
{
    throw `
        "RegAsm registration failed with exit code $LASTEXITCODE."
}

$MachineAddinPath =
    "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\$AddinGuid"

$StartupPath =
    "Registry::HKEY_CURRENT_USER\SOFTWARE\SolidWorks\AddInsStartup"

New-Item `
    -Path $MachineAddinPath `
    -Force |
Out-Null

Set-Item `
    -Path $MachineAddinPath `
    -Value 1

New-ItemProperty `
    -Path $MachineAddinPath `
    -Name "Title" `
    -PropertyType String `
    -Value $AddinTitle `
    -Force |
Out-Null

New-ItemProperty `
    -Path $MachineAddinPath `
    -Name "Description" `
    -PropertyType String `
    -Value $AddinDescription `
    -Force |
Out-Null

New-Item `
    -Path $StartupPath `
    -Force |
Out-Null

$StartupValue =
    if ($DisableStartup)
    {
        0
    }
    else
    {
        1
    }

New-ItemProperty `
    -Path $StartupPath `
    -Name $AddinGuid `
    -PropertyType DWord `
    -Value $StartupValue `
    -Force |
Out-Null

Write-Host "`n===== REGISTRATION VERIFICATION ====="

$MachineRegistration =
    Get-ItemProperty `
        -Path $MachineAddinPath

$StartupRegistration =
    Get-ItemProperty `
        -Path $StartupPath

Write-Host "Title:       $($MachineRegistration.Title)"
Write-Host "Description: $($MachineRegistration.Description)"
Write-Host "Startup:     $($StartupRegistration.$AddinGuid)"
Write-Host
Write-Host "MPC SOLIDWORKS add-in registration completed."
Write-Warning `
    "The current development assembly is unsigned. The RegAsm /codebase warning is expected for local development registration."
