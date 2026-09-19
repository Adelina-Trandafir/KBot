Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Compression
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports Xunit
Imports KBot.Updater

' Slice 0067: the file-writing heart of the updater, against real zips in a temp
' folder. Pinned: the top-folder strip (the Release zip has one), Logs\ untouched,
' extra files kept, existing files overwritten, zip-slip refused, locked-file retry.
Public Class UpdateApplierTests
    Implements IDisposable

    Private ReadOnly _root As String

    Public Sub New()
        _root = Path.Combine(Path.GetTempPath(), "kbot_applier_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_root)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            If Directory.Exists(_root) Then Directory.Delete(_root, recursive:=True)
        Catch
            ' temp cleanup only
        End Try
    End Sub

    Private Function MakeZip(entries As IDictionary(Of String, String)) As String
        Dim zipPath As String = Path.Combine(_root, "pkg_" & Guid.NewGuid().ToString("N") & ".zip")
        Using zip As ZipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create)
            For Each kv In entries
                Dim e As ZipArchiveEntry = zip.CreateEntry(kv.Key)
                If kv.Value IsNot Nothing Then
                    Using w As New StreamWriter(e.Open(), Encoding.UTF8)
                        w.Write(kv.Value)
                    End Using
                End If
            Next
        End Using
        Return zipPath
    End Function

    Private Function Target() As String
        Dim t As String = Path.Combine(_root, "KBOT")
        Directory.CreateDirectory(t)
        Return t
    End Function

    Private Shared Function ReadText(path As String) As String
        Return File.ReadAllText(path, Encoding.UTF8)
    End Function

    ' ---------------------------------------------------------------- DetectTopFolder

    <Fact>
    Public Sub DetectTopFolder_FindsTheOneCommonFolder()
        Assert.Equal("KBot_Release_1", UpdateApplier.DetectTopFolder({"KBot_Release_1/", "KBot_Release_1/a.dll", "KBot_Release_1/Logs/_keep.txt"}))
    End Sub

    <Theory>
    <InlineData(New String() {"a.dll", "b.dll"})>
    <InlineData(New String() {"x/a.dll", "y/b.dll"})>
    <InlineData(New String() {"x/a.dll", "b.dll"})>
    <InlineData(New String() {})>
    Public Sub DetectTopFolder_EmptyWhenNothingToStrip(names As String())
        Assert.Equal(String.Empty, UpdateApplier.DetectTopFolder(names))
    End Sub

    ' ---------------------------------------------------------------- Apply

    <Fact>
    Public Sub Apply_StripsTopFolder_WritesFiles_AndSkipsLogs()
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {
            {"KBot_Release_20260918/", Nothing},
            {"KBot_Release_20260918/KBot.App.exe", "new app"},
            {"KBot_Release_20260918/KBot.App.dll", "new dll"},
            {"KBot_Release_20260918/Workflows/adlop - Conectare.wfl", "<wfl/>"},
            {"KBot_Release_20260918/Logs/_keep.txt", "keep"},
            {"KBot_Release_20260918/Migrare/KBot.Migrator.exe", "mig"}})
        Dim t = Target()
        Dim lines As New List(Of String)()

        Dim r = UpdateApplier.Apply(zipPath, t, Sub(s) lines.Add(s), Nothing, CancellationToken.None, attempts:=1, retryDelayMs:=0)

        Assert.Equal("KBot_Release_20260918", r.TopFolder)
        Assert.Equal(4, r.Written)
        Assert.Equal(1, r.Skipped)
        Assert.Equal("new app", ReadText(Path.Combine(t, "KBot.App.exe")))
        Assert.Equal("new dll", ReadText(Path.Combine(t, "KBot.App.dll")))
        Assert.Equal("<wfl/>", ReadText(Path.Combine(t, "Workflows", "adlop - Conectare.wfl")))
        Assert.Equal("mig", ReadText(Path.Combine(t, "Migrare", "KBot.Migrator.exe")))
        Assert.False(Directory.Exists(Path.Combine(t, "Logs")))
        Assert.False(Directory.Exists(Path.Combine(t, "KBot_Release_20260918")))
        Assert.Empty(Directory.GetFiles(t, "*.kbot-new", SearchOption.AllDirectories))
        Assert.Contains(lines, Function(s) s.Contains("Scrise 4"))
    End Sub

    <Fact>
    Public Sub Apply_WithoutTopFolder_WritesAtRoot()
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {{"KBot.App.exe", "root app"}})
        Dim t = Target()
        Dim r = UpdateApplier.Apply(zipPath, t, Nothing, Nothing, CancellationToken.None, attempts:=1, retryDelayMs:=0)
        Assert.Equal(String.Empty, r.TopFolder)
        Assert.Equal("root app", ReadText(Path.Combine(t, "KBot.App.exe")))
    End Sub

    <Fact>
    Public Sub Apply_OverwritesExisting_KeepsExtraFiles_AndExistingLogs()
        Dim t = Target()
        File.WriteAllText(Path.Combine(t, "KBot.App.dll"), "old dll")
        File.WriteAllText(Path.Combine(t, "kbot_paths.json"), "{""operator"":true}")
        Directory.CreateDirectory(Path.Combine(t, "Asociere"))
        File.WriteAllText(Path.Combine(t, "Asociere", "ANG1.json"), "dossier")
        Directory.CreateDirectory(Path.Combine(t, "Logs"))
        File.WriteAllText(Path.Combine(t, "Logs", "harness_errors.log"), "errors")
        File.WriteAllText(Path.Combine(t, "Removed.dll"), "gone from product")

        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {
            {"R/KBot.App.dll", "new dll"},
            {"R/Logs/_keep.txt", "keep"}})

        UpdateApplier.Apply(zipPath, t, Nothing, Nothing, CancellationToken.None, attempts:=1, retryDelayMs:=0)

        Assert.Equal("new dll", ReadText(Path.Combine(t, "KBot.App.dll")))
        Assert.Equal("{""operator"":true}", ReadText(Path.Combine(t, "kbot_paths.json")))
        Assert.Equal("dossier", ReadText(Path.Combine(t, "Asociere", "ANG1.json")))
        Assert.Equal("errors", ReadText(Path.Combine(t, "Logs", "harness_errors.log")))
        Assert.False(File.Exists(Path.Combine(t, "Logs", "_keep.txt")))
        Assert.Equal("gone from product", ReadText(Path.Combine(t, "Removed.dll")))
    End Sub

    <Fact>
    Public Sub Apply_LogsFolderMatchIsCaseInsensitive_AndTopLevelOnly()
        Dim t = Target()
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {
            {"R/LOGS/x.log", "no"},
            {"R/Migrare/Logs/y.log", "yes, nested Logs is part of the product tree"}})
        Dim r = UpdateApplier.Apply(zipPath, t, Nothing, Nothing, CancellationToken.None, attempts:=1, retryDelayMs:=0)
        Assert.Equal(1, r.Skipped)
        Assert.Equal(1, r.Written)
        Assert.False(File.Exists(Path.Combine(t, "LOGS", "x.log")))
        Assert.True(File.Exists(Path.Combine(t, "Migrare", "Logs", "y.log")))
    End Sub

    <Fact>
    Public Sub Apply_ReportsProgressForEveryEntry()
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {
            {"R/", Nothing}, {"R/a.dll", "a"}, {"R/b.dll", "b"}})
        Dim seen As New List(Of (Integer, Integer))()
        UpdateApplier.Apply(zipPath, Target(), Nothing, Sub(d, n) seen.Add((d, n)), CancellationToken.None, attempts:=1, retryDelayMs:=0)
        Assert.Equal({(1, 3), (2, 3), (3, 3)}, seen.ToArray())
    End Sub

    <Fact>
    Public Sub Apply_RefusesZipSlip_AndWritesNothingOutside()
        Dim t = Target()
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {{"R/../escaped.txt", "evil"}})
        Assert.Throws(Of IOException)(
            Sub() UpdateApplier.Apply(zipPath, t, Nothing, Nothing, CancellationToken.None, attempts:=1, retryDelayMs:=0))
        Assert.False(File.Exists(Path.Combine(_root, "escaped.txt")))
        Assert.False(File.Exists(Path.Combine(t, "escaped.txt")))
    End Sub

    <Fact>
    Public Sub Apply_LockedFile_RetriesThenFailsNamingIt_AndLeavesOldFileIntact()
        Dim t = Target()
        Dim locked As String = Path.Combine(t, "KBot.App.dll")
        File.WriteAllText(locked, "old dll")
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {{"R/KBot.App.dll", "new dll"}})
        Dim lines As New List(Of String)()

        Using New FileStream(locked, FileMode.Open, FileAccess.Read, FileShare.None)
            Dim ex = Assert.Throws(Of IOException)(
                Sub() UpdateApplier.Apply(zipPath, t, Sub(s) lines.Add(s), Nothing, CancellationToken.None, attempts:=3, retryDelayMs:=1))
            Assert.Contains("KBot.App.dll", ex.Message)
            Assert.Contains("3", ex.Message)
        End Using

        Assert.Equal("old dll", ReadText(locked))
        Assert.Equal(2, lines.FindAll(Function(s) s.Contains("reîncerc")).Count)
        Assert.False(File.Exists(locked & ".kbot-new"))
    End Sub

    <Fact>
    Public Sub Apply_LockReleasedDuringRetry_Succeeds()
        Dim t = Target()
        Dim locked As String = Path.Combine(t, "KBot.App.dll")
        File.WriteAllText(locked, "old dll")
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {{"R/KBot.App.dll", "new dll"}})

        Dim handle As New FileStream(locked, FileMode.Open, FileAccess.Read, FileShare.None)
        Dim releaser As New Thread(Sub()
                                       Thread.Sleep(150)
                                       handle.Dispose()
                                   End Sub)
        releaser.Start()
        Try
            Dim r = UpdateApplier.Apply(zipPath, t, Nothing, Nothing, CancellationToken.None, attempts:=50, retryDelayMs:=20)
            Assert.Equal(1, r.Written)
            Assert.Equal("new dll", ReadText(locked))
        Finally
            releaser.Join()
            handle.Dispose()
        End Try
    End Sub

    <Fact>
    Public Sub Apply_MissingZip_ThrowsFileNotFound()
        Assert.Throws(Of FileNotFoundException)(
            Sub() UpdateApplier.Apply(Path.Combine(_root, "nope.zip"), Target(), Nothing, Nothing, CancellationToken.None))
    End Sub

    <Fact>
    Public Sub Apply_Cancelled_StopsBeforeWriting()
        Dim zipPath = MakeZip(New Dictionary(Of String, String) From {{"R/a.dll", "a"}})
        Dim t = Target()
        Using cts As New CancellationTokenSource()
            cts.Cancel()
            Assert.Throws(Of OperationCanceledException)(
                Sub() UpdateApplier.Apply(zipPath, t, Nothing, Nothing, cts.Token))
        End Using
        Assert.False(File.Exists(Path.Combine(t, "a.dll")))
    End Sub

    ' ---------------------------------------------------------------- Sha / IsWritable

    <Fact>
    Public Sub VerifySha256_AcceptsMatch_AnyCase_RejectsMismatch()
        Dim p As String = Path.Combine(_root, "blob.bin")
        File.WriteAllBytes(p, {1, 2, 3, 4})
        Dim hex As String = Convert.ToHexString(SHA256.HashData(New Byte() {1, 2, 3, 4}))
        UpdateApplier.VerifySha256(p, hex.ToLowerInvariant())
        UpdateApplier.VerifySha256(p, " " & hex.ToUpperInvariant() & " ")
        Assert.Throws(Of InvalidDataException)(Sub() UpdateApplier.VerifySha256(p, New String("0"c, 64)))
    End Sub

    <Fact>
    Public Sub IsWritable_TrueForTempFolder_AndLeavesNoProbeBehind()
        Dim t = Target()
        Assert.True(UpdateApplier.IsWritable(t))
        Assert.Empty(Directory.GetFiles(t))
    End Sub
End Class
