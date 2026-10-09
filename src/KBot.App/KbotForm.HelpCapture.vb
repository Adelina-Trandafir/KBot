Option Strict On
Imports KBot.Common
Imports KBot.Controls

' Slice 0000-02 -- the main window's side of the help capture tool: the «Capturi pentru ajutor»
' menu entry (shown only in capture mode) and the navigation a capture's «goto» asks for.
Partial Public Class KbotForm
    Implements IHelpCaptureNavigator

    Private Const HelpCaptureMenuKey As String = "capturi_ajutor"
    Private Const AdminMenuKey As String = "admin"

    ''' <summary>True while the capture mode is usable: switched on AND advanced options on.</summary>
    Friend Shared ReadOnly Property HelpCaptureModeOn As Boolean
        Get
            Dim s As AppSettings = AppSettings.Current
            Return s.AdvancedOptions AndAlso s.HelpCaptureMode
        End Get
    End Property

    ' The entry follows the setting at every opening, so a change in the settings window needs no restart.
    Private Sub MenuNou_Opening(sender As Object, e As ComponentModel.CancelEventArgs) Handles menuNou.Opening
        Try
            Dim shown As Boolean = HelpCaptureModeOn
            Dim logShown As Boolean = FeatureSwitches.VizualizatorJurnaleActiv
            For Each item As KBotMenuItem In menuNou.Items
                ' Slice 0112: the «ADMIN» folder holds the operator tools; it shows only while one of them does.
                '   - help captures: capture mode on (0000-02);
                '   - tutorial designer and message catalog: Debug build only (000T-10, 0112).
                If String.Equals(item.Key, AdminMenuKey, StringComparison.Ordinal) Then
                    Dim anyShown As Boolean = False
                    For Each child As KBotMenuItem In item.Items
                        If String.Equals(child.Key, HelpCaptureMenuKey, StringComparison.Ordinal) Then
                            child.Visible = shown
                        ElseIf String.Equals(child.Key, TutorialDesignerMenuKey, StringComparison.Ordinal) Then
                            child.Visible = TutorialDesignerAvailable
                        ElseIf String.Equals(child.Key, MessageCatalogMenuKey, StringComparison.Ordinal) Then
                            child.Visible = MessageCatalogAvailable
                        End If
                        If child.Visible Then anyShown = True
                    Next
                    item.Visible = anyShown
                End If
                If String.Equals(item.Key, "jurnal", StringComparison.Ordinal) Then item.Visible = logShown OrElse _helpMenuReveal
                ' Slice 0000-31: the tour shows the rows that appear only sometimes.
                If _helpMenuReveal AndAlso String.Equals(item.Key, UncorrelatedMenuKey, StringComparison.Ordinal) Then
                    _helpUncorrelatedWas = item.Visible
                    item.Visible = True
                End If
            Next
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("MainForm.MenuNou_Opening", ex)
        End Try
    End Sub

    Private Sub DeschideCapturileAjutorului()
        Try
            Dim help As HelpService = TryCast(KBotHelp.Provider, HelpService)
            If help Is Nothing Then Throw New InvalidOperationException("The help service is not installed.")
            HelpCaptureForm.ShowFor(Me, help)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideCapturileAjutorului", ex)
            KBotMessage.Show(Me, "Fereastra capturilor nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Capturi pentru ajutor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' <c>view:&lt;key&gt;</c> selects a view of the bar (it must be on for the selected angajament);
    ''' <c>menu:&lt;key&gt;</c> runs a header menu entry; <c>setari:&lt;page&gt;</c> opens the settings
    ''' window on a page.
    ''' </summary>
    Public Function NavigateForCapture(target As String) As String Implements IHelpCaptureNavigator.NavigateForCapture
        Try
            If WindowState = FormWindowState.Minimized Then WindowState = FormWindowState.Normal
            Activate()
            Dim colon As Integer = target.IndexOf(":"c)
            If colon <= 0 Then Throw New ArgumentException("Capture target without 'kind:' -> " & target, NameOf(target))
            Dim kind As String = target.Substring(0, colon).Trim().ToLowerInvariant()
            Dim key As String = target.Substring(colon + 1).Trim()
            Select Case kind
                Case "view"
                    Dim item As KBotNavItem = navViews.Items.FirstOrDefault(Function(i) String.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase))
                    If item Is Nothing Then Throw New ArgumentException("No view '" & key & "' in navViews.", NameOf(target))
                    If Not item.Enabled OrElse Not item.Visible Then
                        Return "Vederea «" & item.Text & "» nu e disponibilă pentru angajamentul selectat. Selectați în arbore un angajament care o are, apoi deschideți-o."
                    End If
                    navViews.SelectedKey = item.Key
                    Return Nothing
                Case "menu"
                    MenuNou_ItemClicked(menuNou, New KBotMenuItemClickedEventArgs(
                        menuNou.Items.Concat(menuNou.Items.SelectMany(Function(i) i.Items)) _
                                     .First(Function(i) String.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase))))
                    Return Nothing
                Case "setari"
                    SetariForm.ShowFor(Me, _setariFactory).ShowPage(key)
                    Return Nothing
                Case Else
                    Throw New ArgumentException("Unknown capture target kind '" & kind & "'.", NameOf(target))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.NavigateForCapture", ex)
            Throw
        End Try
    End Function

    ''' <summary>Slice 0000-19: the caption of a view button / menu row, «Setari» (the settings window) for a settings page.</summary>
    Public Function TargetCaption(target As String) As String Implements IHelpCaptureNavigator.TargetCaption
        Try
            Dim colon As Integer = If(target, String.Empty).IndexOf(":"c)
            If colon <= 0 Then Return Nothing
            Dim kind As String = target.Substring(0, colon).Trim().ToLowerInvariant()
            Dim key As String = target.Substring(colon + 1).Trim()
            Select Case kind
                Case "view"
                    Dim item As KBotNavItem = navViews.Items.FirstOrDefault(Function(i) String.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase))
                    Return If(item Is Nothing OrElse String.IsNullOrWhiteSpace(item.Text), Nothing, item.Text.Trim())
                Case "menu"
                    Dim row As KBotMenuItem = menuNou.Items.Concat(menuNou.Items.SelectMany(Function(i) i.Items)) _
                                                     .FirstOrDefault(Function(i) String.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase))
                    If row Is Nothing Then Return Nothing
                    ' The row text may carry rich-text marks (<b>) and a «(!)» flag: the caption is the words.
                    Dim caption As String = System.Text.RegularExpressions.Regex.Replace(If(row.Text, String.Empty), "<[^>]+>", String.Empty)
                    caption = caption.Replace("(!)", String.Empty).Trim()
                    Return If(caption.Length = 0, Nothing, caption)
                Case "setari"
                    Return "Setări"
                Case Else
                    Return Nothing
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TargetCaption", ex)
            Throw
        End Try
    End Function

End Class
