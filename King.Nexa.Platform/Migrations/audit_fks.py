import subprocess
import json
from collections import defaultdict

# 1. Get all tables that are tenant-scoped (have a tenant_id column)
cmd_tenant_tables = "docker exec -i nexa-postgres psql -U nexa -d nexa_platform_db -t -c \"SELECT table_name FROM information_schema.columns WHERE column_name = 'tenant_id' AND table_schema = 'public';\""
tenant_tables = set(subprocess.check_output(cmd_tenant_tables, shell=True).decode().strip().split())

# 2. Get all foreign key column mappings
query_fks = """
SELECT
    tc.table_name AS child_table,
    kcu.column_name AS child_column,
    ccu.table_name AS parent_table,
    ccu.column_name AS parent_column,
    tc.constraint_name
FROM
    information_schema.table_constraints AS tc
    JOIN information_schema.referential_constraints AS rc
      ON tc.constraint_name = rc.constraint_name
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
    JOIN information_schema.constraint_column_usage AS ccu
      ON rc.unique_constraint_name = ccu.constraint_name AND ccu.table_name = rc.unique_constraint_name -- Wait, pg unique constraint names sometimes differ, let's use a simpler query from pg_catalog if this doesn't work perfectly.
"""

# Actually, let's write a robust PG catalog query for FKs:
query_fks_robust = """
SELECT
    conname AS constraint_name,
    child_tbl.relname AS child_table,
    child_cols.attname AS child_column,
    parent_tbl.relname AS parent_table,
    parent_cols.attname AS parent_column
FROM pg_constraint con
JOIN pg_class child_tbl ON con.conrelid = child_tbl.oid
JOIN pg_class parent_tbl ON con.confrelid = parent_tbl.oid
JOIN pg_attribute child_cols ON child_cols.attrelid = con.conrelid AND child_cols.attnum = ANY(con.conkey)
JOIN pg_attribute parent_cols ON parent_cols.attrelid = con.confrelid AND parent_cols.attnum = ANY(con.confkey)
WHERE con.contype = 'f';
"""

# Let's execute the query. Note that because of the ANY(con.conkey) and JOIN pg_attribute, we need to make sure we map child_cols and parent_cols in order.
# A simpler way to get FK definition from pg_catalog is:
query_fks_simple = """
SELECT
    tc.constraint_name,
    tc.table_name AS child_table,
    kcu.column_name AS child_column,
    ccu.table_name AS parent_table,
    ccu.column_name AS parent_column
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
    JOIN information_schema.referential_constraints AS rc
      ON tc.constraint_name = rc.constraint_name
    JOIN information_schema.constraint_column_usage AS ccu
      ON rc.unique_constraint_name = ccu.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY';
"""

# Let's run query_fks_simple and group columns by constraint_name
cmd_fks = f"docker exec -i nexa-postgres psql -U nexa -d nexa_platform_db -A -F ',' -c \"{query_fks_simple}\""
fks_raw = subprocess.check_output(cmd_fks, shell=True).decode().strip().split('\n')

if not fks_raw or len(fks_raw) <= 1:
    print("No foreign keys found or query failed.")
    exit(0)

# Parse headers
headers = fks_raw[0].split(',')
rows = []
for line in fks_raw[1:]:
    if line.strip() and not line.endswith('rows)'):
        rows.append(line.split(','))

# Group columns by constraint name
constraints = defaultdict(list)
for r in rows:
    if len(r) >= 5:
        cname, child_t, child_col, parent_t, parent_col = r[0], r[1], r[2], r[3], r[4]
        constraints[cname].append({
            'child_table': child_t,
            'child_column': child_col,
            'parent_table': parent_t,
            'parent_column': parent_col
        })

print(f"Total FK constraints found in database: {len(constraints)}")

non_composite_risks = []
composite_count = 0
standard_non_tenant_count = 0

for cname, mappings in constraints.items():
    child_table = mappings[0]['child_table']
    parent_table = mappings[0]['parent_table']
    
    # Check if parent table is tenant-scoped
    parent_is_tenant = parent_table in tenant_tables
    
    # Count columns in this FK constraint
    cols_count = len(mappings)
    
    # We want to identify if it's composite
    is_composite = cols_count > 1
    
    if is_composite:
        composite_count += 1
    else:
        # It's a single-column FK. If the parent table is tenant-scoped, this is a potential risk!
        # Exceptions: if the parent table is 'tenants' itself, it's fine (since tenant_id points to tenants.id).
        # Also, check if it points to a non-tenant-scoped table like reference tables (countries, document_types, etc.)
        if parent_is_tenant and parent_table != 'tenants':
            non_composite_risks.append((cname, child_table, mappings[0]['child_column'], parent_table, mappings[0]['parent_column']))
        else:
            standard_non_tenant_count += 1

print(f"Composite foreign keys: {composite_count}")
print(f"Standard non-tenant foreign keys: {standard_non_tenant_count}")
print(f"Non-composite FK risks (pointing to tenant-owned parent tables without tenant_id composite check): {len(non_composite_risks)}")

if non_composite_risks:
    print("\nList of non-composite FK risks:")
    for risk in non_composite_risks:
        print(f"Constraint: {risk[0]} | Child: {risk[1]}.{risk[2]} -> Parent: {risk[3]}.{risk[4]}")
