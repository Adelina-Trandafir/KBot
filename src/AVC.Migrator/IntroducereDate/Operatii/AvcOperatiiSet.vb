Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common
Imports KBot.Migrator

' Slice AV02-01 (migration): what the Access tables NoteC, Documente, Oper, Debit and Credit of the ticked units hold, read into memory (one unit file has some hundreds of operations).

''' <summary>One accounting note of an Access unit file.</summary>
Friend NotInheritable Class AvcNotaRow
    Public Property IdUnitate As Integer
    Public Property NumarNC As Integer
    Public Property DataNc As Date?
End Class

''' <summary>One document of an Access unit file.</summary>
Friend NotInheritable Class AvcDocumentRow
    Public Property IdUnitate As Integer
    Public Property IdDocument As Integer
    Public Property IdSalarii As Integer?
    Public Property NumarNC As Integer?
    Public Property NumarDocument As String = String.Empty
    Public Property DataDocument As Date?
    Public Property FelDocument As String = String.Empty
    Public Property Fel As String = String.Empty
    Public Property Auto As Boolean
    Public Property EditManual As Boolean
    Public Property Platit As Boolean
    Public Property Dtq As Date?
End Class

''' <summary>One operation of an Access unit file.</summary>
Friend NotInheritable Class AvcOperRow
    Public Property IdUnitate As Integer
    Public Property IdOperatie As Integer
    Public Property IdDocument As Integer?
    Public Property IdClsfAccess As Integer?
    Public Property Idx As Integer?
    Public Property DataOperatie As Date?
    Public Property Explicatie As String = String.Empty
    Public Property CodJurnal As String = String.Empty
    Public Property Tip As String = String.Empty
    Public Property Fel As String = String.Empty
    Public Property Comp As String = String.Empty
    Public Property Cfpp As Boolean
    Public Property Chelt As Boolean
    Public Property Ordonantare As Boolean
    Public Property Angajament As Boolean
    Public Property Propunere As Boolean
    Public Property Auto As Boolean
    Public Property Platit As Boolean
    Public Property NrAng As Integer?
    Public Property NrOpe As Integer?
    Public Property Dtq As Date?
End Class

''' <summary>One debit or credit line of an Access unit file (the symbol and the partner code are the Access texts).</summary>
Friend NotInheritable Class AvcLinieRow
    Public Property IdUnitate As Integer
    Public Property IdOperatie As Integer
    Public Property SimbolCont As String = String.Empty
    Public Property CodPartener As String = String.Empty
    Public Property Suma As Decimal
End Class

''' <summary>One partner of an Access unit file (<c>Parteneri</c>): the code the posting lines use, its name and its tax code (slice ME01-36).</summary>
Friend NotInheritable Class AvcPartnerRow
    Public Property IdUnitate As Integer
    Public Property CodPartener As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property CodFiscal As String = String.Empty
    Public Property ContIBAN As String = String.Empty
    Public Property Banca As String = String.Empty
    Public Property Adresa As String = String.Empty
    Public Property Tip As String = String.Empty
    Public Property Ascuns As Integer?
End Class

''' <summary>Everything read from the posting tables of the ticked units.</summary>
Friend NotInheritable Class AvcOperatiiSet
    Public ReadOnly Property Note As New List(Of AvcNotaRow)()
    Public ReadOnly Property Documente As New List(Of AvcDocumentRow)()
    Public ReadOnly Property Operatii As New List(Of AvcOperRow)()
    Public ReadOnly Property Debit As New List(Of AvcLinieRow)()
    Public ReadOnly Property Credit As New List(Of AvcLinieRow)()
    ''' <summary>"IdUnitate|CodPartener" ▸ the partner of that unit's Access file (to name a partner that the target lacks and to find it by its tax code).</summary>
    Public ReadOnly Property Parteneri As New Dictionary(Of String, AvcPartnerRow)(StringComparer.OrdinalIgnoreCase)
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    Public ReadOnly Property MissingColumns As New List(Of String)()
End Class

''' <summary>Reads the five tables of every ticked unit's file.</summary>
Friend NotInheritable Class AvcOperatiiReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "Oper"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcOperatiiSet
        Try
            Dim k_result As New AvcOperatiiSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
                    Dim k_tables As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                    For Each k_name In {"NoteC", "Documente", "Oper", "Debit", "Credit"}
                        k_tables(k_name) = AccessSchema.ResolveTableName(k_cn, k_name)
                    Next
                    If k_tables.Values.Any(Function(k_t) k_t Is Nothing) Then
                        k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                        Continue For
                    End If
                    ReadNotes(k_cn, k_tables("NoteC"), k_unit.IdUnitate, k_result)
                    ReadDocuments(k_cn, k_tables("Documente"), k_unit.IdUnitate, k_result)
                    ReadOperations(k_cn, k_tables("Oper"), k_unit.IdUnitate, k_result)
                    ReadLines(k_cn, k_tables("Debit"), k_unit.IdUnitate, "SumaDebit", k_result.Debit, k_result)
                    ReadLines(k_cn, k_tables("Credit"), k_unit.IdUnitate, "SumaCredit", k_result.Credit, k_result)
                    ReadPartners(k_cn, k_unit.IdUnitate, k_result)
                End Using
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcOperatiiReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadNotes(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_result As AvcOperatiiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, "NoteC", {"NumarNC", "DataNc"}, k_result)
            While k_reader.Read()
                Dim k_number = Int(k_reader.ValueOrMissing("NumarNC"))
                If Not k_number.HasValue Then Continue While
                k_result.Note.Add(New AvcNotaRow() With {.IdUnitate = k_unit, .NumarNC = k_number.Value, .DataNc = Dat(k_reader.ValueOrMissing("DataNc"))})
            End While
        End Using
    End Sub

    Private Shared Sub ReadDocuments(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_result As AvcOperatiiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, "Documente", {"IdDocument", "NumarNC", "NumarDocument", "DataDocument", "FelDocument", "Fel"}, k_result)
            While k_reader.Read()
                Dim k_id = Int(k_reader.ValueOrMissing("IdDocument"))
                If Not k_id.HasValue Then Continue While
                k_result.Documente.Add(New AvcDocumentRow() With {
                    .IdUnitate = k_unit, .IdDocument = k_id.Value, .IdSalarii = Pos(k_reader.ValueOrMissing("IDSalarii")), .NumarNC = Int(k_reader.ValueOrMissing("NumarNC")),
                    .NumarDocument = AvcSql.AsText(k_reader.ValueOrMissing("NumarDocument")), .DataDocument = Dat(k_reader.ValueOrMissing("DataDocument")),
                    .FelDocument = AvcSql.AsText(k_reader.ValueOrMissing("FelDocument")), .Fel = AvcSql.AsText(k_reader.ValueOrMissing("Fel")),
                    .Auto = Yes(k_reader.ValueOrMissing("Auto")), .EditManual = Yes(k_reader.ValueOrMissing("EditManual")), .Platit = Yes(k_reader.ValueOrMissing("Platit")),
                    .Dtq = Dat(k_reader.ValueOrMissing("DTQ"))})
            End While
        End Using
    End Sub

    Private Shared Sub ReadOperations(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_result As AvcOperatiiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, "Oper", {"IdOperatie", "IdDocument", "IDClsf", "DataOperatie", "Explicatie", "CodJurnal"}, k_result)
            While k_reader.Read()
                Dim k_id = Int(k_reader.ValueOrMissing("IdOperatie"))
                If Not k_id.HasValue Then Continue While
                k_result.Operatii.Add(New AvcOperRow() With {
                    .IdUnitate = k_unit, .IdOperatie = k_id.Value, .IdDocument = Pos(k_reader.ValueOrMissing("IdDocument")), .IdClsfAccess = Pos(k_reader.ValueOrMissing("IDClsf")),
                    .Idx = Pos(k_reader.ValueOrMissing("IDX")), .DataOperatie = Dat(k_reader.ValueOrMissing("DataOperatie")),
                    .Explicatie = AvcSql.AsText(k_reader.ValueOrMissing("Explicatie")), .CodJurnal = AvcSql.AsText(k_reader.ValueOrMissing("CodJurnal")),
                    .Tip = AvcSql.AsText(k_reader.ValueOrMissing("Tip")), .Fel = AvcSql.AsText(k_reader.ValueOrMissing("Fel")), .Comp = AvcSql.AsText(k_reader.ValueOrMissing("Comp")),
                    .Cfpp = Yes(k_reader.ValueOrMissing("CFPP")), .Chelt = Yes(k_reader.ValueOrMissing("Chelt")), .Ordonantare = Yes(k_reader.ValueOrMissing("Ordonantare")),
                    .Angajament = Yes(k_reader.ValueOrMissing("Angajament")), .Propunere = Yes(k_reader.ValueOrMissing("Propunere")), .Auto = Yes(k_reader.ValueOrMissing("Auto")),
                    .Platit = Yes(k_reader.ValueOrMissing("Platit")), .NrAng = Pos(k_reader.ValueOrMissing("NrANG")), .NrOpe = Pos(k_reader.ValueOrMissing("NrOPE")),
                    .Dtq = Dat(k_reader.ValueOrMissing("DTQ"))})
            End While
        End Using
    End Sub

    ''' <summary>Slice ME01-36: the Access partners of every ticked unit that has a unit file ("IdUnitate|CodPartener"). Access I/O: logs and rethrows.</summary>
    Public Shared Function ReadAllPartners(k_context As AvcContext) As Dictionary(Of String, AvcPartnerRow)
        Try
            Dim k_set As New AvcOperatiiSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
                    ReadPartners(k_cn, k_unit.IdUnitate, k_set)
                End Using
            Next
            Return k_set.Parteneri
        Catch ex As Exception
            GlobalErrorLog.Write("AvcOperatiiReader.ReadAllPartners", ex)
            Throw
        End Try
    End Function

    ''' <summary>0 or 1 (a Yes/No column comes as True/False or -1/0); Nothing when empty.</summary>
    Private Shared Function Pos0(k_value As Object) As Integer?
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        If TypeOf k_value Is Boolean Then Return If(CBool(k_value), 1, 0)
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.InvariantCulture).Trim()
        If k_text.Length = 0 Then Return Nothing
        Dim k_number As Decimal
        If Not Decimal.TryParse(k_text, NumberStyles.Any, CultureInfo.InvariantCulture, k_number) Then Return Nothing
        Return If(k_number <> 0D, 1, 0)
    End Function

    ''' <summary>The partners of the unit file; a file without a Parteneri table simply gives none (the partner is then reported as unknown in Access).</summary>
    Private Shared Sub ReadPartners(k_cn As OleDbConnection, k_unit As Integer, k_result As AvcOperatiiSet)
        Dim k_table As String = AccessSchema.ResolveTableName(k_cn, "Parteneri")
        If k_table Is Nothing Then Return
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            While k_reader.Read()
                Dim k_code As String = AvcSql.AsText(k_reader.ValueOrMissing("CodPartener"))
                If k_code.Length = 0 Then Continue While
                Dim k_name As String = AvcSql.AsText(k_reader.ValueOrMissing("DenumirePartener"))
                If k_name.Length = 0 Then k_name = AvcSql.AsText(k_reader.ValueOrMissing("NumePartener"))
                k_result.Parteneri(k_unit.ToString(CultureInfo.InvariantCulture) & "|" & k_code) =
                    New AvcPartnerRow() With {.IdUnitate = k_unit, .CodPartener = k_code, .Denumire = k_name, .CodFiscal = AvcSql.AsText(k_reader.ValueOrMissing("CodFiscal")),
                     .ContIBAN = AvcSql.AsText(k_reader.ValueOrMissing("ContIBAN")), .Banca = AvcSql.AsText(k_reader.ValueOrMissing("Banca")),
                     .Adresa = AvcSql.AsText(k_reader.ValueOrMissing("Adresa")), .Tip = AvcSql.AsText(k_reader.ValueOrMissing("Tip")), .Ascuns = Pos0(k_reader.ValueOrMissing("Ascuns"))}
            End While
        End Using
    End Sub

    Private Shared Sub ReadLines(k_cn As OleDbConnection, k_table As String, k_unit As Integer, k_amount As String, k_target As List(Of AvcLinieRow), k_result As AvcOperatiiSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            CheckMissing(k_reader, k_table, {"IdOperatie", "SimbolCont", "CodPartener", k_amount}, k_result)
            While k_reader.Read()
                Dim k_id = Int(k_reader.ValueOrMissing("IdOperatie"))
                If Not k_id.HasValue Then Continue While
                k_target.Add(New AvcLinieRow() With {
                    .IdUnitate = k_unit, .IdOperatie = k_id.Value, .SimbolCont = AvcSql.AsText(k_reader.ValueOrMissing("SimbolCont")),
                    .CodPartener = AvcSql.AsText(k_reader.ValueOrMissing("CodPartener")), .Suma = Dec(k_reader.ValueOrMissing(k_amount))})
            End While
        End Using
    End Sub

    Private Shared Sub CheckMissing(k_reader As AccessTableReader, k_table As String, k_columns As String(), k_result As AvcOperatiiSet)
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
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return 0D
        Dim k_number As Decimal
        If Decimal.TryParse(AvcSql.AsText(k_value), NumberStyles.Float, CultureInfo.InvariantCulture, k_number) Then Return Math.Round(k_number, 2, MidpointRounding.AwayFromZero)
        Return 0D
    End Function

    Private Shared Function Yes(k_value As Object) As Boolean
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return False
        If TypeOf k_value Is Boolean Then Return DirectCast(k_value, Boolean)
        Dim k_text As String = AvcSql.AsText(k_value)
        Return String.Equals(k_text, "-1", StringComparison.Ordinal) OrElse String.Equals(k_text, "1", StringComparison.Ordinal) OrElse
               String.Equals(k_text, "True", StringComparison.OrdinalIgnoreCase)
    End Function

End Class
