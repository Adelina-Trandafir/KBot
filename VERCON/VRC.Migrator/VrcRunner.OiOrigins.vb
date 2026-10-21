Imports System.Globalization

''' <summary>Recover dangling origins only from a unique compatible receipt in the same source snapshot.</summary>
Partial Friend NotInheritable Class VrcRunner

    Private ReadOnly _oiOriginRecovery As New Dictionary(Of Integer, OiOriginRecovery)()

    Private NotInheritable Class OiOriginRecovery
        Private ReadOnly _rows As List(Of Dictionary(Of String, Object))
        Private ReadOnly _positions As New Dictionary(Of String, Dictionary(Of String, Object))(StringComparer.Ordinal)
        Private ReadOnly _documents As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        Private ReadOnly _labels As New Dictionary(Of String, HashSet(Of String))(StringComparer.Ordinal)
        Private ReadOnly _receipts As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        Private ReadOnly _errors As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Private ReadOnly _replacements As New Dictionary(Of String, String)(StringComparer.Ordinal)

        Public Sub New(k_rows As List(Of Dictionary(Of String, Object)), k_bp As List(Of Dictionary(Of String, Object)),
                       k_bt As List(Of Dictionary(Of String, Object)), k_bs As List(Of Dictionary(Of String, Object)))
            ' Clone before correcting links. Neither the Access file nor the original dictionaries are changed.
            _rows = k_rows.Select(Function(k_r) New Dictionary(Of String, Object)(k_r, StringComparer.OrdinalIgnoreCase)).ToList()
            AddDocuments("IDBP", k_bp)
            AddDocuments("IDBT", k_bt)
            AddDocuments("IDBC", k_bs)
            FillReceiptProducts(k_bp)
            Dim k_groups As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
            For Each k_row In _rows
                Dim k_position = VrcConvert.KeyText(k_row("IDOI")), k_family = Family(k_row)
                _positions(k_position) = k_row
                If k_family.Length = 0 Then Continue For ' contradictory markers are reported by ConvertOiDocument
                If Quantity(k_row) > 0D AndAlso k_family <> "IDBC" Then
                    AddRow(_receipts, DocumentKey(k_family, OiLabel(k_row(k_family)), k_row) & "|" & HolderAccount(k_row), k_row)
                End If
                Dim k_origin = OiLabel(k_row("IDBO"))
                If k_family <> "IDBP" AndAlso k_origin.Length > 0 AndAlso
                   Not _labels("IDBP").Contains(k_origin) AndAlso Not _labels("IDBT").Contains(k_origin) Then
                    Dim k_key = DocumentKey(k_family, OiLabel(k_row(k_family)), k_row) & "|" &
                                OiLabel(k_row("IDM")) & "|" & k_origin
                    AddRow(k_groups, k_key, k_row)
                End If
            Next
            For Each k_group In k_groups.OrderBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
                Try
                    Recover(k_group.Value)
                Catch ex As InvalidOperationException
                    For Each k_row In k_group.Value
                        _errors(VrcConvert.KeyText(k_row("IDOI"))) = ex.Message
                    Next
                End Try
            Next
        End Sub

        ''' <summary>Register position (IDOI) -> product taken from OI_Predare, for receipt rows the old program saved without one.</summary>
        Private ReadOnly _filled As New Dictionary(Of String, String)(StringComparer.Ordinal)
        ''' <summary>Receipt rows that stay without a product: no single product on the bon fits their quantity and price.</summary>
        Private ReadOnly _unfilled As New HashSet(Of String)(StringComparer.Ordinal)

        Private Shared Function GroupKey(k_bon As String, k_quantity As Decimal, k_price As Decimal?) As String
            Return k_bon & "|" & k_quantity.ToString("G29", CultureInfo.InvariantCulture) & "|" &
                   If(k_price.HasValue, k_price.Value.ToString("G29", CultureInfo.InvariantCulture), "NULL")
        End Function

        ''' <summary>
        ''' A bon de predare receipt row of the register without IdProdus gets the product of the bon's own OI_Predare line with the same
        ''' quantity and price, but only when that line is unambiguous: as many unused lines as empty rows, all of the same product.
        ''' Anything less certain stays empty (VERCON's «Reglare registru OI» window settles it with the operator). Access is not changed.
        ''' </summary>
        Private Sub FillReceiptProducts(k_bp As List(Of Dictionary(Of String, Object)))
            Dim k_taken As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim k_open As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
            For Each k_row In _rows
                Dim k_bon = OiLabel(k_row("IDBP"))
                If k_bon.Length = 0 OrElse OiLabel(k_row("IDBT")).Length > 0 OrElse Quantity(k_row) <= 0D Then Continue For
                Dim k_group = GroupKey(k_bon, Quantity(k_row), VrcConvert.ToDecimal(k_row("PUTVA")))
                If OiLabel(k_row("IdProdus")).Length > 0 Then
                    Dim k_key = k_group & "|" & VrcConvert.KeyText(k_row("IdProdus"))
                    Dim k_count As Integer = 0
                    k_taken.TryGetValue(k_key, k_count)
                    k_taken(k_key) = k_count + 1
                Else
                    AddRow(k_open, k_group, k_row)
                End If
            Next
            If k_open.Count = 0 Then Return
            Dim k_free As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
            For Each k_line In k_bp
                Dim k_annulled As Object = Nothing
                If k_line.TryGetValue("Anulat", k_annulled) AndAlso VrcConvert.ToDecimal(k_annulled).GetValueOrDefault() <> 0D Then Continue For
                Dim k_bon = OiLabel(k_line("IDBP")), k_product = OiLabel(k_line("IdProdus"))
                Dim k_quantity = VrcConvert.ToDecimal(k_line("Cant")).GetValueOrDefault()
                If k_bon.Length = 0 OrElse k_product.Length = 0 OrElse k_quantity <= 0D Then Continue For
                Dim k_group = GroupKey(k_bon, k_quantity, VrcConvert.ToDecimal(k_line("PUTVA")))
                Dim k_left As Integer = 0
                If k_taken.TryGetValue(k_group & "|" & k_product, k_left) AndAlso k_left > 0 Then
                    k_taken(k_group & "|" & k_product) = k_left - 1
                    Continue For
                End If
                AddRow(k_free, k_group, k_line)
            Next
            For Each k_pair In k_open
                Dim k_candidates As List(Of Dictionary(Of String, Object)) = Nothing
                k_free.TryGetValue(k_pair.Key, k_candidates)
                Dim k_products As List(Of String) = If(k_candidates Is Nothing, New List(Of String)(),
                    k_candidates.Select(Function(k_l) OiLabel(k_l("IdProdus"))).Distinct(StringComparer.Ordinal).ToList())
                For Each k_row In k_pair.Value
                    Dim k_position = VrcConvert.KeyText(k_row("IDOI"))
                    If k_candidates IsNot Nothing AndAlso k_candidates.Count = k_pair.Value.Count AndAlso k_products.Count = 1 Then
                        k_row("IdProdus") = k_candidates(0)("IdProdus")
                        _filled(k_position) = k_products(0)
                    Else
                        _unfilled.Add(k_position)
                    End If
                Next
            Next
        End Sub

        ''' <summary>The product filled in for this position from OI_Predare, or empty when none was.</summary>
        Public Function FilledProduct(k_position As String) As String
            Dim k_product As String = Nothing
            Return If(_filled.TryGetValue(k_position, k_product), k_product, String.Empty)
        End Function

        Public Function StaysWithoutProduct(k_position As String) As Boolean
            Return _unfilled.Contains(k_position)
        End Function

        Private Shared Sub AddRow(k_index As Dictionary(Of String, List(Of Dictionary(Of String, Object))),
                                  k_key As String, k_row As Dictionary(Of String, Object))
            If Not k_index.ContainsKey(k_key) Then k_index(k_key) = New List(Of Dictionary(Of String, Object))()
            k_index(k_key).Add(k_row)
        End Sub

        Private Sub AddDocuments(k_family As String, k_rows As List(Of Dictionary(Of String, Object)))
            _labels(k_family) = New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_row In k_rows
                Dim k_label = OiLabel(k_row(k_family))
                If k_label.Length = 0 Then Continue For
                ' Even an annulled document makes this an existing reference, not a deleted/recreated one.
                _labels(k_family).Add(k_label)
                Dim k_annulled As Object = Nothing
                If k_row.TryGetValue("Anulat", k_annulled) AndAlso VrcConvert.ToDecimal(k_annulled).GetValueOrDefault() <> 0D Then Continue For
                AddRow(_documents, DocumentKey(k_family, k_label, k_row), k_row)
            Next
        End Sub

        Private Shared Function Family(k_row As Dictionary(Of String, Object)) As String
            Dim k_bp = OiLabel(k_row("IDBP")), k_bt = OiLabel(k_row("IDBT"))
            If k_bp.Length > 0 AndAlso k_bt.Length > 0 Then Return String.Empty
            Return If(k_bp.Length > 0, "IDBP", If(k_bt.Length > 0, "IDBT", "IDBC"))
        End Function

        ''' <summary>«bonul de predare», «bonul de transfer» or «bonul de casare», for the operator-visible messages.</summary>
        Private Shared Function DocName(k_family As String) As String
            Return If(k_family = "IDBP", "bonul de predare", If(k_family = "IDBT", "bonul de transfer", "bonul de casare"))
        End Function

        Private Shared Function Quantity(k_row As Dictionary(Of String, Object)) As Decimal
            Return VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault()
        End Function

        Private Shared Function ProductPrice(k_row As Dictionary(Of String, Object)) As String
            Dim k_price = VrcConvert.ToDecimal(k_row("PUTVA"))
            Return VrcConvert.KeyText(k_row("IdProdus")) & "|" &
                   If(k_price.HasValue, k_price.GetValueOrDefault().ToString("G29", CultureInfo.InvariantCulture), "NULL")
        End Function

        Private Shared Function HolderAccount(k_row As Dictionary(Of String, Object)) As String
            Return OiLabel(k_row("IDC_PTC")) & "|" & OiLabel(k_row("IDM"))
        End Function

        Private Shared Function DocumentKey(k_family As String, k_label As String, k_row As Dictionary(Of String, Object)) As String
            Return k_family & "|" & k_label & "|" & ProductPrice(k_row)
        End Function

        Private Function Context(k_row As Dictionary(Of String, Object)) As (Day As Date, Giver As String, Receiver As String)
            Dim k_family = Family(k_row), k_label = OiLabel(k_row(k_family))
            Dim k_lines As List(Of Dictionary(Of String, Object)) = Nothing
            If Not _documents.TryGetValue(DocumentKey(k_family, k_label, k_row), k_lines) Then
                Throw New InvalidOperationException($"Pe {DocName(k_family)} nr. {k_label} nu există nicio linie activă (neanulată) cu același produs și același preț.")
            End If
            Dim k_dayField = If(k_family = "IDBP", "DATABP", If(k_family = "IDBT", "DATABT", "DATABC"))
            Dim k_giverField = If(k_family = "IDBP", "IDC_BP", If(k_family = "IDBT", "IDC_OLD", "IDC_BC"))
            If k_lines.Any(Function(k_l) k_l(k_dayField) Is Nothing OrElse TypeOf k_l(k_dayField) Is DBNull) Then
                Throw New InvalidOperationException($"{DocName(k_family)} nr. {k_label} are linii fără dată completată.")
            End If
            Dim k_days = k_lines.Select(Function(k_l) Convert.ToDateTime(k_l(k_dayField), CultureInfo.InvariantCulture).Date).Distinct().ToList()
            Dim k_givers = k_lines.Select(Function(k_l) OiLabel(k_l(k_giverField))).Distinct(StringComparer.Ordinal).ToList()
            Dim k_receivers = If(k_family = "IDBT", k_lines.Select(Function(k_l) OiLabel(k_l("IDC_BT"))).Distinct(StringComparer.Ordinal).ToList(), k_givers)
            If k_days.Count <> 1 OrElse k_givers.Count <> 1 OrElse k_receivers.Count <> 1 OrElse
               k_givers(0).Length = 0 OrElse k_receivers(0).Length = 0 Then
                Throw New InvalidOperationException($"{DocName(k_family)} nr. {k_label} are linii cu date sau persoane diferite (sau lipsă); pe toate liniile bonului trebuie să fie la fel.")
            End If
            Return (k_days(0), k_givers(0), k_receivers(0))
        End Function

        Private Sub Recover(k_group As List(Of Dictionary(Of String, Object)))
            Dim k_first = k_group(0), k_family = Family(k_first), k_origin = OiLabel(k_first("IDBO"))
            Dim k_context = Context(k_first), k_account = OiLabel(k_first("IDM")), k_productPrice = ProductPrice(k_first)
            Dim k_reason = $"Bonul de origine nr. {k_origin} nu mai există în Access (nici ca bon de predare, nici ca bon de transfer). Migratorul a încercat să găsească singur primirea corectă, dar "
            If k_account.Length = 0 OrElse Not VrcConvert.ToDecimal(k_first("PUTVA")).HasValue Then
                Throw New InvalidOperationException(k_reason & "liniile nu au contul sau prețul completat.")
            End If
            Dim k_taken = k_group.Sum(Function(k_r) Math.Max(0D, -Quantity(k_r)))
            Dim k_received = k_group.Sum(Function(k_r) Math.Max(0D, Quantity(k_r)))
            If k_taken <= 0D OrElse (k_family = "IDBT" AndAlso k_taken <> k_received) OrElse
               (k_family = "IDBC" AndAlso k_received <> 0D) OrElse
               k_group.Any(Function(k_r) OiLabel(k_r("IDC_PTC")) <> If(Quantity(k_r) > 0D, k_context.Receiver, k_context.Giver)) Then
                Throw New InvalidOperationException(k_reason & "liniile acestui transfer sau acestei casări nu arată clar cine a predat obiectul și ce cantitate.")
            End If
            Dim k_holderAccount = k_context.Giver & "|" & k_account
            Dim k_candidates As New List(Of (Family As String, Label As String))()
            For Each k_receipt In _receipts.Values
                Dim k_row = k_receipt(0), k_sourceFamily = Family(k_row), k_label = OiLabel(k_row(k_sourceFamily))
                If ProductPrice(k_row) <> k_productPrice OrElse HolderAccount(k_row) <> k_holderAccount Then Continue For
                If k_sourceFamily = k_family AndAlso k_label = OiLabel(k_first(k_family)) Then Continue For
                If _labels("IDBP").Contains(k_label) AndAlso _labels("IDBT").Contains(k_label) Then Continue For
                Dim k_sourceContext As (Day As Date, Giver As String, Receiver As String)
                Try
                    k_sourceContext = Context(k_row)
                Catch ex As InvalidOperationException
                    Continue For ' an unverifiable receipt cannot establish a replacement
                End Try
                If k_sourceContext.Day > k_context.Day OrElse k_sourceContext.Receiver <> k_context.Giver Then Continue For
                Dim k_balance = k_receipt.Sum(Function(k_r) Quantity(k_r))
                For Each k_withdrawal In _rows
                    If Quantity(k_withdrawal) < 0D AndAlso OiLabel(k_withdrawal("IDBO")) = k_label AndAlso
                       ProductPrice(k_withdrawal) = k_productPrice AndAlso HolderAccount(k_withdrawal) = k_holderAccount Then
                        k_balance += Quantity(k_withdrawal)
                    End If
                Next
                ' All existing withdrawals, including later ones, reserve their quantities first.
                If k_balance >= k_taken Then k_candidates.Add((k_sourceFamily, k_label))
            Next
            If k_candidates.Count <> 1 Then
                Throw New InvalidOperationException(k_reason & If(k_candidates.Count = 0,
                    "nu a găsit nicio primire potrivită (același produs, același preț, aceeași persoană și același cont, dată anterioară și cantitate rămasă suficientă).",
                    $"a găsit {k_candidates.Count} primiri la fel de potrivite și nu alege la întâmplare."))
            End If
            For Each k_row In k_group
                Dim k_position = VrcConvert.KeyText(k_row("IDOI"))
                k_row("IDBO") = k_candidates(0).Label
                _replacements(k_position) = k_candidates(0).Family
            Next
        End Sub

        Public ReadOnly Property Rows As List(Of Dictionary(Of String, Object))
            Get
                Return _rows
            End Get
        End Property

        Public Function RegistryRow(k_position As String) As Dictionary(Of String, Object)
            Dim k_error As String = Nothing
            If _errors.TryGetValue(k_position, k_error) Then Throw New InvalidOperationException(k_error)
            Return _positions(k_position)
        End Function

        Public Function ReplacementFamily(k_position As String) As String
            Dim k_family As String = Nothing
            Return If(_replacements.TryGetValue(k_position, k_family), k_family, String.Empty)
        End Function
    End Class

    Private Function OriginRecovery(k_source As VrcPlanSource, k_sourceIndex As Integer) As OiOriginRecovery
        Dim k_resolver As OiOriginRecovery = Nothing
        If Not _oiOriginRecovery.TryGetValue(k_sourceIndex, k_resolver) Then
            k_resolver = New OiOriginRecovery(ReadRows(k_source, "OI_Produse"), ReadRows(k_source, "OI_Predare"),
                                             ReadRows(k_source, "OI_Transfer"), ReadRows(k_source, "OI_Casare"))
            _oiOriginRecovery(k_sourceIndex) = k_resolver
        End If
        Return k_resolver
    End Function

    Private Function PrepareOiRegistryRow(k_source As VrcPlanSource, k_sourceIndex As Integer,
                                          k_original As Dictionary(Of String, Object), k_key As String,
                                          ByRef k_ok As Boolean) As Dictionary(Of String, Object)
        Try
            Dim k_resolver = OriginRecovery(k_source, k_sourceIndex), k_row = k_resolver.RegistryRow(k_key)
            Dim k_family = k_resolver.ReplacementFamily(k_key)
            Dim k_filledProduct = k_resolver.FilledProduct(k_key)
            If k_filledProduct.Length > 0 Then
                AddIssue(k_source, "OI_Produse", k_key, "IdProdus", "N16", "CONVERSIE", "NULL", k_filledProduct,
                         "Poziția de primire din registru nu avea produs. Produsul a fost luat din linia bonului de predare cu aceeași cantitate și același preț (o singură variantă posibilă). Fișierul Access nu se modifică.")
            ElseIf k_resolver.StaysWithoutProduct(k_key) Then
                AddIssue(k_source, "OI_Produse", k_key, "IdProdus", "N17", "AVERTIZARE", "NULL", "NULL",
                         "Poziția de primire din registru nu are produs și bonul nu arată o singură variantă sigură. Se transferă fără produs; o reglați din VERCON, butonul «Reglare registru OI» din paginile bonurilor.")
            End If
            If k_family.Length > 0 Then
                AddIssue(k_source, "OI_Produse", k_key, "IDBO", "N14", "CONVERSIE", OiLabel(k_original("IDBO")),
                         k_family & "=" & OiLabel(k_row("IDBO")) & " (sursă)",
                         "Bonul de origine nu mai exista în Access. S-a găsit o singură primire potrivită (același produs, preț, persoană și cont, dată anterioară și cantitate suficientă) și linia a fost legată de ea, numai în transfer. Fișierul Access nu se modifică.")
            End If
            Return k_row
        Catch ex As InvalidOperationException
            AddIssue(k_source, "OI_Produse", k_key, "IDBO", "X17", "BLOCAJ", OiLabel(k_original("IDBO")), String.Empty, ex.Message)
            k_ok = False
            Return k_original
        End Try
    End Function

End Class
