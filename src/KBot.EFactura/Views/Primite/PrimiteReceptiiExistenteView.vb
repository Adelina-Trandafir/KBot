Option Strict On
Imports System
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>SLICE-KE20-05: view separat, editabil în propriul designer; logica recepției stă în KeReceptiiView.</summary>
Public Class PrimiteReceptiiExistenteView
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Event ActionChanged As EventHandler
    Public Event ReceptionSelectionChanged As EventHandler

    Private Sub CmbMod_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbMod.SelectedIndexChanged
        RaiseEvent ActionChanged(Me, e)
    End Sub

    Private Sub GridReceptii_SelectionChanged(sender As Object, e As EventArgs) Handles gridReceptii.SelectionChanged
        RaiseEvent ReceptionSelectionChanged(Me, e)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If k_scheme Is Nothing Then Return
        BackColor = k_scheme.PanelBack()
        pnlMod.BackColor = BackColor
    End Sub
End Class
