using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Contracts;

public interface IStepDataEntityReader
{
    CadConnectorResult<StepDataInventory> Read(
        string filePath);
}