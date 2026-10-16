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
''' line with the total, what is allocated and the rest; under it the links, one per reception and classification; at the bottom the buttons of the accountant:
''' the classifications of the lines, link to an existing reception, a new reception, modify a reception, remove a link. Anyone else sees the same
''' figures and no buttons. The page only OWNS its controls (declared in its designer file); what it shows and what the buttons do are the work of
''' <see cref="PrimiteView"/>, which the page tells through its events.
''' </summary>
Public Class PrimiteReceptiiPage
    Implements IThemedContainer

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    ''' <summary>The accountant wants to choose the classification of each line.</summary>
    Public Event ClassificationsRequested()

    ''' <summary>The accountant wants a link of this kind: one of <see cref="EFacturaKeTip"/>.</summary>
    Public Event LinkRequested(k_tip As String)

    ''' <summary>The accountant wants to remove a link row (its <c>IdLegatura</c>).</summary>
    Public Event RemoveRequested(k_idLegatura As Integer)

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
            k_notes.Add("Unele linii nu au clasificație: alegeți-o din «Clasificații pe linii».")
        End If
        If k_stare.NealocatLiniilor <> 0D Then
            k_notes.Add($"Din total, {Money(k_stare.NealocatLiniilor)} nu se explică prin linii (o reducere sau un cost pe toată factura); nu se alocă pe linii.")
        End If
        If Not k_stare.PoateScrie Then
            k_notes.Add("Numai un utilizator cu rolul «Contabil» poate lega facturile de recepții.")
        ElseIf Not k_robot Then
            k_notes.Add("Robotul FOREXE nu este disponibil: se poate lega doar de o recepție existentă.")
        End If
        lblNota.Text = String.Join("  ", k_notes)
        pnlButoane.Visible = k_stare.PoateScrie
        btnNoua.Enabled = k_robot
        btnModifica.Enabled = k_robot

        gridLegaturi.BeginUpdate()
        Try
            gridLegaturi.ClearRows()
            For Each k_l As EFacturaKeLegatura In k_stare.Legaturi
                Dim k_row As KBotDataRow = gridLegaturi.AddRow()
                k_row.Tag = k_l
                k_row("angajament") = k_l.CodAngajament
                k_row("clsf") = k_l.Clsf
                k_row("receptie") = ReceptionText(k_l)
                k_row("tip") = KindWords(k_l.Tip)
                k_row("suma") = CDbl(k_l.Suma)
            Next
        Finally
            gridLegaturi.EndUpdate()
        End Try
        btnScoate.Enabled = False
    End Sub

    ''' <summary>Says why the links could not be read; the table is emptied and the buttons are off.</summary>
    Public Sub ShowProblem(k_text As String)
        lblRezumat.Text = k_text
        lblNota.Text = String.Empty
        gridLegaturi.ClearRows()
        pnlButoane.Visible = False
    End Sub

    ''' <summary>Empties the page (no invoice is shown).</summary>
    Public Sub Clear()
        lblRezumat.Text = String.Empty
        lblNota.Text = String.Empty
        gridLegaturi.ClearRows()
        pnlButoane.Visible = False
    End Sub

    ''' <summary>The buttons are off while a reception is being written in FOREXE, so nothing is sent twice.</summary>
    Public Sub SetBusy(k_busy As Boolean)
        pnlButoane.Enabled = Not k_busy
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

    Private Shared Function KindWords(k_tip As String) As String
        Select Case k_tip
            Case EFacturaKeTip.Noua : Return "recepție nouă"
            Case EFacturaKeTip.Modificata : Return "recepție modificată"
            Case Else : Return "recepție existentă"
        End Select
    End Function

    Private Shared Function ReceptionText(k_l As EFacturaKeLegatura) As String
        If k_l.AsteaptaDescarcarea Then Return "așteaptă descărcarea din FOREXE"
        Dim k_parts As New List(Of String)()
        If k_l.DataR.HasValue Then k_parts.Add(k_l.DataR.Value.ToString("dd.MM.yyyy", _ro))
        If Not String.IsNullOrWhiteSpace(k_l.TipReceptie) Then k_parts.Add(k_l.TipReceptie)
        If Not String.IsNullOrWhiteSpace(k_l.DescriereR) Then k_parts.Add(k_l.DescriereR)
        Dim k_text As String = String.Join(" · ", k_parts)
        Return If(k_l.StearsaPeSite, k_text & " (ștearsă în FOREXE)", k_text)
    End Function

    Private Sub GridLegaturi_SelectionChanged(sender As Object, e As EventArgs) Handles gridLegaturi.SelectionChanged
        Try
            btnScoate.Enabled = TryCast(gridLegaturi.CurrentRow?.Tag, EFacturaKeLegatura) IsNot Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.GridLegaturi_SelectionChanged", ex)
        End Try
    End Sub

    ' UI boundary for the five handlers below: they only raise the page's events; the work is the view's.
    Private Sub BtnClasificatii_Click(sender As Object, e As EventArgs) Handles btnClasificatii.Click
        Try
            RaiseEvent ClassificationsRequested()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.BtnClasificatii_Click", ex)
        End Try
    End Sub

    Private Sub BtnExistenta_Click(sender As Object, e As EventArgs) Handles btnExistenta.Click
        Try
            RaiseEvent LinkRequested(EFacturaKeTip.Existenta)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.BtnExistenta_Click", ex)
        End Try
    End Sub

    Private Sub BtnNoua_Click(sender As Object, e As EventArgs) Handles btnNoua.Click
        Try
            RaiseEvent LinkRequested(EFacturaKeTip.Noua)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.BtnNoua_Click", ex)
        End Try
    End Sub

    Private Sub BtnModifica_Click(sender As Object, e As EventArgs) Handles btnModifica.Click
        Try
            RaiseEvent LinkRequested(EFacturaKeTip.Modificata)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.BtnModifica_Click", ex)
        End Try
    End Sub

    Private Sub BtnScoate_Click(sender As Object, e As EventArgs) Handles btnScoate.Click
        Try
            Dim k_l As EFacturaKeLegatura = TryCast(gridLegaturi.CurrentRow?.Tag, EFacturaKeLegatura)
            If k_l IsNot Nothing Then RaiseEvent RemoveRequested(k_l.IdLegatura)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.BtnScoate_Click", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            pnlButoane.BackColor = k_scheme.Palette.SurfaceAltColor
            lblNota.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteReceptiiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
