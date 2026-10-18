''' <summary>Statistics of one measured column (VC05-01). Values are never copied into the report, only counted/measured.</summary>
Public NotInheritable Class VrcFieldStat

    Public Property Name As String = String.Empty
    ''' <summary>Class from the dictionary: VALOARE-INT, COTA, NUMAR, INT, ...</summary>
    Public Property FieldClass As String = String.Empty
    Public Property NonNull As Long
    Public Property Nulls As Long
    Public Property Zeros As Long
    Public Property Negatives As Long
    Public Property Minimum As Double = Double.NaN
    Public Property Maximum As Double = Double.NaN
    ''' <summary>Most decimals any value needs (0..4); 5 = some value needs more than 4; -1 = no value.</summary>
    Public Property MaxDecimals As Integer = -1
    ''' <summary>How many values need exactly N decimals (index = N, 0..5; 5 = more than 4).</summary>
    Public Property ByDecimals As Long() = New Long(5) {}

End Class

''' <summary>What one table of a source looks like against the expected schema.</summary>
Public NotInheritable Class VrcTableProfile

    Public Property Name As String = String.Empty
    ''' <summary>Rows in the source; -1 when the table could not be read.</summary>
    Public Property Rows As Long = -1
    Public Property Columns As New List(Of String)()
    ''' <summary>Expected columns this source does not have. ABSENT is different from NULL (a present column without a value).</summary>
    Public Property Absent As New List(Of String)()
    ''' <summary>Columns the source has beyond the expected schema.</summary>
    Public Property Extra As New List(Of String)()
    Public Property Fields As New List(Of VrcFieldStat)()
    Public Property Problem As String = String.Empty

End Class

''' <summary>NIR / bon de consum headers against the sum of their lines (rule N01/N02): only counted, nothing changed.</summary>
Public NotInheritable Class VrcHeaderCheck

    Public Property HeaderTable As String = String.Empty
    Public Property Headers As Long
    Public Property HeaderZero As Long
    Public Property HeaderNull As Long
    ''' <summary>Headers whose absolute difference to the sum of lines is more than 1 leu (not counting zero/NULL headers).</summary>
    Public Property DiffOverOneLeu As Long
    Public Property MaxAbsDiff As Decimal
    ''' <summary>Headers with no lines at all.</summary>
    Public Property WithoutLines As Long

End Class

''' <summary>Everything the reader learned about one Access file.</summary>
Public NotInheritable Class VrcFileProfile

    Public Property FileName As String = String.Empty
    ''' <summary>Full path of the file as the operator chose it (the same name can exist in several folders).</summary>
    Public Property FullPath As String = String.Empty
    ''' <summary>What the messages show to identify the file: the full path, or the name when the path is not known.</summary>
    Public ReadOnly Property Label As String
        Get
            Return If(FullPath.Length > 0, FullPath, FileName)
        End Get
    End Property
    Public Property Kind As VrcFileKind
    ''' <summary>Exercise year of a year file; 0 for a special base (tz_2100 is not a year).</summary>
    Public Property Year As Integer
    Public Property SizeBytes As Long
    Public Property HashBefore As String = String.Empty
    Public Property HashAfter As String = String.Empty
    Public Property Stable As Boolean
    Public Property Provider As String = String.Empty
    Public Property ReadError As String = String.Empty
    ''' <summary>Tables linked to another file (their data is NOT in this file).</summary>
    Public Property LinkedTables As New List(Of String)()
    Public Property HasVerTable As Boolean
    Public Property OtherTables As New List(Of String)()
    Public Property MissingTables As New List(Of String)()
    Public Property Tables As New List(Of VrcTableProfile)()
    Public Property Config As New SortedDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Public Property HeaderChecks As New List(Of VrcHeaderCheck)()
    Public Property ProductCheck As VrcProductCheck = Nothing
    Public Property Split As VrcSplitCheck = Nothing
    ''' <summary>Smallest year of NIR.Data (0 = unknown). The exercise year of a special base (tz_2100) is this year.</summary>
    Public Property MinNirYear As Integer
    Public Property LineSigns As New List(Of VrcLineSignCheck)()

End Class
