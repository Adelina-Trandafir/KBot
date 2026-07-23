Option Strict On
Imports System.IO
Imports System.Xml
Imports KBot.Common

' Fațada publică a librăriei XFA (înlocuiește Sub Main / parsarea CLI din XFA_WRITTER).
' K-BOT o apelează DIRECT, în proces — NU mai lansează un .exe separat cum făcea Access.
'
' Trei moduri, oglindind exe-ul original:
'   • Genereaza            — completează XFA + embedează atașamente (fără Adobe).
'   • GenereazaSiSemneaza  — ca mai sus, apoi deschide Adobe și așteaptă semnarea.
'   • Semneaza             — deschide un PDF deja generat în Adobe și așteaptă semnarea.
'
' ATENȚIE: modurile cu semnare deschid Adobe și BLOCHEAZĂ (WaitForExit, fără timeout)
' până la închiderea Adobe. Apelantul UI trebuie să le ruleze pe un thread de fundal
' (ex. Await Task.Run(...)), ca să nu înghețe fereastra.
<System.Runtime.Versioning.SupportedOSPlatform("windows")>
Public NotInheritable Class XfaWriter

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Generează PDF-ul: descarcă macheta (după tip), completează XFA din XML și embedează
    ''' atașamentele base64 din XML. NU deschide Adobe. Opțional deschide PDF-ul rezultat cu
    ''' aplicația implicită. Metodă de graniță: loghează în GlobalErrorLog și rearuncă.
    ''' </summary>
    Public Shared Function Genereaza(xmlPath As String, outputPdfPath As String, docType As String,
                                     Optional deschidePdf As Boolean = False) As XfaResult
        Try
            XfaLog.Init(docType)
            XfaLog.Log("INFO", $"Genereaza — xml: {xmlPath}, output: {outputPdfPath}, tip: {docType}")

            Dim templatePath As String = Nothing
            Dim cleanXmlPath As String = Nothing
            Dim attachments As List(Of AttachmentModel) = PregatesteGenerare(xmlPath, docType, templatePath, cleanXmlPath)

            Try
                Dim cPDF As New AdobeUtils(attachments)
                XfaLog.LogSection("ProcessXfa")
                cPDF.ProcessXfa(templatePath, outputPdfPath, cleanXmlPath)
            Finally
                CurataXmlTemporar(cleanXmlPath, xmlPath)
            End Try

            If deschidePdf Then DeschideCuAplicatiaImplicita(outputPdfPath)

            XfaLog.Log("INFO", "Genereaza — OK")
            Return XfaResult.Generat(outputPdfPath)

        Catch ex As Exception
            GlobalErrorLog.Write("XfaWriter.Genereaza", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Generează PDF-ul (ca <see cref="Genereaza"/>), apoi deschide Adobe și AȘTEAPTĂ
    ''' semnarea + închiderea Adobe, întorcând masca semnatarilor. Metodă de graniță.
    ''' </summary>
    Public Shared Function GenereazaSiSemneaza(xmlPath As String, outputPdfPath As String, docType As String) As XfaResult
        Try
            XfaLog.Init(docType)
            XfaLog.Log("INFO", $"GenereazaSiSemneaza — xml: {xmlPath}, output: {outputPdfPath}, tip: {docType}")

            Dim templatePath As String = Nothing
            Dim cleanXmlPath As String = Nothing
            Dim attachments As List(Of AttachmentModel) = PregatesteGenerare(xmlPath, docType, templatePath, cleanXmlPath)

            Dim masca As Integer
            Try
                Dim cPDF As New AdobeUtils(attachments)
                XfaLog.LogSection("ProcessAndVerifySignature")
                masca = cPDF.ProcessAndVerifySignature(templatePath, outputPdfPath, cleanXmlPath, docType)
            Finally
                CurataXmlTemporar(cleanXmlPath, xmlPath)
            End Try

            XfaLog.Log("INFO", $"GenereazaSiSemneaza — mască: {masca}")
            Return XfaResult.DupaSemnare(masca, outputPdfPath)

        Catch ex As Exception
            GlobalErrorLog.Write("XfaWriter.GenereazaSiSemneaza", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Deschide un PDF deja generat în Adobe și AȘTEAPTĂ semnarea + închiderea Adobe,
    ''' întorcând masca semnatarilor. Modul „doar semnare". Metodă de graniță.
    ''' </summary>
    Public Shared Function Semneaza(outputPdfPath As String, docType As String) As XfaResult
        Try
            XfaLog.Init(If(String.IsNullOrEmpty(docType), "SIGN", docType))
            XfaLog.Log("INFO", $"Semneaza — pdf: {outputPdfPath}, tip: {docType}")

            If Not File.Exists(outputPdfPath) Then
                Throw New FileNotFoundException($"Fișierul PDF nu a fost găsit: {outputPdfPath}", outputPdfPath)
            End If

            XfaLog.LogSection("WaitForSignature (doar semnare)")
            Dim masca As Integer = AdobeUtils.WaitForSignature(outputPdfPath, docType)

            XfaLog.Log("INFO", $"Semneaza — mască: {masca}")
            Return XfaResult.DupaSemnare(masca, outputPdfPath)

        Catch ex As Exception
            GlobalErrorLog.Write("XfaWriter.Semneaza", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Extrage atașamentele base64 din nodul &lt;Attachments&gt; și întoarce calea unui XML
    ''' „curat" (fără acel nod) pentru XFA. Dacă nu există atașamente, cleanXmlPath = xmlPath.
    ''' Public pentru că e util și la verificare/testare izolată.
    ''' </summary>
    Public Shared Function ExtrageAtasamente(xmlPath As String, ByRef cleanXmlPath As String) As List(Of AttachmentModel)
        Dim attachments As New List(Of AttachmentModel)

        Dim doc As New XmlDocument()
        doc.Load(xmlPath)

        Dim attachmentsNode As XmlNode = doc.SelectSingleNode("//Attachments")

        If attachmentsNode IsNot Nothing Then
            For Each attNode As XmlNode In attachmentsNode.SelectNodes("Attachment")
                Dim fileNameNode = attNode.SelectSingleNode("FileName")
                Dim fileDataNode = attNode.SelectSingleNode("FileData")

                If fileNameNode IsNot Nothing AndAlso fileDataNode IsNot Nothing Then
                    Dim fileName = fileNameNode.InnerText.Trim()
                    Dim base64Data = fileDataNode.InnerText.Trim()

                    If Not String.IsNullOrEmpty(fileName) AndAlso Not String.IsNullOrEmpty(base64Data) Then
                        Try
                            attachments.Add(New AttachmentModel() With {
                                .FileName = fileName,
                                .FileData = Convert.FromBase64String(base64Data),
                                .IsDeleted = False
                            })
                        Catch ex As FormatException
                            ' base64 invalid — nu oprim procesarea, dar NU înghițim eroarea: o logăm.
                            XfaLog.Log("WARN", $"ExtrageAtasamente — base64 invalid pentru atașamentul '{fileName}', ignorat: {ex.Message}")
                        End Try
                    End If
                End If
            Next

            ' Scoate nodul Attachments din XML înainte de a-l trimite la XFA
            attachmentsNode.ParentNode.RemoveChild(attachmentsNode)

            ' Salvează XML curat în temp
            cleanXmlPath = Path.Combine(Path.GetTempPath(), $"xfa_data_{Guid.NewGuid():N}.xml")
            doc.Save(cleanXmlPath)
        Else
            ' Fără atașamente - folosește XML-ul original ca atare
            cleanXmlPath = xmlPath
        End If

        Return attachments
    End Function

    ''' <summary>
    ''' Pași comuni de generare: verifică XML-ul, separă atașamentele și descarcă macheta.
    ''' </summary>
    Private Shared Function PregatesteGenerare(xmlPath As String, docType As String,
                                               ByRef templatePath As String, ByRef cleanXmlPath As String) As List(Of AttachmentModel)
        If Not File.Exists(xmlPath) Then
            Throw New FileNotFoundException($"Fișierul XML nu a fost găsit: {xmlPath}", xmlPath)
        End If

        XfaLog.LogSection("ExtrageAtasamente")
        Dim attachments As List(Of AttachmentModel) = ExtrageAtasamente(xmlPath, cleanXmlPath)
        XfaLog.Log("INFO", $"ExtrageAtasamente — attachments: {attachments.Count}, cleanXml: {cleanXmlPath}")

        XfaLog.LogSection("GetTemplatePath")
        templatePath = TemplateDownloader.GetTemplatePath(docType)
        XfaLog.Log("INFO", $"GetTemplatePath — templatePath: {templatePath}")

        Return attachments
    End Function

    ''' <summary>Șterge XML-ul temporar „curat" (dacă e diferit de originalul primit).</summary>
    Private Shared Sub CurataXmlTemporar(cleanXmlPath As String, xmlPath As String)
        If cleanXmlPath Is Nothing Then Return
        If cleanXmlPath <> xmlPath AndAlso File.Exists(cleanXmlPath) Then
            Try
                File.Delete(cleanXmlPath)
                XfaLog.Log("INFO", $"Cleanup — șters XML temporar: {cleanXmlPath}")
            Catch ex As Exception
                XfaLog.Log("WARN", $"Cleanup — nu s-a putut șterge XML temporar '{cleanXmlPath}': {ex.Message}")
            End Try
        End If
    End Sub

    ''' <summary>Deschide PDF-ul cu aplicația implicită (echivalent Process.Start din README).</summary>
    Private Shared Sub DeschideCuAplicatiaImplicita(pdfPath As String)
        Dim psi As New ProcessStartInfo(pdfPath) With {.UseShellExecute = True}
        Process.Start(psi)
        XfaLog.Log("INFO", $"DeschidePdf — deschis cu aplicația implicită: {pdfPath}")
    End Sub

End Class
