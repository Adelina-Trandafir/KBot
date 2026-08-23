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
