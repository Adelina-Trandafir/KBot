# Tooltip comun ADECHIT / K-BOT / VERCON

Sigla aplicației → Setări tooltip: configurare comună, salvată în localStorage, cheia `kbot-tooltip-settings-v1` pentru browserul și originea curente, preluată și de celelalte file ale aplicațiilor. Stocare blocată → setări numai în pagina curentă; formularul explică limita după Salvare.

Opțiuni: aspect pastel / colorat / simplu, paletă pentru informări, poziție, lățime maximă, emoticon și text de antet. Antetul este opțional, **dezactivat implicit** în toate aplicațiile (`showHeader: false`). Se poate activa din setări sau la un apel explicit `showTooltip(..., { showHeader: true })`. Cu antet activ, textul gol folosește titlul corespunzător nivelului mesajului. Erorile, avertizările și succesele au culori distincte.

Timpi (ms): întârziere 0 implicit, animație 180, durată după apariție 6000; durata 0 cere închidere manuală. Durata se suspendă la hover/focus în notificare sau filă ascunsă. Previzualizare înainte de salvare; Implicit resetează formularul, Salvare memorează.

## Integrare

Componenta și CSS-ul ei sunt în `PYTHON/static/js/components/tooltip/`. Nu folosește rute noi, baze de date sau schimbări de schemă.

```js
import { showTooltip } from '../components/tooltip/tooltip.js';

const notice = showTooltip('Documentul a fost salvat.', {
  level: 'success', title: 'Salvare', icon: 'ℹ️', delay: 0, duration: 5000,
});
// Anulează inclusiv o notificare care încă așteaptă să apară:
notice.close();
// notice.closed se rezolvă la solicitarea închiderii.
```

`showMessage(element,text,level,options)` din `portal/messages.js` oferă mesaj contextual și tooltip, păstrând jurnalul sesiunii și evenimentul `ui:message` fără text de business. Același element: mesajul nou îl înlocuiește pe cel vechi, textul gol anulează. `options.tooltip: false`: numai contextual, inclusiv linkuri; `options.inline: false`: numai notificare, fără bandă în pagină. Aspectul și timpii se pot suprascrie per apel.

ADECHIT folosește tooltip-ul pentru confirmările informative, iar erorile rămân și lângă zona de lucru. VERCON și K-BOT păstrează mesajele contextuale. Notificările temporare ale monitorului de sesiune folosesc aceeași componentă. Confirmările cu Da/Nu și avertizările de expirare a sesiunii își păstrează comportamentul de confirmare și de numărătoare inversă.

Maximum trei notificări simultane per context (pagină/dialog nativ); restul în coadă. Cozi independente per dialog permit previzualizarea setărilor chiar dacă fundalul are notificări manuale. Durata începe la apariția efectivă. `source: element` anulează la eliminarea sursei, ascunderea prin părinte `hidden` sau închiderea dialogului nativ care o conține. Grupare după poziție, derulare pe ecrane mici; Popover API peste dialoguri native, fallback z-index. Fără mutarea focusului; text simplu, ARIA status/alert.

Numele `tooltip` este intern: exporturile ES modules nu creează obiecte globale. Clasele și variabilele CSS folosesc prefixul `kbot-tooltip`, fără selectori generici `.tooltip` sau modificări ale tooltipurilor din alte controale. Containerul are `title=""`, pentru a opri tooltipurile native moștenite din strămoșii DOM; mesajul și butonul de închidere nu au texte native `title`.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Texte din title și proba în RAPOARTE

tooltip-titles.js în ADECHIT/K-BOT/VERCON și raportul HTML ADECHIT: memorează exact textele title și elimină atributele DOM, inclusiv la adăugări/actualizări. Iframe: numele accesibil trece în aria-label, fără tooltip. Elementul `<title>` al paginii rămâne.

Pe desktop, textele apar la hover sau focus, lângă control. Escape, activarea controlului și părăsirea lui închid tooltipul; trecerea cursorului pe tooltip îl păstrează deschis. Un singur tooltip de control este activ la un moment dat, independent de coada notificărilor. Legătura accesibilă folosește `aria-describedby` și `role="tooltip"`.

Butoanele din fereastra efectivă a raportului (`.report-view`) și din bara raportului HTML (`.rpt-bar`) au suprascrieri locale: întârziere **100 ms**, durată **20000 ms** după animația de apariție. Textul este cel din `title` existent, fără reformulare. Configurația generală a celorlalte controale folosește în continuare preferințele salvate în browser.

Pe mobil nu se afișează niciun tooltip, nici notificările create prin `showTooltip`: lățime de cel mult 900 px, lipsă hover sau pointer principal coarse. Atingerea și apăsarea lungă nu activează tooltipuri. Textele native `title` sunt eliminate și pe mobil. Trecerea din desktop în mobil închide tooltipurile existente și anulează afișările întârziate; CSS-ul le ascunde imediat. Mesajele contextuale din pagină rămân disponibile.

Pentru publicare se includ toate fișierele componentei, entry point-urile JS modificate și `PYTHON/routes/adechit/report_render.py` (scriptul paginii HTML de raport este acum un modul ES). Proba de pe server: deschideți un raport și țineți cursorul pe un buton; textul apare după 100 ms și rămâne 20 de secunde dacă nu părăsiți controlul. În modul mobil nu trebuie să apară.

## Aspect compact și blocare după click

Font: `--fs-2xs` (minimul `typography.css`), bold `--fw-strong` și italic. Padding exterior 2 px/latură, fără padding/margini pe text. Lățime după conținut până la maximul configurat; valorile vechi salvate nu mai lățesc textele scurte. Paletă implicită galben pal, distinctă de aplicație, cu variantă contrastantă pentru tema întunecată. Indicator spre centrul controlului, sus/jos după spațiul disponibil.

Clickul sau pointerdown pe control închide tooltipul imediat, inclusiv înainte de terminarea animației. Reapariția este blocată până la mișcarea efectivă a mouse-ului în afara controlului apăsat. Schimbările de focus, deschiderea unui meniu și înlocuirea DOM-ului sub cursor nu eliberează blocarea.

În VERCON, arborele **Selectare date** din RAPOARTE marchează zona nodurilor cu `data-kbot-tooltip="off"`. Adaptorul elimină în continuare atributele native `title`, dar nu generează tooltipuri în acea zonă, nici pentru ramuri încărcate ulterior. Celelalte controale și ceilalți arbori păstrează comportamentul lor.

## Istoric și proveniență

Cele trei etape din 20.10.2026 au fost confirmate TESTAT, RULAT, PUBLICAT de operator la 23.10.2026. Antetul inițial bold cu ℹ️ a devenit ulterior opțional și implicit oprit, inclusiv la normalizarea preferințelor vechi fără showHeader. Contractul de mai sus descrie rezultatul acestor etape.

- Componenta inițială: fișiere noi PYTHON/static/js/components/tooltip/{tooltip-config.js,tooltip.js,tooltip-settings.js,tooltip.css}; integrare în portal/{messages.js,app.js,portal.js,apps-menu.js}, adechit/{app.js,payers.js}, vercon/application-menu.js și session/session-monitoring-ui.js, toate sub PYTHON/static/js/. Documentație: acest fișier și implementare_vercon_v2.md.
- Preluarea title și oprirea pe mobil: noi tooltip-environment.js și tooltip-titles.js; ajustate tooltip.js, tooltip.css, tooltip-settings.js din aceeași componentă, adechit/{app.js,report-print.js}, portal/portal.js, vercon/app.js și PYTHON/routes/adechit/report_render.py.
- Aspectul compact și excluderea nodurilor: tooltip-config.js, tooltip-settings.js, tooltip.js, tooltip-titles.js, tooltip.css și PYTHON/static/js/vercon/report-selection.js.

Extensia fontului din 21.10.2026 (familie, puncte și culoare) are istoricul și limitele de publicare în [Intervenții web — Font tooltip](../PYTHON/docs/worklog/INTERVENTII_WEB_2026-10.md). Descrierea fontului compact de mai sus consemnează configurația din 20 octombrie; confirmarea din 23 octombrie a celor trei etape nu este extinsă implicit asupra intervenției de font.
