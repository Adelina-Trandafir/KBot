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
