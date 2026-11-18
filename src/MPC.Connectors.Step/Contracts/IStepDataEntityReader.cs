using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Contracts;

public interface IStepDataEntityReader
{
    /// <summary>Reads a STEP DATA inventory and preserves supported reader diagnostics.</summary>
    /// <exception cref="System.FormatException">
    /// A simple declaration uses COMPLEX_ENTITY, the internal complex-record discriminator.
    /// The read is rejected before constructing an inconsistent record; this is not
    /// a guarantee that every malformed STEP expression throws this exception.
    /// </exception>
    CadConnectorResult<StepDataInventory> Read(
        string filePath);
}