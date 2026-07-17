using MPC.Validation.Manifests;

namespace MPC.Validation.Contracts;

public interface IModelManifestSource
{
    IReadOnlyList<ModelManifest> ReadModels();
}