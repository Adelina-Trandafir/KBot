# Tipografie: migrarea la variabile CSS (14.10.2026)

Reperul migrării din 14.10 (revizia 23.10 este mai jos): `PYTHON/static/css/typography.css` (`--fs-base` = 14px, familii `--font-ui` / `--font-heading` / `--font-mono`, trepte `--fs-2xs … --fs-2xl` față de bază, roluri `--fs-control`, `--fs-label`, `--fs-table`, `--fs-hint`…). Toate dimensiunile absolute (px, rem, pt, `calc` de acestea) din `static/css` au fost înlocuite cu treapta cea mai apropiată; valorile care nu cad pe o treaptă (sub 9px sau peste 20px) au devenit `calc(var(--fs-base) ± N px)`.

**Rămase intenționat neschimbate:** dimensiunile relative (`em`, `%`: urmăresc textul din jur, deci urmăresc și baza), documentele pentru tipărire (`adechit-report*.css`, `-receipt`, `-statement`, `-payment-notice*`, `-export`, `-richtext`, `pdfview`, blocurile `@media print`; au puncte și fonturi proprii), site-ul de prezentare (`static/site/`), familiile `inherit`.

Încercare live (doar în modul de test): `kbotDebug.fontBase(16)`, `kbotDebug.fontFamily('ui', 'Arial, sans-serif')`, `kbotDebug.fontReset()`.

## Fiecare înlocuire (fișier · valoare veche · în px · nou)

Pozițiile păstrează ordinea celor 167 de intrări originale. Rândurile identice sunt redate o dată cu toate pozițiile lor, astfel încât și multiplicitatea și succesiunea inventarului pot fi reconstruite; 131 de mapări distincte.

| Fișier | Vechi | Echivalent | Nou | Poziții în inventarul original |
|---|---|---|---|---|
| css/adechit-annual.css | 1rem | (16px) | var(--fs-lg) | 1 |
| css/adechit-bank-associations.css | 1rem | (16px) | var(--fs-lg) | 2, 4 |
| css/adechit-bank-associations.css | calc(.74rem + 1px) | (12.84px) | var(--fs-ms) | 3 |
| css/adechit-catalog-forms.css | 1.05rem | (16.8px) | var(--fs-lg) | 5 |
| css/adechit-catalog-forms.css | 1rem | (16px) | var(--fs-lg) | 6 |
| css/adechit-catalog-forms.css | 1.2rem | (19.2px) | var(--fs-2xl) | 7 |
| css/adechit-catalogs.css | 1.2rem | (19.2px) | var(--fs-2xl) | 8 |
| css/adechit-catalogs.css | .8rem | (12.8px) | var(--fs-ms) | 9 |
| css/adechit-controls.css | 18px | (18px) | var(--fs-xl) | 10 |
| css/adechit-controls.css | 11px | (11px) | var(--fs-xs) | 11 |
| css/adechit-controls.css | 13px | (13px) | var(--fs-ms) | 12 |
| css/adechit-controls.css | 15.33px | (15.33px) | var(--fs-mp) | 13, 15 |
| css/adechit-controls.css | 16px | (16px) | var(--fs-lg) | 14 |
| css/adechit-controls.css | 8.33px | (8.33px) | calc(var(--fs-base) - 6px) | 16 |
| css/adechit-controls.css | 10.33px | (10.33px) | var(--fs-2xs) | 17 |
| css/adechit-controls.css | 12.67px | (12.67px) | var(--fs-ms) | 18 |
| css/adechit-controls.css | 13.33px | (13.33px) | var(--fs-ms) | 19 |
| css/adechit-controls.css | 14px | (14px) | var(--fs-md) | 20, 21, 22 |
| css/adechit-help.css | .88rem | (14.08px) | var(--fs-md) | 23 |
| css/adechit-help.css | 1.7rem | (27.2px) | calc(var(--fs-base) + 13px) | 24 |
| css/adechit-help.css | 1.4rem | (22.4px) | calc(var(--fs-base) + 8px) | 25 |
| css/adechit-help.css | 1.1rem | (17.6px) | var(--fs-xl) | 26 |
| css/adechit-help.css | .95rem | (15.2px) | var(--fs-mp) | 27 |
| css/adechit-layout.css | calc(.74rem + 1px) | (12.84px) | var(--fs-ms) | 28, 30, 31 |
| css/adechit-layout.css | calc(.9rem + 1px) | (15.4px) | var(--fs-mp) | 29 |
| css/adechit-layout.css | calc(.74rem + 1px + 2pt) | (15.51px) | var(--fs-lg) | 32 |
| css/adechit-layout.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) | 33, 40 |
| css/adechit-layout.css | 12px | (12px) | var(--fs-sm) | 34 |
| css/adechit-layout.css | calc(.72rem + 1px + 2pt) | (15.19px) | var(--fs-mp) | 35 |
| css/adechit-layout.css | calc(1rem + 2.33px) | (18.33px) | var(--fs-xl) | 36 |
| css/adechit-layout.css | calc(1.2rem + 1px) | (20.2px) | var(--fs-2xl) | 37 |
| css/adechit-layout.css | calc(.72rem + 2.33px) | (13.85px) | var(--fs-md) | 38 |
| css/adechit-layout.css | calc(.74rem + 2.33px) | (14.17px) | var(--fs-md) | 39 |
| css/adechit-ledger.css | calc(.75rem + 1px) | (13px) | var(--fs-ms) | 41 |
| css/adechit-ledger.css | calc(.68rem + 1px) | (11.88px) | var(--fs-sm) | 42 |
| css/adechit-ledger.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) | 43 |
| css/adechit-ledger.css | calc(1rem + 1px) | (17px) | var(--fs-xl) | 44 |
| css/adechit-parents.css | 16px | (16px) | var(--fs-lg) | 45 |
| css/adechit-parents.css | 1.8rem | (28.8px) | calc(var(--fs-base) + 15px) | 46 |
| css/adechit-parents.css | 1.2rem | (19.2px) | var(--fs-2xl) | 47 |
| css/adechit-parents.css | .9rem | (14.4px) | var(--fs-md) | 48, 51 |
| css/adechit-parents.css | 1.6rem | (25.6px) | calc(var(--fs-base) + 12px) | 49 |
| css/adechit-parents.css | 10px | (10px) | var(--fs-2xs) | 50 |
| css/adechit-reports.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) | 52 |
| css/adechit-reports.css | calc(.75rem + 1px + 2pt) | (15.67px) | var(--fs-lg) | 53, 54 |
| css/adechit-reports.css | calc(1rem + 1px) | (17px) | var(--fs-xl) | 55 |
| css/adechit-reports.css | calc(.78rem + 1px) | (13.48px) | var(--fs-ms) | 56 |
| css/adechit-workspace.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) | 57 |
| css/app-menu.css | 13px | (13px) | var(--fs-ms) | 58 |
| css/app-menu.css | 16px | (16px) | var(--fs-lg) | 59 |
| css/combobox.css | 15px | (15px) | var(--fs-mp) | 60 |
| css/combobox.css | 13px | (13px) | var(--fs-ms) | 61, 62 |
| css/combobox.css | 12px | (12px) | var(--fs-sm) | 63, 64 |
| css/combobox.css | 11px | (11px) | var(--fs-xs) | 65, 66 |
| css/combobox.css | 20px | (20px) | var(--fs-2xl) | 67 |
| css/combobox.css | 22px | (22px) | calc(var(--fs-base) + 8px) | 68 |
| css/combobox.css | 18px | (18px) | var(--fs-xl) | 69 |
| css/confirm-box.css | 1.15rem | (18.4px) | var(--fs-xl) | 70 |
| css/confirm-box.css | 1.05rem | (16.8px) | var(--fs-lg) | 71 |
| css/datepicker.css | 14.33px | (14.33px) | var(--fs-md) | 72 |
| css/datepicker.css | 16px | (16px) | var(--fs-lg) | 73 |
| css/dgv.css | 10px | (10px) | var(--fs-2xs) | 74 |
| css/dgv.css | 14px | (14px) | var(--fs-md) | 75 |
| css/dgv.css | 12px | (12px) | var(--fs-sm) | 76 |
| css/inregistrare.css | 13px | (13px) | var(--fs-ms) | 77, 79, 81, 82 |
| css/inregistrare.css | 20px | (20px) | var(--fs-2xl) | 78 |
| css/inregistrare.css | 12px | (12px) | var(--fs-sm) | 80, 84, 86 |
| css/inregistrare.css | 15px | (15px) | var(--fs-mp) | 83, 85 |
| css/inregistrare.css | 14px | (14px) | var(--fs-md) | 87, 88, 89, 92 |
| css/inregistrare.css | 28px | (28px) | calc(var(--fs-base) + 14px) | 90 |
| css/inregistrare.css | 18px | (18px) | var(--fs-xl) | 91 |
| css/monitoring.css | 1.2rem | (19.2px) | var(--fs-2xl) | 93 |
| css/operator.css | 14px | (14px) | var(--fs-md) | 94, 96, 97, 99 |
| css/operator.css | 16px | (16px) | var(--fs-lg) | 95 |
| css/operator.css | 12px | (12px) | var(--fs-sm) | 98 |
| css/operator.css | font-family: ui-monospace, 'Cascadia Mono', Consolas, monospace |  | var(--font-mono) | 100, 102, 103 |
| css/operator.css | 13px | (13px) | var(--fs-ms) | 101, 104 |
| css/portal-angajamente.css | .85rem | (13.6px) | var(--fs-md) | 105, 109, 110, 116 |
| css/portal-angajamente.css | 1.05rem | (16.8px) | var(--fs-lg) | 106 |
| css/portal-angajamente.css | 1rem | (16px) | var(--fs-lg) | 107 |
| css/portal-angajamente.css | 1.25rem | (20px) | var(--fs-2xl) | 108 |
| css/portal-angajamente.css | .72rem | (11.52px) | var(--fs-sm) | 111 |
| css/portal-angajamente.css | 1.4rem | (22.4px) | calc(var(--fs-base) + 8px) | 112 |
| css/portal-angajamente.css | 14px | (14px) | var(--fs-md) | 113 |
| css/portal-angajamente.css | 13px | (13px) | var(--fs-ms) | 114 |
| css/portal-angajamente.css | .95rem | (15.2px) | var(--fs-mp) | 115 |
| css/portal-angajamente.css | .78rem | (12.48px) | var(--fs-sm) | 117, 118 |
| css/portal-angajamente.css | 12px | (12px) | var(--fs-sm) | 119 |
| css/portal-angajamente.css | 10.5px | (10.5px) | var(--fs-xs) | 120 |
| css/portal-angajamente.css | .62rem | (9.92px) | var(--fs-2xs) | 121, 122 |
| css/portal-angajamente.css | 1.5rem | (24px) | calc(var(--fs-base) + 10px) | 123 |
| css/portal-certificate.css | .9rem | (14.4px) | var(--fs-md) | 124 |
| css/portal-certificate.css | .92rem | (14.72px) | var(--fs-mp) | 125 |
| css/portal-clasificatii.css | .88rem | (14.08px) | var(--fs-md) | 126 |
| css/portal-document-details.css | .85rem | (13.6px) | var(--fs-md) | 127, 128 |
| css/portal-filters.css | 1rem | (16px) | var(--fs-lg) | 129 |
| css/portal-filters.css | .8rem | (12.8px) | var(--fs-ms) | 130 |
| css/portal-filters.css | .95rem | (15.2px) | var(--fs-mp) | 131 |
| css/portal-layout.css | font-family: Calibri, Carlito, "Segoe UI", system-ui, sans-serif |  | var(--font-ui) | 132 |
| css/portal-layout.css | 13.33px | (13.33px) | var(--fs-ms) | 133 |
| css/portal-layout.css | 14.33px | (14.33px) | var(--fs-md) | 134 |
| css/portal-layout.css | 10.33px | (10.33px) | var(--fs-2xs) | 135 |
| css/portal-layout.css | 12.33px | (12.33px) | var(--fs-sm) | 136 |
| css/portal-layout.css | 7.33px | (7.33px) | calc(var(--fs-base) - 7px) | 137 |
| css/portal-layout.css | 12px | (12px) | var(--fs-sm) | 138 |
| css/portal-layout.css | 9.33px | (9.33px) | var(--fs-2xs) | 139 |
| css/portal-layout.css | 11.67px | (11.67px) | var(--fs-sm) | 140 |
| css/portal-layout.css | .88rem | (14.08px) | var(--fs-md) | 141 |
| css/portal-layout.css | .92rem | (14.72px) | var(--fs-mp) | 142 |
| css/portal-layout.css | .9rem | (14.4px) | var(--fs-md) | 143, 145 |
| css/portal-layout.css | 1.7rem | (27.2px) | calc(var(--fs-base) + 13px) | 144 |
| css/portal-menu.css | .92rem | (14.72px) | var(--fs-mp) | 146 |
| css/portal-menu.css | .78rem | (12.48px) | var(--fs-sm) | 147 |
| css/portal-menu.css | .85rem | (13.6px) | var(--fs-md) | 148 |
| css/portal-signin.css | 22px | (22px) | calc(var(--fs-base) + 8px) | 149 |
| css/portal-signin.css | 17px | (17px) | var(--fs-xl) | 150 |
| css/portal-signin.css | 16px | (16px) | var(--fs-lg) | 151 |
| css/variables.css | font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif |  | var(--font-ui) | 152 |
| css/vercon-controls.css | 13px | (13px) | var(--fs-ms) | 153 |
| css/vercon-controls.css | calc(13px + 2pt) | (15.67px) | var(--fs-lg) | 154 |
| css/vercon-controls.css | calc(10.33px + 2pt) | (13px) | var(--fs-ms) | 155 |
| css/vercon-preview.css | .85rem | (13.6px) | var(--fs-md) | 156 |
| css/vercon-receptie.css | 19px | (19px) | var(--fs-2xl) | 157 |
| css/treeview/treeview_base.css | 15px | (15px) | var(--fs-mp) | 158 |
| css/treeview/treeview_base.css | 12px | (12px) | var(--fs-sm) | 159 |
| css/treeview/treeview_breadcrumbs.css | 12px | (12px) | var(--fs-sm) | 160 |
| css/treeview/treeview_breadcrumbs.css | 11px | (11px) | var(--fs-xs) | 161, 162 |
| css/treeview/treeview_breadcrumbs.css | 14px | (14px) | var(--fs-md) | 163 |
| css/treeview/treeview_breadcrumbs.css | 13px | (13px) | var(--fs-ms) | 164 |
| css/treeview/treeview_items.css | 12px | (12px) | var(--fs-sm) | 165 |
| css/treeview/treeview_search.css | 12px | (12px) | var(--fs-sm) | 166, 167 |

## Reviziile fontului și panoul de test — 14/23.10.2026 <a id="d001-13-tipografie"></a>

Preluare D001-13. În etapa 14.10, typography.css se încărca primul în toate aplicațiile web, fără site-ul de prezentare. Normalizarea la trepte putea schimba unele controale cu 1–2px și putea face reducerile responsive mici egale cu baza; nevăzut pe ecran atunci, numai sintaxă JS în etapa consemnată. Fontul arborilor și --tv-* sunt în [contractul TreeView](CONTROALE_CUSTOM_THREAD.md#treeview), bara operațiilor folosea --fs-xl. API-ul debug/test-mode este în contractul ModalFrame/debug.

La 23.10, baza devine valoarea curentă clamp(min, max − (start − lățime) × rată, max), cu --fs-max=16px, --fs-fit-start=1920px, --fs-fit-min=11px, --fs-fit-rate=0 (oprit, fără schimbare vizibilă). Treptele --fs-2xs…--fs-2xl sunt proporții, la 16px reproduc exact valorile vechi; se compun cu zoomul meniului prin --application-vw.

Panou numai în test-mode, inițial Aa stânga jos, Ctrl+Alt+F ori kbotDebug.fontFit(): glisoare maxim/minim/lățime de start/lățime de atingere a minimului, valori între pagini în localStorage. Copiază valorile produce liniile pentru typography.css; Resetare le scoate. **Revizia finală scoate butonul Aa** (rămâne posibil în cod, mount({ button:true })); deschidere numai prin scurtătură/debug. Fără deschidere nu rulează nimic, iar valorile de test salvate nu mai ating paginile.

Stare: **Confirmat 23.10**. Rămân de ales valorile finale/paginile aplicabile; calc(var(--fs-base) ± Npx) urmărește baza cu pas fix px. Fișiere: PYTHON/static/css/typography.css, font-fit-debug.css; PYTHON/static/js/utils/font-fit-panel.js, debug-tools.js. Inventarul de mai sus rămâne istoric, nu o listă a valorilor actuale impuse de panoul de test.
