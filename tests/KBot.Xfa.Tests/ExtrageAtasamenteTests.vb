Option Strict On
Imports System.IO
Imports System.Text
Imports Xunit
Imports KBot.Xfa

' Teste pentru XfaWriter.ExtrageAtasamente — pasul offline care separă nodul <Attachments>
' (base64) de restul XML-ului trimis la XFA. Fără rețea, fără Adobe.
Public Class ExtrageAtasamenteTests

    Private Shared Function ScrieTemp(xml As String) As String
        Dim p As String = Path.Combine(Path.GetTempPath(), $"xfa_test_{Guid.NewGuid():N}.xml")
        File.WriteAllText(p, xml, New UTF8Encoding(False))
        Return p
    End Function

    Private Shared Function B64(text As String) As String
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
    End Function

    <Fact>
    Public Sub CandNuExistaAttachments_CleanEsteChiarOriginalul()
        Dim xmlPath As String = ScrieTemp("<root><MainForm><cif>123</cif></MainForm></root>")
        Try
            Dim cleanXmlPath As String = Nothing
            Dim att = XfaWriter.ExtrageAtasamente(xmlPath, cleanXmlPath)

            Assert.Empty(att)
            Assert.Equal(xmlPath, cleanXmlPath)
        Finally
            File.Delete(xmlPath)
        End Try
    End Sub

    <Fact>
    Public Sub Base64Valid_EsteDecodatSiScosDinXmlCurat()
        Dim continut As String = "salut-atasament"
        Dim xml As String =
            "<root>" &
            "  <MainForm><cif>123</cif></MainForm>" &
            "  <Attachments>" &
            "    <Attachment><FileName>ok.txt</FileName><FileData>" & B64(continut) & "</FileData></Attachment>" &
            "  </Attachments>" &
            "</root>"
        Dim xmlPath As String = ScrieTemp(xml)
        Dim cleanXmlPath As String = Nothing
        Try
            Dim att = XfaWriter.ExtrageAtasamente(xmlPath, cleanXmlPath)

            Assert.Single(att)
            Assert.Equal("ok.txt", att(0).FileName)
            Assert.Equal(continut, Encoding.UTF8.GetString(att(0).FileData))
            Assert.False(att(0).IsDeleted)

            ' Un XML curat NOU în temp, diferit de original, fără nodul Attachments.
            Assert.NotEqual(xmlPath, cleanXmlPath)
            Assert.True(File.Exists(cleanXmlPath))
            Assert.DoesNotContain("Attachments", File.ReadAllText(cleanXmlPath), StringComparison.OrdinalIgnoreCase)
        Finally
            File.Delete(xmlPath)
            If cleanXmlPath IsNot Nothing AndAlso cleanXmlPath <> xmlPath AndAlso File.Exists(cleanXmlPath) Then File.Delete(cleanXmlPath)
        End Try
    End Sub

    <Fact>
    Public Sub Base64Invalid_EsteIgnoratFaraSaOpreascaProcesarea()
        Dim xml As String =
            "<root>" &
            "  <Attachments>" &
            "    <Attachment><FileName>ok.txt</FileName><FileData>" & B64("bun") & "</FileData></Attachment>" &
            "    <Attachment><FileName>rau.txt</FileName><FileData>@@nu-e-base64@@</FileData></Attachment>" &
            "  </Attachments>" &
            "</root>"
        Dim xmlPath As String = ScrieTemp(xml)
        Dim cleanXmlPath As String = Nothing
        Try
            Dim att = XfaWriter.ExtrageAtasamente(xmlPath, cleanXmlPath)

            ' Doar cel valid trece; cel cu base64 stricat e sărit.
            Assert.Single(att)
            Assert.Equal("ok.txt", att(0).FileName)
        Finally
            File.Delete(xmlPath)
            If cleanXmlPath IsNot Nothing AndAlso cleanXmlPath <> xmlPath AndAlso File.Exists(cleanXmlPath) Then File.Delete(cleanXmlPath)
        End Try
    End Sub

    <Fact>
    Public Sub AtasamentFaraNumeSauFaraDate_EsteSarit()
        Dim xml As String =
            "<root>" &
            "  <Attachments>" &
            "    <Attachment><FileName></FileName><FileData>" & B64("x") & "</FileData></Attachment>" &
            "    <Attachment><FileName>fara-date.txt</FileName><FileData></FileData></Attachment>" &
            "    <Attachment><FileName>bun.txt</FileName><FileData>" & B64("y") & "</FileData></Attachment>" &
            "  </Attachments>" &
            "</root>"
        Dim xmlPath As String = ScrieTemp(xml)
        Dim cleanXmlPath As String = Nothing
        Try
            Dim att = XfaWriter.ExtrageAtasamente(xmlPath, cleanXmlPath)

            Assert.Single(att)
            Assert.Equal("bun.txt", att(0).FileName)
        Finally
            File.Delete(xmlPath)
            If cleanXmlPath IsNot Nothing AndAlso cleanXmlPath <> xmlPath AndAlso File.Exists(cleanXmlPath) Then File.Delete(cleanXmlPath)
        End Try
    End Sub

End Class
