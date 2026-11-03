Option Strict On
Imports System.Collections.Generic

' Slice 0093-01 -- the partners as they travel from an Access unit file (table Parteneri) to the K-BOT server
' (PYTHON/routes/forexe/parteneri_access.py). Plain data, no logic. The unit is never a column of the Access table: it is the file's.

''' <summary>A row of the Access table <c>Parteneri</c> (only the columns K-BOT keeps; the Access ids do not travel).</summary>
Public NotInheritable Class AccessPartner
    ''' <summary>Access <c>CodPartener</c>: the key of the partner inside its unit.</summary>
    Public Property CodPartener As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property CodFiscal As String = String.Empty
    Public Property ContIban As String = String.Empty
    Public Property Banca As String = String.Empty
    Public Property Adresa As String = String.Empty
    Public Property Tip As String = String.Empty
    Public Property Ascuns As Boolean
End Class

''' <summary>What the server did with a batch of <see cref="AccessPartner"/> rows (and, summed, with a whole unit or run).</summary>
Public NotInheritable Class PartenerPreluare
    Public Property Primite As Integer
    ''' <summary>Rows that were not in K-BOT and are now.</summary>
    Public Property Adaugate As Integer
    ''' <summary>Rows K-BOT already had (left as they are).</summary>
    Public Property Existente As Integer
    ''' <summary>Rows that could not be taken (no code, code too long, repeated in the batch).</summary>
    Public Property Sarite As Integer
    ''' <summary><c>IdPartener</c> of the rows added.</summary>
    Public Property Ids As New List(Of Integer)()
    ''' <summary>Romanian sentences naming what was skipped and why.</summary>
    Public Property Probleme As New List(Of String)()
End Class
