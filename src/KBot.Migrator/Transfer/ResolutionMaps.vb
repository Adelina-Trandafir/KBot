''' <summary>
''' <c>(Access IdClsf, IdUnitate)</c> ▸ the <c>IDClsf</c> MariaDB assigned.
''' </summary>
''' <remarks>
''' MAPARE_ACCESS_MARIADB.md Rule 1. MariaDB assigns <c>Clasificatii.IDClsf</c>, so the
''' <c>IdClsfPY</c> values sitting in the Access <c>FX_*</c> rows were assigned by the OLD
''' server and mean nothing here. The dumps prove it: classification <c>IdClsf = 123</c>
''' carries <c>IdClsfPY = 1363</c> in Clasificatii but <c>1309</c> in FX_DDF_REV_SA and
''' FX_ORD_TBL. Matching on it would have mismatched silently.
'''
''' The map is built AS THE ROWS ARE WRITTEN, from the id read back, never re-queried per
''' row.
'''
''' The unit half of the key does not come from the nomenclator row - Access
''' <c>Clasificatii</c> has no <c>IdUnitate</c> column at all. It comes from the
''' <c>cai</c> row whose file is open. The <c>FX_*</c> side does carry its own
''' <c>IdUnitate</c>, so its half is a real column read.
''' </remarks>
Public NotInheritable Class ClasificatiiMap

    Private ReadOnly _map As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    Private ReadOnly _misses As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

    ''' <summary>Records the id MariaDB assigned to one Access classification.</summary>
    Public Sub Add(accessIdClsf As Integer, idUnitate As Integer, assignedId As Integer)
        _map(Key(accessIdClsf, idUnitate)) = assignedId
    End Sub

    ''' <summary>
    ''' Resolves one Access classification id. False when the pair was never written.
    ''' </summary>
    Public Function TryResolve(accessIdClsf As Integer, idUnitate As Integer,
                               ByRef assignedId As Integer) As Boolean
        Return _map.TryGetValue(Key(accessIdClsf, idUnitate), assignedId)
    End Function

    ''' <summary>Records a miss, for the report. Counts repeats rather than listing them.</summary>
    Public Sub RecordMiss(accessIdClsf As Integer, idUnitate As Integer)
        Dim k = Key(accessIdClsf, idUnitate)
        Dim count As Integer
        _misses.TryGetValue(k, count)
        _misses(k) = count + 1
    End Sub

    ''' <summary>Every distinct miss, as "IdClsf / unit / row count".</summary>
    Public Function Misses() As List(Of ClasificatieMiss)
        Return _misses.
            Select(Function(kv)
                       Dim parts = kv.Key.Split("|"c)
                       Return New ClasificatieMiss(
                           Integer.Parse(parts(0), Globalization.CultureInfo.InvariantCulture),
                           Integer.Parse(parts(1), Globalization.CultureInfo.InvariantCulture),
                           kv.Value)
                   End Function).
            OrderBy(Function(m) m.IdUnitate).
            ThenBy(Function(m) m.AccessIdClsf).
            ToList()
    End Function

    Public ReadOnly Property Count As Integer
        Get
            Return _map.Count
        End Get
    End Property

    Public Sub ClearMisses()
        _misses.Clear()
    End Sub

    Private Shared Function Key(accessIdClsf As Integer, idUnitate As Integer) As String
        Return accessIdClsf.ToString(Globalization.CultureInfo.InvariantCulture) & "|" &
               idUnitate.ToString(Globalization.CultureInfo.InvariantCulture)
    End Function

End Class

''' <summary>One classification the transferred nomenclator would not cover.</summary>
Public NotInheritable Class ClasificatieMiss

    Public Sub New(accessIdClsf As Integer, idUnitate As Integer, rowCount As Integer)
        Me.AccessIdClsf = accessIdClsf
        Me.IdUnitate = idUnitate
        Me.RowCount = rowCount
    End Sub

    Public ReadOnly Property AccessIdClsf As Integer
    Public ReadOnly Property IdUnitate As Integer
    Public ReadOnly Property RowCount As Integer

End Class

''' <summary>
''' <c>(CodPartener, IdUnitate)</c> ▸ the <c>IdPartener</c> MariaDB assigned.
''' </summary>
''' <remarks>
''' Needed since <c>ParteneriAng</c> ▸ <c>Parteneri_Coduri</c> came into scope (operator
''' decision, 23.08): that table's <c>IdPartener</c> is <c>NOT NULL</c> with a foreign key
''' to <c>Parteneri</c>, and Access only carries the partner CODE.
'''
''' The Access file's own <c>IdPartener</c> column (7605..7621) is NOT this map - those
''' are ids the server assigned on an earlier sync, the same trap as <c>IdClsfPY</c>, and
''' they never travel.
''' </remarks>
Public NotInheritable Class ParteneriMap

    Private ReadOnly _map As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _misses As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

    Public Sub Add(codPartener As String, idUnitate As Integer, assignedId As Integer)
        _map(Key(codPartener, idUnitate)) = assignedId
    End Sub

    Public Function TryResolve(codPartener As String, idUnitate As Integer,
                               ByRef assignedId As Integer) As Boolean
        If String.IsNullOrEmpty(codPartener) Then Return False
        Return _map.TryGetValue(Key(codPartener, idUnitate), assignedId)
    End Function

    Public Sub RecordMiss(codPartener As String, idUnitate As Integer)
        Dim k = Key(If(codPartener, String.Empty), idUnitate)
        Dim count As Integer
        _misses.TryGetValue(k, count)
        _misses(k) = count + 1
    End Sub

    ''' <summary>Every distinct miss, as "code / unit / row count".</summary>
    Public Function Misses() As List(Of PartenerMiss)
        Return _misses.
            Select(Function(kv)
                       Dim cut = kv.Key.LastIndexOf("|"c)
                       Return New PartenerMiss(
                           kv.Key.Substring(0, cut),
                           Integer.Parse(kv.Key.Substring(cut + 1), Globalization.CultureInfo.InvariantCulture),
                           kv.Value)
                   End Function).
            OrderBy(Function(m) m.IdUnitate).
            ThenBy(Function(m) m.CodPartener, StringComparer.OrdinalIgnoreCase).
            ToList()
    End Function

    Public ReadOnly Property Count As Integer
        Get
            Return _map.Count
        End Get
    End Property

    ' The code goes FIRST and the unit last, so the split can be taken from the last
    ' separator - a partner code is operator data and could itself contain '|'.
    Private Shared Function Key(codPartener As String, idUnitate As Integer) As String
        Return If(codPartener, String.Empty) & "|" &
               idUnitate.ToString(Globalization.CultureInfo.InvariantCulture)
    End Function

End Class

''' <summary>One partner code the transferred nomenclator would not cover.</summary>
Public NotInheritable Class PartenerMiss

    Public Sub New(codPartener As String, idUnitate As Integer, rowCount As Integer)
        Me.CodPartener = codPartener
        Me.IdUnitate = idUnitate
        Me.RowCount = rowCount
    End Sub

    Public ReadOnly Property CodPartener As String
    Public ReadOnly Property IdUnitate As Integer
    Public ReadOnly Property RowCount As Integer

End Class

''' <summary>
''' The primary-key values actually written per table, so a child row can be kept or
''' skipped by whether its parent travelled.
''' </summary>
''' <remarks>
''' This replaces slice 0044's hand-built A-E routing maps. Tables are written in
''' topological order, so a parent's key set is always complete before its children are
''' read - which means "did this row's parent travel?" is answerable with no separate
''' map-building pass and no second read of the Access file.
'''
''' Only the six FX_* tables that carry IdUnitate can be filtered directly. The rest reach
''' their unit through this chain.
''' </remarks>
Public NotInheritable Class WrittenKeys

    Private ReadOnly _keys As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Records that one primary-key value was written for a table.</summary>
    Public Sub Add(targetTable As String, keyValue As Object)
        If keyValue Is Nothing OrElse keyValue Is DBNull.Value Then Return
        Dim set1 As HashSet(Of String) = Nothing
        If Not _keys.TryGetValue(targetTable, set1) Then
            set1 = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            _keys(targetTable) = set1
        End If
        set1.Add(Normalise(keyValue))
    End Sub

    ''' <summary>True when this table has any recorded keys at all.</summary>
    Public Function Tracks(targetTable As String) As Boolean
        Return _keys.ContainsKey(targetTable)
    End Function

    ''' <summary>True when this parent key value was written.</summary>
    Public Function Contains(targetTable As String, keyValue As Object) As Boolean
        If keyValue Is Nothing OrElse keyValue Is DBNull.Value Then Return False
        Dim set1 As HashSet(Of String) = Nothing
        If Not _keys.TryGetValue(targetTable, set1) Then Return False
        Return set1.Contains(Normalise(keyValue))
    End Function

    Public Function CountOf(targetTable As String) As Integer
        Dim set1 As HashSet(Of String) = Nothing
        If Not _keys.TryGetValue(targetTable, set1) Then Return 0
        Return set1.Count
    End Function

    ''' <summary>
    ''' One canonical text form per value, so an Integer 1 and a Long 1 are the same key.
    ''' </summary>
    Private Shared Function Normalise(value As Object) As String
        Dim formattable = TryCast(value, IFormattable)
        If formattable IsNot Nothing Then
            Return formattable.ToString(Nothing, Globalization.CultureInfo.InvariantCulture)
        End If
        Return Convert.ToString(value, Globalization.CultureInfo.InvariantCulture)
    End Function

End Class
