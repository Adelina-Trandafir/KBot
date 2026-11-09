# SLICE-VC06-41 — Filtrul de afișare în NIR din eFactura

09.11.2026 — Implementat local.

Butonul „Afișare: Active” apela `commitEdit()` la fiecare click. Grila returnează `false` când nu are o celulă în editare, iar acțiunea se oprea înainte să deschidă opțiunile. Butonul cere acum salvarea numai dacă există o editare activă. După alegerea Active, Inactive sau Toate, lista se reîncarcă cu filtrul ales.

Fișierele slice-ului sunt `PYTHON/static/js/vercon/efactura-queue-dialog.js`, acest worklog, `VERCON/VERCON_STATUS.md` și indexul `implementare_vercon_v2.md`.

Adelina a confirmat proba și a autorizat înregistrarea prin `$cm`. Agentul a analizat sursele.
