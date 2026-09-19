Option Strict On
Imports KBot.Common

''' <summary>
''' The two host switches the operator sets in «Setări» (slice 0072), read from
''' <see cref="AppSettings"/>: how the hosted Adobe window is let go, and whether the floating
''' popup is hunted. Text in the store, enum here -- the same split as
''' <see cref="AdobeViewerSettings"/>, for the same reason (the enum lives in KBot.Controls).
'''
''' Fallback rule, as everywhere in this folder: an unrecognised value falls back to the
''' default that shipped (A, kill the process) with a warning for the working log -- NEVER an
''' exception, a broken setting must not stop a document from opening.
''' </summary>
Public NotInheritable Class AdobeHostSettings

    Private Sub New()
    End Sub

    ''' <summary>Stored text -&gt; detach mode. Unknown -&gt; KillProcess + warning.</summary>
    Public Shared Function ParseDetachMode(stored As String) As AdobeSettingRead(Of AdobeDetachMode)
        If String.IsNullOrWhiteSpace(stored) Then
            Return New AdobeSettingRead(Of AdobeDetachMode)(AdobeDetachMode.KillProcess, "")
        End If
        Select Case stored.Trim().ToLowerInvariant()
            Case "killprocess", "a"
                Return New AdobeSettingRead(Of AdobeDetachMode)(AdobeDetachMode.KillProcess, "")
            Case "closewindow", "b"
                Return New AdobeSettingRead(Of AdobeDetachMode)(AdobeDetachMode.CloseWindow, "")
            Case Else
                Return New AdobeSettingRead(Of AdobeDetachMode)(
                    AdobeDetachMode.KillProcess,
                    $"Setarea «AdobeDetachMode» are valoarea nerecunoscută «{stored}» — " &
                    "se folosește «KillProcess». Valori acceptate: KillProcess, CloseWindow.")
        End Select
    End Function

    ''' <summary>The value to write for a detach mode.</summary>
    Public Shared Function DetachModeToText(mode As AdobeDetachMode) As String
        If mode = AdobeDetachMode.CloseWindow Then Return AppSettings.DetachCloseWindow
        Return AppSettings.DetachKillProcess
    End Function

    ''' <summary>The Romanian combo label for a detach mode.</summary>
    Public Shared Function DetachModeLabel(mode As AdobeDetachMode) As String
        If mode = AdobeDetachMode.CloseWindow Then Return "Închide doar fereastra (procesul rămâne cald)"
        Return "Oprește procesul Adobe pornit de K-BOT"
    End Function

    ''' <summary>The detach mode in force now.</summary>
    Public Shared Function CurrentDetachMode() As AdobeSettingRead(Of AdobeDetachMode)
        Return ParseDetachMode(AppSettings.Current.AdobeDetachMode)
    End Function

    ''' <summary>Whether the floating popup is hunted while a document is hosted.</summary>
    Public Shared Function CurrentPopupWatch() As Boolean
        Return AppSettings.Current.AdobePopupWatch
    End Function

    ''' <summary>
    ''' Applies both switches to a host, logging a broken value. Called where the host is built,
    ''' so every preview in the application reads the same two lines.
    ''' </summary>
    Public Shared Sub ApplyTo(host As AdobeReaderHost, log As Action(Of String))
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        Dim detach As AdobeSettingRead(Of AdobeDetachMode) = CurrentDetachMode()
        If detach.HasWarning AndAlso log IsNot Nothing Then log("ATENȚIE: " & detach.Warning)
        host.Options.DetachMode = detach.Value
        host.PopupWatchEnabled = CurrentPopupWatch()
    End Sub

End Class
