Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Clasificatii bugetare» (slice 0087-01, operator 26.09.2026): the classifications of the
''' database as a tree Capitol > Subcapitol > Articol > Alineat (code in the first column, name in
''' the second), and for the chosen alineat its budget for the working year as VERSIONS (slice 0102:
''' one row = the budget from its «Început» date on, quarters 1-4 typed, no total; «+» in the footer
''' adds a version, «✕» removes one) and its corrections (typed straight in the grid, «+» adds a row,
''' «✕» removes one, totals in the footer). «Salveaza» writes both in one transaction. The «+»
''' in the tree footer opens <see cref="ClasificatiiAddForm"/>.
''' </summary>
Public Class ClasificatiiForm

    Private Const ColTrim1 As String = "trim1"
    Private Const ColTrim2 As String = "trim2"
    Private Const ColTrim3 As String = "trim3"
    Private Const ColTrim4 As String = "trim4"
    Private Const ColTotal As String = "total"
    Private Const ColStart As String = "inceput"
    Private Const ColDocument As String = "document"
    Private Const ColData As String = "data"
    Private Const ColDelete As String = "sterge"
    Private Const ColId As String = "id"
    Private Const DeleteCaption As String = "✕"
    Private Shared ReadOnly QuarterColumns As String() = {ColTrim1, ColTrim2, ColTrim3, ColTrim4}
    Private Shared ReadOnly DateFormats As String() = {
        "d.M.yyyy", "dd.MM.yyyy", "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "yyyy-MM-dd", "ddMMyyyy"}

    Private ReadOnly _api As INomenclatoareApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _an As Integer
    Private ReadOnly _deletedIds As New List(Of Integer)()
    Private ReadOnly _deletedBudgetIds As New List(Of Integer)()
    Private _catalog As ClasificatiiCatalog
    Private _current As Clasificatie
    Private _currentNode As AdvancedTreeControl.TreeItem
    Private _dirty As Boolean
    Private _busy As Boolean
    Private _closeAfterSave As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    Public Sub New(api As INomenclatoareApi, gate As ReauthGate, an As Integer)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(gate)
        InitializeComponent()
        _api = api
        _gate = gate
        _an = an
    End Sub

    Private Sub ClasificatiiForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            capBar.Text = $"K-BOT — Clasificații bugetare {_an}"
            ShowNoSelection()
            LoadTree(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.ClasificatiiForm_Load", ex)
        End Try
    End Sub

    ' ── Tree ────────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadTree(selectIdClsf As Integer?)
        Try
            SetBusy(True, "Se încarcă clasificațiile…")
            Dim catalog As ClasificatiiCatalog = Await _gate.RunAsync(
                Function() _api.GetClasificatiiAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _catalog = catalog
            BuildTree(catalog, selectIdClsf)
            SetStatus($"{catalog.Items.Count} clasificații.")
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.LoadTree", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.LoadTree", ex)
            If Not IsDisposed Then SetStatus("Clasificațiile nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub BuildTree(catalog As ClasificatiiCatalog, selectIdClsf As Integer?)
        tree.Clear()
        _currentNode = Nothing
        Dim folder As Image = My.Resources.Resources.folder_open
        Dim toSelect As AdvancedTreeControl.TreeItem = Nothing

        Dim chapters = catalog.Items.GroupBy(Function(c) New With {Key c.Capitol, Key c.Ss}).
                                     OrderBy(Function(gp) gp.Key.Capitol, StringComparer.Ordinal).
                                     ThenBy(Function(gp) gp.Key.Ss, StringComparer.Ordinal)
        For Each chapter In chapters
            Dim capitol As String = chapter.Key.Capitol
            Dim ss As String = chapter.Key.Ss
            Dim chapterName As String = NameOr(catalog.CapitolNames, Left2(capitol))
            Dim ssName As String = NameOr(catalog.SsNames, ss)
            Dim root As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"C|{capitol}|{ss}",
                $"{capitol} ({ss})~~~{Escape(JoinNames(chapterName, ssName))}",
                pLeftIconClosed:=folder, pLeftIconOpen:=folder, pExpanded:=True)
            root.Bold = True

            For Each sub1 In chapter.GroupBy(Function(c) c.Subcapitol).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                Dim subName As String = NameOr(catalog.SubcapitolNames, Left2(capitol) & sub1.Key.Replace(".", String.Empty))
                Dim subNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                    $"S|{capitol}|{ss}|{sub1.Key}", $"{sub1.Key}~~~{Escape(subName)}", root,
                    pLeftIconClosed:=folder, pLeftIconOpen:=folder)

                For Each art In sub1.GroupBy(Function(c) c.Articol).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                    Dim artNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                        $"A|{capitol}|{ss}|{sub1.Key}|{art.Key}",
                        $"{art.Key}~~~{Escape(NameOr(catalog.ArticolNames, art.Key))}", subNode,
                        pLeftIconClosed:=folder, pLeftIconOpen:=folder)

                    For Each c As Clasificatie In art.OrderBy(Function(x) x.Alineat, StringComparer.Ordinal)
                        Dim leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                            $"L|{c.IdClsf}", $"{c.Articol}.{c.Alineat}~~~{Escape(c.Denumire)}", artNode)
                        leaf.Tag = c
                        leaf.Tooltip = c.Clsf
                        If selectIdClsf.HasValue AndAlso c.IdClsf = selectIdClsf.Value Then toSelect = leaf
                    Next
                Next
            Next
        Next

        If toSelect IsNot Nothing Then
            tree.SelectAndReveal(toSelect)
            _currentNode = toSelect
        End If
        tree.Invalidate()
    End Sub

    Private Shared Function Left2(capitol As String) As String
        Return If(capitol.Length >= 2, capitol.Substring(0, 2), capitol)
    End Function

    Private Shared Function NameOr(names As Dictionary(Of String, String), key As String) As String
        Dim name As String = Nothing
        If names IsNot Nothing AndAlso names.TryGetValue(key, name) AndAlso Not String.IsNullOrWhiteSpace(name) Then
            Return name.Trim()
        End If
        Return String.Empty
    End Function

    Private Shared Function JoinNames(first As String, second As String) As String
        If String.IsNullOrEmpty(first) Then Return second
        If String.IsNullOrEmpty(second) Then Return first
        Return first & " — " & second
    End Function

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(text As String) As String
        Return If(text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing OrElse pNode Is _currentNode Then Return
            Dim previous As AdvancedTreeControl.TreeItem = _currentNode
            ChangeSelection(pNode, previous)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' UI boundary: asks about unsaved changes, then loads the node's budget.
    Private Async Sub ChangeSelection(node As AdvancedTreeControl.TreeItem, previous As AdvancedTreeControl.TreeItem)
        Try
            If _busy Then
                If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                Return
            End If
            If _dirty Then
                Dim answer As DialogResult = AskToSave()
                If answer = DialogResult.Cancel Then
                    If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                    Return
                End If
                If answer = DialogResult.Yes Then
                    Dim saved As Boolean = Await SaveAsync().ConfigureAwait(True)
                    If Not saved Then
                        If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                        Return
                    End If
                End If
            End If

            _currentNode = node
            Dim c As Clasificatie = TryCast(node.Tag, Clasificatie)
            If c Is Nothing Then
                ShowNoSelection()
                Return
            End If
            Await LoadBudgetAsync(c).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.ChangeSelection", ex)
        End Try
    End Sub

    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            If _busy OrElse _api Is Nothing Then Return
            Using f As New ClasificatiiAddForm(_api, _gate, _an)
                If f.ShowDialog(Me) <> DialogResult.OK Then Return
                If f.Result IsNot Nothing AndAlso f.Result.Inserted > 0 Then
                    LoadTree(If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf))
                End If
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Tree_FooterRightIconClicked", ex)
            KBotMessage.Show(Me, "Fereastra de adăugare nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Clasificații", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── Budget + corrections ────────────────────────────────────────────────────

    ' Risky (HTTP): shows its own error in the status line; never throws to the caller.
    Private Async Function LoadBudgetAsync(c As Clasificatie) As Task
        Try
            SetBusy(True, "Se încarcă bugetul…")
            Dim data As BugetClasificatie = Await _gate.RunAsync(
                Function() _api.GetBugetClasificatieAsync(c.IdClsf, _an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _current = c
            FillGrids(data)
            SetStatus(String.Empty)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus("Bugetul nu a putut fi încărcat. Detalii în jurnalul de erori.")
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    Private Sub FillGrids(data As BugetClasificatie)
        gridBuget.BeginUpdate()
        Try
            gridBuget.ClearRows()
            For Each v As BudgetVersion In data.Budgets
                Dim row As KBotDataRow = gridBuget.AddRow()
                row(ColId) = If(v.Id.HasValue, CObj(v.Id.Value), Nothing)
                row(ColStart) = If(v.StartDate.HasValue, CObj(v.StartDate.Value), Nothing)
                row(ColTrim1) = Box(v.Amounts.Trim1)
                row(ColTrim2) = Box(v.Amounts.Trim2)
                row(ColTrim3) = Box(v.Amounts.Trim3)
                row(ColTrim4) = Box(v.Amounts.Trim4)
                row(ColDelete) = DeleteCaption
            Next
        Finally
            gridBuget.EndUpdate()
        End Try

        gridRectificari.BeginUpdate()
        Try
            gridRectificari.ClearRows()
            For Each r As RectificareBugetara In data.Corrections
                Dim row As KBotDataRow = gridRectificari.AddRow()
                row(ColId) = If(r.Id.HasValue, CObj(r.Id.Value), Nothing)
                row(ColDocument) = r.Document
                row(ColData) = If(r.Data.HasValue, CObj(r.Data.Value), Nothing)
                row(ColTrim1) = Box(r.Amounts.Trim1)
                row(ColTrim2) = Box(r.Amounts.Trim2)
                row(ColTrim3) = Box(r.Amounts.Trim3)
                row(ColTrim4) = Box(r.Amounts.Trim4)
                row(ColTotal) = r.Amounts.Total
                row(ColDelete) = DeleteCaption
            Next
        Finally
            gridRectificari.EndUpdate()
        End Try
        gridBuget.ClearDirty()
        gridRectificari.ClearDirty()
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()

        gridBuget.Enabled = True
        gridRectificari.Enabled = True
        lblBuget.Text = $"Buget {_an} — {_current.Clsf}"
        lblRectificari.Text = $"Rectificări bugetare {_an}"
        tips.SetToolTipText(lblBuget, _current.Denumire)
        SetDirty(False)
    End Sub

    Private Sub ShowNoSelection()
        _current = Nothing
        gridBuget.ClearRows()
        gridRectificari.ClearRows()
        gridBuget.Enabled = False
        gridRectificari.Enabled = False
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()
        lblBuget.Text = "Buget — alegeți un alineat din arbore"
        lblRectificari.Text = "Rectificări bugetare"
        SetDirty(False)
    End Sub

    Private Shared Function Box(value As Decimal?) As Object
        If value.HasValue Then Return value.Value
        Return Nothing
    End Function

    Private Shared Function ReadAmount(row As KBotDataRow, key As String) As Decimal?
        Dim v As Object = row(key)
        If v Is Nothing OrElse TypeOf v Is DBNull Then Return Nothing
        If TypeOf v Is Decimal Then Return DirectCast(v, Decimal)
        Dim parsed As Decimal
        If Decimal.TryParse(Convert.ToString(v, CultureInfo.CurrentCulture), NumberStyles.Number,
                            CultureInfo.CurrentCulture, parsed) Then Return parsed
        Return Nothing
    End Function

    Private Shared Function RowTotal(row As KBotDataRow) As Decimal
        Dim total As Decimal = 0D
        For Each key As String In QuarterColumns
            total += ReadAmount(row, key).GetValueOrDefault()
        Next
        Return total
    End Function

    ' Typed text -> the value the cell keeps: Decimal for the quarters, Date for «Data».
    Private Sub Grid_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) _
        Handles gridBuget.CellValidating, gridRectificari.CellValidating
        Try
            Dim text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
            If QuarterColumns.Contains(e.ColumnKey) Then
                If text.Length = 0 Then
                    e.ProposedValue = Nothing
                    Return
                End If
                Dim amount As Decimal
                If Not Decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, amount) Then
                    e.Cancel = True
                    SetStatus($"«{text}» nu este o sumă.")
                    Return
                End If
                e.ProposedValue = Math.Round(amount, 2)
            ElseIf e.ColumnKey = ColData OrElse e.ColumnKey = ColStart Then
                If text.Length = 0 Then
                    e.ProposedValue = Nothing
                    Return
                End If
                Dim day As Date
                If Not Date.TryParseExact(text, DateFormats, CultureInfo.CurrentCulture, DateTimeStyles.None, day) AndAlso
                   Not Date.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, day) Then
                    e.Cancel = True
                    SetStatus($"«{text}» nu este o dată (zz.ll.aaaa).")
                    Return
                End If
                e.ProposedValue = day.Date
            ElseIf e.ColumnKey = ColDocument Then
                e.ProposedValue = text
            End If
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Grid_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles gridBuget.CellValueChanged, gridRectificari.CellValueChanged
        Try
            Dim grid As KBotDataView = DirectCast(sender, KBotDataView)
            ' Only a correction's row has a «Total» column; a budget has none (slice 0102).
            If QuarterColumns.Contains(e.ColumnKey) AndAlso grid Is gridRectificari Then
                grid(ColTotal, e.RowIndex) = RowTotal(grid.Rows(e.RowIndex))
            End If
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Grid_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridBuget_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridBuget.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy Then Return
            If Not gridBuget.CommitPendingEdit() Then Return
            Dim row As KBotDataRow = gridBuget.AddRow()
            ' The first version of a year starts on 01.01; the next ones default to today (inside
            ' the year), the day a budget is usually changed.
            row(ColStart) = If(gridBuget.RowCount > 1 AndAlso Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
            row(ColDelete) = DeleteCaption
            row.IsDirty = True
            SetDirty(True)
            Dim index As Integer = gridBuget.RowCount - 1
            gridBuget.EnsureVisible(index)
            gridBuget.EditCell(ColStart, index)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridBuget_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridBuget_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridBuget.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridBuget.RowCount Then Return
            Dim id As Object = gridBuget(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedBudgetIds.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridBuget.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridBuget_ButtonClick", ex)
        End Try
    End Sub

    Private Sub GridRectificari_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridRectificari.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy Then Return
            If Not gridRectificari.CommitPendingEdit() Then Return
            Dim row As KBotDataRow = gridRectificari.AddRow()
            row(ColDocument) = String.Empty
            row(ColData) = If(Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
            row(ColTotal) = 0D
            row(ColDelete) = DeleteCaption
            row.IsDirty = True
            SetDirty(True)
            Dim index As Integer = gridRectificari.RowCount - 1
            gridRectificari.EnsureVisible(index)
            gridRectificari.EditCell(ColDocument, index)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridRectificari_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridRectificari_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridRectificari.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridRectificari.RowCount Then Return
            Dim id As Object = gridRectificari(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedIds.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridRectificari.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridRectificari_ButtonClick", ex)
        End Try
    End Sub

    ' ── Save / close ────────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Writes the budget and the corrections of the current classification. True = saved (or
    ''' nothing to save). Every failure is shown to the operator here.
    ''' </summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        If _current Is Nothing OrElse _api Is Nothing Then Return True
        Try
            If Not gridBuget.CommitPendingEdit() OrElse Not gridRectificari.CommitPendingEdit() Then Return False

            Dim problem As String = Nothing
            Dim budgets As List(Of BudgetVersion) = ReadBudgets(problem)
            If problem IsNot Nothing Then
                KBotMessage.Show(Me, problem, "Buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
            Dim corrections As List(Of RectificareBugetara) = ReadCorrections(problem)
            If problem IsNot Nothing Then
                KBotMessage.Show(Me, problem, "Rectificări bugetare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim deletedBudgets As New List(Of Integer)(_deletedBudgetIds)
            Dim deleted As New List(Of Integer)(_deletedIds)
            Dim idClsf As Integer = _current.IdClsf

            SetBusy(True, "Se salvează…")
            Dim saved As BugetClasificatie = Await _gate.RunAsync(
                Function() _api.SaveBugetClasificatieAsync(idClsf, _an, budgets, deletedBudgets, corrections,
                                                           deleted, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return True
            FillGrids(saved)
            SetStatus($"Salvat: {_current.Clsf}.")
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Bugetul nu a putut fi salvat. Detalii în jurnalul de erori.",
                                 "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The budget versions as the grid shows them; the first incomplete or repeated row is reported and
    ' selected. «Început» is required, inside the working year, and one per day.
    Private Function ReadBudgets(ByRef problem As String) As List(Of BudgetVersion)
        Dim list As New List(Of BudgetVersion)()
        Dim seen As New HashSet(Of Date)()
        For i As Integer = 0 To gridBuget.RowCount - 1
            Dim row As KBotDataRow = gridBuget.Rows(i)
            Dim startValue As Object = row(ColStart)
            If Not TypeOf startValue Is Date Then
                problem = $"Bugetul de la rândul {i + 1}: lipsește data de început."
            ElseIf DirectCast(startValue, Date).Year <> _an Then
                problem = $"Bugetul de la rândul {i + 1}: data de început {DirectCast(startValue, Date):dd.MM.yyyy} nu este în anul {_an}."
            ElseIf Not seen.Add(DirectCast(startValue, Date).Date) Then
                problem = $"Bugetul de la rândul {i + 1}: există deja o versiune care începe la {DirectCast(startValue, Date):dd.MM.yyyy}."
            End If
            If problem IsNot Nothing Then
                gridBuget.EnsureVisible(i)
                gridBuget.EditCell(ColStart, i)
                Return list
            End If
            Dim idValue As Object = row(ColId)
            list.Add(New BudgetVersion() With {
                .Id = If(idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(idValue, CultureInfo.InvariantCulture)),
                .StartDate = DirectCast(startValue, Date).Date,
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ReadAmount(row, ColTrim1), .Trim2 = ReadAmount(row, ColTrim2),
                    .Trim3 = ReadAmount(row, ColTrim3), .Trim4 = ReadAmount(row, ColTrim4)}})
        Next
        Return list
    End Function

    ' The corrections as the grid shows them; the first incomplete row is reported and selected.
    Private Function ReadCorrections(ByRef problem As String) As List(Of RectificareBugetara)
        Dim list As New List(Of RectificareBugetara)()
        For i As Integer = 0 To gridRectificari.RowCount - 1
            Dim row As KBotDataRow = gridRectificari.Rows(i)
            Dim document As String = Convert.ToString(row(ColDocument), CultureInfo.CurrentCulture).Trim()
            Dim dataValue As Object = row(ColData)
            If document.Length = 0 Then
                problem = $"Rândul {i + 1}: lipsește numărul documentului."
            ElseIf Not TypeOf dataValue Is Date Then
                problem = $"Rândul {i + 1}: lipsește data."
            ElseIf DirectCast(dataValue, Date).Year <> _an Then
                problem = $"Rândul {i + 1}: data {DirectCast(dataValue, Date):dd.MM.yyyy} nu este în anul {_an}."
            End If
            If problem IsNot Nothing Then
                gridRectificari.EnsureVisible(i)
                gridRectificari.EditCell(If(document.Length = 0, ColDocument, ColData), i)
                Return list
            End If
            Dim idValue As Object = row(ColId)
            list.Add(New RectificareBugetara() With {
                .Id = If(idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(idValue, CultureInfo.InvariantCulture)),
                .Document = document,
                .Data = DirectCast(dataValue, Date),
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ReadAmount(row, ColTrim1), .Trim2 = ReadAmount(row, ColTrim2),
                    .Trim3 = ReadAmount(row, ColTrim3), .Trim4 = ReadAmount(row, ColTrim4)}})
        Next
        Return list
    End Function

    Private Function AskToSave() As DialogResult
        Return KBotMessage.Show(Me, $"Bugetul clasificației {_current?.Clsf} are modificări nesalvate." & vbLf &
                                "Le salvați?", "Clasificații", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
    End Function

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _dirty AndAlso Not _closeAfterSave AndAlso e.CloseReason = CloseReason.UserClosing Then
                Dim answer As DialogResult = AskToSave()
                If answer = DialogResult.Cancel Then
                    e.Cancel = True
                ElseIf answer = DialogResult.Yes Then
                    e.Cancel = True
                    SaveThenClose()
                End If
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.OnFormClosing", ex)
        End Try
    End Sub

    ' UI boundary: the save cannot be awaited inside FormClosing; the window closes after it.
    Private Async Sub SaveThenClose()
        Try
            If Await SaveAsync().ConfigureAwait(True) AndAlso Not IsDisposed Then
                _closeAfterSave = True
                Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.SaveThenClose", ex)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetDirty(value As Boolean)
        _dirty = value
        btnSalveaza.Enabled = value AndAlso Not _busy AndAlso _current IsNot Nothing
    End Sub

    Private Sub SetBusy(busy As Boolean, status As String)
        _busy = busy
        UseWaitCursor = busy
        btnSalveaza.Enabled = _dirty AndAlso Not busy AndAlso _current IsNot Nothing
        If status IsNot Nothing Then SetStatus(status)
    End Sub

    Private Sub SetStatus(text As String)
        lblStare.Text = If(text, String.Empty)
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            Dim p As ThemePalette = scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(1)).
            BackColor = p.BorderColor
            tlyMain.BackColor = p.SurfaceAltColor
            pnlCard.BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyRight.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            lblBuget.BackColor = p.SurfaceAltColor
            lblBuget.ForeColor = p.TextColor
            lblRectificari.BackColor = p.SurfaceAltColor
            lblRectificari.ForeColor = p.TextColor
            lblStare.ForeColor = p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            ButtonStyles.ApplySecondary(btnInchide, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
