Option Strict On
Imports System
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>SLICE-KE20-05: view separat, editabil în propriul designer; logica recepției stă în KeReceptiiView.</summary>
Public Class FacturiCumparareReceptieNouaView
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Event ReceptionDateChanged As EventHandler

    Private Sub DtpData_ValueChanged(sender As Object, e As EventArgs) Handles dtpData.ValueChanged
        RaiseEvent ReceptionDateChanged(Me, e)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If k_scheme Is Nothing Then Return
        BackColor = k_scheme.PanelBack()
        tlyNoua.BackColor = BackColor
    End Sub
End Class
