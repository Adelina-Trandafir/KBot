# SLICE-VC00-04 — schitele machetelor si rapoartelor

Data: 11.10.2026. Registru: [VERCON_STATUS](../../VERCON/VERCON_STATUS.md).

## Ce s-a schimbat si de ce

Parser al exporturilor FORMS/REPORTS (sectiuni, controale, pozitii in twips, Caption/ControlSource/SourceObject, linii sursa) si randare PNG cu System.Drawing. 101 schite (43 formulare + 58 rapoarte) pentru obiectele INCLUS-* si DE-CLARIFICAT, plus `VaerCon`/`Main` ca referinta pentru selectorul de context. Index, trasabilitate pe fiecare control si tabelele de capabilitate pe tip (MAT/OI/MF/AL) in MAPARE_ACCESS_WEB si RAPOARTE (dependente de date transitive, parametri `Forms!`/`TempVars`).

## Fisiere atinse

- `VERCON/SCHITE/*.png` (101), `VERCON/SCHITE/INDEX.md`, `VERCON/SCHITE/trasabilitate/*.md`
- `VERCON/MAPARE_ACCESS_WEB.md`, `VERCON/RAPOARTE.md`
- `VERCON/tools/schite.js`, `schite_render.ps1`, `mapare.js`

## Rezultate / probe

- Generare fara erori (0 esecuri de parsare); `Main` are 0 controale in export (ribbon).
- Verificate vizual 4 imagini (BR, FC, NIR, OI_Predare): lizibile, recognoscibile fata de structura exportului; suprapunerile din FC sunt stari alternative de vizibilitate din Access.

## Nevalidat sau amanat

- Restul de 97 imagini nu au fost inspectate una cate una.
- Schitele nu reprezinta functionalitate implementata si nu sunt designul web final.
- Vizibilitatea setata din VBA, ribbonul, imaginile si ActiveX nu sunt recuperabile din export.
- Rapoartele BF/AFB/furnizor nu au schita (exclus tranzitiv prin Coduri).

## Revizie 11.10.2026

- **Toate cele 101 imagini au fost privite individual** (initial doar 4; esantionarea nu avea justificare tehnica). A gasit: (a) defect de randare — controale fara `Height` explicit desenate cu dimensiune 0 (`Fisa_Produsului` aparea fara date) → geometria foloseste acum `LayoutCached*`; (b) text cu `\015\012` literal → curatat; (c) pagini de tab nedesenate (`SETARI`) → desenate; (d) latime minima pentru titluri.
- Dupa corectie: 26 imagini reprivite individual; restul sunt acoperite de verificarea automata (23 controale vizibile cu dimensiune 0 in export, listate in INDEX).
- Defecte ale sursei vazute pe schite: DECIZII_DESCHISE F14. Meniul SFP integrat in RAPOARTE.md; ribbonul si regulile `Form_Open` in MAPARE_ACCESS_WEB.md.
