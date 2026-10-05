# SLICE-0110-01 - landing: «K-BOT te invata singur» (operator request, 05.10.2026)

## What changed and why
The public page had no word about the interactive tutorials (slice 000T). New section `tutorials` in
`PYTHON/routes/landing/content.json` (kicker, title, image, blocks, three steps), rendered by a new block in
`templates/landing/index.html` after «Pentru toti utilizatorii» (same markup as the «multi» section), a nav entry
and a side dot (`tutoriale`). Image: existing `fereastra-principala.png`. Text says what is true today (the tutorial waits
for the user, dims the rest, is opened from «?», list keeps growing); no promise about specific flows.

## Files touched
`PYTHON/routes/landing/content.json`, `PYTHON/routes/landing/templates/landing/index.html`.

## Checks / not done
content.json parses. Page not rendered, not seen. Server not deployed. No help change (the site is not part of the in-app help).
A dedicated tutorial screenshot would suit better than the main window; not made.

## Corrective, 05.10.2026 - the top bar after 0110-02..0110-04 (operator: «arata rau», the pills grew tall, must work on a phone)
Cause: nine menu links («Tutoriale» and «Cere detalii» were new) plus two buttons in a 1240 px container, with nothing stopping
the text from wrapping, so the pills grew vertically. Fix, in `static/site/site.css` (appended, so it wins) and the two templates:
- nothing in the bar wraps any more (`white-space: nowrap`), container up to 1560 px;
- «Cere detalii» leaves the section menu (it stays in the closing call to action);
- what does not fit is dropped in steps: the tagline under 1480 px, the section menu under 1320 px (the side dots and the page itself
  still lead everywhere), and under 520 px the bar keeps the logo, «Intră în cont» and a short «Înregistrează» (the « unitatea» part
  is `.btn__more`, hidden on a phone). «Intră în cont» was hidden under 520 px by an older rule; it is visible now.
- Measured in the browser pane at 1950, 1440, 1366, 1330, 1100, 375 and 320 px: no element taller than one line, nothing outside the
  bar, no horizontal scroll, menu and buttons do not overlap (28 px free at 1330). Looked at on screen at 375 px.
- Not looked at: tablet widths between 520 and 900 px by eye (measured at 1100 only), and the form page's bar on a phone by eye.
