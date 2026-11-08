# SLICE-KE20-05 — View-uri separate pentru Nouă, Existente și Asociate

Data: 06.11.2026.

## Cererea Adelinei

Zona „Adaugă / Existente / Asociate” a recepțiilor trebuie să poată fi editată vizual. Fiecare element are propriul view și propriul designer; câmpurile și grilele nu se mai suprapun în același panel. Navbarul conține toate cele trei elemente vizibile în designer. La rulare se păstrează regulile existente: „Asociate” se ascunde fără asocieri, iar „Adaugă” se ascunde pentru facturile integral asociate.

## Implementare

- `PrimiteReceptieNouaView`: tip, dată, descriere. Emite evenimentul schimbării datei către fosta fereastră de recepții.
- `PrimiteReceptiiExistenteView`: alegerea acțiunii și grila recepțiilor existente. Emite evenimentele alegerii acțiunii și selecției recepției.
- `PrimiteReceptiiAsociateView`: grilă proprie pentru asocieri și butonul eliminării asocierii, folosit în vederea unei facturi acoperite. Nu reutilizează grila sau selecția view-ului Existente.
- Fiecare view are `.vb`, `.Designer.vb` și `.resx`. Se deschide fișierul său `.vb` cu **View Designer / Shift+F7**; structura controalelor se editează acolo.
- În fosta fereastră de recepții, cele trei view-uri ocupă celule distincte în `tlyRecViews`; toate sunt prezente în arborele designerului. La rulare navigarea ascunde view-urile inactive și închide coloanele lor prin API-ul `KBotTableLayoutPanel`; view-ul selectat primește întreaga lățime. Nu se mută controale între view-uri și nu se folosesc straturi suprapuse.
- Toate cele trei elemente ale `navMod` sunt vizibile în designer. Constructorul de rulare aplică `UpdateAssociatedTab`; constructorul destinat designerului nu aplică regulile dependente de date. Calculele `k_has`, `k_full`, `IsCovered` și regulile existente de ascundere sunt păstrate.
- Cele două grile de recepții păstrează date calendaristice ca `Date`, cu formatul `dd.MM.yyyy` în designer, și sume numerice. Încărcarea inițială ordonează cronologic și apoi după descriere în cultura română; mecanismul existent al grilelor gestionează sortarea aleasă pe ecran.
- Integrarea comună a paginii de recepții și a recepțiilor din E-Factura s-a mutat în KE21: ambele încarcă același `KeReceptiiView` (`DetachDate()` / `EmbedDate()` au dispărut).
- Dimensiunile și amprenta DPI 144 existente în fosta fereastră de recepții sunt păstrate. `FileVersion` al componentei este incrementat la `1.14.2.2`, cu `AssemblyVersion` neschimbat.
- În sursele existente înaintea intervenției, `lblTitluDate` lipsea deja din designer, dar era încă referit în codul formularului. Eliminarea controlului din machetă este păstrată; referințele rămase din aceeași zonă sunt eliminate.

## Fișiere proprii și motive pentru înregistrare

Înregistrarea se execută numai după confirmarea explicită a Adelinei, conform `AGENTS.md` și `docs/deployment/README.md`.

| Fișier relativ | Motiv concret |
| --- | --- |
| fosta fereastră de recepții | Conectarea celor trei view-uri separate și selectarea view-ului activ, cu păstrarea regulilor existente de asociere. |
| fosta fereastră de recepții | Înlocuirea controalelor suprapuse cu trei view-uri în celule distincte și afișarea tuturor elementelor navbarului în designer. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptieNouaView.vb` | View separat pentru recepția nouă și transmiterea schimbării datei către formular. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptieNouaView.Designer.vb` | Câmpurile recepției noi și dispunerea lor editabilă în designerul propriu. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptieNouaView.resx` | Resursa WinForms a view-ului recepției noi. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptiiExistenteView.vb` | View separat pentru recepțiile existente și evenimentele alegerii acțiunii și recepției. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptiiExistenteView.Designer.vb` | Selectorul acțiunii și grila recepțiilor existente în designerul propriu. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptiiExistenteView.resx` | Resursa WinForms a view-ului recepțiilor existente. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptiiAsociateView.vb` | View separat pentru recepțiile asociate și evenimentul eliminării asocierii. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptiiAsociateView.Designer.vb` | Grila proprie a asocierilor și butonul eliminării în designerul propriu. |
| `src/KBot.EFactura/Views/Primite/PrimiteReceptiiAsociateView.resx` | Resursa WinForms a view-ului recepțiilor asociate. |
| `src/KBot.EFactura/KBot.EFactura.vbproj` | Metadatele Visual Studio pentru designerii noilor view-uri și incrementarea versiunii componentei. |
| `docs/worklog/SLICE-KE20-05-view-uri-date-receptie-designer.md` | Documentarea separării view-urilor, a modului de editare și a fișierelor proprii ale taskului. |

## Stare

Implementat în surse. Revizuire de proiectare prin citirea codului și a diferențelor față de fișierele existente la începutul intervenției. Fără compilare, teste, deschiderea designerului Visual Studio, rularea aplicației sau publicare, conform instrucțiunii de a nu executa verificări fără cerere explicită.

Taskul așteaptă proba și confirmarea explicită a Adelinei. Scriptul `Register-DeploymentTask.ps1` nu a fost executat; taskul nu este închis în registrul de deployment.
