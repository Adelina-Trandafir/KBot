Imports System.Globalization
Imports KBot.Common
Imports KBot.Migrator

''' <summary>
''' The subunits of the unit database (the one the DC names), ported from the registry: one line per unit of the DC (<c>cale.accdb</c>
''' ▸ <c>cai</c>). The operator ticks the units to port, corrects the denomination (from <c>AlteDetalii</c>), the chapter (from
''' <c>SURSA</c>) and the salary decimals right in the grid, and the ticked ones are written in one transaction. A unit with a parent
''' (<c>IdUnitateParinte</c>) keeps the link: it reads the classifications and the budget of the parent. The chart of accounts
''' migrated by <see cref="AvcPlanContStep"/> goes into the subunit of its own unit.
''' Every control is declared in the designer file; this class only wires behaviour.
''' </summary>
Public Class AvcSubunitsForm

    Private Const AppTitle As String = "Subunități"

    Private ReadOnly _server As TargetServer
    Private ReadOnly _database As String
    Private ReadOnly _commonDatabase As String
    Private ReadOnly _units As List(Of CaiUnit)
    Private ReadOnly _parents As IReadOnlyDictionary(Of Integer, Integer)
    Private ReadOnly _preticked As HashSet(Of Integer)
    Private _sectors As New List(Of String)()
    Private _existing As New List(Of AvcSubunit)()

    ''' <summary>True once this dialog has added subunits.</summary>
    Public Property Added As Boolean

    ''' <param name="k_units">The units of the DC, in id order.</param>
    ''' <param name="k_parents">IdUnitate ▸ IdUnitate of the parent, for the units that have one.</param>
    ''' <param name="k_preticked">The units ticked on the main window: they start ticked here when not yet ported.</param>
    Friend Sub New(k_server As TargetServer, k_database As String, k_commonDatabase As String,
                   k_units As IEnumerable(Of CaiUnit), k_parents As IReadOnlyDictionary(Of Integer, Integer),
                   k_preticked As IEnumerable(Of Integer))
        InitializeComponent()
        _server = k_server
        _database = k_database
        _commonDatabase = k_commonDatabase
        _units = New List(Of CaiUnit)(k_units)
        _parents = k_parents
        _preticked = New HashSet(Of Integer)(k_preticked)
    End Sub

    Private Sub AvcSubunitsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Text = $"Subunități — {_database}"
            Reload()
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitsForm.Load", ex)
            lblStare.Text = "Subunitățile nu au putut fi citite."
            KBotMessage.Show("Subunitățile nu au putut fi citite din «" & _database & "»." & Environment.NewLine & Environment.NewLine &
                             ex.Message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
            btnPorteaza.Enabled = False
        End Try
    End Sub

    ''' <summary>Reads the target again and rebuilds the grid; the units already ported are shown as they are on the server.</summary>
    Private Sub Reload()
        Using k_cn = _server.Open(_database)
            If Not AvcSql.TableExists(k_cn, _database, "TZ_Subunitati") Then
                btnPorteaza.Enabled = False
                lblStare.Text = "Tabelul TZ_Subunitati lipsește."
                KBotMessage.Show($"Tabelul «TZ_Subunitati» nu există în «{_database}». Rulați scriptele sql/AVACONT/Configurari/PlanCont/AV01_04*.sql pe AVACONT_SURSA, " &
                                 "apoi «Sincronizare schemă» pe baza unității.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim k_missing = AvcSql.MissingColumns(k_cn, _database, "TZ_Subunitati", AvcSubunitStore.LinkColumns)
            If k_missing.Count > 0 Then
                btnPorteaza.Enabled = False
                lblStare.Text = "Lipsesc coloane în TZ_Subunitati."
                KBotMessage.Show($"«TZ_Subunitati» din «{_database}» nu are coloanele: {String.Join(", ", k_missing)}. Rulați " &
                                 "sql/AVACONT/Configurari/PlanCont/AV01_04d_subunitati_unitate_parinte.sql pe AVACONT_SURSA, apoi «Sincronizare schemă» pe baza unității.",
                                 AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            _sectors = AvcSubunitStore.SectorSources(k_cn, _commonDatabase)
            _existing = AvcSubunitStore.List(k_cn)
        End Using
        FillRows()
        btnPorteaza.Enabled = True
        Dim k_todo = dgvSubunitati.Rows.Where(Function(r) r.Enabled).Count()
        lblStare.Text = $"{_units.Count} unități în registru, {_units.Count - k_todo} deja portate."
    End Sub

    Private Sub FillRows()
        Dim k_usedCapitols As New HashSet(Of String)(_existing.Select(Function(s) s.Capitol), StringComparer.OrdinalIgnoreCase)
        dgvSubunitati.BeginUpdate()
        Try
            dgvSubunitati.ClearRows()
            For Each k_unit In _units
                Dim k_row = dgvSubunitati.AddRow()
                k_row.Tag = k_unit
                k_row("id") = k_unit.IdUnitate.ToString(CultureInfo.InvariantCulture)
                k_row("sursa") = k_unit.Sursa
                k_row("parinte") = If(_parents.ContainsKey(k_unit.IdUnitate), _parents(k_unit.IdUnitate).ToString(CultureInfo.InvariantCulture), String.Empty)

                Dim k_done = _existing.FirstOrDefault(Function(s) s.IdUnitateAccess = k_unit.IdUnitate)
                If k_done IsNot Nothing Then
                    k_row.Enabled = False
                    k_row("bifa") = False
                    k_row("denumire") = k_done.Denumire
                    k_row("capitol") = k_done.Capitol
                    k_row("zec") = k_done.Zecimale.ToString(CultureInfo.InvariantCulture)
                    k_row("stare") = $"Portată (id {k_done.IdSubunitate.ToString(CultureInfo.InvariantCulture)})"
                Else
                    ' A chapter already taken (by a subunit on the server or an earlier line) is left empty for the operator.
                    Dim k_capitol = AvcSubunitStore.SuggestedCapitol(k_unit.Sursa)
                    If k_capitol.Length > 0 AndAlso Not k_usedCapitols.Add(k_capitol) Then k_capitol = String.Empty
                    k_row("bifa") = _preticked.Contains(k_unit.IdUnitate)
                    k_row("denumire") = If(String.IsNullOrWhiteSpace(k_unit.AlteDetalii), k_unit.NumeUnitate, k_unit.AlteDetalii.Trim())
                    k_row("capitol") = k_capitol
                    k_row("zec") = "0"
                    k_row("stare") = If(k_capitol.Length = 0, "De portat (completați capitolul)", "De portat")
                End If
            Next
        Finally
            dgvSubunitati.EndUpdate()
        End Try
    End Sub

    ''' <summary>The salary decimals are 0 or 2: anything else is refused where it is typed.</summary>
    Private Sub DgvSubunitati_CellValidating(sender As Object, e As KBot.Controls.KBotCellValidatingEventArgs) Handles dgvSubunitati.CellValidating
        Try
            If e.ColumnKey <> "zec" Then Return
            Dim k_text = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
            If k_text IsNot Nothing Then k_text = k_text.Trim()
            If k_text = "0" OrElse k_text = "2" Then
                e.ProposedValue = k_text
            Else
                e.Cancel = True
                lblStare.Text = "Zecimalele salariilor pot fi 0 sau 2."
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitsForm.CellValidating", ex)
        End Try
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Try
            DialogResult = If(Added, DialogResult.OK, DialogResult.Cancel)
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitsForm.Close", ex)
        End Try
    End Sub

    Private Shared Function LabelOf(k_unit As CaiUnit) As String
        Return $"{k_unit.IdUnitate} ({k_unit.NumeUnitate})"
    End Function

    Private Sub BtnPorteaza_Click(sender As Object, e As EventArgs) Handles btnPorteaza.Click
        Try
            dgvSubunitati.CommitPendingEdit()
            Dim k_drafts As New List(Of AvcSubunit)()
            Dim k_unitOf As New Dictionary(Of Integer, CaiUnit)()
            For Each k_row In dgvSubunitati.Rows
                Dim k_unit = TryCast(k_row.Tag, CaiUnit)
                If k_unit Is Nothing OrElse Not k_row.Enabled Then Continue For
                If Not Convert.ToBoolean(k_row("bifa"), CultureInfo.InvariantCulture) Then Continue For
                Dim k_zec As Integer
                If Not Integer.TryParse(Convert.ToString(k_row("zec"), CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_zec) Then k_zec = -1
                Dim k_parent As Integer = 0
                _parents.TryGetValue(k_unit.IdUnitate, k_parent)
                k_drafts.Add(New AvcSubunit() With {
                    .Denumire = Convert.ToString(k_row("denumire"), CultureInfo.InvariantCulture).Trim(),
                    .Capitol = Convert.ToString(k_row("capitol"), CultureInfo.InvariantCulture).Trim(),
                    .SursaSector = k_unit.Sursa.Trim().ToUpperInvariant(),
                    .Zecimale = k_zec,
                    .IdUnitateAccess = k_unit.IdUnitate,
                    .ParinteUnitateAccess = k_parent})
                k_unitOf(k_unit.IdUnitate) = k_unit
            Next
            If k_drafts.Count = 0 Then
                KBotMessage.Show("Bifați cel puțin o unitate de portat.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim k_reason = CheckDrafts(k_drafts, k_unitOf)
            If k_reason IsNot Nothing Then
                KBotMessage.Show(k_reason, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_ordered = ParentsFirst(k_drafts)
            If k_ordered Is Nothing Then
                KBotMessage.Show("Legăturile de părinte dintre unitățile bifate se rotesc în cerc (o unitate ar fi propriul ei strămoș). Corectați-le în registru.",
                                 AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim k_count As Integer
            Using k_cn = _server.Open(_database)
                Try
                    k_count = AvcSubunitStore.AddMany(k_cn, _commonDatabase, k_ordered)
                Catch ex As InvalidOperationException
                    KBotMessage.Show(ex.Message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End Try
            End Using
            Added = True
            Reload()
            lblStare.Text = $"{k_count} subunități portate. {lblStare.Text}"
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitsForm.Port", ex)
            KBotMessage.Show("Subunitățile nu au putut fi portate." & Environment.NewLine & Environment.NewLine & ex.Message,
                             AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>The Romanian reason the ticked lines cannot be written, or Nothing. The store re-checks what only the server knows.</summary>
    Private Function CheckDrafts(k_drafts As List(Of AvcSubunit), k_unitOf As Dictionary(Of Integer, CaiUnit)) As String
        Dim k_sectors As New HashSet(Of String)(_sectors, StringComparer.OrdinalIgnoreCase)
        Dim k_names As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each k_old In _existing
            k_names(k_old.Denumire) = $"subunitatea «{k_old.Denumire}»"
        Next
        Dim k_ported As New HashSet(Of Integer)(_existing.Where(Function(s) s.IdUnitateAccess > 0).Select(Function(s) s.IdUnitateAccess))
        Dim k_ticked As New HashSet(Of Integer)(k_drafts.Select(Function(d) d.IdUnitateAccess))

        For Each k_draft In k_drafts
            Dim k_label = LabelOf(k_unitOf(k_draft.IdUnitateAccess))
            Dim k_reason = AvcSubunitStore.Validate(k_draft)
            If k_reason IsNot Nothing Then Return $"Unitatea {k_label}: {k_reason}"
            If Not k_sectors.Contains(k_draft.SursaSector) Then
                Return $"Unitatea {k_label}: sursa «{k_draft.SursaSector}» din registru nu există în «{_commonDatabase}» (DefaSursaSector)."
            End If
            Dim k_other As String = Nothing
            If k_names.TryGetValue(k_draft.Denumire, k_other) Then Return $"Unitatea {k_label}: denumirea «{k_draft.Denumire}» o are deja {k_other}."
            k_names(k_draft.Denumire) = $"unitatea {k_label}"

            Dim k_parent = k_draft.ParinteUnitateAccess
            If k_parent > 0 AndAlso Not k_ported.Contains(k_parent) AndAlso Not k_ticked.Contains(k_parent) Then
                Dim k_parentUnit = _units.FirstOrDefault(Function(u) u.IdUnitate = k_parent)
                If k_parentUnit Is Nothing Then
                    Return $"Unitatea {k_label} are părintele {k_parent}, care nu este în acest DC. Clasificațiile și bugetul lui nu pot fi citite din această bază."
                End If
                Return $"Unitatea {k_label} citește clasificațiile și bugetul unității {LabelOf(k_parentUnit)}, care nu este încă subunitate. Bifați-o și pe ea."
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>The ticked lines with every parent before its children, or Nothing when the parent links go in a circle.</summary>
    Private Function ParentsFirst(k_drafts As List(Of AvcSubunit)) As List(Of AvcSubunit)
        Dim k_done As New HashSet(Of Integer)(_existing.Where(Function(s) s.IdUnitateAccess > 0).Select(Function(s) s.IdUnitateAccess))
        Dim k_pending As New List(Of AvcSubunit)(k_drafts)
        Dim k_result As New List(Of AvcSubunit)()
        While k_pending.Count > 0
            Dim k_ready = k_pending.Where(Function(d) d.ParinteUnitateAccess = 0 OrElse k_done.Contains(d.ParinteUnitateAccess)).ToList()
            If k_ready.Count = 0 Then Return Nothing
            For Each k_draft In k_ready
                k_result.Add(k_draft)
                k_pending.Remove(k_draft)
            Next
            For Each k_draft In k_ready
                k_done.Add(k_draft.IdUnitateAccess)
            Next
        End While
        Return k_result
    End Function

End Class
