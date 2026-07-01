' Model de domeniu Angajament. Câmpurile scrise de fluxul ListaAngajamente sunt
' CodAngajament / Descriere / Stare (vezi AngajamentMapper). Id/An rămân pentru
' fluxurile GET (date de referință) care le populează separat.
Public Class Angajament
    Public Property Id As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property An As Integer
    Public Property Descriere As String = String.Empty
    Public Property Stare As String = String.Empty
End Class
