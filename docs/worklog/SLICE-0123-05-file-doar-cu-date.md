# SLICE-0123-05 - Filele VERCON si ADECHIT apar doar daca unitatea are date (23.10.2026)

Operator: filele ADECHIT si VERCON trebuie sa apara DOAR daca baza de date are date in tabelele VC_ si AD_.

## What changed and why
- **Server, `PYTHON/routes/setari.py`** (ruta `GET /api/setari`, citita de K-BOT la autentificare si la schimbarea unitatii): raspunsul primeste doua randuri CALCULATE, nu citite din tabelul `Setari`: `AdechitDate` si `VerconDate` (Tip int, 1 sau 0). 1 = cel putin un rand in oricare dintre tabelele `AD_Subunits`, `AD_LunaD`, `AD_Platitori` (ADECHIT), respectiv `VC_Gestiuni`, `VC_Produse`, `VC_Intrari`, `VC_NIR` (VERCON). Un tabel care nu exista conteaza ca gol. Listele sunt constante in fisier (`_DATA_TABLES`), usor de schimbat. Randurile calculate se dau si cand unitatea nu are tabelul `Setari`; un rand cu aceeasi cheie din `Setari` e ignorat.
- **K-BOT, `KBot.Common/ServerSettings.vb`**: `KeyAdechitDate` / `KeyVerconDate`, `AdechitHasData` / `VerconHasData`, incluse in detectarea schimbarii (`Changed`). Ca la orice setare a serverului: fara rand = oprit.
- **`AvacontMain`**: filele VERCON si ADECHIT pornesc ascunse (Designer: `Visible = False`) si `ApplyAppTabs` le arata/ascunde la `ServerSettings.Changed` (autentificare, schimbarea unitatii) si la deschiderea ferestrei. Daca fila aleasa dispare, ribbonul cade pe prima fila vizibila (`ActiveTab`).
- Ajutor: `ribbon.md` si `adechit.md` spun ca filele apar doar cand unitatea are date (0000-119 continuat, eticheta `0123-05`).

## Files to publish (server): `routes/setari.py` (un singur fisier; trebuie urcat INAINTE de K-BOT-ul nou).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. `py_compile routes/setari.py`: ok. Nimic rulat, nimic apelat pe server.

## Unverified / deferred
- **Cu un server care nu are inca `setari.py` nou, filele raman ascunse** (fara rand = oprit): atunci nici «Trimite la Vercon» nu se mai ajunge din ribbon. Urcati serverul inainte de K-BOT.
- Ce inseamna «are date» e alegerea mea (lista de tabele de mai sus); o unitate care a creat doar o subunitate ADECHIT sau o gestiune VERCON, fara alte date, are fila. De confirmat/schimbat de operator.
- Filele nu tin cont de drepturile utilizatorului (roluri AD / VR): un utilizator fara drept vede fila si primeste refuzul serverului in pagina.
- O pagina VERCON/ADECHIT deja deschisa ramane deschisa daca se schimba unitatea intr-una fara date (nu am atins asta); nevazut pe ecran.
- Nu am scris nimic in `NOUTATI.md` pentru aceasta schimbare.
