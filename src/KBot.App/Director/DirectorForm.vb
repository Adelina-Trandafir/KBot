Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0081-06 -- the director's K-BOT. Opened by <c>Program</c> instead of the main shell when
''' the login's role is <see cref="RolDirector"/>. «Aveti N documente de fundamentare de semnat»,
''' the list across every unit of the director (<c>GET /api/forexe/ddf/director/de-semnat</c>), and
''' the selected document in the DDF view on the right -- its signing session (slice 0078) is the
''' one that signs and uploads, so «Ordonator» lands in <c>Semnatura</c> -> S4.
'''
''' <para><b>Several units, one session.</b> A session is bound to one unit database, and the
''' document routes read the session's. A document of ANOTHER unit is opened after a login on that
''' unit (the login window, with that unit pre-selected); the old session is logged out. No new
''' authentication route: the director proves the password again, as at any login.</para>
''' </summary>
Public Class DirectorForm

    ''' <summary>The <c>Unitati_Utilizatori.Rol</c> value of a director (operator, 25.09.2026).</summary>
    Public Const RolDirector As String = "Director"

    Private Const Titlu As String = "Documente de semnat"

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _session As SessionContext
    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _loginFactory As Func(Of LoginForm)

    Private _ddfView As DdfView
    Private _loading As Boolean

    Public Sub New(apiClient As IApiClient, session As SessionContext, authApi As IAuthApi,
                   loginFactory As Func(Of LoginForm))
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(session)
        ArgumentNullException.ThrowIfNull(authApi)
        ArgumentNullException.ThrowIfNull(loginFactory)
        InitializeComponent()
        _apiClient = apiClient
        _session = session
        _authApi = authApi
        _loginFactory = loginFactory
    End Sub

    ''' <summary>Is this login a director's? Read by <c>Program</c> to pick the window.</summary>
    Public Shared Function EsteDirector(session As SessionContext) As Boolean
        Return session IsNot Nothing AndAlso String.Equals(session.Role, RolDirector, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>Slice 0124-06: the roles that have a list of documents to sign inside the full K-BOT: the director, the accountant and
    ''' the FOREXE proxy. (A Secretar / Administrator has the narrowed window of their own, see <c>DeSemnatRestrans</c>.)</summary>
    Public Shared Function AreListaRol(k_rol As String) As Boolean
        Return String.Equals(k_rol, RolDirector, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(k_rol, "Contabil", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(k_rol, "Imputernicit", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Function AreListaDeSemnat(session As SessionContext) As Boolean
        Return session IsNot Nothing AndAlso AreListaRol(session.Role)
    End Function

    ''' <summary>
    ''' The list: the flow's list for every role (<c>GET /api/forexe/ddf/de-semnat</c>); a director also gets the older list (A and B
    ''' signed, no «Ordonator», <c>GET .../director/de-semnat</c>), the two joined without a revision twice.
    ''' </summary>
    Private Async Function CitesteListaAsync(k_api As IDdfSendApi) As Task(Of DdfDeSemnatLista)
        Dim k_flux As DdfDeSemnatLista = Await k_api.GetDdfDeSemnatAsync(CancellationToken.None).ConfigureAwait(True)
        If Not EsteDirector(_session) Then Return k_flux
        Dim k_vechi As DdfDeSemnatLista = Await k_api.GetDdfDeSemnatDirectorAsync(CancellationToken.None).ConfigureAwait(True)
        Return k_vechi.Uneste(k_flux)
    End Function

    Private Sub DirectorForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ArataUnitatea()
            IncarcaListaAsync()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("DirectorForm.DirectorForm_Load", ex)
        End Try
    End Sub

    ' ── The list ─────────────────────────────────────────────────────────────────

    ' UI boundary: logs and SHOWS the error; started without await.
    Private Async Sub IncarcaListaAsync()
        If _loading Then Return
        _loading = True
        Try
            SetBusy(True)
            Dim sendApi As IDdfSendApi = TryCast(_apiClient, IDdfSendApi)
            If sendApi Is Nothing Then Throw New InvalidOperationException("The API client does not implement IDdfSendApi.")
            Dim lista As DdfDeSemnatLista = Await WithReauth(Of DdfDeSemnatLista)(
                Function() CitesteListaAsync(sendApi)).ConfigureAwait(True)
            If IsDisposed Then Return
            Umple(lista)
        Catch ex As ApiException
            GlobalErrorLog.Write("DirectorForm.IncarcaListaAsync", ex)
            If IsDisposed Then Return
            lblTitlu.Text = ex.Message
        Catch ex As Exception
            GlobalErrorLog.Write("DirectorForm.IncarcaListaAsync", ex)
            If IsDisposed Then Return
            lblTitlu.Text = "Lista nu a putut fi încărcată. Detalii în jurnalul de erori."
        Finally
            _loading = False
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    Private Sub Umple(lista As DdfDeSemnatLista)
        ' Only the unit the director is logged in on: the server lists every unit he is a director of.
        Dim revizii As List(Of DdfDeSemnat) = If(lista?.Revizii, New List(Of DdfDeSemnat)()) _
            .Where(Function(k_rev) String.Equals(k_rev.DbName, _session.DbName, StringComparison.OrdinalIgnoreCase)).ToList()
        Dim ro As New CultureInfo("ro-RO")
        lstDocumente.BeginUpdate()
        Try
            lstDocumente.ClearRows()
            For Each d As DdfDeSemnat In revizii
                Dim k_row As KBotDataRow = lstDocumente.AddRow()
                k_row.Tag = d
                k_row("cod") = d.CodAngajament
                k_row("obiect") = d.ObiectDdf
                k_row("rev") = d.NumarRev.ToString(CultureInfo.InvariantCulture)
                k_row("data") = If(d.DataRev.HasValue, d.DataRev.Value.ToString("dd.MM.yyyy", ro), String.Empty)
                k_row("total") = d.Total.ToString("#,##0.00", ro)
            Next
        Finally
            lstDocumente.EndUpdate()
        End Try

        SelecteazaRevizia(0)

        lblTitlu.Text = If(revizii.Count = 0,
                           "Nu aveți documente de fundamentare de semnat.",
                           If(revizii.Count = 1, "Aveți 1 document de fundamentare de semnat.",
                              $"Aveți {revizii.Count} documente de fundamentare de semnat."))
        ' The unit that could not be read is named only when it is this one.
        If lista IsNot Nothing AndAlso lista.UnitatiNecitite.Any(Function(k_name) String.Equals(k_name, _session.NumeUnitate, StringComparison.OrdinalIgnoreCase)) Then
            lblTitlu.Text &= "  Lista unității nu a putut fi citită acum."
        End If
    End Sub

    ''' <summary>The revision to pick as soon as the list is read (the tray's click on a document); 0 = none.</summary>
    Friend Property IdrevInitial As Integer

    ''' <summary>
    ''' Picks the revision in the list, which opens its document. With 0 it picks <see cref="IdrevInitial"/> (once, when the
    ''' list has just been read). A revision that is not in the list (signed meanwhile, or the list is older than the tray's)
    ''' makes the list be read again first; if it is still not there, nothing is picked.
    ''' </summary>
    Friend Sub SelecteazaRevizia(k_idrev As Integer)
        Try
            Dim k_wanted As Integer = If(k_idrev > 0, k_idrev, IdrevInitial)
            If k_wanted <= 0 Then Return
            For k_index As Integer = 0 To lstDocumente.Rows.Count - 1
                Dim k_document As DdfDeSemnat = TryCast(lstDocumente.Rows(k_index).Tag, DdfDeSemnat)
                If k_document IsNot Nothing AndAlso k_document.Idrev = k_wanted Then
                    IdrevInitial = 0
                    lstDocumente.CurrentRowIndex = k_index   ' raises SelectionChanged, which opens the document
                    Return
                End If
            Next
            ' Not among the rows. After a fresh read (IdrevInitial was already tried once) it is simply gone.
            If k_idrev > 0 Then
                IdrevInitial = k_idrev
                IncarcaListaAsync()
            Else
                IdrevInitial = 0
            End If
        Catch ex As Exception
            ' UI boundary: log and swallow (the list stays as it is).
            GlobalErrorLog.Write("DirectorForm.SelecteazaRevizia", ex)
        End Try
    End Sub

    Private Sub LstDocumente_SelectionChanged(sender As Object, e As EventArgs) Handles lstDocumente.SelectionChanged
        Try
            Dim k_current As KBotDataRow = lstDocumente.CurrentRow
            If k_current Is Nothing Then Return
            Dim d As DdfDeSemnat = TryCast(k_current.Tag, DdfDeSemnat)
            If d Is Nothing Then Return
            Deschide(d)
        Catch ex As Exception
            ' UI boundary (event handler): log and show.
            GlobalErrorLog.Write("DirectorForm.LstDocumente_SelectionChanged", ex)
            KBotMessage.Show(Me, "Documentul nu a putut fi deschis. Detalii în jurnalul de erori.", Titlu,
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Opens the revision in the DDF view. The list holds only the unit the director is logged in on, so the
    ''' revision is always on the session's database.</summary>
    Private Sub Deschide(d As DdfDeSemnat)
        Dim view As DdfView = VedereaDdf()
        lblDocument.Visible = False
        view.Visible = True
        view.ArataRevizia(New AngajamentTreeInfo() With {
            .CodAngajament = d.CodAngajament, .AreDDF = True, .IDDF = CLng(d.Iddf)}, d.Idrev)
    End Sub

    ''' <summary>The DDF view, created once (its constructor takes the API client, so it cannot be
    ''' declared in the Designer -- same as the shell's views). The write commands are not offered
    ''' to a director.</summary>
    Private Function VedereaDdf() As DdfView
        If _ddfView IsNot Nothing Then Return _ddfView
        _ddfView = New DdfView(_apiClient, Function(op) WithReauth(Of DdfInfo)(op), _session, AddressOf ComandaRefuzata)
        _ddfView.ArboreAscuns = True   ' the revision is the one picked in the list: straight to «Vizualizare» / the document
        _ddfView.Dock = DockStyle.Fill
        pnlDocument.Controls.Add(_ddfView)
        _ddfView.BringToFront()
        ThemeManager.Apply(_ddfView)
        Return _ddfView
    End Function

    Private Sub ComandaRefuzata(comanda As DdfComanda)
        KBotMessage.Show(Me, If(EsteDirector(_session), DirectorMode.ReadOnlyMessage,
                                "Din această listă documentele se deschid și se semnează. Celelalte comenzi se dau din fereastra principală."),
                        Titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' The 401 net: a login dialog on the SAME unit, then one retry. A second failure propagates.
    ''' </summary>
    Private Async Function WithReauth(Of T)(action As Func(Of Task(Of T))) As Task(Of T)
        Dim expired As ApiException
        Try
            Return Await action().ConfigureAwait(True)
        Catch ex As ApiException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 401
            expired = ex
        End Try
        Using login As LoginForm = _loginFactory()
            login.PreferredDc = _session.DbName
            If login.ShowDialog(Me) <> DialogResult.OK Then Throw expired
        End Using
        ArataUnitatea()
        Return Await action().ConfigureAwait(True)
    End Function

    ' ── Footer / chrome ──────────────────────────────────────────────────────────

    Private Sub BtnReincarca_Click(sender As Object, e As EventArgs) Handles btnReincarca.Click
        IncarcaListaAsync()
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    Private Sub ArataUnitatea()
        lblStare.Text = $"{_session.OperatorName} — {_session.NumeUnitate}"
    End Sub

    Private Sub SetBusy(busy As Boolean)
        btnReincarca.Enabled = Not busy
        UseWaitCursor = busy
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            Dim p As ThemePalette = scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(1)).
            BackColor = p.BorderColor
            tlyMain.BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor
            pnlDocument.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            lblTitlu.BackColor = p.SurfaceAltColor
            lblDocument.BackColor = p.SurfaceAltColor
            lblDocument.ForeColor = p.TextDimColor
            lblStare.ForeColor = p.TextDimColor
            ButtonStyles.ApplyPrimary(btnReincarca, scheme)
            ButtonStyles.ApplySecondary(btnInchide, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DirectorForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
