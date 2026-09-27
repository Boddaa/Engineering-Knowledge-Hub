namespace EngineeringKnowledgeHub.Application.Rag;

using System.Text.RegularExpressions;

/// <summary>
/// Deterministic citation and evidence grounding validator.
/// Ensures AI-generated text contains valid citation markers referencing only evidence supplied in the context.
/// Rejects ungrounded claims, missing citations, and out-of-range/unsupplied citation markers.
/// </summary>
public sealed class GroundingValidator : IGroundingValidator
{
    private static readonly Regex CitationMarkerRegex = new(@"\[(\d+)\]", RegexOptions.Compiled);

    public GroundingValidationResult ValidateGrounding(
        string? content,
        IReadOnlyList<RagEvidenceItem> availableEvidence)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new GroundingValidationResult(
                IsGrounded: false,
                FailureReason: "Generated content is null or whitespace.",
                CitedNumbers: new HashSet<int>());
        }

        if (availableEvidence is null || availableEvidence.Count == 0)
        {
            return new GroundingValidationResult(
                IsGrounded: false,
                FailureReason: "No evidence was available to ground the generated content.",
                CitedNumbers: new HashSet<int>());
        }

        var validCitationNumbers = availableEvidence
            .Select(e => e.CitationNumber)
            .ToHashSet();

        var matches = CitationMarkerRegex.Matches(content);
        if (matches.Count == 0)
        {
            return new GroundingValidationResult(
                IsGrounded: false,
                FailureReason: "Generated content does not cite any evidence sources.",
                CitedNumbers: new HashSet<int>());
        }

        var citedNumbers = new HashSet<int>();
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out var citedNum))
            {
                if (!validCitationNumbers.Contains(citedNum))
                {
                    return new GroundingValidationResult(
                        IsGrounded: false,
                        FailureReason: $"Generated content references invalid or unsupplied citation number [{citedNum}].",
                        CitedNumbers: citedNumbers);
                }

                citedNumbers.Add(citedNum);
            }
        }

        return new GroundingValidationResult(
            IsGrounded: true,
            FailureReason: null,
            CitedNumbers: citedNumbers);
    }
}
