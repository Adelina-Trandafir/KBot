Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' THE VALUE CORRECTION of ONE snapshot (slice 0111), opened from the context menu of a snapshot
''' in the association window.
'''
''' <para><b>Why it exists.</b> FOREXE's own history sometimes writes a wrong total on a reception
''' header (the row «Receptie: ..., valoare: 0» of a reception whose line says 1635). The total is
''' what the snapshot is matched to its reception by, and what DIF -- hence the ordonantare --
''' starts from. The operator corrects the figure here; the figure FOREXE gave is kept beside it
''' (<c>TotalOrig</c> / <c>ValoareOrig</c>) and «Revino la valorile din FOREXE» brings it back.</para>
'''
''' <para><b>Header and lines together.</b> One window, one save: the total row and every line of
''' the snapshot. The total must equal the sum of the lines -- a BLOCKING error here and again on the
''' server, which is the one that decides. A reason is required; it is kept with the operator's name
''' and the date.</para>
'''
''' <para><b>The save is made here</b>, through the call the caller hands in (the shell's re-login
''' net around <c>CorecteazaValoareaAsync</c>). Outcome through <see cref="Form.DialogResult"/>:
''' <c>OK</c> = saved; <c>Retry</c> = nothing written because the base holds other values than the
''' ones shown (or an ordonantare froze the snapshot) -- the caller reloads; <c>Cancel</c> = nothing
''' happened.</para>
''' </summary>
Public Class CorectieValoareForm

    ' The keys of `grdValori`'s columns -- identical to the designer's.
    Private Const COL_NAME As String = "nume"
    Private Const COL_FOREXE As String = "forexe"
    Private Const COL_VALUE As String = "valoare"

    ''' <summary>The longest reason the server accepts (<c>MAX_REASON_LENGTH</c> in asociere.py).</summary>
    Friend Const MAX_REASON As Integer = 500

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _snapshot As InstantaneuLegat
    Private ReadOnly _cod As String
    Private ReadOnly _save As Func(Of CorectieValoare, Task(Of AsociereRezultat))
    Private _saving As Boolean

    ' The total is OFFERED as the sum of the lines (what the rule asks of it) and follows them while
    ' the operator edits lines. Once the operator types in the total row itself the figure is theirs
    ' and stops following. `_updating` marks the writes the form makes itself, so they are not
    ' taken for typing.
    Private _totalManual As Boolean
    Private _updating As Boolean

    ''' <param name="snapshot">The snapshot as the association window read it, lines with their <c>Idr</c>.</param>
    ''' <param name="cod">Angajament code, for the title.</param>
    ''' <param name="save">The call that writes the correction (with the shell's re-login net).</param>
    Public Sub New(snapshot As InstantaneuLegat, cod As String,
                   save As Func(Of CorectieValoare, Task(Of AsociereRezultat)))
        ArgumentNullException.ThrowIfNull(snapshot)
        ArgumentNullException.ThrowIfNull(save)
        InitializeComponent()
        Try
            _snapshot = snapshot
            _cod = If(cod, String.Empty).Trim()
            _save = save
            capBar.IconImage = My.Resources.kbot_64
            capBar.Text = $"K-BOT — Corectează valoarea · {_cod} · {snapshot.DataH.ToString("dd.MM.yyyy HH:mm", _roCulture)}"
            Me.Text = capBar.Text
            If snapshot.CorectatLa.HasValue Then
                lblAntet.Text &= vbLf & "Ultima corecție: " & snapshot.CorectatDe & " · " &
                                 snapshot.CorectatLa.Value.ToString("dd.MM.yyyy HH:mm", _roCulture) & " · " &
                                 snapshot.CorectatMotiv
            End If
            txtMotiv.MaxLength = MAX_REASON
        Catch ex As Exception
            GlobalErrorLog.Write("CorectieValoareForm.New", ex)
            Throw
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The rules, pure -- checked here for the operator's convenience, decided on the server
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' What stops a save, in the words the operator reads, or empty when nothing does. The total
    ''' against the lines first: it is the rule the whole correction exists to keep.
    ''' </summary>
    Friend Shared Function ProblemWith(total As Double, linesSum As Double,
                                       reason As String, changed As Boolean) As String
        Dim mismatch As String = SumMismatch(total, linesSum)
        If mismatch.Length > 0 Then Return mismatch
        If Not changed Then Return "Nu ați schimbat nicio valoare."
        Dim k_reason As String = If(reason, String.Empty).Trim()
        If k_reason.Length = 0 Then Return "Scrieți motivul corecției."
        If k_reason.Length > MAX_REASON Then
            Return $"Motivul are {k_reason.Length} caractere; cel mult {MAX_REASON}."
        End If
        Return String.Empty
    End Function

    ''' <summary>The blocking error «total is not the sum of the lines», or empty when they agree (to two decimals).</summary>
    Friend Shared Function SumMismatch(total As Double, linesSum As Double) As String
        If Math.Round(total, 2) = Math.Round(linesSum, 2) Then Return String.Empty
        Return $"Totalul ({Money(total)}) nu este egal cu suma liniilor ({Money(linesSum)}). " &
               "Corectați totalul și liniile împreună."
    End Function

    Private Shared Function Money(k_value As Double) As String
        Return k_value.ToString("N2", _roCulture)
    End Function

    ''' <summary>A number typed in ro-RO («1.234,56»); empty = 0. False when it is not a number.</summary>
    Private Shared Function ReadNumber(k_value As Object, ByRef k_number As Double) As Boolean
        If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then
            k_number = 0.0R
            Return True
        End If
        If TypeOf k_value Is Double Then
            k_number = DirectCast(k_value, Double)
            Return True
        End If
        Dim k_text As String = Convert.ToString(k_value, _roCulture).Trim()
        If k_text.Length = 0 Then
            k_number = 0.0R
            Return True
        End If
        Return Double.TryParse(k_text, NumberStyles.Number, _roCulture, k_number)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The grid
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            FillGrid()
            ShowSums()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow, or the window would not open at all.
            GlobalErrorLog.Write("CorectieValoareForm.OnLoad", ex)
            lblSume.Text = "Fereastra nu s-a putut pregăti. Detalii în jurnalul de erori."
        End Try
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            ' Straight into the total: the figure that is usually the wrong one.
            grdValori.EditCell(COL_VALUE, 0)
        Catch ex As Exception
            GlobalErrorLog.Write("CorectieValoareForm.OnShown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Row 0 is the header total (<c>Tag = Nothing</c>), then one row per line (<c>Tag</c> = the
    ''' line). «Din FOREXE» is the original where the server filled it, else the value as it is now.
    ''' </summary>
    Private Sub FillGrid()
        grdValori.BeginUpdate()
        Try
            grdValori.ClearRows()
            Dim head As KBotDataRow = grdValori.AddRow()
            head.Tag = Nothing
            head(COL_NAME) = "Total recepție (antet)"
            head(COL_FOREXE) = If(_snapshot.TotalOrig.HasValue, _snapshot.TotalOrig.Value, _snapshot.Total)
            ' Offered: the sum of the lines per indicator, not the figure the history gave (which is
            ' the one that is usually wrong). A snapshot without lines keeps its own total.
            Dim k_sum As Double = 0.0R
            For Each l As LinieInstantaneu In _snapshot.Linii
                k_sum += l.Valoare
            Next
            head(COL_VALUE) = If(_snapshot.Linii.Count > 0, Math.Round(k_sum, 2), _snapshot.Total)
            For Each line As LinieInstantaneu In _snapshot.Linii
                Dim row As KBotDataRow = grdValori.AddRow()
                row.Tag = line
                row(COL_NAME) = line.CodIndicator
                row(COL_FOREXE) = If(line.ValoareOrig.HasValue, line.ValoareOrig.Value, line.Valoare)
                row(COL_VALUE) = line.Valoare
            Next
        Finally
            grdValori.EndUpdate()
        End Try
        grdValori.ClearDirty()
    End Sub

    ''' <summary>
    ''' Reads the typed values: the total, the lines (old value beside the new one) and whether
    ''' anything differs from what the window opened with. False when a cell is not a number.
    ''' </summary>
    Private Function ReadGrid(ByRef k_total As Double, k_lines As List(Of CorectieLinie),
                              ByRef k_changed As Boolean) As Boolean
        k_total = 0.0R
        k_changed = False
        For i As Integer = 0 To grdValori.RowCount - 1
            Dim row As KBotDataRow = grdValori.Rows(i)
            Dim k_number As Double
            If Not ReadNumber(row(COL_VALUE), k_number) Then Return False
            Dim line As LinieInstantaneu = TryCast(row.Tag, LinieInstantaneu)
            If line Is Nothing Then
                k_total = k_number
                If Math.Round(k_number, 2) <> Math.Round(_snapshot.Total, 2) Then k_changed = True
            Else
                k_lines.Add(New CorectieLinie() With {
                    .Idr = line.Idr, .CodIndicator = line.CodIndicator,
                    .ValoareVeche = line.Valoare, .Valoare = k_number})
                If Math.Round(k_number, 2) <> Math.Round(line.Valoare, 2) Then k_changed = True
            End If
        Next
        Return True
    End Function

    Private Shared Function SumOf(k_lines As IEnumerable(Of CorectieLinie)) As Double
        Dim k_sum As Double = 0.0R
        For Each l As CorectieLinie In k_lines
            k_sum += l.Valoare
        Next
        Return k_sum
    End Function

    ''' <summary>The live line under the grid: the sum of the lines, or the blocking mismatch in the error colour.</summary>
    Private Sub ShowSums()
        Dim k_total As Double
        Dim k_changed As Boolean
        Dim k_lines As New List(Of CorectieLinie)()
        If Not ReadGrid(k_total, k_lines, k_changed) Then Return
        Dim k_sum As Double = SumOf(k_lines)
        Dim k_mismatch As String = SumMismatch(k_total, k_sum)
        Dim palette As ThemePalette = ThemeManager.Current?.Palette
        If k_mismatch.Length > 0 Then
            lblSume.Text = "⚠ " & k_mismatch
            If palette IsNot Nothing Then lblSume.ForeColor = palette.ErrorColor
        Else
            lblSume.Text = $"Suma liniilor: {Money(k_sum)} · Total: {Money(k_total)} — se potrivesc."
            If palette IsNot Nothing Then lblSume.ForeColor = palette.TextDimColor
        End If
    End Sub

    ' Boundary UI (event handler): logged and swallowed.
    Private Sub grdValori_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles grdValori.CellValidating
        Try
            If e.ColumnKey <> COL_VALUE Then Return
            Dim k_number As Double
            If Not ReadNumber(e.ProposedValue, k_number) Then
                KBotMessage.Show(Me, "Valoarea nu este un număr.", Me.Text,
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
                Return
            End If
            e.ProposedValue = k_number
        Catch ex As Exception
            GlobalErrorLog.Write("CorectieValoareForm.grdValori_CellValidating", ex)
            ' A validator that threw must not let the value through.
            e.Cancel = True
        End Try
    End Sub

    Private Sub grdValori_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grdValori.CellValueChanged
        Try
            If e.ColumnKey <> COL_VALUE OrElse _updating Then Return
            If e.RowIndex >= 0 AndAlso e.RowIndex < grdValori.RowCount AndAlso
               grdValori.Rows(e.RowIndex).Tag Is Nothing Then
                ' Typed in the total row: the figure is the operator's from now on.
                _totalManual = True
            Else
                FollowLines()
            End If
            ShowSums()
        Catch ex As Exception
            GlobalErrorLog.Write("CorectieValoareForm.grdValori_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' While the total is still the offered one, sets it to the sum of the lines as they are now.
    ''' </summary>
    Private Sub FollowLines()
        If _totalManual Then Return
        Dim k_total As Double
        Dim k_changed As Boolean
        Dim k_lines As New List(Of CorectieLinie)()
        If Not ReadGrid(k_total, k_lines, k_changed) Then Return
        If k_lines.Count = 0 Then Return
        _updating = True
        Try
            grdValori.Rows(0)(COL_VALUE) = Math.Round(SumOf(k_lines), 2)
            grdValori.Invalidate()
        Finally
            _updating = False
        End Try
    End Sub

    ''' <summary>Puts the figures FOREXE gave back in the «Valoare corectă» column. Nothing is written yet.</summary>
    Private Sub btnRevino_Click(sender As Object, e As EventArgs) Handles btnRevino.Click
        Try
            If Not grdValori.CommitPendingEdit() Then Return
            _updating = True
            grdValori.BeginUpdate()
            Try
                For i As Integer = 0 To grdValori.RowCount - 1
                    Dim row As KBotDataRow = grdValori.Rows(i)
                    Dim k_original As Double
                    If ReadNumber(row(COL_FOREXE), k_original) Then row(COL_VALUE) = k_original
                Next
            Finally
                grdValori.EndUpdate()
                _updating = False
            End Try
            ' Back to the start: the total follows the lines again if the operator edits them.
            _totalManual = False
            ShowSums()
        Catch ex As Exception
            GlobalErrorLog.Write("CorectieValoareForm.btnRevino_Click", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The save
    ' ══════════════════════════════════════════════════════════════════════════

    Private Async Sub btnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        If _saving Then Return
        Try
            ' Enter on the accept button never reaches the grid's editor: a value still being
            ' typed is committed first, and a refused one keeps the window open.
            If Not grdValori.CommitPendingEdit() Then Return

            Dim k_total As Double
            Dim k_changed As Boolean
            Dim k_lines As New List(Of CorectieLinie)()
            If Not ReadGrid(k_total, k_lines, k_changed) Then
                KBotMessage.Show(Me, "O valoare nu este un număr.", Me.Text,
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim k_problem As String = ProblemWith(k_total, SumOf(k_lines), txtMotiv.Text, k_changed)
            If k_problem.Length > 0 Then
                KBotMessage.Show(Me, k_problem, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim correction As New CorectieValoare() With {
                .Idrh = _snapshot.Idrh,
                .TotalVechi = _snapshot.Total,
                .Total = k_total,
                .Motiv = txtMotiv.Text.Trim()}
            correction.Linii.AddRange(k_lines)

            _saving = True
            SetBusy(True)
            Await _save(correction)

            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            ' UI boundary (async handler): log and show, never re-throw onto the UI thread.
            GlobalErrorLog.Write("CorectieValoareForm.btnSalveaza_Click", ex)
            ShowSaveError(ex)
        Finally
            _saving = False
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    ''' <summary>
    ''' Two refusals mean the picture is old and nothing was written: the caller reloads
    ''' (<c>Retry</c>). A 400 is a sentence for the operator and the window stays open to fix it.
    ''' </summary>
    Private Sub ShowSaveError(ex As Exception)
        If IsDisposed Then Return
        Dim api As ApiException = TryCast(ex, ApiException)
        If api IsNot Nothing Then
            Select Case api.Reason
                Case PrelucrarePropunere.MotivStareModificata
                    KBotMessage.Show(Me, "Valorile acestui instantaneu s-au schimbat între timp. " &
                                     "Nu s-a scris nimic — se reîncarcă imaginea.", Me.Text,
                                     MessageBoxButtons.OK, MessageBoxIcon.Information)
                    DialogResult = DialogResult.Retry
                    Close()
                    Return
                Case AsociereStare.MotivInstantaneuBlocat
                    KBotMessage.Show(Me, "Nu s-a scris nimic. " & api.Message, Me.Text,
                                     MessageBoxButtons.OK, MessageBoxIcon.Information)
                    DialogResult = DialogResult.Retry
                    Close()
                    Return
            End Select
            KBotMessage.Show(Me, api.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        KBotMessage.Show(Me, "Nu am putut salva corecția: " & ex.Message, Me.Text,
                         MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Sub SetBusy(busy As Boolean)
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
        btnSalveaza.Enabled = Not busy
        btnRevino.Enabled = Not busy
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Theming
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyCorp.BackColor = p.SurfaceAltColor
            For Each lbl As Label In {lblAntet, lblSume, lblMotivCaption}
                lbl.ForeColor = p.TextDimColor
                lbl.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnRevino, scheme)
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            If grdValori.RowCount > 0 Then ShowSums()
        Catch ex As Exception
            GlobalErrorLog.Write("CorectieValoareForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
