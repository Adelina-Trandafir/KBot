' POCO-urile vederii Sumar (felia 0011) — echivalentul lui frmFX_MAIN_Sumar.
'
' Sursa: qFX_MAIN_SUMAR. Granulatia este UN RAND PER INDICATOR al angajamentului;
' coloanele de antet (CodAngajament .. Preluat) se repeta identic pe fiecare rand in
' Access, deci serverul le ridica o singura data in "header". De aceea sunt doua
' clase aici, nu una: antetul umple panoul de sus, randurile umplu grila.
'
' Modele pure (fara logica) -> nu poarta Try/Catch (regula casei: POCO-uri simple).

''' <summary>
''' Blocul de antet al sumarului: datele angajamentului insusi, identice pe toate
''' randurile. Null cand angajamentul nu are niciun indicator.
''' </summary>
Public NotInheritable Class SumarHeader
    Public Property CodAngajament As String = String.Empty
    Public Property DataFX As Date?
    Public Property DataCreare As Date?
    Public Property DataDefinitivare As Date?
    Public Property Descriere As String = String.Empty
    Public Property Stare As String = String.Empty
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean
End Class

''' <summary>
''' Un rand de sumar = un indicator al angajamentului, cu cele cinci totaluri.
''' Totalurile vin deja COALESCE-ate la 0 de server (niciodata null), deci sunt
''' Double simplu, nu Double?.
''' </summary>
Public NotInheritable Class SumarRow
    ''' <summary>Codul de clasificatie punctat (ex. «65.02.04.02.20.01.03»).
    ''' Poate fi gol: un indicator fara clasificatie ramane in grila (LEFT JOIN).</summary>
    Public Property Clsf As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property Partener As String = String.Empty
    Public Property TotalRezervari As Double
    Public Property TotalReceptii As Double
    Public Property TotalPlati As Double
    Public Property TotalRevizii As Double
    Public Property TotalOrdonantari As Double
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/sumar: antet (poate lipsi) + randuri.
''' </summary>
Public NotInheritable Class SumarInfo
    ''' <summary>Nothing cand angajamentul nu are indicatori (raspuns 200, nu 404).</summary>
    Public Property Header As SumarHeader
    Public Property Rows As New List(Of SumarRow)()
End Class
