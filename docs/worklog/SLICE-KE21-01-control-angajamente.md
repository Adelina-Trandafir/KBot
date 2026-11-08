# SLICE-KE21-01 — Tabelul de angajamente, control separat

Data: 08.11.2026. Face parte din KE21 ([KE21-02..04](SLICE-KE21-02-view-recepti-comun.md)).

Adelina a cerut ca recepțiile legate de E-Factura să aibă o singură implementare, oricare ar fi ruta. Tabelul de angajamente (angajamentele asociate partenerului facturii selectate) **nu stă în view**: gazda îl pune sub arbore și îi dă view-ului angajamentul curent.

## Ce s-a făcut

Control nou `KeAngajamenteView` în `src/KBot.EFactura/Views/Receptii/`, pe responsabilități: `KeAngajamenteView.vb` (starea, interfața `IKeAngajamenteHost`, evenimentele `AngajamentChanging` / `Ready` / `Cleared` / `OthersLoaded` / `RefillRequested`), `.Fill` (rândurile: candidate, legate, adăugate, rândul gol; varianta «doar privit»), `.Grid` (formatarea celulelor, schimbarea rândului, butonul de ștergere), `.Load` (datele unui angajament și ale celorlalte), `.Link` (adăugarea unui angajament prin CUI-ul furnizorului și scoaterea asocierii), `.Designer`. View-ul de recepții implementează `IKeAngajamenteHost` (furnizorii facturilor, citirea din nou a stărilor, scoaterea legăturilor manuale, mesajele).

Comportamentul nu s-a schimbat pe ecran, deci ajutorul nu s-a atins.

## Verificare

Fluxul cu date reale a fost rulat de Adelina (08.11.2026). Starea «doar privit» se ia la umplere (`FillViewed`), nu din starea curentă a ferestrei.
