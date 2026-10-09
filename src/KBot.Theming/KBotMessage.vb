Option Strict On
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The ONLY gate through which K-BOT puts a dialog in front of the operator -- and, in the
''' same motion, writes its text to <c>&lt;AppDir&gt;\Logs\mesaje_operator.log</c>.
'''
''' <para><b>Why it exists.</b> House rule (operator, 08.09.2026): every message given to the
''' user must also reach a log. A <c>Write</c> pasted next to each <c>MessageBox.Show</c> would
''' have held for a day -- the first new call written without it breaks the rule silently. Here
''' the two things are ONE, so the rule cannot be forgotten.</para>
'''
''' <para><b>The source is not written at the call site.</b> <c>&lt;CallerFilePath&gt;</c> and
''' <c>&lt;CallerMemberName&gt;</c> compose it themselves, at COMPILE time -- so it cannot be
''' wrong, costs nothing at run time, and does not depend on a stack that may be optimised. The
''' result is <c>FileName.Method</c>, the same convention as <see cref="GlobalErrorLog.Write"/>,
''' so the two logs read side by side.</para>
'''
''' <para><b>Called exactly like MessageBox.Show / MsgBox.</b> The overloads below mirror the
''' shapes used in the solution and return the same thing, so replacing an existing call is
''' only a change of name.</para>
'''
''' <para>House placement: here (KBot.Theming) because it is the lowest WinForms layer -- every
''' project that shows dialogs already has it (Controls, App, Forexe, Migrator, DevHarness), so
''' the rule lands without a single new reference and without a cycle. It is not a control: it
''' stays a helper module, next to <c>AppScaling</c> and <c>KBotDesignTime</c>.</para>
'''
''' <para>Logging can never stop a dialog: <see cref="OperatorLog.Write"/> is a terminal sink
''' and never throws.</para>
''' </summary>
Public Module KBotMessage

    ' ---------------- MessageBox ----------------

    ''' <summary>Counterpart of <c>MessageBox.Show(owner, text, caption, buttons, icon)</c>.</summary>
    Public Function Show(owner As IWin32Window, text As String, caption As String,
                         buttons As MessageBoxButtons, icon As MessageBoxIcon,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, False)
    End Function

    ''' <summary>
    ''' Counterpart of <c>MessageBox.Show(owner, text, caption, buttons, icon, defaultButton)</c>
    ''' -- the form with a default button, used by the confirmations where "No" has to be the
    ''' one under the finger (closing without saving, deleting logs).
    ''' </summary>
    Public Function Show(owner As IWin32Window, text As String, caption As String,
                         buttons As MessageBoxButtons, icon As MessageBoxIcon,
                         defaultButton As MessageBoxDefaultButton,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, owner, text, caption, buttons, icon, defaultButton, False)
    End Function

    ''' <summary>The same shape, without an owner.</summary>
    Public Function Show(text As String, caption As String,
                         buttons As MessageBoxButtons, icon As MessageBoxIcon,
                         defaultButton As MessageBoxDefaultButton,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, Nothing, text, caption, buttons, icon, defaultButton, False)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(owner, text, caption, buttons)</c>.</summary>
    Public Function Show(owner As IWin32Window, text As String, caption As String,
                         buttons As MessageBoxButtons,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, False)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(owner, text, caption)</c>.</summary>
    Public Function Show(owner As IWin32Window, text As String, caption As String,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, False)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(text, caption, buttons, icon)</c> -- no owner.</summary>
    Public Function Show(text As String, caption As String,
                         buttons As MessageBoxButtons, icon As MessageBoxIcon,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, Nothing, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, False)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(text, caption, buttons)</c> -- no owner.</summary>
    Public Function Show(text As String, caption As String, buttons As MessageBoxButtons,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, Nothing, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, False)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(text, caption)</c> -- no owner.</summary>
    Public Function Show(text As String, caption As String,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, Nothing, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, False)
    End Function

    ''' <summary>
    ''' Like <c>Show(owner, text, caption, buttons, icon)</c> but the box is TOP-MOST (Win32
    ''' <c>MB_TOPMOST</c>), so it stays above other top-most windows. Needed by the interactive
    ''' tutorials (slice 000T): their step card and ring are top-most, and an ordinary box would
    ''' open underneath them. Same log line, same result.
    ''' </summary>
    Public Function ShowOnTop(owner As IWin32Window, text As String, caption As String,
                              buttons As MessageBoxButtons, icon As MessageBoxIcon,
                              <CallerFilePath> Optional file As String = Nothing,
                              <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, True)
    End Function

    ''' <summary>The top-most form without an owner.</summary>
    Public Function ShowOnTop(text As String, caption As String,
                              buttons As MessageBoxButtons, icon As MessageBoxIcon,
                              <CallerFilePath> Optional file As String = Nothing,
                              <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As DialogResult
        Return Run(file, member, line, Nothing, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, True)
    End Function

    ' THERE IS NO SINGLE-ARGUMENT OVERLOAD. It would be ambiguous with Show(text, caption): the
    ' caller-info parameters are String and optional too, so Show("a", "b") would fit it as
    ' well, and the rule that decides (the candidate filling fewer optionals wins) is too thin
    ' to rest every dialog in the application on. A message with no caption is written with an
    ' empty caption -- and a dialog without one tells the operator nothing about where it came
    ' from anyway.

    ' ---------------- MsgBox (the VB form) ----------------

    ''' <summary>
    ''' Counterpart of <c>MsgBox(prompt, style, title)</c>, for the places written in the VB
    ''' form (today: <c>RecorderForm</c>). Same dialog, same result -- only routed through
    ''' the log.
    ''' </summary>
    Public Function Show(prompt As String, style As MsgBoxStyle, title As String,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = 0) As MsgBoxResult
        Return CType(Run(file, member, line, Nothing, prompt, title, ButtonsOf(style), IconOf(style), DefaultOf(style), False), MsgBoxResult)
    End Function

    ' ---------------- who puts the dialog on screen ----------------

    ''' <summary>
    ''' The window that shows a message. KBot.Controls supplies K-BOT's own themed box
    ''' (<c>KBotMessageBox.Present</c>) and the application installs it at start-up; theming cannot
    ''' reference Controls, so the link is this delegate. <c>topMost</c> = the old
    ''' <c>MB_TOPMOST</c> of <see cref="ShowOnTop"/>.
    ''' </summary>
    Public Delegate Function MessagePresenter(owner As IWin32Window, text As String, caption As String,
                                              buttons As MessageBoxButtons, icon As MessageBoxIcon,
                                              defaultButton As MessageBoxDefaultButton,
                                              topMost As Boolean, extras As MessageExtras) As DialogResult

    ''' <summary>
    ''' Nothing (the default, and in the unit tests) = the native Windows box. Set once at start-up;
    ''' every <c>Show</c> in the solution then opens the K-BOT window instead, with no change at the
    ''' call sites.
    ''' </summary>
    Public Property Presenter As MessagePresenter

    ''' <summary>
    ''' What sends an error message to the server (slice 0112-04). The message window shows its «send the
    ''' error» button only when this is set and the message is an error; the application sets it once the
    ''' services exist (session, API). Controls and theming cannot reference the API, so the link is this delegate.
    ''' The task faults when the report could not be sent; the window tells the operator.
    ''' </summary>
    Public Delegate Function ErrorReportHandler(report As MessageErrorReport) As Task

    Public Property ErrorReporter As ErrorReportHandler

    <ThreadStatic> Private _lastExtraClicked As Boolean

    ''' <summary>
    ''' True when the LAST message shown on this thread was answered with its extra button (one the
    ''' message catalog added). That answer comes back as <c>DialogResult.None</c>; a call that cares
    ''' reads this right after <c>Show</c>. Set by the presenter.
    ''' </summary>
    Public Property LastExtraClicked As Boolean
        Get
            Return _lastExtraClicked
        End Get
        Set(value As Boolean)
            _lastExtraClicked = value
        End Set
    End Property

    ''' <summary>
    ''' The message catalog file (<c>Config/mesaje_catalog.json</c>) whose edited entries replace the
    ''' wording of a call; Nothing = the one next to the executable. Setting it re-reads the file.
    ''' </summary>
    Public Property CatalogPath As String
        Get
            Return MessageOverrides.CatalogPath
        End Get
        Set(value As String)
            MessageOverrides.CatalogPath = value
        End Set
    End Property

    ''' <summary>Re-reads the message catalog (the editor calls it after a save).</summary>
    Public Sub ReloadCatalog()
        MessageOverrides.Reload()
    End Sub

    ' The one road every message takes: catalog edits first (so the log keeps what the operator
    ' really read), then the log line, then the window.
    Private Function Run(k_file As String, k_member As String, k_line As Integer,
                         k_owner As IWin32Window, k_text As String, k_caption As String,
                         k_buttons As MessageBoxButtons, k_icon As MessageBoxIcon,
                         k_default As MessageBoxDefaultButton, k_topMost As Boolean) As DialogResult
        Dim k_extras As New MessageExtras()
        Try
            MessageOverrides.Apply(k_file, k_member, k_line, k_caption, k_text, k_buttons, k_icon, k_extras)
        Catch ex As Exception
            ' A broken catalog never stops a dialog: the call keeps its own wording.
            GlobalErrorLog.Write("KBotMessage.Run", ex)
            k_extras = New MessageExtras()
        End Try
        k_extras.Source = Source(k_file, k_member)
        k_extras.SourceLine = k_line
        Journal(k_file, k_member, k_caption, k_text, k_icon)
        LastExtraClicked = False
        Return Present(k_owner, k_text, k_caption, k_buttons, k_icon, k_default, k_topMost, k_extras)
    End Function

    Private Function Present(owner As IWin32Window, text As String, caption As String,
                             buttons As MessageBoxButtons, icon As MessageBoxIcon,
                             defaultButton As MessageBoxDefaultButton, topMost As Boolean,
                             extras As MessageExtras) As DialogResult
        Dim k_presenter As MessagePresenter = Presenter
        If k_presenter IsNot Nothing Then
            Return k_presenter(owner, text, caption, buttons, icon, defaultButton, topMost, extras)
        End If
        ' MB_TOPMOST (0x40000): MessageBoxOptions has no member for it; the value passes straight
        ' through into the style word of MessageBox. (The native box has no extra button.)
        Dim k_options As MessageBoxOptions = If(topMost, CType(&H40000, MessageBoxOptions), CType(0, MessageBoxOptions))
        Return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, k_options)
    End Function

    ' MsgBoxStyle packs buttons (low nibble), icon (&HF0) and default button (&HF00).
    Private Function ButtonsOf(style As MsgBoxStyle) As MessageBoxButtons
        Select Case CInt(style) And &HF
            Case 1 : Return MessageBoxButtons.OKCancel
            Case 2 : Return MessageBoxButtons.AbortRetryIgnore
            Case 3 : Return MessageBoxButtons.YesNoCancel
            Case 4 : Return MessageBoxButtons.YesNo
            Case 5 : Return MessageBoxButtons.RetryCancel
            Case Else : Return MessageBoxButtons.OK
        End Select
    End Function

    Private Function IconOf(style As MsgBoxStyle) As MessageBoxIcon
        Select Case CInt(style) And &HF0
            Case 16 : Return MessageBoxIcon.Error
            Case 32 : Return MessageBoxIcon.Question
            Case 48 : Return MessageBoxIcon.Warning
            Case 64 : Return MessageBoxIcon.Information
            Case Else : Return MessageBoxIcon.None
        End Select
    End Function

    Private Function DefaultOf(style As MsgBoxStyle) As MessageBoxDefaultButton
        Select Case CInt(style) And &HF00
            Case &H100 : Return MessageBoxDefaultButton.Button2
            Case &H200 : Return MessageBoxDefaultButton.Button3
            Case Else : Return MessageBoxDefaultButton.Button1
        End Select
    End Function

    ' ---------------- the shared part ----------------

    Private Sub Journal(file As String, member As String, caption As String,
                        text As String, icon As MessageBoxIcon)
        OperatorLog.Write(Source(file, member), caption, text, LevelOf(icon))
    End Sub

    ''' <summary>
    ''' <c>FileName.Method</c> -- for example <c>KBOT.DuLaIngestieAsync</c>. The file name, not
    ''' the type: the house convention is one type per file, and the path comes from the
    ''' compiler, so it cannot fall behind a renamed method.
    ''' </summary>
    Private Function Source(file As String, member As String) As String
        Dim where As String = String.Empty
        If Not String.IsNullOrEmpty(file) Then
            Try
                where = Path.GetFileNameWithoutExtension(file)
            Catch ex As ArgumentException
                ' An impossible path must not stop a dialog. Only the method name remains.
                where = String.Empty
            End Try
        End If
        If String.IsNullOrEmpty(where) Then Return If(member, String.Empty)
        Return where & "." & If(member, String.Empty)
    End Function

    Private Function LevelOf(icon As MessageBoxIcon) As KBotLogLevel
        ' Hand / Stop / Error share one value (16), so do Exclamation / Warning (48) and
        ' Asterisk / Information (64) -- which is why the canonical names are compared.
        Select Case icon
            Case MessageBoxIcon.Error : Return KBotLogLevel.Error
            Case MessageBoxIcon.Warning : Return KBotLogLevel.Warn
            Case Else : Return KBotLogLevel.Info
        End Select
    End Function

    Private Function LevelOf(style As MsgBoxStyle) As KBotLogLevel
        ' The style carries the buttons in its low bits: read ONLY the icon.
        Dim glyph As MsgBoxStyle = CType(style And CType(&HF0, MsgBoxStyle), MsgBoxStyle)
        Select Case glyph
            Case MsgBoxStyle.Critical : Return KBotLogLevel.Error
            Case MsgBoxStyle.Exclamation : Return KBotLogLevel.Warn
            Case Else : Return KBotLogLevel.Info
        End Select
    End Function

End Module
