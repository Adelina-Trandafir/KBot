# SLICE-0000-67 - help for the mandatory change of the initial password (11.10.2026)

Work: see `SLICELESS-securitate-api.md` (security plan, new system next to the legacy one).

- Changed: `src/KBot.App/HelpContent/contabil/autentificare.md`, new section «Prima conectare: schimbarea parolei initiale», tag `0000-67`.
- Not changed: the Setari help page (its password section says nothing about the removed 8-character rule).
- Tests: `tools\HelpCheck\Check-Help.ps1` run, the only message for this page was the missing worklog (this file).
- Unverified: no capture was added for the new window; nothing seen on screen.
