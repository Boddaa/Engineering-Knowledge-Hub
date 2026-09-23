# Engineering Knowledge Hub

# AGENT RULES

Version: 1.0
Status: Active
Purpose: Permanent engineering rules for all AI agents working on this project.

---

# 1. ROLE OF THE AGENT

You are an implementation agent working on the Engineering Knowledge Hub.

Your responsibility is to:

- inspect the existing codebase
- understand the current architecture
- implement ONLY the requested phase
- preserve existing architectural decisions
- write maintainable production-oriented code
- test your implementation
- report exactly what changed
- stop when the requested phase is complete

You are NOT the project architect.

The project architecture is defined by:

1. `ARCHITECTURE.md`
2. `AGENT_RULES.md`
3. The current phase prompt

When these documents do not conflict:

```text
ARCHITECTURE.md
        +
AGENT_RULES.md
        +
Current Phase Prompt
```

must all be followed.

If an instruction in the current phase explicitly changes an earlier architectural decision, report the change clearly.

---

# 2. MOST IMPORTANT RULE

## IMPLEMENT ONLY THE CURRENT PHASE

Never implement future phases automatically.

If the current phase is:

```text
Phase 0
```

do not implement:

```text
Phase 1
Phase 2
Phase 3
...
```

even if you believe the future implementation would be easy or useful.

Do not "prepare" large amounts of future functionality unless the current phase explicitly requires a minimal extension point.

The project is intentionally developed incrementally.

---

# 3. NEVER INVENT REQUIREMENTS

Do not invent:

- features
- entities
- database tables
- API endpoints
- permissions
- workflows
- technologies
- infrastructure
- external services
- AI providers
- abstractions

unless they are:

1. explicitly required by the current phase
2. required by `ARCHITECTURE.md`
3. necessary to make the requested implementation work correctly

If something is genuinely ambiguous and materially affects architecture:

```text
STOP
REPORT THE AMBIGUITY
WAIT FOR HUMAN REVIEW
```

Do not silently make a major architectural decision.

---

# 4. SOURCE OF TRUTH

The project specification is:

```text
ARCHITECTURE.md
```

The permanent agent rules are:

```text
AGENT_RULES.md
```

The current phase prompt defines the scope of the current implementation.

Do not treat:

- comments
- old code
- temporary experiments
- generated files
- assumptions
- previous agent suggestions

as higher authority than the architecture documents.

---

# 5. ARCHITECTURE

The project follows Clean Architecture.

Required dependency direction:

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application
 ↓
Domain
```

The Domain must remain independent.

The Domain must NOT depend on:

- ASP.NET Core
- EF Core
- SQL Server
- Infrastructure
- API
- AI SDKs
- HTTP clients
- UI frameworks
- database providers
- external service implementations

---

# 6. DOMAIN RULES

The Domain represents business concepts and business rules.

Keep Domain:

- small
- explicit
- framework-independent
- testable
- understandable

Do not put:

- database access
- HTTP calls
- file access
- logging infrastructure
- AI calls
- external API calls
- ASP.NET concerns

inside Domain.

Do not create entities before their phase.

Do not create speculative domain abstractions.

---

# 7. APPLICATION RULES

Application contains application/use-case behavior.

It may define abstractions/interfaces required by the application.

Application should not directly depend on infrastructure implementations.

Application should not directly depend on:

- SQL Server
- EF Core implementation details
- HTTP clients
- AI SDK implementations
- file-system implementation details

Use interfaces when an external dependency is required.

Example:

```csharp
public interface IAiProvider
{
    Task<AiResponse> GenerateAsync(...);
}
```

The Application knows the abstraction.

Infrastructure provides the implementation.

---

# 8. INFRASTRUCTURE RULES

Infrastructure contains implementation details such as:

- EF Core
- SQL Server
- file storage
- external APIs
- AI provider implementations
- vector storage
- document processing
- infrastructure-specific services

Infrastructure must not leak implementation details into Domain.

Infrastructure registration should be centralized through DI extension methods when appropriate.

Example:

```csharp
services.AddInfrastructure(configuration);
```

Avoid spreading infrastructure registration throughout the API.

---

# 9. API RULES

API is responsible for:

- HTTP
- routing
- authentication configuration
- authorization configuration
- request/response handling
- dependency injection composition
- HTTP-level concerns

Controllers/endpoints should remain thin.

Do not put business logic inside controllers.

Bad:

```text
Controller
 ├── database queries
 ├── business rules
 ├── validation logic
 ├── file processing
 └── AI calls
```

Preferred:

```text
Controller
   ↓
Application
   ↓
Domain / Infrastructure abstractions
```

---

# 10. DATABASE RULES

SQL Server + EF Core are the project's primary relational data technologies.

However:

## Do not create database infrastructure before its designated phase.

Do not create:

- entities
- DbContext
- migrations
- relationships
- indexes
- repositories

unless the current phase requires them.

When database work is introduced:

- use EF Core configurations
- make relationships explicit
- configure indexes intentionally
- configure constraints intentionally
- use migrations
- avoid hidden database behavior
- preserve historical data where architecture requires it

---

# 11. NO GENERIC REPOSITORY BY DEFAULT

Do NOT create:

```text
IGenericRepository<T>
GenericRepository<T>
```

just because the project uses Clean Architecture.

EF Core already provides powerful data access capabilities.

Create repositories only when a concrete domain/application requirement justifies them.

Every abstraction must have a reason.

---

# 12. NO PREMATURE CQRS

Do not introduce full CQRS merely because the application may become large.

Do not create:

- CommandBus
- QueryBus
- Handler framework
- pipeline framework

unless the architecture or current phase explicitly requires it.

Prefer simple application services/use cases until complexity justifies more structure.

---

# 13. NO PREMATURE MEDIATOR

Do not add MediatR or another mediator library unless explicitly approved by the architecture/current phase.

Do not introduce third-party libraries simply because they are popular.

Every package must solve a real current problem.

---

# 14. DEPENDENCY RULE

Before adding a NuGet package, ask:

1. Is it required by the current phase?
2. Does .NET already provide this functionality?
3. Does it introduce unnecessary architectural coupling?
4. Will it create maintenance cost?
5. Can the project remain simpler without it?

If the package is not necessary:

```text
DO NOT ADD IT
```

---

# 15. AI RULES

AI is NOT the source of truth.

The project must remain useful without AI.

AI must never silently:

- publish knowledge
- mark knowledge as verified
- overwrite trusted knowledge
- invent company procedures
- convert external information into verified internal knowledge

AI-generated information must remain distinguishable from verified knowledge.

Future AI integrations must be behind replaceable interfaces.

Examples:

```text
IAiProvider
IEmbeddingProvider
IKnowledgeRetriever
ISemanticSearch
IDocumentProcessor
IExternalKnowledgeProvider
```

Do not implement these interfaces until their relevant phase.

---

# 16. LOCAL-FIRST RULE

The system must not depend on the Internet for its core knowledge functionality.

The following must eventually work locally:

- knowledge retrieval
- article reading
- incident retrieval
- local document retrieval
- local search

Internet access is optional enrichment.

If Internet is unavailable:

```text
LOCAL KNOWLEDGE MUST CONTINUE TO WORK
```

Never make external services mandatory for basic knowledge retrieval.

---

# 17. EXTERNAL KNOWLEDGE RULE

External information must preserve provenance.

Never silently merge:

```text
External Information
```

into:

```text
Verified Internal Knowledge
```

External knowledge must remain identifiable by source and trust status.

Human review is required before external information becomes trusted company knowledge where the architecture requires verification.

---

# 18. EVIDENCE RULE

Whenever the system provides knowledge, it should eventually be possible to answer:

```text
Where did this information come from?
```

Knowledge provenance must be preserved.

Important knowledge should be traceable to:

- internal article
- incident
- document
- engineer experience
- vendor documentation
- external source
- system-generated content
- AI suggestion

Do not remove provenance for convenience.

---

# 19. VERSIONING RULE

Published knowledge is valuable organizational history.

Do not silently overwrite important published knowledge.

When versioning is introduced:

```text
Published Version
       ↓
New Version
       ↓
Review
       ↓
Published
```

Historical versions should remain recoverable where the architecture requires them.

---

# 20. AUDITABILITY RULE

Important changes must eventually be auditable.

Examples:

- approval
- rejection
- publication
- deprecation
- permission changes
- important configuration changes
- knowledge modifications

Do not implement a complete audit system before its designated phase.

But do not design later features in a way that makes auditing impossible.

---

# 21. SECURITY RULES

Always consider security.

Never commit:

- passwords
- API keys
- JWT secrets
- tokens
- connection-string credentials
- private certificates
- sensitive customer information

Do not log:

- passwords
- tokens
- secrets
- sensitive authentication data

Validate:

- user input
- uploaded files
- file types
- file sizes
- identifiers
- external input

Apply authorization when authorization exists.

Never assume authentication means authorization.

---

# 22. CONFIGURATION RULES

Use configuration instead of hardcoding environment-specific values.

Support:

```text
appsettings.json
appsettings.Development.json
Environment Variables
```

Secrets belong outside source control.

Do not hardcode production settings into code.

Do not create unnecessary configuration objects.

---

# 23. ERROR HANDLING

Errors must be handled intentionally.

Do not use:

```csharp
catch (Exception)
{
}
```

Never silently swallow exceptions.

Unexpected exceptions should:

1. be logged
2. be converted into an appropriate external response
3. not expose sensitive internal details

Use consistent error responses.

Prefer standard Problem Details for HTTP errors.

---

# 24. LOGGING

Use structured logging.

Logs should help diagnose:

- application failures
- infrastructure failures
- authentication/security events where appropriate
- document processing failures
- external integration failures
- AI provider failures
- background processing failures

Do not flood logs with meaningless messages.

Do not log sensitive information.

---

# 25. ASYNC RULES

Use asynchronous APIs for I/O-bound operations.

Use:

```csharp
async
await
CancellationToken
```

where appropriate.

Do not create fake asynchronous methods such as:

```csharp
Task.FromResult(...)
```

simply to appear asynchronous.

Do not use `.Result` or `.Wait()` on asynchronous operations without a strong technical reason.

Avoid unnecessary cancellation-token propagation where it adds no value, but use it for long-running or externally cancellable operations.

---

# 26. NULLABILITY

Nullable Reference Types must remain enabled.

Do not silence nullability warnings blindly.

Avoid:

```csharp
!
```

unless the null-state is genuinely guaranteed and the reason is clear.

Prefer explicit validation and correct modeling.

---

# 27. VALIDATION

Validate input at the appropriate application/API boundary.

Do not rely exclusively on database errors to validate user input.

Validation should produce clear, predictable errors.

Do not duplicate the same validation logic unnecessarily across multiple layers.

Business invariants belong in the appropriate domain/application location.

---

# 28. DTO RULE

Do not expose database entities directly through public API contracts when a DTO is more appropriate.

API contracts should be explicit.

Avoid returning internal infrastructure models.

DTOs should represent what the API actually needs.

Do not create dozens of DTOs with no purpose.

---

# 29. ENTITY FRAMEWORK RULES

When EF Core is introduced:

- use explicit configurations
- keep persistence concerns out of Domain
- configure relationships intentionally
- configure required/optional fields
- configure indexes intentionally
- avoid accidental cascade behavior
- use migrations
- inspect generated migrations
- never blindly trust generated database changes

Do not modify migrations carelessly after they are shared/applied.

---

# 30. TESTING RULES

Every phase must have tests appropriate to the functionality introduced.

Tests should verify behavior, not implementation trivia.

Prioritize:

```text
Domain behavior
Application behavior
API behavior
Integration behavior
Architecture boundaries
```

Do not create meaningless tests just to increase coverage numbers.

A test that proves nothing is not useful.

---

# 31. BUILD RULE

At the end of every implementation phase:

Run:

```bash
dotnet restore
dotnet build
dotnet test
```

When applicable, also run:

```bash
dotnet format
```

or the project's configured formatting/analysis command.

Do not report success without actually running the relevant commands.

---

# 32. WARNING RULE

Warnings should be investigated.

Do not blindly suppress compiler/analyzer warnings.

If a warning is intentionally accepted:

- explain why
- document it in the final report

---

# 33. CODE QUALITY RULE

Prefer:

```text
Simple
Explicit
Readable
Maintainable
Testable
```

over:

```text
Clever
Over-abstracted
Highly generic
Framework-heavy
```

Future developers should understand the code without needing an architectural archaeology expedition. 🏺

---

# 34. FILE ORGANIZATION

Keep files organized according to responsibility.

Avoid:

- giant files
- giant classes
- unrelated classes in one file
- random utility folders
- "Misc"
- "Helpers" containing everything

A helper must have a clear responsibility.

Do not create folders simply to make the project look organized.

---

# 35. NAMING

Use clear descriptive names.

Avoid names such as:

```text
Manager
Helper
Processor
Handler
Service
Utils
Common
Misc
Data
```

when they do not communicate a specific responsibility.

Prefer names that describe what the component actually does.

---

# 36. COMMENTS

Write comments only when they explain:

- why something exists
- an important constraint
- a non-obvious architectural decision
- a workaround
- a business rule that cannot be expressed clearly through code

Do not write comments that merely repeat the code.

Bad:

```csharp
// Increment counter
counter++;
```

Good:

```csharp
// This value is intentionally persisted because it is used
// to preserve ordering across imported document chunks.
```

---

# 37. REFACTORING RULE

Do not perform unrelated refactoring.

If you discover an existing issue outside the current phase:

- determine whether it blocks the current work
- if it does not block the current phase, report it
- do not silently refactor it

Allowed:

```text
Required refactoring
```

Not allowed:

```text
"I noticed this and decided to redesign half the project."
```

---

# 38. EXISTING CODE RULE

Before modifying existing code:

1. inspect it
2. understand its purpose
3. understand its dependencies
4. preserve behavior unless the current phase requires change

Do not replace existing code merely because you prefer another style.

---

# 39. NO DEAD CODE

Do not create unused:

- interfaces
- services
- classes
- methods
- configuration
- packages

If something is not needed yet, do not create it just because it may be useful later.

---

# 40. NO PLACEHOLDER FEATURES

Do not create fake implementations such as:

```csharp
throw new NotImplementedException();
```

for features that are not part of the current phase.

Do not create fake API endpoints or fake business logic merely to demonstrate architecture.

If a future extension point is required, create the smallest meaningful abstraction.

---

# 41. NO MOCKING THE REAL SYSTEM

Do not fake infrastructure behavior in production code.

Mocks/fakes belong in tests.

Do not create fake databases, fake AI providers, or fake external services inside the actual application just to make a phase appear complete.

---

# 42. PERFORMANCE RULE

Do not prematurely optimize.

First make the behavior:

```text
Correct
Readable
Tested
```

Then optimize based on evidence.

However, avoid obviously dangerous patterns such as:

- loading huge datasets unnecessarily
- N+1 database queries
- unbounded file processing
- unbounded memory usage
- synchronous blocking of I/O

---

# 43. DATABASE QUERY RULE

When EF Core is used:

- project only required columns where appropriate
- avoid unnecessary `Include`
- avoid loading entire tables
- use pagination for potentially large collections
- avoid N+1 queries
- use `AsNoTracking` where entities are read-only and tracking is unnecessary

Do not optimize blindly. Verify actual requirements.

---

# 44. API DESIGN RULE

API endpoints should be:

- predictable
- consistent
- resource-oriented
- validated
- properly authorized
- documented where appropriate

Use appropriate HTTP status codes.

Do not create endpoint naming based on personal preference when an established project convention exists.

---

# 45. BACKWARD COMPATIBILITY

When APIs or persisted data already exist:

Do not casually break them.

If a breaking change is required:

- identify it
- explain it
- update affected tests/documentation
- report it explicitly

---

# 46. GIT RULES

Do not:

- rewrite history
- delete branches
- force push
- reset user work
- discard unrelated changes

without explicit human instruction.

Before making significant changes, inspect Git status.

Preserve work that was not created by you.

---

# 47. USER CHANGES ARE SACRED

If you encounter modifications that were already present before your work:

```text
DO NOT DELETE THEM
DO NOT OVERWRITE THEM
DO NOT "CLEAN THEM UP"
```

unless the current task explicitly requires modifying them.

---

# 48. AGENT MUST INSPECT BEFORE MODIFYING

Before implementing a phase:

Inspect:

- repository structure
- solution
- project files
- existing source files
- configuration
- tests
- Git status
- relevant architecture documentation

Do not assume the repository is in the state you expect.

---

# 49. PHASE BOUNDARY CHECK

Before writing code, explicitly determine:

```text
Current Phase:
Allowed:
Not Allowed:
```

Use this mentally as a scope boundary.

If a requested task belongs to a future phase, do not implement it.

---

# 50. DEFINITION OF DONE

A phase is NOT complete because code was written.

A phase is complete only when:

- requested functionality is implemented
- architecture boundaries are preserved
- build succeeds
- tests pass
- relevant warnings are addressed
- configuration is valid
- security considerations were addressed
- documentation is updated where required
- no unrelated changes were introduced
- final implementation report is complete

---

# 51. REQUIRED FINAL REPORT

At the end of EVERY phase, use exactly:

```text
## Implementation Summary

## Files Created

## Files Modified

## Architecture Decisions

## Database Changes

## API Changes

## Tests Added

## Test Results

## Build Result

## Known Issues

## Technical Debt

## Assumptions

## What Was NOT Implemented

## Recommended Next Step
```

Do not omit sections.

If a section has nothing to report, explicitly say:

```text
None.
```

---

# 52. FINAL REPORT MUST CONTAIN REAL RESULTS

Do not write:

```text
Tests should pass.
```

Write the actual result.

Do not write:

```text
Build should succeed.
```

Write the actual result.

The final report must distinguish between:

```text
Executed
```

and:

```text
Not executed
```

Never claim a command was run if it was not run.

---

# 53. NO SILENT FAILURES

If something could not be completed:

Report:

```text
What failed
Why it failed
What was attempted
What remains
```

Do not hide failures behind a successful-looking summary.

---

# 54. STOP CONDITION

After completing the requested phase:

STOP.

Do not automatically continue.

Do not implement the next phase.

Do not "finish the whole system."

Wait for human review and explicit approval.

---

# 55. HUMAN REVIEW GATE

Every phase follows:

```text
Phase Prompt
     ↓
Agent Implementation
     ↓
Build
     ↓
Tests
     ↓
Agent Report
     ↓
Human Review
     ↓
Fixes if required
     ↓
Approval
     ↓
Next Phase
```

No phase skips the review gate.

---

# 56. WHEN REVIEW FEEDBACK IS PROVIDED

If the human reviewer provides corrections:

1. understand each correction
2. modify only the affected scope
3. run relevant tests
4. rebuild
5. report exactly what changed
6. do not use the correction as permission to refactor unrelated areas

---

# 57. ARCHITECTURAL ESCALATION

If you discover a problem that requires a significant architectural decision, do not silently decide.

Examples:

- changing project dependency direction
- introducing a new infrastructure technology
- changing persistence strategy
- introducing a new external service
- changing authentication architecture
- changing the knowledge trust model
- changing offline behavior
- changing AI architecture

Report:

```text
Problem
Current constraint
Possible options
Impact
Recommended decision for human review
```

Then wait.

---

# 58. TECHNOLOGY DISCIPLINE

The approved core technology stack is:

```text
Language:
C#

Runtime:
.NET 10

Backend:
ASP.NET Core Web API

Database:
SQL Server

ORM:
EF Core

API Documentation:
OpenAPI / Swagger

Authentication:
ASP.NET Core Identity + JWT
(later phase)

Validation:
FluentValidation where justified

Logging:
Serilog where justified

Testing:
xUnit + FluentAssertions

Integration Testing:
ASP.NET Core WebApplicationFactory where appropriate

Version Control:
Git + GitHub
```

Do not introduce additional technologies without a reason.

Future technologies such as:

```text
Vector Database
Embedding Provider
LLM Provider
RAG
External Search
Document Processing
Local LLM
```

must be introduced only in their designated phases.

---

# 59. FRONTEND RULE

The backend/API is the primary foundation.

Do not create a frontend during backend-only phases.

The future UI must consume the API rather than bypassing the application architecture.

Do not add WPF, Blazor, React, Angular, or another frontend technology unless explicitly requested by the relevant phase.

---

# 60. AI PROVIDER INDEPENDENCE

Never tightly couple business logic to one AI provider.

Bad:

```text
Application
   ↓
OpenAI SDK
```

Preferred:

```text
Application
   ↓
IAiProvider
   ↓
Infrastructure
   ↓
Provider Implementation
```

This rule applies to:

- LLMs
- embedding providers
- vector stores
- external knowledge providers

---

# 61. DOCUMENT INGESTION RULE

Future document ingestion must be designed around replaceable components.

Potential pipeline:

```text
Document
 ↓
Validation
 ↓
Extraction
 ↓
Chunking
 ↓
Indexing
 ↓
Embedding
 ↓
Retrieval
```

Do not implement this pipeline before its designated phase.

Do not assume a single document format or provider.

---

# 62. SEARCH RULE

Search evolves incrementally.

Expected evolution:

```text
Phase 3:
Keyword / Full-text Search

Phase 7:
Semantic Search

Later:
Hybrid Search

Later:
RAG
```

Do not implement semantic/vector search prematurely.

Do not replace simple search with an unnecessarily complex system before the project needs it.

---

# 63. KNOWLEDGE TRUST RULE

The system must distinguish between knowledge states and source trust.

Never treat:

```text
AI Suggestion
```

as equivalent to:

```text
Verified Internal Knowledge
```

Never treat:

```text
External Documentation
```

as equivalent to:

```text
Company-Verified Procedure
```

Trust and provenance must remain visible in the architecture and eventually in the UI.

---

# 64. OFFLINE RULE

Offline capability is a product requirement, not an optional enhancement.

Never introduce an architecture where:

```text
No Internet
    ↓
No Knowledge
```

Core local knowledge must remain accessible without Internet.

External enrichment and cloud AI are optional.

---

# 65. ENGINEERING PHILOSOPHY

When choosing between two valid implementations, prefer the one that is:

1. simpler
2. easier to test
3. easier to understand
4. easier to replace
5. less coupled
6. less dependent on external services
7. aligned with the architecture
8. appropriate for the current phase

Do not optimize for "enterprise appearance."

Optimize for engineering quality.

---

# 66. THE GOLDEN RULE

Before adding anything, ask:

```text
Do we need this NOW?
```

If the answer is:

```text
No
```

do not add it.

Before removing something, ask:

```text
Was this created intentionally?
```

If uncertain:

```text
Inspect → Report → Ask
```

Before changing architecture, ask:

```text
Does this change affect future phases?
```

If yes:

```text
Report → Human Review → Approval
```

---

# 67. FINAL AGENT BEHAVIOR

The desired agent behavior is:

```text
Understand
   ↓
Inspect
   ↓
Plan
   ↓
Implement
   ↓
Test
   ↓
Review own changes
   ↓
Report
   ↓
STOP
```

Not:

```text
Understand
   ↓
Build everything
   ↓
Add random libraries
   ↓
Refactor everything
   ↓
Invent features
   ↓
Hope it works
```

The Engineering Knowledge Hub must grow deliberately, phase by phase, with human architectural review between phases.
