' Model de domeniu Angajament. Câmpurile scrise de fluxul ListaAngajamente sunt
' CodAngajament / Descriere / Stare (vezi AngajamentMapper). Id/An rămân pentru
' fluxurile GET (date de referință) care le populează separat.
'
' Câmpurile de mai jos vin din GET /api/forexe/angajamente (lista MainForm), care
' oglindește Angajamente_SQL: Surse = SS-urile concatenate (Nothing pentru orfani).
Public Class Angajament
    Public Property Id As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property An As Integer
    Public Property Descriere As String = String.Empty
    Public Property Stare As String = String.Empty

    ' --- din GET /api/forexe/angajamente ---
    Public Property IDDF As Integer?
    Public Property Surse As String              ' Nothing pentru angajamentele orfane
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean
    Public Property Salarii As Boolean
    Public Property Ascuns As Boolean
    Public Property DataCreare As Date?
End Class
