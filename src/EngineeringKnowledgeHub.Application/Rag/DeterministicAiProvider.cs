namespace EngineeringKnowledgeHub.Application.Rag;

using System.Text;
using System.Text.RegularExpressions;
using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Local-first, offline, deterministic implementation of IAiProvider.
/// Generates structured answers strictly grounded on the evidence citations provided in the context prompt.
/// Requires zero external internet access, zero API keys, and zero external AI vendor SDKs.
/// </summary>
public sealed class DeterministicAiProvider : IAiProvider
{
    private const string ModelIdentifier = "DeterministicLocalModel";
    private static readonly Regex CitationRegex = new(@"\[(\d+)\]\s*\(([^)]+)\)\s*(?:\[([^]]+)\])?\s*Title:\s*([^\r\n]+)\s*Content:\s*([^\r\n]+(?:\r?\n(?!\s*\[\d+\])[^\r\n]+)*)", RegexOptions.Compiled);

    public Task<AiResponse> GenerateAsync(
        AiPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new ValidationException("AI prompt request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            throw new ValidationException("User prompt cannot be null or whitespace.");
        }

        // Check if context explicitly declares insufficient evidence
        if (request.UserPrompt.Contains("NO_EVIDENCE_AVAILABLE", StringComparison.OrdinalIgnoreCase))
        {
            var fallbackText = "Insufficient evidence was found in the local engineering repository to answer this question. Please consult a senior engineer or submit a new knowledge article.";
            return Task.FromResult(new AiResponse(
                Content: fallbackText,
                ModelName: ModelIdentifier,
                PromptTokens: EstimateTokenCount(request.SystemPrompt + request.UserPrompt),
                CompletionTokens: EstimateTokenCount(fallbackText)));
        }

        var matches = CitationRegex.Matches(request.UserPrompt);
        if (matches.Count == 0)
        {
            var fallbackText = "No relevant engineering evidence citations were provided to ground an answer.";
            return Task.FromResult(new AiResponse(
                Content: fallbackText,
                ModelName: ModelIdentifier,
                PromptTokens: EstimateTokenCount(request.SystemPrompt + request.UserPrompt),
                CompletionTokens: EstimateTokenCount(fallbackText)));
        }

        var sb = new StringBuilder();
        sb.AppendLine("Based on the indexed engineering knowledge base:");
        sb.AppendLine();

        int citationCount = 0;
        foreach (Match match in matches)
        {
            citationCount++;
            var citationNumber = match.Groups[1].Value;
            var sourceType = match.Groups[2].Value.Trim();
            var verification = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;
            var title = match.Groups[4].Value.Trim();
            var content = match.Groups[5].Value.Trim();

            // Summarize the evidence point
            var preview = content.Length > 200 ? content[..200].TrimEnd() + "..." : content;
            sb.Append($"- According to [{citationNumber}] ({title}");
            if (!string.IsNullOrEmpty(verification) && !string.Equals(verification, "Unverified", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($" - {verification}");
            }
            sb.AppendLine($"): {preview}");
        }

        sb.AppendLine();
        sb.Append($"Refer to citation{(citationCount > 1 ? "s" : string.Empty)} ");
        var citationNums = matches.Select(m => $"[{m.Groups[1].Value}]");
        sb.Append(string.Join(", ", citationNums));
        sb.Append(" for complete technical procedures and source documentation.");

        var generatedContent = sb.ToString().Trim();

        var promptTokens = EstimateTokenCount(request.SystemPrompt + " " + request.UserPrompt);
        var completionTokens = EstimateTokenCount(generatedContent);

        return Task.FromResult(new AiResponse(
            Content: generatedContent,
            ModelName: ModelIdentifier,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens));
    }

    private static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Approximate token count: ~4 characters per token
        return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
    }
}
