# SLICE-0000-74 - Ajutorul pentru 0120: editorul DDF, randul de jos; verificarea finala a ajutorului (13.10.2026)

Ultima din sub-feliile de ajutor 0000-69 … 0000-74 (noua fereastra principala). Context, capturi si navigare:
`SLICE-0000-69-ajutor-machete-ca-pagini.md`.

## Ce s-a schimbat si de ce
- `contabil.ddf.editor`: editorul se deschide ca pagina (0113); in «Sectiunea A», paragraf nou: **«Adauga rand»** si
  **«Sterge randul»** stau pe randul de jos al editorului, langa **«Salveaza documentul»**, cat e deschisa pagina
  Sectiunea A (0120); «Salvarea»: fara «Renunta», «Inchide» din ribbon / Esc inchide fara salvare si elibereaza numerele
  rezervate (inchiderea le elibera si inainte, din `FormClosed`). Captura `ddf-editor` rosie (titlu nou al capturii).
- `contabil.ddf.nou`: captura `angajament-nou` rosie pentru acelasi motiv (si deschiderea din ribbon, 0000-69).

## Fisiere
`src/KBot.App/HelpContent/contabil/ddf/editor.md` (si `contabil/ddf/nou.md`, scris in 0000-69).

## Teste / verificare (finala, pentru 0000-69 … 0000-74)
`powershell -NoProfile -ExecutionPolicy Bypass -File tools\HelpCheck\Check-Help.ps1 -Coverage -Map`:
- «Help check: 61 topics, 19 tours, 23 tutorials, 92 capture tags.» (inainte: 56 subiecte, 80 de etichete de captura).
- **Subiectele: nicio eroare.** Eroarea veche `contabil\efactura\token.md: control 'btnToken' not found` a disparut.
- **Raman 20 de erori, niciuna intr-un subiect**: `tours\tur-setari.md` (pasul «Jurnal», `item:jurnal` — pagina a iesit
  din Setari in 0118; asteapta decizia despre tururi, 0000-69), plus 19 vechi, de dinainte de aceasta lucrare:
  `tutorials\efactura-factura-noua.md` (4 controale care nu mai exista in `FacturiForm`) si
  `tutorials\ordonantare-din-plata.md` (15 pasi fara eticheta `slice`).
- Acoperirea: `AcasaPage`, `AvacontMain`, `JurnalPage`, `IstoricVersiuniPage` si `SetariIstoricView` au acum subiect.
  Raman fara subiect, toate de dinainte: `EFacturaView`, `ForcedPasswordChangeForm`, `HelpCaptureViewForm`,
  `IstoricIntervalForm`, `PrintListPage`, `SetariAccessView`, `UpdateOfferForm`.
- Avertismentele «does not implement IKBotTutorialHost» ale tutorialelor: aceleasi ca inainte.
- `help-version.txt` = `2026-10-13`. Fara build si fara rulare (regula proiectului; ajutorul e copiat la build).

## Capturi de refacut
Rosii: `ddf-editor`, `angajament-nou` (lista completa: 0000-69).

## Neverificat
- Nimic vazut pe ecran.
