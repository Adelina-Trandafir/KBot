Imports KBot.Migrator
Imports System.Globalization

''' <summary>
''' Throwaway harness: drives the PRODUCTION Access classes against the real files.
''' Not in KBot.sln, no UI, nothing written anywhere.
''' </summary>
''' <remarks>
''' The 0045-01 spike proved ACE opens the files, but it was separate code in a separate
''' process. This proves the classes the migrator actually uses - AccessProvider,
''' AccessSchema, AccessTableReader, CaiRegistry, ColumnPlan - do the same.
''' </remarks>
Friend Module Smoke

    Friend Sub Main(args As String())
        Dim registry = If(args.Length > 0, args(0), "Avacont\cale.accdb")
        Dim password = If(args.Length > 1, args(1), String.Empty)
        If password = "-" Then password = String.Empty

        Console.OutputEncoding = Text.Encoding.UTF8

        Try
            Console.WriteLine("== CaiRegistry.Read ==")
            Dim units = CaiRegistry.Read(registry, password)
            Console.WriteLine($"units: {units.Count}")

            Console.WriteLine()
            Console.WriteLine("== DistinctDcs ==")
            For Each dc In CaiRegistry.DistinctDcs(units)
                Dim ofDc = CaiRegistry.UnitsOf(units, dc)
                Console.WriteLine($"  {dc}: {ofDc.Count} units")
            Next

            Console.WriteLine()
            Console.WriteLine("== units (id / name / source / files resolved?) ==")
            For Each u In units
                Console.WriteLine(
                    $"  {u.IdUnitate,5} {u.NumeUnitate,-28} {u.Sursa,-4} " &
                    $"nom={If(u.HasUnitFile, "yes", "NO "),3} fx={If(u.HasForexeFile, "yes", "NO "),3}  {u.UnitFilePath}")
            Next

            Dim withUnit = units.FirstOrDefault(Function(u) u.HasUnitFile)
            If withUnit IsNot Nothing Then
                Console.WriteLine()
                Console.WriteLine($"== AccessSchema over {withUnit.NumeUnitate} ==")
                Using cn = AccessProvider.Open(withUnit.UnitFilePath, password)
                    Dim tables = AccessSchema.TableNames(cn)
                    Console.WriteLine($"tables: {tables.Count}")
                    For Each wanted In {"Clasificatii", "Parteneri", "Rectificari", "ParteneriAng", "UNIT"}
                        Dim real1 = AccessSchema.ResolveTableName(cn, wanted)
                        If real1 Is Nothing Then
                            Console.WriteLine($"  {wanted,-14} ABSENT")
                            Continue For
                        End If
                        Dim columns = AccessSchema.Columns(cn, real1)
                        Console.WriteLine($"  {real1,-14} rows={AccessSchema.CountRows(cn, real1),6}  cols={columns.Count}")
                    Next

                    Console.WriteLine()
                    Console.WriteLine("== AccessTableReader over Clasificatii (first 3) ==")
                    Dim clsf = AccessSchema.ResolveTableName(cn, "Clasificatii")
                    If clsf IsNot Nothing Then
                        Using reader = AccessSchema.OpenReader(cn, clsf)
                            Console.WriteLine("  columns: " & String.Join(", ", reader.ColumnNames.Take(8)) & " …")
                            Console.WriteLine("  HasColumn(idunitate) = " & reader.HasColumn("idunitate").ToString() &
                                              "   (expected False - the unit is NOT on the row)")
                            Dim shown = 0
                            While reader.Read() AndAlso shown < 3
                                shown += 1
                                Dim id = reader.ValueOrMissing("IDClsf")
                                Dim cap = reader.ValueOrMissing("capitol")
                                Dim den = reader.ValueOrMissing("Denumire")
                                Dim missing = reader.ValueOrMissing("ColoanaCareNuExista")
                                Console.WriteLine($"  IDClsf={id} Capitol={cap} Denumire={Truncate(Convert.ToString(den, CultureInfo.InvariantCulture), 30)}")
                                Console.WriteLine($"     absent column -> {If(missing Is Nothing, "Nothing (correct)", "NOT Nothing (WRONG)")}")
                            End While
                        End Using
                    End If
                End Using
            End If

            Console.WriteLine()
            Console.WriteLine("== ClasificatieDerived (replicates the GENERATED columns) ==")
            Dim d As New ClasificatieDerived("65.01", "04.02", "10.01", "01")
            Console.WriteLine($"  Clsf={d.Clsf} Titlu={d.Titlu} ClsfF={d.ClsfF} ClsfE={d.ClsfE} Sector={d.Sector} Sursa={d.Sursa} SS={d.SS}")

            Console.WriteLine()
            Console.WriteLine("== TableMaps catalogue ==")
            Dim maps = TableMaps.All()
            Console.WriteLine($"  tables in catalogue: {maps.Count}")
            Console.WriteLine($"  name-match-only:     {maps.Where(Function(m) m.NameMatchOnly).Count}")
            Console.WriteLine($"  excluded by decision:{TableMaps.Excluded.Count}")

            Console.WriteLine()
            Console.WriteLine("OK")

        Catch ex As Exception
            Console.WriteLine()
            Console.WriteLine("FAILED: " & ex.GetType().FullName)
            Console.WriteLine(ex.Message)
            Console.WriteLine(ex.StackTrace)
            Environment.ExitCode = 1
        End Try
    End Sub

    Private Function Truncate(value As String, length As Integer) As String
        If value Is Nothing Then Return "(null)"
        If value.Length <= length Then Return value
        Return value.Substring(0, length) & "…"
    End Function

End Module
