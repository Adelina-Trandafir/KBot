# SLICE-0000-175 — help: payment in «Contari» (slice 04EF-01)

**What changed and why.** The help of «Contari» said the booking has nothing to do with receptions and did not know the payment. `contabil/efactura/index.md` (section «Contări») now explains: evidence + payment, payment alone (take the evidence template off), the rest to pay, the PROVISIONAL order number with its tooltip and the real number after the save, the reception warning / block, and «Șterge plata» with its lock reasons. `contabil/setari.md` names the tick «Protectie contari Plati/Receptii». Tags `04EF-01, 0000-175`.

**Files touched.** `src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/contabil/setari.md`.

**Test results.** `tools\HelpCheck\Check-Help.ps1` was run: it reports many errors that were there before (old slice ids "not in KBOT_STATUS.md", missing controls in tutorials); the ones on these two files are of the same kind (`04EF-01` / `0000-175` not found as index entries). No error is about the text itself.

**Unverified / deferred.** Nothing seen on screen. The captures `efactura-cumparare` and `setari` may need a redo (new button, new tick). The text describes code that is compiled but never run.
