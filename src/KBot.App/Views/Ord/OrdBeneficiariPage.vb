Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Beneficiari» a editorului de ordonantare (felia 0049) — portul lui
''' <c>frmFX_ORD_PART</c> + <c>frmFX_ORD_TBL</c>.
'''
''' <para><b><c>chkClsf</c> — o bifa, trei efecte.</b> Obiceiul de lucru pe care operatorul a
''' cerut explicit sa-l pastram (D10). Nebifata: lista din stanga arata BENEFICIARII, coloana
''' a doua a grilei arata <c>CodSSI</c> si se numeste «Cod SSI». Bifata: lista arata
''' CLASIFICATIILE, coloana arata <c>ContIBAN</c> si se numeste «Cont IBAN», iar grila se
''' ordoneaza dupa <c>CodSSI</c>. Cele trei efecte stau intr-un SINGUR loc
''' (<see cref="AplicaModulListei"/>), ca sa nu poata devia unul de altul — in Access erau deja
''' impreuna, in <c>chkClsf_AfterUpdate</c>.</para>
'''
''' <para><b>Cand nu are ce comuta, bifa nu se mai arata</b> (cerinta operatorului,
''' 03.09.2026): un singur beneficiar SI o singura clasificatie inseamna ca ambele moduri ar
''' arata acelasi rand unic, deci banda ei se strange la zero
''' (<see cref="AplicaVizibilitateaBifei"/>). Cu mai multe clasificatii bifa RAMANE, chiar pe
''' un singur beneficiar — acolo ea chiar desface liniile. Tot atunci se retrage si randul
''' sintetic din lista din stanga: pe o lista de o intrare, «toti» si acea intrare sunt
''' acelasi lucru.</para>
'''
''' <para><b>Ce se editeaza.</b> «Valoare» si «Explicatie». Restul coloanelor sunt derivate:
''' clasificatia, codul SSI si <c>CodAI</c> vin din plata pe care o acopera linia si trebuie sa
''' ramana in acord intre ele (altfel cheile straine ale lui <c>FX_ORD_TBL</c> resping salvarea),
''' iar «Receptii» / «Plati ant.» sunt sume citite. «Ramas» NU se editeaza: se RECALCULEAZA la
''' fiecare schimbare de valoare, dupa formula din <c>Adauga_Ord_Tbl</c>
''' (<c>Round(receptii − plati anterioare − valoare, 2)</c>) — doua cifre care spun acelasi
''' lucru nu au voie sa se poata contrazice pe ecran.</para>
'''
''' <para><b><c>btnClsf</c> nu s-a portat.</b> In tot exportul Access nu exista niciun
''' <c>btnClsf_Click</c>; singura lui aparitie e in <c>PositionElements</c>, o functie al carei
''' prim rand e <c>Exit Function</c>. Un buton care nu face nimic ar fi exact no-op-ul tacut pe
''' care regulile casei il interzic.</para>
'''
''' <para>Pagina NU face cereri de retea: scrie direct in graful primit de la formular.</para>
''' </summary>
Public Class OrdBeneficiariPage
    Implements IOrdEditPage, IThemedControl

    ' Cheile coloanelor — trebuie sa ramana in acord cu literalele scrise in designer.
    Private Const COL_ETICHETA As String = "eticheta"
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_SSI_IBAN As String = "ssi_iban"
    Private Const COL_EXPLICATIE As String = "explicatie"
    Private Const COL_TOTAL_RECEPTII As String = "total_receptii"
    Private Const COL_PLATI_ANT As String = "plati_ant"
    Private Const COL_VALOARE As String = "valoare"
    Private Const COL_RAMAS As String = "ramas"

    ''' <summary>Primul rand al listei din stanga — «fara filtru», nu un beneficiar. E un
    ''' LITERAL, deci comparatia pe el e sigura (perechea lui <c>lstDenBene</c> din Access).</summary>
    Private Const TOTI_BENEFICIARII As String = "< TOȚI BENEFICIARII >"
    ''' <summary>Perechea lui, pe ramura «grupeaza pe clasificatii».</summary>
    Private Const TOATE_CLASIFICATIILE As String = "< TOATE CLASIFICAȚIILE >"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private _draft As OrdDraft
    ' Cheia randului selectat in lista din stanga: identitatea beneficiarului (reala sau
    ' temporara) cand bifa e stinsa, IdClsf cand e aprinsa. 0 = randul sintetic «toti/toate».
    Private _cheieSelectata As Integer
    ' Repopularea listelor ridica evenimente de selectie care NU sunt alegeri ale operatorului.
    Private _suspenda As Boolean
    ' Inaltimea benzii pe care sta bifa, retinuta la prima ascundere ca sa poata fi pusa
    ' inapoi. -1 = inca nu s-a ascuns niciodata. Vezi `AplicaVizibilitateaBifei`.
    Private _inaltimeaBifei As Single = -1.0F
    ' Ce s-a HOTARAT despre bifa, nu ce se vede: `chkClsf.Visible` raspunde False cat timp
    ' formularul insusi e inca nearatat (getter-ul da vizibilitatea EFECTIVA), iar prima
    ' asezare se face tocmai atunci, din `Form_Load`. Nothing = inca nu s-a hotarat nimic.
    Private _bifaAreRost As Boolean?

    Public Event DraftModificat As EventHandler Implements IOrdEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IOrdEditPage.PageKey
        Get
            Return "beneficiari"
        End Get
    End Property

    ''' <summary>Umple pagina din graful primit. <c>Nothing</c> -&gt; pagina goala.</summary>
    Public Sub SetDraft(draft As OrdDraft) Implements IOrdEditPage.SetDraft
        Try
            _draft = draft
            _cheieSelectata = 0
            AplicaVizibilitateaBifei()
            AplicaModulListei()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.SetDraft", ex)
            Throw
        End Try
    End Sub

    ' ── Bifa chkClsf: o singura functie pentru cele trei efecte ──────────────────────────

    ' Boundary UI: logam si inghitim — un handler de eveniment nu are cui sa arunce mai departe.
    Private Sub ChkClsf_CheckedChanged(sender As Object, e As EventArgs) Handles chkClsf.CheckedChanged
        Try
            If _suspenda Then Return
            _cheieSelectata = 0
            AplicaModulListei()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.ChkClsf_CheckedChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Cele TREI efecte ale bifei, impreuna: antetul listei din stanga, continutul ei, si
    ''' antetul + sursa coloanei a doua a grilei. Apoi reumple ambele suprafete.
    ''' </summary>
    Private Sub AplicaModulListei()
        Dim peClasificatii As Boolean = chkClsf.Checked

        Dim colStanga As KBotDataColumn = grdStanga.Column(COL_ETICHETA)
        If colStanga IsNot Nothing Then
            colStanga.HeaderText = If(peClasificatii, "Clasificații", "Beneficiari")
        End If
        Dim colSsi As KBotDataColumn = grdLinii.Column(COL_SSI_IBAN)
        If colSsi IsNot Nothing Then
            colSsi.HeaderText = If(peClasificatii, "Cont IBAN", "Cod SSI")
        End If

        PopuleazaStanga()
        AplicaFiltrulGrilei()
        ActualizeazaAntetBeneficiar()
    End Sub

    ' ── Lista din stanga ─────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Reface lista din stanga: randul sintetic, apoi beneficiarii (sau clasificatiile
    ''' distincte ale liniilor), ordonate romaneste. Selectia de dinainte se pastreaza daca
    ''' mai exista; altfel se cade pe randul sintetic — un filtru ramas pe ceva absent ar arata
    ''' o grila goala fara sa spuna de ce.
    ''' </summary>
    Private Sub PopuleazaStanga()
        Dim anterior As Integer = _cheieSelectata
        _suspenda = True
        Try
            grdStanga.BeginUpdate()
            Try
                grdStanga.ClearRows()

                ' Intai intrarile REALE, apoi se vede daca randul sintetic mai are ce aduna:
                ' cu o singura intrare, «toti» inseamna exact acea intrare, iar un rand care
                ' nu filtreaza nimic e doar un al doilea nume pentru acelasi lucru (cerinta
                ' operatorului, 03.09.2026). Proba e pe UNU, nu pe «cel putin doua»: fara nicio
                ' intrare randul sintetic e singurul lucru de care se poate agata cursorul.
                Dim intrari As List(Of KeyValuePair(Of Integer, String)) = IntrarileListei()
                If intrari.Count <> 1 Then
                    Dim sintetic As KBotDataRow = grdStanga.AddRow()
                    sintetic.Tag = 0
                    sintetic(COL_ETICHETA) = If(chkClsf.Checked, TOATE_CLASIFICATIILE, TOTI_BENEFICIARII)
                End If
                For Each intrare As KeyValuePair(Of Integer, String) In intrari
                    Dim r As KBotDataRow = grdStanga.AddRow()
                    r.Tag = intrare.Key
                    r(COL_ETICHETA) = intrare.Value
                Next
            Finally
                grdStanga.EndUpdate()
            End Try

            ' Re-aseaza selectia pe acelasi lucru, daca mai exista.
            Dim index As Integer = 0
            For i As Integer = 0 To grdStanga.Rows.Count - 1
                If TypeOf grdStanga.Rows(i).Tag Is Integer AndAlso
                   CInt(grdStanga.Rows(i).Tag) = anterior Then
                    index = i
                    Exit For
                End If
            Next
            grdStanga.CurrentRowIndex = If(grdStanga.Rows.Count > 0, index, -1)
            _cheieSelectata = CheiaRanduluiCurent()
        Finally
            _suspenda = False
        End Try
    End Sub

    ''' <summary>
    ''' Intrarile REALE ale listei din stanga, in ordinea ceruta de modul ei: beneficiarii
    ''' (bifa stinsa) sau clasificatiile DISTINCTE ale liniilor (bifa aprinsa). Cheia e ce
    ''' ajunge in <c>Tag</c>-ul randului; randul sintetic NU e printre ele.
    ''' </summary>
    Private Function IntrarileListei() As List(Of KeyValuePair(Of Integer, String))
        Dim intrari As New List(Of KeyValuePair(Of Integer, String))()
        If _draft Is Nothing Then Return intrari

        If chkClsf.Checked Then
            ' Clasificatiile DISTINCTE ale liniilor, ordonate dupa cod.
            Dim vazute As New HashSet(Of Integer)()
            For Each l As OrdDraftLinie In _draft.Linii.
                    OrderBy(Function(x) x.Clsf, StringComparer.CurrentCulture)
                If l.IdClsf = 0 OrElse Not vazute.Add(l.IdClsf) Then Continue For
                intrari.Add(New KeyValuePair(Of Integer, String)(
                    l.IdClsf, If(String.IsNullOrWhiteSpace(l.Clsf), CStr(l.IdClsf), l.Clsf)))
            Next
        Else
            For Each p As OrdDraftPart In _draft.Parteneri.
                    OrderBy(Function(x) x.DenBene, StringComparer.CurrentCulture)
                intrari.Add(New KeyValuePair(Of Integer, String)(p.Cheie, p.DenBene))
            Next
        End If
        Return intrari
    End Function

    ''' <summary>
    ''' Bifa se ascunde CU TOT CU BANDA pe care sta atunci cand n-are ce comuta: un singur
    ''' beneficiar SI o singura clasificatie inseamna ca ambele moduri ar arata acelasi rand
    ''' unic (cerinta operatorului, 03.09.2026). Cu mai multe clasificatii bifa RAMANE, chiar
    ''' daca beneficiarul e unul singur — acolo ea chiar desface liniile.
    '''
    ''' <para>Inaltimea benzii se RETINE la ascundere, nu se scrie inapoi de la 96 dpi:
    ''' <c>TableLayoutPanel</c> a scalat-o deja pentru ecranul de fata, iar o valoare logica
    ''' pusa inapoi ar strange banda pe un ecran marit (regula DPI a casei). Punerea inapoi e
    ''' plasa de siguranta: graful nu-si schimba beneficiarii cat tine un editor deschis, deci
    ''' hotararea nu se poate rasturna in practica.</para>
    ''' </summary>
    Private Sub AplicaVizibilitateaBifei()
        Dim nrBene As Integer = If(_draft Is Nothing, 0, _draft.Parteneri.Count)
        Dim nrClsf As Integer = If(_draft Is Nothing, 0,
            _draft.Linii.Where(Function(l) l.IdClsf <> 0).
                         Select(Function(l) l.IdClsf).Distinct().Count())
        Dim areRost As Boolean = nrBene > 1 OrElse nrClsf > 1
        If _bifaAreRost.HasValue AndAlso _bifaAreRost.Value = areRost Then Return
        _bifaAreRost = areRost

        ' Ascunsa SI bifata ar lasa lista pe «clasificatii» fara nicio cale de intoarcere.
        If Not areRost AndAlso chkClsf.Checked Then
            _suspenda = True
            Try
                chkClsf.Checked = False
            Finally
                _suspenda = False
            End Try
        End If

        Dim banda As RowStyle = tlyStanga.RowStyles(0)
        If areRost Then
            If _inaltimeaBifei > 0.0F Then banda.Height = _inaltimeaBifei
        Else
            If _inaltimeaBifei <= 0.0F Then _inaltimeaBifei = banda.Height
            banda.Height = 0.0F
        End If
        chkClsf.Visible = areRost
    End Sub

    Private Function CheiaRanduluiCurent() As Integer
        Dim r As KBotDataRow = grdStanga.CurrentRow
        If r Is Nothing OrElse Not (TypeOf r.Tag Is Integer) Then Return 0
        Return CInt(r.Tag)
    End Function

    Private Sub GrdStanga_SelectionChanged(sender As Object, e As EventArgs) Handles grdStanga.SelectionChanged
        Try
            If _suspenda Then Return
            _cheieSelectata = CheiaRanduluiCurent()
            AplicaFiltrulGrilei()
            ActualizeazaAntetBeneficiar()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.GrdStanga_SelectionChanged", ex)
        End Try
    End Sub

    ' ── Campurile beneficiarului selectat ────────────────────────────────────────────────

    ''' <summary>
    ''' Scrie in benzile de sus datele beneficiarului curent — cel numit de lista din stanga
    ''' cand ea numeste unul, altfel cel al LINIEI selectate in grila (vezi
    ''' <see cref="PartenerulCurent"/>). Cand nici linia nu numeste unul, campurile se golesc
    ''' si se sting: mai bine goale decat ramase de la selectia dinainte, care ar arata ca ale
    ''' randului de acum.
    ''' </summary>
    Private Sub ActualizeazaAntetBeneficiar()
        Dim p As OrdDraftPart = PartenerulCurent()
        _suspenda = True
        Try
            txtDenBene.Text = If(p Is Nothing, String.Empty, p.DenBene)
            txtCodFiscal.Text = If(p Is Nothing, String.Empty, p.CodFiscal)
            txtContIban.Text = If(p Is Nothing, String.Empty, p.ContIban)
            txtBanca.Text = If(p Is Nothing, String.Empty, p.Banca)

            ' Alegatorul de partener: intrarile sunt partenerii DEJA prezenti pe liniile
            ' beneficiarului. Nomenclatorul intreg (Access: RowSource peste `Parteneri` cu
            ' `CodPartener <> 'XXX'`, `IdPartener <> 0`, `CodFiscal <> ''`) ar cere o ruta de
            ' citire pe care felia asta nu o are; ce se poate arata fara sa se inventeze nimic
            ' este ce poarta chiar liniile. Consemnat in worklog ca limita cunoscuta.
            cboCodPartener.Items.Clear()
            If p IsNot Nothing AndAlso _draft IsNot Nothing Then
                For Each nume As String In _draft.LiniiPentru(p.Cheie).
                        Select(Function(l) l.CodPartener).
                        Where(Function(n) Not String.IsNullOrWhiteSpace(n)).
                        Distinct(StringComparer.CurrentCultureIgnoreCase).
                        OrderBy(Function(n) n, StringComparer.CurrentCulture)
                    cboCodPartener.Items.Add(nume)
                Next
                If cboCodPartener.Items.Count > 0 Then cboCodPartener.SelectedIndex = 0
            End If

            Dim activ As Boolean = p IsNot Nothing
            txtDenBene.Enabled = activ
            txtCodFiscal.Enabled = activ
            txtContIban.Enabled = activ
            txtBanca.Enabled = activ
            cboCodPartener.Enabled = activ AndAlso cboCodPartener.Items.Count > 0
        Finally
            _suspenda = False
        End Try
    End Sub

    ''' <summary>
    ''' Beneficiarul de care tin campurile de sus, cu DOUA surse, in ordinea asta:
    '''
    ''' <para>1. lista din stanga, cand ea numeste un beneficiar (bifa stinsa, rand nesintetic);</para>
    '''
    ''' <para>2. altfel — pe randul «&lt; TOȚI BENEFICIARII &gt;» si pe toata ramura
    ''' «clasificatii», unde lista din stanga NU numeste niciun beneficiar — beneficiarul
    ''' LINIEI selectate in grila. Fara treapta a doua, campurile ramaneau goale tocmai in cele
    ''' doua moduri in care operatorul umbla prin liniile mai multor beneficiari deodata, desi
    ''' randul din grila spune fara echivoc al cui e (cerinta operatorului, 02.09.2026).</para>
    '''
    ''' <para><c>Nothing</c> doar cand nici linia nu numeste unul (grila goala).</para>
    ''' </summary>
    Private Function PartenerulCurent() As OrdDraftPart
        If _draft Is Nothing Then Return Nothing
        If Not chkClsf.Checked AndAlso _cheieSelectata <> 0 Then
            Return _draft.PartenerDupaCheie(_cheieSelectata)
        End If
        Dim l As OrdDraftLinie = LiniaCurenta()
        If l Is Nothing Then Return Nothing
        Return _draft.PartenerDupaCheie(l.CheiePart)
    End Function

    ''' <summary>Linia selectata acum in grila din dreapta; <c>Nothing</c> pe grila goala.</summary>
    Private Function LiniaCurenta() As OrdDraftLinie
        Dim r As KBotDataRow = grdLinii.CurrentRow
        If r Is Nothing Then Return Nothing
        Return TryCast(r.Tag, OrdDraftLinie)
    End Function

    Private Sub CampBeneficiar_TextChanged(sender As Object, e As EventArgs) _
        Handles txtDenBene.TextChanged, txtCodFiscal.TextChanged,
                txtContIban.TextChanged, txtBanca.TextChanged
        Try
            If _suspenda Then Return
            Dim p As OrdDraftPart = PartenerulCurent()
            If p Is Nothing Then Return

            p.DenBene = txtDenBene.Text
            p.CodFiscal = txtCodFiscal.Text
            p.ContIban = txtContIban.Text
            p.Banca = txtBanca.Text

            ' Numele se vede si in lista din stanga: se rescrie pe loc, fara repopulare (care
            ' ar muta cursorul de sub degetul operatorului la fiecare litera tastata). Randul
            ' se cauta dupa cheie, nu se ia cel CURENT — de cand campurile pot tine de linia
            ' selectata, beneficiarul editat nu mai e neaparat randul pe care sta cursorul.
            If Not chkClsf.Checked Then
                For i As Integer = 0 To grdStanga.Rows.Count - 1
                    Dim r As KBotDataRow = grdStanga.Rows(i)
                    If Not (TypeOf r.Tag Is Integer) OrElse CInt(r.Tag) <> p.Cheie Then Continue For
                    r(COL_ETICHETA) = p.DenBene
                    grdStanga.InvalidateRow(i)
                    Exit For
                Next
            Else
                ' Pe ramura «clasificatii» coloana a doua a grilei ARATA IBAN-ul: se rescrie
                ' odata cu campul, ca cele doua locuri sa nu poata spune lucruri diferite.
                For i As Integer = 0 To grdLinii.Rows.Count - 1
                    Dim l As OrdDraftLinie = TryCast(grdLinii.Rows(i).Tag, OrdDraftLinie)
                    If l Is Nothing OrElse l.CheiePart <> p.Cheie Then Continue For
                    grdLinii.Rows(i)(COL_SSI_IBAN) = p.ContIban
                    grdLinii.InvalidateRow(i)
                Next
            End If

            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.CampBeneficiar_TextChanged", ex)
        End Try
    End Sub

    ' ── Grila liniilor ───────────────────────────────────────────────────────────────────

    ''' <summary>Liniile care trec de selectia curenta, in ordinea ceruta de modul listei.</summary>
    Private Function LiniiVizibile() As List(Of OrdDraftLinie)
        If _draft Is Nothing Then Return New List(Of OrdDraftLinie)()

        Dim sursa As IEnumerable(Of OrdDraftLinie) = _draft.Linii
        If _cheieSelectata <> 0 Then
            If chkClsf.Checked Then
                sursa = sursa.Where(Function(l) l.IdClsf = _cheieSelectata)
            Else
                sursa = sursa.Where(Function(l) l.CheiePart = _cheieSelectata)
            End If
        End If

        ' Ordinea din Access: pe ramura «clasificatii» dupa CodSSI, altfel dupa numele
        ' beneficiarului. `Cheie` tine locul lui `ID` din `OrderBy "…, ID"`.
        If chkClsf.Checked Then
            Return sursa.OrderBy(Function(l) l.CodSsi, StringComparer.CurrentCulture).
                         ThenBy(Function(l) l.Cheie).ToList()
        End If
        Return sursa.OrderBy(Function(l) NumeBeneficiar(l), StringComparer.CurrentCulture).
                     ThenBy(Function(l) l.Cheie).ToList()
    End Function

    Private Function NumeBeneficiar(l As OrdDraftLinie) As String
        If _draft Is Nothing Then Return String.Empty
        Dim p As OrdDraftPart = _draft.PartenerDupaCheie(l.CheiePart)
        Return If(p Is Nothing, String.Empty, p.DenBene)
    End Function

    Private Sub AplicaFiltrulGrilei()
        _suspenda = True
        Try
            grdLinii.BeginUpdate()
            Try
                grdLinii.ClearRows()
                For Each l As OrdDraftLinie In LiniiVizibile()
                    Dim r As KBotDataRow = grdLinii.AddRow()
                    r.Tag = l
                    ScrieRand(r, l)
                Next
            Finally
                grdLinii.EndUpdate()
            End Try
            grdLinii.CurrentRowIndex = If(grdLinii.Rows.Count > 0, 0, -1)
        Finally
            _suspenda = False
        End Try
    End Sub

    ' Coloana a doua isi schimba SURSA odata cu bifa, nu doar antetul (D10, efectul al treilea).
    Private Sub ScrieRand(r As KBotDataRow, l As OrdDraftLinie)
        r(COL_CLSF) = l.Clsf
        r(COL_SSI_IBAN) = If(chkClsf.Checked, IbanBeneficiar(l), l.CodSsi)
        r(COL_EXPLICATIE) = l.Explicatie
        r(COL_TOTAL_RECEPTII) = l.TotalReceptii
        r(COL_PLATI_ANT) = l.PlatiAnt
        r(COL_VALOARE) = l.Valoare
        r(COL_RAMAS) = l.Ramas
    End Sub

    Private Function IbanBeneficiar(l As OrdDraftLinie) As String
        If _draft Is Nothing Then Return String.Empty
        Dim p As OrdDraftPart = _draft.PartenerDupaCheie(l.CheiePart)
        Return If(p Is Nothing, String.Empty, p.ContIban)
    End Function

    ''' <summary>
    ''' Randul selectat in grila numeste un beneficiar, deci benzile de sus il urmeaza — asta
    ''' e ce face campurile sa se umple si pe «&lt; TOȚI BENEFICIARII &gt;» si pe ramura
    ''' «clasificatii». Cand lista din stanga numeste deja unul, recalculul da acelasi raspuns.
    ''' </summary>
    Private Sub GrdLinii_SelectionChanged(sender As Object, e As EventArgs) Handles grdLinii.SelectionChanged
        Try
            If _suspenda Then Return
            ActualizeazaAntetBeneficiar()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.GrdLinii_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' O celula editata -&gt; graful. «Ramas» se RECALCULEAZA aici, dupa formula din
    ''' <c>Adauga_Ord_Tbl</c>, si se rescrie in grila — nu se lasa cifra veche langa valoarea
    ''' noua.
    ''' </summary>
    Private Sub GrdLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles grdLinii.CellValueChanged
        Try
            If _suspenda Then Return
            If e Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= grdLinii.Rows.Count Then Return
            Dim r As KBotDataRow = grdLinii.Rows(e.RowIndex)
            Dim l As OrdDraftLinie = TryCast(r.Tag, OrdDraftLinie)
            If l Is Nothing Then Return

            Select Case e.ColumnKey
                Case COL_VALOARE
                    l.Valoare = CitesteNumar(e.NewValue, l.Valoare)
                    l.Ramas = Math.Round(l.TotalReceptii - l.PlatiAnt - l.Valoare, 2)
                    _suspenda = True
                    Try
                        r(COL_VALOARE) = l.Valoare
                        r(COL_RAMAS) = l.Ramas
                    Finally
                        _suspenda = False
                    End Try
                    grdLinii.InvalidateRow(e.RowIndex)
                Case COL_EXPLICATIE
                    l.Explicatie = If(e.NewValue Is Nothing, String.Empty, e.NewValue.ToString())
                Case Else
                    ' Restul coloanelor sunt read-only in designer; nimic de scris.
                    Return
            End Select

            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.GrdLinii_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Text/obiect -&gt; numar, cu cultura romaneasca SI cu cea invarianta (operatorul poate
    ''' tasta si «1234.56»). Un text de neinteles pastreaza valoarea dinainte — o suma
    ''' inventata s-ar scrie tacut in document.
    ''' </summary>
    Private Shared Function CitesteNumar(valoare As Object, implicit As Double) As Double
        If valoare Is Nothing Then Return implicit
        If TypeOf valoare Is Double Then Return DirectCast(valoare, Double)
        Dim text As String = valoare.ToString()
        If String.IsNullOrWhiteSpace(text) Then Return 0.0R
        Dim rezultat As Double
        If Double.TryParse(text, NumberStyles.Any, _roCulture, rezultat) Then Return rezultat
        If Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, rezultat) Then Return rezultat
        Return implicit
    End Function

    ''' <summary>Grilele si campurile se auto-temeaza (IThemedControl, prin cascada
    ''' <c>ThemeManager</c>); aici raman fundalul paginii si etichetele simple.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor
            tlyStanga.BackColor = p.SurfaceAltColor
            tlyDreapta.BackColor = p.SurfaceAltColor
            tlyAntetBene.BackColor = p.SurfaceAltColor

            chkClsf.ForeColor = p.TextColor
            chkClsf.BackColor = Color.Transparent
            For Each capt As Label In New Label() {lblDenBene, lblCodPartener, lblCodFiscal,
                                                   lblContIban, lblBanca}
                capt.ForeColor = p.TextDimColor
                capt.BackColor = Color.Transparent
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("OrdBeneficiariPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
