# SLICE-VC06-42 — Înlocuirea și eliminarea asocierilor NIR–eFactura

09.11.2026 — Implementat local.

- NIR-urile anulate sau dispărute eliberează cantitățile asociate pe poziții. O factură fără alocări valabile poate fi asociată unui NIR salvat sau preluată din nou prin „NIR din e-Factura…”. La o nouă preluare integrală, legăturile expirate ale vechilor alocări se curăță în tranzacția de salvare. Marcajul „Inactiv” continuă să controleze filtrul de afișare.
- Meniul din arbore grupează „Asociază eFactura…”, „Asociază cu altă eFactură”, „Elimină asocierea cu eFactura” și „Afișează eFactura” înaintea separatorului. Comenzile de înlocuire și eliminare apar numai pentru un NIR cu asociere activă. Dacă sunt mai multe facturi pe același NIR, se alege legătura vizată.
- Înlocuirea folosește filtrul existent după CUI și data facturii. Serverul verifică revizia NIR, legătura veche și cantitățile, apoi elimină legătura veche și scrie alocările noi în aceeași tranzacție. Cheia de cerere permite reluarea aceleiași salvări. „Elimină asocierea cu eFactura” cere confirmare și elimină numai legătura aleasă.
- Cantitățile rezervate de alte NIR-uri active rămân asociate. Pentru preluările integrale vechi repartizate pe mai multe NIR-uri, factura se eliberează când dispare ultima legătură activă. Aceste preluări nu au alocări pe poziții din care să se poată calcula restul.

Fișiere proprii:

- `PYTHON/routes/vercon/efactura_association.py`
- `PYTHON/routes/vercon/efactura_queue.py`
- `PYTHON/static/js/vercon/docs.js`
- `PYTHON/static/js/vercon/efactura-actions.js`
- `PYTHON/static/js/vercon/efactura-association.js`
- `PYTHON/static/js/vercon/efactura-view.js`
- `docs/worklog/SLICE-VC06-42-inlocuire-eliminare-asociere-nir-efactura.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`

Adelina a confirmat proba și a autorizat înregistrarea prin `$cm`. Agentul a analizat sursele.
