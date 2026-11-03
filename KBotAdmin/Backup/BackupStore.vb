Option Strict On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Text.RegularExpressions

''' <summary>The one file in the store that is not encrypted: what is needed to derive the keys and to check the passphrase.</summary>
Public Class BackupStoreHeader
    <JsonPropertyName("magic")> Public Property Magic As String = "KBK1"
    <JsonPropertyName("kdf")> Public Property Kdf As String = "pbkdf2-sha256"
    <JsonPropertyName("iterations")> Public Property Iterations As Integer
    <JsonPropertyName("salt")> Public Property Salt As String = String.Empty
    <JsonPropertyName("check")> Public Property Check As String = String.Empty
End Class

Public NotInheritable Class BackupPutResult
    Public Property Name As String = String.Empty
    Public Property IsNew As Boolean
    ''' <summary>Bytes written to disk; 0 when the object was already there.</summary>
    Public Property StoredBytes As Long
End Class

Public NotInheritable Class BackupSnapshotListing
    Public ReadOnly Property Items As New List(Of SnapshotManifest)()
    ''' <summary>Snapshot files that could not be opened (damaged, or sealed with another key), one readable line each.</summary>
    Public ReadOnly Property Problems As New List(Of String)()
End Class

Public NotInheritable Class BackupVerifyReport
    Public Property Objects As Integer
    Public Property Bytes As Long
    Public Property Snapshots As Integer
    Public Property Orphans As Integer
    Public ReadOnly Property Missing As New List(Of String)()
    Public ReadOnly Property Damaged As New List(Of String)()
    Public ReadOnly Property IsClean As Boolean
        Get
            Return Missing.Count = 0 AndAlso Damaged.Count = 0
        End Get
    End Property
End Class

''' <summary>
''' The folder of the local backup (slice ME02-03, docs/ME02_FORMAT.md): objects named by HMAC of their content, and
''' snapshots that list the objects of one database at one moment. Everything but the header is encrypted.
''' </summary>
Public NotInheritable Class BackupStore

    Public Const HeaderFile As String = "store.kbk"
    Private Const CheckLabel As String = "check"
    Private Shared ReadOnly RxObject As New Regex("^[0-9a-f]{64}$", RegexOptions.Compiled)
    Private Shared ReadOnly RxSnapshot As New Regex("^[0-9a-f]{32}$", RegexOptions.Compiled)
    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {.WriteIndented = True}

    Private ReadOnly _root As String
    Private ReadOnly _keys As BackupKeys
    Private ReadOnly _storeId As String

    Private Sub New(k_root As String, k_keys As BackupKeys, k_storeId As String)
        _root = k_root
        _keys = k_keys
        _storeId = k_storeId
    End Sub

    Public ReadOnly Property Root As String
        Get
            Return _root
        End Get
    End Property

    ''' <summary>12 hex characters from the salt: it names this store's key file on this PC.</summary>
    Public ReadOnly Property StoreId As String
        Get
            Return _storeId
        End Get
    End Property

    Public ReadOnly Property Keys As BackupKeys
        Get
            Return _keys
        End Get
    End Property

    ' ------------------------------------------------------------------
    ' Create / open
    ' ------------------------------------------------------------------

    Public Shared Function Exists(k_root As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(k_root) AndAlso File.Exists(Path.Combine(k_root, HeaderFile))
    End Function

    ''' <summary>The id of the store in this folder, read from the header without any key; empty when there is none.</summary>
    Public Shared Function ReadStoreId(k_root As String) As String
        If Not Exists(k_root) Then Return String.Empty
        Return StoreIdOf(Convert.FromBase64String(ReadHeader(k_root).Salt))
    End Function

    Public Shared Function Create(k_root As String, k_passphrase As String) As BackupStore
        Try
            If String.IsNullOrWhiteSpace(k_root) Then Throw New BackupException("Alegeți dosarul copiilor locale.")
            If Exists(k_root) Then Throw New BackupException("În acest dosar există deja o copie locală.")
            Directory.CreateDirectory(k_root)
            Dim k_salt As Byte() = BackupCrypto.NewSalt()
            Dim k_keys As BackupKeys = BackupCrypto.DeriveKeys(k_passphrase, k_salt, BackupCrypto.DefaultIterations)
            Dim k_header As New BackupStoreHeader With {
                .Iterations = BackupCrypto.DefaultIterations,
                .Salt = Convert.ToBase64String(k_salt),
                .Check = Convert.ToBase64String(BackupCrypto.Seal(BackupCrypto.KindCheck, Encoding.UTF8.GetBytes("kbk1-check"), CheckLabel, k_keys))
            }
            WriteAtomic(Path.Combine(k_root, HeaderFile), Encoding.UTF8.GetBytes(JsonSerializer.Serialize(k_header, JsonOptions)))
            Return New BackupStore(k_root, k_keys, StoreIdOf(k_salt))
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.Create", ex)
            Throw
        End Try
    End Function

    Public Shared Function Open(k_root As String, k_passphrase As String) As BackupStore
        Try
            Dim k_header As BackupStoreHeader = ReadHeader(k_root)
            Dim k_salt As Byte() = Convert.FromBase64String(k_header.Salt)
            Return OpenWithKeys(k_root, BackupCrypto.DeriveKeys(k_passphrase, k_salt, k_header.Iterations))
        Catch ex As BackupException
            GlobalErrorLog.Write("BackupStore.Open", ex)
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.Open", ex)
            Throw
        End Try
    End Function

    ''' <summary>Opens with keys already derived (the passphrase typed earlier, or the key file protected by Windows).</summary>
    Public Shared Function OpenWithKeys(k_root As String, k_keys As BackupKeys) As BackupStore
        Try
            Dim k_header As BackupStoreHeader = ReadHeader(k_root)
            Try
                BackupCrypto.Open(BackupCrypto.KindCheck, Convert.FromBase64String(k_header.Check), CheckLabel, k_keys)
            Catch ex As BackupException
                Throw New BackupException("Parola nu se potrivește cu această copie locală.", ex)
            End Try
            Return New BackupStore(k_root, k_keys, StoreIdOf(Convert.FromBase64String(k_header.Salt)))
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.OpenWithKeys", ex)
            Throw
        End Try
    End Function

    Private Shared Function ReadHeader(k_root As String) As BackupStoreHeader
        Dim k_path As String = Path.Combine(k_root, HeaderFile)
        If Not File.Exists(k_path) Then Throw New BackupException("În dosarul ales nu există o copie locală (lipsește " & HeaderFile & ").")
        Dim k_header As BackupStoreHeader = JsonSerializer.Deserialize(Of BackupStoreHeader)(File.ReadAllText(k_path, Encoding.UTF8))
        If k_header Is Nothing OrElse k_header.Magic <> "KBK1" OrElse k_header.Salt = String.Empty OrElse k_header.Iterations <= 0 Then
            Throw New BackupException("Antetul copiei locale nu poate fi citit sau e de un alt format.")
        End If
        Return k_header
    End Function

    Private Shared Function StoreIdOf(k_salt As Byte()) As String
        Return Convert.ToHexString(SHA256.HashData(k_salt)).Substring(0, 12).ToLowerInvariant()
    End Function

    ' ------------------------------------------------------------------
    ' Objects
    ' ------------------------------------------------------------------

    Private Function ObjectPath(k_name As String) As String
        If Not RxObject.IsMatch(k_name) Then Throw New BackupException("Nume de obiect nepermis în copia locală.")
        Return Path.Combine(_root, "o", k_name.Substring(0, 2), k_name.Substring(2))
    End Function

    Public Function HasObject(k_name As String) As Boolean
        Return File.Exists(ObjectPath(k_name))
    End Function

    ''' <summary>Stores a content once: an object with the same content is not written again.</summary>
    Public Function PutObject(k_plain As Byte()) As BackupPutResult
        Try
            Dim k_name As String = BackupCrypto.ObjectName(k_plain, _keys)
            Dim k_path As String = ObjectPath(k_name)
            If File.Exists(k_path) Then Return New BackupPutResult With {.Name = k_name, .IsNew = False, .StoredBytes = 0}
            Dim k_file As Byte() = BackupCrypto.Seal(BackupCrypto.KindObject, BackupCrypto.Pack(k_plain), k_name, _keys)
            WriteAtomic(k_path, k_file)
            Return New BackupPutResult With {.Name = k_name, .IsNew = True, .StoredBytes = k_file.Length}
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.PutObject", ex)
            Throw
        End Try
    End Function

    Public Function GetObject(k_name As String) As Byte()
        Try
            Dim k_path As String = ObjectPath(k_name)
            If Not File.Exists(k_path) Then Throw New BackupException("Lipsește un obiect al copiei locale (" & k_name.Substring(0, 8) & "...).")
            Return BackupCrypto.Unpack(BackupCrypto.Open(BackupCrypto.KindObject, File.ReadAllBytes(k_path), k_name, _keys))
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.GetObject", ex)
            Throw
        End Try
    End Function

    Public Function ObjectSize(k_name As String) As Long
        Dim k_path As String = ObjectPath(k_name)
        Return If(File.Exists(k_path), New FileInfo(k_path).Length, 0L)
    End Function

    Public Function DeleteObject(k_name As String) As Long
        Try
            Dim k_path As String = ObjectPath(k_name)
            If Not File.Exists(k_path) Then Return 0
            Dim k_size As Long = New FileInfo(k_path).Length
            File.Delete(k_path)
            Return k_size
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.DeleteObject", ex)
            Throw
        End Try
    End Function

    Public Iterator Function ObjectNames() As IEnumerable(Of String)
        Dim k_dir As String = Path.Combine(_root, "o")
        If Not Directory.Exists(k_dir) Then Return
        For Each k_sub As String In Directory.EnumerateDirectories(k_dir)
            Dim k_prefix As String = Path.GetFileName(k_sub)
            For Each k_file As String In Directory.EnumerateFiles(k_sub)
                Dim k_name As String = k_prefix & Path.GetFileName(k_file)
                If RxObject.IsMatch(k_name) Then Yield k_name
            Next
        Next
    End Function

    ' ------------------------------------------------------------------
    ' Snapshots
    ' ------------------------------------------------------------------

    Private Function SnapshotPath(k_id As String) As String
        If Not RxSnapshot.IsMatch(k_id) Then Throw New BackupException("Identificator de instantaneu nepermis.")
        Return Path.Combine(_root, "s", k_id & ".snp")
    End Function

    Public Shared Function NewSnapshotId() As String
        Dim k_bytes(15) As Byte
        RandomNumberGenerator.Fill(k_bytes)
        Return Convert.ToHexString(k_bytes).ToLowerInvariant()
    End Function

    ''' <summary>Writes the snapshot file. Always the LAST write of a run: every object it names is already on disk.</summary>
    Public Function PutSnapshot(k_manifest As SnapshotManifest) As Long
        Try
            Dim k_file As Byte() = BackupCrypto.Seal(BackupCrypto.KindSnapshot, BackupCrypto.Pack(k_manifest.ToJsonBytes()), "s:" & k_manifest.Id, _keys)
            WriteAtomic(SnapshotPath(k_manifest.Id), k_file)
            Return k_file.Length
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.PutSnapshot", ex)
            Throw
        End Try
    End Function

    Public Function ReadSnapshot(k_id As String) As SnapshotManifest
        Try
            Dim k_path As String = SnapshotPath(k_id)
            If Not File.Exists(k_path) Then Throw New BackupException("Instantaneul cerut nu mai există.")
            Dim k_plain As Byte() = BackupCrypto.Unpack(BackupCrypto.Open(BackupCrypto.KindSnapshot, File.ReadAllBytes(k_path), "s:" & k_id, _keys))
            Dim k_manifest As SnapshotManifest = SnapshotManifest.FromJsonBytes(k_plain)
            If k_manifest.Id <> k_id Then Throw New BackupException("Un instantaneu are alt identificator decât numele fișierului.")
            Return k_manifest
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.ReadSnapshot", ex)
            Throw
        End Try
    End Function

    ''' <summary>Every snapshot that opens, oldest first; the ones that do not are listed in Problems, not hidden.</summary>
    Public Function ListSnapshots() As BackupSnapshotListing
        Dim k_listing As New BackupSnapshotListing()
        Try
            Dim k_dir As String = Path.Combine(_root, "s")
            If Not Directory.Exists(k_dir) Then Return k_listing
            For Each k_file As String In Directory.EnumerateFiles(k_dir, "*.snp")
                Dim k_id As String = Path.GetFileNameWithoutExtension(k_file)
                Try
                    k_listing.Items.Add(ReadSnapshot(k_id))
                Catch ex As Exception When TypeOf ex Is BackupException OrElse TypeOf ex Is IOException OrElse TypeOf ex Is JsonException
                    k_listing.Problems.Add("Instantaneul " & k_id.Substring(0, 8) & ": " & ex.Message)
                End Try
            Next
            k_listing.Items.Sort(Function(k_a, k_b) k_a.FinishedAt.CompareTo(k_b.FinishedAt))
            Return k_listing
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.ListSnapshots", ex)
            Throw
        End Try
    End Function

    ''' <summary>The newest snapshot of a database; Nothing when there is none.</summary>
    Public Function LatestSnapshot(k_db As String) As SnapshotManifest
        Dim k_best As SnapshotManifest = Nothing
        For Each k_item As SnapshotManifest In ListSnapshots().Items
            If String.Equals(k_item.Db, k_db, StringComparison.OrdinalIgnoreCase) Then k_best = k_item
        Next
        Return k_best
    End Function

    Public Sub DeleteSnapshot(k_id As String)
        Try
            Dim k_path As String = SnapshotPath(k_id)
            If File.Exists(k_path) Then File.Delete(k_path)
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.DeleteSnapshot", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Every object name a snapshot points at.</summary>
    Public Shared Function ReferencedObjects(k_manifest As SnapshotManifest) As IEnumerable(Of String)
        Dim k_names As New List(Of String)()
        For Each k_table As BackupTable In k_manifest.Tables
            If k_table.DdlObject <> String.Empty Then k_names.Add(k_table.DdlObject)
            For Each k_page As BackupPage In k_table.Pages
                k_names.Add(k_page.ObjectName)
            Next
        Next
        For Each k_extra As BackupExtra In k_manifest.Extras
            If k_extra.DdlObject <> String.Empty Then k_names.Add(k_extra.DdlObject)
        Next
        Return k_names
    End Function

    ' ------------------------------------------------------------------
    ' Whole-store operations
    ' ------------------------------------------------------------------

    ''' <summary>Only one run at a time may write to the store; the lock goes away when the returned object is disposed.</summary>
    Public Function AcquireLock() As IDisposable
        Try
            Directory.CreateDirectory(_root)
            Return New FileStream(Path.Combine(_root, "store.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose)
        Catch ex As IOException
            GlobalErrorLog.Write("BackupStore.AcquireLock", ex)
            Throw New BackupException("O altă copie locală rulează deja (sau dosarul e folosit de alt program).", ex)
        End Try
    End Function

    Public Function TotalBytes() As Long
        Dim k_total As Long = 0
        If Not Directory.Exists(_root) Then Return 0
        For Each k_file As String In Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories)
            k_total += New FileInfo(k_file).Length
        Next
        Return k_total
    End Function

    ''' <summary>
    ''' Opens every object (the name must come out the same as the file's), every snapshot, and checks that each snapshot's
    ''' objects exist. Objects that no snapshot names are only counted (the retention step removes them).
    ''' </summary>
    Public Function Verify(k_progress As Action(Of String)) As BackupVerifyReport
        Dim k_report As New BackupVerifyReport()
        Try
            k_progress?.Invoke("Se citesc instantaneele...")
            Dim k_listing As BackupSnapshotListing = ListSnapshots()
            k_report.Snapshots = k_listing.Items.Count
            For Each k_problem As String In k_listing.Problems
                k_report.Damaged.Add(k_problem)
            Next
            Dim k_referenced As New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_manifest As SnapshotManifest In k_listing.Items
                For Each k_name As String In ReferencedObjects(k_manifest)
                    k_referenced.Add(k_name)
                Next
            Next
            Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
            Dim k_count As Integer = 0
            For Each k_name As String In ObjectNames()
                k_seen.Add(k_name)
                k_count += 1
                If k_count Mod 200 = 0 Then k_progress?.Invoke("Se verifică obiectele: " & k_count.ToString(Globalization.CultureInfo.InvariantCulture) & "...")
                Try
                    Dim k_path As String = ObjectPath(k_name)
                    Dim k_file As Byte() = File.ReadAllBytes(k_path)
                    k_report.Bytes += k_file.Length
                    Dim k_plain As Byte() = BackupCrypto.Unpack(BackupCrypto.Open(BackupCrypto.KindObject, k_file, k_name, _keys))
                    If BackupCrypto.ObjectName(k_plain, _keys) <> k_name Then k_report.Damaged.Add("Obiect " & k_name.Substring(0, 8) & ": conținutul nu se potrivește cu numele.")
                Catch ex As Exception When TypeOf ex Is BackupException OrElse TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException
                    k_report.Damaged.Add("Obiect " & k_name.Substring(0, 8) & ": " & ex.Message)
                End Try
            Next
            k_report.Objects = k_count
            For Each k_name As String In k_referenced
                If Not k_seen.Contains(k_name) Then k_report.Missing.Add(k_name.Substring(0, 8))
            Next
            k_report.Orphans = k_seen.Count - k_seen.Intersect(k_referenced).Count()
            Return k_report
        Catch ex As Exception
            GlobalErrorLog.Write("BackupStore.Verify", ex)
            Throw
        End Try
    End Function

    ' ------------------------------------------------------------------

    ''' <summary>Writes to a temporary file in the same folder, flushes it to disk, then moves it over the target.</summary>
    Private Shared Sub WriteAtomic(k_path As String, k_bytes As Byte())
        Dim k_dir As String = Path.GetDirectoryName(k_path)
        Directory.CreateDirectory(k_dir)
        Dim k_temp As String = k_path & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            Using k_stream As New FileStream(k_temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.WriteThrough)
                k_stream.Write(k_bytes, 0, k_bytes.Length)
                k_stream.Flush(True)
            End Using
            File.Move(k_temp, k_path, True)
        Catch
            If File.Exists(k_temp) Then File.Delete(k_temp)
            Throw
        End Try
    End Sub

End Class
