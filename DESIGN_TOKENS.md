إليك التصميم الكامل لـ Engineering Knowledge Hub، مبني على فلسفة بصرية تمزج بين العمق، الهدوء، والتطور التقني لتكون بالفعل "أداة متطورة فعلًا".

1. High-Fidelity Mockup (Visual Structure)
   Viewport: 1536px × 1024px | Theme: Deep Slate & Electric Cyan | Font: Geist & JetBrains Mono

Plaintext
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│ ✦ ENG HUB [ Ctrl + K ] Search knowledge, projects, or concepts... [A] ⚲ ⚙ │
├───────────────┬───────────────────────────────────────────────────────┬─────────────────────┤
│ ⌂ Home │ KNOWLEDGE VIEW │ RELATED CONTEXT │
│ ⎈ Knowledge ◄ │ [Backend] [C#] [Intermediate] │ │
│ ◫ Projects │ │ ⚯ Connections │
│ ✎ Notes │ # ASP.NET Core Middleware Architecture │ ├─ Dependency Inj. │
│ ⚯ Boards │ Updated 2h ago • ◷ 5 min read │ ├─ MongoDB.Driver │
│ │ │ └─ Entity Framework │
│ ▾ WORKSPACE │ Middleware is software that's assembled into an app │ │
│ ├ Employee │ pipeline to handle requests and responses. Each │ ▾ Referenced In │
│ │ Notes Mgr │ component chooses whether to pass the request to │ ├ Toll Collection │
│ ├ Suez Toll │ the next component in the pipeline. │ │ System UI │
│ │ System │ │ └ Auth Microservice │
│ └ API Docs │ ┌──────────────────────────────────────────────┐ │ │
│ │ │ csharp [Copy]│ │ ✦ Quick Actions │
│ ▾ GRAPH │ │ public class ErrorHandlingMiddleware │ │ [ + New Note ] │
│ ├ EF Core │ │ { │ │ [ ⎈ Add Resource ] │
│ ├ LINQ │ │ private readonly RequestDelegate \_next; │ │ │
│ └ MongoDB │ │ │ │ ◷ Activity │
│ │ │ public async Task Invoke(HttpContext ctx)│ │ You edited │
│ ✺ New Node │ │ { │ │ ErrorHandling.cs │
│ │ │ await \_next(ctx); │ │ 2 hours ago │
│ │ └──────────────────────────────────────────────┘ │ │
└───────────────┴───────────────────────────────────────────────────────┴─────────────────────┘
Visual details applied in this view:

Background: Deep #080B12 with a very subtle radial noise texture radiating from the center workspace.

Navigation: Floating left panel, separated by a 1px border (#273449). The active "Knowledge" state has an inner left luminous border (#5CE1E6) and a #202B3B background.

Center Workspace: Slightly elevated surface (#0D111A). The typography is crisp Geist, with JetBrains Mono for the code block.

Command Bar: Soft translucent glass (rgba(32, 43, 59, 0.6)) with a strong focus ring and #5CE1E6 text highlighting the Ctrl + K hint.

Graph Elements: The "Connections" on the right panel feature subtle glowing connection lines connecting to MongoDB.Driver and Entity Framework.

2. DESIGN_SYSTEM.md
   Design Philosophy
   The Engineering Knowledge Hub is a digital knowledge operating system. It is not a website; it is a cockpit. It leverages spatial hierarchy, layered surfaces, and typography to create a sense of deep focus. The futurism is derived from precision, intelligent context-awareness, and micro-interactions, avoiding cheap neon overload.

Visual Identity & Theming

Base: Deep, dimensional dark mode. No pure blacks (#000000). We use deep slate and graphite to reduce eye strain over 8-hour sessions.

Primary Accent: Electric Cyan (#5CE1E6). Used exclusively for interactive focus, active states, and primary knowledge connections.

Surfaces: 3 levels of elevation defined by lightness and extremely subtle inner borders, not heavy drop shadows.

Typography System

Primary UI Font: Geist (Clean, geometric, highly legible at small sizes).

Monospace Font: JetBrains Mono (Features ligatures, perfect for C++, C#, and JSON).

Hierarchy: Strict adherence to predefined T-shirt sizes. Avoid arbitrary pixel values.

Layout System

Application Shell:

Left Nav: 260px fixed width, floating above the background.

Command Header: 64px height, integrated directly into the workspace canvas.

Center Canvas: Fluid width, max-width 900px for text readability.

Right Context Panel: 320px width, contextual rendering.

Component Rules

Cards: Avoid standard cards. Use "Elevated Surfaces" with a 1px border of #273449 and a subtle top inner highlight (rgba(255, 255, 255, 0.04)) to simulate depth.

Buttons: Default to ghost or subtle elevated states. Solid accent colors are reserved ONLY for primary CTA (e.g., "Create Project").

Inputs: Darker than the surrounding surface (#080B12 inside #0D111A), creating a physical "indentation" feel.

Micro-Interactions & Motion

Hover: 120ms ease-out. Elevates surface lightness by 5%.

Focus: 180ms ease-in-out. Reveals a 2px solid Electric Cyan ring with a 4px translucent blur.

Expand (Tree): 240ms cubic-bezier(0.2, 0.8, 0.2, 1). Smooth mask-image reveal.

3. DESIGN_TOKENS.md
   Markdown

# COLOR TOKENS

## Surface

- color-surface-base: #080B12 (App Background)
- color-surface-elevated: #0D111A (Main Content Canvas)
- color-surface-floating: #141A24 (Panels, Dropdowns, Cards)
- color-surface-overlay: #192231 (Hover states, Modals)
- color-surface-input: rgba(8, 11, 18, 0.8)

## Borders

- color-border-subtle: #202B3B (Dividers)
- color-border-default: #273449 (Cards, Panels)
- color-border-focus: #5CE1E6 (Active Inputs)

## Typography

- color-text-primary: #F5F7FA (Headings, Body)
- color-text-secondary: #B8C2D1 (Subtitles, Metadata)
- color-text-muted: #778399 (Placeholders, Inactive)
- color-text-accent: #5CE1E6 (Links, Active states)

## Semantic / Accents

- color-accent-primary: #5CE1E6 (Electric Cyan)
- color-accent-secondary: #8B7CFF (Electric Violet - for Graph Nodes)
- color-status-success: #45E0A8
- color-status-warning: #FFB86C
- color-status-error: #FF5555

# TYPOGRAPHY TOKENS

- font-family-sans: 'Geist', sans-serif
- font-family-mono: 'JetBrains Mono', monospace

## Font Sizes & Line Heights

- text-display: size: 32px | line-height: 40px | weight: 600
- text-title: size: 24px | line-height: 32px | weight: 600
- text-section: size: 20px | line-height: 28px | weight: 500
- text-body-lg: size: 16px | line-height: 24px | weight: 400
- text-body: size: 14px | line-height: 22px | weight: 400
- text-secondary: size: 13px | line-height: 20px | weight: 400
- text-caption: size: 12px | line-height: 16px | weight: 500
- text-code: size: 13px | line-height: 20px | weight: 400

# SPACING TOKENS (Base 4px)

- space-xs: 4px
- space-sm: 8px
- space-md: 16px
- space-lg: 24px
- space-xl: 32px
- space-2xl: 48px
- space-3xl: 64px

# COMPONENT DIMENSIONS

## Heights

- height-input-sm: 32px
- height-input-md: 40px (Default)
- height-input-lg: 48px
- height-button-sm: 32px
- height-button-md: 40px
- height-button-lg: 48px
- height-header: 64px
- height-table-row: 36px

## Layout

- width-sidebar: 260px
- width-panel-right: 320px
- indent-tree: 16px

# RADII & SHADOWS

- radius-sm: 4px (Code blocks, tags)
- radius-md: 8px (Buttons, inputs, compact tiles)
- radius-lg: 12px (Cards, panels, floating menus)
- radius-full: 99px (Pills, avatars)

- shadow-subtle: 0 4px 12px rgba(0, 0, 0, 0.2)
- shadow-floating: 0 8px 32px rgba(0, 0, 0, 0.4), 0 0 0 1px #273449
- shadow-glow: 0 0 12px rgba(92, 225, 230, 0.2)

# MOTION TOKENS

- duration-fast: 120ms
- duration-normal: 180ms
- duration-emphasis: 240ms
- ease-default: cubic-bezier(0.4, 0, 0.2, 1)
- ease-spring: cubic-bezier(0.175, 0.885, 0.32, 1.275)

4. AGENT IMPLEMENTATION REFERENCE
   Provide these strict key-value rules to the AI/Frontend engineer implementing the code.

1. CSS Variables Setup:

Map all variables to the exact hex codes above. Do NOT use Tailwind default blues or grays. Generate a custom preset.

Example: --color-primary: #5CE1E6; --bg-base: #080B12;

2. Focus & State Management:

Disabled State: opacity: 0.5; pointer-events: none; filter: grayscale(100%);

Hover State (Buttons/Cards): background-color: #192231; transform: translateY(-1px);

Focus Ring: outline: 2px solid #5CE1E6; outline-offset: 2px; (Never use outline: none without replacing it).

3. Component Specifics:

Command Palette (Ctrl + K): Must use shadow-floating (0 8px 32px rgba(0, 0, 0, 0.4), 0 0 0 1px #273449) and a backdrop filter of blur(12px). Background must be rgba(20, 26, 36, 0.7).

Tree Navigation: Active items get a left border: border-left: 2px solid #5CE1E6. Indentation is strictly padding-left: calc(16px \* var(--depth)).

Code Blocks: Background #080B12, Border 1px solid #202B3B, Font JetBrains Mono, 13px. Include a 1px inner top border of rgba(255,255,255,0.05) for the premium depth effect.

4. Iconography:

Use lucide-react or SVG equivalent.

Default icon size: 18px.

Stroke width: 1.5px (do not use 2px, it looks too heavy/childish for a futuristic app).

Icon color default: #B8C2D1. Hover: #F5F7FA. Active: #5CE1E6.
