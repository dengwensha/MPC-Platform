using MPC.Connectors.Step.Models;

namespace MPC.Connectors.Step.Semantics;

public sealed class SwStepSignedToroidalCompatibilityPolicy
{
    public const string SupportedPreprocessorVersion =
        "SwSTEP 2.0";

    public const string SupportedOriginatingSystem =
        "SolidWorks 2019";

    private readonly bool _isSupportedExporter;

    public SwStepSignedToroidalCompatibilityPolicy(
        StepHeaderInfo header)
    {
        ArgumentNullException.ThrowIfNull(
            header);

        _isSupportedExporter =
            string.Equals(
                header.PreprocessorVersion,
                SupportedPreprocessorVersion,
                StringComparison.Ordinal) &&
            string.Equals(
                header.OriginatingSystem,
                SupportedOriginatingSystem,
                StringComparison.Ordinal);
    }

    public bool TryInterpret(
        double sourceMajorRadius,
        double sourceMinorRadius,
        out double standardMajorRadius)
    {
        standardMajorRadius =
            default;

        if (!_isSupportedExporter)
        {
            return false;
        }

        if (!double.IsFinite(
                sourceMajorRadius) ||
            sourceMajorRadius >= 0.0)
        {
            return false;
        }

        if (!double.IsFinite(
                sourceMinorRadius) ||
            sourceMinorRadius <= 0.0)
        {
            return false;
        }

        double positiveMajorRadius =
            -sourceMajorRadius;

        if (!double.IsFinite(
                positiveMajorRadius) ||
            positiveMajorRadius <= 0.0 ||
            positiveMajorRadius >= sourceMinorRadius)
        {
            return false;
        }

        standardMajorRadius =
            positiveMajorRadius;

        return true;
    }
}
