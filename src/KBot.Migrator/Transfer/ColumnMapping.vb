''' <summary>Where a target column's value comes from.</summary>
Public Enum ColumnSourceKind
    ''' <summary>Read straight off the Access row.</summary>
    AccessColumn = 0
    ''' <summary>The IdUnitate of the file being read - the nomenclator rows do not carry it.</summary>
    UnitId = 1
    ''' <summary>A fixed value, e.g. An = 2026 (decision D1) or Ascuns = 0.</summary>
    Constant = 2
    ''' <summary>
    ''' An Access Clasificatii.IDClsf resolved, with the unit, to the assigned
    ''' MariaDB Clasificatii.IDClsf. MAPARE_ACCESS_MARIADB.md Rule 1.
    ''' </summary>
    ResolvedClasificatie = 3
    ''' <summary>An Access CodPartener resolved, with the unit, to the assigned IdPartener.</summary>
    ResolvedPartener = 4
    ''' <summary>
    ''' Travels as NULL by decision, never from the row. MAPARE_ACCESS_MARIADB.md §5.2:
    ''' Parteneri is not matched on for the FX_* tables, so their IdPartener is nulled
    ''' and the count is logged per table.
    ''' </summary>
    ForcedNull = 5
    ''' <summary>
    ''' Written outside ColumnPlan entirely, by a hardcoded writer (e.g.
    ''' TransferRunner.WriteUnitati). Carries no real value - it exists only so
    ''' ColumnPlan.Build reports the column as covered, keeping the verifier and the
    ''' actual writer in agreement.
    ''' </summary>
    WrittenElsewhere = 6
End Enum

''' <summary>
''' One target column and where its value comes from.
''' </summary>
Public NotInheritable Class ColumnMapping

    Private Sub New(targetColumn As String, kind As ColumnSourceKind,
                    accessColumn As String, constantValue As Object, blockingOnMiss As Boolean)
        Me.TargetColumn = targetColumn
        Me.Kind = kind
        Me.AccessColumn = accessColumn
        Me.ConstantValue = constantValue
        Me.BlockingOnMiss = blockingOnMiss
    End Sub

    Public ReadOnly Property TargetColumn As String
    Public ReadOnly Property Kind As ColumnSourceKind
    ''' <summary>The Access column read, for the kinds that read one.</summary>
    Public ReadOnly Property AccessColumn As String
    Public ReadOnly Property ConstantValue As Object
    ''' <summary>
    ''' True when a resolution miss stops the whole run rather than writing NULL.
    ''' </summary>
    ''' <remarks>
    ''' Blocking is not optional on FX_DDF_REV_SA.IdClsf and _SB.IdClsf (NOT NULL with a
    ''' foreign key). On FX_ORD_TBL.IdClsf the column is nullable with DEFAULT 0, and
    ''' blocking there is a CHOICE: a silently unclassified order line is worse than a
    ''' refusal.
    ''' </remarks>
    Public ReadOnly Property BlockingOnMiss As Boolean

    Public Shared Function FromAccess(targetColumn As String, accessColumn As String) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.AccessColumn, accessColumn, Nothing, False)
    End Function

    Public Shared Function FromUnit(targetColumn As String) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.UnitId, Nothing, Nothing, True)
    End Function

    Public Shared Function FromConstant(targetColumn As String, value As Object) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.Constant, Nothing, value, False)
    End Function

    Public Shared Function FromClasificatie(targetColumn As String, accessColumn As String,
                                            blocking As Boolean) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.ResolvedClasificatie,
                                 accessColumn, Nothing, blocking)
    End Function

    Public Shared Function FromPartener(targetColumn As String, accessColumn As String,
                                        blocking As Boolean) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.ResolvedPartener,
                                 accessColumn, Nothing, blocking)
    End Function

    Public Shared Function AlwaysNull(targetColumn As String) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.ForcedNull, Nothing, Nothing, False)
    End Function

    Public Shared Function WrittenElsewhere(targetColumn As String) As ColumnMapping
        Return New ColumnMapping(targetColumn, ColumnSourceKind.WrittenElsewhere, Nothing, Nothing, False)
    End Function

    Public Overrides Function ToString() As String
        Select Case Kind
            Case ColumnSourceKind.AccessColumn
                Return $"{AccessColumn} -> {TargetColumn}"
            Case ColumnSourceKind.UnitId
                Return $"(unitate) -> {TargetColumn}"
            Case ColumnSourceKind.Constant
                Return $"(constanta {ConstantValue}) -> {TargetColumn}"
            Case ColumnSourceKind.ResolvedClasificatie
                Return $"{AccessColumn} (clasificatie rezolvata) -> {TargetColumn}"
            Case ColumnSourceKind.ResolvedPartener
                Return $"{AccessColumn} (partener rezolvat) -> {TargetColumn}"
            Case ColumnSourceKind.WrittenElsewhere
                Return $"(scrisă de WriteUnitati) -> {TargetColumn}"
            Case Else
                Return $"(NULL) -> {TargetColumn}"
        End Select
    End Function

End Class
