Option Strict On
Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' UN NIVEL DE GRUPARE al <see cref="KBotDataView"/> (slice 0029) — echivalentul unei linii din
''' fereastra «Sorting and Grouping» a unui raport Access: o coloană după care se grupează, plus
''' banda de antet și banda de subsol pe care le aduce cu ea.
'''
''' <para><b>Ordinea nivelurilor E ierarhia.</b> Nivelul 0 e cel dinafară, ultimul e cel mai
''' dinăuntru; rândurile de date stau doar sub ultimul. Un rând de model se sortează întâi după
''' cheile de grupare, în ordinea lor, și abia apoi după sortarea cerută de operator din antet —
''' vezi <c>KBotDataView.ApplySort</c>. Asta e precedența din Access, și e motivul pentru care un
''' click pe un antet nu poate rupe gruparea.</para>
'''
''' <para><b>Ce agregat aduce o coloană în banda de grup</b> nu se cere aici: e chiar
''' <see cref="KBotDataColumn.Aggregate"/>, cel care alimentează și subsolul grilei. Un raport
''' Access scrie <c>=Sum([x])</c> o dată și îl pune și în subsolul de grup, și în cel de raport;
''' două proprietăți ar însemna două adevăruri despre aceeași coloană. Ce se alege pe nivel e DACĂ
''' agregatele se arată (<see cref="ShowFooterAggregates"/> / <see cref="ShowHeaderAggregates"/>),
''' nu care sunt.</para>
'''
''' <para><b>Culorile și fonturile</b> respectă regula casei: <c>Color.Empty</c> / <c>Nothing</c>
''' înseamnă «din temă», orice valoare pusă explicit câștigă. De aceea fiecare are perechea
''' <c>ShouldSerialize</c>/<c>Reset</c> — fără ea, Visual Studio ar scrie culoarea REZOLVATĂ în
''' <c>.Designer.vb</c> și de-atunci ea ar trece drept alegerea deliberată a operatorului.</para>
''' </summary>
<TypeConverter(GetType(ExpandableObjectConverter))>
Public NotInheritable Class KBotGroupLevel

    ''' <summary>Grila care deține nivelul (Nothing pentru o instanță liberă, ex. în designer).</summary>
    Friend Property Owner As KBotDataView

    ' Notifică grila că ceva din nivel s-a schimbat. Un nivel liber (fără Owner) nu are pe cine.
    Private Sub Notifica(structural As Boolean)
        If structural Then
            Owner?.OnGroupLevelsChanged()
        Else
            Owner?.OnGroupLevelAppearanceChanged()
        End If
    End Sub

    Public Sub New()
    End Sub

    ''' <summary>Nivel gata făcut: cheia coloanei + sensul de grupare.</summary>
    Public Sub New(columnKey As String, direction As KBotSortDirection)
        Me.ColumnKey = columnKey
        Me.SortDirection = direction
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' CE SE GRUPEAZĂ
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Cheia coloanei după care se grupează. O cheie vidă înseamnă un nivel INACTIV — el se sare
    ''' cu totul, nu aruncă: exact ca la coloane, designerul inserează un element gol în clipa în
    ''' care apeși «Add», cu mult înainte să apuce cineva să-i dea o cheie. O cheie NECUNOSCUTĂ,
    ''' în schimb, e o greșeală de model și se verifică la <c>EndInit</c>.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Cheia coloanei după care se grupează. Vidă = nivel inactiv (se sare).")>
    <DefaultValue(GetType(String), Nothing)>
    Public Property ColumnKey As String
        Get
            Return _columnKey
        End Get
        Set(value As String)
            If String.Equals(_columnKey, value, StringComparison.Ordinal) Then Return
            _columnKey = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _columnKey As String

    ''' <summary>
    ''' Sensul în care se așază GRUPURILE unele față de altele. Implicit crescător.
    '''
    ''' <para><see cref="KBotSortDirection.None"/> se REFUZĂ, și nu din strictețe: gruparea cere ca
    ''' rândurile aceleiași chei să stea lipite, iar «ordinea de încărcare» nu promite asta. Ar
    ''' ieși același grup de trei ori, cu trei antete, ceea ce arată exact ca o defecțiune.</para>
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Sensul în care se așază grupurile. None nu e permis (gruparea cere rândurile aceleiași chei lipite).")>
    <DefaultValue(KBotSortDirection.Ascending)>
    Public Property SortDirection As KBotSortDirection
        Get
            Return _sortDirection
        End Get
        Set(value As KBotSortDirection)
            If value = KBotSortDirection.None Then
                Throw New ArgumentException(
                    "Un nivel de grupare nu poate fi «None»: grupurile cer o ordine, altfel aceeași cheie " &
                    "ar apărea de mai multe ori, cu mai multe antete.", NameOf(SortDirection))
            End If
            If _sortDirection = value Then Return
            _sortDirection = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _sortDirection As KBotSortDirection = KBotSortDirection.Ascending

    ' ══════════════════════════════════════════════════════════════════════════
    ' BENZILE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Nivelul are bandă de ANTET (titlul grupului). Implicit True.
    '''
    ''' <para>Stinsă, nivelul nu se mai poate STRÂNGE nici dacă <see cref="Collapsible"/> e True:
    ''' antetul e singurul lucru care rămâne pe ecran când grupul e închis, deci fără el nu ar mai
    ''' exista pe ce apăsa ca să se redeschidă. Vezi <see cref="EffectiveCollapsible"/>.</para>
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Nivelul are bandă de antet (titlul grupului). Fără ea, grupul nu se poate strânge.")>
    <DefaultValue(True)>
    Public Property ShowHeader As Boolean
        Get
            Return _showHeader
        End Get
        Set(value As Boolean)
            If _showHeader = value Then Return
            _showHeader = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _showHeader As Boolean = True

    ''' <summary>Nivelul are bandă de SUBSOL (linia de totaluri a grupului). Implicit True.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Nivelul are bandă de subsol (linia de totaluri a grupului).")>
    <DefaultValue(True)>
    Public Property ShowFooter As Boolean
        Get
            Return _showFooter
        End Get
        Set(value As Boolean)
            If _showFooter = value Then Return
            _showFooter = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _showFooter As Boolean = True

    ''' <summary>
    ''' Înălțimea benzii de antet, în px. <c>0</c> (implicit) = urmărește <c>RowHeight</c>-ul
    ''' grilei. Nu poartă <c>DefaultValue</c> pe valoarea rezolvată din același motiv ca
    ''' <c>FooterHeight</c>-ul grilei: «0» și «28» sunt două stări diferite, iar designerul n-are
    ''' voie să le confunde.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Înălțimea benzii de antet a grupului (px). 0 = urmărește RowHeight.")>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            Dim nou As Integer = Math.Max(0, value)
            If _headerHeight = nou Then Return
            _headerHeight = nou
            Notifica(structural:=True)
        End Set
    End Property
    Private _headerHeight As Integer

    Private Function ShouldSerializeHeaderHeight() As Boolean
        Return _headerHeight > 0
    End Function

    Private Sub ResetHeaderHeight()
        HeaderHeight = 0
    End Sub

    ''' <summary>Înălțimea benzii de subsol a grupului (px). <c>0</c> (implicit) = urmărește <c>RowHeight</c>.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Înălțimea benzii de subsol a grupului (px). 0 = urmărește RowHeight.")>
    Public Property FooterHeight As Integer
        Get
            Return _footerHeight
        End Get
        Set(value As Integer)
            Dim nou As Integer = Math.Max(0, value)
            If _footerHeight = nou Then Return
            _footerHeight = nou
            Notifica(structural:=True)
        End Set
    End Property
    Private _footerHeight As Integer

    Private Function ShouldSerializeFooterHeight() As Boolean
        Return _footerHeight > 0
    End Function

    Private Sub ResetFooterHeight()
        FooterHeight = 0
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' TITLURILE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Șablonul titlului din antetul grupului. Trei locuri de completat, toate opționale:
    ''' <c>{0}</c> = titlul coloanei de grupare, <c>{1}</c> = valoarea grupului (formatată exact ca
    ''' în celulele coloanei), <c>{2}</c> = câte rânduri are grupul.
    '''
    ''' <para>Implicit <c>«{0}: {1} ({2})»</c>. Un șablon vid lasă banda fără titlu (rămân
    ''' agregatele, dacă sunt cerute).</para>
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Șablonul titlului din antet: {0} = titlul coloanei, {1} = valoarea grupului, {2} = numărul de rânduri.")>
    <DefaultValue("{0}: {1} ({2})")>
    Public Property HeaderCaptionFormat As String
        Get
            Return _headerCaptionFormat
        End Get
        Set(value As String)
            If String.Equals(_headerCaptionFormat, value, StringComparison.Ordinal) Then Return
            _headerCaptionFormat = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _headerCaptionFormat As String = "{0}: {1} ({2})"

    ''' <summary>
    ''' Șablonul titlului din subsolul grupului, cu aceleași trei locuri de completat ca la antet.
    ''' Implicit <c>«Total {1}»</c>.
    '''
    ''' <para>Titlul se OPREȘTE la prima coloană agregată, exact ca titlul din subsolul grilei
    ''' (slice 0028-02): un text care ar curge pe sub totalul cuiva s-ar citi ca eticheta acelui
    ''' total, adică altă propoziție decât cea scrisă.</para>
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Șablonul titlului din subsol: {0} = titlul coloanei, {1} = valoarea grupului, {2} = numărul de rânduri.")>
    <DefaultValue("Total {1}")>
    Public Property FooterCaptionFormat As String
        Get
            Return _footerCaptionFormat
        End Get
        Set(value As String)
            If String.Equals(_footerCaptionFormat, value, StringComparison.Ordinal) Then Return
            _footerCaptionFormat = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _footerCaptionFormat As String = "Total {1}"

    ''' <summary>
    ''' Cu ce se scrie <c>{1}</c> când valoarea grupului e goală. Implicit <c>«(goale)»</c> — un
    ''' titlu care ar rămâne «: (12)» nu spune nimănui că acela e grupul rândurilor fără valoare.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Ce se scrie în locul valorii când grupul e cel al rândurilor fără valoare.")>
    <DefaultValue("(goale)")>
    Public Property EmptyCaption As String
        Get
            Return _emptyCaption
        End Get
        Set(value As String)
            If String.Equals(_emptyCaption, value, StringComparison.Ordinal) Then Return
            _emptyCaption = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _emptyCaption As String = "(goale)"

    ''' <summary>
    ''' Retragerea (px) pe care nivelul o adaugă benzilor de SUB el — nu propriilor lui benzi.
    ''' Implicit 16, deci nivelul 0 stă lipit de margine, nivelul 1 la 16px, nivelul 2 la 32px:
    ''' retragerea cumulativă e ce face ierarhia vizibilă pe hârtie, ca într-un raport Access.
    ''' <c>0</c> aliniază totul la marginea din stânga.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Retragerea (px) pe care nivelul o adaugă benzilor de sub el (nu propriilor benzi). Cumulativă.")>
    <DefaultValue(16)>
    Public Property Indent As Integer
        Get
            Return _indent
        End Get
        Set(value As Integer)
            Dim nou As Integer = Math.Max(0, value)
            If _indent = nou Then Return
            _indent = nou
            Notifica(structural:=False)
        End Set
    End Property
    Private _indent As Integer = 16

    ' ══════════════════════════════════════════════════════════════════════════
    ' AGREGATELE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Banda de SUBSOL a grupului arată agregatele coloanelor (<see cref="KBotDataColumn.Aggregate"/>),
    ''' fiecare sub coloana lui. Implicit True — acesta e rostul benzii.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Subsolul grupului arată agregatele coloanelor, fiecare sub coloana lui.")>
    <DefaultValue(True)>
    Public Property ShowFooterAggregates As Boolean
        Get
            Return _showFooterAggregates
        End Get
        Set(value As Boolean)
            If _showFooterAggregates = value Then Return
            _showFooterAggregates = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _showFooterAggregates As Boolean = True

    ''' <summary>
    ''' Banda de ANTET arată și ea agregatele, pe aceeași linie cu titlul. Implicit False.
    '''
    ''' <para>E jumătatea care contează pentru grupurile STRÂNSE: un grup închis își arată doar
    ''' antetul, deci fără asta totalurile lui dispar exact când sunt singurul lucru rămas de citit.
    ''' Titlul se decupează înaintea primei coloane agregate, ca în subsol.</para>
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Antetul grupului arată și el agregatele, pe aceeași linie cu titlul (contează mai ales strâns).")>
    <DefaultValue(False)>
    Public Property ShowHeaderAggregates As Boolean
        Get
            Return _showHeaderAggregates
        End Get
        Set(value As Boolean)
            If _showHeaderAggregates = value Then Return
            _showHeaderAggregates = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _showHeaderAggregates As Boolean = False

    ' ══════════════════════════════════════════════════════════════════════════
    ' STRÂNGEREA
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Grupurile nivelului se pot strânge (click pe antet, sau săgețile stânga/dreapta de la
    ''' tastatură). Implicit True.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Grupurile nivelului se pot strânge. Are nevoie de ShowHeader (altfel n-ar exista pe ce apăsa).")>
    <DefaultValue(True)>
    Public Property Collapsible As Boolean
        Get
            Return _collapsible
        End Get
        Set(value As Boolean)
            If _collapsible = value Then Return
            _collapsible = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _collapsible As Boolean = True

    ''' <summary>
    ''' Strângerea EFECTIVĂ: cerută ȘI posibilă. Fără bandă de antet nu există pe ce apăsa pentru
    ''' redeschidere, deci nivelul rămâne desfăcut oricât ar cere <see cref="Collapsible"/>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveCollapsible As Boolean
        Get
            Return _collapsible AndAlso _showHeader
        End Get
    End Property

    ''' <summary>
    ''' Grupurile nivelului pornesc STRÂNSE la prima construcție a benzilor. Implicit False.
    ''' Odată ce operatorul a atins un grup, alegerea lui rămâne (vezi
    ''' <c>KBotDataView.ExpandAllGroups</c> / <c>CollapseAllGroups</c>).
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Grupurile nivelului pornesc strânse.")>
    <DefaultValue(False)>
    Public Property CollapsedByDefault As Boolean
        Get
            Return _collapsedByDefault
        End Get
        Set(value As Boolean)
            If _collapsedByDefault = value Then Return
            _collapsedByDefault = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _collapsedByDefault As Boolean = False

    ' ══════════════════════════════════════════════════════════════════════════
    ' ASPECT — gol / Nothing = din temă
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Fundalul benzii de antet. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Fundalul benzii de antet a grupului. Gol = culoarea din schema activă.")>
    Public Property HeaderBackColor As Color
        Get
            Return _headerBackColor
        End Get
        Set(value As Color)
            If _headerBackColor = value Then Return
            _headerBackColor = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _headerBackColor As Color = Color.Empty

    Private Function ShouldSerializeHeaderBackColor() As Boolean
        Return _headerBackColor <> Color.Empty
    End Function

    Private Sub ResetHeaderBackColor()
        HeaderBackColor = Color.Empty
    End Sub

    ''' <summary>Culoarea titlului din antet. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Culoarea textului din antetul grupului. Gol = culoarea din schema activă.")>
    Public Property HeaderForeColor As Color
        Get
            Return _headerForeColor
        End Get
        Set(value As Color)
            If _headerForeColor = value Then Return
            _headerForeColor = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _headerForeColor As Color = Color.Empty

    Private Function ShouldSerializeHeaderForeColor() As Boolean
        Return _headerForeColor <> Color.Empty
    End Function

    Private Sub ResetHeaderForeColor()
        HeaderForeColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Fontul benzii de antet. <c>Nothing</c> (implicit) = fontul de bandă al grilei (cel derivat
    ''' din schemă). Perechea ShouldSerialize/Reset e obligatorie: <c>Font</c> nu poate purta
    ''' <c>DefaultValue</c>, deci fără ea designerul ar îngheța fontul rezolvat în formular.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Fontul benzii de antet a grupului. Nesetat = fontul de bandă al schemei active.")>
    Public Property HeaderFont As Font
        Get
            Return _headerFont
        End Get
        Set(value As Font)
            If _headerFont Is value Then Return
            _headerFont = value
            Notifica(structural:=True)      ' fontul poate schimba înălțimea măsurată a benzii
        End Set
    End Property
    Private _headerFont As Font

    Private Function ShouldSerializeHeaderFont() As Boolean
        Return _headerFont IsNot Nothing
    End Function

    Private Sub ResetHeaderFont()
        HeaderFont = Nothing
    End Sub

    ''' <summary>Fundalul benzii de subsol a grupului. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Fundalul benzii de subsol a grupului. Gol = culoarea din schema activă.")>
    Public Property FooterBackColor As Color
        Get
            Return _footerBackColor
        End Get
        Set(value As Color)
            If _footerBackColor = value Then Return
            _footerBackColor = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _footerBackColor As Color = Color.Empty

    Private Function ShouldSerializeFooterBackColor() As Boolean
        Return _footerBackColor <> Color.Empty
    End Function

    Private Sub ResetFooterBackColor()
        FooterBackColor = Color.Empty
    End Sub

    ''' <summary>Culoarea textului din subsolul grupului. <c>Color.Empty</c> (implicit) = din schema activă.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Culoarea textului din subsolul grupului. Gol = culoarea din schema activă.")>
    Public Property FooterForeColor As Color
        Get
            Return _footerForeColor
        End Get
        Set(value As Color)
            If _footerForeColor = value Then Return
            _footerForeColor = value
            Notifica(structural:=False)
        End Set
    End Property
    Private _footerForeColor As Color = Color.Empty

    Private Function ShouldSerializeFooterForeColor() As Boolean
        Return _footerForeColor <> Color.Empty
    End Function

    Private Sub ResetFooterForeColor()
        FooterForeColor = Color.Empty
    End Sub

    ''' <summary>Fontul benzii de subsol a grupului. <c>Nothing</c> (implicit) = fontul de bandă al grilei.</summary>
    <Category("K-BOT: Grupare")>
    <Description("Fontul benzii de subsol a grupului. Nesetat = fontul de bandă al schemei active.")>
    Public Property FooterFont As Font
        Get
            Return _footerFont
        End Get
        Set(value As Font)
            If _footerFont Is value Then Return
            _footerFont = value
            Notifica(structural:=True)
        End Set
    End Property
    Private _footerFont As Font

    Private Function ShouldSerializeFooterFont() As Boolean
        Return _footerFont IsNot Nothing
    End Function

    Private Sub ResetFooterFont()
        FooterFont = Nothing
    End Sub

    ''' <summary>Payload al apelantului. Nefolosit de grilă.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>Nivelul e activ? (are o cheie de coloană nevidă)</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsActive As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(_columnKey)
        End Get
    End Property

    ''' <summary>Ce se vede în dialogul de colecție al designerului, pe fiecare linie.</summary>
    Public Overrides Function ToString() As String
        If Not IsActive Then Return "(nivel fără coloană)"
        Return $"{_columnKey} ({_sortDirection})"
    End Function

End Class
