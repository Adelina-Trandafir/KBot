Option Strict On
Imports System.Collections.Generic
Imports System.Xml
Imports KBot.Common

''' <summary>
''' Slice 0078-05: brings the DDF's embedded <c>NOTAFD.xml</c> in line with section B of the form
''' data, after section B was written into an already-signed document.
'''
''' The attachment is made by the form's own «Valideaza» from the form data. On a new angajament
''' the first validation runs on the PLACEHOLDER section B (cod «___________», indicator «___»), and
''' after the real B is written nobody validates again -- so the attachment kept the placeholders.
''' This does what «Valideaza» would do for section B: each <c>rowT_ang_ctrl_ang</c> takes the values
''' of the matching filled <c>Table3/Row1</c> of the form, in order. Only attributes whose cell has a
''' value are written (a total Adobe has not computed yet keeps the attachment's value).
'''
''' Pure XML, no PDF: the caller reads and writes the attachment.
''' </summary>
Public NotInheritable Class DdfNotafdSync

    Private Sub New()
    End Sub

    ''' <summary>The attachment's file name inside the DDF.</summary>
    Public Const AttachmentName As String = "NOTAFD.xml"

    ' NOTAFD attribute of a section B row -> form cell of Table3/Row1 (same order as the form's
    ' columns: previous / influence / current, for the commitment credit then the budget credit).
    Private Shared ReadOnly RowMap As String(,) = {
        {"cod_angajament", "Cell1"},
        {"indicator_angajament", "Cell2"},
        {"program", "Cell3"},
        {"cod_SSI", "Cell4"},
        {"sum_rezv_crdt_ang_af_rvz_prc", "Cell5"},
        {"influente_c6", "Cell6"},
        {"sum_rezv_crdt_ang_act", "Cell7"},
        {"sum_rezv_crdt_bug_af_rvz_prc", "Cell8"},
        {"influente_c9", "Cell9"},
        {"sum_rezv_crdt_bug_act", "Cell10"}}

    ''' <summary>
    ''' The attachment text with section B taken from <paramref name="formData"/> (the XFA datasets
    ''' node, or any node holding <c>SubformSectiuneaB</c>). Returns Nothing when nothing changes, and
    ''' puts in <paramref name="note"/> one Romanian line saying what was done or why not. Never
    ''' changes a document whose row counts disagree (the pairing would be a guess).
    ''' Risky (XML): logs and rethrows.
    ''' </summary>
    Public Shared Function SyncSectionB(formData As XmlNode, notafdXml As String, ByRef note As String) As String
        Try
            note = ""
            If formData Is Nothing OrElse String.IsNullOrWhiteSpace(notafdXml) Then
                note = "NOTAFD.xml: nimic de sincronizat."
                Return Nothing
            End If
            Dim subB As XmlElement = FindElement(formData, "SubformSectiuneaB")
            If subB Is Nothing Then
                note = "NOTAFD.xml: datele formularului nu au Secțiunea B."
                Return Nothing
            End If
            Dim table As XmlElement = FindElement(subB, "Table3")
            Dim formRows As New List(Of XmlElement)()
            If table IsNot Nothing Then
                For Each n As XmlNode In table.ChildNodes
                    Dim row As XmlElement = TryCast(n, XmlElement)
                    ' The template's empty first row carries no Cell1 value: only filled rows count.
                    If row IsNot Nothing AndAlso row.LocalName = "Row1" AndAlso CellText(row, "Cell1").Length > 0 Then formRows.Add(row)
                Next
            End If

            Dim doc As New XmlDocument() With {.PreserveWhitespace = True}
            doc.LoadXml(notafdXml)
            Dim secB As XmlElement = FindElement(doc.DocumentElement, "sectiuneaB")
            If secB Is Nothing Then
                note = "NOTAFD.xml: atașamentul nu are «sectiuneaB»."
                Return Nothing
            End If
            Dim notaRows As New List(Of XmlElement)()
            For Each n As XmlNode In secB.GetElementsByTagName("*")
                Dim e As XmlElement = TryCast(n, XmlElement)
                If e IsNot Nothing AndAlso e.LocalName = "rowT_ang_ctrl_ang" Then notaRows.Add(e)
            Next
            If notaRows.Count <> formRows.Count Then
                note = $"NOTAFD.xml NESCHIMBAT: {notaRows.Count} rând(uri) în atașament, {formRows.Count} în Secțiunea B a formularului."
                Return Nothing
            End If

            Dim changed As Integer = 0
            Dim check As String = CellText(subB, "CheckBox9")
            If check.Length > 0 AndAlso secB.GetAttribute("ckbx_secta_inreg_ctrl_ang") <> check Then
                secB.SetAttribute("ckbx_secta_inreg_ctrl_ang", check)
                changed += 1
            End If
            For i As Integer = 0 To notaRows.Count - 1
                For k As Integer = 0 To RowMap.GetLength(0) - 1
                    Dim value As String = CellText(formRows(i), RowMap(k, 1))
                    If value.Length = 0 OrElse notaRows(i).GetAttribute(RowMap(k, 0)) = value Then Continue For
                    notaRows(i).SetAttribute(RowMap(k, 0), value)
                    changed += 1
                Next
            Next
            If changed = 0 Then
                note = "NOTAFD.xml: deja la zi cu Secțiunea B."
                Return Nothing
            End If
            note = $"NOTAFD.xml actualizat din Secțiunea B: {notaRows.Count} rând(uri), {changed} valori schimbate."
            Return doc.OuterXml
        Catch ex As Exception
            GlobalErrorLog.Write("DdfNotafdSync.SyncSectionB", ex)
            Throw
        End Try
    End Function

    ' First descendant (or self) with that local name, whatever the namespace.
    Private Shared Function FindElement(root As XmlNode, localName As String) As XmlElement
        If root Is Nothing Then Return Nothing
        Dim self As XmlElement = TryCast(root, XmlElement)
        If self IsNot Nothing AndAlso self.LocalName = localName Then Return self
        For Each child As XmlNode In root.ChildNodes
            Dim found As XmlElement = FindElement(child, localName)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    ' Trimmed text of a direct child element, "" when absent.
    Private Shared Function CellText(parent As XmlElement, localName As String) As String
        For Each n As XmlNode In parent.ChildNodes
            If n.NodeType = XmlNodeType.Element AndAlso n.LocalName = localName Then Return n.InnerText.Trim()
        Next
        Return ""
    End Function

End Class
