"""Compare the full target audit projection, excluding only Application/reader DLL provenance hashes.
Not an evidence issuer. Preserves reason order, multiplicity, severity and indices.
Inner cause text is intentionally omitted by the existing audit adapter for privacy.
"""
import argparse, hashlib, json, pathlib
p=argparse.ArgumentParser()
p.add_argument('before');p.add_argument('after');p.add_argument('output');p.add_argument('--count',type=int,required=True)
a=p.parse_args()
def read(directory):
    files=sorted(pathlib.Path(directory).glob('*.json'))
    assert len(files)==a.count,(directory,len(files),a.count)
    rows={f.name:json.loads(f.read_text(encoding='utf-8')) for f in files}
    assert len({r['ApplicationAssemblySha256'] for r in rows.values()})==1
    return rows
before,after=read(a.before),read(a.after)
assert before.keys()==after.keys(),'Input set changed'
rows=[]
for name,b in before.items():
    c=after[name]
    for r in (b,c):
        assert r['InputUnchanged'] and r['InputSha256']==r['CapturedSha256']==r['IndependentCapturedSha256']
    bp={k:v for k,v in b.items() if k not in ('ApplicationAssemblySha256','ReaderAssemblySha256')}
    cp={k:v for k,v in c.items() if k not in ('ApplicationAssemblySha256','ReaderAssemblySha256')}
    assert bp==cp,('Target audit semantics changed',name,[k for k in bp if bp[k]!=cp[k]])
    digest=hashlib.sha256(json.dumps(bp,sort_keys=True,separators=(',',':')).encode()).hexdigest().upper()
    rows.append({'InputName':c['InputName'],'InputSha256':c['InputSha256'],'RecordCount':c['RecordCount'],
                 'Status':c['TargetStatus'],'ReasonCount':len(c['Reasons']),'EqualSemanticProjectionSha256':digest})
report={'Count':len(rows),'ExcludedProvenanceFields':['ApplicationAssemblySha256','ReaderAssemblySha256'],'AllProjectedFieldsExactlyEqual':True,
        'CauseTextBoundary':'Existing audit excludes inner cause text; no new omission introduced',
        'BeforeApplicationAssemblySha256':next(iter(before.values()))['ApplicationAssemblySha256'],
        'AfterApplicationAssemblySha256':next(iter(after.values()))['ApplicationAssemblySha256'],
        'BeforeReaderAssemblySha256':next(iter(before.values()))['ReaderAssemblySha256'],
        'AfterReaderAssemblySha256':next(iter(after.values()))['ReaderAssemblySha256'],'Rows':rows}
out=pathlib.Path(a.output);assert not out.exists(),'Stale comparison output';out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS exact target audit comparison',len(rows))
