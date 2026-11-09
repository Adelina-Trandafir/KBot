Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Controls
Imports KBot.Common
Imports KBot.Domain

' SLICE-KE21-01: the rows of the table of angajamente.
Partial Public Class KeAngajamenteView

    Private Shared Function LabelOf(k_a As EFacturaKeAngajamentAlegere) As String
        Return If(String.IsNullOrWhiteSpace(k_a.Eticheta), k_a.CodAngajament, $"{k_a.CodAngajament} — {k_a.Eticheta}")
    End Function

    ' The choices of the empty row: every angajament the server lists (typing narrows them: the code or the start of the description).
    Private Sub FillChoices()
        _angByLabel.Clear()
        Dim k_labels As New List(Of Object)()
        ' The list offered does not contain the angajamente already in the grid (the ones of the same invoices / supplier); a typed one is still found in _angByLabel.
        Dim k_used As New HashSet(Of String)(_items.Select(Function(k_i) k_i.Cod), StringComparer.OrdinalIgnoreCase)
        For Each k_a As EFacturaKeAngajamentAlegere In _allAng
            Dim k_label As String = LabelOf(k_a)
            If _angByLabel.ContainsKey(k_label) Then Continue For
            _angByLabel(k_label) = k_a
            If Not k_used.Contains(k_a.CodAngajament) Then k_labels.Add(k_label)
        Next
        gridAng.Column(ColAng).ComboItems = k_labels
    End Sub

    ''' <summary>
    ''' The rows: the angajamente the invoices can go to (the server's candidates: DDF of the supplier, links already made) and the ones added here; then ALWAYS an empty row. The angajament
    ''' in use stays the current row. <paramref name="k_sources"/> are the states of the ticked invoices (or of the one pressed, when none is ticked); <paramref name="k_hasContext"/> says that
    ''' there is such an invoice at all.
    ''' </summary>
    Public Sub Fill(k_sources As IReadOnlyList(Of EFacturaKeStare), k_hasContext As Boolean)
        ArgumentNullException.ThrowIfNull(k_sources)
        _viewMode = False
        Dim k_items As New List(Of AngItem)()
        Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        ' The view of an angajament: that angajament and no other.
        If _fixedAng.Length > 0 Then
            k_items.Add(New AngItem() With {.Cod = _fixedAng, .Sursa = "fix"})
            k_seen.Add(_fixedAng)
        End If
        ' Slice KE14: an angajament a reception of these invoices is linked to cannot be taken off the list (its button is hidden).
        Dim k_linked As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_state As EFacturaKeStare In k_sources
            For Each k_link As EFacturaKeLegatura In k_state.Legaturi
                k_linked.Add(k_link.CodAngajament)
            Next
        Next
        If _fixedAng.Length = 0 Then
            For Each k_state As EFacturaKeStare In k_sources
                For Each k_a As EFacturaKeAngajament In k_state.Angajamente
                    If k_seen.Add(k_a.CodAngajament) Then
                        k_items.Add(New AngItem() With {.Cod = k_a.CodAngajament, .Descriere = k_a.Descriere, .Sursa = k_a.Sursa})
                    End If
                Next
            Next
        End If
        For Each k_a As AngItem In k_items
            k_a.Linked = k_linked.Contains(k_a.Cod)
        Next
        _items = k_items
        FillChoices()
        _angFilling = True
        gridAng.BeginUpdate()
        Try
            gridAng.ClearRows()
            For Each k_a As AngItem In k_items
                Dim k_row As KBotDataRow = gridAng.AddRow()
                k_row.Tag = k_a
                ' In a view of the invoices the code of the angajament is not shown: only its description (the code when it has none).
                k_row(ColAng) = If(_shortLabels AndAlso Not String.IsNullOrWhiteSpace(k_a.Descriere), k_a.Descriere, k_a.Eticheta)
            Next
            Dim k_empty As KBotDataRow = gridAng.AddRow()
            k_empty.Tag = Nothing
            k_empty(ColAng) = String.Empty
        Finally
            gridAng.EndUpdate()
            _angFilling = False
        End Try
        Dim k_index As Integer = k_items.FindIndex(Function(k_a) String.Equals(k_a.Cod, _angCod, StringComparison.OrdinalIgnoreCase))
        If k_index < 0 AndAlso k_items.Count = 1 Then k_index = 0
        If k_index >= 0 Then
            gridAng.CurrentRowIndex = k_index
            BeginUseAngajament(k_items(k_index).Cod)
        ElseIf k_hasContext Then
            ' The angajament in use is no longer offered (its invoice was unticked, or another supplier was pressed): nothing is chosen.
            _angCod = String.Empty
            _data = Nothing
            RaiseEvent AngajamentCleared(Me, EventArgs.Empty)
        End If
        BeginLoadOthers()
    End Sub

    ''' <summary>The angajamente of the links of the invoice only a look is taken at (no empty row, no button).</summary>
    Public Sub FillViewed(k_viewState As EFacturaKeStare)
        _viewMode = True
        Dim k_items As New List(Of AngItem)()
        If k_viewState IsNot Nothing Then
            For Each k_link As EFacturaKeLegatura In k_viewState.Legaturi
                If k_items.Any(Function(k_a) String.Equals(k_a.Cod, k_link.CodAngajament, StringComparison.OrdinalIgnoreCase)) Then Continue For
                Dim k_known As EFacturaKeAngajament = k_viewState.Angajamente.FirstOrDefault(
                    Function(k_a) String.Equals(k_a.CodAngajament, k_link.CodAngajament, StringComparison.OrdinalIgnoreCase))
                Dim k_description As String = If(k_known Is Nothing, String.Empty, k_known.Descriere)
                If String.IsNullOrWhiteSpace(k_description) Then
                    Dim k_choice As EFacturaKeAngajamentAlegere = _allAng.FirstOrDefault(
                        Function(k_a) String.Equals(k_a.CodAngajament, k_link.CodAngajament, StringComparison.OrdinalIgnoreCase))
                    If k_choice IsNot Nothing Then k_description = k_choice.Descriere
                End If
                k_items.Add(New AngItem() With {.Cod = k_link.CodAngajament, .Descriere = k_description, .Sursa = "legatura", .Linked = True})
            Next
        End If
        _items = k_items
        _angFilling = True
        gridAng.BeginUpdate()
        Try
            gridAng.ClearRows()
            For Each k_a As AngItem In k_items
                Dim k_row As KBotDataRow = gridAng.AddRow()
                k_row.Tag = k_a
                k_row(ColAng) = k_a.Eticheta
            Next
        Finally
            gridAng.EndUpdate()
            _angFilling = False
        End Try
    End Sub

End Class
