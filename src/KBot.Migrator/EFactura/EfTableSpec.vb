Imports System.Globalization
Imports KBot.Common

''' <summary>The kind of value a target column holds; decides how an Access value is converted.</summary>
Friend Enum EfKind
    ''' <summary>Text, trimmed; longer than <see cref="EfColumn.MaxLength"/> is a blocking finding (never cut).</summary>
    Text
    ''' <summary>Whole number that fits a 32-bit integer; a fraction is a blocking finding.</summary>
    Whole
    ''' <summary>Decimal with <see cref="EfColumn.Scale"/> places; more places than that are rounded and reported.</summary>
    Amount
    ''' <summary>Yes/No: 1 or 0.</summary>
    Flag
    ''' <summary>A calendar date.</summary>
    DateOnly
    ''' <summary>A date with time of day.</summary>
    DateAndTime
    ''' <summary>Bytes (the stored XML of a received invoice).</summary>
    Binary
End Enum

''' <summary>One column of a target table and where its value comes from.</summary>
Friend NotInheritable Class EfColumn

    Public Sub New(k_target As String, k_source As String, k_kind As EfKind)
        Target = k_target
        Source = k_source
        Kind = k_kind
    End Sub

    ''' <summary>The MariaDB column.</summary>
    Public ReadOnly Property Target As String
    ''' <summary>The Access column, or Nothing when <see cref="Compute"/> produces the value.</summary>
    Public ReadOnly Property Source As String
    Public ReadOnly Property Kind As EfKind
    ''' <summary>Width of a text column; 0 = no limit checked.</summary>
    Public Property MaxLength As Integer
    ''' <summary>Decimal places of an <see cref="EfKind.Amount"/> column.</summary>
    Public Property Scale As Integer
    ''' <summary>The column is NOT NULL and has no default the server would apply.</summary>
    Public Property Required As Boolean
    ''' <summary>What a missing Access value becomes; Nothing = NULL.</summary>
    Public Property DefaultValue As Object
    ''' <summary>The Access column may be absent from the file (old layout); the value is then missing.</summary>
    Public Property OptionalSource As Boolean
    ''' <summary>Produces the raw value from the whole Access row, instead of reading <see cref="Source"/>.</summary>
    Public Property Compute As Func(Of AccessTableReader, Object)

End Class

''' <summary>What a row-level check decided about one Access row.</summary>
Friend Enum EfRowKind
    Keep
    LeaveOut
    Block
End Enum

Friend Structure EfRowDecision
    Public Kind As EfRowKind
    Public Text As String

    Public Shared ReadOnly Property Keep As EfRowDecision
        Get
            Return New EfRowDecision With {.Kind = EfRowKind.Keep}
        End Get
    End Property

    Public Shared ReadOnly Property LeaveOut As EfRowDecision
        Get
            Return New EfRowDecision With {.Kind = EfRowKind.LeaveOut}
        End Get
    End Property

    Public Shared Function Block(k_text As String) As EfRowDecision
        Return New EfRowDecision With {.Kind = EfRowKind.Block, .Text = k_text}
    End Function
End Structure

''' <summary>One Access table copied into one MariaDB table.</summary>
Friend NotInheritable Class EfTableSpec

    Public Sub New(k_sourceTable As String, k_targetTable As String,
                   k_keyColumn As String, k_keySource As String)
        SourceTable = k_sourceTable
        TargetTable = k_targetTable
        KeyColumn = k_keyColumn
        KeySource = k_keySource
        Columns = New List(Of EfColumn)()
        ExtraSourceColumns = New List(Of String)()
        BatchSize = 200
    End Sub

    Public ReadOnly Property SourceTable As String
    Public ReadOnly Property TargetTable As String
    ''' <summary>The target column of the primary key (the duplicate-key rule hangs on it).</summary>
    Public ReadOnly Property KeyColumn As String
    ''' <summary>The Access column that identifies a row in a message.</summary>
    Public ReadOnly Property KeySource As String
    Public ReadOnly Property Columns As List(Of EfColumn)
    ''' <summary>Access columns the filter or a computed column reads, besides the plain sources.</summary>
    Public ReadOnly Property ExtraSourceColumns As List(Of String)
    ''' <summary>True: a row already in the target is updated. False (default): it is left exactly as it is. (No table uses True today.)</summary>
    Public Property UpdateExisting As Boolean
    Public Property BatchSize As Integer
    ''' <summary>Decides on a raw Access row before it is converted. Nothing = every row is kept.</summary>
    Public Property RowCheck As Func(Of AccessTableReader, EfRowDecision)
    ''' <summary>Called with every converted row that will be written (collects keys for the tables below).</summary>
    Public Property AfterRow As Action(Of Object())

    Public Function IndexOf(k_target As String) As Integer
        For i = 0 To Columns.Count - 1
            If String.Equals(Columns(i).Target, k_target, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Throw New ArgumentException($"Coloana «{k_target}» nu există în specificația tabelului {TargetTable}.", NameOf(k_target))
    End Function

    ''' <summary>The Access columns to read, without the binary ones when <paramref name="k_includeBinary"/> is False.</summary>
    Public Function SourceColumnsToRead(k_includeBinary As Boolean) As List(Of String)
        Dim names As New List(Of String)()
        For Each c In Columns
            If c.Source Is Nothing Then Continue For
            If c.Kind = EfKind.Binary AndAlso Not k_includeBinary Then Continue For
            If Not names.Contains(c.Source, StringComparer.OrdinalIgnoreCase) Then names.Add(c.Source)
        Next
        For Each extra In ExtraSourceColumns
            If Not names.Contains(extra, StringComparer.OrdinalIgnoreCase) Then names.Add(extra)
        Next
        If Not String.IsNullOrEmpty(KeySource) AndAlso Not names.Contains(KeySource, StringComparer.OrdinalIgnoreCase) Then names.Add(KeySource)
        Return names
    End Function

End Class

Friend Enum EfIssueKind
    None
    Warning
    Blocking
End Enum

Friend Structure EfIssue
    Public Kind As EfIssueKind
    Public Text As String

    Public Shared ReadOnly Property Fine As EfIssue
        Get
            Return New EfIssue With {.Kind = EfIssueKind.None}
        End Get
    End Property
End Structure

''' <summary>Converts one raw Access value to the value written to MariaDB, or says why it cannot.</summary>
Friend NotInheritable Class EfConverter

    Private Sub New()
    End Sub

    Public Shared Function ToTarget(k_column As EfColumn, k_raw As Object, ByRef k_result As Object) As EfIssue
        k_result = DBNull.Value
        Try
            Dim isMissing As Boolean = (k_raw Is Nothing OrElse k_raw Is DBNull.Value)
            If Not isMissing AndAlso k_column.Kind = EfKind.Text Then
                isMissing = (Convert.ToString(k_raw, CultureInfo.InvariantCulture).Trim().Length = 0 AndAlso Not k_column.Required)
            End If

            If isMissing Then
                If k_column.DefaultValue IsNot Nothing Then
                    k_result = k_column.DefaultValue
                    Return EfIssue.Fine
                End If
                If k_column.Required Then
                    Return Problem(EfIssueKind.Blocking, "valoare lipsă într-o coloană obligatorie")
                End If
                Return EfIssue.Fine
            End If

            Select Case k_column.Kind
                Case EfKind.Text
                    Dim trimmed = Convert.ToString(k_raw, CultureInfo.InvariantCulture).Trim()
                    If k_column.MaxLength > 0 AndAlso trimmed.Length > k_column.MaxLength Then
                        Return Problem(EfIssueKind.Blocking,
                            $"textul are {trimmed.Length} caractere, coloana primește cel mult {k_column.MaxLength}")
                    End If
                    k_result = trimmed

                Case EfKind.Whole
                    Dim d = Convert.ToDecimal(k_raw, CultureInfo.InvariantCulture)
                    If d <> Math.Truncate(d) Then Return Problem(EfIssueKind.Blocking, $"{d} nu este număr întreg")
                    If d < Integer.MinValue OrElse d > Integer.MaxValue Then Return Problem(EfIssueKind.Blocking, $"{d} nu încape într-un număr întreg")
                    k_result = CInt(d)

                Case EfKind.Amount
                    Dim d = Convert.ToDecimal(k_raw, CultureInfo.InvariantCulture)
                    Dim rounded = Math.Round(d, k_column.Scale, MidpointRounding.AwayFromZero)
                    Dim limit = CDec(Math.Pow(10, 18 - k_column.Scale))
                    If Math.Abs(rounded) >= limit Then Return Problem(EfIssueKind.Blocking, $"{d} este prea mare pentru coloană")
                    k_result = rounded
                    If rounded <> d Then
                        Return Problem(EfIssueKind.Warning, $"{d.ToString(CultureInfo.InvariantCulture)} rotunjit la {k_column.Scale} zecimale")
                    End If

                Case EfKind.Flag
                    If TypeOf k_raw Is Boolean Then
                        k_result = If(CBool(k_raw), 1, 0)
                    Else
                        k_result = If(Convert.ToDecimal(k_raw, CultureInfo.InvariantCulture) <> 0D, 1, 0)
                    End If

                Case EfKind.DateOnly
                    Dim dt = Convert.ToDateTime(k_raw, CultureInfo.InvariantCulture)
                    If dt.Year < 1990 OrElse dt.Year > 2100 Then Return Problem(EfIssueKind.Blocking, $"data {dt:dd.MM.yyyy} nu este realistă")
                    k_result = dt.Date

                Case EfKind.DateAndTime
                    Dim dt = Convert.ToDateTime(k_raw, CultureInfo.InvariantCulture)
                    If dt.Year < 1990 OrElse dt.Year > 2100 Then Return Problem(EfIssueKind.Blocking, $"data {dt:dd.MM.yyyy HH:mm} nu este realistă")
                    k_result = dt

                Case EfKind.Binary
                    Dim bytes = TryCast(k_raw, Byte())
                    If bytes Is Nothing Then
                        Dim asText = TryCast(k_raw, String)
                        If asText IsNot Nothing Then bytes = System.Text.Encoding.UTF8.GetBytes(asText)
                    End If
                    If bytes Is Nothing Then Return Problem(EfIssueKind.Blocking, $"conținut de tip {k_raw.GetType().Name} care nu se poate citi ca fișier")
                    k_result = If(bytes.Length = 0, CType(DBNull.Value, Object), bytes)
            End Select
            Return EfIssue.Fine
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
            ' A value that does not convert is a fact about the DATA: it is reported against the row, not thrown.
            k_result = DBNull.Value
            Return Problem(EfIssueKind.Blocking, $"valoarea «{Convert.ToString(k_raw, CultureInfo.InvariantCulture)}» nu se poate converti ({ex.GetType().Name})")
        Catch ex As Exception
            GlobalErrorLog.Write("EfConverter.ToTarget", ex)
            Throw
        End Try
    End Function

    Private Shared Function Problem(k_kind As EfIssueKind, k_text As String) As EfIssue
        Return New EfIssue With {.Kind = k_kind, .Text = k_text}
    End Function

End Class
