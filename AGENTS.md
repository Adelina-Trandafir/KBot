# Project instructions

- The assistant has no authorization or access to modify MariaDB schemas. Prepare DDL and seed SQL as files for the user to execute; never execute schema changes or add automatic schema creation to application code. Applies to AVACONT_COMUN and all other MariaDB databases.
- SQL supplied for AVACONT_COMUN must use uppercase identifiers for new tables and columns.
- Do not run tests or other verification unless the user explicitly requests it. Design checks are allowed.
