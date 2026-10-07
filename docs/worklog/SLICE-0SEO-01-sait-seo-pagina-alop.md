# SLICE-0SEO-01 - public site: search-engine basics + the «PDF Inteligent» page kept as a draft (operator request, 06.10.2026)

Slice name as the operator wrote it («slice-0SEO»); no number was assigned.

## What changed and why
1. **Presentation page (`/`)**: real `<title>` and description in `content.json` (`meta`), `rel=canonical` to `https://k-bot.ro/`, Open Graph + Twitter card tags, JSON-LD (`Organization`, `SoftwareApplication`). Until now the page had only the title «K-BOT» and a description.
2. **`/robots.txt` and `/sitemap.xml`** (new `PYTHON/routes/landing/seo.py`, registered in `main.py`). Robots keeps `/api/`, `/portal`, `/inregistrare` out; the sitemap lists `/` and `/detalii`.
3. **Top bar of the presentation page**: the section menu shows emoji with the words as a tooltip (`icon` per entry of `nav` in `content.json`, `data-tip` + CSS in `site.css`); the red «PDF „Inteligent”» button follows the brand; «Înregistrează» (short) comes before «Intră în cont».
4. **The Adobe/ALOP article is a DRAFT.** Page at `/alop-eroare-adobe-acrobat-pro` (`PYTHON/routes/pdf_inteligent/`, style `static/site/pdf.css`), with its own title/description/canonical/Open Graph, JSON-LD (`Article` + `FAQPage`), a visible date and an FAQ section. The operator is not yet sure of the text, so one switch controls it: `content.json` → `"alop_page": {"published": false}`.
   - **false (now)**: no button in the top bar, not in `sitemap.xml`, the page itself still opens by its direct address (so it can be read and tested) but carries `<meta name="robots" content="noindex, nofollow">` and the header `X-Robots-Tag: noindex, nofollow`; the old address `/pdf-inteligent` answers 404.
   - **true**: the button shows, the page is in the sitemap and indexable, `/pdf-inteligent` redirects (301) to the new address.

## Publishing the article later (the flow)
1. Final read of the text in `routes/pdf_inteligent/templates/pdf_inteligent/index.html` (the FAQ answers and the JSON-LD block at the top repeat the same sentences: edit both).
2. Set `"published": true` in `content.json` (no restart needed: the file is re-read when it changes).
3. In Google Search Console: submit `https://k-bot.ro/sitemap.xml`, then «Inspect URL» on the article and «Request indexing».
4. If a date matters: change `datePublished` / `dateModified` in the JSON-LD and the visible «Actualizat» line.

## Files touched
`PYTHON/main.py`, `PYTHON/routes/landing/{content.json, seo.py (new), templates/landing/index.html}`, `PYTHON/routes/pdf_inteligent/{__init__.py, pdf_inteligent.py, templates/pdf_inteligent/index.html}` (new), `PYTHON/static/site/{site.css, pdf.css (new)}`.

## Test results
Only a throw-away check with Flask's test client (no server, nothing on screen): both pages answer 200, JSON-LD parses, `robots.txt` / `sitemap.xml` answer 200, and the two states of the switch behave as above. No test code was added (standing rule).

## Left unverified or deferred
- Nothing was looked at in a browser: the look of the page, the red button, the emoji menu and the tooltips are unseen. The pending server restart is needed for the new routes.
- How a shared link looks (WhatsApp / Facebook) and whether Google takes the markup are only known after deployment. The share image is the square logo (`logo-512.png`); a 1200x630 image is a separate job.
- The description of `/` is ~165 characters (Google cuts near 155).
- Not added: a link to the article inside the presentation text (only the top-bar button).
- Help (`HelpContent/`): not touched, the change is on the public site, not in the application.

## Update 07.10.2026
The operator confirmed the problem is real (she saved and signed by hand a form downloaded from the Ministry's site, same error) and asked to publish: `alop_page.published` is now `true`, the date of the page and of the JSON-LD is 07.10.2026, and a «Verificat și manual» note was added under the error messages. Still to do by hand: submit the sitemap in Search Console and request indexing. Nothing seen in a browser; the server must be restarted/deployed.
