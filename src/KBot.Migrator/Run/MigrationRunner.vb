Imports System.Collections.Generic
Imports System.Text.Json
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' Secvența din §5.2, în două butoane:
'''
''' <para><b>Verificare</b> (pașii 1–5) — nu scrie NIMIC pe server. Citește manifestul și
''' verifică fișierele, construiește hărțile de rutare, compară coloanele exportate cu cele
''' reale din MariaDB și verifică id-urile IDDF/IDREV. Produce raportul complet.</para>
'''
''' <para><b>Transfer</b> (pașii 6–7) — scrie cu <c>mode="insert_missing"</c> și, după fiecare
''' tabel, închide socoteala: citit = rutat + respins (+ copii suplimentare), iar per DC
''' rutat = inserat + sărit. O nepotrivire e eroare DURĂ în jurnal, nu avertisment.</para>
''' </summary>
Public NotInheritable Class MigrationRunner

    Private ReadOnly _reader As ArtifactReader
    Private ReadOnly _api As SeedApiClient
    Private ReadOnly _log As RunLog
    Private ReadOnly _selectedDcs As List(Of String)

    Public Sub New(reader As ArtifactReader, api As SeedApiClient, log As RunLog, selectedDcs As IEnumerable(Of String))
        _reader = reader
        _api = api
        _log = log
        _selectedDcs = New List(Of String)(selectedDcs)
    End Sub

    ' =========================================================================
    ' VERIFICARE — pașii 1..5. Nu trimite nicio scriere.
    ' =========================================================================
    Public Async Function VerifyAsync() As Task(Of VerificationResult)
        Try
            Dim v As New VerificationResult()

            ' --- 1. manifest + fișiere -----------------------------------------
            _log.Write("=== Pasul 1: manifest și fișiere ===")
            v.Manifest = _reader.ReadManifest()
            _log.Write("Export din " & v.Manifest.Exported & ", sursă " & v.Manifest.Source & ".")
            If v.Manifest.UnexpectedTables.Count > 0 Then
                _log.Write("ATENȚIE: tabele în afara domeniului există în Access și NU au fost exportate: " &
                           String.Join(", ", v.Manifest.UnexpectedTables) & ".")
            End If

            Dim problems As List(Of String) = _reader.VerifyFiles(v.Manifest)
            If problems.Count > 0 Then
                For Each p As String In problems
                    _log.Write("EROARE artefacte: " & p)
                Next
                ' Oprire ÎNAINTE de a trimite orice: artefactele nu sunt întregi.
                Throw New InvalidOperationException(
                    "Artefactele nu sunt complete (" & problems.Count.ToString() &
                    " probleme). Rularea s-a oprit înainte de a trimite ceva.")
            End If
            _log.Write("Artefacte complete: fiecare fișier există și numărul de rânduri se potrivește.")

            ' --- 2 + 4a. hărțile de rutare -------------------------------------
            _log.Write("=== Pașii 2 și 4: hărțile de rutare ===")
            Dim builder As New RoutingMapBuilder(_reader, v.Manifest, Sub(t) _log.Write(t))
            v.Maps = builder.Build()

            Dim known As List(Of String) = v.Maps.AllDcs()
            For Each dc As String In _selectedDcs
                If Not known.Contains(dc, StringComparer.OrdinalIgnoreCase) Then
                    v.BlockedDcs(dc) = "DC-ul nu apare în [Cai]."
                    _log.Write("EROARE: DC-ul «" & dc & "» nu apare în [Cai]; a fost oprit.")
                End If
            Next

            ' --- 3. coloanele reale din MariaDB --------------------------------
            _log.Write("=== Pasul 3: coloanele țintă ===")
            Await CheckColumnsAsync(v).ConfigureAwait(False)

            ' --- 4b. rutarea propriu-zisă + culegerea id-urilor DDF -------------
            _log.Write("=== Pasul 4: rutarea rândurilor ===")
            Dim ddfIds As Dictionary(Of String, HashSet(Of Long)) = RouteAll(v)

            ' --- 5. IDDF / IDREV ------------------------------------------------
            _log.Write("=== Pasul 5: verificarea id-urilor IDDF/IDREV ===")
            Await CheckDdfIdsAsync(v, ddfIds).ConfigureAwait(False)

            For Each dc As String In _selectedDcs
                If Not v.BlockedDcs.ContainsKey(dc) Then v.CleanDcs.Add(dc)
            Next
            v.IsClean = (v.BlockedDcs.Count = 0 AndAlso v.BlockedTables.Count = 0)

            _log.Write("Verificare încheiată. " &
                       If(v.IsClean, "Curat — transferul poate porni.",
                          "Cu opriri: " & v.BlockedDcs.Count.ToString() & " DC-uri și " &
                          v.BlockedTables.Count.ToString() & " tabele blocate."))
            Return v

        Catch ex As Exception
            GlobalErrorLog.Write("MigrationRunner.VerifyAsync", ex)
            Throw
        End Try
    End Function

    ' =========================================================================
    ' TRANSFER — pașii 6..7.
    ' =========================================================================
    Public Async Function TransferAsync(v As VerificationResult) As Task
        Try
            _log.Write("=== Pasul 6: scrierea rândurilor (insert_missing) ===")

            For Each st In SeedTables.All()
                Dim mt As ManifestTable = v.Manifest.FindTable(st.Name)
                If mt Is Nothing Then Continue For

                Dim stats As TableStats = v.StatsFor(st.Name)
                For Each dcKey As String In v.CleanDcs
                    If v.IsBlocked(dcKey, st.Name) Then
                        _log.WriteFor(dcKey, st.Name & ": sărit — tabelul e blocat de verificare.")
                    End If
                Next

                For Each fileName As String In mt.Files
                    Dim chunk As ChunkFile = _reader.ReadChunk(fileName)
                    Dim router As New RowRouter(st, v.Maps)
                    router.Prepare(chunk)

                    ' Găleți per DC, în limitele unui chunk (max 500 rânduri), deci mereu sub
                    ' plafonul serverului de 1000 rânduri pe cerere.
                    Dim buckets As New Dictionary(Of String, List(Of JsonElement()))(StringComparer.OrdinalIgnoreCase)

                    For Each row As JsonElement() In chunk.Rows
                        Dim r As RouteResult = router.Route(row)
                        If r.IsRejected Then Continue For       ' deja consemnat la verificare

                        For Each dc As String In r.Dcs
                            If Not v.CleanDcs.Contains(dc, StringComparer.OrdinalIgnoreCase) Then Continue For
                            If v.IsBlocked(dc, st.Name) Then Continue For
                            Dim list As List(Of JsonElement()) = Nothing
                            If Not buckets.TryGetValue(dc, list) Then
                                list = New List(Of JsonElement())()
                                buckets.Add(dc, list)
                            End If
                            list.Add(row)
                        Next
                    Next

                    For Each kv As KeyValuePair(Of String, List(Of JsonElement())) In buckets
                        If kv.Value.Count = 0 Then Continue For
                        Dim res As SeedWriteResult = Await _api.InsertMissingAsync(
                            kv.Key, st.Name, chunk.Columns, kv.Value).ConfigureAwait(False)
                        Dim s As DcTableStats = stats.For1(kv.Key)
                        s.Inserted += res.Inserted
                        s.Skipped += res.Skipped
                    Next
                Next

                ReconcileTable(v, st.Name)
            Next

            _log.Write("Transfer încheiat.")

        Catch ex As Exception
            GlobalErrorLog.Write("MigrationRunner.TransferAsync", ex)
            Throw
        End Try
    End Function

    ' =========================================================================
    ' Pașii, în detaliu. Private, atinși doar prin cele două metode de mai sus.
    ' =========================================================================

    ''' <summary>
    ''' Pasul 3. O coloană prezentă în export și absentă pe țintă OPREȘTE tabelul, cu numele
    ''' ei: înseamnă că propagarea de schemă (AVACONT_COMUN) nu a fost rulată acolo.
    ''' Utilitarul ăsta nu creează și nu modifică niciodată un tabel.
    ''' </summary>
    Private Async Function CheckColumnsAsync(v As VerificationResult) As Task
        For Each dc As String In _selectedDcs
            If v.BlockedDcs.ContainsKey(dc) Then Continue For

            For Each st In SeedTables.All()
                Dim mt As ManifestTable = v.Manifest.FindTable(st.Name)
                If mt Is Nothing Then Continue For

                Dim target As List(Of String) = Await _api.GetColumnsAsync(dc, st.Name).ConfigureAwait(False)
                If target.Count = 0 Then
                    v.BlockedTables(VerificationResult.BlockKey(dc, st.Name)) =
                        "Tabelul nu există în baza «" & dc & "»."
                    _log.WriteFor(dc, "EROARE: tabelul «" & st.Name & "» nu există. Schema se instalează separat.")
                    Continue For
                End If

                Dim targetSet As New HashSet(Of String)(target, StringComparer.OrdinalIgnoreCase)
                Dim missing As New List(Of String)()
                For Each c As String In mt.Columns
                    If Not targetSet.Contains(c) Then missing.Add(c)
                Next

                Dim exportSet As New HashSet(Of String)(mt.Columns, StringComparer.OrdinalIgnoreCase)
                Dim extra As New List(Of String)()
                For Each c As String In target
                    If Not exportSet.Contains(c) Then extra.Add(c)
                Next

                If extra.Count > 0 Then
                    ' Doar raportat: o coloană în plus pe țintă rămâne pe valoarea ei implicită.
                    _log.WriteFor(dc, st.Name & ": ținta are " & extra.Count.ToString() &
                                  " coloane în plus, netrimise — " & String.Join(", ", extra) & ".")
                End If

                If missing.Count > 0 Then
                    v.BlockedTables(VerificationResult.BlockKey(dc, st.Name)) =
                        "Coloane absente pe țintă: " & String.Join(", ", missing) & "."
                    _log.WriteFor(dc, "EROARE: tabelului «" & st.Name & "» îi lipsesc pe țintă coloanele " &
                                  String.Join(", ", missing) & ". Propagarea de schemă nu a fost rulată.")
                End If
            Next
        Next
    End Function

    ''' <summary>
    ''' Pasul 4. Rutează fiecare rând, umple contorii și consemnează orfanii. Nu reține datele
    ''' rândurilor — transferul recitește fișierele, ca memoria să rămână mărginită indiferent
    ''' de cât de mare e FX_Istoric.
    '''
    ''' Întoarce, pe cheia „DC|Tabel|Coloană", mulțimea de id-uri IDDF/IDREV de verificat.
    ''' </summary>
    Private Function RouteAll(v As VerificationResult) As Dictionary(Of String, HashSet(Of Long))
        Dim ddfIds As New Dictionary(Of String, HashSet(Of Long))(StringComparer.OrdinalIgnoreCase)

        For Each st In SeedTables.All()
            Dim mt As ManifestTable = v.Manifest.FindTable(st.Name)
            If mt Is Nothing Then Continue For
            Dim stats As TableStats = v.StatsFor(st.Name)

            For Each fileName As String In mt.Files
                Dim chunk As ChunkFile = _reader.ReadChunk(fileName)
                Dim router As New RowRouter(st, v.Maps)
                router.Prepare(chunk)

                ' Indexurile coloanelor DDF, o dată per chunk.
                Dim ddfIdx As New List(Of KeyValuePair(Of String, Integer))()
                For Each col As String In st.DdfColumns
                    Dim idx As Integer = chunk.IndexOfColumn(col)
                    If idx >= 0 Then ddfIdx.Add(New KeyValuePair(Of String, Integer)(col, idx))
                Next

                For Each row As JsonElement() In chunk.Rows
                    stats.Read += 1
                    Dim r As RouteResult = router.Route(row)

                    If r.IsRejected Then
                        stats.Rejected += 1
                        _log.Reject(st.Name, router.PrimaryKeyOf(row), r.Reject)
                        Continue For
                    End If

                    If r.Dcs.Count > 1 Then stats.Duplicated += (r.Dcs.Count - 1)

                    For Each dc As String In r.Dcs
                        If Not _selectedDcs.Contains(dc, StringComparer.OrdinalIgnoreCase) Then
                            stats.OutOfScope += 1
                            Continue For
                        End If
                        stats.For1(dc).Routed += 1

                        For Each pair As KeyValuePair(Of String, Integer) In ddfIdx
                            Dim id As Long
                            If JsonValues.TryAsLong(row(pair.Value), id) Then
                                Dim key As String = dc & "|" & st.Name & "|" & pair.Key
                                Dim set1 As HashSet(Of Long) = Nothing
                                If Not ddfIds.TryGetValue(key, set1) Then
                                    set1 = New HashSet(Of Long)()
                                    ddfIds.Add(key, set1)
                                End If
                                set1.Add(id)
                            End If
                        Next
                    Next
                Next
            Next

            _log.Write(st.Name & ": citite " & stats.Read.ToString() &
                       ", rutate " & stats.RoutedTotal.ToString() &
                       ", respinse " & stats.Rejected.ToString() &
                       If(stats.Duplicated > 0, ", copii suplimentare " & stats.Duplicated.ToString(), "") &
                       If(stats.OutOfScope > 0, ", în DC-uri neselectate " & stats.OutOfScope.ToString(), "") & ".")
        Next

        Return ddfIds
    End Function

    ''' <summary>
    ''' Pasul 5. Se VERIFICĂ, nu se traduce: orice id lipsă oprește DC-ul, cu lista.
    ''' </summary>
    Private Async Function CheckDdfIdsAsync(v As VerificationResult,
                                            ddfIds As Dictionary(Of String, HashSet(Of Long))) As Task
        For Each kv As KeyValuePair(Of String, HashSet(Of Long)) In ddfIds
            Dim parts As String() = kv.Key.Split("|"c)
            Dim dc As String = parts(0)
            Dim table As String = parts(1)
            Dim column As String = parts(2)

            If v.BlockedDcs.ContainsKey(dc) Then Continue For
            If kv.Value.Count = 0 Then Continue For

            ' IDDF trăiește în FX_DDF, IDREV în FX_DDF_REV — singura pereche pe care ruta o acceptă.
            Dim targetTable As String =
                If(String.Equals(column, "IDDF", StringComparison.OrdinalIgnoreCase),
                   SeedTables.DdfTable, SeedTables.DdfRevTable)

            Dim wanted As New List(Of Long)(kv.Value)
            wanted.Sort()

            Dim res As SeedIdsResult = Await _api.CheckIdsAsync(dc, targetTable, column, wanted).ConfigureAwait(False)
            If res.Missing.Count = 0 Then
                _log.WriteFor(dc, table & "." & column & ": toate cele " & wanted.Count.ToString() &
                              " id-uri există în " & targetTable & ".")
            Else
                Dim shown As Integer = Math.Min(50, res.Missing.Count)
                Dim list As String = String.Join(", ", res.Missing.GetRange(0, shown))
                If res.Missing.Count > shown Then list &= ", … (" & res.Missing.Count.ToString() & " în total)"
                v.BlockedDcs(dc) = "Id-uri " & column & " absente din " & targetTable & ": " & list
                _log.WriteFor(dc, "EROARE: " & table & "." & column & " — " & res.Missing.Count.ToString() &
                              " id-uri lipsesc din " & targetTable & ": " & list &
                              ". DC-ul a fost oprit; nu se remapează nimic.")
            End If
        Next
    End Function

    ''' <summary>
    ''' Pasul 7. Socoteala trebuie să închidă exact:
    ''' <list type="bullet">
    ''' <item>global: citit = rutat + respins + în DC-uri neselectate − copii suplimentare;</item>
    ''' <item>per DC: rutat = inserat + sărit.</item>
    ''' </list>
    ''' O nepotrivire e eroare DURĂ în jurnal, nu avertisment.
    ''' </summary>
    Private Sub ReconcileTable(v As VerificationResult, table As String)
        Dim s As TableStats = v.StatsFor(table)

        Dim accounted As Integer = s.RoutedTotal + s.Rejected + s.OutOfScope - s.Duplicated
        If accounted <> s.Read Then
            _log.Write("EROARE de socoteală la «" & table & "»: citite " & s.Read.ToString() &
                       ", dar rutate+respinse+neselectate−copii = " & accounted.ToString() & ".")
        End If

        For Each dc As String In v.CleanDcs
            If v.IsBlocked(dc, table) Then Continue For
            Dim d As DcTableStats = s.For1(dc)
            If d.Routed <> d.Inserted + d.Skipped Then
                _log.WriteFor(dc, "EROARE de socoteală la «" & table & "»: rutate " & d.Routed.ToString() &
                              ", dar inserate+sărite = " & (d.Inserted + d.Skipped).ToString() & ".")
            Else
                _log.WriteFor(dc, table & ": rutate " & d.Routed.ToString() &
                              ", inserate " & d.Inserted.ToString() &
                              ", deja existente " & d.Skipped.ToString() & ".")
            End If
        Next
    End Sub

End Class
