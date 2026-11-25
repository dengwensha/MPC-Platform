"""Five Senior43 downstream negative fixtures, golden-derived; no private CAD."""
import pathlib,re,sys
repo=pathlib.Path(sys.argv[1]);out=pathlib.Path(sys.argv[2]);assert not out.exists();out.mkdir(parents=True)
s=(repo/'tests/MPC.Tests/TestData/Step/Golden/T001_PRISM.step').read_bytes().decode('utf-8-sig',errors='replace').replace('\r\n','\n')
def entity(id,body):return re.sub(re.escape(id)+r'\s*=.*?;',id+'='+body+';',s,flags=re.S)
def add(body):i=s.rfind('ENDSEC;');return s[:i]+body+s[i:]
cases={'B01-MissingPlacement':entity('#175',"CARTESIAN_POINT('',(0.,0.,0.))"),
 'B02-UnknownStyled':entity('#48',"(STYLED_ITEM('',(#135),#63) UNKNOWN())"),
 'B03-UnknownLayer':entity('#133',"(PRESENTATION_LAYER_ASSIGNMENT('','',(#63)) UNKNOWN())"),
 'B04-SecondShape':add("#990001=ADVANCED_BREP_SHAPE_REPRESENTATION('',(#63),#61);"),
 'B05-PartialIncoming':add('#990001=( #63 );')}
for name,text in cases.items():(out/(name+'.step')).write_bytes(text.encode('utf-8'))
print('Wrote5 golden-derived downstream negatives')
