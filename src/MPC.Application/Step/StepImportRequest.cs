namespace MPC.Application.Step;

public sealed class StepImportRequest
{
    public string InputFilePath { get; }

    public StepImportRequest(
        string inputFilePath)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            throw new ArgumentException(
                "STEP input file path cannot be null, empty, or whitespace.",
                nameof(inputFilePath));
        }

        InputFilePath =
            inputFilePath.Trim();
    }
}
