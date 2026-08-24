Imports KBot.Common

''' <summary>
''' The Access ▸ MariaDB catalogue, in write order.
''' </summary>
''' <remarks>
''' <para>
''' Every entry here traces to <c>MAPARE_ACCESS_MARIADB.md</c> (the DDF and ORD families,
''' §3 and §4) or <c>docs/MAPARE_NOMENCLATOARE.md</c> (the nomenclators). Nothing is
''' inferred from a column name. Where a table has no explicit mapping in either file, it
''' is marked <see cref="TableMap.NameMatchOnly"/> and the verifier says so out loud
''' rather than letting it pass silently.
''' </para>
''' <para>
''' The order below is the EXPECTED result of the topological sort, not a substitute for
''' it - <see cref="WriteOrder.Derive"/> reads the live foreign keys and this list is only
''' the seed order it stabilises. FX_DDF and FX_DDF_REV sit before FX_Rezervari
''' deliberately: FX_Rezervari.IDREV has a foreign key to FX_DDF_REV, and that pair in the
''' wrong order is what produced <c>1452 ... FX_Rezervari__FX_DDF_REV</c> on 21.08.
''' </para>
''' </remarks>
Public NotInheritable Class TableMaps

    ''' <summary>The year every transferred row belongs to (decision D1).</summary>
    Public Const TransferYear As Integer = 2026

    ''' <summary>
    ''' Tables deliberately NOT migrated - MAPARE_ACCESS_MARIADB.md §2 plus decision D4.
    ''' </summary>
    ''' <remarks>
    ''' FX_ORD_TBL_REC is the link between a payment and the order line it settled
    ''' (FX_Plati ▸ FX_ORD_TBL_REC ▸ FX_ORD_TBL). Both its parents are migrated; leaving
    ''' it out is deliberate, and the link does not survive the migration.
    ''' FX_Salarii and FX_Receptii_Plati (D4) exist in neither the .accdb nor the MariaDB
    ''' schema, which is why FX_ORD_TBL.IDRP is an orphan column by construction.
    ''' </remarks>
    Public Shared ReadOnly Excluded As IReadOnlyList(Of String) = New String() {
        "FX_DDF_REV_ATT", "FX_DDF_REV_PRT", "FX_ORD_ATT", "FX_ORD_TBL_REC", "FX_ORD_PDF",
        "FX_Salarii", "FX_Receptii_Plati",
        "ClasificatiiV", "RectificariV", "ParteneriSI"
    }

    Private Sub New()
    End Sub

    ''' <summary>The nomenclator tables, in the order they must be written.</summary>
    Public Shared Function Nomenclators() As List(Of TableMap)
        Dim maps As New List(Of TableMap)()

        ' --- Unitati ---------------------------------------------------------------
        ' Not read from Access at all: built from the cai registry. Decision D7 makes
        ' this unavoidable - a database created from AVACONT_SURSA has structure but no
        ' rows, so Unitati is EMPTY, and Clasificatii, Clasificatii_Buget, Parteneri and
        ' FX_ORD_TBL all carry a foreign key into it. Nothing can be written until it is
        ' populated. IdUnitate is a primary key WITHOUT auto_increment, so it cannot
        ' appear by itself.
        maps.Add(New TableMap(String.Empty, "Unitati", SourceFile.Derived).
            Add(ColumnMapping.FromUnit("IdUnitate")).
            Add(ColumnMapping.FromConstant("An", TransferYear)).
            Add(ColumnMapping.FromConstant("Ascuns", 0)).
            Add(ColumnMapping.WrittenElsewhere("Detalii")).
            Add(ColumnMapping.WrittenElsewhere("SursaSector")).
            Add(ColumnMapping.WrittenElsewhere("CodProgram")).
            ScopedBy("IdUnitate").
            WithNote("Construit din registrul «cai»; «Detalii», «SursaSector» și «CodProgram» " &
                     "sunt scrise de TransferRunner.WriteUnitati, în afara acestui plan - " &
                     "declarația lor aici există doar ca verificatorul să le vadă acoperite, " &
                     "nu ca să poarte o valoare reală. «Detalii» ia UNIT.Detalii, apoi " &
                     "«AlteDetalii» din «cai», apoi numele unității."))

        ' --- Clasificatii ----------------------------------------------------------
        ' Rule 1: Access IDClsf goes into IdClsfAcc, and MariaDB assigns IDClsf.
        ' The rename RELEASES the name match it replaces - without that, Access IDClsf
        ' would also match the target's own IDClsf (auto_increment is writable) and the
        ' Access id would land in the primary key.
        ' Nine target columns are GENERATED (Clsf, Titlu, ClsfSal, ClsfF, ClsfE, ClsfX,
        ' Sector, Sursa, SS) and need no exclusion: they are not writable at all.
        maps.Add(New TableMap("Clasificatii", "Clasificatii", SourceFile.UnitFile).
            Rename("IDClsf", "IdClsfAcc").
            Add(ColumnMapping.FromUnit("IdUnitate")).
            Exclude("IdClsfPY", "TOTAL", "TOTALFX", "CodSSI", "CodAng", "CodInd",
                    "DTQ", "Esinc", "Document", "Data", "IdLegatura",
                    "Trim1", "Trim2", "Trim3", "Trim4").
            AsInsertOnly("IdUnitate").
            Feeding(ResolutionTarget.Clasificatii, "IDClsf").
            WithNote("D8: fără cheie unică pe (IdClsfAcc, IdUnitate), deci NU se poate " &
                     "face upsert. Se scrie o singură dată; o a doua rulare e refuzată."))

        ' --- Clasificatii_Buget ----------------------------------------------------
        ' Same Access table, second target: trim1..trim4 become four COLUMNS of one row,
        ' not four rows (settled by the DDL - UNIQUE (IdClsf, An)).
        ' Access IDClsf is excluded because it would case-insensitively match the target's
        ' IdClsf and write the RAW Access id where the RESOLVED one belongs.
        ' TOTAL is GENERATED on the target and therefore never written.
        maps.Add(New TableMap("Clasificatii", "Clasificatii_Buget", SourceFile.UnitFile).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IDClsf", True)).
            Add(ColumnMapping.FromUnit("IdUnitate")).
            Add(ColumnMapping.FromConstant("An", TransferYear)).
            Exclude("IDClsf", "IdClsfPY", "TOTAL", "TOTALFX", "Capitol", "Subcapitol",
                    "Articol", "Alineat", "Denumire", "CodSSI", "CodAng", "CodInd",
                    "DTQ", "Esinc", "Document", "Data", "IdLegatura").
            WithNote("An = 2026, scris fix (D1). Tabelul-sursă e tot «Clasificatii»."))

        ' --- Clasificatii_Rectificari ----------------------------------------------
        ' Access ID is excluded: the target's ID is its own auto_increment and a name
        ' match would write the Access key into it.
        ' The unique key (IdClsf, Data, Document) contains two NULLABLE columns, and in
        ' MariaDB a NULL never equals a NULL in a unique index - so a rectification with a
        ' NULL Data or Document will not match on a re-run. Harmless for a one-off (D1).
        maps.Add(New TableMap("Rectificari", "Clasificatii_Rectificari", SourceFile.UnitFile).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Exclude("ID", "DTQ", "Esinc").
            WithNote("Gol în fișierul disponibil - maparea e doar de schemă, " &
                     "niciun rând nu a trecut vreodată prin ea."))

        ' --- Parteneri -------------------------------------------------------------
        ' Access carries THREE id-shaped columns and only CodPartener travels:
        '   IdPartener 7605+ - live ids on the server, the IdClsfPY trap wearing another
        '                      name, and it would name-match the target's auto PK;
        '   IDPART     1,2,3 - a purely local sequence;
        '   CodPartener "001" - the real key, UNIQUE (IdUnitate, CodPartener) on the target.
        maps.Add(New TableMap("Parteneri", "Parteneri", SourceFile.UnitFile).
            Add(ColumnMapping.FromUnit("IdUnitate")).
            Exclude("IdPartener", "IDPART", "NumePartener", "ContPl", "F8", "F9",
                    "CodClient", "DTQ", "Esinc").
            ScopedBy("IdUnitate").
            Feeding(ResolutionTarget.Parteneri, "CodPartener").
            WithNote("«Ascuns» călătorește - ruta Flask nu-l scrie, dar ținta îl are."))

        ' --- Parteneri_Coduri ------------------------------------------------------
        ' In scope by operator decision, 23.08. The only target found that keeps IdClsf
        ' and IdClsfAcc side by side, so the Access IdClsf feeds BOTH: resolved into
        ' IdClsf, raw into IdClsfAcc.
        maps.Add(New TableMap("ParteneriAng", "Parteneri_Coduri", SourceFile.UnitFile).
            Add(ColumnMapping.FromPartener("IdPartener", "CodPartener", True)).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Add(ColumnMapping.FromAccess("IdClsfAcc", "IdClsf")).
            Rename("ContBanca", "ContBancar").
            Exclude("Id", "Clsf", "DTQ").
            WithNote("Singura redenumire e «ContBanca» ▸ «ContBancar». " &
                     "IdClsf-ul Access hrănește AMÂNDOUĂ coloanele țintei."))

        Return maps
    End Function

    ''' <summary>The FX_* tables, in the expected write order.</summary>
    Public Shared Function Forexe() As List(Of TableMap)
        Dim maps As New List(Of TableMap)()

        maps.Add(NameMatched("FX_Angajamente"))
        maps.Add(NameMatched("FX_Indicatori"))

        ' --- FX_DDF ----------------------------------------------------------------
        ' MAPARE_ACCESS_MARIADB.md §3. The upsert matches on (IDDF, CUAL), both present.
        ' Cual is Integer on both sides here - Rule 2 holds for FX_DDF.
        maps.Add(New TableMap("FX_DDF", "FX_DDF", SourceFile.ForexeFile).
            Exclude("IdUnitate", "IdPartener", "CodPartener", "SS", "DTQ").
            WithNote("Cheia de upsert e (IDDF, CUAL), compusă."))

        ' --- FX_DDF_REV ------------------------------------------------------------
        ' ESpeciala is on MariaDB and absent from Access (verified 0045-01 on the live
        ' file), and it is nullable there, so it simply does not travel.
        maps.Add(New TableMap("FX_DDF_REV", "FX_DDF_REV", SourceFile.ForexeFile).
            Exclude("ArePDFDDF", "CalePDFDDF", "AreDDF", "CaleDDF").
            WithNote("«ESpeciala» e doar pe MariaDB, nulabilă - nu călătorește."))

        maps.Add(NameMatched("FX_Istoric"))
        maps.Add(NameMatched("FX_Rezervari"))
        maps.Add(NameMatched("FX_Receptii_H"))
        maps.Add(NameMatched("FX_Receptii"))
        maps.Add(NameMatched("FX_Plati"))
        maps.Add(NameMatched("FX_Extrase_F"))
        maps.Add(NameMatched("FX_Extrase_H"))

        ' FX_Extrase carries an IdUnitate column and it is NULL on ALL 3.110 rows, so
        ' without a chain the whole table would be read as belonging to every selected
        ' unit and written once per unit. The owner is the statement header: every row
        ' joins FX_Extrase_H on IDFXH ▸ IDEXH with zero orphans (checked 23.08 on the
        ' live file, 3.110 rows against 338 headers), and FX_Extrase_H.IdUnitate is
        ' filled in on all of them.
        ' FromUnit for the same reason: the rows are filtered to their real owner now, so
        ' the unit of the pass IS the row's unit, and writing it beats writing the NULL
        ' the Access column holds.
        maps.Add(NameMatched("FX_Extrase").
            OwnedVia("FX_Extrase_H", "IDFXH", "IDEXH").
            Add(ColumnMapping.FromUnit("IdUnitate")).
            WithNote("«IdUnitate» e gol pe toate cele 3.110 rânduri Access; unitatea vine " &
                     "din «FX_Extrase_H» prin «IDFXH» ▸ «IDEXH» și se scrie pe țintă."))
        maps.Add(NameMatched("FX_Receptii_R"))

        ' --- FX_DDF_REV_SA ---------------------------------------------------------
        ' IdClsf is NOT NULL with a foreign key, so a resolution miss is BLOCKING.
        ' IdClsfAcc is NOT NULL with no default on the target - contradicting
        ' MAPARE_ACCESS_MARIADB.md §5, which claims it was made nullable on 22.08. It was
        ' not. Decision D9: write the Access IdClsf into it, which is exactly what
        ' IdClsfAcc means on Clasificatii.
        ' IdPartener travels as NULL (§5.2) and the count is logged per table.
        ' OwnedVia: four of the 32 rows carry IdUnitate = NULL (verified 23.08 on the live
        ' file). Their unit comes from FX_DDF through IDDF - 138 ▸ IDDF 73 ▸ unit 76, and
        ' 146/150/152 ▸ IDDF 77/79/80 ▸ unit 75. Without the chain each of the four was
        ' checked against EVERY selected unit and resolved IdClsf against the wrong
        ' nomenclator, which is what produced the mirrored 141 / 97+374 findings.
        maps.Add(New TableMap("FX_DDF_REV_SA", "FX_DDF_REV_SA", SourceFile.ForexeFile).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Add(ColumnMapping.FromAccess("IdClsfAcc", "IdClsf")).
            Add(ColumnMapping.AlwaysNull("IdPartener")).
            Add(ColumnMapping.FromUnit("IdUnitate")).
            Exclude("ID", "IdClsfPY").
            OwnedVia("FX_DDF", "IDDF", "IDDF").
            WithNote("D9: «IdClsfAcc» e NOT NULL fără implicit, deci primește IdClsf-ul " &
                     "Access. «IdPartener» pleacă NULL (§5.2). Rândurile cu «IdUnitate» " &
                     "gol își află unitatea din «FX_DDF» prin «IDDF», și pe țintă se " &
                     "scrie unitatea aflată, nu golul din Access."))

        maps.Add(New TableMap("FX_DDF_REV_SB", "FX_DDF_REV_SB", SourceFile.ForexeFile).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Add(ColumnMapping.FromAccess("IdClsfAcc", "IdClsf")).
            Add(ColumnMapping.AlwaysNull("IdPartener")).
            Add(ColumnMapping.FromUnit("IdUnitate")).
            Exclude("ID", "IdClsfPY").
            OwnedVia("FX_DDF", "IDDF", "IDDF").
            WithNote("Aceeași formă ca SA, inclusiv cele patru rânduri fără «IdUnitate»."))

        ' --- FX_ORD ----------------------------------------------------------------
        ' Option (A), MAPARE_ACCESS_MARIADB.md §0: the Access id is written explicitly
        ' into the AUTO_INCREMENT primary key. Auto-increment only fires when the column
        ' is OMITTED; supplied, the value lands verbatim, every parent link is correct,
        ' and the upsert has a real key to match on.
        ' The rename releases the name match it replaces, so Access IDORD does NOT also
        ' land in the target's legacy 'ACCESS' varchar IDORD (Rule 3 - it stays empty).
        ' Access IDORDP carries 117..123, the OLD server's ids, and is never sent.
        ' CUAL is varchar on BOTH sides here, so no conversion - Rule 2 is wrong both
        ' ways for FX_ORD.
        maps.Add(New TableMap("FX_ORD", "FX_ORD", SourceFile.ForexeFile).
            Rename("IDORD", "IDORDP").
            Exclude("IDORDP", "IDRR", "IDRH", "ArePDF", "CalePDF", "DTQ").
            WithNote("Opțiunea (A): IDORD-ul Access se scrie explicit în IDORDP. " &
                     "IDORDP-ul Access (117+) NU pleacă."))

        maps.Add(New TableMap("FX_ORD_PART", "FX_ORD_PART", SourceFile.ForexeFile).
            Rename("IDORDPART", "IDORDPARTP").
            Rename("IDORD", "IDORDP").
            Exclude("IDORDPartP", "IDORDP", "IdPartener", "CodPartener").
            WithNote("«IdPartener»/«CodPartener» sunt doar în Access - ținta nu le are."))

        ' --- FX_ORD_TBL ------------------------------------------------------------
        ' IdClsf is nullable with DEFAULT 0 on the target, so a miss COULD be nulled -
        ' but blocking is chosen deliberately: a silently unclassified order line is
        ' worse than a refusal.
        ' IdUnitate is NOT NULL with a foreign key to Unitati: it must travel and cannot
        ' be nulled, which is why Unitati has to be populated first.
        ' IDRP points at FX_Receptii_Plati, which never travels (D4), so it is an orphan
        ' column by construction and is excluded rather than written as a dangling id.
        maps.Add(New TableMap("FX_ORD_TBL", "FX_ORD_TBL", SourceFile.ForexeFile).
            Rename("IDORDTBL", "IDORDTBLP").
            Rename("IDORD", "IDORDP").
            Rename("IDORDPART", "IDORDPARTP").
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Add(ColumnMapping.AlwaysNull("IdPartener")).
            Exclude("IDORDTBLP", "IDORDP", "IdClsfPY", "IDRR", "IDORDT", "IDRD", "IDRP").
            WithNote("«IDRP» e orfan prin construcție (FX_Receptii_Plati nu călătorește, D4). " &
                     "«CodAI» - a treia grafie a coloanei; potrivirea e neinsensibilă la caz."))

        maps.Add(New TableMap("FX_ORD_DOC", "FX_ORD_DOC", SourceFile.ForexeFile).
            Rename("IDORDDOC", "IDORDDOCP").
            Rename("IDORD", "IDORDP").
            Rename("IDORDPART", "IDORDPARTP").
            Exclude("IDORDDOCP", "IDORDP", "IDORDJ").
            WithNote("Oglinzile Access au derivat vizibil (17▸40, 18▸41, 19▸42), " &
                     "de-aia nu sunt o sursă folosibilă."))

        maps.Add(NameMatched("FX_Receptii_RHR"))
        maps.Add(NameMatched("FX_Rezervarii_IMG"))
        maps.Add(NameMatched("FX_Receptii_IMG"))

        Return maps
    End Function

    ''' <summary>Nomenclators followed by the FX_* tables.</summary>
    Public Shared Function All() As List(Of TableMap)
        Dim maps = Nomenclators()
        maps.AddRange(Forexe())
        Return maps
    End Function

    ''' <summary>Finds a map by target table name, or Nothing.</summary>
    Public Shared Function ByTarget(maps As IEnumerable(Of TableMap), targetTable As String) As TableMap
        Return maps.FirstOrDefault(
            Function(m) String.Equals(m.TargetTable, targetTable, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>
    ''' A table with no explicit mapping in either MAPARE file: every column travels by
    ''' name, case-insensitively, and IdClsfPY is dropped wherever it appears.
    ''' </summary>
    ''' <remarks>
    ''' MAPARE_ACCESS_MARIADB.md documents only the DDF and ORD families column by column.
    ''' For the rest, a case-insensitive name match is the documented default (Rule 4) and
    ''' is what the Python side did - but it is NOT a verified mapping, so the verifier
    ''' raises an informational finding naming every such table. The operator sees which
    ''' tables are travelling on a name match rather than on a read mapping.
    ''' </remarks>
    Private Shared Function NameMatched(table As String) As TableMap
        Dim map As New TableMap(table, table, SourceFile.ForexeFile)
        map.NameMatchOnly = True
        ' Rule 1: IdClsfPY never travels, in any table, under any spelling. The exclusion
        ' set is case-insensitive, so IdClsfPy and IdClsfPY are the same entry.
        map.Exclude("IdClsfPY", "DTQ")
        Return map
    End Function

End Class
