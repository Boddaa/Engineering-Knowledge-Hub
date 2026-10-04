# Engineering Knowledge Hub — Frontend (Phase F0)

This directory contains the React + TypeScript + Vite frontend client for the **Engineering Knowledge Hub**.

## Architecture & Boundary

```text
Frontend (React 19 + TypeScript + Vite)
   │
   ├── Presentation & Pages (/src/pages)
   │     ├── HomePage (Engineering Cockpit)
   │     ├── KnowledgePage (Repository Workspace)
   │     ├── ProjectsPage, NotesPage, BoardsPage
   │     └── NotFoundPage (404)
   │
   ├── Application Shell (/src/components/layout)
   │     ├── CommandHeader (64px, search shortcut trigger, user status)
   │     ├── Sidebar (260px, primary nav, workspaces, graph nodes)
   │     ├── ContextPanel (320px, inspector, connections, activity)
   │     ├── PageShell (Reusable title, description, actions, loading, empty, error)
   │     └── AppLayout (Shell orchestrator, Ctrl+K modal handler)
   │
   ├── Design System Primitives (/src/components/ui)
   │     └── Button, IconButton, Input, Badge, Avatar, Divider, Panel, Spinner, Skeleton, EmptyState, ErrorState
   │
   ├── API Client Boundary (/src/services/api)
   │     ├── apiConfig.ts (Base URL, headers, timeout)
   │     ├── apiErrors.ts (ApiError class, RFC 7807 ProblemDetails)
   │     └── apiClient.ts (Typed HTTP methods, timeout handling)
   │
   └── Design Tokens & Styles (/src/styles)
         ├── tokens.css (Hex-exact surfaces, borders, typography, status colors, 4px grid)
         ├── typography.css (Geist UI hierarchy & JetBrains Mono technical scale)
         └── globals.css (Base reset, scrollbars, focus ring with Electric Cyan)
```

## Design System Compliance

All styling strictly adheres to `DESIGN_SYSTEM.md` and `DESIGN_TOKENS.md`:
* **Surfaces**:
  * `--color-surface-base`: `#080B12`
  * `--color-surface-primary`: `#0D111A`
  * `--color-surface-elevated`: `#141A24`
  * `--color-surface-overlay`: `#192231`
* **Borders**:
  * `--color-border-default`: `#202B3B`
  * `--color-border-hover`: `#273449`
  * `--color-border-focus`: `#5CE1E6`
* **Typography**:
  * `--color-text-primary`: `#F5F7FA`
  * `--color-text-secondary`: `#B8C2D1`
  * `--color-text-muted`: `#778399`
  * `--color-brand-cyan`: `#5CE1E6`
  * Fonts: `Geist` (Sans-serif) and `JetBrains Mono` (Monospace)
* **Semantic Colors**:
  * Cyan: `#5CE1E6`
  * Violet: `#8B7CFF`
  * Success: `#45E0A8`
  * Warning: `#FFB86C`
  * Error: `#FF5555`

## Development Setup

### Prerequisites
* Node.js (v18+)
* npm

### Running Locally
```bash
# 1. Install dependencies
npm install

# 2. Run the Vite development server
npm run dev

# 3. Build for production
npm run build

# 4. Check linting / TypeScript compilation
npm run lint
```

## API Configuration
Environment variable `VITE_API_BASE_URL` configures the backend REST API endpoint.
Default development target: `http://localhost:5245/api` (reverse-proxied via Vite at `/api`).
Mock data is strictly quarantined inside `src/lib/mock/` for shell presentation and is never mixed into `src/services/api/`.
