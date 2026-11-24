"""Comparison of actual capture/gate, independent unchanged R-U and historical draft.
No new evidence or evaluator permission is issued by this audit script.
"""
import argparse,collections,hashlib,json,pathlib
p=argparse.ArgumentParser();p.add_argument('target');p.add_argument('draft');p.add_argument('output');p.add_argument('--count',type=int,required=True);a=p.parse_args()
target=[json.loads(f.read_text(encoding='utf-8')) for f in sorted(pathlib.Path(a.target).glob('*.json'))]
d=pathlib.Path(a.draft)
if d.is_dir():
    draft={r['InputName']:r for f in d.glob('*.json') if (r:=json.loads(f.read_text(encoding='utf-8'))) and 'InputName' in r}
else:
    draft={r['Control']+'.step':{'InputSha256':r['InputSha256'],'DraftOutcome':r['Observed']} for r in json.loads(d.read_text(encoding='utf-8'))}
assert len(target)==a.count,(len(target),a.count)
assert len({r['InputName'] for r in target})==len(target)
assert len({r['ApplicationAssemblySha256'] for r in target})==1
assert len({r['ReaderAssemblySha256'] for r in target})==1
rows=[]
for r in target:
    name=r['InputName'];baseline=draft.get(name)
    extra=name=='C13-ConeTypedProduct.step'
    assert baseline is not None or extra,name
    assert r['InputUnchanged'] and r['InputSha256']==r['CapturedSha256']==r['IndependentCapturedSha256']
    if baseline:
        assert r['InputSha256']==baseline['InputSha256'],name
        if 'RecordCount' in baseline:assert r['RecordCount']==baseline['RecordCount'],name
    positive=r['TargetStatus']=='TargetBoundOnly';ru=r['IndependentPrecheck']
    if ru=='Rejected':
        assert not positive and r['GuardCalled'] and r['Compatibility']=='Blocked',name
        assert any(x['Code']=='CurrentImportIncompatible' for x in r['Reasons']),name
    elif ru=='Passed':assert r['GuardCalled'] and r['Compatibility']=='Observed',name
    else:assert not positive and r['Compatibility']=='Unavailable',name
    for key in ['PhysicalOccurrenceProofIssued','AngleAbsenceProofIssued','GeometryValidityProofIssued','ReadyIssued']:assert r[key] is False
    explanation=None
    if baseline and baseline['DraftOutcome']=='DraftCandidateOnly' and not positive:
        assert name in ['C11-ConeRadian.step','RU-QuarterCone.step'],('Unapproved candidate loss',name,r['Reasons'])
        assert any(x['Code']=='ReferenceRoleMismatch' and x['SourceId']=='#1009' and '#1011' in x['RelatedIds'] for x in r['Reasons'])
        explanation='Historical analytic cone PRODUCT.frame_of_reference points to APPLICATION_CONTEXT #1011 instead of PRODUCT_CONTEXT; full typed product tail blocks. Geometry unchanged. Original control retained; independent C13 changes only product metadata.'
    if positive and baseline:assert baseline['DraftOutcome']=='DraftCandidateOnly',('Unexpected relaxation',name)
    if extra:assert positive
    rows.append({'InputName':name,'InputSha256':r['InputSha256'],'RecordCount':r['RecordCount'],
        'Draft':baseline['DraftOutcome'] if baseline else 'Additional positive control; not historical set',
        'IndependentImportUnitPrecheck':ru,'IndependentPrecheckReason':r['IndependentPrecheckReason'],
        'GuardCalled':r['GuardCalled'],'Binding':r['Binding'],'Compatibility':r['Compatibility'],'Target':r['TargetStatus'],
        'ReasonCounts':dict(sorted(collections.Counter(x['Code'] for x in r['Reasons']).items())),
        'CandidateLossExplanation':explanation,'FullReasonSnapshotSha256':hashlib.sha256((pathlib.Path(a.target)/(name+'.json')).read_bytes()).hexdigest().upper()})
report={'Scope':'Target binding only; no physical occurrence/angle absence/geometry validity/Ready',
    'Count':len(rows),'OutcomeCounts':dict(collections.Counter(r['Target'] for r in rows)),
    'EveryIndependentPrecheckRejectionBlocks':True,'NoUnexplainedHistoricalCandidateLoss':True,'Rows':rows}
pathlib.Path(a.output).parent.mkdir(parents=True,exist_ok=True)
pathlib.Path(a.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS target comparison',len(rows),report['OutcomeCounts'])
