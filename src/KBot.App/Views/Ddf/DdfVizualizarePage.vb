Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Vizualizare» a vederii DDF: liniile de secțiune A ale nodului selectat, într-o grilă
''' — exact tiparul paginii omonime din ORD (<c>OrdVizualizarePage</c>).
'''
''' DE CE NU MAI CITEȘTE XFA (revizuire de operator, 2026-08-15): pagina găzduia
''' <see cref="XfaXmlPreview"/>, care deschidea PDF-ul (sau siblingul lui .xml) și RECONSTRUIA
''' liniile din XML-ul XFA la fiecare selecție. Aceleași valori vin deja de la server, în
''' <see cref="DdfPageContext.Linii"/>, dintr-un singur apel pe CodAngajament — deci citirea
''' XFA era muncă în plus peste date pe care le aveam, și pica de tot când PDF-ul lipsea
''' (o revizie negenerată nu are ce arăta, deși liniile ei există în bază).
'''
''' Ce a rămas din vechea suprafață: ANTETUL. <c>lblNota</c> reproduce TOATE rândurile pe care le
''' arăta tabela de antet a lui <c>XfaXmlPreview</c> — aceleași etichete, aceeași ordine — plus
''' descrierea dedesubt. Vezi <see cref="NotaFor"/> pentru corespondența câmp-cu-câmp.
'''
''' Grila propriu-zisă e <see cref="DdfValoriPage"/>, găzduită ca sub-control: aceleași coloane,
''' aceeași ordonare, un singur designer de întreținut. (Ca pagină de sine stătătoare rămâne
''' PARCATĂ — nu are intrare în <c>navSub</c>; vezi nota din clasa ei.)
'''
''' PDF-ul REAL a rămas neatins, pe pagina «Document» (<see cref="DdfDocumentPage"/>), care are
''' și butonul «Generează documentul».
''' </summary>
Public Class DdfVizualizarePage
    Implements IDdfPage, IThemedControl

    ' Evenimentele contractului: de când suprafața XFA (cu butonul ei «Generează documentul») a
    ' plecat de pe pagină, «Vizualizare» NU le mai ridică niciodată. Generarea a rămas pe pagina
    ' «Document». Rămân declarate ca gazda să se poată abona uniform la toate paginile.
    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "previzualizare"
        End Get
    End Property

    ''' <summary>
    ''' Împinge contextul mai departe în grilă și pune nota din revizia selectată. Fără nicio
    ''' atingere de disc: totul e deja în context.
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            lblNota.Text = NotaFor(ctx)
            pagValori.SetContext(ctx)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Format românesc pentru date (dd.MM.yyyy), la fel ca în restul vederilor.
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ''' <summary>
    ''' Textul antetului: EXACT perechile pe care le arăta tabela de antet a lui
    ''' <c>XfaXmlPreview</c> (aceleași etichete, aceeași ordine), plus descrierea pe rândul de
    ''' jos. Sursa nu mai e XML-ul XFA, ci contextul — dar câmpurile sunt puse în corespondență
    ''' unu-la-unu cu nodurile pe care le scrie <c>DdfXmlBuilder</c> în «SubformAntet», ca
    ''' antetul de pe ecran să spună același lucru cu cel tipărit în PDF:
    '''
    '''   DenInstPb      -&gt; sesiune «NumeUnitate» (majuscule, ca în XML)
    '''   cif            -&gt; sesiune «CF»
    '''   NrUnicInreg    -&gt; Antet.Cual
    '''   SubtitluDF     -&gt; Antet.ObiectDDF
    '''   DataRevizuirii -&gt; Revizie.DataRev
    '''   Revizuirea     -&gt; Revizie.NumarRev
    '''
    ''' Perechile cu valoarea goală se sar (regula veche a parserului), deci pe o rădăcină de
    ''' lună — unde nu există O revizie — banda arată doar rândurile de unitate/document.
    ''' </summary>
    Private Shared Function NotaFor(ctx As DdfPageContext) As String
        If ctx Is Nothing Then Return "Selectați un angajament din arbore."

        Dim antet As DdfAntet = ctx.Antet
        Dim rev As RevizieRow = ctx.Revizie

        Dim perechi As New List(Of KeyValuePair(Of String, String))() From {
            AntetHeaderText.Pair("Instituția publică", UCaseSafe(ctx.NumeUnitate)),
            AntetHeaderText.Pair("Cod fiscal", ctx.CodFiscal),
            AntetHeaderText.Pair("Nr. unic", If(antet Is Nothing, String.Empty,
                                                antet.Cual.ToString(CultureInfo.InvariantCulture))),
            AntetHeaderText.Pair("Obiectul documentului", If(antet Is Nothing, String.Empty, antet.ObiectDDF)),
            AntetHeaderText.Pair("Compartiment", If(antet Is Nothing, String.Empty, UCaseSafe(antet.Comp))),
            AntetHeaderText.Pair("Data revizuirii", DataText(If(rev Is Nothing, Nothing, rev.DataRev))),
            AntetHeaderText.Pair("Revizuirea", If(rev Is Nothing, String.Empty,
                                                  rev.NumarRev.ToString(CultureInfo.InvariantCulture)))
        }

        Dim text As String = AntetHeaderText.Build(perechi)
        ' Descrierea, sub perechi: scurtă preferată, altfel lungă — regula lui XfaXmlPreview.Render.
        Dim desc As String = If(rev Is Nothing, String.Empty,
                                If(Not String.IsNullOrWhiteSpace(rev.DescScurta), rev.DescScurta, rev.DescLunga))
        text = AntetHeaderText.WithParagraph(text, desc)
        If String.IsNullOrEmpty(text) Then Return "Selectați o revizie din arbore."
        Return text
    End Function

    Private Shared Function DataText(d As Date?) As String
        Return If(d.HasValue, d.Value.ToString("dd.MM.yyyy", _roCulture), String.Empty)
    End Function

    ' Majuscule ca în XML (DenInstPb / ComparimentSpecialitate), fără să pice pe Nothing.
    Private Shared Function UCaseSafe(value As String) As String
        Return If(value, String.Empty).ToUpper(_roCulture)
    End Function

    ''' <summary>Cascadă: nota + pagina de valori găzduită (care își temează singură grila).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            lblNota.ForeColor = p.TextDimColor
            lblNota.BackColor = p.SurfaceAltColor
            ' `pagValori` e IThemedControl, deci ThemeManager nu recurge în el — îl cascadăm noi.
            pagValori.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
