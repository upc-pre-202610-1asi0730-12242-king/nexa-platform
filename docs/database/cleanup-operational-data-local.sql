-- Local-only operational data cleanup for Nexa PostgreSQL.
-- Review counts first. Execute only after explicit confirmation.

BEGIN;

SELECT 'conversation_messages' AS table_name, COUNT(*) FROM conversation_messages
UNION ALL SELECT 'notification_records', COUNT(*) FROM notification_records
UNION ALL SELECT 'payment_process_records', COUNT(*) FROM payment_process_records
UNION ALL SELECT 'payments', COUNT(*) FROM payments
UNION ALL SELECT 'invoices', COUNT(*) FROM invoices
UNION ALL SELECT 'business_documents', COUNT(*) FROM business_documents
UNION ALL SELECT 'customer_portal_tasks', COUNT(*) FROM customer_portal_tasks
UNION ALL SELECT 'proof_of_delivery_records', COUNT(*) FROM proof_of_delivery_records
UNION ALL SELECT 'temperature_logs', COUNT(*) FROM temperature_logs
UNION ALL SELECT 'dispatch_events', COUNT(*) FROM dispatch_events
UNION ALL SELECT 'dispatch_orders', COUNT(*) FROM dispatch_orders
UNION ALL SELECT 'purchase_request_lines', COUNT(*) FROM purchase_request_lines
UNION ALL SELECT 'purchase_requests', COUNT(*) FROM purchase_requests
UNION ALL SELECT 'order_items', COUNT(*) FROM order_items
UNION ALL SELECT 'orders', COUNT(*) FROM orders
UNION ALL SELECT 'workspace_resource_records.order_related', COUNT(*) FROM workspace_resource_records
WHERE resource_key IN (
  'activity-log',
  'alerts',
  'chat-threads',
  'credit-requests',
  'customer-portals',
  'dispatch-items',
  'order-timeline-events',
  'portal-requirements',
  'proof-of-delivery-uploads',
  'support-conversations'
);

DELETE FROM conversation_messages;
DELETE FROM notification_records;
DELETE FROM payment_process_records;
DELETE FROM payments;
DELETE FROM invoices;
DELETE FROM business_documents;
DELETE FROM customer_portal_tasks;
DELETE FROM proof_of_delivery_records;
DELETE FROM temperature_logs;
DELETE FROM dispatch_events;
DELETE FROM dispatch_orders;
DELETE FROM purchase_request_lines;
DELETE FROM purchase_requests;
DELETE FROM order_items;
DELETE FROM orders;
DELETE FROM workspace_resource_records
WHERE resource_key IN (
  'activity-log',
  'alerts',
  'chat-threads',
  'credit-requests',
  'customer-portals',
  'dispatch-items',
  'order-timeline-events',
  'portal-requirements',
  'proof-of-delivery-uploads',
  'support-conversations'
);

COMMIT;
