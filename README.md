# Engineering Knowledge Hub

[![Status: Production Ready](https://img.shields.io/badge/Status-Production%20Ready-success.svg)](#)
[![Backend: .NET 10](https://img.shields.io/badge/Backend-.NET%2010%20%2F%20C%23%2014-blue.svg)](#)
[![Frontend: React 19](https://img.shields.io/badge/Frontend-React%2019%20%2F%20TypeScript-61dafb.svg)](#)
[![Tests: 864 Passing](https://img.shields.io/badge/Tests-864%20Passing%20(100%25)-brightgreen.svg)](#)
[![Security: RBAC & Audit](https://img.shields.io/badge/Security-RBAC%20%26%20Audit%20Compliance-orange.svg)](#)

> **Engineering Knowledge Hub** is an enterprise-grade internal technical knowledge and compliance platform designed to capture troubleshooting experience, preserve organizational technical knowledge, streamline incident resolution, and provide localized AI retrieval assistance (RAG) with **Zero Cloud Egress**.

---

## 📑 Table of Contents
1. [Architecture Overview](#-architecture-overview)
2. [Technology Stack](#-technology-stack)
3. [Completed Milestones (Backend & Frontend)](#-completed-milestones)
4. [Core Platform Capabilities](#-core-platform-capabilities)
5. [API Surface & Controllers](#-api-surface--controllers)
6. [Testing & Verification](#-testing--verification)
7. [Getting Started & Local Execution](#-getting-started--local-execution)
8. [Project Structure](#-project-structure)

---

## 🏛️ Architecture Overview

The solution adheres strictly to **Clean Architecture** principles, enforcing inward dependency flow across four segregated layers:

```text
       ┌────────────────────────────────────────────────────────┐
       │                EngineeringKnowledgeHub.Api             │  (ASP.NET Core 10 Web API, 13 Controllers, RBAC, Swagger)
       └───────────┬────────────────────────────────┬───────────┘
                   │                                │
                   ▼                                ▼
       ┌────────────────────────┐      ┌────────────────────────┐
       │     Infrastructure     │      │      Application       │  (CQRS/Services, DTOs, Business Rules, Validation)
       └───────────┬────────────┘      └────────────┬───────────┘
                   │                                │
                   └───────────────┬────────────────┘
                                   ▼
                       ┌────────────────────────┐
                       │         Domain         │  (Core Entities, Aggregates, Pure Domain Rules, Zero Dependencies)
                       └────────────────────────┘
```

* **Domain (`EngineeringKnowledgeHub.Domain`)**: Core enterprise models and invariants. Zero external dependencies, zero framework references.
* **Application (`EngineeringKnowledgeHub.Application`)**: Business logic, commands, queries, DTO definitions, and service interfaces.
* **Infrastructure (`EngineeringKnowledgeHub.Infrastructure`)**: Entity Framework Core 10 persistence, SQL Server integration, repositories, deterministic AI adapters, and Ollama integration.
* **API (`EngineeringKnowledgeHub.Api`)**: 13 REST Controllers, RFC 7807 ProblemDetails error handling, Serilog logging, RBAC governance policies, and OpenAPI documentation.
* **Frontend (`frontend/`)**: Modern Single Page Application built with React 19, TypeScript 5.7, Vite 6, React Router 7, and a custom design tokens system (`tokens.css`) without heavyweight third-party UI framework overhead.

---

## 💻 Technology Stack

| Layer | Technologies & Tools |
| :--- | :--- |
| **Backend Runtime** | .NET 10 (C# 14), ASP.NET Core Web API |
| **Persistence / ORM** | Entity Framework Core 10, Microsoft SQL Server (LocalDB / Dedicated) |
| **Security & Auth** | Role-Based Access Control (RBAC), `AuditGovernancePolicy` (`Admin` & `Auditor` roles), Bearer Authentication |
| **Local AI Engine** | [Ollama](https://ollama.ai) integration (`llama3`, `nomic-embed-text`) with strict dimension validation & fallback safety |
| **Frontend Runtime** | React 19, TypeScript 5.7, Vite 6.4 |
| **Routing & State** | React Router 7, Custom Hooks with `AbortController` cancellation for race-condition prevention |
| **Styling & UI** | Pure CSS Modules + Custom Design Token System (`tokens.css`), `lucide-react` icons |
| **Test Suites** | xUnit, `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory), 864 automated tests |

---

## 🚀 Completed Milestones

### 1. Backend Milestones (Phases 0 through 10) — 100% Complete
- [x] **Phase 0: Foundation**: Clean Architecture structure, EF Core 10 setup, SQL Server configuration, dependency injection.
- [x] **Phase 1: Identity & Core Domain**: User entities, role assignment, author identity tracking.
- [x] **Phase 2: Knowledge Core**: Knowledge articles, versioning aggregate (`KnowledgeVersion`), lifecycle state management.
- [x] **Phase 3: Search Engine**: In-memory and database search indexes, tokenization, multi-field filtering.
- [x] **Phase 4: Incidents**: Incident tracking, severity classification, post-mortem notes, and `Incident-to-Knowledge` article generation.
- [x] **Phase 5: Documents & Sources**: Ingestion pipeline, metadata extraction, document status lifecycle.
- [x] **Phase 6: Governance**: Review workflow (submission, reviewer approval, rejection with reasons), article deprecation, and archival.
- [x] **Phase 7: Semantic Search**: Vector embeddings generation, cosine similarity matching, semantic indexing.
- [x] **Phase 8: RAG Assistant**: Retrieval-Augmented Generation pipeline, evidence card provenance, answer synthesis.
- [x] **Phase 9: Online Enrichment**: External documentation sources, vendor documentation sync metadata.
- [x] **Phase 10: Local AI**: On-premise Ollama adapter, model health checks, offline deterministic fallbacks.
- [x] **REST API Surface**: 13 Controllers with comprehensive integration tests (**864 passing tests**).

### 2. Frontend Milestones (Phases F0 through F10) — 100% Complete
- [x] **Phase F0: Shell & Design System**: Application layout, responsive sidebar, breadcrumbs, theme tokens.
- [x] **Phase F1 & F2: Knowledge Listing & Detail View**: Typed API integration, article reader, Markdown rendering, metadata chips.
- [x] **Phase F3: Knowledge Authoring**: Article creation workspace, live Markdown preview, author identity hydration.
- [x] **Phase F4: Governance UI**: Review queue, approval/rejection modals with mandatory reasons, version history.
- [x] **Phase F5: Search & Discovery UX**: Real-time keyword search, multi-faceted filtering (product, category, status).
- [x] **Phase F6: Incidents & User Feedback**: Incident management dashboard, incident-to-article converter, feedback reactions.
- [x] **Phase F7: Taxonomies & Sources**: Management interfaces for Products, Technologies, Categories, and Knowledge Sources.
- [x] **Phase F8: Documents Workspace**: Document ingestion pipeline, status tracking, Propose Article Modal with strict `KnowledgeTypeId` validation.
- [x] **Phase F9: AI Knowledge Assistant & RAG**: Interactive Q&A, evidence provenance cards with relevance scores, Ollama health indicator, query history.
- [x] **Phase F10: System Governance & Audit Log**: Audit log table, scope filtering (All, Entity, User), UUID validation, race-condition prevention via `AbortController`, detail inspection modal, and RBAC authorization.

---

## 🔒 Core Platform Capabilities

### 1. Local-First AI & RAG (Zero Cloud Egress)
* **Local Inference**: All embeddings and chat completions execute against an on-premise Ollama instance. No prompt or document content leaves the local network.
* **Evidence Provenance**: RAG responses display discrete evidence cards linking back to original verified articles and documents, with distinct relevance scores.
* **Operational Health**: Real-time health badge monitors Ollama availability and model presence via `/api/ai/local/health`.

### 2. Governance, Versioning & Audit Trail
* **Immutable Audit Records**: Every critical action (article approval, rejection, deprecation, version creation) is recorded in an immutable audit log.
* **Role-Based Access Control (RBAC)**: Audit inspection endpoints are secured via `AuditGovernancePolicy`, restricting access to `Admin` and `Auditor` roles.
* **Review Lifecycle**: Articles transition through `Draft` ➔ `InReview` ➔ `Approved` / `Rejected` ➔ `Deprecated` / `Archived`.

### 3. Incident-to-Knowledge Pipeline
* Resolving production incidents directly captures the root cause and remediation steps into a reusable knowledge base article, preventing duplicate troubleshooting effort.

---

## 📡 API Surface & Controllers

The backend exposes **13 RESTful controllers** documented with Swagger/OpenAPI:

| Controller | Route | Purpose |
| :--- | :--- | :--- |
| `KnowledgeArticlesController` | `/api/knowledge/articles` | CRUD, draft saving, article lifecycle |
| `KnowledgeGovernanceController` | `/api/knowledge/governance` | Review submission, approval, rejection, deprecation |
| `KnowledgeTaxonomyController` | `/api/taxonomy` | Categories, tags, and classification |
| `SearchController` | `/api/search` | Full-text search and faceted filtering |
| `SemanticSearchController` | `/api/search/semantic` | Vector similarity search using embeddings |
| `RagController` | `/api/rag` | Retrieval-augmented Q&A synthesis |
| `LocalAiController` | `/api/ai/local` | Ollama model status, readiness, and diagnostics |
| `IncidentsController` | `/api/incidents` | Production incident logging and article conversion |
| `FeedbackController` | `/api/feedback` | User ratings, helpfulness votes, and comments |
| `DocumentsController` | `/api/documents` | Ingested documents and extraction management |
| `KnowledgeSourcesController` | `/api/sources` | External vendor documentation and sync endpoints |
| `AuditController` | `/api/audit` | System-wide compliance and audit log inspection (Protected) |
| `IdentityController` | `/api/identity` | User profiles, author attribution, and role mapping |

---

## 🧪 Testing & Verification

The solution enforces strict automated verification across both backend and frontend:

### Backend Automated Tests
```bash
dotnet test
```
* **Results**: **864 Passed**, 0 Failed, 0 Skipped (100% pass rate).
* **Coverage**: Architecture rule tests, unit tests, repository tests, WebApplicationFactory integration tests, and RBAC security tests.

### Frontend Quality & Production Build
```bash
cd frontend
npm run lint    # TypeScript type-check
npm run build   # Production bundle compilation
```
* **Results**: **0 Errors**, **0 Warnings**. Production bundle successfully built in `frontend/dist/`.

---

## 🚀 Getting Started & Local Execution

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Version 10.0.100+)
* Microsoft SQL Server (LocalDB or dedicated instance)
* [Node.js](https://nodejs.org) (v20+ or v22+) & npm
* *(Optional for Local AI)* [Ollama](https://ollama.ai)

### 1. Database & Backend Setup
1. Configure connection string in `src/EngineeringKnowledgeHub.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EngineeringKnowledgeHubDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
2. Start the API service:
   ```bash
   dotnet run --project src/EngineeringKnowledgeHub.Api/EngineeringKnowledgeHub.Api.csproj
   ```
   * Swagger Documentation: `http://localhost:5000/swagger`
   * Health Check: `http://localhost:5000/health`

### 2. Frontend Setup
```bash
cd frontend
npm install
npm run dev
```
* The web application will launch at `http://localhost:5173`.

### 3. Local AI Setup (Optional)
```bash
ollama serve
ollama pull llama3
ollama pull nomic-embed-text
```

---

## 📂 Project Structure

```text
EngineeringKnowledgeHub/
├── src/
│   ├── EngineeringKnowledgeHub.Domain/         # Pure domain entities & aggregates
│   ├── EngineeringKnowledgeHub.Application/    # Services, commands, DTOs, validations
│   ├── EngineeringKnowledgeHub.Infrastructure/ # EF Core, SQL Server, Ollama adapter
│   └── EngineeringKnowledgeHub.Api/            # 13 REST controllers, RBAC, Program.cs
├── tests/
│   └── EngineeringKnowledgeHub.Tests/          # 864 architecture & integration tests
├── frontend/
│   ├── src/
│   │   ├── api/                                # Typed API client with auth & abort
│   │   ├── components/                         # UI layout, shell, modals, shared UI
│   │   ├── features/                           # Feature modules (knowledge, rag, audit, incidents)
│   │   ├── pages/                              # Routed page components
│   │   └── styles/tokens.css                   # Custom design token system
│   ├── package.json
│   └── vite.config.ts
├── README.md                                   # Comprehensive platform documentation (English)
├── README_AR.md                                # Executive summary documentation (Arabic)
├── ARCHITECTURE.md                             # Architectural principles & standards
└── AGENT_RULES.md                              # Operational engineering rules
```

---

## 📄 License & Confidentiality
Internal proprietary system — for engineering organizational use only.
