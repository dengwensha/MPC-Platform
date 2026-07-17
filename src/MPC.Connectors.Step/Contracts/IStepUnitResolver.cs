using MPC.Connectors.Results;
using MPC.Connectors.Snapshots.Units;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Contracts;

public interface IStepUnitResolver
{
    CadConnectorResult<CadUnitInfo> Resolve(
        StepDataInventory inventory);
}