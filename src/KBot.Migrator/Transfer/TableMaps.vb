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
    ''' FX_ORD_TBL_REC WAS on this list. It came OFF on 26.08.2026 — correction C1 of
    ''' docs/FUNDAMENT_Asociere_Receptii.md. The operator: «PLAN_ForexeIngest.md IS WRONG.
    ''' The FX_ORD_TBL_REC IS NOT A RELIC. I WAS WRONG! it needs to travel through
    ''' migration and it needs to be used.» It is the link between a payment and the
    ''' ordonanțare line that settled it (FX_Plati ▸ FX_ORD_TBL_REC ▸ FX_ORD_TBL), both
    ''' parents migrate, and the link now survives. Its map is in <see cref="Forexe"/>.
    '''
    ''' FX_Receptii_Plati STAYS out, and more firmly than before (correction C2): «NOT
    ''' used anymore. it contains no data anymore. Excluded completely from migration.»
    ''' It was an early attempt to join a reception to a payment directly, made before the
    ''' flow was understood, and it is empty. FX_ORD_TBL.IDRP pointed at it and is dead
    ''' too — the column stays in the schema, unmapped and unwritten, carrying 0 on every
    ''' sample row and no foreign key on MariaDB.
    '''
    ''' FX_Salarii exists in neither the .accdb nor the MariaDB schema.
    ''' </remarks>
    Public Shared ReadOnly Excluded As IReadOnlyList(Of String) = New String() {
        "FX_DDF_REV_ATT", "FX_DDF_REV_PRT", "FX_ORD_ATT", "FX_ORD_PDF",
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
        ' MAPARE_ACCESS_MARIADB.md §3. PK is IDDF alone as of 24.08 - the operator
        ' dropped the CUAL half of the old composite key, which is also what let
        ' WrittenKeys track FX_DDF at all (PrimaryKeyColumn only handles single-column
        ' PKs) and fixed the FX_DDF_REV orphan-parent check. Cual still travels as an
        ' ordinary column - Rule 2 still holds for it, it's just not part of the key.
        ' The verifier no longer TRUSTS that: slice 0046 added a gate that refuses the run
        ' when a parent's recordable key cannot be determined, so a composite key here
        ' stops the run by name instead of silently disarming every child's orphan check.
        '
        ' D1: IdUnitate is a RELIC on this table. It is excluded not because a mirror
        ' column would be wrong, but because it is never read to decide anything - one
        ' IDDF serves many units, and the live file proves it holds 0 on five of nine rows
        ' while their section A names units 75 and 76. The authority is declared below.
        maps.Add(New TableMap("FX_DDF", "FX_DDF", SourceFile.ForexeFile).
            Exclude("IdUnitate", "IdPartener", "CodPartener", "SS", "DTQ").
            OwnedVia("FX_DDF_REV_SA", "IDDF", "IDDF").
            WithNote("Cheia e IDDF (fostă compusă cu CUAL, până pe 24.08). D1: «IdUnitate» " &
                     "de aici e o relicvă și nu se citește niciodată — un IDDF poate servi " &
                     "mai multe unități. Unitățile documentului vin din «FX_DDF_REV_SA» (D2), " &
                     "citite înainte de prima scriere de «OwnershipPlan»."))

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
        ' --- the Extrase family, selected by CodFiscal (D8) -------------------------
        ' FX_Extrase_F.NumeFisier reads
        ' TREZ521_ExtrasEP_PDFCLI_2842919_XML_SIGNED_03062026h1717.pdf. The file travels
        ' when ANY segment that is entirely digits equals the DC's CodFiscal. No positional
        ' rule and no digging inside a segment: a name with an extra piece in front still
        ' works, and 03062026h1717.pdf cannot collide because it is not all digits. All 72
        ' rows of the live file carry exactly one all-digit segment, 2842919, which is what
        ' the registry holds for 000_DEMO. There is no RO prefix on either side.
        maps.Add(NameMatched("FX_Extrase_F").
            WithNote("Se alege după codul fiscal din numele fișierului (D8): orice segment " &
                     "numeric întreg dintre «_» care e egal cu codul fiscal al DC-ului."))

        ' D10: IdUnitate travels here BY NAME and unfiltered - it is the operator's
        ' information, not an authority. Nothing is derived from it and nothing is filtered
        ' by it, which is why unit 77 (112 rows) and unit 0 (7 rows) no longer raise
        ' anything: the column has no foreign key on the target, so they cost nothing.
        maps.Add(NameMatched("FX_Extrase_H").
            WithNote("D10: «IdUnitate» călătorește ca informație, nefiltrat — antetul " &
                     "pleacă dacă fișierul lui de extras a plecat (D11)."))

        ' D10: the Extrase family carries no LIVE unit at all, and slice 0046 stopped
        ' pretending otherwise. FX_Extrase.IdUnitate is a relic - NULL on all 3.110 rows -
        ' and it leaves the map entirely: the target column is nullable, so nothing needs
        ' to fill it, and the FromUnit that used to write the pass's unit into it was
        ' writing an answer nobody had asked for. Statements are selected by CodFiscal
        ' against FX_Extrase_F.NumeFisier (D8), and a file that travels takes its headers
        ' and their lines with it (D11) - so the row filter is a subtree lookup, not a
        ' unit comparison, and OwnershipPlan holds it.
        maps.Add(NameMatched("FX_Extrase").
            Exclude("IdUnitate").
            WithNote("D10: «IdUnitate» e relicvă (gol pe toate cele 3.110 rânduri) și nu " &
                     "călătorește deloc — coloana-țintă e nulabilă. Rândul pleacă dacă " &
                     "antetul lui a plecat, iar antetul dacă fișierul de extras s-a " &
                     "potrivit pe cod fiscal (D8, D11)."))
        maps.Add(NameMatched("FX_Receptii_R"))

        ' --- FX_DDF_REV_SA ---------------------------------------------------------
        ' IdClsf is NOT NULL with a foreign key, so a resolution miss is BLOCKING.
        ' IdClsfAcc is NOT NULL with no default on the target - contradicting
        ' MAPARE_ACCESS_MARIADB.md §5, which claims it was made nullable on 22.08. It was
        ' not. Decision D9: write the Access IdClsf into it, which is exactly what
        ' IdClsfAcc means on Clasificatii.
        ' IdPartener travels as NULL (§5.2) and the count is logged per table.
        '
        ' D12 reverses slice 0045-07 here on two counts, and both reversals matter.
        ' (1) FromUnit is GONE. The loop's unit is not the row's unit: this table IS the
        '     authority (D2), so its own IdUnitate is the answer and it travels by name.
        '     Writing the pass's unit over it was only ever safe while the pass had one
        '     unit, and D4 collapsed the per-unit loop away.
        ' (2) OwnedVia("FX_DDF", ...) is GONE. The arrow points the other way now - the
        '     parent asks SA, SA never asks the parent - and the declaration moved onto
        '     FX_DDF, where it reads as what it is.
        ' Four of the 32 rows carry IdUnitate = NULL (verified 23.08, still true 24.08:
        ' IDDF 73, 77, 79, 80, one line each). Under D5 those four documents keep their
        ' whole subtree behind, so a NULL never reaches the target and the nullable target
        ' column stays unexercised - it is NOT filled from FX_DDF any more, because that
        ' column is a relic (D1).
        maps.Add(New TableMap("FX_DDF_REV_SA", "FX_DDF_REV_SA", SourceFile.ForexeFile).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Add(ColumnMapping.FromAccess("IdClsfAcc", "IdClsf")).
            Add(ColumnMapping.AlwaysNull("IdPartener")).
            Exclude("ID", "IdClsfPY").
            WithNote("D9: «IdClsfAcc» e NOT NULL fără implicit, deci primește IdClsf-ul " &
                     "Access. «IdPartener» pleacă NULL (§5.2). D2/D12: tabelul ăsta E " &
                     "autoritatea — «IdUnitate» al lui e răspunsul, călătorește ca atare, " &
                     "și el spune cărei unități aparține documentul, nu invers."))

        maps.Add(New TableMap("FX_DDF_REV_SB", "FX_DDF_REV_SB", SourceFile.ForexeFile).
            Add(ColumnMapping.FromClasificatie("IdClsf", "IdClsf", True)).
            Add(ColumnMapping.FromAccess("IdClsfAcc", "IdClsf")).
            Add(ColumnMapping.AlwaysNull("IdPartener")).
            Exclude("ID", "IdClsfPY").
            WithNote("Aceeași formă ca SA, dar NU e autoritate: D2 spune că unitatea " &
                     "documentului vine din «FX_DDF_REV_SA» și din nicio altă parte, " &
                     "deși coloana există și aici. «IdUnitate» al rândului decide doar " &
                     "dacă pleacă rândul, nu dacă pleacă documentul."))

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
        ' D3: FX_ORD carries no IdUnitate at all - vestigial or otherwise - so the only
        ' place an order's units exist is FX_ORD_TBL, and that is where they are read from.
        ' Verified on the live file: all seventeen orders resolve to unit 76, none NULL.
        maps.Add(New TableMap("FX_ORD", "FX_ORD", SourceFile.ForexeFile).
            Rename("IDORD", "IDORDP").
            Exclude("IDORDP", "IDRR", "IDRH", "ArePDF", "CalePDF", "DTQ").
            OwnedVia("FX_ORD_TBL", "IDORD", "IDORD").
            WithNote("Opțiunea (A): IDORD-ul Access se scrie explicit în IDORDP. " &
                     "IDORDP-ul Access (117+) NU pleacă. D3: tabelul n-are deloc " &
                     "«IdUnitate» — unitățile ordonanțării vin din «FX_ORD_TBL», citite " &
                     "înainte de prima scriere de «OwnershipPlan»."))

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
            WithNote("«IDRP» e mort: arăta către FX_Receptii_Plati, care e gol și exclus " &
                     "complet din migrare (corecția C2). Rămâne în schemă, nemapat. " &
                     "«CodAI» - a treia grafie a coloanei; potrivirea e neinsensibilă la caz."))

        maps.Add(New TableMap("FX_ORD_DOC", "FX_ORD_DOC", SourceFile.ForexeFile).
            Rename("IDORDDOC", "IDORDDOCP").
            Rename("IDORD", "IDORDP").
            Rename("IDORDPART", "IDORDPARTP").
            Exclude("IDORDDOCP", "IDORDP", "IDORDJ").
            WithNote("Oglinzile Access au derivat vizibil (17▸40, 18▸41, 19▸42), " &
                     "de-aia nu sunt o sursă folosibilă."))

        ' --- FX_ORD_TBL_REC --------------------------------------------------------
        ' Legătura plată ▸ rând de ordonanțare. Ambii părinți sunt deja scriși mai sus:
        ' FX_Plati și FX_ORD_TBL. Coloanele Access, citite din fișierul real
        ' (artifacts/accdb-schema/FX_2026.md):
        '
        '   IDORDREC   1..n     cheia Access — călătorește ca ea însăși
        '   IDORDRECP  475..    oglinda serverului VECHI — NU călătorește; pe MariaDB
        '                       coloana omonimă e AUTO_INCREMENT și și-o pune singură
        '   IDORDTBL            rândul de ordonanțare ▸ se redenumește IDORDTBLP, fiindcă
        '                       acolo a aterizat cheia lui FX_ORD_TBL (vezi harta de mai sus)
        '   IDRP       0 peste tot — MORT (corecția C2), nemapat
        '   Valoare, IdPlataFX  direct
        maps.Add(New TableMap("FX_ORD_TBL_REC", "FX_ORD_TBL_REC", SourceFile.ForexeFile).
            Rename("IDORDTBL", "IDORDTBLP").
            Exclude("IDORDRECP", "IDRP", "IdClsfPY", "DTQ").
            WithNote("Scos din lista de excluderi pe 26.08.2026 (corecția C1): NU e o " &
                     "relicvă. «IDRP» rămâne nemapat — arăta către FX_Receptii_Plati, mort."))

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
