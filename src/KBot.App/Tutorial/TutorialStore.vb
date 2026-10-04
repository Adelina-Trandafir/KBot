Option Strict On
Imports System.IO
Imports System.Text
Imports KBot.Common

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

End Class
