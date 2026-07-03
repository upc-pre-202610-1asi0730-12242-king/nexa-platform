# Database Check Constraint Hardening Plan

## Scope

This plan covers non-destructive CHECK constraints proposed after the final audit preflight.
The local database preflight found zero invalid rows for the inspected business invariants.

Evidence:

- `../Nexa-support/raw-evidence/sql-results/check-constraint-preflight.txt`

## Preflight Result

All checked invalid-row counts were `0` in the local validation database:

- purchase request line quantity is positive.
- purchase request estimated weight is not negative.
- payment amounts are not negative.
- payment process subtotal, discount, shipping, tax and total are not negative.
- invoice amount is not negative.
- inventory available and reserved quantities are not negative.
- inventory lot quantity and reserved quantity are not negative.
- inventory lot expiration date is not earlier than entry date.

## Proposed Constraints

Apply these only after running the same preflight against the target database.

```sql
ALTER TABLE purchase_request_lines
  ADD CONSTRAINT ck_purchase_request_lines_quantity_positive CHECK (quantity > 0),
  ADD CONSTRAINT ck_purchase_request_lines_estimated_weight_nonnegative CHECK (estimated_weight_kg IS NULL OR estimated_weight_kg >= 0);

ALTER TABLE payments
  ADD CONSTRAINT ck_payments_amount_nonnegative CHECK (amount >= 0);

ALTER TABLE payment_process_records
  ADD CONSTRAINT ck_payment_process_records_subtotal_nonnegative CHECK (subtotal >= 0),
  ADD CONSTRAINT ck_payment_process_records_discount_nonnegative CHECK (discount >= 0),
  ADD CONSTRAINT ck_payment_process_records_shipping_nonnegative CHECK (shipping >= 0),
  ADD CONSTRAINT ck_payment_process_records_igv_nonnegative CHECK (igv >= 0),
  ADD CONSTRAINT ck_payment_process_records_total_nonnegative CHECK (total >= 0);

ALTER TABLE invoices
  ADD CONSTRAINT ck_invoices_amount_nonnegative CHECK (amount >= 0);

ALTER TABLE inventory_items
  ADD CONSTRAINT ck_inventory_items_available_nonnegative CHECK (available_quantity >= 0),
  ADD CONSTRAINT ck_inventory_items_reserved_nonnegative CHECK (reserved_quantity >= 0);

ALTER TABLE inventory_lots
  ADD CONSTRAINT ck_inventory_lots_quantity_nonnegative CHECK (quantity >= 0),
  ADD CONSTRAINT ck_inventory_lots_reserved_nonnegative CHECK (reserved_quantity >= 0),
  ADD CONSTRAINT ck_inventory_lots_dates_valid CHECK (expiration_date IS NULL OR entry_date IS NULL OR expiration_date >= entry_date);
```

## Deferred Step

Do not apply this as a migration until the target database preflight is captured.
Adding constraints is safe only when existing data in that database passes the same checks.

## Rollback Shape

If a future migration adds these constraints, its rollback should drop only the named CHECK constraints.
It must not delete rows, truncate tables, or rewrite business data.
