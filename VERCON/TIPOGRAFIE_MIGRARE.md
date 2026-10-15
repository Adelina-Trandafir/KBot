# Tipografie: migrarea la variabile CSS (14.10.2026)

Sursa de adevăr: `PYTHON/static/css/typography.css` (`--fs-base` = 14px, familii `--font-ui` / `--font-heading` / `--font-mono`, trepte `--fs-2xs … --fs-2xl` față de bază, roluri `--fs-control`, `--fs-label`, `--fs-table`, `--fs-hint`…). Toate dimensiunile absolute (px, rem, pt, `calc` de acestea) din `static/css` au fost înlocuite cu treapta cea mai apropiată; valorile care nu cad pe o treaptă (sub 9px sau peste 20px) au devenit `calc(var(--fs-base) ± N px)`.

**Rămase intenționat neschimbate:** dimensiunile relative (`em`, `%`: urmăresc textul din jur, deci urmăresc și baza), documentele pentru tipărire (`adechit-report*.css`, `-receipt`, `-statement`, `-payment-notice*`, `-export`, `-richtext`, `pdfview`, blocurile `@media print`; au puncte și fonturi proprii), site-ul de prezentare (`static/site/`), familiile `inherit`.

Încercare live (doar în modul de test): `kbotDebug.fontBase(16)`, `kbotDebug.fontFamily('ui', 'Arial, sans-serif')`, `kbotDebug.fontReset()`.

## Fiecare înlocuire (fișier · valoare veche · în px · nou)

| Fișier | Vechi | Echivalent | Nou |
|---|---|---|---|
| css/adechit-annual.css | 1rem | (16px) | var(--fs-lg) |
| css/adechit-bank-associations.css | 1rem | (16px) | var(--fs-lg) |
| css/adechit-bank-associations.css | calc(.74rem + 1px) | (12.84px) | var(--fs-ms) |
| css/adechit-bank-associations.css | 1rem | (16px) | var(--fs-lg) |
| css/adechit-catalog-forms.css | 1.05rem | (16.8px) | var(--fs-lg) |
| css/adechit-catalog-forms.css | 1rem | (16px) | var(--fs-lg) |
| css/adechit-catalog-forms.css | 1.2rem | (19.2px) | var(--fs-2xl) |
| css/adechit-catalogs.css | 1.2rem | (19.2px) | var(--fs-2xl) |
| css/adechit-catalogs.css | .8rem | (12.8px) | var(--fs-ms) |
| css/adechit-controls.css | 18px | (18px) | var(--fs-xl) |
| css/adechit-controls.css | 11px | (11px) | var(--fs-xs) |
| css/adechit-controls.css | 13px | (13px) | var(--fs-ms) |
| css/adechit-controls.css | 15.33px | (15.33px) | var(--fs-mp) |
| css/adechit-controls.css | 16px | (16px) | var(--fs-lg) |
| css/adechit-controls.css | 15.33px | (15.33px) | var(--fs-mp) |
| css/adechit-controls.css | 8.33px | (8.33px) | calc(var(--fs-base) - 6px) |
| css/adechit-controls.css | 10.33px | (10.33px) | var(--fs-2xs) |
| css/adechit-controls.css | 12.67px | (12.67px) | var(--fs-ms) |
| css/adechit-controls.css | 13.33px | (13.33px) | var(--fs-ms) |
| css/adechit-controls.css | 14px | (14px) | var(--fs-md) |
| css/adechit-controls.css | 14px | (14px) | var(--fs-md) |
| css/adechit-controls.css | 14px | (14px) | var(--fs-md) |
| css/adechit-help.css | .88rem | (14.08px) | var(--fs-md) |
| css/adechit-help.css | 1.7rem | (27.2px) | calc(var(--fs-base) + 13px) |
| css/adechit-help.css | 1.4rem | (22.4px) | calc(var(--fs-base) + 8px) |
| css/adechit-help.css | 1.1rem | (17.6px) | var(--fs-xl) |
| css/adechit-help.css | .95rem | (15.2px) | var(--fs-mp) |
| css/adechit-layout.css | calc(.74rem + 1px) | (12.84px) | var(--fs-ms) |
| css/adechit-layout.css | calc(.9rem + 1px) | (15.4px) | var(--fs-mp) |
| css/adechit-layout.css | calc(.74rem + 1px) | (12.84px) | var(--fs-ms) |
| css/adechit-layout.css | calc(.74rem + 1px) | (12.84px) | var(--fs-ms) |
| css/adechit-layout.css | calc(.74rem + 1px + 2pt) | (15.51px) | var(--fs-lg) |
| css/adechit-layout.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) |
| css/adechit-layout.css | 12px | (12px) | var(--fs-sm) |
| css/adechit-layout.css | calc(.72rem + 1px + 2pt) | (15.19px) | var(--fs-mp) |
| css/adechit-layout.css | calc(1rem + 2.33px) | (18.33px) | var(--fs-xl) |
| css/adechit-layout.css | calc(1.2rem + 1px) | (20.2px) | var(--fs-2xl) |
| css/adechit-layout.css | calc(.72rem + 2.33px) | (13.85px) | var(--fs-md) |
| css/adechit-layout.css | calc(.74rem + 2.33px) | (14.17px) | var(--fs-md) |
| css/adechit-layout.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) |
| css/adechit-ledger.css | calc(.75rem + 1px) | (13px) | var(--fs-ms) |
| css/adechit-ledger.css | calc(.68rem + 1px) | (11.88px) | var(--fs-sm) |
| css/adechit-ledger.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) |
| css/adechit-ledger.css | calc(1rem + 1px) | (17px) | var(--fs-xl) |
| css/adechit-parents.css | 16px | (16px) | var(--fs-lg) |
| css/adechit-parents.css | 1.8rem | (28.8px) | calc(var(--fs-base) + 15px) |
| css/adechit-parents.css | 1.2rem | (19.2px) | var(--fs-2xl) |
| css/adechit-parents.css | .9rem | (14.4px) | var(--fs-md) |
| css/adechit-parents.css | 1.6rem | (25.6px) | calc(var(--fs-base) + 12px) |
| css/adechit-parents.css | 10px | (10px) | var(--fs-2xs) |
| css/adechit-parents.css | .9rem | (14.4px) | var(--fs-md) |
| css/adechit-reports.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) |
| css/adechit-reports.css | calc(.75rem + 1px + 2pt) | (15.67px) | var(--fs-lg) |
| css/adechit-reports.css | calc(.75rem + 1px + 2pt) | (15.67px) | var(--fs-lg) |
| css/adechit-reports.css | calc(1rem + 1px) | (17px) | var(--fs-xl) |
| css/adechit-reports.css | calc(.78rem + 1px) | (13.48px) | var(--fs-ms) |
| css/adechit-workspace.css | calc(.72rem + 1px) | (12.52px) | var(--fs-ms) |
| css/app-menu.css | 13px | (13px) | var(--fs-ms) |
| css/app-menu.css | 16px | (16px) | var(--fs-lg) |
| css/combobox.css | 15px | (15px) | var(--fs-mp) |
| css/combobox.css | 13px | (13px) | var(--fs-ms) |
| css/combobox.css | 13px | (13px) | var(--fs-ms) |
| css/combobox.css | 12px | (12px) | var(--fs-sm) |
| css/combobox.css | 12px | (12px) | var(--fs-sm) |
| css/combobox.css | 11px | (11px) | var(--fs-xs) |
| css/combobox.css | 11px | (11px) | var(--fs-xs) |
| css/combobox.css | 20px | (20px) | var(--fs-2xl) |
| css/combobox.css | 22px | (22px) | calc(var(--fs-base) + 8px) |
| css/combobox.css | 18px | (18px) | var(--fs-xl) |
| css/confirm-box.css | 1.15rem | (18.4px) | var(--fs-xl) |
| css/confirm-box.css | 1.05rem | (16.8px) | var(--fs-lg) |
| css/datepicker.css | 14.33px | (14.33px) | var(--fs-md) |
| css/datepicker.css | 16px | (16px) | var(--fs-lg) |
| css/dgv.css | 10px | (10px) | var(--fs-2xs) |
| css/dgv.css | 14px | (14px) | var(--fs-md) |
| css/dgv.css | 12px | (12px) | var(--fs-sm) |
| css/inregistrare.css | 13px | (13px) | var(--fs-ms) |
| css/inregistrare.css | 20px | (20px) | var(--fs-2xl) |
| css/inregistrare.css | 13px | (13px) | var(--fs-ms) |
| css/inregistrare.css | 12px | (12px) | var(--fs-sm) |
| css/inregistrare.css | 13px | (13px) | var(--fs-ms) |
| css/inregistrare.css | 13px | (13px) | var(--fs-ms) |
| css/inregistrare.css | 15px | (15px) | var(--fs-mp) |
| css/inregistrare.css | 12px | (12px) | var(--fs-sm) |
| css/inregistrare.css | 15px | (15px) | var(--fs-mp) |
| css/inregistrare.css | 12px | (12px) | var(--fs-sm) |
| css/inregistrare.css | 14px | (14px) | var(--fs-md) |
| css/inregistrare.css | 14px | (14px) | var(--fs-md) |
| css/inregistrare.css | 14px | (14px) | var(--fs-md) |
| css/inregistrare.css | 28px | (28px) | calc(var(--fs-base) + 14px) |
| css/inregistrare.css | 18px | (18px) | var(--fs-xl) |
| css/inregistrare.css | 14px | (14px) | var(--fs-md) |
| css/monitoring.css | 1.2rem | (19.2px) | var(--fs-2xl) |
| css/operator.css | 14px | (14px) | var(--fs-md) |
| css/operator.css | 16px | (16px) | var(--fs-lg) |
| css/operator.css | 14px | (14px) | var(--fs-md) |
| css/operator.css | 14px | (14px) | var(--fs-md) |
| css/operator.css | 12px | (12px) | var(--fs-sm) |
| css/operator.css | 14px | (14px) | var(--fs-md) |
| css/operator.css | font-family: ui-monospace, 'Cascadia Mono', Consolas, monospace |  | var(--font-mono) |
| css/operator.css | 13px | (13px) | var(--fs-ms) |
| css/operator.css | font-family: ui-monospace, 'Cascadia Mono', Consolas, monospace |  | var(--font-mono) |
| css/operator.css | font-family: ui-monospace, 'Cascadia Mono', Consolas, monospace |  | var(--font-mono) |
| css/operator.css | 13px | (13px) | var(--fs-ms) |
| css/portal-angajamente.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-angajamente.css | 1.05rem | (16.8px) | var(--fs-lg) |
| css/portal-angajamente.css | 1rem | (16px) | var(--fs-lg) |
| css/portal-angajamente.css | 1.25rem | (20px) | var(--fs-2xl) |
| css/portal-angajamente.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-angajamente.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-angajamente.css | .72rem | (11.52px) | var(--fs-sm) |
| css/portal-angajamente.css | 1.4rem | (22.4px) | calc(var(--fs-base) + 8px) |
| css/portal-angajamente.css | 14px | (14px) | var(--fs-md) |
| css/portal-angajamente.css | 13px | (13px) | var(--fs-ms) |
| css/portal-angajamente.css | .95rem | (15.2px) | var(--fs-mp) |
| css/portal-angajamente.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-angajamente.css | .78rem | (12.48px) | var(--fs-sm) |
| css/portal-angajamente.css | .78rem | (12.48px) | var(--fs-sm) |
| css/portal-angajamente.css | 12px | (12px) | var(--fs-sm) |
| css/portal-angajamente.css | 10.5px | (10.5px) | var(--fs-xs) |
| css/portal-angajamente.css | .62rem | (9.92px) | var(--fs-2xs) |
| css/portal-angajamente.css | .62rem | (9.92px) | var(--fs-2xs) |
| css/portal-angajamente.css | 1.5rem | (24px) | calc(var(--fs-base) + 10px) |
| css/portal-certificate.css | .9rem | (14.4px) | var(--fs-md) |
| css/portal-certificate.css | .92rem | (14.72px) | var(--fs-mp) |
| css/portal-clasificatii.css | .88rem | (14.08px) | var(--fs-md) |
| css/portal-document-details.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-document-details.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-filters.css | 1rem | (16px) | var(--fs-lg) |
| css/portal-filters.css | .8rem | (12.8px) | var(--fs-ms) |
| css/portal-filters.css | .95rem | (15.2px) | var(--fs-mp) |
| css/portal-layout.css | font-family: Calibri, Carlito, "Segoe UI", system-ui, sans-serif |  | var(--font-ui) |
| css/portal-layout.css | 13.33px | (13.33px) | var(--fs-ms) |
| css/portal-layout.css | 14.33px | (14.33px) | var(--fs-md) |
| css/portal-layout.css | 10.33px | (10.33px) | var(--fs-2xs) |
| css/portal-layout.css | 12.33px | (12.33px) | var(--fs-sm) |
| css/portal-layout.css | 7.33px | (7.33px) | calc(var(--fs-base) - 7px) |
| css/portal-layout.css | 12px | (12px) | var(--fs-sm) |
| css/portal-layout.css | 9.33px | (9.33px) | var(--fs-2xs) |
| css/portal-layout.css | 11.67px | (11.67px) | var(--fs-sm) |
| css/portal-layout.css | .88rem | (14.08px) | var(--fs-md) |
| css/portal-layout.css | .92rem | (14.72px) | var(--fs-mp) |
| css/portal-layout.css | .9rem | (14.4px) | var(--fs-md) |
| css/portal-layout.css | 1.7rem | (27.2px) | calc(var(--fs-base) + 13px) |
| css/portal-layout.css | .9rem | (14.4px) | var(--fs-md) |
| css/portal-menu.css | .92rem | (14.72px) | var(--fs-mp) |
| css/portal-menu.css | .78rem | (12.48px) | var(--fs-sm) |
| css/portal-menu.css | .85rem | (13.6px) | var(--fs-md) |
| css/portal-signin.css | 22px | (22px) | calc(var(--fs-base) + 8px) |
| css/portal-signin.css | 17px | (17px) | var(--fs-xl) |
| css/portal-signin.css | 16px | (16px) | var(--fs-lg) |
| css/variables.css | font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif |  | var(--font-ui) |
| css/vercon-controls.css | 13px | (13px) | var(--fs-ms) |
| css/vercon-controls.css | calc(13px + 2pt) | (15.67px) | var(--fs-lg) |
| css/vercon-controls.css | calc(10.33px + 2pt) | (13px) | var(--fs-ms) |
| css/vercon-preview.css | .85rem | (13.6px) | var(--fs-md) |
| css/vercon-receptie.css | 19px | (19px) | var(--fs-2xl) |
| css/treeview/treeview_base.css | 15px | (15px) | var(--fs-mp) |
| css/treeview/treeview_base.css | 12px | (12px) | var(--fs-sm) |
| css/treeview/treeview_breadcrumbs.css | 12px | (12px) | var(--fs-sm) |
| css/treeview/treeview_breadcrumbs.css | 11px | (11px) | var(--fs-xs) |
| css/treeview/treeview_breadcrumbs.css | 11px | (11px) | var(--fs-xs) |
| css/treeview/treeview_breadcrumbs.css | 14px | (14px) | var(--fs-md) |
| css/treeview/treeview_breadcrumbs.css | 13px | (13px) | var(--fs-ms) |
| css/treeview/treeview_items.css | 12px | (12px) | var(--fs-sm) |
| css/treeview/treeview_search.css | 12px | (12px) | var(--fs-sm) |
| css/treeview/treeview_search.css | 12px | (12px) | var(--fs-sm) |
