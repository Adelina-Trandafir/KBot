# PYTHON (Flask API + web apps) - rules

Apply on top of the root `CLAUDE.md`. (Written without diacritics, RULE 0.)

## CSS / code structure

- CSS is written in separate `.css` files, apart from HTML, JavaScript and Python. No `<style>` blocks, no stylesheet strings in JS, no `style` attributes for styling. Dynamic values a control needs (sizes, positioning, CSS variables) go through the control's API.
- Each application has one entry stylesheet with ordered imports, and CSS modules by responsibility: structure/context/navigation, pages or document families, nomenclatures, reports/preview, and responsive adaptations. Do not pile every page into a single CSS file.
- CSS of shared controls stays next to their components. App-specific adaptations go in that app's CSS, with selectors limited to that app.
- When splitting an existing stylesheet, keep the cascade order, selectors, values, media queries and relative URL resolution. Do not mix the reorganization with visual changes nobody asked for.
- No own code module (Python, JS, CSS, HTML) should exceed about 1000 lines. When one gets close, split it by responsibility, with explicit interfaces. Do not compress code or move lines around arbitrarily to hide an overrun. Third-party libraries and generated files are not rewritten for this limit.
- VERCON progress is recorded in `../implementare_vercon_v2.md` after every page/slice, with the files changed, the real state, and the limits of the verification.
