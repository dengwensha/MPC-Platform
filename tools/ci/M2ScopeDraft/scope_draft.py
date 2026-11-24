"""Untrusted T3 discovery only. Cannot issue Application evidence or readiness."""
import base64,collections,hashlib,json,math,pathlib,sys
from build_manifest import shape,signature

def refs(p):
 if p['Kind']=='ref':return [p['Id']]
 return [r for v in p.get('Items',[]) for r in refs(v)]
def ranges(ids):
 nums=sorted(set(int(x[1:]) for x in ids));chunks=[]
 for n in nums:
  if chunks and chunks[-1][1]+1==n:chunks[-1][1]=n
  else:chunks.append([n,n])
 return ','.join(str(a) if a==b else f'{a}-{b}' for a,b in chunks)
def encode_ids(ids):
 previous=0;output=bytearray()
 for n in sorted(int(x[1:]) for x in ids):
  delta=n-previous;previous=n
  while delta>=128:output.append((delta&127)|128);delta>>=7
  output.append(delta)
 return base64.b64encode(output).decode('ascii')
def decode_ids(value):
 previous=0;delta=shift=0;out=[]
 for b in base64.b64decode(value):
  delta|=(b&127)<<shift
  if b&128:shift+=7
  else:previous+=delta;out.append('#'+str(previous));delta=shift=0
 if shift:raise ValueError('Truncated varint')
 return out

def classify(data,manifest):
 rules={r['signature']:r for r in manifest['rules']};groups=collections.defaultdict(list)
 records=data['Records'];index={};duplicates=set()
 for r in records:
  if r['Id'] in index:duplicates.add(r['Id'])
  index[r['Id']]=r
 problems=collections.defaultdict(set);roles={};components={}
 scope=[];unknown=[]
 for r in records:
  id=r['Id'];sig=signature(r);rule=rules.get(sig);parts={c['Name']:c['Parameters'] for c in r['Components']};components[id]=parts
  role=rule['role'] if rule else 'Unclassified';roles[id]=role
  if id in duplicates:problems[id].add('DuplicateIdentity')
  if r['ParseFailure'] or r['Status']!='Complete':problems[id].add('PartialOrUnparsedRecord')
  if not rule or role=='Unclassified':problems[id].add('UnclassifiedSignatureOrRole')
  if rule:
   for c in r['Components']:
    spec=rule['components'][c['Name']]['fields']
    for p,f in zip(c['Parameters'],spec):
     if shape(p) not in f['shapes']:problems[id].add('UnreviewedParameterShape')
     if f['name'].startswith('UNREVIEWED'):problems[id].add('UnreviewedFieldRole')
     def finite(v):
      if v['Kind'] in ('number','integer') and not math.isfinite(float(v['Value'])):problems[id].add('NonfiniteNumeric')
      for child in v.get('Items',[]):finite(child)
     finite(p)
     expected=manifest['geometry_reference_fields'].get(c['Name'],{}).get(f['name'])
     if expected:
      for target in refs(p):
       if target in index and not any(n in components.get(target,{c['Name']:c['Parameters'] for c in index[target]['Components']}) for n in expected):problems[id].add('GeometryReferenceRoleMismatch')
  for target in r['References']:
   if target not in index or target in duplicates:problems[id].add('UnresolvedReference')
  if role in ('UnsupportedPath','UnsupportedGeometry') or any(n in parts for n in ('BREP_WITH_VOIDS','FACETED_BREP','MAPPED_ITEM','REPRESENTATION_MAP','SHELL_BASED_SURFACE_MODEL','FACE_BASED_SURFACE_MODEL','GEOMETRIC_CURVE_SET','GEOMETRIC_SET')) or r['Type'] in ('BREP_WITH_VOIDS','FACETED_BREP','MAPPED_ITEM','REPRESENTATION_MAP','SHELL_BASED_SURFACE_MODEL','FACE_BASED_SURFACE_MODEL','GEOMETRIC_CURVE_SET','GEOMETRIC_SET'):
   problems[id].add('RecognizedUnsupportedScope')
 def param(id,name):return components.get(id,{}).get(name,[])
 def fieldref(p):return p['Id'] if p and p['Kind']=='ref' else None
 def closure(seeds):
  seen=set();todo=list(seeds)
  while todo:
   id=todo.pop()
   if id in seen or id not in index:continue
   seen.add(id);todo.extend(index[id]['References'])
  return seen
 roots=[r['Id'] for r in records if roles[r['Id']]=='TargetRoot']
 shape_ids=[r['Id'] for r in records if roles[r['Id']]=='ShapeRepresentation']
 root=None;representation=None;context=None;bridge=None;binding=None
 if len(roots)!=1:scope.append('RootCountNotOne')
 else:
  root=roots[0];members=[]
  for id in shape_ids:
   p=param(id,index[id]['Type'])
   if len(p)==3 and p[1]['Kind']=='aggregate':
    count=refs(p[1]).count(root)
    if count:members.append(id)
    if count>1:scope.append('DuplicateRootMembership')
  if len(members)!=1:scope.append('RootRepresentationNotUnique')
  else:representation=members[0];context=fieldref(param(representation,index[representation]['Type'])[2])
 def units(ctx):
  p=components.get(ctx,{})
  try:
   if p['GEOMETRIC_REPRESENTATION_CONTEXT'][0].get('Value')!='3':return False
   if len(p['REPRESENTATION_CONTEXT'])!=2:return False
   ids=refs(p['GLOBAL_UNIT_ASSIGNED_CONTEXT'][0]);length=angle=0
   for id in ids:
    u=components.get(id,{});dims=set(u)&{'LENGTH_UNIT','PLANE_ANGLE_UNIT','SOLID_ANGLE_UNIT'}
    if len(dims)!=1 or set(u)!=(dims|{'NAMED_UNIT','SI_UNIT'}):return False
    if len(u['NAMED_UNIT'])!=1 or u['NAMED_UNIT'][0]['Kind']!='omitted':return False
    d=next(iter(dims));v=u['SI_UNIT']
    if d=='LENGTH_UNIT':
     length+=1
     if [x.get('Value') for x in v]!=['MILLI','METRE']:return False
    elif d=='PLANE_ANGLE_UNIT':
     angle+=1
     if len(v)!=2 or v[0]['Kind']!='null' or v[1].get('Value')!='RADIAN':return False
    else:
     if len(v)!=2 or v[0]['Kind']!='null' or v[1].get('Value')!='STERADIAN':return False
   return length==1 and angle==1
  except (KeyError,IndexError,TypeError):return False
 main=representation
 if representation:
  if not units(context):scope.append('TargetUnitsNotMmRadian3D')
  bridges=[r['Id'] for r in records if roles[r['Id']]=='NontransformingBridge' and representation in r['References']]
  if len(bridges)>1:scope.append('MultipleBridges')
  elif bridges:
   bridge=bridges[0];p=param(bridge,'SHAPE_REPRESENTATION_RELATIONSHIP')
   endpoints=[fieldref(x) for x in p[2:4]]
   if len(endpoints)!=2 or endpoints[0]==endpoints[1] or representation not in endpoints:scope.append('InvalidBridge')
   else:
    main=next(x for x in endpoints if x!=representation)
    if main not in shape_ids or index[main]['Type']!='SHAPE_REPRESENTATION':scope.append('UnsupportedMainRepresentation')
    else:
     mp=param(main,'SHAPE_REPRESENTATION')
     if not units(fieldref(mp[2])):scope.append('BridgeUnitsNotMmRadian3D')
     if any(index[t]['Type']!='AXIS2_PLACEMENT_3D' for t in refs(mp[1]) if t in index):scope.append('GeometryInBridgeMain')
     if sum(roles[r['Id']]=='NontransformingBridge' and main in r['References'] for r in records)!=1:scope.append('BridgeChainOrBranch')
  definitions=[]
  for r in records:
   if roles[r['Id']]=='ShapeBinding':
    p=param(r['Id'],'SHAPE_DEFINITION_REPRESENTATION')
    if len(p)==2 and fieldref(p[1]) in {representation,main}:definitions.append(r['Id'])
  if len(definitions)!=1:scope.append('MissingOrDualSDR')
  else:
   binding=definitions[0];p=param(binding,'SHAPE_DEFINITION_REPRESENTATION')
   if fieldref(p[1])!=main:scope.append('SDRBridgeEndpoint')
   ps=fieldref(p[0]);product_shape=param(ps,'PRODUCT_DEFINITION_SHAPE')
   if len(product_shape)!=3 or fieldref(product_shape[2]) is None:scope.append('ProductShapeBindingInvalid')
   else:
    pd=fieldref(product_shape[2])
    if len(param(pd,'PRODUCT_DEFINITION'))!=4:scope.append('ProductDefinitionBindingInvalid')
    for r in records:
     if pd in r['References'] and r['Id']!=ps:scope.append('AdditionalProductPath');problems[r['Id']].add('UnreviewedProductPath')
   if sum(r['Type']=='SHAPE_DEFINITION_REPRESENTATION' and ps in r['References'] for r in records)!=1:scope.append('DuplicateProductShapeBinding')
  allowed={representation,main,bridge,binding}
  for r in records:
   if root in r['References'] and r['Id'] not in allowed and r['Type'] not in ('STYLED_ITEM','PRESENTATION_LAYER_ASSIGNMENT'):scope.append('UnreviewedRootIncomingPath');problems[r['Id']].add('UnreviewedRootIncomingPath')
  for r in records:
   if r['Id'] not in allowed and any(x in r['References'] for x in {representation,main}):scope.append('UnreviewedRepresentationPath');problems[r['Id']].add('UnreviewedRepresentationPath')
 root_geometry=closure(roots)
 placements=[]
 for id in shape_ids:
  p=param(id,index[id]['Type'])
  if len(p)==3:
   for item in refs(p[1]):
    if item in index and index[item]['Type']=='AXIS2_PLACEMENT_3D':placements.append(item)
    elif item not in roots:scope.append('OtherGeometryRepresentationItem');problems[item].add('OtherGeometryRepresentationItem')
 placement_geometry=closure(placements)
 admitted_geometry=root_geometry|placement_geometry
 for r in records:
  id=r['Id']
  if roles[id]=='GeometryDependency' and id not in admitted_geometry:problems[id].add('UnreferencedGeometry');scope.append('UnreferencedGeometry')
  if 'RecognizedUnsupportedScope' in problems[id]:scope.append('RecognizedUnsupportedScope')
  if any(x!='RecognizedUnsupportedScope' and x not in ('UnreferencedGeometry','OtherGeometryRepresentationItem','UnreviewedRootIncomingPath','UnreviewedRepresentationPath','UnreviewedProductPath') for x in problems[id]):unknown.append('ClassificationUnavailable')
  groups[(signature(r),roles[id],tuple(sorted(problems[id])))].append(id)
 inventory_bad=any(d['Severity'] not in ('Info','Warning') for d in data['Diagnostics']) or data['Status'] not in ('Succeeded','SucceededWithDiagnostics')
 reasons=sorted(set(scope+unknown+(['InventoryNotClear'] if inventory_bad else [])))
 outcome='ScopeNotSupported' if scope else 'ClassificationUnavailable' if unknown or inventory_bad else 'DraftCandidateOnly'
 ledger=[{'Signature':s,'Role':role,'Issues':list(issues),'Count':len(ids),'EntityIdsDeltaVarintBase64':encode_ids(ids),'IssueEntityIdRanges':ranges(ids) if issues else None} for (s,role,issues),ids in sorted(groups.items())]
 return {'InputName':data['InputName'],'InputSha256':data['InputSha256'],'InputUnchanged':data['InputUnchanged'],'ReaderAssemblySha256':data['ReaderAssemblySha256'],'InventoryStatus':data['Status'],'InventoryDiagnostics':data['Diagnostics'],'RecordCount':len(records),'DraftOutcome':outcome,'Reasons':reasons,'RootIds':roots,'RepresentationId':representation,'ContextId':context,'BridgeId':bridge,'BindingId':binding,'PhysicalOccurrenceProofIssued':False,'KnownAngularRecordIds':[r['Id'] for r in records if any(c['Name']=='CONICAL_SURFACE' for c in r['Components'])],'AngleAbsenceProofIssued':False,'ApplicationEvidenceIssued':False,'LedgerGroups':ledger,'LedgerSha256':hashlib.sha256(json.dumps(ledger,sort_keys=True,separators=(',',':')).encode()).hexdigest().upper()}

if __name__=='__main__':
 manifest=json.loads(pathlib.Path(sys.argv[1]).read_text());source=pathlib.Path(sys.argv[2]);out=pathlib.Path(sys.argv[3]);out.mkdir(parents=True,exist_ok=True)
 for p in sorted(source.glob('*.json')):
  result=classify(json.loads(p.read_text()),manifest);(out/p.name).write_text(json.dumps(result,indent=2)+'\n');print(result['InputName'],result['DraftOutcome'],','.join(result['Reasons']))
