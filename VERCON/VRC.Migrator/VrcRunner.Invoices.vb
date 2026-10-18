Imports System.Globalization
Imports MySqlConnector

Partial Friend NotInheritable Class VrcRunner

    ''' <summary>One provisional invoice fragment per historical NIR. No grouping or amount recalculation.</summary>
    Private Sub AttachHistoricalInvoices(k_subunitId As Long)
        Log("Asociere facturi: fragmente provizorii, fără total global presupus și fără modificarea NIR-urilor.")
        Dim k_headers = InvoiceRows(
            "SELECT n.* FROM VC_NIR n WHERE n.SubunitId=@s AND COALESCE(n.Storno,0)=0 AND n.TipDoc<>'STOC_INITIAL' " &
            "AND NOT EXISTS (SELECT 1 FROM VC_NIR_FACTURI f WHERE f.SUBUNIT_ID=n.SubunitId AND f.GESTIUNE_ID=n.GestiuneId " &
            "AND f.EXERCITIU_ID=n.ExercitiuId AND f.NIR_ID=n.IdIntrare) ORDER BY n.GestiuneId,n.IdIntrare", "@s", k_subunitId)
        For Each k_row In k_headers
            _cancel.ThrowIfCancellationRequested()
            Dim k_nir = k_row("IdIntrare"), k_gest = k_row("GestiuneId"), k_exercise = k_row("ExercitiuId")
            Dim k_invoice = Insert(
                "INSERT INTO VC_FACTURI (PARTENER_ID,NUMAR,DATA,TOTAL_CONFIRMAT,MIGRATA,STARE,CREAT_DE) " &
                "VALUES (@p,@n,@d,0,1,'MIGRATA',@u)", "@p", k_row("IdPartener"), "@n", If(k_row("NrFact"), String.Empty),
                "@d", k_row("DataFc"), "@u", _user)
            Dim k_origin = InvoiceRows(
                "SELECT m.SourceId,m.SourceKey FROM VC_IdMap m JOIN VC_ImportSources s ON s.SourceId=m.SourceId " &
                "WHERE s.SubunitId=@s AND s.GestiuneId=@g AND m.TableName='NIR' AND m.Kind='ROW' AND m.TargetId=@n LIMIT 1",
                "@s", k_subunitId, "@g", k_gest, "@n", k_nir).FirstOrDefault()
            Dim k_link = Insert(
                "INSERT INTO VC_NIR_FACTURI (FACTURA_ID,SUBUNIT_ID,GESTIUNE_ID,EXERCITIU_ID,NIR_ID,TOTAL_SURSA,SOURCE_ID,SOURCE_KEY) " &
                "VALUES (@f,@s,@g,@e,@n,@v,@o,@k)", "@f", k_invoice, "@s", k_subunitId, "@g", k_gest,
                "@e", k_exercise, "@n", k_nir, "@v", k_row("Valoare"),
                "@o", If(k_origin Is Nothing, Nothing, k_origin("SourceId")), "@k", If(k_origin Is Nothing, Nothing, k_origin("SourceKey")))
            Dim k_lines = InvoiceRows(
                "SELECT i.*,p.DenumireP,p.UM,t.TVA AS RATE FROM VC_Intrari i " &
                "LEFT JOIN VC_ProduseP pp ON pp.SubunitId=i.SubunitId AND pp.GestiuneId=i.GestiuneId AND pp.IdProdus=i.IdProdus " &
                "LEFT JOIN VC_Produse p ON p.SubunitId=pp.SubunitId AND p.GestiuneId=pp.GestiuneId AND p.ID=pp.ID " &
                "LEFT JOIN VC_TVA t ON t.SubunitId=i.SubunitId AND t.GestiuneId=i.GestiuneId AND t.IdTVA=i.IDTVA " &
                "WHERE i.SubunitId=@s AND i.GestiuneId=@g AND i.ExercitiuId=@e AND i.IdIntrare=@n ORDER BY i.ID",
                "@s", k_subunitId, "@g", k_gest, "@e", k_exercise, "@n", k_nir)
            For Each k_line In k_lines
                Dim k_invoiceLine = Insert(
                    "INSERT INTO VC_FACTURI_LINII (FACTURA_ID,DENUMIRE,UM,CANT,COTA,VALOARE,RECONSTRUITA) VALUES (@f,@n,@u,@q,@t,@v,1)",
                    "@f", k_invoice, "@n", If(k_line("DenumireP"), "Produs sursă"), "@u", k_line("UM"),
                    "@q", k_line("Cant"), "@t", k_line("RATE"), "@v", k_line("ValoareCUTVA"))
                Insert("INSERT INTO VC_FACTURI_REPARTIZARI (NIR_FACTURA_ID,FACTURA_LINIE_ID,SUBUNIT_ID,GESTIUNE_ID,EXERCITIU_ID,INTRARE_ID,CANT,VALOARE) " &
                       "VALUES (@f,@l,@s,@g,@e,@i,@q,@v)", "@f", k_link, "@l", k_invoiceLine, "@s", k_subunitId,
                       "@g", k_gest, "@e", k_exercise, "@i", k_line("ID"), "@q", k_line("Cant"), "@v", k_line("ValoareCUTVA"))
            Next
        Next
        Log($"Facturi: {k_headers.Count} fragmente asociate; totalurile sursă rămân separat pe fiecare NIR.")
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
