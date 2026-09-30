Option Strict On
Imports System.Windows.Forms

''' <summary>
''' The application's help system, as seen from the controls (slice 0000-01). Implemented in
''' KBot.App (<c>HelpService</c>), which knows the topics, the login role and the help window.
''' </summary>
Public Interface IKBotHelpProvider

    ''' <summary>
    ''' Opens the help for <paramref name="origin"/>: the focused control on F1, the caption bar
    ''' on its «?» button. The provider walks up from it to the nearest control that has a topic.
    ''' </summary>
    Sub ShowHelp(origin As Control)

End Interface

''' <summary>
''' The one place a control asks for help (slice 0000-01). Lives in Theming, next to
''' <see cref="ThemeManager"/>, because the caption bar (KBot.Controls) cannot reference KBot.App,
''' where the topics and the help window are. The application sets <see cref="Provider"/> once at
''' startup; until then (DevHarness, designer) there is no help and the «?» button stays hidden.
''' </summary>
Public NotInheritable Class KBotHelp

    Private Sub New()
    End Sub

    ''' <summary>The application's help. Nothing = no help installed.</summary>
    Public Shared Property Provider As IKBotHelpProvider

    ''' <summary>True once the application has installed its help.</summary>
    Public Shared ReadOnly Property IsAvailable As Boolean
        Get
            Return Provider IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Opens the help for <paramref name="origin"/>. Callers check <see cref="IsAvailable"/>
    ''' first (the «?» button is not even drawn without it), so a call with no provider is a bug.
    ''' </summary>
    Public Shared Sub Request(origin As Control)
        Dim p As IKBotHelpProvider = Provider
        If p Is Nothing Then Throw New InvalidOperationException("KBotHelp.Request called with no help provider installed.")
        p.ShowHelp(origin)
    End Sub

End Class
