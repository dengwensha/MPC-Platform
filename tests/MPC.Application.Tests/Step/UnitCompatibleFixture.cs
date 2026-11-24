namespace MPC.Application.Tests.Step;

// Preserve B-spline mapper-boundary tests by supplying the newly required unit/product binding.
internal static class UnitCompatibleFixture
{
    internal static void AddBinding(string path)
    {
        string text=File.ReadAllText(path).Replace("\r\n","\n");
        const string records="""
            #700=ADVANCED_BREP_SHAPE_REPRESENTATION('',(#110),#701);
            #701=(GEOMETRIC_REPRESENTATION_CONTEXT(3) GLOBAL_UNIT_ASSIGNED_CONTEXT((#702,#703)) REPRESENTATION_CONTEXT('',''));
            #702=(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT(.MILLI.,.METRE.));
            #703=(PLANE_ANGLE_UNIT() NAMED_UNIT(*) SI_UNIT($,.RADIAN.));
            #704=SHAPE_DEFINITION_REPRESENTATION(#705,#700);
            #705=PRODUCT_DEFINITION_SHAPE('','',#706);
            #706=PRODUCT_DEFINITION('','',#707,#709);
            #707=PRODUCT_DEFINITION_FORMATION('','',#708);
            #708=PRODUCT('','','',());
            #709=PRODUCT_DEFINITION_CONTEXT('',#710,'');
            #710=APPLICATION_CONTEXT('');
            """;
        File.WriteAllText(path,text.Replace("ENDSEC;\nEND-ISO-10303-21;",records+"\nENDSEC;\nEND-ISO-10303-21;"));
    }
}
