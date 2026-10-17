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
''' Slice 00EF-41 -- what the received-invoices view shows on the right when the ROOT «Toate facturile» of its tree is chosen (the pattern of the DDF
''' and ORD views): a grid with every invoice the tree holds, one tick per row, and two actions on the TICKED rows in the footer: «Listează» (the invoices
''' are made ready and sent to the printer without being opened) and «Salvează XML» (the XML files, in a folder the operator picks). The last column of
''' every row is a button «…» whose menu offers the same two actions for THAT row only.
''' The page only OWNS its controls (declared in its designer file) and tells the view what was asked through its events; the view does the work.
''' </summary>
Public Class PrimiteListaPage
    Implements IThemedContainer

    Private Const ColSel As String = "sel"
    Private Const ColData As String = "data"
    Private Const ColNr As String = "nr"
    Private Const ColReceptie As String = "receptie"
    Private Const ColScad As String = "scad"
    Private Const ColTotal As String = "total"
    Private Const ColOpt As String = "opt"

    Private Const MenuList As String = "listeaza"
    Private Const MenuXml As String = "xml"

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    Private _items As New List(Of EFacturaPrimita)()
    ' An action is running: the grid and the buttons are locked.
    Private _busy As Boolean
    ' The page writes cells itself: its own writes must not be taken for the operator's.
    Private _suppress As Boolean

    ''' <summary>The operator wants these invoices listed (printed): the ticked ones, or the one of a row.</summary>
    Public Event ListRequested(k_items As List(Of EFacturaPrimita))

    ''' <summary>The operator wants the XML of these invoices saved: the ticked ones, or the one of a row.</summary>
    Public Event SaveXmlRequested(k_items As List(Of EFacturaPrimita))

    Public Sub New()
        InitializeComponent()
        UpdateButtons()
    End Sub

    ''' <summary>Replaces the rows. Nothing is ticked afterwards.</summary>
    Public Sub SetItems(k_items As IEnumerable(Of EFacturaPrimita))
        Try
            _items = If(k_items Is Nothing, New List(Of EFacturaPrimita)(), k_items.ToList())
            _suppress = True
            grila.BeginUpdate()
            Try
                grila.ClearRows()
                For Each k_p As EFacturaPrimita In _items
                    Dim k_row As KBotDataRow = grila.AddRow()
                    k_row.Tag = k_p
                    k_row(ColSel) = False
                    k_row(ColData) = If(k_p.DataFact.HasValue, k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
                    k_row(ColNr) = If(k_p.Tip = "NC", "NC ", String.Empty) & k_p.NrFact
                    k_row(ColReceptie) = ReceptionText(k_p)
                    k_row(ColScad) = If(k_p.DataScad.HasValue, k_p.DataScad.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                    k_row(ColTotal) = CDbl(k_p.Total)
                Next
            Finally
                grila.EndUpdate()
                _suppress = False
            End Try
            UpdateButtons()
            lblStatus.Text = StatusText()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.SetItems", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The reception(s) of an invoice in ONE cell: date, description and value of each («15.09.2026 · plata factura 12/01.09.2026 · 3.063,72»); several
    ''' receptions are separated by « | ». Empty when the invoice is not linked to any.
    ''' </summary>
    Private Shared Function ReceptionText(k_p As EFacturaPrimita) As String
        Dim k_parts As New List(Of String)()
        For Each k_r As EFacturaPrimitaReceptie In k_p.Receptii
            If k_r.AsteaptaDescarcarea Then
                k_parts.Add($"așteaptă descărcarea din FOREXE · {k_r.Suma.ToString("N2", _ro)}")
                Continue For
            End If
            Dim k_fields As New List(Of String)()
            If k_r.DataR.HasValue Then k_fields.Add(k_r.DataR.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))
            If Not String.IsNullOrWhiteSpace(k_r.Descriere) Then k_fields.Add(k_r.Descriere)
            k_fields.Add(k_r.Suma.ToString("N2", _ro))
            k_parts.Add(String.Join(" · ", k_fields))
        Next
        Return String.Join("  |  ", k_parts)
    End Function

    ''' <summary>The actions are off (and the grid read-only) while one runs, so nothing is sent twice.</summary>
    Public Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        grila.ReadOnlyGrid = k_busy
        UpdateButtons()
    End Sub

    ''' <summary>Replaces the line at the bottom (progress of an action); empty = the count of ticked invoices.</summary>
    Public Sub SetStatus(k_text As String)
        lblStatus.Text = If(String.IsNullOrEmpty(k_text), StatusText(), k_text)
    End Sub

    ' ── Selection ───────────────────────────────────────────────────────────────

    Private Shared Function IsTicked(k_value As Object) As Boolean
        Return TypeOf k_value Is Boolean AndAlso CBool(k_value)
    End Function

    Private Function TickedItems() As List(Of EFacturaPrimita)
        Dim k_result As New List(Of EFacturaPrimita)()
        For k_i As Integer = 0 To grila.RowCount - 1
            Dim k_row As KBotDataRow = grila.Rows(k_i)
            Dim k_p As EFacturaPrimita = TryCast(k_row.Tag, EFacturaPrimita)
            If k_p IsNot Nothing AndAlso IsTicked(k_row(ColSel)) Then k_result.Add(k_p)
        Next
        Return k_result
    End Function

    Private Function StatusText() As String
        If _items.Count = 0 Then Return "Nu sunt facturi de afișat."
        Dim k_n As Integer = TickedItems().Count
        If k_n = 0 Then Return "Nimic bifat."
        Return $"{k_n} din {_items.Count} facturi bifate."
    End Function

    Private Sub UpdateButtons()
        Dim k_any As Boolean = Not _busy AndAlso TickedItems().Count > 0
        btnListeaza.Enabled = k_any
        btnXml.Enabled = k_any
    End Sub

    ' The icon in the header of the tick column: ticks every row, or unticks them all when they are all ticked already.
    Private Sub Grila_HeaderRightIconClicked(sender As Object, e As KBotColumnEventArgs) Handles grila.HeaderRightIconClicked
        Try
            If _busy OrElse Not String.Equals(e.ColumnKey, ColSel, StringComparison.Ordinal) OrElse grila.RowCount = 0 Then Return
            Dim k_target As Boolean = TickedItems().Count < grila.RowCount
            _suppress = True
            grila.BeginUpdate()
            Try
                For k_i As Integer = 0 To grila.RowCount - 1
                    grila.Rows(k_i)(ColSel) = k_target
                Next
            Finally
                grila.EndUpdate()
                _suppress = False
            End Try
            UpdateButtons()
            lblStatus.Text = StatusText()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.Grila_HeaderRightIconClicked", ex)
        End Try
    End Sub

    Private Sub Grila_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grila.CellValueChanged
        Try
            If _suppress OrElse Not String.Equals(e.ColumnKey, ColSel, StringComparison.Ordinal) Then Return
            UpdateButtons()
            lblStatus.Text = StatusText()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.Grila_CellValueChanged", ex)
        End Try
    End Sub

    ' ── The actions ─────────────────────────────────────────────────────────────

    ' UI boundary for the handlers below: they only raise the page's events; the work is the view's.
    Private Sub BtnListeaza_Click(sender As Object, e As EventArgs) Handles btnListeaza.Click
        Try
            Dim k_ticked As List(Of EFacturaPrimita) = TickedItems()
            If k_ticked.Count > 0 AndAlso Not _busy Then RaiseEvent ListRequested(k_ticked)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.BtnListeaza_Click", ex)
        End Try
    End Sub

    Private Sub BtnXml_Click(sender As Object, e As EventArgs) Handles btnXml.Click
        Try
            Dim k_ticked As List(Of EFacturaPrimita) = TickedItems()
            If k_ticked.Count > 0 AndAlso Not _busy Then RaiseEvent SaveXmlRequested(k_ticked)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.BtnXml_Click", ex)
        End Try
    End Sub

    ' The button «…» of a row: a menu with the two actions, for that row only (whatever is ticked).
    Private Sub Grila_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles grila.ButtonClick
        Try
            If _busy OrElse Not String.Equals(e.ColumnKey, ColOpt, StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= grila.RowCount Then Return
            Dim k_p As EFacturaPrimita = TryCast(grila.Rows(e.RowIndex).Tag, EFacturaPrimita)
            If k_p Is Nothing Then Return
            Dim k_menu As New CustomPopup(New List(Of CustomPopupItem) From {
                New CustomPopupItem(MenuList, "&Listează"),
                New CustomPopupItem(MenuXml, "Salvează &XML")})
            AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) RowActionChosen(k_args.Item.Key, k_p)
            k_menu.ShowAt(grila, Control.MousePosition)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.Grila_ButtonClick", ex)
        End Try
    End Sub

    Private Sub RowActionChosen(k_key As String, k_p As EFacturaPrimita)
        Try
            Select Case k_key
                Case MenuList : RaiseEvent ListRequested(New List(Of EFacturaPrimita) From {k_p})
                Case MenuXml : RaiseEvent SaveXmlRequested(New List(Of EFacturaPrimita) From {k_p})
                Case Else
                    ' No silent no-ops: an unknown key is a programming defect.
                    Throw New ArgumentException($"Comandă de meniu necunoscută: {k_key}", NameOf(k_key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.RowActionChosen", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            pnlJos.BackColor = k_scheme.Palette.SurfaceAltColor
            pnlGap.BackColor = k_scheme.Palette.SurfaceAltColor
            lblStatus.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteListaPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
