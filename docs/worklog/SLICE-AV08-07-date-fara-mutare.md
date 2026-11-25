# SLICE-AV08-07 — Datele și numărul facturii nu se mai schimbă la salvare

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Decizia Adelinei (24.11.2026): nimeni nu mută nimic pe ascuns. O dată greșită oprește salvarea cu un mesaj de eroare, iar numărul scris se păstrează.

## What changed and why

- `NoteC.verifica_data_document` (în `NoteC_reguli.py`) urmează regulile Access `OPuri2017_Fact.DataDocument_BeforeUpdate` și `EF_P` (mesajele 218 și 362). Refuză data din afara anului de lucru și data din perioada închisă (`EInchis`: până la ultimul document de închidere, sau un an anterior după ianuarie). Mesajul numește data («Data facturii FV 12 (05.03.2026) este într-o lună închisă …»).
- **Facturi și plăți** (`NoteC_facturi_plati.py`):
  - data facturii noi nu mai trece prin `ajusteaza_data`, care o muta pe 1 a lunii următoare sau pe 1 ianuarie; acum e refuzată;
  - data O.P. la fel;
  - numărul scris de operator se înregistrează exact. Înainte, `numar_document`, regula e-facturilor `QEF_EV`, păstra doar cifrele ultimului cuvânt, deci «FV 123/2026» devenea «2026».
- **Facturi și plăți, avertismentul «lună nouă»** (Access 217, `OPuri2017_Fact`; cerut de Adelina 24.11.2026): `NoteC.luna_noua` și `NoteC.confirma`, mutate din editor în `NoteC_reguli.py`, sunt o singură copie folosită de editor (`luna_noua_cerere`, `confirma_cerere`) și de Facturi și plăți.
  - Datele facturilor și a O.P. se verifică înaintea oricărei scrieri. O factură nouă dintr-o lună de după cea mai nouă operație a subunității și anului dă refuzul 409 `CONFIRMARE` / `LUNA_NOUA`; ruta întoarce acum corpul complet al refuzului (`NoteCEroare.corp()`).
  - Pagina (`FacturiPlatiPage.SaveAsync`) întreabă Da / Nu. **Da** retrimite cu `confirmari = [LUNA_NOUA]`. **Nu** oprește salvarea, iar ce s-a completat rămâne.
- **Contări, plata** (`NoteC_contari.py`): data O.P. dintr-o lună închisă e refuzată, nu mai e mutată. Anul e verificat mai jos, de `AN_DIFERIT`.

## Files touched

`PYTHON/routes/avacont/introducere_date/operatii/NoteC_reguli.py`, `NoteC.py`, `NoteC_facturi_plati.py`, `NoteC_contari.py`, `NoteC_editor.py`, `facturi_plati.py`; `src/KBot.Domain/Avacont/IntroducereDate/Operatii/OperatiiInfo.vb`, `src/KBot.Api/Avacont/IntroducereDate/Operatii/ApiClient.Operatii.vb`, `src/KBot.App/Avacont/IntroducereDate/Operatii/FacturiPlatiPage.vb`; ajutor (0000-182): `contabil/nomenclatoare/operatii.md`, `contabil/efactura/index.md`.

## Test results

`py_compile` fără erori; build `KBot.Api` (cu `KBot.Domain`) 0/0. Build `KBot.App` oprit de 3 erori din `NotaManualaForm.vb` (linia 266, `End Sub` în loc de `End Function`), fișier aflat în lucru la altcineva în același timp (21:12–21:20, plus `NotaManualaForm.Antet.vb` nou); `FacturiPlatiPage.vb` nu apare în erori. Nimic rulat.

## Unverified / deferred

- Evidențierea din Contări (`NoteC_contari_calcul.py`, `ajusteaza_data`) mută încă data e-facturii din luna închisă / anul anterior, ca Access `EFACTURA_2025.AjusteazaDataFacturi`. Data nouă se vede doar în coloana «Data» a tabelului, iar motivul doar în sfatul butonului «Salvează». Tot acolo, numărul documentului e luat din cifrele numărului e-facturii (`QEF_EV`), cu numărul întreg între paranteze în coloana «Document».
- Decis de Adelina (24.11.2026): mutarea datei și scurtarea numărului la evidențierea din Contări rămân, ca în Access.
