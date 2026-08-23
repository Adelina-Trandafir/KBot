Imports System.Data.OleDb

''' <summary>
''' A streaming reader over one Access table that owns its command as well as its
''' reader, so a single Using disposes both.
''' </summary>
''' <remarks>
''' Column lookup is case-insensitive by name, because Access spelling is not
''' predictable (MAPARE_ACCESS_MARIADB.md Rule 4). The ordinal map is built once per
''' reader rather than per row.
''' </remarks>
Public NotInheritable Class AccessTableReader
    Implements IDisposable

    Private ReadOnly _command As OleDbCommand
    Private ReadOnly _reader As OleDbDataReader
    Private ReadOnly _ordinals As Dictionary(Of String, Integer)
    Private _disposed As Boolean

    Friend Sub New(command As OleDbCommand, reader As OleDbDataReader)
        _command = command
        _reader = reader
        _ordinals = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i = 0 To reader.FieldCount - 1
            ' A duplicate name cannot happen in an Access table, but indexing rather than
            ' Add() keeps this from throwing if one ever does.
            _ordinals(reader.GetName(i)) = i
        Next
    End Sub

    ''' <summary>Column names in ordinal order, with the file's own spelling.</summary>
    Public ReadOnly Property ColumnNames As IReadOnlyList(Of String)
        Get
            Dim names As New List(Of String)(_reader.FieldCount)
            For i = 0 To _reader.FieldCount - 1
                names.Add(_reader.GetName(i))
            Next
            Return names
        End Get
    End Property

    ''' <summary>Advances to the next row. False at the end.</summary>
    Public Function Read() As Boolean
        Return _reader.Read()
    End Function

    ''' <summary>True when the table carries this column, case-insensitively.</summary>
    Public Function HasColumn(name As String) As Boolean
        Return _ordinals.ContainsKey(name)
    End Function

    ''' <summary>
    ''' The raw value of one column on the current row, with Access NULL as
    ''' <see cref="DBNull.Value"/>.
    ''' </summary>
    ''' <exception cref="ArgumentException">The column is not in this table.</exception>
    Public Function Value(name As String) As Object
        Dim ordinal As Integer
        If Not _ordinals.TryGetValue(name, ordinal) Then
            Throw New ArgumentException($"Coloana «{name}» nu există în acest tabel Access.", NameOf(name))
        End If
        Return _reader.GetValue(ordinal)
    End Function

    ''' <summary>
    ''' The value of one column, or Nothing when the column is absent from the table.
    ''' </summary>
    ''' <remarks>
    ''' Absent and NULL are deliberately different: absent returns Nothing, a real NULL
    ''' returns <see cref="DBNull.Value"/>. Collapsing the two is exactly the mistake
    ''' slice 0044 recorded against mdb-export's CSV.
    ''' </remarks>
    Public Function ValueOrMissing(name As String) As Object
        Dim ordinal As Integer
        If Not _ordinals.TryGetValue(name, ordinal) Then Return Nothing
        Return _reader.GetValue(ordinal)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _reader.Dispose()
        _command.Dispose()
    End Sub

End Class
