Imports System.Globalization

''' <summary>Use accounts follow the initial BP, independently of erroneous legacy TC accounts.</summary>
Partial Friend NotInheritable Class VrcRunner

    Private ReadOnly _oiAccounts As New Dictionary(Of Integer, OiAccounts)()

    Private NotInheritable Class OiAccounts
        Private ReadOnly _receipts As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        Private ReadOnly _documents As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        Private ReadOnly _cache As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Private ReadOnly _active As New HashSet(Of String)(StringComparer.Ordinal)

        ''' <summary>
        ''' Receipts of a bon de predare come from OI_Predare itself (bon + product, the way the Access program reads them): the receipt rows
        ''' kept in OI_Produse are often missing IdProdus. Receipts of a bon de transfer still come from OI_Produse.
        ''' </summary>
        Public Sub New(k_rows As List(Of Dictionary(Of String, Object)), k_predare As List(Of Dictionary(Of String, Object)))
            For Each k_row In k_rows
                Dim k_q = VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault()
                Dim k_family = Family(k_row)
                If (k_family = "IDBT" AndAlso k_q >= 0D) OrElse (k_family = "IDBC" AndAlso k_q <= 0D) Then
                    Dim k_docKey = DocumentKey(k_family, k_row)
                    If Not _documents.ContainsKey(k_docKey) Then _documents(k_docKey) = New List(Of Dictionary(Of String, Object))()
                    _documents(k_docKey).Add(k_row)
                End If
                If k_q <= 0D Then Continue For
                Dim k_document = OiLabel(k_row("IDBT"))
                If k_document.Length = 0 Then Continue For
                AddReceipt(OriginKey(k_document, k_row), k_row)
            Next
            Dim k_number As Integer = 0
            For Each k_line In k_predare
                Dim k_bon = OiLabel(k_line("IDBP"))
                If k_bon.Length = 0 OrElse VrcConvert.ToDecimal(k_line("Cant")).GetValueOrDefault() <= 0D Then Continue For
                k_number += 1
                ' Same shape as an OI_Produse row, so the account lookup treats both alike; a negative IDOI cannot clash with a real position.
                Dim k_receipt As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                    {"IDOI", -k_number}, {"IDBP", k_line("IDBP")}, {"IDBT", 0}, {"IDBC", 0}, {"IDBO", 0},
                    {"IDM", k_line("IdMasina")}, {"IdProdus", k_line("IdProdus")}, {"Cant", k_line("Cant")}, {"PUTVA", k_line("PUTVA")}}
                AddReceipt(OriginKey(k_bon, k_receipt), k_receipt)
            Next
        End Sub

        Private Sub AddReceipt(k_key As String, k_row As Dictionary(Of String, Object))
            If Not _receipts.ContainsKey(k_key) Then _receipts(k_key) = New List(Of Dictionary(Of String, Object))()
            _receipts(k_key).Add(k_row)
        End Sub

        Private Shared Function Family(k_row As Dictionary(Of String, Object)) As String
            Dim k_bp = OiLabel(k_row("IDBP")), k_bt = OiLabel(k_row("IDBT"))
            If k_bp.Length > 0 AndAlso k_bt.Length > 0 Then Throw New InvalidOperationException("Poziția are completate și numărul bonului de predare, și numărul bonului de transfer; trebuie să fie doar unul dintre ele.")
            Return If(k_bp.Length > 0, "IDBP", If(k_bt.Length > 0, "IDBT", "IDBC"))
        End Function

        ''' <summary>«bonul de predare», «bonul de transfer» or «bonul de casare», for the operator-visible messages.</summary>
        Private Shared Function DocName(k_family As String) As String
            Return If(k_family = "IDBP", "bonul de predare", If(k_family = "IDBT", "bonul de transfer", "bonul de casare"))
        End Function

        Private Shared Function Price(k_row As Dictionary(Of String, Object)) As String
            Dim k_price = VrcConvert.ToDecimal(k_row("PUTVA"))
            Return If(k_price.HasValue, k_price.GetValueOrDefault().ToString("G29", CultureInfo.InvariantCulture), "NULL")
        End Function

        Private Shared Function ReceiptKey(k_document As String, k_row As Dictionary(Of String, Object)) As String
            Return k_document & "|" & VrcConvert.KeyText(k_row("IdProdus")) & "|" & Price(k_row)
        End Function

        ''' <summary>Bon + product, without the price: the link the Access program uses between a withdrawal and its receipt.</summary>
        Private Shared Function OriginKey(k_document As String, k_row As Dictionary(Of String, Object)) As String
            Return k_document & "|" & VrcConvert.KeyText(k_row("IdProdus"))
        End Function

        Private Shared Function DocumentKey(k_family As String, k_row As Dictionary(Of String, Object)) As String
            Dim k_quantity = Math.Abs(VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault()).ToString("G29", CultureInfo.InvariantCulture)
            Return k_family & "|" & ReceiptKey(OiLabel(k_row(k_family)), k_row) & "|" & k_quantity
        End Function

        Public Function Account(k_row As Dictionary(Of String, Object)) As String
            Dim k_position = VrcConvert.KeyText(k_row("IDOI"))
            If k_position.Length = 0 Then Throw New InvalidOperationException("O poziție din OI_Produse nu are număr (coloana IDOI goală).")
            Dim k_account As String = Nothing
            If _cache.TryGetValue(k_position, k_account) Then Return k_account
            If Not _active.Add(k_position) Then Throw New InvalidOperationException($"Poziția {k_position}: bonurile de origine se învârt în cerc (un bon trimite la altul, care trimite înapoi la primul), deci nu se ajunge niciodată la primirea inițială.")
            Try
                Dim k_family = Family(k_row), k_origin = OiLabel(k_row("IDBO"))
                If k_family = "IDBP" OrElse (k_origin.Length = 0 AndAlso VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault() = 0D) Then
                    k_account = OiLabel(k_row("IDM"))
                Else
                    Dim k_sources As List(Of Dictionary(Of String, Object)) = Nothing
                    If k_origin.Length = 0 Then
                        Throw New InvalidOperationException($"Poziția {k_position} (produsul {VrcConvert.KeyText(k_row("IdProdus"))}) este o scoatere, dar nu are completat bonul din care a plecat obiectul (câmpul IDBO).")
                    End If
                    If Not _receipts.TryGetValue(OriginKey(k_origin, k_row), k_sources) Then
                        Throw New InvalidOperationException($"Poziția {k_position}: produsul {VrcConvert.KeyText(k_row("IdProdus"))} a plecat din bonul {k_origin}, dar bonul {k_origin} nu are nicio linie cu acest produs (nici în OI_Predare, nici ca transfer primit). Fără ea nu se știe în ce cont era obiectul.")
                    End If
                    If k_sources.Select(Function(k_r) Family(k_r)).Distinct(StringComparer.Ordinal).Count() <> 1 Then
                        Throw New InvalidOperationException($"Poziția {k_position}: numărul {k_origin} există și ca bon de predare, și ca bon de transfer; nu se știe din care a plecat obiectul.")
                    End If
                    Dim k_accounts = k_sources.Select(Function(k_r) Account(k_r)).Distinct(StringComparer.Ordinal).ToList()
                    If k_accounts.Count <> 1 Then
                        Throw New InvalidOperationException($"Poziția {k_position}: bonul {k_origin} a primit același produs în conturi diferite, deci contul nu poate fi ales fără să ghicim.")
                    End If
                    k_account = k_accounts(0)
                End If
                _cache(k_position) = k_account
                Return k_account
            Finally
                _active.Remove(k_position)
            End Try
        End Function

        Public Function DocumentAccount(k_table As String, k_row As Dictionary(Of String, Object)) As String
            Dim k_family = If(k_table = "OI_Transfer", "IDBT", "IDBC")
            Dim k_document = OiLabel(k_row(k_family)), k_product = VrcConvert.KeyText(k_row("IdProdus"))
            Dim k_candidates As List(Of Dictionary(Of String, Object)) = Nothing
            If Not _documents.TryGetValue(DocumentKey(k_family, k_row), k_candidates) Then
                Throw New InvalidOperationException($"{DocName(k_family)} nr. {k_document}, produsul {k_product}: în OI_Produse nu există poziția care corespunde acestei linii (același produs, același preț, aceeași cantitate), deci nu se știe contul.")
            End If
            Dim k_accounts = k_candidates.Select(Function(k_r) Account(k_r)).Distinct(StringComparer.Ordinal).ToList()
            If k_accounts.Count <> 1 Then
                Throw New InvalidOperationException($"{DocName(k_family)} nr. {k_document}, produsul {k_product}: linia se potrivește cu mai multe poziții din conturi diferite, deci nu se știe care este contul ei.")
            End If
            Return k_accounts(0)
        End Function
    End Class

    Private Function ConvertOiAccount(k_gest As GestInfo, k_source As VrcPlanSource, k_sourceIndex As Integer,
                                      k_table As VrcImportTable, k_column As VrcImportColumn,
                                      k_row As Dictionary(Of String, Object), k_key As String, ByRef k_ok As Boolean) As Object
        Dim k_resolver As OiAccounts = Nothing
        Dim k_original = OiLabel(k_row(k_column.Name))
        Dim k_account As String
        Try
            If Not _oiAccounts.TryGetValue(k_sourceIndex, k_resolver) Then
                k_resolver = New OiAccounts(OriginRecovery(k_source, k_sourceIndex).Rows, ReadRows(k_source, "OI_Predare"))
                _oiAccounts(k_sourceIndex) = k_resolver
            End If
            k_account = If(k_table.Name = "OI_Produse", k_resolver.Account(k_row), k_resolver.DocumentAccount(k_table.Name, k_row))
        Catch ex As InvalidOperationException
            ' On OI_Produse the useful value to show is the bon the object left from, not the account that could not be confirmed.
            If k_table.Name = "OI_Produse" Then
                AddIssue(k_source, k_table.Name, k_key, "IDBO", "X16", "BLOCAJ", OiLabel(k_row("IDBO")), String.Empty, ex.Message)
            Else
                AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X16", "BLOCAJ", k_original, String.Empty, ex.Message)
            End If
            k_ok = False
            Return Nothing
        End Try
        Dim k_converted = ConvertColumn(k_gest, k_source, k_sourceIndex, k_table, k_column, k_account, k_key, k_ok)
        If k_ok AndAlso k_account <> k_original Then
            AddIssue(k_source, k_table.Name, k_key, k_column.Name, "N13", "CONVERSIE", k_original,
                     If(k_converted Is Nothing, "NULL", Convert.ToString(k_converted, CultureInfo.InvariantCulture)),
                     $"Contul obiectului se ia de la primirea inițială (bonul de predare), urmărind bonurile de origine până acolo: contul {k_account}, nu cel scris pe această linie ({k_original}).")
        End If
        Return k_converted
    End Function

End Class
