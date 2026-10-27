# SLICE-VC00-06 — corecții mockupuri și fluxuri

Data: 12.10.2026. Cerere: citirea adnotărilor din VERCON/MOCKUPURI/Corectii_mockup-uri_01, corectarea PNG-urilor și actualizarea fluxurilor afectate.

Au fost citite toate cele șase imagini; 02–05 conțin adnotări, 01 și 06 nu au adnotări. Originalele și adnotările utilizatorului sunt păstrate. Revizia este în VERCON/MOCKUPURI/2026-10-12-corectate, cu prompturi complete și index.

Decizii preluate: antet NIR grupat, valoare factură manuală, combobox produs, MF integrat în grila produselor; DGV BC și arbori de documente în dreapta, ascunși în editor; mese Alimente în DGV, fără Alimente necesare, BC existent neasociat ales din popup, fără generare BC la salvarea listei; OI fără sidebarul perioade/deținători/efect/istoric, cu arbore familie doar în consultare, predare din cont de intrare, transfer/casare numai din produsele predătorului, casare fără primitor.

FLUXURI_UI.md descrie consultare/adăugare/modificare/salvare/Renunță și erori, distinct de anularea unui document salvat. PLAN_IMPLEMENTARE, DECIZII_DESCHISE, MAPARE_ACCESS_WEB, CONTRACT_CALCULE, PROBE_BROWSER și VERCON_STATUS au fost aliniate. Fluxul Alimente din plan nu mai prevede generarea/resalvarea BC; asocierea se revalidează și se salvează cu lista/mesele fără consum suplimentar. Nu s-a schimbat regula N01/N02, cardinalitatea DDL, migrarea sau drepturile. Setul din 11.10.2026 este marcat ca referință istorică.

Verificare: inspecție vizuală a rezultatelor și corectarea defectelor de grilă/etichete observate; semnături, dimensiuni și inventar PNG verificate la livrare. Instrument: image_gen integrat, inclusiv editările din verificare. Nu s-au executat probe browser/server și nu s-au declarat implementate aceste fluxuri în aplicație. Nu s-au modificat sursele Access sau codul aplicației pentru acest task.

## SLICE-VC00-07 — corecții mockupuri, revizia 2 <a id="slice-vc00-07"></a>

12.10.2026. Autoritate: cerințele explicite ale utilizatorului din mesajul cu cinci capturi. Atașamentele sunt fragmente ale vechilor mockupuri; cerințele sunt textul mesajului, nu instrucțiuni deduse din imaginile criticate.

### Livrabile

[Index v2](../../VERCON/MOCKUPURI/2026-10-12-corectate-v2/INDEX.md), 9 PNG-uri revizuite. La cererea ulterioară a utilizatorului, cele 4 copii nemodificate (Stocuri, NIR consultare/editor, Rapoarte) au fost eliminate din folderul v2 și din arhiva sa; originalele rămân în setul anterior. Indexul, manifestul și legăturile către referințele NIR au fost actualizate. image_gen integrat, editări cu NIR ca referință vizuală; prompturile și proveniența sunt în directorul setului.

- BC consultare/editor: grilă compactă în stil NIR, fără padding mare; Adaugă rând în footer și totaluri în celule.
- ListaAlimente: rezumatele porțiilor și felurile au același aspect NIR; DGV pentru mai multe BC-uri, valoare per rând și total general. Arborele listelor în dreapta în consultare, format NIR/BC/BP; ascuns în editor.
- Popupul adaugă un bon la set, fără înlocuirea celor deja alese; eliminarea din DGV scoate numai asocierea. Setul complet se salvează atomic, fără resalvare BC sau consum repetat.
- BP/BT/BS: grile în stil NIR, Adaugă rând în footer, fără buton în titlul grilei.

### Documentație

FLUXURI_UI, PLAN_IMPLEMENTARE, DECIZII_DESCHISE, MAPARE_ACCESS_WEB, CONTRACT_CALCULE, PROBE_BROWSER și VERCON_STATUS aliniate. SCHEMA_VC/DICTIONAR_DATE disting exportul Access (IdLista unic în LegBCLA) de ținta solicitată listă 1:N BC. DDL/generatorul și serviciile necesită aliniere la această cardinalitate în VC03-02/VC07-01; nu s-au modificat aici.

Totalul BC este suma valorilor bonurilor distincte asociate; diferența = valoare listă minus total BC. Exemplul vizual 780,0000 + 660,0000 = 1.440,0000, diferență 0,0000 nu introduce obligativitatea egalității. Eligibilitatea din gestiunea Alimente și neasocierea la altă listă rămân valabile; fără duplicate în draft sau la salvare. Importul nu repară tacit legături istorice.

### Verificare și limite

Inspecție vizuală a celor nouă PNG-uri revizuite, cu corecții ulterioare ale densității/editoarelor și arborelui; verificare antete PNG, hash-uri pentru cele patru imagini păstrate, legături din index/fluxuri și conținutul ZIP. Probe browser/API planificate U14–U17, neexecutate. Nu s-au schimbat codul aplicației, SQL-ul, generatorul, sursele/exporturile Access sau bazele de date. Mockupurile nu certifică implementarea.
