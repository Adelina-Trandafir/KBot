Option Strict On
Imports System.Collections.Generic
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Vizualizare» a vederii ORD (felia 0033): liniile <c>FX_ORD_TBL</c> ale
''' ordonanțării selectate, într-o grilă <see cref="KBotDataView"/>.
'''
''' E analogul paginii «Vizualizare» a DDF-ului — conținutul documentului reconstruit — dar
''' o GRILĂ adevărată, nu o reconstrucție XFA: fără generare nu există XML de reconstruit,
''' iar liniile sunt oricum tabelare.
'''
''' Lista e PLATĂ: gruparea pe beneficiar (<c>FX_ORD_PART</c>) e o felie ulterioară, iar
''' ordinea e cea trimisă de server (IDORDP, Clsf, IDORDTBLP) — pagina nu re-sortează.
''' </summary>
Public Class OrdVizualizarePage
    Implements IOrdPage, IThemedControl

    ' Cheile coloanelor — trebuie să rămână în acord cu literalele scrise în designer.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_DESCRIERE As String = "descriere"
    Private Const COL_TOTAL_RECEPTII As String = "total_receptii"
    Private Const COL_PLATI_ANT As String = "plati_ant"
    Private Const COL_VALOARE As String = "valoare"
    Private Const COL_RAMAS As String = "ramas"

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IOrdPage.PageKey
        Get
            Return "vizualizare"
        End Get
    End Property

    ''' <summary>
    ''' Umple grila din contextul primit. <c>Nothing</c> -&gt; grilă goală. O rădăcină de lună
    ''' ajunge aici cu liniile TUTUROR ordonanțărilor lunii — o listă plată e exact ce arată
    ''' o lună, deci nu e un caz separat.
    ''' </summary>
    Public Sub SetContext(ctx As OrdPageContext) Implements IOrdPage.SetContext
        Try
            FillGrid(If(ctx Is Nothing, Nothing, ctx.Linii))
        Catch ex As Exception
            GlobalErrorLog.Write("OrdVizualizarePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' BeginUpdate/EndUpdate: o singură repictare la final, nu una per rând.
    Private Sub FillGrid(rows As List(Of OrdLinieRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows IsNot Nothing Then
                For Each r As OrdLinieRow In rows
                    Dim row As KBotDataRow = grid.AddRow()
                    row.Tag = r
                    row(COL_CLSF) = r.Clsf
                    row(COL_DESCRIERE) = r.Descriere
                    row(COL_TOTAL_RECEPTII) = r.TotalReceptii
                    row(COL_PLATI_ANT) = r.PlatiAnt
                    row(COL_VALOARE) = r.Valoare
                    row(COL_RAMAS) = r.Ramas
                Next
            End If
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    ''' <summary>Grila se auto-temează (IThemedControl); aici rămâne doar fundalul paginii.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            BackColor = scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("OrdVizualizarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
