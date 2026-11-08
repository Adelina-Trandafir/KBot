Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Xml.Linq

' SLICE-VC12-01. Read the supplied inventory layout without Excel/ACE or extra packages.
Public Class InventoryItem
    Public Property Row As Integer
    Public Property Name As String = String.Empty
    Public Property Code As String = String.Empty
    Public Property Um As String = String.Empty
    Public Property Price As String = String.Empty
    Public Property Quantity As String = String.Empty
    Public Property Kind As String = String.Empty
End Class

Public NotInheritable Class InventoryExcel
    Private Sub New()
    End Sub

    Private Shared Function Xml(k_zip As ZipArchive, k_path As String) As XDocument
        Dim k_entry = k_zip.GetEntry(k_path)
        If k_entry Is Nothing OrElse k_entry.Length > 25000000 Then Throw New AdminException("Structură XLSX lipsă sau prea mare: " & k_path)
        Using k_stream = k_entry.Open()
            Return XDocument.Load(k_stream)
        End Using
    End Function

    Private Shared Function SheetPaths(k_zip As ZipArchive) As Dictionary(Of String, String)
        Dim k_workbook = Xml(k_zip, "xl/workbook.xml")
        Dim k_rels = Xml(k_zip, "xl/_rels/workbook.xml.rels")
        Dim k_ns = k_workbook.Root.Name.Namespace
        Dim k_relNs As XNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
        Dim k_targets = k_rels.Root.Elements().Where(Function(k_r) CStr(k_r.Attribute("TargetMode")) <> "External").
            ToDictionary(Function(k_r) CStr(k_r.Attribute("Id")), Function(k_r) CStr(k_r.Attribute("Target")))
        Dim k_result As New Dictionary(Of String, String)(StringComparer.Ordinal)
        For Each k_sheet In k_workbook.Descendants(k_ns + "sheet")
            Dim k_id = CStr(k_sheet.Attribute(k_relNs + "id"))
            If Not k_targets.ContainsKey(k_id) Then Continue For
            Dim k_uri As New Uri(New Uri("https://xlsx.local/xl/workbook.xml"), k_targets(k_id))
            If k_uri.Host <> "xlsx.local" Then Throw New AdminException("Foaie XLSX externă neacceptată.")
            k_result.Add(CStr(k_sheet.Attribute("name")), Uri.UnescapeDataString(k_uri.AbsolutePath.TrimStart("/"c)))
        Next
        Return k_result
    End Function

    Public Shared Function Sheets(k_path As String) As List(Of String)
        Using k_file As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using k_zip As New ZipArchive(k_file, ZipArchiveMode.Read)
                Return SheetPaths(k_zip).Keys.OrderBy(Function(k_name) k_name,
                    StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)).ToList()
            End Using
        End Using
    End Function

    Private Shared Function Text(k_cell As XElement, k_shared As List(Of String), k_ns As XNamespace) As String
        Dim k_type = CStr(k_cell.Attribute("t"))
        If k_type = "inlineStr" Then Return String.Concat(k_cell.Descendants(k_ns + "t").Select(Function(k_t) k_t.Value)).Trim()
        Dim k_value = k_cell.Element(k_ns + "v")?.Value
        If k_type = "e" Then Return String.Empty
        If k_type = "s" Then
            Dim k_index As Integer
            If Integer.TryParse(k_value, k_index) AndAlso k_index >= 0 AndAlso k_index < k_shared.Count Then Return k_shared(k_index).Trim()
            Throw New AdminException("Referință text invalidă în XLSX.")
        End If
        Return If(k_value, String.Empty).Trim()
    End Function

    Private Shared Function Value(k_cells As Dictionary(Of String, String), k_column As String) As String
        Dim k_value As String = Nothing
        Return If(k_cells.TryGetValue(k_column, k_value), k_value, String.Empty)
    End Function

    Public Shared Function Read(k_path As String, k_sheetName As String) As List(Of InventoryItem)
        Using k_file As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using k_zip As New ZipArchive(k_file, ZipArchiveMode.Read)
                Dim k_paths = SheetPaths(k_zip)
                If Not k_paths.ContainsKey(k_sheetName) Then Throw New AdminException("Foaia aleasă nu mai există în fișier.")
                Dim k_shared As New List(Of String)()
                If k_zip.GetEntry("xl/sharedStrings.xml") IsNot Nothing Then
                    Dim k_strings = Xml(k_zip, "xl/sharedStrings.xml")
                    Dim k_sns = k_strings.Root.Name.Namespace
                    k_shared = k_strings.Descendants(k_sns + "si").Select(
                        Function(k_si) String.Concat(k_si.Descendants(k_sns + "t").Select(Function(k_t) k_t.Value))).ToList()
                End If
                Dim k_doc = Xml(k_zip, k_paths(k_sheetName))
                Dim k_ns = k_doc.Root.Name.Namespace
                Dim k_kind As String = String.Empty
                Dim k_result As New List(Of InventoryItem)()
                For Each k_row In k_doc.Descendants(k_ns + "row")
                    Dim k_cells As New Dictionary(Of String, String)(StringComparer.Ordinal)
                    For Each k_cell In k_row.Elements(k_ns + "c")
                        Dim k_address = CStr(k_cell.Attribute("r"))
                        If String.IsNullOrEmpty(k_address) Then Continue For
                        Dim k_column = New String(k_address.TakeWhile(Function(k_ch) Char.IsLetter(k_ch)).ToArray())
                        k_cells(k_column) = Text(k_cell, k_shared, k_ns)
                    Next
                    If Value(k_cells, "K").Trim().TrimEnd(":"c).Equals("Gestiunea", StringComparison.OrdinalIgnoreCase) Then
                        Dim k_label = Value(k_cells, "L").ToLowerInvariant()
                        k_kind = If(k_label.Contains("mijloace fixe"), "MF", If(k_label.Contains("obiecte de inventar"), "OI", String.Empty))
                    End If
                    Dim k_number As Integer
                    If Not Integer.TryParse(Value(k_cells, "A"), k_number) OrElse k_number <= 0 Then Continue For
                    Dim k_name = Value(k_cells, "B")
                    If String.IsNullOrWhiteSpace(k_name) Then Throw New AdminException("Denumire lipsă la rândul " & CStr(k_row.Attribute("r")) & ".")
                    If k_kind = String.Empty Then Throw New AdminException("Nu pot identifica secțiunea MF/OI la rândul " & CStr(k_row.Attribute("r")) & ". Este acceptată macheta listei de inventariere din fișierul furnizat.")
                    k_result.Add(New InventoryItem With {.Row = Integer.Parse(CStr(k_row.Attribute("r")), CultureInfo.InvariantCulture),
                        .Name = k_name, .Code = Value(k_cells, "C"), .Um = Value(k_cells, "D"), .Quantity = Value(k_cells, "F"),
                        .Price = Value(k_cells, "I"), .Kind = k_kind})
                Next
                If k_result.Count = 0 Then Throw New AdminException("Nu am găsit poziții în macheta de inventar aleasă.")
                Return k_result
            End Using
        End Using
    End Function
End Class
