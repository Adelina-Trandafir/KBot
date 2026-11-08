# K-BOT — STATUS, slices 0130–0139

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0130 — Borduri liniștite în Classic și Modern

Operator, 08.11.2026: în Classic și Modern chenarul exterior al controalelor = ActiveBorder (dacă nu e fixat de operator); separatoarele de antet / subsol din DGV și arbore = ActiveBorder în Classic, albastrul actual în Modern; marginile interioare ale DGV (celule, antete) = InactiveBorder; bara de navigare: linia de subsol = ActiveBorder în Classic, albastrul actual în Modern; Dark nu se schimbă.

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0130-01 | Regula în `ThemeScheme.OuterBorder` / `InnerLine`, aplicată controalelor, grilelor, arborilor și listei de navigare; pinurile manuale scoase din designere | GATA pe cod (build 0 erori); nevăzut pe ecran; fără ajutor (vezi «Ajutor de actualizat») | [0130-01](../SLICE-0130-01-borduri-linistite-classic-modern.md) |

### Open threads — 0130

- Nevăzut pe ecran în nicio temă. Revizie 08.11.2026: culorile inversate față de prima variantă (InactiveBorder #F4F7FC e mai deschis decât ActiveBorder #B4B4B4); liniile interioare ale grilelor folosesc acum `ThemeScheme.SoftBorder` #D2D2D2 (InactiveBorder era prea deschis).
- Butoanele (`KBotButton`, `ModernRenderer`) și `KBotCheckBox` nu au fost atinse (au lucrare proprie în 0028-10).
- `FileVersion` neridicat; nimic în `NOUTATI.md`; nu s-a atins niciun fișier care ajunge pe server.
