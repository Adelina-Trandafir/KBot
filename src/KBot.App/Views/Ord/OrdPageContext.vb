Option Strict On
Imports System.Collections.Generic
Imports KBot.Domain

''' <summary>
''' Tot ce-i trebuie unei sub-pagini ORD ca să redea nodul selectat acum în arbore. E
''' reconstruit de <c>OrdView</c> la fiecare schimbare de nod. O pagină folosește câmpurile
''' de care are nevoie și le ignoră pe celelalte — un singur tip de context pentru amândouă,
''' ca bucla de găzduire din părinte să rămână uniformă (tiparul lui <c>DdfPageContext</c>).
'''
''' POCO -&gt; fără Try/Catch.
''' </summary>
Public NotInheritable Class OrdPageContext

    ''' <summary>
    ''' Liniile ordonanțării selectate (deja filtrate pe nod). Pe o rădăcină de lună sunt
    ''' liniile TUTUROR ordonanțărilor lunii. Niciodată <c>Nothing</c>.
    ''' </summary>
    Public ReadOnly Property Linii As List(Of OrdLinieRow)

    ''' <summary>Nodul e o rădăcină de lună? (Atunci nu există UN singur document, deci nici cale PDF.)</summary>
    Public ReadOnly Property IsRoot As Boolean

    ''' <summary>Numărul ordonanțării selectate; 0 pe o rădăcină de lună.</summary>
    Public ReadOnly Property NrOrd As Integer

    ''' <summary>Data ordonanțării selectate; <c>Nothing</c> pe o rădăcină de lună.</summary>
    Public ReadOnly Property DataOrd As Date?

    ''' <summary>CodAngajament-ul curent.</summary>
    Public ReadOnly Property Cod As String

    ''' <summary>
    ''' Calea PDF așteptată pentru ordonanțarea curentă, calculată de părinte prin
    ''' <c>OrdPdfLocator</c>. Goală pe o rădăcină de lună.
    ''' </summary>
    Public ReadOnly Property PdfPath As String

    ''' <summary>Există acel PDF pe discul CLIENTULUI chiar acum?</summary>
    Public ReadOnly Property PdfExists As Boolean

    Public Sub New(linii As List(Of OrdLinieRow), isRoot As Boolean, nrOrd As Integer,
                   dataOrd As Date?, cod As String, pdfPath As String, pdfExists As Boolean)
        Me.Linii = If(linii, New List(Of OrdLinieRow)())
        Me.IsRoot = isRoot
        Me.NrOrd = nrOrd
        Me.DataOrd = dataOrd
        Me.Cod = cod
        Me.PdfPath = pdfPath
        Me.PdfExists = pdfExists
    End Sub

End Class
