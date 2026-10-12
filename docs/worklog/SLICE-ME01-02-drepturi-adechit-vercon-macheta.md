# SLICE-ME01-02 — drepturi ADECHIT / VERCON în machetă, nu în coloane

Stare: **IMPLEMENTAT_LOCAL**, `KBotAdmin` se compilează curat (0 avertismente, 0 erori). **Nerulat**: fereastra nu a fost deschisă, nimic scris pe server. Cererea operatorului, 12.10.2026.

## Ce s-a schimbat
- Pagina «Drepturi pe conturi» (`Views/RightsView`) nu mai are coloane de bife: au rămas Cont, Baza, Rol K-BOT. Butoanele «Toate ADECHIT» și «Fără ADECHIT» au fost înlocuite cu **«Drepturi ADECHIT…»** și **«Drepturi VERCON…»**.
- Fiecare deschide `RightsForm` (nouă) pentru contul din rândul ales: un DGV cu drepturile **unul sub altul**, coloanele **Explicație** (denumirea rolului de pe server) și **Bifă**. O bifă dă/retrage dreptul pe loc; un refuz al serverului pune bifa la loc. **Click pe titlul «Bifă»** bifează tot sau, dacă tot e bifat, debifează tot, cu **un singur apel** către server (lista de coduri separată prin virgulă, pe care `grant`/`revoke` o acceptau deja).
- Serverul nu a fost modificat: rolurile `VR_*` (și operațiile lor) există deja pe server, iar `grant`/`revoke` lucrează cu orice rol din secțiunea lor.

## Fișiere
`KBotAdmin/RightsForm.vb`, `RightsForm.Designer.vb` (noi); `KBotAdmin/Views/RightsView.vb`, `RightsView.Designer.vb`.

## Neverificat
- Nimic rulat pe ecran sau pe server. Machetele noi nu au fost deschise în designerul Visual Studio.
- `AD_EDIT_INCHISA` lipsă de pe server: butonul de creare rămâne în pagină, iar rolul apare în machetă abia după ce există.
- Fără ajutor (KBotAdmin nu este pentru clienți).
