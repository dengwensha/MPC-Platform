param(
 [Parameter(Mandatory=$true)][string]$BaselineApp,
 [Parameter(Mandatory=$true)][string]$CurrentApp,
 [Parameter(Mandatory=$true)][string]$Inputs,
 [Parameter(Mandatory=$true)][string]$Output,
 [switch]$PublicSubset
)
$ErrorActionPreference='Stop'
$PSNativeCommandUseErrorActionPreference=$false
$repo=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$Output=[IO.Path]::GetFullPath($Output)
if(Test-Path $Output){throw 'Fresh output required.'}
$files=@(Get-ChildItem -LiteralPath $Inputs -File | Where-Object { $_.Extension -in '.step','.stp' })
$expected=if($PublicSubset){9}else{20}
if($files.Count -ne $expected){throw "Expected $expected STEP inputs; got $($files.Count)."}
if(-not $PublicSubset){
 $manifest=Get-Content -LiteralPath (Join-Path $repo 'docs/reviews/Tooling/evidence/B6-A-Experiment/InputManifest.json') -Raw | ConvertFrom-Json
 foreach($row in $manifest.Files){
  $matches=@($files | Where-Object {$_.Name -ceq $row.InputName})
  if($matches.Count -ne 1 -or (Get-FileHash -LiteralPath $matches[0].FullName -Algorithm SHA256).Hash -cne $row.Sha256){throw "Input identity mismatch: $($row.InputName)"}
 }
}
New-Item -ItemType Directory $Output | Out-Null
function Invoke-Checked([string[]]$Arguments){$executable=$Arguments[0];$remaining=$Arguments[1..($Arguments.Length-1)];& $executable @remaining;if($LASTEXITCODE -ne 0){throw "Experiment command failed: $executable"}}
$fixtureRoot=Join-Path ([IO.Path]::GetTempPath()) ("mpc-a-fixtures-"+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $fixtureRoot | Out-Null
Push-Location $repo
try {
 Invoke-Checked -Arguments @('dotnet','build','tools/ci/B6CaptureProbe/B6CaptureProbe.csproj','-c','Release','--warnaserror','-m:1')
 Invoke-Checked -Arguments @('dotnet','build','tools/ci/M2TargetProbe/M2TargetProbe.csproj','-c','Release','--warnaserror','-m:1')
 $controls=Join-Path $fixtureRoot 'controls'
 Invoke-Checked -Arguments @('python','tools/ci/M2ScopeDraft/make_controls.py','.',$controls)
 Copy-Item 'tests/MPC.Application.Tests/TestData/Step/RU-QuarterCone-TargetMetadata.step' (Join-Path $controls 'C13-ConeTypedProduct.step')
 $negative=Join-Path $fixtureRoot 'negatives'
 Invoke-Checked -Arguments @('python','tools/ci/B6CaptureProbe/make_a_negatives.py','.',$negative)
 $groups=@(@{Name='corpus';Path=$Inputs;Count=$expected},@{Name='controls';Path=$controls;Count=13},@{Name='negatives';Path=$negative;Count=5})
 foreach($trial in 1..2){
  foreach($label in @('before','after')){
   $app=if($label -eq 'before'){$BaselineApp}else{$CurrentApp}
   foreach($group in $groups){
    Invoke-Checked -Arguments @('dotnet','tools/ci/M2TargetProbe/bin/Release/net8.0/M2TargetProbe.dll',$app,$group.Path,(Join-Path $Output "audit-$trial/$label/$($group.Name)"))
   }
  }
  foreach($group in $groups){
   Invoke-Checked -Arguments @('python','tools/ci/B6CaptureProbe/compare_target_semantics.py',(Join-Path $Output "audit-$trial/before/$($group.Name)"),(Join-Path $Output "audit-$trial/after/$($group.Name)"),(Join-Path $Output "Comparison-$trial-$($group.Name).json"),'--count',"$($group.Count)")
  }
 }
 foreach($label in @('before','after')){
  foreach($group in $groups){
   Invoke-Checked -Arguments @('python','tools/ci/B6CaptureProbe/compare_target_semantics.py',(Join-Path $Output "audit-1/$label/$($group.Name)"),(Join-Path $Output "audit-2/$label/$($group.Name)"),(Join-Path $Output "Repeat-$label-$($group.Name).json"),'--count',"$($group.Count)")
  }
 }
 Invoke-Checked -Arguments @('python','tools/ci/B6CaptureProbe/run_b6.py','--probe','tools/ci/B6CaptureProbe/bin/Release/net8.0/B6CaptureProbe.dll','--baseline',$BaselineApp,'--current',$CurrentApp,'--inputs',$Inputs,'--output',(Join-Path $Output 'capture'),'--require-target-equality')
 Invoke-Checked -Arguments @('python','tools/ci/B6CaptureProbe/evaluate_a.py',(Join-Path $Output 'capture'),(Join-Path $Output 'Assessment.json'),'--platform','Windows','--expected-count','20')
} finally {
 if(Test-Path $fixtureRoot){Remove-Item -Recurse -LiteralPath $fixtureRoot}
 Pop-Location
}
