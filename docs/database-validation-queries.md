# Database Validation Queries

Run schema inspection before data validation. Do not assume column names.

## Inspect Columns

```sql
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = '<table_name>'
ORDER BY ordinal_position;
```

## Inspect Indexes

```sql
SELECT schemaname, tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename IN (
    'orders',
    'purchase_requests',
    'dispatch_orders',
    'invoices',
    'payments',
    'catalog_items',
    'inventory_items',
    'inventory_lots'
  )
ORDER BY tablename, indexname;
```

## Invalid Quantity Candidates

```sql
SELECT id, tenant_id, quantity
FROM order_items
WHERE quantity <= 0;

SELECT id, tenant_id, quantity
FROM purchase_request_lines
WHERE quantity <= 0;

SELECT id, tenant_id, available_quantity, reserved_quantity
FROM inventory_items
WHERE available_quantity < 0
   OR reserved_quantity < 0
   OR reserved_quantity > available_quantity;

SELECT id, tenant_id, quantity, reserved_quantity
FROM inventory_lots
WHERE quantity < 0
   OR reserved_quantity < 0
   OR reserved_quantity > quantity;

SELECT id, tenant_id, units
FROM inventory_reservations
WHERE units <= 0;
```

## Invalid Money Candidates

```sql
SELECT id, tenant_id, total_amount
FROM orders
WHERE total_amount < 0;

SELECT id, tenant_id, amount
FROM invoices
WHERE amount < 0;

SELECT id, tenant_id, amount
FROM payments
WHERE amount < 0;

SELECT id, tenant_id, subtotal, discount, shipping, igv, total
FROM payment_process_records
WHERE subtotal < 0
   OR discount < 0
   OR shipping < 0
   OR igv < 0
   OR total < 0;
```

## Invalid Date Candidates

```sql
SELECT id, tenant_id, lot_code, entry_date, expiration_date
FROM inventory_lots
WHERE expiration_date IS NOT NULL
  AND expiration_date < entry_date;

SELECT id, tenant_id, code, valid_from, valid_until
FROM promotions
WHERE valid_until IS NOT NULL
  AND valid_from IS NOT NULL
  AND valid_until < valid_from;
```

## Tenant Relationship Checks

```sql
SELECT oi.id, oi.tenant_id, oi.order_id
FROM order_items oi
LEFT JOIN orders o ON o.id = oi.order_id AND o.tenant_id = oi.tenant_id
WHERE o.id IS NULL;

SELECT prl.id, prl.tenant_id, prl.purchase_request_id
FROM purchase_request_lines prl
LEFT JOIN purchase_requests pr
  ON pr.id = prl.purchase_request_id AND pr.tenant_id = prl.tenant_id
WHERE pr.id IS NULL;

SELECT d.id, d.tenant_id, d.order_id
FROM dispatch_orders d
LEFT JOIN orders o ON o.id = d.order_id AND o.tenant_id = d.tenant_id
WHERE o.id IS NULL;
```
