Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>What one table cost in one copy: the server's own timings (per kind of work) and what the client measured on its side.</summary>
Public NotInheritable Class BackupTableProfile
    Public Property Name As String = String.Empty
    ''' <summary>What the server did with the table: count-skip, checked, count-differs, no-n, first, prev-unusable; "?" when the server sends no timings.</summary>
    Public Property Action As String = "?"
    Public Property Rows As Long
    Public Property Pages As Integer
    Public Property PagesNew As Integer
    Public Property PagesSame As Integer
    Public Property ServerTotalMs As Integer
    Public Property DescribeMs As Integer
    Public Property CountMs As Integer
    Public Property CheckMs As Integer
    Public Property ReadMs As Integer
    Public Property EmitMs As Integer
    Public Property Checks As Integer
    ''' <summary>From the previous table's end (or the start of the stream) to this table's end, as the client saw it: server work + network + client work.</summary>
    Public Property WallMs As Long
    ''' <summary>Time the client spent handling this table's lines (unpack, encrypt, write to disk).</summary>
    Public Property ClientMs As Long
End Class

''' <summary>One step of the client side of a copy (reading the previous copy, uploading the state, the stream, writing the snapshot...).</summary>
Public NotInheritable Class BackupPhaseProfile
    Public Property Name As String = String.Empty
    Public Property Ms As Long
End Class

''' <summary>
''' How long one copy of one database took and where the time went: client phases, server phases, and every table with what the
''' server did to it. Written as a text file under Logs\backup_profile\ after every copy (the «Profil» button of «Copii locale» shows
''' the latest one). Nothing here changes how the copy works.
''' </summary>
Public NotInheritable Class BackupProfile

    Private Const FolderName As String = "backup_profile"
    Private Const KeepPerDatabase As Integer = 30

    Public Property Db As String = String.Empty
    Public Property Mode As String = String.Empty
    Public Property Kind As String = String.Empty
    Public Property Started As DateTimeOffset = DateTimeOffset.Now
    Public Property TotalMs As Long
    Public Property [Error] As String = String.Empty
    ''' <summary>False when the server's admin_snapshot.py is older than the profiler: it sent no timings.</summary>
    Public Property ServerProfiled As Boolean
    Public Property ServerListMs As Integer
    Public Property ServerTablesMs As Integer
    Public Property ServerExtrasMs As Integer
    ''' <summary>How many tables the state file sent to the server knew (0 = the server had nothing to compare with).</summary>
    Public Property StateTables As Integer
    Public ReadOnly Property Phases As New List(Of BackupPhaseProfile)()
    Public ReadOnly Property Tables As New List(Of BackupTableProfile)()

    Public Sub AddPhase(k_name As String, k_ms As Long)
        Phases.Add(New BackupPhaseProfile With {.Name = k_name, .Ms = k_ms})
    End Sub

    ' ------------------------------------------------------------------
    ' Text report
    ' ------------------------------------------------------------------

    Private Shared Function Sec(k_ms As Long) As String
        Return (k_ms / 1000.0).ToString("0.00", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function Col(k_text As String, k_width As Integer) As String
        Return If(k_text.Length > k_width, k_text.Substring(0, k_width), k_text.PadRight(k_width))
    End Function

    Private Shared Function Num(k_text As String, k_width As Integer) As String
        Return k_text.PadLeft(k_width)
    End Function

    ''' <summary>What an action means, in words.</summary>
    Public Shared Function ActionText(k_action As String) As String
        Select Case k_action
            Case "count-skip" : Return "același număr de rânduri ca în copia precedentă: nu s-a verificat nimic"
            Case "checked" : Return "fiecare pagină: sumă de control calculată în bază (MD5 pe rând); se citesc doar paginile schimbate"
            Case "count-differs" : Return "număr de rânduri diferit de copia precedentă: verificare normală, pagină cu pagină"
            Case "no-n" : Return "copia precedentă nu are numărul de rânduri pe pagini: verificare normală"
            Case "first" : Return "nu există copie precedentă a bazei: se citește tot"
            Case "prev-unusable" : Return "copia precedentă a tabelei nu se potrivește (altă cheie sau pagini cu goluri): se citește tot"
            Case Else : Return "serverul nu a trimis timpi (admin_snapshot.py vechi pe server)"
        End Select
    End Function

    Public Function ToText() As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine("PROFIL COPIE LOCALĂ  -  baza " & Db & "  -  mod " & Mode & "  -  tip " & Kind & "  -  început " & Started.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture))
        k_sb.AppendLine("Durata totală: " & Sec(TotalMs) & " s" & If(Me.[Error] <> String.Empty, "   ·   EȘUAT: " & Me.[Error], String.Empty))
        If Not ServerProfiled Then
            k_sb.AppendLine("ATENȚIE: serverul nu a trimis timpi pe tabele. admin_snapshot.py de pe server e vechi; instalați cel nou ca să apară tabelele de mai jos.")
        End If
        k_sb.AppendLine()
        k_sb.AppendLine("FAZELE DE PE ACEST CALCULATOR")
        For Each k_phase As BackupPhaseProfile In Phases
            k_sb.AppendLine("  " & Col(k_phase.Name, 52) & Num(Sec(k_phase.Ms), 9) & " s")
        Next
        If ServerProfiled Then
            k_sb.AppendLine()
            k_sb.AppendLine("PE SERVER (snap-run)")
            k_sb.AppendLine("  " & Col("pornit tranzacția + lista tabelelor", 52) & Num(Sec(ServerListMs), 9) & " s")
            k_sb.AppendLine("  " & Col("toate tabelele", 52) & Num(Sec(ServerTablesMs), 9) & " s")
            k_sb.AppendLine("  " & Col("view-uri, trigger-e, proceduri, evenimente", 52) & Num(Sec(ServerExtrasMs), 9) & " s")
            k_sb.AppendLine("  Tabele cunoscute din copia precedentă trimise serverului: " & StateTables.ToString(CultureInfo.InvariantCulture))
        End If

        Dim k_sorted As List(Of BackupTableProfile) = Tables.OrderByDescending(Function(k_t) If(k_t.ServerTotalMs > 0, k_t.ServerTotalMs, k_t.WallMs)).ToList()
        k_sb.AppendLine()
        k_sb.AppendLine("TABELE, cele mai lente întâi (toți timpii în secunde)")
        k_sb.AppendLine("  " & Col("Tabela", 28) & Col("Ce face", 14) & Num("Rânduri", 11) & Num("Pagini", 7) & Num("Noi", 5) &
                        Num("Server", 8) & Num("descr.", 7) & Num("count", 7) & Num("verif.", 8) & Num("citire", 8) & Num("pachet", 8) & Num("#verif", 7) &
                        Num("Total*", 8) & Num("Client", 8) & Num("Rețea", 8))
        For Each k_t As BackupTableProfile In k_sorted
            Dim k_net As Long = Math.Max(0L, k_t.WallMs - k_t.ServerTotalMs - k_t.ClientMs)
            k_sb.AppendLine("  " & Col(k_t.Name, 28) & Col(k_t.Action, 14) & Num(k_t.Rows.ToString("N0", CultureInfo.InvariantCulture), 11) &
                            Num(k_t.Pages.ToString(CultureInfo.InvariantCulture), 7) & Num(k_t.PagesNew.ToString(CultureInfo.InvariantCulture), 5) &
                            Num(Sec(k_t.ServerTotalMs), 8) & Num(Sec(k_t.DescribeMs), 7) & Num(Sec(k_t.CountMs), 7) & Num(Sec(k_t.CheckMs), 8) &
                            Num(Sec(k_t.ReadMs), 8) & Num(Sec(k_t.EmitMs), 8) & Num(k_t.Checks.ToString(CultureInfo.InvariantCulture), 7) &
                            Num(Sec(k_t.WallMs), 8) & Num(Sec(k_t.ClientMs), 8) & Num(Sec(k_net), 8))
        Next
        k_sb.AppendLine("  Server = timpul tabelei pe server; descr. = SHOW CREATE + coloane; count = COUNT(*); verif. = suma de control MD5 pe pagini;")
        k_sb.AppendLine("  citire = rândurile citite din bază; pachet = comprimat + trimis; #verif = câte sume de control s-au calculat;")
        k_sb.AppendLine("  Total* = cât a văzut calculatorul acesta între sfârșitul tabelei precedente și sfârșitul acesteia; Client = despachetat, criptat, scris pe disc;")
        k_sb.AppendLine("  Rețea = Total* - Server - Client (transfer și așteptare).")

        k_sb.AppendLine()
        k_sb.AppendLine("UNDE S-A DUS TIMPUL, pe felul de lucru")
        For Each k_group In Tables.GroupBy(Function(k_t) k_t.Action).OrderByDescending(Function(k_g) k_g.Sum(Function(k_t) CLng(k_t.ServerTotalMs)))
            k_sb.AppendLine("  " & Col(k_group.Key, 14) & Num(k_group.Count().ToString(CultureInfo.InvariantCulture), 4) & " tabele, " &
                            Num(Sec(k_group.Sum(Function(k_t) CLng(k_t.ServerTotalMs))), 8) & " s pe server   -   " & ActionText(k_group.Key))
        Next
        Return k_sb.ToString()
    End Function

    ' ------------------------------------------------------------------
    ' Files
    ' ------------------------------------------------------------------

    Private Shared Function Folder() As String
        Dim k_dir As String = Path.Combine(LogPaths.EnsureLogsDirectory(), FolderName)
        Directory.CreateDirectory(k_dir)
        Return k_dir
    End Function

    Private Shared Function SafeName(k_db As String) As String
        Dim k_text As String = k_db
        For Each k_bad As Char In Path.GetInvalidFileNameChars()
            k_text = k_text.Replace(k_bad, "_"c)
        Next
        Return k_text
    End Function

    ''' <summary>Writes the report next to the other logs and keeps the latest ones of this database. A failure here only goes to the error log.</summary>
    Public Sub Save()
        Try
            Dim k_dir As String = Folder()
            Dim k_prefix As String = SafeName(Db) & "_"
            File.WriteAllText(Path.Combine(k_dir, k_prefix & Started.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) & ".txt"), ToText(), New UTF8Encoding(True))
            Dim k_stampLength As Integer = "yyyyMMdd_HHmmss.txt".Length
            Dim k_old As String() = Directory.GetFiles(k_dir, k_prefix & "*.txt").
                Where(Function(k_f) Path.GetFileName(k_f).Length = k_prefix.Length + k_stampLength).
                OrderByDescending(Function(k_f) k_f, StringComparer.Ordinal).Skip(KeepPerDatabase).ToArray()
            For Each k_file As String In k_old
                File.Delete(k_file)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("BackupProfile.Save", ex)
        End Try
    End Sub

    ''' <summary>The text of the latest report of each of the given databases (all of them when the list is empty), newest copy first; empty when there is none.</summary>
    Public Shared Function LatestReports(k_dbs As IEnumerable(Of String)) As String
        Try
            Dim k_dir As String = Path.Combine(LogPaths.LogsDirectory(), FolderName)
            If Not Directory.Exists(k_dir) Then Return String.Empty
            Dim k_files As IEnumerable(Of String) = Directory.GetFiles(k_dir, "*.txt")
            Dim k_wanted As List(Of String) = k_dbs.Select(Function(k_d) SafeName(k_d) & "_").ToList()
            Dim k_latest As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each k_file As String In k_files.OrderBy(Function(k_f) k_f, StringComparer.Ordinal)
                Dim k_name As String = Path.GetFileNameWithoutExtension(k_file)
                Dim k_cut As Integer = k_name.Length - "yyyyMMdd_HHmmss".Length - 1
                If k_cut <= 0 Then Continue For
                Dim k_dbPart As String = k_name.Substring(0, k_cut + 1)
                If k_wanted.Count = 0 OrElse k_wanted.Contains(k_dbPart, StringComparer.OrdinalIgnoreCase) Then k_latest(k_dbPart) = k_file
            Next
            Return String.Join(vbLf & New String("="c, 100) & vbLf & vbLf, k_latest.Values.Select(Function(k_f) File.ReadAllText(k_f, Encoding.UTF8)))
        Catch ex As Exception
            GlobalErrorLog.Write("BackupProfile.LatestReports", ex)
            Throw
        End Try
    End Function

End Class
