Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq

''' <summary>
''' Modelul redabil al unui document DDF, extras din XML-ul XFA (felia 0020-03). Acoperă
''' EXACT setul de elemente pe care le scrie constructorul din mdl_FX_DDF_PDF
''' (GenereazaXML_PentruPython) — pe care felia 05 îl portează — deci NU este un randor XFA
''' general: știm forma fiindcă noi o producem.
'''
''' Structura sursă (arborele «form1», fie brut, fie sub «xdp:xdp/xfa:data»):
'''   form1
'''     SubformAntet           -> DenInstPb, cif, NrUnicInreg, SubtitluDF, DataRevizuirii, Revizuirea
'''     SubformSectiuneaA
'''       Subform123           -> ComparimentSpecialitate, DescrieObFundRevizuireScurt/Lung
'''       Subform4/Table1/Row1 -> primul Row1 e FICTIV (index 0, sărit); apoi câte un Row1 cu
'''                               Cell1=ElementFund, Cell3=SS+clsf, Cell5=ValPrec, Cell6=ValCur
''' Modelul e pur (fără I/O, fără WinForms) -> se poate testa fără STA.
''' </summary>
Public NotInheritable Class DdfXfaModel

    ''' <summary>Perechile de antet, în ordinea de afișare (etichetă românească, valoare).</summary>
    Public ReadOnly Property AntetFields As New List(Of KeyValuePair(Of String, String))()
    Public Property Compartiment As String = String.Empty
    Public Property DescScurt As String = String.Empty
    Public Property DescLung As String = String.Empty
    ''' <summary>Liniile de secțiune A (fără rândul fictiv).</summary>
    Public ReadOnly Property Linii As New List(Of DdfXfaLinie)()

    ''' <summary>Modelul e gol (nicio pereche de antet și nicio linie)?</summary>
    Public ReadOnly Property EsteGol As Boolean
        Get
            Return AntetFields.Count = 0 AndAlso Linii.Count = 0
        End Get
    End Property

End Class

''' <summary>O linie de secțiune A, așa cum apare în XFA. POCO -> fără Try/Catch.</summary>
Public NotInheritable Class DdfXfaLinie
    Public Property ElementFund As String = String.Empty
    ''' <summary>Codul din Cell3 (SS + capitol + clasificația fără puncte).</summary>
    Public Property Clsf As String = String.Empty
    Public Property ValPrec As String = String.Empty
    Public Property ValCur As String = String.Empty
End Class

''' <summary>
''' Parsează arborele XFA «form1» într-un <see cref="DdfXfaModel"/>. Fără spații de nume:
''' caută pe LOCAL-NAME, ca să meargă și pe XML-ul brut (form1 la rădăcină) și pe cel
''' embedat în PDF (înfășurat în xdp:xdp/xfa:data). NU aruncă pe XML nevalid — întoarce
''' Nothing, iar apelantul își arată starea „nu pot citi documentul".
''' </summary>
Public NotInheritable Class DdfXfaParser

    Private Sub New()
    End Sub

    ' Eticheta românească pentru fiecare câmp de antet cunoscut, în ordinea de afișare.
    Private Shared ReadOnly _antetLabels As (Tag As String, Label As String)() = {
        ("DenInstPb", "Instituția publică"),
        ("cif", "Cod fiscal"),
        ("NrUnicInreg", "Nr. unic"),
        ("SubtitluDF", "Obiectul documentului"),
        ("DataRevizuirii", "Data revizuirii"),
        ("Revizuirea", "Revizuirea")
    }

    ''' <summary>
    ''' Parsează <paramref name="xml"/> în model. Întoarce Nothing când XML-ul e gol/nevalid
    ''' sau nu conține un nod «form1». Nu aruncă.
    ''' </summary>
    Public Shared Function Parse(xml As String) As DdfXfaModel
        If String.IsNullOrWhiteSpace(xml) Then Return Nothing

        Dim doc As XDocument
        Try
            doc = XDocument.Parse(xml)
        Catch
            ' XML nevalid — apelantul arată starea de eroare, nu o excepție.
            Return Nothing
        End Try

        Dim form1 As XElement = FirstLocal(doc.Root, "form1")
        If form1 Is Nothing Then Return Nothing

        Dim model As New DdfXfaModel()

        ' --- Antet ---
        Dim antet As XElement = FirstLocal(form1, "SubformAntet")
        If antet IsNot Nothing Then
            For Each pair In _antetLabels
                Dim el As XElement = FirstLocal(antet, pair.Tag)
                If el IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(el.Value) Then
                    model.AntetFields.Add(New KeyValuePair(Of String, String)(pair.Label, el.Value.Trim()))
                End If
            Next
        End If

        ' --- Secțiunea A ---
        Dim sectA As XElement = FirstLocal(form1, "SubformSectiuneaA")
        If sectA IsNot Nothing Then
            Dim sub123 As XElement = FirstLocal(sectA, "Subform123")
            If sub123 IsNot Nothing Then
                model.Compartiment = ValueOf(sub123, "ComparimentSpecialitate")
                model.DescScurt = ValueOf(sub123, "DescrieObFundRevizuireScurt")
                model.DescLung = ValueOf(sub123, "DescrieObFundRevizuireLung")
            End If

            ' Table1 poate sta sub Subform4; îl căutăm oriunde sub secțiunea A ca să fim robuști.
            Dim table1 As XElement = DescendantsLocal(sectA, "Table1").FirstOrDefault()
            If table1 IsNot Nothing Then
                For Each row As XElement In DirectLocal(table1, "Row1")
                    ' Rândul FICTIV (index 0) are doar Cell1 gol — nu are Cell6. Îl sărim, exact
                    ' ca JS-ul din macheta Access.
                    Dim cell6 As XElement = FirstLocal(row, "Cell6")
                    If cell6 Is Nothing Then Continue For

                    model.Linii.Add(New DdfXfaLinie() With {
                        .ElementFund = ValueOf(row, "Cell1"),
                        .Clsf = ValueOf(row, "Cell3"),
                        .ValPrec = ValueOf(row, "Cell5"),
                        .ValCur = cell6.Value.Trim()
                    })
                Next
            End If
        End If

        Return model
    End Function

    ' Primul descendent (la orice adâncime) cu local-name-ul dat. Nothing dacă nu există.
    Private Shared Function FirstLocal(root As XElement, localName As String) As XElement
        If root Is Nothing Then Return Nothing
        If String.Equals(root.Name.LocalName, localName, StringComparison.Ordinal) Then Return root
        Return DescendantsLocal(root, localName).FirstOrDefault()
    End Function

    ' Toți descendenții (la orice adâncime) cu local-name-ul dat.
    Private Shared Function DescendantsLocal(root As XElement, localName As String) As IEnumerable(Of XElement)
        If root Is Nothing Then Return Enumerable.Empty(Of XElement)()
        Return root.Descendants().Where(Function(e) String.Equals(e.Name.LocalName, localName, StringComparison.Ordinal))
    End Function

    ' Copiii DIRECȚI cu local-name-ul dat (nu descendenți — contează pentru Row1 sub Table1).
    Private Shared Function DirectLocal(parent As XElement, localName As String) As IEnumerable(Of XElement)
        If parent Is Nothing Then Return Enumerable.Empty(Of XElement)()
        Return parent.Elements().Where(Function(e) String.Equals(e.Name.LocalName, localName, StringComparison.Ordinal))
    End Function

    Private Shared Function ValueOf(parent As XElement, localName As String) As String
        Dim el As XElement = FirstLocal(parent, localName)
        Return If(el Is Nothing, String.Empty, el.Value.Trim())
    End Function

End Class
