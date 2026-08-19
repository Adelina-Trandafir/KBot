Imports System.Collections.Generic
Imports System.Text.Json
Imports KBot.Common

''' <summary>
''' Hărțile de rutare, construite TOATE înainte de orice scriere.
'''
''' <list type="bullet">
''' <item>Cai: <c>IdUnitate → DC</c>, din Cai.json. IdUnitate e cheia primară a lui [Cai],
''' deci o unitate aparține exact unui DC — dar un DC acoperă MAI MULTE unități.</item>
''' <item>A: <c>CodAngajament → DC</c>, din FX_Angajamente.</item>
''' <item>B: <c>IDRZ → DC</c>, din FX_Rezervari (rutat el însuși prin A).</item>
''' <item>C: <c>IDRR → DC</c>, din FX_Receptii_R (rutat prin A).</item>
''' <item>D: <c>IDRH → DC</c>, din FX_Receptii_H (rutat prin A).</item>
''' <item>E: <c>IDEXF → mulțime de DC</c>, din FX_Extrase_H (rutat prin IdUnitate propriu).
''' Mulțime, nu valoare: un fișier de extras poate purta linii pentru mai multe unități.</item>
''' </list>
''' </summary>
Public NotInheritable Class RoutingMaps

    Public ReadOnly Property UnitToDc As New Dictionary(Of Long, String)()
    Public ReadOnly Property Angajament As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Public ReadOnly Property Rezervare As New Dictionary(Of Long, String)()
    Public ReadOnly Property ReceptieR As New Dictionary(Of Long, String)()
    Public ReadOnly Property ReceptieH As New Dictionary(Of Long, String)()
    Public ReadOnly Property ExtrasFile As New Dictionary(Of Long, HashSet(Of String))()

    ''' <summary>Toate DC-urile văzute în Cai.json, sortate.</summary>
    Public Function AllDcs() As List(Of String)
        Dim set1 As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each kv As KeyValuePair(Of Long, String) In UnitToDc
            If Not String.IsNullOrWhiteSpace(kv.Value) Then set1.Add(kv.Value)
        Next
        Return New List(Of String)(set1)
    End Function

End Class

''' <summary>
''' Construiește <see cref="RoutingMaps"/> citind artefactele, în ordinea din §3.1: Cai, apoi
''' A, apoi B/C/D/E. Fiecare hartă are nevoie de cea dinaintea ei.
'''
''' Metode de graniță (I/O prin ArtifactReader): logăm și RE-ARUNCĂM.
''' </summary>
Public NotInheritable Class RoutingMapBuilder

    Private ReadOnly _reader As ArtifactReader
    Private ReadOnly _manifest As ExportManifest
    Private ReadOnly _log As Action(Of String)

    Public Sub New(reader As ArtifactReader, manifest As ExportManifest, log As Action(Of String))
        _reader = reader
        _manifest = manifest
        _log = log
    End Sub

    Public Function Build() As RoutingMaps
        Try
            Dim maps As New RoutingMaps()

            BuildCai(maps)
            BuildAngajamente(maps)
            BuildFromParent(maps, "FX_Rezervari", "IDRZ", "CodAngajament", maps.Rezervare, maps.Angajament)
            BuildFromParent(maps, "FX_Receptii_R", "IDRR", "CodAngajament", maps.ReceptieR, maps.Angajament)
            BuildFromParent(maps, "FX_Receptii_H", "IDRH", "CodAngajament", maps.ReceptieH, maps.Angajament)
            BuildExtrasFiles(maps)

            Return maps

        Catch ex As Exception
            GlobalErrorLog.Write("RoutingMapBuilder.Build", ex)
            Throw
        End Try
    End Function

    ' --- Cai: IdUnitate -> DC ------------------------------------------------------
    Private Sub BuildCai(maps As RoutingMaps)
        Dim chunk As ChunkFile = _reader.ReadChunk(_manifest.CaiFile)
        Dim iUnit As Integer = chunk.IndexOfColumn("IdUnitate")
        Dim iDc As Integer = chunk.IndexOfColumn("DC")
        If iUnit < 0 OrElse iDc < 0 Then
            Throw New InvalidOperationException(
                "«" & _manifest.CaiFile & "» nu are coloanele IdUnitate și DC. Fără ele nu se poate ruta nimic.")
        End If

        For Each row As JsonElement() In chunk.Rows
            Dim unit As Long
            If Not JsonValues.TryAsLong(row(iUnit), unit) Then Continue For
            Dim dc As String = JsonValues.AsText(row(iDc))
            If String.IsNullOrWhiteSpace(dc) Then
                _log("[Cai] unitatea " & unit.ToString() & " nu are DC — rândurile ei vor fi respinse.")
                Continue For
            End If
            ' IdUnitate e cheia primară a lui [Cai]; un duplicat înseamnă că artefactul minte.
            If maps.UnitToDc.ContainsKey(unit) Then
                If Not String.Equals(maps.UnitToDc(unit), dc, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException(
                        "[Cai]: unitatea " & unit.ToString() & " apare cu două DC-uri diferite («" &
                        maps.UnitToDc(unit) & "» și «" & dc & "»). IdUnitate trebuie să fie cheie primară.")
                End If
            Else
                maps.UnitToDc.Add(unit, dc)
            End If
        Next

        _log("Hartă [Cai]: " & maps.UnitToDc.Count.ToString() & " unități, " &
             maps.AllDcs().Count.ToString() & " DC-uri distincte.")
    End Sub

    ' --- A: CodAngajament -> DC ----------------------------------------------------
    Private Sub BuildAngajamente(maps As RoutingMaps)
        Dim mt As ManifestTable = _manifest.FindTable("FX_Angajamente")
        If mt Is Nothing Then Throw New InvalidOperationException("FX_Angajamente lipsește din manifest.")

        Dim withoutDc As Integer = 0
        For Each f As String In mt.Files
            Dim chunk As ChunkFile = _reader.ReadChunk(f)
            Dim iCod As Integer = chunk.IndexOfColumn("CodAngajament")
            Dim iDc As Integer = chunk.IndexOfColumn("DC")
            Dim iUnit As Integer = chunk.IndexOfColumn("IdUnitate")
            If iCod < 0 Then
                Throw New InvalidOperationException("«" & f & "» nu are coloana CodAngajament.")
            End If

            For Each row As JsonElement() In chunk.Rows
                Dim cod As String = JsonValues.AsText(row(iCod))
                If String.IsNullOrWhiteSpace(cod) Then Continue For

                ' DC propriu dacă e completat; altfel se cade pe IdUnitate prin [Cai].
                Dim dc As String = Nothing
                If iDc >= 0 Then dc = JsonValues.AsText(row(iDc))
                If String.IsNullOrWhiteSpace(dc) Then
                    withoutDc += 1
                    dc = Nothing
                    Dim unit As Long
                    If iUnit >= 0 AndAlso JsonValues.TryAsLong(row(iUnit), unit) Then
                        Dim viaUnit As String = Nothing
                        If maps.UnitToDc.TryGetValue(unit, viaUnit) Then dc = viaUnit
                    End If
                End If
                If String.IsNullOrWhiteSpace(dc) Then Continue For   ' orfan — se respinge la rutare

                If maps.Angajament.ContainsKey(cod) Then
                    ' Presupunerea 2: un CodAngajament aparține exact unei unități. Dacă nu,
                    ' raportăm și oprim — nu alegem noi care DC câștigă.
                    If Not String.Equals(maps.Angajament(cod), dc, StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidOperationException(
                            "CodAngajament «" & cod & "» se rezolvă în două DC-uri diferite («" &
                            maps.Angajament(cod) & "» și «" & dc & "»). Harta A nu mai e o funcție; migrarea se oprește.")
                    End If
                Else
                    maps.Angajament.Add(cod, dc)
                End If
            Next
        Next

        _log("Harta A (CodAngajament → DC): " & maps.Angajament.Count.ToString() & " angajamente" &
             If(withoutDc > 0, " (" & withoutDc.ToString() & " fără DC propriu, rutate prin IdUnitate)", "") & ".")
    End Sub

    ' --- B / C / D: cheie proprie -> DC, prin părintele deja rutat ------------------
    Private Sub BuildFromParent(maps As RoutingMaps, table As String, keyColumn As String,
                                parentColumn As String, target As Dictionary(Of Long, String),
                                parent As Dictionary(Of String, String))
        Dim mt As ManifestTable = _manifest.FindTable(table)
        If mt Is Nothing Then Throw New InvalidOperationException(table & " lipsește din manifest.")

        For Each f As String In mt.Files
            Dim chunk As ChunkFile = _reader.ReadChunk(f)
            Dim iKey As Integer = chunk.IndexOfColumn(keyColumn)
            Dim iParent As Integer = chunk.IndexOfColumn(parentColumn)
            If iKey < 0 OrElse iParent < 0 Then
                Throw New InvalidOperationException(
                    "«" & f & "» nu are coloanele " & keyColumn & " și " & parentColumn & ".")
            End If

            For Each row As JsonElement() In chunk.Rows
                Dim key As Long
                If Not JsonValues.TryAsLong(row(iKey), key) Then Continue For
                Dim parentKey As String = JsonValues.AsText(row(iParent))
                If String.IsNullOrWhiteSpace(parentKey) Then Continue For
                Dim dc As String = Nothing
                If Not parent.TryGetValue(parentKey, dc) Then Continue For   ' orfan
                If Not target.ContainsKey(key) Then target.Add(key, dc)
            Next
        Next

        _log("Hartă " & table & " (" & keyColumn & " → DC): " & target.Count.ToString() & " intrări.")
    End Sub

    ' --- E: IDEXF -> mulțime de DC -------------------------------------------------
    Private Sub BuildExtrasFiles(maps As RoutingMaps)
        Dim mt As ManifestTable = _manifest.FindTable("FX_Extrase_H")
        If mt Is Nothing Then Throw New InvalidOperationException("FX_Extrase_H lipsește din manifest.")

        For Each f As String In mt.Files
            Dim chunk As ChunkFile = _reader.ReadChunk(f)
            Dim iFile As Integer = chunk.IndexOfColumn("IDEXF")
            Dim iUnit As Integer = chunk.IndexOfColumn("IdUnitate")
            If iFile < 0 OrElse iUnit < 0 Then
                Throw New InvalidOperationException("«" & f & "» nu are coloanele IDEXF și IdUnitate.")
            End If

            For Each row As JsonElement() In chunk.Rows
                Dim idexf As Long
                If Not JsonValues.TryAsLong(row(iFile), idexf) Then Continue For
                Dim unit As Long
                If Not JsonValues.TryAsLong(row(iUnit), unit) Then Continue For
                Dim dc As String = Nothing
                If Not maps.UnitToDc.TryGetValue(unit, dc) Then Continue For

                Dim set1 As HashSet(Of String) = Nothing
                If Not maps.ExtrasFile.TryGetValue(idexf, set1) Then
                    set1 = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                    maps.ExtrasFile.Add(idexf, set1)
                End If
                set1.Add(dc)
            Next
        Next

        Dim fanOut As Integer = 0
        For Each kv As KeyValuePair(Of Long, HashSet(Of String)) In maps.ExtrasFile
            If kv.Value.Count > 1 Then fanOut += 1
        Next
        _log("Harta E (IDEXF → DC-uri): " & maps.ExtrasFile.Count.ToString() & " fișiere de extras, " &
             fanOut.ToString() & " dintre ele pentru mai multe DC-uri.")
    End Sub

End Class
