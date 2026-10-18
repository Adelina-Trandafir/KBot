Imports System.Globalization

''' <summary>Use accounts follow the initial BP, independently of erroneous legacy TC accounts.</summary>
Partial Friend NotInheritable Class VrcRunner

    Private ReadOnly _oiAccounts As New Dictionary(Of Integer, OiAccounts)()

    Private NotInheritable Class OiAccounts
        Private ReadOnly _receipts As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        Private ReadOnly _documents As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        Private ReadOnly _cache As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Private ReadOnly _active As New HashSet(Of String)(StringComparer.Ordinal)

        Public Sub New(k_rows As List(Of Dictionary(Of String, Object)))
            For Each k_row In k_rows
                Dim k_q = VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault()
                Dim k_family = Family(k_row)
                If (k_family = "IDBT" AndAlso k_q >= 0D) OrElse (k_family = "IDBC" AndAlso k_q <= 0D) Then
                    Dim k_docKey = DocumentKey(k_family, k_row)
                    If Not _documents.ContainsKey(k_docKey) Then _documents(k_docKey) = New List(Of Dictionary(Of String, Object))()
                    _documents(k_docKey).Add(k_row)
                End If
                If k_q <= 0D Then Continue For
                Dim k_document = If(OiLabel(k_row("IDBP")).Length > 0, OiLabel(k_row("IDBP")), OiLabel(k_row("IDBT")))
                If k_document.Length = 0 Then Continue For
                Dim k_key = ReceiptKey(k_document, k_row)
                If Not _receipts.ContainsKey(k_key) Then _receipts(k_key) = New List(Of Dictionary(Of String, Object))()
                _receipts(k_key).Add(k_row)
            Next
        End Sub

        Private Shared Function Family(k_row As Dictionary(Of String, Object)) As String
            Dim k_bp = OiLabel(k_row("IDBP")), k_bt = OiLabel(k_row("IDBT"))
            If k_bp.Length > 0 AndAlso k_bt.Length > 0 Then Throw New InvalidOperationException("Marcaje BP și BT simultane în registru.")
            Return If(k_bp.Length > 0, "IDBP", If(k_bt.Length > 0, "IDBT", "IDBC"))
        End Function

        Private Shared Function Price(k_row As Dictionary(Of String, Object)) As String
            Dim k_price = VrcConvert.ToDecimal(k_row("PUTVA"))
            Return If(k_price.HasValue, k_price.GetValueOrDefault().ToString("G29", CultureInfo.InvariantCulture), "NULL")
        End Function

        Private Shared Function ReceiptKey(k_document As String, k_row As Dictionary(Of String, Object)) As String
            Return k_document & "|" & VrcConvert.KeyText(k_row("IdProdus")) & "|" & Price(k_row)
        End Function

        Private Shared Function DocumentKey(k_family As String, k_row As Dictionary(Of String, Object)) As String
            Dim k_quantity = Math.Abs(VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault()).ToString("G29", CultureInfo.InvariantCulture)
            Return k_family & "|" & ReceiptKey(OiLabel(k_row(k_family)), k_row) & "|" & k_quantity
        End Function

        Public Function Account(k_row As Dictionary(Of String, Object)) As String
            Dim k_position = VrcConvert.KeyText(k_row("IDOI"))
            If k_position.Length = 0 Then Throw New InvalidOperationException("Lipsește IDOI din registrul sursă.")
            Dim k_account As String = Nothing
            If _cache.TryGetValue(k_position, k_account) Then Return k_account
            If Not _active.Add(k_position) Then Throw New InvalidOperationException($"Ciclu de proveniență în poziția OI {k_position}.")
            Try
                Dim k_family = Family(k_row), k_origin = OiLabel(k_row("IDBO"))
                If k_family = "IDBP" OrElse (k_origin.Length = 0 AndAlso VrcConvert.ToDecimal(k_row("Cant")).GetValueOrDefault() = 0D) Then
                    k_account = OiLabel(k_row("IDM"))
                Else
                    Dim k_sources As List(Of Dictionary(Of String, Object)) = Nothing
                    If k_origin.Length = 0 OrElse Not _receipts.TryGetValue(ReceiptKey(k_origin, k_row), k_sources) Then
                        Throw New InvalidOperationException($"Poziția OI {k_position}: lipsește primirea BP/BT pentru IDBO={k_origin}, produs/preț identice.")
                    End If
                    If k_sources.Select(Function(k_r) Family(k_r)).Distinct(StringComparer.Ordinal).Count() <> 1 Then
                        Throw New InvalidOperationException($"Poziția OI {k_position}: proveniență ambiguă BP/BT pentru IDBO={k_origin}.")
                    End If
                    Dim k_accounts = k_sources.Select(Function(k_r) Account(k_r)).Distinct(StringComparer.Ordinal).ToList()
                    If k_accounts.Count <> 1 Then
                        Throw New InvalidOperationException($"Poziția OI {k_position}: IDBO={k_origin} conține conturi de predare diferite; nu se alege arbitrar contul.")
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
                Throw New InvalidOperationException($"Bon {k_family}={k_document}, produs {k_product}: lipsește poziția de registru cu același preț și cantitate.")
            End If
            Dim k_accounts = k_candidates.Select(Function(k_r) Account(k_r)).Distinct(StringComparer.Ordinal).ToList()
            If k_accounts.Count <> 1 Then
                Throw New InvalidOperationException($"Bon {k_family}={k_document}, produs {k_product}: mai multe conturi de proveniență; linia nu poate fi asociată univoc.")
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
                k_resolver = New OiAccounts(ReadRows(k_source, "OI_Produse"))
                _oiAccounts(k_sourceIndex) = k_resolver
            End If
            k_account = If(k_table.Name = "OI_Produse", k_resolver.Account(k_row), k_resolver.DocumentAccount(k_table.Name, k_row))
        Catch ex As InvalidOperationException
            AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X16", "BLOCAJ", k_original, String.Empty, ex.Message)
            k_ok = False
            Return Nothing
        End Try
        Dim k_converted = ConvertColumn(k_gest, k_source, k_sourceIndex, k_table, k_column, k_account, k_key, k_ok)
        If k_ok AndAlso k_account <> k_original Then
            AddIssue(k_source, k_table.Name, k_key, k_column.Name, "N13", "CONVERSIE", k_original,
                     If(k_converted Is Nothing, "NULL", Convert.ToString(k_converted, CultureInfo.InvariantCulture)),
                     $"Contul de folosință se păstrează de la predarea inițială până la casare, prin IDBO. Contul sursă corect este {k_account}, apoi se remapează în țintă; contul TC din Access este ignorat.")
        End If
        Return k_converted
    End Function

End Class
