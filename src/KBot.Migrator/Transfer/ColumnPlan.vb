Imports KBot.Common

''' <summary>
''' The final INSERT column list for one table: which target columns are written and where
''' each value comes from.
''' </summary>
''' <remarks>
''' <para>
''' Built by ONE function, called identically by the verifier and by the writer. Two
''' copies of the rule drifting apart is exactly how the 0044-04 defects were born: a rule
''' verified on the ROUTES instead of on the RESULT always leaves a path unguarded.
''' </para>
''' <para>
''' Three layers, in the order they beat each other - name match ‹ rename ‹ derived:
''' <list type="number">
''' <item><b>Name match</b> - an Access column whose name equals a WRITABLE target column,
''' case-insensitively.</item>
''' <item><b>Rename</b> - an explicit Access ▸ target pair. It RELEASES the name match it
''' replaces, otherwise Access IDORD would land in both IDORDP and the legacy IDORD.</item>
''' <item><b>Derived</b> - the value does not come from the row (the unit, the year, a
''' resolved id, a forced NULL). Wins over everything.</item>
''' </list>
''' </para>
''' <para>
''' A GENERATED target column is never a candidate, because it is not writable at all -
''' writing one is an error, not a no-op. That single rule accounts for nine of
''' Clasificatii's would-be name matches and Clasificatii_Buget.TOTAL.
''' </para>
''' </remarks>
Public NotInheritable Class ColumnPlan

    Private Sub New(targetTable As String, mappings As List(Of ColumnMapping),
                    skipped As List(Of String), duplicateTargets As List(Of String))
        Me.TargetTable = targetTable
        Me.Mappings = mappings
        Me.Skipped = skipped
        Me.DuplicateTargets = duplicateTargets
    End Sub

    Public ReadOnly Property TargetTable As String
    ''' <summary>Every target column written, with its source, in a stable order.</summary>
    Public ReadOnly Property Mappings As IReadOnlyList(Of ColumnMapping)
    ''' <summary>Access columns that do not travel, each with the reason.</summary>
    Public ReadOnly Property Skipped As IReadOnlyList(Of String)
    ''' <summary>
    ''' Target columns claimed twice at the same priority - always a hard error.
    ''' </summary>
    Public ReadOnly Property DuplicateTargets As IReadOnlyList(Of String)

    ''' <summary>The target column names written, in order.</summary>
    Public Function ColumnNames() As List(Of String)
        Return Mappings.Select(Function(m) m.TargetColumn).ToList()
    End Function

    ''' <summary>
    ''' Resolves one table's column list.
    ''' </summary>
    ''' <param name="map">The catalogue entry.</param>
    ''' <param name="accessColumns">
    ''' The Access column names, with the file's own spelling. Empty for a derived table.
    ''' </param>
    ''' <param name="schema">The live target description.</param>
    Public Shared Function Build(map As TableMap,
                                 accessColumns As IEnumerable(Of String),
                                 schema As TargetSchema) As ColumnPlan
        Try
            Dim writable = schema.WritableColumns(map.TargetTable)
            Dim writableByName As New Dictionary(Of String, TargetColumn)(StringComparer.OrdinalIgnoreCase)
            For Each c In writable
                writableByName(c.Name) = c
            Next

            Dim claimed As New Dictionary(Of String, ColumnMapping)(StringComparer.OrdinalIgnoreCase)
            Dim skipped As New List(Of String)()
            Dim duplicates As New List(Of String)()

            ' --- layer 3: derived, highest priority ---------------------------------
            For Each derived In map.Derived
                If Not writableByName.ContainsKey(derived.TargetColumn) Then
                    ' The target simply has no such column. Not an error - a schema that
                    ' has moved on should not stop a run - but it must be visible.
                    skipped.Add($"{derived.TargetColumn}: ținta nu are coloana (mapare derivată)")
                    Continue For
                End If
                If claimed.ContainsKey(derived.TargetColumn) Then
                    duplicates.Add(derived.TargetColumn)
                    Continue For
                End If
                claimed(derived.TargetColumn) = derived
            Next

            ' Access columns whose value is consumed by a derived mapping should not also
            ' be offered to the name match. Collect them once.
            Dim consumedByDerived As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each derived In map.Derived
                If Not String.IsNullOrEmpty(derived.AccessColumn) Then
                    consumedByDerived.Add(derived.AccessColumn)
                End If
            Next

            ' --- layers 2 and 1, per Access column ----------------------------------
            For Each accessColumn In accessColumns
                If map.Excluded.Contains(accessColumn) Then
                    skipped.Add($"{accessColumn}: exclusă explicit")
                    Continue For
                End If

                Dim targetName As String = Nothing
                Dim viaRename = map.Renames.TryGetValue(accessColumn, targetName)

                If Not viaRename Then
                    ' A renamed Access column releases its name match, so an Access column
                    ' whose name equals a target that some OTHER rename already claims
                    ' must not steal it back.
                    targetName = accessColumn
                End If

                If String.IsNullOrEmpty(targetName) Then
                    skipped.Add($"{accessColumn}: nu călătorește")
                    Continue For
                End If

                Dim targetColumn As TargetColumn = Nothing
                If Not writableByName.TryGetValue(targetName, targetColumn) Then
                    Dim generated = schema.Column(map.TargetTable, targetName)
                    If generated IsNot Nothing AndAlso generated.IsGenerated Then
                        skipped.Add($"{accessColumn}: ținta «{targetName}» e GENERATED, nu se poate scrie")
                    ElseIf viaRename Then
                        skipped.Add($"{accessColumn}: ținta «{targetName}» nu există")
                    Else
                        skipped.Add($"{accessColumn}: nicio coloană cu acest nume pe țintă")
                    End If
                    Continue For
                End If

                Dim existing As ColumnMapping = Nothing
                If claimed.TryGetValue(targetColumn.Name, existing) Then
                    If existing.Kind = ColumnSourceKind.AccessColumn AndAlso Not viaRename Then
                        ' Two Access columns onto one target by plain name match cannot
                        ' happen - one name, one column. If it ever does, it is a hard
                        ' error, not a silent pick.
                        duplicates.Add(targetColumn.Name)
                    ElseIf viaRename AndAlso existing.Kind = ColumnSourceKind.AccessColumn Then
                        ' A rename beats a name match. Record what it displaced.
                        skipped.Add($"{existing.AccessColumn}: înlocuită de redenumirea " &
                                    $"«{accessColumn}» ▸ «{targetColumn.Name}»")
                        claimed(targetColumn.Name) = ColumnMapping.FromAccess(targetColumn.Name, accessColumn)
                    Else
                        ' A derived mapping already owns it. That is the design.
                        skipped.Add($"{accessColumn}: «{targetColumn.Name}» vine dintr-o mapare derivată")
                    End If
                    Continue For
                End If

                If consumedByDerived.Contains(accessColumn) AndAlso Not viaRename Then
                    ' e.g. Access IdClsf feeds the resolved IdClsf; its raw value must not
                    ' also be written by a name match onto the same target.
                    If claimed.ContainsKey(targetColumn.Name) Then
                        skipped.Add($"{accessColumn}: consumată de o mapare derivată")
                        Continue For
                    End If
                End If

                claimed(targetColumn.Name) = ColumnMapping.FromAccess(targetColumn.Name, accessColumn)
            Next

            ' Stable order: the target's own ordinal order, so the INSERT reads like the
            ' table and two runs produce byte-identical SQL.
            Dim ordered = writable.
                Where(Function(c) claimed.ContainsKey(c.Name)).
                Select(Function(c) claimed(c.Name)).
                ToList()

            Return New ColumnPlan(map.TargetTable, ordered, skipped, duplicates)

        Catch ex As Exception
            GlobalErrorLog.Write("ColumnPlan.Build", ex)
            Throw
        End Try
    End Function

End Class
