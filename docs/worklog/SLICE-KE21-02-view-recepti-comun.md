# SLICE-KE21-02..04 — Un singur view de recepții, două rute

Data: 08.11.2026. Continuă [KE21-01](SLICE-KE21-01-control-angajamente.md).

## Cererea Adelinei

O singură implementare (single source of truth) pentru recepțiile legate de E-Factura. Rămân **două rute**: formularul «Facturi primite» și vederea «E-Factura» a angajamentului din shell (`KbotForm`). Fosta fereastră «Recepții <> EFACTURA» (`KeLegaForm`: legarea facturilor de o recepție FOREXE nouă, existentă sau modificată, istoric în [KE12](SLICE-KE12-receptii-pagina-in-ribbon.md)), butonul ei din ribbon și macheta `efactura_receptii` au fost **eliminate**. Codul stă în fișiere pe responsabilități (sub 500 de linii) în folderul view-ului.

## View-ul `KeReceptiiView` (`src/KBot.EFactura/Views/Receptii/`)

Trei locuri pentru gazdă (schița Adelinei): `NavPane` (bara filelor, sus), `TreePane` (arborele facturilor, stânga), `JokerPane` (stânga-jos). `JokerUse`: `Table` (tabelul de angajamente în joker), `TreeDown` (arborele coboară peste joker), `GridLeft` (grila clasificațiilor trece pe sub arbore). Restul e al view-ului: eticheta cu detaliile facturilor bifate, grila lor, «Adaugă / Asociate / Existente» cu panoul de mesaje sub el (vizibil numai la mesaje), grila clasificațiilor. Evenimente: `Committed`, `ActionStateChanged`, `BusyChanged`, `UnlinkRequested`. Fișiere: `KeReceptiiView` + `.Ribbon`, `.Invoices`, `.Viewing`, `.Angajamente`, `.Receptii`, `.Lines`, `.Amounts`, `.Commit`, `.Designer`.

## Gazdele

- **`PrimiteView`** (formularul «Facturi primite» și, prin `PrimiteAngajamentView`, vederea din shell). Cât timp fila «Recepții» e pe ecran, view-ul acoperă fereastra: arborele (cu căsuțe de bifat), bara filelor și tabelul de angajamente intră în locurile lui și se întorc la schimbarea filei.
  - **Mai multe facturi pe o recepție**: bifele dau view-ului facturile; fără bifă, factura apăsată (`PrimiteView.RecepArbore.vb`). Factura acoperită se vede doar privită, cu «Elimină asociere».
  - Butonul de propuneri pe furnizor și filtrul «fără recepții / asociate / toate» (meniul antetului): numai cu fila «Recepții» activă.
  - **În shell** (`AngajamentCode` setat): fără tabelul de angajamente, grila clasificațiilor sub arbore, fără meniul antetului (lista are toate facturile angajamentului, cele acoperite blocate).
- **Ribbon**: «Creează recepția / Leagă / Modifică recepția», «Generează ordonanțarea» și «Creează rezervare» sunt legate de view prin `PrimiteForm`, iar în shell prin `PrimiteAngajamentView` (`IViewRibbon`).
- `KBotNotice` își măsoară înălțimea și când devine vizibil sau i se schimbă lățimea (mesajul nu mai e tăiat la prima afișare).

## Verificări

`dotnet build KBot.sln`: curat. Nu există teste pentru ferestre.

## Verificare cu date reale

Fluxul a fost rulat de Adelina cu date și server reale (08.11.2026). Rămân neatinse butonul ascuns `btnOk` al view-ului (ține textul și starea butonului din panglică) și câmpul `_embedded`.
