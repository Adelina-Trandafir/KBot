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
                Function() sendApi.GetDdfDeSemnatDirectorAsync(CancellationToken.None)).ConfigureAwait(True)
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
        Dim revizii As List(Of DdfDeSemnat) = If(lista?.Revizii, New List(Of DdfDeSemnat)())
        Dim ro As New CultureInfo("ro-RO")
        lstDocumente.BeginUpdate()
        Try
            lstDocumente.Items.Clear()
            For Each d As DdfDeSemnat In revizii
                Dim item As New ListViewItem(If(String.IsNullOrWhiteSpace(d.NumeUnitate), d.DbName, d.NumeUnitate))
                item.SubItems.Add(d.CodAngajament)
                item.SubItems.Add(d.ObiectDdf)
                item.SubItems.Add(d.NumarRev.ToString(CultureInfo.InvariantCulture))
                item.SubItems.Add(If(d.DataRev.HasValue, d.DataRev.Value.ToString("dd.MM.yyyy", ro), String.Empty))
                item.SubItems.Add(d.Total.ToString("#,##0.00", ro))
                item.Tag = d
                item.ToolTipText = d.DescScurta
                lstDocumente.Items.Add(item)
            Next
        Finally
            lstDocumente.EndUpdate()
        End Try

        lblTitlu.Text = If(revizii.Count = 0,
                           "Nu aveți documente de fundamentare de semnat.",
                           If(revizii.Count = 1, "Aveți 1 document de fundamentare de semnat.",
                              $"Aveți {revizii.Count} documente de fundamentare de semnat."))
        If lista IsNot Nothing AndAlso lista.UnitatiNecitite.Count > 0 Then
            lblTitlu.Text &= "  Nu s-au putut citi: " & String.Join(", ", lista.UnitatiNecitite) & "."
        End If
    End Sub

    Private Async Sub LstDocumente_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDocumente.SelectedIndexChanged
        Try
            If lstDocumente.SelectedItems.Count = 0 Then Return
            Dim d As DdfDeSemnat = TryCast(lstDocumente.SelectedItems(0).Tag, DdfDeSemnat)
            If d Is Nothing Then Return
            Await DeschideAsync(d).ConfigureAwait(True)
        Catch ex As Exception
            ' UI boundary (async Sub).
            GlobalErrorLog.Write("DirectorForm.LstDocumente_SelectedIndexChanged", ex)
            KBotMessage.Show(Me, "Documentul nu a putut fi deschis. Detalii în jurnalul de erori.", Titlu,
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Opens the revision in the DDF view -- after a login on its unit when it lives in
    ''' another one than the session's.</summary>
    Private Async Function DeschideAsync(d As DdfDeSemnat) As Task
        If Not String.Equals(d.DbName, _session.DbName, StringComparison.OrdinalIgnoreCase) Then
            Dim unitate As String = If(String.IsNullOrWhiteSpace(d.NumeUnitate), d.DbName, d.NumeUnitate)
            If KBotMessage.Show(Me, $"Documentul este în unitatea «{unitate}», iar sesiunea de acum este pe «{_session.NumeUnitate}»." &
                                vbCrLf & vbCrLf & "Pentru a-l deschide și semna vă autentificați pe acea unitate. Continuați?",
                                Titlu, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If
            If Not Await SchimbaUnitateaAsync(d.DbName).ConfigureAwait(True) Then Return
        End If

        Dim view As DdfView = VedereaDdf()
        lblDocument.Visible = False
        view.Visible = True
        view.SetContext(New AngajamentTreeInfo() With {
            .CodAngajament = d.CodAngajament, .AreDDF = True, .IDDF = CLng(d.Iddf)})
        view.Reincarca(d.Idrev)
    End Function

    ''' <summary>The DDF view, created once (its constructor takes the API client, so it cannot be
    ''' declared in the Designer -- same as the shell's views). The write commands are not offered
    ''' to a director.</summary>
    Private Function VedereaDdf() As DdfView
        If _ddfView IsNot Nothing Then Return _ddfView
        _ddfView = New DdfView(_apiClient, Function(op) WithReauth(Of DdfInfo)(op), _session, AddressOf ComandaRefuzata)
        _ddfView.Dock = DockStyle.Fill
        pnlDocument.Controls.Add(_ddfView)
        _ddfView.BringToFront()
        ThemeManager.Apply(_ddfView)
        Return _ddfView
    End Function

    Private Sub ComandaRefuzata(comanda As DdfComanda)
        KBotMessage.Show(Me, "Aici documentul se semnează doar. Modificările se fac din K-BOT-ul unității.",
                        Titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' A login on unit <paramref name="dc"/>. True when the session is on it now. The previous
    ''' session is logged out best-effort only after the new one exists.
    ''' </summary>
    Private Async Function SchimbaUnitateaAsync(dc As String) As Task(Of Boolean)
        Dim tokenVechi As String = _session.Token
        Using login As LoginForm = _loginFactory()
            login.PreferredDc = dc
            If login.ShowDialog(Me) <> DialogResult.OK Then Return False
        End Using
        If Not String.Equals(tokenVechi, _session.Token, StringComparison.Ordinal) AndAlso
           Not String.IsNullOrEmpty(tokenVechi) Then
            Try
                Await _authApi.LogoutAsync(tokenVechi, CancellationToken.None).ConfigureAwait(True)
            Catch ex As Exception
                ' Best effort: an old token left alive expires by itself.
                GlobalErrorLog.Write("DirectorForm.SchimbaUnitateaAsync", ex)
            End Try
        End If
        ArataUnitatea()
        If Not EsteDirector(_session) Then
            KBotMessage.Show(Me, "Pe unitatea aleasă nu aveți rolul «Director».", Titlu,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If Not String.Equals(_session.DbName, dc, StringComparison.OrdinalIgnoreCase) Then
            KBotMessage.Show(Me, "V-ați autentificat pe altă unitate decât cea a documentului.", Titlu,
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return True
    End Function

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
