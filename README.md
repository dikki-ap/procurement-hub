# ProcureHub

Supplier Management & Procurement Workflow Portal for manufacturing companies.

## Overview

ProcureHub manages the full procurement lifecycle from Purchase Requisition to Invoice, with multi-level approval, vendor bidding, and real-time notifications.

```
PR → RFQ → Bidding → Evaluation → Approval → PO → GRN → Invoice
```

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 LTS, Clean Architecture, CQRS (MediatR 11) |
| Frontend | React 19, TypeScript, Vite, TailwindCSS, shadcn/ui |
| Database | MariaDB 11.4 (primary), PostgreSQL 16 |
| Auth | Keycloak 26.4.7 (JWT + RBAC) |
| Storage | SeaweedFS 3.93 (S3-compatible) |
| Background Jobs | Hangfire |
| Real-time | SignalR |
| Email | MailKit + Mailpit (dev) |

## User Roles

**Internal Portal**
- `super_admin` — Full access, master data, system configuration
- `requester` — Create Purchase Requisitions
- `purchasing` — RFQ, bidding, evaluation, PO management
- `approver` — Approve / Revise / Reject documents
- `finance` — Invoice verification, payment confirmation
- `management` — Read-only analytics and reports

**Vendor Portal**
- `vendor_admin` — Manage company profile, documents, vendor users
- `vendor_staff` — Submit quotations and bids

## Project Structure

```
procurehub/
├── src/
│   ├── ProcureHub.API/
│   ├── ProcureHub.SharedKernel/
│   └── Modules/
│       ├── MasterData/
│       ├── VendorManagement/
│       ├── Procurement/
│       ├── ApprovalEngine/
│       ├── DocumentManagement/
│       ├── Notifications/
│       └── Analytics/
├── frontend/
├── tests/
│   ├── ProcureHub.UnitTests/
│   └── ProcureHub.IntegrationTests/
├── keycloak/
├── docker-compose.yml
├── Dockerfile
└── .env.example
```

## Getting Started

### Prerequisites

- Docker & Docker Compose
- .NET 8 SDK
- Node.js 20+

### Run with Docker

```bash
cp .env.example .env
docker compose up -d
```

Services:
| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Keycloak Admin | http://localhost:9090 |
| Mailpit | http://localhost:8025 |
| Hangfire | http://localhost:8080/hangfire |
| SeaweedFS S3 | http://localhost:8333 |
| SeaweedFS Master | http://localhost:9333 |

### Run Locally (Development)

```bash
# Backend
dotnet restore
dotnet run --project src/ProcureHub.API

# Frontend
cd frontend
npm install
npm run dev
```

## Development Phases

| Phase | Description | Status |
|---|---|---|
| 0 | Setup & Infrastructure | ⬜ |
| 1 | SharedKernel | ⬜ |
| 2 | Master Data | ⬜ |
| 3 | Vendor Management | ⬜ |
| 4 | PR & RFQ | ⬜ |
| 5 | Bidding & Evaluation | ⬜ |
| 6 | Approval Engine | ⬜ |
| 7 | PO & Fulfillment | ⬜ |
| 8 | Notifications & Real-time | ⬜ |
| 9 | Analytics & Dashboard | ⬜ |
| 10 | Audit Log | ⬜ |
| 11 | Integration Tests | ⬜ |
| 12 | Production Readiness | ⬜ |

## License

Private — All rights reserved.
