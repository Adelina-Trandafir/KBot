Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slices KE03..KE07 -- the view «Receptii» of a received invoice: how much of the invoice (with VAT) receptions already cover, and the links. Above, one
''' line with the total, what is allocated and the rest; under it the links, one per reception and classification; nothing below it (slice KE12: the buttons that linked an invoice, made or
''' modified a reception and removed a link are gone for now; the work is done in the page «Recepții» of the E-Factura ribbon). Slice KE12-04: the table of links is gone too;
''' under the summary the page hosts the reception form of the shown invoice (<see cref="KeLegaForm"/> in its embedded variant, without the choice of the invoice and,
''' in the view of an angajament, without the choice of the angajament). The page only OWNS its controls (declared in its designer file); what it shows is the work of <see cref="PrimiteView"/>.
''' </summary>
Public Class PrimiteReceptiiPage
    Implements IThemedContainer

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Puts the state of the invoice on screen. <paramref name="k_robot"/> = the shell's FOREXE robot is wired (a new or changed reception needs it).</summary>
    Public Sub ShowState(k_stare As EFacturaKeStare, k_robot As Boolean)
        ArgumentNullException.ThrowIfNull(k_stare)
        Dim k_text As String = StateWords(k_stare.Stare)
        lblRezumat.Text = $"Total factură (cu TVA): {Money(k_stare.Total)}   ·   alocat pe recepții: {Money(k_stare.Alocat)}   ·   " &
                          $"rest: {Money(k_stare.Rest)}   ·   {k_text}"
        Dim k_notes As New List(Of String)()
        If k_stare.FaraClasificatie <> 0D OrElse k_stare.Linii.Any(Function(k_l) Not k_l.IdClsf.HasValue) Then
            k_notes.Add("Unele linii nu au clasificație: se alege în pagina «Recepții».")
        End If
        If k_stare.NealocatLiniilor <> 0D Then
            k_notes.Add($"Din total, {Money(k_stare.NealocatLiniilor)} nu se explică prin linii (o reducere sau un cost pe toată factura); nu se alocă pe linii.")
        End If
        If Not k_stare.PoateScrie Then
            k_notes.Add("Numai un utilizator cu rolul «Contabil» poate lega facturile de recepții.")
        End If
        lblNota.Text = String.Join("  ", k_notes)
        lblNota.Visible = lblNota.Text.Length > 0   ' an empty note must not leave a blank band under the form
    End Sub

    ''' <summary>Puts the form in the page, once (it fills the space under the summary).</summary>
    Public Sub Embed(k_form As Form)
        ArgumentNullException.ThrowIfNull(k_form)
        k_form.Dock = DockStyle.Fill
        pnlForma.Controls.Add(k_form)
        k_form.Show()
    End Sub

    ''' <summary>Says why the links could not be read; the table is emptied and the buttons are off.</summary>
    Public Sub ShowProblem(k_text As String)
        lblRezumat.Text = k_text
        lblNota.Text = String.Empty
        lblNota.Visible = False
    End Sub

    ''' <summary>Empties the page (no invoice is shown).</summary>
    Public Sub Clear()
        lblRezumat.Text = String.Empty
        lblNota.Text = String.Empty
        lblNota.Visible = False
    End Sub

    Private Shared Function Money(k_value As Decimal) As String
        Return k_value.ToString("N2", _ro)
    End Function

    Private Shared Function StateWords(k_state As String) As String
        Select Case k_state
            Case "acoperita" : Return "acoperită"
            Case "partiala" : Return "acoperită parțial"
            Case "depasita" : Return "alocarea depășește factura"
            Case Else : Return "nelegată de recepții"
        End Select
    End Function

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            lblNota.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
