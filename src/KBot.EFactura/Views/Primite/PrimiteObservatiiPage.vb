Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 00EF-25 -- the view «Observații» of a received invoice: the notes the supplier wrote in the XML (they used to share the view
''' «Mesaje»). One column, no date; some notes are very long, so under the list a read-only rich text box shows the WHOLE note of the
''' selected row. The page owns its controls; <see cref="PrimiteView"/> hands it the notes through <see cref="SetNotes"/>.
''' </summary>
Public Class PrimiteObservatiiPage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
        rtbObservatie.Editabil = False
    End Sub

    ''' <summary>
    ''' A note as it is shown: the empty lines are dropped (a line that is only spaces too) and each line is trimmed, so the text is
    ''' one block (operator, 15.10.2026). A note with nothing in it gives an empty string.
    ''' </summary>
    Friend Shared Function CleanNote(k_note As String) As String
        If String.IsNullOrWhiteSpace(k_note) Then Return String.Empty
        Dim k_lines As IEnumerable(Of String) = k_note.Split({ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries).
            Select(Function(k_l) k_l.Trim()).Where(Function(k_l) k_l.Length > 0)
        Return String.Join(vbCrLf, k_lines)
    End Function

    ''' <summary>The notes of an invoice with the blank ones left out and the empty lines of each one dropped.</summary>
    Friend Shared Function CleanNotes(k_notes As IEnumerable(Of String)) As List(Of String)
        Dim k_result As New List(Of String)()
        If k_notes Is Nothing Then Return k_result
        For Each k_note As String In k_notes
            Dim k_clean As String = CleanNote(k_note)
            If k_clean.Length > 0 Then k_result.Add(k_clean)
        Next
        Return k_result
    End Function

    ''' <summary>Shows the notes of the invoice on screen (Nothing or empty = none); the first one is selected.</summary>
    Public Sub SetNotes(k_notes As IEnumerable(Of String))
        gridObservatii.BeginUpdate()
        Try
            gridObservatii.ClearRows()
            If k_notes IsNot Nothing Then
                For Each k_note As String In CleanNotes(k_notes)
                    Dim k_row As KBotDataRow = gridObservatii.AddRow()
                    k_row.Tag = k_note
                    k_row("text") = FirstLine(k_note)
                Next
            End If
        Finally
            gridObservatii.EndUpdate()
        End Try
        ShowSelected()
    End Sub

    ' The grid cell holds one line; the whole note is in the box below.
    Private Shared Function FirstLine(k_note As String) As String
        If String.IsNullOrEmpty(k_note) Then Return String.Empty
        Dim k_text As String = k_note.Replace(vbCr, " ").Replace(vbLf, " ").Trim()
        Return k_text
    End Function

    ''' <summary>UI boundary: logs and swallows.</summary>
    Private Sub GridObservatii_SelectionChanged(sender As Object, e As EventArgs) Handles gridObservatii.SelectionChanged
        Try
            ShowSelected()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteObservatiiPage.GridObservatii_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub ShowSelected()
        Dim k_index As Integer = gridObservatii.CurrentRowIndex
        If k_index < 0 AndAlso gridObservatii.RowCount > 0 Then k_index = 0
        Dim k_full As String = String.Empty
        If k_index >= 0 AndAlso k_index < gridObservatii.RowCount Then
            k_full = TryCast(gridObservatii.Rows(k_index).Tag, String)
        End If
        rtbObservatie.Rtf = k_full
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteObservatiiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
