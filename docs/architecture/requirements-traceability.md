# Nexa Requirements Traceability

Source: `nexa-ecosystem-report/report/30-chapter-3-requirements-specification/3-1-user-stories.md`.

Status values:

- `Complete`: frontend, API/model, persistence, and reasonable validation exist.
- `Partial`: capability exists, but one layer, rule, test, or REST quality remains incomplete.
- `Missing`: no concrete implementation found.
- `Out of scope`: intentionally not implemented in this project phase.
- `Needs update`: story wording lagged behind current product model.

## Summary

| Metric | Count |
|---|---:|
| Epics | 13 |
| User stories | 92 |
| Complete | 53 |
| Partial | 36 |
| Missing | 0 |
| Out of scope | 0 |
| Needs update | 3 |

## Cross-Layer Delivery Traceability

| Feature | Website CTA | Webapp View | Store/Service | Backend Endpoint | Bounded Context | Tables | Tenant Scope | Status |
|---|---|---|---|---|---|---|---|---|
| Sign-in and workspace session | Login | `/auth/login` | IAM store / IAM API | `POST /api/v1/authentication/sign-in` | IAM, Tenant Management | `users`, `user_workspace_memberships`, `workspaces` | Signed user, tenant, workspace, membership | Complete |
| Catalog | Platform and pricing CTAs | `/portal/product-catalog`, `/ops/product-catalog` | Catalog application/API | `/api/v1/catalog-items` | Catalog Management | `catalog_items`, `categories`, `brands` | Tenant for items; shared read-only metadata | Complete |
| Purchase request | Register workspace / Login | `/portal/request-builder`, commercial validation | Data store / PurchaseRequestsApi | `/api/v1/purchase-requests`, canonical transition subresources | Sales | `purchase_requests`, `purchase_request_lines`, `conversation_messages` | Tenant plus buyer client account | Complete |
| Orders | Login | buyer and commercial order views | Purchase orders application/API | `/api/v1/orders` | Sales | `orders`, `order_items`, `client_accounts` | Tenant plus normalized client account | Complete |
| Inventory | Login | `/ops/operations/inventory-control` | Inventory application/API | inventory items, lots, movements, reservations | Warehouse | `inventory_items`, `inventory_lots`, `inventory_movements`, `inventory_reservation_records` | Tenant and inventory policy on writes | Complete |
| Dispatch and evidence | Platform CTA | dispatch board/detail and buyer tracking | Dispatch application/API | dispatch orders, events, POD, temperature logs | Logistics | `dispatch_orders`, `dispatch_events`, `proof_of_delivery_records`, `temperature_logs` | Tenant; buyer sees assigned client | Complete |
| Billing and documents | Pricing CTA | payment and document views | Payments and BusinessDocuments APIs | invoices, payments, business documents | Invoicing | `invoices`, `payments`, `business_documents` | Tenant; buyer sees assigned client and authorized docs | Complete |
| Company administration | Login | company administration | Company administration store / Tenant API | tenants, workspaces, memberships, subscriptions | Tenant Management, IAM | tenant and membership tables | Tenant plus workspace-management policy | Complete |
| Public contact | Company support form | n/a | browser `mailto:` | n/a | Website | n/a | No data stored by website | Limited: user must send email |

## Matrix

| Epic | US | Capability | Frontend route/view | API endpoints | DB tables | Test coverage | Status | Gap | Action |
|---|---|---|---|---|---|---|---|---|---|
| EP01 | US01 | Value proposition | Website `/` | n/a | n/a | web smoke | Complete | none | keep |
| EP01 | US02 | Segment solutions | Website `/pages/solutions/*` | n/a | n/a | none | Complete | none | keep |
| EP01 | US03 | Platform capabilities | Website `/pages/platform.html` | n/a | n/a | none | Complete | none | keep |
| EP01 | US04 | Pricing | Website `/pages/pricing.html` | n/a | n/a | none | Complete | none | keep |
| EP01 | US05 | FAQ | Website `/pages/faq.html` | n/a | n/a | none | Complete | none | keep |
| EP01 | US06 | Contact support | Website company contact | `mailto:hello@nexa.lat` | n/a | manual browser check | Partial | user must send from email application; no CRM | keep scope explicit |
| EP02 | US07 | Authorized catalog | `/portal/product-catalog` | `/api/v1/catalog-items` | `catalog_items` | API surface | Partial | account-specific catalog authorization not deeply tested | add projection test later |
| EP02 | US08 | Product search | catalog views | `/api/v1/catalog-items` | `catalog_items` | none | Complete | client-side filter | keep |
| EP02 | US09 | Category filter | catalog views | `/api/v1/categories` | `categories` | none | Complete | none | keep |
| EP02 | US10 | Product detail | `/portal/product-catalog/:id` | `/api/v1/catalog-items/{id}` | `catalog_items` | none | Complete | none | keep |
| EP02 | US11 | Limited availability | catalog/request builder | catalog/inventory | `catalog_items`, `inventory_items` | inventory domain | Partial | buyer-safe availability projection partial | document future projection |
| EP02 | US12 | Catalog during validation | `/ops/commercial/purchase-requests/:id` | catalog + purchase requests | `catalog_items`, `purchase_requests` | none | Partial | mixed store orchestration | keep, harden later |
| EP02 | US13 | Update product | `/ops/product-catalog` | PUT catalog-items | `catalog_items` | API surface | Partial | no product policy test | add later |
| EP02 | US14 | Add product | `/ops/product-catalog` | POST catalog-items | `catalog_items` | API surface | Partial | basic validation only | keep |
| EP02 | US15 | Deactivate product | `/ops/product-catalog` | catalog update/delete | `catalog_items` | none | Partial | historical removal guard not proven | avoid destructive delete |
| EP03 | US16 | Manage promotions | `/ops/commercial/promotions` | `/api/v1/promotions` | `promotions` | policy/API surface | Complete | none | keep capability policy |
| EP03 | US17 | Buyer offers | catalog/promotions | `/api/v1/promotions` | `promotions` | none | Partial | account scoping not proven | future test |
| EP04 | US18 | Start request | `/portal/request-builder` | `/api/v1/purchase-requests` | `purchase_requests` | security integration | Complete | none | keep buyer client scope |
| EP04 | US19 | Adjust quantities | request builder | purchase request lines | `purchase_request_lines` | none | Partial | line validation thin | add test later |
| EP04 | US20 | Purchase observations | request builder | purchase requests | `purchase_requests` | none | Complete | none | keep |
| EP04 | US21 | Request summary | request builder | purchase requests | request tables | none | Complete | UI flow | keep |
| EP04 | US22 | Submit request | request builder | `POST /submissions` | `purchase_requests` | API surface | Partial | transition rule test thin | add deeper API test later |
| EP04 | US23 | Submission receipt | success/detail | purchase requests | `purchase_requests` | none | Complete | UI only | keep |
| EP04 | US24 | My requests | `/portal/purchase-requests` | purchase requests | `purchase_requests` | none | Complete | none | keep |
| EP04 | US25 | Request detail | `/portal/purchase-requests/:id` | purchase requests | `purchase_requests` | none | Complete | none | keep |
| EP04 | US26 | Buyer response | buyer request detail | messages | `conversation_messages` | none | Partial | generic resource | promote later |
| EP04 | US27 | Cancel request | request detail | `POST /cancellations` | `purchase_requests` | API surface | Partial | cancellation rule test thin | add deeper API test later |
| EP04 | US28 | Confirmed orders | `/portal/purchase-orders` | `/api/v1/orders` | `orders` | integration | Complete | none | keep |
| EP04 | US29 | Order detail | `/portal/purchase-orders/:id` | `/api/v1/orders/{id}` | `orders` | integration | Complete | none | keep |
| EP04 | US30 | Buyer delivery data | request/order forms | purchase requests/orders | delivery snapshot fields | none | Partial | geography FKs future | keep snapshots |
| EP05 | US31 | Request inbox | `/ops/commercial/purchase-requests` | purchase requests | `purchase_requests` | none | Complete | none | keep |
| EP05 | US32 | Request filters | inbox | purchase requests | `purchase_requests` | none | Complete | client filter | keep |
| EP05 | US33 | Client validation | validation view | clients + requests | `client_accounts` | none | Partial | validation rule not deeply tested | future test |
| EP05 | US34 | Availability validation | validation view | inventory/catalog | `inventory_items` | inventory domain | Partial | exact commercial rule partial | keep |
| EP05 | US35 | Request adjustment | validation view | adjustment transition | `purchase_requests` | API surface | Partial | route hardening pending | future REST test |
| EP05 | US36 | Reject request | validation view | rejection transition | `purchase_requests` | API surface | Partial | route hardening pending | future REST test |
| EP05 | US37 | Commercial validation | validation view | `POST /commercial-validations` | `purchase_requests` | domain/API | Complete | none | keep canonical route |
| EP05 | US38 | Accept request into order | validation view | `POST /acceptances` | `orders` | API surface | Complete | none | keep |
| EP05 | US39 | Confirmation communication | order detail | notifications/messages | `notification_records`, `conversation_messages` | none | Partial | notification delivery is in-app only | document limitation |
| EP05 | US40 | Buyer conversation | support/messages | conversation messages | `conversation_messages` | security integration | Complete | none | keep client scope |
| EP06 | US41 | External channel order | `/ops/commercial/manual-order-entry` | `/api/v1/orders` | `orders` | integration | Complete | none | keep |
| EP06 | US42 | Client association | manual order | `/api/v1/clients` | `client_accounts` | none | Complete | none | keep |
| EP06 | US43 | Manual lines | manual order | orders/items | `order_items` | none | Complete | none | keep |
| EP06 | US44 | Manual review | manual order | n/a | n/a | none | Complete | UI flow | keep |
| EP06 | US45 | Request origin | orders/requests | orders/requests | `orders`, `purchase_requests` | none | Partial | origin not uniformly enforced | future rule |
| EP06 | US46 | Register client | `/ops/commercial/client-accounts` | `/api/v1/clients` | `client_accounts` | none | Complete | none | keep |
| EP06 | US47 | Update client | client accounts | PUT clients | `client_accounts` | none | Complete | none | keep |
| EP06 | US48 | Client classification | client accounts | clients | `client_accounts` | none | Complete | none | keep |
| EP07 | US49 | Company data | company admin | tenants/workspaces | `tenants`, `workspaces` | security | Partial | some admin utility CRUD | harden later |
| EP07 | US50 | Internal users | company admin | users/memberships | users/memberships | security | Partial | direct UsersController | harden later |
| EP07 | US51 | Team responsibilities | company admin | memberships | memberships | security | Partial | UI partial | keep |
| EP07 | US52 | Plan capabilities | company admin | subscriptions/features | tenant tables | none | Partial | entitlement enforcement is not uniform across all modules | harden later |
| EP07 | US53 | Portal requirements | customer portals | `/api/v1/customer-portal-tasks` | `customer_portal_tasks` | policy/API surface | Complete | external portal automation excluded | keep explicit |
| EP08 | US54 | Inventory board | `/ops/operations/inventory-control` | `/api/v1/inventory-items` | `inventory_items` | domain/API | Complete | none | keep |
| EP08 | US55 | Lot detail | inventory lots | inventory items | `inventory_items` | domain | Partial | no separate lot aggregate | future |
| EP08 | US56 | Expiring lots | inventory | inventory items | `inventory_items` | none | Partial | UI filter only | future test |
| EP08 | US57 | Low stock | inventory | `/low-stock/{threshold}` | `inventory_items` | none | Complete | none | keep |
| EP08 | US58 | Inventory update | inventory | PUT inventory-items | `inventory_items` | API surface | Complete | none | keep |
| EP08 | US59 | Stock movement | inventory | stock-movements | `inventory_movements` | policy/API surface | Complete | none | keep |
| EP08 | US60 | Adjustment reason | inventory | stock-movements | `inventory_movements` | none | Partial | reason remains narrative | future value object if rules grow |
| EP08 | US61 | Commercial stock visibility | validation/order | inventory/catalog | inventory tables | domain | Partial | projection not strict | future |
| EP08 | US62 | Safe availability | catalog | catalog/inventory | catalog/inventory | none | Partial | no projection test | future |
| EP09 | US63 | Reserve inventory | inventory/order | `/reserve` | `inventory_items` | domain | Complete | none | keep |
| EP09 | US64 | Release reserve | inventory/order | `/release-reservation` | `inventory_items` | domain | Complete | none | keep |
| EP09 | US65 | FEFO | inbox/inventory | inventory | `inventory_items` | none | Partial | algorithm not proven | future |
| EP09 | US66 | Lot preparation | dispatch/inventory | dispatch/inventory | inventory/dispatch | none | Partial | lot trace weak | future |
| EP09 | US67 | Block without reservation | request/order | inventory/order | orders/inventory | none | Partial | not proven | future |
| EP10 | US68 | Create dispatch | dispatch board | `/api/v1/dispatch-orders` | `dispatch_orders` | policy/API surface | Complete | none | keep |
| EP10 | US69 | Dispatch board | `/ops/operations/dispatch-orders` | dispatch-orders | `dispatch_orders` | none | Complete | none | keep |
| EP10 | US70 | Assign responsible | dispatch detail | `/assignees` | `dispatch_orders` | domain/API | Complete | compatibility route remains | prefer canonical |
| EP10 | US71 | Schedule delivery | dispatch detail | `/schedules` | `dispatch_orders` | domain | Complete | compatibility route remains | prefer canonical |
| EP10 | US72 | Start route | dispatch detail | `/route-starts` | `dispatch_orders` | domain/API | Complete | none | keep canonical |
| EP10 | US73 | Dispatch status | dispatch detail | dispatch routes | `dispatch_orders` | domain | Complete | none | keep |
| EP10 | US74 | Incident | dispatch detail | `POST /incidents` | `dispatch_orders` | policy/API surface | Complete | compatibility route remains | keep canonical client |
| EP10 | US75 | Reprogram | dispatch detail | `POST /reschedules` | `dispatch_orders` | policy/API surface | Complete | compatibility route remains | keep canonical client |
| EP10 | US76 | Commercial logistics view | order/detail | dispatch-orders | dispatch tables | none | Complete | none | keep |
| EP10 | US77 | Buyer tracking | `/portal/purchase-orders/:id` | orders + dispatch-events | orders/dispatch | none | Complete | none | keep |
| EP11 | US78 | Temperature log | dispatch/evidence | `/api/v1/temperature-logs` | `temperature_logs` | policy/API surface | Complete | sensor ingestion excluded | keep scope explicit |
| EP11 | US79 | Temperature alert | temperature logs | `resolve-alert` | `temperature_logs`, alerts | none | Partial | alert generic | future |
| EP11 | US80 | Complete delivery | dispatch detail | `/deliveries` | `dispatch_orders` | domain | Complete | none | keep |
| EP11 | US81 | POD | `/ops/operations/proof-of-delivery` | proof-of-delivery-records | `proof_of_delivery_records` | policy/API surface | Complete | binary storage is reference-based | document limitation |
| EP11 | US82 | Buyer evidence | portal order/docs | POD/docs | POD/docs | none | Partial | buyer authorization test missing | future |
| EP11 | US83 | Dispatch analytics | `/ops/operations/operational-analytics` | mixed | mixed | none | Partial | UI aggregation | document scope |
| EP12 | US84 | Billing summary | portal payments/order | invoices/payments | `invoices`, `payments` | payment tests | Complete | none | keep |
| EP12 | US85 | Payment option | `/portal/payment-methods` | reference/payment-options + payments | `payment_options`, `payments` | payment tests | Needs update | story wording predates core payment model | updated wording |
| EP12 | US86 | Confirm payment | portal payments | `/payments/{id}/confirmations` | `payments`, `audit_logs` | domain/security | Needs update | story wording predates core payment model | updated wording |
| EP12 | US87 | Payment result | portal payments | payment status routes | `payments` | domain/security | Needs update | story wording predates core payment model | updated wording |
| EP13 | US88 | Business documents | business documents center | `/api/v1/business-documents` | `business_documents` | API/domain | Complete | none | keep |
| EP13 | US89 | Operational collections | payment view | `/api/v1/payments` | `payments` | payment tests | Complete | none | keep |
| EP13 | US90 | Visible documents | portal business docs | business-documents | `business_documents` | API surface | Partial | buyer authorization not deep tested | future |
| EP13 | US91 | Payment/document notification | portal/support | notification records | `notification_records` | none | Partial | no external delivery provider | future integration |
| EP13 | US92 | Order payment status | order/payment views | `/api/v1/payments` | `payments` | payment/security | Complete | none | keep |

## Recommended Next Hardening

1. Add buyer-safe availability projection tests.
2. Add FEFO-specific reservation tests before presenting FEFO as fully automated.
3. Define production notification delivery, backup, retention, and SLA policies before commercial deployment.
4. Remove compatibility verb routes only after all consumers and OpenAPI clients migrate.
