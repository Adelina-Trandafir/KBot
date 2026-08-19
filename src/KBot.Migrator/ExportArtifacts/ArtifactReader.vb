Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json
Imports KBot.Common

''' <summary>
''' Citește artefactele scrise de <c>mdl_FX_ExportSeed</c> în <c>VBA_ARTEFACTS</c>.
''' Metode de graniță (I/O + parsare JSON): logăm și RE-ARUNCĂM, niciodată nu înghițim —
''' un cititor care crede că a citit când n-a citit e mai rău decât unul care se plânge.
''' </summary>
Public NotInheritable Class ArtifactReader

    Private ReadOnly _dir As String

    Public Sub New(artifactsDirectory As String)
        If String.IsNullOrWhiteSpace(artifactsDirectory) Then
            Throw New ArgumentException("Folderul cu artefacte nu poate fi gol.", NameOf(artifactsDirectory))
        End If
        _dir = artifactsDirectory
    End Sub

    Public ReadOnly Property Directory As String
        Get
            Return _dir
        End Get
    End Property

    ''' <summary>Calea completă a unui fișier din folderul de artefacte.</summary>
    Public Function PathOf(fileName As String) As String
        Return Path.Combine(_dir, fileName)
    End Function

    ''' <summary>Citește <c>manifest.json</c>.</summary>
    Public Function ReadManifest() As ExportManifest
        Try
            Dim file As String = PathOf("manifest.json")
            If Not IO.File.Exists(file) Then
                Throw New FileNotFoundException(
                    "Nu găsesc «manifest.json» în " & _dir & ". Rulează întâi exportul din Access.", file)
            End If

            Dim m As New ExportManifest()
            Using doc As JsonDocument = JsonDocument.Parse(IO.File.ReadAllText(file, Text.Encoding.UTF8))
                Dim root As JsonElement = doc.RootElement
                m.Exported = ReadString(root, "exported")
                m.Source = ReadString(root, "source")
                m.CaiSource = ReadString(root, "cai_source")

                Dim cai As JsonElement
                If root.TryGetProperty("cai", cai) AndAlso cai.ValueKind = JsonValueKind.Object Then
                    Dim f As String = ReadString(cai, "file")
                    If Not String.IsNullOrWhiteSpace(f) Then m.CaiFile = f
                    m.CaiColumns = ReadStringList(cai, "columns")
                    m.CaiRows = ReadInt(cai, "rows")
                End If

                m.UnexpectedTables = ReadStringList(root, "unexpected_tables")

                Dim tables As JsonElement
                If root.TryGetProperty("tables", tables) AndAlso tables.ValueKind = JsonValueKind.Array Then
                    For Each te As JsonElement In tables.EnumerateArray()
                        Dim mt As New ManifestTable()
                        mt.Table = ReadString(te, "table")
                        mt.Columns = ReadStringList(te, "columns")
                        mt.Rows = ReadInt(te, "rows")
                        mt.Files = ReadStringList(te, "files")
                        m.Tables.Add(mt)
                    Next
                End If
            End Using
            Return m

        Catch ex As Exception
            GlobalErrorLog.Write("ArtifactReader.ReadManifest", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Pasul 1 din §5.2: fiecare fișier listat există, iar numărul de rânduri al fiecărui
    ''' tabel e egal cu suma chunk-urilor lui. Întoarce lista de probleme; goală = curat.
    ''' Nu trimite nimic pe fir — se rulează ÎNAINTE de orice scriere.
    ''' </summary>
    Public Function VerifyFiles(manifest As ExportManifest) As List(Of String)
        Try
            Dim problems As New List(Of String)()

            Dim caiPath As String = PathOf(manifest.CaiFile)
            If Not IO.File.Exists(caiPath) Then
                problems.Add("Lipsește fișierul «" & manifest.CaiFile & "» (tabelul [Cai] — cheia de rutare).")
            End If

            For Each st In SeedTables.All()
                Dim mt As ManifestTable = manifest.FindTable(st.Name)
                If mt Is Nothing Then
                    problems.Add("Tabelul «" & st.Name & "» lipsește din manifest.")
                    Continue For
                End If

                Dim total As Integer = 0
                For Each f As String In mt.Files
                    Dim p As String = PathOf(f)
                    If Not IO.File.Exists(p) Then
                        problems.Add("Lipsește fișierul «" & f & "» (tabelul " & st.Name & ").")
                        Continue For
                    End If
                    total += CountRows(p)
                Next

                If total <> mt.Rows Then
                    problems.Add("Tabelul «" & st.Name & "»: manifestul anunță " & mt.Rows.ToString() &
                                 " rânduri, dar chunk-urile însumează " & total.ToString() & ".")
                End If

                If mt.Columns.Count = 0 AndAlso mt.Rows > 0 Then
                    problems.Add("Tabelul «" & st.Name & "» are rânduri, dar nicio coloană în manifest.")
                End If
            Next

            Return problems

        Catch ex As Exception
            GlobalErrorLog.Write("ArtifactReader.VerifyFiles", ex)
            Throw
        End Try
    End Function

    ''' <summary>Citește un fișier de chunk (sau Cai.json — aceeași formă).</summary>
    Public Function ReadChunk(fileName As String) As ChunkFile
        Try
            Dim file As String = PathOf(fileName)
            Dim c As New ChunkFile() With {.FileName = fileName}

            Using doc As JsonDocument = JsonDocument.Parse(IO.File.ReadAllText(file, Text.Encoding.UTF8))
                Dim root As JsonElement = doc.RootElement
                c.Table = ReadString(root, "table")
                c.Chunk = ReadInt(root, "chunk")
                c.Columns = ReadStringList(root, "columns")

                Dim rows As JsonElement
                If root.TryGetProperty("rows", rows) AndAlso rows.ValueKind = JsonValueKind.Array Then
                    Dim ncols As Integer = c.Columns.Count
                    For Each re As JsonElement In rows.EnumerateArray()
                        If re.ValueKind <> JsonValueKind.Array Then
                            Throw New InvalidDataException(
                                "Fișierul «" & fileName & "» conține un rând care nu e tablou.")
                        End If
                        Dim vals(ncols - 1) As JsonElement
                        Dim i As Integer = 0
                        For Each v As JsonElement In re.EnumerateArray()
                            If i >= ncols Then
                                Throw New InvalidDataException(
                                    "Fișierul «" & fileName & "» are un rând cu mai multe valori decât coloane.")
                            End If
                            ' Clone: elementul supraviețuiește eliberării documentului.
                            vals(i) = v.Clone()
                            i += 1
                        Next
                        If i <> ncols Then
                            Throw New InvalidDataException(
                                "Fișierul «" & fileName & "» are un rând cu " & i.ToString() &
                                " valori, dar " & ncols.ToString() & " coloane.")
                        End If
                        c.Rows.Add(vals)
                    Next
                End If
            End Using

            Return c

        Catch ex As Exception
            GlobalErrorLog.Write("ArtifactReader.ReadChunk", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Numără rândurile unui chunk fără a materializa valorile — pasul de verificare nu are
    ''' nevoie de ele, iar FX_Extrase_F poate purta un document XML întreg pe rând.
    ''' </summary>
    Private Function CountRows(fullPath As String) As Integer
        Using doc As JsonDocument = JsonDocument.Parse(IO.File.ReadAllText(fullPath, Text.Encoding.UTF8))
            Dim rows As JsonElement
            If doc.RootElement.TryGetProperty("rows", rows) AndAlso rows.ValueKind = JsonValueKind.Array Then
                Return rows.GetArrayLength()
            End If
            Return 0
        End Using
    End Function

    ' --- ajutoare de parsare -------------------------------------------------------
    ' Private, atinse doar prin metodele publice deja învelite mai sus.

    Private Shared Function ReadString(parent As JsonElement, name As String) As String
        Dim v As JsonElement
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then
            Return If(v.GetString(), "")
        End If
        Return ""
    End Function

    Private Shared Function ReadInt(parent As JsonElement, name As String) As Integer
        Dim v As JsonElement
        Dim n As Integer
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt32(n) Then
            Return n
        End If
        Return 0
    End Function

    Private Shared Function ReadStringList(parent As JsonElement, name As String) As List(Of String)
        Dim result As New List(Of String)()
        Dim v As JsonElement
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Array Then
            For Each e As JsonElement In v.EnumerateArray()
                If e.ValueKind = JsonValueKind.String Then
                    result.Add(If(e.GetString(), ""))
                End If
            Next
        End If
        Return result
    End Function

End Class
