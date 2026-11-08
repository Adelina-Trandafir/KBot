Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common
Imports KBot.Migrator

' Slice AV02-02 (migration): what the Access tables PlatiFacturi, OPuri and OPME of the ticked units hold, read into memory, plus the two flags that make the setting
' PlajaComunaOPuri (the row _OP9 of Scheme, column C6, and LEGUNIT.OP). Amounts are rounded to cents; the history is moved AS IT IS.

''' <summary>One allocation of a payment on an invoice (Access PlatiFacturi).</summary>
Friend NotInheritable Class AvcAlocareRow
    Public Property IdUnitate As Integer
    Public Property Id As Integer
    Public Property IdPlata As Integer?
    Public Property IdFactura As Integer?
    Public Property IdFacturaAA As Integer?
    Public Property IdOp As Integer?
    Public Property IdEft As Integer?
    Public Property ValoarePlata As Decimal
    Public Property ValoareFactura As Decimal
    Public Property Platit As Decimal
    Public Property Diferenta As Decimal
    Public Property DataPlata As Date?
    Public Property DtDoc As Date?
    Public Property NrDoc As String = String.Empty
    Public Property NrAl As Decimal?
    Public Property Numerar As Boolean
    Public Property AlteDetalii As String = String.Empty
    Public Property Dtq As Date?
End Class

''' <summary>One order of payment (Access OPuri).</summary>
Friend NotInheritable Class AvcOpRow
    Public Property IdUnitate As Integer
    Public Property IdOp As Integer
    Public Property IdOpme As Integer?
    Public Property Oper As Integer?
    Public Property NrOp As Integer?
    Public Property Data As Date?
    Public Property Valoare As Decimal
    Public Property Documente As String = String.Empty
    Public Property CodPartener As String = String.Empty
    Public Property CfPartener As String = String.Empty
    Public Property DenPartener As String = String.Empty
    Public Property ContBeneficiar As String = String.Empty
    Public Property BBic As String = String.Empty
    Public Property CfUnitate As String = String.Empty
    Public Property DenUnitate As String = String.Empty
    Public Property ContUnitate As String = String.Empty
    Public Property PBic As String = String.Empty
    Public Property Explicatie As String = String.Empty
    Public Property CodAngajament As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property CodProgram As String = String.Empty
    Public Property Auto As Boolean
    Public Property DataIn As Date?
    Public Property Dtq As Date?
End Class

''' <summary>One header of a multiple order of payment (Access OPME).</summary>
Friend NotInheritable Class AvcOpmeRow
    Public Property IdUnitate As Integer
    Public Property IdOpme As Integer
    Public Property NrOpme As Integer
    Public Property DataDoc As Date?
    Public Property NrRec As Integer
    Public Property Total As Decimal
    Public Property Platitor As String = String.Empty
    Public Property Cif As String = String.Empty
    Public Property Adresa As String = String.Empty
    Public Property Dtq As Date?
End Class

''' <summary>The flags of one unit that make the setting PlajaComunaOPuri.</summary>
Friend NotInheritable Class AvcPlajaRow
    Public Property IdUnitate As Integer
    ''' <summary><c>Scheme</c> row <c>_OP9</c>, column <c>C6</c>: the OP numbers are common with other units.</summary>
    Public Property Comuna As Boolean
    ''' <summary>The units whose <c>LEGUNIT.OP</c> is true in this unit's file (they share the range).</summary>
    Public ReadOnly Property Legate As New List(Of Integer)()
End Class

Friend NotInheritable Class AvcPlatiSet
    Public ReadOnly Property Alocari As New List(Of AvcAlocareRow)()
    Public ReadOnly Property Opuri As New List(Of AvcOpRow)()
    Public ReadOnly Property Opme As New List(Of AvcOpmeRow)()
    Public ReadOnly Property Plaje As New List(Of AvcPlajaRow)()
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    Public ReadOnly Property MissingColumns As New List(Of String)()
End Class

''' <summary>Reads PlatiFacturi, OPuri, OPME and the range flags of every ticked unit's file.</summary>
Friend NotInheritable Class AvcPlatiReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "PlatiFacturi"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcPlatiSet
        Try
            Dim k_result As New AvcPlatiSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
                    Dim k_plati As String = AccessSchema.ResolveTableName(k_cn, "PlatiFacturi")
                    Dim k_opuri As String = AccessSchema.ResolveTableName(k_cn, "OPuri")
                    If k_plati Is Nothing OrElse k_opuri Is Nothing Then
                        k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                        Continue For
                    End If
                    ReadAllocations(k_cn, k_plati, k_unit.IdUnitate, k_result)
                    ReadOrders(k_cn, k_opuri, k_unit.IdUnitate, k_result)
                    Dim k_opme As String = AccessSchema.ResolveTableName(k_cn, "OPME")
                    If k_opme IsNot Nothing Then ReadOpme(k_cn, k_opme, k_unit.IdUnitate, k_result)
                    ReadRange(k_cn, k_unit.IdUnitate, k_result)
                End Using
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlatiReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadAllocations(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_result As AvcPlatiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, "PlatiFacturi", {"ID", "IdPlata", "IdFactura", "IdFacturaAA", "IDOP", "Platit"}, k_result)
            While k_reader.Read()
                Dim k_id = Int(k_reader.ValueOrMissing("ID"))
                If Not k_id.HasValue Then Continue While
                k_result.Alocari.Add(New AvcAlocareRow() With {
                    .IdUnitate = k_unit, .Id = k_id.Value, .IdPlata = Pos(k_reader.ValueOrMissing("IdPlata")), .IdFactura = Pos(k_reader.ValueOrMissing("IdFactura")),
                    .IdFacturaAA = Pos(k_reader.ValueOrMissing("IdFacturaAA")), .IdOp = Pos(k_reader.ValueOrMissing("IDOP")), .IdEft = Pos(k_reader.ValueOrMissing("IDEFT")),
                    .ValoarePlata = Dec(k_reader.ValueOrMissing("ValoarePlata")), .ValoareFactura = Dec(k_reader.ValueOrMissing("ValoareFactura")),
                    .Platit = Dec(k_reader.ValueOrMissing("Platit")), .Diferenta = Dec(k_reader.ValueOrMissing("Diferenta")),
                    .DataPlata = Dat(k_reader.ValueOrMissing("DataPlata")), .DtDoc = Dat(k_reader.ValueOrMissing("DtDoc")), .NrDoc = AvcSql.AsText(k_reader.ValueOrMissing("NrDoc")),
                    .NrAl = DecOrNothing(k_reader.ValueOrMissing("NrAL")), .Numerar = Yes(k_reader.ValueOrMissing("Numerar")),
                    .AlteDetalii = AvcSql.AsText(k_reader.ValueOrMissing("AlteDetalii")), .Dtq = Dat(k_reader.ValueOrMissing("DTQ"))})
            End While
        End Using
    End Sub

    Private Shared Sub ReadOrders(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_result As AvcPlatiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, "OPuri", {"IDOP", "Oper", "NrOP", "Data", "Valoare", "Explicatie"}, k_result)
            While k_reader.Read()
                Dim k_id = Int(k_reader.ValueOrMissing("IDOP"))
                If Not k_id.HasValue Then Continue While
                k_result.Opuri.Add(New AvcOpRow() With {
                    .IdUnitate = k_unit, .IdOp = k_id.Value, .IdOpme = Pos(k_reader.ValueOrMissing("IDOPME")), .Oper = Pos(k_reader.ValueOrMissing("Oper")),
                    .NrOp = Pos(k_reader.ValueOrMissing("NrOP")), .Data = Dat(k_reader.ValueOrMissing("Data")), .Valoare = Dec(k_reader.ValueOrMissing("Valoare")),
                    .Documente = AvcSql.AsText(k_reader.ValueOrMissing("DOCUMENTE")), .CodPartener = AvcSql.AsText(k_reader.ValueOrMissing("CodPartener")),
                    .CfPartener = AvcSql.AsText(k_reader.ValueOrMissing("CFPartener")), .DenPartener = AvcSql.AsText(k_reader.ValueOrMissing("DenPartener")),
                    .ContBeneficiar = AvcSql.AsText(k_reader.ValueOrMissing("ContBeneficiar")), .BBic = AvcSql.AsText(k_reader.ValueOrMissing("bBIC")),
                    .CfUnitate = AvcSql.AsText(k_reader.ValueOrMissing("CFUnitate")), .DenUnitate = AvcSql.AsText(k_reader.ValueOrMissing("DenUnitate")),
                    .ContUnitate = AvcSql.AsText(k_reader.ValueOrMissing("ContUnitate")), .PBic = AvcSql.AsText(k_reader.ValueOrMissing("pBIC")),
                    .Explicatie = AvcSql.AsText(k_reader.ValueOrMissing("Explicatie")), .CodAngajament = AvcSql.AsText(k_reader.ValueOrMissing("CodAngajament")),
                    .CodIndicator = AvcSql.AsText(k_reader.ValueOrMissing("CodIndicator")), .CodProgram = AvcSql.AsText(k_reader.ValueOrMissing("CodProgram")),
                    .Auto = Yes(k_reader.ValueOrMissing("Auto")), .DataIn = Dat(k_reader.ValueOrMissing("DataIN")), .Dtq = Dat(k_reader.ValueOrMissing("DTQ"))})
            End While
        End Using
    End Sub

    Private Shared Sub ReadOpme(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_result As AvcPlatiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, "OPME", {"IDOPME", "NrOPME"}, k_result)
            While k_reader.Read()
                Dim k_id = Int(k_reader.ValueOrMissing("IDOPME"))
                If Not k_id.HasValue Then Continue While
                k_result.Opme.Add(New AvcOpmeRow() With {
                    .IdUnitate = k_unit, .IdOpme = k_id.Value, .NrOpme = If(Int(k_reader.ValueOrMissing("NrOPME")), 0), .DataDoc = Dat(k_reader.ValueOrMissing("DataDoc")),
                    .NrRec = If(Int(k_reader.ValueOrMissing("NrRec")), 0), .Total = Dec(k_reader.ValueOrMissing("TOTAL")), .Platitor = AvcSql.AsText(k_reader.ValueOrMissing("Platitor")),
                    .Cif = AvcSql.AsText(k_reader.ValueOrMissing("CIF")), .Adresa = AvcSql.AsText(k_reader.ValueOrMissing("Adresa")), .Dtq = Dat(k_reader.ValueOrMissing("DTQ"))})
            End While
        End Using
    End Sub

    ' The range flags: Scheme row _OP9, column C6 (copied by Access into LEGUNIT.OP of every linked unit) and the LEGUNIT rows with OP true. A file without them means OFF.
    Private Shared Sub ReadRange(k_cn As OleDbConnection, k_unit As Integer, k_result As AvcPlatiSet)
        Dim k_row As New AvcPlajaRow() With {.IdUnitate = k_unit}
        Dim k_scheme As String = AccessSchema.ResolveTableName(k_cn, "Scheme")
        If k_scheme IsNot Nothing Then
            Using k_reader = AccessSchema.OpenReader(k_cn, k_scheme)
                While k_reader.Read()
                    If String.Equals(AvcSql.AsText(k_reader.ValueOrMissing("NumeForm")), "_OP9", StringComparison.OrdinalIgnoreCase) Then
                        Dim k_flag = Int(k_reader.ValueOrMissing("C6"))
                        k_row.Comuna = k_flag.HasValue AndAlso k_flag.Value <> 0
                    End If
                End While
            End Using
        End If
        Dim k_leg As String = AccessSchema.ResolveTableName(k_cn, "LEGUNIT")
        If k_leg IsNot Nothing Then
            Using k_reader = AccessSchema.OpenReader(k_cn, k_leg)
                While k_reader.Read()
                    Dim k_id = Int(k_reader.ValueOrMissing("IdUnitate"))
                    If k_id.HasValue AndAlso Yes(k_reader.ValueOrMissing("OP")) Then k_row.Legate.Add(k_id.Value)
                End While
            End Using
        End If
        k_result.Plaje.Add(k_row)
    End Sub

    Private Shared Sub CheckMissing(k_reader As AccessTableReader, k_table As String, k_columns As String(), k_result As AvcPlatiSet)
        For Each k_column In k_columns
            If Not k_reader.HasColumn(k_column) Then
                Dim k_label As String = k_table & "." & k_column
                If Not k_result.MissingColumns.Contains(k_label) Then k_result.MissingColumns.Add(k_label)
            End If
        Next
    End Sub

    ' ── cell values ─────────────────────────────────────────────────────────────

    Private Shared Function Int(k_value As Object) As Integer?
        Dim k_parsed As Integer
        If k_value IsNot Nothing AndAlso k_value IsNot DBNull.Value AndAlso
           Integer.TryParse(AvcSql.AsText(k_value), NumberStyles.Integer, CultureInfo.InvariantCulture, k_parsed) Then Return k_parsed
        If k_value IsNot Nothing AndAlso k_value IsNot DBNull.Value Then
            Dim k_double As Double
            If Double.TryParse(AvcSql.AsText(k_value), NumberStyles.Float, CultureInfo.InvariantCulture, k_double) Then Return CInt(Math.Round(k_double))
        End If
        Return Nothing
    End Function

    ' A positive number, or Nothing (Access writes 0 for "none").
    Private Shared Function Pos(k_value As Object) As Integer?
        Dim k_number = Int(k_value)
        Return If(k_number.HasValue AndAlso k_number.Value > 0, k_number, Nothing)
    End Function

    Private Shared Function Dat(k_value As Object) As Date?
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        If TypeOf k_value Is Date Then Return DirectCast(k_value, Date)
        Dim k_date As Date
        If Date.TryParse(AvcSql.AsText(k_value), CultureInfo.InvariantCulture, DateTimeStyles.None, k_date) Then Return k_date
        Return Nothing
    End Function

    Private Shared Function Dec(k_value As Object) As Decimal
        Return If(DecOrNothing(k_value), 0D)
    End Function

    Private Shared Function DecOrNothing(k_value As Object) As Decimal?
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        Dim k_number As Decimal
        If Decimal.TryParse(AvcSql.AsText(k_value), NumberStyles.Float, CultureInfo.InvariantCulture, k_number) Then Return Math.Round(k_number, 2, MidpointRounding.AwayFromZero)
        Return Nothing
    End Function

    Private Shared Function Yes(k_value As Object) As Boolean
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return False
        If TypeOf k_value Is Boolean Then Return DirectCast(k_value, Boolean)
        Dim k_text As String = AvcSql.AsText(k_value)
        Return String.Equals(k_text, "-1", StringComparison.Ordinal) OrElse String.Equals(k_text, "1", StringComparison.Ordinal) OrElse
               String.Equals(k_text, "True", StringComparison.OrdinalIgnoreCase)
    End Function

End Class
