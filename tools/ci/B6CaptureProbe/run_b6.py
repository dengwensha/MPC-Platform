"""Sequential per-file/process B6 measurement; never changes production code.
Each child validates same-byte identity, semantic repeat and released object lifetime.
"""
import argparse,collections,hashlib,json,pathlib,statistics,subprocess,sys

p=argparse.ArgumentParser();p.add_argument('--dotnet',default='dotnet');p.add_argument('--probe',required=True)
p.add_argument('--baseline',required=True);p.add_argument('--current',required=True);p.add_argument('--inputs',required=True)
p.add_argument('--require-target-equality',action='store_true');p.add_argument('--output',required=True);p.add_argument('--trials',type=int,default=2);a=p.parse_args()
out=pathlib.Path(a.output);out.mkdir(parents=True,exist_ok=True)
assert not list(out.rglob('*.json')),'Output must be empty'
inputs=sorted(p for p in pathlib.Path(a.inputs).iterdir() if p.suffix.lower() in ['.step','.stp'])
assert inputs and a.trials>=2
mode_order=[('baseline','capture',a.baseline),('current','capture',a.current),('current','target',a.current)]
reports=[]
def launch(path,digest,label,mode,app,trial):
    dest=out/f'trial-{trial}'/label/mode/(path.name+'.json')
    command=[a.dotnet,a.probe,app,str(path),digest,mode,str(dest),label]
    done=subprocess.run(command,capture_output=True,text=True)
    if done.returncode:raise RuntimeError((command,done.returncode,done.stdout,done.stderr))
    r=json.loads(dest.read_text(encoding='utf-8'));assert r['StageAvailable'] and r['AllReleasedObjectsCollected']
    assert len(r['Measurements'])==4 and r['ReleasedObjects']==6 and r['WarmupCount']==2
    assert r['NoIndependentPrecheckOrJsonInsideTimedRegion']
    if mode=='capture':
        c=r['SemanticSummary']['Common']
        assert c['ObservationAvailable'] and c['HasInventory'] and c['RecordCount']>0,(path.name,c)
    reports.append((trial,r))
    print(f'B6 trial {trial} {label}/{mode}: {path.name}',flush=True)
for trial in range(1,a.trials+1):
    for path in inputs:
        digest=hashlib.sha256(path.read_bytes()).hexdigest().upper()
        order=mode_order if trial%2 else list(reversed(mode_order))
        for label,mode,app in order:launch(path,digest,label,mode,app,trial)
        assert hashlib.sha256(path.read_bytes()).hexdigest().upper()==digest

# Separate expected-unavailable controls exercise actual CaptureFromFile failure
# stages. These observations are never counted as successful corpus measurements.
controls=out/'failure-inputs';controls.mkdir()
missing=controls/'Missing.step';bad=controls/'ParserRejected.step'
bad.write_text("ISO-10303-21;HEADER;FILE_DESCRIPTION(('control'),'1');FILE_NAME('a','',(''),(''),'','','');FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));ENDSEC;DATA;#1=COMPLEX_ENTITY();ENDSEC;END-ISO-10303-21;",encoding='utf-8')
failures=[]
for label,app in [('baseline',a.baseline),('current',a.current)]:
    for path,digest,stage in [(missing,'MISSING','SourceCapture'),(bad,hashlib.sha256(bad.read_bytes()).hexdigest().upper(),'InventoryParse')]:
        dest=out/'failure-controls'/label/(path.name+'.json')
        done=subprocess.run([a.dotnet,a.probe,app,str(path),digest,'capture',str(dest),label],capture_output=True,text=True)
        if done.returncode:raise RuntimeError((done.returncode,done.stdout,done.stderr))
        r=json.loads(dest.read_text(encoding='utf-8'));c=r['SemanticSummary']['Common']
        assert c['ObservationAvailable'] is False and c['RecordCount'] is None and c['FailureStage']==stage
        assert r['AllReleasedObjectsCollected'];failures.append({'Label':label,'Case':path.name,'ExpectedStage':stage,'ObservedStage':c['FailureStage']})
bad.unlink();controls.rmdir()

rows=[]
def mode(r):return 'capture' if r['Unit']=='ProductionCaptureFromFile' else 'target'
for path in inputs:
    group=[r for trial,r in reports if r['InputName']==path.name]
    base=[r for r in group if r['Label']=='baseline'];current=[r for r in group if r['Label']=='current' and mode(r)=='capture'];target=[r for r in group if mode(r)=='target']
    assert len(base)==len(current)==len(target)==a.trials
    assert all(r['SemanticSummary']['Common']==base[0]['SemanticSummary']['Common'] for r in base+current)
    assert all(r['SemanticSummary']['TargetData']==current[0]['SemanticSummary']['TargetData'] for r in current+target)
    if a.require_target_equality:
        assert all(r['SemanticSummary']==current[0]['SemanticSummary'] for r in base+current),('Target semantics changed',path.name)
    assert all(r['InputSha256']==base[0]['InputSha256'] for r in group)
    assert all(r['ApplicationAssemblySha256']==base[0]['ApplicationAssemblySha256'] for r in base)
    assert len({r['ApplicationAssemblySha256'] for r in current+target})==1
    assert len({r['SummarySha256'] for r in base})==len({r['SummarySha256'] for r in current})==len({r['SummarySha256'] for r in target})==1
    result={'InputName':path.name,'InputSha256':base[0]['InputSha256'],'InputBytes':base[0]['InputBytes'],
        'RecordCount':base[0]['SemanticSummary']['Common']['RecordCount'],'TargetStatus':current[0]['SemanticSummary']['TargetStatus'],
        'BaselineCurrentCommonEvidenceEqual':True,'IsolatedAndOwnedTargetDataEqual':True,'TrialSemanticsEqual':True}
    for key,values in [('BaselineCapture',base),('CurrentCapture',current),('IsolatedTarget',target)]:
        first=[r['Measurements'][0] for r in values];warm=[s for r in values for s in r['Measurements'][1:]]
        result[key]={'ProcessFirstMilliseconds':[round(s['ElapsedMilliseconds'],4) for s in first],
            'WarmMedianMilliseconds':round(statistics.median(s['ElapsedMilliseconds'] for s in warm),4),
            'WarmMaxMilliseconds':round(max(s['ElapsedMilliseconds'] for s in warm),4),
            'WarmMedianAllocatedBytes':int(statistics.median(s['AllocatedBytes'] for s in warm)),
            'MaxRetainedHeapDeltaBytes':max(s['RetainedHeapDeltaBytes'] for s in first+warm),
            'FirstInvocationMaxProcessPeakBytes':max(s['LifetimePeakAfterCallBytes'] for s in first),
            'FirstInvocationMaxWorkingSetBytes':max(s['WorkingSetAfterBytes'] for s in first),
            'AllSamplesMaxLifetimeProcessPeakBytes':max(s['LifetimePeakAfterCallBytes'] for s in first+warm)}
    result['DerivedWarmCaptureMedianDeltaMilliseconds']=round(result['CurrentCapture']['WarmMedianMilliseconds']-result['BaselineCapture']['WarmMedianMilliseconds'],4)
    rows.append(result)
summary={'SchemaRevision':1,'FileCount':len(inputs),'Trials':a.trials,'FreshProcesses':len(reports),
    'MeasuredProductionCalls':len(reports)*4,'BeforeAfterTargetSemanticsRequired':a.require_target_equality,'UntimedWarmupProductionCalls':len(reports)*2,
    'TargetStatusCounts':dict(collections.Counter(r['TargetStatus'] for r in rows)),
    'AllSemanticsAndInputIdentitiesVerified':True,'AllReleasedObjectsCollected':True,'FailureControls':failures,
    'MemoryCaveat':'Process peak includes setup/warmups/earlier summaries; isolated-target memory includes prepared inventory. Never subtract peaks as attributable production memory.',
    'TimingCaveat':'First invocation after assembly/delegate binding and input preflight; no cold OS-cache/startup claim. Explicit full GC before calls. Derived capture delta is empirical difference, not exact stage attribution. No CI timing thresholds.',
    'Rows':rows}
(out/'Summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print('PASS B6',len(inputs),'files;',len(reports),'fresh production-stage processes;',len(failures),'unavailable controls',flush=True)
