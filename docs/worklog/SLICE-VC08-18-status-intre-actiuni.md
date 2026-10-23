# SLICE-VC08-18 — mesaje între acțiunile din footer

Cerere: `vc-save-status` trebuie să stea între ieșire/renunțare și
Modifică/Salvează; apariția mesajului nu trebuie să mute butoanele.
Textul lung folosește elipsis și tooltip custom.

## Implementare locală

- `PYTHON/static/js/vercon/status-bar.js`: montare comună a barei, cu
  grupuri stânga/dreapta și zonă centrală rezervată inclusiv fără mesaj.
- `PYTHON/static/css/vercon-status.css`, importat ultimul din `vercon.css`:
  coloane max-content / minmax(0,1fr) / max-content, fără împachetarea
  mesajului sau redimensionarea butoanelor din cauza textului.
- `docs.js`, `lista.js`, `editor.js`, `mf.js`, `nomenclatoare.js`:
  mesajul precedă acțiunile Modifică/Salvează, după acțiunile din stânga;
  restricția de perioadă închisă folosește aceeași zonă centrală.
- `efactura-association.js`, `efactura-intake.js`, `depreciation.js`:
  statusul este în footer între Închide și acțiunile din dreapta.
  `vercon-efactura.css` păstrează aspectul butoanelor din noile grupuri.
- `ui.js`: toate elementele `vc-save-status`, inclusiv cele independente
  din nomenclatoare și selectoarele CPV, primesc opt-in pentru tooltip
  la trunchiere și acces prin tastatură.
- `components/tooltip/tooltip-titles.js`: opt-in generic
  `data-kbot-tooltip-overflow`; textul integral vine din conținutul actual,
  iar tooltipul custom apare numai când textul depășește lățimea disponibilă.
  Redimensionarea și înlocuirea textului închid tooltipul devenit neactual.
  Preferințele comune pentru tooltip și comportamentul touch rămân aplicate.
- `PYTHON/static/help/vercon.json`: explicație pentru mesaje și elipsis.

Nu se adaugă butoane de ieșire acolo unde ecranul nu avea asemenea acțiuni.
Mesajele existente din nomenclatoare/selectoare fără footer de acțiuni
primesc același elipsis și tooltip; nu se inventează operații suplimentare.

## Stare și limite

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
