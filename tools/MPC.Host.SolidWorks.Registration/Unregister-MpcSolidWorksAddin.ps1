[CmdletBinding()]
param(
    [string] $Configuration = "Debug",

    [ValidateSet("x64")]
    [string] $Platform = "x64"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AddinGuid =
    "{9A54C5C5-5457-4B41-9147-184FF399B057}"

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
            "Unregistration requires an elevated PowerShell session."
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

Write-Host "===== MPC SOLIDWORKS COM UNREGISTRATION ====="
Write-Host "Assembly: $HostAssemblyPath"
Write-Host "RegAsm:   $RegAsmPath"

& $RegAsmPath `
    $HostAssemblyPath `
    /unregister

if ($LASTEXITCODE -ne 0)
{
    throw `
        "RegAsm unregistration failed with exit code $LASTEXITCODE."
}

$MachineAddinPath =
    "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\$AddinGuid"

$StartupPath =
    "Registry::HKEY_CURRENT_USER\SOFTWARE\SolidWorks\AddInsStartup"

if (Test-Path $MachineAddinPath)
{
    Remove-Item `
        -Path $MachineAddinPath `
        -Recurse `
        -Force
}

if (Test-Path $StartupPath)
{
    $StartupProperties =
        Get-ItemProperty `
            -Path $StartupPath

    if ($StartupProperties.PSObject.Properties.Name -contains
        $AddinGuid)
    {
        Remove-ItemProperty `
            -Path $StartupPath `
            -Name $AddinGuid
    }
}

Write-Host "`n===== UNREGISTRATION VERIFICATION ====="

$MachineRegistrationExists =
    Test-Path $MachineAddinPath

$StartupRegistrationExists =
    $false

if (Test-Path $StartupPath)
{
    $StartupProperties =
        Get-ItemProperty `
            -Path $StartupPath

    $StartupRegistrationExists =
        $StartupProperties.PSObject.Properties.Name -contains
        $AddinGuid
}

Write-Host `
    "Machine registration exists: $MachineRegistrationExists"

Write-Host `
    "Startup registration exists: $StartupRegistrationExists"

if ($MachineRegistrationExists -or
    $StartupRegistrationExists)
{
    throw `
        "One or more SOLIDWORKS registration entries remain."
}

Write-Host
Write-Host "MPC SOLIDWORKS add-in unregistration completed."
