Option Strict On
Imports System.Collections.Generic
Imports System.Linq
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
''' TREI benzi, de sus în jos:
'''   * ANTETUL — filtrul pe beneficiar (<c>cboBeneficiar</c>). Lista e PLATĂ (serverul nu
'''     grupează pe <c>FX_ORD_PART</c>), dar fiecare linie își poartă beneficiarul, deci
'''     gruparea se face aici, ca ALEGERE a operatorului, nu ca structură impusă. Prima
'''     intrare e mereu «&lt;TOȚI BENEFICIARII&gt;» — perechea lui <c>lstDenBene</c> din
'''     Access (<c>frmFX_ORD_PART</c>), care are exact același rând sintetic.
'''   * GRILA — liniile care trec de filtru, în ordinea trimisă de server (IDORDP, DenBene,
'''     Clsf, IDORDTBLP); pagina nu re-sortează.
'''   * SUBSOLUL — datele beneficiarului RÂNDULUI CURENT (nume, cod fiscal, IBAN, documente
'''     justificative, obiectul DDF-ului). Se rescrie la fiecare schimbare de selecție în
'''     grilă, fiindcă un nod de lună poate amesteca ordonanțări cu beneficiari diferiți:
'''     singurul lucru care spune cui se plătește ACUM e rândul pe care stă cursorul.
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

    ''' <summary>Prima intrare a filtrului — «fără filtru», nu un beneficiar. E un LITERAL,
    ''' nu un nume care ar putea veni din date, deci comparația pe el e sigură.</summary>
    Private Const TOTI_BENEFICIARII As String = "<TOȚI BENEFICIARII>"

    ' Liniile nodului selectat acum, NEFILTRATE — filtrul lucrează mereu peste ele, ca o a
    ' doua alegere din combo să nu filtreze peste rezultatul primeia.
    Private _linii As New List(Of OrdLinieRow)()

    ' Repopularea combo-ului ridică SelectedIndexChanged, dar aceea NU e o alegere a
    ' operatorului: filtrul s-ar aplica peste o grilă pe care oricum tocmai o reumplem.
    Private _suspendaFiltru As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IOrdPage.PageKey
        Get
            Return "vizualizare"
        End Get
    End Property

    ''' <summary>
    ''' Umple pagina din contextul primit. <c>Nothing</c> -&gt; pagină goală. O rădăcină de lună
    ''' ajunge aici cu liniile TUTUROR ordonanțărilor lunii — o listă plată e exact ce arată
    ''' o lună, deci nu e un caz separat; atunci filtrul pe beneficiar are cel mai mult de spus.
    ''' </summary>
    Public Sub SetContext(ctx As OrdPageContext) Implements IOrdPage.SetContext
        Try
            _linii = If(ctx Is Nothing OrElse ctx.Linii Is Nothing,
                        New List(Of OrdLinieRow)(), ctx.Linii)
            PopuleazaBeneficiari()
            AplicaFiltru()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdVizualizarePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' ── Filtrul pe beneficiar ────────────────────────────────────────────────

    ''' <summary>
    ''' Reface lista combo-ului din liniile nodului curent: rândul sintetic, apoi beneficiarii
    ''' DISTINCȚI, ordonați românește. Selecția de dinainte se păstrează dacă beneficiarul mai
    ''' există în noul nod — altfel se cade pe «toți», fiindcă un filtru rămas pe un nume
    ''' absent ar arăta o grilă goală fără să spună de ce.
    ''' </summary>
    Private Sub PopuleazaBeneficiari()
        Dim anterior As String = TryCast(cboBeneficiar.SelectedItem, String)

        Dim nume As List(Of String) =
            _linii.Select(Function(l) If(l.DenBene, String.Empty)).
                   Where(Function(n) Not String.IsNullOrWhiteSpace(n)).
                   Distinct(StringComparer.CurrentCultureIgnoreCase).
                   OrderBy(Function(n) n, StringComparer.CurrentCulture).
                   ToList()

        _suspendaFiltru = True
        Try
            cboBeneficiar.BeginUpdate()
            Try
                cboBeneficiar.Items.Clear()
                cboBeneficiar.Items.Add(TOTI_BENEFICIARII)
                For Each n As String In nume
                    cboBeneficiar.Items.Add(n)
                Next
            Finally
                cboBeneficiar.EndUpdate()
            End Try

            Dim index As Integer = -1
            If anterior IsNot Nothing Then index = cboBeneficiar.Items.IndexOf(anterior)
            cboBeneficiar.SelectedIndex = If(index >= 0, index, 0)
        Finally
            _suspendaFiltru = False
        End Try
    End Sub

    ' Boundary UI: logăm și înghițim — un handler de eveniment nu are cui să arunce mai departe.
    Private Sub CboBeneficiar_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cboBeneficiar.SelectedIndexChanged
        Try
            If _suspendaFiltru Then Return
            AplicaFiltru()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdVizualizarePage.CboBeneficiar_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>Umple grila cu liniile care trec de filtrul curent.</summary>
    Private Sub AplicaFiltru()
        Dim ales As String = TryCast(cboBeneficiar.SelectedItem, String)
        If ales Is Nothing OrElse String.Equals(ales, TOTI_BENEFICIARII, StringComparison.Ordinal) Then
            FillGrid(_linii)
            Return
        End If

        Dim filtrate As New List(Of OrdLinieRow)()
        For Each l As OrdLinieRow In _linii
            If String.Equals(If(l.DenBene, String.Empty), ales, StringComparison.CurrentCultureIgnoreCase) Then
                filtrate.Add(l)
            End If
        Next
        FillGrid(filtrate)
    End Sub

    ' ── Grila ────────────────────────────────────────────────────────────────

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

        ' Prima linie devine rândul curent, ca subsolul să nu rămână gol până la primul click.
        ' Actualizarea se cere EXPLICIT: când indexul e deja cel cerut (0 -> 0, sau -1 -> -1),
        ' grila nu ridică SelectionChanged, dar rândul de sub el e altul.
        grid.CurrentRowIndex = If(grid.Rows.Count > 0, 0, -1)
        ActualizeazaSubsol()
    End Sub

    Private Sub Grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            ActualizeazaSubsol()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdVizualizarePage.Grid_SelectionChanged", ex)
        End Try
    End Sub

    ' ── Subsolul ─────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Scrie în subsol datele beneficiarului rândului curent. Fără rând curent — grilă goală
    ''' sau filtru care n-a lăsat nimic — etichetele se golesc: mai bine gol decât beneficiarul
    ''' rămas de la selecția dinainte, care ar arăta ca al liniei de acum.
    ''' </summary>
    Private Sub ActualizeazaSubsol()
        Dim linie As OrdLinieRow = TryCast(grid.CurrentRow?.Tag, OrdLinieRow)

        lblBeneficiar.Text = If(linie Is Nothing, String.Empty, If(linie.DenBene, String.Empty))
        lblCodFiscal.Text = If(linie Is Nothing, String.Empty, If(linie.CodFiscal, String.Empty))
        lblContIban.Text = If(linie Is Nothing, String.Empty, If(linie.ContIban, String.Empty))
        lblDocJust.Text = If(linie Is Nothing, String.Empty, If(linie.DocJust, String.Empty))
        lblInfoPlata.Text = If(linie Is Nothing, String.Empty, If(linie.ObiectDdf, String.Empty))
    End Sub

    ''' <summary>Grila și combo-ul se auto-temează (IThemedControl, prin cascada
    ''' <c>ThemeManager</c>); aici rămân fundalul paginii și etichetele simple ale subsolului.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            tlyMain.BackColor = p.SurfaceAltColor
            tlyHeader.BackColor = p.SurfaceAltColor
            tblFooter.BackColor = p.SurfaceAltColor

            ' Etichetele-titlu sunt stinse, valorile sunt în culoarea textului: perechea se
            ' citește ca «etichetă: valoare», nu ca două texte de aceeași greutate.
            For Each capt As Label In New Label() {lblSelecteazaBeneficiar, lblBeneficiarCaption,
                                                   lblCodFiscalCaption, lblContIbanCaption,
                                                   lblDocJustCaption, lblInfoPlataCaption}
                capt.ForeColor = p.TextDimColor
                capt.BackColor = Color.Transparent
            Next
            For Each val As Label In New Label() {lblBeneficiar, lblCodFiscal, lblContIban,
                                                  lblDocJust, lblInfoPlata}
                val.ForeColor = p.TextColor
                val.BackColor = Color.Transparent
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("OrdVizualizarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
