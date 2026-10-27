# SLICE-VC05-06 — parteneri din ANAF pentru furnizorii fără partener

Stare: **VALIDAT_SERVER** (000_DEMO, 13 parteneri adăugați și confirmați). Cererea operatorului, 12.10.2026. Fereastra și bifarea SS nu au fost văzute de mine pe ecran.

## Problema
«Testează» avertiza «N furnizori fără partener KBOT»: furnizorii din Access al căror cod fiscal nu există în `Parteneri` din DC se importau fără partener. Pasul nou: pentru fiecare cod fiscal lipsă se cere ANAF și se creează partenerul.

## Facut
- `VrcAnaf.vb`: apel ANAF v9 (același ca `PYTHON/routes/inregistrare/anaf.py`: POST, `[{cui, data}]`, `found` / `notFound`), câte 100 de coduri pe cerere, o secundă între cereri; pagină HTML = eroare, nu «negăsit». Doar coduri din cifre (2–10).
- `VrcPartnersForm.vb` + `.Designer.vb` (butonul «Parteneri ANAF…» între «Testează» și «Migrează»): lista furnizorilor fără partener, «Interogare ANAF» (denumire + adresă), coloane de bifat, câte una pe SS al DC-ului. Coloanele SS depind de date și se adaugă din cod; restul sunt în designer. Un click pe titlul unei coloane SS bifează sau debifează caseta pentru toți furnizorii (`KBotDataView.HeaderClick`, înregistrat în SLICE-0008-03).
- Regula (decisă de operator): bifezi pe furnizor SS-ul (unul sau mai multe); `IdUnitate` = cea mai mică unitate dintre SS-urile bifate (aceeași regulă ca `_unit_for_new` din fereastra Parteneri). Mai multe SS bifate dau tot **un** partener (un cod fiscal = un partener în DC, verificat în `_check_fiscal_code`). `CodPartener` = următorul număr liber (minim 3 cifre) din SS-ul unității alese, ca `NextCode`. `Tip` 1, `Ascuns` 0, `CodFiscal` doar cifre.
- Fără cod fiscal, negăsiți la ANAF sau fără SS bifat: nu se adaugă; NIR-urile lor rămân fără partener (X04).

## Decizie privind `IdUnitate`
Analiza înlocuirii lui `IdUnitate` în `Parteneri` / `Clasificatii` cu cod fiscal / SS a fost făcută și **amânată** (prea multe efecte: extrase, DDF, FX_*, migrările Access). Rămâne ca slice separat, cu o primă fază fără scriere, dacă se mai cere.

## Neacoperit
- Fereastra și coloanele SS nu au fost văzute pe ecran de mine; au fost folosite de operator în rularea din raportul VC05-07.
- Nu există gestionare a limitei ANAF peste ritmul de o cerere pe secundă.

## SLICE-VC05-07 — partenerii din ANAF în aceeași tranzacție cu transferul <a id="slice-vc05-07"></a>

Stare: **VALIDAT_SERVER** (000_DEMO). Cererea operatorului, 12.10.2026: «dacă dă fail, pică și ei».

### Facut
- Fereastra «Parteneri ANAF» nu mai scrie nimic în bază. «Pregătește pentru transfer» reține partenerii în `VrcPlan.NewPartners` (nu intră în hash-ul planului); «Anulează pregătirea» îi șterge.
- `VrcPartnerStore.AddInTransaction(cn, tx, dc, items)` rulează pe conexiunea și tranzacția transferului și nu face niciodată COMMIT. `VrcRunner.AddPreparedPartners` îl apelează după `OpenSources` și înainte de `LoadPartners`, deci NIR-urile se leagă direct de partenerii noi.
- Orice eșec, oprire sau diferență de reconciliere => rollback pentru tot, inclusiv partenerii. Dacă între timp apare un partener cu același cod fiscal, cel pregătit nu se adaugă, iar linia din raport o spune.
- «Testează» nu mai numără furnizorii pregătiți ca lipsă și spune câți vor fi adăugați în aceeași tranzacție.
- Raportul are secțiunea «Parteneri din ANAF», cu o linie pe partener; apare și la rollback, alături de verdictul de sus.
- Lista pregătită se pierde la alegerea altui folder sau la citirea altor fișiere; rămâne după un transfer anulat.

### Dovada
Raportul din rularea operatorului pe `000_DEMO` (COMMIT confirmat): 13 parteneri din ANAF adăugați (coduri 1073–1085, unitatea 75 / 02A), numărătorile sursă/țintă egale pe toate tabelele, 4 NIR fără cod fiscal rămase fără partener (X04).

### Neacoperit
- Nu s-a provocat un eșec după inserarea partenerilor ca să se vadă dispariția lor (rollback): urmează din construcția tranzacției, dar n-a fost probat.
- Un singur DC (000_DEMO) și o singură unitate de SS.
