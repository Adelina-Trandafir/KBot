Option Strict On
Imports System.Collections.Generic
Imports System.Drawing.Imaging
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
''' <para><b>Randul sintetic</b> are exact intelesul din Access: pe «&lt; TOTI BENEFICIARII &gt;»
''' se vad atasamentele intregii ordonantari (fara legatura de beneficiar), iar pe un
''' beneficiar anume se vad acelea SI ale lui.</para>
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

    Private _draft As OrdDraft
    Private _cheieBene As Integer
    Private _suspenda As Boolean
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
                Dim sintetic As KBotDataRow = grdBene.AddRow()
                sintetic.Tag = 0
                sintetic(COL_ETICHETA) = TOTI_BENEFICIARII
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

    Private Function AtasamenteVizibile() As List(Of OrdDraftAtt)
        If _draft Is Nothing Then Return New List(Of OrdDraftAtt)()
        Dim sursa As IEnumerable(Of OrdDraftAtt) = _draft.Atasamente
        If _cheieBene = 0 Then
            sursa = sursa.Where(Function(a) CheiaPart(a) = 0)
        Else
            sursa = sursa.Where(Function(a) CheiaPart(a) = 0 OrElse CheiaPart(a) = _cheieBene)
        End If
        Return sursa.OrderBy(Function(a) CheiaPart(a)).ThenBy(Function(a) a.Cheie).ToList()
    End Function

    Private Sub ReumpleLista()
        _suspenda = True
        Try
            grdAtasamente.BeginUpdate()
            Try
                grdAtasamente.ClearRows()
                For Each a As OrdDraftAtt In AtasamenteVizibile()
                    Dim r As KBotDataRow = grdAtasamente.AddRow()
                    r.Tag = a
                    r(COL_NUME_FISIER) = a.NumeFisier
                    r(COL_STARE) = Stare(a)
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

    Private Function AtasamentulCurent() As OrdDraftAtt
        Dim r As KBotDataRow = grdAtasamente.CurrentRow
        If r Is Nothing Then Return Nothing
        Return TryCast(r.Tag, OrdDraftAtt)
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
            Dim a As OrdDraftAtt = TryCast(grdAtasamente.Rows(e.RowIndex).Tag, OrdDraftAtt)
            If a Is Nothing OrElse e.ColumnKey <> COL_NUME_FISIER Then Return

            Dim nume As String = If(e.NewValue Is Nothing, String.Empty, e.NewValue.ToString()).Trim()
            If nume = "" Then
                ' Numele fisierului e obligatoriu la incarcare (serverul cere antetul), deci un
                ' nume golit se refuza pe loc, nu la salvare.
                _suspenda = True
                Try
                    grdAtasamente.Rows(e.RowIndex)(COL_NUME_FISIER) = a.NumeFisier
                Finally
                    _suspenda = False
                End Try
                grdAtasamente.InvalidateRow(e.RowIndex)
                Return
            End If

            a.NumeFisier = nume
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

            Dim ultimul As OrdDraftAtt = Nothing
            For Each cale As String In dlgImagine.FileNames
                ultimul = AdaugaAtasament(Path.GetFileName(cale), File.ReadAllBytes(cale))
            Next
            If ultimul Is Nothing Then Return

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
            If Not Clipboard.ContainsImage() Then
                MessageBox.Show(Me, "În memoria temporară nu se află nicio imagine.",
                                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim octeti As Byte()
            Using img As Image = Clipboard.GetImage()
                If img Is Nothing Then Return
                Using flux As New MemoryStream()
                    img.Save(flux, ImageFormat.Png)
                    octeti = flux.ToArray()
                End Using
            End Using

            Dim nume As String = $"Captura_{Date.Now:yyyyMMdd_HHmmss}.png"
            Dim a As OrdDraftAtt = AdaugaAtasament(nume, octeti)
            ReumpleLista()
            SelecteazaInLista(a)
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
            Dim a As OrdDraftAtt = AtasamentulCurent()
            If a Is Nothing Then Return

            ' Aceeasi regula ca la documente: un atasament COMUN nu se sterge de sub ceilalti
            ' beneficiari fara ca operatorul sa-si dea seama (Access: `dtnDel_Click`).
            If CheiaPart(a) = 0 AndAlso _cheieBene <> 0 Then
                MessageBox.Show(Me,
                    "Imaginea selectată nu este a beneficiarului curent, ci a întregii " &
                    "ordonanțări. Nu se poate șterge de aici." & vbCrLf &
                    "Selectați «" & TOTI_BENEFICIARII & "» dacă vreți să o ștergeți.",
                    "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            _draft.Atasamente.Remove(a)
            ReumpleLista()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.BtnSterge_Click", ex)
        End Try
    End Sub

    ''' <summary>Adauga un rand de atasament nou, legat de beneficiarul selectat (sau comun,
    ''' pe randul sintetic), cu octetii in memorie si steagul de «schimbat» ridicat.</summary>
    Private Function AdaugaAtasament(nume As String, octeti As Byte()) As OrdDraftAtt
        Dim a As New OrdDraftAtt() With {
            .TempId = _draft.UrmatorulTempId(),
            .NumeFisier = nume,
            .Continut = octeti,
            .Dimensiune = octeti.Length,
            .Modificat = True}
        If _cheieBene > 0 Then
            a.Idordpartp = _cheieBene
        ElseIf _cheieBene < 0 Then
            a.PartTempId = _cheieBene
        End If
        _draft.Atasamente.Add(a)
        Return a
    End Function

    Private Sub SelecteazaInLista(a As OrdDraftAtt)
        If a Is Nothing Then Return
        For i As Integer = 0 To grdAtasamente.Rows.Count - 1
            If ReferenceEquals(grdAtasamente.Rows(i).Tag, a) Then
                grdAtasamente.CurrentRowIndex = i
                Return
            End If
        Next
    End Sub

    ''' <summary>Grilele se auto-temeaza; aici raman fundalurile, titlul si starea goala.</summary>
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
            tlyButoane.BackColor = p.SurfaceAltColor
            pnlPreview.BackColor = p.SurfaceColor
            picPreview.BackColor = p.SurfaceColor

            lblLista.ForeColor = p.TextColor
            lblLista.BackColor = Color.Transparent
            lblPreviewGol.ForeColor = p.TextDimColor
            lblPreviewGol.BackColor = Color.Transparent
        Catch ex As Exception
            GlobalErrorLog.Write("OrdAtasamentePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
