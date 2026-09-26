Option Strict On
Imports System.Xml
Imports Xunit
Imports KBot.Xfa

' Slice 0081-05: a table cell's attributes travel into the PDF's XFA DOM. Without them an image
' cell (xfa:contentType="image/png") is drawn as its base64 text. Written, not run (house rule).
Public Class CopyAttributesTests

    Private Const XfaData As String = "http://www.xfa.org/schema/xfa-data/1.0/"

    <Fact>
    Public Sub ImageCell_KeepsContentTypeWithItsNamespace_AndHref()
        Dim src As New XmlDocument()
        src.LoadXml($"<Row1 xmlns:xfa=""{XfaData}""><Cell1 xfa:contentType=""image/png"" href="""">QUJD</Cell1></Row1>")
        Dim cell As XmlNode = src.DocumentElement.FirstChild

        Dim target As New XmlDocument()
        target.LoadXml($"<Table4 xmlns:xfa=""{XfaData}""/>")
        Dim twin As XmlElement = target.CreateElement("Cell1")
        target.DocumentElement.AppendChild(twin)

        AdobeUtils.CopyAttributes(target, cell, twin)

        Assert.Equal("image/png", twin.GetAttribute("contentType", XfaData))
        Assert.True(twin.HasAttribute("href"))
    End Sub

    <Fact>
    Public Sub NamespaceDeclarations_AreNotCopied()
        Dim src As New XmlDocument()
        src.LoadXml($"<Cell1 xmlns:xfa=""{XfaData}"" xfa:contentType=""image/png""/>")
        Dim target As New XmlDocument()
        target.LoadXml("<Row1/>")
        Dim twin As XmlElement = target.CreateElement("Cell1")
        target.DocumentElement.AppendChild(twin)

        AdobeUtils.CopyAttributes(target, src.DocumentElement, twin)

        Assert.Equal(1, twin.Attributes.Count)
    End Sub

    <Fact>
    Public Sub NoAttributes_NothingHappens()
        Dim src As New XmlDocument()
        src.LoadXml("<Cell1>x</Cell1>")
        Dim target As New XmlDocument()
        target.LoadXml("<Row1/>")
        Dim twin As XmlElement = target.CreateElement("Cell1")

        AdobeUtils.CopyAttributes(target, src.DocumentElement, twin)

        Assert.Equal(0, twin.Attributes.Count)
    End Sub
End Class
