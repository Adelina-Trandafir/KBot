# ADECHIT · coloanele istoricului și comutarea modului de asociere

13.10.2026. Ajustări solicitate pentru fereastra Asociere extrase cu copii.

- Plătitor: bază și minimum 242 px, conform imaginii. Ocupă automat spațiul
  rămas, fără să poată fi restrâns sub această lățime.
- Coloana CUI își păstrează datele și primește titlul CNP Plătitor și lățimea
  135 px, față de 90 px (+50%).
- Cont: 168 px, față de 210 px (−20%).
- Coloana nouă Banca are 152 px, conform aceleiași imagini. Închis are 60 px.
  Istoricul are o lățime de bază de 757 px; numai Plătitor preia spațiul liber.
  Valoarea de afișare BancaCont se calculează din cont, folosind aceeași regulă
  ca Parteneri: IBAN fără spații, în majuscule, caracterele 5–8 căutate în
  AVACONT_COMUN.BIC (Cod → Banca). Conturile fără cod cunoscut au banca goală.
  Valoarea Banca păstrată în istoricul existent nu este rescrisă de această citire.
- Endpoint-ul bank-extracts/payers citește copilul și plătitorii numai din
  subunitatea autorizată, apoi consultă BIC prin conexiunea comună K-BOT.
  Conexiunea și cursorul se închid după citire; nu se execută modificări SQL.
- Butonul Asociere manuală este ascuns în modul manual; butonul Previzualizare
  automată este ascuns în modul automat. Rămâne vizibil butonul de trecere
  la celălalt mod. Starea se reaplică la fiecare deschidere a ferestrei.

Modificate: bank-associations.js, adechit.html, bank_associations.py și
routes/adechit/__init__.py. Nu sunt necesare modificări de schemă.
Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
