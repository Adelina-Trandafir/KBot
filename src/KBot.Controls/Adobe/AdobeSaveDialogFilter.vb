Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports KBot.Common

''' <summary>What the Save As trap decided a dialog is (slice 0078-02).</summary>
Public Enum AdobeDialogKind
    ''' <summary>Not a dialog of the hosted Adobe (another process, or not a <c>#32770</c>).</summary>
    NotOurs = 0
    ''' <summary>A file dialog of the hosted Adobe: file-name box + Save button. We fill and press it.</summary>
    SaveAs = 1
    ''' <summary>The «file already exists, replace?» prompt owned by the Save As we just pressed.</summary>
    ConfirmOverwrite = 2
    ''' <summary>Some other Adobe dialog (a warning, the signature panel...). Left alone, logged once.</summary>
    Other = 3
End Enum

''' <summary>
''' Plain facts about one top-level window, gathered by <see cref="AdobeSaveTrap"/> through Win32 so
''' the decision itself (<see cref="AdobeSaveDialogFilter.Classify"/>) is pure and unit-testable.
''' </summary>
Public NotInheritable Class AdobeDialogFacts
    Public Property ClassName As String = ""
    Public Property OwnerPid As Integer
    ''' <summary>GW_OWNER of the window (the window it is modal to).</summary>
    Public Property OwnerWindow As IntPtr = IntPtr.Zero
    ''' <summary>A file-name edit box was found (Edit under a ComboBox with id 1001 or 1148, or edt1 = 1152).</summary>
    Public Property HasFileNameEdit As Boolean
    ''' <summary>A child Button with id IDOK (1) -- the Save button of both dialog styles.</summary>
    Public Property HasOkButton As Boolean
    ''' <summary>A child Button with id IDYES (6) -- the classic message-box confirm.</summary>
    Public Property HasYesButton As Boolean
    ''' <summary>A DirectUIHWND child and no file-name edit: a TaskDialog (Vista-style confirm).</summary>
    Public Property IsTaskDialog As Boolean
    ''' <summary>A child Button with id IDNO (7) -- with Yes and Cancel, the «save changes?» message box.</summary>
    Public Property HasNoButton As Boolean
    ''' <summary>The readable text of the dialog (its Static children joined by a space); empty when none is readable.</summary>
    Public Property Text As String = ""
    Public Property Title As String = ""

    Public Function Describe() As String
        Return $"clasă={ClassName} proces={OwnerPid} titlu=«{Title}» numeFișier={HasFileNameEdit} " &
               $"butonOK={HasOkButton} butonDa={HasYesButton} butonNu={HasNoButton} taskDialog={IsTaskDialog}" &
               If(Text.Length > 0, $" text=«{If(Text.Length > 200, Text.Substring(0, 200) & "…", Text)}»", "")
    End Function
End Class

''' <summary>
''' Pure decisions of the Save As trap. No Win32 here -- the trap gathers
''' <see cref="AdobeDialogFacts"/> and asks.
''' </summary>
Public NotInheritable Class AdobeSaveDialogFilter

    Private Sub New()
    End Sub

    ''' <summary>The window class of every standard Windows dialog, file dialogs and message boxes included.</summary>
    Public Const DialogClass As String = "#32770"

    ''' <summary>Control ids of the file-name combo box: 1148 (cmb13, classic) and 1001 (Vista IFileDialog).</summary>
    Public Shared ReadOnly FileNameComboIds As IReadOnlyList(Of Integer) = New Integer() {1148, 1001}
    ''' <summary>Control id of the plain file-name edit (edt1) in the oldest dialog template.</summary>
    Public Const FileNameEditId As Integer = 1152

    ''' <summary>
    ''' Classifies a top-level window. <paramref name="awaitingConfirmFor"/> = the Save As we pressed
    ''' last (IntPtr.Zero when none): only a prompt OWNED by that dialog is ever answered «Yes», so an
    ''' unrelated Adobe question can never be accepted blindly.
    ''' </summary>
    Public Shared Function Classify(facts As AdobeDialogFacts, adobePids As IEnumerable(Of Integer),
                                    awaitingConfirmFor As IntPtr) As AdobeDialogKind
        If facts Is Nothing Then Return AdobeDialogKind.NotOurs
        If Not String.Equals(facts.ClassName, DialogClass, StringComparison.Ordinal) Then Return AdobeDialogKind.NotOurs
        If adobePids Is Nothing Then Return AdobeDialogKind.NotOurs
        Dim ours As Boolean = False
        For Each p As Integer In adobePids
            If p > 0 AndAlso p = facts.OwnerPid Then ours = True
        Next
        If Not ours Then Return AdobeDialogKind.NotOurs

        If facts.HasFileNameEdit AndAlso facts.HasOkButton Then Return AdobeDialogKind.SaveAs

        If awaitingConfirmFor <> IntPtr.Zero AndAlso facts.OwnerWindow = awaitingConfirmFor AndAlso
           (facts.HasYesButton OrElse facts.IsTaskDialog) Then
            Return AdobeDialogKind.ConfirmOverwrite
        End If

        ' Slice 0078-13: MEASURED 05.10.2026 on Acrobat 19.12 -- the «Replace existing file?» box is NOT
        ' owned by the Save As we pressed, so the owner test above never matched and the box stayed on
        ' screen (Save As timed out and reappeared 44 times). While a Save As of ours is pending, a
        ' Yes/No box of the watched process whose text is the replace question is that confirm.
        If awaitingConfirmFor <> IntPtr.Zero AndAlso facts.HasYesButton AndAlso IsReplaceQuestion(facts.Text) Then
            Return AdobeDialogKind.ConfirmOverwrite
        End If

        Return AdobeDialogKind.Other
    End Function

    ''' <summary>
    ''' True when <paramref name="text"/> is the «file already exists, replace it?» question (English
    ''' wording of Windows / Adobe, or the Romanian one).
    ''' </summary>
    Public Shared Function IsReplaceQuestion(text As String) As Boolean
        If String.IsNullOrWhiteSpace(text) Then Return False
        For Each marker As String In New String() {"already exists", "Replace existing", "există deja", "exista deja", "înlocui", "inlocui"}
            If text.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' True when the text read back from the file-name box names exactly <paramref name="target"/>:
    ''' trimmed, surrounding quotes removed, case-insensitive, both sides normalised by
    ''' <see cref="Path.GetFullPath(String)"/>. A relative or truncated name is a mismatch.
    ''' </summary>
    Public Shared Function SamePath(readBack As String, target As String) As Boolean
        If String.IsNullOrWhiteSpace(readBack) OrElse String.IsNullOrWhiteSpace(target) Then Return False
        Dim a As String = readBack.Trim().Trim(""""c).Trim()
        Dim b As String = target.Trim()
        If Not Path.IsPathRooted(a) Then Return False
        Try
            a = Path.GetFullPath(a)
            b = Path.GetFullPath(b)
        Catch ex As Exception
            ' An unparsable path is simply «not the target»; the trap cancels and says so.
            GlobalErrorLog.Write("AdobeSaveDialogFilter.SamePath", ex)
            Return False
        End Try
        Return String.Equals(a, b, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' True when the file name Adobe OFFERS in a fresh Save As names the document at
    ''' <paramref name="target"/>. Adobe offers either the file name or -- MEASURED 23.09.2026 on the
    ''' ActiveX control -- the whole path with the separators turned into «_»
    ''' (<c>C__Users_..._BANC_DDF_41.pdf</c>). So: the same path, or the same file stem, or a text
    ''' ending in «_» / «\» + that stem. The extension is ignored.
    ''' </summary>
    Public Shared Function NameMatches(offered As String, target As String) As Boolean
        If String.IsNullOrWhiteSpace(offered) OrElse String.IsNullOrWhiteSpace(target) Then Return False
        Dim text As String = offered.Trim().Trim(""""c).Trim()
        If Path.IsPathRooted(text) AndAlso SamePath(text, target) Then Return True
        Dim stem As String = Path.GetFileNameWithoutExtension(target.Trim())
        If String.IsNullOrEmpty(stem) Then Return False
        If text.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) Then text = text.Substring(0, text.Length - 4)
        If String.Equals(text, stem, StringComparison.OrdinalIgnoreCase) Then Return True
        If text.Length > stem.Length AndAlso text.EndsWith(stem, StringComparison.OrdinalIgnoreCase) Then
            Dim before As Char = text(text.Length - stem.Length - 1)
            Return before = "_"c OrElse before = "\"c OrElse before = "/"c
        End If
        Return False
    End Function

    ''' <summary>
    ''' True when <paramref name="facts"/> is Adobe's «Do you want to save changes to '...' before
    ''' closing?» box for the document at <paramref name="target"/>. Only asked while K-BOT itself is
    ''' closing that document (<see cref="AdobeSaveTrap.BeginClose"/>), and only for a process the
    ''' trap watches, so the operator's own documents are never answered.
    '''
    ''' A message box: Yes + No buttons. A TaskDialog (buttons are not windows, nothing readable) is
    ''' accepted only when it carries Adobe's title. When the text IS readable it must name the
    ''' document: the file name, or its stem.
    ''' </summary>
    Public Shared Function IsSaveOnClosePrompt(facts As AdobeDialogFacts, target As String) As Boolean
        If facts Is Nothing OrElse String.IsNullOrWhiteSpace(target) Then Return False
        If facts.HasFileNameEdit Then Return False
        Dim adobeTitle As Boolean = facts.Title.StartsWith("Adobe", StringComparison.OrdinalIgnoreCase)
        Dim buttons As Boolean = facts.HasYesButton AndAlso facts.HasNoButton
        Dim adobeTask As Boolean = facts.IsTaskDialog AndAlso adobeTitle
        Dim stem As String = Path.GetFileNameWithoutExtension(target.Trim())
        Dim namesDocument As Boolean = Not String.IsNullOrEmpty(stem) AndAlso
            facts.Text.IndexOf(stem, StringComparison.OrdinalIgnoreCase) >= 0

        ' MEASURED 01.10.2026 (the prompt Adobe really shows): class #32770, Adobe's title, the
        ' text readable and naming the document -- and NO button with id 6 / 7 (the buttons are
        ' found by their caption, see IsNoCaption). Title + the document's name is enough.
        If adobeTitle AndAlso namesDocument Then Return True
        If Not buttons AndAlso Not adobeTask Then Return False
        If String.IsNullOrWhiteSpace(facts.Text) Then Return True
        Return namesDocument
    End Function

    ''' <summary>
    ''' True for the caption of a «No» button: No / Nu / Nein / Non / Nao, with or without an
    ''' accelerator ampersand or a trailing mnemonic like «(&amp;N)». Adobe's own prompts do not use
    ''' the standard button ids, so the button is known by its text.
    ''' </summary>
    Public Shared Function IsNoCaption(caption As String) As Boolean
        If String.IsNullOrWhiteSpace(caption) Then Return False
        Dim t As String = caption.Replace("&", "").Trim()
        Dim paren As Integer = t.IndexOf("("c)
        If paren > 0 Then t = t.Substring(0, paren).Trim()
        For Each no As String In {"No", "Nu", "Nein", "Non", "Nao"}
            If String.Equals(t, no, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' True for the windows Adobe opens because a form script failed: the alert box (title starting
    ''' with <paramref name="alertTitle"/>) and the script console (<paramref name="consoleTitle"/>).
    ''' </summary>
    Public Shared Function IsScriptNoise(title As String, alertTitle As String, consoleTitle As String) As Boolean
        If String.IsNullOrWhiteSpace(title) Then Return False
        Dim t As String = title.Trim()
        Return (Not String.IsNullOrEmpty(alertTitle) AndAlso t.StartsWith(alertTitle, StringComparison.OrdinalIgnoreCase)) OrElse
               (Not String.IsNullOrEmpty(consoleTitle) AndAlso t.StartsWith(consoleTitle, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>
    ''' True for the caption of an OK button: «OK» in any case, with or without an accelerator
    ''' ampersand. Adobe's script alert gives every button id 0, so OK is known only by its text.
    ''' </summary>
    Public Shared Function IsOkCaption(caption As String) As Boolean
        If String.IsNullOrWhiteSpace(caption) Then Return False
        Return String.Equals(caption.Replace("&", "").Trim(), "OK", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>Romanian label for the log.</summary>
    Public Shared Function Label(kind As AdobeDialogKind) As String
        Select Case kind
            Case AdobeDialogKind.SaveAs : Return "«Salvare ca»"
            Case AdobeDialogKind.ConfirmOverwrite : Return "confirmare de suprascriere"
            Case AdobeDialogKind.Other : Return "alt dialog Adobe (lăsat în pace)"
            Case Else : Return "nu e al nostru"
        End Select
    End Function

End Class
