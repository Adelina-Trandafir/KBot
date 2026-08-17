Option Strict On
Imports System.Globalization
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Vizualizare» a vederii DDF: ANTETUL documentului (o tabelă de perechi
''' etichetă/valoare) și, dedesubt, liniile de secțiune A ale nodului selectat.
'''
''' DE CE NU MAI CITEȘTE XFA (revizuire de operator, 2026-08-15): pagina găzduia
''' <see cref="XfaXmlPreview"/>, care deschidea PDF-ul (sau siblingul lui .xml) și RECONSTRUIA
''' liniile din XML-ul XFA la fiecare selecție. Aceleași valori vin deja de la server, în
''' <see cref="DdfPageContext.Linii"/>, dintr-un singur apel pe CodAngajament — deci citirea
''' XFA era muncă în plus peste date pe care le aveam, și pica de tot când PDF-ul lipsea
''' (o revizie negenerată nu are ce arăta, deși liniile ei există în bază).
'''
''' ANTETUL (revizuire de operator, 2026-08-16): era UN singur <c>lblNota</c> cu perechile
''' înșirate într-un bloc de text construit de <c>AntetHeaderText</c> — text monolitic, fără
''' aliniere între rânduri. Acum e <c>tblHeader</c>, un <c>TableLayoutPanel</c> autorat în
''' designer: fiecare câmp are eticheta lui și celula lui de valoare, deci coloanele se aliniază
''' singure și fiecare valoare se poate tema/formata separat. Codul de aici NU face decât să
''' pună textele — pozițiile, fonturile și rândurile sunt ale designerului.
'''
''' SURSA VALORILOR: <see cref="DdfPageContext.Antet"/>, adică rândul FX_DDF adus de
''' <c>GET /api/forexe/ddf</c> (MariaDB) și ales de <c>DdfView</c> pe IDDF-ul nodului de arbore.
''' Nicio cerere nouă de rețea și nicio atingere de disc: totul e deja în context. Antetul e al
''' ANGAJAMENTULUI, nu al reviziei, deci nu se schimbă la click-uri în arbore.
'''
''' Corespondența câmp-cu-câmp (coloană FX_DDF -&gt; etichetă):
'''   CodAngajament -&gt; «Cod angajament»   (cade pe <c>ctx.Cod</c> dacă antetul lipsește)
'''   CUAL          -&gt; «CUAL»             (numărul unic de înregistrare)
'''   DataCreare    -&gt; «Data creare»
'''   Comp          -&gt; «Compartimentul»
'''   NumePartener  -&gt; «Beneficiar»       (+ codul fiscal AL PARTENERULUI, când există)
'''   ObiectDDF     -&gt; «Obiect DDF»
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

    ' Ce se pune într-o celulă de valoare când baza n-are nimic acolo. O liniuță spune
    ' „am întrebat și e gol"; o celulă goală arată ca un antet care nu s-a încărcat.
    Private Const GOL As String = "—"

    ' Format românesc pentru date (dd.MM.yyyy), la fel ca în restul vederilor.
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ' Evenimentele contractului: de când suprafața XFA (cu butonul ei «Generează documentul») a
    ' plecat de pe pagină, «Vizualizare» NU le mai ridică niciodată. Generarea a rămas pe pagina
    ' «Document». Rămân declarate ca gazda să se poată abona uniform la toate paginile.
    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
        ' Designerul lasă celulele de valoare fără text; le punem pe starea „gol" încă de la
        ' construcție, ca antetul să arate la fel înainte și după prima selecție.
        RandeazaAntet(Nothing)
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "previzualizare"
        End Get
    End Property

    ''' <summary>
    ''' Umple antetul din contextul primit și împinge contextul mai departe în grilă. Fără nicio
    ''' atingere de disc și fără cerere de rețea: totul e deja în context.
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            RandeazaAntet(ctx)
            pagValori.SetContext(ctx)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Pune valorile în celulele lui <c>tblHeader</c>. <c>Nothing</c> (nimic selectat) sau un
    ''' angajament fără rând FX_DDF -&gt; toate celulele arată <see cref="GOL"/>; structura tabelei
    ''' rămâne pe ecran, deci antetul nu „sare" între o selecție și alta.
    ''' </summary>
    Private Sub RandeazaAntet(ctx As DdfPageContext)
        Dim antet As DdfAntet = If(ctx Is Nothing, Nothing, ctx.Antet)

        ' Codul vine din antet, dar dacă angajamentul n-are rând FX_DDF îl știm oricum din
        ' contextul de navigare — celula asta n-are de ce să fie goală.
        Dim cod As String = If(antet Is Nothing, Nothing, antet.CodAngajament)
        If String.IsNullOrWhiteSpace(cod) AndAlso ctx IsNot Nothing Then cod = ctx.Cod

        lblCod.Text = TextSau(cod)
        lblCUAL.Text = If(antet Is Nothing, GOL,
                          antet.Cual.ToString(CultureInfo.InvariantCulture))
        lblDataCreare.Text = DataText(If(antet Is Nothing, Nothing, antet.DataCreare))
        lblCompartiment.Text = TextSau(If(antet Is Nothing, Nothing, antet.Comp))
        lblBeneficiar.Text = BeneficiarText(antet)
        lblObiectDDF.Text = TextSau(If(antet Is Nothing, Nothing, antet.ObiectDDF))
    End Sub

    ''' <summary>
    ''' Beneficiarul: numele partenerului, cu codul lui fiscal în paranteză când baza îl are.
    ''' Când documentul NU e legat de un partener (<c>PartAng = False</c>, cazul «GENERAL» din
    ''' convenția de foldere) o spunem explicit — numele ar putea fi rămas pe rând din altă
    ''' revizie, iar afișarea lui ar minți despre cine e beneficiarul.
    ''' </summary>
    Private Shared Function BeneficiarText(antet As DdfAntet) As String
        If antet Is Nothing Then Return GOL
        If Not antet.PartAng Then Return "Fără partener"
        If String.IsNullOrWhiteSpace(antet.NumePartener) Then Return GOL
        If String.IsNullOrWhiteSpace(antet.CodFiscal) Then Return antet.NumePartener.Trim()
        ' CodFiscal e AL PARTENERULUI (cel al unității stă în sesiune, vezi DdfPageContext).
        Return $"{antet.NumePartener.Trim()} (CIF {antet.CodFiscal.Trim()})"
    End Function

    Private Shared Function TextSau(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return GOL
        Return value.Trim()
    End Function

    Private Shared Function DataText(d As Date?) As String
        Return If(d.HasValue, d.Value.ToString("dd.MM.yyyy", _roCulture), GOL)
    End Function

    ''' <summary>
    ''' Cascadă: tabela de antet (etichetele estompat, valorile în culoarea textului) + pagina de
    ''' valori găzduită (care își temează singură grila). Etichetele stau transparente peste
    ''' fundalul tabelei, deci o singură culoare de fundal se schimbă la schimbarea temei.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            tblHeader.BackColor = p.SurfaceAltColor

            For Each eticheta As Label In {lblCodCaption, lblDataCreareCaption, lblCompartimentCaption,
                                           lblStareCaption, lblBeneficiarCaption, lblObiectDDFCaption}
                eticheta.BackColor = Color.Transparent
                eticheta.ForeColor = p.TextDimColor
            Next

            For Each valoare As Label In {lblCod, lblDataCreare, lblCompartiment,
                                          lblCUAL, lblBeneficiar, lblObiectDDF}
                valoare.BackColor = Color.Transparent
                valoare.ForeColor = p.TextColor
            Next

            ' `pagValori` e IThemedControl, deci ThemeManager nu recurge în el — îl cascadăm noi.
            pagValori.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfVizualizarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
