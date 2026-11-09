Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' SLICE-KE21-02: KeReceptiiView.Commit - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ' What the angajament in use still has (reservations minus receptions) on each classification must hold the sum received (this reception, all the ticked invoices): the
    ' rest may not go below 0. Not for a link to an existing reception (its value does not change). A storno (a negative sum) is not checked.
    Private Function AngajamentProblem() As String
        If _data Is Nothing OrElse _mode = EFacturaKeTip.Existenta OrElse _viewing IsNot Nothing Then Return Nothing
        Dim k_total As Dictionary(Of Integer, Decimal) = ReceptionTotals()
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_total
            If k_pair.Value <= Tolerance Then Continue For
            Dim k_ang As Decimal? = AngDisponibil(k_pair.Key)
            If Not k_ang.HasValue OrElse k_pair.Value <= k_ang.Value + Tolerance Then Continue For
            Dim k_ind As EFacturaKeIndicator = IndicatorOf(k_pair.Key)
            Return $"Valoarea depășește disponibilul angajamentului pe {If(k_ind IsNot Nothing, k_ind.Clsf, k_pair.Key.ToString(CultureInfo.InvariantCulture))}: " &
                   $"disponibil {Fmt(k_ang.Value)}, de recepționat {Fmt(k_pair.Value)}. Scădeți «Recepționată» sau «Valoare»."
        Next
        Return Nothing
    End Function

    ' A number as the grids show it: the operator's own regional format (a dot or a comma for the decimals), the same one the cells are painted and typed in.
    Private Shared Function Fmt(k_value As Decimal) As String
        Return k_value.ToString("N2", CultureInfo.CurrentCulture)
    End Function

    ' A typed amount must be a number in the operator's own regional format (the cells are painted in it, so «77.49» is 77.49 for a dot user, not 7749); empty = 0.
    Private Shared Function ReadNumber(k_value As Object, ByRef k_number As Decimal) As Boolean
        If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then
            k_number = 0D
            Return True
        End If
        If TypeOf k_value Is Double Then
            k_number = Math.Round(CDec(DirectCast(k_value, Double)), 2)
            Return True
        End If
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.CurrentCulture).Trim()
        If k_text.Length = 0 Then
            k_number = 0D
            Return True
        End If
        Dim k_parsed As Decimal
        If Decimal.TryParse(k_text, NumberStyles.Number Or NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, k_parsed) Then
            k_number = Math.Round(k_parsed, 2)
            Return True
        End If
        Return False
    End Function

    ' An existing reception takes only what it still has on each classification: the sum over all the ticked invoices is checked here.
    ' k_fromLineValues = nothing is typed or proposed (every amount was cut to 0 because the reception has no rest on the classification of the lines): the check is then
    ' made with what the lines are worth (less what the invoice already has linked), so the operator is told why «Leagă» stays disabled instead of seeing nothing.
    Private Function ReceptionFitProblem(Optional k_fromLineValues As Boolean = False) As String
        If _mode <> EFacturaKeTip.Existenta Then Return Nothing
        Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
        If k_rec Is Nothing Then Return Nothing
        Dim k_total As New Dictionary(Of Integer, Decimal)()
        For Each k_id As Integer In _checked
            If k_fromLineValues Then
                ' Slice KE20-04: what the invoice would take on the classification of its first row is all that is left of it.
                Dim k_first As KBotDataRow = RowsOf(k_id).FirstOrDefault(Function(k_r) ChosenClass(k_r).HasValue)
                If k_first IsNot Nothing AndAlso Math.Abs(InvoiceRest(k_id)) >= Tolerance Then
                    Dim k_firstCls As Integer = ChosenClass(k_first).Value
                    k_total(k_firstCls) = k_total.GetValueOrDefault(k_firstCls) + InvoiceRest(k_id)
                End If
            Else
                For Each k_pair As KeyValuePair(Of Integer, Decimal) In AmountsOf(k_id)
                    k_total(k_pair.Key) = k_total.GetValueOrDefault(k_pair.Key) + k_pair.Value
                Next
            End If
        Next
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_total
            Dim k_pc As EFacturaKeReceptieClasificatie = k_rec.PeClasificatie.FirstOrDefault(Function(k_x) k_x.IdClsf = k_pair.Key)
            Dim k_left As Decimal = If(k_pc IsNot Nothing, k_pc.Rest, 0D)
            If Math.Sign(k_left) <> Math.Sign(k_pair.Value) OrElse Math.Abs(k_pair.Value) > Math.Abs(k_left) + Tolerance Then
                Dim k_label As String = If(_labelOf.ContainsKey(k_pair.Key), _labelOf(k_pair.Key), k_pair.Key.ToString(CultureInfo.InvariantCulture))
                ' Say where the reception DOES have rest, so a sum in «Rest nealocat» of the list is not a mystery when this classification has none.
                Dim k_elsewhere As List(Of String) = k_rec.PeClasificatie.Where(Function(k_x) k_x.IdClsf <> k_pair.Key AndAlso Math.Abs(k_x.Rest) >= Tolerance).
                    Select(Function(k_x) $"{If(_labelOf.ContainsKey(k_x.IdClsf), _labelOf(k_x.IdClsf), k_x.Clsf)} ({Fmt(k_x.Rest)})").ToList()
                Dim k_hint As String = If(k_elsewhere.Count > 0, $" Recepția are rest pe: {String.Join("; ", k_elsewhere)}. Alegeți clasificația facturii în acord cu ea sau o altă recepție.",
                                          " Scădeți «Recepționată», modificați recepția sau faceți una nouă.")
                Return $"Recepția aleasă nu are destul rest pe {k_label}: rest {Fmt(k_left)}, de alocat {Fmt(k_pair.Value)}." & k_hint
            End If
        Next
        Return Nothing
    End Function

    ' What FOREXE checks when a reception is saved: on every indicator the sum received (this reception, all the ticked invoices) must fit in the Disponibil
    ' (the budget of the classification on the day of the reception minus the receptions before that day, as in the fundamentation document). Not for a link to an existing reception: its value does not change. A storno (a negative sum) is not checked.
    Private Function AvailabilityProblem() As String
        If _data Is Nothing OrElse _mode = EFacturaKeTip.Existenta Then Return Nothing
        Dim k_total As Dictionary(Of Integer, Decimal) = ReceptionTotals()
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_total
            If k_pair.Value <= Tolerance Then Continue For
            Dim k_d As EFacturaKeDisponibil = DispoOf(k_pair.Key)
            If k_d Is Nothing OrElse Not k_d.Buget.HasValue Then Continue For   ' not an indicator of the angajament (the button says so), or the budget is not known yet
            Dim k_ind As EFacturaKeIndicator = IndicatorOf(k_pair.Key)
            Dim k_available As Decimal = k_d.Buget.Value - k_d.Receptii
            If k_pair.Value > k_available + Tolerance Then
                Return $"Nu există rezervare suficientă pe {If(k_ind IsNot Nothing, k_ind.Clsf, k_pair.Key.ToString(CultureInfo.InvariantCulture))}: disponibil {Fmt(k_available)}, " &
                       $"de recepționat {Fmt(k_pair.Value)}. Scădeți «Recepționată» sau «Valoare» ori faceți întâi o revizie de rezervare (documentul de fundamentare)."
            End If
        Next
        Return Nothing
    End Function

    Private Sub RefreshOk()
        If _viewing IsNot Nothing Then
            pgAsociate.gridReceptii.Column(FacturiCumparareReceptiiAsociateView.ColElimina).Enabled =
                _viewState IsNot Nothing AndAlso _viewState.PoateScrie AndAlso Not _busy
            Return
        End If
        ' Slice 02EF-07: nothing is made from «Asociate» (the list of what is already linked).
        If _assocTab Then
            btnOk.Enabled = False
            Return
        End If
        Dim k_any As Boolean = _checked.Any(Function(k_id) AmountsOf(k_id).Count > 0)
        Dim k_ok As Boolean = k_any AndAlso _data IsNot Nothing AndAlso _checked.All(Function(k_id) _states(k_id).PoateScrie)
        If _mode <> EFacturaKeTip.Noua Then k_ok = k_ok AndAlso ChosenReceptie() IsNot Nothing
        Dim k_kind As NoticeKind = NoticeKind.Error
        Dim k_problem As String = AvailabilityProblem()
        If k_problem Is Nothing Then k_problem = AngajamentProblem()
        If k_problem Is Nothing AndAlso k_ok Then
            k_problem = ReceptionFitProblem()
            k_kind = NoticeKind.Warning
        ElseIf k_problem Is Nothing AndAlso Not k_any AndAlso _mode = EFacturaKeTip.Existenta AndAlso _data IsNot Nothing Then
            ' Every sum was cut to 0 (the reception has nothing on the classification of the lines): say it, do not just leave «Leagă» grey.
            k_problem = ReceptionFitProblem(True)
            k_kind = NoticeKind.Warning
        End If
        If k_problem IsNot Nothing Then
            k_ok = False
            ShowNotice(k_problem, k_kind)
            _fitShown = True
        ElseIf _dispoWarning.Length > 0 AndAlso _mode <> EFacturaKeTip.Existenta Then
            ShowNotice(_dispoWarning, NoticeKind.Warning)
            _fitShown = True
        ElseIf _fitShown Then
            ntfMesaj.Visible = False
            _fitShown = False
        End If
        btnOk.Enabled = k_ok
        ' Slice 02EF-12: the ribbon button Creeaza rezervare follows the same state (it needs the amounts, not the room the reservations leave).
        RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub Notify(k_text As String, k_icon As MessageBoxIcon)
        KBotMessage.Show(Me, k_text, "Recepții", MessageBoxButtons.OK, k_icon)
    End Sub

    ' While the work runs nothing can be changed and nothing is sent twice.
    Friend Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        RaiseEvent BusyChanged(Me, EventArgs.Empty)
        tlyRec.Enabled = Not k_busy
        editorReceptii.SetPageEnabled(Not k_busy)
        If k_busy Then btnOk.Enabled = False Else RefreshOk()
    End Sub

    ' The request of every ticked invoice that has an amount, or Nothing (a notice says why).
    Private Function BuildRequests() As List(Of KeLegaFactura)
        Dim k_facturi As New List(Of KeLegaFactura)()
        For Each k_id As Integer In _checked
            Dim k_amounts As Dictionary(Of Integer, Decimal) = AmountsOf(k_id)
            If k_amounts.Count = 0 Then Continue For
            ' A classification that is not an indicator of the angajament cannot be received: the law says the indicator comes first.
            If k_amounts.Keys.Any(Function(k_c) IndicatorOf(k_c) Is Nothing) Then
                ShowNotice("O clasificație nu este indicator al angajamentului: este nevoie întâi de o revizie nouă de rezervare " &
                           "(documentul de fundamentare), apoi de recepție.", NoticeKind.Error)
                Return Nothing
            End If
            Dim k_p As EFacturaPrimita = _invoices(k_id)
            Dim k_one As New KeLegaFactura() With {.Factura = k_p, .Eticheta = $"{k_p.NrFact} ({k_p.Furnizor})".Trim()}
            ' Slice KE20-02: «Leagă» is sent with the day its receptions were measured on, so the server measures the rest the same way.
            Dim k_cerere As New EFacturaKeCerere() With {.CodAngajament = _angCod, .Tip = _mode, .LaData = ReceptionDay()}
            Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
            If k_rec IsNot Nothing Then k_cerere.Idrr = k_rec.Idrr
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_amounts
                k_cerere.Alocari.Add(New EFacturaKeAlocare() With {.IdClsf = k_pair.Key, .Suma = k_pair.Value})
            Next
            k_one.Cerere = k_cerere
            ' Slice KE20-04: the classification is stored on the lines of the invoice (the rest of K-BOT reads it there) only when the invoice goes on ONE classification;
            ' with several, the links are what says how it was split and the lines are left as they are.
            If k_amounts.Count = 1 Then
                Dim k_only As Integer? = k_amounts.Keys.First()
                For Each k_line As EFacturaKeLinie In _states(k_id).Linii
                    If Not Nullable.Equals(k_line.IdClsf, k_only) Then k_one.ClasificariSchimbate = True
                    k_one.Clasificari.Add(New EFacturaKeLinie() With {.IdLinie = k_line.IdLinie, .IdClsf = k_only})
                Next
            End If
            k_facturi.Add(k_one)
        Next
        ' Slice KE20-01: the value above the invoices belongs to the reception; it rides on the FIRST request only (the server answers it as a row, SumRows adds it once).
        If k_facturi.Count > 0 AndAlso AllowsExtra() Then
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In _extra
                If k_pair.Value <= Tolerance Then Continue For
                If IndicatorOf(k_pair.Key) Is Nothing Then
                    ShowNotice("O clasificație nu este indicator al angajamentului: este nevoie întâi de o revizie nouă de rezervare " &
                               "(documentul de fundamentare), apoi de recepție.", NoticeKind.Error)
                    Return Nothing
                End If
                k_facturi(0).Cerere.Surplus.Add(New EFacturaKeAlocare() With {.IdClsf = k_pair.Key, .Suma = k_pair.Value})
            Next
        End If
        Return k_facturi
    End Function

    ' UI boundary (async Sub): the button. Logs and swallows; what the operator must know is said in a box.
    Private Async Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If _busy OrElse _data Is Nothing OrElse String.IsNullOrWhiteSpace(_angCod) Then Return
            If _mode <> EFacturaKeTip.Existenta AndAlso KeRobot.Current Is Nothing Then
                Notify("Robotul FOREXE nu este disponibil, deci nu se poate face sau modifica o recepție. Puteți lega doar de o recepție existentă.", MessageBoxIcon.Information)
                Return
            End If
            Dim k_facturi As List(Of KeLegaFactura) = BuildRequests()
            If k_facturi Is Nothing OrElse k_facturi.Count = 0 Then Return
            If _mode = EFacturaKeTip.Noua AndAlso (pgNoua.cmbTip.SelectedIndex < 0 OrElse String.IsNullOrWhiteSpace(pgNoua.txtDescriere.Text)) Then
                ShowNotice("Alegeți tipul și scrieți descrierea recepției.", NoticeKind.Warning)
                Return
            End If
            ' Optional (Setari): one reception per day on an angajament. K-BOT's own receptions carry their numbers, so this is only a wish.
            If _mode = EFacturaKeTip.Noua AndAlso AppSettings.Current.KeBlockSameDayReception Then
                Dim k_day As Date = pgNoua.dtpData.Value.Date
                If _data.Receptii.Any(Function(k_x) k_x.DataR.HasValue AndAlso k_x.DataR.Value.Date = k_day) Then
                    ShowNotice($"Angajamentul are deja o recepție pe {k_day:dd.MM.yyyy}. Modificați-o (alegeți «Modifică o recepție existentă») sau alegeți altă dată.",
                               NoticeKind.Warning)
                    Return
                End If
            End If
            ntfMesaj.Visible = False
            SetBusy(True)
            Try
                Dim k_description As String = System.Text.RegularExpressions.Regex.Replace(pgNoua.txtDescriere.Text, "\s*[\r\n]+\s*", " ").Trim()
                Dim k_done As String = Await CommitAsync(k_facturi, Convert.ToString(pgNoua.cmbTip.SelectedItem, CultureInfo.InvariantCulture),
                                                         pgNoua.dtpData.Value.Date, k_description, ChosenReceptie()).ConfigureAwait(True)
                If k_done IsNot Nothing Then
                    If IsDisposed Then Return
                    ' The work is done: the invoices just covered leave the tree (the rest of it stays as it was), the receptions are read again.
                    pgNoua.txtDescriere.Text = String.Empty
                    _descrAuto = String.Empty
                    _dataAuto = Date.MinValue
                    _extra.Clear()
                    ' The host reads the invoices again (the window its tree, the view of an invoice the invoice) and gives them back; the message of success waits for that.
                    _pendingNotice = k_done
                    RaiseEvent Committed(Me, EventArgs.Empty)
                End If
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeReceptiiView.BtnOk_Click", ex)
            Notify(ex.Message, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.BtnOk_Click", ex)
            Notify("Legarea facturilor de recepție nu s-a putut face. Detalii în jurnalul de erori.", MessageBoxIcon.Error)
        End Try
    End Sub

    ' The numbers the K-BOT marker carries: written in the description of the reception on the site, so the download that follows knows whose it is.
    Private NotInheritable Class KeMarker
        Public Property Text As String = String.Empty
        Public Property Idrh As Integer
        Public Property Idr As Integer
    End Class

    Private Shared ReadOnly _markerPattern As New System.Text.RegularExpressions.Regex("\(IDRH:\s*(\d+);\s*IDR:\s*(\d+)\)", System.Text.RegularExpressions.RegexOptions.CultureInvariant)

    Private Async Function ReserveMarkerAsync(k_cod As String) As Task(Of KeMarker)
        Dim k_marker As IMarcajApi = TryCast(_api, IMarcajApi)
        If k_marker Is Nothing Then Throw New InvalidOperationException("The API client does not implement IMarcajApi.")
        Dim k_text As String = Await _gate.RunAsync(Function() k_marker.RezervaMarcajAsync("receptie", k_cod, _cts.Token)).ConfigureAwait(True)
        Dim k_match As System.Text.RegularExpressions.Match = _markerPattern.Match(k_text)
        If Not k_match.Success Then Throw New ApiException("Serverul a trimis un marcaj necunoscut pentru recepție.")
        Return New KeMarker() With {.Text = k_text, .Idrh = Integer.Parse(k_match.Groups(1).Value, CultureInfo.InvariantCulture),
                                    .Idr = Integer.Parse(k_match.Groups(2).Value, CultureInfo.InvariantCulture)}
    End Function

    ' The pattern of the marker numbers K-BOT may have written in the description of a reception; empty when it never wrote one.
    ' No colon in it: the workflow's IfVar splits its comparison on colons, and the dot stands for the one after «IDRH».
    Private Shared Function MarkerRegex(k_rec As EFacturaKeReceptie) As String
        If k_rec.Idrhs.Count = 0 Then Return String.Empty
        Return "IDRH.\s*(" & String.Join("|", k_rec.Idrhs.Select(Function(k_n) k_n.ToString(CultureInfo.InvariantCulture))) & ")\b"
    End Function

    ' The rows of several checks as the rows of one reception: the amounts of the same indicator are added up. For a reception that is MODIFIED the value
    ' written is its value now plus all the amounts (the server answers the value now of each indicator with every check).
    Private Shared Function SumRows(k_checks As IEnumerable(Of EFacturaKeVerificare), k_modify As Boolean) As List(Of KeReceptieRand)
        Dim k_rows As New List(Of KeReceptieRand)()
        For Each k_check As EFacturaKeVerificare In k_checks
            For Each k_r As EFacturaKeRand In k_check.Randuri
                Dim k_same As KeReceptieRand = k_rows.FirstOrDefault(
                    Function(k_x) String.Equals(k_x.ClsfSal, k_r.ClsfSal, StringComparison.Ordinal) AndAlso
                                  String.Equals(k_x.CodIndicator, k_r.CodIndicator, StringComparison.Ordinal))
                If k_same Is Nothing Then
                    ' Slice KE20-01: the value above the invoices (Surplus) is part of what the reception takes.
                    Dim k_start As Decimal = k_r.Suma + k_r.Surplus
                    If k_modify Then k_start = If(k_r.ValoareVeche.HasValue, k_r.ValoareVeche.Value + k_r.Suma + k_r.Surplus, If(k_r.ValoareNoua, k_r.Suma + k_r.Surplus))
                    k_rows.Add(New KeReceptieRand() With {.ClsfSal = k_r.ClsfSal, .Clsf = k_r.Clsf, .CodIndicator = k_r.CodIndicator, .Valoare = k_start})
                Else
                    k_same.Valoare += k_r.Suma + k_r.Surplus
                End If
            Next
        Next
        Return k_rows
    End Function

    ''' <summary>
    ''' Order, so that nothing reaches FOREXE before every check passed: (1) the changed choices of the lines are saved and the server checks every invoice
    ''' (a problem stops everything); (2) new / modified: ONE reception in FOREXE with the rows of all the invoices added up per indicator and ONE marker;
    ''' (3) only then every invoice gets its link (the same IDRH / IDR for a new reception). Nothing = it stopped before writing links (the box said why); empty = done (a box already told what went wrong); else the sentence of success.
    ''' </summary>
    Private Async Function CommitAsync(k_facturi As List(Of KeLegaFactura), k_tipReceptie As String, k_dataReceptie As Date,
                                       k_descriere As String, k_receptie As EFacturaKeReceptie) As Task(Of String)
        Dim k_tip As String = _mode
        Dim k_cod As String = _angCod
        Dim k_warnings As New List(Of String)()
        For Each k_one As KeLegaFactura In k_facturi
            Dim k_id As Integer = k_one.Factura.IdPrimita
            If k_one.ClasificariSchimbate Then
                Dim k_chosen As List(Of EFacturaKeLinie) = k_one.Clasificari.ToList()
                Await _gate.RunAsync(Function() _api.SetKeClasificatiiAsync(k_id, k_chosen, _cts.Token)).ConfigureAwait(True)
            End If
            Dim k_request As EFacturaKeCerere = k_one.Cerere
            k_one.Verificare = Await _gate.RunAsync(Function() _api.VerifyKeAsync(k_id, k_request, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return Nothing
            If Not k_one.Verificare.Ok Then
                Notify($"Factura {k_one.Eticheta}:" & Environment.NewLine & String.Join(Environment.NewLine, k_one.Verificare.Probleme), MessageBoxIcon.Warning)
                Return Nothing
            End If
            k_warnings.AddRange(k_one.Verificare.Avertismente)
        Next
        If k_warnings.Count > 0 AndAlso KBotMessage.Show(Me,
                String.Join(Environment.NewLine, k_warnings.Distinct()) & Environment.NewLine & Environment.NewLine & "Continuați?",
                "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return Nothing

        Dim k_marker As KeMarker = Nothing
        Dim k_saved As KeReceptieSalvata = Nothing
        Dim k_label As String = If(k_facturi.Count = 1, k_facturi(0).Eticheta, $"{k_facturi.Count} facturi")
        If k_tip = EFacturaKeTip.Noua Then
            k_marker = Await ReserveMarkerAsync(k_cod).ConfigureAwait(True)
            Dim k_job As New KeReceptieDeSalvat() With {
                .CodAngajament = k_cod, .Modifica = False, .TipReceptie = k_tipReceptie, .Data = k_dataReceptie,
                .Descriere = (k_descriere & " " & k_marker.Text).Trim(), .Marcaj = k_marker.Text, .Eticheta = k_label}
            k_job.Randuri.AddRange(SumRows(k_facturi.Select(Function(k_x) k_x.Verificare), False))
            k_saved = Await KeRobot.Current.SalveazaReceptieAsync(k_job, _cts.Token).ConfigureAwait(True)
        ElseIf k_tip = EFacturaKeTip.Modificata Then
            If k_receptie Is Nothing OrElse Not k_receptie.DataR.HasValue Then
                Notify("Recepția aleasă nu are dată; nu poate fi găsită în FOREXE.", MessageBoxIcon.Warning)
                Return Nothing
            End If
            k_marker = Await ReserveMarkerAsync(k_cod).ConfigureAwait(True)
            Dim k_job As New KeReceptieDeSalvat() With {
                .CodAngajament = k_cod, .Modifica = True, .Marcaj = k_marker.Text, .DataTinta = k_receptie.DataR, .SumaTinta = k_receptie.Total,
                .MarcajRegex = MarkerRegex(k_receptie), .Eticheta = k_label}
            k_job.Randuri.AddRange(SumRows(k_facturi.Select(Function(k_x) k_x.Verificare), True))
            k_saved = Await KeRobot.Current.SalveazaReceptieAsync(k_job, _cts.Token).ConfigureAwait(True)
        End If
        If IsDisposed Then Return Nothing
        If k_tip <> EFacturaKeTip.Existenta AndAlso (k_saved Is Nothing OrElse Not k_saved.Salvata) Then
            Notify(If(k_saved Is Nothing, If(k_tip = EFacturaKeTip.Noua, "Recepția nu s-a putut salva în FOREXE.", "Recepția nu s-a putut modifica în FOREXE."), k_saved.Mesaj),
                   MessageBoxIcon.Warning)
            Return Nothing
        End If
        ' Slice 02EF-11: the reception exists in FOREXE now; the ribbon offers the ordonantare built on it.
        If k_tip <> EFacturaKeTip.Existenta AndAlso k_marker IsNot Nothing Then
            _ordCod = k_cod
            _ordIdrh = k_marker.Idrh
            RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
        End If

        ' The links: one that fails does not stop the others; the reception exists in FOREXE, so the rest is told.
        Dim k_failed As New List(Of String)()
        For Each k_one As KeLegaFactura In k_facturi
            Try
                Dim k_id As Integer = k_one.Factura.IdPrimita
                Dim k_request As EFacturaKeCerere = k_one.Cerere
                If k_tip = EFacturaKeTip.Noua Then
                    k_request.Idrh = k_marker.Idrh
                    k_request.Idr = k_marker.Idr
                End If
                Await _gate.RunAsync(Function() _api.AddKeLegaturiAsync(k_id, k_request, _cts.Token)).ConfigureAwait(True)
            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                GlobalErrorLog.Write("KeReceptiiView.CommitAsync", ex)
                k_failed.Add($"{k_one.Eticheta}: {ex.Message}")
            End Try
        Next
        If IsDisposed Then Return Nothing
        If k_failed.Count > 0 Then
            Notify(If(k_tip = EFacturaKeTip.Existenta, "Legătura nu s-a scris pentru:", "Recepția este în FOREXE, dar legătura nu s-a scris pentru:") &
                   Environment.NewLine & String.Join(Environment.NewLine, k_failed) &
                   Environment.NewLine & Environment.NewLine & "Legați acele facturi de recepția existentă (ultima recepție a angajamentului).", MessageBoxIcon.Warning)
        ElseIf k_saved IsNot Nothing AndAlso Not k_saved.Preluata Then
            Notify(If(k_tip = EFacturaKeTip.Noua,
                      "Recepția este salvată în FOREXE, dar K-BOT nu a citit-o încă. Legăturile așteaptă descărcarea angajamentului.",
                      "Recepția este modificată în FOREXE, dar K-BOT nu a citit-o încă. Descărcați angajamentul ca să vedeți valoarea nouă."), MessageBoxIcon.Information)
            Return String.Empty
        End If
        Return If(k_facturi.Count = 1, "Factura a fost legată de recepție.", $"{k_facturi.Count} facturi au fost legate de recepție.")
    End Function

End Class
