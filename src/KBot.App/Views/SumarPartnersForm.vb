Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The shell's 401 net for the two response shapes the partners window needs -- the partner
''' picker's list and the associated partners. A parameter object like <c>DdfEditReauth</c>:
''' <c>MainForm.WithReauth</c> is private and generic, so the form is handed one closure per
''' response type and the re-login policy stays in one place.
''' </summary>
Public NotInheritable Class SumarPartnersReauth

    Public ReadOnly Property Candidates As Func(Of Func(Of Task(Of List(Of DdfPartener))), Task(Of List(Of DdfPartener)))
    Public ReadOnly Property Associated As Func(Of Func(Of Task(Of DdfParteneriAsociati)), Task(Of DdfParteneriAsociati))

    Public Sub New(candidates As Func(Of Func(Of Task(Of List(Of DdfPartener))), Task(Of List(Of DdfPartener))),
                   associated As Func(Of Func(Of Task(Of DdfParteneriAsociati)), Task(Of DdfParteneriAsociati)))
        ArgumentNullException.ThrowIfNull(candidates)
        ArgumentNullException.ThrowIfNull(associated)
        _Candidates = candidates
        _Associated = associated
    End Sub
End Class

''' <summary>
''' «Asociaza parteneri» (slice 0084-02) -- the window the Sumar button opens: the partners
''' associated with the angajament's DDF, and a picker for more.
'''
''' <para><b>Add only.</b> The partners the DDF already has are shown and stay; the operator can
''' add others, and take back one they added in this window before saving. Taking a partner off a
''' DDF is the DDF editor's job (its «Parteneri» page), because the editor is where the rest of
''' the document is visible.</para>
'''
''' <para><b>What the save does.</b> The partners marked «De adaugat» go up in one request; the
''' server adds those the DDF does not have yet and never writes the same fiscal code twice. The
''' DDF's header (its main partner, <c>FX_DDF.CodFiscal</c>) is not touched from here.</para>
'''
''' <para>The list and the picker are the shared <see cref="DdfPartnersView"/>.</para>
''' </summary>
Public Class SumarPartnersForm

    Private ReadOnly _api As IApiClient
    Private ReadOnly _partnersApi As IDdfParteneriApi
    Private ReadOnly _cod As String
    Private ReadOnly _reauth As SumarPartnersReauth

    ' The partners the DDF has (as the server said) and the ones picked in this window.
    Private ReadOnly _existing As New List(Of DdfPartenerAsociat)()
    Private ReadOnly _pending As New List(Of DdfPartenerAsociat)()

    ''' <summary>Did a save add partners? The host has nothing to refresh today, but a window that
    ''' changed data says so.</summary>
    Public ReadOnly Property Saved As Boolean

    Public Sub New(api As IApiClient, cod As String, reauth As SumarPartnersReauth)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(reauth)
        If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("The angajament code is missing.", NameOf(cod))
        _partnersApi = TryCast(api, IDdfParteneriApi)
        If _partnersApi Is Nothing Then
            Throw New InvalidOperationException("The api client does not implement IDdfParteneriApi.")
        End If

        InitializeComponent()
        _api = api
        _cod = cod
        _reauth = reauth

        capBar.Text = $"K-BOT — Asociază parteneri · {cod}"
        Text = capBar.Text
        vwPartners.RoleOf = Function(p) If(_pending.Contains(p), "De adăugat", If(p.DinAntet, "Principal", "Asociat"))
        vwPartners.CanRemove = Function(p) _pending.Contains(p)
    End Sub

    ' Boundary UI async (Shown): logged and said; a throw would land on the UI thread.
    Private Async Sub SumarPartnersForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Try
            busy.Running = True
            Try
                Dim associated As DdfParteneriAsociati = Await _reauth.Associated(
                    Function() _partnersApi.GetDdfParteneriAsociatiAsync(_cod, CancellationToken.None)).ConfigureAwait(True)
                Dim candidates As List(Of DdfPartener) = Await _reauth.Candidates(
                    Function() _api.GetDdfParteneriAsync(_cod, CancellationToken.None)).ConfigureAwait(True)

                _existing.Clear()
                _existing.AddRange(associated.Parteneri)
                vwPartners.SetCandidates(candidates)
                ShowList()
            Finally
                busy.Running = False
            End Try
        Catch ex As ApiException
            GlobalErrorLog.Write("SumarPartnersForm.Shown", ex)
            KBotMessage.Show(Me, ex.Message, "Asociază parteneri", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            DialogResult = DialogResult.Cancel
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("SumarPartnersForm.Shown", ex)
            KBotMessage.Show(Me, "Partenerii nu au putut fi încărcați. Detalii în jurnalul de erori.",
                             "Asociază parteneri", MessageBoxButtons.OK, MessageBoxIcon.Error)
            DialogResult = DialogResult.Cancel
            Close()
        End Try
    End Sub

    Private Sub ShowList()
        Dim all As New List(Of DdfPartenerAsociat)(_existing)
        all.AddRange(_pending)
        vwPartners.SetAssociated(all)
        btnSave.Enabled = _pending.Count > 0
    End Sub

    Private Sub VwPartners_AddRequested(sender As Object, e As DdfPartnerEventArgs) Handles vwPartners.AddRequested
        Try
            Dim key As String = DdfPartenerAsociat.Cheie(e.Partner.CodFiscal)
            If _existing.Concat(_pending).Any(Function(p) String.Equals(DdfPartenerAsociat.Cheie(p.CodFiscal), key, StringComparison.Ordinal)) Then
                KBotMessage.Show(Me, "Partenerul este deja asociat.", "Asociază parteneri",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            _pending.Add(e.Partner)
            ShowList()
        Catch ex As Exception
            GlobalErrorLog.Write("SumarPartnersForm.VwPartners_AddRequested", ex)
        End Try
    End Sub

    Private Sub VwPartners_RemoveRequested(sender As Object, e As DdfPartnerEventArgs) Handles vwPartners.RemoveRequested
        Try
            If _pending.Remove(e.Partner) Then ShowList()
        Catch ex As Exception
            GlobalErrorLog.Write("SumarPartnersForm.VwPartners_RemoveRequested", ex)
        End Try
    End Sub

    ' Boundary UI async (button): logged and said.
    Private Async Sub BtnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If _pending.Count = 0 Then Return
            btnSave.Enabled = False
            btnClose.Enabled = False
            busy.Running = True
            Try
                Await _reauth.Associated(
                    Function() _partnersApi.AdaugaDdfParteneriAsync(_cod, _pending.ToList(), CancellationToken.None)).ConfigureAwait(True)
                _Saved = True
                DialogResult = DialogResult.OK
                Close()
            Finally
                busy.Running = False
                btnClose.Enabled = True
                btnSave.Enabled = _pending.Count > 0
            End Try
        Catch ex As ApiException
            ' The server's message is already in Romanian (including «run sql/0084_02...»).
            GlobalErrorLog.Write("SumarPartnersForm.BtnSave_Click", ex)
            KBotMessage.Show(Me, ex.Message, "Asociază parteneri", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("SumarPartnersForm.BtnSave_Click", ex)
            KBotMessage.Show(Me, "Partenerii nu au putut fi asociați. Detalii în jurnalul de erori.",
                             "Asociază parteneri", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Closing with partners picked and not sent asks first: they would be lost.</summary>
    Private Sub SumarPartnersForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If DialogResult = DialogResult.OK OrElse _pending.Count = 0 Then Return
            If KBotMessage.Show(Me, "Ai ales parteneri care nu au fost salvați. Închizi fereastra fără să-i asociezi?",
                                "Asociază parteneri", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                e.Cancel = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SumarPartnersForm.FormClosing", ex)
        End Try
    End Sub
End Class
