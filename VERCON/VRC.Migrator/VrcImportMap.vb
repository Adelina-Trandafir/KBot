Imports System.IO
Imports System.Reflection
Imports System.Text.Json
Imports KBot.Common
Imports KBot.Migrator

''' <summary>How a key column of a source row is carried into the target.</summary>
Public Enum VrcLinkKind
    ''' <summary>Not a key: converted by its class (value, text, date, ...).</summary>
    None = 0
    ''' <summary>Declared Access relation. Remapped through the id map of the parent table.</summary>
    ForeignKey = 1
    ''' <summary>Documented Access reference without a declared source relation; also remapped.</summary>
    Logical = 2
    ''' <summary>OI document label: a typed source identity, allocated from one common sequence per gestiune.</summary>
    Group = 3
    ''' <summary>NIR.IDF: the supplier, matched to a KBOT partner by tax code.</summary>
    Supplier = 4
    ''' <summary>Left out on purpose (the supplier columns replaced by IdPartener).</summary>
    Skip = 5
    ''' <summary>A key column nobody has classified: allowed only while every source value is NULL or 0.</summary>
    Open = 6
    ''' <summary>A technical row number that is not the primary key (an extra AutoNumber): not a reference, stored as NULL.</summary>
    Technical = 7
    ''' <summary>Meal IdFel is derived from the source Tip label, never from the Access IdFel.</summary>
    MealType = 8
    ''' <summary>OI_Produse.IDBO: the bon the object came from, a BP (IDBP of OI_Predare) or a BT (IDBT of OI_Transfer); the source value does not say which, so it is looked up in both.</summary>
    Origin = 9
    ''' <summary>OI_Produse.IDBC: the current BP/BT/BS, selected using the row's IDBP and IDBT markers.</summary>
    OiDocument = 10
End Enum

Public NotInheritable Class VrcImportColumn

    Public Property Name As String = String.Empty
    ''' <summary>Class from the dictionary: VALOARE-INT, COTA, NUMAR, INT, CHEIE, TEXT, DATA, BOOL.</summary>
    Public Property FieldClass As String = String.Empty
    Public Property Link As VrcLinkKind
    ''' <summary>Parent table (Access name) for <see cref="VrcLinkKind.ForeignKey"/> and <see cref="VrcLinkKind.Logical"/>.</summary>
    Public Property Parent As String = String.Empty
    ''' <summary>Group namespace (the Access column name) for <see cref="VrcLinkKind.Group"/>.</summary>
    Public Property GroupName As String = String.Empty
    ''' <summary>Key column of the parent table named by the map (for the report's Access queries); empty = the parent's source key.</summary>
    Public Property ParentField As String = String.Empty
    ''' <summary>True when a value without a parent does NOT block: the row is written with the link empty (NULL) and the case is noted (map key «orphan»: «null»).</summary>
    Public Property OrphanToNull As Boolean

End Class

Public NotInheritable Class VrcImportTable

    ''' <summary>Access table name.</summary>
    Public Property Name As String = String.Empty
    ''' <summary>MariaDB table name (VC_...).</summary>
    Public Property TargetName As String = String.Empty
    ''' <summary>True for tables keyed by gestiune + exercise; false for gestiune-level tables (shared by all years).</summary>
    Public Property IsExercise As Boolean
    ''' <summary>Source-row identity policy; empty when rows are tracked by a synthetic source key.</summary>
    Public Property SourceKey As String = String.Empty
    Public Property Columns As New List(Of VrcImportColumn)()

End Class

''' <summary>
''' Access import semantics only: source keys, conversion classes and reference remapping.
''' The physical target schema is read by KBot.Migrator.TargetSchema, never from this resource.
''' </summary>
Public NotInheritable Class VrcImportMap

    Private Shared _tables As List(Of VrcImportTable) = Nothing

    Private Sub New()
    End Sub

    Public Shared Function Tables() As List(Of VrcImportTable)
        Try
            If _tables IsNot Nothing Then Return _tables
            Dim k_result As New List(Of VrcImportTable)()
            Using k_stream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRC.Migrator.import_map.json")
                If k_stream Is Nothing Then Throw New InvalidOperationException("Maparea importului Access lipsește din executabil.")
                Using k_doc As JsonDocument = JsonDocument.Parse(k_stream)
                    For Each k_t In k_doc.RootElement.GetProperty("tables").EnumerateArray()
                        Dim k_table As New VrcImportTable() With {
                            .Name = k_t.GetProperty("name").GetString(),
                            .TargetName = k_t.GetProperty("vname").GetString(),
                            .IsExercise = k_t.GetProperty("scope").GetString() = "E"}
                        Dim k_pk As JsonElement = k_t.GetProperty("source_key")
                        If k_pk.ValueKind = JsonValueKind.String Then k_table.SourceKey = k_pk.GetString()
                        For Each k_c In k_t.GetProperty("columns").EnumerateArray()
                            Dim k_column As New VrcImportColumn() With {
                                .Name = k_c.GetProperty("name").GetString(),
                                .FieldClass = k_c.GetProperty("cls").GetString()}
                            Dim k_link As JsonElement = k_c.GetProperty("link")
                            If k_link.ValueKind = JsonValueKind.Object Then
                                Dim k_parentField As JsonElement
                                If k_link.TryGetProperty("parentField", k_parentField) AndAlso k_parentField.ValueKind = JsonValueKind.String Then k_column.ParentField = k_parentField.GetString()
                                Dim k_orphan As JsonElement
                                If k_link.TryGetProperty("orphan", k_orphan) AndAlso k_orphan.ValueKind = JsonValueKind.String Then k_column.OrphanToNull = String.Equals(k_orphan.GetString(), "null", StringComparison.OrdinalIgnoreCase)
                                Select Case k_link.GetProperty("kind").GetString()
                                    Case "fk"
                                        k_column.Link = VrcLinkKind.ForeignKey
                                        k_column.Parent = k_link.GetProperty("parent").GetString()
                                    Case "logical"
                                        k_column.Link = VrcLinkKind.Logical
                                        k_column.Parent = k_link.GetProperty("parent").GetString()
                                    Case "group"
                                        k_column.Link = VrcLinkKind.Group
                                        k_column.GroupName = k_link.GetProperty("group").GetString()
                                    Case "supplier"
                                        k_column.Link = VrcLinkKind.Supplier
                                    Case "skip"
                                        k_column.Link = VrcLinkKind.Skip
                                    Case "tech"
                                        k_column.Link = VrcLinkKind.Technical
                                    Case "meal_type"
                                        k_column.Link = VrcLinkKind.MealType
                                    Case "origin"
                                        k_column.Link = VrcLinkKind.Origin
                                    Case "oi_document"
                                        k_column.Link = VrcLinkKind.OiDocument
                                    Case Else
                                        k_column.Link = VrcLinkKind.Open
                                End Select
                            End If
                            k_table.Columns.Add(k_column)
                        Next
                        k_result.Add(k_table)
                    Next
                End Using
            End Using
            _tables = k_result
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("VrcImportMap.Tables", ex)
            Throw
        End Try
    End Function

    Public Shared Function Find(k_name As String) As VrcImportTable
        Return Tables().FirstOrDefault(Function(k_t) String.Equals(k_t.Name, k_name, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>Use the same live-FK sorter as KBot.Migrator; also guard Access-only remapping dependencies.</summary>
    Public Shared Function OrderedTables(k_schema As TargetSchema) As List(Of VrcImportTable)
        Dim k_tables = Tables()
        Dim k_byTarget = k_tables.ToDictionary(Function(k_t) k_t.TargetName, StringComparer.OrdinalIgnoreCase)
        Dim k_dependencies As New List(Of (Parent As String, Child As String))()
        For Each k_child In k_tables
            For Each k_column In k_child.Columns.Where(Function(k_c) k_c.Link = VrcLinkKind.ForeignKey OrElse k_c.Link = VrcLinkKind.Logical)
                Dim k_parent = Find(k_column.Parent)
                If k_parent Is Nothing OrElse k_parent Is k_child Then Continue For
                If Not k_child.IsExercise AndAlso k_parent.IsExercise Then
                    Throw New InvalidOperationException($"Maparea Access {k_child.Name}.{k_column.Name} → {k_parent.Name} cere un document anual înaintea unui nomenclator. Actualizați maparea înainte de import.")
                End If
                k_dependencies.Add((k_parent.TargetName, k_child.TargetName))
            Next
        Next
        For Each k_fk In k_schema.ForeignKeys.Where(Function(k_f) Not k_f.IsCrossSchema(k_schema.SchemaName))
            Dim k_child As VrcImportTable = Nothing, k_parent As VrcImportTable = Nothing
            If k_byTarget.TryGetValue(k_fk.ChildTable, k_child) AndAlso k_byTarget.TryGetValue(k_fk.ParentTable, k_parent) AndAlso
               Not k_child.IsExercise AndAlso k_parent.IsExercise Then
                Throw New InvalidOperationException($"FK {k_fk.ConstraintName} cere un document anual înaintea nomenclatorului {k_child.TargetName}; fazele importului sunt incompatibile cu schema reală.")
            End If
        Next
        Dim k_order = WriteOrder.Derive(k_schema, k_tables.Select(Function(k_t) k_t.TargetName), k_dependencies)
        Return k_order.Select(Function(k_name) k_byTarget(k_name)).ToList()
    End Function

End Class
