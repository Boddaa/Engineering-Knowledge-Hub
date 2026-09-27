namespace EngineeringKnowledgeHub.Application.Rag;

using System.Text.RegularExpressions;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Application.Search;
using EngineeringKnowledgeHub.Application.SemanticSearch;
using EngineeringKnowledgeHub.Domain.Entities;
using EngineeringKnowledgeHub.Domain.Enums;

/// <summary>
/// Orchestrates multi-source evidence retrieval for RAG.
/// Queries knowledge articles, vector-indexed document chunks, and operational incidents.
/// Performs candidate deduplication, relevance thresholding, and deterministic ranking.
/// </summary>
public sealed class KnowledgeRetriever : IKnowledgeRetriever
{
    private readonly ISearchService _searchService;
    private readonly ISemanticSearchService? _semanticSearchService;
    private readonly IIncidentRepository? _incidentRepository;
    private readonly IDocumentRepository? _documentRepository;
    private readonly IKnowledgeArticleRepository? _articleRepository;

    public KnowledgeRetriever(
        ISearchService searchService,
        ISemanticSearchService? semanticSearchService = null,
        IIncidentRepository? incidentRepository = null,
        IDocumentRepository? documentRepository = null,
        IKnowledgeArticleRepository? articleRepository = null)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _semanticSearchService = semanticSearchService;
        _incidentRepository = incidentRepository;
        _documentRepository = documentRepository;
        _articleRepository = articleRepository;
    }

    public async Task<IReadOnlyList<RagEvidenceItem>> RetrieveEvidenceAsync(
        RagRetrievalRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var query = request.Query.Trim();
        var candidates = new List<RagEvidenceItem>();

        // 1. Retrieve Knowledge Articles via Hybrid Search
        var hybridRequest = new HybridSearchArticleRequest
        {
            Query = query,
            Page = 1,
            PageSize = Math.Max(10, request.MaxEvidenceItems * 2),
            ProductId = request.ProductId,
            TechnologyId = request.TechnologyId,
            KnowledgeTypeId = request.KnowledgeTypeId,
            TagId = request.TagId,
            VerificationStatus = request.VerificationStatus,
            Options = new HybridSearchOptions
            {
                KeywordWeight = 0.5,
                SemanticWeight = 0.5,
                VerifiedBoost = request.PreferVerified ? 1.2 : 1.0
            }
        };

        var articleResults = await _searchService.HybridSearchArticlesAsync(hybridRequest, cancellationToken);
        foreach (var art in articleResults.Items)
        {
            var score = art.HybridScore ?? (art.KeywordScore ?? 0.0);
            if (score <= 0.0)
            {
                continue;
            }

            var normalizedScore = Math.Min(1.0, score);
            var content = !string.IsNullOrWhiteSpace(art.Summary) && !string.IsNullOrWhiteSpace(art.Excerpt)
                ? $"{art.Summary}\n{art.Excerpt}"
                : !string.IsNullOrWhiteSpace(art.Summary) ? art.Summary : art.Excerpt;

            candidates.Add(new RagEvidenceItem(
                CitationNumber: 0,
                SourceType: "KnowledgeArticle",
                ReferenceId: art.Id,
                SecondaryId: null,
                Title: art.Title,
                Content: content,
                RelevanceScore: normalizedScore,
                VerificationStatus: art.VerificationStatus,
                Metadata: new Dictionary<string, string>
                {
                    ["Slug"] = art.Slug,
                    ["Status"] = art.Status.ToString()
                }));
        }

        // 2. Retrieve Document Chunks via Semantic Vector Search
        if (_semanticSearchService != null)
        {
            var semanticResults = await _semanticSearchService.SearchAsync(
                new SemanticSearchRequest
                {
                    Query = query,
                    TopK = Math.Max(10, request.MaxEvidenceItems * 2),
                    ProductId = request.ProductId,
                    TechnologyId = request.TechnologyId,
                    KnowledgeTypeId = request.KnowledgeTypeId,
                    TagId = request.TagId,
                    VerificationStatus = request.VerificationStatus,
                    MinSimilarity = request.MinRelevanceScore
                },
                cancellationToken);

            foreach (var sem in semanticResults)
            {
                if (sem.DocumentId.HasValue)
                {
                    var docTitle = "Document Chunk";
                    if (_documentRepository != null)
                    {
                        var doc = await _documentRepository.GetByIdAsync(sem.DocumentId.Value, cancellationToken);
                        if (doc != null)
                        {
                            docTitle = doc.FileName;
                        }
                    }

                    candidates.Add(new RagEvidenceItem(
                        CitationNumber: 0,
                        SourceType: "DocumentChunk",
                        ReferenceId: sem.DocumentId.Value,
                        SecondaryId: sem.ChunkId,
                        Title: docTitle,
                        Content: sem.Text,
                        RelevanceScore: Math.Min(1.0, sem.SimilarityScore),
                        VerificationStatus: null,
                        Metadata: new Dictionary<string, string>(sem.Metadata)));
                }
            }
        }

        // 3. Retrieve Incidents (if repository available and incidents can be proven to match requested filters)
        if (_incidentRepository != null)
        {
            var incidents = await _incidentRepository.GetAllAsync(cancellationToken);
            var queryTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var inc in incidents)
            {
                if (inc.Status != IncidentStatus.Resolved)
                {
                    continue;
                }

                // Verify structured filter integrity
                if (!await IncidentMatchesFiltersAsync(inc, request, cancellationToken))
                {
                    continue;
                }

                var matchScore = ComputeIncidentScore(inc, queryTerms);
                if (matchScore > 0)
                {
                    var normalizedScore = Math.Min(1.0, matchScore / 100.0);
                    if (normalizedScore >= request.MinRelevanceScore)
                    {
                        var incidentContent = $"Problem: {inc.ProblemDescription}\nSymptoms: {inc.Symptoms}\nResolution: {inc.Resolution}";
                        candidates.Add(new RagEvidenceItem(
                            CitationNumber: 0,
                            SourceType: "Incident",
                            ReferenceId: inc.Id,
                            SecondaryId: null,
                            Title: inc.Title,
                            Content: incidentContent,
                            RelevanceScore: normalizedScore,
                            VerificationStatus: null,
                            Metadata: new Dictionary<string, string>
                            {
                                ["Severity"] = inc.Severity.ToString(),
                                ["Status"] = inc.Status.ToString()
                            }));
                    }
                }
            }
        }

        // 4. Deduplicate and filter by MinRelevanceScore
        var deduplicated = new Dictionary<string, RagEvidenceItem>();
        foreach (var item in candidates)
        {
            if (item.RelevanceScore < request.MinRelevanceScore)
            {
                continue;
            }

            var key = $"{item.SourceType}:{item.ReferenceId}:{item.SecondaryId}";
            if (!deduplicated.TryGetValue(key, out var existing) || item.RelevanceScore > existing.RelevanceScore)
            {
                deduplicated[key] = item;
            }
        }

        // 5. Deterministic sorting: RelevanceScore descending, Verified first, then Title ascending
        var ranked = deduplicated.Values
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.VerificationStatus == VerificationStatus.Verified)
            .ThenBy(x => x.Title, StringComparer.Ordinal)
            .Take(request.MaxEvidenceItems)
            .ToList();

        // 6. Number citations 1-indexed
        var numbered = new List<RagEvidenceItem>(ranked.Count);
        for (int i = 0; i < ranked.Count; i++)
        {
            var r = ranked[i];
            numbered.Add(new RagEvidenceItem(
                CitationNumber: i + 1,
                SourceType: r.SourceType,
                ReferenceId: r.ReferenceId,
                SecondaryId: r.SecondaryId,
                Title: r.Title,
                Content: r.Content,
                RelevanceScore: r.RelevanceScore,
                VerificationStatus: r.VerificationStatus,
                Metadata: r.Metadata));
        }

        return numbered;
    }

    private static void ValidateRequest(RagRetrievalRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("Retrieval request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ValidationException("Retrieval query cannot be null or whitespace.");
        }

        if (request.MaxEvidenceItems < 1 || request.MaxEvidenceItems > 50)
        {
            throw new ValidationException("MaxEvidenceItems must be between 1 and 50.");
        }

        if (double.IsNaN(request.MinRelevanceScore) || double.IsInfinity(request.MinRelevanceScore) ||
            request.MinRelevanceScore < 0.0 || request.MinRelevanceScore > 1.0)
        {
            throw new ValidationException("MinRelevanceScore must be between 0.0 and 1.0.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ValidationException("ProductId cannot be an empty GUID.");
        }

        if (request.TechnologyId == Guid.Empty)
        {
            throw new ValidationException("TechnologyId cannot be an empty GUID.");
        }

        if (request.KnowledgeTypeId == Guid.Empty)
        {
            throw new ValidationException("KnowledgeTypeId cannot be an empty GUID.");
        }

        if (request.TagId == Guid.Empty)
        {
            throw new ValidationException("TagId cannot be an empty GUID.");
        }

        if (request.VerificationStatus.HasValue && !Enum.IsDefined(typeof(VerificationStatus), request.VerificationStatus.Value))
        {
            throw new ValidationException("Invalid verification status filter.");
        }
    }

    private static double ComputeIncidentScore(Incident incident, string[] terms)
    {
        double score = 0;
        foreach (var term in terms)
        {
            if (incident.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 30;
            }

            if (incident.Resolution.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 25;
            }

            if (incident.ProblemDescription.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 20;
            }

            if (incident.Symptoms.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 15;
            }
        }

        return score;
    }

    private async Task<bool> IncidentMatchesFiltersAsync(
        Incident incident,
        RagRetrievalRequest request,
        CancellationToken cancellationToken)
    {
        bool hasStructuredFilter = request.ProductId.HasValue ||
                                   request.TechnologyId.HasValue ||
                                   request.KnowledgeTypeId.HasValue ||
                                   request.TagId.HasValue ||
                                   request.VerificationStatus.HasValue;

        // If no structured filters are specified, the incident matches
        if (!hasStructuredFilter)
        {
            return true;
        }

        // If structured filters are specified, the incident can participate ONLY if
        // the system can prove that it satisfies the requested filters via linked articles.
        if (incident.ArticleLinks.Count == 0 || _articleRepository == null)
        {
            return false;
        }

        foreach (var link in incident.ArticleLinks)
        {
            var article = await _articleRepository.GetByIdAsync(link.ArticleId, cancellationToken);
            if (article == null)
            {
                continue;
            }

            bool matches = true;

            if (request.ProductId.HasValue && !article.ProductIds.Contains(request.ProductId.Value))
            {
                matches = false;
            }

            if (request.TechnologyId.HasValue && !article.TechnologyIds.Contains(request.TechnologyId.Value))
            {
                matches = false;
            }

            if (request.TagId.HasValue && !article.TagIds.Contains(request.TagId.Value))
            {
                matches = false;
            }

            if (request.KnowledgeTypeId.HasValue && article.KnowledgeTypeId != request.KnowledgeTypeId.Value)
            {
                matches = false;
            }

            if (request.VerificationStatus.HasValue && article.VerificationStatus != request.VerificationStatus.Value)
            {
                matches = false;
            }

            if (matches)
            {
                return true;
            }
        }

        return false;
    }
}
