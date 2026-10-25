Imports KBot.Migrator

''' <summary>
''' The rules that decide whether the ticked units of the grid can be ported as subunits, and in which order. Pure logic over what the
''' form hands in (the grid lines already turned into <see cref="AvcSubunit"/> drafts, what the server holds); the server write stays in
''' <see cref="AvcSubunitStore"/>.
''' </summary>
Friend NotInheritable Class AvcSubunitRules

    Private Sub New()
    End Sub

    Public Shared Function LabelOf(k_unit As CaiUnit) As String
        Return $"{k_unit.IdUnitate} ({k_unit.NumeUnitate})"
    End Function

    ''' <summary>The Romanian reason the drafts cannot be written, or Nothing. The store re-checks what only the server knows.</summary>
    ''' <param name="k_drafts">The lines to port.</param>
    ''' <param name="k_unitOf">IdUnitate ▸ the registry unit of each draft.</param>
    ''' <param name="k_units">Every unit of the DC.</param>
    ''' <param name="k_existing">The subunits the server already holds.</param>
    ''' <param name="k_sectors">The sector+source codes of the common database.</param>
    Public Shared Function Check(k_drafts As List(Of AvcSubunit), k_unitOf As Dictionary(Of Integer, CaiUnit), k_units As IEnumerable(Of CaiUnit),
                                 k_existing As IEnumerable(Of AvcSubunit), k_sectors As IEnumerable(Of String), k_commonDatabase As String) As String
        Dim k_knownSectors As New HashSet(Of String)(k_sectors, StringComparer.OrdinalIgnoreCase)
        ' The denomination is unique; the chapter is not (several subunits may share 65.01 ...).
        Dim k_names As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each k_old In k_existing
            k_names(k_old.Denumire) = $"subunitatea «{k_old.Denumire}»"
        Next
        Dim k_ported As New HashSet(Of Integer)(k_existing.Where(Function(k_s) k_s.IdUnitateAccess > 0).Select(Function(k_s) k_s.IdUnitateAccess))
        Dim k_ticked As New HashSet(Of Integer)(k_drafts.Select(Function(k_d) k_d.IdUnitateAccess))

        For Each k_draft In k_drafts
            Dim k_label = LabelOf(k_unitOf(k_draft.IdUnitateAccess))
            Dim k_reason = AvcSubunitStore.Validate(k_draft)
            If k_reason IsNot Nothing Then Return $"Unitatea {k_label}: {k_reason}"
            If Not k_knownSectors.Contains(k_draft.SursaSector) Then
                Return $"Unitatea {k_label}: sursa «{k_draft.SursaSector}» din registru nu există în «{k_commonDatabase}» (DefaSursaSector)."
            End If
            Dim k_other As String = Nothing
            If k_names.TryGetValue(k_draft.Denumire, k_other) Then Return $"Unitatea {k_label}: denumirea «{k_draft.Denumire}» o are deja {k_other}."
            k_names(k_draft.Denumire) = $"unitatea {k_label}"

            Dim k_parent = k_draft.ParinteUnitateAccess
            If k_parent > 0 AndAlso Not k_ported.Contains(k_parent) AndAlso Not k_ticked.Contains(k_parent) Then
                Dim k_parentUnit = k_units.FirstOrDefault(Function(k_u) k_u.IdUnitate = k_parent)
                If k_parentUnit Is Nothing Then
                    Return $"Unitatea {k_label} are părintele {k_parent}, care nu este în acest DC. Clasificațiile și bugetul lui nu pot fi citite din această bază."
                End If
                Return $"Unitatea {k_label} citește clasificațiile și bugetul unității {LabelOf(k_parentUnit)}, care nu este încă subunitate. Bifați-o și pe ea."
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>The drafts with every parent before its children, or Nothing when the parent links go in a circle.</summary>
    Public Shared Function ParentsFirst(k_drafts As List(Of AvcSubunit), k_existing As IEnumerable(Of AvcSubunit)) As List(Of AvcSubunit)
        Dim k_done As New HashSet(Of Integer)(k_existing.Where(Function(k_s) k_s.IdUnitateAccess > 0).Select(Function(k_s) k_s.IdUnitateAccess))
        Dim k_pending As New List(Of AvcSubunit)(k_drafts)
        Dim k_result As New List(Of AvcSubunit)()
        While k_pending.Count > 0
            Dim k_ready = k_pending.Where(Function(k_d) k_d.ParinteUnitateAccess = 0 OrElse k_done.Contains(k_d.ParinteUnitateAccess)).ToList()
            If k_ready.Count = 0 Then Return Nothing
            For Each k_draft In k_ready
                k_result.Add(k_draft)
                k_pending.Remove(k_draft)
            Next
            For Each k_draft In k_ready
                k_done.Add(k_draft.IdUnitateAccess)
            Next
        End While
        Return k_result
    End Function

End Class
