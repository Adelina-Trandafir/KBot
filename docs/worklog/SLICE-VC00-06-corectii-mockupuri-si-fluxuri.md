# SLICE-VC00-06 — corecții mockupuri și fluxuri

Data: 12.10.2026. Cerere: citirea adnotărilor din VERCON/MOCKUPURI/Corectii_mockup-uri_01, corectarea PNG-urilor și actualizarea fluxurilor afectate.

Au fost citite toate cele șase imagini; 02–05 conțin adnotări, 01 și 06 nu au adnotări. Originalele și adnotările utilizatorului sunt păstrate. Revizia este în VERCON/MOCKUPURI/2026-10-12-corectate, cu prompturi complete și index.

Decizii preluate: antet NIR grupat, valoare factură manuală, combobox produs, MF integrat în grila produselor; DGV BC și arbori de documente în dreapta, ascunși în editor; mese Alimente în DGV, fără Alimente necesare, BC existent neasociat ales din popup, fără generare BC la salvarea listei; OI fără sidebarul perioade/deținători/efect/istoric, cu arbore familie doar în consultare, predare din cont de intrare, transfer/casare numai din produsele predătorului, casare fără primitor.

FLUXURI_UI.md descrie consultare/adăugare/modificare/salvare/Renunță și erori, distinct de anularea unui document salvat. PLAN_IMPLEMENTARE, DECIZII_DESCHISE, MAPARE_ACCESS_WEB, CONTRACT_CALCULE, PROBE_BROWSER și VERCON_STATUS au fost aliniate. Fluxul Alimente din plan nu mai prevede generarea/resalvarea BC; asocierea se revalidează și se salvează cu lista/mesele fără consum suplimentar. Nu s-a schimbat regula N01/N02, cardinalitatea DDL, migrarea sau drepturile. Setul din 11.10.2026 este marcat ca referință istorică.

Verificare: inspecție vizuală a rezultatelor și corectarea defectelor de grilă/etichete observate; semnături, dimensiuni și inventar PNG verificate la livrare. Instrument: image_gen integrat, inclusiv editările din verificare. Nu s-au executat probe browser/server și nu s-au declarat implementate aceste fluxuri în aplicație. Nu s-au modificat sursele Access sau codul aplicației pentru acest task.
