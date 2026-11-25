"""Source-version regression: same semantics must never hide an obsolete profile copy."""
import hashlib,json,os,pathlib,subprocess,sys,tempfile
project=pathlib.Path(sys.argv[1]).resolve();builder=project/'tools/ci/B6HeapProbe/build_diagnostic_copy.py'
paths=['src/MPC.Application/Inventory/InventoryEvidence.cs','src/MPC.Application/Inventory/TargetProducer.cs','src/MPC.Application/Inventory/TargetBinding.cs']
def sha(data):return hashlib.sha256(data.decode('utf-8-sig').replace('\r\n','\n').encode()).hexdigest().upper()
with tempfile.TemporaryDirectory(prefix='b6-source-guard-') as tmp:
    root=pathlib.Path(tmp);repo=root/'repo';repo.mkdir()
    def git(*args):return subprocess.check_output(['git',*args],cwd=repo,stderr=subprocess.STDOUT)
    git('init');git('config','user.name','B6 guard test');git('config','user.email','b6-test@example.invalid');git('config','core.autocrlf','false')
    for name in paths:
        f=repo/name;f.parent.mkdir(parents=True,exist_ok=True)
        f.write_bytes((project/name).read_bytes().decode('utf-8-sig').replace('\r\n','\n').encode())
    def commit(message):git('add','.');git('commit','-m',message);return git('rev-parse','HEAD').decode().strip()
    first=commit('initial source')
    def invoke(label,success):
        dest=root/label;r=subprocess.run([sys.executable,str(builder),str(repo),str(dest)],capture_output=True,text=True,
            env={**os.environ,'B6_REVIEW_HEAD':git('rev-parse','HEAD').decode().strip()})
        if success:
            assert r.returncode==0,(r.stdout,r.stderr)
            return json.loads((dest/'B6DiagnosticOverlay.json').read_text(encoding='utf-8'))
        assert r.returncode!=0 and not dest.exists(),(label,r.stdout,r.stderr)
    a=invoke('clean',True);assert a['SourceCommit']==a['ReviewHeadCommit']==first and not a['ApprovalInferred']
    f=repo/paths[1];original=f.read_bytes();f.write_bytes(original+b'\n// same semantics, uncommitted source change\n')
    invoke('dirty',False)
    git('add',paths[1]);invoke('staged',False)
    second=commit('committed semantically neutral future source')
    b=invoke('current',True)
    assert b['SourceCommit']==b['ReviewHeadCommit']==second and second!=first
    h=b['SourceGuard']['CurrentThreeFileNormalizedSha256']
    assert h[paths[1]]==sha(f.read_bytes()) and h[paths[1]]!=a['SourceGuard']['CurrentThreeFileNormalizedSha256'][paths[1]]
    assert next(c for c in b['Changes'] if c['Path']==paths[1])['BeforeNormalizedSha256']==h[paths[1]]
    new=repo/'src/UntrackedCompileInput.cs';new.write_text('// new compile input',encoding='utf-8');invoke('untracked',False);new.unlink()
    other=repo/'src/OtherCompileInput.cs';other.write_text('// tracked other source',encoding='utf-8');commit('other source')
    other.write_text('// changed other source',encoding='utf-8');invoke('other-dirty',False)
print('PASS source guard: clean/current accepted; dirty/staged/untracked/other-dirty rejected; same-semantics commit changes profile source')
