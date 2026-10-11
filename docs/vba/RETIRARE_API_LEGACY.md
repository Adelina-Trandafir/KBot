# Retragerea apelurilor API și a acreditărilor din VBA

Acest ghid însoțește [planul de securitate](../PLAN_Securitate_API.md). Stare: cod pregătit pentru integrare; API-ul serverului și frontend-urile Access nu au fost modificate de acest pas de planificare.

## Ce cod se pune în VBA

**Nu există un modul care, prin simpla importare, să elimine toate apelurile HTTP existente.** Acestea pot fi în module, cod de formular/raport, macrocomenzi, Timer, AutoExec, Ribbon sau biblioteci. Un flag nou nu este consultat automat de vechile apeluri, iar ștergerea numai a header-ului API key păstrează cererile și produce erori.

Pentru un inventar complet, importați [mdl_KBot_ExportAudit.bas](retirare_legacy/mdl_KBot_ExportAudit.bas) într-o copie de lucru a frontend-ului Access sursă, apoi executați în Immediate Window:

```vb
Kbot_ExportPentruCuratare
```

Exportă module, formulare, rapoarte, macrocomenzi și query-uri în `KBOT_VBA_EXPORT_<data_ora>`, lângă frontend. Manifestul păstrează numele obiectelor și raportează erorile. La orice eroare, exportul este declarat incomplet. Exportul folosește [Application.SaveAsText, documentat de Microsoft](https://github.com/MicrosoftDocs/office-developer-client-docs/blob/main/docs/access/desktop-database-reference/application-save-as-text.md); nu cere acces programatic la VBProject și nu modifică obiectele originale.

Se execută pe sursa MDB/ACCDB cu Access complet. Nu se poate folosi un frontend compilat ACCDE/MDE sau o bază cu proiectul inaccesibil ca dovadă că sursa a fost exportată complet. Codul nu a fost compilat într-o sesiune Access în acest workspace.

Exporturile conțin sursa originală și pot conține secretele încă neeliminate. Rămân locale pe durata curățării; manifestul nu copiază valorile acestora. Tabelele de configurație, registrul și fișierele externe se inventariază separat: acest modul exportă definiții, nu toate datele.

## Înlocuire concretă pentru modulul existent în repository

Modulul original `mdl_FX_CLS_SYNC_TO_KBOT` trimite clasificări la API și setează X-API-Key. Înlocuiți-l integral cu [varianta retrasă](retirare_legacy/mdl_FX_CLS_SYNC_TO_KBOT.bas), păstrând numele modulului; nu importați încă un modul cu aceeași funcție.

Varianta tranzitorie păstrează semnătura funcției, dar nu trimite nimic și ridică o eroare explicită:

```vb
Public Function FX_CLS_Sync_To_KBot(ByVal vIdUnitate As Long, ByVal vAn As Long, _
                                   Optional ByVal vIdClsf As Long = 0, _
                                   Optional ByVal vDataInceput As Variant) As Boolean
    FX_CLS_Sync_To_KBot = False
    Err.Raise vbObjectError + 2112, "FX_CLS_Sync_To_KBot", _
              "Sincronizarea VBA cu K-BOT a fost retrasa. Folositi Kbot.App."
End Function
```

La curățarea finală se șterg apelurile funcției din formulare și macrocomenzi, controlul de UI dacă nu mai are scop, apoi modulul stub. Aceeași regulă se aplică celorlalți entry points HTTP, cu semnăturile lor reale. Nu se returnează True/200, nu se ignoră eroarea și nu se confirmă o sincronizare care nu a avut loc.

Pentru un handler exclusiv de trimitere, codul tranzitoriu al butonului poate fi:

```vb
Private Sub cmdTrimiteKbot_Click()
    MsgBox "Trimiterea din VBA a fost retrasa. Folositi Kbot.App.", _
           vbInformation, "K-BOT"
End Sub
```

Acest exemplu înlocuiește doar acțiunea butonului respectiv; nu blochează apeluri din alte locuri. Dacă handler-ul are și salvări locale necesare, ele se păstrează separat după revizuire.

## Eliminarea peste tot

1. Căutare în toate obiectele exportate: `API_KEY`, `ApiKey`, `X-API-Key`, `API_BASE_URL`, `Authorization`, `Bearer`, `/api/`, hostname-urile vechi, `WinHttpRequest`, `XMLHTTP`, `ServerXMLHTTP`, `MSXML2`, `.SetRequestHeader`, `.Send`, `.Open`, `URLDownloadToFile`, comenzi curl/PowerShell și helper-ele proprii de HTTP. Verificarea este manuală pe apelanți și destinație: unele HTTP-uri pot aparține altui serviciu, iar URL-urile pot fi construite din bucăți.
2. Eliminarea procedurilor exclusiv API sau separarea și păstrarea numai a operațiilor locale. Eliminarea apelurilor lor din toate evenimentele și macrocomenzile; pentru Timer/AutoExec se elimină și planificarea/retry-urile.
3. Ștergerea constantelor/variabilelor cu acreditări și a citirilor lor din registru, tabele de configurare și fișiere. Ștergerea scrierilor care ar recrea aceste valori. Nu se înlocuiește secretul cu șir gol și nu se mută într-un alt modul/fișier.
4. Verificarea procedurilor auxiliare, DLL-urilor și referințelor care pot face trimiterea indirect. Referințele la WinHTTP/MSXML se elimină numai dacă nu mai sunt folosite de funcționalitățile rămase; eliminarea unei referințe nu blochează CreateObject late-bound.
5. Eliminarea separată a secretelor ANAF/tokenurilor dacă există în VBA. Un `Dictionary.Key` sau o cheie primară dintr-un tabel nu reprezintă acreditare și nu se șterge prin replace global.
6. Debug → Compile, apoi testarea fluxurilor locale rămase. Reexport și verificarea apelanților, inclusiv macrocomenzi încorporate și query-uri care execută funcții. Verificarea efectivă a traficului în pornire și în operațiile vechi.
7. Rebuild și distribuire ACCDE/MDE pentru toate frontend-urile, inclusiv lansatoare și variante de pe share. API-ul trebuie să refuze deja cheia veche, astfel încât versiunile vechi rămase pe calculatoare să nu poată accesa serverul.

În repository există numai două exporturi VBA relevante. Nu se poate declara «eliminat peste tot din VBA» fără sursele tuturor frontend-urilor folosite. Bazele ADECHIT prezente nu înlocuiesc inventarul frontend-ului AVACONT/FOREXE.

## Exportul offline pentru migrare

`Surse/VBA_MIGRARE/mdl_FX_ExportSeed.bas` exportă fișiere JSON local; nu este în sine un client HTTP. Poate rămâne ca export local dacă este necesar. Conține totuși o parolă Access literală. Aceasta trebuie eliminată împreună cu orice valoare implicită echivalentă; parola se primește la rulare printr-un mecanism potrivit, fără păstrarea ei în sursă sau loguri. Migratorul .NET care consumă exportul trebuie adaptat la canalul administrativ nou, fără rutele HTTP cu cheie.

## Condiții de finalizare

- Nicio acreditare pentru API-ul Python în VBA sau în configurația lui.
- Niciun traseu executabil către API-ul Python, direct sau prin helper/bibliotecă.
- Niciun mesaj de succes fals pentru o funcționalitate retrasă.
- Frontend-uri compilate și distribuite din sursa curățată.
- Toate endpoint-urile și aliasurile legacy retrase server-side, pe toate intrările: 404/410 și fără efecte.

Doar schimbarea VBA nu protejează serverul de un client vechi sau modificat; doar închiderea serverului nu elimină secretele și erorile din VBA. Ambele părți sunt incluse în plan.
