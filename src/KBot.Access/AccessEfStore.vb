Option Strict On
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 00EF-43: the Access e-invoice store <c>ef_&lt;year&gt;.accdb</c> (tables EF, EFT, EFS, EFT_C, EFT_M, EFT_O; column layout in
''' <c>Surse/RawExport/EF_ACCDB/tables</c>). Reads the received messages for «Preia din Access» and adds the ones K-BOT has for «Trimite in Access».
''' </summary>
''' <remarks>
''' <para>
''' <b>Which file.</b> <c>&lt;folder of the registry cale.accdb&gt;\EFACTURA\ef_&lt;year&gt;.accdb</c> -- the place the Migrator's E-Factura tab
''' looks first. One store holds the messages of every unit of the folder; a message belongs to a unit by <c>EF.cui_unit</c>.
''' </para>
''' <para>
''' <b>Reading</b> is by message: the invoices (EFT), their lines (EFS), notes (EFT_C), ANAF messages (EFT_M) and booked operations (EFT_O). The Access
''' numbers are read only to tie the children to their parents on the PC; they are never sent to the server as identity.
''' </para>
''' <para>
''' <b>Writing</b> adds what the file does not have, by <c>id_sol</c> (the primary key of EF), in one transaction. The new rows get fresh Access
''' numbers (AutoNumber, read back with <c>@@IDENTITY</c>), so nothing can collide with a row Access created meanwhile. EFT_O is never written
''' (the accounting belongs to Access). A Text column is 255 characters in Access: a longer text is cut and counted.
''' </para>
''' </remarks>
Public NotInheritable Class AccessEfStore

    Private Const InChunk As Integer = 40
    Private Const TextLimit As Integer = 255

    Private Sub New()
    End Sub

    ' ---- which file ---------------------------------------------------------------------------------

    Public Shared Function ResolveStore(registryPath As String, an As Integer) As String
        Try
            If String.IsNullOrWhiteSpace(registryPath) OrElse Not File.Exists(registryPath) Then
                Throw New InvalidOperationException($"Registrul AVACONT «{registryPath}» nu există pe acest calculator.")
            End If
            Dim folder As String = Path.GetDirectoryName(Path.GetFullPath(registryPath))
            Dim store As String = Path.Combine(folder, "EFACTURA", $"ef_{an.ToString(CultureInfo.InvariantCulture)}.accdb")
            If Not File.Exists(store) Then
                Throw New InvalidOperationException($"Magazinul de facturi «{store}» nu există pe acest calculator.")
            End If
            Return store
        Catch ex As Exception
            GlobalErrorLog.Write("AccessEfStore.ResolveStore", ex)
            Throw
        End Try
    End Function

    ' ---- the ids ------------------------------------------------------------------------------------

    Public Shared Function ListMessageIds(storeFile As String) As List(Of String)
        Return ListIds(storeFile, String.Empty, "AccessEfStore.ListMessageIds")
    End Function

    Public Shared Function ListUnitMessageIds(storeFile As String, cuiNormalizat As String) As List(Of String)
        If String.IsNullOrWhiteSpace(cuiNormalizat) Then Throw New ArgumentException("Codul fiscal lipsește.", NameOf(cuiNormalizat))
        Return ListIds(storeFile, cuiNormalizat, "AccessEfStore.ListUnitMessageIds")
    End Function

    Private Shared Function ListIds(storeFile As String, cuiNormalizat As String, k_source As String) As List(Of String)
        Try
            Dim ids As New List(Of String)()
            Using cn As OleDbConnection = AccessBudgetSender.OpenAccess(storeFile)
                Using cmd As New OleDbCommand("SELECT [id_sol], [cui_unit] FROM [EF]", cn)
                    Using r As OleDbDataReader = cmd.ExecuteReader()
                        While r.Read()
                            Dim id As String = Txt(r.GetValue(0))
                            If id.Length = 0 Then Continue While
                            If cuiNormalizat.Length > 0 AndAlso Not String.Equals(NormalizeCui(Txt(r.GetValue(1))), cuiNormalizat, StringComparison.Ordinal) Then Continue While
                            ids.Add(id)
                        End While
                    End Using
                End Using
            End Using
            Return ids
        Catch ex As Exception
            GlobalErrorLog.Write(k_source, ex)
            Throw
        End Try
    End Function

    ''' <summary>The tax code without spaces, dots and the «RO» before the digits, upper case (the same rule as the server's CuiNormalizat).</summary>
    Friend Shared Function NormalizeCui(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return String.Empty
        Dim kept As New StringBuilder(value.Length)
        For Each ch As Char In value
            If Char.IsLetterOrDigit(ch) Then kept.Append(Char.ToUpperInvariant(ch))
        Next
        Dim cleaned As String = kept.ToString()
        If cleaned.Length > 2 AndAlso cleaned.StartsWith("RO", StringComparison.Ordinal) AndAlso Char.IsDigit(cleaned(2)) Then cleaned = cleaned.Substring(2)
        Return cleaned
    End Function

    ' ---- reading ------------------------------------------------------------------------------------

    Public Shared Function ReadMessages(storeFile As String, idSol As IEnumerable(Of String)) As List(Of AccessEfMessage)
        ArgumentNullException.ThrowIfNull(idSol)
        Try
            Dim result As New List(Of AccessEfMessage)()
            Using cn As OleDbConnection = AccessBudgetSender.OpenAccess(storeFile)
                For Each chunk As List(Of String) In Chunks(idSol.Where(Function(k_id) Not String.IsNullOrWhiteSpace(k_id)).Distinct().ToList())
                    ReadChunk(cn, chunk, result)
                Next
            End Using
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("AccessEfStore.ReadMessages", ex)
            Throw
        End Try
    End Function

    ' Reached only through ReadMessages (wrapped).
    Private Shared Sub ReadChunk(cn As OleDbConnection, chunk As List(Of String), result As List(Of AccessEfMessage))
        Dim byId As New Dictionary(Of String, AccessEfMessage)(StringComparer.OrdinalIgnoreCase)
        For Each row As Dictionary(Of String, Object) In ReadRows(cn, "EF", {"id_sol", "id", "cif", "data", "cui_unit", "nou", "nume_fisier", "XML", "DTQ"}, "id_sol", chunk)
            Dim m As New AccessEfMessage() With {
                .IdSol = Txt(row("id_sol")), .IdIncarcare = Txt(row("id")), .CifEmitent = Txt(row("cif")), .DataMesaj = DateOf(row("data")),
                .CuiUnitate = Txt(row("cui_unit")), .Nou = Flag(row("nou")), .NumeFisier = Txt(row("nume_fisier")), .Xml = Bin(row("XML")),
                .Added = DateOf(row("DTQ"))}
            byId(m.IdSol) = m
            result.Add(m)
        Next
        If byId.Count = 0 Then Return

        Dim invoices As New Dictionary(Of Integer, AccessEfInvoice)()
        Dim refs As New Dictionary(Of Integer, Integer)()
        For Each row As Dictionary(Of String, Object) In ReadRows(cn, "EFT",
            {"IDEFT", "id_sol", "NrFact", "DataFact", "DataScad", "CotaTVA", "TVA", "Valoare", "TOTAL", "CUI", "DenumireP", "Adresa", "Attach", "Tip", "Ref", "IDEFT_REF", "NC", "DTQ"},
            "id_sol", chunk)
            Dim parent As AccessEfMessage = Nothing
            If Not byId.TryGetValue(Txt(row("id_sol")), parent) Then Continue For
            Dim inv As New AccessEfInvoice() With {
                .AccessId = Whole(row("IDEFT")), .NrFact = Txt(row("NrFact")), .DataFact = DateOf(row("DataFact")), .DataScad = DateOf(row("DataScad")),
                .CotaTva = Num(row("CotaTVA")), .Tva = Num(row("TVA")), .Valoare = Num(row("Valoare")), .Total = Num(row("TOTAL")), .Cui = Txt(row("CUI")),
                .DenumireP = Txt(row("DenumireP")), .Adresa = Txt(row("Adresa")), .Atasament = Txt(row("Attach")),
                .Tip = If(Txt(row("Tip")).Length = 0, "FC", Txt(row("Tip"))), .Semn = If(Whole(row("NC")) < 0, -1, 1), .Ref = Txt(row("Ref")),
                .Added = DateOf(row("DTQ"))}
            invoices(inv.AccessId) = inv
            parent.Invoices.Add(inv)
            Dim refId As Integer = Whole(row("IDEFT_REF"))
            If refId <> 0 Then refs(inv.AccessId) = refId
        Next
        If invoices.Count = 0 Then Return

        Dim ftIds As List(Of String) = invoices.Keys.Select(Function(k_i) k_i.ToString(CultureInfo.InvariantCulture)).ToList()
        For Each row As Dictionary(Of String, Object) In ReadRows(cn, "EFS", {"IDEFS", "IDEFT", "ID", "UNIT", "Cant", "Valoare", "Pret", "Denumire", "Explicatie"}, "IDEFT", ftIds, True)
            Dim inv As AccessEfInvoice = Nothing
            If invoices.TryGetValue(Whole(row("IDEFT")), inv) Then
                inv.Lines.Add(New AccessEfLine() With {
                    .NrLinie = Txt(row("ID")), .Denumire = Txt(row("Denumire")), .Explicatie = Txt(row("Explicatie")), .Unit = Txt(row("UNIT")),
                    .Cant = Num(row("Cant")), .Pret = Num(row("Pret")), .Valoare = Num(row("Valoare"))})
            End If
        Next
        For Each row As Dictionary(Of String, Object) In ReadRows(cn, "EFT_C", {"IDEFTC", "IDEFT", "Note", "DTQ"}, "IDEFT", ftIds, True)
            Dim inv As AccessEfInvoice = Nothing
            If invoices.TryGetValue(Whole(row("IDEFT")), inv) AndAlso Txt(row("Note")).Length > 0 Then
                inv.Notes.Add(New AccessEfNote() With {.Nota = Txt(row("Note")), .Added = DateOf(row("DTQ"))})
            End If
        Next
        For Each row As Dictionary(Of String, Object) In ReadRows(cn, "EFT_M", {"IDMSG", "id_sol", "IDEFT", "ID", "Mesaj", "Data", "DTQ"}, "IDEFT", ftIds, True)
            Dim inv As AccessEfInvoice = Nothing
            If invoices.TryGetValue(Whole(row("IDEFT")), inv) Then
                inv.Messages.Add(New AccessEfInvoiceMessage() With {
                    .IdSol = Txt(row("id_sol")), .IdMesajAnaf = Txt(row("ID")), .Mesaj = Txt(row("Mesaj")), .DataMesaj = DateOf(row("Data")), .Added = DateOf(row("DTQ"))})
            End If
        Next
        For Each row As Dictionary(Of String, Object) In ReadRows(cn, "EFT_O", {"IDEFTO", "IDEFT", "IdOperatie", "IdClsf", "IdUnitate", "Sursa", "Suma", "DTQ"}, "IDEFT", ftIds, True)
            Dim inv As AccessEfInvoice = Nothing
            If invoices.TryGetValue(Whole(row("IDEFT")), inv) Then
                inv.Operations.Add(New AccessEfOperation() With {
                    .IdOperatiePrimita = Whole(row("IDEFTO")), .IdOperatie = Whole(row("IdOperatie")), .IdClsfAcc = Whole(row("IdClsf")),
                    .IdUnitate = Whole(row("IdUnitate")), .Sursa = Txt(row("Sursa")), .Suma = Num(row("Suma")), .Added = DateOf(row("DTQ"))})
            End If
        Next

        ' a credit note's reference, told by natural key (the Access number means nothing on the server)
        For Each pair As KeyValuePair(Of Integer, Integer) In refs
            Using cmd As New OleDbCommand("SELECT [id_sol], [NrFact] FROM [EFT] WHERE [IDEFT] = ?", cn)
                cmd.Parameters.AddWithValue("@r", pair.Value)
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    If r.Read() Then
                        invoices(pair.Key).RefIdSol = Txt(r.GetValue(0))
                        invoices(pair.Key).RefNrFact = Txt(r.GetValue(1))
                    End If
                End Using
            End Using
        Next
    End Sub

    ''' <summary>
    ''' The rows whose <paramref name="keyColumn"/> is in <paramref name="keys"/>: the wanted columns that the table really has (an old layout
    ''' may lack one: its value is then Nothing), as name ▸ value. <paramref name="keyIsNumber"/> = the key column is a number.
    ''' A table the file does not have gives no rows (EFT_O of an old file).
    ''' </summary>
    Private Shared Function ReadRows(cn As OleDbConnection, table As String, wanted As String(), keyColumn As String, keys As List(Of String),
                                     Optional keyIsNumber As Boolean = False) As List(Of Dictionary(Of String, Object))
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim present As HashSet(Of String) = ColumnsOf(cn, table)
        If present.Count = 0 OrElse Not present.Contains(keyColumn) Then Return rows
        Dim read As List(Of String) = wanted.Where(Function(k_c) present.Contains(k_c)).ToList()
        If read.Count = 0 Then Return rows
        For Each chunk As List(Of String) In Chunks(keys)
            Dim k_list As String = String.Join(", ", read.Select(Function(k_c) "[" & k_c & "]"))
            Dim k_marks As String = String.Join(", ", chunk.Select(Function(k_k) "?"))
            Using cmd As New OleDbCommand("SELECT " & k_list & " FROM [" & table & "] WHERE [" & keyColumn & "] IN (" & k_marks & ")", cn)
                For Each key As String In chunk
                    If keyIsNumber Then
                        cmd.Parameters.Add(New OleDbParameter("@k", OleDbType.Integer) With {.Value = Integer.Parse(key, CultureInfo.InvariantCulture)})
                    Else
                        cmd.Parameters.Add(New OleDbParameter("@k", OleDbType.VarWChar, 255) With {.Value = key})
                    End If
                Next
                Using r As OleDbDataReader = cmd.ExecuteReader()
                    While r.Read()
                        Dim row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                        For Each name As String In wanted
                            row(name) = Nothing
                        Next
                        For i As Integer = 0 To read.Count - 1
                            row(read(i)) = If(r.IsDBNull(i), Nothing, r.GetValue(i))
                        Next
                        rows.Add(row)
                    End While
                End Using
            End Using
        Next
        Return rows
    End Function

    Private Shared Function ColumnsOf(cn As OleDbConnection, table As String) As HashSet(Of String)
        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Using schema As DataTable = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, New Object() {Nothing, Nothing, table, Nothing})
            For Each row As DataRow In schema.Rows
                names.Add(Convert.ToString(row("COLUMN_NAME"), CultureInfo.InvariantCulture))
            Next
        End Using
        Return names
    End Function

    Private Shared Iterator Function Chunks(items As List(Of String)) As IEnumerable(Of List(Of String))
        For i As Integer = 0 To items.Count - 1 Step InChunk
            Yield items.Skip(i).Take(InChunk).ToList()
        Next
    End Function

    ' ---- writing ------------------------------------------------------------------------------------

    Public Shared Function WriteMessages(storeFile As String, messages As IEnumerable(Of AccessEfMessage)) As AccessEfWriteResult
        ArgumentNullException.ThrowIfNull(messages)
        Dim result As New AccessEfWriteResult()
        Try
            Using cn As OleDbConnection = AccessBudgetSender.OpenAccess(storeFile)
                Using tx As OleDbTransaction = cn.BeginTransaction()
                    Try
                        Dim pending As New List(Of AccessEfInvoice)()
                        For Each m As AccessEfMessage In messages
                            If String.IsNullOrWhiteSpace(m.IdSol) Then Continue For
                            If MessageExists(cn, tx, m.IdSol) Then
                                result.MessagesSkipped += 1
                                Continue For
                            End If
                            InsertMessage(cn, tx, m, result)
                            For Each inv As AccessEfInvoice In m.Invoices
                                Dim id As Integer = InsertInvoice(cn, tx, m.IdSol, inv, result)
                                inv.AccessId = id
                                If inv.RefNrFact.Length > 0 Then pending.Add(inv)
                            Next
                        Next
                        For Each inv As AccessEfInvoice In pending
                            LinkReference(cn, tx, inv)
                        Next
                        tx.Commit()
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("AccessEfStore.WriteMessages", ex)
            Throw
        End Try
    End Function

    ' ---- helpers below are reached only through WriteMessages (wrapped) -------------------------------

    Private Shared Function MessageExists(cn As OleDbConnection, tx As OleDbTransaction, idSol As String) As Boolean
        Using cmd As New OleDbCommand("SELECT COUNT(*) FROM [EF] WHERE [id_sol] = ?", cn, tx)
            cmd.Parameters.Add(New OleDbParameter("@s", OleDbType.VarWChar, 255) With {.Value = idSol})
            Return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Shared Sub InsertMessage(cn As OleDbConnection, tx As OleDbTransaction, m As AccessEfMessage, result As AccessEfWriteResult)
        Dim cols As New List(Of KeyValuePair(Of String, OleDbParameter))()
        Add(cols, "id_sol", TextParam(m.IdSol, result))
        Add(cols, "id", TextParam(m.IdIncarcare, result))
        Add(cols, "cif", TextParam(m.CifEmitent, result))
        Add(cols, "data", DateOnlyParam(m.DataMesaj))
        Add(cols, "cui_unit", TextParam(m.CuiUnitate, result))
        Add(cols, "nou", New OleDbParameter("@p", OleDbType.Boolean) With {.Value = m.Nou})
        Add(cols, "nume_fisier", TextParam(m.NumeFisier, result))
        If m.Xml IsNot Nothing AndAlso m.Xml.Length > 0 Then Add(cols, "XML", New OleDbParameter("@p", OleDbType.LongVarBinary) With {.Value = m.Xml})
        Add(cols, "DTQ", DateParam(m.Added))
        Insert(cn, tx, "EF", cols)
        result.MessagesAdded += 1
    End Sub

    Private Shared Function InsertInvoice(cn As OleDbConnection, tx As OleDbTransaction, idSol As String, inv As AccessEfInvoice, result As AccessEfWriteResult) As Integer
        Dim cols As New List(Of KeyValuePair(Of String, OleDbParameter))()
        Add(cols, "id_sol", TextParam(idSol, result))
        Add(cols, "NrFact", TextParam(inv.NrFact, result))
        Add(cols, "DataFact", DateOnlyParam(inv.DataFact))
        Add(cols, "DataScad", DateOnlyParam(inv.DataScad))
        Add(cols, "CotaTVA", New OleDbParameter("@p", OleDbType.Integer) With {.Value = CInt(Math.Round(inv.CotaTva, 0, MidpointRounding.AwayFromZero))})
        Add(cols, "TVA", DoubleParam(inv.Tva))
        Add(cols, "Valoare", DoubleParam(inv.Valoare))
        Add(cols, "TOTAL", DoubleParam(inv.Total))
        Add(cols, "CUI", TextParam(inv.Cui, result))
        Add(cols, "DenumireP", TextParam(inv.DenumireP, result))
        Add(cols, "Adresa", TextParam(inv.Adresa, result))
        Add(cols, "Attach", TextParam(inv.Atasament, result))
        Add(cols, "Tip", TextParam(If(inv.Tip.Length = 0, "FC", inv.Tip), result))
        Add(cols, "Ref", TextParam(inv.Ref, result))
        ' what the Access import writes: -1 for a credit note that counts against the account, else 1
        Add(cols, "NC", New OleDbParameter("@p", OleDbType.Integer) With {.Value = If(inv.Semn < 0, -1, 1)})
        Add(cols, "DTQ", DateParam(inv.Added))
        Dim id As Integer = Insert(cn, tx, "EFT", cols, True)
        result.InvoicesAdded += 1

        Dim index As Integer = 0
        For Each line As AccessEfLine In inv.Lines
            index += 1
            Dim k_number As Integer
            If Not Integer.TryParse(line.NrLinie, NumberStyles.None, CultureInfo.InvariantCulture, k_number) Then k_number = index
            Dim lc As New List(Of KeyValuePair(Of String, OleDbParameter))()
            Add(lc, "IDEFT", New OleDbParameter("@p", OleDbType.Integer) With {.Value = id})
            Add(lc, "ID", New OleDbParameter("@p", OleDbType.Integer) With {.Value = k_number})
            Add(lc, "UNIT", TextParam(line.Unit, result))
            Add(lc, "Cant", DoubleParam(line.Cant))
            Add(lc, "Valoare", DoubleParam(line.Valoare))
            Add(lc, "Pret", DoubleParam(line.Pret))
            Add(lc, "Denumire", TextParam(line.Denumire, result))
            Add(lc, "Explicatie", TextParam(line.Explicatie, result))
            Insert(cn, tx, "EFS", lc)
            result.LinesAdded += 1
        Next
        For Each note As AccessEfNote In inv.Notes
            Dim nc As New List(Of KeyValuePair(Of String, OleDbParameter))()
            Add(nc, "IDEFT", New OleDbParameter("@p", OleDbType.Integer) With {.Value = id})
            Add(nc, "Note", MemoParam(note.Nota))
            Add(nc, "DTQ", DateParam(note.Added))
            Insert(cn, tx, "EFT_C", nc)
            result.NotesAdded += 1
        Next
        For Each msg As AccessEfInvoiceMessage In inv.Messages
            Dim mc As New List(Of KeyValuePair(Of String, OleDbParameter))()
            Add(mc, "IDEFT", New OleDbParameter("@p", OleDbType.Integer) With {.Value = id})
            Add(mc, "id_sol", TextParam(msg.IdSol, result))
            Add(mc, "ID", TextParam(msg.IdMesajAnaf, result))
            Add(mc, "Mesaj", MemoParam(msg.Mesaj))
            Add(mc, "Data", DateParam(msg.DataMesaj))
            Add(mc, "DTQ", DateParam(msg.Added))
            Insert(cn, tx, "EFT_M", mc)
            result.InvoiceMessagesAdded += 1
        Next
        Return id
    End Function

    ''' <summary>A credit note's IDEFT_REF: the invoice it refers to, found by message + number among what the file has now. Not found = left empty.</summary>
    Private Shared Sub LinkReference(cn As OleDbConnection, tx As OleDbTransaction, inv As AccessEfInvoice)
        Dim target As Integer
        Using find As New OleDbCommand("SELECT TOP 1 [IDEFT] FROM [EFT] WHERE [id_sol] = ? AND [NrFact] = ?", cn, tx)
            find.Parameters.Add(New OleDbParameter("@s", OleDbType.VarWChar, 255) With {.Value = inv.RefIdSol})
            find.Parameters.Add(New OleDbParameter("@n", OleDbType.VarWChar, 255) With {.Value = inv.RefNrFact})
            Dim value As Object = find.ExecuteScalar()
            If value Is Nothing OrElse value Is DBNull.Value Then Return
            target = Convert.ToInt32(value, CultureInfo.InvariantCulture)
        End Using
        If target = inv.AccessId Then Return
        Using upd As New OleDbCommand("UPDATE [EFT] SET [IDEFT_REF] = ? WHERE [IDEFT] = ?", cn, tx)
            upd.Parameters.Add(New OleDbParameter("@r", OleDbType.Integer) With {.Value = target})
            upd.Parameters.Add(New OleDbParameter("@i", OleDbType.Integer) With {.Value = inv.AccessId})
            upd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>INSERT of the columns that have a value (the others take the table's default); the new AutoNumber when <paramref name="wantId"/>.</summary>
    Private Shared Function Insert(cn As OleDbConnection, tx As OleDbTransaction, table As String,
                                   cols As List(Of KeyValuePair(Of String, OleDbParameter)), Optional wantId As Boolean = False) As Integer
        Dim used As List(Of KeyValuePair(Of String, OleDbParameter)) = cols.Where(Function(k_c) k_c.Value IsNot Nothing).ToList()
        Dim k_names As String = String.Join(", ", used.Select(Function(k_c) "[" & k_c.Key & "]"))
        Dim k_marks As String = String.Join(", ", used.Select(Function(k_c) "?"))
        Using cmd As New OleDbCommand("INSERT INTO [" & table & "] (" & k_names & ") VALUES (" & k_marks & ")", cn, tx)
            For Each c As KeyValuePair(Of String, OleDbParameter) In used
                cmd.Parameters.Add(c.Value)
            Next
            cmd.ExecuteNonQuery()
        End Using
        If Not wantId Then Return 0
        Using ident As New OleDbCommand("SELECT @@IDENTITY", cn, tx)
            Return Convert.ToInt32(ident.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
    End Function

    Private Shared Sub Add(cols As List(Of KeyValuePair(Of String, OleDbParameter)), name As String, p As OleDbParameter)
        cols.Add(New KeyValuePair(Of String, OleDbParameter)(name, p))
    End Sub

    ''' <summary>A Text column value, cut to Access's 255 characters; Nothing (column left to its default) when empty.</summary>
    Private Shared Function TextParam(value As String, result As AccessEfWriteResult) As OleDbParameter
        If String.IsNullOrEmpty(value) Then Return Nothing
        Dim v As String = value
        If v.Length > TextLimit Then
            v = v.Substring(0, TextLimit)
            result.TextsShortened += 1
        End If
        Return New OleDbParameter("@p", OleDbType.VarWChar, TextLimit) With {.Value = v}
    End Function

    Private Shared Function MemoParam(value As String) As OleDbParameter
        If String.IsNullOrEmpty(value) Then Return Nothing
        Return New OleDbParameter("@p", OleDbType.LongVarWChar) With {.Value = value}
    End Function

    Private Shared Function DoubleParam(value As Decimal) As OleDbParameter
        Return New OleDbParameter("@p", OleDbType.Double) With {.Value = CDbl(value)}
    End Function

    Private Shared Function DateParam(value As Date?) As OleDbParameter
        If Not value.HasValue Then Return Nothing
        Return New OleDbParameter("@p", OleDbType.Date) With {.Value = value.Value}
    End Function

    Private Shared Function DateOnlyParam(value As Date?) As OleDbParameter
        If Not value.HasValue Then Return Nothing
        Return New OleDbParameter("@p", OleDbType.Date) With {.Value = value.Value.Date}
    End Function

    ' ---- value readers --------------------------------------------------------------------------------

    Private Shared Function Txt(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function Num(value As Object) As Decimal
        If value Is Nothing OrElse value Is DBNull.Value Then Return 0D
        Return Convert.ToDecimal(value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function Whole(value As Object) As Integer
        If value Is Nothing OrElse value Is DBNull.Value Then Return 0
        Return CInt(Math.Round(Convert.ToDecimal(value, CultureInfo.InvariantCulture), 0, MidpointRounding.AwayFromZero))
    End Function

    Private Shared Function Flag(value As Object) As Boolean
        If value Is Nothing OrElse value Is DBNull.Value Then Return False
        If TypeOf value Is Boolean Then Return CBool(value)
        Return Convert.ToDecimal(value, CultureInfo.InvariantCulture) <> 0D
    End Function

    Private Shared Function DateOf(value As Object) As Date?
        If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
        If TypeOf value Is Date Then Return CDate(value)
        Return Nothing
    End Function

    Private Shared Function Bin(value As Object) As Byte()
        Dim bytes As Byte() = TryCast(value, Byte())
        If bytes IsNot Nothing AndAlso bytes.Length > 0 Then Return bytes
        Dim asText As String = TryCast(value, String)
        If Not String.IsNullOrEmpty(asText) Then Return Encoding.UTF8.GetBytes(asText)
        Return Nothing
    End Function

End Class
