# Fisa de citire: Jurnale - AV01

> DECIZII operator (23.10.2026): jurnalele sunt **pe unitate**. Va fi nevoie si de **subunitati**, ca la VERCON (web).
> VERCON: ierarhie DC -> subunitate -> gestiune, tabel `VC_Subunitati`, drepturi doar pe rol la nivel de DC (nu pe
> subunitate) - `VERCON/DECIZII_DESCHISE.md`. In Access exista tab «SUBUNITATI» (`LEGUNIT`, parinte/sclav de buget).
> Ramane de lamurit ce este o subunitate in contabilitate si daca jurnalele se definesc o data pe unitate sau pe subunitate (S7).

Buton ribbon Access: `btnJurnale` (tab Configurari > Baza de date > «Jurnale»).
Formular: `Jurnale` (21 controale, 68 linii cod). Stare in K-BOT: **LIPSESTE**.
Marcaj: VERIFICAT = citit in fisierul numit; PRESUPUS = dedus.

## 1. Ce face

VERIFICAT (`Forms/Jurnale_code.txt`, `Forms/Jurnale_header.txt`): grila simpla pe tabelul
`Jurnale` cu doua coloane: **Cod Jurnal** (`CodJurnal`) si **Denumire** (`DenumireJurnal`).
Titlu: «Configurari jurnale».

## 2. Reguli

- Codul jurnalului nu poate fi gol (la inserare si la modificare).
- La stergere se numara operatiile care folosesc jurnalul
  (`DCount("IdOperatie","Oper","CodJurnal=...")`, pus in campul `Cio`), apoi `MsBox(285)` cere
  confirmare. VERIFICAT: la «Da» stergerea CONTINUA (se afiseaza mesajul 284 si iese din
  procedura fara `Cancel`); la «Nu» se anuleaza (`Cancel = True`). Textele 284/285 sunt in `tblMsg`,
  neexportat. In plus, relatia Jurnale -> Oper este cu Delete in CASCADA (VERIFICAT, `baza2026.accdb`): in
  Access, confirmarea «Da» ar sterge si operatiile jurnalului.
- **STERGERE (cerinta operatorului, 23.10.2026): un jurnal care are operatii este BLOCAT la stergere.** Fara
  confirmare, fara cascada. In MariaDB legatura Oper -> Jurnale este `RESTRICT`. Mesajul spune cate operatii
  il folosesc. Acelasi lucru daca il refera un sablon (`DefaNote`/`DefaNoteA`).
- Fiecare adaugare / modificare scrie o actiune in istoric (mesajele 10980 / 10981).

## 3. Date

`Jurnale` (tabel LOCAL in `baza2026.accdb`, 15 randuri - VERIFICAT): `CodJurnal` (Text 50, **cheie primara**),
`DenumireJurnal` (Text 255). Fara alte coloane (`ID`, `S` apar doar in `CorelatiiJurnale`, tabelul de pasaj).
Valori in aceasta baza: `J0D`, `JBM`, `JBS`, `JC`, `JCM`, `JG`, `JI`, `JPM`, `JS`, `JV`, `L`, `LC`, `RB`, `RBV`, `RC`.
Folosite efectiv: doar `J0D` (527 operatii) si `JI` (3). Observatie: `LC` are denumirea `JC` (dubura aparenta).
Relatii (cascada Update+Delete in Access): Jurnale -> Oper.CodJurnal, Jurnale -> DefaNote.Jurnal.
Cine il foloseste: `Oper.CodJurnal` (operatiile contabile), `DefaNoteA.Jurnal` / `DefaNote.Jurnal`
(sabloane), interogarile `Jurnale_CFPP`, `Jurnale_Centralizator`, `Query2`, rapoartele de
registre/jurnale (etapa 6). PRESUPUS: lista de jurnale (ex. cumparari, vanzari, banca, casa,
operatii diverse) e mica si aproape fixa.

## 4. De lasat deoparte

Textele de eroare prin `MsgBox` directe (in K-BOT: `KBotMessage.Show`), `DoCmd.Close`.

## 5. Suprapuneri cu K-BOT

Nimic. «Jurnal activitate» din ribbon-ul KBOT este jurnalul aplicatiei (loguri), **nu** jurnalele
contabile.

## 6. Propunere

| Element | Eticheta | Nota |
|---|---|---|
| Tabel `Jurnale` (cod unic, denumire) + grila de editare mica | PORT | stergere BLOCATA daca are operatii (decis de operator) |
| Copiere intre unitati (`CorelatiiJurnale`) | DE DECIS | nu e in ribbon |

## 7. Intrebari pentru operator

1. Lista de jurnale e aceeasi la toate unitatile (nomenclator comun) sau fiecare unitate si-o
   face singura?
2. Codul jurnalului se poate schimba dupa ce are operatii (in Access da) sau il blocam?
3. (REZOLVAT de operator: stergerea e blocata cand jurnalul are operatii.) Ramane: schimbarea CODULUI unui
   jurnal care are operatii (in Access se propaga prin cascada) - o blocam si pe ea?
4. Ce jurnale folosesc rapoartele din etapa 6 (registre/jurnale)? Le fixam cu cod standard?
