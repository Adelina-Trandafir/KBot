Option Strict On

''' <summary>The kinds of fact K-BOT can see happen to a reception in the FOREXE page.</summary>
Public Enum FelObservatieReceptie
    ''' <summary>The date was changed and saved (<see cref="ObservatieReceptie.DataVeche"/> -> <see cref="ObservatieReceptie.DataNoua"/>).</summary>
    DataSchimbata = 0
    ''' <summary>The reception was deleted (slice 0076-02); only <see cref="ObservatieReceptie.DataVeche"/> is filled.</summary>
    Stearsa = 1
End Enum

''' <summary>
''' Slice 0076-01: a fact about a reception that K-BOT SAW happen in the FOREXE page of its own
''' browser, kept on disk until the server has acknowledged it. The DATE of a reception was
''' changed (<see cref="DataVeche"/> -> <see cref="DataNoua"/>), saved and confirmed by the page:
''' the server moves the stored reception to the new date BEFORE the download is ingested, so
''' step 4b finds it instead of creating a second reception. Slice 0076-02: the reception was
''' DELETED (<see cref="Fel"/> = Stearsa): the server marks it deleted at once.
''' </summary>
Public NotInheritable Class ObservatieReceptie

    ''' <summary>What happened. Files written before 0076-02 have no value here: they read as a date change.</summary>
    Public Property Fel As FelObservatieReceptie = FelObservatieReceptie.DataSchimbata

    ''' <summary>Angajament code the reception belongs to.</summary>
    Public Property CodAngajament As String = String.Empty

    ''' <summary>The reception's date when the operator opened it (dd/MM/yyyy, as the page writes it).</summary>
    Public Property DataVeche As String = String.Empty

    ''' <summary>The date the operator saved (dd/MM/yyyy).</summary>
    Public Property DataNoua As String = String.Empty

    ''' <summary>The row's sum as the page showed it when the reception was opened (text, as read).</summary>
    Public Property SumaVeche As String = String.Empty

    ''' <summary>Rank of the row among the rows with the OLD date, in list order (1-based); 0 = unknown.</summary>
    Public Property Rang As Integer

    ''' <summary>Position of the row in the whole list (1-based); 0 = unknown. Kept for the log.</summary>
    Public Property Pozitie As Integer

    ''' <summary>When the page confirmed the save (local clock).</summary>
    Public Property Moment As DateTime = DateTime.Now

End Class
