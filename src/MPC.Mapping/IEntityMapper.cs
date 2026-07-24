namespace MPC.Mapping;

public interface IEntityMapper<in TSource, TTarget>
    where TSource : class
    where TTarget : class
{
    MappingResult<TTarget> Map(
        TSource source,
        MappingContext context);
}