# SLICE-VC24-02 — Afișarea numărului propus la Liste Alimente

24.11.2026 — La deschiderea unei liste noi, câmpul **Număr listă** afișează următorul număr al seriei din gestiunea și exercițiul curent. Formularul citește seria `lista` din `/api/vercon/numbering`, aceeași sursă folosită în **Setări gestiune**.

Propunerea apare ca text orientativ în câmpul gol. Dacă operatorul introduce un număr, se trimite valoarea introdusă. Cu câmpul gol, serverul atribuie numărul disponibil la salvare. Dacă propunerea nu poate fi citită, câmpul afișează «Automat».

Fișierele lucrării sunt `PYTHON/static/js/vercon/lista.js`, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md` și acest worklog.

Implementat local. Proba în aplicație și publicarea sunt în așteptare. Teste nerulate.
