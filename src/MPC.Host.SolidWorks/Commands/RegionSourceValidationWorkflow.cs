using System;
using System.IO;
using System.Security.Cryptography;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Host.SolidWorks.Commands;

internal interface IRegionSourceValidationInteraction
{
    string? ChooseStepFile();
    string? ChooseSourceFace(string filePath, string fingerprint);
    void ShowResult(string message, bool isError);
}

// UI orchestration only. Never maps a SOLIDWORKS selected face to a STEP entity.
internal sealed class RegionSourceValidationWorkflow
{
    private readonly IRegionSourceValidationInteraction _interaction;
    private readonly Func<RegionSourceValidationRequest, RegionSourceValidationHostCommandResult> _execute;

    internal RegionSourceValidationWorkflow(IRegionSourceValidationInteraction interaction,
        Func<RegionSourceValidationRequest, RegionSourceValidationHostCommandResult> execute)
    {
        _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    internal void Run()
    {
        try
        {
            string? path = _interaction.ChooseStepFile();
            if (string.IsNullOrWhiteSpace(path)) return;
            string fingerprint;
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                fingerprint = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");

            // Capture before the prompt: later file changes must not silently repin identity.
            string? sourceFace = _interaction.ChooseSourceFace(path, fingerprint);
            if (sourceFace is null) return;
            var request = new RegionSourceValidationRequest(
                ContractVersion.RegionSourceValidation, Guid.NewGuid().ToString("D"),
                WorkerOperationKind.ValidateRegionSource, path, sourceFace, fingerprint,
                RegionSourceValidationRequirement.SourceLinkOnly);
            if (!RegionSourceValidationRequestValidator.Validate(request).IsValid)
            {
                _interaction.ShowResult("STEP yüz kimliği '#8' biçiminde pozitif bir kaynak kimliği olmalıdır.", true);
                return;
            }

            var result = _execute(request);
            if (result.Status == RegionSourceHostStatus.SourceLinkMatched &&
                result.WorkerResult?.Outcome == RegionSourceValidationOutcome.SourceLinkMatched &&
                RegionSourceValidationResultValidator.Validate(result.WorkerResult, request.RequestId).IsValid)
            {
                _interaction.ShowResult(
                    "Dosya/yüz bağlantısı eşleşti: " + sourceFace + Environment.NewLine +
                    "Bu sonuç malzeme yönü, takım veya fiziksel uygunluk onayı değildir.", false);
            }
            else
            {
                string reason = result.Status == RegionSourceHostStatus.SourceLinkMatched ?
                    "Yanıt doğrulanamadı" : result.WorkerResult is null ? result.Status.ToString() :
                    result.WorkerResult.Outcome.ToString();
                _interaction.ShowResult(
                    "Kaynak yüz kontrolü onay üretmedi: " + reason + Environment.NewLine +
                    "Dosyanın değişmediğini ve STEP yüz kimliğini kontrol edin.", true);
            }
        }
        catch (Exception exception) when (exception is IOException ||
            exception is UnauthorizedAccessException || exception is CryptographicException ||
            exception is ArgumentException || exception is NotSupportedException)
        {
            _interaction.ShowResult(
                "Kaynak dosyası kontrol edilemedi: " + exception.GetType().Name, true);
        }
    }
}
