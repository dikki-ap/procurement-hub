# Procurement Hub

**Supplier Management & Procurement Workflow Portal**

Procurement Hub is a web-based procurement management system designed for manufacturing companies. It manages the complete procurement lifecycle — from Purchase Requisition through invoice payment — with an integrated vendor portal, configurable multi-level approval workflows, and a full audit trail on every transaction.

---

## Procurement Lifecycle

```
Purchase Requisition (PR)
    → RFQ (Request for Quotation)
        → Vendor Bidding
            → Bid Evaluation
                → Multi-Level Approval
                    → Purchase Order (PO)
                        → Goods Receipt (GRN)
                            → Invoice
                                → Payment
```

---

## Key Features

| Feature | Description |
|---|---|
| **Vendor Portal** | Vendors self-register, upload compliance documents, and submit quotations independently |
| **Competitive Bidding** | Invite multiple vendors to a single RFQ, compare offers, and perform structured evaluation |
| **Multi-Level Approval** | Configurable approval engine per document type (PR / PO / RFQ) and value threshold. Supports Soft Reject (return one level) and Hard Reject |
| **Vendor Scoring** | Automatic KPI scorecard based on transaction history (quality, on-time delivery, pricing) |
| **Indonesian Tax** | Automatic calculation of PPN 11% (PKP vendors) and PPh withholding at per-vendor rates |
| **Document Management** | Upload, versioning, and expiry tracking for vendor compliance documents (SIUP, NPWP, ISO, etc.) |
| **Real-time Notifications** | Push notifications via SignalR when online; persisted to database for offline delivery |
| **Spend Analytics Dashboard** | Role-based dashboards — purchasing sees the procurement funnel; management sees spend by category |
| **Complete Audit Trail** | Every create / update / delete is recorded with before/after JSON diff, actor identity, and UTC timestamp |
| **3-Way Matching** | Invoice validation against PO and GRN before finance approval |
| **Contract Management** | Contract creation from selected POs, validity tracking, and auto-reminder before expiry |
| **Document Access Logging** | Every document access and download is logged to the audit trail (who, when, which file, from which IP) |

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | .NET 8 LTS · Clean Architecture · CQRS via MediatR |
| **Frontend** | React 19 · TypeScript · Vite · TailwindCSS · shadcn/ui |
| **Database** | MariaDB 11.4 |
| **Auth** | Keycloak 26.4 (JWT + RBAC via realm roles) |
| **File Storage** | SeaweedFS 3.93 (S3-compatible, presigned URL download) |
| **Background Jobs** | Hangfire (job persistence in MariaDB) |
| **Real-time** | ASP.NET Core SignalR |
| **Email** | MailKit · Mailpit (development inbox) |
| **PDF Generation** | PDFsharp 6 |
| **Testing** | xUnit · FluentAssertions · Testcontainers (MariaDB) |
| **Container** | Docker + Docker Compose |

---

## Architecture

```
ProcureHub/
├── src/
│   ├── ProcureHub.API/              # Entry point: middleware, DI, controllers
│   ├── ProcureHub.SharedKernel/     # Base entities, EF DbContext, behaviors, abstractions
│   └── Modules/
│       ├── MasterData/              # Company, currencies, UoM, locations, material categories
│       ├── VendorManagement/        # Vendor lifecycle, documents, scoring, portal
│       ├── Procurement/             # PR, RFQ, bidding, evaluation, PO, contracts
│       ├── ApprovalEngine/          # Configurable multi-level approval workflow
│       ├── Fulfillment/             # GRN, invoice (3-way matching), payment, return orders
│       ├── DocumentManagement/      # PDF generation, SeaweedFS integration
│       ├── Notifications/           # SignalR hub, email templates, in-app notifications
│       ├── Analytics/               # Role-based dashboards, spend analytics
│       └── Audit/                   # Immutable audit trail with before/after diff
├── frontend/                        # Vite + React SPA
├── tests/
│   ├── ProcureHub.UnitTests/
│   └── ProcureHub.IntegrationTests/ # Full end-to-end via Testcontainers
├── scripts/
│   └── seed.sql                     # Seed data (company, vendors, users, master data)
├── docker-compose.yml
├── Dockerfile
└── .env.example
```

Each module follows a consistent internal structure:

```
Module/
├── Domain/          # Entities, value objects, domain events, enums
├── Application/     # Commands, queries, validators, handlers (CQRS + MediatR)
└── Infrastructure/  # Repositories, EF configurations, external service adapters
```

---

## User Roles

### Internal Portal

| Role | Access |
|---|---|
| `super_admin` | Full access — master data, user management, audit log, system configuration |
| `requester` | Create and track Purchase Requisitions |
| `purchasing` | Manage RFQs, bidding, evaluation, and issue Purchase Orders |
| `approver` | Approve, revise, or reject documents at the assigned approval level |
| `finance` | Verify invoices and confirm payments |
| `management` | Read-only access to analytics, reports, and dashboards |

### Vendor Portal

| Role | Access |
|---|---|
| `vendor_admin` | Manage company profile, compliance documents, and vendor user accounts |
| `vendor_staff` | Submit quotations and respond to RFQs |

---

## Prerequisites

Ensure the following tools are installed before proceeding:

| Tool | Minimum Version | Link |
|---|---|---|
| Docker Engine | 24+ | https://docs.docker.com/get-docker/ |
| Docker Compose | V2 plugin | Included in Docker Desktop |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8 |
| Node.js | 20 LTS | https://nodejs.org/ |
| Git | Any | https://git-scm.com/ |

> **Note:** For development, all infrastructure services (MariaDB, Keycloak, SeaweedFS, Mailpit) run via Docker. Only .NET SDK and Node.js need to be installed locally.

---

## Installation & Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ProcureHub
```

### 2. Configure Environment Variables

```bash
# Copy the example environment file
cp .env.example .env

# Edit .env as needed — at minimum, change all default passwords before production use
# For local development, the default values work out of the box
```

### 3. Start All Services (Docker Compose)

```bash
docker compose up -d
```

On first run, the system will automatically:
- Create the database and run all EF Core migrations
- Initialize the SeaweedFS storage bucket
- Start Keycloak with the configured realm settings

Check container status:
```bash
docker compose ps
```

Tail application logs:
```bash
docker compose logs -f app
```

### 4. Configure Keycloak

Run the automated setup script — it imports the realm, creates all 10 internal users, assigns roles, and generates the SQL to sync `keycloak_id` in the database:

```bash
bash scripts/setup-keycloak.sh
```

Then apply the generated SQL:

```bash
mysql -h 127.0.0.1 -P 3307 -u root -prootsecret procurehub < scripts/keycloak-id-updates.sql
```

The script accepts the following environment overrides (all optional — defaults match the `.env.example` values):

| Variable | Default | Description |
|---|---|---|
| `KC_URL` | `http://localhost:9090` | Keycloak base URL |
| `KC_ADMIN` | `admin` | Keycloak admin username |
| `KC_ADMIN_PASSWORD` | `admin` | Keycloak admin password |
| `DB_NAME` | `procurehub` | Database name (written into the SQL file) |
| `TEMP_PASSWORD` | `ProcureHub@2024!` | Temporary password for all created users |

All created users are marked with `temporary: true` — they must change their password on first login.

> **Vendor users** (vendor_admin and vendor_staff) are created when the vendor registers via the self-registration portal. They do not need to be created manually in Keycloak.

### 5. Load Seed Data

After migrations have run successfully:

```bash
mysql -h 127.0.0.1 -P 3307 -u root -prootsecret procurehub < scripts/seed.sql
```

Or from inside the MariaDB container:
```bash
docker compose exec mariadb mysql -u root -prootsecret procurehub -e "SOURCE /scripts/seed.sql"
```

---

## Development Mode (Local App + Docker Infrastructure)

For active development, run infrastructure via Docker and the app and frontend locally:

```bash
# 1. Start infrastructure services only
docker compose up -d mariadb keycloak seaweedfs-master seaweedfs-filer seaweedfs-s3 mailpit

# 2. Start the backend
dotnet restore
dotnet run --project src/ProcureHub.API

# 3. Start the frontend (separate terminal)
cd frontend
npm install
cp .env.example .env   # Edit VITE_API_URL and VITE_KEYCLOAK_URL as needed
npm run dev
```

---

## Service URLs

### Development (Docker Compose)

| Service | URL | Notes |
|---|---|---|
| **API Backend** | http://localhost:8080 | ASP.NET Core Web API |
| **Swagger UI** | http://localhost:8080/swagger | API documentation and interactive testing |
| **Hangfire Dashboard** | http://localhost:8080/hangfire | Background job monitoring |
| **Keycloak Admin** | http://localhost:9090 | User and realm management |
| **Mailpit (dev inbox)** | http://localhost:8025 | Captures all outgoing emails in development |
| **MariaDB** | localhost:3307 | Database (connect with DBeaver or similar) |
| **SeaweedFS S3 API** | http://localhost:8333 | File storage (S3-compatible API) |
| **SeaweedFS Master** | http://localhost:9333 | SeaweedFS cluster coordinator |
| **Frontend Dev Server** | http://localhost:5173 | Vite dev server (when running locally) |

### Production (Deployment)

In production, all services are exposed via a reverse proxy (Nginx or Traefik). Example URL mapping:

| Service | Public URL | Notes |
|---|---|---|
| Frontend SPA | `https://procurement.company.com` | Static files served via Nginx |
| API Backend | `https://api.procurement.company.com` | Proxied to container app:8080 |
| Keycloak | `https://auth.procurement.company.com` | Proxied to keycloak:8080 |

---

## Environment Variables

### Root `.env` (Docker Compose)

```env
# ─── Application ─────────────────────────────────────────────
APP_PORT=8080
ASPNETCORE_ENVIRONMENT=Production
APP_FRONTEND_URL=https://procurement.company.com

# ─── Database (MariaDB) ──────────────────────────────────────
DB_NAME=procurehub
DB_USER=app
DB_PASSWORD=CHANGE_THIS_TO_A_STRONG_PASSWORD
DB_ROOT_PASSWORD=CHANGE_THIS_TO_A_STRONG_ROOT_PASSWORD
DB_PORT=3307

# ─── Keycloak ────────────────────────────────────────────────
KC_DB_USER=keycloak
KC_DB_PASSWORD=CHANGE_THIS
KC_DB_PORT=5433
KC_ADMIN_USER=admin
KC_ADMIN_PASSWORD=CHANGE_THIS_TO_A_STRONG_ADMIN_PASSWORD
KC_PORT=9090
KC_CLIENT_ID=procurehub-api
KC_CLIENT_SECRET=CHANGE_THIS_TO_YOUR_CLIENT_SECRET

# ─── SeaweedFS (File Storage) ────────────────────────────────
SEAWEEDFS_ACCESS_KEY=CHANGE_THIS
SEAWEEDFS_SECRET_KEY=CHANGE_THIS
S3_PORT=8333

# ─── Email (SMTP) ────────────────────────────────────────────
# Development: leave as default to use Mailpit
# Production: set to your real SMTP server
SMTP_HOST=mailpit
SMTP_PORT=1025
SMTP_USERNAME=
SMTP_PASSWORD=
SMTP_FROM=noreply@procurement.company.com
SMTP_USE_SSL=false

# ─── Frontend (Keycloak) ─────────────────────────────────────
VITE_KEYCLOAK_URL=http://localhost:9090
VITE_KEYCLOAK_REALM=procurehub
VITE_KEYCLOAK_CLIENT_ID=procurehub-web
```

### Frontend `frontend/.env`

```env
# Backend API base URL
VITE_API_URL=http://localhost:8080

# Keycloak
VITE_KEYCLOAK_URL=http://localhost:9090
VITE_KEYCLOAK_REALM=procurehub
VITE_KEYCLOAK_CLIENT_ID=procurehub-web

# Default company ID for the public vendor self-registration form (unauthenticated page).
# Set this to the CompanyId of the client company in the database.
VITE_DEFAULT_COMPANY_ID=11111111-1111-1111-1111-111111111111
```

---

## Database Migrations

Migrations run automatically on application startup. To run them manually:

```bash
# Apply all pending migrations
dotnet ef database update \
  --project src/ProcureHub.SharedKernel \
  --startup-project src/ProcureHub.API

# Create a new migration
dotnet ef migrations add MigrationName \
  --project src/ProcureHub.SharedKernel \
  --startup-project src/ProcureHub.API
```

---

## Running Tests

```bash
# Unit tests
dotnet test tests/ProcureHub.UnitTests

# Integration tests (requires Docker — Testcontainers auto-starts a MariaDB container)
dotnet test tests/ProcureHub.IntegrationTests
```

Integration tests cover the full procurement cycle: PR → RFQ → bidding → approval → PO → GRN → Invoice → Payment, as well as security tests (401 / 403) and the vendor registration flow.

---

## Health Check

```
GET /health        → Liveness check (application is running)
GET /health/ready  → Readiness check (database, storage, and all dependencies are healthy)
```

---

## Production Deployment

### Pre-Deployment Checklist

- [ ] Replace all default passwords in `.env` (DB, Keycloak admin, client secret, SeaweedFS keys)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure a real SMTP server (not Mailpit)
- [ ] Set up HTTPS with a valid SSL certificate (Let's Encrypt or a company CA)
- [ ] Configure a reverse proxy (Nginx or Traefik) for all public-facing services
- [ ] Start Keycloak in production mode (`start`, not `start-dev`)
- [ ] Import the Keycloak realm and create all internal users and vendor users
- [ ] Update `keycloak_id` in the database to match the actual Keycloak user UUIDs
- [ ] Run `scripts/seed.sql` with the client company name and code updated (see below)
- [ ] Configure automated backups for the MariaDB volume and SeaweedFS volume
- [ ] Verify the readiness endpoint returns 200: `GET /health/ready`

### Build the Production Image

```bash
# Build the Docker image
docker build -t procurehub:latest .

# Or build and start via Docker Compose
docker compose -f docker-compose.yml up -d --build
```

### Company Configuration

Before running the seed script, update the company record in `scripts/seed.sql` to match the client:

```sql
INSERT IGNORE INTO companies (id, name, code, ...)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'PT. Client Company Name',  -- replace with actual company name
    'COMPANY_CODE',             -- replace with company code (max 50 chars, must be unique)
    'Internal',
    '123 Main Street, City',    -- replace with actual address
    '+62-xxx-xxxx-xxxx',        -- replace with actual phone
    'email@company.com',        -- replace with actual contact email
    ...
);
```

---

## API Documentation

Swagger UI is available at `/swagger` when `Swagger__Enabled=true` (enabled by default in Development; can be enabled in Production via environment variable).

All endpoints require a JWT Bearer token issued by Keycloak, except:
- `POST /api/v1/vendor-registration` — public (vendor self-registration, no auth required)
- `GET /health` and `GET /health/ready` — public

---

## Seed Data Summary

The `scripts/seed.sql` script provides a realistic starting dataset for development and demonstration:

| Data | Count | Notes |
|---|---|---|
| Company | 1 | PT. Nexcore Industries — update to match the actual client before running |
| Currencies | 5 | IDR (base), USD, EUR, SGD, JPY |
| Units of Measure | 10 | KG, TON, PCS, LTR, MTR, BOX, SET, UNIT, ROLL, DRUM |
| Payment Terms | 8 | COD, NET7, NET14, NET30, NET45, NET60, DP30%, DP50% |
| Material Categories | 8 | Raw Material, Spare Parts, MRO, CAPEX, Packaging, Consumables, Chemical, Electrical |
| Locations | 5 | Main warehouse, production floor, and auxiliary locations |
| Departments | 6 | Procurement, Production, Engineering, Finance, Management, IT |
| Internal Users | 10 | All roles: super_admin, purchasing (×2), requester (×2), approver (×3), finance, management |
| Approver Matrix | 6 | L1 / L2 / L3 for both PR and PO document types |
| Vendors | 8 | Mixed: Gold/Silver/Bronze/Probation tier · Active/Suspended/Blacklisted status · PKP+PPh / PKP / non-PKP tax · IDR and USD billing |
| Vendor Users | 13 | vendor_admin and vendor_staff accounts across all vendors |
| Bank Accounts | 8 | One per vendor, covering multiple banks and currencies |
| Vendor Capabilities | 10 | One intentionally expired to demonstrate expiry tracking |
| Materials | 16 | Multiple categories, strategic and non-strategic, active and inactive |

---

## License

Private — All rights reserved.
