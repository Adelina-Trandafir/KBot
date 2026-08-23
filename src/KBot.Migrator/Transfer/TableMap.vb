''' <summary>Which Access file a table is read from.</summary>
Public Enum SourceFile
    ''' <summary>No Access file at all - built from the cai registry (Unitati).</summary>
    Derived = 0
    ''' <summary>The unit's own <c>baza&lt;year&gt;.accdb</c>: the nomenclators.</summary>
    UnitFile = 1
    ''' <summary>The unit's <c>FX_&lt;year&gt;.accdb</c>: the FX_* families.</summary>
    ForexeFile = 2
End Enum

''' <summary>
''' How one Access table becomes one MariaDB table.
''' </summary>
''' <remarks>
''' <para>
''' Columns travel BY NAME, case-insensitively, unless this map says otherwise. That is
''' the default because MAPARE_ACCESS_MARIADB.md documents only the DDF and ORD families
''' column by column; the rest of the FX_* set is a name match, exactly as the Python
''' side did it. Rule 4: never compare column names with '=', Access spelling is not
''' predictable.
''' </para>
''' <para>
''' Three things override the name match, and all three are explicit:
''' <list type="number">
''' <item><see cref="Renames"/> - the name is wrong or crosses (IDORD ▸ IDORDP).</item>
''' <item><see cref="Excluded"/> - the column must NOT travel even though a target column
''' of that name exists (FX_DDF.IdUnitate, every mirror column, IdClsfPY everywhere).</item>
''' <item><see cref="Derived"/> - the value does not come from the row at all (the unit,
''' the year, a resolved id, a forced NULL).</item>
''' </list>
''' </para>
''' <para>
''' A GENERATED target column is never written and needs no exclusion entry: it is not in
''' <see cref="TargetSchema.WritableColumns"/> at all. That alone accounts for nine of
''' Clasificatii's would-be name matches (Clsf, Titlu, ClsfSal, ClsfF, ClsfE, ClsfX,
''' Sector, Sursa, SS) and Clasificatii_Buget.TOTAL.
''' </para>
''' </remarks>
Public NotInheritable Class TableMap

    Public Sub New(accessTable As String, targetTable As String, source As SourceFile)
        Me.AccessTable = accessTable
        Me.TargetTable = targetTable
        Me.Source = source
        Renames = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Excluded = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Derived = New List(Of ColumnMapping)()
    End Sub

    ''' <summary>The Access table name. Empty for <see cref="SourceFile.Derived"/>.</summary>
    Public ReadOnly Property AccessTable As String
    Public ReadOnly Property TargetTable As String
    Public ReadOnly Property Source As SourceFile

    ''' <summary>Access column ▸ target column, where the name match is wrong.</summary>
    Public ReadOnly Property Renames As Dictionary(Of String, String)

    ''' <summary>Access columns that must not travel.</summary>
    Public ReadOnly Property Excluded As HashSet(Of String)

    ''' <summary>Target columns whose value does not come from the Access row.</summary>
    Public ReadOnly Property Derived As List(Of ColumnMapping)

    ''' <summary>
    ''' True when the table is written once and never upserted.
    ''' </summary>
    ''' <remarks>
    ''' Decision D8, and it applies to exactly one table: Clasificatii's only unique index
    ''' is PRIMARY KEY (IDClsf), so ON DUPLICATE KEY UPDATE has nothing to match
    ''' (IdClsfAcc, IdUnitate) on. Rather than add an index to the server, the run refuses
    ''' to start when the target already holds rows for a selected unit.
    ''' </remarks>
    Public Property InsertOnly As Boolean

    ''' <summary>
    ''' Target column carrying the row count check for <see cref="InsertOnly"/>, and the
    ''' column an existing-rows refusal is counted on. Usually IdUnitate.
    ''' </summary>
    Public Property UnitScopeColumn As String

    ''' <summary>Free-text note shown in the log, explaining anything unusual.</summary>
    Public Property Note As String

    ''' <summary>
    ''' True when neither MAPARE file documents this table column by column, so every
    ''' column travels on a case-insensitive name match.
    ''' </summary>
    ''' <remarks>
    ''' MAPARE_ACCESS_MARIADB.md covers only the DDF and ORD families in detail. A name
    ''' match is the documented default (Rule 4) and is what the Python side did, but it
    ''' is not a READ mapping - so the verifier raises an informational finding naming
    ''' every table in this state, rather than letting it pass unremarked.
    ''' </remarks>
    Public Property NameMatchOnly As Boolean

    ' ---- fluent builders, so the catalogue below reads as a table ----------------

    Public Function Rename(accessColumn As String, targetColumn As String) As TableMap
        Renames(accessColumn) = targetColumn
        Return Me
    End Function

    Public Function Exclude(ParamArray accessColumns As String()) As TableMap
        For Each c In accessColumns
            Excluded.Add(c)
        Next
        Return Me
    End Function

    Public Function Add(mapping As ColumnMapping) As TableMap
        Derived.Add(mapping)
        Return Me
    End Function

    Public Function WithNote(note As String) As TableMap
        Me.Note = note
        Return Me
    End Function

    Public Function AsInsertOnly(unitScopeColumn As String) As TableMap
        InsertOnly = True
        Me.UnitScopeColumn = unitScopeColumn
        Return Me
    End Function

    Public Function ScopedBy(unitScopeColumn As String) As TableMap
        Me.UnitScopeColumn = unitScopeColumn
        Return Me
    End Function

    Public Overrides Function ToString() As String
        Return $"{AccessTable} -> {TargetTable}"
    End Function

End Class
