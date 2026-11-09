---
name: ssh
description: Înregistrează în KBOT fișierele unui task confirmat de Adelina după probă, pentru publicare sau finalizare debugging din KBOTADMIN. Folosește registrul local de deployment.
---

# Înregistrează fișierele KBOT pentru serverul SSH

Când Adelina trimite `$ssh`, tratează comanda ca o confirmare explicită că a verificat versiunea curentă a taskului din chat și că autorizează înregistrarea. Opțional, `$ssh SLICE-wwnn-nn` identifică slice-ul confirmat. O menționare a comenzii ca exemplu, o cerere de implementare sau o cerere de publicare debugging nu reprezintă confirmare. Aplică acest flux numai în KBOT.

**Blocaj obligatoriu înainte de înregistrare.** Fiecare fișier din task trebuie să aibă deja atribuit slice-ul valid al taskului, în formatul `SLICE-wwnn-nn` din `AGENTS.md`. Dacă lipsește asocierea pentru cel puțin un fișier, dacă slice-ul este invalid sau dacă fișierul ori taskul este marcat `SLICELESS`, oprește întregul lot înainte să rulezi `Register-DeploymentTask.ps1`. Indică fișierele blocate și cere clarificarea slice-ului. Argumentul din `$ssh` identifică un slice deja atribuit. Nu inventa și nu atribui retroactiv un slice pentru a trece de acest blocaj.

1. Identifică taskul din chat și urmează `AGENTS.md` și `docs/deployment/README.md` din KBOT. Extrage slice-ul, un motiv concret al modificării și lista completă a fișierelor create sau modificate în acel task, inclusiv documentația și resursele din proiect. Folosește căi relative explicite, cu `/`. Exclude fișierele altor taskuri și registrul însuși. Dacă taskul, lista sau versiunea confirmată nu se pot stabili din context, cere numai informația lipsă.
2. Din rădăcina KBOT, rulează o singură dată `./tools/Register-DeploymentTask.ps1 -Slice SLICE-wwnn-nn -Reason 'motivul concret' -Files @('cale/relativa/fișier', ...)` cu valorile reale. Comanda utilizatoarei este suficientă pentru acest pas. Pentru editări ulterioare este necesară o nouă confirmare. Evită dublarea înregistrărilor aceleiași versiuni din același task.
3. Comunică slice-ul și fișierele înregistrate. La eșec, spune că taskul este neînchis și indică eroarea. Dacă execuția s-a întrerupt după începerea scrierii, stabilește ce intrări există înainte de orice reluare. Intrările sunt imuabile și scriptul de înregistrare calculează SHA-256.

Acest flux se încheie în `docs/deployment/pending/`. Publicarea, finalizarea debugging, SSH și Git rămân operații separate în KBOTADMIN.
