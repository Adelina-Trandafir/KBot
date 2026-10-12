# VERCON — propunere vizuală
11 octombrie 2026. Șase mockupuri desktop PNG, cu date fictive. Generate cu instrumentul integrat image_gen și verificate vizual. Acestea descriu direcția de interfață propusă.

## Ecrane

| PNG | Scop | Bază funcțională |
|---|---|---|
| [01 — Gestiune Materiale](01-gestiune-materiale.png) | Stocuri, mișcări, variante de preț, detalii și rapoarte rapide | SFP, balanță produse, fișă de magazie |
| [02 — Recepție NIR / MF](02-nir-mijloace-fixe.png) | Antet document, produse, TVA, comisie și extensia pentru mijloace fixe | FC, TmpFactMF, R_NIR |
| [03 — Bon de consum](03-bon-consum.png) | Produse disponibile, primitor, cantități și sold după salvare | BC, TmpFactV |
| [04 — Lista de alimente](04-lista-alimente.png) | Mic dejun, dejun, cină/gustare, necesar și bon asociat | ListaAlimente, MD, D, CN, LEGBCLA |
| [05 — Transfer OI](05-transfer-obiecte-inventar.png) | Predător, primitor și efectul transferului pe soldurile deținătorilor | OI_Transfer |
| [06 — Rapoarte și previzualizare](06-rapoarte-previzualizare.png) | Catalog, filtre și document tipăribil | SFP, RAPOARTE, MF_Balanta |

## Direcția vizuală

Antetul albastru, arborele contextual, tabelele compacte și panourile laterale provin din ADECHIT. Paleta, fontul Calibri/Carlito, controalele și bordurile provin din Portal și site.css. Contextul unitate → subunitate → gestiune → exercițiu/perioadă rămâne permanent vizibil. Meniul se adaptează tipului gestiunii; MF/OI folosesc perioade din istoricul multianual și nu au bon de consum sau închidere anuală.

Paletă: antet #3b5a85; accent #309eff; text #0b2a5b; fundal #f3f9ff; antete secțiuni #e3f1ff; borduri #cfe3f8; panouri albe. Materiale/Alimente ilustrează precizia de 4 zecimale; MF/OI ilustrează 2 zecimale, ca configurații demonstrative pe gestiune.

## Surse consultate

- [Planul VERCON](../../PLAN_IMPLEMENTARE.md), în special VC02, VC06, VC07, VC08 și deciziile finale ale utilizatorului.
- [Maparea Access → web](../../MAPARE_ACCESS_WEB.md), [decizii](../../DECIZII_DESCHISE.md), [contract calcule](../../CONTRACT_CALCULE.md), [rapoarte](../../RAPOARTE.md), [integrarea sistemelor](../../INTEGRARE_SISTEME.md) și [status](../../VERCON_STATUS.md).
- [Schițele sursă](../../SCHITE/INDEX.md): FC, TmpFactMF, BC, ListaAlimente, OI_Predare, OI_Transfer, SFP și NIR.
- [ADECHIT CSS](../../../PYTHON/static/css/adechit.css), [Portal CSS](../../../PYTHON/static/css/portal.css), [paleta comună](../../../PYTHON/static/site/site.css), variabilele și structura paginilor ADECHIT/Portal/VERCON.
- Referințe vizuale ADECHIT: artifacts/ade6-05-debug.png și artifacts/ade6-06-lists.png.

## Generare și verificare

Prompturile complete și referințele folosite: [prompts.json](prompts.json). Corecțiile aplicate la verificarea vizuală: [prompts-corectii.json](prompts-corectii.json). Bonul de consum folosește „Valoare bon”, iar transferul are un singur tab „Nomenclatoare”. S-au verificat vizual toate cele șase rezultate și semnătura/dimensiunile fișierelor PNG.

Denumirile, numerele documentelor și valorile sunt exemple pentru compoziție. Rapoartele din preview sunt simplificate pentru lizibilitate; detalierea finală a coloanelor urmează catalogul funcțional. Aspectul butoanelor, dimensiunile panourilor și gruparea controalelor sunt propuneri de design.

