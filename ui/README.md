# SentinelVault UI

A modern React-based web interface for the SentinelVault database backup system with AI-powered insights.

## Features

- **Modern Dashboard**: Clean, responsive interface for managing database backups
- **Profile Management**: Create, edit, and delete backup profiles with full CRUD operations
- **Backup Operations**: Run full and incremental backups with real-time status updates
- **AI Explanations**: Get AI-powered insights and explanations for your backup operations
- **Responsive Design**: Works seamlessly across desktop and mobile devices
- **Real-time Updates**: Live status updates with toast notifications

## Tech Stack

- **React 18** with TypeScript for type safety
- **Vite** for fast development and building
- **TailwindCSS** for modern, responsive styling
- **React Router v6** for client-side routing
- **TanStack React Query** for efficient data fetching and caching
- **Axios** for HTTP requests
- **React Markdown** for rendering AI explanations
- **React Hot Toast** for notifications
- **Lucide React** for beautiful icons

## Prerequisites

- Node.js 18+ 
- npm or pnpm package manager
- SentinelVault backend API running (default: http://localhost:5000)

## Getting Started

1. **Install dependencies:**
   ```bash
   npm install
   # or
   pnpm install
   ```

2. **Configure environment (optional):**
   ```bash
   cp .env.example .env
   ```
   Edit `.env` to customize the backend API URL if different from default.

3. **Start development server:**
   ```bash
   npm run dev
   # or
   pnpm dev
   ```

4. **Open in browser:**
   Navigate to `http://localhost:5173`

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_BASE_URL` | `http://localhost:5000` | Backend API base URL |

## Backend API Integration

The UI integrates with the following SentinelVault backend endpoints:

- `GET /api/profiles` - List all backup profiles
- `POST /api/profiles` - Create new backup profile
- `PUT /api/profiles/{id}` - Update backup profile
- `DELETE /api/profiles/{id}` - Delete backup profile
- `POST /api/backups/{profileId}/full` - Run full backup
- `POST /api/backups/{profileId}/incremental` - Run incremental backup
- `GET /api/backups/{profileId}` - Get backup history
- `POST /api/restores` - Restore from backup
- `GET /api/ai/explain/{runId}` - Get AI explanation for backup

## Project Structure

```
src/
├── api/           # API client and endpoint wrappers
├── components/    # Reusable UI components
├── hooks/         # Custom React hooks for data fetching
├── pages/         # Page components for routing
└── main.tsx       # Application entry point
```

## Key Components

- **Layout**: Main application layout with sidebar navigation
- **Sidebar**: Navigation sidebar with profile list
- **Dashboard**: Main backup management interface
- **ProfileForm**: Create/edit backup profiles
- **BackupTable**: Display backup history with actions
- **AIExplanationDrawer**: Side drawer for AI insights

## Building for Production

```bash
npm run build
# or
pnpm build
```

The built files will be in the `dist/` directory, ready for deployment to any static hosting service.

## Development

- **Linting**: `npm run lint`
- **Type checking**: Built into the development server
- **Hot reload**: Automatic reload on file changes

## Contributing

1. Follow the existing code style and TypeScript patterns
2. Use semantic commit messages
3. Test changes across different screen sizes
4. Ensure all API integrations work with the backend

## License

This project is part of the SentinelVault suite. See the main repository for license information.