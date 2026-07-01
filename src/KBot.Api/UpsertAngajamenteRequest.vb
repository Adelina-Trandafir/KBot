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
