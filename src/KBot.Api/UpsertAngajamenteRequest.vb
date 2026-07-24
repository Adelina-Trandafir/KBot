Option Strict On
Imports System.Collections.Generic

' DTO-uri de wire pentru POST /api/forexe/angajamente/upsert.
' Numele proprietăților SUNT contractul JSON (JsonSerializer cu
' PropertyNamingPolicy=Nothing serializează as-is): db_name / rows / Cod /
' Descriere / Stare — trebuie să corespundă exact cheilor citite de ruta Python.
Public NotInheritable Class UpsertAngajamenteRequest
    Public Property db_name As String
    Public Property rows As New List(Of AngajamentRow)()
End Class

Public NotInheritable Class AngajamentRow
    Public Property Cod As String
    Public Property Descriere As String
    Public Property Stare As String
End Class

' Wire DTO for GET /api/forexe/angajamente (list view). Property names match the
' JSON keys the route returns (db_name / count / rows).
Public NotInheritable Class GetAngajamenteResponse
    Public Property db_name As String
    Public Property count As Integer
    Public Property rows As New List(Of GetAngajamenteRow)()
End Class

' Wire DTO for ONE row of GET /api/forexe/angajamente. Kept separate from
' AngajamentRow so the upsert keeps sending ONLY Cod/Descriere/Stare (its wire
' contract) — this DTO adds the read-only list fields. Property names ARE the JSON
' keys (JsonSerializer PropertyNamingPolicy=Nothing): Cod / Descriere / Stare /
' IDDF / Surse / Incarcat / Preluat / Ascuns / DataCreare. Surse is null
' for orphan angajamente.
'
' Salarii was dropped (deprecated; the new system does not use it). The route no
' longer returns the key. Both directions stay safe during a staggered rollout:
' System.Text.Json ignores an unknown key from an older server, and a missing key
' simply leaves the property unset — so client and server need not deploy together.
Public NotInheritable Class GetAngajamenteRow
    Public Property Cod As String
    Public Property Descriere As String
    Public Property Stare As String
    Public Property IDDF As Integer?
    Public Property Surse As String
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean
    Public Property Ascuns As Boolean
    Public Property DataCreare As Date?
End Class

' Wire DTO for GET /api/forexe/tree (MainForm tree, slice 0008). Same envelope as
' the list route (db_name / count / rows).
Public NotInheritable Class GetTreeResponse
    Public Property db_name As String
    Public Property count As Integer
    Public Property rows As New List(Of GetTreeRow)()
End Class

' Wire DTO for ONE row of GET /api/forexe/tree. Property names ARE the JSON keys
' (JsonSerializer PropertyNamingPolicy=Nothing) — they must match routes/forexe/
' tree.py exactly. Unlike the list route this one DOES carry Salarii: it is a real
' qFX_MAIN_TREE_DESCRIERE row-source column (the deprecation applies to the list
' path only). Note AreOrd here vs AreORD on the POCO — the JSON key wins on the
' wire; ApiClient maps it across.
Public NotInheritable Class GetTreeRow
    Public Property CodAngajament As String
    Public Property IDDF As Long?
    Public Property Descriere As String
    Public Property Stare As String
    Public Property DataCreare As Date?
    Public Property DataDefinitivare As Date?
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean
    Public Property Salarii As Boolean
    Public Property Ascuns As Boolean
    Public Property Surse As String
    Public Property AreIndicatori As Boolean
    Public Property AreIstoric As Boolean
    Public Property AreRevizii As Boolean
    Public Property AreRezervari As Boolean
    Public Property AreReceptii As Boolean
    Public Property ArePlati As Boolean
    Public Property AreDDF As Boolean
    Public Property ArePartener As Boolean
    Public Property AreOrd As Boolean
End Class

' Wire DTOs for GET /api/forexe/sumar (vederea Sumar, slice 0011).
' Property names ARE the JSON keys (PropertyNamingPolicy=Nothing), so unlike the
' tree route — whose Python side emits PascalCase — these are snake_case verbatim,
' matching routes/forexe/sumar.py exactly. ApiClient maps them onto the SumarInfo
' POCOs so the snake_case stops at the wire boundary.
Public NotInheritable Class GetSumarResponse
    ' Null când angajamentul nu are indicatori — un caz legitim, nu o eroare.
    Public Property header As GetSumarHeader
    Public Property rows As New List(Of GetSumarRow)()
End Class

Public NotInheritable Class GetSumarHeader
    Public Property cod_angajament As String
    Public Property data_fx As Date?
    Public Property data_creare As Date?
    Public Property data_definitivare As Date?
    Public Property descriere As String
    Public Property stare As String
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
End Class

Public NotInheritable Class GetSumarRow
    Public Property clsf As String
    Public Property cod_indicator As String
    Public Property partener As String
    Public Property total_rezervari As Double
    Public Property total_receptii As Double
    Public Property total_plati As Double
    Public Property total_revizii As Double
    Public Property total_ordonantari As Double
End Class

' Wire DTOs for GET /api/forexe/rezervari (vederea Rezervari, slice 0014).
' Property names ARE the JSON keys (PropertyNamingPolicy=Nothing) — snake_case verbatim,
' matching routes/forexe/rezervari.py exactly. ApiClient maps them onto the RezervareRow
' POCOs so the snake_case stops at the wire boundary. No header block: unlike Sumar,
' Rezervari is a flat list of rows (the client shapes tree + grid from it).
Public NotInheritable Class GetRezervariResponse
    Public Property rows As New List(Of GetRezervareRow)()
End Class

Public NotInheritable Class GetRezervareRow
    Public Property idrz As Integer
    Public Property cod_indicator As String
    Public Property clsf As String
    Public Property denumire As String
    Public Property data_rezervare As Date?
    Public Property r_credit_bug As Double
    Public Property r_initiala As Double
    Public Property r_valoare As Double
    Public Property r_definitiva As Double
    Public Property e_initiala As Boolean
    Public Property e_marire As Boolean
    Public Property e_micsorare As Boolean
    Public Property are_ddf As Boolean
End Class

' Wire DTOs for GET /api/forexe/receptii (vederea Recepții, slice 0015).
' Property names ARE the JSON keys (PropertyNamingPolicy=Nothing) — snake_case verbatim,
' matching routes/forexe/receptii.py exactly. ApiClient maps them onto the Receptii POCOs
' so the snake_case stops at the wire boundary. The envelope carries BOTH arrays
' (receptii + plati) in one response, so the client half is a single call.
Public NotInheritable Class GetReceptiiResponse
    Public Property cod As String
    Public Property receptii As New List(Of GetReceptieRow)()
    Public Property plati As New List(Of GetReceptiePlata)()
End Class

' One raw FX_Receptii line, with its antet (H) and receptie (R) parents. nrcrt_* and idr
' are nullable: NrCrt can be missing on an indicator/header, and idr is null for an antet
' with no lines (LEFT JOIN branch).
Public NotInheritable Class GetReceptieRow
    Public Property idrr As Integer
    Public Property nrcrt_r As Integer?
    Public Property data_r As Date?
    Public Property suma_antet As Double
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
    Public Property idrh As Integer
    Public Property nrcrt_h As Integer?
    Public Property data_h As Date?
    Public Property total As Double
    Public Property difh As Double
    Public Property sters_h As Boolean
    Public Property descriere_h As String
    Public Property idr As Integer?
    Public Property id_clsf As Integer
    Public Property cod_indicator As String
    Public Property clsf As String
    Public Property denumire As String
    Public Property nrcrt_ind As Integer?
    Public Property valoare As Double
    Public Property dif As Double
End Class

Public NotInheritable Class GetReceptiePlata
    Public Property data_plata As Date?
    Public Property suma As Double
End Class

' Wire DTOs for GET /api/forexe/plati (vederea Plăți, slice 0017).
' Property names ARE the JSON keys (PropertyNamingPolicy=Nothing) — snake_case verbatim,
' matching routes/forexe/plati.py exactly. ApiClient maps them onto the Plati POCOs (with the
' flat extras fields folded into a nested ExtrasBancar) so the snake_case stops at the wire.
Public NotInheritable Class GetPlatiResponse
    Public Property cod As String
    Public Property plati As New List(Of GetPlataRow)()
End Class

' One raw FX_Plati record, with its bank statement (FX_Extrase) fields carried FLAT. idfxe is
' nullable: null means the payment has no statement (LEFT JOIN branch) -> Extras stays Nothing.
Public NotInheritable Class GetPlataRow
    Public Property id_plata_fx As Integer
    Public Property id_clsf As Integer
    Public Property cod_ai As String
    Public Property cod_indicator As String
    Public Property nr_op As String
    Public Property data_plata As Date?
    Public Property suma As Double
    Public Property tip As String
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
    Public Property referinta_trezor As String
    Public Property clsf As String
    Public Property denumire As String
    Public Property clsf_plata As String
    Public Property are_ord As Boolean
    Public Property idfxe As Integer?
    Public Property data_banca As Date?
    Public Property data_doc As String
    Public Property nr_doc_extras As String
    Public Property referinta As String
    Public Property platitor_nume As String
    Public Property platitor_cui As String
    Public Property platitor_iban As String
    Public Property suma_debit As Double
    Public Property suma_credit As Double
    Public Property explicatii As String
End Class

' Wire DTOs for GET /api/forexe/ddf (vederea DDF, slice 0020).
' Property names ARE the JSON keys (PropertyNamingPolicy=Nothing) — snake_case verbatim,
' matching routes/forexe/ddf.py exactly. ApiClient maps them onto the DDF POCOs so the
' snake_case stops at the wire. Three arrays, one round trip: the client filters locally.
Public NotInheritable Class GetDdfResponse
    Public Property cod As String
    ' ARRAY, not an object: FX_DDF's PK is composite (IDDF, CUAL) and nothing enforces one
    ' header per CodAngajament. The view picks explicitly and logs when there is more than one.
    Public Property antet As New List(Of GetDdfAntetRow)()
    Public Property revizii As New List(Of GetDdfRevizieRow)()
    Public Property linii As New List(Of GetDdfLinieRow)()
    ' Empty unless the caller passed pentru_generare=1 (slice 05).
    Public Property sectiuneb As New List(Of GetDdfSectiuneBRow)()
    Public Property atasamente As New List(Of GetDdfAtasamentRow)()
End Class

Public NotInheritable Class GetDdfAntetRow
    Public Property iddf As Integer
    Public Property cod_angajament As String
    Public Property cual As Integer
    Public Property obiect_ddf As String
    Public Property comp As String
    Public Property program As String
    Public Property data_creare As Date?
    Public Property data_def As Date?
    Public Property stare As String
    Public Property part_ang As Boolean
    Public Property cod_fiscal As String
    Public Property nume_partener As String
    Public Property salarii As Boolean
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
End Class

' One FX_DDF_REV record. total_revizie is the server-side SUM(ValCur) over section A —
' NOT Access's `SA.ValCur AS TotalRevizie` (a single line wearing a total's name).
Public NotInheritable Class GetDdfRevizieRow
    Public Property idrev As Integer
    Public Property iddf As Integer
    Public Property numar_rev As Integer
    Public Property data_rev As Date?
    Public Property desc_scurta As String
    Public Property desc_lunga As String
    Public Property tip As String
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
    Public Property semnatura As String
    Public Property total_revizie As Double
End Class

' One FX_DDF_REV_SA record. parametrii_fund/ss are carried but not displayed (decision 4) —
' the slice-05 XML builder writes parametrii_fund into Cell4 and ss into Cell3/codSSI.
Public NotInheritable Class GetDdfLinieRow
    Public Property id_sec_a As Integer
    Public Property idrev As Integer
    Public Property id_clsf As Integer
    Public Property clsf As String
    Public Property ss As String
    Public Property element_fund As String
    Public Property parametrii_fund As String
    Public Property val_prec As Double
    Public Property val_cur As Double
    Public Property val_tot As Double
End Class

' One FX_DDF_REV_SB record (section B) — generation-only. Cell7 is deliberately absent (the
' XML builder skips it). Values are doubles; the builder Int()-truncates them.
Public NotInheritable Class GetDdfSectiuneBRow
    Public Property id_sec_b As Integer
    Public Property idrev As Integer
    Public Property cod_angajament As String
    Public Property cod_indicator As String
    Public Property cod_ssi As String
    Public Property ca_anterior As Double
    Public Property inf1 As Double
    Public Property cb_anterior As Double
    Public Property inf2 As Double
End Class

' One FX_DDF_REV_ATT record — generation-only. date_fisier is already base64.
Public NotInheritable Class GetDdfAtasamentRow
    Public Property id_rev_att As Integer
    Public Property idrev As Integer
    Public Property cale_fisier As String
    Public Property prt_scr As Boolean
    Public Property date_fisier As String
End Class

' Wire DTOs for GET /api/forexe/istoric (vederea Istoric, slice 0022).
' Property names ARE the JSON keys (PropertyNamingPolicy=Nothing) — snake_case verbatim,
' matching routes/forexe/istoric.py exactly. ApiClient maps them onto the Istoric POCOs so the
' snake_case stops at the wire. Two arrays, one round trip: the client filters locally.
Public NotInheritable Class GetIstoricResponse
    Public Property cod As String
    Public Property randuri As New List(Of GetIstoricRandRow)()
    Public Property clasificatii As New List(Of GetIstoricClasificatieRow)()
End Class

' One raw FX_Istoric record. data_fx carries a TIME component (§2.3) — it deserializes to a
' Date (DateTime) with the hours intact, NOT truncated to the day like the DDF revision date.
' idrev is nullable: many istoric rows have no revision.
Public NotInheritable Class GetIstoricRandRow
    Public Property id As Integer
    Public Property data_fx As Date?
    Public Property clsf As String
    Public Property id_clsf As Integer
    Public Property tip_rand As String
    Public Property cod_indicator As String
    Public Property cod_ai As String
    Public Property descriere As String
    Public Property observatii As String
    Public Property val_rezervare_i As Double
    Public Property val_rezervare_d As Double
    Public Property val_rezervare_ant As Double
    Public Property val_rezervare_dif As Double
    Public Property val_ang_leg As Double
    Public Property val_receptie As Double
    Public Property val_plata As Double
    Public Property id_trezor As String
    Public Property doc As String
    Public Property idrev As Integer?
End Class

' One classification-hierarchy entry (deduped on IdClsfAcc). id_clsf is the ACCESS id
' (= Clasificatii.IdClsfAcc), matching FX_Istoric.IdClsf — the opposite direction from DDF.
Public NotInheritable Class GetIstoricClasificatieRow
    Public Property id_clsf As Integer
    Public Property clsf As String
    Public Property capitol As String
    Public Property subcapitol As String
    Public Property articol As String
    Public Property alineat As String
    Public Property den_subcapitol As String
    Public Property den_articol As String
    Public Property den_alineat As String
End Class
