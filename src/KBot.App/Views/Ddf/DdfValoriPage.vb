Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Valori» a vederii DDF: grila liniilor de secțiune A ale nodului selectat.
'''
''' PAGINĂ PARCATĂ (decizie de operator, felia 0025, dusă mai departe în 0032): <c>navSub</c> NU
''' are o intrare cu cheia «valori», deci pagina nu se activează niciodată. Codul rămâne viu și
''' are deja un caz în <c>DdfView.CreatePage</c> — ca s-o repui, adaugi în designerul lui
''' <c>DdfView</c> UN element în <c>navSub.Items</c> cu <c>Key = "valori"</c>. Atât.
'''
''' FILTRUL PE CLASIFICAȚIE A DISPĂRUT (felia 0032): banda <c>pnlFilter</c> cu combo-ul de
''' clasificații nu mai există, fiindcă grila (<see cref="KBotDataView"/>) filtrează singură
''' pe coloană — două mecanisme peste aceleași rânduri erau unul în plus.
''' </summary>
Public Class DdfValoriPage
    Implements IDdfPage, IThemedControl

    ' Cheile coloanelor — trebuie să rămână în acord cu literalele scrise în designer.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_ELEMENT As String = "element"
    Private Const COL_VALPREC As String = "valprec"
    Private Const COL_VALCUR As String = "valcur"
    Private Const COL_VALTOT As String = "valtot"

    ' Reviziile CodAngajament-ului curent — sursa datei folosite la ordonarea pe rădăcină.
    Private _revizii As List(Of RevizieRow)
    ' Nodul curent e o rădăcină de lună? Doar atunci ordonarea are un al doilea criteriu.
    Private _isRoot As Boolean

    ' Evenimentele contractului: pagina «Valori» nu le ridică niciodată (nu are nici suprafață
    ' de generare, nici listă de fișiere). Rămân declarate ca gazda să se poată abona uniform.
    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "valori"
        End Get
    End Property

    ''' <summary>
    ''' Umple grila din contextul primit. <c>Nothing</c> -&gt; grilă goală. Ordonarea e cea din
    ''' felia 0020: pe o rădăcină de lună «Clsf, DataRev» (listă plată peste mai multe revizii),
    ''' pe o frunză doar «Clsf».
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            _revizii = If(ctx Is Nothing, Nothing, ctx.Revizii)
            _isRoot = ctx IsNot Nothing AndAlso ctx.IsRoot
            FillGrid(If(ctx Is Nothing, Nothing, ctx.Linii))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfValoriPage.SetContext", ex)
            Throw
        End Try
    End Sub

    Private Sub FillGrid(rows As List(Of LinieSaRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows IsNot Nothing Then
                For Each r As LinieSaRow In SortRows(rows)
                    Dim row As KBotDataRow = grid.AddRow()
                    row.Tag = r
                    row(COL_CLSF) = r.Clsf
                    row(COL_ELEMENT) = r.ElementFund
                    row(COL_VALPREC) = r.ValPrec
                    row(COL_VALCUR) = r.ValCur
                    row(COL_VALTOT) = r.ValTot
                Next
            End If
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Function SortRows(rows As List(Of LinieSaRow)) As List(Of LinieSaRow)
        If _isRoot Then
            Return rows.OrderBy(Function(r) If(r.Clsf, String.Empty), StringComparer.Ordinal).
                        ThenBy(Function(r) DataRevOf(r.Idrev)).ToList()
        End If
        Return rows.OrderBy(Function(r) If(r.Clsf, String.Empty), StringComparer.Ordinal).ToList()
    End Function

    Private Function RevizieOf(idrev As Integer) As RevizieRow
        If _revizii Is Nothing Then Return Nothing
        For Each r As RevizieRow In _revizii
            If r.Idrev = idrev Then Return r
        Next
        Return Nothing
    End Function

    ' O revizie necunoscută (sau fără dată) se duce la coada ordonării, nu aruncă.
    Private Function DataRevOf(idrev As Integer) As Date
        Dim r As RevizieRow = RevizieOf(idrev)
        If r Is Nothing OrElse Not r.DataRev.HasValue Then Return Date.MaxValue
        Return r.DataRev.Value
    End Function

    ''' <summary>Grila se auto-temează (IThemedControl); aici rămâne doar fundalul paginii.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            BackColor = scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("DdfValoriPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
