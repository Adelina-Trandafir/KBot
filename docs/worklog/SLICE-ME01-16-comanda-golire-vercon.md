# SLICE-ME01-16 - «Comenzi»: Golire VERCON, ca o baza migrata gresit sa se poata migra din nou (03.11.2026)

Cererea operatorului: o comanda in VERCON pentru golirea tabelelor VC_ si a tabelelor legate relevante, fara sa afecteze FX, AD sau TZ; scopul: sa poata repeta migrarea pe o baza migrata gresit. Raspunsuri la intrebari: se goleste **absolut tot** din VC_ (inclusiv subunitati, gestiuni, exercitii, produse, setari, numerotare) si **da**, tabelele EF_ de Vercon. Continuarea lui SLICE-VC03-06 (`sql/VC_Golire.sql`, acelasi lucru, rulat de mana); aceasta e varianta din KBotAdmin.

## What it is
- **Verb nou `vercon_golire`** (`PYTHON/routes/admin_cli/vercon_wipe.py`, inregistrat in `admin_cli.py`: `COMMANDS`, `COMMAND_PARAMS`, `SINGLE_DB_COMMANDS`, `_c_vercon_golire`). `writes=True`, ruleaza pe **o singura baza** (refuza `<TOATE BAZELE>`).
- **Ce goleste**: toate tabelele de baza ale bazei al caror nume incepe cu `VC_` (citite din `information_schema`, deci tabelele noi intra singure) plus `EF_PrimiteVercon`, `EF_PrimiteVerconLinii`, `EF_PrimiteVerconNir`, `EF_VERCON_ALOCARI`, `EF_VERCON_ASOCIERI`, `EF_VERCON_CORELARI`, `EF_VERCON_VIZIBILITATE` (aceasta se creeaza la prima folosire, deci poate lipsi). `EF_Primite` si restul EF_ raman: facturile primite nu sunt ale Vercon.
- **Ce NU atinge**: `FX_`, `AD_`, `TZ_`, `Parteneri` (nici cei adaugati din ANAF de migrare; migrarea ii recitește, nu ii dubleaza, conform `VERCON/VRC.Migrator/README.md`), `Clasificatii`, `Users_*`, `Setari`. Tintele sunt prefixul `VC_` + lista fixa EF_, iar fiecare tabela e verificata din nou chiar inainte de `TRUNCATE`.
- **Confirmare**: valoarea tastata a comenzii («Numele bazei (confirmare)») trebuie sa fie numele bazei alese; verificata in client SI pe server. In client, dupa potrivire, o fereastra spune pe sleau ca nu se poate anula.
- **Cum**: `FOREIGN_KEY_CHECKS=0` pe sesiune, `TRUNCATE TABLE` pe fiecare tabela (DDL: se comite singur, reseteaza `AUTO_INCREMENT`, ceea ce o migrare noua vrea), apoi `FOREIGN_KEY_CHECKS=1`. **Nu e atomic**: la o eroare la mijloc, tabelele deja golite raman goale; se reia comanda. Eroarea si tabelele deja golite intra in Jurnal cu rezultatul `EROARE`.
- **Rezultat**: un bloc de sumar (tabele golite, randuri sterse, «TOATE tabelele sunt goale» sau lista celor cu randuri ramase) si un bloc pe tabele (randuri inainte / dupa). Intrare in `Jurnal` (actiunea `vercon_golire`).
- **Client** (`KBotAdmin/Views/CommandsView.vb`): comanda e in grupa VERCON (`CommandApps`), cu explicatie proprie in `Expectations`; `KBotAdmin` 1.2.0.5.

## Files touched
`PYTHON/routes/admin_cli/vercon_wipe.py` (nou), `PYTHON/routes/admin_cli/admin_cli.py`; `KBotAdmin/Views/CommandsView.vb`, `KBotAdmin.vbproj`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build KBotAdmin/KBotAdmin.vbproj`: 0 avertismente, 0 erori. Python: doar verificare de sintaxa (`ast.parse`) pe `vercon_wipe.py` si `admin_cli.py`; fisierul nou are ASCII in afara ghilimelelor «» din mesajele pentru operator. Comanda a fost publicata pentru probe (debugging, fara Git) si **rulata de Adelina pe server: a mers** (03.11.2026). Nimic rulat de mine: nici un test, niciun SQL, nimic trimis pe server de mine.

## Unverified / deferred
- Publicata de operator pentru probe; **inregistrata in nomenclator** dupa confirmarea ei (`Register-DeploymentTask.ps1`, `SLICE-ME01-16`, fisierele `PYTHON/routes/admin_cli/vercon_wipe.py` si `PYTHON/routes/admin_cli/admin_cli.py`). Ramane «Finalizeaza debugging» din KBOTADMIN (numai Git pe server), facut de ea.
- **Doar `VC_` si lista EF_ de mai sus sunt golite.** Daca migrarea scrie si altceva in afara lor (README-ul spune doar ca citeste `Parteneri`), acel ceva nu e atins; de verificat pe prima baza migrata gresit, dupa golire, rulind migrarea din nou.
- Nu exista blocare impotriva utilizatorilor conectati la baza in timpul golirii (`VC_ContextLock` e el insusi golit); de rulat cand nu lucreaza nimeni pe ea.
- `TRUNCATE` nu poate fi anulat si nu pastreaza nimic: copia bazei (Copii / backup din KBotAdmin) se face inainte, de operator.
- Ajutor: niciunul (KBotAdmin nu e pentru clienti).
