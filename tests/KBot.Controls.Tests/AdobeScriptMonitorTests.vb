Option Strict On
Imports KBot.Controls
Imports Xunit

' Slice 0078-12: the tally of the script windows Adobe raised for one document (error alerts K-BOT
' pressed OK on, the form's own messages, the JavaScript Debugger console).
Public Class AdobeScriptMonitorTests

    Private Shared Function Make(k_action As AdobeScriptAlertAction, Optional k_text As String = "GeneralErrorOperation failed.") As AdobeScriptAlert
        Return New AdobeScriptAlert(New DateTime(2026, 10, 6, 12, 0, 0), New IntPtr(&H230F10), "Warning: JavaScript Window",
                                    k_text, k_action, Nothing, 1)
    End Function

    <Fact>
    Public Sub NothingSeen_IsEmpty_AndSaysSo()
        Dim monitor As New AdobeScriptMonitor()
        Assert.True(monitor.IsEmpty)
        Assert.Equal(0, monitor.ErrorCount)
        Assert.Equal("niciun mesaj de script", monitor.Summary())
    End Sub

    <Fact>
    Public Sub Counts_SplitErrorsFromMessagesAndConsoles()
        Dim monitor As New AdobeScriptMonitor()
        monitor.Record(Make(AdobeScriptAlertAction.Pressed))
        monitor.Record(Make(AdobeScriptAlertAction.Pressed))
        monitor.Record(Make(AdobeScriptAlertAction.Hidden))
        monitor.Record(Make(AdobeScriptAlertAction.LeftToOperator, "Nu sunt erori la sectiunea A"))
        monitor.Record(Make(AdobeScriptAlertAction.ConsoleHidden))
        monitor.Record(Make(AdobeScriptAlertAction.ConsoleLeftAlone))

        Assert.False(monitor.IsEmpty)
        Assert.Equal(3, monitor.ErrorCount)
        Assert.Equal(1, monitor.MessageCount)
        Assert.Equal(2, monitor.ConsoleCount)
        Assert.Equal(6, monitor.Alerts.Count)
    End Sub

    <Fact>
    Public Sub StuckIsAPartOfTheErrors_NotAnAddition()
        ' A stuck alert always follows its first press, which already counted it as an error.
        Dim monitor As New AdobeScriptMonitor()
        monitor.Record(Make(AdobeScriptAlertAction.Pressed))
        monitor.Record(Make(AdobeScriptAlertAction.Stuck))

        Assert.Equal(1, monitor.ErrorCount)
        Assert.Equal(1, monitor.StuckCount)
        Assert.Equal("1 erori de script (OK apăsat de K-BOT; 1 nu s-a închis)", monitor.Summary())
    End Sub

    <Fact>
    Public Sub Summary_ListsEveryKindThatWasSeen()
        Dim monitor As New AdobeScriptMonitor()
        monitor.Record(Make(AdobeScriptAlertAction.Pressed))
        monitor.Record(Make(AdobeScriptAlertAction.Pressed))
        monitor.Record(Make(AdobeScriptAlertAction.LeftToOperator))
        monitor.Record(Make(AdobeScriptAlertAction.ConsoleHidden))

        Assert.Equal("2 erori de script (OK apăsat de K-BOT); 1 mesaje lăsate operatorului; consola de script ascunsă de 1 ori",
                     monitor.Summary())
    End Sub

    <Fact>
    Public Sub Reset_ForgetsTheDocument_WithoutRaisingRecorded()
        Dim monitor As New AdobeScriptMonitor()
        Dim raised As Integer = 0
        AddHandler monitor.Recorded, Sub(k_a) raised += 1
        monitor.Record(Make(AdobeScriptAlertAction.Pressed))
        monitor.Reset()

        Assert.True(monitor.IsEmpty)
        Assert.Equal(1, raised)
    End Sub

    <Fact>
    Public Sub Record_TellsTheListenerWhichWindow()
        Dim monitor As New AdobeScriptMonitor()
        Dim seen As AdobeScriptAlert = Nothing
        AddHandler monitor.Recorded, Sub(k_a) seen = k_a
        Dim sent As AdobeScriptAlert = Make(AdobeScriptAlertAction.LeftToOperator)
        monitor.Record(sent)

        Assert.Same(sent, seen)
    End Sub

    <Theory>
    <InlineData(AdobeScriptAlertAction.Pressed, True, False, "Eroare")>
    <InlineData(AdobeScriptAlertAction.Stuck, True, False, "Eroare")>
    <InlineData(AdobeScriptAlertAction.Hidden, True, False, "Eroare")>
    <InlineData(AdobeScriptAlertAction.LeftToOperator, False, False, "Mesaj")>
    <InlineData(AdobeScriptAlertAction.ConsoleHidden, False, True, "Consolă")>
    <InlineData(AdobeScriptAlertAction.ConsoleLeftAlone, False, True, "Consolă")>
    Public Sub Alert_KnowsWhatItIs(k_action As AdobeScriptAlertAction, k_isError As Boolean, k_isConsole As Boolean, k_kind As String)
        Dim alert As AdobeScriptAlert = Make(k_action)
        Assert.Equal(k_isError, alert.IsError)
        Assert.Equal(k_isConsole, alert.IsConsole)
        Assert.Equal(k_kind, alert.KindLabel)
    End Sub

    <Fact>
    Public Sub Describe_CarriesTheTextAndThePresses()
        Dim line As String = Make(AdobeScriptAlertAction.Pressed, "GeneralErrorOperation failed.").Describe()
        Assert.Contains("Eroare", line)
        Assert.Contains("OK apăsat", line)
        Assert.Contains("GeneralErrorOperation failed.", line)
        Assert.Contains("apăsări: 1", line)
    End Sub

End Class
