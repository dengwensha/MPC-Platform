# MPC SOLIDWORKS Add-in Registration

This directory contains the explicit local-development registration
workflow for MPC.Host.SolidWorks.

## Prerequisites

- Windows x64
- SOLIDWORKS 2024
- .NET Framework 4.8
- .NET SDK 8
- Elevated PowerShell session
- SOLIDWORKS closed during build and registration

## Build

Run from the repository root:

    dotnet build .\src\MPC.Host.SolidWorks\MPC.Host.SolidWorks.csproj -c Debug -p:Platform=x64

Expected development assembly:

    src\MPC.Host.SolidWorks\bin\x64\Debug\net48\MPC.Host.SolidWorks.dll

## Register

Run from an elevated PowerShell session:

    .\tools\MPC.Host.SolidWorks.Registration\Register-MpcSolidWorksAddin.ps1 -Configuration Debug -Platform x64

The script registers the assembly with 64-bit RegAsm, creates the
SOLIDWORKS add-in registry entry, configures startup loading, and verifies
the resulting registry values.

Use -DisableStartup to register without automatic startup.

## Unregister

Run from an elevated PowerShell session:

    .\tools\MPC.Host.SolidWorks.Registration\Unregister-MpcSolidWorksAddin.ps1 -Configuration Debug -Platform x64

The script removes the COM registration, SOLIDWORKS add-in entry, and
current-user startup entry.

## Development Signing Warning

The development assembly is currently unsigned. RegAsm therefore emits
warning RA0000 when /codebase is used.

This warning is accepted only for local development validation.
Strong-name signing and production deployment packaging remain separate
delivery work.
