# SLICE-0084-04 - «Asociaza parteneri»: out of the Sumar view, into the ribbon (16.10.2026)

Operator request: the button leaves the Sumar view; it is a ribbon button that exists only while «Sumar» is on screen
(control part: `SLICE-0114-02-ribbon-separator-si-butoane-fixate-conditionate.md`). Follows 0084-03 (the popup).

## What changed and why
- `SumarView`: `btnPartners` removed (code, designer, theming, and the 5th column of the header table that only held it).
  The view keeps the rule «partners go on the DDF»: `CanAssociatePartners` (info.AreDDF), event `PartnersAvailabilityChanged`,
  and `OpenPartnersWindow` (the popup of 0084-03, `ShowDialog` over the shell).
- `KbotForm` (`KbotForm.Views.vb`): event `ActiveViewChanged` (raised when a view is activated, and when the Sumar view says its
  availability changed), `ActiveViewKey`, `CanAssociatePartners`, `OpenSumarPartners`. The ribbon (AvacontMain) listens.
- Interpretation: the old button was HIDDEN without a DDF. The ribbon button is visible whenever «Sumar» is on screen (as asked)
  and **dimmed** without a DDF, so it does not appear and vanish while the operator walks the tree. Easy to change to «hidden».

## Files touched
`src/KBot.App/Views/SumarView.vb`, `SumarView.Designer.vb`, `src/KBot.App/KbotForm.Views.vb` (+ the ribbon files of 0114-02);
help `contabil/vederi/sumar.md`, `tours/tur-sumar.md`.

## Test results
Build `KBot.App`: 0 warnings, 0 errors. No tests run (house rule). Not run, not seen on screen.

## Unverified / deferred
- Not seen: the header of the view after the 5th column went (the description still spans 3 columns); the ribbon button showing /
  hiding when the tree selection or the view changes; the popup opened from it.
- The tour step «Asociaza parteneri» has no `target:` (ribbon buttons are painted, the tour cannot ring them).
- Not committed.
