Option Strict On
Imports System.IO
Imports System.Xml
Imports iTextSharp.text.pdf
Imports KBot.Common

' Motorul XFA (iTextSharp 5): completează câmpurile/tabelele XFA din XML, embedează
' atașamentele și — opțional — deschide Adobe și așteaptă semnarea, întorcând masca
' semnatarilor. Portat din XFA_WRITTER (AdobeUtils), fără blocul Namespace (regula casei).
<System.Runtime.Versioning.SupportedOSPlatform("windows")>
Public Class AdobeUtils
    Private ReadOnly pAttachments As List(Of AttachmentModel)

    ' Măști semnatari (combinate pe biți în rezultat).
    ' ORD: bit1 = SubformSemnaturaAB, bit2 = SubformSemnaturaCD, bit4 = Ordonator.
    ' DDF: bit1 = SubformSemnaturaA,  bit2 = SubformSemnaturaB,  bit4 = Ordonator.
    Public Const SIGNER_AB As Integer = 1
    Public Const SIGNER_CD As Integer = 2
    Public Const SIGNER_ORDONATOR As Integer = 4

    ' Sentinelă iTextSharp pentru versiunea PDF: NUL înseamnă „păstrează versiunea machetei".
    ' (În exe-ul original era literalul "\0", tratat pe Option Strict Off ca primul caracter.)
    Private Const KEEP_PDF_VERSION As Char = ChrW(0)

    Public Sub New(Optional attList As List(Of AttachmentModel) = Nothing)
        If attList Is Nothing Then
            pAttachments = New List(Of AttachmentModel)
        Else
            pAttachments = attList
        End If
    End Sub

    ''' <summary>
    ''' Modifică XFA din XML, apoi embedează atașamentele în PDF-ul de ieșire.
    ''' NU deschide Adobe și NU așteaptă semnătura.
    ''' </summary>
    Public Sub ProcessXfa(inputPdfPath As String, outputPdfPath As String, dataXMLPath As String)
        Try
            Dim rsp = ModifyXfaFromXml(inputPdfPath, outputPdfPath, dataXMLPath)
            If rsp <> "OK" Then
                Throw New Exception(rsp)
            End If

            If Not AddAttachmentsInPlace(outputPdfPath, pAttachments) Then
                Throw New Exception("Eroare la adăugarea atașamentelor în PDF.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUtils.ProcessXfa", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Modifică XFA, embedează atașamentele, deschide Adobe și așteaptă (fără timeout)
    ''' semnarea + închiderea Adobe, apoi verifică ce semnatari au semnat.
    ''' Aruncă DOAR pentru erori reale. Pentru rezultat întoarce masca semnatarilor (0 = nesemnat).
    ''' </summary>
    ''' <returns>Mască semnatari: SIGNER_AB | SIGNER_CD | SIGNER_ORDONATOR (0 dacă nimic semnat)</returns>
    Public Function ProcessAndVerifySignature(inputPdfPath As String, outputPdfPath As String, dataXMLPath As String, docType As String) As Integer
        Try
            Dim rsp = ModifyXfaFromXml(inputPdfPath, outputPdfPath, dataXMLPath)
            If rsp <> "OK" Then
                Throw New Exception(rsp)
            End If

            If Not AddAttachmentsInPlace(outputPdfPath, pAttachments) Then
                Throw New Exception("Eroare la adăugarea atașamentelor în PDF.")
            End If

            Return WaitForSignatureInternal(outputPdfPath, docType)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUtils.ProcessAndVerifySignature", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Deschide un PDF existent în Adobe, așteaptă (fără timeout) închiderea Adobe,
    ''' apoi verifică ce semnatari au semnat. Folosit pentru modul „doar semnare".
    ''' </summary>
    ''' <returns>Mască semnatari (0 dacă nimic semnat)</returns>
    Public Shared Function WaitForSignature(pdfPath As String, docType As String) As Integer
        Try
            Return WaitForSignatureInternal(pdfPath, docType)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUtils.WaitForSignature", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Masca semnatarilor pentru o listă de nume de câmpuri de semnătură (utilă și pentru
    ''' verificare fără Adobe/PDF). Combinație SIGNER_AB/CD/ORDONATOR; 0 = nimic clasificat.
    ''' </summary>
    Public Shared Function MascaSemnatari(fieldNames As IEnumerable(Of String), docType As String) As Integer
        Dim mask As Integer = 0
        If fieldNames Is Nothing Then Return 0
        For Each nm In fieldNames
            mask = mask Or ClassifySigner(nm, docType)
        Next
        Return mask
    End Function

    ''' <summary>
    ''' Modifică câmpurile/tabelele XFA pe baza XML-ului de configurare.
    ''' </summary>
    Private Shared Function ModifyXfaFromXml(inputPdfPath As String, outputPdfPath As String, configXmlPath As String) As String
        XfaLog.Log("INFO", $"ModifyXfaFromXml — inputPdf: {inputPdfPath}, outputPdf: {outputPdfPath}, dataXml: {configXmlPath}")
        Dim reader As PdfReader = Nothing
        Dim stamper As PdfStamper = Nothing
        Try
            reader = New PdfReader(inputPdfPath)
            stamper = New PdfStamper(reader, New FileStream(outputPdfPath, FileMode.Create), KEEP_PDF_VERSION, True)

            Dim xfaForm As XfaForm = stamper.AcroFields.Xfa
            Dim domDoc As System.Xml.XmlDocument = xfaForm.DomDocument

            Dim configDoc As New System.Xml.XmlDocument()
            configDoc.Load(configXmlPath)

            ProcessXmlNodes(configDoc.DocumentElement, domDoc, Nothing)

            xfaForm.DomDocument = domDoc
            xfaForm.Changed = True

            XfaLog.Log("INFO", "ModifyXfaFromXml: OK")
            Return "OK"

        Catch ex As Exception
            XfaLog.Log("ERROR", $"ModifyXfaFromXml: {ex.Message}{vbCrLf}{ex.StackTrace}")
            Return $"Eroare la modificarea XFA: {ex.Message}"
        Finally
            Try
                stamper?.Close()
            Catch ex As Exception
                XfaLog.Log("WARN", $"ModifyXfaFromXml — eroare la închiderea stamper: {ex.Message}")
            End Try
            Try
                reader?.Close()
            Catch ex As Exception
                XfaLog.Log("WARN", $"ModifyXfaFromXml — eroare la închiderea reader: {ex.Message}")
            End Try
        End Try
    End Function

    ' Subformulare care se pot repeta în DOM-ul PDF (clonate când XML are mai multe instanțe decât macheta)
    Private Shared ReadOnly RepeatingSubforms As New HashSet(Of String)({"SubformInf"})

    ''' <summary>
    ''' Procesează recursiv nodurile XML și le aplică pe XFA.
    ''' </summary>
    Private Shared Sub ProcessXmlNodes(xmlNode As XmlNode, pdfDoc As XmlDocument, pdfContextNode As XmlNode)
        Dim searchRoot As XmlNode = If(pdfContextNode, CType(pdfDoc, XmlNode))

        Dim isTable = xmlNode.SelectSingleNode("Row1") IsNot Nothing

        If isTable Then
            Dim tableName = xmlNode.Name
            Dim pdfTableNode = searchRoot.SelectSingleNode($".//{tableName}")
            Dim found = pdfTableNode IsNot Nothing

            Dim rowsRemoved = 0
            Dim rowsAdded = 0

            If found Then
                Dim existingRows = pdfTableNode.SelectNodes("Row1")
                rowsRemoved = existingRows.Count
                For Each row As XmlNode In existingRows
                    pdfTableNode.RemoveChild(row)
                Next

                For Each row As XmlNode In xmlNode.SelectNodes("Row1")
                    Dim newRow As XmlElement = pdfDoc.CreateElement("Row1")
                    For Each cell As XmlNode In row.ChildNodes
                        newRow.AppendChild(CreateElement(pdfDoc, cell.Name, CStr(cell.InnerText)))
                    Next
                    pdfTableNode.AppendChild(newRow)
                    rowsAdded += 1
                Next
            End If

            If found Then
                XfaLog.Log("DEBUG", $"ProcessXmlNodes: TABLE '{tableName}' — found in DOM: YES, rows removed: {rowsRemoved}, rows added: {rowsAdded}")
            Else
                XfaLog.Log("WARN", $"ProcessXmlNodes: TABLE '{tableName}' — found in DOM: NO (node not found)")
            End If

        Else
            For Each childNode As XmlNode In xmlNode.ChildNodes
                Dim nodeName = childNode.Name
                Dim nodeValue = CStr(childNode.InnerText)

                If childNode.HasChildNodes AndAlso childNode.SelectNodes("*").Count > 0 Then
                    If RepeatingSubforms.Contains(nodeName) Then
                        Dim idx = GetChildIndex(childNode)
                        Dim allInstances = pdfDoc.SelectNodes($"//{nodeName}")
                        Dim targetPdfNode As XmlNode

                        If idx < allInstances.Count Then
                            targetPdfNode = allInstances(idx)
                            XfaLog.Log("DEBUG", $"ProcessXmlNodes: REPEATING '{nodeName}' idx={idx} — reusing existing instance")
                        Else
                            Dim lastInstance = allInstances(allInstances.Count - 1)
                            targetPdfNode = lastInstance.CloneNode(True)
                            lastInstance.ParentNode.AppendChild(targetPdfNode)
                            XfaLog.Log("DEBUG", $"ProcessXmlNodes: REPEATING '{nodeName}' idx={idx} — cloned new instance (was {allInstances.Count})")
                        End If

                        ProcessXmlNodes(childNode, pdfDoc, targetPdfNode)
                    Else
                        XfaLog.Log("DEBUG", $"ProcessXmlNodes: RECURSE '{nodeName}'")
                        ProcessXmlNodes(childNode, pdfDoc, pdfContextNode)
                    End If
                ElseIf Not String.IsNullOrWhiteSpace(nodeValue) Then
                    Dim displayValue = If(nodeValue.Length > 80, String.Concat(nodeValue.AsSpan(0, 80), "…"), nodeValue)
                    XfaLog.Log("DEBUG", $"ProcessXmlNodes: FIELD '{nodeName}' — value: '{displayValue}'")
                    Dim fieldXPath = $".//{nodeName}"
                    Dim fieldNode = searchRoot.SelectSingleNode(fieldXPath)
                    If fieldNode IsNot Nothing Then
                        fieldNode.InnerText = nodeValue
                    Else
                        XfaLog.Log("WARN", $"ProcessXmlNodes: FIELD '{nodeName}' — not found in DOM (context: {If(pdfContextNode IsNot Nothing, pdfContextNode.Name, "root")})")
                    End If
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' Câți frați anteriori au același nume (index 0-based al acestei apariții).
    ''' </summary>
    Private Shared Function GetChildIndex(node As XmlNode) As Integer
        Dim count As Integer = 0
        Dim sibling = node.PreviousSibling
        Do While sibling IsNot Nothing
            If sibling.Name = node.Name Then count += 1
            sibling = sibling.PreviousSibling
        Loop
        Return count
    End Function

    ''' <summary>
    ''' Adaugă atașamentele în PDF, în loc.
    ''' </summary>
    Private Shared Function AddAttachmentsInPlace(pPdfPath As String, pAttList As List(Of AttachmentModel)) As Boolean
        Try
            If pAttList Is Nothing OrElse pAttList.Count = 0 Then
                XfaLog.Log("INFO", "AddAttachmentsInPlace: 0 atașamente — skip")
                Return True
            End If

            Dim pdfBytes As Byte() = File.ReadAllBytes(pPdfPath)
            Dim output As New MemoryStream()

            Dim reader As New PdfReader(pdfBytes)
            Dim stamper As New PdfStamper(reader, output, KEEP_PDF_VERSION, True)

            Dim addedCount = 0
            For Each att As AttachmentModel In pAttList
                If att Is Nothing Then Continue For
                If att.IsDeleted Then Continue For
                If att.FileData Is Nothing OrElse att.FileData.Length = 0 Then Continue For

                Dim pName As String = Path.GetFileName(att.FileName)

                Dim fileSpec As PdfFileSpecification =
                    PdfFileSpecification.FileEmbedded(
                        stamper.Writer,
                        att.FileName,
                        pName,
                        att.FileData
                    )

                stamper.AddFileAttachment(pName, fileSpec)
                addedCount += 1
            Next

            stamper.Close()
            reader.Close()

            File.WriteAllBytes(pPdfPath, output.ToArray())

            XfaLog.Log("INFO", $"AddAttachmentsInPlace: {addedCount} atașamente adăugate")
            Return True

        Catch ex As Exception
            XfaLog.Log("ERROR", $"AddAttachmentsInPlace — eroare: {ex.Message}{vbCrLf}{ex.StackTrace}")
            Throw New Exception($"Eroare la adăugarea atașamentelor în PDF: {ex.Message}", ex)
        End Try
    End Function

    ''' <summary>
    ''' Deschide PDF-ul în Adobe și AȘTEAPTĂ (fără timeout) închiderea. După închidere
    ''' calculează masca semnatarilor. Plasă de siguranță: refuză dacă handler-ul nu e Adobe.
    ''' </summary>
    ''' <returns>Mască semnatari (0 dacă nimic semnat)</returns>
    Private Shared Function WaitForSignatureInternal(pdfPath As String, docType As String) As Integer
        XfaLog.Log("INFO", $"WaitForSignature — pdf: {pdfPath}, tip: {docType}")

        Dim adobePath As String
        Try
            adobePath = GetAdobeReaderPath()
        Catch ex As Exception
            XfaLog.Log("ERROR", $"WaitForSignature — eroare la căutarea Adobe Reader: {ex.Message}{vbCrLf}{ex.StackTrace}")
            Throw New Exception($"Eroare la căutarea Adobe Reader: {ex.Message}", ex)
        End Try

        If String.IsNullOrEmpty(adobePath) Then
            XfaLog.Log("ERROR", "WaitForSignature — Adobe Reader nu a fost găsit")
            Throw New Exception("Adobe Reader nu a fost găsit.")
        End If
        XfaLog.Log("INFO", $"WaitForSignature — Adobe: {adobePath}")

        ' Plasă de siguranță: handler-ul implicit de PDF trebuie să fie Adobe
        If Not IsAdobeViewer(adobePath) Then
            XfaLog.Log("ERROR", $"WaitForSignature — handler-ul implicit de PDF nu este Adobe: {adobePath}. Semnarea a fost anulată.")
            Throw New Exception(
                $"Aplicația implicită pentru PDF nu este Adobe ({Path.GetFileName(adobePath)})." & vbCrLf &
                "Semnarea a fost anulată. Setați Adobe Reader/Acrobat ca aplicație implicită pentru fișierele PDF."
            )
        End If

        Dim proc As Process = Nothing
        Try
            proc = New Process()
            proc.StartInfo.FileName = adobePath
            proc.StartInfo.Arguments = $"""{pdfPath}"""
            proc.StartInfo.UseShellExecute = False

            Dim startedAt = DateTime.Now
            proc.Start()
            XfaLog.Log("INFO", $"WaitForSignature — Adobe pornit (PID {proc.Id}), aștept închiderea (fără timeout)...")

            proc.WaitForExit()

            Dim elapsed = DateTime.Now - startedAt
            XfaLog.Log("INFO", $"WaitForSignature — Adobe închis după {elapsed.TotalSeconds:F0}s, verific semnăturile...")
            If elapsed.TotalSeconds < 3 Then
                XfaLog.Log("WARN", "WaitForSignature — Adobe s-a închis foarte repede; posibil single-instance (fereastra predată unei instanțe deja deschise). Verificarea poate fi prematură.")
            End If

        Catch ex As Exception
            XfaLog.Log("ERROR", $"WaitForSignature — eroare la pornirea/așteptarea Adobe: {ex.Message}{vbCrLf}{ex.StackTrace}")
            Throw New Exception($"Eroare la pornirea/așteptarea Adobe: {ex.Message}", ex)
        Finally
            Try
                proc?.Dispose()
            Catch ex As Exception
                XfaLog.Log("WARN", $"WaitForSignature — eroare la dispose proces: {ex.Message}")
            End Try
        End Try

        Dim mask As Integer = GetSignerMask(pdfPath, docType)
        XfaLog.Log("INFO", $"WaitForSignature — mască semnatari: {mask}")
        Return mask
    End Function

    ''' <summary>
    ''' Numele câmpurilor de semnătură care AU semnătură (semnate).
    ''' Aruncă dacă PDF-ul nu poate fi citit (eroare reală, NU „nesemnat").
    ''' </summary>
    Private Shared Function GetSignedFieldNames(pdfPath As String) As List(Of String)
        Dim reader As PdfReader = Nothing
        Try
            reader = New PdfReader(pdfPath)
            Dim af As AcroFields = reader.AcroFields
            Dim names As List(Of String) = af.GetSignatureNames()
            If names Is Nothing Then names = New List(Of String)()
            XfaLog.Log("INFO", $"GetSignedFieldNames — câmpuri semnate: {names.Count} [{String.Join(", ", names)}]")
            Return names
        Catch ex As Exception
            XfaLog.Log("ERROR", $"GetSignedFieldNames — eroare la citirea semnăturilor din '{pdfPath}': {ex.Message}{vbCrLf}{ex.StackTrace}")
            Throw New Exception($"Eroare la verificarea semnăturii PDF: {ex.Message}", ex)
        Finally
            Try
                reader?.Close()
            Catch ex As Exception
                XfaLog.Log("WARN", $"GetSignedFieldNames — eroare la închiderea reader: {ex.Message}")
            End Try
        End Try
    End Function

    ''' <summary>
    ''' Masca semnatarilor dintr-un PDF (combinație SIGNER_AB/CD/ORDONATOR). 0 = nimic semnat.
    ''' </summary>
    Private Shared Function GetSignerMask(pdfPath As String, docType As String) As Integer
        Dim names = GetSignedFieldNames(pdfPath)
        Dim mask As Integer = 0
        For Each nm In names
            mask = mask Or ClassifySigner(nm, docType)
        Next
        XfaLog.Log("INFO", $"GetSignerMask — mască: {mask} (AB/A={(mask And SIGNER_AB) <> 0}, CD/B={(mask And SIGNER_CD) <> 0}, Ordonator={(mask And SIGNER_ORDONATOR) <> 0})")
        Return mask
    End Function

    ''' <summary>
    ''' Mapează un nume de câmp de semnătură pe rolul (masca) corespunzător.
    ''' Întâi după numele subformularului (acoperă ORD și DDF), apoi fallback numeric pe tip.
    ''' </summary>
    Private Shared Function ClassifySigner(fieldName As String, docType As String) As Integer
        If String.IsNullOrEmpty(fieldName) Then Return 0
        Dim dt As String = If(docType, "").ToUpperInvariant()

        ' 1) După subform. Ordonator întâi; AB/CD (ORD) ÎNAINTE de A/B (DDF) din cauza coliziunii de substring.
        If fieldName.Contains("SubformSemnaturaOrdonator", StringComparison.OrdinalIgnoreCase) Then Return SIGNER_ORDONATOR
        If fieldName.Contains("SubformSemnaturaAB", StringComparison.OrdinalIgnoreCase) Then Return SIGNER_AB   ' ORD
        If fieldName.Contains("SubformSemnaturaCD", StringComparison.OrdinalIgnoreCase) Then Return SIGNER_CD   ' ORD
        If fieldName.Contains("SubformSemnaturaA", StringComparison.OrdinalIgnoreCase) Then Return SIGNER_AB    ' DDF (A) — după AB
        If fieldName.Contains("SubformSemnaturaB", StringComparison.OrdinalIgnoreCase) Then Return SIGNER_CD    ' DDF (B)
        ' 2) Fallback numeric, în funcție de tipul documentului
        Dim num As Integer = ExtractSignatureFieldNumber(fieldName)
        If num >= 0 Then
            Select Case dt
                Case "ORD"
                    ' ORD: 1/2 = AB, 3/4 = CD, 5 = Ordonator
                    Select Case num
                        Case 1, 2 : Return SIGNER_AB
                        Case 3, 4 : Return SIGNER_CD
                        Case 5 : Return SIGNER_ORDONATOR
                    End Select
                Case "DDF"
                    ' DDF: 1 + 11..16 = A; 2 + 21..26 = B; 3 = Ordonator
                    Select Case num
                        Case 1 : Return SIGNER_AB
                        Case 2 : Return SIGNER_CD
                        Case 3 : Return SIGNER_ORDONATOR
                        Case 11 To 16 : Return SIGNER_AB
                        Case 21 To 26 : Return SIGNER_CD
                    End Select
            End Select
        End If

        XfaLog.Log("WARN", $"ClassifySigner — câmp neclasificat: '{fieldName}' (tip: {dt})")
        Return 0
    End Function

    ''' <summary>
    ''' Extrage numărul de după „SignatureField" (citește toate cifrele consecutive).
    ''' Întoarce -1 dacă nu găsește.
    ''' </summary>
    Private Shared Function ExtractSignatureFieldNumber(fieldName As String) As Integer
        Const marker As String = "SignatureField"
        Dim idx As Integer = fieldName.IndexOf(marker, StringComparison.OrdinalIgnoreCase)
        If idx < 0 Then Return -1

        Dim i As Integer = idx + marker.Length
        Dim sb As New System.Text.StringBuilder()
        While i < fieldName.Length AndAlso Char.IsDigit(fieldName(i))
            sb.Append(fieldName(i))
            i += 1
        End While

        If sb.Length = 0 Then Return -1
        Dim n As Integer
        If Integer.TryParse(sb.ToString(), n) Then Return n
        Return -1
    End Function

    ''' <summary>
    ''' Verifică tolerant dacă exe-ul e Adobe: după numele fișierului
    ''' (Acrobat.exe / AcroRd32.exe) SAU după metadata (CompanyName conține "Adobe").
    ''' </summary>
    Private Shared Function IsAdobeViewer(exePath As String) As Boolean
        If String.IsNullOrWhiteSpace(exePath) Then Return False

        ' 1) După numele fișierului
        Try
            Dim fileName As String = Path.GetFileName(exePath).ToLowerInvariant()
            If fileName = "acrobat.exe" OrElse fileName = "acrord32.exe" Then
                XfaLog.Log("INFO", $"IsAdobeViewer — potrivire după nume: {fileName}")
                Return True
            End If
        Catch ex As Exception
            XfaLog.Log("WARN", $"IsAdobeViewer — eroare la citirea numelui din '{exePath}': {ex.Message}")
        End Try

        ' 2) După metadata (CompanyName conține "Adobe")
        Try
            Dim fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath)
            Dim company As String = If(fvi.CompanyName, "")
            If company.Contains("Adobe", StringComparison.OrdinalIgnoreCase) Then
                XfaLog.Log("INFO", $"IsAdobeViewer — potrivire după metadata CompanyName: '{company}'")
                Return True
            End If
            XfaLog.Log("INFO", $"IsAdobeViewer — nepotrivit (nume + CompanyName='{company}')")
        Catch ex As Exception
            XfaLog.Log("WARN", $"IsAdobeViewer — eroare la citirea metadata din '{exePath}': {ex.Message}")
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Returnează exe-ul înregistrat ca handler implicit pentru .pdf.
    ''' Încearcă mai întâi UserChoice (Windows 10/11), apoi HKCR\.pdf.
    ''' </summary>
    Private Shared Function GetAdobeReaderPath() As String
        Try
            Dim progIds As New List(Of String)

            ' 1. UserChoice (Windows 10/11)
            Try
                Using userChoiceKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.pdf\UserChoice")
                    If userChoiceKey IsNot Nothing Then
                        Dim progId = TryCast(userChoiceKey.GetValue("ProgId"), String)
                        If Not String.IsNullOrWhiteSpace(progId) Then
                            progIds.Add(progId)
                            XfaLog.Log("INFO", $"GetAdobeReaderPath — ProgID .pdf din UserChoice: {progId}")
                        End If
                    End If
                End Using
            Catch ex As Exception
                XfaLog.Log("WARN", $"GetAdobeReaderPath — eroare la citirea UserChoice: {ex.Message}")
            End Try

            ' 2. HKCR\.pdf
            Try
                Using extKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(".pdf")
                    If extKey IsNot Nothing Then
                        Dim progId = TryCast(extKey.GetValue(Nothing), String)
                        If Not String.IsNullOrWhiteSpace(progId) Then
                            If Not progIds.Any(Function(x) String.Equals(x, progId, StringComparison.OrdinalIgnoreCase)) Then
                                progIds.Add(progId)
                            End If
                            XfaLog.Log("INFO", $"GetAdobeReaderPath — ProgID .pdf din HKCR: {progId}")
                        End If
                    End If
                End Using
            Catch ex As Exception
                XfaLog.Log("WARN", $"GetAdobeReaderPath — eroare la citirea HKCR\.pdf: {ex.Message}")
            End Try

            If progIds.Count = 0 Then
                XfaLog.Log("WARN", "GetAdobeReaderPath — nu există niciun ProgID pentru .pdf")
                Return Nothing
            End If

            ' 3. Caută shell\open\command pentru fiecare ProgID găsit
            For Each progId In progIds
                Try
                    XfaLog.Log("INFO", $"GetAdobeReaderPath — verific ProgID: {progId}")

                    Dim cmd As String = Nothing
                    Using cmdKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($"{progId}\shell\open\command")
                        If cmdKey IsNot Nothing Then
                            cmd = TryCast(cmdKey.GetValue(Nothing), String)
                        End If
                    End Using

                    If String.IsNullOrWhiteSpace(cmd) Then
                        XfaLog.Log("WARN", $"GetAdobeReaderPath — nu există shell\open\command pentru ProgID '{progId}'")
                        Continue For
                    End If

                    XfaLog.Log("INFO", $"GetAdobeReaderPath — command pentru '{progId}': {cmd}")

                    Dim exePath As String = ExtractExeFromCommand(cmd)

                    If String.IsNullOrWhiteSpace(exePath) Then
                        XfaLog.Log("WARN", $"GetAdobeReaderPath — nu s-a putut extrage exe din comandă: {cmd}")
                        Continue For
                    End If

                    If Not File.Exists(exePath) Then
                        XfaLog.Log("WARN", $"GetAdobeReaderPath — exe extras nu există pe disc: {exePath}")
                        Continue For
                    End If

                    XfaLog.Log("INFO", $"GetAdobeReaderPath — handler PDF găsit: {exePath}")
                    Return exePath

                Catch ex As Exception
                    XfaLog.Log("WARN", $"GetAdobeReaderPath — eroare la procesarea ProgID '{progId}': {ex.Message}")
                End Try
            Next

            XfaLog.Log("WARN", "GetAdobeReaderPath — nu s-a găsit niciun handler PDF valid")
            Return Nothing

        Catch ex As Exception
            XfaLog.Log("ERROR", $"GetAdobeReaderPath — eroare la determinarea aplicației PDF din registry: {ex.Message}{vbCrLf}{ex.StackTrace}")
            Throw New Exception($"Eroare la determinarea aplicației PDF din registry: {ex.Message}", ex)
        End Try
    End Function

    ''' <summary>
    ''' Extrage calea exe dintr-o linie de comandă din registry.
    ''' Tratează atât cazul cu ghilimele ("C:\...\Acrobat.exe" "%1") cât și fără.
    ''' </summary>
    Private Shared Function ExtractExeFromCommand(cmd As String) As String
        If String.IsNullOrWhiteSpace(cmd) Then Return Nothing

        If cmd.StartsWith(""""c) Then
            Dim p As Integer = cmd.IndexOf(""""c, 1)
            If p > 1 Then
                Return cmd.Substring(1, p - 1)
            End If
            Return Nothing
        Else
            Dim p As Integer = cmd.IndexOf(".exe", StringComparison.OrdinalIgnoreCase)
            If p >= 0 Then
                Return cmd.Substring(0, p + 4)
            End If
            Return Nothing
        End If
    End Function

    Private Shared Function CreateElement(doc As XmlDocument, name As String, value As String) As XmlElement
        Dim elem = doc.CreateElement(name)
        elem.InnerText = value
        Return elem
    End Function
End Class
