using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MPC.Contracts;
using MPC.Host.SolidWorks.Commands;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Commands;

public sealed class RegionSourceValidationWorkflowTests
{
    private static string Hash(string path)
    {
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
    }

    private static RegionSourceValidationHostCommandResult Match(RegionSourceValidationRequest request) =>
        new RegionSourceValidationHostCommandResult(RegionSourceHostStatus.SourceLinkMatched,
            workerResult: new RegionSourceValidationResult("1.1", request.RequestId,
                WorkerOperationKind.ValidateRegionSource, RegionSourceValidationOutcome.SourceLinkMatched));

    [Fact]
    public void CancelFile_DoesNotReadOrExecute()
    {
        var interaction = new FakeInteraction();
        int calls = 0;
        new RegionSourceValidationWorkflow(interaction, r => { calls++; return Match(r); }).Run();
        Assert.Equal(0, calls);
        Assert.Equal(0, interaction.FaceCalls);
        Assert.Empty(interaction.Messages);
    }

    [Fact]
    public void CancelFace_DoesNotExecuteOrDisplaySuccess()
    {
        using (var file = new TempFile())
        {
            var interaction = new FakeInteraction { FilePath = file.Path, Face = null };
            int calls = 0;
            new RegionSourceValidationWorkflow(interaction, r => { calls++; return Match(r); }).Run();
            Assert.Equal(0, calls);
            Assert.Empty(interaction.Messages);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("#0")]
    [InlineData("#08")]
    [InlineData("#-1")]
    [InlineData("8")]
    [InlineData("#8 ")]
    [InlineData("#８")]
    public void InvalidFace_NeverLaunchesWorker(string face)
    {
        using (var file = new TempFile())
        {
            var interaction = new FakeInteraction { FilePath = file.Path, Face = face };
            int calls = 0;
            new RegionSourceValidationWorkflow(interaction, r => { calls++; return Match(r); }).Run();
            Assert.Equal(0, calls);
            Assert.True(Assert.Single(interaction.Errors));
        }
    }

    [Fact]
    public void CapturedRequest_BindsFileHashFaceAndNarrowRequirement()
    {
        using (var file = new TempFile())
        {
            string expected = Hash(file.Path);
            var interaction = new FakeInteraction { FilePath = file.Path };
            RegionSourceValidationRequest? captured = null;
            new RegionSourceValidationWorkflow(interaction, r => { captured = r; return Match(r); }).Run();
            Assert.NotNull(captured);
            Assert.Equal("1.1", captured!.ContractVersion);
            Assert.True(Guid.TryParse(captured.RequestId, out _));
            Assert.Equal(file.Path, captured.InputArtifactPath);
            Assert.Equal("#8", captured.SourceFaceId);
            Assert.Equal(expected, captured.ExpectedDocumentSha256);
            Assert.Equal(expected, interaction.PromptHash);
            Assert.Equal(RegionSourceValidationRequirement.SourceLinkOnly, captured.Requirement);
            Assert.False(Assert.Single(interaction.Errors));
            Assert.Contains("fiziksel uygunluk onayı değildir", Assert.Single(interaction.Messages));
        }
    }

    [Fact]
    public void MissingFile_ShowsErrorWithoutPromptingForFace()
    {
        var interaction = new FakeInteraction { FilePath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step") };
        int calls = 0;
        new RegionSourceValidationWorkflow(interaction, r => { calls++; return Match(r); }).Run();
        Assert.Equal(0, calls);
        Assert.Equal(0, interaction.FaceCalls);
        Assert.True(Assert.Single(interaction.Errors));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("wrong-id")]
    [InlineData("wrong-code")]
    public void InconsistentMatchedResult_NeverDisplaysSuccess(string scenario)
    {
        using (var file = new TempFile())
        {
            var interaction = new FakeInteraction { FilePath = file.Path };
            new RegionSourceValidationWorkflow(interaction, r =>
                new RegionSourceValidationHostCommandResult(RegionSourceHostStatus.SourceLinkMatched,
                    workerResult: scenario == "missing" ? null :
                        new RegionSourceValidationResult("1.1",
                            scenario == "wrong-id" ? Guid.NewGuid().ToString("D") : r.RequestId,
                            WorkerOperationKind.ValidateRegionSource,
                            scenario == "wrong-code" ? RegionSourceValidationOutcome.MaterialSideUnverified :
                                RegionSourceValidationOutcome.SourceLinkMatched))).Run();
            Assert.True(Assert.Single(interaction.Errors));
            Assert.Contains("Yanıt doğrulanamadı", Assert.Single(interaction.Messages));
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(2)]
    [InlineData(8)]
    public void BlockedResult_ShowsReasonWithoutGrantingApproval(int outcome)
    {
        using (var file = new TempFile())
        {
            var interaction = new FakeInteraction { FilePath = file.Path };
            new RegionSourceValidationWorkflow(interaction, r =>
                new RegionSourceValidationHostCommandResult(RegionSourceHostStatus.Blocked,
                    workerResult: new RegionSourceValidationResult("1.1", r.RequestId,
                        WorkerOperationKind.ValidateRegionSource, (RegionSourceValidationOutcome)outcome))).Run();
            Assert.True(Assert.Single(interaction.Errors));
            Assert.Contains(((RegionSourceValidationOutcome)outcome).ToString(),
                Assert.Single(interaction.Messages));
        }
    }

    [Fact]
    public void CallbackProxy_ExposesNewCallbackAndRemainsDisabledWhenDisconnected()
    {
        var proxy = new SolidWorksAddinCallbackProxy(new SolidWorksAddin());
        Assert.Equal(0, proxy.CanValidateStepSource());
        Assert.Equal(0, proxy.CanImportStep());
        proxy.OnValidateStepSource();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Workflow_ToRealWorker_UsesPrePromptSnapshot(bool changeDuringPrompt)
    {
        string golden = System.IO.Path.Combine(AppContext.BaseDirectory,
            "TestData", "Step", "Golden", "T001_PRISM.step");
        string copy = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        try
        {
            File.Copy(golden, copy);
            var interaction = new FakeInteraction { FilePath = copy };
            interaction.BeforeFaceReturn = () =>
            {
                if (changeDuringPrompt) File.AppendAllText(copy, Environment.NewLine);
            };
            var command = new RegionSourceValidationHostCommand();
            new RegionSourceValidationWorkflow(interaction,
                r => command.Execute(Worker(), r, 30000)).Run();
            Assert.Equal(changeDuringPrompt, Assert.Single(interaction.Errors));
            Assert.Contains(changeDuringPrompt ? "DocumentMismatch" : "Dosya/yüz bağlantısı eşleşti",
                Assert.Single(interaction.Messages));
        }
        finally { File.Delete(copy); }
    }

    private static string Worker()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "src", "MPC.sln")))
            directory = directory.Parent;
        if (directory is null) throw new DirectoryNotFoundException("MPC root");
#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif
        string path = System.IO.Path.Combine(directory.FullName, "src", "MPC.Worker", "bin",
            "x64", configuration, "net8.0", "MPC.Worker.exe");
        Assert.True(File.Exists(path), path);
        return path;
    }

    private sealed class FakeInteraction : IRegionSourceValidationInteraction
    {
        internal string? FilePath;
        internal string? Face = "#8";
        internal int FaceCalls;
        internal string? PromptHash;
        internal Action? BeforeFaceReturn;
        internal readonly List<string> Messages = new List<string>();
        internal readonly List<bool> Errors = new List<bool>();
        public string? ChooseStepFile() => FilePath;
        public string? ChooseSourceFace(string filePath, string fingerprint)
        {
            FaceCalls++;
            PromptHash = fingerprint;
            BeforeFaceReturn?.Invoke();
            return Face;
        }
        public void ShowResult(string message, bool isError)
        {
            Messages.Add(message);
            Errors.Add(isError);
        }
    }

    private sealed class TempFile : IDisposable
    {
        internal readonly string Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        internal TempFile() { File.WriteAllText(Path, "controlled hash bytes"); }
        public void Dispose() { File.Delete(Path); }
    }
}
