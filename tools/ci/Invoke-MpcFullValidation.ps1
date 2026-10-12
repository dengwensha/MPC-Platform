param([Parameter(Mandatory=$true)][ValidateSet('Prepare','Regression','Corpus')][string]$Stage)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = Join-Path $env:RUNNER_TEMP 'mpc-full-validation'
$evidence = Join-Path $root 'evidence'
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
function Check-Hash([string]$Path, [string]$Expected) {
    if ((Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -ne $Expected) {
        throw "SHA256 mismatch: $([IO.Path]::GetFileName($Path))"
    }
}
if ($Stage -eq 'Prepare') {
    $packages = @(
        @{Name='solidworks-interop.zip'; Hash='7e657973ac997091cd893e10065135e804777b70a4026b3125569a07a71ffbc0'; Folder='interop'},
        @{Name='p01-real-parts.zip'; Hash='497d4fc9b7df2d2b5ff93b89fa7ad2b78fd53ba5ddd26ddb85898ef2dbc516da'; Folder='corpus'}
    )
    foreach ($package in $packages) {
        gh release download ci-inputs-v1 --repo $env:GITHUB_REPOSITORY --pattern $package.Name --dir $root
        if ($LASTEXITCODE -ne 0) { throw "Release download failed: $($package.Name)" }
        $zip = Join-Path $root $package.Name
        Check-Hash $zip $package.Hash
        Expand-Archive -LiteralPath $zip -DestinationPath (Join-Path $root $package.Folder)
    }
    $interop = Join-Path $root 'verified-interop'
    New-Item -ItemType Directory -Path $interop | Out-Null
    $manifest = @()
    foreach ($name in @('SolidWorks.Interop.sldworks.dll','SolidWorks.Interop.swconst.dll','SolidWorks.Interop.swpublished.dll')) {
        $found = @(Get-ChildItem (Join-Path $root 'interop') -Recurse -File -Filter $name)
        if ($found.Count -ne 1) { throw "Expected one DLL: $name" }
        Copy-Item -LiteralPath $found[0].FullName -Destination $interop
        $version = [Reflection.AssemblyName]::GetAssemblyName($found[0].FullName).Version.ToString()
        $manifest += [ordered]@{Name=$name; AssemblyVersion=$version; SHA256=(Get-FileHash $found[0].FullName).Hash}
    }
    $manifest | ConvertTo-Json | Set-Content (Join-Path $evidence 'interop-manifest.json')
    "SolidWorksApiRedistPath=$interop" | Add-Content $env:GITHUB_ENV
    [ordered]@{Commit=$env:GITHUB_SHA; Run="$env:GITHUB_SERVER_URL/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID"; Release='ci-inputs-v1'; Packages=$packages} |
        ConvertTo-Json -Depth 5 | Set-Content (Join-Path $evidence 'provenance.json')
    Write-Host 'PASS - release package hashes and three Interop assemblies verified.'
}
if ($Stage -eq 'Regression') {
    dotnet build src/MPC.sln -c Release --no-incremental --warnaserror "-p:SolidWorksApiRedistPath=$env:SolidWorksApiRedistPath"
    if ($LASTEXITCODE -ne 0) { throw 'Full solution Release build failed.' }
    $trxDir = Join-Path $evidence 'tests'
    dotnet test src/MPC.sln -c Release --no-build --no-restore "-p:SolidWorksApiRedistPath=$env:SolidWorksApiRedistPath" --logger trx --results-directory $trxDir
    if ($LASTEXITCODE -ne 0) { throw 'Full solution regression failed.' }
    $files = @(Get-ChildItem $trxDir -Filter '*.trx')
    if ($files.Count -ne 5) { throw "Expected five test project TRX files; got $($files.Count)." }
    $expected = @('MPC.Tests.dll','MPC.Contracts.Tests.dll','MPC.Application.Tests.dll','MPC.Worker.Tests.dll','MPC.Host.SolidWorks.Tests.dll')
    $observed = @()
    $total = 0
    foreach ($file in $files) {
        [xml]$trx = Get-Content $file.FullName -Raw
        $c = $trx.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
        $methods = $trx.SelectNodes("//*[local-name()='TestMethod']")
        $assemblies = @($methods | ForEach-Object { [IO.Path]::GetFileName($_.codeBase) } | Sort-Object -Unique)
        if ($assemblies.Count -ne 1) { throw 'Unexpected test assembly grouping.' }
        $observed += $assemblies[0]
        if ($null -eq $c -or [int]$c.total -le 0 -or [int]$c.total -ne [int]$c.passed -or [int]$c.failed -ne 0 -or [int]$c.executed -ne [int]$c.total) {
            throw "Incomplete test results: $($file.Name)"
        }
        $total += [int]$c.total
    }
    if (@(Compare-Object ($expected | Sort-Object) ($observed | Sort-Object)).Count -ne 0) { throw 'Test project coverage mismatch.' }
    "PASS - full solution regression: $total/$total; failed 0; skipped 0." |
        Tee-Object -FilePath (Join-Path $evidence 'regression-summary.txt') |
        Add-Content $env:GITHUB_STEP_SUMMARY
    git diff --check HEAD^ HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Whitespace check failed.' }
    $dirty = @(git status --porcelain=v1 --untracked-files=all)
    if ($LASTEXITCODE -ne 0 -or $dirty.Count -gt 0) { throw 'Runner checkout is not clean.' }
}
if ($Stage -eq 'Corpus') {
    $parts = @(
        @{Id='R01'; Name='P01-R01-Prismatic.STEP'; Hash='E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD'},
        @{Id='R02'; Name='P01-R02-InternalBoundaries.STEP'; Hash='0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F'},
        @{Id='R03'; Name='P01-R03-CurvedSurfaces.STEP'; Hash='319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12'}
    )
    $worker = (Resolve-Path 'src/MPC.Worker/bin/x64/Release/net8.0/MPC.Worker.dll').Path
    $rows = @()
    $allSucceeded = $true
    foreach ($part in $parts) {
        $found = @(Get-ChildItem (Join-Path $root 'corpus') -Recurse -File -Filter $part.Name)
        if ($found.Count -ne 1) { throw "Expected one corpus input: $($part.Name)" }
        Check-Hash $found[0].FullName $part.Hash
        $fingerprints = @()
        foreach ($iteration in 1..3) {
            $prefix = Join-Path $evidence "$($part.Id)-$iteration"
            $requestId = [Guid]::NewGuid().ToString()
            @{ContractVersion='1.0'; RequestId=$requestId; Operation=0; InputArtifactPath=$found[0].FullName} |
                ConvertTo-Json | Set-Content "$prefix-request.json" -Encoding utf8
            $watch = [Diagnostics.Stopwatch]::StartNew()
            $arguments = @( ('"{0}"' -f $worker), ('"{0}-request.json"' -f $prefix), ('"{0}-result.json"' -f $prefix) )
            $process = Start-Process dotnet -ArgumentList $arguments -PassThru -NoNewWindow -RedirectStandardOutput "$prefix-stdout.txt" -RedirectStandardError "$prefix-stderr.txt"
            if (-not $process.WaitForExit(120000)) {
                $process.Kill($true)
                throw "Worker timeout: $($part.Id)"
            }
            $process.WaitForExit()
            $watch.Stop()
            if (-not (Test-Path "$prefix-result.json")) { throw "Worker result missing: $($part.Id)" }
            $result = Get-Content "$prefix-result.json" -Raw | ConvertFrom-Json
            if ($result.RequestId -ne $requestId -or $result.ContractVersion -ne '1.0') { throw 'Result correlation/version mismatch.' }
            $ok = $process.ExitCode -eq 0 -and $result.Status -eq 0 -and @($result.Diagnostics).Count -eq 0 -and $null -ne $result.Summary
            if (-not $ok) { $allSucceeded = $false }
            $rows += [pscustomobject][ordered]@{Part=$part.Id; Run=$iteration; Exit=$process.ExitCode; Status=$result.Status; Diagnostics=@($result.Diagnostics).Count; DurationMs=$watch.ElapsedMilliseconds; Summary=$result.Summary}
            $result.PSObject.Properties.Remove('RequestId')
            $payload = $result | ConvertTo-Json -Depth 50 -Compress
            $fingerprints += [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($payload)))
        }
        Check-Hash $found[0].FullName $part.Hash
        if (@($fingerprints | Sort-Object -Unique).Count -ne 1) { throw "Nondeterministic worker result: $($part.Id)" }
    }
    $rows | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $evidence 'corpus-summary.json')
    $summary = @('## Real STEP corpus (three worker processes per part)','', '| Part | Run | Exit | Status (0=Succeeded, 2=Failed) | Diagnostics |','|---|---:|---:|---:|---:|')
    foreach ($row in $rows) { $summary += "| $($row.Part) | $($row.Run) | $($row.Exit) | $($row.Status) | $($row.Diagnostics) |" }
    $summary += ''
    $summary += 'Real STEP worker runs on GitHub; interactive SOLIDWORKS add-in operation is not tested.'
    $summary | Set-Content (Join-Path $evidence 'corpus-summary.md')
    $summary | Add-Content $env:GITHUB_STEP_SUMMARY
    $summary | Write-Output
    if (-not $allSucceeded) { throw 'Corpus compatibility is not complete. See retained results; full regression is reported separately.' }
}
