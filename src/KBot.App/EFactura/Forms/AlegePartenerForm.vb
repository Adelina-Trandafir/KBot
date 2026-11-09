Option Strict On
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice 00EF-55 -- the operator picks which partner an operation on an invoice is for (sending it to Vercon, booking it in «Contari») when several partners have the
''' supplier's tax code. Hidden partners are listed too: the server makes the chosen one visible again when the operation is done. Opens only when there are several.
''' </summary>
Public Class AlegePartenerForm

    Private Const ColName As String = "denumire"
    Private Const ColAddress As String = "adresa"
    Private Const ColIban As String = "iban"
    Private Const ColState As String = "stare"

    Private ReadOnly _partners As IReadOnlyList(Of EFacturaPartenerCui)
    Private ReadOnly _title As String
    Private ReadOnly _forWhat As String
    Private ReadOnly _cui As String

    Private _selected As EFacturaPartenerCui

    ''' <summary>The partner chosen, set when the form closes with OK; Nothing otherwise.</summary>
    Public ReadOnly Property Selected As EFacturaPartenerCui
        Get
            Return _selected
        End Get
    End Property

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _partners = New List(Of EFacturaPartenerCui)()
        _title = String.Empty
        _forWhat = String.Empty
        _cui = String.Empty
    End Sub

    ''' <param name="k_title">The title of the window, e.g. «Trimite la Vercon».</param>
    ''' <param name="k_forWhat">What the partner is chosen for, in the sentence «Alegeți partenerul pentru …», e.g. «factura X».</param>
    Public Sub New(k_title As String, k_forWhat As String, k_cui As String, k_partners As IReadOnlyList(Of EFacturaPartenerCui))
        ArgumentNullException.ThrowIfNull(k_partners)
        InitializeComponent()
        _partners = k_partners
        _title = If(k_title, String.Empty)
        _forWhat = If(k_forWhat, String.Empty)
        _cui = If(k_cui, String.Empty)
    End Sub

    Private Sub AlegePartenerForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _title.Length > 0 Then
                Text = _title & " — alegerea partenerului"
                capBar.Text = Text
            End If
            lblAntet.Text = $"Mai mulți parteneri au codul fiscal {_cui}. Alegeți partenerul pentru {_forWhat}. Partenerul ales devine activ dacă era ascuns și rămâne legat de acest cod fiscal, deci nu se mai întreabă."
            FillGrid()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow; the list stays empty and «Alege» stays off.
            GlobalErrorLog.Write("AlegePartenerForm.AlegePartenerForm_Load", ex)
        End Try
    End Sub

    Private Sub FillGrid()
        grilaParteneri.BeginUpdate()
        Try
            grilaParteneri.ClearRows()
            For Each k_partner As EFacturaPartenerCui In _partners
                Dim k_row As KBotDataRow = grilaParteneri.AddRow()
                k_row.Tag = k_partner
                k_row(ColName) = k_partner.Denumire
                k_row(ColAddress) = k_partner.Adresa
                k_row(ColIban) = k_partner.ContIban
                k_row(ColState) = If(k_partner.Ascuns, "Ascuns", "Activ")
            Next
        Finally
            grilaParteneri.EndUpdate()
        End Try
        If grilaParteneri.RowCount > 0 Then grilaParteneri.CurrentRowIndex = 0
        btnAlege.Enabled = grilaParteneri.RowCount > 0
    End Sub

    Private Sub btnAlege_Click(sender As Object, e As EventArgs) Handles btnAlege.Click
        Try
            ' The button already carries DialogResult.OK: the form closes after this handler.
            _selected = CurrentPartner()
        Catch ex As Exception
            ' UI boundary (handler): log and swallow.
            GlobalErrorLog.Write("AlegePartenerForm.btnAlege_Click", ex)
        End Try
    End Sub

    Private Sub grilaParteneri_CellDoubleClick(sender As Object, e As KBotCellEventArgs) Handles grilaParteneri.CellDoubleClick
        Try
            If CurrentPartner() Is Nothing Then Return
            btnAlege.PerformClick()
        Catch ex As Exception
            ' UI boundary (handler): log and swallow.
            GlobalErrorLog.Write("AlegePartenerForm.grilaParteneri_CellDoubleClick", ex)
        End Try
    End Sub

    Private Function CurrentPartner() As EFacturaPartenerCui
        Dim k_row As KBotDataRow = grilaParteneri.CurrentRow
        Return If(k_row Is Nothing, Nothing, TryCast(k_row.Tag, EFacturaPartenerCui))
    End Function

End Class
