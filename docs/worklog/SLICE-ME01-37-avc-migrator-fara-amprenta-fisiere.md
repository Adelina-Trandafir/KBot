# SLICE-ME01-37 - AVC.Migrator: the Access-file fingerprint check is removed (10.11.2026)

Slice agreed by Adelina (10.11.2026: «sunt de acord cu me01-37»). Built clean (`AVC.Migrator.vbproj`: 0 errors, 0 warnings). Nothing run.

## What changed and why

«Migreaza» stopped with «Fisierul Access `...\Scoala_NB_ISJ\baza2026.accdb` s-a modificat dupa citire. Cititi din nou si verificati.» although Adelina touched nothing. The check came from `AvcRunner` (commit a9dd16e8, 25.10.2026, not from today's work): «Citeste» took a SHA-256 of every ticked unit file, «Migreaza» compared it with the file again and refused on any difference. Its own comment says the Access driver rewrites bytes of the file header on every open/close, and the steps (E-Factura among them) open the files again in «Verifica», so the comparison can fail with no outside change. Adelina's rule: the file never changes between «Verifica» and «Migreaza», so the check is useless; it is removed.

Removed: `AvcRunner.HashOf`, the hashing at the end of `ReadAll`, the comparison at the start of `Run`, the line «Fisiere Access (SHA-256)» of the SQL journal header, `AvcContext.FileHashes`, the `System.Security.Cryptography` import. The read still reports its end to the progress bar. The reads of «Verifica» and «Migreaza» are unchanged.

What is lost: the run no longer notices an Access file that really changed between «Citeste» and «Migreaza» (the data written would then be what the steps read at that moment).

## Files touched
- `src/AVC.Migrator/AvcRunner.vb`
- `src/AVC.Migrator/AvcContext.vb`

## Unverified / deferred
- Never run. The cause of the mismatch was not proven (the file is not on the development PC; a test on a copy of another file was started and stopped), only that the check is the one that fired.
