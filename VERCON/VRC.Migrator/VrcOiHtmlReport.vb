Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Text

' SLICE-VC10-01. Business report built from the analysis snapshot, without querying Access or the server.
Friend NotInheritable Class VrcOiHtmlReport
    Private Shared ReadOnly Ro As CultureInfo = CultureInfo.GetCultureInfo("ro-RO")
    Private Shared ReadOnly Names As StringComparer = StringComparer.Create(Ro, True)

    Private Sub New()
    End Sub

    Friend Shared Function Create(k_result As VrcOiConversion, k_choices As List(Of VrcOiChoice), k_catalog As List(Of VrcOiData), k_repeats As List(Of VrcOiRepeat), k_accepted As HashSet(Of String), k_generate As Boolean) As String
        Dim k_body As New StringBuilder()
        Dim k_blocked = k_result.Blockers.Count > 0
        k_body.Append("<div class='card status ").Append(If(k_blocked, "blocked", "ready")).Append("'><h2>").Append(If(k_blocked, "Generarea este oprită", If(k_generate, "Opțiunile sunt pregătite în plan", "Analiza permite generarea"))).AppendLine("</h2>")
        P(k_body, If(k_blocked, $"Sunt {k_result.Blockers.Count} probleme de rezolvat. Niciuna dintre modificările de mai jos nu este aplicată planului cât timp există blocaje.", If(k_generate, "Datele sunt pregătite în memorie. Transferul se face din Migrare, prin Verifică planul → Testează → Migrează.", "Nu s-a modificat planul. Folosiți Generează separarea pentru aplicarea opțiunilor.")))
        P(k_body, "Acest raport descrie o pregătire a migrării. Nu s-a scris în bazele Access sau pe server prin această analiză.")
        k_body.AppendLine("</div><div class='card'><h2>Ce se întâmplă cu datele</h2>")
        P(k_body, "Materiale nebifat păstrează contul în gestiunea inițială. Materiale bifat îl mută într-o gestiune nouă, cu documentele sale anuale. OI creează o gestiune multianuală; Nu migra elimină contul și datele lui din transfer.")
        P(k_body, $"Pentru OI, intrările ajung în «{VrcOiChoice.WarehouseAccount}» (OIM). Fiecare BC eligibil produce un BP și BC-ul aferent: cantitatea iese din magazie și intră în folosință, în «{VrcOiChoice.DefaultUseAccount}» (OIF), la deținător.")
        P(k_body, "Produsele cu preț NULL și mișcările lor sunt omise fără blocaj. Zero este un preț completat și nu este tratat ca NULL. Un document rămas fără linii după omitere nu este migrat; la documentele parțiale se păstrează valorile antetului din sursă, fără recalculare.")
        P(k_body, "NIR-urile cu produse din mai multe conturi se împart automat pe conturile produselor. Părțile păstrează numărul, data și factura originală, inclusiv în OI. Fiecare parte primește numai liniile sale și totalul valorilor cu TVA de pe acele linii. Contul din antet urmează produsele; împărțirea nu este un blocaj.")
        k_body.AppendLine("</div>")

        Dim k_clean = k_catalog.ToDictionary(Function(k_d) k_d.Source.Source.Path, Function(k_d) VrcOiNullPrices.Prepare(k_d, New List(Of VrcOiIgnoredPrice)()), StringComparer.OrdinalIgnoreCase)
        Destinations(k_body, k_choices, k_catalog, k_clean)
        Problems(k_body, k_result.Blockers)
        ReceiptFragments(k_body, k_result.ReceiptFragments, k_clean)
        MixedDocuments(k_body, k_choices, k_clean)
        Ignored(k_body, k_result.IgnoredPrices)
        Holders(k_body, k_choices, k_clean.Values.ToList())
        Years(k_body, k_result.Years)
        Repeats(k_body, k_repeats, k_accepted)
        Documents(k_body, k_result.Documents, k_clean)
        k_body.AppendLine("<div class='card'><h2>Limitele reconstrucției</h2>")
        P(k_body, "Un obiect predat într-un an rămâne în folosință și în anii următori, chiar dacă nu mai apare în baza anuală de Materiale. Absența lui nu generează o casare. Reportul de magazie se elimină din anul următor numai după compararea cu închiderea anului precedent.")
        P(k_body, "Sold neconcluziv înseamnă că un document sau o linie a fost respinsă, astfel încât intrările și ieșirile acelui an nu sunt complete pentru comparație. Nu înseamnă că a fost demonstrată o lipsă de stoc. Problemele sunt enumerate mai sus.")
        P(k_body, "Nu se reconstruiesc predări dinaintea primei baze încărcate. Același nume și același preț nu dovedesc că două obiecte sunt fizic același exemplar; suprapunerile cer confirmarea unei predări distincte.")
        k_body.AppendLine("</div>")
        Return Page("Separarea conturilor din Materiale", k_body.ToString())
    End Function

    Friend Shared Function Messages(k_title As String, k_problems As List(Of String)) As String
        Dim k_body As New StringBuilder()
        Problems(k_body, k_problems)
        Return Page(k_title, k_body.ToString())
    End Function

    Private Shared Sub Destinations(k_body As StringBuilder, k_choices As List(Of VrcOiChoice), k_catalog As List(Of VrcOiData), k_clean As Dictionary(Of String, VrcOiData))
        k_body.AppendLine("<div class='card'><h2>1. Destinațiile și conturile excluse</h2>")
        StartTable(k_body, "Subunitate / baza sursă", "An", "Cont sursă", "Decizie", "Produse eligibile", "Circuit OI")
        For Each k_data In k_catalog.OrderBy(Function(k_d) k_d.Source.SubunitName, Names).ThenBy(Function(k_d) k_d.Source.Year).ThenBy(Function(k_d) k_d.Source.Source.Path, Names)
            For Each k_account In k_data.Rows("Masini").Where(Function(k_r) VrcOiData.Key(k_r, "IdMasina") <> "0").OrderBy(Function(k_r) VrcOiData.Text(k_r, "Masina"), Names)
                Dim k_key = VrcOiData.Key(k_account, "IdMasina")
                Dim k_choice = k_choices.FirstOrDefault(Function(k_c) SameFile(k_c.Data, k_data.Source.Source.Path) AndAlso k_c.AccountKey = k_key)
                Dim k_decision = If(k_choice Is Nothing, "Materiale: rămâne în «" & k_data.Source.GestiuneName & "»", If(k_choice.TargetType = VrcOiChoice.ExcludeType, "NU MIGRA — fără destinație", k_choice.TargetType & ": gestiune nouă «" & k_choice.TargetName & "»"))
                Dim k_count = k_clean(k_data.Source.Source.Path).Rows("Produse").Where(Function(k_r) VrcOiData.Key(k_r, "IdMasina") = k_key).Count()
                Row(k_body, k_data.Source.SubunitName & Environment.NewLine & k_data.Source.Source.Path, k_data.Source.Year, VrcOiData.Text(k_account, "Masina"), k_decision, k_count, If(k_choice IsNot Nothing AndAlso k_choice.TargetType = "OI", VrcOiChoice.WarehouseAccount & " (OIM) → " & k_choice.UseAccountName & " (OIF)", "—"))
            Next
        Next
        EndTable(k_body)
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub Problems(k_body As StringBuilder, k_problems As List(Of String))
        k_body.AppendLine("<div class='card'><h2>2. Probleme care împiedică generarea</h2>")
        If k_problems.Count = 0 Then
            P(k_body, "Nu au fost identificate blocaje în această analiză.")
        Else
            For Each k_group In k_problems.GroupBy(Function(k_p) Category(k_p)).OrderBy(Function(k_g) k_g.Key, Names)
                k_body.Append("<h3>").Append(H(k_group.Key)).Append(" — ").Append(k_group.Count()).AppendLine("</h3>")
                StartTable(k_body, "Constatare și document afectat", "De ce contează / ce trebuie făcut")
                For Each k_problem In k_group.OrderBy(Function(k_p) k_p, Names)
                    Row(k_body, k_problem, ActionFor(k_group.Key))
                Next
                EndTable(k_body)
            Next
        End If
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Function Category(k_text As String) As String
        If k_text.Contains("alte conturi", StringComparison.OrdinalIgnoreCase) OrElse k_text.Contains("contul antetului", StringComparison.OrdinalIgnoreCase) Then Return "Documente care amestecă mai multe conturi"
        If k_text.Contains("deținător", StringComparison.OrdinalIgnoreCase) OrElse k_text.Contains("primitor", StringComparison.OrdinalIgnoreCase) Then Return "Primitor neidentificabil"
        If k_text.Contains("repetat", StringComparison.OrdinalIgnoreCase) OrElse k_text.Contains("două ori", StringComparison.OrdinalIgnoreCase) Then Return "Posibile dubluri"
        If k_text.Contains("sold", StringComparison.OrdinalIgnoreCase) OrElse k_text.Contains("magazie negativă", StringComparison.OrdinalIgnoreCase) OrElse k_text.Contains("deschidere", StringComparison.OrdinalIgnoreCase) Then Return "Stoc și continuitate între ani"
        If k_text.Contains("preț", StringComparison.OrdinalIgnoreCase) OrElse k_text.Contains("cantitat", StringComparison.OrdinalIgnoreCase) Then Return "Valori de pe documente"
        Return "Alte date sau opțiuni de clarificat"
    End Function

    Private Shared Function ActionFor(k_category As String) As String
        Select Case k_category
            Case "Documente care amestecă mai multe conturi"
                Return "NIR-urile cu produse identificabile sunt împărțite automat și nu blochează. Pentru un BC mixt sau o linie fără produs/cont identificabil, consultați detaliile documentului; nu se alege un cont prin presupunere."
            Case "Primitor neidentificabil"
                Return "Nu există un nume utilizabil în seria anuală a acestui deținător. Completați primitorul în sursă și recitiți bazele; o persoană nu este inventată automat. Schimbarea numelui unei înregistrări existente se asociază automat și nu este blocaj."
            Case "Posibile dubluri"
                Return "Comparați documentele și produsele. Pentru o predare nouă, distinctă, confirmați rândul din grila suprapunerilor și regenerați. O dublură reală nu trebuie confirmată ca predare nouă."
            Case "Stoc și continuitate între ani"
                Return "Comparați închiderea anului precedent cu deschiderea celui următor și documentele indicate. Nu se adaugă intrări, ieșiri sau casări pentru a acoperi o diferență."
            Case "Valori de pe documente"
                Return "Verificați cantitatea și prețul numeric din documentul indicat. Produsele cu preț NULL sunt omise separat și nu provoacă blocaj; valorile nenumerice sau corecțiile cu semne speciale trebuie clarificate."
            Case Else
                Return "Consultați constatarea, baza și documentul indicate. Corectați opțiunea sau datele sursă, recitiți bazele când au fost modificate și regenerați raportul."
        End Select
    End Function

    Private Shared Sub ReceiptFragments(k_body As StringBuilder, k_fragments As List(Of VrcOiReceiptFragment), k_catalog As Dictionary(Of String, VrcOiData))
        k_body.AppendLine("<div class='card'><h2>3. NIR-uri împărțite automat — fără blocaj</h2>")
        If k_fragments.Count = 0 Then P(k_body, "Nu a fost necesară împărțirea NIR-urilor sau corectarea contului din antet pentru opțiunile alese.")
        For Each k_group In k_fragments.GroupBy(Function(k_f) k_f.Source & "|" & k_f.OriginalKey).OrderBy(Function(k_g) k_g.First().Day).ThenBy(Function(k_g) NumericLabel(k_g.First().Number)).ThenBy(Function(k_g) k_g.First().Source, Names)
            Dim k_first = k_group.First()
            Dim k_data = k_catalog(k_first.Source)
            k_body.Append("<h3>").Append(H($"NIR {k_first.Number} / {k_first.Day:dd.MM.yyyy}, factura {k_first.Invoice}")).AppendLine("</h3>")
            P(k_body, k_first.Source & " · totalul antetului original: " & F(k_first.OriginalValue) & ". Fiecare parte păstrează același număr și aceeași dată. Liniile păstrează cantitățile, prețurile și valorile sursă.")
            StartTable(k_body, "Contul produselor", "Destinație", "Număr NIR", "Data NIR", "Valoarea părții", "Linii")
            For Each k_part In k_group.OrderBy(Function(k_f) k_f.Account, Names)
                Row(k_body, k_part.Account, k_part.Destination, k_part.Number, k_part.Day, k_part.Value, k_part.Lines.Count)
            Next
            EndTable(k_body)
            For Each k_part In k_group.OrderBy(Function(k_f) k_f.Account, Names)
                P(k_body, k_part.Account & " → " & k_part.Destination)
                StartTable(k_body, "Produs", "UM", "Cantitate", "Preț", "Valoare cu TVA din sursă")
                For Each k_line In SortLines(k_data, k_part.Lines)
                    Dim k_product = Product(k_data, k_line)
                    Row(k_body, ProductName(k_product), If(k_product Is Nothing, "", VrcOiData.Text(k_product, "UM")), VrcOiData.Value(k_line, "Cant"), VrcOiData.Value(k_line, "PUTVA"), VrcOiData.Value(k_line, "ValoareCUTVA"))
                Next
                EndTable(k_body)
            Next
        Next
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub MixedDocuments(k_body As StringBuilder, k_choices As List(Of VrcOiChoice), k_catalog As Dictionary(Of String, VrcOiData))
        k_body.AppendLine("<div class='card'><h2>Bonuri de consum cu conturi amestecate — de clarificat</h2>")
        Dim k_count As Integer = 0
        For Each k_choice In k_choices.OrderBy(Function(k_c) k_c.Data.Source.Year).ThenBy(Function(k_c) k_c.AccountName, Names)
            Dim k_data = k_catalog(k_choice.Data.Source.Source.Path)
            For Each k_kind In {"BC"}
                Dim k_nir = k_kind = "NIR"
                Dim k_key = If(k_nir, "IdIntrare", "IdBonConsum")
                Dim k_table = If(k_nir, "Intrari", "Iesiri")
                Dim k_number = If(k_nir, "Numar", "NumarBC")
                Dim k_day = If(k_nir, "Data", "DataBC")
                For Each k_header In k_data.Rows(If(k_nir, "NIR", "Iesiri_H")).OrderBy(Function(k_r) DayOf(k_r, k_day)).ThenBy(Function(k_r) NumericLabel(VrcOiData.Text(k_r, k_number)))
                    Dim k_lines = k_data.Rows(k_table).Where(Function(k_r) VrcOiData.Key(k_r, k_key) = VrcOiData.Key(k_header, k_key)).ToList()
                    Dim k_selected = k_lines.Any(Function(k_r) AccountKey(k_data, k_r) = k_choice.AccountKey)
                    If Not k_selected AndAlso VrcOiData.Key(k_header, "IdMasina") <> k_choice.AccountKey Then Continue For
                    Dim k_other = k_lines.Any(Function(k_r) AccountKey(k_data, k_r) <> k_choice.AccountKey)
                    Dim k_headerMismatch = VrcOiData.Key(k_header, "IdMasina").Length > 0 AndAlso VrcOiData.Key(k_header, "IdMasina") <> "0" AndAlso VrcOiData.Key(k_header, "IdMasina") <> k_choice.AccountKey
                    If Not k_other AndAlso Not k_headerMismatch Then Continue For
                    k_count += 1
                    k_body.Append("<h3>").Append(H(k_kind & " " & VrcOiData.Text(k_header, k_number) & " / " & F(VrcOiData.Value(k_header, k_day)))).AppendLine("</h3>")
                    P(k_body, k_data.Source.Source.Path & " · cont ales: " & k_choice.AccountName & " · opțiune: " & k_choice.TargetType)
                    P(k_body, "Contul din antet: " & AccountName(k_data, VrcOiData.Key(k_header, "IdMasina")) & If(k_nir, " · factura: " & VrcOiData.Text(k_header, "NrFact"), " · primitor: " & VrcOiHolders.SourceName(k_data, VrcOiData.Key(k_header, "IDC"))))
                    StartTable(k_body, "Produs", "UM", "Contul produsului", "Cantitate", "Preț", "Valoare din sursă", "Efectul opțiunii")
                    For Each k_line In SortLines(k_data, k_lines)
                        Dim k_product = Product(k_data, k_line)
                        Row(k_body, ProductName(k_product), If(k_product Is Nothing, "", VrcOiData.Text(k_product, "UM")), AccountName(k_data, AccountKey(k_data, k_line)), VrcOiData.Value(k_line, "Cant"), VrcOiData.Value(k_line, "PUTVA"), VrcOiData.Value(k_line, "ValoareCUTVA"), If(AccountKey(k_data, k_line) = k_choice.AccountKey, If(k_choice.TargetType = VrcOiChoice.ExcludeType, "De exclus", "De separat"), "Din alt cont — nu se mută odată cu acest cont"))
                    Next
                    EndTable(k_body)
                Next
            Next
        Next
        If k_count = 0 Then P(k_body, "Nu s-au identificat BC-uri cu conturi amestecate pentru opțiunile alese.")
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub Ignored(k_body As StringBuilder, k_items As List(Of VrcOiIgnoredPrice))
        k_body.AppendLine("<div class='card'><h2>4. Preț NULL — omise, fără blocaj</h2>")
        P(k_body, $"{k_items.Count} poziții de nomenclator sau mișcare omise. Lista include separat varianta și mișcările afectate; numărul nu reprezintă un total de exemplare.")
        StartTable(k_body, "Baza sursă", "An", "Cont", "Produs", "UM", "Document / poziție omisă", "Cantitate din sursă")
        For Each k_item In k_items.OrderBy(Function(k_i) k_i.Product, Names).ThenBy(Function(k_i) k_i.Year).ThenBy(Function(k_i) k_i.Source, Names).ThenBy(Function(k_i) k_i.Day).ThenBy(Function(k_i) k_i.DocumentNumber)
            Row(k_body, k_item.Source, k_item.Year, k_item.Account, k_item.Product, k_item.Unit, k_item.Document, k_item.Quantity)
        Next
        EndTable(k_body)
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub Holders(k_body As StringBuilder, k_choices As List(Of VrcOiChoice), k_catalog As List(Of VrcOiData))
        k_body.AppendLine("<div class='card'><h2>5. Deținători — asocieri automate și istoricul numelor</h2>")
        P(k_body, "Aceeași înregistrare de deținător din aceeași serie de baze anuale se asociază automat la cel mai recent nume disponibil. O schimbare de persoană scrisă peste acea înregistrare urmează aceeași regulă. Numele originale de pe fiecare an sunt păstrate în tabelul de mai jos. Înregistrările cu același nume normalizat sunt reunite în destinație.")
        P(k_body, "Identificatorii locali din foldere sau gestiuni sursă diferite nu sunt asociați între ei doar pentru că au același număr. Referințele fără o identitate și fără un nume recuperabil rămân probleme de date.")
        Dim k_aliases = VrcOiHolders.Automatic(k_catalog)
        StartTable(k_body, "Baza / anul", "Numele în sursă", "Numele în destinație", "Bonuri afectate", "Asociere")
        For Each k_data In k_catalog.OrderBy(Function(k_d) k_d.Source.Year).ThenBy(Function(k_d) k_d.Source.Source.Path, Names)
            Dim k_accounts = k_choices.Where(Function(k_c) SameFile(k_c.Data, k_data.Source.Source.Path) AndAlso k_c.TargetType <> VrcOiChoice.ExcludeType).Select(Function(k_c) k_c.AccountKey).ToHashSet(StringComparer.Ordinal)
            Dim k_bonKeys = k_data.Rows("Iesiri").Where(Function(k_r) k_accounts.Contains(AccountKey(k_data, k_r))).Select(Function(k_r) VrcOiData.Key(k_r, "IdBonConsum")).ToHashSet(StringComparer.Ordinal)
            For Each k_group In k_data.Rows("Iesiri_H").Where(Function(k_r) k_bonKeys.Contains(VrcOiData.Key(k_r, "IdBonConsum")) OrElse k_accounts.Contains(VrcOiData.Key(k_r, "IdMasina"))).GroupBy(Function(k_r) VrcOiData.Key(k_r, "IDC")).OrderBy(Function(k_g) VrcOiHolders.SourceName(k_data, k_g.Key), Names)
                Dim k_alias = k_aliases.FirstOrDefault(Function(k_h) String.Equals(k_h.SourcePath, k_data.Source.Source.Path, StringComparison.OrdinalIgnoreCase) AndAlso k_h.SourceKey = k_group.Key)
                Dim k_target = If(k_alias?.TargetName, String.Empty)
                Row(k_body, k_data.Source.Source.Path & " · " & k_data.Source.Year.ToString(Ro), VrcOiHolders.SourceName(k_data, k_group.Key), If(k_target.Length = 0, "Nu poate fi identificat", k_target), String.Join("; ", k_group.OrderBy(Function(k_b) VrcOiHolders.DocumentDate(k_b)).ThenBy(Function(k_b) VrcOiHolders.DocumentNumber(k_b)).Select(Function(k_b) VrcOiHolders.DocumentLabel(k_b))), If(k_target.Length = 0, "De clarificat în sursă", "Automată — cel mai recent nume"))
            Next
        Next
        EndTable(k_body)
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub Years(k_body As StringBuilder, k_years As List(Of VrcOiYearSummary))
        k_body.AppendLine("<div class='card'><h2>6. Continuitatea OI și stocul de magazie pe ani</h2>")
        P(k_body, "Numerele sunt ale documentelor candidate pregătite. Dacă există blocaje, ele nu sunt documente deja migrate. «Report eliminat» este o intrare de stoc inițial care ar dubla stocul rămas din anul precedent.")
        StartTable(k_body, "Destinație / cont sursă / baza", "An", "NIR candidate", "Reporturi eliminate", "BC candidate pentru BP", "Starea reconcilierii", "Magazie la sfârșitul anului")
        For Each k_year In k_years.OrderBy(Function(k_y) k_y.Subunit, Names).ThenBy(Function(k_y) k_y.Destination, Names).ThenBy(Function(k_y) k_y.Account, Names).ThenBy(Function(k_y) k_y.Year)
            Dim k_stock = k_year.Stock.OrderBy(Function(k_s) k_s.Product, Names).ThenBy(Function(k_s) k_s.Unit, Names).ThenBy(Function(k_s) k_s.Price).Select(Function(k_s) $"{k_s.Product} / {k_s.Unit}, preț {F(k_s.Price)}: {F(k_s.Quantity)}")
            Row(k_body, k_year.Subunit & " / " & k_year.Destination & Environment.NewLine & k_year.Account & Environment.NewLine & k_year.Source, k_year.Year, k_year.Receipts, k_year.RemovedOpenings, k_year.Handovers, If(k_year.BalanceComplete, "Calculabil din documentele acestui an", "Neconcluziv: documente/valori respinse; consultați problemele"), If(k_year.BalanceComplete, If(k_year.Stock.Count = 0, "Stoc zero", String.Join(Environment.NewLine, k_stock)), "Nu se afirmă un sold complet"))
        Next
        EndTable(k_body)
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub Repeats(k_body As StringBuilder, k_repeats As List(Of VrcOiRepeat), k_accepted As HashSet(Of String))
        k_body.AppendLine("<div class='card'><h2>7. Posibile suprapuneri ale predărilor</h2>")
        P(k_body, "Se compară produsul, unitatea de măsură, prețul, deținătorul asociat automat și contul de folosință între baze diferite. Confirmarea unei predări distincte se face în grila suprapunerilor din aplicație, apoi se regenerează raportul.")
        If k_repeats.Count = 0 Then P(k_body, "Nu au fost semnalate suprapuneri în documentele eligibile analizate. Documentele blocate nu sunt o dovadă a absenței dublurilor.")
        StartTable(k_body, "Produs / UM", "Preț", "Deținător", "Predare anterioară", "Predare analizată", "Cantitate", "Decizie")
        For Each k_item In k_repeats.OrderBy(Function(k_r) k_r.Product, Names).ThenBy(Function(k_r) k_r.Unit, Names).ThenBy(Function(k_r) k_r.Price).ThenBy(Function(k_r) k_r.Holder, Names)
            Row(k_body, k_item.Product & " / " & k_item.Unit, k_item.Price, k_item.Holder, k_item.PreviousFile, k_item.File, k_item.Quantity, If(k_accepted.Contains(k_item.Key), "Predare distinctă confirmată", "Așteaptă clarificare — blochează"))
        Next
        EndTable(k_body)
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Sub Documents(k_body As StringBuilder, k_documents As List(Of VrcOiDocumentSummary), k_catalog As Dictionary(Of String, VrcOiData))
        k_body.AppendLine("<div class='card'><h2>8. Documente — traseu și linii</h2>")
        P(k_body, "Fiecare document de mai jos arată sursa și operația rezultată. O problemă pe o linie lasă documentul incomplet; niciun candidat nu este transferat cât timp analiza are blocaje. NIR-urile de report eliminate sunt numărate în situația anuală și nu devin intrări noi.")
        For Each k_doc In k_documents.OrderBy(Function(k_d) k_d.Destination, Names).ThenBy(Function(k_d) k_d.Day).ThenBy(Function(k_d) NumericLabel(k_d.Number)).ThenBy(Function(k_d) k_d.Kind, Names)
            Dim k_data = k_catalog(k_doc.Source)
            k_body.Append("<h3>").Append(H(k_doc.Kind & " " & k_doc.Number & " / " & k_doc.Day.ToString("dd.MM.yyyy", Ro))).AppendLine("</h3>")
            P(k_body, k_doc.Source & " → «" & k_doc.Destination & "» · " & k_doc.Target & " · " & If(k_doc.Complete, "Candidat complet", "Candidat incomplet"))
            StartTable(k_body, "Produs", "UM", "Cantitate", "Preț", "Valoare din sursă")
            For Each k_line In SortLines(k_data, k_doc.Lines)
                Dim k_product = Product(k_data, k_line)
                Row(k_body, ProductName(k_product), If(k_product Is Nothing, "", VrcOiData.Text(k_product, "UM")), VrcOiData.Value(k_line, "Cant"), VrcOiData.Value(k_line, "PUTVA"), VrcOiData.Value(k_line, "ValoareCUTVA"))
            Next
            EndTable(k_body)
        Next
        k_body.AppendLine("</div>")
    End Sub

    Private Shared Function Product(k_data As VrcOiData, k_line As Dictionary(Of String, Object)) As Dictionary(Of String, Object)
        Dim k_variant = k_data.Rows("ProduseP").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdProdus") = VrcOiData.Key(k_line, "IdProdus"))
        Return If(k_variant Is Nothing, Nothing, k_data.Rows("Produse").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "ID") = VrcOiData.Key(k_variant, "ID")))
    End Function

    Private Shared Function ProductName(k_product As Dictionary(Of String, Object)) As String
        Return If(k_product Is Nothing, "Produs neidentificabil", VrcOiData.Text(k_product, "DenumireP"))
    End Function

    Private Shared Function AccountKey(k_data As VrcOiData, k_line As Dictionary(Of String, Object)) As String
        Dim k_product = Product(k_data, k_line)
        Return If(k_product Is Nothing, String.Empty, VrcOiData.Key(k_product, "IdMasina"))
    End Function

    Private Shared Function AccountName(k_data As VrcOiData, k_key As String) As String
        Dim k_row = k_data.Rows("Masini").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdMasina") = k_key)
        Return If(k_row Is Nothing, "Cont neidentificabil", VrcOiData.Text(k_row, "Masina"))
    End Function

    Private Shared Function SortLines(k_data As VrcOiData, k_lines As IEnumerable(Of Dictionary(Of String, Object))) As IEnumerable(Of Dictionary(Of String, Object))
        Return k_lines.OrderBy(Function(k_r) ProductName(Product(k_data, k_r)), Names).ThenBy(Function(k_r) VrcConvert.ToDecimal(VrcOiData.Value(k_r, "PUTVA")))
    End Function

    Private Shared Function SameFile(k_data As VrcOiData, k_path As String) As Boolean
        Return String.Equals(k_data.Source.Source.Path, k_path, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function DayOf(k_row As Dictionary(Of String, Object), k_field As String) As Date
        Dim k_value = VrcOiData.Value(k_row, k_field)
        Return If(TypeOf k_value Is Date, CDate(k_value), Date.MaxValue)
    End Function

    Private Shared Function NumericLabel(k_text As String) As Decimal
        Dim k_match = System.Text.RegularExpressions.Regex.Match(k_text, "-?[0-9]+(?:[.,][0-9]+)?")
        Dim k_number As Decimal
        If k_match.Success AndAlso Decimal.TryParse(k_match.Value.Replace(",", "."), NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, k_number) Then Return k_number
        Return Decimal.MaxValue
    End Function

    Private Shared Sub StartTable(k_body As StringBuilder, ParamArray k_headers As String())
        k_body.AppendLine("<table><thead><tr>")
        For Each k_header In k_headers
            k_body.Append("<th>").Append(H(k_header)).AppendLine("</th>")
        Next
        k_body.AppendLine("</tr></thead><tbody>")
    End Sub

    Private Shared Sub EndTable(k_body As StringBuilder)
        k_body.AppendLine("</tbody></table>")
    End Sub

    Private Shared Sub Row(k_body As StringBuilder, ParamArray k_values As Object())
        k_body.Append("<tr>")
        For Each k_value In k_values
            k_body.Append("<td>").Append(H(F(k_value)).Replace(vbCrLf, "<br>").Replace(vbLf, "<br>")).Append("</td>")
        Next
        k_body.AppendLine("</tr>")
    End Sub

    Private Shared Sub P(k_body As StringBuilder, k_text As String)
        k_body.Append("<p>").Append(H(k_text)).AppendLine("</p>")
    End Sub

    Private Shared Function F(k_value As Object) As String
        If k_value Is Nothing OrElse Convert.IsDBNull(k_value) Then Return "—"
        If TypeOf k_value Is Date Then Return CDate(k_value).ToString("dd.MM.yyyy", Ro)
        If TypeOf k_value Is Decimal Then Return CDec(k_value).ToString("0.####", Ro)
        Return Convert.ToString(k_value, Ro)
    End Function

    Private Shared Function H(k_text As String) As String
        Return WebUtility.HtmlEncode(k_text)
    End Function

    Private Shared Function Page(k_title As String, k_body As String) As String
        Dim k_css As String
        Using k_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRC.Migrator.VrcOiHtmlReport.css")
            If k_stream Is Nothing Then Throw New InvalidOperationException("Stilul raportului de separare lipsește din executabil.")
            Using k_reader As New StreamReader(k_stream, Encoding.UTF8)
                k_css = k_reader.ReadToEnd()
            End Using
        End Using
        Return "<!doctype html><html lang='ro'><head><meta charset='utf-8'><meta http-equiv='X-UA-Compatible' content='IE=edge'><title>" & H(k_title) & "</title><style>" & k_css & "</style></head><body><h1>" & H(k_title) & "</h1><p class='meta'>VRC.Migrator · " & Date.Now.ToString("dd.MM.yyyy HH:mm:ss", Ro) & " · SLICE-VC10-01</p>" & k_body & "</body></html>"
    End Function
End Class
