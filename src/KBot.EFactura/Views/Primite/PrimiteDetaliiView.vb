Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>SLICE-02EF-14: panoul comun al facturii selectate, cu bara de navigare și paginile ei.</summary>
Public Class PrimiteDetaliiView
    Implements IThemedContainer

    Friend Const ViewContare As String = "contare"
    Friend Const ViewLinii As String = "linii"
    Friend Const ViewPdf As String = "pdf"
    Friend Const ViewMesaje As String = "mesaje"
    Friend Const ViewReceptii As String = "receptii"
    Friend Const ViewXml As String = "xml"

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    Public Sub New()
        InitializeComponent()
    End Sub

    Friend Sub ShowPage(k_key As String)
        pgContare.Visible = String.Equals(k_key, ViewContare, StringComparison.Ordinal)
        pgLinii.Visible = String.Equals(k_key, ViewLinii, StringComparison.Ordinal)
        pgPdf.Visible = String.Equals(k_key, ViewPdf, StringComparison.Ordinal)
        pgMesaje.Visible = String.Equals(k_key, ViewMesaje, StringComparison.Ordinal)
        pgXml.Visible = String.Equals(k_key, ViewXml, StringComparison.Ordinal)
    End Sub

    ''' <summary>Slice KE21: while the view «Recepții» is on screen its layout has a place for the bar of the pages (over the details and the data of the reception): the bar stands there.</summary>
    Friend Sub HostNavBar(k_place As Control)
        ArgumentNullException.ThrowIfNull(k_place)
        k_place.Controls.Add(pnlNav)
        pnlNav.Dock = DockStyle.Fill
    End Sub

    ''' <summary>The bar of the pages comes back over its pages.</summary>
    Friend Sub TakeNavBack()
        If pnlNav.Parent Is Me Then Return
        Controls.Add(pnlNav)
        pnlNav.Dock = DockStyle.Top
    End Sub


    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteDetaliiView.ApplyTheme", ex)
        End Try
    End Sub

End Class
