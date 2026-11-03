Option Strict On
Imports System.Globalization

''' <summary>What a retention pass would remove (slice ME02-08). A snapshot is a complete copy, so removing one never breaks another.</summary>
Public NotInheritable Class BackupRetentionPlan
    Public ReadOnly Property Keep As New List(Of SnapshotManifest)()
    Public ReadOnly Property Remove As New List(Of SnapshotManifest)()
    ''' <summary>Objects no remaining snapshot points at (those of the removed snapshots, and any left by a run that failed).</summary>
    Public ReadOnly Property Garbage As New List(Of String)()
    Public Property FreedBytes As Long
End Class

''' <summary>
''' Keeps, for every database: the newest copy of each of the last N days, of the last N ISO weeks and of the last N months, the very
''' latest copy whatever its age, and EVERY manual copy (a manual copy is removed only by hand, with «Șterge»). Everything else goes,
''' then the objects nothing points at.
''' </summary>
Public NotInheritable Class BackupRetention

    Private Sub New()
    End Sub

    Public Shared Function Plan(k_store As BackupStore, k_daily As Integer, k_weekly As Integer, k_monthly As Integer, k_now As DateTime) As BackupRetentionPlan
        Try
            Dim k_plan As New BackupRetentionPlan()
            Dim k_all As List(Of SnapshotManifest) = k_store.ListSnapshots().Items
            Dim k_keepIds As New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_group In k_all.GroupBy(Function(k_m) k_m.Db, StringComparer.OrdinalIgnoreCase)
                Dim k_newestFirst As List(Of SnapshotManifest) = k_group.OrderByDescending(Function(k_m) k_m.FinishedAt).ToList()
                If k_newestFirst.Count > 0 Then k_keepIds.Add(k_newestFirst(0).Id)
                KeepFirstOfEach(k_newestFirst, Function(k_d) k_d.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture), k_daily, k_keepIds)
                KeepFirstOfEach(k_newestFirst, Function(k_d) ISOWeek.GetYear(k_d).ToString(CultureInfo.InvariantCulture) & "-" & ISOWeek.GetWeekOfYear(k_d).ToString(CultureInfo.InvariantCulture), k_weekly, k_keepIds)
                KeepFirstOfEach(k_newestFirst, Function(k_d) k_d.Year.ToString(CultureInfo.InvariantCulture) & "-" & k_d.Month.ToString(CultureInfo.InvariantCulture), k_monthly, k_keepIds)
                For Each k_item As SnapshotManifest In k_newestFirst
                    If k_item.FinishedAt = DateTime.MinValue Then k_keepIds.Add(k_item.Id)                                    ' never delete what cannot be dated
                    If k_item.Kind = "manual" Then k_keepIds.Add(k_item.Id)                                                  ' removed only by hand
                Next
            Next
            Dim k_referenced As New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_item As SnapshotManifest In k_all
                If k_keepIds.Contains(k_item.Id) Then
                    k_plan.Keep.Add(k_item)
                    For Each k_name As String In BackupStore.ReferencedObjects(k_item)
                        k_referenced.Add(k_name)
                    Next
                Else
                    k_plan.Remove.Add(k_item)
                End If
            Next
            For Each k_name As String In k_store.ObjectNames()
                If Not k_referenced.Contains(k_name) Then
                    k_plan.Garbage.Add(k_name)
                    k_plan.FreedBytes += k_store.ObjectSize(k_name)
                End If
            Next
            Return k_plan
        Catch ex As Exception
            GlobalErrorLog.Write("BackupRetention.Plan", ex)
            Throw
        End Try
    End Function

    Private Shared Sub KeepFirstOfEach(k_newestFirst As List(Of SnapshotManifest), k_bucket As Func(Of DateTime, String), k_limit As Integer, k_keep As HashSet(Of String))
        Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_item As SnapshotManifest In k_newestFirst
            If k_item.FinishedAt = DateTime.MinValue Then Continue For
            If k_seen.Count >= k_limit Then Exit For
            If k_seen.Add(k_bucket(k_item.FinishedAt)) Then k_keep.Add(k_item.Id)
        Next
    End Sub

    ''' <summary>
    ''' Removes ONE copy by the operator's decision, then the objects no other copy uses. Takes the store lock. Returns the bytes freed.
    ''' </summary>
    Public Shared Function DeleteSnapshot(k_store As BackupStore, k_id As String) As Long
        Try
            Using k_lock As IDisposable = k_store.AcquireLock()
                k_store.DeleteSnapshot(k_id)
                Dim k_listing As BackupSnapshotListing = k_store.ListSnapshots()
                ' A copy that cannot be opened may still need objects nobody else names: then nothing is swept (run «Verifică» first).
                If k_listing.Problems.Count > 0 Then Return 0
                Dim k_referenced As New HashSet(Of String)(StringComparer.Ordinal)
                For Each k_item As SnapshotManifest In k_listing.Items
                    For Each k_name As String In BackupStore.ReferencedObjects(k_item)
                        k_referenced.Add(k_name)
                    Next
                Next
                Dim k_freed As Long = 0
                For Each k_name As String In k_store.ObjectNames().ToList()
                    If Not k_referenced.Contains(k_name) Then k_freed += k_store.DeleteObject(k_name)
                Next
                Return k_freed
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("BackupRetention.DeleteSnapshot", ex)
            Throw
        End Try
    End Function

    ''' <summary>Removes the snapshots and the objects of the plan. Takes the store lock, so it cannot run while a copy is being written.</summary>
    Public Shared Function Apply(k_store As BackupStore, k_plan As BackupRetentionPlan, k_progress As Action(Of String)) As Long
        Try
            Using k_lock As IDisposable = k_store.AcquireLock()
                Dim k_freed As Long = 0
                For Each k_item As SnapshotManifest In k_plan.Remove
                    k_progress?.Invoke("Se șterge copia " & k_item.Db & " din " & k_item.Finished & "...")
                    k_store.DeleteSnapshot(k_item.Id)
                Next
                ' The plan was made before the lock: look at the store again so a copy written in between is not treated as garbage.
                Dim k_listing As BackupSnapshotListing = k_store.ListSnapshots()
                If k_listing.Problems.Count > 0 Then Return k_freed       ' see DeleteSnapshot: no sweep while a copy cannot be opened
                Dim k_referenced As New HashSet(Of String)(StringComparer.Ordinal)
                For Each k_item As SnapshotManifest In k_listing.Items
                    For Each k_name As String In BackupStore.ReferencedObjects(k_item)
                        k_referenced.Add(k_name)
                    Next
                Next
                Dim k_count As Integer = 0
                For Each k_name As String In k_store.ObjectNames().ToList()
                    If k_referenced.Contains(k_name) Then Continue For
                    k_freed += k_store.DeleteObject(k_name)
                    k_count += 1
                    If k_count Mod 500 = 0 Then k_progress?.Invoke("Obiecte șterse: " & k_count.ToString(CultureInfo.InvariantCulture))
                Next
                Return k_freed
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("BackupRetention.Apply", ex)
            Throw
        End Try
    End Function

End Class
