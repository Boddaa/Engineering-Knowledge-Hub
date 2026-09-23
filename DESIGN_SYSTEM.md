# Engineering Knowledge Hub

## Design System

**Document Status:** Approved Design Foundation
**Document Type:** UI/UX and Visual Design Contract
**Applies To:** Entire Engineering Knowledge Hub
**Authority:** Mandatory for all UI implementation
**Version:** 1.0
**Last Updated:** 2026-09-23

---

# 1. Purpose

This document defines the visual language, interaction patterns, component standards, layout rules, accessibility requirements, and UI architecture for the Engineering Knowledge Hub.

The purpose of this Design System is to ensure that:

- All features share one consistent visual language.
- New features do not introduce unrelated visual patterns.
- UI implementation remains predictable across the application.
- Components are reusable instead of duplicated.
- Design decisions remain stable as the application grows.
- Future features can be implemented without redesigning existing screens.
- UI/UX changes are intentional and centralized.
- Accessibility is treated as a first-class requirement.
- The application remains visually coherent across the entire product lifecycle.

This document is a **design contract**, not optional guidance.

---

# 2. Design Principles

The Engineering Knowledge Hub follows these principles.

## 2.1 Clarity Over Decoration

Every visual element must have a purpose.

Do not add:

- Decorative panels without meaningful content.
- Excessive gradients.
- Unnecessary animations.
- Visual effects that reduce readability.
- Icons that do not communicate useful information.

The interface should feel engineered, not decorated.

---

## 2.2 Information Density Without Visual Noise

The application is intended for engineering and technical knowledge.

It may contain:

- Documentation
- Code
- Projects
- Notes
- Trees
- Metadata
- Search results
- Tags
- Relationships
- Technical references

Therefore the UI must support relatively high information density while maintaining clear visual hierarchy.

Dense does not mean cluttered.

---

## 2.3 Consistency Over Local Optimization

A feature must not optimize its own appearance at the expense of application-wide consistency.

If an existing component or pattern can satisfy a requirement:

> Reuse it.

Do not create a visually different alternative simply because it is easier to implement locally.

---

## 2.4 Progressive Disclosure

Do not show every piece of information simultaneously.

Prefer:

```text
Summary
   ↓
Relevant Details
   ↓
Advanced Information
```

Complex technical information should be revealed progressively when appropriate.

---

## 2.5 User Context First

The interface should make it obvious:

- Where the user is.
- What they are looking at.
- What they can do.
- What changed.
- What requires attention.
- How to recover from an error.

The user should not have to reverse-engineer the interface.

---

## 2.6 Accessibility by Default

Accessibility is not a final polishing phase.

All UI components must consider:

- Readability
- Contrast
- Keyboard navigation
- Focus visibility
- Hit targets
- Semantic meaning
- Error communication
- Color independence
- Disabled states
- Loading states

---

# 3. Visual Identity

The Engineering Knowledge Hub should communicate:

- Engineering
- Precision
- Knowledge
- Organization
- Reliability
- Modern software tooling

The visual style should be:

**Modern + Technical + Calm + Structured + Professional**

Avoid:

- Gaming UI aesthetics.
- Excessive glassmorphism.
- Excessive neon.
- Excessive gradients.
- Overly corporate enterprise styling.
- Heavy skeuomorphism.
- Decorative dashboards that prioritize appearance over information.

The interface should feel like a serious engineering workspace.

---

# 4. Theme Strategy

The application should support a centralized theme architecture.

All visual values must be defined through centralized design tokens.

Components must not hard-code arbitrary colors throughout their implementation.

Preferred architecture:

```text
Design Tokens
     ↓
Theme Resources
     ↓
Shared Components
     ↓
Feature Components
     ↓
Pages
```

A theme change should be possible primarily through centralized resources rather than modifying individual pages.

---

# 5. Color System

## 5.1 Color Roles

Colors are defined by semantic role rather than by component.

Use semantic names such as:

```text
Background
Surface
SurfaceElevated
Border
TextPrimary
TextSecondary
TextMuted
Accent
AccentHover
Success
Warning
Error
Info
```

Do not name colors according to their visual appearance when a semantic role exists.

Bad:

```text
DarkBlue2
GreenButton
LightGrayText
```

Preferred:

```text
Surface
Accent
TextSecondary
```

---

## 5.2 Primary Palette

The initial visual foundation uses a dark technical theme.

### Background

```text
Background.Primary
Background.Secondary
Background.Tertiary
```

Recommended base:

```text
Background.Primary   = #0F172A
Background.Secondary = #111827
Background.Tertiary  = #1E293B
```

---

## 5.3 Surface Palette

```text
Surface.Default
Surface.Elevated
Surface.Interactive
Surface.Selected
```

Recommended foundation:

```text
Surface.Default     = #1E293B
Surface.Elevated    = #243247
Surface.Interactive = #29384D
Surface.Selected    = #334155
```

---

## 5.4 Border Palette

Borders should be subtle.

```text
Border.Default
Border.Strong
Border.Focus
```

Recommended:

```text
Border.Default = #334155
Border.Strong  = #475569
```

Focus borders may use the accent color.

---

## 5.5 Text Palette

```text
Text.Primary
Text.Secondary
Text.Muted
Text.Disabled
Text.OnAccent
```

Recommended:

```text
Text.Primary   = #F8FAFC
Text.Secondary = #CBD5E1
Text.Muted     = #94A3B8
Text.Disabled  = #64748B
```

Primary text must remain clearly readable against the surface it appears on.

---

# 6. Semantic Colors

Semantic colors communicate meaning.

## Success

Used for:

- Successful operations
- Completed states
- Positive confirmations
- Healthy system states

```text
Success = #22C55E
```

## Warning

Used for:

- Attention required
- Non-critical warnings
- Pending conditions

```text
Warning = #F59E0B
```

## Error

Used for:

- Failed operations
- Validation errors
- Destructive consequences
- System failures

```text
Error = #EF4444
```

## Info

Used for:

- Informational messages
- Neutral system information
- Helpful hints

```text
Info = #3B82F6
```

Semantic colors must not be used purely for decoration.

---

# 7. Accent Color

The application should have one primary accent color.

Recommended:

```text
Accent = #38BDF8
```

Use accent for:

- Primary actions
- Active navigation
- Links
- Focus indicators
- Important interactive elements
- Selected states

Do not use the accent color everywhere.

Accent must retain visual meaning.

---

# 8. Typography

Typography must establish a clear hierarchy.

Recommended font family:

```text
Primary:
Inter

Fallback:
Segoe UI
System UI
Sans-serif
```

If the platform environment does not provide Inter, use the fallback stack.

---

## 8.1 Type Scale

Recommended baseline:

```text
Display        32 px
Heading 1      28 px
Heading 2      24 px
Heading 3      20 px
Heading 4      18 px

Body Large     16 px
Body           14 px
Body Small     13 px

Caption        12 px
```

The exact implementation may adapt slightly to the target platform.

---

## 8.2 Font Weights

```text
Regular     400
Medium      500
SemiBold    600
Bold        700
```

Preferred usage:

```text
Page title       600
Section heading  600
Body text        400
Labels           500
Primary buttons  600
```

Avoid using bold text everywhere.

---

# 9. Spacing System

Use a consistent spacing scale.

Base unit:

```text
4 px
```

Allowed spacing values:

```text
4
8
12
16
20
24
32
40
48
64
```

Avoid arbitrary values unless technically required.

Preferred:

```text
Padding = 16
Gap     = 12
Margin  = 24
```

Avoid:

```text
Padding = 13
Gap     = 17
Margin  = 27
```

unless there is a documented reason.

---

# 10. Layout System

The UI should use predictable layout rules.

Preferred structure:

```text
Application Shell
    ↓
Page Container
    ↓
Page Header
    ↓
Toolbar / Actions
    ↓
Main Content
```

---

## 10.1 Page Padding

Default page padding:

```text
24 px
```

Large desktop layouts may use:

```text
32 px
```

---

## 10.2 Content Width

Content should not become excessively wide when readability suffers.

Use reasonable maximum widths for:

- Documentation
- Forms
- Reading-heavy content

Wide layouts are acceptable for:

- Tables
- Boards
- Trees
- Dashboards
- Graphs
- Technical visualization

---

# 11. Border Radius

Use a small and consistent radius system.

```text
Radius.Small  = 4 px
Radius.Medium = 8 px
Radius.Large  = 12 px
Radius.Pill   = 999 px
```

Default:

```text
Cards      = 8 px
Inputs     = 6-8 px
Buttons    = 6-8 px
Badges     = Pill
Dialogs    = 12 px
```

Do not mix many unrelated corner radii.

---

# 12. Elevation and Shadows

Use elevation sparingly.

The application should not depend on heavy shadows to separate content.

Preferred hierarchy:

```text
Background
    ↓
Surface
    ↓
Elevated Surface
    ↓
Modal / Dialog
```

Borders may be preferred over shadows for technical UI.

---

# 13. Iconography

Use one consistent icon family.

Recommended:

```text
Lucide Icons
```

Icons must:

- Have consistent visual weight.
- Communicate meaning.
- Use tooltips when meaning is not obvious.
- Not replace text when the action would otherwise be ambiguous.

Avoid mixing unrelated icon libraries.

---

## 13.1 Icon Sizes

Recommended:

```text
Small       16 px
Default     20 px
Large       24 px
Hero        32 px+
```

---

# 14. Buttons

Buttons must communicate action hierarchy.

## Primary

Used for:

- Main page action
- Save
- Create
- Confirm

Only one primary action should normally dominate a given context.

---

## Secondary

Used for:

- Alternative actions
- Cancel
- Additional workflows

---

## Tertiary / Ghost

Used for:

- Low-emphasis actions
- Toolbar actions
- Navigation-related actions

---

## Destructive

Used for:

- Delete
- Permanent removal
- Irreversible actions

Destructive actions must be visually distinguishable and should require confirmation when consequences are significant.

---

# 15. Button Rules

Buttons should:

- Have clear labels.
- Use verbs when possible.
- Maintain consistent height.
- Show hover state.
- Show pressed state.
- Show disabled state.
- Show focus state.
- Show loading state when the operation is asynchronous.

Preferred:

```text
Create Project
Save Changes
Delete Note
```

Avoid ambiguous labels:

```text
OK
Do It
Proceed
```

when a more descriptive action is possible.

---

# 16. Forms

Forms should prioritize clarity and error prevention.

Structure:

```text
Field Label
Helper Text (optional)
Input
Validation Message
```

Labels must not depend exclusively on placeholder text.

Placeholder text is supplementary information, not the field label.

---

## 16.1 Validation

Validation should:

- Be close to the invalid field.
- Explain the problem.
- Provide actionable guidance.
- Not rely only on color.

Bad:

```text
Input turns red.
```

Preferred:

```text
Project name is required.
```

---

# 17. Inputs

All inputs must have:

```text
Default
Hover
Focus
Disabled
Error
Read-only
```

where applicable.

Focus state must be clearly visible.

---

# 18. Search

Search is a first-class interaction within the Knowledge Hub.

Search components should support:

- Clear visual affordance.
- Keyboard access.
- Clear button when text exists.
- Loading state.
- Empty results state.
- Error state.
- Result highlighting where appropriate.

Search should not require unnecessary navigation.

---

# 19. Cards

Cards should group related information.

Use cards when:

- Content forms a meaningful unit.
- Visual separation improves scanning.
- The content has independent actions.

Do not wrap every small piece of content in a card.

Over-cardization creates visual noise.

---

# 20. Tables

Tables should be used when comparing structured records.

Rules:

- Clear column headers.
- Consistent alignment.
- Reasonable row height.
- Hover state where useful.
- Selected state where applicable.
- Empty state.
- Loading state.
- Error state.
- Pagination when necessary.
- Sorting/filtering only when useful.

Avoid extremely dense tables without a clear need.

---

# 21. Lists

Lists should provide:

- Clear hierarchy.
- Consistent spacing.
- Predictable interaction.
- Selection state where applicable.

Use lists instead of cards when the user primarily needs scanning rather than visual grouping.

---

# 22. Navigation

The navigation system must clearly communicate location.

Recommended structure:

```text
Application
├── Dashboard
├── Knowledge
├── Projects
├── Notes
├── Boards
├── Search
└── Settings
```

Actual navigation entries must follow the application's domain model and should not be invented without architectural justification.

---

## 22.1 Active Navigation

Active navigation must be distinguishable using more than color alone.

Possible indicators:

- Background
- Accent indicator
- Icon treatment
- Typography

---

# 23. Breadcrumbs

Breadcrumbs should be used where hierarchical navigation is meaningful.

Example:

```text
Knowledge
  /
Backend
  /
ASP.NET Core
  /
Authentication
```

Do not use breadcrumbs when the hierarchy provides no useful navigation value.

---

# 24. Page Header

Standard page header:

```text
Title
Description
Actions
```

Example:

```text
Projects

Manage engineering projects and their technical knowledge.

[Create Project]
```

Page headers must remain consistent across the application.

---

# 25. Standard Page Patterns

The following patterns are mandatory reusable patterns.

## 25.1 List Page

```text
Page Header
    ↓
Toolbar
    ↓
Filters / Search
    ↓
Content List / Table
    ↓
Pagination
```

---

## 25.2 Details Page

```text
Breadcrumb
    ↓
Page Header
    ↓
Summary
    ↓
Sections
    ↓
Related Content
```

---

## 25.3 Create / Edit Page

```text
Page Header
    ↓
Form Sections
    ↓
Validation
    ↓
Action Bar
```

---

## 25.4 Dashboard

```text
Page Header
    ↓
Summary Metrics
    ↓
Primary Information
    ↓
Secondary Information
```

Dashboard widgets must represent useful information.

Avoid decorative KPI cards that provide no actionable or meaningful value.

---

# 26. Empty States

Empty states are part of the design, not exceptions.

Every collection-based feature should define an empty state.

Example structure:

```text
[Icon]

No projects yet

Create your first project to start organizing
engineering knowledge.

[Create Project]
```

Empty states should explain:

1. What is empty.
2. Why it matters.
3. What the user can do next.

---

# 27. Loading States

Loading states must be intentional.

Use:

- Progress indicators.
- Skeletons where appropriate.
- Disabled action states when necessary.

Avoid blocking the entire interface for small localized operations.

Prefer localized loading:

```text
Page remains available
+
Specific component shows loading
```

when technically possible.

---

# 28. Error States

Errors must be understandable and recoverable.

Error UI should communicate:

```text
What happened
+
What the user can do
```

Example:

```text
Unable to load projects.

Check your connection and try again.

[Retry]
```

Do not expose raw technical exceptions directly to end users.

Technical details may be available through logs or an advanced diagnostic mechanism where appropriate.

---

# 29. Notifications

Notifications should communicate:

- Success
- Warning
- Error
- Information

Use the least disruptive mechanism appropriate.

Preferred hierarchy:

```text
Inline feedback
    ↓
Toast
    ↓
Dialog
```

Do not use dialogs for information that does not require user attention.

---

# 30. Dialogs

Dialogs should be reserved for:

- Important confirmation.
- Focused tasks.
- Destructive actions.
- Short workflows that should not require leaving the current context.

Dialogs must:

- Have a clear title.
- Have a clear primary action.
- Have a clear cancellation path.
- Support keyboard interaction.
- Trap focus appropriately where required.

Avoid deeply nested dialogs.

---

# 31. Confirmation Rules

Confirmation is required when an action is:

- Destructive.
- Irreversible.
- Potentially data-loss causing.
- Security-sensitive.

Do not require confirmation for every trivial action.

Excessive confirmations create confirmation fatigue.

---

# 32. Code and Technical Content

Engineering Knowledge Hub is a technical product.

Code blocks must be visually distinct from normal text.

Recommended requirements:

- Monospace font.
- Clear syntax presentation.
- Copy action.
- Horizontal scrolling when required.
- Line numbers only when useful.
- Language indicator where relevant.

Example structure:

```text
┌──────────────────────────────────────────┐
│ C#                              [Copy]   │
├──────────────────────────────────────────┤
│ public class Example                     │
│ {                                        │
│     ...                                  │
│ }                                        │
└──────────────────────────────────────────┘
```

Code must never be rendered as ordinary paragraph text when the user needs to read it as code.

---

# 33. Documentation / Knowledge Content

Long-form technical content should optimize for reading.

Recommended:

```text
Title
Metadata
Table of Contents / Outline
Content
Related Knowledge
```

Reading surfaces should generally use a narrower content width than dashboards and tables.

---

# 34. Tree Views

Knowledge trees and hierarchical structures must provide:

- Clear indentation.
- Expand/collapse affordance.
- Selected state.
- Hover state.
- Keyboard navigation.
- Context actions where appropriate.

Tree depth must remain visually understandable.

---

# 35. Boards / Canvas

Boards and mind maps may use a more flexible spatial layout than standard pages.

However:

- Node styling must remain consistent.
- Colors must have semantic meaning.
- Selection must be obvious.
- Interaction states must be predictable.
- Zoom controls must be discoverable.
- Pan and zoom must not interfere with ordinary selection.

---

# 36. Accessibility

Accessibility requirements apply to every feature.

## 36.1 Contrast

Text and important UI elements must maintain sufficient contrast.

Do not use low-contrast text merely because it looks visually subtle.

---

## 36.2 Color Independence

Do not communicate important information through color alone.

Bad:

```text
Green = completed
Red = failed
```

Preferred:

```text
[✓] Completed
[!] Failed
```

with color as an additional visual signal.

---

## 36.3 Keyboard Navigation

Interactive elements must support logical keyboard navigation.

Focus order must follow the visual and semantic structure.

---

## 36.4 Focus

Focus must always be visible.

Do not remove focus indicators without replacing them with an equally clear mechanism.

---

## 36.5 Hit Targets

Interactive controls must provide sufficiently large clickable areas.

Small icons should generally have a larger interactive container than the visible icon itself.

---

# 37. Responsive / Adaptive Design

The UI must adapt to the target application environment.

Do not assume that every screen has unlimited space.

Components should define behavior for:

```text
Large
Medium
Small
```

where applicable.

Prioritize:

1. Content visibility
2. Usability
3. Navigation
4. Secondary decoration

When space becomes limited, remove or collapse secondary information before compromising core functionality.

---

# 38. Animation

Animation must communicate state or relationship.

Good uses:

- Page transitions.
- Expand/collapse.
- Loading.
- Selection.
- Dialog appearance.
- State changes.

Avoid:

- Constant movement.
- Decorative animations.
- Long transitions.
- Animations that delay user interaction.

Default animations should be short and subtle.

---

# 39. Component Architecture

UI components must follow this hierarchy:

```text
Design Tokens
        ↓
Primitives
        ↓
Shared Components
        ↓
Feature Components
        ↓
Pages
```

---

## 39.1 Primitives

Examples:

```text
Button
Text
Icon
Input
Checkbox
Radio
Select
Divider
Tooltip
```

---

## 39.2 Shared Components

Examples:

```text
PageHeader
SearchBox
FilterBar
DataTable
EmptyState
ErrorState
LoadingState
Dialog
Toast
Breadcrumbs
```

---

## 39.3 Feature Components

Examples:

```text
KnowledgeTree
NoteCard
ProjectCard
DocumentViewer
MindMapNode
```

Feature components must remain inside the appropriate feature boundary unless there is a genuine reason to promote them into shared components.

---

# 40. Component Reuse Rules

Before creating a new UI component, the developer or Agent must answer:

1. Does an existing component already solve this problem?
2. Can an existing component be configured or composed?
3. Is the new component genuinely reusable?
4. Does the new component belong to the Design System or a specific feature?
5. Will introducing it create visual duplication?

If an existing component satisfies the requirement:

> Reuse it.

---

# 41. Design Tokens

All reusable visual values should be represented through tokens.

Categories:

```text
Colors
Typography
Spacing
Radius
Borders
Elevation
Icon Sizes
Control Heights
Animation
```

Example:

```text
Color.Text.Primary
Color.Surface.Default
Spacing.MD
Radius.MD
Typography.Body
```

Exact implementation depends on the UI framework.

---

# 42. Hard-Coded UI Values

Avoid scattered hard-coded values such as:

```text
Margin="17"
Color="#27384A"
FontSize="15"
```

when an appropriate design token exists.

Hard-coded values may be used when:

- Required for a unique visual component.
- Required by a third-party control.
- Required by a platform-specific behavior.

Such cases should remain local and intentional.

---

# 43. Design Drift Prevention

Design drift occurs when features slowly begin using different:

- Colors
- Spacing
- Typography
- Buttons
- Cards
- Navigation
- Dialogs
- Interaction patterns

This is prohibited.

If a new feature requires a new visual pattern:

```text
Feature Requirement
       ↓
Check Existing Pattern
       ↓
Existing Pattern?
   ├── YES → Reuse
   └── NO
        ↓
Evaluate New Pattern
        ↓
Add to Design System if reusable
        ↓
Implement Feature
```

---

# 44. New Component Approval Rule

A new shared component should only be introduced when:

- Existing components cannot satisfy the requirement.
- The pattern is expected to recur.
- The abstraction is understandable.
- The component has clear ownership.
- The component does not create unnecessary complexity.

Do not build a component merely to demonstrate abstraction.

---

# 45. Feature Design Process

Every major feature should follow:

```text
Requirement
    ↓
UX Flow
    ↓
Existing Pattern Analysis
    ↓
Component Selection
    ↓
Design
    ↓
Implementation
    ↓
Verification
```

Before implementation, the Agent must determine whether existing components and page patterns can be reused.

---

# 46. Design Before Implementation

For significant UI work, implementation should not begin with arbitrary visual decisions.

The Agent should first establish:

- Page purpose.
- User flow.
- Information hierarchy.
- Main actions.
- Required states.
- Reusable components.
- Empty state.
- Loading state.
- Error state.
- Accessibility requirements.

Then implementation may begin.

---

# 47. Required UI States

Interactive components should consider the states relevant to their behavior:

```text
Default
Hover
Pressed
Focused
Selected
Disabled
Loading
Error
Success
Empty
```

Not every component requires every state.

Only applicable states should be implemented.

---

# 48. UX Consistency Rules

Equivalent actions must look and behave consistently.

Examples:

If "Save" is a primary action on one form, it should not become a tertiary action on another equivalent form without a documented reason.

If deletion requires confirmation in one feature, equivalent destructive operations should follow the same principle.

---

# 49. Terminology

UI terminology must remain consistent.

Do not alternate between:

```text
Create
Add
New
```

for the same conceptual action without a reason.

Choose terminology based on the domain and use it consistently.

The same applies to:

```text
Delete / Remove
Project / Workspace
Document / Article
Note / Memo
```

Domain terminology must be established centrally and reused throughout the application.

---

# 50. Localization

The design system must not assume that all text has a fixed length.

UI layouts should tolerate:

- Longer labels.
- Different languages.
- Different font metrics.

Avoid fixed widths for text-heavy controls when possible.

---

# 51. Internationalization

Text should not be embedded directly into reusable UI components when localization is expected.

Prefer:

```text
Resources
Localization Service
Resource Keys
```

rather than hard-coded user-facing strings inside reusable components.

---

# 52. User Feedback Principle

Every meaningful user action should produce an understandable result.

Examples:

```text
Save
 ↓
Saving...
 ↓
Saved
```

or:

```text
Delete
 ↓
Confirmation
 ↓
Deleted
```

The user should not be left wondering whether an operation happened.

---

# 53. Destructive Operations

Destructive actions must be visually and behaviorally consistent.

Examples:

```text
Delete
Remove
Clear
Reset
Discard
```

Potentially destructive operations should:

- Be visually distinguishable.
- Clearly describe the consequence.
- Avoid accidental activation.
- Provide confirmation when appropriate.

---

# 54. Performance-Aware UI

Visual quality must not come at the expense of application performance.

Avoid unnecessary:

- Animations.
- Heavy shadows.
- Excessive transparency.
- Large visual trees.
- Re-rendering.
- Complex effects.

For large knowledge trees, lists, documents, or boards, virtualization and incremental rendering should be considered where supported by the framework.

---

# 55. UI Architecture Boundaries

The UI layer must not become a place where domain logic is hidden.

Visual components should handle:

```text
Presentation
Interaction
State Representation
```

They should not contain business rules that belong to:

```text
Domain
Application
Infrastructure
```

The UI must respect `ARCHITECTURE.md`.

---

# 56. Separation of Concerns

Preferred:

```text
UI
 ↓
Presentation Logic
 ↓
Application Layer
 ↓
Domain
```

Avoid:

```text
UI
 ↓
Database
```

Avoid putting:

- EF Core queries
- Domain rules
- Business calculations
- Infrastructure concerns

directly into visual components.

---

# 57. Design System Governance

`DESIGN_SYSTEM.md` is a living document.

However, changes must be intentional.

When modifying the Design System:

1. Identify the reason.
2. Identify affected components.
3. Identify affected features.
4. Evaluate backward compatibility.
5. Update the relevant tokens/patterns.
6. Verify existing screens.
7. Document the change.

Do not modify global design tokens casually.

---

# 58. Agent Rules

The AI Agent implementing the project MUST follow these rules.

## Rule 1

Do not invent a new visual language for a feature.

## Rule 2

Reuse existing Design System components whenever possible.

## Rule 3

Do not introduce arbitrary colors.

## Rule 4

Do not introduce arbitrary spacing values when a design token exists.

## Rule 5

Do not duplicate existing components.

## Rule 6

Do not create feature-specific versions of shared components without justification.

## Rule 7

Do not redesign the Application Shell during feature implementation.

## Rule 8

Do not modify global tokens to solve a local component problem.

## Rule 9

Do not introduce a new icon library when an approved icon library exists.

## Rule 10

Do not bypass accessibility requirements for convenience.

## Rule 11

Do not implement only the happy path.

Relevant loading, empty, error, disabled, and success states must be considered.

## Rule 12

Do not move business logic into UI components.

## Rule 13

Do not introduce UI patterns from external templates without adapting them to this Design System.

## Rule 14

Do not perform broad visual refactoring while implementing an unrelated feature.

## Rule 15

If a requirement conflicts with this Design System, stop and report the conflict instead of silently creating an exception.

---

# 59. Feature Implementation Checklist

Before considering a UI feature complete, verify:

### Design

- [ ] Existing patterns were checked.
- [ ] Existing components were reused where possible.
- [ ] No arbitrary visual values were introduced.
- [ ] Typography follows the system.
- [ ] Spacing follows the system.
- [ ] Colors follow semantic tokens.
- [ ] Icons follow the approved icon system.

### UX

- [ ] Main user flow is clear.
- [ ] Primary action is obvious.
- [ ] Destructive actions are distinguishable.
- [ ] Empty state exists where relevant.
- [ ] Loading state exists where relevant.
- [ ] Error state exists where relevant.
- [ ] Success feedback exists where relevant.

### Accessibility

- [ ] Contrast is acceptable.
- [ ] Color is not the only information carrier.
- [ ] Keyboard navigation works where applicable.
- [ ] Focus state is visible.
- [ ] Interactive targets are usable.
- [ ] Labels are meaningful.

### Architecture

- [ ] UI does not contain business logic.
- [ ] UI respects `ARCHITECTURE.md`.
- [ ] Shared components remain reusable.
- [ ] Feature-specific code stays within its feature boundary.

---

# 60. Definition of Design Done

A feature is not considered visually complete merely because:

> "The page looks good."

A feature is Design Done when:

```text
Visual consistency
        +
UX consistency
        +
Component reuse
        +
Accessibility
        +
Required states
        +
Architecture compliance
        +
No unnecessary design drift
```

are all satisfied.

---

# 61. Final Design Philosophy

Engineering Knowledge Hub should feel like a coherent product rather than a collection of independently designed screens.

The user should be able to move from:

```text
Dashboard
    ↓
Knowledge
    ↓
Project
    ↓
Document
    ↓
Note
    ↓
Board
```

and feel that every part belongs to the same system.

The visual language should remain stable while the content and capabilities evolve.

The goal is not to make every screen identical.

The goal is to make every screen feel like it belongs to the same product.

---

# 62. Design System Golden Rules

These rules summarize the entire document:

> **1. Reuse before creating.**

> **2. Consistency before local visual optimization.**

> **3. Semantic tokens before hard-coded values.**

> **4. Accessibility from the beginning, not at the end.**

> **5. Every feature must support its relevant UI states.**

> **6. New patterns require justification.**

> **7. Global design changes must be intentional.**

> **8. UI must respect architectural boundaries.**

> **9. Design decisions must scale with the product.**

> **10. No feature is allowed to create visual debt for future phases.**

---

# 63. Relationship With Other Project Documents

The Design System works together with the project's other foundational documents.

```text
ARCHITECTURE.md
      │
      ├── Defines system structure
      │
      ↓
AGENT_RULES.md
      │
      ├── Defines implementation behavior
      │
      ↓
DESIGN_SYSTEM.md
      │
      ├── Defines visual and interaction behavior
      │
      ↓
FEATURE IMPLEMENTATION
```

No document should be interpreted in isolation.

If a feature introduces a conflict between architecture, implementation rules, and visual design:

1. Stop implementation.
2. Identify the conflict.
3. Report it.
4. Resolve the rule/design conflict explicitly.
5. Continue only after the decision is clear.

---

# 64. Non-Negotiable Principle

The Engineering Knowledge Hub must not accumulate design debt in the same way software projects accumulate technical debt.

Every new feature must strengthen the existing system rather than quietly creating another system inside it.

**Build the product as one system.**
**Do not build a collection of screens.**
