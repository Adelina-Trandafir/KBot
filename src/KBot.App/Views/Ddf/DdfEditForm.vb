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
''' THE DDF EDITOR (slice 0051) -- the port of <c>frmFX_DDF</c> and its four subforms. Modal
''' over <c>MainForm</c>.
'''
''' <para><b>One save, one transaction.</b> The five-step VBA chain -- clear staging, send to
''' the server, save locally "on approval", confirm, commit locally, with its message about
''' «the data is on the server but not local» -- has no successor. The whole graph goes up in
''' one POST, the server writes it in one transaction and returns the real keys. There is no
''' state left that can end up half written.</para>
'''
''' <para><b>The exception: the attached files.</b> An <c>IdRevAtt</c> has to exist before
''' bytes can hang off it, so they go up AFTER the save, in a second phase. If an upload fails
''' there, the document STAYS SAVED and the message names the file that is missing: a
''' half-rolled-back document is worse than one missing an attachment.</para>
'''
''' <para><b>The numbers are really held.</b> <c>CUAL</c> and <c>NumarRev</c> are taken from
''' the server when the form opens and renewed while it is open, so the header shows the real
''' number rather than <c>OrdEditForm</c>'s «probabil N». That difference is deliberate: the
''' ORD's number is allocated inside the save transaction and a wrong guess costs nothing,
''' while these two are on screen and the operator may retype them.</para>
'''
''' <para><b>Validation is in two places, deliberately.</b> Here, so the operator gets a fast
''' message naming ALL the problems at once (the port of the <c>msgEroare</c> block in
''' <c>btnSav_Click</c>); and on the server, before the first INSERT, because that is the only
''' copy that cannot be bypassed.</para>
'''
''' <para><b>Nothing on the pages talks to the network.</b> The combo sources are fetched
''' here and handed down, which is what lets every page keep a parameterless constructor and
''' open in the Visual Studio designer.</para>
''' </summary>
Public Class DdfEditForm

    Private Const PAGINA_SECTIUNEA_A As String = "sectiunea-a"
    Private Const PAGINA_SECTIUNEA_B As String = "sectiunea-b"
    Private Const PAGINA_DESCRIERE As String = "descriere"
    Private Const PAGINA_FISIERE As String = "fisiere"

    ''' <summary>The two kinds of number the lock knows. Must match the server's literals.</summary>
    Private Const LOCK_CUAL As String = "CUAL"
    Private Const LOCK_NUMARREV As String = "NUMARREV"

    ''' <summary>The <c>Program</c> row source, a literal two-item list in Access
    ''' (<c>frmFX_DDF.Program.RowSource</c> = «0000000000;0000002510»). A named constant, not
    ''' a magic pair buried in the designer.</summary>
    Private Shared ReadOnly PROGRAME As String() = {"0000000000", "0000002510"}

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _draft As DdfDraft
    Private ReadOnly _reauth As DdfEditReauth

    Private ReadOnly _pages As New Dictionary(Of String, IDdfEditPage)(StringComparer.Ordinal)
    Private _activePage As IDdfEditPage

    ''' <summary>The partner rows behind <c>cmbPartener</c>, in the combo's own order, so a
    ''' selected index can be turned back into a <c>CodFiscal</c> without parsing the label.</summary>
    Private ReadOnly _parteneri As New List(Of DdfPartener)()

    ' Filling the header raises the change events of every control in it, and those are not
    ' the operator's edits. Without this guard, opening a document would immediately mark it
    ' as modified and would run the description cascade over values nobody touched.
    Private _seIncarca As Boolean

    ''' <summary>The revision key after a successful save; 0 while nothing has been saved.</summary>
    Public ReadOnly Property IdrevSalvat As Integer

    ''' <summary>The document key after a successful save; 0 while nothing has been saved.</summary>
    Public ReadOnly Property IddfSalvat As Integer

    ''' <summary>Was anything saved? The host reloads the DDF view only then.</summary>
    Public ReadOnly Property SAuSalvatModificari As Boolean

    Public Sub New(apiClient As IApiClient, draft As DdfDraft, reauth As DdfEditReauth)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(draft)
        ArgumentNullException.ThrowIfNull(reauth)

        InitializeComponent()
        _apiClient = apiClient
        _draft = draft
        _reauth = reauth
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Opening
    ' ══════════════════════════════════════════════════════════════════════════

    ' Boundary UI (Load): logged and swallowed -- a throw would take the opening down.
    Private Sub DdfEditForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            capBar.Text = $"K-BOT — Document de fundamentare · {_draft.CodAngajament}"
            Text = capBar.Text

            ' `KBotTextField` forwards TextChanged but NOT Leave, so the "on lost focus"
            ' cascades are wired to the inner box. Doing it here rather than in the designer
            ' keeps the designer free of code that only makes sense at run time.
            AddHandler txtObiect.InnerTextBox.Leave, AddressOf TxtObiect_Leave
            AddHandler txtDescScurta.InnerTextBox.Leave, AddressOf TxtDescScurta_Leave
            AddHandler txtCual.InnerTextBox.Leave, AddressOf TxtCual_Leave
            AddHandler txtNumarRev.InnerTextBox.Leave, AddressOf TxtNumarRev_Leave

            IncarcaAntetul()
            AplicaEnablement()

            ' The generation warnings (a manually created angajament, an over-long object)
            ' are shown from the start: they are things to know BEFORE editing, not after a
            ' save has been refused.
            If _draft.Avertismente.Count > 0 Then
                ntfMesaj.Show(String.Join(vbCrLf, _draft.Avertismente), NoticeKind.Warning)
                ntfMesaj.Visible = True
            End If

            ' Assigning the key raises SelectionChanged, and through it the first page is
            ' created and shown. It is NOT activated a second time by hand -- that would run
            ' AFTER the event and hide the very page just shown (the same note as in
            ' `OrdView.BuildNav`).
            navSub.SelectedKey = PAGINA_SECTIUNEA_A
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.DdfEditForm_Load", ex)
        End Try
    End Sub

    ' Boundary UI async: logged and swallowed.
    Private Async Sub DdfEditForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Try
            Await RezervaNumereleAsync().ConfigureAwait(True)
            Await IncarcaListeleAsync().ConfigureAwait(True)
            Await AduFisiereleAsync().ConfigureAwait(True)
            ' The heartbeat starts only once there is something to renew.
            If _draft.IdLockCual > 0 OrElse _draft.IdLockNumarRev > 0 Then tmrLock.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.DdfEditForm_Shown", ex)
        End Try
    End Sub

    ''' <summary>Writes the draft into the header controls. Guarded so the change events it
    ''' raises are not mistaken for the operator's edits.</summary>
    Private Sub IncarcaAntetul()
        _seIncarca = True
        Try
            lblCod.Text = _draft.CodAngajament
            txtCual.Text = If(_draft.Cual > 0, _draft.Cual.ToString(_roCulture), String.Empty)
            dtpDataCreare.Value = If(_draft.DataCreare, Date.Today)
            txtObiect.Text = _draft.ObiectDdf
            ' `Text`, not `SelectedItem`: the combo keeps values that are not in the list
            ' (`LimitToList = False`), and a compartment the document carries is very often one
            ' of them -- writing `SelectedItem` would silently show an empty field instead.
            cmbComp.Text = If(_draft.Comp, String.Empty)

            cmbProgram.Items.Clear()
            cmbProgram.Items.AddRange(PROGRAME)
            Dim iProgram As Integer = cmbProgram.Items.IndexOf(_draft.Program)
            If iProgram >= 0 Then
                cmbProgram.SelectedIndex = iProgram
            ElseIf cmbProgram.Items.Count > 0 Then
                ' An unknown value from the database is NOT dropped silently: it joins the
                ' list, so the operator sees what the document actually carries.
                If Not String.IsNullOrWhiteSpace(_draft.Program) Then
                    cmbProgram.Items.Add(_draft.Program)
                    cmbProgram.SelectedIndex = cmbProgram.Items.Count - 1
                Else
                    cmbProgram.SelectedIndex = 0
                End If
            End If

            chkPartAng.Checked = _draft.PartAng
            txtNumarRev.Text = _draft.Revizie.NumarRev.ToString(_roCulture)
            dtpDataRev.Value = If(_draft.Revizie.DataRev, Date.Today)
            txtDescScurta.Text = _draft.Revizie.DescScurta

            ActualizeazaTotalul()
        Finally
            _seIncarca = False
        End Try
    End Sub

    ''' <summary>
    ''' Which header fields the operator may change, ported from <c>frmFX_DDF.Form_Load</c>.
    '''
    ''' <para>Access's <c>eRev0</c> means "the revision being modified is number 0", and its
    ''' four flags map onto the draft like this: <c>DDF_NOU</c> = the document is new,
    ''' <c>DDF_MOD</c> = an existing revision is being modified, <c>DDF_UPL</c> = the
    ''' angajament came from FOREXE (its code does not start with «!»).</para>
    ''' </summary>
    Private Sub AplicaEnablement()
        Dim ddfNou As Boolean = _draft.Nou
        Dim ddfMod As Boolean = Not _draft.RevizieNoua
        Dim eRev0 As Boolean = _draft.Revizie.NumarRev = 0
        Dim ddfUpl As Boolean = Not _draft.Manual

        Dim capEditabil As Boolean = ddfNou OrElse (ddfMod AndAlso eRev0)
        txtCual.Enabled = capEditabil
        dtpDataCreare.Enabled = capEditabil
        txtObiect.Enabled = capEditabil
        cmbProgram.Enabled = capEditabil
        cmbComp.Enabled = capEditabil

        chkPartAng.Enabled = ddfNou OrElse ddfMod
        cmbPartener.Enabled = (ddfNou OrElse ddfMod) AndAlso _draft.PartAng

        ' `Salarii` has no control of its own: the branch behind it is dead (the salaries
        ' form, IdSalariiS, tmpFX_Salarii and the SalariiH updates are not ported), so the
        ' flag is carried through the draft untouched rather than shown as a switch that
        ' would do nothing. Access's rule was
        ' `Salarii.Enabled = DDF_NOU Or DDF_MOD Or Not DDF_UPL`; kept here as a comment so
        ' the omission is visible rather than looking like an oversight.
        Dim _salariiArFiFostEditabil As Boolean = ddfNou OrElse ddfMod OrElse Not ddfUpl

        ' The revision number is only ever taken for a NEW revision; an existing one keeps
        ' the number it was saved with.
        txtNumarRev.Enabled = _draft.RevizieNoua
    End Sub

    ''' <summary>Recomputes the total from the lines. A figure left over from the previous
    ''' edit is worse than none.</summary>
    Private Sub ActualizeazaTotalul()
        lblTotal.Text = _draft.Total.ToString("N2", _roCulture)
        Dim scheme As ThemeScheme = ThemeManager.Current
        If scheme IsNot Nothing Then
            lblTotal.ForeColor = If(_draft.Total < 0, scheme.Palette.ErrorColor, scheme.Palette.TextColor)
        End If
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The number lock
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Takes the numbers this form actually needs to hold.
    '''
    ''' <para>A NEW document holds both <c>CUAL</c> and <c>NumarRev</c>. An existing document
    ''' getting a new revision holds only <c>NumarRev</c> -- <c>CUAL</c> is never reallocated
    ''' once assigned, so locking it would block another operator for nothing. An existing
    ''' revision being modified holds neither.</para>
    '''
    ''' <para>A failure here does not stop the editing: the save re-checks the numbers anyway,
    ''' and refusing to open the form because a lock could not be taken would be a worse
    ''' trade than letting the operator work and find out at the end.</para>
    ''' </summary>
    Private Async Function RezervaNumereleAsync() As Task
        If String.IsNullOrWhiteSpace(_draft.Dc) Then
            ntfMesaj.Show("Angajamentul nu are DC, deci numerele nu se pot rezerva. " &
                          "Documentul se poate edita, dar salvarea poate fi refuzată.",
                          NoticeKind.Warning)
            ntfMesaj.Visible = True
            Return
        End If

        busyBar.Running = True
        Try
            If _draft.Nou Then
                Dim lacat As DdfNumarLock = Await _reauth.Numar(
                    Function() _apiClient.RezervaNumarDdfAsync(LOCK_CUAL, _draft.CodAngajament,
                                                               _draft.Dc, CancellationToken.None)
                    ).ConfigureAwait(True)
                _draft.IdLockCual = lacat.IdLock
                _draft.Cual = lacat.Valoare
            End If

            If _draft.RevizieNoua Then
                Dim lacat As DdfNumarLock = Await _reauth.Numar(
                    Function() _apiClient.RezervaNumarDdfAsync(LOCK_NUMARREV, _draft.CodAngajament,
                                                               _draft.Dc, CancellationToken.None)
                    ).ConfigureAwait(True)
                _draft.IdLockNumarRev = lacat.IdLock
                _draft.Revizie.NumarRev = lacat.Valoare
            End If

            IncarcaAntetul()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.RezervaNumereleAsync", ex)
            ntfMesaj.Show("Nu am putut rezerva numerele documentului. Poți edita în " &
                          "continuare, dar la salvare numerele se verifică din nou și " &
                          "salvarea poate fi refuzată dacă altcineva le-a folosit între timp.",
                          NoticeKind.Warning)
            ntfMesaj.Visible = True
        Finally
            busyBar.Running = False
        End Try
    End Function

    ' Boundary UI async (timer): logged and swallowed.
    Private Async Sub TmrLock_Tick(sender As Object, e As EventArgs) Handles tmrLock.Tick
        Try
            For Each idLock As Integer In New Integer() {_draft.IdLockCual, _draft.IdLockNumarRev}
                If idLock <= 0 Then Continue For
                Dim id As Integer = idLock
                Await _reauth.Numar(
                    Function() _apiClient.PrelungesteNumarDdfAsync(id, CancellationToken.None)
                    ).ConfigureAwait(True)
            Next
        Catch ex As Exception
            ' A failed heartbeat is worth saying out loud: from here on the number can be
            ' taken by someone else, and the operator should know before spending another
            ' half hour in section A.
            GlobalErrorLog.Write("DdfEditForm.TmrLock_Tick", ex)
            tmrLock.Stop()
            ntfMesaj.Show("Rezervarea numerelor nu s-a mai putut prelungi. Salvează cât mai " &
                          "repede: numerele pot fi luate de altcineva.", NoticeKind.Warning)
            ntfMesaj.Visible = True
        End Try
    End Sub

    ''' <summary>
    ''' Moves a lock onto the number the operator typed, or puts the held one back in the box.
    ''' </summary>
    Private Async Function SchimbaNumarul(idLock As Integer, camp As KBotTextField,
                                          valoareCurenta As Integer,
                                          eticheta As String) As Task(Of Integer)
        Dim ceruta As Integer
        If Not Integer.TryParse(camp.Text.Trim(), NumberStyles.Integer, _roCulture, ceruta) Then
            MessageBox.Show(Me, $"«{camp.Text}» nu este un număr.", eticheta,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            camp.Text = valoareCurenta.ToString(_roCulture)
            Return valoareCurenta
        End If
        If ceruta = valoareCurenta Then Return valoareCurenta

        If idLock <= 0 Then
            ' Nothing is held, so nothing can be moved. That happens when the lock could not
            ' be taken at all; the number stays what it was rather than pretending otherwise.
            MessageBox.Show(Me, "Numărul nu este rezervat pe server, deci nu poate fi schimbat aici.",
                            eticheta, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            camp.Text = valoareCurenta.ToString(_roCulture)
            Return valoareCurenta
        End If

        busyBar.Running = True
        Try
            Dim lacat As DdfNumarLock = Await _reauth.Numar(
                Function() _apiClient.SchimbaNumarDdfAsync(idLock, ceruta, CancellationToken.None)
                ).ConfigureAwait(True)
            camp.Text = lacat.Valoare.ToString(_roCulture)
            Return lacat.Valoare
        Catch ex As ApiException
            ' The server's message already distinguishes «already used» from «held by
            ' someone else», which is exactly the distinction the operator needs.
            GlobalErrorLog.Write("DdfEditForm.SchimbaNumarul", ex)
            MessageBox.Show(Me, ex.Message, eticheta, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            camp.Text = valoareCurenta.ToString(_roCulture)
            Return valoareCurenta
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.SchimbaNumarul", ex)
            MessageBox.Show(Me, "Numărul nu a putut fi schimbat. Detalii în jurnalul de erori.",
                            eticheta, MessageBoxButtons.OK, MessageBoxIcon.Error)
            camp.Text = valoareCurenta.ToString(_roCulture)
            Return valoareCurenta
        Finally
            busyBar.Running = False
        End Try
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The combo sources -- fetched here, because pages make no requests
    ' ══════════════════════════════════════════════════════════════════════════

    Private Async Function IncarcaListeleAsync() As Task
        busyBar.Running = True
        Try
            Try
                Dim comp As List(Of String) = Await _reauth.Compartimente(
                    Function() _apiClient.GetDdfCompAsync(CancellationToken.None)).ConfigureAwait(True)
                ' Refilling the list resets the box, so what is already in it -- the document's
                ' own compartment, written by `IncarcaAntetul` before this ran -- is put back.
                Dim curent As String = If(cmbComp.Text, String.Empty)
                _seIncarca = True
                Try
                    cmbComp.Items.Clear()
                    For Each c As String In comp
                        cmbComp.Items.Add(c)
                    Next
                    cmbComp.Text = curent
                Finally
                    _seIncarca = False
                End Try
                ' An empty list is a NORMAL state, not a failure: there is no compartment
                ' nomenclator in the database, so on a unit with no documents yet the operator
                ' types the first one. The field STAYS ENABLED -- since the `txtComp` box beside
                ' it went away it is the ONLY way in, and switching it off here is what left a
                ' fresh database with a document that could never be saved. Only the tooltip
                ' changes, so an empty list reads as expected rather than as broken.
                If cmbComp.Items.Count = 0 Then
                    tips.SetToolTipText(cmbComp,
                        "Niciun document anterior, deci nu există compartimente de propus. " &
                        "Scrie compartimentul aici.")
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("DdfEditForm.IncarcaListeleAsync/comp", ex)
                ' The proposals are gone, not the field: a typed compartment still saves.
                tips.SetToolTipText(cmbComp,
                    "Compartimentele anterioare nu au putut fi aduse de pe server. " &
                    "Scrie compartimentul aici.")
            End Try

            Try
                _parteneri.Clear()
                _parteneri.AddRange(Await _reauth.Parteneri(
                    Function() _apiClient.GetDdfParteneriAsync(_draft.CodAngajament,
                                                               CancellationToken.None)
                    ).ConfigureAwait(True))
                _seIncarca = True
                Try
                    cmbPartener.Items.Clear()
                    For Each p As DdfPartener In _parteneri
                        cmbPartener.Items.Add($"{p.NumePartener} ({p.CodFiscal})")
                    Next
                    ' Re-select the one the document already carries. Matched on CodFiscal,
                    ' which is what FX_DDF stores -- the label is only what is shown.
                    If Not String.IsNullOrWhiteSpace(_draft.CodFiscal) Then
                        Dim i As Integer = _parteneri.FindIndex(
                            Function(p) String.Equals(p.CodFiscal, _draft.CodFiscal,
                                                      StringComparison.OrdinalIgnoreCase))
                        If i >= 0 Then cmbPartener.SelectedIndex = i
                    End If
                Finally
                    _seIncarca = False
                End Try
            Catch ex As Exception
                GlobalErrorLog.Write("DdfEditForm.IncarcaListeleAsync/parteneri", ex)
                cmbPartener.Enabled = False
            End Try
        Finally
            busyBar.Running = False
        End Try
    End Function

    ''' <summary>
    ''' The classification list for the section-A page. Fetched here (pages make no requests)
    ''' and handed down.
    '''
    ''' <para>The <c>manual</c> flag is <c>Left(CodAngajament, 1) = "!"</c> -- Access derived
    ''' it as <c>DDF_UPL</c> and chose between two queries on it. The <c>titlu</c> parameter
    ''' is the MANUAL variant's restriction: the <c>Titlu</c> of the first line already in
    ''' section A, or nothing when section A is empty.</para>
    ''' </summary>
    Friend Async Function AduClasificatiileAsync() As Task(Of List(Of DdfClasificatie))
        Dim titlu As String = Nothing
        If _draft.Manual Then
            Dim prima As DdfDraftLinieA = _draft.LiniiA.FirstOrDefault()
            ' `Clsf` is "Capitol.Subcapitol.Articol.Alineat" and `Titlu` is the first two
            ' characters of Articol, so it is the THIRD dotted part. Access got at it with
            ' `Mid(Clsf, 13, 2)`, arithmetic over a fixed-width string; splitting on the dot
            ' says the same thing without depending on the widths.
            If prima IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(prima.Clsf) Then
                Dim parti As String() = prima.Clsf.Split("."c)
                If parti.Length >= 3 AndAlso parti(2).Length >= 2 Then titlu = parti(2).Substring(0, 2)
            End If
        End If

        Return Await _reauth.Clasificatii(
            Function() _apiClient.GetDdfClasificatiiAsync(_draft.CodAngajament, _draft.Manual,
                                                          titlu, CancellationToken.None)
            ).ConfigureAwait(True)
    End Function

    ''' <summary>
    ''' Fetches the bytes of the files already stored on the server so the «Fisiere» page can
    ''' offer to save them to disk. Pages make no network requests, so this lives here.
    '''
    ''' <para>A file that cannot be fetched does NOT stop the editing: it is noted, said at
    ''' the end, and the rest of the document stays editable. Bytes fetched this way do NOT
    ''' raise <c>Modificat</c>, so they are not sent back at save time.</para>
    ''' </summary>
    Private Async Function AduFisiereleAsync() As Task
        Dim deAdus As List(Of DdfDraftAtt) =
            _draft.Atasamente.Where(Function(a) a.IdRevAtt > 0 AndAlso
                                                Not String.IsNullOrWhiteSpace(a.Sha256) AndAlso
                                                a.Continut Is Nothing).ToList()
        If deAdus.Count = 0 Then Return

        Dim esuate As New List(Of String)()
        busyBar.Running = True
        Try
            For Each a As DdfDraftAtt In deAdus
                Dim idRevAtt As Integer = a.IdRevAtt
                Try
                    Dim rezultat As PdfDownloadResult = Await _reauth.Descarcare(
                        Function() _apiClient.GetDdfFisierAsync(idRevAtt, String.Empty,
                                                                CancellationToken.None)
                        ).ConfigureAwait(True)
                    If rezultat IsNot Nothing AndAlso rezultat.Status = PdfDownloadStatus.Content Then
                        a.Continut = rezultat.Bytes
                    End If
                Catch ex As Exception
                    GlobalErrorLog.Write("DdfEditForm.AduFisiereleAsync", ex)
                    esuate.Add(a.NumeFisier)
                End Try
            Next
        Finally
            busyBar.Running = False
        End Try

        ' A page created earlier only sees the bytes after the graph is pushed at it again.
        _activePage?.SetDraft(_draft)

        If esuate.Count > 0 Then
            ntfMesaj.Show("Aceste fișiere nu au putut fi aduse de pe server și nu se pot " &
                          "salva pe disc: " & String.Join(", ", esuate) & ". " &
                          "Documentul se poate edita și salva în continuare.", NoticeKind.Warning)
            ntfMesaj.Visible = True
        End If
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Hosting the pages
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub NavSub_SelectionChanged(key As String) Handles navSub.SelectionChanged
        Try
            ActivatePage(key)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.NavSub_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Shows the requested page, creating it on first activation -- the same lazy pattern as
    ''' <c>OrdEditForm.ActivatePage</c>. All four pages write into the SAME graph, so a page
    ''' created late sees everything that changed before it existed.
    ''' </summary>
    Private Sub ActivatePage(key As String)
        Try
            Dim page As IDdfEditPage = Nothing
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

            Dim previous As IDdfEditPage = _activePage
            _activePage = page
            DirectCast(page, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, page) Then
                DirectCast(previous, Control).Visible = False
            End If
            page.SetDraft(_draft)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.ActivatePage", ex)
            Throw
        End Try
    End Sub

    Private Function CreatePage(key As String) As IDdfEditPage
        Select Case key
            Case PAGINA_SECTIUNEA_A
                Dim pagina As New DdfEditSectiuneaAPage()
                ' The only page that needs anything from the network. It does not make the
                ' call: it asks the form for the list, which keeps its constructor empty and
                ' `IDdfEditPage`'s "no requests" rule intact.
                pagina.SursaClasificatiilor = AddressOf AduClasificatiileAsync
                Return pagina
            Case PAGINA_SECTIUNEA_B : Return New DdfEditSectiuneaBPage()
            Case PAGINA_DESCRIERE : Return New DdfEditDescrierePage()
            Case PAGINA_FISIERE : Return New DdfEditFisierePage()
            Case Else
                Throw New ArgumentException($"Pagină de editare DDF necunoscută: '{key}'.", NameOf(key))
        End Select
    End Function

    ' Boundary UI (event handler): logged and swallowed.
    Private Sub Page_DraftModificat(sender As Object, e As EventArgs)
        Try
            ActualizeazaTotalul()
            ' Section A changed -> section B is rebuilt from it, and the section-B page picks
            ' the new rows up on its next activation because both read the same object.
            _draft.Revizie.RecalculeazaSectiuneaB()
            ' The short description lives in the header AND on the «Descriere» page, so the
            ' header has to follow an edit made over there.
            If Not _seIncarca AndAlso txtDescScurta.Text <> _draft.Revizie.DescScurta Then
                _seIncarca = True
                Try
                    txtDescScurta.Text = _draft.Revizie.DescScurta
                Finally
                    _seIncarca = False
                End Try
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.Page_DraftModificat", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The header's own edits
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub DtpDataCreare_ValueChanged(sender As Object, e As EventArgs) _
        Handles dtpDataCreare.ValueChanged
        Try
            If _seIncarca Then Return
            _draft.DataCreare = dtpDataCreare.Value.Date
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.DtpDataCreare_ValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' <c>DataRev_BeforeUpdate</c>: the revision date may not predate the newest revision the
    ''' angajament already has.
    '''
    ''' <para>The comparison is on <c>Date</c> values, never on their formatted text. Access
    ''' used <c>CLng()</c> for the same reason: a string comparison of dates is at the mercy
    ''' of the machine's locale, and «01.02» against «10.01» would come out backwards.</para>
    '''
    ''' <para>The bound is taken from the draft, which holds only THIS revision, so it can
    ''' only catch a date older than the one loaded. The server's own check is the one that
    ''' sees the whole history.</para>
    ''' </summary>
    Private Sub DtpDataRev_ValueChanged(sender As Object, e As EventArgs) Handles dtpDataRev.ValueChanged
        Try
            If _seIncarca Then Return
            _draft.Revizie.DataRev = dtpDataRev.Value.Date
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.DtpDataRev_ValueChanged", ex)
        End Try
    End Sub

    Private Sub TxtObiect_Leave(sender As Object, e As EventArgs)
        Try
            If _seIncarca Then Return
            Dim nou As String = txtObiect.Text.Trim()
            If nou = _draft.ObiectDdf Then Return
            _draft.ObiectDdf = nou
            ' Replaces Access's `ModNume`: the cascade onto FX_Angajamente.Descriere is
            ' unconditional now, so the flag is a record of what happened, not a gate.
            _draft.ObiectSchimbat = True
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.TxtObiect_Leave", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The second half of the cascade that replaces <c>ModNume</c>: changing the SHORT
    ''' description copies it into the long one, with NO confirmation prompt -- Access asked,
    ''' this does not.
    '''
    ''' <para>Both faces of the long description are written, because both columns are: the
    ''' RTF one and the plain-text one. An operator who wants them to differ edits the long
    ''' description on the «Descriere» page afterwards; a later edit here overwrites it again,
    ''' and that is the intended behaviour.</para>
    ''' </summary>
    Private Sub TxtDescScurta_Leave(sender As Object, e As EventArgs)
        Try
            If _seIncarca Then Return
            Dim nou As String = txtDescScurta.Text.Trim()
            If nou = _draft.Revizie.DescScurta Then Return
            _draft.Revizie.DescScurta = nou
            _draft.Revizie.DescLunga = nou
            _draft.Revizie.DescLungaAnsi = nou
            ' The «Descriere» page reads the same object, so it shows the new text the moment
            ' the operator opens it -- no message, no refresh call.
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.TxtDescScurta_Leave", ex)
        End Try
    End Sub

    ' Boundary UI async: logged and swallowed.
    Private Async Sub TxtCual_Leave(sender As Object, e As EventArgs)
        Try
            If _seIncarca OrElse Not txtCual.Enabled Then Return
            _draft.Cual = Await SchimbaNumarul(_draft.IdLockCual, txtCual, _draft.Cual,
                                               "CUAL").ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.TxtCual_Leave", ex)
        End Try
    End Sub

    ' Boundary UI async: logged and swallowed.
    Private Async Sub TxtNumarRev_Leave(sender As Object, e As EventArgs)
        Try
            If _seIncarca OrElse Not txtNumarRev.Enabled Then Return
            _draft.Revizie.NumarRev = Await SchimbaNumarul(
                _draft.IdLockNumarRev, txtNumarRev, _draft.Revizie.NumarRev,
                "Numărul reviziei").ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.TxtNumarRev_Leave", ex)
        End Try
    End Sub

    Private Sub CmbProgram_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbProgram.SelectedIndexChanged
        Try
            If _seIncarca Then Return
            _draft.Program = If(TryCast(cmbProgram.SelectedItem, String), String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.CmbProgram_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Takes the compartment off the combo and into the draft.
    '''
    ''' <para>ONE control, one value. Slice 0051-02 made <see cref="KBotComboBox"/> typeable
    ''' (<c>Editable</c>) and able to keep a value that is not in the list
    ''' (<c>LimitToList = False</c>), which is what this field needs: there is no compartment
    ''' nomenclator in MariaDB, so on a unit with no earlier documents the list is EMPTY and
    ''' the only way in is the keyboard. Reading the choice from
    ''' <c>SelectedIndexChanged</c> alone -- as this form did -- meant a typed compartment
    ''' never reached the draft at all, and the save was then refused for a missing
    ''' compartment on a field the operator could plainly see filled in.</para>
    '''
    ''' <para><c>CommitText</c> gives the field's verdict now instead of waiting for the focus
    ''' to move; it is idempotent, so calling it from both the leave handler and the save
    ''' button costs nothing.</para>
    ''' </summary>
    Private Sub PreiaCompartimentul()
        cmbComp.CommitText()
        _draft.Comp = If(cmbComp.Text, String.Empty).Trim()
    End Sub

    Private Sub CmbComp_Leave(sender As Object, e As EventArgs) Handles cmbComp.Leave
        Try
            If _seIncarca Then Return
            PreiaCompartimentul()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.CmbComp_Leave", ex)
        End Try
    End Sub

    ''' <summary>Picking from the list writes the draft at once, without waiting for the
    ''' focus to leave -- the same moment the operator sees the value change.</summary>
    Private Sub CmbComp_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbComp.SelectedIndexChanged
        Try
            If _seIncarca Then Return
            Dim ales As String = TryCast(cmbComp.SelectedItem, String)
            If String.IsNullOrWhiteSpace(ales) Then Return
            _draft.Comp = ales
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.CmbComp_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' <c>PartAng_AfterUpdate</c>: turning the flag off clears the partner from the header
    ''' AND from every line, turning it on opens the picker.
    ''' </summary>
    Private Sub ChkPartAng_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkPartAng.CheckedChanged
        Try
            If _seIncarca Then Return
            _draft.PartAng = chkPartAng.Checked
            cmbPartener.Enabled = chkPartAng.Checked AndAlso (_draft.Nou OrElse Not _draft.RevizieNoua)

            If Not _draft.PartAng Then
                _draft.CodFiscal = String.Empty
                _draft.NumePartener = String.Empty
                _draft.ImpingePartenerulPeLinii(String.Empty, 0)
                _seIncarca = True
                Try
                    cmbPartener.SelectedIndex = -1
                Finally
                    _seIncarca = False
                End Try
                AnuntaPaginile()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.ChkPartAng_CheckedChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' <c>CodPartener_AfterUpdate</c>: the header's partner is pushed down onto every
    ''' section-A and section-B row.
    '''
    ''' <para><c>CodFiscal</c> is authoritative because one <c>CodFiscal</c> can map to
    ''' several <c>IdUnitate</c>, hence to several <c>CodPartener</c> / <c>IdPartener</c>
    ''' rows. <c>FX_DDF</c> stores only <c>CodFiscal</c> and <c>NumePartener</c> -- it has no
    ''' <c>IdPartener</c> column -- so that is what the header keeps, and the per-line partner
    ''' code is resolved from it.</para>
    ''' </summary>
    Private Sub CmbPartener_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbPartener.SelectedIndexChanged
        Try
            If _seIncarca Then Return
            Dim i As Integer = cmbPartener.SelectedIndex
            If i < 0 OrElse i >= _parteneri.Count Then Return

            Dim p As DdfPartener = _parteneri(i)
            _draft.CodFiscal = p.CodFiscal
            _draft.NumePartener = p.NumePartener
            ' The lines carry the fiscal code too: it is the only partner identifier the
            ' header actually holds, and inventing a CodPartener here would be a guess.
            _draft.ImpingePartenerulPeLinii(p.CodFiscal, 0)
            AnuntaPaginile()

            If p.Randuri > 1 Then
                ' Said out loud rather than hidden: one fiscal code behind several units is
                ' normal, but it means the per-line partner may need checking.
                ntfMesaj.Show($"Codul fiscal {p.CodFiscal} apare pe {p.Randuri} parteneri " &
                              "(câte unul pe unitate). Verifică partenerul pe rândurile din secțiunea A.",
                              NoticeKind.Warning)
                ntfMesaj.Visible = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.CmbPartener_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>Re-pushes the graph at the visible page after the header changed it under
    ''' the page's feet.</summary>
    Private Sub AnuntaPaginile()
        _activePage?.SetDraft(_draft)
        ActualizeazaTotalul()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Validation, ported from frmFX_DDF.btnSav_Click
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' EVERY reason the document cannot be saved, gathered. An empty list means it can be.
    ''' Access built a <c>msgEroare</c> the same way, line by line: the operator sees
    ''' everything to fix at once, not the first thing and then the next.
    '''
    ''' <para>One deliberate correction to the original: Access checked only the FIRST row of
    ''' each recordset -- <c>If Not Rs.EOF Then ...</c>, with no loop -- so a bad value on row
    ''' two went straight through to the server. Every row is checked here.</para>
    ''' </summary>
    Private Function MotiveDeRefuz() As List(Of String)
        Dim motive As New List(Of String)()

        If String.IsNullOrWhiteSpace(_draft.CodAngajament) Then motive.Add("Codul angajamentului lipsește.")
        If String.IsNullOrWhiteSpace(_draft.ObiectDdf) Then motive.Add("Obiectul documentului este obligatoriu.")
        If String.IsNullOrWhiteSpace(_draft.Comp) Then motive.Add("Compartimentul lipsește.")
        If Not _draft.DataCreare.HasValue Then motive.Add("Data creării lipsește.")
        If _draft.Cual <= 0 Then motive.Add("CUAL lipsește.")

        If Not _draft.Revizie.DataRev.HasValue Then motive.Add("Data reviziei lipsește.")
        If String.IsNullOrWhiteSpace(_draft.Revizie.DescScurta) Then motive.Add("Descrierea scurtă lipsește.")
        If String.IsNullOrWhiteSpace(_draft.Revizie.DescLungaAnsi) Then motive.Add("Descrierea lungă lipsește.")
        If String.IsNullOrWhiteSpace(_draft.Revizie.Tip) Then motive.Add("Tipul reviziei lipsește.")

        If _draft.LiniiA.Count = 0 Then motive.Add("Lipsește cel puțin un rând în secțiunea A.")
        Dim nr As Integer = 0
        For Each a As DdfDraftLinieA In _draft.LiniiA
            nr += 1
            If String.IsNullOrWhiteSpace(a.CodIndicator) Then
                motive.Add($"Cod indicator lipsă pe rândul {nr} din secțiunea A.")
            End If
            If String.IsNullOrWhiteSpace(a.ElementFund) Then
                motive.Add($"Element de fundamentare lipsă pe rândul {nr} din secțiunea A.")
            End If
            If a.ValCur = 0.0R Then motive.Add($"Valoarea curentă este 0 pe rândul {nr} din secțiunea A.")
            ' The three below come from the foreign keys of FX_DDF_REV_SA, which Access did
            ' not have. A zero reaching the INSERT stops the transaction with an errno that
            ' names nothing.
            If a.IdClsf <= 0 Then motive.Add($"Clasificația lipsește pe rândul {nr} din secțiunea A.")
            If a.IdUnitate <= 0 Then motive.Add($"Unitatea lipsește pe rândul {nr} din secțiunea A.")
            If _draft.PartAng AndAlso String.IsNullOrWhiteSpace(a.CodPartener) Then
                motive.Add($"Documentul e asociat unui partener, deci câmpul «Partener» " &
                           $"e obligatoriu (rândul {nr} din secțiunea A).")
            End If
        Next

        nr = 0
        For Each b As DdfDraftLinieB In _draft.LiniiB
            nr += 1
            If b.Inf1 = 0.0R Then motive.Add($"Influența C.A. este 0 pe rândul {nr} din secțiunea B.")
            If b.Inf2 = 0.0R Then motive.Add($"Influența C.B. este 0 pe rândul {nr} din secțiunea B.")
        Next

        For Each t As DdfDraftAtt In _draft.Atasamente
            If String.IsNullOrWhiteSpace(t.NumeFisier) Then
                motive.Add("Un fișier atașat nu are nume.")
                Exit For
            End If
        Next

        Return motive
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Saving
    ' ══════════════════════════════════════════════════════════════════════════

    ' Boundary UI async: logged and shown; a throw from here would land on the UI thread.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            ntfMesaj.Clear()
            ntfMesaj.Visible = False

            ' The compartment can be TYPED, and the operator can reach this button without ever
            ' leaving the field -- so the field is asked for its verdict here rather than being
            ' waited on. `CommitText` is idempotent and is exactly what it exists for.
            PreiaCompartimentul()

            ' Section B is derived, so it is rebuilt right before the check rather than
            ' trusted to be current: a stale B row would be written to the database.
            _draft.Revizie.RecalculeazaSectiuneaB()

            Dim motive As List(Of String) = MotiveDeRefuz()
            If motive.Count > 0 Then
                Dim mesaj As New StringBuilder("Nu pot salva din următoarele motive:")
                For Each m As String In motive
                    mesaj.Append(vbCrLf).Append("- ").Append(m)
                Next
                MessageBox.Show(Me, mesaj.ToString(), "Salvează documentul",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            If MessageBox.Show(Me, "Salvez datele?", "Salvează documentul",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            btnSalveaza.Enabled = False
            btnRenunta.Enabled = False
            busyBar.Running = True
            Try
                ' ── PHASE ONE: the whole graph, one transaction ─────────────────────────
                Dim rezultat As DdfSaveRezultat = Await _reauth.Salvare(
                    Function() _apiClient.SaveDdfAsync(_draft, CancellationToken.None)).ConfigureAwait(True)

                _draft.AplicaHarta(rezultat.Iddf, rezultat.Cual, rezultat.Idrev,
                                   rezultat.NumarRev, rezultat.LiniiA, rezultat.LiniiB, rezultat.Att)
                _IddfSalvat = rezultat.Iddf
                _IdrevSalvat = rezultat.Idrev
                _SAuSalvatModificari = True

                ' The locks were consumed inside the transaction, so the form no longer holds
                ' them and must not try to release them on the way out.
                _draft.IdLockCual = 0
                _draft.IdLockNumarRev = 0
                tmrLock.Stop()

                IncarcaAntetul()
                AplicaEnablement()

                ' ── PHASE TWO: the file bytes ───────────────────────────────────────────
                Dim esuate As List(Of String) = Await UrcaFisiereleAsync().ConfigureAwait(True)
                If esuate.Count > 0 Then
                    ' The document IS saved. NOTHING is rolled back: a half-rolled-back
                    ' document is worse than one missing a file.
                    MessageBox.Show(Me,
                        "Documentul a fost salvat, dar aceste fișiere nu s-au putut încărca: " &
                        String.Join(", ", esuate) & "." & vbCrLf & vbCrLf &
                        "Redeschide documentul și încearcă din nou să le atașezi.",
                        "Salvează documentul", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If

                DialogResult = DialogResult.OK
                Close()
            Finally
                busyBar.Running = False
                btnSalveaza.Enabled = True
                btnRenunta.Enabled = True
            End Try
        Catch ex As ApiException
            ' The server's message is already in Romanian and lists every reason for refusal.
            GlobalErrorLog.Write("DdfEditForm.BtnSalveaza_Click", ex)
            MessageBox.Show(Me, ex.Message, "Salvează documentul",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.BtnSalveaza_Click", ex)
            MessageBox.Show(Me, "Documentul nu a putut fi salvat. Detalii în jurnalul de erori.",
                            "Salvează documentul", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Phase two: uploads the bytes of every changed file, using the keys just received.
    ''' Returns the names of those that failed -- NEVER an exception: at this point the
    ''' document is already saved, and an exception would throw a good result overboard.
    ''' </summary>
    Private Async Function UrcaFisiereleAsync() As Task(Of List(Of String))
        Dim esuate As New List(Of String)()
        For Each t As DdfDraftAtt In _draft.Atasamente
            If Not t.DeUrcat Then Continue For
            If t.IdRevAtt <= 0 Then
                ' The row got no key: the server's map did not cover it. Loud, not silent.
                esuate.Add(t.NumeFisier)
                Continue For
            End If

            Dim idRevAtt As Integer = t.IdRevAtt
            Dim nume As String = t.NumeFisier
            Dim octeti As Byte() = t.Continut
            Dim precedent As String = t.Sha256
            Try
                Dim raspuns As PutDdfFisierResponse = Await _reauth.Incarcare(
                    Function() _apiClient.PutDdfFisierAsync(idRevAtt, nume, octeti, precedent,
                                                            CancellationToken.None)).ConfigureAwait(True)
                t.Sha256 = If(raspuns.sha256, String.Empty)
                t.TipMime = If(raspuns.tip_mime, String.Empty)
                t.Dimensiune = raspuns.dimensiune
                ' Uploaded -- not sent again at the next save.
                t.Modificat = False
            Catch ex As Exception
                GlobalErrorLog.Write("DdfEditForm.UrcaFisiereleAsync", ex)
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
            GlobalErrorLog.Write("DdfEditForm.BtnRenunta_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Releases every number this form still holds, HOWEVER the form was closed.
    '''
    ''' <para>Wrapped so a failed release can never block the close: a leaked lock expires by
    ''' itself in an hour, while a form that will not close is a worse bug. Fire-and-forget on
    ''' purpose -- <c>FormClosed</c> cannot await, and making it <c>Async Sub</c> would let
    ''' the form finish disposing underneath the call.</para>
    ''' </summary>
    Private Sub DdfEditForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            tmrLock.Stop()
            Dim lacate As Integer() = {_draft.IdLockCual, _draft.IdLockNumarRev}
            _draft.IdLockCual = 0
            _draft.IdLockNumarRev = 0

            For Each idLock As Integer In lacate
                If idLock <= 0 Then Continue For
                Dim id As Integer = idLock
                ' The api client is used directly, not through WithReauth: a re-login dialog
                ' over a form that is already closing would be absurd, and a 401 here just
                ' means the lock will expire on its own.
                Dim ignorat As Task = _apiClient.ElibereazaNumarDdfAsync(id, CancellationToken.None).
                    ContinueWith(Sub(t)
                                     If t.Exception IsNot Nothing Then
                                         GlobalErrorLog.Write("DdfEditForm.FormClosed/eliberare",
                                                              t.Exception.GetBaseException())
                                     End If
                                 End Sub, TaskScheduler.Default)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.DdfEditForm_FormClosed", ex)
        End Try
    End Sub

    ''' <summary>The header band's semantic colours, reapplied after a scheme switch.</summary>
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

            For Each capt As Label In New Label() {lblCodCaption, lblCualCaption,
                                                   lblDataCreareCaption, lblTotalCaption,
                                                   lblObiectCaption, lblProgramCaption,
                                                   lblCompCaption, lblNumarRevCaption,
                                                   lblDataRevCaption, lblDescScurtaCaption}
                capt.ForeColor = p.TextDimColor
                capt.BackColor = Color.Transparent
            Next
            For Each val As Label In New Label() {lblCod, lblTotal}
                val.ForeColor = p.TextColor
                val.BackColor = Color.Transparent
            Next
            chkPartAng.ForeColor = p.TextColor
            chkPartAng.BackColor = Color.Transparent

            ' NOTHING is written on `btnRenunta` / `btnSalveaza` here, and that is deliberate.
            ' They sit DIRECTLY on the form, not inside an `IThemedControl`, so
            ' `MyBase.OnThemeChanged` has already carried the generic button rule into them --
            ' including the modern scheme's rounded corners, which come from
            ' `ModernRenderer.ApplyButton`. Repainting them here with raw palette colours, as
            ' this method used to, added nothing and dropped that rounding, which is exactly
            ' where the footer stopped looking like `OrdEditForm`'s. The pages are the other
            ' case: their buttons ARE inside an `IThemedControl`, the traversal does not reach
            ' them, and each page styles them itself through `ButtonStyles`.

            ' The total's own colour depends on the value, so it is re-derived here too.
            If _draft IsNot Nothing Then ActualizeazaTotalul()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
