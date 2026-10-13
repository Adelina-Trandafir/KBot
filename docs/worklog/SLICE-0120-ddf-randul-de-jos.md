# SLICE-0120 - Editorul DDF: «Adauga rand / Sterge randul» pe randul lui «Salveaza documentul» (13.10.2026)

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md`
(sectiunea 3h). Felie inregistrata dupa unire, ca lucru deja facut (operatorul a ales numar nou, nu o sub-felie 0051).

## Ce s-a schimbat si de ce
1. Cat timp e deschisa pagina **Sectiunea A**, randul ei de butoane («Adauga rand», «Sterge randul» si starea) se
   muta in subsolul editorului (`tlySubsol` al `DdfEditForm`), langa «Salveaza documentul»: un singur rand de
   butoane, mai mult loc pentru tabel. La trecerea pe alta pagina randul se intoarce in Sectiunea A.
   `DdfEditSectiuneaAPage.AttachActionsTo` / `DetachActions`; `DdfEditForm.ActivatePage` face mutarea.
2. Impreuna cu 0113: editorul e o pagina a ferestrei principale, iar «Renunta» din subsol e ascuns (se inchide din
   «Inchide» al ribbonului sau cu Esc; inchiderea elibereaza numerele rezervate, ca inainte, din `FormClosed`).

## Fisiere
- Modificate: `src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.vb`.

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune. Probat pe ecran in `KBOT_TEST`.

## Neverificat / amanat
- Pasul turului `tur-ddf-editor` tintit pe `DdfEditForm.btnRenunta` (acum ascuns): vezi propunerea din pasul 7 al
  sesiunii (tururile nu s-au modificat fara confirmare).
- FileVersion `KBot.App`: nebumpat.
- Ajutorul: 0000-74.
