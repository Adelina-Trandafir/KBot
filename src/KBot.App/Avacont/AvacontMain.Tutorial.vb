Option Strict On
Imports System.Globalization
Imports System.Text
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 000T-13: the main window as a tutorial host. It owns the ribbon, so it answers the places only it knows
''' (a tab, a button, the right-hand end of the panel) and raises a signal when the operator presses a tab or a button.
''' Everything else is the shell's (<see cref="KbotForm"/>): its anchors and signals pass through, so the tutorials written
''' for the shell keep working now that the veil is laid over the whole window.
''' </summary>
Partial Public Class AvacontMain
    Implements IKBotTutorialHost

    ''' <summary>Anchor prefix: <c>ribbon-tab.&lt;name&gt;</c> = that tab of the ribbon (<c>configurari</c>, <c>angajamente</c>, <c>e-factura</c>, <c>extrase</c>).</summary>
    Friend Const AnchorRibbonTabPrefix As String = "ribbon-tab."

    ''' <summary>Anchor prefix: <c>ribbon.&lt;key&gt;</c> = the ribbon button with that key (<c>ribbon.ef_token</c>); <c>ribbon.right</c> = the right-hand end of the panel.</summary>
    Friend Const AnchorRibbonPrefix As String = "ribbon."

    ''' <summary>The name after <see cref="AnchorRibbonPrefix"/> that is not a button: the view group, the form group and «Inchide» together.</summary>
    Friend Const AnchorRibbonRight As String = "right"

    ''' <summary>Signal prefix: <c>ribbon-tab:&lt;name&gt;</c> = that tab was pressed (also when it was the open one).</summary>
    Friend Const SignalRibbonTabPrefix As String = "ribbon-tab:"

    ''' <summary>Signal prefix: <c>ribbon:&lt;key&gt;</c> = that button was pressed and its command has run (a command that opens a modal window only returns when it closes: wait for that window with <c>opens:</c>).</summary>
    Friend Const SignalRibbonPrefix As String = "ribbon:"

    Public Event TutorialSignal(k_name As String) Implements IKBotTutorialHost.TutorialSignal

    ' Called once by the constructor: the shell's signals (selectors, reservations) reach the runner through this window.
    Private Sub HookTutorialSignals()
        AddHandler _kbot.TutorialSignal, AddressOf Shell_TutorialSignal
    End Sub

    ' UI boundary (event of the shell): log and swallow.
    Private Sub Shell_TutorialSignal(k_name As String)
        Try
            RaiseEvent TutorialSignal(k_name)
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Shell_TutorialSignal", ex)
        End Try
    End Sub

    Public Function TutorialSupports(k_key As String) As Boolean Implements IKBotTutorialHost.TutorialSupports
        Return _kbot.TutorialSupports(k_key)
    End Function

    Public Sub TutorialBegin(k_request As KBotTutorialRequest) Implements IKBotTutorialHost.TutorialBegin
        _kbot.TutorialBegin(k_request)
    End Sub

    Public Sub TutorialEnd() Implements IKBotTutorialHost.TutorialEnd
        _kbot.TutorialEnd()
    End Sub

    Public Function TutorialAnchor(k_name As String) As IReadOnlyList(Of Rectangle) Implements IKBotTutorialHost.TutorialAnchor
        Dim k_none As New List(Of Rectangle)()
        Try
            If k_name.StartsWith(AnchorRibbonTabPrefix, StringComparison.OrdinalIgnoreCase) Then
                Dim k_wanted As String = k_name.Substring(AnchorRibbonTabPrefix.Length)
                For k_i As Integer = 0 To ribbon.Tabs.Count - 1
                    If Not String.Equals(TabName(ribbon.Tabs(k_i).Text), k_wanted, StringComparison.OrdinalIgnoreCase) Then Continue For
                    Dim k_tab As Rectangle = ribbon.TabScreenBounds(k_i)
                    If Not k_tab.IsEmpty Then k_none.Add(k_tab)
                    Exit For
                Next
                Return k_none
            End If
            If k_name.StartsWith(AnchorRibbonPrefix, StringComparison.OrdinalIgnoreCase) Then
                Dim k_key As String = k_name.Substring(AnchorRibbonPrefix.Length)
                Dim k_rect As Rectangle = If(String.Equals(k_key, AnchorRibbonRight, StringComparison.OrdinalIgnoreCase),
                                             ribbon.RightEndScreenBounds(), ribbon.ButtonScreenBounds(k_key))
                If Not k_rect.IsEmpty Then k_none.Add(k_rect)
                Return k_none
            End If
            Return _kbot.TutorialAnchor(k_name)
        Catch ex As Exception
            ' UI boundary (called by the tutorial's timer): an anchor that cannot be read is «not on screen».
            GlobalErrorLog.Write("AvacontMain.TutorialAnchor", ex)
            Return k_none
        End Try
    End Function

    ' UI boundary (event of the ribbon): log and swallow.
    Private Sub Ribbon_TabClicked(sender As Object, e As KBotRibbonTabClickedEventArgs) Handles ribbon.TabClicked
        Try
            RaiseEvent TutorialSignal(SignalRibbonTabPrefix & TabName(e.Tab.Text))
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontMain.Ribbon_TabClicked", ex)
        End Try
    End Sub

    ' The name a tutorial uses for a tab: its caption without diacritics, lower case, spaces as dashes.
    Private Shared Function TabName(k_text As String) As String
        Dim k_builder As New StringBuilder()
        For Each k_char As Char In If(k_text, String.Empty).Normalize(NormalizationForm.FormD)
            If CharUnicodeInfo.GetUnicodeCategory(k_char) <> UnicodeCategory.NonSpacingMark Then k_builder.Append(k_char)
        Next
        Return k_builder.ToString().ToLowerInvariant().Replace(" "c, "-"c)
    End Function

End Class
