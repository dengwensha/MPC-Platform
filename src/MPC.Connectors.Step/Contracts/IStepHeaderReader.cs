using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Contracts;

public interface IStepHeaderReader
{
    CadConnectorResult<StepHeaderInfo> Read(
        string filePath);
}