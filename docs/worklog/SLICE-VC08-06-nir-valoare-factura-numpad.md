# 2026-10-19 — Separator zecimal din numpad în valoarea facturii NIR

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Valoarea reală a facturii NIR folosește TextBox cu `numberLocale: 'en-US'` (`1,234.56`). Tasta zecimală a numpad-ului românesc poate produce virgulă: parserul respinge `123,45`, blocând ajustarea.

TextBox comun tratează `NumpadDecimal` pentru `decimal` și `money`: inserează separatorul câmpului (punct pentru `en-US`, virgulă implicit), respectă cursorul/selecția și emite `input` pentru actualizarea draftului și disponibilității ajustării prin fluxul existent. Delete fără Num Lock și combinațiile cu modificatori rămân neschimbate.

Formatul afișării, validarea separatoarelor de mii și datele trimise serverului rămân cele existente. Nu se interpretează arbitrar toate virgulele ca separatoare zecimale în câmpurile cu format american.

Fișiere: `PYTHON/static/js/components/textbox/textbox.js`, acest jurnal și `implementare_vercon_v2.md`.

Numai analiză de design a surselor. Nu s-au rulat teste, build-uri sau verificări automate/vizuale, conform AGENTS.md. Nu s-a accesat MariaDB. Necesită publicarea fișierului JavaScript.
