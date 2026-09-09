Option Strict On
Imports System.IO
Imports System.Runtime.CompilerServices
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
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, icon)
        Return MessageBox.Show(owner, text, caption, buttons, icon)
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
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, icon)
        Return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton)
    End Function

    ''' <summary>The same shape, without an owner.</summary>
    Public Function Show(text As String, caption As String,
                         buttons As MessageBoxButtons, icon As MessageBoxIcon,
                         defaultButton As MessageBoxDefaultButton,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, icon)
        Return MessageBox.Show(text, caption, buttons, icon, defaultButton)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(owner, text, caption, buttons)</c>.</summary>
    Public Function Show(owner As IWin32Window, text As String, caption As String,
                         buttons As MessageBoxButtons,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, MessageBoxIcon.None)
        Return MessageBox.Show(owner, text, caption, buttons)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(owner, text, caption)</c>.</summary>
    Public Function Show(owner As IWin32Window, text As String, caption As String,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, MessageBoxIcon.None)
        Return MessageBox.Show(owner, text, caption)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(text, caption, buttons, icon)</c> -- no owner.</summary>
    Public Function Show(text As String, caption As String,
                         buttons As MessageBoxButtons, icon As MessageBoxIcon,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, icon)
        Return MessageBox.Show(text, caption, buttons, icon)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(text, caption, buttons)</c> -- no owner.</summary>
    Public Function Show(text As String, caption As String, buttons As MessageBoxButtons,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, MessageBoxIcon.None)
        Return MessageBox.Show(text, caption, buttons)
    End Function

    ''' <summary>Counterpart of <c>MessageBox.Show(text, caption)</c> -- no owner.</summary>
    Public Function Show(text As String, caption As String,
                         <CallerFilePath> Optional file As String = Nothing,
                         <CallerMemberName> Optional member As String = Nothing) As DialogResult
        Journal(file, member, caption, text, MessageBoxIcon.None)
        Return MessageBox.Show(text, caption)
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
                         <CallerMemberName> Optional member As String = Nothing) As MsgBoxResult
        OperatorLog.Write(Source(file, member), title, prompt, LevelOf(style))
        Return MsgBox(prompt, style, title)
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
