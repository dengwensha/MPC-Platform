"""Create a disposable instrumented source copy. NEVER edits the repository's src/.
Only clean, committed current production inputs are admitted. Diagnostic DLLs are not products.
"""
import hashlib,io,json,pathlib,subprocess,sys,tarfile
repo=pathlib.Path(sys.argv[1]).resolve();dest=pathlib.Path(sys.argv[2]).resolve()
def git(*args):
    return subprocess.check_output(['git',*args],cwd=repo)
def normalized(data):
    return data.decode('utf-8-sig').replace('\r\n','\n')
def digest(data):
    return hashlib.sha256(normalized(data).encode()).hexdigest().upper()
ref=git('rev-parse','HEAD').decode().strip()
checked=['src/MPC.Application/Inventory/InventoryEvidence.cs',
         'src/MPC.Application/Inventory/TargetProducer.cs',
         'src/MPC.Application/Inventory/TargetBinding.cs']
inputs=['src','Directory.Build.props','Directory.Build.targets','Directory.Packages.props',
        'global.json','NuGet.Config','NuGet.config','nuget.config',
        'docs/reviews/Tooling/M2-Target-Role-Manifest-v1-Contract.json']
def source_state():
    hashes={}
    for path in checked:
        current=digest((repo/path).read_bytes());committed=digest(git('show',ref+':'+path))
        if current!=committed:
            raise RuntimeError('Current source differs from archived HEAD: '+path)
        hashes[path]=current
    if git('status','--porcelain','--untracked-files=all','--',*inputs).strip():
        raise RuntimeError('Production/build inputs must be clean and committed before profiling')
    if git('rev-parse','HEAD').decode().strip()!=ref:
        raise RuntimeError('HEAD changed during source guard')
    return hashes
assert dest!=repo and repo not in dest.parents,'Diagnostic copy must be outside repository'
assert not dest.exists(),'Fresh destination required'
source_hashes=source_state()  # Fail BEFORE creating any diagnostic output.
data=git('archive',ref)       # Current commit, never a historical hard-coded tree.
if source_state()!=source_hashes:
    raise RuntimeError('Source changed while creating diagnostic archive')
dest.mkdir(parents=True)
with tarfile.open(fileobj=io.BytesIO(data)) as archive:
    if hasattr(tarfile,'data_filter'):
        archive.extractall(dest,filter='data')
    else:
        members=archive.getmembers()
        assert all((m.isfile() or m.isdir()) and not pathlib.PurePosixPath(m.name).is_absolute() and '..' not in pathlib.PurePosixPath(m.name).parts for m in members)
        archive.extractall(dest,members=members)
for path,expected in source_hashes.items():
    if digest((dest/path).read_bytes())!=expected:
        raise RuntimeError('Diagnostic starting source differs from current source: '+path)
changes=[]
def inject(path,old,new):
    p=dest/path;s=p.read_text(encoding='utf-8-sig');assert s.count(old)==1,(path,old,s.count(old))
    before=hashlib.sha256(s.encode()).hexdigest().upper();s=s.replace(old,new)
    p.write_text(s,encoding='utf-8');changes.append({'Path':path,'BeforeNormalizedSha256':before,'AfterNormalizedSha256':hashlib.sha256(s.encode()).hexdigest().upper(),'Anchor':old,'Replacement':new})
def mark(name,root='this'):
    return '((System.Action<string, object?>?)System.AppDomain.CurrentDomain.GetData("MPC.B6.DiagnosticOnly.Checkpoint"))?.Invoke("'+name+'", '+root+');'
p='src/MPC.Application/Inventory/InventoryEvidence.cs'
a='        TargetData = TargetProducer.Read(result.Value);';inject(p,a,'        '+mark('Target.Before','result.Value')+'\n'+a)
a='            return new InventoryCaptureResult(new ImportSession(hash, result, revision, bytes, headerUnitRevision), null, null);';inject(p,a,'            '+mark('Inventory.Complete','result.Value')+'\n'+a)
p='src/MPC.Application/Inventory/TargetProducer.cs'
a='        internal TargetData Run()\n        {';inject(p,a,a+'\n            '+mark('Target.Enter'))
a='            // Include incomplete/unknown records';inject(p,a,'            '+mark('Target.Rows')+'\n'+a)
a='            foreach (var (id, row) in _rows)\n            {';inject(p,a,'            '+mark('Target.Incoming')+'\n'+a)
a='            foreach (var (id, row) in _rows.Where(p => p.Value.Role == "Context")) Context(id);';inject(p,a,'            '+mark('Target.Fields')+'\n'+a+'\n            '+mark('Target.Contexts'))
a='            PresentationAndOwnership();';inject(p,a,a+'\n            '+mark('Target.Ownership'))
a='            return new(_root, _geometry, _main, _context, _inventory.TotalEntityCount, _guardCalled, binding, _compatibility, _reasons);';inject(p,a,'            '+mark('Target.End')+'\n'+a)
p='src/MPC.Application/Inventory/TargetBinding.cs'
a='                try { ImportUnitPrecheck.EnsureCompatible(_inventory!, _root); _compatibility = TargetLayerStatus.Observed; }';inject(p,a,'                '+mark('Precheck.Before')+'\n'+a)
# Place AFTER the existing try/catch, still inside the same unique-root branch.
a='            }\n            else Add("Compatibility", "DependencyUnavailable");';inject(p,a,'                '+mark('Precheck.After')+'\n'+a)
report={'SourceCommit':ref,'ApprovalInferred':False,'SourceGuard':{'CurrentThreeFileNormalizedSha256':source_hashes,'ProductionInputsClean':True,'ArchiveMatchesCurrent':True},'Purpose':'Disposable diagnostic clone only. Forced-GC/rooted checkpoints perturb liveness/timing. No product authority or budget acceptance.','Changes':changes}
(dest/'B6DiagnosticOverlay.json').write_text(json.dumps(report,indent=2)+'\n')
print('Created diagnostic-only copy',dest,'with',len(changes),'exact-anchor insertions')
