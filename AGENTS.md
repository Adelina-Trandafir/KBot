# Project instructions

- Orice lucrare se face pe un slice de forma `SLICE-wwnn-nn`, unde fiecare `w` este o literă sau o cifră, iar fiecare `n` este o cifră.
- **REGULA 0 — Limita intervenției:** Dacă o modificare ar afecta mai mult decât scopul discutat și autorizat cu utilizatorul, asistentul se oprește și îl anunță înainte de a continua. Nu extinde automat modificarea la alte tabele, module, documente sau fluxuri.
- The assistant has no authorization or access to modify MariaDB schemas. Prepare DDL and seed SQL as files for the user to execute; never execute schema changes or add automatic schema creation to application code. Applies to AVACONT_COMUN and all other MariaDB databases.
- SQL supplied for AVACONT_COMUN must use uppercase identifiers for new tables and columns.
- Do not run tests or other verification unless the user explicitly requests it. Design checks are allowed.
- În toate aplicațiile, datele afișate se ordonează după tipul real: texte alfabetic în limba română, date calendaristice cronologic, numere numeric (inclusiv zecimale și valori negative), niciodată după ID-ul intern sau șirul de afișare. Regula se aplică arborilor la fiecare nivel, listelor, selectoarelor, grilelor și filtrelor, inclusiv după încărcare lazy, căutare și reîmprospătare. Etichetele compuse folosesc chei de sortare explicite. Sensul crescător/descrescător ales în ecran se păstrează. În machetele documentelor, rândurile de total, antetele și structura impusă explicit se păstrează în poziția lor.

## Help screenshots

- Mask sensitive data in future help screenshots with strong blur, not opaque gray rectangles. Cover names, identifiers, accounts, contact details, financial and attendance data, and sensitive chat content. Keep labels and controls readable. Save only processed screenshots in the project and delete temporary originals after processing.
- Existing VERCON screenshots retain their current masking unless the user asks to revisit them.
