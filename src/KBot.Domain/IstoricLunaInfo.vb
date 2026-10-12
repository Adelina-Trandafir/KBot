''' <summary>
''' The last month of the FOREXE history and what deleting it would remove
''' (<c>GET /api/forexe/istoric/luna-ultima</c> and the answer of <c>.../sterge-luna</c>).
''' <see cref="Luna"/> empty = the history is empty.
''' </summary>
Public NotInheritable Class IstoricLunaInfo
    ''' <summary>«YYYY-MM» as the server sends it; empty = no history.</summary>
    Public Property Luna As String = String.Empty

    ''' <summary>«MM.YYYY», for the operator.</summary>
    Public Property Eticheta As String = String.Empty
    Public Property RanduriIstoric As Integer
    Public Property Angajamente As Integer
    Public Property Rezervari As Integer
    Public Property Receptii As Integer
    Public Property Plati As Integer

    ''' <summary>DDF documents the month has (a revision dated in it, or a reservation a DDF touches).</summary>
    Public Property Ddf As Integer

    ''' <summary>ORD documents dated in the month.</summary>
    Public Property Ord As Integer

    ''' <summary>True when DDF or ORD exist: the month cannot be deleted.</summary>
    Public Property Blocata As Boolean

    ''' <summary>The server's Romanian reason when <see cref="Blocata"/>; empty otherwise.</summary>
    Public Property Motiv As String = String.Empty

    ''' <summary>Only on the delete answer: reception headers left without any snapshot, removed too.</summary>
    Public Property ReceptiiRSterse As Integer
End Class
