' POCO for POST /api/forexe/rezervari/reanaliza: what re-walking the FX_Istoric chain of one
' angajament found wrong in its reservations, and what was written. Same shape for the dry
' run (Applied = False, every *Corrected count 0) and the real run.
' Pure model (no I/O) -> no Try/Catch (house rule: simple POCOs).

''' <summary>One reservation whose value changes.</summary>
Public NotInheritable Class RezervareCorectata
    Public Property Data As String = String.Empty
    Public Property Indicator As String = String.Empty
    Public Property OldValue As Double
    Public Property NewValue As Double
    Public Property OldPrevious As Double
    Public Property NewPrevious As Double
End Class

''' <summary>Result of re-analysing the reservations of one angajament.</summary>
Public NotInheritable Class RezervariReanalizaResult
    Public Property Cod As String = String.Empty
    ''' <summary>False = dry run: nothing was written.</summary>
    Public Property Applied As Boolean
    Public Property HistoryRows As Integer
    Public Property HistoryToCorrect As Integer
    Public Property HistoryCorrected As Integer
    Public Property ReservationsToCorrect As Integer
    Public Property ReservationsCorrected As Integer
    Public Property TypeChanged As Integer
    Public Property Details As New List(Of RezervareCorectata)()
    Public Property Warnings As New List(Of String)()

    ''' <summary>True when neither the history nor the reservations differ from the replay.</summary>
    Public ReadOnly Property NothingToDo As Boolean
        Get
            Return HistoryToCorrect = 0 AndAlso ReservationsToCorrect = 0
        End Get
    End Property
End Class
