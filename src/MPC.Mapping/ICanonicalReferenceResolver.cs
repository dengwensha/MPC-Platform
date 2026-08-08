namespace MPC.Mapping;

public interface ICanonicalReferenceResolver
{
    MappingResult<T> Resolve<T>(
        string referenceId,
        MappingContext context)
        where T : class;
}