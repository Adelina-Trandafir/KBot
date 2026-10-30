Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0124-06 -- the K-BOT of a «Secretar» or an «Administrator» (operator, 30.10.2026, CRITICAL: they see ONLY the documents addressed
''' to their role and NOTHING else). <c>Program</c> opens THIS window instead of the main one for those roles. It shows the list of
''' documents addressed to the role (<c>GET /api/forexe/ddf/de-semnat</c>) and, for the selected one, generates the document ON THIS PC with section A
''' only (as the accountant does), shows it in Adobe with a signing session that accepts section A only; the upload is the normal PDF
''' route, and the server moves the document on to the next signer. Nothing is stored on the server before the signature.
'''
''' <para><b>The server is the guarantee, not this window.</b> A restricted session reads only the DDF revisions addressed to its role
''' (<c>ddf_flux_core.refuza_restrans</c>, <c>filtreaza_ddf</c>); everything else works for it as it always did. This window just has nothing more to show.</para>
''' </summary>
Public Class DeSemnatRestransForm

    Private Const Titlu As String = "Documente de semnat"

    ''' <summary>The only signature a restricted role may add (section A). The server refuses anything else too.</summary>
    Private Shared ReadOnly RoluriPermise As String() = {"A"}

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _session As SessionContext
    Private ReadOnly _loginFactory As Func(Of LoginForm)

    Private _loading As Boolean
    Private _signing As PdfSigningSession
    ' The local files of the document on screen (the one Adobe shows and what the generation left); deleted when it is done with.
    Private ReadOnly _files As New List(Of String)()

    Public Sub New(apiClient As IApiClient, session As SessionContext, loginFactory As Func(Of LoginForm))
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(session)
        ArgumentNullException.ThrowIfNull(loginFactory)
        InitializeComponent()
        _apiClient = apiClient
        _session = session
        _loginFactory = loginFactory
    End Sub

    ''' <summary>Is this login a Secretar's or an Administrator's? Read by <c>Program</c> to pick the window.</summary>
    Public Shared Function EsteRolRestrans(session As SessionContext) As Boolean
        Return session IsNot Nothing AndAlso
               (String.Equals(session.Role, "Secretar", StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(session.Role, "Administrator", StringComparison.OrdinalIgnoreCase))
    End Function

    Private Sub DeSemnatRestransForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ArataUnitatea()
            IncarcaListaAsync()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("DeSemnatRestransForm.DeSemnatRestransForm_Load", ex)
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
                Function() sendApi.GetDdfDeSemnatAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            Umple(lista)
        Catch ex As ApiException
            GlobalErrorLog.Write("DeSemnatRestransForm.IncarcaListaAsync", ex)
            If IsDisposed Then Return
            lblTitlu.Text = ex.Message
        Catch ex As Exception
            GlobalErrorLog.Write("DeSemnatRestransForm.IncarcaListaAsync", ex)
            If IsDisposed Then Return
            lblTitlu.Text = "Lista nu a putut fi încărcată. Detalii în jurnalul de erori."
        Finally
            _loading = False
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    Private Sub Umple(lista As DdfDeSemnatLista)
        ' Only the unit signed in on: a document of another unit is reached by signing in on that unit.
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

        lblTitlu.Text = If(revizii.Count = 0,
                           "Nu aveți documente de semnat.",
                           If(revizii.Count = 1, "Aveți 1 document de semnat.", $"Aveți {revizii.Count} documente de semnat."))
        If lista IsNot Nothing AndAlso lista.UnitatiNecitite.Any(Function(k_name) String.Equals(k_name, _session.NumeUnitate, StringComparison.OrdinalIgnoreCase)) Then
            lblTitlu.Text &= "  Lista unității nu a putut fi citită acum."
        End If
        If revizii.Count = 0 Then
            ReleaseDocument()
            lblDocument.Text = "Nu e nimic de semnat."
            lblDocument.Visible = True
        End If
    End Sub

    Private Sub LstDocumente_SelectionChanged(sender As Object, e As EventArgs) Handles lstDocumente.SelectionChanged
        Try
            Dim k_current As KBotDataRow = lstDocumente.CurrentRow
            If k_current Is Nothing Then Return
            Dim d As DdfDeSemnat = TryCast(k_current.Tag, DdfDeSemnat)
            If d Is Nothing Then Return
            DeschideAsync(d)
        Catch ex As Exception
            ' UI boundary (event handler): log and show.
            GlobalErrorLog.Write("DeSemnatRestransForm.LstDocumente_SelectionChanged", ex)
            KBotMessage.Show(Me, "Documentul nu a putut fi deschis. Detalii în jurnalul de erori.", Titlu,
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── The document ─────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Generates the document of the revision on this PC with section A only (the revision, read through the DDF read that gives a restricted role
    ''' only the revisions addressed to it) into a NEW file of the work area (Acrobat serves a reused name from memory) and shows it with a
    ''' signing session. UI boundary (async Sub): logs, tells the operator, never throws.
    ''' </summary>
    Private Async Sub DeschideAsync(d As DdfDeSemnat)
        Try
            ReleaseDocument()
            lblDocument.Text = "Se pregătește documentul…"
            lblDocument.Visible = True
            Dim k_generat As DdfPdfGenerator.Rezultat = Await DdfPdfGenerator.GenereazaAsync(
                Function() WithReauth(Of DdfInfo)(
                    Function() _apiClient.GetDdfAsync(d.CodAngajament, CancellationToken.None, pentruGenerare:=True)),
                Nothing, _session, d.Idrev, DdfPdfMode.OnlyA).ConfigureAwait(True)
            If IsDisposed Then Return
            If k_generat Is Nothing Then
                lblDocument.Text = "Documentul nu a putut fi pregătit."   ' no template (not signed in): the operator was told
                Return
            End If
            _files.Add(k_generat.PdfPath)
            _files.Add(Path.ChangeExtension(k_generat.PdfPath, ".xml"))
            ' The row picked meanwhile is another one: this answer is dropped.
            Dim k_now As DdfDeSemnat = TryCast(lstDocumente.CurrentRow?.Tag, DdfDeSemnat)
            If k_now Is Nothing OrElse k_now.Idrev <> d.Idrev Then
                DeleteLocalAsync(TakeFiles())
                Return
            End If

            Dim k_path As String = TempPdfStore.PathFor($"DESEMNAT_{d.Idrev}_{DateTime.Now:HHmmss}.pdf")
            File.Copy(k_generat.PdfPath, k_path, overwrite:=True)
            _files.Add(k_path)

            ' The server holds a signed copy only if A was signed before and something went wrong afterwards; d.PdfSha256 is then its sha.
            _signing = New PdfSigningSession(PdfDocKind.Ddf, d.Idrev, k_path, k_path, d.PdfSha256, _apiClient)
            _signing.AllowedNewRoles = RoluriPermise
            AddHandler _signing.Completed, AddressOf OnSigningCompleted
            _signing.Begin()
            preview.Signing = _signing
            lblDocument.Visible = False
            preview.Visible = True
            preview.ShowDocument(k_path, exists:=True)
        Catch ex As ApiException
            GlobalErrorLog.Write("DeSemnatRestransForm.DeschideAsync", ex)
            If Not IsDisposed Then lblDocument.Text = ex.Message
        Catch ex As Exception
            GlobalErrorLog.Write("DeSemnatRestransForm.DeschideAsync", ex)
            If Not IsDisposed Then lblDocument.Text = "Documentul nu a putut fi deschis. Detalii în jurnalul de erori."
        End Try
    End Sub

    ''' <summary>The tracked local files, handed over for deletion; the list starts again empty.</summary>
    Private Function TakeFiles() As List(Of String)
        Dim k_taken As New List(Of String)(_files)
        _files.Clear()
        Return k_taken
    End Function

    ' UI thread (the session posts back to it). UI boundary: log and swallow.
    Private Sub OnSigningCompleted(k_session As PdfSigningSession, k_outcome As PdfSigningOutcome)
        Try
            If k_outcome Is Nothing Then Return
            SigningMessages.ShowOutcome(Me, k_outcome)
            If k_outcome.Status = PdfSigningStatus.Uploaded Then
                ' Signed and kept on the server, which moved the document on: it leaves this person's list.
                ReleaseDocument()
                lblDocument.Text = "Documentul a fost semnat și trimis mai departe."
                lblDocument.Visible = True
                DeleteLocalAsync(TakeFiles())
                IncarcaListaAsync()
            ElseIf k_outcome.Status = PdfSigningStatus.Conflict OrElse k_outcome.Status = PdfSigningStatus.Refused Then
                ' Somebody else changed it, or the signature was in a field this role may not use: back to what the server holds.
                ReleaseDocument()
                DeleteLocalAsync(TakeFiles())
                IncarcaListaAsync()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DeSemnatRestransForm.OnSigningCompleted", ex)
        End Try
    End Sub

    ''' <summary>Adobe lets the file go and the signing session ends; the viewer is emptied.</summary>
    Private Sub ReleaseDocument()
        preview.Signing = Nothing
        If _signing IsNot Nothing Then
            RemoveHandler _signing.Completed, AddressOf OnSigningCompleted
            _signing.Dispose()
            _signing = Nothing
        End If
        preview.Clear()
        preview.Visible = False
    End Sub

    ' The local copy of a document that was signed or refused is deleted (nothing stays on this PC). Adobe may hold it for a moment, so
    ' it is retried; what stays is left to the work area's own cleanup. UI boundary (async Sub): log and swallow.
    Private Async Sub DeleteLocalAsync(k_paths As List(Of String))
        Try
            Dim k_left As List(Of String) = k_paths.Where(Function(k_p) File.Exists(k_p)).ToList()
            For k_try As Integer = 1 To 10
                Dim k_still As New List(Of String)()
                For Each k_file As String In k_left
                    Try
                        File.Delete(k_file)
                    Catch ex As IOException
                        k_still.Add(k_file)   ' still held: next try
                    Catch ex As UnauthorizedAccessException
                        k_still.Add(k_file)
                    End Try
                Next
                k_left = k_still
                If k_left.Count = 0 Then Return
                Await Task.Delay(500).ConfigureAwait(True)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("DeSemnatRestransForm.DeleteLocalAsync", ex)
        End Try
    End Sub

    ''' <summary>The 401 net: a login dialog on the SAME unit, then one retry. A second failure propagates.</summary>
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

    Private Sub DeSemnatRestransForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            ReleaseDocument()
            DeleteLocalAsync(TakeFiles())
        Catch ex As Exception
            GlobalErrorLog.Write("DeSemnatRestransForm.DeSemnatRestransForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub ArataUnitatea()
        lblStare.Text = $"{_session.OperatorName} — {_session.NumeUnitate} ({_session.Role})"
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
            GlobalErrorLog.Write("DeSemnatRestransForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
