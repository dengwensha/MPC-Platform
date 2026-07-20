using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public interface IStepSemanticEntityHandler
{
    string SupportedEntityType { get; }

    StepSemanticEntity Read(
        StepDataEntityRecord record,
        IReadOnlyList<StepParameter> parameters);
}