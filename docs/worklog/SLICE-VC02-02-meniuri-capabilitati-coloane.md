# SLICE-VC02-02 — meniuri, capabilitati, coloane

Stare: **IMPLEMENTAT_LOCAL**. Verificat doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau testat; nimic validat pe server.

## Facut
- `routes/vercon/capabilities.py`: un singur tabel (formular → tipuri permise, drept necesar) pentru meniu SI pentru refuzul apelului direct (`GET /api/vercon/forms/<form>/open`: 404/403 `FORM_NOT_AVAILABLE`/403/409 ocupat). Sursa: ribbonul din MAPARE_ACCESS_WEB + regulile din plan.
- BC/BR doar Materiale+Alimente; Lista alimente doar Alimente; BP/BT/BS doar OI+MF; Inchidere an nu pentru OI/MF; coloanele NIR de MF (nr. inventar, data intrarii, durata) doar pentru MF; preferinta de formular salvata cu cheia tipului.
- Tip editabil doar pe gestiune goala: `POST /api/vercon/gestiune/type` (drept `configure`) → 409 `TYPE_LOCKED` dupa populare; selectorul din pagina se dezactiveaza.

## Limite
Schitele se folosesc ca ghid de campuri, nu ca machete pixel cu pixel (formularele reale vin la VC06/VC07). Operatiile `edit`/`configure` se definesc oficial la VC04-01. Formularele non-document (produse, categorii etc.) sunt doar intrari de meniu cu text de pregatire.
