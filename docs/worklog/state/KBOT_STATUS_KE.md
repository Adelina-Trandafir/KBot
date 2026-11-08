# K-BOT — STATUS, slice family KE (received invoices <-> receptions <-> payments)

Plan: [`docs/KE_PLAN.md`](../../KE_PLAN.md). KE is a family inside K-BOT, like 00EF (it joins E-Factura with Receptii and, later, Plati). Numbering KE01, KE02, ... Operator request 16.10.2026. **Initial checkpoint (16.10.2026, KE01…KE07)** — code and text only: **the SQL was not run, the server not deployed, nothing run against the live site or seen on screen, no test written or run.**

Etapele ulterioare își păstrează propriile probe și limite. Interfața inițială din checklistul de mai jos este istorică: 00EF-39 mută clasificațiile în legare, KE09 schimbă formatul și oprirea la eroare, KE11/KE12 refac interfața.

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| KE01 | Model, TVA, rute și contract | GATA pe cod (build curat, SQL nerulat, server nedeployat) | `SLICE-KE01-modelul-legaturii-si-calculele.md` | — |
| KE01-02 | Fișa diagnostică a angajamentului | GATA pe cod, **SQL nerulat** | `SLICE-KE01-02-fisa-angajament.md` | — |
| KE02 | Angajament și clasificații pe linii | GATA pe cod | `SLICE-KE02-angajament-si-clasificatii-pe-linii.md` | — |
| KE03 | Vederea Recepții | GATA pe cod | `SLICE-KE02-angajament-si-clasificatii-pe-linii.md#slice-KE03` | — |
| KE04 | Legare la recepție existentă | GATA pe cod | `SLICE-KE04-legare-de-receptie-existenta.md` | — |
| KE05 | Robot: recepție nouă | GATA pe cod, **workflow nerulat pe site** | `SLICE-KE05-robotul-si-receptie-noua-in-forexe.md` | — |
| KE06 | Recepție nouă din factură | GATA pe cod, nerulat | `SLICE-KE04-legare-de-receptie-existenta.md#slice-KE06` | — |
| KE07 | Modificarea recepției | GATA pe cod, **workflow nerulat pe site** | `SLICE-KE04-legare-de-receptie-existenta.md#slice-KE07` | — |
| KE09 | Formate FOREXE și eroarea la salvare | GATA pe cod, **workflow nerulat pe site** | `SLICE-KE05-robotul-si-receptie-noua-in-forexe.md#slice-KE09` | — |
| KE10 | Recepție comună mai multor facturi | GATA pe cod, nerulat, nevazut | `SLICE-KE04-legare-de-receptie-existenta.md#slice-KE10` | — |
| KE11 | Fereastra comună după machetă și corecțiile ei | GATA pe cod (build curat), **nevazut pe ecran**, nerulat | `SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE11` | — |
| KE12 | Pagina Recepții din ribbon | GATA pe cod (build curat), **nevazut pe ecran**, nerulat | `SLICE-KE12-receptii-pagina-in-ribbon.md` | — |
| KE12-02 | Arbore, disponibil pe zi, navigare și corecții 05…07 | GATA pe cod (build curat), **nevazut pe ecran** | `SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE12-02` | — |
| KE12-03 | Casete de bifat în KBotDataView | GATA pe cod (build curat), **nevazut pe ecran** | `SLICE-KE12-03-dgv-casete-de-bifat.md` | — |
| KE12-04 | Formular încorporat în vederile facturii | GATA pe cod (build curat), **nevazut pe ecran** | `SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE12-04` | — |
| KE13 | Toate facturile și blocarea celor acoperite | GATA pe cod (build curat), **nevazut pe ecran** | `SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE13` | — |
| KE14 | Vizualizare doar citire și eliminarea asocierii | GATA pe cod (build curat), **nevazut pe ecran** | `SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE14` | — |
| KE15 | Format regional și restul pe clasificație | GATA pe cod (build curat), **nevazut pe ecran**; cauza inițial dedusă din cod, apoi confirmată prin FX_FisaAngajament în actualizarea din 19.10 | `SLICE-KE12-receptii-pagina-in-ribbon.md#slice-KE15` | — |
| KE16 | Propuneri de legare în bloc și context furnizor | GATA pe cod (build curat, Python compilat), **server nedeployat, nerulat pe date, nevazut pe ecran** | `SLICE-KE16-propuneri-legare-in-bloc.md` | — |
| KE17 | Propuneri: angajament, sume parțiale și grupuri | GATA pe cod (build curat, Python compilat), **server nedeployat, nerulat pe date, nevazut pe ecran** | `SLICE-KE16-propuneri-legare-in-bloc.md#slice-KE17` | — |
| KE20-01 | O recepție nouă/modificată poate lua MAI MULT decât facturile bifate (surplusul rămâne doar pe recepție; tabelul clasificațiilor arată toți indicatorii; antetul facturii scade de la prima linie) | GATA pe cod (build curat, Python compilat), **server nedeployat, nerulat, nevăzut pe ecran** | `SLICE-KE20-01-valoare-peste-facturi.md` | Ajutor 0000-157. Urmează KE20-02: valoarea recepției la data facturii. |
| KE20-02 | «Recepții existente» (modul «Leagă») arată recepțiile și valoarea lor LA DATA FACTURII (cea mai veche bifată, dacă e înainte de azi); restul nealocat ține cont doar de facturile cu data până atunci | GATA pe cod (build curat, Python compilat), **server nedeployat, nerulat pe date, nevăzut pe ecran** | `SLICE-KE20-02-receptii-la-data-facturii.md` | Ajutor 0000-158. Axa de timp = instantaneele (DataH), nu DataR; o recepție făcută după factură apare cu valoarea cu care a fost făcută (operator, 06.11.2026). |
| KE20-03 | O factură fără rânduri în EF_PrimiteTVA (cele aduse din sistemul vechi) se rotunjește pe TVA-ul din antet, ca liniile ei să dea exact totalul (altfel rămânea «parțială» cu câțiva bani) | GATA pe cod (Python compilat), **server nedeployat, nerulat pe baza de date**; legătura 131 deja scrisă rămâne cu 0,03 în minus | `SLICE-KE20-03-tva-din-antet-fara-defalcare.md` | Descoperit în jurnalul AAB2MAACHXB. Rămâne deschisă propunerea min(valoarea la zi, valoarea de acum). |
| KE20-04 | Factura se leagă pe TOTALUL ei (nu pe linii): în tabelul cu facturi, un grup pe factură cu ALOCĂRI (clasificație + sumă), primul rând = clasificația din antet cu tot restul facturii, un rând gol cât timp mai e de alocat; fără produse, fără bife pe linii, fără suma scrisă pe antet; serverul verifică pe total | GATA pe cod (build curat, Python compilat), **server nedeployat, nerulat, nevăzut pe ecran** | `SLICE-KE20-04-factura-pe-total-alocari.md` | Ajutor 0000-163; control nou `GroupHeaderComboFirstRow`. Înlocuiește logica pe linii din KE02/02EF-02 pentru pagina «Recepții». |
| KE21-01 | Tabelul de angajamente devine control separat (`KeAngajamenteView`, în `Views/Receptii/`, împărțit pe responsabilități) | GATA; rulat de operator cu date reale (08.11.2026) | `SLICE-KE21-01-control-angajamente.md` | — |
| KE21-02 | `KeReceptiiView`: un singur view de recepții cu locuri pentru gazdă (bara filelor, arborele, jokerul), codul în 10 fișiere sub 470 de linii, în `Views/Receptii/` | GATA; rulat de operator cu date reale (08.11.2026) | `SLICE-KE21-02-view-recepti-comun.md` | — |
| KE21-03 | Formularul E-Factura «Facturi primite» și vederea «E-Factura» din shell încarcă același view; arbore cu bifat (mai multe facturi pe o recepție), propuneri pe furnizor, filtru, factură acoperită cu «Elimină asociere», doar cu fila «Recepții» activă; dispar fereastra «Recepții <> EFACTURA», butonul ei din ribbon și `Embed` / `DetachLines` / `DetachDate` | GATA; rulat de operator cu date reale (08.11.2026) | `SLICE-KE21-02-view-recepti-comun.md` | — |
| KE21-04 | Ribbon și shell: butoanele «Leagă», «Generează ordonanțarea», «Creează rezervare» prin `PrimiteForm` și `PrimiteAngajamentView`; în vederea unui angajament fără tabel de angajamente, grila clasificațiilor sub arbore; panoul de mesaje măsurat corect la prima afișare; curățenia; ajutorul `0000-170` | GATA; rulat de operator cu date reale (08.11.2026) | `SLICE-KE21-02-view-recepti-comun.md`, `SLICE-0000-170-ajutor-receptii-pagina-urmareste-angajamentul.md` | — |
| KE08 | Payments and payment orders | **NEPORNIT** | — | The plan leaves it for after KE01-KE07 (the payment will tie to the reception, `FX_Receptii.SumaPlatita` already exists). |

## Initial readiness checkpoint — 16.10.2026

- **KE01..KE07 are in code (16.10.2026).** To see them work, in this order: (1) run `sql/KE_01_primite_receptii.sql` on `AVACONT_SURSA` and sync it to the unit databases, deploy the server; (2) log in as a `Contabil`, open an invoice of a supplier that has a DDF, «Receptii» > «Clasificatii pe linii...» > «Leaga de receptie...» (touches nothing in FOREXE); (3) only then, on a TEST angajament and with the dry run on, «Receptie noua...» and «Modifica receptia...» (the two workflows were never run on the real site).
- Decisions of the operator (16.10.2026), all in the plan: amounts WITH VAT; storno = a normal reception with a negative sign; the classification comes from `FX_Indicatori` and must exist before the reception (else a new reservation revision first); one supplier may have several angajamente (the accountant may pick any); default reception type «Final»; only the role `Contabil` writes; no «draft» step.

## Open threads from the initial checkpoint

- The two workflows (`Receptie Noua`, `Receptie Modificata`) must be run once on a test angajament: selectors, the value format of the indicator field (assumed «1.380,00»), a possible confirmation after «Salveaza», the success element.
- Reception pictures for the ALOP file (guide p.21-22) are not taken by the robot (only by the hand-made path).
- Two receptions of one day with the same total and no K-BOT marker cannot be told apart by the site's list (the first is taken); the `RangZi` tie-break of the plan was not built (operator 16.10.2026: the marker `(IDRH; IDR)` identifies what K-BOT writes, the day does not matter; `RangZi` is already stamped by the download if it is ever needed for old receptions).
- KE06: «one reception per day on an angajament» is an OPTIONAL setting (`KeBlockSameDayReception`, off by default, «Setari > Aplicatie»), checked in the former reception window.
- KE08 (payments, payment orders).
- **Ajutor**: `0000-83` covers the text; capture `efactura-receptii` to take, `efactura-cumparare` to redo.
