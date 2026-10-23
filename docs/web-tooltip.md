# Tooltip comun ADECHIT / K-BOT / VERCON

În meniul deschis din sigla aplicației, **Setări tooltip** deschide configurarea
comună. Preferințele sunt salvate în localStorage, sub cheia
`kbot-tooltip-settings-v1`, pentru același browser și aceeași origine web.
Modificările sunt preluate și de celelalte file deschise ale aplicațiilor.
Dacă stocarea este blocată, setările se aplică doar paginii curente;
formularul explică această limitare după apăsarea Salvare.

Opțiuni: aspect pastel / colorat / simplu, paletă pentru informări,
poziție, lățime maximă, emoticon și text de antet. Antetul este opțional,
**dezactivat implicit** în toate aplicațiile (`showHeader: false`). Se poate
activa din setări sau la un apel explicit `showTooltip(..., { showHeader: true })`.
Cu antet activ, textul gol folosește titlul corespunzător nivelului mesajului.
Erorile, avertizările și succesele au culori distincte.

Timpii sunt în milisecunde: întârziere înainte de afișare (implicit 0),
animație (180) și durata după apariție (6000). Durata 0 cere închidere manuală.
Durata se suspendă la hover, focus în notificare și cât timp fila este ascunsă.
Setările se pot previzualiza înainte de salvare; Implicit resetează formularul,
iar Salvare memorează valorile.

## Integrare

Componenta și CSS-ul ei sunt în `PYTHON/static/js/components/tooltip/`.
Nu folosește rute noi, baze de date sau schimbări de schemă.

```js
import { showTooltip } from '../components/tooltip/tooltip.js';

const notice = showTooltip('Documentul a fost salvat.', {
  level: 'success', title: 'Salvare', icon: 'ℹ️', delay: 0, duration: 5000,
});
// Anulează inclusiv o notificare care încă așteaptă să apară:
notice.close();
// notice.closed se rezolvă la solicitarea închiderii.
```

`showMessage(element, text, level, options)` din `portal/messages.js` livrează
mesajul contextual și tooltip-ul, păstrând jurnalul din sesiune și evenimentul
`ui:message` fără text de business. Un mesaj nou pentru același element îl
înlocuiește pe cel anterior; textul gol îl anulează. `options.tooltip: false`
permite mesaje exclusiv contextuale, inclusiv cele cu linkuri din formulare;
`options.inline: false` permite notificări fără bandă în pagină.
Opțiunile de aspect și timpi pot fi suprascrise pentru fiecare apel.

ADECHIT folosește tooltip-ul pentru confirmările informative, iar erorile
rămân și lângă zona de lucru. VERCON și K-BOT păstrează mesajele contextuale.
Notificările temporare ale monitorului de sesiune folosesc aceeași componentă.
Confirmările cu Da/Nu și avertizările de expirare a sesiunii își păstrează
comportamentul de confirmare și de numărătoare inversă.

Cel mult trei notificări sunt afișate simultan pentru fiecare context
(pagina sau un dialog nativ); celelalte așteaptă în coadă. Dialogurile au
cozi independente, astfel încât previzualizarea setărilor să poată apărea
și când pagina din fundal are notificări cu închidere manuală.
Durata începe după apariția efectivă, nu în coadă. Opțiunea `source: element`
anulează mesajul când sursa este eliminată, ascunsă printr-un părinte `hidden`
sau când dialogul nativ care o conține se închide. Notificările sunt grupate
după poziție, cu derulare pe ecrane mici. Popover API permite afișarea peste
dialoguri native, cu fallback prin z-index. Componenta nu mută focusul.
Conținutul este text simplu, iar notificările au roluri ARIA status / alert.

Numele `tooltip` este intern: exporturile ES modules nu creează obiecte globale.
Clasele și variabilele CSS folosesc prefixul `kbot-tooltip`, fără selectori
generici `.tooltip` sau modificări ale tooltipurilor din alte controale.
Containerul are `title=""`, pentru a opri tooltipurile native moștenite din
strămoșii DOM; mesajul și butonul de închidere nu au texte native `title`.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Texte din title și proba în RAPOARTE

`tooltip-titles.js` este încărcat în ADECHIT, K-BOT, VERCON și în pagina
HTML de raport ADECHIT. Preia exact textele atributelor `title`, le păstrează
în memorie și elimină atributele din DOM, inclusiv pentru controalele adăugate
sau actualizate ulterior. Pentru iframe-uri, numele accesibil este transferat
în `aria-label`, fără tooltip. Elementul HTML `<title>` al paginii este păstrat.

Pe desktop, textele apar la hover sau focus, lângă control. Escape,
activarea controlului și părăsirea lui închid tooltipul;
trecerea cursorului pe tooltip îl păstrează deschis. Un singur tooltip de
control este activ la un moment dat, independent de coada notificărilor.
Legătura accesibilă folosește `aria-describedby` și `role="tooltip"`.

Butoanele din fereastra efectivă a raportului (`.report-view`) și din bara
raportului HTML (`.rpt-bar`) au suprascrieri locale: întârziere **100 ms**,
durată **20000 ms** după animația de apariție. Textul este cel din `title`
existent, fără reformulare. Configurația generală a celorlalte controale
folosește în continuare preferințele salvate în browser.

Pe mobil nu se afișează niciun tooltip, nici notificările create prin
`showTooltip`: lățime de cel mult 900 px, lipsă hover sau pointer principal
coarse. Atingerea și apăsarea lungă nu activează tooltipuri. Textele native
`title` sunt eliminate și pe mobil. Trecerea din desktop în mobil închide
tooltipurile existente și anulează afișările întârziate; CSS-ul le ascunde
imediat. Mesajele contextuale din pagină rămân disponibile.

Pentru publicare se includ toate fișierele componentei, entry point-urile
JS modificate și `PYTHON/routes/adechit/report_render.py` (scriptul paginii
HTML de raport este acum un modul ES). Proba de pe server: deschideți un
raport și țineți cursorul pe un buton; textul apare după 100 ms și rămâne
20 de secunde dacă nu părăsiți controlul. În modul mobil nu trebuie să apară.

## Aspect compact și blocare după click

Fontul tooltipului este `--fs-2xs`, cel mai mic din `typography.css`, bold
(`--fw-strong`) și italic. Paddingul exterior este de 2 px pe fiecare latură,
iar textul nu are padding sau margini suplimentare. Lățimea se adaptează
conținutului, până la maximul configurat; valorile de lățime salvate anterior
nu mai produc bule largi pentru texte scurte. Paleta implicită are fundal
galben pal, distinct de fundalul aplicației, cu variantă contrastantă în tema
întunecată. Un mic indicator pointează către centrul controlului, deasupra
sau dedesubtul bulei, după spațiul disponibil.

Clickul sau pointerdown pe control închide tooltipul imediat, inclusiv înainte
de terminarea animației. Reapariția este blocată până la mișcarea efectivă a
mouse-ului în afara controlului apăsat. Schimbările de focus, deschiderea unui
meniu și înlocuirea DOM-ului sub cursor nu eliberează blocarea.

În VERCON, arborele **Selectare date** din RAPOARTE marchează zona nodurilor
cu `data-kbot-tooltip="off"`. Adaptorul elimină în continuare atributele native
`title`, dar nu generează tooltipuri în acea zonă, nici pentru ramuri încărcate
ulterior. Celelalte controale și ceilalți arbori păstrează comportamentul lor.
