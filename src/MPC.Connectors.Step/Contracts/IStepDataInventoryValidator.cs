using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Contracts;

public interface IStepDataInventoryValidator
{
    IReadOnlyList<CadDiagnostic> Validate(
        IReadOnlyList<StepDataEntityRecord> entities);
}