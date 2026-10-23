Imports System.Globalization
Imports System.Text
Imports MySqlConnector

Partial Friend NotInheritable Class VrcRunner

    ''' <summary>NIR headers per statement chunk: one round trip carries a whole chunk (invoice, link, lines, allocations).</summary>
    Private Const InvoiceChunk As Integer = 20

    ''' <summary>
    ''' One provisional invoice fragment per historical NIR. No grouping or amount recalculation. The origins and the NIR lines are read
    ''' once for the subunit, and a chunk of NIRs is written by ONE multi-statement command (a round trip per row was the whole cost);
    ''' the statements and their order per NIR are the same as when each was sent alone.
    ''' </summary>
    Private Sub AttachHistoricalInvoices(k_subunitId As Long)
        Log("Asociere facturi: fragmente provizorii, fără total global presupus și fără modificarea NIR-urilor.")
        Dim k_headers = InvoiceRows(
            "SELECT n.* FROM VC_NIR n WHERE n.SubunitId=@s AND COALESCE(n.Storno,0)=0 AND n.TipDoc<>'STOC_INITIAL' " &
            "AND NOT EXISTS (SELECT 1 FROM VC_NIR_FACTURI f WHERE f.SUBUNIT_ID=n.SubunitId AND f.GESTIUNE_ID=n.GestiuneId " &
            "AND f.EXERCITIU_ID=n.ExercitiuId AND f.NIR_ID=n.IdIntrare) ORDER BY n.GestiuneId,n.IdIntrare", "@s", k_subunitId)
        Dim k_origins As New Dictionary(Of String, Dictionary(Of String, Object))(StringComparer.Ordinal)
        Dim k_linesOf As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.Ordinal)
        If k_headers.Count > 0 Then
            For Each k_found In InvoiceRows(
                "SELECT s.GestiuneId,m.TargetId,m.SourceId,m.SourceKey FROM VC_IdMap m JOIN VC_ImportSources s ON s.SourceId=m.SourceId " &
                "WHERE s.SubunitId=@s AND m.TableName='NIR' AND m.Kind='ROW' ORDER BY m.SourceId,m.SourceKey", "@s", k_subunitId)
                Dim k_originKey As String = Plain(k_found("GestiuneId")) & "|" & Plain(k_found("TargetId"))
                If Not k_origins.ContainsKey(k_originKey) Then k_origins(k_originKey) = k_found
            Next
            For Each k_found In InvoiceRows(
                "SELECT i.*,p.DenumireP,p.UM,t.TVA AS RATE FROM VC_Intrari i " &
                "LEFT JOIN VC_ProduseP pp ON pp.SubunitId=i.SubunitId AND pp.GestiuneId=i.GestiuneId AND pp.IdProdus=i.IdProdus " &
                "LEFT JOIN VC_Produse p ON p.SubunitId=pp.SubunitId AND p.GestiuneId=pp.GestiuneId AND p.ID=pp.ID " &
                "LEFT JOIN VC_TVA t ON t.SubunitId=i.SubunitId AND t.GestiuneId=i.GestiuneId AND t.IdTVA=i.IDTVA " &
                "WHERE i.SubunitId=@s ORDER BY i.GestiuneId,i.ExercitiuId,i.IdIntrare,i.ID", "@s", k_subunitId)
                Dim k_lineKey As String = NirKey(k_found("GestiuneId"), k_found("ExercitiuId"), k_found("IdIntrare"))
                If Not k_linesOf.ContainsKey(k_lineKey) Then k_linesOf(k_lineKey) = New List(Of Dictionary(Of String, Object))()
                k_linesOf(k_lineKey).Add(k_found)
            Next
        End If
        For k_start As Integer = 0 To k_headers.Count - 1 Step InvoiceChunk
            _cancel.ThrowIfCancellationRequested()
            Dim k_count As Integer = Math.Min(InvoiceChunk, k_headers.Count - k_start)
            Using k_cmd As New MySqlCommand(String.Empty, _cn, _tx)
                Dim k_sql As New StringBuilder()
                AddInvoiceParam(k_cmd, "@s", k_subunitId)
                AddInvoiceParam(k_cmd, "@u", _user)
                For k_i As Integer = 0 To k_count - 1
                    Dim k_row = k_headers(k_start + k_i)
                    Dim k_nir = k_row("IdIntrare"), k_gest = k_row("GestiuneId"), k_exercise = k_row("ExercitiuId")
                    Dim k_origin As Dictionary(Of String, Object) = Nothing
                    k_origins.TryGetValue(Plain(k_gest) & "|" & Plain(k_nir), k_origin)
                    AddInvoiceParam(k_cmd, $"@p{k_i}", k_row("CodPartener"))
                    AddInvoiceParam(k_cmd, $"@n{k_i}", If(k_row("NrFact"), String.Empty))
                    AddInvoiceParam(k_cmd, $"@d{k_i}", k_row("DataFc"))
                    AddInvoiceParam(k_cmd, $"@g{k_i}", k_gest)
                    AddInvoiceParam(k_cmd, $"@e{k_i}", k_exercise)
                    AddInvoiceParam(k_cmd, $"@r{k_i}", k_nir)
                    AddInvoiceParam(k_cmd, $"@v{k_i}", k_row("Valoare"))
                    AddInvoiceParam(k_cmd, $"@o{k_i}", If(k_origin Is Nothing, Nothing, k_origin("SourceId")))
                    AddInvoiceParam(k_cmd, $"@k{k_i}", If(k_origin Is Nothing, Nothing, k_origin("SourceKey")))
                    k_sql.Append($"INSERT INTO VC_FACTURI (CodPartener,NUMAR,DATA,TOTAL_CONFIRMAT,MIGRATA,STARE,CREAT_DE) VALUES (@p{k_i},@n{k_i},@d{k_i},0,1,'MIGRATA',@u);")
                    k_sql.Append("SET @inv=LAST_INSERT_ID();")
                    k_sql.Append($"INSERT INTO VC_NIR_FACTURI (FACTURA_ID,SUBUNIT_ID,GESTIUNE_ID,EXERCITIU_ID,NIR_ID,TOTAL_SURSA,SOURCE_ID,SOURCE_KEY) VALUES (@inv,@s,@g{k_i},@e{k_i},@r{k_i},@v{k_i},@o{k_i},@k{k_i});")
                    k_sql.Append("SET @lnk=LAST_INSERT_ID();")
                    Dim k_lines As List(Of Dictionary(Of String, Object)) = Nothing
                    If Not k_linesOf.TryGetValue(NirKey(k_gest, k_exercise, k_nir), k_lines) Then Continue For
                    For k_j As Integer = 0 To k_lines.Count - 1
                        Dim k_line = k_lines(k_j)
                        Dim k_tag As String = $"{k_i}_{k_j}"
                        AddInvoiceParam(k_cmd, "@dn" & k_tag, If(k_line("DenumireP"), "Produs sursă"))
                        AddInvoiceParam(k_cmd, "@um" & k_tag, k_line("UM"))
                        AddInvoiceParam(k_cmd, "@q" & k_tag, k_line("Cant"))
                        AddInvoiceParam(k_cmd, "@t" & k_tag, k_line("RATE"))
                        AddInvoiceParam(k_cmd, "@w" & k_tag, k_line("ValoareCUTVA"))
                        AddInvoiceParam(k_cmd, "@l" & k_tag, k_line("ID"))
                        k_sql.Append($"INSERT INTO VC_FACTURI_LINII (FACTURA_ID,DENUMIRE,UM,CANT,COTA,VALOARE,RECONSTRUITA) VALUES (@inv,@dn{k_tag},@um{k_tag},@q{k_tag},@t{k_tag},@w{k_tag},1);")
                        k_sql.Append($"INSERT INTO VC_FACTURI_REPARTIZARI (NIR_FACTURA_ID,FACTURA_LINIE_ID,SUBUNIT_ID,GESTIUNE_ID,EXERCITIU_ID,INTRARE_ID,CANT,VALOARE) VALUES (@lnk,LAST_INSERT_ID(),@s,@g{k_i},@e{k_i},@l{k_tag},@q{k_tag},@w{k_tag});")
                    Next
                Next
                k_cmd.CommandText = k_sql.ToString()
                k_cmd.ExecuteNonQuery()
            End Using
            Log($"Facturi: {k_start + k_count:N0}/{k_headers.Count:N0} NIR-uri asociate; tranzacția nu este încă confirmată.")
        Next
        Log($"Facturi: {k_headers.Count} fragmente asociate; totalurile sursă rămân separat pe fiecare NIR.")
    End Sub

    Private Shared Function Plain(k_value As Object) As String
        Return Convert.ToString(k_value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function NirKey(k_gest As Object, k_exercise As Object, k_nir As Object) As String
        Return Plain(k_gest) & "|" & Plain(k_exercise) & "|" & Plain(k_nir)
    End Function

    Private Shared Sub AddInvoiceParam(k_cmd As MySqlCommand, k_name As String, k_value As Object)
        k_cmd.Parameters.AddWithValue(k_name, If(k_value, DBNull.Value))
    End Sub

    Private Function InvoiceRows(k_sql As String, ParamArray k_args As Object()) As List(Of Dictionary(Of String, Object))
        Dim k_rows As New List(Of Dictionary(Of String, Object))()
        Using k_cmd As New MySqlCommand(k_sql, _cn, _tx)
            For k_i As Integer = 0 To k_args.Length - 1 Step 2
                k_cmd.Parameters.AddWithValue(Convert.ToString(k_args(k_i), CultureInfo.InvariantCulture), If(k_args(k_i + 1), DBNull.Value))
            Next
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                    For k_i As Integer = 0 To k_reader.FieldCount - 1
                        k_row(k_reader.GetName(k_i)) = If(k_reader.IsDBNull(k_i), Nothing, k_reader.GetValue(k_i))
                    Next
                    k_rows.Add(k_row)
                End While
            End Using
        End Using
        Return k_rows
    End Function
End Class
