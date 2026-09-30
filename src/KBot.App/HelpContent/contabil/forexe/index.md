---
id: contabil.forexe
title: Legătura cu FOREXE
part: contabil
order: 30
parent: contabil
screens: ForexeFooterView, KbotForm.forexeFooter
keywords: forexe, forexecab, robot, certificat, sesiune, banda de jos
---
<!-- slice: 0034, 0040, 0072 -->
K-BOT lucrează în FOREXE printr-un **robot**: un browser pe care îl conduce singur, cu
certificatul tău digital. Robotul citește datele angajamentelor și, la trimiterea unui document
de fundamentare, face în FOREXE pașii pe care altfel i-ai face de mână.

Tot ce ține de FOREXE se vede în **banda de jos** a ferestrei principale.

<!-- capture: banda-forexe | caption: Banda FOREXE din josul ferestrei | prepare: Conectați-vă la FOREXE, ca banda să arate certificatul și starea. -->

| Element | Ce face |
|---------|---------|
| **Conectare** | pornește sesiunea FOREXE cu certificatul implicit |
| iconița de lângă «Conectare» | alege alt certificat |
| **Certificat** | certificatul cu care ești conectat |
| linia de stare | ultimul lucru pe care l-a făcut robotul |
| **⟲ Istoric** | acțiunile făcute prin FOREXE în această sesiune, cu rezultatul fiecăreia |
| **⟲ Browser** | arată browserul robotului |
| butonul de extindere | deschide **consola** FOREXE: progres detaliat și jurnal |

> **Atenție:** în browserul FOREXE lucrezi pe serverul real. Orice modificare făcută acolo de
> mână este definitivă și ocolește K-BOT (vezi regula de aur din [Despre K-BOT](topic:contabil)).
