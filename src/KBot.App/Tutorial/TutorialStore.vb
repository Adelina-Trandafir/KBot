Option Strict On
Imports System.IO
Imports System.Text
Imports KBot.Common
Imports Microsoft.VisualBasic.FileIO

''' <summary>
''' Where tutorials are written (slice 000T), the same way help pictures are
''' (<see cref="HelpCaptureStore"/>): ALWAYS into <c>&lt;AppDir&gt;\Help\tutorials\</c>, where the
''' help reads them, so a saved tutorial is live after a reload; ALSO into
''' <c>src\KBot.App\HelpContent\tutorials\</c> when K-BOT runs from a build inside the repository.
''' </summary>
Friend NotInheritable Class TutorialStore

    Private Sub New()
    End Sub

    ''' <summary>The folder the help reads tutorials from.</summary>
    Public Shared Function RuntimeFolder() As String
        Return Path.Combine(HelpLibrary.DefaultRoot(), HelpLibrary.TutorialsFolderName)
    End Function

    ''' <summary>The repository's tutorials folder, or Nothing when K-BOT does not run from a repository build.</summary>
    Public Shared Function SourceFolder() As String
        Dim img As String = HelpCaptureStore.FindSourceFolder()
        If String.IsNullOrEmpty(img) Then Return Nothing
        Return Path.Combine(Directory.GetParent(img).FullName, HelpLibrary.TutorialsFolderName)
    End Function

    ''' <summary>
    ''' Writes <paramref name="k_flow"/> (UTF-8 without a byte-order mark, like every help file) to every
    ''' folder it belongs in; returns the paths written. The id must be a plain file name.
    ''' </summary>
    Public Shared Function Save(k_flow As TutorialFlow) As List(Of String)
        Try
            If k_flow.Id.Length = 0 OrElse k_flow.Id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
                Throw New ArgumentException("The tutorial id '" & k_flow.Id & "' is not usable as a file name.")
            End If
            ' What is written must read back: refuse a tutorial the parser would refuse.
            Dim text As String = k_flow.ToMarkdown()
            TutorialFlow.Parse(text)
            Dim written As New List(Of String)()
            For Each folder As String In {RuntimeFolder(), SourceFolder()}
                If String.IsNullOrEmpty(folder) Then Continue For
                Directory.CreateDirectory(folder)
                Dim target As String = Path.Combine(folder, k_flow.Id & ".md")
                File.WriteAllText(target, text, New UTF8Encoding(False))
                written.Add(target)
            Next
            Return written
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialStore.Save", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The files <see cref="Delete"/> would remove for <paramref name="k_id"/>: the runtime copy and, when K-BOT runs
    ''' from a repository build, the repository copy -- only those that exist.
    ''' </summary>
    Public Shared Function FilesOf(k_id As String) As List(Of String)
        Try
            If String.IsNullOrEmpty(k_id) OrElse k_id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
                Throw New ArgumentException("The tutorial id '" & k_id & "' is not usable as a file name.")
            End If
            Dim found As New List(Of String)()
            For Each k_folder As String In {RuntimeFolder(), SourceFolder()}
                If String.IsNullOrEmpty(k_folder) Then Continue For
                Dim k_path As String = Path.Combine(k_folder, k_id & ".md")
                If File.Exists(k_path) Then found.Add(k_path)
            Next
            Return found
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialStore.FilesOf", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The other tutorials whose step texts link to <paramref name="k_id"/> (<c>&lt;link tutorial="id"&gt;</c>, slice
    ''' 000T-09), one line each for the operator: the tutorial and the step. Both folders are read; a tutorial present in
    ''' both is listed once. A file the parser refuses is listed whole.
    ''' </summary>
    Public Shared Function LinksTo(k_id As String) As List(Of String)
        Try
            Dim found As New List(Of String)()
            If String.IsNullOrEmpty(k_id) Then Return found
            Dim k_link As New Text.RegularExpressions.Regex(
                "<(?:link|a)\b[^>]*?\btutorial\s*=\s*[""']" & Text.RegularExpressions.Regex.Escape(k_id) & "[""']",
                Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.CultureInvariant)
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each k_folder As String In {RuntimeFolder(), SourceFolder()}
                If String.IsNullOrEmpty(k_folder) OrElse Not Directory.Exists(k_folder) Then Continue For
                For Each k_file As String In Directory.EnumerateFiles(k_folder, "*.md")
                    Dim k_name As String = Path.GetFileNameWithoutExtension(k_file)
                    If String.Equals(k_name, k_id, StringComparison.OrdinalIgnoreCase) OrElse Not seen.Add(k_name) Then Continue For
                    Dim k_raw As String = File.ReadAllText(k_file, Encoding.UTF8)
                    If Not k_link.IsMatch(k_raw) Then Continue For
                    Try
                        Dim k_flow As TutorialFlow = TutorialFlow.Parse(k_raw)
                        Dim k_steps As List(Of String) = k_flow.Steps.Where(Function(k_step) k_link.IsMatch(k_step.Text)).
                            Select(Function(k_step) k_step.Title).ToList()
                        If k_steps.Count = 0 Then
                            found.Add("«" & k_flow.Title & "» (" & k_name & ")")
                        Else
                            For Each k_title As String In k_steps
                                found.Add("«" & k_flow.Title & "» (" & k_name & "), pasul «" & k_title & "»")
                            Next
                        End If
                    Catch ex As ArgumentException
                        GlobalErrorLog.Write("TutorialStore.LinksTo", ex)
                        found.Add("«" & k_name & "» (fișierul nu se poate citi, dar conține legătura)")
                    End Try
                Next
            Next
            Return found
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialStore.LinksTo", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Removes the tutorial <paramref name="k_id"/> from every folder <see cref="Save"/> writes to. The files go to the
    ''' Recycle Bin, so a wrong click can be undone; returns the paths removed.
    ''' </summary>
    Public Shared Function Delete(k_id As String) As List(Of String)
        Try
            Dim k_files As List(Of String) = FilesOf(k_id)
            For Each k_file As String In k_files
                FileSystem.DeleteFile(k_file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)
            Next
            Return k_files
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialStore.Delete", ex)
            Throw
        End Try
    End Function

End Class
