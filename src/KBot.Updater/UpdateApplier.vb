Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Compression
Imports System.Security.Cryptography
Imports System.Threading

''' <summary>
''' Writes a K-BOT package (the Release zip) over an installation folder (slice 0067).
'''
''' <para><b>Rules, in order:</b></para>
''' <list type="number">
''' <item>The zip from publish-release.ps1 has ONE top folder (<c>KBot_Release_&lt;stamp&gt;/</c>);
''' it is stripped. A zip without a top folder is applied as-is.</item>
''' <item><c>Logs\</c> is never touched (<see cref="PreservedFolders"/>): the operator's journals
''' outlive every update.</item>
''' <item>Only files IN the package are written. Nothing already in the folder is deleted, so
''' the data folders (<c>Asociere\</c>, <c>WorkflowResults\</c>, <c>Extrase\</c>, ...) and
''' <c>kbot_paths.json</c> survive untouched. A file removed from the product stays behind
''' as a harmless orphan.</item>
''' <item>Each file lands next to its target as <c>*.kbot-new</c> and is then moved over it, so a
''' file is either the old one or the new one, never half of each.</item>
''' <item>A locked file (Windows keeps a DLL of a closing process for a moment) is retried;
''' after the last attempt the failure names the file and stops the update.</item>
''' <item>Zip-slip: an entry that resolves outside the target folder aborts everything.</item>
''' </list>
''' </summary>
Friend NotInheritable Class UpdateApplier

    ''' <summary>Top-level folders of the installation that the package must not overwrite.</summary>
    Public Shared ReadOnly PreservedFolders As String() = {"Logs"}

    Private Const NEW_SUFFIX As String = ".kbot-new"

    Public NotInheritable Class ApplyResult
        Public Property Written As Integer
        Public Property Skipped As Integer
        Public Property TopFolder As String
    End Class

    Private Sub New()
    End Sub

    ''' <summary>SHA-256 of a file, upper-case hex.</summary>
    Public Shared Function ComputeSha256(filePath As String) As String
        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Return Convert.ToHexString(SHA256.HashData(fs))
        End Using
    End Function

    ''' <summary>Throws <see cref="InvalidDataException"/> when the file does not match.</summary>
    Public Shared Sub VerifySha256(filePath As String, expectedHex As String)
        Dim actual As String = ComputeSha256(filePath)
        If Not String.Equals(actual, expectedHex.Trim(), StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException("Pachetul nu corespunde sumei de control așteptate (" & Path.GetFileName(filePath) & ").")
        End If
    End Sub

    ''' <summary>
    ''' True when a file can be created in <paramref name="dir"/>. False on access denied --
    ''' the caller relaunches elevated. Any other failure (folder missing, disk gone) throws.
    ''' </summary>
    Public Shared Function IsWritable(dir As String) As Boolean
        Dim probe As String = Path.Combine(dir, ".kbot-write-probe-" & Guid.NewGuid().ToString("N"))
        Try
            Using New FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)
            End Using
            Return True
        Catch ex As UnauthorizedAccessException
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The common top folder of every entry, or empty when entries sit at the root or do
    ''' not share one. Exposed for the tests and the log.
    ''' </summary>
    Public Shared Function DetectTopFolder(entryNames As IEnumerable(Of String)) As String
        Dim top As String = Nothing
        Dim any As Boolean = False
        For Each rawName As String In entryNames
            any = True
            Dim name As String = NormalizeEntryName(rawName)
            Dim slash As Integer = name.IndexOf("/"c)
            If slash <= 0 Then Return String.Empty      ' a root-level entry: nothing to strip
            Dim first As String = name.Substring(0, slash)
            If top Is Nothing Then
                top = first
            ElseIf Not String.Equals(top, first, StringComparison.Ordinal) Then
                Return String.Empty
            End If
        Next
        If Not any Then Return String.Empty
        Return top
    End Function

    ''' <summary>
    ''' Applies <paramref name="zipPath"/> onto <paramref name="targetDir"/>.
    ''' <paramref name="progress"/> gets (done, total) per entry; <paramref name="log"/> one
    ''' line per notable event. <paramref name="attempts"/> / <paramref name="retryDelayMs"/>
    ''' govern the locked-file retry.
    ''' </summary>
    Public Shared Function Apply(zipPath As String, targetDir As String,
                                 log As Action(Of String), progress As Action(Of Integer, Integer),
                                 ct As CancellationToken,
                                 Optional attempts As Integer = 10, Optional retryDelayMs As Integer = 1000) As ApplyResult
        If String.IsNullOrWhiteSpace(zipPath) Then Throw New ArgumentException("zipPath")
        If String.IsNullOrWhiteSpace(targetDir) Then Throw New ArgumentException("targetDir")
        If attempts < 1 Then Throw New ArgumentOutOfRangeException(NameOf(attempts))
        If Not File.Exists(zipPath) Then Throw New FileNotFoundException("Pachetul de actualizare lipsește.", zipPath)

        Directory.CreateDirectory(targetDir)
        Dim targetFull As String = Path.GetFullPath(targetDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar
        Dim result As New ApplyResult()

        Using archive As ZipArchive = ZipFile.OpenRead(zipPath)
            Dim names As New List(Of String)(archive.Entries.Count)
            For Each e As ZipArchiveEntry In archive.Entries
                names.Add(NormalizeEntryName(e.FullName))
            Next
            Dim top As String = DetectTopFolder(names)
            result.TopFolder = top
            Dim prefix As String = If(top.Length > 0, top & "/", String.Empty)
            Say(log, "Pachet: " & Path.GetFileName(zipPath) & " (" & archive.Entries.Count & " intrări" &
                     If(prefix.Length > 0, ", folderul de sus «" & top & "» se elimină", "") & ")")

            Dim total As Integer = archive.Entries.Count
            Dim done As Integer = 0
            For Each entry As ZipArchiveEntry In archive.Entries
                ct.ThrowIfCancellationRequested()
                done += 1
                progress?.Invoke(done, total)

                Dim rel As String = NormalizeEntryName(entry.FullName)
                If prefix.Length > 0 AndAlso rel.StartsWith(prefix, StringComparison.Ordinal) Then
                    rel = rel.Substring(prefix.Length)
                End If
                If rel.Length = 0 OrElse rel.EndsWith("/", StringComparison.Ordinal) Then
                    Continue For                                   ' directory entry
                End If

                Dim firstSegment As String = rel.Split("/"c)(0)
                If IsPreserved(firstSegment) Then
                    result.Skipped += 1
                    Continue For
                End If

                Dim relOs As String = rel.Replace("/"c, Path.DirectorySeparatorChar)
                Dim destination As String = Path.GetFullPath(Path.Combine(targetFull, relOs))
                If Not destination.StartsWith(targetFull, StringComparison.OrdinalIgnoreCase) Then
                    Throw New IOException("Intrare în afara folderului țintă (zip-slip): " & entry.FullName)
                End If

                Directory.CreateDirectory(Path.GetDirectoryName(destination))
                WriteWithRetry(entry, destination, attempts, retryDelayMs, log, ct)
                result.Written += 1
            Next
        End Using

        Say(log, "Scrise " & result.Written & " fișiere, sărite " & result.Skipped & " (" & String.Join(", ", PreservedFolders) & "\).")
        Return result
    End Function

    ''' <summary>
    ''' Entry names with "/" only. A zip written under Windows PowerShell 5.1 by
    ''' ZipFile.CreateFromDirectory carries BACKSLASHES ("KBot_Release_x\KBot.App.exe");
    ''' read as-is, no top folder would be found and the whole package would land in
    ''' a subfolder of the target (seen 20.09.2026). publish-release.ps1 no longer
    ''' writes such names, but the package is not the only thing that must be right.
    ''' </summary>
    Public Shared Function NormalizeEntryName(name As String) As String
        If String.IsNullOrEmpty(name) Then Return String.Empty
        Return name.Replace("\"c, "/"c)
    End Function

    Private Shared Function IsPreserved(folder As String) As Boolean
        For Each p As String In PreservedFolders
            If String.Equals(p, folder, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    ' Extract next to the target, then move over it. The move is what can hit a lock
    ' (the old file still mapped by a process that is closing); the extraction never
    ' does, so a failure here leaves the old file intact and the .kbot-new beside it.
    Private Shared Sub WriteWithRetry(entry As ZipArchiveEntry, destination As String,
                                      attempts As Integer, retryDelayMs As Integer,
                                      log As Action(Of String), ct As CancellationToken)
        Dim staging As String = destination & NEW_SUFFIX
        entry.ExtractToFile(staging, overwrite:=True)
        Dim lastError As Exception = Nothing
        For attempt As Integer = 1 To attempts
            ct.ThrowIfCancellationRequested()
            Try
                File.Move(staging, destination, overwrite:=True)
                Return
            Catch ex As IOException
                lastError = ex
            Catch ex As UnauthorizedAccessException
                lastError = ex
            End Try
            If attempt < attempts Then
                Say(log, "Fișier ocupat, reîncerc (" & attempt & "/" & attempts & "): " & Path.GetFileName(destination))
                Thread.Sleep(retryDelayMs)
            End If
        Next
        Try
            File.Delete(staging)
        Catch ex As Exception
            ' The stale .kbot-new is reported with the real failure below; a second exception would hide it.
            Say(log, "Nu s-a putut șterge " & staging & ": " & ex.Message)
        End Try
        Throw New IOException("Fișierul nu a putut fi înlocuit după " & attempts & " încercări: " & destination, lastError)
    End Sub

    Private Shared Sub Say(log As Action(Of String), text As String)
        log?.Invoke(text)
    End Sub
End Class
