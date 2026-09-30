---
id: contabil.forexe.conectare
title: Conectarea la FOREXE
part: contabil
order: 10
parent: contabil.forexe
screens: ForexeFooterView.btnConectare, ForexeFooterView.btnSelectieCertificate
keywords: conectare, certificat, token, semnatura electronica, sesiune
---
<!-- slice: 0034, 0072 -->
1. Introdu în calculator dispozitivul cu **certificatul digital** (tokenul).
2. Apasă **Conectare** în banda de jos. Robotul deschide FOREXE cu certificatul implicit.
3. Dacă ai mai multe certificate, folosește iconița de lângă «Conectare» ca să alegi altul.

Când conectarea reușește, banda arată certificatul, iar în bara de vederi apare **Browser FOREXE**.

> Dacă tokenul nu e în calculator, conectarea **nu se poate face**: FOREXE nu primește
> certificatul.

## Imediat după conectare: operațiunile necorectate
<!-- slice: 0084, 0088 -->

Robotul citește pagina de start a FOREXE. Dacă acolo există tabelul **«Operațiuni necorectate»**,
K-BOT îl salvează. Operațiunile marcate «ERRRRRRRRRR» (plăți pe care FOREXE nu le-a putut lega de
un angajament) se corectează cu o notă contabilă — vezi
[Operațiuni necorelate și note de corecție](topic:contabil.notecab).

## Setări care contează
<!-- slice: 0072, 0091 -->

În **Setări › FOREXE** poți măsura viteza internetului și poți mări timpii de așteptare ai
robotului, dacă FOREXE răspunde greu și citirile ies incomplete.
