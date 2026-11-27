# SaigonBite

A cloud-kitchen food ordering & delivery platform built with ASP.NET Core MVC and EF Core.

Final project for **Introduction to Software Engineering**
Team F1 · Tier 3 implementation.

---

## Overview

SaigonBite is a web platform for a cloud-kitchen operator: multiple virtual food
brands cook out of shared kitchen hubs, and customers order through a single
storefront. The system covers the full order lifecycle — browsing menus, placing
and tracking orders, applying vouchers, assigning deliveries, and reporting for
operators.

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET `<8.0 / 9.0>`) |
| Data access | Entity Framework Core (code-first migrations) |
| Database | `<SQL Server LocalDB / SQL Server>` |
| UI | Razor Views, Bootstrap 5 + custom design system (`site.css`) |
| Source control | Git / GitHub |

## Team F1

| ID | Member | Role | Owns |
|---|---|---|---|
| M1 | Thuc Nhi ([@Bontori1987](https://github.com/Bontori1987)) | Business Analyst & QA Lead | UC1, UC4 |
| M2 | Minh Duc (`<@github>`) | Technical Lead & Scrum Master | UC2, UC7-R2 |
| M3 | Gia Huy (`<@github>`) | Full-Stack Dev & UI/UX Lead | UC3, UC5, UC6, UC7-R1 |

## Use cases

**Main**
- **UC1 — Place and Manage Orders**: cart, checkout, order state machine, cancellation
- **UC2 — Manage Hubs and Capacity**: kitchen hubs, capacity limits, order routing
- **UC3 — Manage Menu and Brands**: virtual brands, menu items, availability

**Supporting**
- **UC4 — Vouchers**: discount rules, validation, redemption
- **UC5 — User Accounts**: registration, authentication, profiles
- **UC6 — Deliveries**: driver assignment, delivery tracking
- **UC7 — Reports**: operational reporting (R1, R2)

## Getting started

### Prerequisites
- .NET SDK `<version>` — https://dotnet.microsoft.com/download
- `<SQL Server LocalDB (ships with Visual Studio) / SQL Server 2022>`
- Visual Studio 2022 or JetBrains Rider

### Setup

```bash
git clone https://github.com/Bontori1987/saigonBite.git
cd saigonBite
```

**1. Configure the connection string.** It is not committed to the repo — each
developer stores it locally with User Secrets:

```bash
cd <SaigonBite.Web>
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=SaigonBite;Trusted_Connection=True;MultipleActiveResultSets=true"
```

In Visual Studio you can instead right-click the project → **Manage User Secrets**.

**2. Create the database.**

```bash
dotnet ef database update
```

**3. Run.**

```bash
dotnet run
```

The app starts at `https://localhost:<port>`.

## Repository conventions

### Branches
- `main` 
- `dev` 

### Commits

[Conventional Commits](https://www.conventionalcommits.org/):

```
feat(uc1): add order state machine
fix(uc4): round voucher discount to VND
test(uc1): EP/BVA cases for quantity field
docs: update setup instructions
```

### Pull requests
Link the PR to its Issue and the matching use case label (`UC1` … `UC7`).

## Project status

Course project under active development. Not intended for production use.
