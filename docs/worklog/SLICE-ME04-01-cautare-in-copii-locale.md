# SLICE-ME04-01 - KBotAdmin, «Copii locale» > «Caută în copii» (16.11.2026)

Slice given by Adelina (`/slice MEnn-mm`, resolved to ME04-01: ME01, ME02 and ME03 are taken). Built clean (`KBotAdmin.vbproj`: 0 errors,
0 warnings). Never run, never seen on screen.

## What changed and why

Request: she deleted something and no longer knows why or when; she needs to look back through the local copies and search them. The page
«Copii locale» (ME02) already had «Istoric rând» (one row by primary key). Missing: finding a row by text when the key is not known. Adelina
chose «cautare text in toate copiile» (not table browsing at a date, not a search inside one open copy).

- New button «Caută în copii» on «Copii locale» (needs one copy of the database chosen in the list; searches ALL copies of that database).
- New window `CopySearchForm`: text + optional exact table name + «Caută». Looks in every text column (binary columns skipped), case-insensitive,
  in every copy. A page shared by several copies is opened once. Each DISTINCT version of a row is one result line: first copy, last copy, number
  of copies holding it, «există și acum» (the newest copy still has exactly this version) or «dispărut/schimbat», table, key and values.
  A deleted row therefore shows the last copy it was seen in.
- Double click / «Istoricul rândului ales» opens the existing `RecordHistoryForm` with the key filled in; that form now searches by itself when
  opened with a key (`OnShown`).
- Local store only, the server is never touched.

## Files touched
- `KBotAdmin/Backup/BackupTextSearch.vb` (new: `BackupTextHit`, `BackupTextSearch.Search`)
- `KBotAdmin/Dialogs/CopySearchForm.vb`, `CopySearchForm.Designer.vb` (new)
- `KBotAdmin/Views/LocalBackupView.vb`, `.Designer.vb` - button `btnSearchCopies` (toolbar grew one column)
- `KBotAdmin/Dialogs/RecordHistoryForm.vb` - `OnShown`
- this file, `state/KBOT_STATUS_ME04.md`, index line in `KBOT_STATUS.md`

## Unverified / deferred
- Never run: no real copy store was opened. Speed on big copies unknown (every distinct page is read and decrypted once per search; no index).
- «dispărut/schimbat» cannot tell a deletion from an edit of the matched column; the row history window shows which.
- Only text matches: a number is found as the text the copy stored it as.
- Browsing a whole table «as it was» at a copy is NOT built (not requested after the question).
- Help: none (KBotAdmin is the owner's private tool).
