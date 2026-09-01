Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' EDITORUL DE ORDONANTARE (felia 0049) — portul lui <c>frmFX_ORD</c> si al celor noua
''' subformulare ale lui. Modal peste <c>MainForm</c>.
'''
''' <para><b>O singura salvare, o singura tranzactie</b> (D5). Lantul VBA in cinci pasi —
''' curatare staging ▸ trimitere pe server ▸ salvare locala „de proba" ▸ confirmare ▸ commit
''' local, cu mesajele lui de «EROARE CRITICA: datele sunt pe server dar nu local» — nu are
''' succesor. Tot graful urca intr-un POST, serverul il scrie intr-o tranzactie si intoarce
''' cheile reale. Nu mai exista stare care sa poata ramane pe jumatate.</para>
'''
''' <para><b>Exceptia: imaginile atasate.</b> Un <c>IDORDATTP</c> trebuie sa existe inainte ca
''' octetii sa poata atarna de el, deci ele urca DUPA salvare, intr-o a doua faza. Daca o
''' incarcare cade acolo, ordonantarea RAMANE salvata si se spune pe sleau ce imagine
''' lipseste: un document pe jumatate derulat inapoi e mai rau decat unul caruia ii lipseste o
''' poza.</para>
'''
''' <para><b>Validarea e in doua locuri, deliberat.</b> Aici, ca operatorul sa primeasca un
''' mesaj rapid care numeste TOATE problemele deodata (portul blocului din
''' <c>btnSav_Click</c>); si pe server, inainte de primul INSERT, fiindca acolo e singurul loc
''' care nu poate fi ocolit. Cele trei verificari in plus fata de Access — clasificatie,
''' <c>CodAI</c>, unitate — vin din cheile straine ale lui <c>FX_ORD_TBL</c>, pe care Access nu
''' le avea.</para>
'''
''' <para><b>Reteaua trece prin plasa de re-autentificare a shell-ului.</b> <c>WithReauth</c> e
''' privat si generic in <c>MainForm</c>, deci formularul primeste cate o specializare pentru
''' fiecare forma de raspuns — acelasi tipar ca <c>AsociereForm</c>.</para>
''' </summary>
Public Class OrdEditForm

    Private Const PAGINA_BENEFICIARI As String = "beneficiari"
    Private Const PAGINA_DOCUMENTE As String = "documente"
    Private Const PAGINA_ATASAMENTE As String = "atasamente"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _draft As OrdDraft
    ' Plasa 401 a shell-ului, specializata pe fiecare forma de raspuns de care are nevoie
    ' formularul: politica de re-login ramane intr-un singur loc, aici doar se foloseste.
    Private ReadOnly _withReauthSalvare As Func(Of Func(Of Task(Of OrdSaveRezultat)), Task(Of OrdSaveRezultat))
    Private ReadOnly _withReauthIncarcare As Func(Of Func(Of Task(Of PutAtasamentResponse)), Task(Of PutAtasamentResponse))
    Private ReadOnly _withReauthImagine As Func(Of Func(Of Task(Of PdfDownloadResult)), Task(Of PdfDownloadResult))

    Private ReadOnly _pages As New Dictionary(Of String, IOrdEditPage)(StringComparer.Ordinal)
    Private _activePage As IOrdEditPage

    ''' <summary>Cheia ordonantarii salvate; 0 cat timp nu s-a salvat nimic.</summary>
    Public ReadOnly Property IdordpSalvat As Integer

    ''' <summary>S-a salvat ceva? Gazda reincarca vederea ORD abia atunci.</summary>
    Public ReadOnly Property SAuSalvatModificari As Boolean

    Public Sub New(apiClient As IApiClient,
                   draft As OrdDraft,
                   withReauthSalvare As Func(Of Func(Of Task(Of OrdSaveRezultat)), Task(Of OrdSaveRezultat)),
                   withReauthIncarcare As Func(Of Func(Of Task(Of PutAtasamentResponse)), Task(Of PutAtasamentResponse)),
                   withReauthImagine As Func(Of Func(Of Task(Of PdfDownloadResult)), Task(Of PdfDownloadResult)))
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If draft Is Nothing Then Throw New ArgumentNullException(NameOf(draft))
        If withReauthSalvare Is Nothing Then Throw New ArgumentNullException(NameOf(withReauthSalvare))
        If withReauthIncarcare Is Nothing Then Throw New ArgumentNullException(NameOf(withReauthIncarcare))
        If withReauthImagine Is Nothing Then Throw New ArgumentNullException(NameOf(withReauthImagine))

        InitializeComponent()
        _apiClient = apiClient
        _draft = draft
        _withReauthSalvare = withReauthSalvare
        _withReauthIncarcare = withReauthIncarcare
        _withReauthImagine = withReauthImagine
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Deschiderea
    ' ══════════════════════════════════════════════════════════════════════════

    ' Boundary UI (Load): se logheaza si se inghite — un throw ar darama deschiderea.
    Private Sub OrdEditForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            capBar.Text = $"K-BOT — Ordonanțare de plată · {_draft.CodAngajament}"
            Text = capBar.Text

            lblCod.Text = _draft.CodAngajament
            lblObiect.Text = _draft.ObiectDdf
            dtpData.Value = If(_draft.DataOrd.HasValue, _draft.DataOrd.Value, Date.Today)
            ActualizeazaAntet()

            ' Avertismentele generarii (clasificatie lipsa, tabela BIC absenta, ziua are peste
            ' 25 de parteneri) se arata de la bun inceput: sunt lucruri de stiut INAINTE de a
            ' edita, nu dupa ce salvarea a fost refuzata.
            If _draft.Avertismente.Count > 0 Then
                ntfMesaj.Show(String.Join(vbCrLf, _draft.Avertismente), NoticeKind.Warning)
                ntfMesaj.Visible = True
            End If

            ' Atribuirea ridica SelectionChanged, iar prin el se creeaza si se arata prima
            ' pagina. NU se activeaza a doua oara de mana — ar rula DUPA eveniment si ar
            ' ascunde exact pagina tocmai aratata (aceeasi nota ca in OrdView.BuildNav).
            navSub.SelectedKey = PAGINA_BENEFICIARI
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.OrdEditForm_Load", ex)
        End Try
    End Sub

    ' Boundary UI async: se logheaza si se inghite.
    Private Async Sub OrdEditForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Try
            Await AduImaginileAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.OrdEditForm_Shown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Aduce octetii imaginilor deja stocate pe server, ca pagina «Atasamente» sa le poata
    ''' ARATA. Paginile nu fac cereri de retea (contractul lui <see cref="IOrdEditPage"/>),
    ''' deci descarcarea traieste aici.
    '''
    ''' <para>O imagine care nu se poate aduce NU opreste editarea: se noteaza, se spune la
    ''' final, si restul documentului ramane editabil. Octetii adusi NU ridica steagul
    ''' <c>Modificat</c>, deci nu se retrimit la salvare.</para>
    ''' </summary>
    Private Async Function AduImaginileAsync() As Task
        Dim deAdus As List(Of OrdDraftAtt) =
            _draft.Atasamente.Where(Function(a) a.Idordattp > 0 AndAlso
                                                Not String.IsNullOrWhiteSpace(a.Sha256) AndAlso
                                                a.Continut Is Nothing).ToList()
        If deAdus.Count = 0 Then Return

        Dim esuate As New List(Of String)()
        busyBar.Running = True
        Try
            For Each a As OrdDraftAtt In deAdus
                Dim idordattp As Integer = a.Idordattp
                Try
                    Dim rezultat As PdfDownloadResult = Await _withReauthImagine(
                        Function() _apiClient.GetOrdAtasamentAsync(idordattp, String.Empty,
                                                                   CancellationToken.None)).ConfigureAwait(True)
                    If rezultat IsNot Nothing AndAlso rezultat.Status = PdfDownloadStatus.Content Then
                        a.Continut = rezultat.Bytes
                    End If
                Catch ex As Exception
                    GlobalErrorLog.Write("OrdEditForm.AduImaginileAsync", ex)
                    esuate.Add(a.NumeFisier)
                End Try
            Next
        Finally
            busyBar.Running = False
        End Try

        ' Paginile deja create vad octetii abia dupa o re-impingere a grafului.
        _activePage?.SetDraft(_draft)

        If esuate.Count > 0 Then
            ntfMesaj.Show("Aceste imagini nu au putut fi aduse de pe server și nu se pot " &
                          "previzualiza: " & String.Join(", ", esuate) & ". " &
                          "Ordonanțarea se poate edita și salva în continuare.",
                          NoticeKind.Warning)
            ntfMesaj.Visible = True
        End If
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Gazduirea paginilor
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub NavSub_SelectionChanged(key As String) Handles navSub.SelectionChanged
        Try
            ActivatePage(key)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.NavSub_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Arata pagina ceruta, creand-o la prima activare — acelasi tipar lenes ca
    ''' <c>OrdView.ActivatePage</c>. Toate paginile scriu in ACELASI graf, deci o pagina creata
    ''' tarziu vede tot ce s-a schimbat inainte de ea.
    ''' </summary>
    Private Sub ActivatePage(key As String)
        Try
            Dim page As IOrdEditPage = Nothing
            If Not _pages.TryGetValue(key, page) Then
                page = CreatePage(key)
                Dim ctrl As Control = DirectCast(page, Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                pnlPages.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                AddHandler page.DraftModificat, AddressOf Page_DraftModificat
                _pages(key) = page
            End If

            Dim previous As IOrdEditPage = _activePage
            _activePage = page
            DirectCast(page, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, page) Then
                DirectCast(previous, Control).Visible = False
            End If
            page.SetDraft(_draft)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.ActivatePage", ex)
            Throw
        End Try
    End Sub

    Private Function CreatePage(key As String) As IOrdEditPage
        Select Case key
            Case PAGINA_BENEFICIARI : Return New OrdBeneficiariPage()
            Case PAGINA_DOCUMENTE : Return New OrdDocumentePage()
            Case PAGINA_ATASAMENTE : Return New OrdAtasamentePage()
            Case Else
                Throw New ArgumentException($"Pagină de editare ORD necunoscută: '{key}'.", NameOf(key))
        End Select
    End Function

    ' Boundary UI (handler de eveniment): se logheaza si se inghite.
    Private Sub Page_DraftModificat(sender As Object, e As EventArgs)
        Try
            ActualizeazaAntet()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.Page_DraftModificat", ex)
        End Try
    End Sub

    ''' <summary>Rescrie numarul si totalul. Totalul se RECALCULEAZA din linii — o cifra care
    ''' ar ramane de la editarea trecuta e mai rea decat lipsa ei.</summary>
    Private Sub ActualizeazaAntet()
        lblNrOrd.Text = If(_draft.NrOrd > 0, _draft.NrOrd.ToString(_roCulture), "se alocă la salvare")
        lblTotal.Text = _draft.Total.ToString("N2", _roCulture)
    End Sub

    Private Sub DtpData_ValueChanged(sender As Object, e As EventArgs) Handles dtpData.ValueChanged
        Try
            _draft.DataOrd = dtpData.Value.Date
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.DtpData_ValueChanged", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Validarea, portata din frmFX_ORD.btnSav_Click
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' TOATE motivele pentru care documentul nu se poate salva, adunate. Lista goala =
    ''' se poate salva. Access construia la fel un <c>msgEroare</c>, rand cu rand: operatorul
    ''' vede tot ce are de reparat dintr-o data, nu primul lucru si apoi urmatorul.
    ''' </summary>
    Private Function MotiveDeRefuz() As List(Of String)
        Dim motive As New List(Of String)()

        If Not _draft.DataOrd.HasValue Then motive.Add("Data ordonanțării lipsește.")
        If String.IsNullOrWhiteSpace(_draft.Comp) Then motive.Add("Compartimentul lipsește.")
        If _draft.Iddf <= 0 Then
            motive.Add("IDDF lipsește (ordonanțarea nu e legată de niciun document de fundamentare).")
        End If
        If String.IsNullOrWhiteSpace(_draft.Cual) Then motive.Add("CUAL lipsește.")

        If _draft.Parteneri.Count = 0 Then motive.Add("Lipsește cel puțin un beneficiar.")
        For Each p As OrdDraftPart In _draft.Parteneri
            Dim eticheta As String = If(String.IsNullOrWhiteSpace(p.Counter), "?", p.Counter)
            If String.IsNullOrWhiteSpace(p.DenBene) Then
                motive.Add($"Denumirea beneficiarului lipsește (beneficiar #{eticheta}).")
            End If
            If String.IsNullOrWhiteSpace(p.CodFiscal) Then
                motive.Add($"Codul fiscal lipsește (beneficiar #{eticheta}).")
            End If
            If String.IsNullOrWhiteSpace(p.ContIban) Then
                motive.Add($"Contul IBAN lipsește (beneficiar #{eticheta}).")
            End If
        Next

        If _draft.Linii.Count = 0 Then motive.Add("Lipsește cel puțin un rând de plată.")
        Dim nr As Integer = 0
        For Each l As OrdDraftLinie In _draft.Linii
            nr += 1
            If l.Valoare = 0.0R Then motive.Add($"Valoare = 0 pe rândul de plată #{nr}.")
            If String.IsNullOrWhiteSpace(l.CodSsi) Then motive.Add($"Cod SSI lipsă pe rândul de plată #{nr}.")
            ' Cele trei de mai jos vin din cheile straine ale lui FX_ORD_TBL, pe care Access
            ' nu le avea. `IdClsf` are DEFAULT 0 SI cheie straina, deci un 0 ajuns la INSERT
            ' opreste tranzactia cu un errno care nu numeste nimic.
            If l.IdClsf = 0 Then motive.Add($"Clasificația lipsește pe rândul de plată #{nr}.")
            If String.IsNullOrWhiteSpace(l.CodAi) Then motive.Add($"CodAI lipsă pe rândul de plată #{nr}.")
            If l.IdUnitate = 0 Then motive.Add($"Unitatea lipsește pe rândul de plată #{nr}.")
        Next

        If _draft.Documente.Count = 0 Then
            motive.Add("Lipsește cel puțin un rând în documentele justificative.")
        ElseIf Not _draft.Documente.Any(Function(d) d.EsteText) Then
            motive.Add("Lipsește cel puțin un rând text în documentele justificative.")
        End If

        For Each a As OrdDraftAtt In _draft.Atasamente
            If String.IsNullOrWhiteSpace(a.NumeFisier) Then
                motive.Add("Un atașament nu are nume de fișier.")
                Exit For
            End If
        Next

        Return motive
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Salvarea
    ' ══════════════════════════════════════════════════════════════════════════

    ' Boundary UI async: se logheaza si se arata; un throw de aici ar cadea pe firul de UI.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            ntfMesaj.Clear()
            ntfMesaj.Visible = False

            Dim motive As List(Of String) = MotiveDeRefuz()
            If motive.Count > 0 Then
                Dim mesaj As New StringBuilder("Nu pot salva din următoarele motive:")
                For Each m As String In motive
                    mesaj.Append(vbCrLf).Append("- ").Append(m)
                Next
                MessageBox.Show(Me, mesaj.ToString(), "Salvează ordonanțarea",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            If MessageBox.Show(Me, "Salvez datele?", "Salvează ordonanțarea",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            btnSalveaza.Enabled = False
            btnRenunta.Enabled = False
            busyBar.Running = True
            Try
                ' ── FAZA INTAI: tot graful, o tranzactie ─────────────────────────────────
                Dim rezultat As OrdSaveRezultat = Await _withReauthSalvare(
                    Function() _apiClient.SaveOrdAsync(_draft, CancellationToken.None)).ConfigureAwait(True)

                _draft.AplicaHarta(rezultat.Idordp, rezultat.NrOrd,
                                   rezultat.Parts, rezultat.Linii, rezultat.Rec,
                                   rezultat.Doc, rezultat.Att)
                _IdordpSalvat = rezultat.Idordp
                _SAuSalvatModificari = True
                ActualizeazaAntet()

                ' ── FAZA A DOUA: octetii imaginilor ──────────────────────────────────────
                Dim esuate As List(Of String) = Await UrcaImaginileAsync().ConfigureAwait(True)
                If esuate.Count > 0 Then
                    ' Ordonantarea E salvata. NU se deruleaza nimic inapoi: un document pe
                    ' jumatate derulat e mai rau decat unul caruia ii lipseste o poza.
                    MessageBox.Show(Me,
                        "Ordonanțarea a fost salvată, dar aceste imagini nu s-au putut încărca: " &
                        String.Join(", ", esuate) & "." & vbCrLf & vbCrLf &
                        "Redeschideți ordonanțarea și încercați din nou să le atașați.",
                        "Salvează ordonanțarea", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If

                DialogResult = DialogResult.OK
                Close()
            Finally
                busyBar.Running = False
                btnSalveaza.Enabled = True
                btnRenunta.Enabled = True
            End Try
        Catch ex As ApiException
            ' Mesajul serverului e deja in romana si enumera toate motivele de refuz.
            GlobalErrorLog.Write("OrdEditForm.BtnSalveaza_Click", ex)
            MessageBox.Show(Me, ex.Message, "Salvează ordonanțarea",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.BtnSalveaza_Click", ex)
            MessageBox.Show(Me, "Ordonanțarea nu a putut fi salvată. Detalii în jurnalul de erori.",
                            "Salvează ordonanțarea", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Faza a doua: urca octetii fiecarei imagini schimbate, folosind cheile abia primite.
    ''' Intoarce numele celor care NU s-au putut incarca — niciodata o exceptie: la punctul
    ''' asta ordonantarea e deja salvata, iar o exceptie ar arunca peste bord un rezultat bun.
    ''' </summary>
    Private Async Function UrcaImaginileAsync() As Task(Of List(Of String))
        Dim esuate As New List(Of String)()
        For Each a As OrdDraftAtt In _draft.Atasamente
            If Not a.DeUrcat Then Continue For
            If a.Idordattp <= 0 Then
                ' Randul n-a primit cheie: harta serverului nu l-a cuprins. Zgomotos, nu tacut.
                esuate.Add(a.NumeFisier)
                Continue For
            End If

            Dim idordattp As Integer = a.Idordattp
            Dim nume As String = a.NumeFisier
            Dim octeti As Byte() = a.Continut
            Dim precedent As String = a.Sha256
            Try
                Dim raspuns As PutAtasamentResponse = Await _withReauthIncarcare(
                    Function() _apiClient.PutOrdAtasamentAsync(idordattp, nume, octeti, precedent,
                                                               CancellationToken.None)).ConfigureAwait(True)
                a.Sha256 = If(raspuns.sha256, String.Empty)
                a.TipMime = If(raspuns.tip_mime, String.Empty)
                a.Dimensiune = raspuns.dimensiune
                ' Urcata — nu se mai retrimite la urmatoarea salvare.
                a.Modificat = False
            Catch ex As Exception
                GlobalErrorLog.Write("OrdEditForm.UrcaImaginileAsync", ex)
                esuate.Add(nume)
            End Try
        Next
        Return esuate
    End Function

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            DialogResult = If(_SAuSalvatModificari, DialogResult.OK, DialogResult.Cancel)
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.BtnRenunta_Click", ex)
        End Try
    End Sub

    ''' <summary>Culorile semantice ale benzii de antet, reluate dupa o comutare de schema.</summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            tlyMain.BackColor = p.SurfaceAltColor
            tlyAntet.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            pnlPages.BackColor = p.SurfaceAltColor

            For Each capt As Label In New Label() {lblCodCaption, lblNrOrdCaption, lblDataCaption,
                                                   lblTotalCaption, lblObiectCaption}
                capt.ForeColor = p.TextDimColor
                capt.BackColor = Color.Transparent
            Next
            For Each val As Label In New Label() {lblCod, lblNrOrd, lblTotal, lblObiect}
                val.ForeColor = p.TextColor
                val.BackColor = Color.Transparent
            Next
            ' Totalul negativ e rosu, ca peste tot in aplicatie.
            If _draft IsNot Nothing AndAlso _draft.Total < 0 Then lblTotal.ForeColor = p.ErrorColor
        Catch ex As Exception
            GlobalErrorLog.Write("OrdEditForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
