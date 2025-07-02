

# SentinelVault

SentinelVault is a full-stack database backup orchestration and monitoring system, providing robust CLI tools and a modern React-based web interface to manage and analyze database backups. It supports MySQL, PostgreSQL, SQLite, and MongoDB, with additional features such as Slack notifications and OpenAI-powered log explanations.

---

## Features

✅ Supports Full and Incremental database backups
✅ RESTful Web API built with .NET 8
✅ CLI for automation and scripting
✅ Frontend in React + TypeScript + Tailwind
✅ Slack integration for backup notifications
✅ OpenAI GPT-based log explanations
✅ Docker Compose environment for easy onboarding
✅ Extensible to S3 object storage

---

## Technologies Used

* C# (.NET 8) for Web API and CLI
* React with TypeScript for frontend
* Docker + Docker Compose for orchestration
* Entity Framework Core for SQLite profile persistence
* MySQL, PostgreSQL, SQLite adapters with pluggable architecture
* Slack Webhooks for notifications
* OpenAI GPT for AI log summaries
* Serilog for advanced structured logging

---

## Architecture Overview

```
.
├── docker-compose.yml
├── DbBackup.WebApi/             ← ASP.NET Core Web API
├── DbBackup.Cli/                ← .NET CLI entrypoint
├── DbBackup.Core/               ← Core logic and orchestration
├── DbBackup.Adapters/           ← Database-specific backup adapters
├── DbBackup.Storage/            ← File storage / S3 support
├── ui/                          ← React + TypeScript Frontend
└── backups/                     ← Local backup storage volume
```
┌────────────────────────────────────────────────────────────────────────┐
│                             SentinelVault                              │
│                                                                        │
│   ┌──────────────────────────────────────────────────────────────┐     │
│   │                        Frontend (React + Vite)               │     │
│   │  - Dashboard                                                │     │
│   │  - Profiles CRUD                                            │     │
│   │  - Backups Table (full/inc/restore/explain)                 │     │
│   │  - React Query API calls                                    │     │
│   └──────────────────────────────────────────────────────────────┘     │
│                 │                                                       │
│                 ▼                                                       │
│   ┌──────────────────────────────────────────────────────────────┐     │
│   │                  Backend (ASP.NET Core Web API)              │     │
│   │  - API Controllers                                           │     │
│   │  - AI Explanation Service (OpenAI GPT)                       │     │
│   │  - Slack Notification Service                                │     │
│   │  - Backup Orchestrator (DbBackup.Core)                       │     │
│   │  - Uses: Sqlite DB for configs                               │     │
│   └──────────────────────────────────────────────────────────────┘     │
│                 │                                                       │
│                 ▼                                                       │
│   ┌──────────────────────────────────────────────────────────────┐     │
│   │                  CLI (DbBackup.Cli)                           │     │
│   │  - Command line interface for backups                         │     │
│   │  - Uses same BackupOrchestrator                               │     │
│   └──────────────────────────────────────────────────────────────┘     │
│                 │                                                       │
│                 ▼                                                       │
│   ┌──────────────────────────────────────────────────────────────┐     │
│   │         Databases                                            │     │
│   │   - MySQL: application data                                   │     │
│   │   - PostgreSQL: application data                              │     │
│   │   - Sqlite: backup profile configurations                     │     │
│   └──────────────────────────────────────────────────────────────┘     │
│                 │                                                       │
│                 ▼                                                       │
│   ┌──────────────────────────────────────────────────────────────┐     │
│   │     Cloud Storage (S3 Bucket)                               │     │
│   │     - Stores backup files                                    │     │
│   └──────────────────────────────────────────────────────────────┘     │
│                                                                        │
│  * All connected via Docker Compose for local orchestration           │
└────────────────────────────────────────────────────────────────────────┘

---

## Running Locally with Docker

1️⃣ Copy the provided `.env.example` to `.env` and fill in:

```bash
OPENAI__APIKEY=your-openai-key
SLACK__WEBHOOKURL=https://hooks.slack.com/services/...
S3__ENABLED=false
S3__BUCKET=my-db-backups
```

2️⃣ Start the entire stack:

```bash
docker compose up --build
```

3️⃣ Access services:

* Web API → [http://localhost:5000](http://localhost:5000)
* React UI → [http://localhost:5173](http://localhost:5173) (if you `cd ui && npm install && npm run dev`)
* Databases

  * MySQL → port 3306
  * PostgreSQL → port 5432

---

## Using the CLI

Inside Docker:

```bash
docker compose run --rm cli backup --engine mysql --host mysql --port 3306 --user root --password root --database test --out /app/backups
```

Or directly on your machine if .NET SDK installed:

```bash
dotnet run --project DbBackup.Cli -- backup --engine mysql ...
```

---

## Environment Variables

| Variable            | Description                          |
| ------------------- | ------------------------------------ |
| `OPENAI__APIKEY`    | Your OpenAI API key                  |
| `SLACK__WEBHOOKURL` | Slack webhook URL                    |
| `S3__ENABLED`       | true/false for S3 usage              |
| `S3__BUCKET`        | Bucket name for S3                   |
| `MYSQL_*`           | MySQL connection string variables    |
| `PG_*`              | Postgres connection string variables |

All variables can be set in `.env` or your `docker-compose.yml`.

---

## Tests

Unit tests are in `DbBackup.Tests`. Run them with:

```bash
dotnet test
```

---

## Extending the Project

✅ Add new database adapters in `DbBackup.Adapters`
✅ Add more backup strategies in `DbBackup.Core`
✅ Extend UI components in `ui/src`
✅ Use `serilog.json` to configure advanced logging

---

## Contributing

Contributions welcome!
Open a PR or issue and follow the clean coding principles of .NET + React projects.

---

## License

MIT License © 2025 SentinelVault Contributors

---

🚀
