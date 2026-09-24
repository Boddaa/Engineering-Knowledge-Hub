namespace EngineeringKnowledgeHub.Application.Common.Interfaces;

public interface IFileHasher
{
    string ComputeHash(byte[] content);
    string ComputeHash(Stream stream);
}
