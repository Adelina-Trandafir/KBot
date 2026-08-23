Imports System.Globalization
Imports System.IO
Imports KBot.Common

''' <summary>
''' One row of <c>cale.accdb</c> ▸ <c>cai</c>: a unit, the DC it belongs to, and the
''' paths to its two data files.
''' </summary>
''' <remarks>
''' Slice 0045-01 read this table. Findings that shape this type:
''' <list type="bullet">
''' <item>The table is spelled <c>cai</c>, lowercase.</item>
''' <item><c>DC</c> is the MariaDB database name verbatim (all 13 rows read
''' <c>000_DEMO</c>). Confirmed on one DC only.</item>
''' <item><c>FullPath</c> and <c>CaleUnitate</c> are RELATIVE (<c>.\...</c>), resolved
''' against the folder holding <c>cale.accdb</c>, and inconsistently terminated - one row
''' has a trailing separator and the next does not.</item>
''' <item><c>CaleForexe</c> is NULL on 2 of the 13 rows. A unit can have nomenclators and
''' no FX file at all.</item>
''' </list>
''' </remarks>
Public NotInheritable Class CaiUnit

    Public Sub New(idUnitate As Integer, dc As String, numeUnitate As String, sursa As String,
                   unitFilePath As String, forexeFilePath As String, anDate As String)
        Me.IdUnitate = idUnitate
        Me.Dc = dc
        Me.NumeUnitate = numeUnitate
        Me.Sursa = sursa
        Me.UnitFilePath = unitFilePath
        Me.ForexeFilePath = forexeFilePath
        Me.AnDate = anDate
    End Sub

    Public ReadOnly Property IdUnitate As Integer
    ''' <summary>The target MariaDB database name.</summary>
    Public ReadOnly Property Dc As String
    ''' <summary>Display label for the operator's unit picker.</summary>
    Public ReadOnly Property NumeUnitate As String
    ''' <summary><c>01A</c> / <c>02A</c> / <c>02E</c>. Feeds Unitati.SursaSector.</summary>
    Public ReadOnly Property Sursa As String
    ''' <summary>Absolute path to this unit's <c>baza&lt;year&gt;.accdb</c>, or empty.</summary>
    Public ReadOnly Property UnitFilePath As String
    ''' <summary>Absolute path to this unit's <c>FX_&lt;year&gt;.accdb</c>, or empty.</summary>
    Public ReadOnly Property ForexeFilePath As String
    ''' <summary>The year as the registry spells it, text. Informational only - the
    ''' transfer year is fixed at 2026 by decision D1.</summary>
    Public ReadOnly Property AnDate As String

    ''' <summary>True when this unit has a Forexe file to read FX_* tables from.</summary>
    Public ReadOnly Property HasForexeFile As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(ForexeFilePath) AndAlso File.Exists(ForexeFilePath)
        End Get
    End Property

    ''' <summary>True when this unit has its own nomenclator file.</summary>
    Public ReadOnly Property HasUnitFile As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(UnitFilePath) AndAlso File.Exists(UnitFilePath)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{IdUnitate} - {NumeUnitate}"
    End Function

End Class

''' <summary>
''' Reads the <c>cai</c> registry out of <c>cale.accdb</c>.
''' </summary>
Public NotInheritable Class CaiRegistry

    Private Const TableName As String = "cai"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Reads every row of <c>cai</c>, resolving the relative paths against the folder
    ''' holding <paramref name="calePath"/>.
    ''' </summary>
    Public Shared Function Read(calePath As String, password As String) As List(Of CaiUnit)
        Try
            Dim baseFolder = Path.GetDirectoryName(Path.GetFullPath(calePath))
            Dim units As New List(Of CaiUnit)()

            Using cn = AccessProvider.Open(calePath, password)
                Dim realName = AccessSchema.ResolveTableName(cn, TableName)
                If realName Is Nothing Then
                    Throw New AccessOpenException(
                        $"Fișierul «{calePath}» nu conține tabelul «{TableName}». " &
                        "Nu este registrul AVACONT.", Nothing)
                End If

                Using reader = AccessSchema.OpenReader(cn, realName)
                    While reader.Read()
                        Dim id = ToInt32OrNothing(reader.ValueOrMissing("IdUnitate"))
                        If Not id.HasValue Then Continue While

                        units.Add(New CaiUnit(
                            id.Value,
                            Text(reader.ValueOrMissing("DC")),
                            Text(reader.ValueOrMissing("NumeUnitate")),
                            Text(reader.ValueOrMissing("SURSA")),
                            ResolvePath(baseFolder, Text(reader.ValueOrMissing("FullPath"))),
                            ResolvePath(baseFolder, Text(reader.ValueOrMissing("CaleForexe"))),
                            Text(reader.ValueOrMissing("AnDate"))))
                    End While
                End Using
            End Using

            units.Sort(Function(a, b) a.IdUnitate.CompareTo(b.IdUnitate))
            Return units

        Catch ex As Exception
            GlobalErrorLog.Write("CaiRegistry.Read", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The distinct DC names in the registry, sorted.
    ''' </summary>
    ''' <remarks>
    ''' One DC in the sample estate (<c>000_DEMO</c>), but the shape is one-to-many and
    ''' the tool must not assume otherwise.
    ''' </remarks>
    Public Shared Function DistinctDcs(units As IEnumerable(Of CaiUnit)) As List(Of String)
        Dim names = units.
            Select(Function(u) u.Dc).
            Where(Function(d) Not String.IsNullOrWhiteSpace(d)).
            Distinct(StringComparer.OrdinalIgnoreCase).
            ToList()
        names.Sort(StringComparer.OrdinalIgnoreCase)
        Return names
    End Function

    ''' <summary>The units of one DC, in id order.</summary>
    Public Shared Function UnitsOf(units As IEnumerable(Of CaiUnit), dc As String) As List(Of CaiUnit)
        Return units.
            Where(Function(u) String.Equals(u.Dc, dc, StringComparison.OrdinalIgnoreCase)).
            OrderBy(Function(u) u.IdUnitate).
            ToList()
    End Function

    ''' <summary>
    ''' Turns a registry path into an absolute one.
    ''' </summary>
    ''' <remarks>
    ''' The registry stores <c>.\ENERGETIC ISJ\baza2026.accdb</c> relative to the folder
    ''' holding cale.accdb, but some rows carry an absolute path (<c>CaleForexe</c> reads
    ''' <c>C:\AVACONT\Forexe\...</c>) and the trailing separator is inconsistent. Both
    ''' shapes have to work.
    ''' </remarks>
    Friend Shared Function ResolvePath(baseFolder As String, stored As String) As String
        If String.IsNullOrWhiteSpace(stored) Then Return String.Empty
        Try
            Dim trimmed = stored.Trim()
            If Path.IsPathRooted(trimmed) Then Return Path.GetFullPath(trimmed)
            If String.IsNullOrEmpty(baseFolder) Then Return trimmed
            Return Path.GetFullPath(Path.Combine(baseFolder, trimmed))
        Catch ex As Exception
            ' A malformed stored path is data, not a crash. Report it as empty and let
            ' the "file does not exist" gate name it.
            GlobalErrorLog.Write("CaiRegistry.ResolvePath", ex)
            Return String.Empty
        End Try
    End Function

    Private Shared Function Text(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function ToInt32OrNothing(value As Object) As Integer?
        If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
        Try
            Return Convert.ToInt32(value, CultureInfo.InvariantCulture)
        Catch ex As Exception
            GlobalErrorLog.Write("CaiRegistry.ToInt32OrNothing", ex)
            Return Nothing
        End Try
    End Function

End Class
