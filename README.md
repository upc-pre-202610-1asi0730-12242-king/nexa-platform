<div align="center">

<br/>

<img src="https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/assets/img/nexa.svg" alt="Nexa Logo" width="250"/>

<br/><br/>

# nexa-platform

**Backend platform and REST API service layer for the Nexa B2B cold-chain distribution platform**

<br/>

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-REST%20API-0EA5E9?style=for-the-badge)
![EF Core](https://img.shields.io/badge/EF%20Core-PostgreSQL-512BD4?style=for-the-badge)
![Render](https://img.shields.io/badge/Render-Deployed-22c55e?style=for-the-badge&logo=render&logoColor=white)

<br/>

![Course](https://img.shields.io/badge/Course-1ASI0730%20Aplicaciones%20Web-0a2540?style=flat-square)
![Cycle](https://img.shields.io/badge/Cycle-2026--10-0a2540?style=flat-square)
![University](https://img.shields.io/badge/University-UPC-0a2540?style=flat-square)
![Team](https://img.shields.io/badge/Team-King-2a67d9?style=flat-square)
![Delivery](https://img.shields.io/badge/Delivery-TB2-0a2540?style=flat-square)
![Status](https://img.shields.io/badge/Status-Release%20v2.0.0-22c55e?style=flat-square)

<br/>

**[Start Website Flow](https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/)** ·
**[Open Swagger UI](https://nexa-platform-20wt.onrender.com/swagger/index.html)** ·
**[Open Live API](https://nexa-platform-20wt.onrender.com)**

<br/>

</div>

---

## Project Entry Flow

Start the Nexa review from the public Website and continue through the operational products:

1. **Website:** https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/
2. **WebApp:** https://nexa-webapp.onrender.com/#/auth/login
3. **Platform API:** https://nexa-platform-20wt.onrender.com
4. **Swagger UI:** https://nexa-platform-20wt.onrender.com/swagger/index.html
5. **Report Repository:** https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-ecosystem-report

---

## Overview

The `nexa-platform` repository contains the ASP.NET Core backend for Nexa. It exposes REST APIs for cold-chain catalog management, B2B purchase requests, commercial orders, warehouse inventory, logistics dispatch, billing documents, payment records, tenant management, and IAM workflows.

This repository is part of the **TB2 delivery** and is currently published as **v2.0.0**.

---

## Repository Map

<table>
  <tr>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-website">nexa-website</a></p>
      <p>Public landing website and central product entry point.</p>
      <p><a href="https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/">Open Live Website</a></p>
      <p>
        <img alt="Version" src="https://img.shields.io/badge/v4.0.1-TB2-22c55e?style=flat-square" />
        <img alt="HTML5" src="https://img.shields.io/badge/HTML5-static-E34F26?style=flat-square&logo=html5&logoColor=white" />
        <img alt="CSS3" src="https://img.shields.io/badge/CSS3-responsive-1572B6?style=flat-square&logo=css3&logoColor=white" />
        <img alt="JavaScript" src="https://img.shields.io/badge/JavaScript-vanilla-F7DF1E?style=flat-square&logo=javascript&logoColor=black" />
      </p>
    </td>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-webapp">nexa-webapp</a></p>
      <p>Main Vue web application for Buyer, Sales, Logistics, and Company Owner workflows.</p>
      <p><a href="https://nexa-webapp.onrender.com/#/auth/login">Open Live WebApp</a></p>
      <p>
        <img alt="Version" src="https://img.shields.io/badge/v3.0.1-TB2-22c55e?style=flat-square" />
        <img alt="Vue 3" src="https://img.shields.io/badge/Vue%203-35495E?style=flat-square&logo=vue.js&logoColor=4FC08D" />
        <img alt="Vite" src="https://img.shields.io/badge/Vite-0F172A?style=flat-square&logo=vite&logoColor=FFD62E" />
        <img alt="PrimeVue" src="https://img.shields.io/badge/PrimeVue-0EA5E9?style=flat-square" />
      </p>
    </td>
  </tr>
  <tr>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-platform">nexa-platform</a> (This Repository)</p>
      <p>Backend platform and API service layer for domain, persistence, security, and deployment concerns.</p>
      <p><a href="https://nexa-platform-20wt.onrender.com/swagger/index.html">Open Swagger UI</a></p>
      <p>
        <img alt="Version" src="https://img.shields.io/badge/v2.0.0-TB2-22c55e?style=flat-square" />
        <img alt="Platform" src="https://img.shields.io/badge/Platform-backend%20workspace-512BD4?style=flat-square" />
        <img alt="API" src="https://img.shields.io/badge/API-domain%20services-0EA5E9?style=flat-square" />
        <img alt="EF Core" src="https://img.shields.io/badge/EF%20Core-PostgreSQL-512BD4?style=flat-square" />
      </p>
    </td>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-ecosystem-report">nexa-ecosystem-report</a></p>
      <p>Academic report, product backlog, architecture documentation, validation, and evidence logs.</p>
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-ecosystem-report">Open Report Repository</a></p>
      <p>
        <img alt="Delivery" src="https://img.shields.io/badge/TB2-report%20scope-0a2540?style=flat-square" />
        <img alt="Documentation" src="https://img.shields.io/badge/Documentation-report-0F172A?style=flat-square" />
        <img alt="UPC" src="https://img.shields.io/badge/UPC-course%20evidence-0EA5E9?style=flat-square" />
      </p>
    </td>
  </tr>
</table>

---

## Live Services

| Component | Current Version | URL |
|---|---:|---|
| Platform API | `v2.0.0` | https://nexa-platform-20wt.onrender.com |
| Swagger UI | `v2.0.0` | https://nexa-platform-20wt.onrender.com/swagger/index.html |
| WebApp | `v3.0.1` | https://nexa-webapp.onrender.com/#/auth/login |
| Website | `v4.0.1` | https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/ |

---

## Application Areas

| Bounded Context | Backend Responsibility |
|---|---|
| **IAM** | Authentication, JWT issuance, password security, and user identity. |
| **Tenant Management** | Workspace membership, tenant access, company administration, and permissions. |
| **Catalog Management** | Products, brands, categories, and cold-chain catalog references. |
| **Sales** | Purchase requests, commercial orders, clients, and commercial validation. |
| **Warehouse** | Inventory items, lots, reservations, warehouses, and movements. |
| **Logistics** | Dispatch orders, shipments, temperature logs, and delivery evidence. |
| **Invoicing** | Business documents, invoices, payments, payment methods, and notifications. |
| **Shared** | Persistence, CORS, security policies, middleware, and reference seed data. |

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 10 / ASP.NET Core |
| **Language** | C# |
| **Persistence** | Entity Framework Core, PostgreSQL |
| **API Contract** | REST, Swagger / OpenAPI |
| **Architecture** | Bounded contexts, DDD, layered architecture |
| **Deployment** | Render, Docker |

---

## Getting Started

### Local Development

```bash
cp King.Nexa.Platform/appsettings.Local.example.json King.Nexa.Platform/appsettings.Local.json
dotnet restore
dotnet build nexa-platform.sln
dotnet run --project King.Nexa.Platform
```

Local API: `http://localhost:5068`  
Local Swagger: `http://localhost:5068/swagger`

### Docker Runtime

```bash
docker compose up --build
```

---

## Project Structure

```text
King.Nexa.Platform/
├── CatalogManagement/
├── Iam/
├── Invoicing/
├── Logistics/
├── Sales/
├── Shared/
├── TenantManagement/
├── Warehouse/
├── Migrations/
└── Program.cs
```

---

## Documentation

- [Code of Conduct](.github/CODE_OF_CONDUCT.md)
- [Contributing Guidelines](.github/CONTRIBUTING.md)
- [Security Policy](SECURITY.md)
- [Latest Release](https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-platform/releases/tag/v2.0.0)

---

<p align="center">
  <strong>Nexa Platform</strong> · Universidad Peruana de Ciencias Aplicadas · Team King · TB2 · 2026-10
</p>
