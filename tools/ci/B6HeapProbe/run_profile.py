"""Sequential CLR sampling and controlled phase profiles; never issues product evidence."""
import argparse,hashlib,json,pathlib,subprocess
p=argparse.ArgumentParser();p.add_argument('--dotnet',default='dotnet');p.add_argument('--probe',required=True)
p.add_argument('--canonical',required=True);p.add_argument('--diagnostic',required=True);p.add_argument('--inputs',required=True)
p.add_argument('--manifest',required=True);p.add_argument('--output',required=True);p.add_argument('--trials',type=int,default=2)
a=p.parse_args();out=pathlib.Path(a.output);assert not out.exists(),'Fresh output directory required';out.mkdir(parents=True)
expected={r['InputName']:r for r in json.loads(pathlib.Path(a.manifest).read_text(encoding='utf-8'))['Rows']}
inputs=sorted(f for f in pathlib.Path(a.inputs).iterdir() if f.suffix.lower() in ['.step','.stp']);assert inputs and a.trials>=2
reports=[]
for trial in range(1,a.trials+1):
 for file in inputs:
  digest=hashlib.sha256(file.read_bytes()).hexdigest().upper();assert digest==expected[file.name]['InputSha256']
  modes=['inventory','canonical','diagnostic'] if trial%2 else ['diagnostic','canonical','inventory']
  for mode in modes:
   output=out/f'trial-{trial}'/mode/(file.name+'.json');app=a.diagnostic if mode=='diagnostic' else a.canonical
   r=subprocess.run([a.dotnet,a.probe,app,str(file),digest,mode,str(output),f'trial-{trial}'],capture_output=True,text=True)
   if r.returncode:raise RuntimeError((file.name,mode,r.stdout,r.stderr))
   value=json.loads(output.read_text(encoding='utf-8'));assert value['InputSha256']==digest and value['ReleasedObjectCollected'] and value['TotalSampledEvents']>0 and value['DroppedEvents']==0
   assert value['ProfileIsEligibilityEvidence'] is False and value['ConsumerInvoked'] is False
   assert value['SemanticSummary']['RecordCount']==expected[file.name]['RecordCount']
   reports.append(value);print('PROFILE',trial,file.name,mode,'events',value['TotalSampledEvents'],flush=True)
  assert hashlib.sha256(file.read_bytes()).hexdigest().upper()==digest
rows=[]
for file in inputs:
 group=[r for r in reports if r['InputName']==file.name];canonical=[r for r in group if r['Mode']=='canonical'];diagnostic=[r for r in group if r['Mode']=='diagnostic'];inventory=[r for r in group if r['Mode']=='inventory']
 assert len(canonical)==len(diagnostic)==len(inventory)==a.trials
 assert len({r['SemanticSha256'] for r in canonical+diagnostic})==1,('Diagnostic changed projected semantics',file.name)
 assert len({r['SemanticSha256'] for r in inventory})==1
 assert all(r['SemanticSummary']['Status']==expected[file.name]['TargetStatus'] for r in canonical+diagnostic)
 assert len({r['ApplicationAssemblySha256'] for r in canonical+inventory})==len({r['ApplicationAssemblySha256'] for r in diagnostic})==1
 assert all(r['ReaderAssemblySha256']==canonical[0]['ReaderAssemblySha256'] for r in canonical+inventory)
 labels=[c['Label'] for c in diagnostic[0]['Checkpoints']];assert all([c['Label'] for c in r['Checkpoints']]==labels for r in diagnostic)
 assert 'Inventory.Complete' in labels and 'Target.Rows' in labels and 'Precheck.Before' in labels and 'Precheck.After' in labels
 rows.append({'InputName':file.name,'InputSha256':canonical[0]['InputSha256'],'RecordCount':expected[file.name]['RecordCount'],'Status':canonical[0]['SemanticSummary']['Status'],
              'CanonicalDiagnosticSemanticsEqual':True,'RepeatedSemanticsEqual':True,'SemanticSha256':canonical[0]['SemanticSha256'],
              'InventoryApproximateRootedHeapDeltas':[r['ApproximateRootedHeapDeltaBytes'] for r in inventory],
              'CanonicalApproximateRootedHeapDeltas':[r['ApproximateRootedHeapDeltaBytes'] for r in canonical],
              'DiagnosticRootedHeapByCheckpoint':{label:[next(c['RootedHeapAfterGC'] for c in r['Checkpoints'] if c['Label']==label) for r in diagnostic] for label in labels}})
# Negative controls must fail without creating a false zero-memory observation.
controls=out/'controls';controls.mkdir();bad=controls/'ParserRejected.step';bad.write_text("ISO-10303-21;HEADER;FILE_DESCRIPTION(('x'),'1');FILE_NAME('x','',(''),(''),'','','');FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));ENDSEC;DATA;#1=COMPLEX_ENTITY();ENDSEC;END-ISO-10303-21;",encoding='utf-8')
failures=[]
for mode,app in [('canonical',a.canonical),('inventory',a.canonical),('diagnostic',a.diagnostic)]:
 f=controls/(mode+'.json');r=subprocess.run([a.dotnet,a.probe,app,str(bad),hashlib.sha256(bad.read_bytes()).hexdigest().upper(),mode,str(f),'negative'],capture_output=True,text=True)
 assert r.returncode!=0 and not f.exists(),('False available observation',mode);failures.append(mode)
bad.unlink();controls.rmdir()
summary={'SchemaRevision':1,'Inputs':len(inputs),'Trials':a.trials,'FreshProcesses':len(reports),'AllocationObserverSelfTestRequired':True,
         'AllReleasedObjectsCollected':True,'AllCanonicalDiagnosticSemanticsEqual':True,'NegativeUnavailableControlsRejected':failures,'Rows':rows,
         'Scope':'Runtime sampled allocations and explicit-root forced-GC phase observations. Not exclusive graph sizes, natural production peaks, latency or real Worker budget acceptance.'}
(out/'Summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print('PASS profile',len(inputs),'files',len(reports),'fresh processes; negative controls',len(failures))
