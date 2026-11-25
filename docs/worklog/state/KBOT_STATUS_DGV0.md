# K-BOT — STATUS, slice family DGV0 (shared corrections of `KBotDataView`)

Opened by the operator on 19.11.2026 («incepem slice pentru dgv astfel DGV0-01»). Changes of the shared grid control that do not belong to a feature slice.

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| DGV0-01 | **Ultima coloana ajunge pana la chenarul grilei:** la 125 % / 150 % coloanele, rotunjite fiecare pe ecran, lasau cativa pixeli goi in dreapta; coloana de umplere preia restul (`KBotDataColumn.LayoutExtraPx`, `AbsorbRoundingGap` in `KBotDataView.AutoSize.vb`) | **GATA pe cod (19.11.2026): build App 0/0; NEVAZUT pe ecran, cauza (rotunjirea) NECONFIRMATA** | `SLICE-DGV0-01-ultima-coloana-pana-la-chenar.md` | Controls 1.85.3.0. Afecteaza toate grilele cu mod de umplere; E-Factura nu a fost editata, dar foloseste acelasi control. |
| DGV0-02 | **Pictograma de filtrare doar la hover:** `KBotDataView.FilterIconVisibility` = `Always` (implicit) / `OnHover` / `OnHoverOrFiltered` (pictograma apare sub cursor si ramane pe coloanele cu filtru pus); locul ei ramane rezervat | **GATA pe cod (24.11.2026): build Controls 0/0; NEPORNIT pe nicio grila, NERULAT, NEVAZUT pe ecran** | `SLICE-DGV0-02-pictograma-filtrare-la-hover.md` | Controls 1.85.4.0. Implicit `Always` = nicio grila existenta nu se schimba. **Pornit pe `gridOperatii` din «Modificare operatii» (`OnHoverOrFiltered`, 24.11.2026); ajutor 0000-181.** |
