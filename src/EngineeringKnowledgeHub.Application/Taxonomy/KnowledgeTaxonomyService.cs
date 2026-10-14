namespace EngineeringKnowledgeHub.Application.Taxonomy;

using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Application.Common.Interfaces;
using EngineeringKnowledgeHub.Application.DTOs;
using EngineeringKnowledgeHub.Domain.Entities;

public sealed class KnowledgeTaxonomyService(
    IKnowledgeCategoryRepository categoryRepository,
    IKnowledgeTypeRepository typeRepository,
    IKnowledgeTagRepository tagRepository,
    IProductRepository productRepository,
    ITechnologyRepository technologyRepository) : IKnowledgeTaxonomyService
{
    // ================= Category Operations =================

    public async Task<KnowledgeCategoryDto> CreateCategoryAsync(
        CreateKnowledgeCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Category name is required.");
        }

        if (command.ParentCategoryId.HasValue)
        {
            var parent = await categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                throw new NotFoundException("KnowledgeCategory", command.ParentCategoryId.Value);
            }
        }

        var category = new KnowledgeCategory(
            Guid.NewGuid(),
            command.Name,
            command.Description,
            command.ParentCategoryId);

        await categoryRepository.AddAsync(category, cancellationToken);

        return KnowledgeCategoryDto.FromDomain(category);
    }

    public async Task<KnowledgeCategoryDto> UpdateCategoryAsync(
        UpdateKnowledgeCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("Category Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Category name is required.");

        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeCategory", command.Id);

        if (command.ParentCategoryId.HasValue && command.ParentCategoryId.Value != command.Id)
        {
            var parent = await categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                throw new NotFoundException("KnowledgeCategory", command.ParentCategoryId.Value);
            }
        }

        category.Update(command.Name, command.Description, command.ParentCategoryId);

        await categoryRepository.UpdateAsync(category, cancellationToken);

        return KnowledgeCategoryDto.FromDomain(category);
    }

    public async Task<KnowledgeCategoryDto> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Category Id cannot be empty.");

        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeCategory", id);

        return KnowledgeCategoryDto.FromDomain(category);
    }

    // ================= KnowledgeType Operations =================

    public async Task<KnowledgeTypeDto> CreateTypeAsync(
        CreateKnowledgeTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("KnowledgeType name is required.");
        }

        var type = new KnowledgeType(
            Guid.NewGuid(),
            command.Name,
            command.Description);

        await typeRepository.AddAsync(type, cancellationToken);

        return KnowledgeTypeDto.FromDomain(type);
    }

    public async Task<KnowledgeTypeDto> UpdateTypeAsync(
        UpdateKnowledgeTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("KnowledgeType Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("KnowledgeType name is required.");

        var type = await typeRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeType", command.Id);

        type.Update(command.Name, command.Description);

        await typeRepository.UpdateAsync(type, cancellationToken);

        return KnowledgeTypeDto.FromDomain(type);
    }

    public async Task<KnowledgeTypeDto> GetTypeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("KnowledgeType Id cannot be empty.");

        var type = await typeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeType", id);

        return KnowledgeTypeDto.FromDomain(type);
    }

    // ================= KnowledgeTag Operations =================

    public async Task<KnowledgeTagDto> CreateTagAsync(
        CreateKnowledgeTagCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Tag name is required.");
        }

        var normalizedName = command.Name.Trim().ToLowerInvariant();
        var existing = await tagRepository.GetByNameAsync(normalizedName, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"Tag '{normalizedName}' already exists.");
        }

        var tag = new KnowledgeTag(
            Guid.NewGuid(),
            command.Name);

        await tagRepository.AddAsync(tag, cancellationToken);

        return KnowledgeTagDto.FromDomain(tag);
    }

    public async Task<KnowledgeTagDto> UpdateTagAsync(
        UpdateKnowledgeTagCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty) throw new ValidationException("Tag Id cannot be empty.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Tag name is required.");

        var tag = await tagRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeTag", command.Id);

        var normalizedName = command.Name.Trim().ToLowerInvariant();
        if (!string.Equals(tag.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await tagRepository.GetByNameAsync(normalizedName, cancellationToken);
            if (existing is not null)
            {
                throw new ConflictException($"Tag '{normalizedName}' already exists.");
            }
        }

        tag.UpdateName(command.Name);

        await tagRepository.UpdateAsync(tag, cancellationToken);

        return KnowledgeTagDto.FromDomain(tag);
    }

    public async Task<KnowledgeTagDto> GetTagByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Tag Id cannot be empty.");

        var tag = await tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("KnowledgeTag", id);

        return KnowledgeTagDto.FromDomain(tag);
    }

    public async Task<IReadOnlyList<KnowledgeCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(KnowledgeCategoryDto.FromDomain).ToList();
    }

    public async Task<IReadOnlyList<KnowledgeTypeDto>> GetTypesAsync(CancellationToken cancellationToken = default)
    {
        var types = await typeRepository.GetAllAsync(cancellationToken);
        return types.Select(KnowledgeTypeDto.FromDomain).ToList();
    }

    public async Task<IReadOnlyList<KnowledgeTagDto>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await tagRepository.GetAllAsync(cancellationToken);
        return tags.Select(KnowledgeTagDto.FromDomain).ToList();
    }

    public async Task<TaxonomySeedResultDto> SeedDefaultTaxonomyAsync(CancellationToken cancellationToken = default)
    {
        // 1. Knowledge Types
        var existingTypes = await typeRepository.GetAllAsync(cancellationToken);
        var existingTypeNames = existingTypes.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var typesToSeed = new (string Name, string Description)[]
        {
            ("Runbook", "Step-by-step operational procedures and recovery workflows"),
            ("Troubleshooting Guide", "Diagnostic steps and root-cause resolution for recurring issues"),
            ("Incident Postmortem", "Root-cause analysis and preventive actions from resolved incidents"),
            ("Architecture Decision", "System design specifications, patterns, and architectural boundaries"),
            ("Known Issue", "Documented defects, workarounds, and affected environments")
        };

        var typesAdded = 0;
        foreach (var (name, description) in typesToSeed)
        {
            if (!existingTypeNames.Contains(name))
            {
                var type = new KnowledgeType(Guid.NewGuid(), name, description);
                await typeRepository.AddAsync(type, cancellationToken);
                existingTypeNames.Add(name);
                typesAdded++;
            }
        }

        // 2. Knowledge Categories
        var existingCategories = await categoryRepository.GetAllAsync(cancellationToken);
        var existingCategoryNames = existingCategories.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoriesToSeed = new (string Name, string Description)[]
        {
            ("Backend & APIs", "ASP.NET Core APIs, SignalR hubs, and background services"),
            ("Infrastructure & IIS", "Windows Server, IIS hosting, networking, and deployment"),
            ("Toll & Field Hardware", "ANPR cameras, barrier controllers, lane sensors, and RTSP streams"),
            ("Database & Persistence", "SQL Server schemas, EF Core migrations, and query tuning")
        };

        var categoriesAdded = 0;
        foreach (var (name, description) in categoriesToSeed)
        {
            if (!existingCategoryNames.Contains(name))
            {
                var category = new KnowledgeCategory(Guid.NewGuid(), name, description);
                await categoryRepository.AddAsync(category, cancellationToken);
                existingCategoryNames.Add(name);
                categoriesAdded++;
            }
        }

        // 3. Products
        var existingProducts = await productRepository.GetAllAsync(null, cancellationToken);
        var existingProductNames = existingProducts.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingProductCodes = existingProducts.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var productsToSeed = new (string Name, string Code, string Description)[]
        {
            ("Suez Toll System", "SUEZ-TOLL", "Core toll collection, lane control, and real-time monitoring platform"),
            ("ANPR Camera Engine", "ANPR", "Automatic number plate recognition and stream processing"),
            ("Barrier Control System", "BARRIER", "Gate controller and hardware actuator integration"),
            ("Engineering Knowledge Hub", "EKH", "Internal engineering documentation and troubleshooting platform")
        };

        var productsAdded = 0;
        foreach (var (name, code, description) in productsToSeed)
        {
            if (!existingProductNames.Contains(name) && !existingProductCodes.Contains(code))
            {
                var product = new Product(Guid.NewGuid(), name, code, description, isActive: true);
                await productRepository.AddAsync(product, cancellationToken);
                existingProductNames.Add(name);
                existingProductCodes.Add(code);
                productsAdded++;
            }
        }

        // 4. Technologies
        var existingTechnologies = await technologyRepository.GetAllAsync(cancellationToken);
        var existingTechNames = existingTechnologies.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var technologiesToSeed = new (string Name, string Description)[]
        {
            ("ASP.NET Core", "Web framework and runtime"),
            ("SignalR", "Real-time bidirectional communication"),
            ("IIS", "Windows web server hosting"),
            ("SQL Server", "Relational database and persistence engine"),
            ("React", "Frontend user interface library"),
            ("ONVIF / RTSP", "Camera streaming and video protocol")
        };

        var technologiesAdded = 0;
        foreach (var (name, description) in technologiesToSeed)
        {
            if (!existingTechNames.Contains(name))
            {
                var tech = new Technology(Guid.NewGuid(), name, description);
                await technologyRepository.AddAsync(tech, cancellationToken);
                existingTechNames.Add(name);
                technologiesAdded++;
            }
        }

        // 5. Knowledge Tags
        var existingTags = await tagRepository.GetAllAsync(cancellationToken);
        var existingTagNames = existingTags.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var tagsToSeed = new string[]
        {
            "signalr", "iis", "sql-server", "anpr", "toll-lane", "runbook"
        };

        var tagsAdded = 0;
        foreach (var tagText in tagsToSeed)
        {
            var normalized = tagText.Trim().ToLowerInvariant();
            if (!existingTagNames.Contains(normalized))
            {
                var tag = new KnowledgeTag(Guid.NewGuid(), normalized);
                await tagRepository.AddAsync(tag, cancellationToken);
                existingTagNames.Add(normalized);
                tagsAdded++;
            }
        }

        return new TaxonomySeedResultDto(typesAdded, categoriesAdded, productsAdded, technologiesAdded, tagsAdded);
    }
}
