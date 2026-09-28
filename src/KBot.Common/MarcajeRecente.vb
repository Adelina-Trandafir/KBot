Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' The last K-BOT marker handed to the FOREXE page, per (kind, angajament)
''' (operator, 28.09.2026).
'''
''' <para><b>Why it is kept.</b> The markers of slice 0076 name the ids a save will become:
''' «(IDREV: n)» for a reservation, «(IDRH: n; IDR: m)» for a reception. The pictures K-BOT
''' takes beside that save have to hang off the same ids once the ingest has written the
''' records, and only the page knew them - they were reserved while the operator's click was
''' held. A reservation's number could be asked for again (the server hands the same one back
''' while the session's lock lives), but a reception's cannot: every reception save reserves a
''' NEW pair, so asking again would invent a snapshot that never existed.</para>
'''
''' <para>Memory only, and deliberately: a number that is stale by the next run is worse than
''' no number, and a picture whose number is gone is uploaded without one and stays on disk to
''' be said out loud, never guessed.</para>
''' </summary>
Public NotInheritable Class MarcajeRecente

    Public Const TipRezervare As String = "rezervare"
    Public Const TipReceptie As String = "receptie"
    Public Const CheieIdrev As String = "IDREV"
    Public Const CheieIdrh As String = "IDRH"
    Public Const CheieIdr As String = "IDR"

    Private ReadOnly _gate As New Object()
    Private ReadOnly _texte As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>The marker text as the page received it. An empty text forgets the entry.</summary>
    Public Sub Pune(tip As String, cod As String, marcaj As String)
        SyncLock _gate
            Dim cheie As String = Cheia(tip, cod)
            If String.IsNullOrWhiteSpace(marcaj) Then
                _texte.Remove(cheie)
            Else
                _texte(cheie) = marcaj
            End If
        End SyncLock
    End Sub

    ''' <summary>The marker text, or an empty one when none was handed over yet.</summary>
    Public Function Text(tip As String, cod As String) As String
        SyncLock _gate
            Dim gasit As String = Nothing
            If _texte.TryGetValue(Cheia(tip, cod), gasit) Then Return gasit
            Return String.Empty
        End SyncLock
    End Function

    ''' <summary>
    ''' One number out of the last marker: <paramref name="cheie"/> = "IDREV", "IDRH" or
    ''' "IDR". Zero when there is no marker or it does not carry that id.
    ''' </summary>
    Public Function Numar(tip As String, cod As String, cheie As String) As Integer
        Return NumarDin(Text(tip, cod), cheie)
    End Function

    ''' <summary>«(IDRH: 41; IDR: 88)» + "IDRH" -> 41. Zero when it is not there.</summary>
    Public Shared Function NumarDin(marcaj As String, cheie As String) As Integer
        Try
            If String.IsNullOrWhiteSpace(marcaj) OrElse String.IsNullOrWhiteSpace(cheie) Then Return 0
            Dim tipar As New Regex("\b" & Regex.Escape(cheie.Trim()) & "\s*:\s*(\d+)",
                                   RegexOptions.IgnoreCase)
            Dim m As Match = tipar.Match(marcaj)
            If Not m.Success Then Return 0
            Dim valoare As Integer
            If Integer.TryParse(m.Groups(1).Value, NumberStyles.Integer,
                                CultureInfo.InvariantCulture, valoare) Then
                Return valoare
            End If
            Return 0
        Catch ex As Exception
            GlobalErrorLog.Write("MarcajeRecente.NumarDin", ex)
            Return 0
        End Try
    End Function

    ''' <summary>The session is filed: its marker means nothing any more.</summary>
    Public Sub Uita(tip As String, cod As String)
        Pune(tip, cod, String.Empty)
    End Sub

    Private Shared Function Cheia(tip As String, cod As String) As String
        Return If(tip, String.Empty).Trim() & "|" & If(cod, String.Empty).Trim()
    End Function

End Class
