"""Authoring helper: observed shapes are proposals, never auto-approved signatures."""
import json,pathlib,collections,sys
fields={
'APPLICATION_CONTEXT':'application','APPLICATION_PROTOCOL_DEFINITION':'status schema_name year application_context',
'PRODUCT':'id name description frame_of_reference','PRODUCT_CONTEXT':'name frame_of_reference discipline_type',
'PRODUCT_DEFINITION':'id description formation frame_of_reference','PRODUCT_DEFINITION_CONTEXT':'name frame_of_reference life_cycle_stage',
'PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE':'id description of_product make_or_buy',
'PRODUCT_DEFINITION_FORMATION':'id description of_product','PRODUCT_DEFINITION_SHAPE':'name description definition','PRODUCT_RELATED_PRODUCT_CATEGORY':'name description products',
'SHAPE_DEFINITION_REPRESENTATION':'definition used_representation','SHAPE_REPRESENTATION_RELATIONSHIP':'name description rep_1 rep_2',
'ADVANCED_BREP_SHAPE_REPRESENTATION':'name items context_of_items','SHAPE_REPRESENTATION':'name items context_of_items',
'MANIFOLD_SOLID_BREP':'name outer','CLOSED_SHELL':'name cfs_faces','ADVANCED_FACE':'name bounds face_geometry same_sense',
'FACE_BOUND':'name bound orientation','FACE_OUTER_BOUND':'name bound orientation','EDGE_LOOP':'name edge_list',
'ORIENTED_EDGE':'name edge_start edge_end edge_element orientation','EDGE_CURVE':'name edge_start edge_end edge_geometry same_sense',
'VERTEX_POINT':'name vertex_geometry','CARTESIAN_POINT':'name coordinates','DIRECTION':'name direction_ratios',
'VECTOR':'name orientation magnitude','AXIS2_PLACEMENT_3D':'name location axis ref_direction','LINE':'name pnt dir',
'CIRCLE':'name position radius','ELLIPSE':'name position semi_axis_1 semi_axis_2','PLANE':'name position',
'CYLINDRICAL_SURFACE':'name position radius','SPHERICAL_SURFACE':'name position radius','CONICAL_SURFACE':'name position radius semi_angle',
'TOROIDAL_SURFACE':'name position major_radius minor_radius',
'B_SPLINE_CURVE_WITH_KNOTS':'name degree control_points_list curve_form closed_curve self_intersect knot_multiplicities knots knot_spec',
'B_SPLINE_SURFACE_WITH_KNOTS':'name u_degree v_degree control_points_list surface_form u_closed v_closed self_intersect u_multiplicities v_multiplicities u_knots v_knots knot_spec',
'B_SPLINE_CURVE':'degree control_points_list curve_form closed_curve self_intersect','B_SPLINE_SURFACE':'u_degree v_degree control_points_list surface_form u_closed v_closed self_intersect',
'RATIONAL_B_SPLINE_CURVE':'weights_data','RATIONAL_B_SPLINE_SURFACE':'weights_data','REPRESENTATION_ITEM':'name',
'STYLED_ITEM':'name styles item','PRESENTATION_LAYER_ASSIGNMENT':'name description assigned_items',
'MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION':'name items context_of_items',
'PRESENTATION_STYLE_ASSIGNMENT':'styles','SURFACE_STYLE_USAGE':'side style','SURFACE_SIDE_STYLE':'name styles',
'SURFACE_STYLE_FILL_AREA':'fill_area','FILL_AREA_STYLE':'name fill_styles','FILL_AREA_STYLE_COLOUR':'name fill_colour','COLOUR_RGB':'name red green blue',
'GEOMETRIC_REPRESENTATION_CONTEXT':'coordinate_space_dimension','GLOBAL_UNIT_ASSIGNED_CONTEXT':'units',
'GLOBAL_UNCERTAINTY_ASSIGNED_CONTEXT':'uncertainty','REPRESENTATION_CONTEXT':'context_identifier context_type',
'NAMED_UNIT':'dimensions','SI_UNIT':'prefix name','UNCERTAINTY_MEASURE_WITH_UNIT':'value_component unit_component name description',
'REPRESENTATION_RELATIONSHIP':'name description rep_1 rep_2','REPRESENTATION_RELATIONSHIP_WITH_TRANSFORMATION':'transformation_operator',
'SHAPE_REPRESENTATION_RELATIONSHIP_COMPLEX':'','NEXT_ASSEMBLY_USAGE_OCCURRENCE':'id name description relating_product_definition related_product_definition reference_designator',
'ITEM_DEFINED_TRANSFORMATION':'name description transform_item_1 transform_item_2','CONTEXT_DEPENDENT_SHAPE_REPRESENTATION':'representation_relation represented_product_relation'}
geometry=set('CLOSED_SHELL ADVANCED_FACE FACE_BOUND FACE_OUTER_BOUND EDGE_LOOP ORIENTED_EDGE EDGE_CURVE VERTEX_POINT CARTESIAN_POINT DIRECTION VECTOR AXIS2_PLACEMENT_3D LINE CIRCLE PLANE CYLINDRICAL_SURFACE SPHERICAL_SURFACE CONICAL_SURFACE TOROIDAL_SURFACE B_SPLINE_CURVE_WITH_KNOTS B_SPLINE_SURFACE_WITH_KNOTS'.split())
presentation=set('STYLED_ITEM PRESENTATION_LAYER_ASSIGNMENT MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION PRESENTATION_STYLE_ASSIGNMENT SURFACE_STYLE_USAGE SURFACE_SIDE_STYLE SURFACE_STYLE_FILL_AREA FILL_AREA_STYLE FILL_AREA_STYLE_COLOUR COLOUR_RGB'.split())
metadata=set('APPLICATION_CONTEXT APPLICATION_PROTOCOL_DEFINITION PRODUCT PRODUCT_CONTEXT PRODUCT_DEFINITION PRODUCT_DEFINITION_CONTEXT PRODUCT_DEFINITION_FORMATION PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE PRODUCT_DEFINITION_SHAPE PRODUCT_RELATED_PRODUCT_CATEGORY'.split())
unsupported=set('NEXT_ASSEMBLY_USAGE_OCCURRENCE ITEM_DEFINED_TRANSFORMATION CONTEXT_DEPENDENT_SHAPE_REPRESENTATION'.split())
def shape(p):
 k=p['Kind']
 if k in ('number','integer'):return 'numeric'
 if k=='aggregate':return {'aggregate':sorted({json.dumps(shape(v),sort_keys=True) for v in p['Items']})}
 if k=='typed':return {'typed':p['Name'],'items':[shape(v) for v in p['Items']]}
 return k

def signature(r):return '|'.join(sorted(c['Name']+':'+str(len(c['Parameters'])) for c in r['Components']))
if __name__=='__main__':
 stats={}
 for p in sorted(pathlib.Path(sys.argv[1]).glob('*.json')):
  for r in json.loads(p.read_text())['Records']:
   sig=signature(r)
   if not sig:continue
   if sig not in stats:
    names={c['Name'] for c in r['Components']};t=r['Type']
    role='Unclassified'
    if t in geometry:role='GeometryDependency'
    if t in presentation:role='Presentation'
    if t in metadata:role='ProductMetadata'
    if t=='MANIFOLD_SOLID_BREP':role='TargetRoot'
    if t in ('SHAPE_REPRESENTATION','ADVANCED_BREP_SHAPE_REPRESENTATION'):role='ShapeRepresentation'
    if t=='SHAPE_DEFINITION_REPRESENTATION':role='ShapeBinding'
    if t=='SHAPE_REPRESENTATION_RELATIONSHIP':role='NontransformingBridge'
    if t in unsupported or 'REPRESENTATION_RELATIONSHIP_WITH_TRANSFORMATION' in names:role='UnsupportedPath'
    if t=='ELLIPSE':role='UnsupportedGeometry'
    if t=='UNCERTAINTY_MEASURE_WITH_UNIT':role='UnitMetadata'
    if 'GEOMETRIC_REPRESENTATION_CONTEXT' in names:role='Context'
    if 'SI_UNIT' in names:role='UnitMetadata'
    if 'RATIONAL_B_SPLINE_CURVE' in names or 'RATIONAL_B_SPLINE_SURFACE' in names:role='GeometryDependency'
    stats[sig]={'signature':sig,'role':role,'components':{},'review_status':'PROPOSED_NOT_RATIFIED','angle_fields':([{'component':'CONICAL_SURFACE','index':3,'unit':'plane_angle'}] if t=='CONICAL_SURFACE' else [])}
   for c in r['Components']:
    spec=stats[sig]['components'].setdefault(c['Name'],{'fields':[]})
    if not spec['fields']:
     labels=({'B_SPLINE_CURVE_WITH_KNOTS':'knot_multiplicities knots knot_spec'} if len(c['Parameters'])==3 else {'B_SPLINE_SURFACE_WITH_KNOTS':'u_multiplicities v_multiplicities u_knots v_knots knot_spec'} if len(c['Parameters'])==5 else {}).get(c['Name'],fields.get(c['Name'],'')).split()
     spec['fields']=[{'name':labels[i] if i<len(labels) else f'UNREVIEWED_{i}','shapes':[]} for i in range(len(c['Parameters']))]
    for f,v in zip(spec['fields'],c['Parameters']):
     s=shape(v)
     if s not in f['shapes']:f['shapes'].append(s)
 # Explicit optional text already admitted by reviewed R-U v1, not inferred from corpus absence.
 for entry in stats.values():
  for name,component in entry['components'].items():
   for field in component['fields']:
    if name in ('SHAPE_REPRESENTATION_RELATIONSHIP','PRODUCT_DEFINITION_SHAPE','PRODUCT_DEFINITION','PRODUCT_DEFINITION_FORMATION','PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE') and field['name']=='description' and 'null' not in field['shapes']:field['shapes'].append('null')
 manifest={'schema_revision':1,'manifest_revision':'1-DRAFT','status':'PROPOSED_NOT_RATIFIED','default_role':'Unclassified','unknown_blocks':True,'same_capture_proof':False,'evaluator_coverage':False,'production_target_issuer':False,'inherited_RU_rules':'docs/reviews/Tooling/RU-Import-Context-Rules-v1-Proposal.md + Senior sections 35/36; import code remains unchanged','T2':'One simple nontransforming bridge; both unique 3D contexts mm+radian; no transforms/assembly; direct+bridge dual SDR rejects','rules':[stats[k] for k in sorted(stats)],'geometry_reference_fields':{'MANIFOLD_SOLID_BREP':{'outer':['CLOSED_SHELL']},'CLOSED_SHELL':{'cfs_faces':['ADVANCED_FACE']},'ADVANCED_FACE':{'bounds':['FACE_OUTER_BOUND','FACE_BOUND'],'face_geometry':['PLANE','CYLINDRICAL_SURFACE','SPHERICAL_SURFACE','CONICAL_SURFACE','TOROIDAL_SURFACE','B_SPLINE_SURFACE_WITH_KNOTS','B_SPLINE_SURFACE']},'FACE_OUTER_BOUND':{'bound':['EDGE_LOOP']},'FACE_BOUND':{'bound':['EDGE_LOOP']},'EDGE_LOOP':{'edge_list':['ORIENTED_EDGE']},'ORIENTED_EDGE':{'edge_element':['EDGE_CURVE']},'EDGE_CURVE':{'edge_start':['VERTEX_POINT'],'edge_end':['VERTEX_POINT'],'edge_geometry':['LINE','CIRCLE','ELLIPSE','B_SPLINE_CURVE_WITH_KNOTS','B_SPLINE_CURVE']},'VERTEX_POINT':{'vertex_geometry':['CARTESIAN_POINT']},'AXIS2_PLACEMENT_3D':{'location':['CARTESIAN_POINT'],'axis':['DIRECTION'],'ref_direction':['DIRECTION']},'VECTOR':{'orientation':['DIRECTION']},'LINE':{'pnt':['CARTESIAN_POINT'],'dir':['VECTOR']},'CIRCLE':{'position':['AXIS2_PLACEMENT_3D']},'PLANE':{'position':['AXIS2_PLACEMENT_3D']},'CYLINDRICAL_SURFACE':{'position':['AXIS2_PLACEMENT_3D']},'CONICAL_SURFACE':{'position':['AXIS2_PLACEMENT_3D']},'SPHERICAL_SURFACE':{'position':['AXIS2_PLACEMENT_3D']},'TOROIDAL_SURFACE':{'position':['AXIS2_PLACEMENT_3D']},'B_SPLINE_CURVE_WITH_KNOTS':{'control_points_list':['CARTESIAN_POINT']},'B_SPLINE_CURVE':{'control_points_list':['CARTESIAN_POINT']},'B_SPLINE_SURFACE_WITH_KNOTS':{'control_points_list':['CARTESIAN_POINT']},'B_SPLINE_SURFACE':{'control_points_list':['CARTESIAN_POINT']}}}
 pathlib.Path(sys.argv[2]).write_text(json.dumps(manifest,indent=2)+'\n')
 print('Proposed signatures:',len(stats),'explicit unclassified:',sum(r['role']=='Unclassified' for r in stats.values()))
