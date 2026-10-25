# SLICE-D001-07 — Hărțile istorice E-Factura

Data: 25.10.2026. Consolidare documentară locală; nu modifică formulare Access, exporturile brute, SQL executabil sau generatoare.

## What changed and why

- În 21 de înregistrări de formular, convențiile de coordonate/Parent/LayoutCached și limitele analizei statice trimit la README. Prefixul identic de patru atribute al celor 20 de clase cu declarații este păstrat o dată în README; declarațiile suplimentare, evenimentele WithEvents și comentariile specifice rămân pe formular. Mesaj_ANAF nu primește declarații inventate.
- Șase valori RecordSource copiate identic în properties/Data sunt înlocuite în properties cu trimitere la SQL-ul integral din Data. SQL-urile distincte ale formularelor nu sunt unificate.
- navigation.md și matrix.md sunt absorbite în forms.md. Cele 22 de muchii ale grafului au fost comparate documentar cu Opens/Subforms din catalog și sunt aceleași. Punctele de intrare externe și interogările-parametru rămân integral.
- Matricea rară păstrează toate cele 25 de obiecte, ordinea celor 45 de coloane sursă și fiecare marker literal, inclusiv rwwW/rwW, fără normalizarea multiplicății/literelor. Celulele goale devin absența markerului, explicit definită; obiectele fără marker rămân.
- MF_PDF_EF.md și Note_PDF_EF.md intră în EFACTURA_PDF.md: proprietățile/secțiunile/controalele/declarările/harta handlerelor identice apar o dată. Pentru fiecare dintre cele trei forme se păstrează numele, lungimea exportului, calea brută, coordonatele (inclusiv negative), SQL-ul și apelantul distinct. Catalogul trimite la ancorele de formular.
- EF_F/EF_F_inlucru și cele două forme de asociere rămân distincte. README/overview trimit la noua structură; descrierea coloanelor catalogului este corectată (nu exista coloana Role).

## Files touched

Inventarul complet și bilanțul sunt mai jos.

## Test results

Numai lectură și comparație documentară sursă/destinație, inclusiv egalitatea secțiunilor comune ale celor trei fișe PDF. Fără teste, builduri, generator, aplicații, SQL sau acces MariaDB. Nu este reinterpretată precizia analizei statice. Stage selectiv al Markdown-ului propriu, fără commit/push.

## Unverified / deferred

Generatorul build-efactura.js rămâne neschimbat: regenerarea ar suprascrie consolidarea și ar recrea navigation.md/matrix.md, fișele PDF separate și boilerplate-ul. README consemnează limita; adaptarea generatorului nu este inclusă în lucrarea Markdown. Surse rămâne nemodificat. AVACONT, documentația activă AV, CLAUDE și ajutorul runtime sunt excluse.

| Cale | Rânduri eliminate (minus = adăugate) |
|---|---|
| `docs/access-map/efactura/forms/EFACTURA_2025.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_AA.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_ADD.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_ADD_FACTURI.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_ADD_SUB.md` | 7 |
| `docs/access-map/efactura/forms/EFACTURA_CLIENTI.md` | 7 |
| `docs/access-map/efactura/forms/EFACTURA_MSG.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_PDF.md` | -16 |
| `docs/access-map/efactura/forms/EFACTURA_PickIBAN.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_TMP.md` | 4 |
| `docs/access-map/efactura/forms/EFACTURA_UM.md` | 7 |
| `docs/access-map/efactura/forms/EFACTURA_VANZATOR.md` | 7 |
| `docs/access-map/efactura/forms/EF_F.md` | 4 |
| `docs/access-map/efactura/forms/EF_F_inlucru.md` | 4 |
| `docs/access-map/efactura/forms/EF_P.md` | 4 |
| `docs/access-map/efactura/forms/MF2019_Asoc_EF.md` | 4 |
| `docs/access-map/efactura/forms/MF_PDF_EF.md` | 75 |
| `docs/access-map/efactura/forms/Mesaj_ANAF.md` | 0 |
| `docs/access-map/efactura/forms/Note_EFactura.md` | 4 |
| `docs/access-map/efactura/forms/Note_PDF_EF.md` | 75 |
| `docs/access-map/efactura/forms/_MF2019_Asoc_EF.md` | 4 |
| `docs/access-map/efactura/forms.md` | -83 |
| `docs/access-map/efactura/navigation.md` | 74 |
| `docs/access-map/efactura/matrix.md` | 31 |
| `docs/access-map/efactura/README.md` | -25 |
| `docs/access-map/efactura/00-overview.md` | 0 |
| `docs/worklog/SLICE-D001-07-harti-istorice-efactura.md` | -58 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |

Bilanț: **29 căi; 4 fișiere retrase, 1 jurnal nou; 3 fișiere și 151 rânduri eliminate net**. Cumul D001-01…07: **30 fișiere și 4127 rânduri mai puțin**.
