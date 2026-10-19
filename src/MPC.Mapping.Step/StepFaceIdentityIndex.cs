using MPC.Connectors.Step.Semantics;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

// Records source identity at the exact mapping boundary, never by face-list position.
public sealed class StepFaceIdentityIndex
{
    private readonly Dictionary<string, Face> _faces = new(StringComparer.Ordinal);
    private bool _sealed;

    public bool TryGetFace(string sourceFaceId, out Face? face)
    {
        face = null;
        return _sealed && !string.IsNullOrWhiteSpace(sourceFaceId)
            && _faces.TryGetValue(sourceFaceId, out face);
    }

    internal void Record(string? sourceFaceId, Face face)
    {
        if (_sealed) throw new InvalidOperationException("Identity index is sealed.");
        if (string.IsNullOrWhiteSpace(sourceFaceId))
            throw new InvalidOperationException("A mapped face must have a STEP source identity.");
        if (!_faces.TryAdd(sourceFaceId, face))
            throw new InvalidOperationException("Duplicate STEP face source identity.");
    }

    internal void Reset()
    {
        _faces.Clear();
        _sealed = false;
    }

    internal void Seal() => _sealed = true;
}

internal sealed class StepFaceIdentityTrackingMapper
    : IEntityMapper<StepAdvancedFaceEntity, Face>
{
    private readonly IEntityMapper<StepAdvancedFaceEntity, Face> _inner;
    private readonly StepFaceIdentityIndex _index;

    public StepFaceIdentityTrackingMapper(
        IEntityMapper<StepAdvancedFaceEntity, Face> inner,
        StepFaceIdentityIndex index)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _index = index ?? throw new ArgumentNullException(nameof(index));
    }

    public MappingResult<Face> Map(StepAdvancedFaceEntity source, MappingContext context)
    {
        MappingResult<Face> result = _inner.Map(source, context);
        if (result.IsSuccessful && result.Value is not null)
            _index.Record(source.SourceEntityId, result.Value);
        return result;
    }
}
