"""Senior44 bounds on isolated B6 measurements. Never accepts product/Worker budget.
First-minus-warm allocation is an initialization proxy, not exclusive attribution.
"""
import argparse,json,pathlib,statistics
p=argparse.ArgumentParser();p.add_argument('capture');p.add_argument('output');p.add_argument('--platform',required=True);p.add_argument('--expected-count',type=int,default=20);a=p.parse_args()
root=pathlib.Path(a.capture);summary=json.loads((root/'Summary.json').read_text(encoding='utf-8-sig'))
assert summary['Trials']==2 and summary['BeforeAfterTargetSemanticsRequired'] and summary['AllSemanticsAndInputIdentitiesVerified'] and summary['AllReleasedObjectsCollected']
inputs={x['InputName'] for x in summary['Rows']};assert len(inputs)==summary['FileCount']
median=statistics.median;MiB=1048576;rows=[]
def read(label,trial,name):
 d=json.loads((root/f'trial-{trial}'/label/'capture'/(name+'.json')).read_text(encoding='utf-8-sig'))
 assert d['StageAvailable'] and d['AllReleasedObjectsCollected'] and d['Unit']=='ProductionCaptureFromFile'
 assert len(d['Measurements'])==4 and len(d['Measurements'][1:])==3
 return d
for name in sorted(inputs):
 before=[read('baseline',i,name) for i in [1,2]];after=[read('current',i,name) for i in [1,2]]
 assert len({d['InputSha256'] for d in before+after})==1
 assert all(d['SemanticSummary']==before[0]['SemanticSummary'] for d in before+after)
 assert len({d['ApplicationAssemblySha256'] for d in before})==len({d['ApplicationAssemblySha256'] for d in after})==1
 bt=[median(s['ElapsedMilliseconds'] for s in d['Measurements'][1:]) for d in before]
 ct=[median(s['ElapsedMilliseconds'] for s in d['Measurements'][1:]) for d in after]
 noise=max(abs(bt[1]-bt[0]),abs(ct[1]-ct[0]));delta=median(ct)-median(bt)
 no_change=abs(delta)<noise
 r03=name=='P01-R03-CurvedSurfaces.STEP';time_limit=.20 if r03 else .25 if median(bt)>1000 else None
 trials=[]
 for i,(b,c) in enumerate(zip(before,after),1):
  bm,cm=b['Measurements'],c['Measurements'];bp=max(s['LifetimePeakAfterCallBytes'] for s in bm);cp=max(s['LifetimePeakAfterCallBytes'] for s in cm)
  assert bp>0 and cp>0
  ba=median(s['AllocatedBytes'] for s in bm[1:]);ca=median(s['AllocatedBytes'] for s in cm[1:]);assert ba>0
  bstart=max(0,bm[0]['AllocatedBytes']-ba);cstart=max(0,cm[0]['AllocatedBytes']-ca)
  retained=max(abs(cm[j]['RetainedHeapDeltaBytes']-bm[j]['RetainedHeapDeltaBytes']) for j in range(4))
  trials.append({'Trial':i,'BeforePeakBytes':bp,'CurrentPeakBytes':cp,'PeakReductionPercent':100*(1-cp/bp),'R03PeakPass':not r03 or cp<=.85*bp,
   'BeforeWarmAllocatedBytes':ba,'CurrentWarmAllocatedBytes':ca,'WarmAllocationIncreasePercent':100*(ca/ba-1),'WarmAllocationPass':ca<=1.30*ba,
   'BeforeStartupAllocationProxyBytes':bstart,'CurrentStartupAllocationProxyBytes':cstart,'IncrementalStartupAllocationProxyBytes':cstart-bstart,'StartupProxyPass':cstart<=MiB,
   'MaxAbsoluteRetainedHeapDeltaDifferenceBytes':retained,'RetainedHeapPass':retained<=MiB,
   'BeforeFirstMilliseconds':bm[0]['ElapsedMilliseconds'],'CurrentFirstMilliseconds':cm[0]['ElapsedMilliseconds'],'FirstTimePass':cm[0]['ElapsedMilliseconds']<=10000})
 rows.append({'InputName':name,'InputSha256':before[0]['InputSha256'],'BeforeWarmTrialMediansMilliseconds':bt,'CurrentWarmTrialMediansMilliseconds':ct,
  'WarmMedianDeltaMilliseconds':delta,'InterTrialNoiseMilliseconds':noise,'NoMeasuredTimeChangeWithinNoise':no_change,
  'WarmTimeLimitIncreasePercent':None if time_limit is None else time_limit*100,
  'WarmTimePass':time_limit is None or no_change or median(ct)<=median(bt)*(1+time_limit),'Trials':trials})
r03=next((r for r in rows if r['InputName']=='P01-R03-CurvedSurfaces.STEP'),None);assert r03 is not None
peak=all(t['R03PeakPass'] for t in r03['Trials']);time=all(r['WarmTimePass'] and all(t['FirstTimePass'] for t in r['Trials']) for r in rows)
allocation=all(t['WarmAllocationPass'] for r in rows for t in r['Trials']);retained=all(t['RetainedHeapPass'] for r in rows for t in r['Trials']);startup=all(t['StartupProxyPass'] for r in rows for t in r['Trials'])
scope=len(rows)==a.expected_count==20
status='ScopeIncomplete' if not scope else 'RejectForPeak' if not peak else 'OwnerTradeoffRequired' if not(time and allocation) else 'FurtherReviewRequired' if not(retained and startup) else 'MeetsMeasuredBoundsOnly'
report={'SchemaRevision':1,'Policy':'Senior44 A throwaway bounds','Platform':a.platform,'ExpectedFiles':a.expected_count,'ObservedFiles':len(rows),'Trials':2,
 'SemanticIdentityAndLifetimesPass':True,'R03PeakBothTrialsPass':peak,'WarmAndFirstTimePass':time,'WarmAllocationPass':allocation,'RetainedHeapPass':retained,'StartupProxyPass':startup,
 'Disposition':status,'ProductAccepted':False,'RealWorkerBudgetAccepted':False,'Windows20Inferred':False,
 'Definitions':{'Peak':'max lifetime process peak across first/warm measured calls, per fresh process/trial; includes setup/warmups',
 'TimeNoise':'max absolute inter-trial warm-median difference of baseline/current. Smaller time delta is no measured change.',
 'Startup':'max(0, first-call allocated bytes minus same-process warm median). Proxy includes first-call effects; not exclusive static-table attribution. Both absolute and incremental proxies reported.',
 'Tradeoff':'If R03 peak passes but time/allocation exceeds bounds, present owner decision; never silently reject/accept.',
 'Scope':'Linux and Windows are separate. Public Windows subset is not full private Windows20 or real Worker/owner16GB acceptance.'},'Rows':rows}
out=pathlib.Path(a.output);assert not out.exists();out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(status,'platform',a.platform,'files',len(rows),'R03 peak',peak,'time',time,'allocation',allocation,'retained',retained,'startup proxy',startup)
