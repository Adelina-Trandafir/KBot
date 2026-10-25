# Plan: interfata pentru subunitati (AV, de facut, NU critic)

Operator, 25.10.2026: fereastra «Plan de conturi» nu mai are randul subunitatii (felia AV01-06), iar butoanele «Subunitate noua» /
«Modifica subunitatea» / «Sterge subunitatea» au fost scoase din cod. Subunitatile trebuie adaugate printr-o interfata proprie.
Momentan nu e un pas critic: subunitatile se pot pune din baza de date sau din Migrator (AV01-05).

## Ce exista deja (ramane)
- Server: `PYTHON/routes/avacont/configurari/plan_cont/subunitati.py` - `GET` / `POST` (`/api/avacont/subunitati`) si `DELETE /api/avacont/subunitati/<id>`
  cu regulile: denumire unica in unitate; capitol (forma 65.01) unic, cu o singura exceptie (o subunitate si copiii ei pot avea acelasi capitol);
  prima subunitate este implicita; o subunitate cu conturi sau cea implicita nu se sterge.
- Client: `IConturiApi.GetSubunitatiAsync / SaveSubunitateAsync / DeleteSubunitateAsync` (`KBot.Api/Avacont/Configurari/PlanCont`).
- Dialog: `SubunitateEditForm` (`KBot.App/Avacont/Configurari/PlanCont/Views`): denumire, capitol, sector+sursa, zecimale salarii (0 sau 2),
  bifele «implicita» si «activa». Este inca in proiect si se poate refolosi; nu il deschide nimic acum.
- Modelul: `TZ_Subunitati` (separat de `VC_Subunitati`, fara legatura); subunitatea decide capitolul (65.01 = 01A, 65.02 = 02A, 65.10 = 02E, 65.11 = 01A) si `Zec2`.

## Ce trebuie decis
1. **Unde sta interfata**: o pagina proprie «Subunitati» (lista + butoane) in ribbon, fila Configurari › Date, langa «Plan de conturi»; sau o pagina
   in Setari › Aplicatie. Recomandare: pagina proprie, pentru ca planul de conturi este deja legat de ea (selectorul «Subunitate» din bara de titlu).
2. **Continutul**: lista subunitatilor (capitol, denumire, sector+sursa, implicita, activa, nr. conturi), cu «Noua», «Modifica», «Sterge» (grupul din dreapta al ribbonului,
   ca la «Plan de conturi»), refolosind `SubunitateEditForm`.
3. **Dupa salvare / stergere**: pagina «Plan de conturi», daca e deschisa, trebuie sa citeasca din nou subunitatile (azi le citeste doar la deschidere si la schimbarea
   sursei / anului) - de adaugat un mesaj intre ele sau citirea la revenirea pe pagina.
4. **Drepturi**: acum orice utilizator conectat poate scrie subunitati; rolurile AV pentru ele nu sunt hotarate.
5. **Prima subunitate**: o unitate fara nicio subunitate vede planul gol si mesajul «Unitatea nu are nicio subunitate.»; mesajul ar trebui sa trimita la noua pagina.

## Pasi propusi (cand se hotaraste)
1. Pagina `SubunitatiForm` (Avacont/Configurari/Subunitati/Views), cu `IViewRibbon` pentru butoane (mecanismul exista, AV01-06).
2. Butonul din ribbon (fila Configurari › Date) + intrare in `MachetaRegistry`, ca la «Plan de conturi».
3. Mesaj catre «Plan de conturi» dupa modificari.
4. Ajutor (sub-felie `0000-NN`) si worklog.
