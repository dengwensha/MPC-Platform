using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

/// <summary>Opts a handler into component-aware reading for selected complex records.</summary>
public interface IStepComplexSemanticEntityHandler : IStepSemanticEntityHandler
{
    bool CanRead(StepDataEntityRecord record);
}
