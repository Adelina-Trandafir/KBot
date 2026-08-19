Attribute VB_Name = "mdl_FX_ExportSeed"
'=============================================================================
' Modul: mdl_FX_ExportSeed          (felia 0042 — migrare FX_ Access -> MariaDB)
'-----------------------------------------------------------------------------
' Exports the 16 un-migrated FX_ tables of FX_2026.accdb, plus [Cai] from
' cale.accdb, as chunked UTF-8 JSON files under <CurrentProject.Path>\VBA_ARTEFACTS.
' KBot.Migrator then reads those files and writes to MariaDB over HTTP. That is why
' this module exists at all: the .NET side must never open an Access file, so it never
' needs an ACE/OleDb driver and there is no 32/64-bit question on the .NET side.
'
' NOT a variant of mdl_FX_ExportMD, and it does not call into it. That module is fine
' for its own purpose but disqualified for a migration on four counts: it truncates
' Memo fields to Left(s,5) & "..." & Right(s,5), it collapses Null to "" via Nz, it
' emits every value as a quoted Romanian-locale string, and it recurses one query per
' parent row. All four lose or corrupt data. It stays untouched; this is separate.
'
' The chunk file shape is EXACTLY what POST /api/forexe/seed/rows already accepts:
' positional value arrays plus one shared column list.
'
' House rules honoured here: all Dim at the top of each procedure, no DLookup, no Nz,
' English comments, Romanian message boxes with real diacritics, errors raised and
' never swallowed (the Fail labels clean up and then re-raise).
'=============================================================================

Option Compare Database
Option Explicit

' --- output ------------------------------------------------------------------
Private Const OUT_FOLDER As String = "VBA_ARTEFACTS"

' --- sources -----------------------------------------------------------------
' Both databases carry the same password. Hard-coded on purpose: the registry branch
' HKCU\Software\VB and VBA Program Settings\AVACONT is NOT used by this migration.
Private Const DEFAULT_FX_PATH As String = "C:\Avacont\FOREXE\FX_2026.accdb"
Private Const DEFAULT_CAI_PATH As String = "C:\AVACONT\cale.accdb"
Private Const DB_PASSWORD As String = "andreI"

' --- chunking ----------------------------------------------------------------
' 500 rows per file normally; 50 for the Memo-heavy tables, whose rows are orders of
' magnitude larger (base64 images, whole bank-statement XML documents, long free text).
Private Const CHUNK_ROWS_DEFAULT As Long = 500
Private Const CHUNK_ROWS_MEMO As Long = 50

' --- ADODB.Stream, late-bound ------------------------------------------------
' Late binding so the host project needs no ADO reference. The names are prefixed
' (STM_*) so they cannot collide with the real ADO constants if a reference does exist.
Private Const STM_BINARY As Long = 1
Private Const STM_TEXT As Long = 2
Private Const STM_OVERWRITE As Long = 2

' Tables declared out of scope. If either turns up in TableDefs it is REPORTED (in the
' manifest and in the closing message box) and still NOT exported — see the plan, §6.
Private Const OUT_OF_SCOPE_1 As String = "FX_PRT_EXPL"
Private Const OUT_OF_SCOPE_2 As String = "FX_CopacAngajamente"


'=============================================================================
' ENTRY POINT
'
'   sOutRoot  - output folder; empty => CurrentProject.Path & "\VBA_ARTEFACTS"
'   sFxPath   - FX_2026.accdb (the external FOREXE back end)
'   sCaiPath  - cale.accdb, which holds [Cai] (IdUnitate -> DC)
'=============================================================================
Public Sub FX_ExportSeed(Optional sOutRoot As String = "", _
                         Optional sFxPath As String = DEFAULT_FX_PATH, _
                         Optional sCaiPath As String = DEFAULT_CAI_PATH)
Dim DB          As DAO.Database
Dim sOutDir     As String
Dim vTables     As Variant
Dim I           As Long
Dim sTable      As String
Dim colFiles    As Collection
Dim colManifest As Collection
Dim sColumns    As String
Dim lRows       As Long
Dim lTotalRows  As Long
Dim sUnexpected As String
Dim sCaiColumns As String
Dim lCaiRows    As Long
Dim sMsg        As String
Dim lErrNum     As Long
Dim sErrDesc    As String
Dim sErrSrc     As String

    Set DB = Nothing

    On Error GoTo Fail

    ' --- 1. output folder ----------------------------------------------------
    If Len(Trim$(sOutRoot)) = 0 Then
        sOutDir = CurrentProject.Path & "\" & OUT_FOLDER
    Else
        sOutDir = sOutRoot
    End If
    EnsureFolder sOutDir

    ' --- 2. open the external FX back end ------------------------------------
    If Len(Dir$(sFxPath)) = 0 Then
        Err.Raise vbObjectError + 3001, "FX_ExportSeed", _
                  "Baza sursă nu există: " & sFxPath
    End If
    Set DB = DBEngine.OpenDatabase(sFxPath, False, True, ";PWD=" & DB_PASSWORD)

    ' --- 3. every listed table must exist. A missing one is a hard error, named.
    vTables = SeedTableList()
    For I = LBound(vTables) To UBound(vTables)
        sTable = CStr(vTables(I))
        If Not TableExists(DB, sTable) Then
            Err.Raise vbObjectError + 3002, "FX_ExportSeed", _
                      "Tabelul «" & sTable & "» lipsește din " & sFxPath & _
                      ". Exportul nu poate continua."
        End If
    Next I

    ' --- 4. report (do not export) the two tables declared out of scope -------
    sUnexpected = ""
    If TableExists(DB, OUT_OF_SCOPE_1) Then sUnexpected = OUT_OF_SCOPE_1
    If TableExists(DB, OUT_OF_SCOPE_2) Then
        If Len(sUnexpected) > 0 Then sUnexpected = sUnexpected & ","
        sUnexpected = sUnexpected & OUT_OF_SCOPE_2
    End If

    ' --- 5. [Cai] from cale.accdb --------------------------------------------
    ' Exported here so KBot.Migrator never has to open an Access file: IdUnitate -> DC
    ' is the routing key the whole migration depends on.
    lCaiRows = ExportCai(sCaiPath, sOutDir, sCaiColumns)

    ' --- 6. the 16 tables, parents before children ---------------------------
    Set colManifest = New Collection
    lTotalRows = 0
    For I = LBound(vTables) To UBound(vTables)
        sTable = CStr(vTables(I))
        Set colFiles = New Collection
        sColumns = ""
        lRows = ExportTable(DB, sTable, sOutDir, colFiles, sColumns)
        lTotalRows = lTotalRows + lRows
        colManifest.Add ManifestEntry(sTable, sColumns, lRows, colFiles)
    Next I

    ' --- 7. manifest last: it carries the row counts, which are only known now -
    WriteManifest sOutDir, sFxPath, sCaiPath, colManifest, _
                  sCaiColumns, lCaiRows, sUnexpected

    DB.Close
    Set DB = Nothing

    sMsg = "Export încheiat." & vbCrLf & vbCrLf & _
           "Folder: " & sOutDir & vbCrLf & _
           "Tabele: " & CStr(UBound(vTables) - LBound(vTables) + 1) & vbCrLf & _
           "Rânduri FX_: " & CStr(lTotalRows) & vbCrLf & _
           "Rânduri [Cai]: " & CStr(lCaiRows)
    If Len(sUnexpected) > 0 Then
        sMsg = sMsg & vbCrLf & vbCrLf & _
               "ATENȚIE: tabele declarate în afara domeniului există totuși în bază și " & _
               "NU au fost exportate: " & sUnexpected
    End If
    MsgBox sMsg, vbInformation, "Migrare FX"
    Exit Sub

Fail:
    lErrNum = Err.Number
    sErrDesc = Err.Description
    sErrSrc = Err.Source
    If Not DB Is Nothing Then
        DB.Close
        Set DB = Nothing
    End If
    ' Raise, never swallow — the caller must see a failed export as a failure.
    Err.Raise lErrNum, sErrSrc, sErrDesc
End Sub


'=============================================================================
' The fixed table list. NO relation discovery and NO prefix matching: discovery is
' exactly what fails today (mdl_FX_ExportMD walks DB.Relations and then recurses).
' Order is parents before children, so the migrator can build its routing maps in the
' same order it reads the files.
'=============================================================================
Private Function SeedTableList() As Variant
    SeedTableList = Array( _
        "FX_Angajamente", "FX_Indicatori", "FX_Istoric", "FX_Salarii", _
        "FX_Rezervari", "FX_Rezervarii_IMG", _
        "FX_Receptii_R", "FX_Receptii_H", "FX_Receptii", "FX_Receptii_RHR", "FX_Receptii_IMG", _
        "FX_Plati", "FX_Receptii_Plati", _
        "FX_Extrase_F", "FX_Extrase_H", "FX_Extrase")
End Function


'=============================================================================
' Memo-heavy tables get a much smaller chunk: FX_Receptii_IMG / FX_Rezervarii_IMG hold
' images, FX_Extrase_F a whole XML document per row, FX_Istoric two long free-text
' fields, FX_Receptii one. 500 of those in a single POST would blow past Flask's
' MAX_CONTENT_LENGTH.
'=============================================================================
Private Function ChunkSizeFor(sTable As String) As Long
    Select Case LCase$(sTable)
        Case "fx_receptii_img", "fx_rezervarii_img", "fx_extrase_f", "fx_istoric", "fx_receptii"
            ChunkSizeFor = CHUNK_ROWS_MEMO
        Case Else
            ChunkSizeFor = CHUNK_ROWS_DEFAULT
    End Select
End Function


'=============================================================================
' Exports one table. Returns the row count; fills colFiles with the chunk file names
' and sColumns with the JSON column list (shared by every chunk of this table).
'
' One forward-only snapshot, SELECT * with no WHERE and no ORDER BY. No recursion, no
' per-row query, no nesting.
'=============================================================================
Private Function ExportTable(DB As DAO.Database, sTable As String, sOutDir As String, _
                             colFiles As Collection, ByRef sColumns As String) As Long
Dim Rs          As DAO.Recordset
Dim stm         As Object
Dim lChunkSize  As Long
Dim lChunkIdx   As Long
Dim lInChunk    As Long
Dim lTotal      As Long
Dim I           As Long
Dim sFile       As String
Dim sHead       As String
Dim lErrNum     As Long
Dim sErrDesc    As String
Dim sErrSrc     As String

    Set Rs = Nothing
    Set stm = Nothing

    On Error GoTo Fail

    Set Rs = DB.OpenRecordset("SELECT * FROM [" & sTable & "]", dbOpenSnapshot, dbForwardOnly)

    ' Column list once, and reject up front any field type we refuse to guess at.
    sColumns = ""
    For I = 0 To Rs.Fields.Count - 1
        If I > 0 Then sColumns = sColumns & ","
        sColumns = sColumns & """" & EscapeJsonSeed(Rs.Fields(I).Name) & """"
        GuardFieldType Rs.Fields(I), sTable
    Next I

    lChunkSize = ChunkSizeFor(sTable)
    lChunkIdx = 0
    lInChunk = 0
    lTotal = 0

    Do While Not Rs.EOF
        If lInChunk = 0 Then
            lChunkIdx = lChunkIdx + 1
            sFile = sTable & "." & Format$(lChunkIdx, "000") & ".json"
            colFiles.Add sFile
            Set stm = NewTextStream()
            sHead = "{""table"":""" & sTable & """,""chunk"":" & CStr(lChunkIdx) & _
                    ",""columns"":[" & sColumns & "],""rows"":["
            stm.WriteText sHead
        End If

        If lInChunk > 0 Then stm.WriteText ","
        ' Straight to the stream. NEVER accumulate the file in a String — VBA string
        ' concatenation is quadratic and FX_Istoric alone is roughly 24.000 rows.
        stm.WriteText RowJson(Rs)

        lInChunk = lInChunk + 1
        lTotal = lTotal + 1
        Rs.MoveNext

        If lInChunk >= lChunkSize Or Rs.EOF Then
            stm.WriteText "]}"
            SaveStreamNoBom stm, sOutDir & "\" & sFile
            stm.Close
            Set stm = Nothing
            lInChunk = 0
        End If
    Loop

    Rs.Close
    Set Rs = Nothing

    ExportTable = lTotal
    Exit Function

Fail:
    lErrNum = Err.Number
    sErrDesc = Err.Description
    sErrSrc = Err.Source
    If Not stm Is Nothing Then
        stm.Close
        Set stm = Nothing
    End If
    If Not Rs Is Nothing Then
        Rs.Close
        Set Rs = Nothing
    End If
    Err.Raise lErrNum, sErrSrc, _
              "Export «" & sTable & "»: " & sErrDesc
End Function


'=============================================================================
' [Cai] from cale.accdb -> Cai.json, one file, same shape as a chunk file so the
' migrator parses it with the same code. IdUnitate is the primary key and the routing
' key: a unit belongs to exactly one DC.
'=============================================================================
Private Function ExportCai(sCaiPath As String, sOutDir As String, _
                           ByRef sColumns As String) As Long
Dim DBc      As DAO.Database
Dim Rs       As DAO.Recordset
Dim stm      As Object
Dim I        As Long
Dim lTotal   As Long
Dim lErrNum  As Long
Dim sErrDesc As String
Dim sErrSrc  As String

    Set DBc = Nothing
    Set Rs = Nothing
    Set stm = Nothing

    On Error GoTo Fail

    If Len(Dir$(sCaiPath)) = 0 Then
        Err.Raise vbObjectError + 3003, "ExportCai", _
                  "Baza cu tabelul [Cai] nu există: " & sCaiPath
    End If

    Set DBc = DBEngine.OpenDatabase(sCaiPath, False, True, ";PWD=" & DB_PASSWORD)
    If Not TableExists(DBc, "Cai") Then
        Err.Raise vbObjectError + 3004, "ExportCai", _
                  "Tabelul [Cai] lipsește din " & sCaiPath & "."
    End If

    Set Rs = DBc.OpenRecordset("SELECT * FROM [Cai]", dbOpenSnapshot, dbForwardOnly)

    sColumns = ""
    For I = 0 To Rs.Fields.Count - 1
        If I > 0 Then sColumns = sColumns & ","
        sColumns = sColumns & """" & EscapeJsonSeed(Rs.Fields(I).Name) & """"
        GuardFieldType Rs.Fields(I), "Cai"
    Next I

    Set stm = NewTextStream()
    stm.WriteText "{""table"":""Cai"",""chunk"":1,""columns"":[" & sColumns & "],""rows"":["

    lTotal = 0
    Do While Not Rs.EOF
        If lTotal > 0 Then stm.WriteText ","
        stm.WriteText RowJson(Rs)
        lTotal = lTotal + 1
        Rs.MoveNext
    Loop

    stm.WriteText "]}"
    SaveStreamNoBom stm, sOutDir & "\Cai.json"
    stm.Close
    Set stm = Nothing

    Rs.Close
    Set Rs = Nothing
    DBc.Close
    Set DBc = Nothing

    ExportCai = lTotal
    Exit Function

Fail:
    lErrNum = Err.Number
    sErrDesc = Err.Description
    sErrSrc = Err.Source
    If Not stm Is Nothing Then
        stm.Close
        Set stm = Nothing
    End If
    If Not Rs Is Nothing Then
        Rs.Close
        Set Rs = Nothing
    End If
    If Not DBc Is Nothing Then
        DBc.Close
        Set DBc = Nothing
    End If
    Err.Raise lErrNum, sErrSrc, sErrDesc
End Function


'=============================================================================
' manifest.json — written LAST, because the row counts are only known once every table
' has been walked. "cai" is a separate entry, not one of "tables": the 16 are what goes
' through the seed routes, [Cai] is routing metadata.
'=============================================================================
Private Sub WriteManifest(sOutDir As String, sFxPath As String, sCaiPath As String, _
                          colManifest As Collection, sCaiColumns As String, _
                          lCaiRows As Long, sUnexpected As String)
Dim stm      As Object
Dim I        As Long
Dim vPart    As Variant
Dim lErrNum  As Long
Dim sErrDesc As String
Dim sErrSrc  As String

    Set stm = Nothing
    On Error GoTo Fail

    Set stm = NewTextStream()
    stm.WriteText "{""exported"":""" & Format$(Now(), "yyyy\-mm\-dd hh\:nn\:ss") & """"
    stm.WriteText ",""source"":""" & EscapeJsonSeed(sFxPath) & """"
    stm.WriteText ",""cai_source"":""" & EscapeJsonSeed(sCaiPath) & """"
    stm.WriteText ",""cai"":{""file"":""Cai.json"",""columns"":[" & sCaiColumns & _
                  "],""rows"":" & CStr(lCaiRows) & "}"

    ' Reported, never exported. An empty array means both are absent, as expected.
    stm.WriteText ",""unexpected_tables"":["
    If Len(sUnexpected) > 0 Then
        vPart = Split(sUnexpected, ",")
        For I = LBound(vPart) To UBound(vPart)
            If I > LBound(vPart) Then stm.WriteText ","
            stm.WriteText """" & EscapeJsonSeed(CStr(vPart(I))) & """"
        Next I
    End If
    stm.WriteText "]"

    stm.WriteText ",""tables"":["
    For I = 1 To colManifest.Count
        If I > 1 Then stm.WriteText ","
        stm.WriteText CStr(colManifest.Item(I))
    Next I
    stm.WriteText "]}"

    SaveStreamNoBom stm, sOutDir & "\manifest.json"
    stm.Close
    Set stm = Nothing
    Exit Sub

Fail:
    lErrNum = Err.Number
    sErrDesc = Err.Description
    sErrSrc = Err.Source
    If Not stm Is Nothing Then
        stm.Close
        Set stm = Nothing
    End If
    Err.Raise lErrNum, sErrSrc, sErrDesc
End Sub


'=============================================================================
' One "tables" element of the manifest. Small enough to build as a String.
'=============================================================================
Private Function ManifestEntry(sTable As String, sColumns As String, _
                               lRows As Long, colFiles As Collection) As String
Dim S As String
Dim I As Long

    S = "{""table"":""" & sTable & """,""columns"":[" & sColumns & _
        "],""rows"":" & CStr(lRows) & ",""files"":["
    For I = 1 To colFiles.Count
        If I > 1 Then S = S & ","
        S = S & """" & EscapeJsonSeed(CStr(colFiles.Item(I))) & """"
    Next I
    ManifestEntry = S & "]}"
End Function


'=============================================================================
' One row as a positional JSON array. Column order is the recordset's, identical in
' every chunk of a table and identical to manifest.columns.
'=============================================================================
Private Function RowJson(Rs As DAO.Recordset) As String
Dim S As String
Dim I As Long

    S = "["
    For I = 0 To Rs.Fields.Count - 1
        If I > 0 Then S = S & ","
        S = S & JsonValue(Rs.Fields(I))
    Next I
    RowJson = S & "]"
End Function


'=============================================================================
' THE PART THAT MATTERS MOST — value conversion.
'
'   Null       -> null          (never "", no Nz anywhere in this module)
'   Boolean    -> 0 / 1         (Access True is -1; -1 must not reach the wire)
'   Date       -> "yyyy-mm-dd hh:nn:ss"   (never #...#, never locale-formatted)
'   numeric    -> unquoted, '.' decimal   (Str$ is locale-independent; CStr on a
'                                          Romanian machine emits a comma and would
'                                          corrupt every amount)
'   Text/Memo  -> quoted, escaped, FULL VALUE — no truncation, ever
'   empty text -> ""            (an empty string is not a NULL; the difference is kept)
'=============================================================================
Private Function JsonValue(Fld As DAO.Field) As String
Dim v As Variant

    v = Fld.Value

    If IsNull(v) Then
        JsonValue = "null"
        Exit Function
    End If

    Select Case Fld.Type

        Case dbBoolean
            If CBool(v) Then
                JsonValue = "1"
            Else
                JsonValue = "0"
            End If

        Case dbDate, dbTime, dbTimeStamp
            ' The separators are escaped: in a VBA format string ":" is the LOCALE time
            ' separator placeholder, not a literal.
            JsonValue = """" & Format$(v, "yyyy\-mm\-dd hh\:nn\:ss") & """"

        Case dbByte, dbInteger, dbLong, dbBigInt, dbSingle, dbDouble, _
             dbCurrency, dbDecimal, dbNumeric, dbFloat
            JsonValue = JsonNumber(v)

        Case Else   ' dbText, dbMemo, dbChar
            JsonValue = """" & EscapeJsonSeed(CStr(v)) & """"

    End Select
End Function


'=============================================================================
' Str$ is the only locale-independent numeric formatter in VBA: it always emits a
' period. It has one quirk that would produce INVALID JSON — for values below 1 it
' drops the leading zero, so Str$(0.1) is " .1" and Str$(-0.1) is "-.1". Both are
' rejected by a strict JSON parser, so the zero is put back.
'=============================================================================
Private Function JsonNumber(v As Variant) As String
Dim S As String

    S = Trim$(Str$(v))
    If Left$(S, 1) = "." Then
        S = "0" & S
    ElseIf Left$(S, 2) = "-." Then
        S = "-0" & Mid$(S, 2)
    End If
    JsonNumber = S
End Function


'=============================================================================
' JSON string escaping. Backslash FIRST — otherwise it would escape the backslashes
' the later replacements just introduced.
'
' The five common cases go through Replace (native, linear). Anything else below 0x20
' is illegal raw inside a JSON string and becomes \u00XX; those are scanned for
' separately because they are vanishingly rare and Replace is not free on a megabyte of
' FX_Extrase_F.XML.
'
' Diacritics are NOT escaped: the file is real UTF-8 (see SaveStreamNoBom), so ă â î ș ț
' travel as themselves and never as \uXXXX.
'=============================================================================
Private Function EscapeJsonSeed(sIn As String) As String
Dim S As String
Dim I As Long
Dim C As String

    S = sIn
    S = Replace(S, "\", "\\")
    S = Replace(S, """", "\""")
    S = Replace(S, vbCr, "\r")
    S = Replace(S, vbLf, "\n")
    S = Replace(S, vbTab, "\t")

    For I = 0 To 31
        ' 9/10/13 are the tab/LF/CR already handled above.
        If I <> 9 And I <> 10 And I <> 13 Then
            C = Chr$(I)
            If InStr(1, S, C, vbBinaryCompare) > 0 Then
                ' Hex$ of a value under 32 is one or two chars, and for 10..15 it is a
                ' LETTER — so it is padded by hand. Format$(Hex$(i), "0000") would not
                ' do it: a numeric format applied to "B" returns "B".
                S = Replace(S, C, "\u00" & Right$("0" & Hex$(I), 2))
            End If
        End If
    Next I

    EscapeJsonSeed = S
End Function


'=============================================================================
' Field types we refuse to guess at. None of the 16 tables (nor [Cai]) is expected to
' have one; if that changes, the export STOPS naming the column instead of writing
' something silently wrong. Binary columns have no lossless JSON form here.
'=============================================================================
Private Sub GuardFieldType(Fld As DAO.Field, sTable As String)
    Select Case Fld.Type
        Case dbBinary, dbLongBinary, dbVarBinary, dbGUID
            Err.Raise vbObjectError + 3005, "GuardFieldType", _
                      "Coloana «" & sTable & "." & Fld.Name & _
                      "» are un tip binar/GUID, care nu poate fi exportat în JSON " & _
                      "fără pierderi. Exportul s-a oprit."
    End Select
End Sub


'=============================================================================
' A fresh UTF-8 text stream.
'
' Open ... For Output would write ANSI and mangle every diacritic in Descriere,
' NumeUnitate and Explicatii, so everything goes through ADODB.Stream instead.
'=============================================================================
Private Function NewTextStream() As Object
Dim stm As Object

    Set stm = CreateObject("ADODB.Stream")
    stm.Type = STM_TEXT
    stm.Charset = "utf-8"
    stm.Open
    Set NewTextStream = stm
End Function


'=============================================================================
' Saves a UTF-8 text stream WITHOUT the 3-byte BOM.
'
' ADODB.Stream always prefixes utf-8 output with EF BB BF. Plenty of JSON parsers
' choke on it, so the text stream is re-read as binary from position 3 and copied into
' a second, binary stream, which is what actually hits the disk.
'
' Setting .Type on an open stream is only legal at position 0 — hence the rewind before
' the switch, and the seek to 3 after it.
'=============================================================================
Private Sub SaveStreamNoBom(stm As Object, sPath As String)
Dim stmOut As Object

    stm.Position = 0
    stm.Type = STM_BINARY
    stm.Position = 3

    Set stmOut = CreateObject("ADODB.Stream")
    stmOut.Type = STM_BINARY
    stmOut.Open
    stm.CopyTo stmOut
    stmOut.SaveToFile sPath, STM_OVERWRITE
    stmOut.Close
    Set stmOut = Nothing
End Sub


'=============================================================================
' Helpers
'=============================================================================
Private Function TableExists(DB As DAO.Database, sTable As String) As Boolean
Dim tdf As DAO.TableDef

    TableExists = False
    For Each tdf In DB.TableDefs
        If LCase$(tdf.Name) = LCase$(sTable) Then
            TableExists = True
            Exit Function
        End If
    Next tdf
End Function


Private Sub EnsureFolder(sPath As String)
    If Len(Dir$(sPath, vbDirectory)) = 0 Then
        MkDir sPath
    End If
End Sub
