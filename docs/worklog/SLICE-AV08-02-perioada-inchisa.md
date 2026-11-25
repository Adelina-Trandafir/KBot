# SLICE-AV08-02 — Perioada închisă și anul anterior, ca în Access

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Decizia Adelinei (24.11.2026): «ca în Access, inclusiv ștergerea»; excepțiile `JI` și CCV ca în Access.

## What changed and why

Citite (VERIFICAT): `EInchis` (`mdl_Functii2022.bas.txt:9`) = dată ≤ ultimul document `Fel = I`, SAU an anterior după ianuarie. Ștergerile Access (`STO_Click`, `STD_Click`) îl folosesc ca REFUZ; `Modifica_Operatie` doar avertizează (138) și deschide `Note`; `Note` blochează salvarea doar pentru documentul ≤ ultima închidere (CCV: doar la data ultimei închideri) și verifică datele la schimbare (an de lucru, `JI` scutit de luna închisă, an mai vechi de un an refuzat).

- `NoteC.perioada_inchisa` (= `EInchis`), `NoteC.este_document_inchidere`, `NoteC.verifica_data_operatie` (an de lucru, «prea vechi», luna închisă cu excepția jurnalului `JI`): singura copie a regulilor.
- Ștergerea («Modificare operații»): `NoteC.motiv_stergere` folosește `NoteC.perioada_inchisa` — regula era deja cea din Access (a rămas neschimbată ca efect).
- Editorul: dispare refuzul «Anul contabil este închis» (de la creare și editare); un an anterior închis doar avertizează (`Detalii.Avertisment`, arătat jos). Refuzul la deschidere: document ≤ ultima închidere; un document de închidere doar dacă nu e ultimul. Ultimul document de închidere (dată = ultima închidere) se editează: liniile și antetul operației, nu documentul. Datele se verifică doar când se schimbă (ca `BeforeUpdate`); jurnalul `JI` poate scrie în luna închisă. Documentul existent ales pentru o operație nouă e refuzat în luna închisă, cu excepția `JI`.

## Files touched

`PYTHON/routes/avacont/introducere_date/operatii/NoteC`, `NoteC_editor.py`; `PYTHON/routes/avacont/modificare_date/operatii/citire.py`; `src/KBot.Domain/.../NotaManualaInfo.vb` (`Inchidere`, `Avertisment`); `NotaManualaForm.vb`.

## Test results

Build App 0/0; `py_compile` fără erori. Nimic rulat.

## Unverified / deferred

- Textul mesajului Access 138 nu e în export (`tblMsg` lipsește): mesajul KBOT e scris de mine.
- Access recunoaște închiderea după `NumarDocument LIKE 'CCV*'` în `Note`, KBOT după `Fel = I` (`data_ultimei_inchideri`), ca înainte.
