Option Strict On
Imports System.Globalization
Imports System.Text
Imports System.Text.Json

''' <summary>
''' The registration requests of new units (slice ME01), the page that replaces the web page /operator:
''' the list, one request with the plan of its approval, approve (creates the database, the classification
''' rows and the account), reject (with the reason the applicant receives) and a new password link.
''' Everything runs on the server over SSH; the approval's progress lines flow into the text area.
''' </summary>
Public Class RequestsView
    Implements IAdminView

    Private ReadOnly _states As String() = {"InAsteptare", "Esuata", "Aprobata", "Respinsa", "toate"}
    Private ReadOnly _stateCaptions As String() = {"În așteptare", "Eșuate", "Aprobate", "Respinse", "Toate"}

    Private _context As AdminContext
    Private _detail As RequestDetail
    Private _items As New List(Of RequestItem)()
    Private _loadedId As Integer = -1
    Private _filling As Boolean
    Private _busy As Boolean

    Public Sub New()
        InitializeComponent()
        For Each k_caption As String In _stateCaptions
            cmbState.Items.Add(k_caption)
        Next
        cmbState.SelectedIndex = 0
        SetButtons(False)
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        Await LoadAsync()
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' Requests do not belong to a database.
    End Sub

    ' ------------------------------------------------------------------
    ' The list
    ' ------------------------------------------------------------------

    Private Async Function LoadAsync() As Task
        Await UiRun.Guard("RequestsView.LoadAsync", _context,
            Async Function()
                _context.Say("Se citesc cererile...")
                Dim k_state As String = _states(Math.Max(0, cmbState.SelectedIndex))
                Dim k_result As RequestsResult = Await _context.Client.CallAsync(Of RequestsResult)("requests", "--state", k_state)
                _items = k_result.Requests
                _filling = True
                grid.BeginUpdate()
                Try
                    grid.ClearRows()
                    For Each k_item As RequestItem In _items
                        Dim k_row As KBotDataRow = grid.AddRow()
                        k_row("id") = k_item.Id.ToString(CultureInfo.InvariantCulture)
                        k_row("date") = k_item.DateText
                        k_row("state") = k_item.State
                        k_row("name") = k_item.Name
                        k_row("email") = k_item.Email
                        k_row("cf") = k_item.Cf
                        k_row("year") = k_item.Year.ToString(CultureInfo.InvariantCulture)
                        k_row("rows") = k_item.Rows.ToString(CultureInfo.InvariantCulture)
                        k_row("decision") = If(k_item.DbName <> String.Empty, k_item.DbName & "  ", String.Empty) & k_item.DecidedBy
                        k_row.Tag = k_item
                    Next
                Finally
                    grid.EndUpdate()
                    _filling = False
                End Try
                _loadedId = -1
                _detail = Nothing
                SetButtons(False)
                txtDetail.Text = String.Empty
                lblState.Text = SummaryOf(k_result)
                _context.Say("Gata.")
                If _items.Count > 0 Then
                    grid.CurrentRowIndex = 0
                    Await ShowDetailAsync(_items(0).Id)
                End If
            End Function)
    End Function

    Private Shared Function SummaryOf(k_result As RequestsResult) As String
        Dim k_waiting As Integer = 0
        k_result.Counts.TryGetValue("InAsteptare", k_waiting)
        Dim k_failed As Integer = 0
        k_result.Counts.TryGetValue("Esuata", k_failed)
        Return k_waiting.ToString(CultureInfo.InvariantCulture) & " în așteptare, " & k_failed.ToString(CultureInfo.InvariantCulture) & " eșuate. Afișate: " &
               k_result.Requests.Count.ToString(CultureInfo.InvariantCulture) & "."
    End Function

    Private Async Sub CmbState_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbState.SelectedIndexChanged
        Try
            If _context Is Nothing Then Return
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RequestsView.CmbState_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub BtnRefresh_Click(k_sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RequestsView.BtnRefresh_Click", ex)
        End Try
    End Sub

    Private Async Sub Grid_SelectionChanged(k_sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            If _filling OrElse _busy Then Return
            Dim k_row As KBotDataRow = grid.CurrentRow
            Dim k_item As RequestItem = If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, RequestItem))
            If k_item Is Nothing OrElse k_item.Id = _loadedId Then Return
            Await UiRun.Guard("RequestsView.Grid_SelectionChanged", _context, Function() ShowDetailAsync(k_item.Id))
        Catch ex As Exception
            GlobalErrorLog.Write("RequestsView.Grid_SelectionChanged", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' One request
    ' ------------------------------------------------------------------

    Private Async Function ShowDetailAsync(k_id As Integer) As Task
        _context.Say("Se citește cererea " & k_id.ToString(CultureInfo.InvariantCulture) & "...")
        Dim k_detail As RequestDetail = Await _context.Client.CallAsync(Of RequestDetail)("request", "--id", k_id.ToString(CultureInfo.InvariantCulture))
        _detail = k_detail
        _loadedId = k_id
        If k_detail.Plan IsNot Nothing AndAlso k_detail.Plan.Ok AndAlso k_detail.Plan.Result IsNot Nothing Then
            txtDb.Text = k_detail.Plan.Result.DbName
            txtCodes.Text = String.Join(";", k_detail.Plan.Result.CodProgram.Select(Function(k_p) k_p.Key & "=" & k_p.Value))
        Else
            txtDb.Text = k_detail.DbName
            txtCodes.Text = String.Empty
        End If
        txtDetail.Text = Describe(k_detail)
        SetButtons(True)
        _context.Say("Gata.")
    End Function

    Private Sub SetButtons(k_hasRequest As Boolean)
        Dim k_approvable As Boolean = k_hasRequest AndAlso _detail IsNot Nothing AndAlso _detail.Approvable
        btnApprove.Enabled = k_approvable AndAlso _detail.Plan IsNot Nothing AndAlso _detail.Plan.Ok
        btnReject.Enabled = k_approvable
        btnPlan.Enabled = k_approvable
        txtDb.Enabled = k_approvable
        txtCodes.Enabled = k_approvable
        btnLink.Enabled = k_hasRequest AndAlso _detail IsNot Nothing AndAlso _detail.State = "Aprobata"
    End Sub

    Private Shared Function Describe(k_d As RequestDetail) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine("CERERE " & k_d.Id.ToString(CultureInfo.InvariantCulture) & " - " & k_d.State)
        k_sb.AppendLine("Unitate:     " & k_d.Name & If(k_d.AnafName <> String.Empty AndAlso k_d.AnafName <> k_d.Name, "   (ANAF: " & k_d.AnafName & ")", String.Empty))
        k_sb.AppendLine("Cod fiscal:  " & k_d.Cf & "     An: " & k_d.Year.ToString(CultureInfo.InvariantCulture))
        k_sb.AppendLine("E-mail:      " & k_d.Email)
        k_sb.AppendLine("Cerută la:   " & k_d.RequestedOn & "   de la IP " & k_d.Ip)
        If k_d.DecidedOn <> String.Empty Then k_sb.AppendLine("Decisă la:   " & k_d.DecidedOn & "   de " & k_d.DecidedBy)
        If k_d.DbName <> String.Empty Then k_sb.AppendLine("Baza:        " & k_d.DbName)
        If k_d.Reason <> String.Empty AndAlso k_d.State <> "Esuata" Then k_sb.AppendLine("Motiv:       " & k_d.Reason)
        If k_d.State = "Aprobata" Then
            k_sb.AppendLine("Link parolă:  " & If(k_d.LinkActive, If(k_d.LinkValid, "activ", "expirat"), "nu există"))
        End If
        If k_d.State = "Esuata" Then AppendFailure(k_sb, k_d.Reason)
        k_sb.AppendLine()
        k_sb.AppendLine("Surse-sectoare cerute (" & k_d.Ss.Count.ToString(CultureInfo.InvariantCulture) & "): " & String.Join(", ", k_d.Ss.Select(Function(k_c) k_c.Cod & " " & k_c.Name)))
        k_sb.AppendLine("Clasificații F (" & k_d.F.Count.ToString(CultureInfo.InvariantCulture) & "), E (" & k_d.E.Count.ToString(CultureInfo.InvariantCulture) & ")  =>  " &
                        k_d.Rows.ToString(CultureInfo.InvariantCulture) & " rânduri de clasificații")
        If k_d.Plan IsNot Nothing Then
            k_sb.AppendLine()
            k_sb.AppendLine("== PLANUL APROBĂRII (nu s-a scris nimic) ==")
            AppendPlan(k_sb, k_d.Plan)
        End If
        Return k_sb.ToString()
    End Function

    Private Shared Sub AppendPlan(k_sb As StringBuilder, k_plan As PlanBlock)
        For Each k_line As String In k_plan.Lines
            k_sb.AppendLine(k_line)
        Next
        If Not k_plan.Ok Then
            k_sb.AppendLine()
            k_sb.AppendLine("!! REVALIDARE: CEREREA NU POATE FI APROBATĂ ÎNCĂ")
            AppendWrapped(k_sb, "   ", k_plan.Problem)
        Else
            k_sb.AppendLine()
            k_sb.AppendLine("OK la revalidare: toate verificările trec; se poate apăsa «Aprobă».")
        End If
    End Sub

    ''' <summary>
    ''' The reason a failed approval stored («Eșuat la pasul N · nume: eroare | NU s-a putut anula: ...»)
    ''' laid out as step, error and what was left behind, instead of one long line.
    ''' </summary>
    Private Shared Sub AppendFailure(k_sb As StringBuilder, k_reason As String)
        k_sb.AppendLine()
        k_sb.AppendLine("== CE A OPRIT CEREREA ==")
        If k_reason = String.Empty Then
            k_sb.AppendLine("Serverul nu a păstrat un motiv.")
            Return
        End If
        Dim k_left As String = String.Empty
        Dim k_cut As Integer = k_reason.IndexOf(" | ", StringComparison.Ordinal)
        If k_cut >= 0 Then
            k_left = k_reason.Substring(k_cut + 3)
            k_reason = k_reason.Substring(0, k_cut)
        End If
        Const k_prefix As String = "Eșuat la pasul "
        If k_reason.StartsWith(k_prefix, StringComparison.Ordinal) Then
            Dim k_rest As String = k_reason.Substring(k_prefix.Length)
            Dim k_colon As Integer = k_rest.IndexOf(": ", StringComparison.Ordinal)
            If k_colon > 0 Then
                k_sb.AppendLine("Pasul:   " & k_rest.Substring(0, k_colon))
                k_sb.AppendLine("Eroarea:")
                AppendWrapped(k_sb, "   ", k_rest.Substring(k_colon + 2))
            Else
                AppendWrapped(k_sb, "   ", k_rest)
            End If
        Else
            AppendWrapped(k_sb, "   ", k_reason)
        End If
        If k_left <> String.Empty Then
            k_sb.AppendLine("Rămas de curățat (de mână):")
            AppendWrapped(k_sb, "   ", k_left)
        End If
        k_sb.AppendLine("Apăsați «Revalidează» după ce ați corectat cauza.")
    End Sub

    ''' <summary>Word-wraps at 100 columns so a long server message reads without sideways scrolling.</summary>
    Private Shared Sub AppendWrapped(k_sb As StringBuilder, k_indent As String, k_text As String)
        Const k_width As Integer = 100
        Dim k_line As New StringBuilder(k_indent)
        For Each k_word As String In k_text.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            If k_line.Length > k_indent.Length AndAlso k_line.Length + 1 + k_word.Length > k_width Then
                k_sb.AppendLine(k_line.ToString())
                k_line.Clear()
                k_line.Append(k_indent)
            End If
            If k_line.Length > k_indent.Length Then k_line.Append(" "c)
            k_line.Append(k_word)
        Next
        If k_line.Length > k_indent.Length Then k_sb.AppendLine(k_line.ToString())
    End Sub

    ' ------------------------------------------------------------------
    ' The operator's edits
    ' ------------------------------------------------------------------

    ''' <summary>The CodProgram text («SS=COD;SS=COD») as the JSON object the server expects.</summary>
    Private Function CodesJson() As String
        Dim k_map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each k_pair As String In txtCodes.Text.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
            Dim k_parts As String() = k_pair.Split({"="c}, 2)
            If k_parts.Length <> 2 OrElse k_parts(0).Trim() = String.Empty OrElse k_parts(1).Trim() = String.Empty Then
                Throw New AdminException("CodProgram: scrieți perechi SURSA-SECTOR=COD separate prin «;». Nu înțeleg: «" & k_pair.Trim() & "».")
            End If
            k_map(k_parts(0).Trim()) = k_parts(1).Trim()
        Next
        Return JsonSerializer.Serialize(k_map)
    End Function

    Private Async Sub BtnPlan_Click(k_sender As Object, e As EventArgs) Handles btnPlan.Click
        Try
            If _detail Is Nothing Then Return
            Await UiRun.Guard("RequestsView.BtnPlan_Click", _context,
                Async Function()
                    _context.Say("Se verifică planul...")
                    Dim k_plan As PlanBlock = Await _context.Client.CallAsync(Of PlanBlock)("request-plan", "--id", _detail.Id.ToString(CultureInfo.InvariantCulture),
                                                                                          "--db", txtDb.Text.Trim(), "--codes", CodesJson())
                    _detail.Plan = k_plan
                    txtDetail.Text = Describe(_detail)
                    SetButtons(True)
                    _context.Say(If(k_plan.Ok,
                                    If(_detail.State = "Esuata", "Revalidat: cauza eșecului nu se mai vede, cererea poate fi aprobată din nou.", "Planul e bun."),
                                    "Revalidare: cererea tot nu poate fi aprobată. " & k_plan.Problem))
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("RequestsView.BtnPlan_Click", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Decisions
    ' ------------------------------------------------------------------

    Private Async Sub BtnApprove_Click(k_sender As Object, e As EventArgs) Handles btnApprove.Click
        Try
            If _detail Is Nothing OrElse _busy Then Return
            Dim k_db As String = txtDb.Text.Trim().ToUpperInvariant()
            If k_db = String.Empty Then
                KBotMessage.Show("Scrieți numele bazei.", "Aprobare", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not UiRun.Confirm("Aprobați cererea " & _detail.Id.ToString(CultureInfo.InvariantCulture) & " (" & _detail.Name & ")?" & vbLf & vbLf &
                                 "Se creează baza " & k_db & ", rândurile de clasificații și contul " & _detail.Email &
                                 ", apoi se trimite linkul de parolă." & vbLf & "Nu porniți altă aprobare cât timp rulează.", "Aprobare cerere") Then Return
            _busy = True
            Dim k_id As Integer = _detail.Id
            Dim k_codes As String = CodesJson()
            Dim k_ok As Boolean = False
            txtDetail.Text = "== APROBAREA CERERII " & k_id.ToString(CultureInfo.InvariantCulture) & " ==" & vbCrLf
            Await UiRun.Guard("RequestsView.BtnApprove_Click", _context,
                Async Function()
                    _context.Say("Se aprobă cererea " & k_id.ToString(CultureInfo.InvariantCulture) & "...")
                    Await _context.Client.CallStreamAsync(Of JsonElement)("request-approve",
                        Sub(k_line) BeginInvoke(Sub() txtDetail.AppendText(k_line & vbCrLf)),
                        "--id", k_id.ToString(CultureInfo.InvariantCulture), "--db", k_db, "--codes", k_codes)
                    k_ok = True
                End Function)
            txtDetail.AppendText(If(k_ok, "GATA: cererea a fost aprobată.", "NEREUȘIT: vezi mesajul de eroare; serverul a desfăcut ce apucase să creeze.") & vbCrLf)
            _busy = False
            If k_ok Then _context.RequestRefresh()
            Await LoadAsync()
        Catch ex As Exception
            _busy = False
            GlobalErrorLog.Write("RequestsView.BtnApprove_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnReject_Click(k_sender As Object, e As EventArgs) Handles btnReject.Click
        Try
            If _detail Is Nothing Then Return
            Dim k_id As Integer = _detail.Id
            Using k_form As New ReasonForm("Motivul respingerii cererii " & k_id.ToString(CultureInfo.InvariantCulture) & " (" & _detail.Name & "). Îl primește solicitantul pe e-mail.")
                If k_form.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                Dim k_reason As String = k_form.Reason
                Dim k_result As RejectResult = Nothing
                Await UiRun.Guard("RequestsView.BtnReject_Click", _context,
                    Async Function()
                        k_result = Await _context.Client.CallAsync(Of RejectResult)("request-reject", "--id", k_id.ToString(CultureInfo.InvariantCulture), "--motiv", k_reason)
                    End Function)
                If k_result Is Nothing Then Return
                _context.Say("Cererea " & k_id.ToString(CultureInfo.InvariantCulture) & " a fost respinsă." &
                             If(k_result.Announced, " Solicitantul a fost anunțat pe e-mail.", " ATENȚIE: e-mailul către solicitant nu a plecat."))
                If Not k_result.Announced Then
                    KBotMessage.Show("Cererea a fost respinsă, dar e-mailul către " & k_result.Email & " nu a putut pleca. Anunțați-l dumneavoastră.", "Respingere", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End Using
            Await LoadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("RequestsView.BtnReject_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnLink_Click(k_sender As Object, e As EventArgs) Handles btnLink.Click
        Try
            If _detail Is Nothing Then Return
            If Not UiRun.Confirm("Generez un link nou de parolă pentru " & _detail.Email & "?" & vbLf & "Linkul vechi nu va mai funcționa.", "Link nou") Then Return
            Dim k_id As Integer = _detail.Id
            Await UiRun.Guard("RequestsView.BtnLink_Click", _context,
                Async Function()
                    Dim k_result As LinkResult = Await _context.Client.CallAsync(Of LinkResult)("request-link", "--id", k_id.ToString(CultureInfo.InvariantCulture))
                    If k_result.LinkTrimis Then
                        _context.Say("Linkul nou a fost trimis pe e-mail.")
                    ElseIf Not String.IsNullOrEmpty(k_result.Link) Then
                        Clipboard.SetText(k_result.Link)
                        txtDetail.AppendText(vbCrLf & "E-mailul nu a plecat. Linkul (copiat în clipboard):" & vbCrLf & k_result.Link & vbCrLf)
                        _context.Say("E-mailul nu a plecat; linkul e în clipboard.")
                    End If
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("RequestsView.BtnLink_Click", ex)
        End Try
    End Sub

End Class
