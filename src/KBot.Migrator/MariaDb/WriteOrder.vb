Imports KBot.Common

''' <summary>
''' A write order could not be derived, because the tables in scope form a cycle.
''' </summary>
Public NotInheritable Class WriteOrderCycleException
    Inherits Exception

    Public Sub New(message As String, tables As IReadOnlyList(Of String))
        MyBase.New(message)
        Me.Tables = tables
    End Sub

    ''' <summary>The tables still unplaced when the sort stalled.</summary>
    Public ReadOnly Property Tables As IReadOnlyList(Of String)

End Class

''' <summary>
''' Derives the order tables must be written in, from the target's live foreign keys.
''' </summary>
''' <remarks>
''' Derived, never hardcoded. A foreign key needs the referenced row PRESENT at INSERT
''' time, so emptying the parent does not help - that is exactly how the 21.08 run got
''' <c>1452 ... FX_Rezervari__FX_DDF_REV</c>, while FX_Istoric had written 3246 rows a
''' second earlier only because FX_Istoric.IDREV carries no constraint.
'''
''' The expected result on today's schema is in PLAN_MigratorDirect.md §6. That list is
''' there to CHECK the sort against, not to replace it: a constraint added on the server
''' changes the order with no code change, and the constraint that failed that run did
''' not exist in the schema copy held at the time.
'''
''' Self-references are ignored - a row pointing at its own table is ordered within the
''' table, not between tables, and treating it as an edge would report a false cycle.
''' Cross-database parents are ignored too: no ordering inside this database can satisfy
''' them, so they are a separate gate (see <see cref="TargetSchema.CrossSchemaForeignKeys"/>).
''' </remarks>
Public NotInheritable Class WriteOrder

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Sorts <paramref name="tablesInScope"/> so every parent precedes its children.
    ''' </summary>
    ''' <param name="schema">The live target description.</param>
    ''' <param name="tablesInScope">
    ''' The tables actually being written. A foreign key onto a table NOT in scope is not
    ''' an ordering constraint - it is an existence question, answered by the orphan gate.
    ''' </param>
    ''' <exception cref="WriteOrderCycleException">The tables in scope form a cycle.</exception>
    Public Shared Function Derive(schema As TargetSchema,
                                  tablesInScope As IEnumerable(Of String)) As List(Of String)
        Try
            Dim scope As New List(Of String)(tablesInScope)
            Dim inScope As New HashSet(Of String)(scope, StringComparer.OrdinalIgnoreCase)

            ' parent -> children, and a pending-parent count per table.
            Dim children As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)
            Dim pending As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim edges As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each t In scope
                children(t) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                pending(t) = 0
            Next

            For Each fk In schema.ForeignKeys
                If fk.IsCrossSchema(schema.SchemaName) Then Continue For
                If Not inScope.Contains(fk.ChildTable) Then Continue For
                If Not inScope.Contains(fk.ParentTable) Then Continue For
                If String.Equals(fk.ChildTable, fk.ParentTable, StringComparison.OrdinalIgnoreCase) Then Continue For

                ' A composite key produces one row per column; the edge is the same one.
                Dim edge = fk.ParentTable & " -> " & fk.ChildTable
                If Not edges.Add(edge) Then Continue For

                children(fk.ParentTable).Add(fk.ChildTable)
                pending(fk.ChildTable) += 1
            Next

            ' Kahn, taking ready tables in the caller's original order so a run with no
            ' constraints at all reproduces the list it was given rather than reordering
            ' it arbitrarily.
            Dim ordered As New List(Of String)()
            Dim placed As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            While ordered.Count < scope.Count
                Dim progressed = False

                For Each t In scope
                    If placed.Contains(t) Then Continue For
                    If pending(t) <> 0 Then Continue For

                    ordered.Add(t)
                    placed.Add(t)
                    progressed = True

                    For Each child In children(t)
                        pending(child) -= 1
                    Next
                Next

                If Not progressed Then
                    Dim stuck = scope.Where(Function(t) Not placed.Contains(t)).ToList()
                    Throw New WriteOrderCycleException(
                        "Tabelele alese formează un ciclu de chei străine, deci nu există " &
                        "nicio ordine de scriere validă: " & String.Join(", ", stuck) & ".",
                        stuck)
                End If
            End While

            Return ordered

        Catch ex As Exception
            GlobalErrorLog.Write("WriteOrder.Derive", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Names the constraints an operator-supplied arrangement violates: a child placed
    ''' before a parent that is also in scope.
    ''' </summary>
    ''' <returns>One line per violation. Empty when the arrangement is sound.</returns>
    Public Shared Function Violations(schema As TargetSchema,
                                      arrangement As IReadOnlyList(Of String)) As List(Of String)
        Try
            Dim position As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For i = 0 To arrangement.Count - 1
                position(arrangement(i)) = i
            Next

            Dim found As New List(Of String)()
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each fk In schema.ForeignKeys
                If fk.IsCrossSchema(schema.SchemaName) Then Continue For
                If String.Equals(fk.ChildTable, fk.ParentTable, StringComparison.OrdinalIgnoreCase) Then Continue For

                Dim childAt As Integer, parentAt As Integer
                If Not position.TryGetValue(fk.ChildTable, childAt) Then Continue For
                If Not position.TryGetValue(fk.ParentTable, parentAt) Then Continue For
                If childAt > parentAt Then Continue For

                If Not seen.Add(fk.ConstraintName) Then Continue For
                found.Add($"«{fk.ChildTable}» este scris înaintea părintelui «{fk.ParentTable}» " &
                          $"(constrângerea «{fk.ConstraintName}»).")
            Next

            Return found

        Catch ex As Exception
            GlobalErrorLog.Write("WriteOrder.Violations", ex)
            Throw
        End Try
    End Function

End Class
