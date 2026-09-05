Option Strict On
Imports System.Collections.Generic
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Pagina «Atasamente» a editorului de ordonantare (felia 0049) — portul lui
''' <c>frmFX_ORD_PRTSCR</c> + <c>_BENE</c> + <c>_S</c>.
'''
''' <para><b>Octetii stau in memorie pana la salvare.</b> Un <c>IDORDATTP</c> trebuie sa existe
''' inainte ca octetii sa poata atarna de el, deci formularul salveaza intai graful si abia
''' apoi urca imaginile. Pagina doar tine octetii pe <see cref="OrdDraftAtt.Continut"/> si
''' ridica steagul <see cref="OrdDraftAtt.Modificat"/> — fara el, o imagine adusa de pe server
''' ca sa poata fi PRIVITA ar fi retrimisa identica la fiecare salvare.</para>
'''
''' <para><b>Randul sintetic</b> inseamna acelasi lucru ca pe pagina «Documente justificative»,
''' si din acelasi motiv (cerinta operatorului, 02.09.2026): pe «&lt; TOTI BENEFICIARII &gt;» se
''' vad DOAR imaginile pe care le au TOTI beneficiarii — fie fara legatura de beneficiar, fie
''' cate una identica la fiecare, stranse intr-un singur rand pe ecran — iar ce se adauga acolo
''' il primesc TOTI, cate o copie fiecare. Pe un beneficiar anume se vad cele fara legatura SI
''' ale lui.</para>
'''
''' <para><b>Cu un SINGUR beneficiar randul sintetic lipseste cu totul</b> (cerinta
''' operatorului, 03.09.2026) — aceeasi regula, si din acelasi motiv, ca pe pagina
''' «Documente justificative»: acolo «toti» si «el» sunt acelasi lucru.</para>
'''
''' <para><b>Si tot ca acolo, randul sintetic nu se editeaza in grila</b> (cerinta operatorului,
''' 02.09.2026): un rand de acolo sta pe N copii, deci o redenumire ar trebui sa se scrie in
''' toate deodata. Grila trece pe doar-citire cat timp e ales «&lt; TOTI BENEFICIARII &gt;»;
''' adaugarea (fisier sau lipire) ramane deschisa, fiindca ea nu are nevoie de o celula. O
''' redenumire se face pe beneficiarul ei, unde randul e al lui singur.</para>
'''
''' <para>Consecinta de trafic, asumata: N copii ale aceleiasi imagini inseamna N urcari dupa
''' salvare. Alternativa — o singura imagine cu legatura NULL — n-ar fi VAZUTA de calea de
''' citire, care aduce randurile prin beneficiar.</para>
'''
''' <para><b>Ce nu s-a portat.</b> <c>hwndAccess</c> / <c>hwndForm</c> si <c>WebBrowser0</c>
''' erau instalatie de gazduire a ferestrelor Access (un WebBrowser reparentat prin
''' <c>SetParent</c>, ca sa se poata face zoom si panoramare pe o imagine base64). In WinForms
''' previzualizarea e un <c>PictureBox</c> cu <c>SizeMode = Zoom</c> — nu exista nimic de
''' reparentat, deci cele trei nu au succesor.</para>
'''
''' <para><c>FX_ORD_ATT.Imagine</c> (base64) ramane pe loc, dar nu se scrie si nu se citeste:
''' octetii traiesc in <c>FX_ORD_ATT_IMG</c> (D9).</para>
''' </summary>
Public Class OrdAtasamentePage
    Implements IOrdEditPage, IThemedControl

    Private Const COL_ETICHETA As String = "eticheta"
    Private Const COL_NUME_FISIER As String = "nume_fisier"
    Private Const COL_STARE As String = "stare"

    Private Const TOTI_BENEFICIARII As String = "< TOȚI BENEFICIARII >"

    ''' <summary>Ce se lipeste la titlul listei cat timp e ales randul sintetic — altfel
    ''' doar-citirea ar parea o grila stricata.</summary>
    Private Const SUFIX_COMUN As String = "  ·  rânduri comune (doar citire)"

    Private _draft As OrdDraft
    Private _cheieBene As Integer
    Private _suspenda As Boolean
    ' Titlul scris in designer, pastrat ca sa i se poata pune si scoate sufixul.
    Private ReadOnly _titluLista As String
    ' Imaginea afisata acum. Se tine ca sa poata fi eliberata: `PictureBox.Image` nu-si elibereaza
    ' singur bitmapul precedent, iar o pagina de capturi de ecran ar aduna memorie in tacere.
    Private _imagineCurenta As Image

    Public Event DraftModificat As EventHandler Implements IOrdEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IOrdEditPage.PageKey
        Get
            Return "atasamente"
        End Get
    End Property

    Public Sub SetDraft(draft As OrdDraft) Implements IOrdEditPage.SetDraft
        Try
            _draft = draft
            _cheieBene = 0
            PopuleazaBeneficiari()
            ReumpleLista()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.SetDraft", ex)
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
            ReumpleLista()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.GrdBene_SelectionChanged", ex)
        End Try
    End Sub

    ' ── Lista de atasamente ──────────────────────────────────────────────────────────────

    Private Shared Function CheiaPart(a As OrdDraftAtt) As Integer
        If a.Idordpartp > 0 Then Return a.Idordpartp
        Return a.PartTempId
    End Function

    ''' <summary>Ce face doua imagini de pe doi beneficiari «aceeasi imagine»: numele plus
    ''' amprenta continutului. Suma de pe server, cand exista, e cea care decide; pentru
    ''' copiile abia adaugate, care n-au apucat sa aiba una, ramane dimensiunea — ele sunt
    ''' oricum nascute impreuna, din aceiasi octeti.</summary>
    Private Shared Function CheieValoare(a As OrdDraftAtt) As String
        Return a.NumeFisier & vbNullChar & a.Sha256 & vbNullChar &
               a.Dimensiune.ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Identitatile beneficiarilor, in ordine stabila.</summary>
    Private Function CheileBeneficiarilor() As List(Of Integer)
        If _draft Is Nothing Then Return New List(Of Integer)()
        Return _draft.Parteneri.Select(Function(p) p.Cheie).Distinct().OrderBy(Function(k) k).ToList()
    End Function

    ''' <summary>
    ''' Ce se vede acum, ca GRUPURI: un grup = un rand pe ecran. Pe un beneficiar fiecare grup
    ''' are un singur element; pe randul sintetic, cate unul per beneficiar. Aceeasi regula ca
    ''' pe pagina «Documente justificative» — vezi nota ei de clasa.
    ''' </summary>
    Private Function GrupuriVizibile() As List(Of List(Of OrdDraftAtt))
        Dim rezultat As New List(Of List(Of OrdDraftAtt))()
        If _draft Is Nothing Then Return rezultat

        If _cheieBene <> 0 Then
            For Each a As OrdDraftAtt In _draft.Atasamente.
                    Where(Function(x) CheiaPart(x) = 0 OrElse CheiaPart(x) = _cheieBene).
                    OrderBy(Function(x) CheiaPart(x)).ThenBy(Function(x) x.Cheie)
                rezultat.Add(New List(Of OrdDraftAtt) From {a})
            Next
            Return rezultat
        End If

        For Each a As OrdDraftAtt In _draft.Atasamente.Where(Function(x) CheiaPart(x) = 0).
                OrderBy(Function(x) x.Cheie)
            rezultat.Add(New List(Of OrdDraftAtt) From {a})
        Next

        Dim cheiBene As List(Of Integer) = CheileBeneficiarilor()
        If cheiBene.Count = 0 Then Return rezultat

        For Each grup In _draft.Atasamente.Where(Function(x) CheiaPart(x) <> 0).
                GroupBy(Function(x) CheieValoare(x), StringComparer.Ordinal).
                OrderBy(Function(g) g.Min(Function(x) x.Cheie))
            Dim peBene As New List(Of List(Of OrdDraftAtt))()
            For Each cheie As Integer In cheiBene
                Dim ale As List(Of OrdDraftAtt) = grup.
                    Where(Function(x) CheiaPart(x) = cheie).
                    OrderBy(Function(x) x.Cheie).ToList()
                If ale.Count = 0 Then
                    peBene.Clear()
                    Exit For
                End If
                peBene.Add(ale)
            Next
            If peBene.Count = 0 Then Continue For

            Dim straturi As Integer = peBene.Min(Function(l) l.Count)
            For i As Integer = 0 To straturi - 1
                Dim strat As Integer = i
                rezultat.Add(peBene.Select(Function(l) l(strat)).ToList())
            Next
        Next
        Return rezultat
    End Function

    Private Sub ReumpleLista()
        _suspenda = True
        Try
            ' Pe randul sintetic un rand de ecran sta pe N copii: nu se redenumeste in grila.
            Dim comun As Boolean = _cheieBene = 0
            grdAtasamente.ReadOnlyGrid = comun

            grdAtasamente.BeginUpdate()
            Try
                grdAtasamente.ClearRows()
                For Each grup As List(Of OrdDraftAtt) In GrupuriVizibile()
                    Dim r As KBotDataRow = grdAtasamente.AddRow()
                    r.Tag = grup
                    r(COL_NUME_FISIER) = grup(0).NumeFisier
                    r(COL_STARE) = Stare(grup(0))
                Next
            Finally
                grdAtasamente.EndUpdate()
            End Try
            grdAtasamente.CurrentRowIndex = If(grdAtasamente.Rows.Count > 0, 0, -1)
        Finally
            _suspenda = False
        End Try
        AratePreviewul()
    End Sub

    ''' <summary>Ce spune coloana «Stare»: daca imaginea e deja pe server sau abia urmeaza.</summary>
    Private Shared Function Stare(a As OrdDraftAtt) As String
        If a.Modificat Then Return "de urcat"
        If Not String.IsNullOrWhiteSpace(a.Sha256) Then Return "pe server"
        Return "fără imagine"
    End Function

    ''' <summary>Grupul din spatele randului dat; <c>Nothing</c> daca randul nu poarta unul.</summary>
    Private Function GrupulCurent() As List(Of OrdDraftAtt)
        Dim r As KBotDataRow = grdAtasamente.CurrentRow
        If r Is Nothing Then Return Nothing
        Dim grup As List(Of OrdDraftAtt) = TryCast(r.Tag, List(Of OrdDraftAtt))
        If grup Is Nothing OrElse grup.Count = 0 Then Return Nothing
        Return grup
    End Function

    ''' <summary>Prima imagine a randului selectat — cea care se previzualizeaza. Pe un rand
    ''' comun toate copiile poarta aceiasi octeti, deci oricare ar spune acelasi lucru.</summary>
    Private Function AtasamentulCurent() As OrdDraftAtt
        Dim grup As List(Of OrdDraftAtt) = GrupulCurent()
        If grup Is Nothing Then Return Nothing
        Return grup(0)
    End Function

    Private Sub GrdAtasamente_SelectionChanged(sender As Object, e As EventArgs) _
        Handles grdAtasamente.SelectionChanged
        Try
            If _suspenda Then Return
            AratePreviewul()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.GrdAtasamente_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GrdAtasamente_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles grdAtasamente.CellValueChanged
        Try
            If _suspenda Then Return
            If e Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= grdAtasamente.Rows.Count Then Return
            Dim grup As List(Of OrdDraftAtt) = TryCast(grdAtasamente.Rows(e.RowIndex).Tag, List(Of OrdDraftAtt))
            If grup Is Nothing OrElse grup.Count = 0 OrElse e.ColumnKey <> COL_NUME_FISIER Then Return

            Dim nume As String = If(e.NewValue Is Nothing, String.Empty, e.NewValue.ToString()).Trim()
            If nume = "" Then
                ' Numele fisierului e obligatoriu la incarcare (serverul cere antetul), deci un
                ' nume golit se refuza pe loc, nu la salvare.
                _suspenda = True
                Try
                    grdAtasamente.Rows(e.RowIndex)(COL_NUME_FISIER) = grup(0).NumeFisier
                Finally
                    _suspenda = False
                End Try
                grdAtasamente.InvalidateRow(e.RowIndex)
                Return
            End If

            ' Un rand comun sta pe N copii: numele se scrie in toate, altfel ar inceta sa mai
            ' fie comun in tacere, chiar in clipa in care e redenumit.
            For Each a As OrdDraftAtt In grup
                a.NumeFisier = nume
            Next
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.GrdAtasamente_CellValueChanged", ex)
        End Try
    End Sub

    ' ── Previzualizarea ──────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Arata imaginea atasamentului selectat, daca octetii ei sunt in memorie. Cand nu sunt
    ''' (imagine de pe server pe care formularul n-a apucat s-o aduca), se spune asta —
    ''' pagina NU face cereri de retea, ea doar reda ce i s-a dat.
    ''' </summary>
    Private Sub AratePreviewul()
        Dim a As OrdDraftAtt = AtasamentulCurent()

        Dim veche As Image = _imagineCurenta
        _imagineCurenta = Nothing
        picPreview.Image = Nothing
        If veche IsNot Nothing Then veche.Dispose()

        If a Is Nothing OrElse a.Continut Is Nothing OrElse a.Continut.Length = 0 Then
            lblPreviewGol.Text = If(a Is Nothing,
                                    "Selectați o imagine din listă.",
                                    "Imaginea nu este disponibilă local.")
            lblPreviewGol.Visible = True
            picPreview.Visible = False
            Return
        End If

        Try
            ' Fluxul ramane DESCHIS cat traieste bitmapul: `Image.FromStream` citeste lenes, iar
            ' un flux inchis da «parametru nevalid» la prima repictare. Se inchide odata cu
            ' imaginea, la urmatoarea selectie.
            Dim flux As New MemoryStream(a.Continut, writable:=False)
            _imagineCurenta = Image.FromStream(flux)
            picPreview.Image = _imagineCurenta
            picPreview.Visible = True
            lblPreviewGol.Visible = False
        Catch ex As Exception
            ' Octeti care nu sunt o imagine valida: se spune, nu se cade.
            GlobalErrorLog.Write("OrdAtasamentePage.AratePreviewul", ex)
            lblPreviewGol.Text = "Fișierul selectat nu este o imagine validă."
            lblPreviewGol.Visible = True
            picPreview.Visible = False
        End Try
    End Sub

    ' ── Butoanele ────────────────────────────────────────────────────────────────────────

    Private Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _draft Is Nothing Then Return
            If dlgImagine.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim ultimul As List(Of OrdDraftAtt) = Nothing
            For Each cale As String In dlgImagine.FileNames
                ultimul = AdaugaAtasament(Path.GetFileName(cale), File.ReadAllBytes(cale))
            Next
            If ultimul Is Nothing OrElse ultimul.Count = 0 Then Return

            ReumpleLista()
            SelecteazaInLista(ultimul)
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.BtnAdauga_Click", ex)
            MessageBox.Show(Me, "Imaginea nu a putut fi citită: " & ex.Message,
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Lipeste captura din memoria temporara a Windows-ului, codificata PNG. Access nu avea
    ''' pasul asta — acolo captura se facea in afara machetei si se alegea ca fisier — dar
    ''' formularul se numeste «PRTSCR» tocmai fiindca asta e treaba lui, iar un drum prin disc
    ''' pentru o captura care e deja in clipboard e munca in plus pentru operator.
    ''' </summary>
    Private Sub BtnLipeste_Click(sender As Object, e As EventArgs) Handles btnLipeste.Click
        Try
            If _draft Is Nothing Then Return
            If Not Clipboard.ContainsImage Then
                MessageBox.Show(Me, "În memoria temporară nu se află nicio imagine.",
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim octeti As Byte()
            Using img = Clipboard.GetImage
                If img Is Nothing Then Return
                Using flux As New MemoryStream
                    img.Save(flux, ImageFormat.Png)
                    octeti = flux.ToArray
                End Using
            End Using

            Dim nume = $"Captura_{Date.Now:yyyyMMdd_HHmmss}.png"
            Dim adaugate = AdaugaAtasament(nume, octeti)
            ReumpleLista()
            SelecteazaInLista(adaugate)
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.BtnLipeste_Click", ex)
            MessageBox.Show(Me, "Imaginea din memoria temporară nu a putut fi preluată: " & ex.Message,
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            If _draft Is Nothing Then Return
            Dim grup = GrupulCurent()
            If grup Is Nothing Then Return

            ' Aceeasi regula ca la documente: un atasament al INTREGII ordonantari nu se sterge
            ' de sub ceilalti beneficiari fara ca operatorul sa-si dea seama (Access:
            ' `dtnDel_Click`).
            ' Fara rand sintetic (un singur beneficiar) refuzul n-ar avea unde sa trimita
            ' operatorul, si nici n-ar avea de ce: acolo «a intregii ordonantari» si «a lui»
            ' inseamna acelasi lucru.
            If AreRandSintetic() AndAlso _cheieBene <> 0 AndAlso grup.Any(Function(a) CheiaPart(a) = 0) Then
                MessageBox.Show(Me,
                    "Imaginea selectată nu este a beneficiarului curent, ci a întregii " &
                    "ordonanțări. Nu se poate șterge de aici." & vbCrLf &
                    "Selectați «" & TOTI_BENEFICIARII & "» dacă vreți să o ștergeți.",
                    "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Un rand comun le sterge pe toate N copiile pe care le reprezinta.
            For Each a In grup
                _draft.Atasamente.Remove(a)
            Next
            ReumpleLista()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.BtnSterge_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Adauga imaginea si o leaga de cine trebuie: pe un beneficiar selectat, o singura copie,
    ''' a lui; pe randul sintetic, cate una pentru FIECARE beneficiar — asa o primesc TOTI.
    ''' Octetii se impart intre copii (acelasi tablou, nimeni nu-l modifica). Fara niciun
    ''' beneficiar ramane o singura copie, a intregii ordonantari.
    ''' </summary>
    Private Function AdaugaAtasament(nume As String, octeti As Byte()) As List(Of OrdDraftAtt)
        Dim adaugate As New List(Of OrdDraftAtt)()
        If _draft Is Nothing Then Return adaugate

        Dim tinte As List(Of Integer)
        If _cheieBene <> 0 Then
            tinte = New List(Of Integer) From {_cheieBene}
        Else
            tinte = CheileBeneficiarilor()
            If tinte.Count = 0 Then tinte.Add(0)
        End If

        For Each cheie As Integer In tinte
            ' `UrmatorulTempId` citeste listele, deci se cere DUPA fiecare adaugare.
            Dim a As New OrdDraftAtt() With {
                .TempId = _draft.UrmatorulTempId(),
                .NumeFisier = nume,
                .Continut = octeti,
                .Dimensiune = octeti.Length,
                .Modificat = True}
            If cheie > 0 Then
                a.Idordpartp = cheie
            ElseIf cheie < 0 Then
                a.PartTempId = cheie
            End If
            _draft.Atasamente.Add(a)
            adaugate.Add(a)
        Next
        Return adaugate
    End Function

    ''' <summary>Aseaza cursorul pe randul care poarta imaginile tocmai adaugate.</summary>
    Private Sub SelecteazaInLista(adaugate As List(Of OrdDraftAtt))
        If adaugate Is Nothing OrElse adaugate.Count = 0 Then Return
        For i As Integer = 0 To grdAtasamente.Rows.Count - 1
            Dim grup As List(Of OrdDraftAtt) = TryCast(grdAtasamente.Rows(i).Tag, List(Of OrdDraftAtt))
            If grup Is Nothing Then Continue For
            If grup.Any(Function(a) adaugate.Any(Function(n) ReferenceEquals(a, n))) Then
                grdAtasamente.CurrentRowIndex = i
                Return
            End If
        Next
    End Sub

    ''' <summary>
    ''' Grilele se auto-temeaza; aici raman fundalurile, titlul, starea goala SI cele trei
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
            tlyLista.BackColor = p.SurfaceAltColor
            pnlPreview.BackColor = p.SurfaceColor
            picPreview.BackColor = p.SurfaceColor

            lblPreviewGol.ForeColor = p.TextDimColor
            lblPreviewGol.BackColor = Color.Transparent

            ' «Adaugă» e actiunea, deci poarta accentul; celelalte doua raman secundare — un
            ' buton distructiv nu se imbraca in culoarea care cheama degetul.
            ButtonStyles.ApplyPrimary(btnAdauga, scheme)
            ButtonStyles.ApplySecondary(btnLipeste, scheme)
            ButtonStyles.ApplySecondary(btnSterge, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.ApplyTheme", ex)
        End Try
    End Sub
End Class
