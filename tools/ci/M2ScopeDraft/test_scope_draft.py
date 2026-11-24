import collections,json,pathlib,sys
from scope_draft import classify,decode_ids
manifest=json.loads(pathlib.Path(sys.argv[1]).read_text(encoding='utf-8'));controls=pathlib.Path(sys.argv[2]);results=[]
expected={'C01-Styled':'DraftCandidateOnly','C02-SecondGeometry':'ScopeNotSupported','C03-Bridge':'DraftCandidateOnly','C04-DualBinding':'ScopeNotSupported','C05-BridgeDifferentUnits':'ScopeNotSupported','C06-UnreferencedPoint':'ScopeNotSupported','C07-Unknown':'ClassificationUnavailable','C08-ComplexVoid':'ScopeNotSupported','C09-SurfaceBody':'ScopeNotSupported','C10-TwoMappedItems':'ScopeNotSupported','C11-ConeRadian':'DraftCandidateOnly','C12-ConeDegree':'ScopeNotSupported'}
for p in sorted(controls.glob('*.json')):
 data=json.loads(p.read_text(encoding='utf-8'));result=classify(data,manifest);name=data['InputName'].removesuffix('.step')
 assert result['DraftOutcome']==expected[name],(name,result['DraftOutcome'],result['Reasons'])
 assert not result['ApplicationEvidenceIssued'] and not result['PhysicalOccurrenceProofIssued'] and not result['AngleAbsenceProofIssued']
 decoded=[id for group in result['LedgerGroups'] for id in decode_ids(group['EntityIdsDeltaVarintBase64'])]
 assert collections.Counter(decoded)==collections.Counter(r['Id'] for r in data['Records'])
 assert sum(g['Count'] for g in result['LedgerGroups'])==result['RecordCount']
 results.append({'Control':name,'InputSha256':result['InputSha256'],'Expected':expected[name],'Observed':result['DraftOutcome'],'LedgerAccountsForEveryRecord':True,'Reasons':result['Reasons']})
assert len(results)==12
if len(sys.argv)>3:pathlib.Path(sys.argv[3]).write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
print('PASS: 12/12 structural discovery controls; not production eligibility acceptance')
