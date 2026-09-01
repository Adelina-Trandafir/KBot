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
''' «arata tot»: in Access el tinea randurile a caror legatura de beneficiar e NULL, adica
''' documentele care apartin INTREGII ordonantari, nu unuia. Intelesul se pastreaza intocmai:
''' pe randul sintetic se vad (si se adauga) DOAR documentele comune; pe un beneficiar anume
''' se vad cele comune SI ale lui.</para>
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

    ''' <summary>Randul sintetic — documentele INTREGII ordonantari (legatura de beneficiar
    ''' NULL), nu «toate documentele». Vezi nota de clasa.</summary>
    Private Const TOTI_BENEFICIARII As String = "< TOȚI BENEFICIARII >"

    Private _draft As OrdDraft
    ' Beneficiarul selectat (identitate reala sau temporara). 0 = randul sintetic.
    Private _cheieBene As Integer
    Private _suspenda As Boolean

    Public Event DraftModificat As EventHandler Implements IOrdEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
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
            ReumpleListele()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.GrdBene_SelectionChanged", ex)
        End Try
    End Sub

    ' ── Filtrul, exact ca in Access ──────────────────────────────────────────────────────

    ''' <summary>
    ''' Ce documente se vad acum. Pe randul sintetic: doar cele comune (fara beneficiar) —
    ''' Access: <c>TIDORDPART IS NULL</c>. Pe un beneficiar: cele comune SI ale lui — Access:
    ''' <c>TIDORDPART IS NULL OR TIDORDPART = X</c>.
    ''' </summary>
    Private Function DocumenteVizibile(cuFisier As Boolean) As List(Of OrdDraftDoc)
        If _draft Is Nothing Then Return New List(Of OrdDraftDoc)()
        Dim sursa As IEnumerable(Of OrdDraftDoc) = _draft.Documente.
            Where(Function(d) d.EsteText <> cuFisier)
        If _cheieBene = 0 Then
            sursa = sursa.Where(Function(d) CheiaPart(d) = 0)
        Else
            sursa = sursa.Where(Function(d) CheiaPart(d) = 0 OrElse CheiaPart(d) = _cheieBene)
        End If
        Return sursa.OrderBy(Function(d) CheiaPart(d)).ThenBy(Function(d) d.Cheie).ToList()
    End Function

    ''' <summary>Identitatea beneficiarului de care atarna documentul; 0 = document comun.</summary>
    Private Shared Function CheiaPart(d As OrdDraftDoc) As Integer
        If d.Idordpartp > 0 Then Return d.Idordpartp
        Return d.PartTempId
    End Function

    Private Sub ReumpleListele()
        _suspenda = True
        Try
            grdText.BeginUpdate()
            Try
                grdText.ClearRows()
                For Each d As OrdDraftDoc In DocumenteVizibile(cuFisier:=False)
                    Dim r As KBotDataRow = grdText.AddRow()
                    r.Tag = d
                    r(COL_DOC_JUST) = d.DocJust
                Next
            Finally
                grdText.EndUpdate()
            End Try
            grdText.CurrentRowIndex = If(grdText.Rows.Count > 0, 0, -1)

            grdFisiere.BeginUpdate()
            Try
                grdFisiere.ClearRows()
                For Each d As OrdDraftDoc In DocumenteVizibile(cuFisier:=True)
                    Dim r As KBotDataRow = grdFisiere.AddRow()
                    r.Tag = d
                    r(COL_NUME_DOC) = d.NumeDoc
                    r(COL_TIP_DOC) = d.TipDoc
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
            If e Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= grdText.Rows.Count Then Return
            Dim d As OrdDraftDoc = TryCast(grdText.Rows(e.RowIndex).Tag, OrdDraftDoc)
            If d Is Nothing Then Return
            If e.ColumnKey <> COL_DOC_JUST Then Return

            d.DocJust = If(e.NewValue Is Nothing, String.Empty, e.NewValue.ToString())
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.GrdText_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub BtnAdaugaText_Click(sender As Object, e As EventArgs) Handles btnAdaugaText.Click
        Try
            If _draft Is Nothing Then Return
            Dim d As New OrdDraftDoc() With {
                .TempId = _draft.UrmatorulTempId(),
                .DocJust = String.Empty,
                .TipDoc = "text"}
            LeagaDeBeneficiar(d)
            _draft.Documente.Add(d)
            ReumpleListele()
            SelecteazaInGrila(grdText, d)
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.BtnAdaugaText_Click", ex)
            MessageBox.Show(Me, "Rândul nu a putut fi adăugat. Detalii în jurnalul de erori.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnStergeText_Click(sender As Object, e As EventArgs) Handles btnStergeText.Click
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
            For Each cale As String In dlgFisiere.FileNames
                Dim octeti As Byte() = File.ReadAllBytes(cale)
                Dim nume As String = Path.GetFileName(cale)
                Dim ext As String = Path.GetExtension(cale)
                If ext.StartsWith(".", StringComparison.Ordinal) Then ext = ext.Substring(1)

                Dim d As New OrdDraftDoc() With {
                    .TempId = _draft.UrmatorulTempId(),
                    .DocJust = Convert.ToBase64String(octeti),
                    .NumeDoc = nume,
                    .TipDoc = ext}
                LeagaDeBeneficiar(d)
                _draft.Documente.Add(d)
                adaugate += 1
            Next

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

    ''' <summary>Leaga un rand nou de beneficiarul selectat; pe randul sintetic ramane comun
    ''' (Access: <c>Form_BeforeInsert</c> scria legatura doar cand selectia nu era «*»).</summary>
    Private Sub LeagaDeBeneficiar(d As OrdDraftDoc)
        If _cheieBene = 0 Then Return
        If _cheieBene > 0 Then
            d.Idordpartp = _cheieBene
        Else
            d.PartTempId = _cheieBene
        End If
    End Sub

    ''' <summary>
    ''' Sterge randul curent al grilei date. Un document COMUN nu se poate sterge cat timp e
    ''' selectat un beneficiar anume — mesajul e cel din Access
    ''' (<c>dtnDel_Click</c>: «Documentul selectat nu este al Beneficiarului curent!»), fiindca
    ''' altfel operatorul ar sterge de sub ceilalti beneficiari fara sa-si dea seama.
    ''' </summary>
    Private Sub StergeRandul(grila As KBotDataView)
        If _draft Is Nothing Then Return
        Dim r As KBotDataRow = grila.CurrentRow
        Dim d As OrdDraftDoc = TryCast(If(r Is Nothing, Nothing, r.Tag), OrdDraftDoc)
        If d Is Nothing Then Return

        If CheiaPart(d) = 0 AndAlso _cheieBene <> 0 Then
            MessageBox.Show(Me,
                "Documentul selectat nu este al beneficiarului curent, ci al întregii " &
                "ordonanțări. Nu se poate șterge de aici." & vbCrLf &
                "Selectați «" & TOTI_BENEFICIARII & "» dacă vreți să-l ștergeți.",
                "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        _draft.Documente.Remove(d)
        ReumpleListele()
        RaiseEvent DraftModificat(Me, EventArgs.Empty)
    End Sub

    Private Shared Sub SelecteazaInGrila(grila As KBotDataView, d As OrdDraftDoc)
        For i As Integer = 0 To grila.Rows.Count - 1
            If ReferenceEquals(grila.Rows(i).Tag, d) Then
                grila.CurrentRowIndex = i
                Return
            End If
        Next
    End Sub

    ''' <summary>Grilele se auto-temeaza; aici raman fundalurile si cele doua titluri.</summary>
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
            tlyButoaneText.BackColor = p.SurfaceAltColor
            tlyButoaneFisiere.BackColor = p.SurfaceAltColor

            For Each capt As Label In New Label() {lblText, lblFisiere}
                capt.ForeColor = p.TextColor
                capt.BackColor = Color.Transparent
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("OrdDocumentePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
