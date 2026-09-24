namespace EngineeringKnowledgeHub.Application.Common.Services;

using System.Security.Cryptography;
using EngineeringKnowledgeHub.Application.Common.Interfaces;

public sealed class Sha256FileHasher : IFileHasher
{
    public string ComputeHash(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var hashBytes = SHA256.HashData(content);
        return Convert.ToHexStringLower(hashBytes);
    }

    public string ComputeHash(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hashBytes);
    }
}
