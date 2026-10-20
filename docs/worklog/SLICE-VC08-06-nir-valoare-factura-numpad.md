# 2026-10-19 — Separator zecimal din numpad în valoarea facturii NIR

Stare: **SCRIS LOCAL**, nerulat și nepublicat.

Caseta valorii reale a facturii NIR folosește TextBox cu
`numberLocale: 'en-US'`: afișare `1,234.56`, cu punct zecimal.
Pe tastatura română, tasta zecimală a numpad-ului poate produce
virgulă. Parserul acestui format respinge `123,45`, iar valoarea
nu mai este disponibilă pentru ajustare.

Controlul comun TextBox tratează acum `NumpadDecimal` pentru
tipurile `decimal` și `money`: inserează separatorul câmpului,
punct pentru `en-US`, virgulă pentru formatul implicit. Inserarea
respectă poziția cursorului/selecția și emite `input`, astfel încât
draftul și disponibilitatea ajustării să se actualizeze prin fluxul
existent. Tasta Delete cu Num Lock dezactivat și combinațiile cu
modificatori își păstrează comportamentul.

Formatul afișării, validarea separatoarelor de mii și datele trimise
serverului rămân cele existente. Nu se interpretează arbitrar toate
virgulele ca separatoare zecimale în câmpurile cu format american.

Fișiere: `PYTHON/static/js/components/textbox/textbox.js`, acest jurnal
și `implementare_vercon_v2.md`.

Numai analiză de design a surselor. Nu s-au rulat teste, build-uri
sau verificări automate/vizuale, conform AGENTS.md. Nu s-a accesat
MariaDB. Necesită publicarea fișierului JavaScript.
