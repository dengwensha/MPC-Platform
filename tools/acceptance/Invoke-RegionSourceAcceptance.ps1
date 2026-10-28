#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),
    [string]$EvidenceDirectory = (Join-Path ([IO.Path]::GetTempPath()) ("mpc-source-acceptance-" + [Guid]::NewGuid().ToString('N')))
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workerProject = Join-Path $RepositoryRoot 'src/MPC.Worker/MPC.Worker.csproj'
# Build only Worker and its non-COM dependencies, never the SOLIDWORKS solution.
dotnet build $workerProject -c Release --warnaserror
if ($LASTEXITCODE -ne 0) { throw 'Worker Release build failed.' }
$worker = Join-Path $RepositoryRoot 'src/MPC.Worker/bin/x64/Release/net8.0/MPC.Worker.dll'
if (-not (Test-Path -LiteralPath $worker)) { throw 'Built Worker DLL is missing.' }
$golden = Join-Path $RepositoryRoot 'tests/MPC.Tests/TestData/Step/Golden/T001_PRISM.step'
$originalHash = (Get-FileHash -LiteralPath $golden -Algorithm SHA256).Hash
# Unique directory; never overwrite earlier evidence.
if (Test-Path -LiteralPath $EvidenceDirectory) { throw 'Evidence directory must not already exist.' }
$null = New-Item -ItemType Directory -Path $EvidenceDirectory
$EvidenceDirectory = (Resolve-Path -LiteralPath $EvidenceDirectory).Path
$copy = Join-Path $EvidenceDirectory 'source copy.step'
Copy-Item -LiteralPath $golden -Destination $copy
$changed = Join-Path $EvidenceDirectory 'changed copy.step'
Copy-Item -LiteralPath $golden -Destination $changed
[IO.File]::AppendAllText($changed, [Environment]::NewLine)
$cases = @(
    @{Name='match'; Path=$copy; Face='#8'; Requirement=1; Outcome=1; Exit=0},
    @{Name='changed-file'; Path=$changed; Face='#8'; Requirement=1; Outcome=4; Exit=2},
    @{Name='missing-face'; Path=$copy; Face='#999999'; Requirement=1; Outcome=2; Exit=2},
    @{Name='invalid-face'; Path=$copy; Face='#0'; Requirement=1; Outcome=8; Exit=2},
    @{Name='unverified-material'; Path=$copy; Face='#8'; Requirement=2; Outcome=6; Exit=2}
)
$rows = @()
foreach ($case in $cases) {
    $requestId = [Guid]::NewGuid().ToString('D')
    $requestPath = Join-Path $EvidenceDirectory ($case.Name + '-request.json')
    $resultPath = Join-Path $EvidenceDirectory ($case.Name + '-result.json')
    @{
        ContractVersion='1.1'; RequestId=$requestId; Operation=1
        InputArtifactPath=$case.Path; SourceFaceId=$case.Face
        ExpectedDocumentSha256=$originalHash; Requirement=$case.Requirement
    } | ConvertTo-Json | Set-Content -LiteralPath $requestPath -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = 'dotnet'
    $start.UseShellExecute = $false
    # ArgumentList preserves spaces without shell quoting.
    $start.ArgumentList.Add($worker)
    $start.ArgumentList.Add($requestPath)
    $start.ArgumentList.Add($resultPath)
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    try {
        if (-not $process.Start()) { throw 'Worker did not start.' }
        if (-not $process.WaitForExit(30000)) {
            $process.Kill($true)
            $process.WaitForExit()
            throw "Worker timeout: $($case.Name)"
        }
        $exitCode = $process.ExitCode
    }
    finally { $process.Dispose() }
    if (-not (Test-Path -LiteralPath $resultPath)) { throw "Missing result: $($case.Name)" }
    $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    if ($result.ContractVersion -cne '1.1' -or $result.RequestId -cne $requestId -or
        $result.Operation -cne 1 -or $result.Outcome -cne $case.Outcome -or
        $exitCode -ne $case.Exit) {
        throw "Unexpected response or exit code: $($case.Name); see $resultPath"
    }
    $rows += [pscustomobject]@{Case=$case.Name; Outcome=$result.Outcome; Exit=$exitCode; Passed=$true}
}
if ((Get-FileHash -LiteralPath $golden -Algorithm SHA256).Hash -cne $originalHash) {
    throw 'Original golden STEP changed.'
}
$commit = & git -C $RepositoryRoot rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Cannot identify repository commit.' }
[pscustomobject]@{
    Commit=$commit; Configuration='Release'; SourceSha256=$originalHash
    Scope='Worker source linkage only; no SOLIDWORKS UI, material or tool approval'
    Cases=$rows
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'summary.json') -Encoding utf8
$rows | Format-Table
Write-Output "PASS: $($rows.Count)/$($cases.Count). Evidence: $EvidenceDirectory"
