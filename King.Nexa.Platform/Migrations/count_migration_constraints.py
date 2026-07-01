import re

file_path = '/Users/diegosandoval284/Documents/Universidad/12242-aplicaciones-web/repositories/nexa-platform/King.Nexa.Platform/Migrations/20260629183140_AddTenantAwareCompositeForeignKeys.cs'

with open(file_path, 'r') as f:
    content = f.read()

# Find occurrences of migrationBuilder.AddUniqueConstraint
unique_constraints = re.findall(r'migrationBuilder\.AddUniqueConstraint\(\s*name:\s*"([^"]+)"', content)
print(f"Number of alternate keys (AddUniqueConstraint): {len(unique_constraints)}")
print("Alternate keys:", unique_constraints)

# Find occurrences of migrationBuilder.AddForeignKey
foreign_keys = re.findall(r'migrationBuilder\.AddForeignKey\(\s*name:\s*"([^"]+)"', content)
print(f"Number of composite foreign keys (AddForeignKey): {len(foreign_keys)}")

# Let's inspect which ones are composite (i.e. reference tenant_id and another column)
# Typically, columns: new[] { "tenant_id", ... }
composite_fks = []
matches = re.finditer(r'migrationBuilder\.AddForeignKey\(\s*name:\s*"([^"]+)"[^;]+columns:\s*new\[\]\s*\{\s*"tenant_id",\s*"([^"]+)"\s*\}', content)
for m in matches:
    composite_fks.append((m.group(1), m.group(2)))

print(f"Detected composite foreign keys: {len(composite_fks)}")
