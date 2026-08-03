# Fitness Center API – Installation Guide

## Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- (Optional) MySQL Workbench, MongoDB Compass, Neo4j Browser

---

## Quick Start – Docker Compose

```bash
git clone https://github.com/ali0509-arch/FitnessCenter-API.git
cd FitnessCenter-API/FitnessCenter.API

# (Optional) set your Anthropic API key for AI enrichment
export ANTHROPIC_API_KEY=your_key_here

docker-compose up --build
```

The API starts at **http://localhost:5000** (Swagger UI).
MySQL auto-runs all scripts in `sql/` on first boot:
- `01_schema.sql` – creates all tables, views, triggers, events, stored procedures
- `02_users_and_privileges.sql` – creates 4 database users
- `03_testdata.sql` – inserts 100+ rows per entity

---

## Manual Setup (without Docker)

### 1. MySQL
```sql
-- Run in order:
-- sql/01_schema.sql
-- sql/02_users_and_privileges.sql
-- sql/03_testdata.sql
```
Update `appsettings.Development.json` with your connection string.

### 2. Run the API
```bash
cd FitnessCenter.API
dotnet restore
dotnet run
```

### 3. Run the Migrator (MySQL → MongoDB + Neo4j)
```bash
cd FitnessCenter.Migrator
# Update appsettings.json with MongoDB and Neo4j connection strings
dotnet run
```

### 4. Run Tests
```bash
cd FitnessCenter.Tests
dotnet test
```

---

## Authentication

```json
POST /api/auth/login
{ "username": "superadmin", "password": "Admin123!" }
```
Copy the returned token → Swagger Authorize → `Bearer <token>`

### Users
| Username       | Password   | Role  |
|----------------|------------|-------|
| superadmin     | Admin123!  | Admin |
| admin          | Admin123!  | Admin |
| trainer_user   | Admin123!  | User  |
| readonly_user  | Admin123!  | User  |
| staff_user     | Admin123!  | User  |

---

## API Endpoints (overview)

| Tag          | Base path              |
|--------------|------------------------|
| General      | `/api/members`, `/api/trainers`, `/api/classes`, ... |
| MySQL        | `/api/mysql/members`, `/api/mysql/trainers`, ... |
| MongoDB      | `/api/mongodb/members`, `/api/mongodb/classes`, ... |
| Neo4j        | `/api/neo4j/members`, `/api/neo4j/trainers`, ... |
| AI Enrichment| `/api/ai/trainers/{id}/enrich`, `/api/ai/classes/{id}/enrich` |

---

## AI Enrichment Feature

```bash
# Generate bio for trainer 1 (Admin required)
POST /api/ai/trainers/1/enrich

# Generate description for class 5
POST /api/ai/classes/5/enrich

# Enrich all trainers at once
POST /api/ai/trainers/enrich-all

# View stored AI bios
GET /api/ai/trainers/bios
```

Set `Anthropic:ApiKey` in appsettings.json or via env variable `Anthropic__ApiKey`.

---

## Database Users

| User               | Privileges                                   |
|--------------------|----------------------------------------------|
| fitness_app        | SELECT, INSERT, UPDATE, DELETE on all tables |
| fitness_admin      | ALL PRIVILEGES WITH GRANT OPTION             |
| fitness_readonly   | SELECT on all tables                         |
| fitness_restricted | SELECT on all tables **except** Payment and audit_log |
