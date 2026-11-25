# SLICE-VC24-03 — Contorul după schimbarea ultimului număr

24.11.2026 — După schimbarea numărului unui document, VERCON actualizează `VC_Numbering` la cel mai mare număr activ rămas dacă numărul vechi era maximul seriei. De exemplu, schimbarea ultimei liste din 10 în 9 duce contorul la 9 și următorul număr automat la 10.

Se folosește calculul maximului existent la anulare. Actualizarea se face în aceeași tranzacție cu schimbarea documentului, din comanda **Modifică număr** sau la salvarea editării. Se aplică seriilor BC, BX, BP, BT, BS și Liste Alimente, în gestiunea și exercițiul documentului. Pentru BC cu numerotare pe cont, se recalculează seria contului din care s-a eliberat numărul. NIR și FCS calculează propunerea din seria comună a documentelor salvate.

Fișierele lucrării sunt `PYTHON/routes/vercon/archive.py`, `number_change.py`, `documents.py`, `lista.py` și `oi.py`, plus `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md` și acest worklog.

Implementat local. Proba în aplicație și publicarea sunt în așteptare. Teste nerulate.
