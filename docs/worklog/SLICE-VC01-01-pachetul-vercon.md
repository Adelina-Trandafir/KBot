# SLICE-VC01-01 — pachetul VERCON si publicarea

Stare: **IMPLEMENTAT_LOCAL**. Nu a fost rulat, testat sau importat nimic (regula: fara teste decat la cerere); nimic validat pe server.

## Facut
- `PYTHON/routes/vercon.py` (fisier gol, 0 octeti) sters; in locul lui pachetul `PYTHON/routes/vercon/`.
- `routes/vercon/__init__.py`: `create_blueprint(authenticate=None)`, `vercon_bp`, `SECTION = "VR"`; un singur endpoint `GET /api/vercon/status` (sesiune portal obligatorie, `Cache-Control: no-store`). Fara conexiune la baza la import.
- (Revizuit la VC01-02/03: `/api/vercon/status` inlocuit de `/api/vercon/context`, cu garda de unitate si drept.)
- `main.py`: import si `register_blueprint(vercon_bp)`.
- `routes/vercon/numeric.py` (motor numeric pur, fara baza): `ZEC`/`SCARA`, `to_int` (refuz la pierdere/overflow, fara rotunjire), `display` (in sus la jumatate), `vat_to_int` (x10000), `calc_round` (HALF_EVEN, propunere), `header_total` (N01/N02, unicul calcul la migrare). Implementeaza CONTRACT_CALCULE §1–§2.3.

## Sectiunea VR
Rolurile sunt date in `AVACONT_COMUN` (`Roluri.Sectiune`), citite de `routes/portal/drepturi.py`; nu e nevoie de cod nou pentru sectiune. Rolurile VR si operatiile se definesc la VC04-01.

## Nefacut / limite
- Manifestul AvacontPush/LocalRoot nu a fost inspectat (nu este in repo; de verificat la publicare).
- Fara test: motorul numeric nu are inca etaloane executate (E01–E17 din contract).
- Nu exista pagina web, DDL sau migrator.
