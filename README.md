# ProcureHub

> **Supplier Management & Procurement Workflow Portal**
> Built for manufacturing companies that need end-to-end procurement visibility, multi-level approval governance, and a structured vendor collaboration portal.

---

## Procurement Lifecycle

```
Purchase Requisition → RFQ → Vendor Bidding → Evaluation → Multi-level Approval → Purchase Order → GRN → Invoice → Payment
```

Every step is tracked, audited, and routed through role-based access controls — from the requester who raises a PR to the finance team that confirms payment.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 LTS · Clean Architecture · CQRS via MediatR |
| Frontend | React 19 · TypeScript · Vite · TailwindCSS · shadcn/ui |
| Database | MariaDB 11.4 |
| Auth | Keycloak 26.4 (JWT + RBAC) |
| Storage | SeaweedFS 3.93 (S3-compatible) |
| Background Jobs | Hangfire |
| Real-time | SignalR (in-app notifications) |
| Email | MailKit · Mailpit (dev inbox) |
| PDF | PDFsharp 6 |
| Testing | xUnit · FluentAssertions · Testcontainers (MariaDB) |

---

## Architecture

```
ProcureHub/
├── src/
│   ├── ProcureHub.API/              # Entry point, middleware, DI composition
│   ├── ProcureHub.SharedKernel/     # Base entities, EF context, behaviors, abstractions
│   └── Modules/
│       ├── MasterData/              # Company, currency, UoM, locations, categories
│       ├── VendorManagement/        # Vendor lifecycle, documents, scoring, portal
│       ├── Procurement/             # PR, RFQ, bidding, evaluation, PO
│       ├── ApprovalEngine/          # Multi-level configurable approval workflows
│       ├── Fulfillment/             # GRN, invoice, payment
│       ├── DocumentManagement/      # PDF generation, file storage
│       ├── Notifications/           # SignalR hub, email templates, in-app notifications
│       ├── Analytics/               # Role-based dashboards, spend analytics
│       └── Audit/                   # Immutable audit trail with before/after diff
├── frontend/                        # Vite + React SPA
├── tests/
│   ├── ProcureHub.UnitTests/
│   └── ProcureHub.IntegrationTests/ # End-to-end flows via Testcontainers
├── docker-compose.yml
├── Dockerfile
└── .env.example
```

Each module follows the same internal structure:

```
Module/
├── Domain/          # Entities, value objects, domain events, enums
├── Application/     # Commands, queries, validators, handlers (CQRS)
└── Infrastructure/  # Repositories, EF configurations, external services
```

---

## User Roles

### Internal Portal

| Role | Permissions |
|---|---|
| `super_admin` | Full access — master data, user management, audit log, system config |
| `requester` | Create and track Purchase Requisitions |
| `purchasing` | Manage RFQs, bids, evaluations, issue Purchase Orders |
| `approver` | Approve, revise, or reject documents at assigned levels |
| `finance` | Verify invoices and confirm payments |
| `management` | Read-only access to analytics, reports, and dashboards |

### Vendor Portal

| Role | Permissions |
|---|---|
| `vendor_admin` | Manage company profile, documents, and vendor users |
| `vendor_staff` | Submit quotations and respond to RFQs |

---

## Key Features

- **Multi-level approval engine** — configurable per document type and value threshold; supports L1→L2→L3 chains with escalation notifications
- **Vendor lifecycle management** — registration, document expiry tracking, scoring, blacklist/reinstate
- **Competitive bidding** — invite multiple vendors to an RFQ, collect sealed bids, compare and evaluate
- **Real-time notifications** — SignalR push when online, DB-persisted fallback when offline
- **Immutable audit trail** — every create/update/delete captured with before/after JSON diff
- **Role-based dashboards** — internal staff see procurement funnels; vendors see their own performance
- **PDF generation** — Purchase Orders generated as PDFs and stored in SeaweedFS
- **Health checks** — `/health` (liveness) and `/health/ready` (readiness) endpoints

---

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/) & Docker Compose
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)

### Run with Docker Compose

```bash
cp .env.example .env
docker compose up -d
```

All services start and the app bootstraps itself automatically:
- Migrations applied on first run
- Master data seeded (company, currencies, units of measure, etc.)
- SeaweedFS buckets created

### Run Locally (Development)

```bash
# Backend
dotnet restore
dotnet run --project src/ProcureHub.API

# Frontend (separate terminal)
cd frontend
npm install
npm run dev
```

---

## Service URLs

| Service | Development URL |
|---|---|
| App | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| Hangfire Dashboard | http://localhost:8080/hangfire |
| Keycloak Admin | http://localhost:9090 |
| Mailpit (dev inbox) | http://localhost:8025 |
| SeaweedFS S3 API | http://localhost:8333 |
| SeaweedFS Master | http://localhost:9333 |

---

## Running Tests

```bash
# Unit tests
dotnet test tests/ProcureHub.UnitTests

# Integration tests (requires Docker — starts a MariaDB container automatically)
dotnet test tests/ProcureHub.IntegrationTests
```

Integration tests cover full end-to-end flows: PR → RFQ → bidding → approval → PO → fulfillment → payment, plus security tests (401/403) and vendor registration flows.

---

## Environment Variables

Copy `.env.example` to `.env` and configure:

```env
# Database
MARIADB_ROOT_PASSWORD=
MARIADB_DATABASE=procurehub
MARIADB_USER=
MARIADB_PASSWORD=

# Keycloak
KEYCLOAK_ADMIN=
KEYCLOAK_ADMIN_PASSWORD=

# SeaweedFS (S3-compatible)
SEAWEEDFS_ACCESS_KEY=
SEAWEEDFS_SECRET_KEY=

# SMTP (use Mailpit in dev)
SMTP_HOST=
SMTP_PORT=587
SMTP_USER=
SMTP_PASSWORD=
```

---

## Current Status

All 12 development phases are complete. The system is in **UAT (User Acceptance Testing)** phase. A comprehensive senior architect audit was performed on 2026-07-21 identifying gaps for enterprise go-live readiness.

### UAT Bug Fixes (Resolved)

| # | Issue | Status |
|---|-------|--------|
| 1 | Document preview/download showing alt text — SeaweedFS aws-chunked encoding corruption | ✅ Fixed |
| 2 | Presigned URL using HTTPS on HTTP SeaweedFS endpoint | ✅ Fixed |
| 3 | `BucketAlreadyExistsException` on startup | ✅ Fixed |
| 4 | Vendor capability dropdown empty (hardcoded company ID) | ✅ Fixed |
| 5 | Notification panel causing 401 spam for vendor sessions | ✅ Fixed |
| 6 | `ExportAuditLogCommandHandler` CS1503 build error | ✅ Fixed |

### Enterprise Completion Roadmap

| Sprint | Focus | Priority |
|--------|-------|----------|
| Sprint 1 | Vendor Address · Bank Account · Default Payment/Currency · Quotation Security Fix · Capability Expiry+MaxQty | 🔴 Critical |
| Sprint 2 | Vendor Score History UI · Company Profile · Department Entity · User Management | 🟠 High |
| Sprint 3 | Contract Management — full feature (commands, API, Hangfire, frontend) | 🟠 High |
| Sprint 4 | Invoice 3-Way Matching · Vendor Portal Edit+Notifications · Return Orders | 🟠 High |
| Sprint 5 | Spend Analytics by Category · Cycle Time KPIs · RFQ/Quotation Attachments · PO Ack Deadline · Multi-Evaluator | 🟡 Medium |
| Sprint 6 | Indonesian Tax (PKP + PPh) · Multi-Tenancy Security Audit · Document Access Logging · Final Hardening | 🟡 Medium |

Full step-by-step details for each sprint are in [`projects/project.md`](projects/project.md) — Section 31.

---

## License

Private — All rights reserved.
