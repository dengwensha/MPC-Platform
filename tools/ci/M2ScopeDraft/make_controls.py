import pathlib,sys
repo=pathlib.Path(sys.argv[1]);out=pathlib.Path(sys.argv[2]);out.mkdir(parents=True,exist_ok=True)
s=(repo/'tests/MPC.Tests/TestData/Step/Golden/T001_PRISM.step').read_text(encoding='utf-8',errors='replace').replace('\r\n','\n')
def add(text,records):
 i=text.rindex('ENDSEC;');return text[:i]+records+'\n'+text[i:]
bridge="#990001=SHAPE_REPRESENTATION('',(#175),#89);#990002=SHAPE_REPRESENTATION_RELATIONSHIP('',$,#990001,#47);"
values={
'C01-Styled.step':s,
'C02-SecondGeometry.step':add(s,"#990001=ADVANCED_BREP_SHAPE_REPRESENTATION('',(#63),#61);"),
'C03-Bridge.step':add(s.replace('#27 = SHAPE_DEFINITION_REPRESENTATION ( #4, #47 ) ;','#27=SHAPE_DEFINITION_REPRESENTATION(#4,#990001);'),bridge),
'C04-DualBinding.step':add(s,bridge+'#990003=SHAPE_DEFINITION_REPRESENTATION(#4,#990001);'),
'C06-UnreferencedPoint.step':add(s,"#990001=CARTESIAN_POINT('',(0.,0.,0.));"),
'C07-Unknown.step':add(s,'#990001=UNREVIEWED_RECORD();'),
'C08-ComplexVoid.step':add(s,"#990001=(MANIFOLD_SOLID_BREP('',#79) BREP_WITH_VOIDS((#79)));"),
'C09-SurfaceBody.step':add(s,"#990001=SHELL_BASED_SURFACE_MODEL('',(#79));"),
'C10-TwoMappedItems.step':add(s,"#990001=REPRESENTATION_MAP(#175,#47);#990002=MAPPED_ITEM('',#990001,#990004);#990003=MAPPED_ITEM('',#990001,#990004);#990004=CARTESIAN_TRANSFORMATION_OPERATOR_3D('',$,$,#7,1.,$);#990005=SHAPE_REPRESENTATION('',(#990002,#990003),#61);")}
values['C05-BridgeDifferentUnits.step']=values['C03-Bridge.step'].replace('#16 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( .MILLI., .METRE. ) );','#16 =( LENGTH_UNIT ( ) NAMED_UNIT ( * ) SI_UNIT ( $, .METRE. ) );')
cone=(repo/'tests/MPC.Application.Tests/TestData/Step/RU-QuarterCone.step').read_text(encoding='utf-8').replace('\r\n','\n');values['C11-ConeRadian.step']=cone
values['C12-ConeDegree.step']=cone.replace('1.,0.7853981633974483)','1.,45.)').replace("#1004=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) SI_UNIT($,.RADIAN.));","#1004=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) CONVERSION_BASED_UNIT('DEGREE',#1012));#1012=PLANE_ANGLE_MEASURE_WITH_UNIT(PLANE_ANGLE_MEASURE(0.017453292519943295),#1013);#1013=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) SI_UNIT($,.RADIAN.));")
for name,text in values.items():(out/name).write_text(text,encoding='utf-8')
print('Wrote',len(values),'public-data structural controls; negative void/mapping cases are not full AP conformance claims')
