Option Strict On

''' <summary>
''' Modelul unui rând <see cref="KBotDataView"/> (control NELEGAT de date). Ține valorile
''' celulelor într-un dicționar cheiat pe <c>Column.Key</c> — o cheie lipsă înseamnă
''' <c>Nothing</c>. Controlul deține colecția de rânduri; caller-ul îl umple prin
''' <c>AddRow</c> + setarea celulelor.
''' </summary>
Public NotInheritable Class KBotDataRow

    ' Store-ul de celule: cheie coloană -> valoare. Cheie absentă => Nothing.
    Private ReadOnly _cells As New Dictionary(Of String, Object)(StringComparer.Ordinal)

    ''' <summary>Rând activ. Implicit True. False => întregul rând e șters (gri) și inert.</summary>
    Public Property Enabled As Boolean = True

    ''' <summary>Payload al caller-ului (ex. DTO-ul din spatele rândului). Nefolosit de control.</summary>
    Public Property Tag As Object

    ''' <summary>
    ''' True dacă rândul a fost EDITAT DE OPERATOR (commit de editare, comutare de bifă/opțiune)
    ''' de la ultima curățare. ATENȚIE: scrierea programatică — <c>row(cheie) = valoare</c> sau
    ''' <c>KBotDataView.Item</c> — NU ridică steagul: aceea e ÎNCĂRCARE de date, nu editare.
    ''' Cine vrea totuși să marcheze o schimbare programatică drept editare, setează explicit
    ''' proprietatea. <see cref="MarkClean"/> o coboară (baseline curat după încărcare).
    ''' </summary>
    Public Property IsDirty As Boolean

    ''' <summary>
    ''' Valoarea celulei pentru cheia de coloană dată. GET întoarce <c>Nothing</c> pentru o
    ''' cheie absentă; SET doar stochează valoarea — NU ridică <see cref="IsDirty"/> (vezi
    ''' explicația de acolo) și nu invalidează vizual (o face controlul, prin
    ''' <c>KBotDataView.Item</c>).
    ''' </summary>
    Default Public Property Item(colKey As String) As Object
        Get
            If colKey Is Nothing Then Return Nothing
            Dim v As Object = Nothing
            Return If(_cells.TryGetValue(colKey, v), v, Nothing)
        End Get
        Set(value As Object)
            If String.IsNullOrEmpty(colKey) Then Throw New ArgumentException("Cheie de coloană vidă.", NameOf(colKey))
            _cells(colKey) = value
        End Set
    End Property

    ''' <summary>True dacă rândul are o valoare stocată (chiar și Nothing) pentru cheia dată.</summary>
    Public Function HasValue(colKey As String) As Boolean
        Return colKey IsNot Nothing AndAlso _cells.ContainsKey(colKey)
    End Function

    ''' <summary>Coboară steagul „murdar” (baseline curat după încărcare/commit).</summary>
    Public Sub MarkClean()
        IsDirty = False
    End Sub

End Class
