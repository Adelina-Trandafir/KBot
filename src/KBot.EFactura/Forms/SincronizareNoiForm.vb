Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice 00EF-32 -- the result of «Sincronizează facturi primite» when invoices were brought: a message window (the look of the K-BOT
''' dialogs: caption bar, theme) with a table of the NEW invoices (date, number, supplier, tax code, total) and one «OK». It only shows
''' what it is given; the sync and the list of what is new are the work of <see cref="PrimiteView"/>. When nothing new came, no window:
''' that case is an ordinary info message (<c>KBotMessage</c>).
''' </summary>
Public Class SincronizareNoiForm

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="k_invoices">The invoices that were not in the list before the sync (at least one).</param>
    Public Sub New(k_invoices As IEnumerable(Of EFacturaPrimita))
        ArgumentNullException.ThrowIfNull(k_invoices)
        InitializeComponent()
        Dim k_list As List(Of EFacturaPrimita) = k_invoices.
            OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita).ToList()
        lblAntet.Text = If(k_list.Count = 1, "S-a descărcat o factură nouă de la ANAF:",
                           $"S-au descărcat {k_list.Count.ToString(CultureInfo.CurrentCulture)} facturi noi de la ANAF:")
        gridNoi.BeginUpdate()
        Try
            gridNoi.ClearRows()
            For Each k_p As EFacturaPrimita In k_list
                Dim k_row As KBotDataRow = gridNoi.AddRow()
                k_row.Tag = k_p
                k_row("data") = If(k_p.DataFact.HasValue, k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_row("numar") = $"{If(k_p.Tip = "NC", "NC ", String.Empty)}{k_p.NrFact}"
                k_row("furnizor") = k_p.Furnizor
                k_row("cui") = k_p.Cui
                k_row("total") = k_p.Total
            Next
        Finally
            gridNoi.EndUpdate()
        End Try
    End Sub

End Class
