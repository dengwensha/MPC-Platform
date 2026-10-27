using System.Security.Cryptography;

namespace MPC.Tooling;

// Byte integrity only: does not authenticate a supplier, approve mapping or persist a source.
public sealed class CatalogSourceSnapshot
{
    private readonly byte[] _bytes;

    public string Sha256 { get; }

    public CatalogSourceSnapshot(byte[] sourceBytes)
    {
        ArgumentNullException.ThrowIfNull(sourceBytes);
        _bytes = (byte[])sourceBytes.Clone();
        Sha256 = Convert.ToHexString(SHA256.HashData(_bytes));
    }

    // Never expose the retained buffer to callers.
    public byte[] CopyBytes() => (byte[])_bytes.Clone();

    public bool MatchesSha256(string? expectedSha256)
    {
        if (expectedSha256 is null || expectedSha256.Length != 64 ||
            expectedSha256.Any(c => !((c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))))
        {
            return false;
        }

        return StringComparer.OrdinalIgnoreCase.Equals(Sha256, expectedSha256);
    }
}
