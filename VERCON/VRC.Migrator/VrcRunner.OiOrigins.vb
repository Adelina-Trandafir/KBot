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
                Throw New InvalidOperationException($"Bonul {k_family}={k_label} nu are linii active pentru produs/preț identice.")
            End If
            Dim k_dayField = If(k_family = "IDBP", "DATABP", If(k_family = "IDBT", "DATABT", "DATABC"))
            Dim k_giverField = If(k_family = "IDBP", "IDC_BP", If(k_family = "IDBT", "IDC_OLD", "IDC_BC"))
            If k_lines.Any(Function(k_l) k_l(k_dayField) Is Nothing OrElse TypeOf k_l(k_dayField) Is DBNull) Then
                Throw New InvalidOperationException($"Bonul {k_family}={k_label} are data lipsă.")
            End If
            Dim k_days = k_lines.Select(Function(k_l) Convert.ToDateTime(k_l(k_dayField), CultureInfo.InvariantCulture).Date).Distinct().ToList()
            Dim k_givers = k_lines.Select(Function(k_l) OiLabel(k_l(k_giverField))).Distinct(StringComparer.Ordinal).ToList()
            Dim k_receivers = If(k_family = "IDBT", k_lines.Select(Function(k_l) OiLabel(k_l("IDC_BT"))).Distinct(StringComparer.Ordinal).ToList(), k_givers)
            If k_days.Count <> 1 OrElse k_givers.Count <> 1 OrElse k_receivers.Count <> 1 OrElse
               k_givers(0).Length = 0 OrElse k_receivers(0).Length = 0 Then
                Throw New InvalidOperationException($"Bonul {k_family}={k_label} are date/deținători lipsă sau neuniformi.")
            End If
            Return (k_days(0), k_givers(0), k_receivers(0))
        End Function

        Private Sub Recover(k_group As List(Of Dictionary(Of String, Object)))
            Dim k_first = k_group(0), k_family = Family(k_first), k_origin = OiLabel(k_first("IDBO"))
            Dim k_context = Context(k_first), k_account = OiLabel(k_first("IDM")), k_productPrice = ProductPrice(k_first)
            Dim k_reason = $"IDBO={k_origin}: bonul sursă lipsește; "
            If k_account.Length = 0 OrElse Not VrcConvert.ToDecimal(k_first("PUTVA")).HasValue Then
                Throw New InvalidOperationException(k_reason & "contul sau prețul nu permite recuperarea provenienței.")
            End If
            Dim k_taken = k_group.Sum(Function(k_r) Math.Max(0D, -Quantity(k_r)))
            Dim k_received = k_group.Sum(Function(k_r) Math.Max(0D, Quantity(k_r)))
            If k_taken <= 0D OrElse (k_family = "IDBT" AndAlso k_taken <> k_received) OrElse
               (k_family = "IDBC" AndAlso k_received <> 0D) OrElse
               k_group.Any(Function(k_r) OiLabel(k_r("IDC_PTC")) <> If(Quantity(k_r) > 0D, k_context.Receiver, k_context.Giver)) Then
                Throw New InvalidOperationException(k_reason & "laturile transferului/casării nu identifică univoc deținătorul și cantitatea.")
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
                    "nu există o primire BP/BT compatibilă cu produsul, prețul, deținătorul, contul, data și disponibilul.",
                    $"există {k_candidates.Count} primiri BP/BT compatibile; proveniența nu se alege arbitrar."))
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
            If k_family.Length > 0 Then
                AddIssue(k_source, "OI_Produse", k_key, "IDBO", "N14", "CONVERSIE", OiLabel(k_original("IDBO")),
                         k_family & "=" & OiLabel(k_row("IDBO")) & " (sursă)",
                         "Bonul de proveniență dispărut a fost asociat univoc cu o primire existentă: același produs, preț, deținător, cont, dată document compatibilă și cantitate disponibilă. Referința recuperată se remapează cu documentul; Access rămâne nemodificat.")
            End If
            Return k_row
        Catch ex As InvalidOperationException
            AddIssue(k_source, "OI_Produse", k_key, "IDBO", "X17", "BLOCAJ", OiLabel(k_original("IDBO")), String.Empty, ex.Message)
            k_ok = False
            Return k_original
        End Try
    End Function

End Class
