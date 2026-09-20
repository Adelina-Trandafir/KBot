Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The standalone log window (slice 0031-04). Since slice 0072-01 the viewer itself is
''' <see cref="SetariJurnalView"/>, the «Jurnal» page of the settings window; this form is
''' the same page with a caption bar around it, kept for the two callers that need a window
''' and not a page: the harness («Jurnale» button, <c>ILogViewerLauncher</c>) and the startup
''' launcher's «Jurnale» choice, which runs without a shell and without a login.
'''
''' <para>The shell no longer opens this form: its «Arată jurnal» row opens the settings
''' window on the «Jurnal» page (<c>KbotForm.ShowLog</c>), so there is one log surface, not
''' two that could drift apart.</para>
'''
''' <para>The test hooks forward to the page one for one, so <c>LogViewerFormTests</c> keeps
''' exercising the real path through a real window.</para>
''' </summary>
Public Class LogViewerForm

    ''' <summary>Viewer without a server -- the harness and any host without an API.</summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Viewer with server logs. <paramref name="api"/> Nothing = the «Server» group does not
    ''' appear at all (see <see cref="SetariJurnalView.ApiClient"/>).
    ''' </summary>
    Public Sub New(api As IApiClient)
        InitializeComponent()
        jurnal.ApiClient = api
    End Sub

    ' The page loads on activation, exactly as it does inside the settings window.
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            jurnal.Activated()
        Catch ex As Exception
            ' UI boundary (Load): a throw would take the window's opening down.
            GlobalErrorLog.Write("LogViewerForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            ' The form background IS the 1px outline of the window (Padding(1, 2, 1, 2)).
            BackColor = ThemeManager.Current.Palette.BorderColor
        Catch ex As Exception
            GlobalErrorLog.Write("LogViewerForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' =====================================================================
    ' FRIEND HOOKS FOR TESTS -- forwarded to the page (see LogViewerFormTests)
    ' =====================================================================

    Friend Sub DebugIncarcaIntrari(entries As IEnumerable(Of LogEntry))
        jurnal.DebugIncarcaIntrari(entries)
    End Sub

    Friend Sub DebugFormateazaRand(e As KBotRowFormattingEventArgs)
        jurnal.DebugFormateazaRand(e)
    End Sub

    Friend Function DebugTextDetaliu() As String
        Return jurnal.DebugTextDetaliu()
    End Function

    Friend Sub DebugSelecteazaRand(index As Integer)
        jurnal.DebugSelecteazaRand(index)
    End Sub

    Friend Function DebugNumarRanduri() As Integer
        Return jurnal.DebugNumarRanduri()
    End Function

    Friend Function DebugIntrareaRandului(index As Integer) As LogEntry
        Return jurnal.DebugIntrareaRandului(index)
    End Function

    Friend Function DebugAduListaServerAsync() As Task
        Return jurnal.DebugAduListaServerAsync()
    End Function

    Friend Function DebugNoticeServerAfisat() As Boolean
        Return jurnal.DebugNoticeServerAfisat()
    End Function

End Class
