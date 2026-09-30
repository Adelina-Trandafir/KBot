Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' K-BOT's help (slice 0000-01): owns the topics, decides which parts this login may read, maps
''' a control to its topic and keeps the one help window. Installed once, before the login, by
''' <c>Program</c>: from then on F1 anywhere and the «?» of every caption bar come here.
'''
''' <para><b>Who reads what.</b> A director reads Part 3 only. Everyone else reads Part 1, plus
''' Part 2 while «Activează opțiuni avansate» is on (the same switch that shows those pages).</para>
'''
''' <para><b>Finding the topic.</b> From the control that asked, up through its parents; each
''' step offers a key (see <see cref="ScreenKeys"/>) and the first key some topic lists in its
''' <c>screens</c> wins. Nothing matches = the start page of the first visible part.</para>
''' </summary>
Public NotInheritable Class HelpService
    Implements IKBotHelpProvider

    Private ReadOnly _session As SessionContext
    Private _library As HelpLibrary
    Private _form As HelpForm

    Public Sub New(session As SessionContext)
        _session = session
    End Sub

    ''' <summary>Makes this the application's help and turns F1 on.</summary>
    Public Shared Sub Install(service As HelpService)
        Try
            If KBotHelp.Provider Is service Then Return   ' one F1 filter, however many logins
            KBotHelp.Provider = service
            Application.AddMessageFilter(New HelpKeyFilter())
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.Install", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The topics; read on first use and again each time the help window is opened anew.</summary>
    Public ReadOnly Property Library As HelpLibrary
        Get
            If _library Is Nothing Then _library = HelpLibrary.Load(HelpLibrary.DefaultRoot())
            Return _library
        End Get
    End Property

    ''' <summary>
    ''' Puts K-BOT on a screen named like a capture's / tour step's <c>goto</c> (slices 0000-02/04):
    ''' <c>help</c> / <c>help:&lt;id&gt;</c> here, the rest through the main window
    ''' (<see cref="IHelpCaptureNavigator"/>). Returns what the operator must do by hand, or Nothing.
    ''' Never throws: a screen K-BOT cannot open must not end the capture or the tour.
    ''' </summary>
    Friend Function Navigate(target As String, topicId As String) As String
        Dim t As String = If(target, String.Empty).Trim()
        If t.Length = 0 Then Return Nothing
        Try
            If String.Equals(t, "help", StringComparison.OrdinalIgnoreCase) Then
                ShowTopic(topicId)
                Return Nothing
            End If
            If t.StartsWith("help:", StringComparison.OrdinalIgnoreCase) Then
                ShowTopic(t.Substring(5).Trim())
                Return Nothing
            End If
            Dim navigator As IHelpCaptureNavigator = Application.OpenForms.OfType(Of IHelpCaptureNavigator)().FirstOrDefault()
            If navigator Is Nothing Then
                Return "Fereastra principală K-BOT nu e deschisă: deschide manual ecranul potrivit."
            End If
            Return navigator.NavigateForCapture(t)
        Catch ex As ArgumentException
            GlobalErrorLog.Write("HelpService.Navigate", ex)
            Return "Destinația «" & t & "» din ajutor nu e cunoscută: deschide manual ecranul potrivit."
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.Navigate", ex)
            Return "K-BOT n-a putut deschide singur ecranul (detalii în jurnalul de erori): deschide-l manual."
        End Try
    End Function

    ''' <summary>Reads the topic files again (the capture list's «Reîncarcă»); an open help window redraws.</summary>
    Public Sub ReloadLibrary()
        _library = Nothing
        RefreshOpenPage()
    End Sub

    ''' <summary>Redraws the page of an open help window (a new picture was just taken).</summary>
    Public Sub RefreshOpenPage()
        Try
            If _form IsNot Nothing AndAlso Not _form.IsDisposed AndAlso _form.Visible Then _form.RefreshPage()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.RefreshOpenPage", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The parts this login may read, in manual order.</summary>
    Public Function VisibleParts() As List(Of HelpPart)
        If _session IsNot Nothing AndAlso DirectorForm.EsteDirector(_session) Then
            Return New List(Of HelpPart) From {HelpPart.Director}
        End If
        Dim parts As New List(Of HelpPart) From {HelpPart.Contabil}
        If AppSettings.Current.AdvancedOptions Then parts.Add(HelpPart.Avansat)
        Return parts
    End Function

    ''' <summary>
    ''' The parts the exported manual carries: what this login reads, and all three once advanced
    ''' options are on (whoever unlocked them is the one preparing the manual for others).
    ''' </summary>
    Public Function ManualParts() As List(Of HelpPart)
        If AppSettings.Current.AdvancedOptions Then
            Return New List(Of HelpPart) From {HelpPart.Contabil, HelpPart.Avansat, HelpPart.Director}
        End If
        Return VisibleParts()
    End Function

    ''' <summary>F1 / «?»: the topic for <paramref name="origin"/>, in the help window.</summary>
    Public Sub ShowHelp(origin As Control) Implements IKBotHelpProvider.ShowHelp
        Try
            Dim keys As List(Of String) = ScreenKeys(origin)
            Dim window As HelpForm = EnsureWindow()
            Dim parts As List(Of HelpPart) = VisibleParts()
            Dim topic As HelpTopic = Nothing
            For Each k As String In keys
                topic = Library.FindByScreen(k, parts)
                If topic IsNot Nothing Then Exit For
            Next
            window.ContextKeys = keys
            If topic Is Nothing Then
                window.ShowHome()
            Else
                window.ShowTopic(topic.Id)
            End If
            Present(window)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.ShowHelp", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Starts a guided tour (slice 0000-04). The help window steps aside (minimized) while it runs
    ''' and comes back when it ends.
    ''' </summary>
    Public Sub StartTour(id As String)
        Try
            Dim tour As HelpTour = Library.FindTour(id)
            If tour Is Nothing Then Throw New ArgumentException("Unknown tour '" & id & "'.", NameOf(id))
            Dim window As HelpForm = If(_form IsNot Nothing AndAlso Not _form.IsDisposed AndAlso _form.Visible, _form, Nothing)
            If window IsNot Nothing Then window.WindowState = FormWindowState.Minimized
            HelpTourRunner.Start(Me, tour,
                Sub()
                    If window IsNot Nothing AndAlso Not window.IsDisposed Then window.WindowState = FormWindowState.Normal
                End Sub)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.StartTour", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Opens the help window at one topic (links from other windows, tours).</summary>
    Public Sub ShowTopic(id As String)
        Try
            Dim window As HelpForm = EnsureWindow()
            window.ShowTopic(id)
            Present(window)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.ShowTopic", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Writes the manual (every part in <see cref="ManualParts"/>) to one HTML file the operator
    ''' picks, then opens it in the default browser, from where it prints or saves as PDF.
    ''' </summary>
    Public Sub ExportManual(owner As IWin32Window)
        Try
            Using dlg As New SaveFileDialog()
                dlg.Title = "Salvează manualul K-BOT"
                dlg.Filter = "Pagină web (*.html)|*.html"
                dlg.FileName = "Manual_KBOT_" & DateTime.Now.ToString("yyyyMMdd") & ".html"
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                If dlg.ShowDialog(owner) <> DialogResult.OK Then Return
                Dim html As String = HelpHtml.Manual(Library, ManualParts(), AppUpdateService.CurrentVersion.ToString())
                File.WriteAllText(dlg.FileName, html, New UTF8Encoding(True))
                Process.Start(New ProcessStartInfo(dlg.FileName) With {.UseShellExecute = True})?.Dispose()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("HelpService.ExportManual", ex)
            Throw
        End Try
    End Sub

    ' A window disabled by a modal dialog opened after it cannot be used: a fresh one is enabled.
    Private Function EnsureWindow() As HelpForm
        If _form IsNot Nothing AndAlso (_form.IsDisposed OrElse Not _form.Enabled) Then
            If Not _form.IsDisposed Then _form.Close()
            _form = Nothing
        End If
        If _form Is Nothing Then
            _library = Nothing   ' re-read the files: an edited topic shows on the next opening
            _form = New HelpForm(Me)
        End If
        Return _form
    End Function

    Private Shared Sub Present(window As HelpForm)
        If Not window.Visible Then window.Show()
        If window.WindowState = FormWindowState.Minimized Then window.WindowState = FormWindowState.Normal
        window.BringToFront()
        window.Activate()
    End Sub

    ''' <summary>
    ''' The keys offered by <paramref name="origin"/> and its parents, nearest first:
    ''' <c>Type.controlName</c> for a named control inside a form or user control, and the type
    ''' name itself for a form or user control (then <c>OwnerType.name</c> for a user control
    ''' placed inside another). The caption bar stands for the focused control of its window.
    ''' </summary>
    Friend Shared Function ScreenKeys(origin As Control) As List(Of String)
        Dim keys As New List(Of String)()
        Dim start As Control = origin
        If TypeOf origin Is KBotCaptionBar Then
            Dim f As Form = origin.FindForm()
            start = If(DeepestActive(f), CType(f, Control))
        End If

        Dim c As Control = start
        While c IsNot Nothing
            Dim owner As Control = OwnerOf(c)
            If TypeOf c Is Form OrElse TypeOf c Is UserControl Then
                AddKey(keys, c.GetType().Name)
                If owner IsNot Nothing AndAlso c.Name.Length > 0 Then AddKey(keys, owner.GetType().Name & "." & c.Name)
            ElseIf owner IsNot Nothing AndAlso c.Name.Length > 0 Then
                AddKey(keys, owner.GetType().Name & "." & c.Name)
            End If
            c = c.Parent
        End While
        Return keys
    End Function

    Private Shared Sub AddKey(keys As List(Of String), key As String)
        If Not keys.Contains(key, StringComparer.OrdinalIgnoreCase) Then keys.Add(key)
    End Sub

    ' The nearest ancestor that is a form or a user control: the one whose designer declared c.
    Private Shared Function OwnerOf(c As Control) As Control
        Dim p As Control = c.Parent
        While p IsNot Nothing
            If TypeOf p Is Form OrElse TypeOf p Is UserControl Then Return p
            p = p.Parent
        End While
        Return Nothing
    End Function

    Private Shared Function DeepestActive(f As Form) As Control
        If f Is Nothing Then Return Nothing
        Dim c As Control = f.ActiveControl
        While TypeOf c Is ContainerControl AndAlso DirectCast(c, ContainerControl).ActiveControl IsNot Nothing
            c = DirectCast(c, ContainerControl).ActiveControl
        End While
        Return c
    End Function

End Class

''' <summary>
''' F1 anywhere in K-BOT (slice 0000-01). A message filter rather than each form's
''' <c>HelpRequested</c>: some custom controls eat their keys, and one filter covers every window,
''' including the ones written before the help existed.
''' </summary>
Friend NotInheritable Class HelpKeyFilter
    Implements IMessageFilter

    Private Const WM_KEYDOWN As Integer = &H100
    Private Const VK_F1 As Integer = &H70

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        Try
            If m.Msg <> WM_KEYDOWN OrElse m.WParam.ToInt64() <> VK_F1 Then Return False
            If Control.ModifierKeys <> Keys.None Then Return False
            If Not KBotHelp.IsAvailable Then Return False
            Dim c As Control = Control.FromChildHandle(m.HWnd)
            If c Is Nothing Then Return False
            KBotHelp.Request(c)
            Return True
        Catch ex As Exception
            ' UI boundary (message pump): log and let the key through.
            GlobalErrorLog.Write("HelpKeyFilter.PreFilterMessage", ex)
            Return False
        End Try
    End Function

End Class
