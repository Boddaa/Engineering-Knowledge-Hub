namespace EngineeringKnowledgeHub.Application.Rag;

using System.Text;
using EngineeringKnowledgeHub.Application.Common.Exceptions;

/// <summary>
/// Assembles retrieved engineering evidence into structured context for AI generation.
/// Enforces token budgets and formats citations with explicit traceability.
/// </summary>
public sealed class ContextBuilder : IContextBuilder
{
    private const string DefaultSystemPrompt =
        "You are an assistant for the Engineering Knowledge Hub. " +
        "Your answers must be strictly grounded in the provided engineering evidence citations. " +
        "Do not fabricate facts, procedures, or solutions. " +
        "If the evidence does not contain sufficient information, state clearly that the available engineering knowledge is insufficient. " +
        "Cite evidence sources using their bracketed numbers (e.g. [1], [2]).";

    public RagContext BuildContext(
        string question,
        IReadOnlyList<RagEvidenceItem> evidence,
        RagContextOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ValidationException("Question cannot be null or whitespace.");
        }

        options ??= new RagContextOptions();
        var trimmedQuestion = question.Trim();

        var qualifyingEvidence = evidence?
            .Where(e => e.RelevanceScore >= options.InsufficientEvidenceThreshold)
            .ToList() ?? new List<RagEvidenceItem>();

        if (qualifyingEvidence.Count == 0)
        {
            var noEvidencePrompt = $"Question: {trimmedQuestion}\nNO_EVIDENCE_AVAILABLE";
            var tokens = EstimateTokens(DefaultSystemPrompt + " " + noEvidencePrompt);

            return new RagContext(
                SystemPrompt: DefaultSystemPrompt,
                UserPrompt: noEvidencePrompt,
                EvidenceItems: Array.Empty<RagEvidenceItem>(),
                HasSufficientEvidence: false,
                EstimatedTokens: tokens);
        }

        var sb = new StringBuilder();
        sb.AppendLine("Evidence Citations:");
        sb.AppendLine();

        var includedItems = new List<RagEvidenceItem>();
        int currentChars = DefaultSystemPrompt.Length + trimmedQuestion.Length + 100;
        int maxChars = options.MaxTokens * 4;

        foreach (var item in qualifyingEvidence)
        {
            var itemSb = new StringBuilder();
            itemSb.Append($"[{item.CitationNumber}] ({item.SourceType})");
            if (options.IncludeVerificationStatus && item.VerificationStatus.HasValue)
            {
                itemSb.Append($" [{item.VerificationStatus.Value}]");
            }

            itemSb.AppendLine($" Title: {item.Title}");

            var availableChars = maxChars - currentChars - itemSb.Length;
            if (availableChars <= 50)
            {
                // Budget exhausted
                break;
            }

            var content = item.Content;
            if (content.Length > availableChars)
            {
                content = content[..availableChars].TrimEnd() + "...";
            }

            itemSb.AppendLine($"Content: {content}");
            itemSb.AppendLine();

            var itemText = itemSb.ToString();
            sb.Append(itemText);
            currentChars += itemText.Length;
            includedItems.Add(item);
        }

        sb.AppendLine($"Question: {trimmedQuestion}");
        sb.AppendLine("Instructions: Synthesize a concise technical answer strictly using the provided citations.");

        var userPrompt = sb.ToString().Trim();
        var totalTokens = EstimateTokens(DefaultSystemPrompt + " " + userPrompt);

        return new RagContext(
            SystemPrompt: DefaultSystemPrompt,
            UserPrompt: userPrompt,
            EvidenceItems: includedItems,
            HasSufficientEvidence: includedItems.Count > 0,
            EstimatedTokens: totalTokens);
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
    }
}
