Option Strict On
Imports System
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>SLICE-KE20-05: view separat, editabil în propriul designer; logica recepției stă în KeReceptiiView.</summary>
Public Class FacturiCumparareReceptiiAsociateView
    Implements IThemedContainer

    Friend Const ColElimina As String = "elimina"

    Public Sub New()
        InitializeComponent()
    End Sub

    Friend Event RemoveAssociationRequested(k_links As EFacturaKeLegatura())

    Private Sub GridReceptii_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridReceptii.ButtonClick
        If Not String.Equals(e.ColumnKey, ColElimina, StringComparison.Ordinal) Then Return
        If e.RowIndex < 0 OrElse e.RowIndex >= gridReceptii.RowCount Then Return
        Dim k_links As EFacturaKeLegatura() = TryCast(gridReceptii.Rows(e.RowIndex).Tag, EFacturaKeLegatura())
        If k_links IsNot Nothing AndAlso k_links.Length > 0 Then RaiseEvent RemoveAssociationRequested(k_links)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If k_scheme Is Nothing Then Return
        BackColor = k_scheme.PanelBack()
    End Sub
End Class
