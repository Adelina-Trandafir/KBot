Option Strict On
Imports System.Collections.Generic
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
''' «Solduri initiale» view of the «Plan de conturi» window (slice AV01-04): a grid with a row per account of the subunit
''' (symbol, S.I. debitor, S.I. creditor, R.P. debitor, R.P. creditor: always DEBIT before CREDIT) where the operator types the opening balances and the
''' cumulated turnovers of the working year. No row to add: the accounts come from the chart. A row that was edited is saved by
''' itself when the operator moves to another row (and when the view is left or the window closed).
''' </summary>
Public Class PlanContSolduriView
    Implements IPlanContView, IThemedControl

    Private Const ColSimbol As String = "simbol"
    Private Const ColSid As String = "sid"
    Private Const ColSic As String = "sic"
    Private Const ColRpd As String = "rpd"
    Private Const ColRpc As String = "rpc"
    Private Const ColIdCont As String = "idcont"
    Private Const Caption As String = "Solduri inițiale"

    Private _ctx As PlanContContext
    Private ReadOnly _dirtyIds As New HashSet(Of Integer)()
    Private _lastRow As Integer = -1
    Private _filling As Boolean
    Private _busy As Boolean

    Public Event StatusMessage As Action(Of String) Implements IPlanContView.StatusMessage
    Public Event PlanChanged As Action(Of Integer?) Implements IPlanContView.PlanChanged
    Public Event BusyChanged As Action(Of Boolean) Implements IPlanContView.BusyChanged

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property HasPending As Boolean Implements IPlanContView.HasPending
        Get
            Return _dirtyIds.Count > 0
        End Get
    End Property

    Public ReadOnly Property CurrentContId As Integer? Implements IPlanContView.CurrentContId
        Get
            Dim k_id As Integer = IdAt(gridSolduri.CurrentRowIndex)
            Return If(k_id > 0, CType(k_id, Integer?), Nothing)
        End Get
    End Property

    Public Sub Bind(k_context As PlanContContext, k_selectCont As Integer?) Implements IPlanContView.Bind
        Try
            ArgumentNullException.ThrowIfNull(k_context)
            _ctx = k_context
            lblTitlu.Text = If(_ctx.Subunitate Is Nothing, "Solduri inițiale și rulaje cumulate",
                $"Solduri inițiale și rulaje cumulate · {_ctx.Subunitate.Denumire} · anul {_ctx.An.ToString(CultureInfo.InvariantCulture)}")
            FillGrid(k_selectCont)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.Bind", ex)
            Throw
        End Try
    End Sub

    Private Sub FillGrid(k_selectCont As Integer?)
        _filling = True
        Try
            _dirtyIds.Clear()
            gridSolduri.BeginUpdate()
            Try
                gridSolduri.ClearRows()
                For Each k_c As ContPlan In _ctx.VisibleAccounts().OrderBy(Function(k_x) k_x.Simbol, StringComparer.Ordinal)
                    Dim k_sold As SoldCont = _ctx.SoldOf(k_c.IdCont)
                    Dim k_row As KBotDataRow = gridSolduri.AddRow()
                    k_row(ColIdCont) = k_c.IdCont
                    k_row(ColSimbol) = k_c.Simbol
                    k_row(ColSid) = k_sold.SoldDebitor
                    k_row(ColSic) = k_sold.SoldCreditor
                    k_row(ColRpd) = k_sold.RulajDebitor
                    k_row(ColRpc) = k_sold.RulajCreditor
                Next
            Finally
                gridSolduri.EndUpdate()
            End Try
            gridSolduri.ClearDirty()
            If k_selectCont.HasValue Then
                Dim k_index As Integer = FindRow(k_selectCont.Value)
                If k_index >= 0 Then gridSolduri.CurrentRowIndex = k_index
            End If
            _lastRow = gridSolduri.CurrentRowIndex
        Finally
            _filling = False
        End Try
    End Sub

    Private Function FindRow(k_idCont As Integer) As Integer
        For k_i As Integer = 0 To gridSolduri.RowCount - 1
            If IdAt(k_i) = k_idCont Then Return k_i
        Next
        Return -1
    End Function

    Private Function IdAt(k_index As Integer) As Integer
        If k_index < 0 OrElse k_index >= gridSolduri.RowCount Then Return 0
        Return Convert.ToInt32(gridSolduri(ColIdCont, k_index), CultureInfo.InvariantCulture)
    End Function

    ' ── Typing ──────────────────────────────────────────────────────────────────

    ''' <summary>An amount typed by the operator: empty = 0, two decimals, never negative.</summary>
    Private Shared Function TryAmount(k_text As String, ByRef k_value As Decimal) As Boolean
        Dim k_t As String = If(k_text, String.Empty).Trim()
        If k_t.Length = 0 Then
            k_value = 0D
            Return True
        End If
        If Not Decimal.TryParse(k_t, NumberStyles.Number, CultureInfo.CurrentCulture, k_value) AndAlso
           Not Decimal.TryParse(k_t, NumberStyles.Number, CultureInfo.InvariantCulture, k_value) Then Return False
        k_value = Math.Round(k_value, 2, MidpointRounding.AwayFromZero)
        Return k_value >= 0D
    End Function

    Private Shared Function ToDecimal(k_cell As Object) As Decimal
        If k_cell Is Nothing Then Return 0D
        If TypeOf k_cell Is Decimal Then Return CDec(k_cell)
        Dim k_v As Decimal
        Return If(TryAmount(Convert.ToString(k_cell, CultureInfo.CurrentCulture), k_v), k_v, 0D)
    End Function

    Private Sub Grid_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridSolduri.CellValidating
        Try
            If String.Equals(e.ColumnKey, ColSimbol, StringComparison.Ordinal) Then Return
            Dim k_value As Decimal
            If TryAmount(Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture), k_value) Then
                e.ProposedValue = k_value
                RaiseEvent StatusMessage(String.Empty)
            Else
                e.Cancel = True
                RaiseEvent StatusMessage("Introduceți o sumă: un număr, cel puțin zero.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.Grid_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridSolduri.CellValueChanged
        Try
            If _filling Then Return
            Dim k_id As Integer = IdAt(e.RowIndex)
            If k_id > 0 Then _dirtyIds.Add(k_id)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.Grid_CellValueChanged", ex)
        End Try
    End Sub

    ' The row the operator leaves is saved when it was edited.
    Private Sub Grid_SelectionChanged(sender As Object, e As EventArgs) Handles gridSolduri.SelectionChanged
        Try
            If _filling Then Return
            Dim k_index As Integer = gridSolduri.CurrentRowIndex
            If k_index = _lastRow Then Return
            Dim k_previous As Integer = IdAt(_lastRow)
            _lastRow = k_index
            If k_previous > 0 AndAlso _dirtyIds.Contains(k_previous) Then SaveLeftRow(k_previous)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.Grid_SelectionChanged", ex)
        End Try
    End Sub

    ' UI boundary: the save runs after the selection event; on a refusal the operator is taken back to the row.
    Private Async Sub SaveLeftRow(k_idCont As Integer)
        Try
            If Not Await SaveRowAsync(k_idCont).ConfigureAwait(True) AndAlso Not IsDisposed Then
                Dim k_index As Integer = FindRow(k_idCont)
                If k_index >= 0 Then
                    _filling = True
                    Try
                        gridSolduri.CurrentRowIndex = k_index
                    Finally
                        _filling = False
                    End Try
                    _lastRow = k_index
                End If
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.SaveLeftRow", ex)
        End Try
    End Sub

    ' ── Saving ──────────────────────────────────────────────────────────────────

    ''' <summary>Writes one row. True = saved. A refusal is shown here and the row stays marked as edited.</summary>
    Private Async Function SaveRowAsync(k_idCont As Integer) As Task(Of Boolean)
        Try
            Dim k_index As Integer = FindRow(k_idCont)
            If k_index < 0 OrElse _ctx Is Nothing Then
                _dirtyIds.Remove(k_idCont)
                Return True
            End If
            Dim k_sold As New SoldCont() With {
                .IdCont = k_idCont,
                .SoldDebitor = ToDecimal(gridSolduri(ColSid, k_index)), .SoldCreditor = ToDecimal(gridSolduri(ColSic, k_index)),
                .RulajDebitor = ToDecimal(gridSolduri(ColRpd, k_index)), .RulajCreditor = ToDecimal(gridSolduri(ColRpc, k_index))}
            SetBusy(True)
            Try
                Await _ctx.Gate.RunAsync(Function() _ctx.Api.SaveSoldAsync(k_sold, _ctx.An, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
            _ctx.Solduri(k_idCont) = k_sold
            _dirtyIds.Remove(k_idCont)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContSolduriView.SaveRowAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.SaveRowAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Soldurile nu au putut fi salvate. Detalii în jurnalul de erori.",
                                 Caption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        End Try
    End Function

    ''' <summary>Closes the open editor and saves every edited row. True = nothing is left unsaved.</summary>
    Public Async Function CommitAsync() As Task(Of Boolean) Implements IPlanContView.CommitAsync
        If Not gridSolduri.CommitPendingEdit() Then Return False
        For Each k_id As Integer In _dirtyIds.ToList()
            If Not Await SaveRowAsync(k_id).ConfigureAwait(True) Then Return False
        Next
        Return True
    End Function

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        UseWaitCursor = k_busy
        RaiseEvent BusyChanged(k_busy)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_p As ThemePalette = k_scheme.Palette
            BackColor = k_p.SurfaceAltColor
            tlyRoot.BackColor = k_p.SurfaceAltColor
            lblTitlu.ForeColor = k_p.TextColor
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContSolduriView.ApplyTheme", ex)
        End Try
    End Sub

End Class
