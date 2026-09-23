# Engineering Knowledge Hub

## Project
Engineering Knowledge Hub

## Purpose
Engineering Knowledge Hub is an internal engineering knowledge platform designed to preserve company technical knowledge, capture troubleshooting experience, and streamline incident resolution. The platform provides a reliable, local-first repository of technical documentation, procedures, known issues, and resolved incidents.

## Architecture
The solution adheres to Clean Architecture principles, enforcing unidirectional dependencies pointing inward towards the core domain:

* **Domain (`EngineeringKnowledgeHub.Domain`)**: Core enterprise models and rules. It has zero external dependencies, no framework references, and no infrastructure knowledge.
* **Application (`EngineeringKnowledgeHub.Application`)**: Application-specific business rules, orchestrations, and interfaces. It references only the Domain.
* **Infrastructure (`EngineeringKnowledgeHub.Infrastructure`)**: Implementations for persistence, external services, and technical concerns. References Application and Domain.
* **Api (`EngineeringKnowledgeHub.Api`)**: ASP.NET Core Web API handling HTTP requests, routing, serialization, global exception handling, health checks, and OpenAPI documentation. References Application and Infrastructure for dependency composition.
* **Tests (`EngineeringKnowledgeHub.Tests`)**: Automated test suite containing architectural rule assertions, integration tests via `WebApplicationFactory`, and infrastructure registration checks.

## Solution Structure
```text
EngineeringKnowledgeHub/
├── src/
│   ├── EngineeringKnowledgeHub.Domain/
│   │   ├── AssemblyMarker.cs
│   │   └── EngineeringKnowledgeHub.Domain.csproj
│   ├── EngineeringKnowledgeHub.Application/
│   │   ├── AssemblyMarker.cs
│   │   └── EngineeringKnowledgeHub.Application.csproj
│   ├── EngineeringKnowledgeHub.Infrastructure/
│   │   ├── DependencyInjection.cs
│   │   └── EngineeringKnowledgeHub.Infrastructure.csproj
│   └── EngineeringKnowledgeHub.Api/
│       ├── Common/
│       │   └── GlobalExceptionHandler.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Program.cs
│       └── EngineeringKnowledgeHub.Api.csproj
├── tests/
│   └── EngineeringKnowledgeHub.Tests/
│       ├── Architecture/
│       │   └── ArchitectureTests.cs
│       ├── Api/
│       │   ├── ApiTests.cs
│       │   └── GlobalExceptionHandlerTests.cs
│       ├── Infrastructure/
│       │   └── InfrastructureTests.cs
│       └── EngineeringKnowledgeHub.Tests.csproj
├── ARCHITECTURE.md
├── AGENT_RULES.md
├── EngineeringKnowledgeHub.sln
└── README.md
```

## Technology Stack
* **Runtime & Framework**: .NET 10 (C# 14) / ASP.NET Core
* **Structured Logging**: Serilog (`Serilog.AspNetCore`)
* **API Documentation**: OpenAPI / Swagger (`Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi`)
* **Diagnostics & Health**: ASP.NET Core Health Checks, RFC 7807 Problem Details
* **Testing Framework**: xUnit, Microsoft.NET.Test.Sdk, `Microsoft.AspNetCore.Mvc.Testing`

## Development Requirements
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Version 10.0.100 or newer)
* Modern IDE or editor (Visual Studio 2025+, Visual Studio Code with C# Dev Kit, or JetBrains Rider)

## Running the API
To run the API locally using standard .NET CLI tooling:

```bash
dotnet run --project src/EngineeringKnowledgeHub.Api/EngineeringKnowledgeHub.Api.csproj
```

Once started:
* **Health Check**: `GET http://localhost:5000/health` (or configured HTTPS port)
* **Swagger UI**: `GET http://localhost:5000/swagger/index.html` (in Development environment)
* **OpenAPI Specification**: `GET http://localhost:5000/swagger/v1/swagger.json`

## Running Tests
To execute all automated architecture, API, and infrastructure tests:

```bash
dotnet test
```

## Current Phase
**Phase 0: Foundation**

## Future Direction
Subsequent phases will incrementally introduce:
* Phase 1: Identity (Authentication, Users, Roles, Claims)
* Phase 2: Knowledge Core (Articles, Categories, Types, Tags, Products, Technologies)
* Phase 3: Search (Local search indexing and filtering)
* Phase 4: Incidents (Incident logging and incident-to-knowledge conversion)
* Phase 5: Documents & Sources (Document ingestion and extraction)
* Phase 6: Governance (Review workflow, versioning, audit logging)
* Phase 7: Semantic Search (Vector embeddings and semantic indexing)
* Phase 8: RAG (Retrieval-augmented generation assistance)
* Phase 9: Online Enrichment (External vendor and reference documentation imports)
* Phase 10: Local AI (On-premise LLM support and inference)
