Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice 00EF-08 -- the «Conținut» tab: the lines of the invoice in a grid typed straight into (Access EFACTURA_ADD_SUB), with
' the unit of measure chosen from the common UN/ECE list. The value of a line (quantity x price, two decimals) is shown as
' the operator types; the server computes it again on save and its figure is the one that counts.
' Slice 00EF-50: while the invoice is being typed the grid ends with ONE SPARE ROW (Tag SpareTag, no button shown): typing in any of
' its cells turns it into a line (unit XPP, quantity 1, price 0 unless typed) and a fresh spare row appears below. There is no
' "Linie noua" button any more; each line has "duplicate" (a copy at the end, the cursor in its "Continut") and "delete".
Partial Public Class FacturiForm

    Private Const ColNr As String = "nr"
    Private Const ColContinut As String = "continut"
    Private Const ColUm As String = "um"
    Private Const ColCant As String = "cant"
    Private Const ColPu As String = "pu"
    Private Const ColValoare As String = "valoare"
    Private Const ColSterge As String = "sterge"
    Private Const ColDuplica As String = "duplica"
    Private Const DeleteCaption As String = "✕"
    Private Const DuplicateCaption As String = "❐"
    ' The unit Access put on a new line (def="XPP" in EFACTURA_ADD_SUB): UN/ECE «bucată».
    Private Const DefaultUm As String = "XPP"

    ''' <summary>One unit of measure of the list: «COD — explanation», so typing either finds it.</summary>
    Private NotInheritable Class UmChoice
        Public ReadOnly Cod As String
        Public ReadOnly Explicatie As String

        Public Sub New(k_cod As String, k_explicatie As String)
            Cod = k_cod
            Explicatie = k_explicatie
        End Sub

        Public Overrides Function ToString() As String
            Return If(Explicatie.Length = 0, Cod, $"{Cod} — {Explicatie}")
        End Function
    End Class

    Private _um As New List(Of UmChoice)()

    ' The Tag of the spare row at the end of the grid (a real new line has Tag Nothing, so Nothing cannot mark it).
    Private Shared ReadOnly SpareTag As New Object()

    ' ── Units of measure ────────────────────────────────────────────────────────

    ''' <summary>Reads the list; a failure is not fatal (the operator may type the code). Returns a warning text or Nothing.</summary>
    Private Async Function LoadUmAsync() As Task(Of String)
        Try
            Dim k_list As List(Of EFacturaUm) = Await _gate.RunAsync(
                Function() _api.GetUmAsync(Nothing, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return Nothing
            FillUm(k_list)
            Return Nothing
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.LoadUmAsync", ex)
            FillUm(New List(Of EFacturaUm)())
            Return "Lista unităților de măsură nu a putut fi citită; scrieți codul unității (de exemplu XPP) direct în tabel. " & ex.Message
        End Try
    End Function

    Private Sub FillUm(k_list As List(Of EFacturaUm))
        _um = k_list.Select(Function(x) New UmChoice(x.Cod, x.Explicatie)).ToList()
        gridLinii.Column(ColUm).ComboItems = _um.Cast(Of Object)().ToList()
    End Sub

    ''' <summary>The list entry of a code; a code the list does not know is kept as a bare entry (it is not lost).</summary>
    Private Function UmFor(k_cod As String) As UmChoice
        Dim k_text As String = If(k_cod, String.Empty).Trim()
        Dim k_found As UmChoice = _um.FirstOrDefault(Function(x) String.Equals(x.Cod, k_text, StringComparison.OrdinalIgnoreCase))
        Return If(k_found, New UmChoice(k_text, String.Empty))
    End Function

    Private Shared Function UmCode(k_value As Object) As String
        Dim k_choice As UmChoice = TryCast(k_value, UmChoice)
        If k_choice IsNot Nothing Then Return k_choice.Cod
        Return Convert.ToString(k_value, CultureInfo.CurrentCulture)?.Trim().ToUpperInvariant()
    End Function

    ' ── Showing and reading the lines ───────────────────────────────────────────

    Private Sub FillLines(k_lines As List(Of EFacturaLinie))
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            For Each k_l As EFacturaLinie In k_lines
                Dim k_row As KBotDataRow = gridLinii.AddRow()
                k_row.Tag = k_l
                k_row(ColNr) = k_l.NrCrt
                k_row(ColContinut) = k_l.Continut
                k_row(ColUm) = UmFor(k_l.Um)
                k_row(ColCant) = k_l.Cant
                k_row(ColPu) = k_l.PU
                k_row(ColValoare) = k_l.Valoare
                k_row(ColDuplica) = DuplicateCaption
                k_row(ColSterge) = DeleteCaption
            Next
        Finally
            gridLinii.EndUpdate()
        End Try
        gridLinii.ClearDirty()
        UpdateTotal()
    End Sub

    ''' <summary>The lines as the grid shows them, or Nothing after telling the operator what is wrong (the row is opened).</summary>
    Private Function ReadLines() As List(Of EFacturaLinie)
        If RealLineCount() = 0 Then
            Problem("Factura trebuie să aibă cel puțin o linie (vederea «Conținut»).", ViewContinut, gridLinii)
            Return Nothing
        End If
        Dim k_lines As New List(Of EFacturaLinie)()
        For k_i As Integer = 0 To gridLinii.RowCount - 1
            Dim k_row As KBotDataRow = gridLinii.Rows(k_i)
            If IsSpare(k_row) Then Continue For
            Dim k_content As String = Convert.ToString(k_row(ColContinut), CultureInfo.CurrentCulture)?.Trim()
            Dim k_um As String = UmCode(k_row(ColUm))
            If String.IsNullOrEmpty(k_content) Then
                LineProblem($"Linia {k_i + 1}: completați conținutul.", k_i, ColContinut)
                Return Nothing
            End If
            If String.IsNullOrEmpty(k_um) Then
                LineProblem($"Linia {k_i + 1}: alegeți unitatea de măsură.", k_i, ColUm)
                Return Nothing
            End If
            Dim k_old As EFacturaLinie = TryCast(k_row.Tag, EFacturaLinie)
            k_lines.Add(New EFacturaLinie() With {
                .NrCrt = Convert.ToString(k_row(ColNr), CultureInfo.CurrentCulture),
                .Continut = k_content,
                .Um = k_um,
                .Cant = AsDecimal(k_row(ColCant)),
                .PU = AsDecimal(k_row(ColPu)),
                .Platit = k_old IsNot Nothing AndAlso k_old.Platit,
                .Grup = If(k_old Is Nothing, 0, k_old.Grup)})
        Next
        Return k_lines
    End Function

    ' ── The spare row ───────────────────────────────────────────────────────────

    Private Shared Function IsSpare(k_row As KBotDataRow) As Boolean
        Return k_row IsNot Nothing AndAlso ReferenceEquals(k_row.Tag, SpareTag)
    End Function

    ''' <summary>The lines of the invoice, without the spare row.</summary>
    Private Function RealLineCount() As Integer
        Dim k_count As Integer = gridLinii.RowCount
        If k_count > 0 AndAlso IsSpare(gridLinii.Rows(k_count - 1)) Then k_count -= 1
        Return k_count
    End Function

    ''' <summary>The grid ends with the spare row while the invoice is typed or modified, and has none while it is only shown.</summary>
    Private Sub ApplySpareLine(k_edit As Boolean)
        Dim k_last As Integer = gridLinii.RowCount - 1
        Dim k_has As Boolean = k_last >= 0 AndAlso IsSpare(gridLinii.Rows(k_last))
        If k_edit = k_has Then Return
        If k_has Then
            gridLinii.RemoveRowAt(k_last)
        Else
            AddSpareRow()
        End If
    End Sub

    Private Sub AddSpareRow()
        Dim k_row As KBotDataRow = gridLinii.AddRow()
        k_row.Tag = SpareTag
    End Sub

    ''' <summary>The spare row was typed into: it becomes a line (the cells not typed get their defaults) and a new spare row follows it.</summary>
    Private Sub PromoteSpareLine(k_row As KBotDataRow, k_columnKey As String)
        Dim k_typed As String = Convert.ToString(k_row(k_columnKey), CultureInfo.CurrentCulture)
        If String.IsNullOrWhiteSpace(k_typed) Then Return
        k_row(ColNr) = NextLineNumber()
        If k_row(ColUm) Is Nothing Then k_row(ColUm) = UmFor(DefaultUm)
        If k_row(ColCant) Is Nothing Then k_row(ColCant) = 1D
        If k_row(ColPu) Is Nothing Then k_row(ColPu) = 0D
        k_row(ColValoare) = Math.Round(AsDecimal(k_row(ColCant)) * AsDecimal(k_row(ColPu)), 2, MidpointRounding.AwayFromZero)
        k_row(ColDuplica) = DuplicateCaption
        k_row(ColSterge) = DeleteCaption
        k_row.Tag = Nothing
        k_row.IsDirty = True
        AddSpareRow()
    End Sub

    Private Sub LineProblem(k_text As String, k_rowIndex As Integer, k_column As String)
        SelectView(ViewContinut)
        KBotMessage.Show(Me, k_text, "Salvare factură", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        gridLinii.EnsureVisible(k_rowIndex)
        gridLinii.EditCell(k_column, k_rowIndex)
    End Sub

    Private Shared Function AsDecimal(k_value As Object) As Decimal
        If k_value Is Nothing Then Return 0D
        If TypeOf k_value Is Decimal Then Return CDec(k_value)
        Dim k_number As Decimal
        If TryParseNumber(Convert.ToString(k_value, CultureInfo.CurrentCulture), k_number) Then Return k_number
        Return 0D
    End Function

    ''' <summary>
    ''' A number as the operator types it: «12,5» or «12.5», a sign allowed, no thousands separator (so «1.234» is 1.234, as
    ''' typed, never 1234).
    ''' </summary>
    Private Shared Function TryParseNumber(k_text As String, ByRef k_number As Decimal) As Boolean
        Dim k_clean As String = If(k_text, String.Empty).Replace(" ", String.Empty).Trim()
        If k_clean.Length = 0 Then Return False
        Const k_style As NumberStyles = NumberStyles.AllowLeadingSign Or NumberStyles.AllowDecimalPoint
        If Decimal.TryParse(k_clean, k_style, CultureInfo.CurrentCulture, k_number) Then Return True
        Return Decimal.TryParse(k_clean, k_style, CultureInfo.InvariantCulture, k_number)
    End Function

    Private Shared Function DecimalsOf(k_value As Decimal) As Integer
        Return CInt((Decimal.GetBits(k_value)(3) >> 16) And &HFF)
    End Function

    Private Sub UpdateTotal()
        Dim k_total As Decimal = 0D
        For k_i As Integer = 0 To gridLinii.RowCount - 1
            k_total += AsDecimal(gridLinii.Rows(k_i)(ColValoare))
        Next
        lblTotal.Text = k_total.ToString("N2", CultureInfo.CurrentCulture)
    End Sub

    ' ── Events ──────────────────────────────────────────────────────────────────

    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridLinii.CellValidating
        Try
            Select Case e.ColumnKey
                Case ColCant, ColPu
                    Dim k_number As Decimal
                    If TypeOf e.ProposedValue Is Decimal Then
                        k_number = CDec(e.ProposedValue)
                    ElseIf Not TryParseNumber(Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture), k_number) Then
                        e.Cancel = True
                        SetStatus("Scrieți un număr (de exemplu 12,5).")
                        Return
                    End If
                    Dim k_places As Integer = If(e.ColumnKey = ColCant, 3, 4)
                    If DecimalsOf(k_number) > k_places AndAlso k_number <> Math.Round(k_number, k_places) Then
                        e.Cancel = True
                        SetStatus(If(e.ColumnKey = ColCant, "Cantitatea poate avea cel mult 3 zecimale.", "Prețul poate avea cel mult 4 zecimale."))
                        Return
                    End If
                    e.ProposedValue = k_number
                Case ColUm
                    If TypeOf e.ProposedValue Is UmChoice Then Return
                    Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture)?.Trim()
                    If String.IsNullOrEmpty(k_text) Then Return
                    If _um.Count = 0 Then
                        e.ProposedValue = New UmChoice(k_text.ToUpperInvariant(), String.Empty)
                        Return
                    End If
                    Dim k_match As UmChoice = _um.FirstOrDefault(
                        Function(x) String.Equals(x.Cod, k_text, StringComparison.OrdinalIgnoreCase) OrElse
                                    String.Equals(x.ToString(), k_text, StringComparison.OrdinalIgnoreCase))
                    If k_match Is Nothing Then
                        e.Cancel = True
                        SetStatus($"«{k_text}» nu este o unitate de măsură din listă; alegeți din listă sau scrieți codul (de exemplu XPP).")
                        Return
                    End If
                    e.ProposedValue = k_match
                Case ColContinut
                    Dim k_content As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture)?.Trim()
                    If k_content IsNot Nothing AndAlso k_content.Length > 1000 Then
                        e.Cancel = True
                        SetStatus("Conținutul unei linii poate avea cel mult 1000 de caractere.")
                        Return
                    End If
                    e.ProposedValue = k_content
            End Select
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.GridLinii_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridLinii.CellValueChanged
        Try
            If _loading OrElse _mode = EditMode.Viewing Then Return
            If e.RowIndex >= 0 AndAlso e.RowIndex < gridLinii.RowCount AndAlso IsSpare(gridLinii.Rows(e.RowIndex)) Then
                PromoteSpareLine(gridLinii.Rows(e.RowIndex), e.ColumnKey)
                If IsSpare(gridLinii.Rows(e.RowIndex)) Then Return
            End If
            If e.ColumnKey = ColCant OrElse e.ColumnKey = ColPu Then
                Dim k_row As KBotDataRow = gridLinii.Rows(e.RowIndex)
                k_row(ColValoare) = Math.Round(AsDecimal(k_row(ColCant)) * AsDecimal(k_row(ColPu)), 2, MidpointRounding.AwayFromZero)
                gridLinii.InvalidateRow(e.RowIndex)
            End If
            UpdateTotal()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>The spare row shows no button; its «Continut» carries a hint.</summary>
    Private Sub GridLinii_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridLinii.CellFormatting
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= gridLinii.RowCount Then Return
            If Not IsSpare(gridLinii.Rows(e.RowIndex)) Then Return
            Select Case e.ColumnKey
                Case ColDuplica, ColSterge
                    e.HideButton = True
                Case ColContinut
                    If String.IsNullOrEmpty(e.Text) Then
                        e.Text = "Scrieți aici conținutul unei linii noi…"
                        e.ForeColor = ThemeManager.Current.Palette.TextDimColor
                    End If
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.GridLinii_CellFormatting", ex)
        End Try
    End Sub

    ' The line number after the highest numeric one already there (migrated invoices may carry any text).
    Private Function NextLineNumber() As String
        Dim k_max As Integer = 0
        For k_i As Integer = 0 To RealLineCount() - 1
            Dim k_n As Integer
            If Integer.TryParse(Convert.ToString(gridLinii.Rows(k_i)(ColNr), CultureInfo.InvariantCulture), NumberStyles.None,
                                CultureInfo.InvariantCulture, k_n) Then k_max = Math.Max(k_max, k_n)
        Next
        Return (Math.Max(k_max, RealLineCount()) + 1).ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>A copy of a line goes at the end of the lines (above the spare row) and the cursor opens its «Continut».</summary>
    Private Sub DuplicateLine(k_rowIndex As Integer)
        If Not gridLinii.CommitPendingEdit() Then Return
        Dim k_source As KBotDataRow = gridLinii.Rows(k_rowIndex)
        Dim k_nr As String = NextLineNumber()
        Dim k_content As Object = k_source(ColContinut)
        Dim k_um As Object = k_source(ColUm)
        Dim k_cant As Object = k_source(ColCant)
        Dim k_pu As Object = k_source(ColPu)
        Dim k_value As Object = k_source(ColValoare)
        ' The grid only appends: the spare row steps aside, the copy takes its place, a new spare row follows.
        gridLinii.RemoveRowAt(gridLinii.RowCount - 1)
        Dim k_row As KBotDataRow = gridLinii.AddRow()
        k_row(ColNr) = k_nr
        k_row(ColContinut) = k_content
        k_row(ColUm) = k_um
        k_row(ColCant) = k_cant
        k_row(ColPu) = k_pu
        k_row(ColValoare) = k_value
        k_row(ColDuplica) = DuplicateCaption
        k_row(ColSterge) = DeleteCaption
        k_row.IsDirty = True
        AddSpareRow()
        SetDirty(True)
        UpdateTotal()
        Dim k_index As Integer = gridLinii.RowCount - 2
        gridLinii.EnsureVisible(k_index)
        gridLinii.EditCell(ColContinut, k_index)
    End Sub

    Private Sub GridLinii_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridLinii.ButtonClick
        Try
            If (e.ColumnKey <> ColSterge AndAlso e.ColumnKey <> ColDuplica) OrElse _busy OrElse _mode = EditMode.Viewing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridLinii.RowCount Then Return
            If IsSpare(gridLinii.Rows(e.RowIndex)) Then Return
            If e.ColumnKey = ColDuplica Then
                DuplicateLine(e.RowIndex)
                Return
            End If
            gridLinii.RemoveRowAt(e.RowIndex)
            UpdateTotal()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.GridLinii_ButtonClick", ex)
        End Try
    End Sub

End Class
