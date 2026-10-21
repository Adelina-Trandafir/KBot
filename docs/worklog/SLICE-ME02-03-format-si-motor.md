# ME02-03 — Formatul copiilor locale și motorul din KBotAdmin

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: nici o copie făcută, nici o conexiune la server, nici un test.**

## Ce s-a cerut

Copii ale bazelor pe PC-ul operatorului, într-un format propriu, criptat (conținutul nu se vede), cu spațiu economisit și cu posibilitatea
unui istoric pe momente. Format propriu hotărât de operator. Specificația: [ME02_FORMAT.md](../ME02_FORMAT.md).

## Ce s-a făcut (`KBotAdmin/Backup/`)

- `BackupCrypto.vb` — doar criptografie standard din .NET: PBKDF2-SHA256 (600.000 de iterații) → HKDF (cheie de criptare, cheie de denumire),
  AES-256-GCM (nonce aleator, tag de 16; numele fișierului intră în datele autentificate), HMAC-SHA256 pentru numele obiectelor, Brotli înainte de criptare.
- `BackupStore.vb` — dosarul: `store.kbk` (singurul fișier necriptat: sare, iterații, verificarea parolei), `o/xx/…` obiecte, `s/….snp` instantanee;
  scriere atomică (fișier temporar, `Flush(True)`, mutare), instantaneul se scrie **ultimul**, lacăt de scriere (`store.lock`), `Verify`
  (deschide fiecare obiect, recalculează numele, verifică obiectele fiecărui instantaneu).
- `BackupModels.vb` — manifestul (JSON): tabele → pagini (interval de chei, rânduri, valori de control, obiectul paginii), DDL și „extra”-uri
  (views, triggere, rutine, evenimente) ca obiecte.
- `BackupRunner.vb` — o rulare: trimite serverului paginile copiei precedente (fișier temporar în `backup/` pe server, prin SFTP, 0600), citește fluxul de
  înregistrări, scrie obiectele noi, scrie instantaneul. O bază eșuată nu oprește celelalte (`RunManyAsync`). Curățarea fișierului de pe server se face după `Try`
  (VB nu permite `Await` în `Finally`).
- `BackupKeyring.vb` — cheia păstrată pe acest calculator, protejată cu DPAPI (cont Windows curent), fișier `backup_key_<id>.dat` lângă program (în `.gitignore`).
- `BackupAccess.vb` — alege dosarul, creează sau deschide depozitul (parola de două ori la creare), folosește cheia păstrată dacă există.
- `AdminClient.StreamLinesAsync`, `UploadBytesAsync`, `DeleteRemoteAsync`; `SshSession.RunStreamingLean` (nu păstrează ieșirea, linii de ordinul MB);
  `SftpSession.UploadBytes` / `DeleteRemote`.
- `PassphraseForm` (+ Designer), setările noi din `admin_settings.json`: `BackupRoot`, `BackupKeepDaily/Weekly/Monthly`, `BackupFullEveryDays`.
- `KBotAdmin.vbproj`: pachetul `System.Security.Cryptography.ProtectedData` 8.0.0 (era deja în cache-ul NuGet).

## Hotărâri de proiectare (și de ce)

- **Pagini pe intervale de chei**, nu fișier pe rând: un obiect pe rând ar însemna milioane de fișiere; o pagină nemodificată e același obiect, deci nu ocupă loc nou.
- **Numele = HMAC cu cheie** a conținutului: dedup fără să se poată confirma un conținut cunoscut din numele fișierului.
- **Instantaneu complet**, nu lanț de diferențe: ștergerea unei copii vechi nu strică pe cele noi (retenția e simplă și sigură).
- Cifrul și derivarea cheii sunt cele din .NET; **nimic criptografic nu e inventat**.

## Nevalidat / limite

- **Nimic din acest cod nu a rulat.** Nici criptarea, nici scrierea atomică, nici citirea, nici DPAPI. Funcția «Verifică» (ME02-06) e menită să dovedească depozitul după prima copie reală.
- Nu am verificat dacă pe contul operatorului DPAPI deschide fișierul după o repornire sau după o schimbare de parolă Windows (dacă nu, rămâne parola din managerul de parole).
- Pe un depozit cu zeci de mii de obiecte, `Verify` citește totul; poate dura minute.
- Lanțul „PC oprit la ora sarcinii”: ratează ziua, nu strică nimic (vezi ME02-05).
- Parola fără recuperare: dacă se pierde, copiile nu se mai pot citi (spus în fereastra de creare).

## Fișiere

`KBotAdmin/Backup/*.vb` (nou, 9 fișiere), `KBotAdmin/PassphraseForm.vb|.Designer.vb` (nou), `KBotAdmin/AdminSettings.vb`, `AdminContext.vb` (proprietatea `LocalStore`),
`Server/AdminClient.vb`, `Ssh/SshSession.vb`, `Ssh/SftpSession.vb`, `KBotAdmin.vbproj`, `KBotAdmin/.gitignore`, `docs/ME02_FORMAT.md`.
