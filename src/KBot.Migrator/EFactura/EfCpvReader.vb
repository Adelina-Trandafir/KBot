Imports System.IO
Imports System.IO.Compression
Imports System.Xml
Imports System.Xml.Linq

''' <summary>
''' Slice 00EF-27. Reads the CPV codes of the lines of ONE received invoice from what Access kept in <c>EF.XML</c>: the ANAF zip (the invoice XML
''' plus the signature file), or, for a row that holds the bare XML, the XML itself. The rule is the one of the server's
''' <c>primite_ubl._cpv</c>: <c>Item/CommodityClassification/ItemClassificationCode</c> whose <c>listID</c> is «STI». Change both or none.
''' </summary>
Friend NotInheritable Class EfCpvReader

    Private Const MaxXmlBytes As Long = 25L * 1024L * 1024L
    Private Const MaxCpvLength As Integer = 16
    Private Const MaxLineNumberLength As Integer = 16

    Private Sub New()
    End Sub

    ''' <summary>One line of the invoice: its number as the server stores it (<c>NrLinie</c>) and its CPV («» when it has none).</summary>
    Friend NotInheritable Class LineCpv
        Public Property NrLinie As String = String.Empty
        Public Property Cpv As String = String.Empty
    End Class

    ''' <summary>
    ''' The lines of the invoice in document order, or <c>Nothing</c> when the bytes hold no readable invoice (no XML in the zip, not
    ''' XML, not a UBL Invoice / CreditNote). That is a fact about the data, reported by the caller, not an exception.
    ''' </summary>
    Public Shared Function Read(k_bytes As Byte()) As List(Of LineCpv)
        If k_bytes Is Nothing OrElse k_bytes.Length = 0 Then Return Nothing
        Dim k_xml As Byte() = XmlOf(k_bytes)
        If k_xml Is Nothing OrElse k_xml.LongLength > MaxXmlBytes Then Return Nothing
        Dim k_doc As XDocument
        Try
            Dim k_settings As New XmlReaderSettings() With {.DtdProcessing = DtdProcessing.Prohibit, .XmlResolver = Nothing}
            Using k_stream As New MemoryStream(k_xml)
                Using k_reader As XmlReader = XmlReader.Create(k_stream, k_settings)
                    k_doc = XDocument.Load(k_reader)
                End Using
            End Using
        Catch ex As XmlException
            Return Nothing
        End Try
        Dim k_root As XElement = k_doc.Root
        If k_root Is Nothing Then Return Nothing
        Dim k_credit As Boolean
        Select Case k_root.Name.LocalName
            Case "Invoice"
                k_credit = False
            Case "CreditNote"
                k_credit = True
            Case Else
                Return Nothing
        End Select

        Dim k_result As New List(Of LineCpv)()
        Dim k_index As Integer = 0
        For Each k_line As XElement In Children(k_root, If(k_credit, "CreditNoteLine", "InvoiceLine"))
            k_index += 1
            Dim k_number As String = Clip(TextOf(Children(k_line, "ID").FirstOrDefault()), MaxLineNumberLength)
            If k_number.Length = 0 Then k_number = k_index.ToString(Globalization.CultureInfo.InvariantCulture)
            k_result.Add(New LineCpv() With {.NrLinie = k_number, .Cpv = CpvOf(k_line)})
        Next
        Return k_result
    End Function

    Private Shared Function CpvOf(k_line As XElement) As String
        For Each k_item As XElement In Children(k_line, "Item")
            For Each k_class As XElement In Children(k_item, "CommodityClassification")
                For Each k_code As XElement In Children(k_class, "ItemClassificationCode")
                    Dim k_list As String = If(CType(k_code.Attribute("listID"), String), String.Empty).Trim()
                    Dim k_text As String = TextOf(k_code)
                    If String.Equals(k_list, "STI", StringComparison.OrdinalIgnoreCase) AndAlso k_text.Length > 0 Then
                        Return Clip(k_text, MaxCpvLength)
                    End If
                Next
            Next
        Next
        Return String.Empty
    End Function

    ''' <summary>The invoice XML: the bytes themselves, or the first non-signature <c>.xml</c> entry of the zip.</summary>
    Private Shared Function XmlOf(k_bytes As Byte()) As Byte()
        If Not (k_bytes.Length > 3 AndAlso k_bytes(0) = &H50 AndAlso k_bytes(1) = &H4B AndAlso k_bytes(2) = 3 AndAlso k_bytes(3) = 4) Then
            Return k_bytes
        End If
        Try
            Using k_stream As New MemoryStream(k_bytes)
                Using k_zip As New ZipArchive(k_stream, ZipArchiveMode.Read)
                    For Each k_entry As ZipArchiveEntry In k_zip.Entries
                        Dim k_name As String = k_entry.Name.ToLowerInvariant()
                        If k_name.EndsWith(".xml", StringComparison.Ordinal) AndAlso Not k_name.StartsWith("semnatura_", StringComparison.Ordinal) Then
                            If k_entry.Length > MaxXmlBytes Then Return Nothing
                            Using k_content As New MemoryStream()
                                Using k_source As Stream = k_entry.Open()
                                    k_source.CopyTo(k_content)
                                End Using
                                Return k_content.ToArray()
                            End Using
                        End If
                    Next
                End Using
            End Using
        Catch ex As InvalidDataException
            ' A damaged archive is a fact about the data: the caller counts it as an unreadable invoice.
            Return Nothing
        End Try
        Return Nothing
    End Function

    Private Shared Iterator Function Children(k_parent As XElement, k_localName As String) As IEnumerable(Of XElement)
        If k_parent Is Nothing Then Return
        For Each k_child As XElement In k_parent.Elements()
            If String.Equals(k_child.Name.LocalName, k_localName, StringComparison.Ordinal) Then Yield k_child
        Next
    End Function

    ''' <summary>The text of an element with white space collapsed (the server's <c>_text</c>).</summary>
    Private Shared Function TextOf(k_element As XElement) As String
        If k_element Is Nothing Then Return String.Empty
        Return String.Join(" ", k_element.Value.Split(New Char() {" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf},
                                                       StringSplitOptions.RemoveEmptyEntries))
    End Function

    Private Shared Function Clip(k_text As String, k_length As Integer) As String
        Return If(k_text.Length > k_length, k_text.Substring(0, k_length), k_text)
    End Function

End Class
