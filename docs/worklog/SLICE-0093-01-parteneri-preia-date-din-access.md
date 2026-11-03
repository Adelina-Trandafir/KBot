# SLICE-0093-01 - Parteneri: «Preia date din Access» (03.11.2026)

Operator, 03.11.2026, fereastra «Parteneri». Numar de felie: ales de operator (felia 0093 = fereastra Parteneri, fara alte sub-felii pana acum, deci primul liber e 0093-01).

## What changed and why
Un buton nou, **«Preia date din Access»**, in grupul de ribbon al formularului (cel din dreapta, ca «Preia din Access» de la facturile primite, 00EF-43). Apare doar cat «Parteneri» e pe ecran si doar la un client care are si aplicatia Access (`AccessFeature.Enabled`). Scopul: partenerii noi adaugati in Access, pe toate unitatile bazei curente, intra in `Parteneri` din K-BOT fara sa se mai deschida aplicatia din nou.

Fluxul (la «Da» dupa o confirmare):
1. Pentru **fiecare unitate** a bazei deschise (`units` din raspunsul `GET /parteneri`, acum citit si in `ParteneriCatalog.Units`) se afla fisierul Access al unitatii prin registrul `cale.accdb` (`bridge.ResolveUnitFile`, acelasi mecanism ca «Trimite in Access» de la clasificatii; calea registrului e cea din Setari).
2. Tabela Access `Parteneri` se citeste pe acest PC (`AccessPartnersReader`, in KBot.Access, prin `IAccessBridge.ReadPartners`). Coloane luate: CodPartener, DenumirePartener, CodFiscal, ContIBAN, Banca, Adresa, Tip, Ascuns; cele trei id-uri Access (IdPartener, IDPART) nu calatoresc (docs/MAPARE_NOMENCLATOARE.md §6).
3. Randurile merg la server cate unitate, in loturi de cel mult 1000 (`POST /api/forexe/nomenclatoare/parteneri/preia-access`, `parteneri_access.py`). Serverul adauga doar ce **nu exista**: cheia e cea a tabelului, `(IdUnitate, CodPartener)`, comparata ca in MariaDB (fara majuscule/spatii finale). Ce exista deja ramane neatins (nu se actualizeaza nimic, deci ce a editat operatorul in K-BOT ramane). Un lot e o tranzactie.
4. Daca s-a adaugat ceva, lista se reincarca singura (`LoadCatalogAsync`, fosta `LoadCatalog` acum cu varianta asteptabila) si partenerii adaugati sunt aratati in lista chiar daca nu au activitate (`_addedIds`, altfel filtrul «activ» i-ar ascunde). La final un mesaj spune cati s-au citit / adaugat / existau deja / nu s-au putut prelua, cate unitati nu s-au putut citi (fara fisier in registru, fisier blocat) si cati dintre cei adaugati nu apar in lista ferestrei (fara cod fiscal, alt Tip decat 1, ascunsi, cod fiscal al unitatii sau deja al altui partener - regulile 0093).

Decizii luate:
- «Nou» = cheia `(IdUnitate, CodPartener)`, nu codul fiscal. Access are parteneri fara cod fiscal sau cu dubluri si Migratorul ii aduce asa cum sunt; fereastra arata oricum un singur partener pe cod fiscal.
- Se aduc **toti** partenerii noi, nu doar cei pe care fereastra ii arata; mesajul final spune cati nu apar.
- `Provenienta = 'avacont'` (valoarea pe care o scriau mereu rutele de sincronizare din Access).
- Unitatile se iau de la server (toate `Unitati` ale bazei deschise), nu din registru; o unitate fara fisier in registru e raportata, nu opreste restul.
- Fisierul unitatii se alege dupa `IdUnitate`, ca la clasificatii, fara filtru pe an (registrul poate avea mai multe randuri pe an pentru aceeasi unitate; se ia cel pe care il da `ResolveUnitFile`).
- Cu modificari nesalvate in fereastra, se intreaba ca la schimbarea partenerului (Da salveaza / Nu renunta / Anuleaza).

## Files touched
- `PYTHON/routes/forexe/parteneri_access.py` (nou), `PYTHON/routes/forexe/__init__.py` (import), `PYTHON/routes/actiuni.py` (eticheta din jurnalul de actiuni).
- `src/KBot.Domain/AccessPartners.vb` (nou: `AccessPartner`, `PartenerPreluare`), `src/KBot.Domain/Nomenclatoare.vb` (`ParteneriUnit`, `ParteneriCatalog.Units`).
- `src/KBot.Common/AccessBridge.vb` (`IAccessBridge.ReadPartners`), `src/KBot.Access/AccessPartnersReader.vb` (nou), `src/KBot.Access/AccessBridgeImpl.vb`.
- `src/KBot.Api/INomenclatoareApi.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb` (`PreiaParteneriAccessAsync`, `units` citit).
- `src/KBot.App/Views/Nomenclatoare/ParteneriForm.Access.vb` (nou, clasa partial), `ParteneriForm.vb` (`LoadCatalogAsync`, `IsListed`).
- `src/KBot.App/AvacontMain.Designer.vb` (butonul `parteneri_access_preia`, imaginea refolosita de la «Preia din Access»), `AvacontMain.vb` (vizibilitate in `UpdateFormGroup`, clic), `KbotForm.Nomenclatoare.vb` (`RunParteneriAccess`).

## Test results
Operatorul a pus ruta pe server si a incercat butonul (03.11.2026): «a mers» - fara alte detalii (cifre, aspect). `dotnet build src\KBot.Access` si `src\KBot.App` (Debug): 0 avertismente, 0 erori. `py_compile` pe fisierele Python atinse: OK. Niciun test scris sau rulat de mine (regula «fara teste»). Cele trei fisiere Python sunt inregistrate pentru Git sub SLICE-0093-01.

## Left open
- **Ajutor: nu se scrie** (regula operatorului din 02.10.2026: nimic despre Access in ajutor); de aceea nu exista etichete `0093-01` in HelpContent.
- **Noutati:** la cererea expresa a operatorului (03.11.2026) exista o linie in sectiunea 1.1.5.0 din `docs/release-notes/NOUTATI.md`, desi regula din 02.10.2026 exclude Access si de acolo; linia ajunge si la clientii fara Access.
- Nu se actualizeaza partenerii existenti; daca operatorul vrea sa se aduca si schimbarile (denumire, IBAN...) din Access, e o decizie noua.
- Butonul nu are un indicator de progres in ribbon; progresul se vede in linia de stare a ferestrei.
