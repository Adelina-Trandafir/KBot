Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Documente justificative» a editorului de ordonantare (felia 0049) — portul lui
''' <c>frmFX_ORD_DOC</c> + <c>_BENE</c> + <c>_TXT</c> + <c>_ATT</c>.
'''
''' <para><b>Ce inseamna randul sintetic.</b> «&lt; TOTI BENEFICIARII &gt;» nu e un filtru
''' «arata tot»: el arata DOAR ce au TOTI beneficiarii, iar ce se adauga pe el il primesc TOTI
''' (cerinta operatorului, 02.09.2026). «Comun» inseamna doua lucruri deodata, si amandoua se
''' arata acolo:</para>
'''
''' <para>1. randurile fara legatura de beneficiar (<c>IDORDPARTP</c> NULL) — cum le tinea
''' Access, documentele INTREGII ordonantari;</para>
'''
''' <para>2. randurile care exista, cu aceeasi valoare, la FIECARE beneficiar — asa cum
''' generarea chiar le produce (<c>Adauga_Ord_Doc</c> agata fiecare document de un beneficiar,
''' deci un filtru «doar NULL» arata o grila goala pe date proaspat generate). Cele N copii se
''' strang intr-UN singur rand pe ecran; ce se scrie in el se scrie in toate N, si stergerea
''' le sterge pe toate N. Interogarea e chiar cea lasata comentata in
''' <c>frmFX_ORD_DOC.Form_Load</c> (<c>HAVING Count(*) = (SELECT COUNT(*) FROM (SELECT DISTINCT
''' IDORDPART …))</c>) — intentia exista in Access, dar nu ajunsese sa fie folosita.</para>
'''
''' <para>Pe un beneficiar anume se vad cele fara legatura SI ale lui, fiecare pe randul ei.</para>
'''
''' <para><b>Cu un SINGUR beneficiar randul sintetic lipseste cu totul</b> (cerinta
''' operatorului, 03.09.2026): acolo «toti» si «el» sunt acelasi lucru, iar doua randuri care
''' spun acelasi lucru nu fac decat sa intrebe operatorul pe care sa dea clic. Randul lui
''' arata si documentele fara legatura, grila ramane editabila (nu mai strange nimic), si tot
''' de acolo se pot si sterge — refuzul «nu e al beneficiarului curent» se retrage odata cu
''' randul sintetic, fiindca n-ar mai avea unde sa trimita.</para>
'''
''' <para><b>Randul sintetic nu se editeaza in grila</b> (cerinta operatorului, 02.09.2026): el
''' arata o STRANGERE a mai multor randuri de date, si o celula editata acolo ar trebui sa se
''' scrie in toate deodata — un gest mic cu urmari mari, greu de vazut si imposibil de intors.
''' Grila trece pe doar-citire, iar adaugarea ramane deschisa: textul se cere in
''' <see cref="OrdTextForm"/> si se imparte la toti beneficiarii. Corecturile se fac pe
''' beneficiarul lor, unde randul e al lui singur.</para>
'''
''' <para><b>Un document e UNIC la un beneficiar</b> (aceeasi cerinta): aceleasi trei coloane
''' nu se repeta la acelasi beneficiar. Adaugarea sare peste beneficiarii care au deja valoarea
''' (asa se completeaza si golurile unui rand aproape-comun), editarea refuza sa faca doua
''' randuri identice, iar generarea de pe server nu mai propune duplicate — <c>Adauga_Ord_Doc</c>
''' scria cate un document pentru FIECARE rand-sursa, deci trei plati cu aceeasi descriere
''' dadeau trei randuri identice. Ordonantarile vechi le pot avea inca, deci strangerea pe
''' straturi de mai jos ramane.</para>
'''
''' <para><b>Text sau fisier.</b> Un rand <c>FX_ORD_DOC</c> fara <c>NumeDoc</c> e un rand TEXT;
''' cu <c>NumeDoc</c> completat, e un fisier anexat, ai carui octeti stau codificati base64 in
''' <c>DocJust</c> — exact ce facea <c>ProceseazaFisiere</c> din <c>frmFX_ORD_DOC_ATT</c>.
''' Mecanismul asta ramane neschimbat: coloana e vie (719 randuri in dump), deci nu se muta
''' nicaieri in felia asta. Doar capturile de ecran din <c>FX_ORD_ATT</c> au primit tabela de
''' octeti noua (pagina «Atasamente»), fiindca acolo tabela era goala.</para>
'''
''' <para><b><c>btnSav</c> a disparut</b> (D2): popup-ul Access avea propria salvare; aici
''' exista O SINGURA salvare, a formularului, pentru tot graful.</para>
''' </summary>
Public Class OrdDocumentePage
    Implements IOrdEditPage, IThemedControl

    Private Const COL_ETICHETA As String = "eticheta"
    Private Const COL_DOC_JUST As String = "doc_just"
    Private Const COL_NUME_DOC As String = "nume_doc"
    Private Const COL_TIP_DOC As String = "tip_doc"

    ''' <summary>Randul sintetic — ce au TOTI beneficiarii, nu «toate documentele». Vezi nota
    ''' de clasa.</summary>
    Private Const TOTI_BENEFICIARII As String = "< TOȚI BENEFICIARII >"

    ''' <summary>Ce se lipeste la titlul grilei cat timp e ales randul sintetic — altfel
    ''' doar-citirea ar parea o grila stricata.</summary>
    Private Const SUFIX_COMUN As String = "  ·  rânduri comune (doar citire)"

    Private _draft As OrdDraft
    ' Beneficiarul selectat (identitate reala sau temporara). 0 = randul sintetic.
    Private _cheieBene As Integer
    Private _suspenda As Boolean
    ' Titlurile scrise in designer, pastrate ca sa li se poata pune si scoate sufixul.
    Private ReadOnly _titluText As String
    Private ReadOnly _titluFisiere As String

    Public Event DraftModificat As EventHandler Implements IOrdEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
        '_titluText = lblText.Text
        '_titluFisiere = lblFisiere.Text
    End Sub

    Public ReadOnly Property PageKey As String Implements IOrdEditPage.PageKey
        Get
            Return "documente"
        End Get
    End Property

    Public Sub SetDraft(draft As OrdDraft) Implements IOrdEditPage.SetDraft
        Try
            _draft = draft
            _cheieBene = 0
            PopuleazaBeneficiari()
            ReumpleListele()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.SetDraft", ex)
            Throw
        End Try
    End Sub

    ' ── Selectorul de beneficiar ─────────────────────────────────────────────────────────

    Private Sub PopuleazaBeneficiari()
        Dim anterior As Integer = _cheieBene
        _suspenda = True
        Try
            grdBene.BeginUpdate()
            Try
                grdBene.ClearRows()
                If AreRandSintetic() Then
                    Dim sintetic As KBotDataRow = grdBene.AddRow()
                    sintetic.Tag = 0
                    sintetic(COL_ETICHETA) = TOTI_BENEFICIARII
                End If
                If _draft IsNot Nothing Then
                    For Each p As OrdDraftPart In _draft.Parteneri.
                            OrderBy(Function(x) x.DenBene, StringComparer.CurrentCulture)
                        Dim r As KBotDataRow = grdBene.AddRow()
                        r.Tag = p.Cheie
                        r(COL_ETICHETA) = p.DenBene
                    Next
                End If
            Finally
                grdBene.EndUpdate()
            End Try

            Dim index As Integer = 0
            For i As Integer = 0 To grdBene.Rows.Count - 1
                If TypeOf grdBene.Rows(i).Tag Is Integer AndAlso
                   CInt(grdBene.Rows(i).Tag) = anterior Then
                    index = i
                    Exit For
                End If
            Next
            grdBene.CurrentRowIndex = If(grdBene.Rows.Count > 0, index, -1)
            _cheieBene = CheiaBeneficiaruluiCurent()
        Finally
            _suspenda = False
        End Try
    End Sub

    ''' <summary>
    ''' Are lista din stanga randul sintetic? NU il are cand ordonantarea are EXACT un
    ''' beneficiar: acolo «toti» si «el» sunt acelasi lucru, iar doua randuri care spun
    ''' acelasi lucru nu fac decat sa intrebe operatorul pe care sa dea clic (cerinta
    ''' operatorului, 03.09.2026).
    '''
    ''' <para>Proba e pe UNU, nu pe «cel putin doi»: fara niciun beneficiar randul sintetic
    ''' e singurul lucru de care se poate agata cursorul, deci ramane.</para>
    ''' </summary>
    Private Function AreRandSintetic() As Boolean
        Return _draft Is Nothing OrElse _draft.Parteneri.Count <> 1
    End Function

    Private Function CheiaBeneficiaruluiCurent() As Integer
        Dim r As KBotDataRow = grdBene.CurrentRow
        If r Is Nothing OrElse Not (TypeOf r.Tag Is Integer) Then Return 0
        Return CInt(r.Tag)
    End Function

    Private Sub GrdBene_SelectionChanged(sender As Object, e As EventArgs) Handles grdBene.SelectionChanged
        Try
            If _suspenda Then Return
            _cheieBene = CheiaBeneficiaruluiCurent()
            ReumpleListele()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.GrdBene_SelectionChanged", ex)
        End Try
    End Sub

    ' ── Filtrul, exact ca in Access ──────────────────────────────────────────────────────

    ''' <summary>
    ''' Ce se vede acum, ca GRUPURI: un grup = un rand pe ecran, iar randul poarta toate
    ''' randurile de date pe care le reprezinta. Pe un beneficiar fiecare grup are un singur
    ''' element; pe randul sintetic, un grup poate avea cate un element per beneficiar.
    ''' </summary>
    Private Function GrupuriVizibile(cuFisier As Boolean) As List(Of List(Of OrdDraftDoc))
        Dim rezultat As New List(Of List(Of OrdDraftDoc))()
        If _draft Is Nothing Then Return rezultat

        ' Impartirea intre cele doua grile se face pe `NumeDoc` SINGUR, ca in Access
        ' (`frmFX_ORD_DOC.Form_Load`: `If IsNull(!NumeDoc)` -> temp1, altfel temp2).
        ' `EsteText` NU serveste aici: el cere in plus `DocJust` necompletat, deci un rand text
        ' proaspat adaugat, inca gol, ar aluneca in grila FISIERELOR — acolo unde operatorul
        ' tocmai nu se uita. `EsteText` ramane ce e: proba de VALIDARE din `btnSav`
        ' (`IsNull(NumeDoc) And Not IsNull(DocJust)`), folosita de `OrdEditForm`.
        Dim sursa As List(Of OrdDraftDoc) = _draft.Documente.
            Where(Function(d) String.IsNullOrWhiteSpace(d.NumeDoc) <> cuFisier).ToList()

        ' Pe un beneficiar: cele fara legatura SI ale lui — Access: `TIDORDPART IS NULL OR
        ' TIDORDPART = X`. Fiecare pe randul ei, ca sa se poata edita si sterge separat.
        If _cheieBene <> 0 Then
            For Each d As OrdDraftDoc In sursa.
                    Where(Function(x) CheiaPart(x) = 0 OrElse CheiaPart(x) = _cheieBene).
                    OrderBy(Function(x) CheiaPart(x)).ThenBy(Function(x) x.Cheie)
                rezultat.Add(New List(Of OrdDraftDoc) From {d})
            Next
            Return rezultat
        End If

        ' Randul sintetic, intelesul 1: randurile INTREGII ordonantari (legatura NULL).
        For Each d As OrdDraftDoc In sursa.Where(Function(x) CheiaPart(x) = 0).
                OrderBy(Function(x) x.Cheie)
            rezultat.Add(New List(Of OrdDraftDoc) From {d})
        Next

        ' …intelesul 2: ce exista, cu aceeasi valoare, la FIECARE beneficiar.
        Dim cheiBene As List(Of Integer) = CheileBeneficiarilor()
        If cheiBene.Count = 0 Then Return rezultat

        For Each grup In sursa.Where(Function(x) CheiaPart(x) <> 0).
                GroupBy(Function(x) CheieValoare(x), StringComparer.Ordinal).
                OrderBy(Function(g) g.Min(Function(x) x.Cheie))
            ' Copiile fiecarui beneficiar, in ordine stabila. Un beneficiar fara nicio copie
            ' inseamna ca valoarea NU e comuna, deci grupul nu se arata deloc.
            Dim peBene As New List(Of List(Of OrdDraftDoc))()
            For Each cheie As Integer In cheiBene
                Dim ale As List(Of OrdDraftDoc) = grup.
                    Where(Function(x) CheiaPart(x) = cheie).
                    OrderBy(Function(x) x.Cheie).ToList()
                If ale.Count = 0 Then
                    peBene.Clear()
                    Exit For
                End If
                peBene.Add(ale)
            Next
            If peBene.Count = 0 Then Continue For

            ' Cate randuri comune poate sustine grupul: cel mai putin bogat beneficiar da
            ' numarul. Doua randuri identice adaugate pe randul sintetic raman DOUA randuri
            ' pe ecran — altfel al doilea «adauga» n-ar parea sa faca nimic.
            Dim straturi As Integer = peBene.Min(Function(l) l.Count)
            For i As Integer = 0 To straturi - 1
                Dim strat As Integer = i
                rezultat.Add(peBene.Select(Function(l) l(strat)).ToList())
            Next
        Next
        Return rezultat
    End Function

    ''' <summary>Identitatile beneficiarilor, in ordine stabila — ordinea in care se aseaza
    ''' copiile unui rand comun si ordinea in care se creeaza la adaugare.</summary>
    Private Function CheileBeneficiarilor() As List(Of Integer)
        If _draft Is Nothing Then Return New List(Of Integer)()
        Return _draft.Parteneri.Select(Function(p) p.Cheie).Distinct().OrderBy(Function(k) k).ToList()
    End Function

    ''' <summary>Ce face doua randuri de pe doi beneficiari «acelasi document»: exact cele trei
    ''' coloane pe care le grupa interogarea din <c>frmFX_ORD_DOC.Form_Load</c>. Separatorul e
    ''' <c>vbNullChar</c> fiindca nu poate aparea in niciuna dintre ele.</summary>
    Private Shared Function CheieValoare(d As OrdDraftDoc) As String
        Return d.DocJust & vbNullChar & d.NumeDoc & vbNullChar & d.TipDoc
    End Function

    ''' <summary>Identitatea beneficiarului de care atarna documentul; 0 = document comun.</summary>
    Private Shared Function CheiaPart(d As OrdDraftDoc) As Integer
        If d.Idordpartp > 0 Then Return d.Idordpartp
        Return d.PartTempId
    End Function

    ''' <summary>Grupul din spatele randului dat; <c>Nothing</c> daca randul nu poarta unul.</summary>
    Private Shared Function GrupulRandului(grila As KBotDataView, index As Integer) As List(Of OrdDraftDoc)
        If index < 0 OrElse index >= grila.Rows.Count Then Return Nothing
        Dim grup As List(Of OrdDraftDoc) = TryCast(grila.Rows(index).Tag, List(Of OrdDraftDoc))
        If grup Is Nothing OrElse grup.Count = 0 Then Return Nothing
        Return grup
    End Function

    Private Sub ReumpleListele()
        _suspenda = True
        Try
            ' Pe randul sintetic un rand de ecran sta pe N randuri de date: nu se editeaza in
            ' grila, se adauga prin dialog. Grila fisierelor e oricum doar-citire (numele si
            ' extensia vin din fisierul ales), deci ei nu i se schimba nimic.
            Dim comun As Boolean = _cheieBene = 0
            grdText.ReadOnlyGrid = comun
            'lblText.Text = If(comun, _titluText & SUFIX_COMUN, _titluText)
            'lblFisiere.Text = If(comun, _titluFisiere & SUFIX_COMUN, _titluFisiere)

            grdText.BeginUpdate()
            Try
                grdText.ClearRows()
                For Each grup As List(Of OrdDraftDoc) In GrupuriVizibile(cuFisier:=False)
                    Dim r As KBotDataRow = grdText.AddRow()
                    r.Tag = grup
                    r(COL_DOC_JUST) = grup(0).DocJust
                Next
            Finally
                grdText.EndUpdate()
            End Try
            grdText.CurrentRowIndex = If(grdText.Rows.Count > 0, 0, -1)

            grdFisiere.BeginUpdate()
            Try
                grdFisiere.ClearRows()
                For Each grup As List(Of OrdDraftDoc) In GrupuriVizibile(cuFisier:=True)
                    Dim r As KBotDataRow = grdFisiere.AddRow()
                    r.Tag = grup
                    r(COL_NUME_DOC) = grup(0).NumeDoc
                    r(COL_TIP_DOC) = grup(0).TipDoc
                Next
            Finally
                grdFisiere.EndUpdate()
            End Try
            grdFisiere.CurrentRowIndex = If(grdFisiere.Rows.Count > 0, 0, -1)
        Finally
            _suspenda = False
        End Try
    End Sub

    ' ── Randurile text ───────────────────────────────────────────────────────────────────

    Private Sub GrdText_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles grdText.CellValueChanged
        Try
            If _suspenda Then Return
            If e Is Nothing Then Return
            Dim grup As List(Of OrdDraftDoc) = GrupulRandului(grdText, e.RowIndex)
            If grup Is Nothing Then Return
            If e.ColumnKey <> COL_DOC_JUST Then Return

            Dim text As String = If(e.NewValue Is Nothing, String.Empty, e.NewValue.ToString())

            ' Un document e unic la un beneficiar: o editare care ar face doua randuri identice
            ' se refuza pe loc, nu la salvare — acolo unul dintre ele s-ar pierde in tacere.
            Dim ciocnire As OrdDraftDoc = grup.
                Select(Function(d) PrimulDuplicat(d, text, d.NumeDoc, d.TipDoc)).
                FirstOrDefault(Function(x) x IsNot Nothing)
            If ciocnire IsNot Nothing Then
                _suspenda = True
                Try
                    grdText.Rows(e.RowIndex)(COL_DOC_JUST) = grup(0).DocJust
                Finally
                    _suspenda = False
                End Try
                grdText.InvalidateRow(e.RowIndex)
                MessageBox.Show(Me,
                    "Beneficiarul are deja un document justificativ cu textul ăsta. " &
                    "Un document nu se repetă la același beneficiar.",
                    "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Un rand comun sta pe N randuri de date: textul se scrie in TOATE, altfel ar
            ' inceta sa mai fie comun in tacere, chiar in clipa in care e editat.
            For Each d As OrdDraftDoc In grup
                d.DocJust = text
            Next
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.GrdText_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Textul se cere in dialog, nu se scrie intr-un rand gol adaugat in grila: pe randul
    ''' sintetic grila e doar-citire, iar pe un beneficiar un rand gol nu se deosebeste de unul
    ''' uitat. Randul se naste completat, si de aici se poate spune pe loc daca valoarea exista
    ''' deja.
    ''' </summary>
    Private Sub BtnAdaugaText_Click(sender As Object, e As EventArgs) Handles btnAdaugaText.Click
        Try
            If _draft Is Nothing Then Return

            Dim text As String
            Using dlg As New OrdTextForm(pentruToti:=_cheieBene = 0)
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
                text = dlg.Textul
            End Using

            Dim adaugate As List(Of OrdDraftDoc) = AdaugaRanduri(text, String.Empty, "text")
            If adaugate.Count = 0 Then
                MessageBox.Show(Me,
                    If(_cheieBene = 0,
                       "Toți beneficiarii au deja un document justificativ cu textul ăsta.",
                       "Beneficiarul are deja un document justificativ cu textul ăsta."),
                    "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ReumpleListele()
            SelecteazaInGrila(grdText, adaugate)
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.BtnAdaugaText_Click", ex)
            MessageBox.Show(Me, "Rândul nu a putut fi adăugat. Detalii în jurnalul de erori.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnStergeText_Click(sender As Object, e As EventArgs) Handles btnAdaugaText.Click
        Try
            StergeRandul(grdText)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.BtnStergeText_Click", ex)
        End Try
    End Sub

    ' ── Fisierele anexate ────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Portul lui <c>ProceseazaFisiere</c>: fiecare fisier ales devine un rand
    ''' <c>FX_ORD_DOC</c> cu <c>NumeDoc</c> = numele, <c>TipDoc</c> = extensia si
    ''' <c>DocJust</c> = continutul codificat base64.
    ''' </summary>
    Private Sub BtnAdaugaFisier_Click(sender As Object, e As EventArgs) Handles btnAdaugaFisier.Click
        Try
            If _draft Is Nothing Then Return
            If dlgFisiere.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim adaugate As Integer = 0
            Dim sarite As New List(Of String)()
            For Each cale As String In dlgFisiere.FileNames
                Dim octeti As Byte() = File.ReadAllBytes(cale)
                Dim nume As String = Path.GetFileName(cale)
                Dim ext As String = Path.GetExtension(cale)
                If ext.StartsWith("."c) Then ext = ext.Substring(1)

                Dim noi As Integer = AdaugaRanduri(Convert.ToBase64String(octeti), nume, ext).Count
                adaugate += noi
                If noi = 0 Then sarite.Add(nume)
            Next

            If sarite.Count > 0 Then
                ' Acelasi fisier de doua ori la acelasi beneficiar nu inseamna nimic in plus, si
                ' nici nu se mai poate deosebi dupa aceea. Se spune care s-au sarit.
                MessageBox.Show(Me,
                    "Fișierele următoare erau deja anexate și nu s-au adăugat încă o dată:" &
                    vbCrLf & String.Join(vbCrLf, sarite),
                    "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            If adaugate = 0 Then Return
            ReumpleListele()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            ' Granita de UI peste I/O de fisier: se logheaza SI se arata; un throw de aici ar
            ' cadea pe firul de interfata.
            GlobalErrorLog.Write("OrdDocumentePage.BtnAdaugaFisier_Click", ex)
            MessageBox.Show(Me, "Fișierul nu a putut fi citit: " & ex.Message,
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnStergeFisier_Click(sender As Object, e As EventArgs) Handles btnStergeFisier.Click
        Try
            StergeRandul(grdFisiere)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.BtnStergeFisier_Click", ex)
        End Try
    End Sub

    ' ── Comun ────────────────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Adauga randul si il leaga de cine trebuie: pe un beneficiar selectat, un singur rand,
    ''' al lui; pe randul sintetic, cate un rand pentru FIECARE beneficiar — asa primesc TOTI
    ''' valoarea (cerinta operatorului, 02.09.2026), si asa o si vede pe ea calea de citire a
    ''' vederii 0033, care aduce documentele prin <c>FX_ORD_DOC.IDORDPARTP = FX_ORD_TBL.IDORDPARTP</c>
    ''' si deci nu vede niciodata un rand cu legatura NULL.
    ''' </summary>
    ''' <remarks>
    ''' <para>Fara niciun beneficiar nu exista de cine sa atarne randul, deci ramane unul singur,
    ''' al intregii ordonantari (legatura NULL) — cazul in care Access scria tot NULL
    ''' (<c>Form_BeforeInsert</c> punea legatura doar cand selectia nu era «*»).</para>
    ''' <para>Beneficiarii care AU deja valoarea se sar: un document e unic la un beneficiar.
    ''' Nu e o refuzare a intregii adaugari — pe un rand aproape-comun, caruia ii lipseste
    ''' valoarea la doi beneficiari din zece, exact asta umple golurile si il face comun.</para>
    ''' </remarks>
    Private Function AdaugaRanduri(docJust As String, numeDoc As String, tipDoc As String) As List(Of OrdDraftDoc)
        Dim adaugate As New List(Of OrdDraftDoc)()
        If _draft Is Nothing Then Return adaugate

        Dim tinte As List(Of Integer)
        If _cheieBene <> 0 Then
            tinte = New List(Of Integer) From {_cheieBene}
        Else
            tinte = CheileBeneficiarilor()
            If tinte.Count = 0 Then tinte.Add(0)
        End If

        For Each cheie As Integer In tinte
            If ExistaDeja(cheie, docJust, numeDoc, tipDoc) Then Continue For

            ' `UrmatorulTempId` citeste listele, deci se cere DUPA fiecare adaugare: cerut o
            ' singura data inainte de bucla, ar da acelasi id tuturor copiilor.
            Dim d As New OrdDraftDoc() With {
                .TempId = _draft.UrmatorulTempId(),
                .DocJust = docJust,
                .NumeDoc = numeDoc,
                .TipDoc = tipDoc}
            If cheie > 0 Then
                d.Idordpartp = cheie
            ElseIf cheie < 0 Then
                d.PartTempId = cheie
            End If
            _draft.Documente.Add(d)
            adaugate.Add(d)
        Next
        Return adaugate
    End Function

    ''' <summary>Are beneficiarul dat deja un document cu exact valorile astea?</summary>
    Private Function ExistaDeja(cheieBene As Integer, docJust As String, numeDoc As String,
                               tipDoc As String) As Boolean
        Return PrimulDuplicat(Nothing, docJust, numeDoc, tipDoc, cheieBene) IsNot Nothing
    End Function

    ''' <summary>
    ''' Primul document al aceluiasi beneficiar care ar avea valorile date — sau <c>Nothing</c>
    ''' daca valoarea e libera acolo. <paramref name="afaraDe"/> e randul care tocmai se editeaza:
    ''' el nu se ciocneste de el insusi.
    ''' </summary>
    Private Function PrimulDuplicat(afaraDe As OrdDraftDoc, docJust As String, numeDoc As String,
                                    tipDoc As String,
                                    Optional cheieBene As Integer = Integer.MinValue) As OrdDraftDoc
        If _draft Is Nothing Then Return Nothing
        Dim cheie As Integer = If(cheieBene = Integer.MinValue, CheiaPart(afaraDe), cheieBene)
        Dim valoare As String = docJust & vbNullChar & numeDoc & vbNullChar & tipDoc
        Return _draft.Documente.FirstOrDefault(
            Function(x) Not ReferenceEquals(x, afaraDe) AndAlso
                        CheiaPart(x) = cheie AndAlso
                        String.Equals(CheieValoare(x), valoare, StringComparison.Ordinal))
    End Function

    ''' <summary>
    ''' Sterge randul curent al grilei date — adica TOATE randurile de date din spatele lui,
    ''' fiindca un rand comun le reprezinta pe toate N. Un document al INTREGII ordonantari
    ''' (legatura NULL) nu se poate sterge cat timp e selectat un beneficiar anume — mesajul e
    ''' cel din Access (<c>dtnDel_Click</c>: «Documentul selectat nu este al Beneficiarului
    ''' curent!»), fiindca altfel operatorul ar sterge de sub ceilalti fara sa-si dea seama.
    ''' </summary>
    Private Sub StergeRandul(grila As KBotDataView)
        If _draft Is Nothing Then Return
        Dim grup As List(Of OrdDraftDoc) = GrupulRandului(grila, grila.CurrentRowIndex)
        If grup Is Nothing Then Return

        ' Fara rand sintetic (un singur beneficiar) refuzul n-ar avea unde sa trimita
        ' operatorul, si nici n-ar avea de ce: acolo «al intregii ordonantari» si «al lui»
        ' inseamna acelasi lucru.
        If AreRandSintetic() AndAlso _cheieBene <> 0 AndAlso grup.Any(Function(d) CheiaPart(d) = 0) Then
            MessageBox.Show(Me,
                "Documentul selectat nu este al beneficiarului curent, ci al întregii " &
                "ordonanțări. Nu se poate șterge de aici." & vbCrLf &
                "Selectați «" & TOTI_BENEFICIARII & "» dacă vreți să-l ștergeți.",
                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        For Each d As OrdDraftDoc In grup
            _draft.Documente.Remove(d)
        Next
        ReumpleListele()
        RaiseEvent DraftModificat(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Aseaza cursorul pe randul care poarta documentele tocmai adaugate.</summary>
    Private Shared Sub SelecteazaInGrila(grila As KBotDataView, adaugate As List(Of OrdDraftDoc))
        If adaugate Is Nothing OrElse adaugate.Count = 0 Then Return
        For i As Integer = 0 To grila.Rows.Count - 1
            Dim grup As List(Of OrdDraftDoc) = TryCast(grila.Rows(i).Tag, List(Of OrdDraftDoc))
            If grup Is Nothing Then Continue For
            If grup.Any(Function(d) adaugate.Any(Function(n) ReferenceEquals(d, n))) Then
                grila.CurrentRowIndex = i
                Return
            End If
        Next
    End Sub

    ''' <summary>
    ''' Grilele se auto-temeaza; aici raman fundalurile, cele doua titluri SI cele patru
    ''' butoane. Butoanele trebuie facute cu mana: <c>ThemeManager.Traverse</c> NU coboara cu
    ''' regulile generice in copiii unui control care e el insusi <c>IThemedControl</c> — si
    ''' pagina asta este — deci ele ramaneau gri de sistem sub orice schema.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor
            splitDreapta.BackColor = p.SurfaceAltColor
            splitDreapta.Panel1.BackColor = p.SurfaceAltColor
            splitDreapta.Panel2.BackColor = p.SurfaceAltColor
            tlyText.BackColor = p.SurfaceAltColor
            tlyFisiere.BackColor = p.SurfaceAltColor


            ' «Adaugă» e actiunea, deci poarta accentul; «Șterge» ramane secundar — un buton
            ' distructiv nu se imbraca in culoarea care cheama degetul.
            ButtonStyles.ApplyPrimary(btnAdaugaText, scheme)
            ButtonStyles.ApplyPrimary(btnAdaugaFisier, scheme)
            ButtonStyles.ApplySecondary(btnStergeText, scheme)
            ButtonStyles.ApplySecondary(btnStergeFisier, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
