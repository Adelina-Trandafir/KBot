# PYTHON (Flask API + web apps) - rules

Apply on top of the root `CLAUDE.md`. (Written without diacritics, RULE 0.)

## REGULA 00 - config.py NU ESTE ATINS NICIODATA

ORICE schimbare posibila a `config.py` PE SERVER este STRICT INTERZISA: nicio trimitere, copiere, suprascriere, editare sau stergere, prin niciun script. `PYTHON/config.py` de pe acest PC este un MOCK; cel real exista doar pe server. Acelasi lucru pentru alte fisiere cu secrete (`*.env`, `admin_settings.json`, chei). `tools/Push-Changed.ps1` le exclude in cod (trei verificari) si nu are comutator care sa ridice regula. O setare care cere `config.py` (de ex. `TEST_MODE`) o pune operatorul, de mana, pe server.

## CSS / code structure

- CSS is written in separate `.css` files, apart from HTML, JavaScript and Python. No `<style>` blocks, no stylesheet strings in JS, no `style` attributes for styling. Dynamic values a control needs (sizes, positioning, CSS variables) go through the control's API.
- Each application has one entry stylesheet with ordered imports, and CSS modules by responsibility: structure/context/navigation, pages or document families, nomenclatures, reports/preview, and responsive adaptations. Do not pile every page into a single CSS file.
- CSS of shared controls stays next to their components. App-specific adaptations go in that app's CSS, with selectors limited to that app.
- When splitting an existing stylesheet, keep the cascade order, selectors, values, media queries and relative URL resolution. Do not mix the reorganization with visual changes nobody asked for.
- No own code module (Python, JS, CSS, HTML) should exceed about 1000 lines. When one gets close, split it by responsibility, with explicit interfaces. Do not compress code or move lines around arbitrarily to hide an overrun. Third-party libraries and generated files are not rewritten for this limit.
- VERCON progress is recorded in `../implementare_vercon_v2.md` after every page/slice, with the files changed, the real state, and the limits of the verification.
