-- Nexa AV2 local reference seed data.
-- Idempotent: natural keys + ON CONFLICT / WHERE NOT EXISTS.
-- No DROP, TRUNCATE, or broad DELETE statements.

-- 1. Categories
INSERT INTO categories (name, description, is_active, created_at, updated_at) VALUES
('Vaccines', 'Temperature-controlled vaccine packs for B2B distribution.', true, NOW(), NULL),
('Pharmaceuticals', 'Cold-chain pharmaceutical kits and sensitive medical supplies.', true, NOW(), NULL),
('Dairy Products', 'Refrigerated dairy goods for commercial buyers.', true, NOW(), NULL),
('Frozen Foods', 'Frozen poultry, vegetables, and prepared food boxes.', true, NOW(), NULL),
('Fresh Seafood', 'Fresh and frozen seafood for horeca and retail buyers.', true, NOW(), NULL),
('Fresh Produce', 'Export-grade fruit and fresh produce crates.', true, NOW(), NULL),
('Cheese', 'Imported and refrigerated cheese catalog items.', true, NOW(), NULL),
('Charcuterie', 'Refrigerated deli meats and specialty charcuterie.', true, NOW(), NULL)
ON CONFLICT (name) DO NOTHING;

-- 2. Brands
INSERT INTO brands (name, description, is_active, created_at, updated_at) VALUES
('Andes Pharma', 'Peruvian cold-chain pharmaceutical distributor.', true, NOW(), NULL),
('Nexa Foods', 'Nexa private-label refrigerated and frozen foods.', true, NOW(), NULL),
('FreshLogix', 'Cold-chain logistics brand for fresh produce programs.', true, NOW(), NULL),
('Pacifico Seafood', 'Seafood importer serving Lima and coastal B2B buyers.', true, NOW(), NULL),
('BioCold Labs', 'Temperature-sensitive lab and medical supply producer.', true, NOW(), NULL),
('AgroAndes', 'Fresh produce exporter for Peru regional routes.', true, NOW(), NULL),
('Agriform', 'Imported cheese brand present in current catalog seed.', true, NOW(), NULL),
('Cavour', 'Charcuterie brand present in current catalog seed.', true, NOW(), NULL),
('Gestam', 'Cheese brand present in current catalog seed.', true, NOW(), NULL),
('Paysan Breton', 'Dairy brand present in current catalog seed.', true, NOW(), NULL)
ON CONFLICT (name) DO NOTHING;

-- 3. Warehouses
INSERT INTO warehouses (name, location, minimum_temperature, maximum_temperature, is_active, created_at, updated_at) VALUES
('Nexa Lima Ate Cold Hub', 'Lima - Ate', 2.00, 8.00, true, NOW(), NULL),
('Nexa Lima South Frozen Hub', 'Lima - Villa El Salvador', -22.00, -18.00, true, NOW(), NULL),
('Nexa Callao Import Hub', 'Callao - Ventanilla', -22.00, 8.00, true, NOW(), NULL),
('Nexa Arequipa Regional Hub', 'Arequipa - Cerro Colorado', 2.00, 8.00, true, NOW(), NULL),
('Nexa Trujillo North Hub', 'Trujillo - Moche', 2.00, 8.00, true, NOW(), NULL),
('Nexa Chiclayo Coastal Hub', 'Chiclayo - Pimentel', -22.00, 8.00, true, NOW(), NULL),
('Nexa Piura Fresh Hub', 'Piura - Castilla', 2.00, 8.00, true, NOW(), NULL),
('Nexa Cusco Pharma Hub', 'Cusco - Wanchaq', 2.00, 8.00, true, NOW(), NULL),
('Nexa Ica Produce Hub', 'Ica - Parcona', 2.00, 8.00, true, NOW(), NULL),
('Nexa Tacna Border Hub', 'Tacna - Pocollay', -22.00, 8.00, true, NOW(), NULL)
ON CONFLICT (location) DO NOTHING;

-- 4. Catalog Items
INSERT INTO catalog_items (catalog_item_id, product_id, item_name, brand_name, category_name, description, image_url, unit_price_amount, unit_price_currency, available_stock, cold_chain_requirement, is_active, created_at, updated_at)
SELECT * FROM (
    SELECT 'AV2-CAT-0001' AS catalog_item_id, 'AV2-PROD-0001' AS product_id, 'Influenza Vaccine Pack' AS item_name, 'Andes Pharma' AS brand_name, 'Vaccines' AS category_name, 'Validated refrigerated vaccine pack for clinic replenishment.' AS description, 'https://nexa.local/assets/catalog/influenza-vaccine-pack.jpg' AS image_url, 185.00 AS unit_price_amount, 'PEN' AS unit_price_currency, 140 AS available_stock, 'Refrigerated' AS cold_chain_requirement, true AS is_active, NOW() AS created_at, NULL::timestamp with time zone AS updated_at
    UNION ALL SELECT 'AV2-CAT-0002', 'AV2-PROD-0002', 'Insulin Cold Storage Kit', 'BioCold Labs', 'Pharmaceuticals', 'Insulin handling kit with controlled cold-chain packaging.', 'https://nexa.local/assets/catalog/insulin-cold-storage-kit.jpg', 240.00, 'PEN', 95, 'Refrigerated', true, NOW(), NULL
    UNION ALL SELECT 'AV2-CAT-0003', 'AV2-PROD-0003', 'Fresh Salmon Box', 'Pacifico Seafood', 'Fresh Seafood', 'Fresh salmon box for premium horeca buyers.', 'https://nexa.local/assets/catalog/fresh-salmon-box.jpg', 165.00, 'PEN', 80, 'Refrigerated', true, NOW(), NULL
    UNION ALL SELECT 'AV2-CAT-0004', 'AV2-PROD-0004', 'Frozen Chicken Breast', 'Nexa Foods', 'Frozen Foods', 'Frozen chicken breast carton for regional distributors.', 'https://nexa.local/assets/catalog/frozen-chicken-breast.jpg', 115.00, 'PEN', 170, 'Frozen', true, NOW(), NULL
    UNION ALL SELECT 'AV2-CAT-0005', 'AV2-PROD-0005', 'Greek Yogurt Crate', 'Nexa Foods', 'Dairy Products', 'Refrigerated Greek yogurt crate for supermarket replenishment.', 'https://nexa.local/assets/catalog/greek-yogurt-crate.jpg', 68.00, 'PEN', 220, 'Refrigerated', true, NOW(), NULL
    UNION ALL SELECT 'AV2-CAT-0006', 'AV2-PROD-0006', 'Blueberry Export Pack', 'AgroAndes', 'Fresh Produce', 'Fresh blueberry pack for export-grade cold-chain routes.', 'https://nexa.local/assets/catalog/blueberry-export-pack.jpg', 92.00, 'PEN', 180, 'Refrigerated', true, NOW(), NULL
    UNION ALL SELECT 'AV2-CAT-0007', 'AV2-PROD-0007', 'Shrimp Frozen Bag', 'Pacifico Seafood', 'Fresh Seafood', 'Frozen shrimp bag for food service distribution.', 'https://nexa.local/assets/catalog/shrimp-frozen-bag.jpg', 145.00, 'PEN', 120, 'Frozen', true, NOW(), NULL
    UNION ALL SELECT 'AV2-CAT-0008', 'AV2-PROD-0008', 'Pharmaceutical Starter Kit', 'Andes Pharma', 'Pharmaceuticals', 'Starter pharmaceutical kit for clinic onboarding orders.', 'https://nexa.local/assets/catalog/pharmaceutical-starter-kit.jpg', 310.00, 'PEN', 70, 'Refrigerated', true, NOW(), NULL
) AS src
WHERE NOT EXISTS (SELECT 1 FROM catalog_items c WHERE c.catalog_item_id = src.catalog_item_id);

-- 5. Inventory Items
INSERT INTO inventory_items (product_id, catalog_item_id, available_quantity, reserved_quantity, warehouse_location, minimum_temperature, maximum_temperature, created_at, updated_at)
SELECT * FROM (
    SELECT 'AV2-PROD-0001' AS product_id, 'AV2-CAT-0001' AS catalog_item_id, 132 AS available_quantity, 8 AS reserved_quantity, 'Lima - Ate' AS warehouse_location, 2.00 AS minimum_temperature, 8.00 AS maximum_temperature, NOW() AS created_at, NULL::timestamp with time zone AS updated_at
    UNION ALL SELECT 'AV2-PROD-0002', 'AV2-CAT-0002', 88, 7, 'Cusco - Wanchaq', 2.00, 8.00, NOW(), NULL
    UNION ALL SELECT 'AV2-PROD-0003', 'AV2-CAT-0003', 74, 6, 'Callao - Ventanilla', 0.00, 4.00, NOW(), NULL
    UNION ALL SELECT 'AV2-PROD-0004', 'AV2-CAT-0004', 156, 14, 'Lima - Villa El Salvador', -22.00, -18.00, NOW(), NULL
    UNION ALL SELECT 'AV2-PROD-0005', 'AV2-CAT-0005', 204, 16, 'Trujillo - Moche', 2.00, 8.00, NOW(), NULL
    UNION ALL SELECT 'AV2-PROD-0006', 'AV2-CAT-0006', 168, 12, 'Ica - Parcona', 2.00, 8.00, NOW(), NULL
    UNION ALL SELECT 'AV2-PROD-0007', 'AV2-CAT-0007', 112, 8, 'Chiclayo - Pimentel', -22.00, -18.00, NOW(), NULL
    UNION ALL SELECT 'AV2-PROD-0008', 'AV2-CAT-0008', 64, 6, 'Arequipa - Cerro Colorado', 2.00, 8.00, NOW(), NULL
) AS src
WHERE NOT EXISTS (SELECT 1 FROM inventory_items i WHERE i.catalog_item_id = src.catalog_item_id);

-- 6. Verification Queries
SELECT COUNT(*) AS users_count FROM users;
SELECT COUNT(*) AS categories_count FROM categories;
SELECT COUNT(*) AS brands_count FROM brands;
SELECT COUNT(*) AS warehouses_count FROM warehouses;
SELECT COUNT(*) AS catalog_items_count FROM catalog_items;
SELECT COUNT(*) AS inventory_items_count FROM inventory_items;
