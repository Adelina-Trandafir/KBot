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
    Public Property nrcrt_ind As Integer?
    Public Property valoare As Double
    Public Property dif As Double
End Class

Public NotInheritable Class GetReceptiePlata
    Public Property data_plata As Date?
    Public Property suma As Double
End Class
