"""Create a disposable instrumented source copy. NEVER edits the repository's src/.
Only the pinned revision is admitted. Generated DLLs are diagnostic-only, not products.
"""
import hashlib,io,json,pathlib,subprocess,sys,tarfile
repo=pathlib.Path(sys.argv[1]).resolve();dest=pathlib.Path(sys.argv[2]).resolve()
ref='9b5f0334001e9d0558e95865bb9001b9ba12cb49'
assert dest!=repo and repo not in dest.parents,'Diagnostic copy must be outside repository'
assert not dest.exists(),'Fresh destination required'
# Tracked project source from exact approved main; raw CAD is not included.
data=subprocess.check_output(['git','archive',ref],cwd=repo)
dest.mkdir(parents=True)
with tarfile.open(fileobj=io.BytesIO(data)) as archive:
    if hasattr(tarfile,'data_filter'):
        archive.extractall(dest,filter='data')
    else:
        members=archive.getmembers()
        assert all((m.isfile() or m.isdir()) and not pathlib.PurePosixPath(m.name).is_absolute() and '..' not in pathlib.PurePosixPath(m.name).parts for m in members)
        archive.extractall(dest,members=members)
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
report={'ApprovedSourceCommit':ref,'Purpose':'Disposable diagnostic clone only. Forced-GC/rooted checkpoints perturb liveness/timing. No product authority or budget acceptance.','Changes':changes}
(dest/'B6DiagnosticOverlay.json').write_text(json.dumps(report,indent=2)+'\n')
print('Created diagnostic-only copy',dest,'with',len(changes),'exact-anchor insertions')
