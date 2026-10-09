# SLICE-ADE0-08 — plan pentru subunități în același DC

Data: 09.10.2026. Stare: **DOCUMENTAT LOCAL**.

Utilizatorul a cerut un plan, fără cod, pentru coexistența mai multor evidențe MDB
în același DC și comutarea între ele. A confirmat denumirea **Subunitate** și
serii și contoare de chitanțe separate pentru fiecare subunitate.

## Livrabile

- `ADECHIT/plan_subunitati.md`: model, izolarea datelor și relațiilor, maparea
  ID-urilor, configurații de chitanțe, API, selector, migrator, conversia datelor
  existente, etape și scenarii de acceptare.
- `ADECHIT_STATUS.md` și `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`:
  înregistrarea planului și actualizarea următoarei subfelii de analiză.

## Analiză și limite

Citire statică a regulilor/statusului, exporturilor locale MariaDB și componentelor
ADE Python/JS/VB relevante. Exportul `000_DEMO.sql` este datat 09.10.2026;
nu confirmă schema fiecărui DC țintă.

A fost identificată dependența temporală din `service.py` care compară numeric
`IDL + 1`. Planul cere păstrarea comportamentului sursei printr-o ordine locală
explicită înaintea remapării globale; nu autorizează schimbarea regulii de business.

Nu s-au modificat codul aplicației, scripturile SQL sau sursele MDB. Nu s-au rulat
teste, verificări vizuale, migrări ori operații pe server. Implementarea și scenariile
de acceptare rămân planificate. Denumirile concrete și exporturile țintelor se
clarifică înaintea etapelor dependente.
