---
id: tur-fereastra
title: Fereastra principală
part: contabil
topic: contabil.fereastra
---
## Bine ai venit
<!-- slice: 0000-04, 0000-23 -->
target: KbotForm
goto: view:sumar
Acesta este un tur al ferestrei principale. Vârful bulei arată mereu locul despre care e vorba. Folosește «Înainte» sau săgeata dreapta ca să treci mai departe și «Închide» sau Esc ca să ieși oricând.

## Butonul MENIU
<!-- slice: 0087, 0084, 0088 -->
target: KbotForm.btnMeniu
De aici pornești un angajament nou, deschizi extrasele de cont, nomenclatoarele și, când există, operațiunile necorelate. Semnul (!) pe buton înseamnă că ai operațiuni necorelate de rezolvat.

## Anul de lucru
<!-- slice: 0001, 0086 -->
target: KbotForm.cboAn
Anul pentru care lucrezi. Schimbarea lui reîncarcă lista de angajamente și toate ecranele.

## Sursa și sectorul
<!-- slice: 0001, 0086 -->
target: KbotForm.cboSs
Sursa și sectorul (de exemplu 02A). Ultima alegere se ține minte pentru data viitoare.

## Lista angajamentelor
<!-- slice: 0009, 0777, 0034, 0080-03 -->
target: KbotForm.tree
Aici alegi angajamentul pe care lucrezi: un clic pe el umple vederea din dreapta cu datele lui. Urmează, pe rând, butoanele listei.

## Lista › Lupa
<!-- slice: 0027, 0035, 0000-23 -->
target: KbotForm.tree
part: header.search
Deschide banda de căutare peste listă: scrii o parte din nume și lista arată doar angajamentele care se potrivesc. Esc golește căutarea și închide banda.

## Lista › Rotița
<!-- slice: 0777, 0000-23 -->
target: KbotForm.tree
part: header.right
Opțiunile listei: sortarea (după nume sau după dată) și coloanele care se văd (codul, sursele). Aceleași opțiuni sunt și în Setări › Aplicație › KBOT.

## Lista › Reîmprospătarea unui angajament
<!-- slice: 0034, 0098, 0000-23 -->
target: KbotForm.tree
part: node.icon
Butonul de la capătul unui rând apare doar când ții mouse-ul pe rând; acum îl vezi pe un rând ca exemplu. Reîmprospătează din FOREXE angajamentul acela: întâi alegi ce recepții se citesc din nou, apoi robotul îl citește. Dacă apeși pe mai multe rânduri la rând, descărcările intră în «Coada robotului» și se fac una după alta.

## Lista › Extrase de cont
<!-- slice: 0080-03, 0095-02, 0000-23 -->
target: KbotForm.tree
part: footer.left
Iconița din stânga, jos, deschide fereastra «Extrase de cont», cu toate extrasele unității. Descărcarea extraselor din FOREXE se face de acolo.

## Lista › Actualizează
<!-- slice: 0034, 0000-23 -->
target: KbotForm.tree
part: footer.right
Iconița din dreapta, jos, aduce din FOREXE angajamentele noi. Cele noi se adaugă în listă; cele existente rămân neatinse.

## Vederile
<!-- slice: 0018, 0088 -->
target: KbotForm.navViews
Fiecare vedere arată alt fel de date ale angajamentului selectat. O vedere e gri cât timp angajamentul nu are date de acel fel. Urmează, pe rând, fiecare vedere.

## Vederi › Sumar
<!-- slice: 0011, 0000-23 -->
target: KbotForm.navViews
part: item:sumar
Prima privire asupra angajamentului: datele lui și câte un rând pe indicator, cu creditul bugetar și totalurile.

## Vederi › Istoric
<!-- slice: 0022, 0000-23 -->
target: KbotForm.navViews
part: item:istoric
Toate rândurile de istoric ale angajamentului, așa cum le are FOREXE: rezervări, recepții, plăți.

## Vederi › Rezervări
<!-- slice: 0014, 0000-23 -->
target: KbotForm.navViews
part: item:rezervari
Rezervările de credite, pe luni și zile, cu valorile pe clasificații și graficul lor.

## Vederi › Recepții
<!-- slice: 0015, 0000-23 -->
target: KbotForm.navViews
part: item:receptii
Recepțiile angajamentului pe luni, cu detaliul fiecăreia pe clasificații.

## Vederi › Plăți
<!-- slice: 0017, 0000-23 -->
target: KbotForm.navViews
part: item:plati
Plățile și încasările, pe luni și zile, cu extrasul bancar al fiecărei plăți.

## Vederi › Extrase
<!-- slice: 0080-02, 0000-23 -->
target: KbotForm.navViews
part: item:extrase
Operațiunile din extrasele de cont care privesc angajamentul selectat.

## Vederi › Browser FOREXE
<!-- slice: 0074, 0000-23 -->
target: KbotForm.navViews
part: item:browser
Pagina FOREXE, în fereastra K-BOT. Apare doar cât ești conectat la FOREXE.

## Vederi › Fundamentare
<!-- slice: 0020-02, 0000-23 -->
target: KbotForm.navViews
part: item:ddf
Documentul de fundamentare al angajamentului, cu reviziile lui, documentul PDF și fișierele atașate.

## Vederi › Ordonanțare
<!-- slice: 0033, 0000-23 -->
target: KbotForm.navViews
part: item:ord
Ordonanțările de plată ale angajamentului, cu documentul fiecăreia.

## Vederi › Note corecție
<!-- slice: 0088, 0000-23 -->
target: KbotForm.navViews
part: item:notecab
Notele de corecție CAB făcute pe angajament. E gri cât timp nu s-a făcut nicio notă pe el.

## Vederi › Strânge bara
<!-- slice: 0025, 0000-23 -->
target: KbotForm.navViews
part: collapse
Butonul din colț îngustează bara vederilor la iconițe, ca să ai mai mult loc; încă un clic o desface.

## Vederea aleasă
<!-- slice: 0006 -->
target: KbotForm.viewHost
Aici se vede vederea aleasă, pentru angajamentul selectat în listă.

## Banda FOREXE
<!-- slice: 0034 -->
target: KbotForm.forexeFooter
Conectarea la FOREXE, certificatul, progresul robotului și ultimul lui mesaj. Turul «Legătura cu FOREXE» o arată pe larg.

## Bara de titlu
<!-- slice: 0028-09, 0000-01, 0097, 0000-20 -->
target: KbotForm.capBar
Bara de sus a ferestrei. O tragi cu mouse-ul ca să muți fereastra; dublu clic pe ea o mărește sau o readuce. Urmează, pe rând, ce are pe ea.

## Bara de titlu › Unitatea
<!-- slice: 0097, 0000-23 -->
target: KbotForm.capBar
part: unit
Unitatea pe care lucrezi. Dacă ai acces la mai multe, un clic pe ea te lasă să treci pe alta, fără parolă; conexiunea FOREXE, dacă e deschisă, se închide singură. Cu o singură unitate, numele ei e scris în titlu și nu se apasă.

## Bara de titlu › Setări
<!-- slice: 0072, 0000-23 -->
target: KbotForm.capBar
part: options
Deschide fereastra «Setări».

## Bara de titlu › Temă
<!-- slice: 0028-09, 0036, 0000-23 -->
target: KbotForm.capBar
part: theme
Alegi culorile ferestrelor și mărimea textului și a controalelor.

## Bara de titlu › Ajutor
<!-- slice: 0000-20, 0000-23 -->
target: KbotForm.capBar
part: help
«?» deschide meniul de ajutor: o căsuță în care scrii o întrebare, pagina despre ce ai pe ecran și tururile ghidate. Tasta F1 deschide direct pagina de ajutor, în orice fereastră.

## Bara de titlu › Minimizează
<!-- slice: 0006, 0000-23 -->
target: KbotForm.capBar
part: minimize
Ascunde fereastra în bara de activități Windows.

## Bara de titlu › Mărește
<!-- slice: 0006, 0000-23 -->
target: KbotForm.capBar
part: maximize
Fereastra ocupă tot ecranul; încă un clic o readuce la mărimea dinainte.

## Bara de titlu › Închide
<!-- slice: 0006, 0000-23 -->
target: KbotForm.capBar
part: close
Închide K-BOT.
