''' <summary>
''' One column of a MariaDB table, as <c>information_schema.COLUMNS</c> reports it.
''' </summary>
''' <remarks>
''' The rules that matter to the transfer are all computed here rather than at each call
''' site, because the same rule checked two ways is how slice 0044-04 produced its
''' defects: a rule verified on the ROUTE instead of on the RESULT always leaves a path
''' unguarded.
''' </remarks>
Public NotInheritable Class TargetColumn

    Public Sub New(name As String, ordinal As Integer, isNullable As Boolean,
                   columnDefault As String, extra As String, dataType As String,
                   columnType As String, characterMaxLength As Long?)
        Me.Name = name
        Me.Ordinal = ordinal
        Me.IsNullable = isNullable
        Me.ColumnDefault = columnDefault
        Me.Extra = If(extra, String.Empty)
        Me.DataType = dataType
        Me.ColumnType = columnType
        Me.CharacterMaxLength = characterMaxLength
    End Sub

    Public ReadOnly Property Name As String
    Public ReadOnly Property Ordinal As Integer
    Public ReadOnly Property IsNullable As Boolean
    ''' <summary>Raw COLUMN_DEFAULT. Nothing means no default at all.</summary>
    Public ReadOnly Property ColumnDefault As String
    ''' <summary>Raw EXTRA: auto_increment, VIRTUAL/STORED GENERATED, on update ...</summary>
    Public ReadOnly Property Extra As String
    Public ReadOnly Property DataType As String
    ''' <summary>Full declared type, e.g. <c>varchar(5)</c> or <c>int(11)</c>.</summary>
    Public ReadOnly Property ColumnType As String
    ''' <summary>Declared character length, or Nothing for non-text columns.</summary>
    Public ReadOnly Property CharacterMaxLength As Long?

    Public ReadOnly Property IsAutoIncrement As Boolean
        Get
            Return Extra.IndexOf("auto_increment", StringComparison.OrdinalIgnoreCase) >= 0
        End Get
    End Property

    ''' <summary>
    ''' True for GENERATED ALWAYS columns, virtual or persistent.
    ''' </summary>
    ''' <remarks>
    ''' Slice 0045-01: Clasificatii carries NINE of these (Clsf, Titlu, ClsfSal, ClsfF,
    ''' ClsfE, ClsfX, Sector, Sursa, SS) and Clasificatii_Buget one (TOTAL). Every one of
    ''' them has an Access column of the same name, so a by-name mapping would try to
    ''' write them - and writing a generated column is an ERROR, not a no-op.
    ''' </remarks>
    Public ReadOnly Property IsGenerated As Boolean
        Get
            Return Extra.IndexOf("GENERATED", StringComparison.OrdinalIgnoreCase) >= 0
        End Get
    End Property

    ''' <summary>True when the server fills this column by itself on insert.</summary>
    Public ReadOnly Property IsServerFilled As Boolean
        Get
            Return IsAutoIncrement OrElse IsGenerated
        End Get
    End Property

    ''' <summary>
    ''' True when this column MUST appear in the INSERT column list.
    ''' </summary>
    ''' <remarks>
    ''' NOT NULL, with no default, and not filled by the server. This is the guard that
    ''' was missing when FX_Angajamente.CodAngajament produced
    ''' <c>1364 ... doesn't have a default value</c> on 21.08 - an error about the COLUMN
    ''' LIST, not about a value (a NULL into a NOT NULL column would be 1048).
    ''' </remarks>
    Public ReadOnly Property IsRequired As Boolean
        Get
            Return Not IsNullable AndAlso ColumnDefault Is Nothing AndAlso Not IsServerFilled
        End Get
    End Property

    ''' <summary>True when a value can be written into this column at all.</summary>
    Public ReadOnly Property IsWritable As Boolean
        Get
            Return Not IsGenerated
        End Get
    End Property

End Class

''' <summary>
''' One foreign key column pair, possibly pointing out of the database.
''' </summary>
''' <remarks>
''' The cross-schema case is not hypothetical: Clasificatii has SIX constraints and FIVE
''' of them reference AVACONT_COMUN (DefaClsfE, DefaClsfF, DefaTitlu, DefaSursaSector,
''' DefaArticol). Four are on GENERATED columns the migrator never writes, so the value
''' being checked is one it cannot see before the INSERT.
''' </remarks>
Public NotInheritable Class TargetForeignKey

    Public Sub New(constraintName As String, childTable As String, childColumn As String,
                   parentSchema As String, parentTable As String, parentColumn As String,
                   ordinal As Integer)
        Me.ConstraintName = constraintName
        Me.ChildTable = childTable
        Me.ChildColumn = childColumn
        Me.ParentSchema = parentSchema
        Me.ParentTable = parentTable
        Me.ParentColumn = parentColumn
        Me.Ordinal = ordinal
    End Sub

    Public ReadOnly Property ConstraintName As String
    Public ReadOnly Property ChildTable As String
    Public ReadOnly Property ChildColumn As String
    ''' <summary>The parent's database. Not always the one being written.</summary>
    Public ReadOnly Property ParentSchema As String
    Public ReadOnly Property ParentTable As String
    Public ReadOnly Property ParentColumn As String
    Public ReadOnly Property Ordinal As Integer

    ''' <summary>True when the parent lives in another database.</summary>
    Public Function IsCrossSchema(currentSchema As String) As Boolean
        Return Not String.Equals(ParentSchema, currentSchema, StringComparison.OrdinalIgnoreCase)
    End Function

End Class

''' <summary>
''' One unique constraint (PRIMARY or UNIQUE), with its columns in order.
''' </summary>
''' <remarks>
''' Needed because <c>ON DUPLICATE KEY UPDATE</c> only matches on a unique key, and one
''' table in scope has none that fits: Clasificatii's only unique index is
''' <c>PRIMARY KEY (IDClsf)</c>, so a match on (IdClsfAcc, IdUnitate) is impossible.
''' Decision D8 makes that table insert-only and refuses a second run.
''' </remarks>
Public NotInheritable Class TargetUniqueKey

    Public Sub New(name As String, columns As IReadOnlyList(Of String), isPrimary As Boolean)
        Me.Name = name
        Me.Columns = columns
        Me.IsPrimary = isPrimary
    End Sub

    Public ReadOnly Property Name As String
    Public ReadOnly Property Columns As IReadOnlyList(Of String)
    Public ReadOnly Property IsPrimary As Boolean

    ''' <summary>True when every column of this key is in the written column list.</summary>
    Public Function IsCoveredBy(written As IEnumerable(Of String)) As Boolean
        Dim set1 As New HashSet(Of String)(written, StringComparer.OrdinalIgnoreCase)
        Return Columns.All(Function(c) set1.Contains(c))
    End Function

End Class
