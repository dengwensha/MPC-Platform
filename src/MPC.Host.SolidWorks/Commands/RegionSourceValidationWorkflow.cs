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
            if (path is null || string.IsNullOrWhiteSpace(path)) return;
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
                    "Seçilen dosyanın istem öncesi ve kontrol anındaki içerik özeti eşleşti." + Environment.NewLine +
                    "Desteklenen düzlemsel STEP yüzü oluşturuldu: " + sourceFace + Environment.NewLine +
                    "Önceki bir içe aktarma kaydıyla süreklilik doğrulanmadı." + Environment.NewLine +
                    "Bu sonuç malzeme yönü, takım veya fiziksel uygunluk onayı değildir.", false);
            }
            else
            {
                _interaction.ShowResult(
                    "Kaynak yüz kontrolü onay üretmedi." + Environment.NewLine +
                    FailureReason(result, request.RequestId), true);
            }
        }
        catch (Exception exception) when (exception is IOException ||
            exception is UnauthorizedAccessException || exception is CryptographicException ||
            exception is ArgumentException || exception is NotSupportedException)
        {
            _interaction.ShowResult(
                "Kaynak dosyası okunamadı veya özeti hesaplanamadı. Dosyanın erişilebilir olduğunu ve okuma iznini kontrol edin.", true);
        }
    }
    private static string FailureReason(RegionSourceValidationHostCommandResult result, string? requestId)
    {
        // Report only a validated blocked outcome. Transport/result failures must
        // not be disguised by an attached, untrusted Worker outcome.
        if (result.Status == RegionSourceHostStatus.Blocked)
        {
            if (!RegionSourceValidationResultValidator.Validate(result.WorkerResult, requestId).IsValid)
                return "Yanıt doğrulanamadı.";
            return result.WorkerResult!.Outcome switch
            {
                RegionSourceValidationOutcome.MissingRegion =>
                    "İstenen desteklenen bölge oluşturulamadı. STEP dosyasını, desteklenen katı/düzlemsel geometriyi ve kaynak yüz kimliğini kontrol edin. Bu sonuç hangi koşulun başarısız olduğunu ayırmaz.",
                RegionSourceValidationOutcome.DocumentMismatch =>
                    "Dosyanın içerik özeti istem öncesi alınan özetle eşleşmedi. Dosyayı yeniden seçerek kontrolü başlatın.",
                RegionSourceValidationOutcome.FaceMismatch =>
                    "Kaynak yüz kimliği beklenen kimlikle eşleşmedi.",
                RegionSourceValidationOutcome.InvalidSourceIdentity =>
                    "Kaynak dosya veya yüz kimliği doğrulanamadı.",
                RegionSourceValidationOutcome.MaterialSideUnverified =>
                    "Malzeme yönü için gerekli bağımsız doğrulama bulunmuyor.",
                RegionSourceValidationOutcome.RequestRejected =>
                    "Kontrol isteği kabul edilmedi. Dosya ve kaynak yüz bilgilerini yeniden girin.",
                _ => "Yanıt doğrulanamadı."
            };
        }
        return result.Status switch
        {
            RegionSourceHostStatus.InvalidRequest => "Kontrol isteği geçersiz.",
            RegionSourceHostStatus.LaunchFailed => "Kontrol işlemi başlatılamadı. Eklentiyle birlikte gelen kontrol programının bulunduğunu kontrol edin.",
            RegionSourceHostStatus.TimedOut => "Kontrol işlemi zaman aşımına uğradı.",
            RegionSourceHostStatus.ObservationFailed => "Kontrol işleminin tamamlandığı doğrulanamadı.",
            RegionSourceHostStatus.ResultReadFailed => "Yanıt doğrulanamadı.",
            RegionSourceHostStatus.WorkerFailed => "STEP içe aktarma işlemi tamamlanamadı. Dosya erişimini ve desteklenen girdi biçimini kontrol edin.",
            RegionSourceHostStatus.TransportFailed => "Kontrol isteği veya sonucu aktarılırken hata oluştu.",
            _ => "Yanıt doğrulanamadı."
        };
    }
}
