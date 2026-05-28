<div align="center">

<br/>

<img src="./docs/assets/nexa-logo.svg" alt="Nexa" width="200"/>

<br/><br/>

# nexa-platform

**Planned backend service layer and API foundation for the Nexa B2B platform**

<br/>

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12-239120?style=flat-square&logo=c-sharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat-square)
![REST API](https://img.shields.io/badge/REST%20API-domain-0EA5E9?style=flat-square)

<br/>

![Course](https://img.shields.io/badge/Course-1ASI0730%20Aplicaciones%20Web-0a2540?style=flat-square)
![Cycle](https://img.shields.io/badge/Cycle-2026--10-0a2540?style=flat-square)
![University](https://img.shields.io/badge/University-UPC-0a2540?style=flat-square)
![Team](https://img.shields.io/badge/Team-King-2a67d9?style=flat-square)
![Status](https://img.shields.io/badge/Status-AV2%20Active-22c55e?style=flat-square)

<br/>

🌐 **[View Live Site →](https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/)**

<br/>

</div>

---

## Overview

The `nexa-platform` repository houses the planned ASP.NET Core backend service layer for the Nexa platform. It establishes the domain core, repository persistence structures, and REST API controllers to support B2B order, stock lot, route, and invoice workflows.

> [!NOTE]
> This repository represents a local backend foundation and architecture skeleton. Complete integration with the public website or web application is planned for future platform milestones.

---

## Repository Map

<table>
  <tr>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-website">nexa-website</a></p>
      <p>Public landing website and central product entry point.</p>
      <p><a href="https://upc-pre-202610-1asi0730-12242-king.github.io/nexa-website/">Open Live Website</a></p>
      <p>
        <img alt="HTML5" src="https://img.shields.io/badge/HTML5-static-E34F26?style=flat-square&logo=html5&logoColor=white" />
        <img alt="CSS3" src="https://img.shields.io/badge/CSS3-responsive-1572B6?style=flat-square&logo=css3&logoColor=white" />
        <img alt="JavaScript" src="https://img.shields.io/badge/JavaScript-vanilla-F7DF1E?style=flat-square&logo=javascript&logoColor=black" />
      </p>
    </td>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-webapp">nexa-webapp</a></p>
      <p>Main web application for operational workflows and buyer-facing coordination.</p>
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-webapp/wiki">Open Engineering Wiki</a></p>
      <p>
        <img alt="Vue 3" src="https://img.shields.io/badge/Vue%203-35495E?style=flat-square&logo=vue.js&logoColor=4FC08D" />
        <img alt="Vite" src="https://img.shields.io/badge/Vite-0F172A?style=flat-square&logo=vite&logoColor=FFD62E" />
        <img alt="PrimeVue" src="https://img.shields.io/badge/PrimeVue-0EA5E9?style=flat-square" />
      </p>
    </td>
  </tr>
  <tr>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-platform">nexa-platform</a> (This Repository)</p>
      <p>Platform and backend work area for API, domain, and infrastructure concerns.</p>
      <p>
        <img alt="Platform" src="https://img.shields.io/badge/Platform-backend%20workspace-512BD4?style=flat-square" />
        <img alt="API" src="https://img.shields.io/badge/API-domain%20services-0EA5E9?style=flat-square" />
      </p>
    </td>
    <td width="50%">
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-report.">nexa-report</a></p>
      <p>Academic report, product research, backlog, architecture documentation, and project evidence.</p>
      <p><a href="https://github.com/upc-pre-202610-1asi0730-12242-king/nexa-report.">Open Report Repository</a></p>
      <p>
        <img alt="Documentation" src="https://img.shields.io/badge/Documentation-report-0F172A?style=flat-square" />
        <img alt="UPC" src="https://img.shields.io/badge/UPC-course%20evidence-0EA5E9?style=flat-square" />
      </p>
    </td>
  </tr>
</table>

---

## Platform Scope

| Domain Context | Managed Aggregate | Planned API Route | Focus Area |
|---|---|---|---|
| **Sales** | `Order` | `/api/v1/orders` | Managing B2B client order validation & queues. |
| **Logistics** | `Shipment` | `/api/v1/shipments` | Delivery routes & logistics tracking. |
| **Warehouse** | `InventoryItem` | `/api/v1/inventory-items` | Cold-storage lots stock & temperature monitoring. |
| **Invoicing** | `Invoice` | `/api/v1/invoices` | Auto-generation of invoicing records. |
| **Catalog** | `Product` | `/api/v1/products` | Cold food items & client-specific price lists. |

---

## Repository Structure

```text
nexa-platform/
├── nexa-platform.sln           # C# Visual Studio solution file
├── docs/                       # Architecture & context specifications
│   ├── assets/                 # Branding logo files
│   ├── architecture-notes.md
│   ├── bounded-contexts.md
│   └── platform-roadmap.md
└── King.Nexa.Platform/         # Main ASP.NET Core project folder
    ├── Program.cs              # Bootstrapper entry point
    ├── appsettings.json        # Solution configs & database templates
    ├── Shared/                 # Shared Kernel domain base
    ├── Sales/                  # Sales context domain layer
    ├── Logistics/              # Logistics context domain layer
    ├── Warehouse/              # Warehouse context domain layer
    ├── Invoicing/              # Invoicing context domain layer
    └── CatalogManagement/      # Catalog context domain layer
```

---

## Getting Started

### Local Setup
Ensure you have the compatible .NET SDK installed on your machine.

1. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

2. **Build the solution**:
   ```bash
   dotnet build
   ```

3. **Start the API service**:
   ```bash
   dotnet run --project King.Nexa.Platform/King.Nexa.Platform.csproj
   ```
   *The application uses local configurations. Ensure any required connection strings are adjusted inside `appsettings.json` for persistent storage validation.*

---

## Team

| Member | Focus | GitHub Identity |
|:---|:---|:---|
| **Diego Yucra** | Bootstrapping, Sales domain & GitFlow | [DiegoS284](https://github.com/DiegoS284) |
| **Gerard Rojas** | Shared Kernel, Persistence & Invoicing | [GerardRojasMancilla](https://github.com/GerardRojasMancilla) |
| **César Marín** | Logistics context & Domain documentation | [Cmarin2802](https://github.com/Cmarin2802) |
| **Gino Torrejón** | Catalog management context | [R0obxdnt-bit](https://github.com/R0obxdnt-bit) |
| **Joaquín Verde** | Warehouse stock verification | [JoaquinVerde115](https://github.com/JoaquinVerde115) |

---

<p align="center">
  <strong>Nexa Platform</strong> · Universidad Peruana de Ciencias Aplicadas · 2026-10
</p>
