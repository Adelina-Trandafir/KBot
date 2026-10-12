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
