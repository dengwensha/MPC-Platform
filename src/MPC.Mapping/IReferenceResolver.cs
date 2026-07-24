namespace MPC.Mapping;

public interface IReferenceResolver
{
    ReferenceResolutionResult<T> Resolve<T>(
        string referenceId)
        where T : class;
}