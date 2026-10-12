# SLICE-VC03-01 — ierarhia si politica anuala

Stare: **IMPLEMENTAT_LOCAL**. Scripturile sunt scrise si generatorul a rulat (a produs fisierul); **nimic nu a fost aplicat pe MariaDB si nicio proba nu a fost rulata**.

## Facut
- `sql/VC_AVACONT_SURSA.sql`: `VC_Subunitati`, `VC_Gestiuni` (tip, stare, `Zec`, `Scara`, `Populated`), `VC_Exercitii` (un an per rand), `VC_ContextLock` (acces exclusiv, PK pe subunitate+gestiune, fara an).
- Triggere: tipul si scara inghetate dupa `Populated`; fara revenire la gol; OI/MF nu se inchid anual.
- Reprezentabil: mai multe gestiuni/tipuri si ani multipli in acelasi DC fara tabele separate (VERCON/SCHEMA_VC.md).

## Limite
Nimic aplicat/rulat. Alegerea `AnEvidenta` vs `DataDocument` ramane la serviciu (VC06): tabelele pastreaza ambele, distincte.

## Corectie (aceeasi zi)
Scriptul initial era gandit „o data pe fiecare DC", cu garda si triggere in proceduri. Gresit: schema se ruleaza pe AVACONT_SURSA si KBotAdmin (schema sync) o duce in DC-uri; sync-ul nu duce CHECK-uri si triggere. Refacut: un singur `sql/VC_AVACONT_SURSA.sql` (instructiuni simple) + `sql/VC_Reguli_DC.sql` optional pentru CHECK/triggere; regulile sunt impuse si de serviciu.
