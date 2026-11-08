# SLICE-AV17-01 — PDF417 compatibil cu rapoartele Access din AVACONT

Propunere pentru codul de bare de pe ordinele de plată. Criteriul este aceeași matrice de bare și spații pentru aceleași date, cu dimensiunile și poziția din raportul Access ales ca referință. Egalitatea fișierelor PDF la nivel de octeți este o cerință separată de geometria simbolului.

## Ce face exportul VBA

- Rapoartele `rptOP` și `rptOP1`–`rptOP5` formează `CODBARE` din `CODBARE1` și `CODBARE2`. Controlul `codb` apelează `pdf417$([CODBARE])`, iar controalele vizibile din cele două exemplare îl afișează cu fontul `Code PDF417`. În `rptOP`, cele două exemplare au 7 pt și 8 pt. `rptOP2_prost` folosește 12 pt și primește `CODBARE` din interogarea `OPuri_Simplu`.
- `CODBARE` este specific variantei de raport. De exemplu, `rptOP1` înlocuiește separatorul zecimal în valoarea convertită la text și scoate `RO` din codul fiscal al beneficiarului. `rptOP3` folosește `Format` pentru sumă și preia `CFPartener` direct în șir. `OPuri_Simplu` aplică `CurataOP` câmpurilor de text selectate și limitează explicația la 50 de caractere înainte de curățare.
- `mdl_FONT` conține funcția v1.5.0. Apelul cu un singur argument pornește cu 6 coloane de date și corecție Reed–Solomon nivel 3, adică 16 cuvinte de corecție la acest nivel. Funcția alege blocuri de text, cifre și octeți, adaugă umplere `900`, descriptorul de lungime, corecția și indicatorii de rând. Fiecare cuvânt se mapează la trei caractere din una dintre cele trei tabele, selectată prin `rând Mod 3`. Rezultatul începe fiecare rând cu `+*` și îl încheie cu `-` și `CRLF`.
- Pragurile de compactare influențează barele. Pentru `A12345678B`, citirea codului arată că VBA păstrează blocul de opt cifre în modul numeric. Modul automat din ZXing.Net comută la numeric de la 13 cifre consecutive, deci setarea acelorași coloane și a aceluiași nivel de corecție nu asigură egalitatea. [Compactarea automată din ZXing.Net](https://github.com/micjahn/ZXing.Net/blob/master/Source/lib/pdf417/encoder/PDF417HighLevelEncoder.cs#L240-L274)

[Sursa `CODBARE` din `rptOP1`](../../AVACONT/EXPORT_VBA/Reports/rptOP1_header.txt#L58) · [Sursa din `rptOP3`](../../AVACONT/EXPORT_VBA/Reports/rptOP3_header.txt#L60) · [Controalele din `rptOP`](../../AVACONT/EXPORT_VBA/Reports/rptOP_header.txt#L1929) · [Interogarea `OPuri_Simplu`](../../AVACONT/EXPORT_VBA/queries/OPuri_Simplu.sql) · [Funcția VBA](../../AVACONT/EXPORT_VBA/Modules/mdl_FONT_code.txt#L5)

## Formatul PDF417 pentru PDF-ul ordinului de plată — SLICE-AV17-03

Regulile se aplică la construirea lui `CODBARE` pentru ordinele de plată destinate ANAF, înainte de apelul oricărui encoder PDF417. Câmpurile se formatează și se validează individual, apoi se asamblează în ordinea din schema de mai jos.

- Câmpurile text transmise în PDF417 se scriu fără diacritice. Literele cu diacritice se înlocuiesc cu literele corespunzătoare fără diacritice, păstrând majusculele și minusculele. Pentru `ș` și `ț` se înlocuiesc atât formele cu virgulă dedesubt, cât și cele cu sedilă.
- Virgula este rezervată delimitării câmpurilor. Se curăță virgulele din fiecare valoare înainte de concatenare. Validarea împiedică generarea codului dacă o valoare mai conține o virgulă.
- Suma se formatează invariant, cu două zecimale, punct ca separator zecimal și fără separator de mii. Data se formatează `dd/MM/yyyy`, cu bare oblice literale, indiferent de setările regionale.
- Se păstrează marcajele `#`, câmpul gol dintre BIC-ul beneficiarului și explicație, reprezentarea `##` pentru câmpurile marcate goale și virgula finală. Identificatorii se tratează ca text pentru a păstra zerourile inițiale.

```text
#NR_OP#,SUMA,#DENUMIRE_PLATITOR#,CIF_PLATITOR,#ADRESA_PLATITOR#,#IBAN_PLATITOR#,#BIC_PLATITOR#,#DENUMIRE_BENEFICIAR#,CIF_BENEFICIAR,#IBAN_BENEFICIAR#,#BIC_BENEFICIAR#,,#EXPLICATIE#,DATA,#COD_ANGAJAMENT#,#COD_INDICATOR#,#COD_PROGRAM#,
```

## Fontul Grand Zebu

`InstallPDF417` caută `Res\pdf417.ttf`, iar numele din rapoarte și maparea caracterelor corespund fontului Grand Zebu. Autorul publică un pachet cu funcția 1.5.0 și fontul 1.10. Acesta este candidatul pentru redarea de referință. Versiunea binară instalată pe stația Access rămâne de identificat. Proiectul este listat cu GPLv2. Înainte de includerea fontului sau a unui port al encoderului în KBOT, se citesc termenii din arhivă pentru componenta respectivă și pentru modul de utilizare, inclusiv distribuirea și încorporarea în PDF. [Instalatorul VBA](../../AVACONT/EXPORT_VBA/Modules/mdl_FontCodBare_code.txt#L11) · [Descrierea fontului de către autor](https://grandzebu.net/informatique/codbar/pdf417-en.htm) · [Pachetul PDF417](https://sourceforge.net/projects/openbarcodes/files/pdf417/) · [Licența listată de proiect](https://sourceforge.net/projects/openbarcodes/)

## Implementare propusă

1. Se iau din Access câteva exemple cu date fictive pentru raportul care urmează să fie portat. Pentru fiecare se păstrează `CODBARE` înainte de codificare, șirul returnat de `pdf417$` cu `CRLF` și PDF-ul Access cu ambele exemplare ale OP-ului.
2. Se reconstruiește `CODBARE` separat de encoder, după formatul documentat pentru PDF-ul OP și trunchierile raportului ales. În probele izolate de compatibilitate a encoderului, caracterele extinse se compară cu rezultatul conversiei `Asc` din Access, inclusiv pentru diacriticele cu sedilă și cu virgulă.
3. Se implementează în VB.NET un encoder care reproduce regulile observate în Access. Rezultatul de referință este șirul exact de glife, cu ordinea rândurilor și `CRLF`. Se păstrează alegerile de compactare, umplerea, corecția și indicatorii, inclusiv comportamentul la limitele pe care le pot atinge ordinele de plată. Se folosește o implementare independentă dacă termenii sursei nu permit integrarea unui port direct.
4. Pentru prima comparație se randează șirul de glife cu același font și cu dimensiunile fiecărui exemplar Access. Fontul poate fi folosit și la livrare dacă termenii și motorul PDF permit. Dacă livrarea prin font nu este potrivită, se desenează vectorial aceleași bare din matricea encoderului. În acest caz se calibrează lățimea modulului, pasul dintre rânduri, marginile și poziția după PDF-ul Access. Fontul are bare cu înălțimea nominală de 3 module și include câte 2 module de margine laterală. [Geometria fontului Grand Zebu](https://grandzebu.net/informatique/codbar/pdf417-en.htm#)

## Criterii de acceptare

- Intrarea `CODBARE` și ieșirea de glife sunt identice cu valorile exportate de Access pentru exemplele de referință.
- Matricea de module coincide rând cu rând. Geometria din PDF se compară separat pentru fiecare exemplar, inclusiv lățime, înălțime, poziție și marginile albe.
- Exemplele acoperă zerouri inițiale, pragurile blocurilor de cifre, punctuație, diacritice, câmpuri goale și o explicație lungă. O decodare independentă verifică lizibilitatea pe lângă comparația matricei.
