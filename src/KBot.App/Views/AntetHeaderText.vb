Option Strict On
Imports System.Collections.Generic
Imports System.Text

''' <summary>
''' Compune textul multi-linie al benzii de antet pe care o poartă paginile «Vizualizare»
''' (DDF și ORD): perechi «Etichetă: valoare», împachetate câte două pe rând.
'''
''' DE CE EXISTĂ (2026-08-15): antetul reconstruit din XFA de vechiul <c>XfaXmlPreview</c> era un
''' <c>TableLayoutPanel</c> cu două etichete per rând. De când pagina se hrănește din datele
''' serverului, aceleași perechi trebuie să încapă într-o singură <c>Label</c> — și în amândouă
''' vederile la fel, ca operatorul să citească același antet indiferent unde e.
'''
''' Regula de conținut e cea din <c>DdfXfaParser</c>: o pereche cu valoarea goală NU se scrie
''' deloc (nu apare «Cod fiscal:» urmat de nimic), iar împachetarea se face peste ce a rămas —
''' deci un antet incomplet se strânge, nu lasă goluri.
'''
''' Clasă pură (fără WinForms, fără I/O) -&gt; testabilă fără STA.
''' </summary>
Public NotInheritable Class AntetHeaderText

    Private Sub New()
    End Sub

    ''' <summary>Separatorul dintre cele două perechi de pe același rând.</summary>
    Private Const SEPARATOR As String = "     |     "

    ''' <summary>
    ''' Textul benzii. <paramref name="perechi"/> = etichetele și valorile, în ordinea de
    ''' afișare; cele cu valoare goală se sar. <paramref name="perLinie"/> = câte perechi intră
    ''' pe un rând (2 = tiparul vechii tabele de antet).
    ''' </summary>
    Public Shared Function Build(perechi As IEnumerable(Of KeyValuePair(Of String, String)),
                                 Optional perLinie As Integer = 2) As String
        If perechi Is Nothing Then Return String.Empty
        If perLinie < 1 Then perLinie = 1

        Dim celule As New List(Of String)()
        For Each p As KeyValuePair(Of String, String) In perechi
            If String.IsNullOrWhiteSpace(p.Value) Then Continue For
            celule.Add($"{p.Key}: {p.Value.Trim()}")
        Next
        If celule.Count = 0 Then Return String.Empty

        Dim sb As New StringBuilder()
        For i As Integer = 0 To celule.Count - 1
            If i > 0 Then
                sb.Append(If(i Mod perLinie = 0, Environment.NewLine, SEPARATOR))
            End If
            sb.Append(celule(i))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Adaugă un paragraf sub banda de perechi (descrierea documentului). Gol -&gt; nu adaugă
    ''' rândul, ca banda să nu crească cu o linie albă.
    ''' </summary>
    Public Shared Function WithParagraph(antet As String, paragraf As String) As String
        Dim p As String = If(paragraf, String.Empty).Trim()
        If p.Length = 0 Then Return If(antet, String.Empty)
        If String.IsNullOrEmpty(antet) Then Return p
        Return antet & Environment.NewLine & p
    End Function

    ''' <summary>O pereche etichetă/valoare, scrisă scurt la locul apelului.</summary>
    Public Shared Function Pair(eticheta As String, valoare As String) As KeyValuePair(Of String, String)
        Return New KeyValuePair(Of String, String)(eticheta, If(valoare, String.Empty))
    End Function

End Class
