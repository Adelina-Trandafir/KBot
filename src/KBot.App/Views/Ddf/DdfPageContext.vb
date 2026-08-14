Option Strict On
Imports System.Collections.Generic
Imports KBot.Domain

''' <summary>
''' Tot ce-i trebuie unei sub-pagini DDF ca să redea nodul selectat acum în arbore. E
''' reconstruit de <c>DdfView</c> la fiecare schimbare de nod (și re-împins după o generare, cu
''' <see cref="PdfExists"/> întors pe True). O pagină folosește câmpurile de care are nevoie și
''' le ignoră pe celelalte — un singur tip de context pentru toate patru, ca bucla de găzduire
''' din părinte să rămână uniformă.
'''
''' POCO -&gt; fără Try/Catch.
''' </summary>
Public NotInheritable Class DdfPageContext

    ''' <summary>Antetul CodAngajament-ului curent (CUAL / PartAng / numele partenerului).</summary>
    Public ReadOnly Property Antet As DdfAntet

    ''' <summary>Liniile de secțiune A ale nodului selectat (deja filtrate pe nod).</summary>
    Public ReadOnly Property Linii As List(Of LinieSaRow)

    ''' <summary>
    ''' Toate reviziile CodAngajament-ului — grila de valori are nevoie de ele ca să afle data
    ''' reviziei fiecărui rând pe o rădăcină de lună. Celelalte pagini le ignoră.
    ''' </summary>
    Public ReadOnly Property Revizii As List(Of RevizieRow)

    ''' <summary>Nodul e o rădăcină de lună? (Atunci grila e o listă plată peste mai multe revizii.)</summary>
    Public ReadOnly Property IsRoot As Boolean

    ''' <summary>Revizia frunzei selectate; <c>Nothing</c> pe o rădăcină de lună.</summary>
    Public ReadOnly Property Revizie As RevizieRow

    ''' <summary>CodAngajament (browserul de fișiere enumeră după el).</summary>
    Public ReadOnly Property Cod As String

    ''' <summary>Calea PDF așteptată pentru frunza curentă (calculată de părinte prin DdfPdfLocator).</summary>
    Public ReadOnly Property PdfPath As String

    ''' <summary>Există acel PDF pe disc chiar acum?</summary>
    Public ReadOnly Property PdfExists As Boolean

    Public Sub New(antet As DdfAntet, linii As List(Of LinieSaRow), revizii As List(Of RevizieRow),
                   isRoot As Boolean, revizie As RevizieRow, cod As String,
                   pdfPath As String, pdfExists As Boolean)
        Me.Antet = antet
        Me.Linii = If(linii, New List(Of LinieSaRow)())
        Me.Revizii = If(revizii, New List(Of RevizieRow)())
        Me.IsRoot = isRoot
        Me.Revizie = revizie
        Me.Cod = cod
        Me.PdfPath = pdfPath
        Me.PdfExists = pdfExists
    End Sub

End Class
