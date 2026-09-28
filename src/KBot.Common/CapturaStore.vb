Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode

''' <summary>
''' One picture taken out of the FOREXE page, while it waits on disk for its number
''' (operator, 28.09.2026). <see cref="Marcaj"/> is the id the picture will hang off once
''' the ingest has written the record: the DDF revision (<c>IDREV</c>) for a reservation,
''' the reception snapshot (<c>IDRH</c>) for a reception. Zero while it is not known yet -
''' the «before» picture is taken before the operator's first save, and the number is only
''' reserved at that save.
''' </summary>
Public NotInheritable Class CapturaForexe

    ''' <summary>"rezervare" or "receptie" - which document the picture ends up in.</summary>
    Public Property Fel As String = String.Empty

    ''' <summary>
    ''' Which of the pictures this is: "inainte" / "dupa" for a reservation session,
    ''' "receptii" / "info-complete" for a reception (the two the guide names, p.21-22).
    ''' </summary>
    Public Property Moment As String = String.Empty

    Public Property Cod As String = String.Empty
    Public Property Marcaj As Integer
    Public Property LuataLa As Date

    ''' <summary>The file on disk. Not written to the sidecar: it IS the sidecar's name.</summary>
    <Text.Json.Serialization.JsonIgnore>
    Public Property Fisier As String = String.Empty

    ''' <summary>The name the server stores next to the bytes (<c>Nume</c> of the IMG tables).</summary>
    <Text.Json.Serialization.JsonIgnore>
    Public ReadOnly Property Nume As String
        Get
            Return If(String.IsNullOrEmpty(Fisier), String.Empty, Path.GetFileName(Fisier))
        End Get
    End Property

End Class

''' <summary>
''' The captures of the FOREXE page between the moment they are taken and the moment they
''' reach the server: <c>&lt;AppDir&gt;\Capturi\&lt;cod&gt;\&lt;stamp&gt;_&lt;fel&gt;_&lt;moment&gt;.jpg</c>,
''' each with a small JSON sidecar of the same name.
'''
''' <para><b>Why disk and not memory</b> (operator, 28.09.2026): the pictures are taken while
''' the operator works in the browser, and they can only be filed once K-BOT has downloaded
''' and taken in what was saved - which may be minutes later, after a failed download, or
''' after K-BOT was closed and opened again. A picture kept in memory would be the one thing
''' the operator could not redo without repeating the work in FOREXE.</para>
'''
''' <para>Nothing here throws at the operator: a folder that cannot be read is logged and
''' answered with an empty list, because a lost picture must never stop the work it was
''' taken beside. What is NOT swallowed is a failed write - the caller is told, so the page
''' is not held waiting for a picture that does not exist.</para>
''' </summary>
Public NotInheritable Class CapturaStore

    Public Const FelRezervare As String = "rezervare"
    Public Const FelReceptie As String = "receptie"
    Public Const MomentInainte As String = "inainte"
    Public Const MomentDupa As String = "dupa"
    Public Const MomentReceptii As String = "receptii"
    Public Const MomentInfoComplete As String = "info-complete"

    Private Const Extensie As String = ".jpg"
    Private Const ExtensieFisa As String = ".json"

    Private Shared ReadOnly _json As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property Folder As String
        Get
            Return KBotPaths.FolderCapturi
        End Get
    End Property

    Private Shared Function FolderSau(folder As String) As String
        Return If(String.IsNullOrWhiteSpace(folder), Folder, folder)
    End Function

    ''' <summary>The folder of one angajament. Says nothing about whether it exists.</summary>
    Public Shared Function FolderAngajament(cod As String, Optional folder As String = Nothing) As String
        Return Path.Combine(FolderSau(folder), CuratNume(cod))
    End Function

    ''' <summary>
    ''' Writes the picture and its sidecar. Throws on a failed write (I/O boundary): the
    ''' caller must know that the page went on without a picture.
    ''' </summary>
    Public Shared Function Salveaza(cod As String, fel As String, moment As String,
                                    octeti As Byte(), Optional marcaj As Integer = 0,
                                    Optional folder As String = Nothing) As CapturaForexe
        Try
            If octeti Is Nothing OrElse octeti.Length = 0 Then
                Throw New ArgumentException("Captura este goală.", NameOf(octeti))
            End If
            Dim dir As String = FolderAngajament(cod, folder)
            Directory.CreateDirectory(dir)
            Dim stamp As String = Date.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture)
            Dim nume As String = $"{stamp}_{CuratNume(fel)}_{CuratNume(moment)}"
            Dim cale As String = Path.Combine(dir, nume & Extensie)
            File.WriteAllBytes(cale, octeti)
            Dim captura As New CapturaForexe With {
                .Fel = fel, .Moment = moment, .Cod = cod, .Marcaj = marcaj,
                .LuataLa = Date.Now, .Fisier = cale}
            File.WriteAllText(Path.Combine(dir, nume & ExtensieFisa),
                              JsonSerializer.Serialize(captura, _json))
            Return captura
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.Salveaza", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The pictures of an angajament that have not left yet, oldest first;
    ''' <paramref name="fel"/> empty = both kinds. An unreadable folder answers empty.
    ''' </summary>
    Public Shared Function AleSale(cod As String, Optional fel As String = Nothing,
                                   Optional folder As String = Nothing) As List(Of CapturaForexe)
        Dim rezultat As New List(Of CapturaForexe)()
        Try
            Dim dir As String = FolderAngajament(cod, folder)
            If Not Directory.Exists(dir) Then Return rezultat
            For Each cale As String In Directory.GetFiles(dir, "*" & Extensie).OrderBy(Function(f) f)
                Dim captura As CapturaForexe = Citeste(cale)
                If captura Is Nothing Then Continue For
                If Not String.IsNullOrEmpty(fel) AndAlso
                   Not String.Equals(captura.Fel, fel, StringComparison.OrdinalIgnoreCase) Then Continue For
                rezultat.Add(captura)
            Next
            Return rezultat
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.AleSale", ex)
            Return rezultat
        End Try
    End Function

    ''' <summary>The picture plus what the sidecar says; a sidecar that is gone or broken
    ''' does not lose the picture - what the file name carries is used instead.</summary>
    Private Shared Function Citeste(cale As String) As CapturaForexe
        Try
            Dim fisa As String = Path.ChangeExtension(cale, ExtensieFisa)
            If File.Exists(fisa) Then
                Dim text As String = File.ReadAllText(fisa)
                Dim captura As CapturaForexe = JsonSerializer.Deserialize(Of CapturaForexe)(text, _json)
                If captura IsNot Nothing Then
                    captura.Fisier = cale
                    Return captura
                End If
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.Citeste", ex)
        End Try
        Return DinNume(cale)
    End Function

    ''' <summary>«20260928_101500_123_rezervare_inainte.jpg» read back into its fields.</summary>
    Private Shared Function DinNume(cale As String) As CapturaForexe
        Try
            Dim nume As String = Path.GetFileNameWithoutExtension(cale)
            Dim parti As String() = nume.Split("_"c)
            Dim captura As New CapturaForexe With {.Fisier = cale, .LuataLa = File.GetLastWriteTime(cale)}
            captura.Cod = Path.GetFileName(Path.GetDirectoryName(cale))
            If parti.Length >= 5 Then
                captura.Fel = parti(3)
                captura.Moment = String.Join("_", parti.Skip(4))
            End If
            Return captura
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.DinNume", ex)
            Return Nothing
        End Try
    End Function

    Public Shared Function CitesteOcteti(captura As CapturaForexe) As Byte()
        Try
            If captura Is Nothing OrElse Not File.Exists(captura.Fisier) Then Return Nothing
            Return File.ReadAllBytes(captura.Fisier)
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.CitesteOcteti", ex)
            Throw
        End Try
    End Function

    ''' <summary>The number the picture will hang off, learned after it was taken.</summary>
    Public Shared Sub PuneMarcaj(captura As CapturaForexe, marcaj As Integer)
        Try
            If captura Is Nothing OrElse marcaj <= 0 Then Return
            captura.Marcaj = marcaj
            File.WriteAllText(Path.ChangeExtension(captura.Fisier, ExtensieFisa),
                              JsonSerializer.Serialize(captura, _json))
        Catch ex As Exception
            ' The picture is on disk and the caller has the number in hand; the sidecar is
            ' only what a later session would read.
            GlobalErrorLog.Write("CapturaStore.PuneMarcaj", ex)
        End Try
    End Sub

    ''' <summary>The picture is on the server: it goes from the disk, sidecar and all.</summary>
    Public Shared Sub Sterge(captura As CapturaForexe)
        Try
            If captura Is Nothing OrElse String.IsNullOrEmpty(captura.Fisier) Then Return
            If File.Exists(captura.Fisier) Then File.Delete(captura.Fisier)
            Dim fisa As String = Path.ChangeExtension(captura.Fisier, ExtensieFisa)
            If File.Exists(fisa) Then File.Delete(fisa)
            ' An angajament with nothing left behind takes its folder with it.
            Dim dir As String = Path.GetDirectoryName(captura.Fisier)
            If Not String.IsNullOrEmpty(dir) AndAlso Directory.Exists(dir) AndAlso
               Directory.GetFileSystemEntries(dir).Length = 0 Then
                Directory.Delete(dir)
            End If
        Catch ex As Exception
            ' A picture that will not go is logged, not thrown: it was already filed.
            GlobalErrorLog.Write("CapturaStore.Sterge", ex)
        End Try
    End Sub

    ''' <summary>One picture in one line, for <see cref="CapturiLog"/>. Never throws.</summary>
    Public Shared Function Descrie(captura As CapturaForexe) As String
        Try
            If captura Is Nothing Then Return "(nicio captură)"
            Dim dimensiune As String = "?"
            If Not String.IsNullOrEmpty(captura.Fisier) AndAlso File.Exists(captura.Fisier) Then
                dimensiune = (New FileInfo(captura.Fisier).Length \ 1024).ToString(CultureInfo.InvariantCulture) & " KB"
            ElseIf Not String.IsNullOrEmpty(captura.Fisier) Then
                dimensiune = "fișierul lipsește"
            End If
            Return $"{captura.Nume} [cod={captura.Cod}, fel={captura.Fel}, moment={captura.Moment}, " &
                   $"marcaj={captura.Marcaj}, luată la {captura.LuataLa:dd.MM.yyyy HH:mm:ss}, {dimensiune}]"
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.Descrie", ex)
            Return If(captura?.Nume, "?")
        End Try
    End Function

    ''' <summary>
    ''' What is waiting under <c>Capturi\</c>, all angajamente: «folder: n (fel/moment, ...)»
    ''' per folder, or «gol». For the log line of an upload that found nothing under its own
    ''' code - a picture kept under ANOTHER code (or under «fara_cod») shows up here. Never throws.
    ''' </summary>
    Public Shared Function Inventar(Optional folder As String = Nothing) As String
        Try
            Dim radacina As String = FolderSau(folder)
            If Not Directory.Exists(radacina) Then Return $"«{radacina}» nu există"
            Dim parti As New List(Of String)()
            For Each dir As String In Directory.GetDirectories(radacina).OrderBy(Function(d) d)
                Dim poze As String() = Directory.GetFiles(dir, "*" & Extensie)
                If poze.Length = 0 Then Continue For
                Dim feluri As IEnumerable(Of String) =
                    poze.Select(Function(p) Citeste(p)).
                         Where(Function(c) c IsNot Nothing).
                         Select(Function(c) $"{c.Fel}/{c.Moment}/marcaj {c.Marcaj}")
                parti.Add($"{Path.GetFileName(dir)}: {poze.Length} ({String.Join(", ", feluri)})")
            Next
            If parti.Count = 0 Then Return $"«{radacina}» e gol"
            Return $"în «{radacina}»: " & String.Join(" | ", parti)
        Catch ex As Exception
            GlobalErrorLog.Write("CapturaStore.Inventar", ex)
            Return "inventarul nu s-a putut citi: " & ex.Message
        End Try
    End Function

    ''' <summary>Nothing from an angajament code or a word ever reaches a path raw.</summary>
    Private Shared Function CuratNume(text As String) As String
        Dim curat As String = If(text, String.Empty).Trim()
        For Each c As Char In Path.GetInvalidFileNameChars()
            curat = curat.Replace(c, "_"c)
        Next
        curat = curat.Replace(" "c, "_"c)
        Return If(String.IsNullOrEmpty(curat), "fara_cod", curat)
    End Function

End Class
