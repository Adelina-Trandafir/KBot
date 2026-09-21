Option Strict On
Imports KBot.Common
Imports KBot.Theming
Imports Newtonsoft.Json.Linq

''' <summary>
''' What the in-page script (<c>ForexeWatch.js</c>) is told about the operator's choices:
''' whether the developer tools stay reachable and which CSS rules to write into the page.
''' Built from <see cref="AppSettings"/>, sent as one JSON text through
''' <c>WorkflowExecutor.ApplyWatchConfigAsync</c>; the page keeps it in its own storage so
''' every later load starts with it before the first paint.
''' </summary>
Public NotInheritable Class ForexeWatchConfig

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The JSON the page expects: <c>{devTools, darkMode, rules:[{enabled, selector, css, note, page}]}</c>.
    ''' <c>darkMode</c> is not a setting of its own: it follows K-BOT's theme (a dark scheme
    ''' darkens the page too - operator, 21.09.2026).
    ''' </summary>
    Public Shared Function JsonFromSettings(settings As AppSettings) As String
        Try
            Dim s As AppSettings = If(settings, AppSettings.Current)
            Dim rules As New JArray()
            If s.ForexePageStyles IsNot Nothing Then
                For Each r As PageStyleRule In s.ForexePageStyles
                    If r Is Nothing OrElse String.IsNullOrWhiteSpace(r.Selector) Then Continue For
                    rules.Add(New JObject(
                        New JProperty("enabled", r.Enabled),
                        New JProperty("selector", r.Selector.Trim()),
                        New JProperty("css", If(r.Css, String.Empty).Trim()),
                        New JProperty("note", If(r.Note, String.Empty)),
                        New JProperty("page", If(r.Page, String.Empty).Trim())))
                Next
            End If
            Dim root As New JObject(
                New JProperty("devTools", s.ForexeDevToolsAllowed),
                New JProperty("darkMode", DarkModeNow()),
                New JProperty("rules", rules))
            Return root.ToString(Newtonsoft.Json.Formatting.None)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeWatchConfig.JsonFromSettings", ex)
            Throw
        End Try
    End Function

    ''' <summary>The current settings, as the page wants them.</summary>
    Public Shared Function JsonNow() As String
        Return JsonFromSettings(AppSettings.Current)
    End Function

    ''' <summary>True while K-BOT runs a dark scheme; False when no theme is loaded (tests, harness).</summary>
    Private Shared Function DarkModeNow() As Boolean
        Try
            Dim scheme As ThemeScheme = ThemeManager.Current
            Return scheme IsNot Nothing AndAlso scheme.IsDark
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeWatchConfig.DarkModeNow", ex)
            Return False
        End Try
    End Function

End Class
