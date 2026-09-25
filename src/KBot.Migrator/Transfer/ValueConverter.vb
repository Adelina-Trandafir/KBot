Imports System.Globalization
Imports KBot.Common

''' <summary>
''' Turns an Access value into the parameter value MariaDB receives.
''' </summary>
''' <remarks>
''' <para>
''' ONE translation, called by the writer and by the journal alike. Slice 0044-04 pass 06
''' recorded the alternative: a verifier that CONVERTED in order to judge, standing next to
''' a writer that sent the original, and MariaDB answering 1292. A converter used by only
''' one end always leaves the other unguarded.
''' </para>
''' <para>
''' The .NET OLE DB provider hands back real CLR types - DateTime, Double, Boolean, String,
''' Byte() - not text, so there is none of the locale parsing the mdbtools path needed.
''' That is a real advantage of reading Access directly, and it is why this class is
''' short: the only work left is NULL handling, the tinyint(1) question, and empty text.
''' </para>
''' <para>
''' One exception since slice 0080-01: a date typed into an Access TEXT column arrives as a
''' String, and a MariaDB date column refuses <c>31.12.2025</c> (1292). Such text is read by
''' <see cref="TryReadDate"/>; text that is not a date stops the run with its value named.
''' </para>
''' </remarks>
Public NotInheritable Class ValueConverter

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The value to bind, given the Access value and the target column.
    ''' </summary>
    ''' <remarks>
    ''' Access NULL becomes <see cref="DBNull.Value"/> - never an empty string, never a
    ''' zero. Collapsing NULL into "" is the mistake slice 0044 recorded against
    ''' mdb-export's CSV, and "NULL in a NOT NULL column" is precisely a finding the
    ''' operator must see rather than a value quietly invented.
    ''' </remarks>
    Public Shared Function ToParameter(accessValue As Object, target As TargetColumn) As Object
        Try
            If accessValue Is Nothing OrElse accessValue Is DBNull.Value Then Return DBNull.Value

            ' Access Boolean is -1/0. MariaDB tinyint(1) wants 1/0.
            If TypeOf accessValue Is Boolean Then
                Return If(CBool(accessValue), 1, 0)
            End If

            Dim text = TryCast(accessValue, String)
            If text IsNot Nothing Then
                ' Empty text into a non-text column is a NULL, not a zero: Access writes ""
                ' where it means "nothing" in numeric-ish columns.
                If text.Length = 0 AndAlso target IsNot Nothing AndAlso Not IsTextual(target) Then
                    Return DBNull.Value
                End If
                ' An Access TEXT column holding a date, going into a MariaDB date column
                ' (slice 0080-01: FX_Extrase.DataDoc, which carries 31.12.2025, 31/12/2025
                ' and 2025-12-31 00:00:00 side by side). MariaDB reads none of the first two.
                If IsDateLike(target) Then
                    If text.Trim().Length = 0 Then Return DBNull.Value
                    Dim parsed As DateTime
                    If Not TryReadDate(text, parsed) Then
                        Throw New TransferException(
                            $"Valoarea «{text}» nu poate fi citită ca dată pentru coloana «{target.Name}». " &
                            "Formele acceptate: zz.ll.aaaa, zz/ll/aaaa, aaaa-ll-zz (cu sau fără oră).")
                    End If
                    Return If(IsDateOnly(target), parsed.Date, parsed)
                End If
                Return text
            End If

            Return accessValue

        Catch ex As Exception
            GlobalErrorLog.Write("ValueConverter.ToParameter", ex)
            Throw
        End Try
    End Function

    ''' <summary>True when the target column holds text.</summary>
    Public Shared Function IsTextual(target As TargetColumn) As Boolean
        If target Is Nothing OrElse target.DataType Is Nothing Then Return False
        Select Case target.DataType.ToLowerInvariant()
            Case "char", "varchar", "tinytext", "text", "mediumtext", "longtext", "enum", "set"
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>True when the target column holds a date, with or without a time.</summary>
    Public Shared Function IsDateLike(target As TargetColumn) As Boolean
        If target Is Nothing OrElse target.DataType Is Nothing Then Return False
        Select Case target.DataType.ToLowerInvariant()
            Case "date", "datetime", "timestamp"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function IsDateOnly(target As TargetColumn) As Boolean
        Return String.Equals(target.DataType, "date", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Reads a date typed as text, by the same rules as the Python migrator
    ''' (<c>PYTHON/routes/migrare/parser.py</c>), so the two read an Access file alike.
    ''' </summary>
    ''' <remarks>
    ''' <list type="bullet">
    ''' <item>A first number above 31 is the year: <c>yyyy-mm-dd</c>, nothing to guess.</item>
    ''' <item>Otherwise the third number is the year; two digits below 70 are 20xx, else 19xx.</item>
    ''' <item>«.» and «-» are DAY first. «/» with a four-digit year is DAY first (typed by a
    ''' person on a Romanian system); «/» with a two-digit year is MONTH first (mdbtools'
    ''' own output). A number above 12 settles the order whatever the rule says.</item>
    ''' <item>An optional time follows (<c>hh:mm[:ss][.fff] [AM|PM]</c>, or after a <c>T</c>).</item>
    ''' </list>
    ''' </remarks>
    Public Shared Function TryReadDate(text As String, ByRef result As DateTime) As Boolean
        result = DateTime.MinValue
        If text Is Nothing Then Return False
        Dim m = DatePart.Match(text)
        If Not m.Success Then Return False

        Dim a = Integer.Parse(m.Groups("a").Value, CultureInfo.InvariantCulture)
        Dim b = Integer.Parse(m.Groups("b").Value, CultureInfo.InvariantCulture)
        Dim c = Integer.Parse(m.Groups("c").Value, CultureInfo.InvariantCulture)
        Dim separator = m.Groups("sep").Value

        Dim year, month, day As Integer
        If a > 31 Then
            year = a : month = b : day = c
        Else
            Dim monthFirst As Boolean
            If c < 100 Then
                year = If(c < 70, 2000 + c, 1900 + c)
                monthFirst = True
            Else
                year = c
                monthFirst = False
            End If
            If separator <> "/" Then monthFirst = False

            If a > 12 AndAlso b > 12 Then Return False
            If a > 12 Then
                monthFirst = False
            ElseIf b > 12 Then
                monthFirst = True
            End If
            If monthFirst Then
                month = a : day = b
            Else
                day = a : month = b
            End If
        End If

        If year < 1 OrElse year > 9999 OrElse month < 1 OrElse month > 12 Then Return False
        If day < 1 OrElse day > DateTime.DaysInMonth(year, month) Then Return False

        Dim hour, minute, second As Integer
        If Not TryReadTime(m.Groups("tail").Value, hour, minute, second) Then Return False
        result = New DateTime(year, month, day, hour, minute, second)
        Return True
    End Function

    Private Shared Function TryReadTime(tail As String, ByRef hour As Integer,
                                        ByRef minute As Integer, ByRef second As Integer) As Boolean
        hour = 0 : minute = 0 : second = 0
        If String.IsNullOrWhiteSpace(tail) Then Return True
        Dim m = TimePart.Match(tail)
        If Not m.Success Then Return False
        hour = Integer.Parse(m.Groups("h").Value, CultureInfo.InvariantCulture)
        minute = Integer.Parse(m.Groups("m").Value, CultureInfo.InvariantCulture)
        If m.Groups("s").Success Then second = Integer.Parse(m.Groups("s").Value, CultureInfo.InvariantCulture)
        Dim ampm = m.Groups("ampm").Value.ToLowerInvariant()
        If ampm = "pm" AndAlso hour < 12 Then
            hour += 12
        ElseIf ampm = "am" AndAlso hour = 12 Then
            hour = 0
        End If
        Return hour <= 23 AndAlso minute <= 59 AndAlso second <= 59
    End Function

    Private Shared ReadOnly DatePart As New Text.RegularExpressions.Regex(
        "^\s*(?<a>\d{1,4})\s*(?<sep>[/.\-])\s*(?<b>\d{1,2})\s*\k<sep>\s*(?<c>\d{1,4})(?<tail>.*)$")

    Private Shared ReadOnly TimePart As New Text.RegularExpressions.Regex(
        "^[\sT]*(?<h>\d{1,2}):(?<m>\d{2})(?::(?<s>\d{2}))?(?:\.\d+)?\s*(?<ampm>[AaPp][Mm])?\s*$")

    ''' <summary>
    ''' True when this value counts as an orphan in a foreign-key column whose parent key
    ''' is <c>AUTO_INCREMENT</c>.
    ''' </summary>
    ''' <remarks>
    ''' A zero is an orphan wherever the parent key is auto-increment, because
    ''' auto-increment never assigns 0. Access writes 0 for "no parent" all over the FX_*
    ''' tables - FX_ORD_TBL.IDORDP is 0 on every row - so treating it as a value would
    ''' point every one of them at a row that cannot exist.
    ''' </remarks>
    Public Shared Function IsOrphanValue(value As Object, parentKeyIsAutoIncrement As Boolean) As Boolean
        If value Is Nothing OrElse value Is DBNull.Value Then Return False
        If Not parentKeyIsAutoIncrement Then Return False
        Try
            Dim text = TryCast(value, String)
            If text IsNot Nothing Then
                Dim parsed As Long
                If Long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, parsed) Then
                    Return parsed = 0
                End If
                Return False
            End If
            If TypeOf value Is Byte() Then Return False
            Return Convert.ToInt64(value, CultureInfo.InvariantCulture) = 0
        Catch ex As Exception
            ' A value that is not a number at all is not an orphan zero.
            GlobalErrorLog.Write("ValueConverter.IsOrphanValue", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' A SQL literal for the journal file.
    ''' </summary>
    ''' <remarks>
    ''' <b>Reconstruction, never a transcript.</b> The driver sends parameters, not text, so
    ''' what lands on disk is this method's rendering of the same value - not the bytes on
    ''' the wire. Every journal file says so in its own header.
    ''' </remarks>
    Public Shared Function ToLiteral(value As Object) As String
        Try
            If value Is Nothing OrElse value Is DBNull.Value Then Return "NULL"

            Dim bytes = TryCast(value, Byte())
            If bytes IsNot Nothing Then
                If bytes.Length = 0 Then Return "''"
                Dim sb As New Text.StringBuilder(bytes.Length * 2 + 2)
                sb.Append("0x")
                For Each b In bytes
                    sb.Append(b.ToString("x2", CultureInfo.InvariantCulture))
                Next
                Return sb.ToString()
            End If

            If TypeOf value Is DateTime Then
                Return "'" & CDate(value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) & "'"
            End If

            If TypeOf value Is Boolean Then Return If(CBool(value), "1", "0")

            If TypeOf value Is Byte OrElse TypeOf value Is Short OrElse TypeOf value Is Integer OrElse
               TypeOf value Is Long OrElse TypeOf value Is Single OrElse TypeOf value Is Double OrElse
               TypeOf value Is Decimal Then
                Return Convert.ToString(value, CultureInfo.InvariantCulture)
            End If

            Return "'" & Escape(Convert.ToString(value, CultureInfo.InvariantCulture)) & "'"

        Catch ex As Exception
            GlobalErrorLog.Write("ValueConverter.ToLiteral", ex)
            ' The journal must never be the thing that breaks a migration.
            Return "'<valoare nereprezentabilă>'"
        End Try
    End Function

    ''' <summary>Escapes a string for a single-quoted MariaDB literal.</summary>
    Private Shared Function Escape(value As String) As String
        If value Is Nothing Then Return String.Empty
        Dim sb As New Text.StringBuilder(value.Length + 8)
        For Each c In value
            Select Case c
                Case ChrW(0) : sb.Append("\0")
                Case ChrW(8) : sb.Append("\b")
                Case ChrW(10) : sb.Append("\n")
                Case ChrW(13) : sb.Append("\r")
                Case ChrW(9) : sb.Append("\t")
                Case ChrW(26) : sb.Append("\Z")
                Case """"c : sb.Append("\""")
                Case "'"c : sb.Append("\'")
                Case "\"c : sb.Append("\\")
                Case Else : sb.Append(c)
            End Select
        Next
        Return sb.ToString()
    End Function

End Class
