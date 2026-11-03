# SLICE-VC06-33 — Parteneri: o singură apariție pentru fiecare cod fiscal

Data: 03.11.2026.

## Cerere

În pagina Parteneri din VERCON, un furnizor trebuie să apară o singură dată, grupat după cod fiscal, chiar dacă tabela Parteneri conține mai multe înregistrări ale sale pentru IdUnitate diferite. Utilizatorul aplicației nu trebuie să gestioneze ori să cunoască această multiplicare tehnică.

## Cauza și implementarea

Selectoarele și rapoartele VERCON folosesc deja proiecția fiscală din `utils/vercon_suppliers.py`. Pagina dedicată `partners.py` citea direct toate înregistrările vizibile de furnizor din Parteneri și le trimitea ca rânduri individuale.

- `PYTHON/routes/vercon/partners.py`: `_display_rows` grupează înregistrările vizibile după `suppliers.code(CodFiscal)`, folosind aceeași normalizare fiscală existentă. Variantele precum prefixul RO, spațiile și semnele nu produc apariții suplimentare ale aceleiași identități fiscale.
- Fiecare grup prezintă un rând complet și stabil din sursă. Reprezentantul păstrează ID-ul și versiunea tehnică necesare actualizării existente. Alegerea stabilă folosește cel mai mic ID din grup numai intern; lista afișată se sortează după denumire, cod partener și cod fiscal, cu chei de text românești. Nu se combină artificial adresa/IBAN-ul/banca din înregistrări diferite.
- `_present` întoarce codul fiscal normalizat și nu expune IdUnitate, lista copiilor sau numărul lor. Nu se introduc rânduri de grup expandabile ori mesaje despre multiplicarea pe unități.
- Înregistrările fără cod fiscal rămân separate. Lipsa codului fiscal nu constituie o identitate comună pentru firme diferite.
- `PYTHON/static/js/vercon/partners.js`: după reîmprospătare, selecția furnizorului se păstrează după cod fiscal, cu fallback la ID numai pentru rânduri fără cod fiscal. Căutarea, filtrele și totalul din grilă lucrează pe lista unică primită de la server.
- `PYTHON/static/help/vercon.json`: descrie lista fiscală unică și operațiile existente, fără detalii despre IdUnitate. Elimină mențiunea depășită că pagina Parteneri ar fi în pregătire; textul despre închiderea anului rămâne neschimbat.

## Limita intervenției

Numai proiecția afișată în pagina Parteneri și păstrarea selecției. Nu se șterg, reunesc ori rescriu înregistrările Parteneri. Operațiile ANAF existente păstrează contractul și ținta lor de actualizare; nu sunt extinse automat la celelalte înregistrări cu același cod fiscal. Drepturile și filtrarea furnizorilor vizibili nu sunt schimbate. Nu se modifică utilitarul fiscal comun, alte aplicații, migratoare, documente economice sau scheme MariaDB.

## Fișiere proprii și motiv pentru înregistrare după confirmare

Motiv: `Afisarea unei singure intrari pentru fiecare cod fiscal normalizat in Parteneri VERCON, fara expunerea copiilor pe IdUnitate, cu pastrarea selectiei dupa identitatea fiscala.`

- `PYTHON/routes/vercon/partners.py`
- `PYTHON/static/js/vercon/partners.js`
- `PYTHON/static/help/vercon.json`
- `docs/worklog/SLICE-VC06-33-parteneri-o-singura-aparitie-pe-cod-fiscal.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`

## Stare

**IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT, NEÎNREGISTRAT PENTRU GIT.** Analiză de design din surse. Fără teste, compilare, verificări automate de execuție, browser, HTTP, ANAF, SQL, conexiuni SSH, publicare, restart, Git sau script de înregistrare. Nu s-au citit ori modificat date reale în acest slice. Versiunile noi ale fișierelor pentru server necesită confirmarea explicită pentru înregistrare, conform AGENTS.md; înregistrările mai vechi nu au fost modificate sau șterse.

## Înregistrare la cererea explicită a utilizatorului — 03.11.2026

Cererea „scrie și mesajele pentru publicare pe server prin SSH — doar la ce ai modificat de ultima dată” a fost aplicată numai celor trei fișiere pentru server din VC06-33: `PYTHON/routes/vercon/partners.py`, `PYTHON/static/js/vercon/partners.js` și `PYTHON/static/help/vercon.json`. Scriptul `tools/Register-DeploymentTask.ps1` a raportat înregistrarea reușită a tuturor celor trei fișiere, cu SHA-256 ale versiunilor curente. Nu au fost reînregistrate fișierele celorlalte slice-uri, iar înregistrările anterioare au rămas intacte. Documentația din afara arborelui publicat nu a fost inclusă, conform limitării la fișierele pentru server.

Mesajul pentru selecția acestui slice:

```text
SLICE-VC06-33

- Afișarea unei singure intrări pentru fiecare cod fiscal normalizat în Parteneri VERCON, fără expunerea copiilor pe IdUnitate, cu păstrarea selecției după identitatea fiscală.
```

Stare actualizată: **ÎNREGISTRAT LA CERERE EXPLICITĂ, FĂRĂ PUBLICARE, SSH SAU GIT EXECUTAT.** Fișierele pentru server nu au fost modificate după înregistrare. Nu s-au executat teste ori alte verificări ale aplicației în etapa de înregistrare.
