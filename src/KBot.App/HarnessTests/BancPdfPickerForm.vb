#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' Slice 0078-05, section B bench: the rows of <c>KBOT_BANC_PDF</c> (000_DEMO) -- every upload a
''' bench sent to the server -- to pick one to load. The rows come in already read; the dialog only
''' shows them and says which one was picked (<see cref="Selected"/>).
''' </summary>
Public NotInheritable Class BancPdfPickerForm

    ''' <summary>The row picked with «Încarcă» (or a double click); Nothing otherwise.</summary>
    Public ReadOnly Property Selected As BancPdfRow

    Public Sub New(rows As IEnumerable(Of BancPdfRow))
        InitializeComponent()
        Try
            lstFisiere.BeginUpdate()
            For Each r As BancPdfRow In If(rows, New List(Of BancPdfRow)())
                Dim item As New ListViewItem(r.id.ToString()) With {.Tag = r}
                item.SubItems.Add(r.primit)
                item.SubItems.Add(r.tip)
                item.SubItems.Add(r.id_doc.ToString())
                item.SubItems.Add(r.nume_fisier)
                item.SubItems.Add(r.semnatura)
                item.SubItems.Add(r.dimensiune.ToString("N0"))
                item.SubItems.Add(r.pas)
                item.SubItems.Add(r.operator)
                lstFisiere.Items.Add(item)
            Next
            If lstFisiere.Items.Count > 0 Then lstFisiere.Items(0).Selected = True
        Catch ex As Exception
            GlobalErrorLog.Write("BancPdfPickerForm.New", ex)
            Throw
        Finally
            lstFisiere.EndUpdate()
        End Try
    End Sub

    Private Sub btnIncarca_Click(sender As Object, e As EventArgs) Handles btnIncarca.Click
        Pick()
    End Sub

    Private Sub lstFisiere_DoubleClick(sender As Object, e As EventArgs) Handles lstFisiere.DoubleClick
        Pick()
    End Sub

    Private Sub Pick()
        If lstFisiere.SelectedItems.Count = 0 Then Return
        _Selected = TryCast(lstFisiere.SelectedItems(0).Tag, BancPdfRow)
        If _Selected Is Nothing Then Return
        DialogResult = DialogResult.OK
        Close()
    End Sub

End Class
#End If
