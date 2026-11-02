# SLICE-VC08-18 — mesaje între acțiunile din footer

Cerere: `vc-save-status` trebuie să stea între ieșire/renunțare și Modifică/Salvează; apariția mesajului nu trebuie să mute butoanele. Textul lung folosește elipsis și tooltip custom.

## Implementare locală

- `PYTHON/static/js/vercon/status-bar.js`: montare comună a barei, cu grupuri stânga/dreapta și zonă centrală rezervată inclusiv fără mesaj.
- `PYTHON/static/css/vercon-status.css`, importat ultimul din `vercon.css`: coloane max-content / minmax(0,1fr) / max-content, fără împachetarea mesajului sau redimensionarea butoanelor din cauza textului.
- `docs.js`, `lista.js`, `editor.js`, `mf.js`, `nomenclatoare.js`: mesajul precedă acțiunile Modifică/Salvează, după acțiunile din stânga; restricția de perioadă închisă folosește aceeași zonă centrală.
- `efactura-association.js`, `efactura-intake.js`, `depreciation.js`: statusul este în footer între Închide și acțiunile din dreapta. `vercon-efactura.css` păstrează aspectul butoanelor din noile grupuri.
- `ui.js`: toate elementele `vc-save-status`, inclusiv cele independente din nomenclatoare și selectoarele CPV, primesc opt-in pentru tooltip la trunchiere și acces prin tastatură.
- `components/tooltip/tooltip-titles.js`: opt-in `data-kbot-tooltip-overflow`, tooltip custom cu textul integral actual numai la depășirea lățimii. Redimensionarea/înlocuirea textului închid tooltipul neactual; preferințele comune și comportamentul touch se păstrează.
- `PYTHON/static/help/vercon.json`: explicație pentru mesaje și elipsis.

Niciun ecran nu primește butoane de ieșire inexistente anterior. Nomenclatoarele/selectoarele fără footer primesc același elipsis și tooltip, fără operații suplimentare.

## Stare și limite

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
