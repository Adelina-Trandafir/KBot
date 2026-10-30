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
        ''' <summary>Write-off positions saved with a positive quantity: position -> (quantity in Access, quantity written, sentence for the report).</summary>
        Private ReadOnly _flipped As New Dictionary(Of String, (Original As Decimal, Fixed As Decimal, Text As String))(StringComparer.Ordinal)
        ''' <summary>Positions whose value had a sign different from their quantity: position -> (value in Access, value written).</summary>
        Private ReadOnly _valueAligned As New Dictionary(Of String, (Original As Decimal, Fixed As Decimal))(StringComparer.Ordinal)
        ''' <summary>Day of each document (family|label), the earliest of its active lines.</summary>
        Private ReadOnly _dates As New Dictionary(Of String, Date)(StringComparer.Ordinal)
        Private _balanceChecked As Boolean

        Public Sub New(k_rows As List(Of Dictionary(Of String, Object)), k_bp As List(Of Dictionary(Of String, Object)),
                       k_bt As List(Of Dictionary(Of String, Object)), k_bs As List(Of Dictionary(Of String, Object)))
            ' Clone before correcting links. Neither the Access file nor the original dictionaries are changed.
            _rows = k_rows.Select(Function(k_r) New Dictionary(Of String, Object)(k_r, StringComparer.OrdinalIgnoreCase)).ToList()
            FlipWrongSignWriteOffs()
            AlignValueSigns()
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
                Dim k_dayValue As Object = Nothing
                Dim k_dayField As String = If(k_family = "IDBP", "DATABP", If(k_family = "IDBT", "DATABT", "DATABC"))
                If k_row.TryGetValue(k_dayField, k_dayValue) AndAlso k_dayValue IsNot Nothing AndAlso Not TypeOf k_dayValue Is DBNull Then
                    Dim k_day As Date = Convert.ToDateTime(k_dayValue, CultureInfo.InvariantCulture).Date
                    Dim k_known As Date
                    If Not _dates.TryGetValue(k_family & "|" & k_label, k_known) OrElse k_day < k_known Then _dates(k_family & "|" & k_label) = k_day
                End If
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
                    $"a găsit {k_candidates.Count} primiri la fel de potrivite și nu alege la întâmplare: {CandidatesText(k_candidates, k_origin)}"))
            End If
            For Each k_row In k_group
                Dim k_position = VrcConvert.KeyText(k_row("IDOI"))
                k_row("IDBO") = k_candidates(0).Label
                _replacements(k_position) = k_candidates(0).Family
            Next
        End Sub

        ''' <summary>The receipts a dangling origin could point to, with their dates, for the operator; hints at a number that looks like the dangling one with zeros added.</summary>
        Private Function CandidatesText(k_candidates As List(Of (Family As String, Label As String)), k_origin As String) As String
            Dim k_parts As New List(Of String)()
            Dim k_hint As String = String.Empty
            For Each k_candidate In k_candidates
                Dim k_day As Date
                Dim k_dayText = If(_dates.TryGetValue(k_candidate.Family & "|" & k_candidate.Label, k_day), " din " & k_day.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_parts.Add($"{DocName(k_candidate.Family)} nr. {k_candidate.Label}{k_dayText}")
                If k_candidate.Label.Length > k_origin.Length AndAlso k_candidate.Label.StartsWith(k_origin, StringComparison.Ordinal) AndAlso
                   k_candidate.Label.Substring(k_origin.Length).All(Function(k_c) k_c = "0"c) Then
                    k_hint = $" Numărul {k_candidate.Label} seamănă cu {k_origin}: poate lipsesc zerourile din IDBO."
                End If
            Next
            Return String.Join("; ", k_parts) & "." & k_hint
        End Function

        ''' <summary>
        ''' A write-off position (bon de casare) saved with a POSITIVE quantity is a wrong sign of the old program: the quantity (and its value)
        ''' is multiplied by -1, in the copy only. Done only when the same document / product / price has no correct (negative) position;
        ''' otherwise the positive ones may be real and are left alone.
        ''' </summary>
        Private Sub FlipWrongSignWriteOffs()
            Dim k_correct As New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_row In _rows
                If Family(k_row) = "IDBC" AndAlso OiLabel(k_row("IDBC")).Length > 0 AndAlso Quantity(k_row) <= 0D Then
                    k_correct.Add(DocumentKey("IDBC", OiLabel(k_row("IDBC")), k_row))
                End If
            Next
            For Each k_row In _rows
                If Family(k_row) <> "IDBC" OrElse Quantity(k_row) <= 0D Then Continue For
                Dim k_label = OiLabel(k_row("IDBC"))
                If k_label.Length = 0 OrElse k_correct.Contains(DocumentKey("IDBC", k_label, k_row)) Then Continue For
                Dim k_original = Quantity(k_row)
                k_row("Cant") = -k_original
                Dim k_valueRaw As Object = Nothing
                If k_row.TryGetValue("ValoareCUTVA", k_valueRaw) Then
                    Dim k_value = VrcConvert.ToDecimal(k_valueRaw)
                    If k_value.HasValue AndAlso k_value.Value > 0D Then k_row("ValoareCUTVA") = -k_value.Value
                End If
                _flipped(VrcConvert.KeyText(k_row("IDOI"))) = (k_original, -k_original,
                    $"Bonul de casare nr. {k_label}, produsul {VrcConvert.KeyText(k_row("IdProdus"))}: cantitatea {k_original.ToString("G29", CultureInfo.InvariantCulture)} a devenit {(-k_original).ToString("G29", CultureInfo.InvariantCulture)}.")
            Next
        End Sub

        ''' <summary>
        ''' The value of a register position follows the sign of its quantity (minus on the side that gives or writes off, plus on the side that
        ''' receives), the way the online application writes it. The old program left a positive value on some giving rows (the giver side of a
        ''' transfer, a write-off): a holder then ends with quantity 0 and a value left over. The value gets the sign of the quantity, in the copy only.
        ''' </summary>
        Private Sub AlignValueSigns()
            For Each k_row In _rows
                Dim k_quantity = Quantity(k_row)
                Dim k_valueRaw As Object = Nothing
                If k_quantity = 0D OrElse Not k_row.TryGetValue("ValoareCUTVA", k_valueRaw) Then Continue For
                Dim k_value = VrcConvert.ToDecimal(k_valueRaw)
                If Not k_value.HasValue OrElse k_value.Value = 0D OrElse (k_value.Value < 0D) = (k_quantity < 0D) Then Continue For
                k_row("ValoareCUTVA") = -k_value.Value
                _valueAligned(VrcConvert.KeyText(k_row("IDOI"))) = (k_value.Value, -k_value.Value)
            Next
        End Sub

        ''' <summary>True when the value of the position had its sign changed; gives the value in Access and the value written.</summary>
        Public Function ValueAligned(k_position As String, ByRef k_original As Decimal, ByRef k_fixed As Decimal) As Boolean
            Dim k_pair As (Original As Decimal, Fixed As Decimal) = Nothing
            If Not _valueAligned.TryGetValue(k_position, k_pair) Then Return False
            k_original = k_pair.Original
            k_fixed = k_pair.Fixed
            Return True
        End Function

        ''' <summary>The sentence for a position whose sign was changed, or empty.</summary>
        Public Function FlippedText(k_position As String) As String
            Dim k_flip As (Original As Decimal, Fixed As Decimal, Text As String) = Nothing
            Return If(_flipped.TryGetValue(k_position, k_flip), k_flip.Text, String.Empty)
        End Function

        Public Function FlippedOriginal(k_position As String) As String
            Dim k_flip As (Original As Decimal, Fixed As Decimal, Text As String) = Nothing
            Return If(_flipped.TryGetValue(k_position, k_flip), k_flip.Original.ToString("G29", CultureInfo.InvariantCulture), String.Empty)
        End Function

        Public Function FlippedFixed(k_position As String) As String
            Dim k_flip As (Original As Decimal, Fixed As Decimal, Text As String) = Nothing
            Return If(_flipped.TryGetValue(k_position, k_flip), k_flip.Fixed.ToString("G29", CultureInfo.InvariantCulture), String.Empty)
        End Function

        ''' <summary>
        ''' After the sign change: every batch (holder, origin bon, product, price, account) that a changed position takes from is followed day by day, the way the
        ''' online application checks its register. A batch that goes below zero is returned with a sentence for the report. Answered once per source.
        ''' </summary>
        Public Function NegativeBalancesAfterFlip() As List(Of (Position As String, Balance As Decimal, Text As String))
            Dim k_result As New List(Of (Position As String, Balance As Decimal, Text As String))()
            If _balanceChecked Then Return k_result
            _balanceChecked = True
            If _flipped.Count = 0 Then Return k_result
            Dim k_events As New Dictionary(Of String, List(Of (EventDay As Date, Order As Integer, Amount As Decimal)))(StringComparer.Ordinal)
            Dim k_touched As New Dictionary(Of String, String)(StringComparer.Ordinal)
            Dim k_order As Integer = 0
            For Each k_row In _rows
                k_order += 1
                Dim k_family = Family(k_row)
                If k_family.Length = 0 Then Continue For
                Dim k_amount = Quantity(k_row)
                If k_amount = 0D Then Continue For
                Dim k_label = OiLabel(k_row(k_family))
                If k_label.Length = 0 Then Continue For
                Dim k_originFamily As String, k_originLabel As String
                If k_amount > 0D Then
                    k_originFamily = k_family
                    k_originLabel = k_label
                Else
                    k_originLabel = OiLabel(k_row("IDBO"))
                    If k_originLabel.Length = 0 Then Continue For
                    Dim k_inBp = _labels("IDBP").Contains(k_originLabel), k_inBt = _labels("IDBT").Contains(k_originLabel)
                    If k_inBp = k_inBt Then Continue For ' unknown or ambiguous origin: reported elsewhere
                    k_originFamily = If(k_inBp, "IDBP", "IDBT")
                End If
                Dim k_day As Date
                _dates.TryGetValue(k_family & "|" & k_label, k_day)
                Dim k_key = OiLabel(k_row("IDC_PTC")) & "|" & k_originFamily & "|" & k_originLabel & "|" & VrcConvert.KeyText(k_row("IdProdus")) & "|" &
                            Price(k_row) & "|" & OiLabel(k_row("IDM"))
                If Not k_events.ContainsKey(k_key) Then k_events(k_key) = New List(Of (EventDay As Date, Order As Integer, Amount As Decimal))()
                k_events(k_key).Add((k_day, k_order, k_amount))
                Dim k_position = VrcConvert.KeyText(k_row("IDOI"))
                If _flipped.ContainsKey(k_position) AndAlso Not k_touched.ContainsKey(k_key) Then k_touched(k_key) = k_position
            Next
            For Each k_pair In k_touched.OrderBy(Function(k_t) k_t.Key, StringComparer.Ordinal)
                Dim k_list = k_events(k_pair.Key)
                Dim k_running As Decimal = 0D, k_lowest As Decimal = 0D
                Dim k_firstNegative As Date? = Nothing
                For Each k_dayGroup In k_list.GroupBy(Function(k_e) k_e.EventDay).OrderBy(Function(k_g) k_g.Key)
                    k_running += k_dayGroup.Sum(Function(k_e) k_e.Amount)
                    If k_running < k_lowest Then k_lowest = k_running
                    If k_running < 0D AndAlso Not k_firstNegative.HasValue Then k_firstNegative = k_dayGroup.Key
                Next
                If k_lowest >= 0D Then Continue For
                Dim k_parts = k_pair.Key.Split("|"c)
                Dim k_received = k_list.Where(Function(k_e) k_e.Amount > 0D).Sum(Function(k_e) k_e.Amount)
                Dim k_taken = -k_list.Where(Function(k_e) k_e.Amount < 0D).Sum(Function(k_e) k_e.Amount)
                Dim k_dayText As String = If(k_firstNegative.HasValue AndAlso k_firstNegative.Value > Date.MinValue, k_firstNegative.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "o dată necunoscută")
                k_result.Add((k_pair.Value, k_running,
                    $"Produsul {k_parts(3)} (preț {k_parts(4)}, cont {k_parts(5)}, deținător {k_parts(0)}), primit pe {DocName(k_parts(1))} nr. {k_parts(2)}: primit {k_received.ToString("G29", CultureInfo.InvariantCulture)}, scos {k_taken.ToString("G29", CultureInfo.InvariantCulture)}, sold {k_running.ToString("G29", CultureInfo.InvariantCulture)}; devine negativ pe {k_dayText}."))
            Next
            Return k_result
        End Function

        Private Shared Function Price(k_row As Dictionary(Of String, Object)) As String
            Dim k_price = VrcConvert.ToDecimal(k_row("PUTVA"))
            Return If(k_price.HasValue, k_price.GetValueOrDefault().ToString("G29", CultureInfo.InvariantCulture), "NULL")
        End Function

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
            Dim k_flipText = k_resolver.FlippedText(k_key)
            If k_flipText.Length > 0 Then
                AddIssue(k_source, "OI_Produse", k_key, "Cant", "N18", "CONVERSIE", k_resolver.FlippedOriginal(k_key), k_resolver.FlippedFixed(k_key), k_flipText)
            End If
            Dim k_valueOriginal As Decimal, k_valueFixed As Decimal
            If k_resolver.ValueAligned(k_key, k_valueOriginal, k_valueFixed) Then
                AddIssue(k_source, "OI_Produse", k_key, "ValoareCUTVA", "N19", "CONVERSIE",
                         k_valueOriginal.ToString("G29", CultureInfo.InvariantCulture), k_valueFixed.ToString("G29", CultureInfo.InvariantCulture),
                         "Valoarea poziției din registru avea alt semn decât cantitatea. A primit semnul cantității (la o ieșire valoarea este negativă, ca și cantitatea). Se schimbă numai în transfer; fișierul Access rămâne neatins.")
            End If
            For Each k_negative In k_resolver.NegativeBalancesAfterFlip()
                AddIssue(k_source, "OI_Produse", k_negative.Position, "Cant", "X18", "AVERTIZARE",
                         k_negative.Balance.ToString("G29", CultureInfo.InvariantCulture), String.Empty,
                         If(k_negative.Text.Length > 480, k_negative.Text.Substring(0, 480), k_negative.Text))
            Next
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
