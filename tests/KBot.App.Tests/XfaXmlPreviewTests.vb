Option Strict On
Imports System
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.App

' Headless state-machine tests for XfaXmlPreview (slice 0020-03). Window hosting (ReaderHost)
' and pixel rendering cannot be verified without a screen, but the DEFAULT preview's state
' transitions and its render-from-sibling-.xml path ARE testable: we drop a real DDF form1 XML
' next to a fake .pdf path and assert the grid fills.
Public Class XfaXmlPreviewTests

    Private Const SampleForm1 As String =
        "<?xml version=""1.0"" encoding=""UTF-8""?>" &
        "<form1><SubformAntet><DenInstPb>INST</DenInstPb><cif>123</cif>" &
        "<SubtitluDF>Obiect</SubtitluDF></SubformAntet>" &
        "<SubformSectiuneaA><Subform123>" &
        "<DescrieObFundRevizuireScurt>Notă scurtă</DescrieObFundRevizuireScurt>" &
        "</Subform123><Subform4><Table1>" &
        "<Row1><Cell1 /></Row1>" &
        "<Row1><Cell1>Burse</Cell1><Cell3>cod1</Cell3><Cell5>0.00</Cell5><Cell6>129705.00</Cell6></Row1>" &
        "<Row1><Cell1>Salarii</Cell1><Cell3>cod2</Cell3><Cell5>1.00</Cell5><Cell6>2.00</Cell6></Row1>" &
        "</Table1></Subform4></SubformSectiuneaA></form1>"

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub

    Private Shared Function PanelVisible(view As Control, name As String) As Boolean
        Dim c As Control = FindByName(view, name)
        Return c IsNot Nothing AndAlso c.Visible
    End Function

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function GridOf(view As Control) As KBotDataView
        Return DirectCast(FindByName(view, "grid"), KBotDataView)
    End Function

    <Fact>
    Public Sub Factory_Default_IsXfaXmlPreview()
        RunSta(Sub()
                   Dim p = DdfPreviewFactory.Create()
                   Assert.IsType(Of XfaXmlPreview)(p)
               End Sub)
    End Sub

    <Fact>
    Public Sub ShowDocument_Missing_ShowsGenerateSurface()
        RunSta(Sub()
                   Using p As New XfaXmlPreview()
                       p.ShowDocument("C:\nu\exista\DDF_NR_3_REV_0_A100.PDF", exists:=False)
                       Assert.True(PanelVisible(p, "pnlMissing"))
                       Assert.False(PanelVisible(p, "pnlContent"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ShowDocument_BlankPath_ShowsMessage()
        RunSta(Sub()
                   Using p As New XfaXmlPreview()
                       p.ShowDocument("", exists:=True)
                       Assert.True(PanelVisible(p, "lblMessage"))
                       Assert.False(PanelVisible(p, "pnlContent"))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ShowDocument_WithSiblingXml_RendersHeaderAndLines()
        RunSta(Sub()
                   Dim dir As String = Path.Combine(Path.GetTempPath(), "kbot_ddf_" & Guid.NewGuid().ToString("N"))
                   Directory.CreateDirectory(dir)
                   Dim pdfPath As String = Path.Combine(dir, "DDF_NR_3_REV_0_A100.PDF")
                   Dim xmlPath As String = Path.ChangeExtension(pdfPath, ".xml")
                   File.WriteAllText(xmlPath, SampleForm1)
                   ' „exists=True" spune vederii că PDF-ul e pe disc; randorul citește siblingul
                   ' .xml (PDF-ul fizic nu trebuie să existe pentru calea siblingului).
                   Try
                       Using p As New XfaXmlPreview()
                           p.ShowDocument(pdfPath, exists:=True)
                           Assert.True(PanelVisible(p, "pnlContent"))
                           Assert.False(PanelVisible(p, "pnlMissing"))
                           Dim g = GridOf(p)
                           ' Rândul fictiv sărit -> exact două linii reale.
                           Assert.Equal(2, g.RowCount)
                       End Using
                   Finally
                       Directory.Delete(dir, True)
                   End Try
               End Sub)
    End Sub

    <Fact>
    Public Sub ShowDocument_ExistsButNoReadableXml_ShowsMessage()
        RunSta(Sub()
                   Dim dir As String = Path.Combine(Path.GetTempPath(), "kbot_ddf_" & Guid.NewGuid().ToString("N"))
                   Directory.CreateDirectory(dir)
                   ' Un „PDF" care nu e nici XML sibling, nici PDF cu XFA embedat -> mesaj, nu excepție.
                   Dim pdfPath As String = Path.Combine(dir, "DDF_NR_1_REV_0_B.PDF")
                   File.WriteAllText(pdfPath, "not a real pdf")
                   Try
                       Using p As New XfaXmlPreview()
                           p.ShowDocument(pdfPath, exists:=True)
                           Assert.True(PanelVisible(p, "lblMessage"))
                           Assert.False(PanelVisible(p, "pnlContent"))
                       End Using
                   Finally
                       Directory.Delete(dir, True)
                   End Try
               End Sub)
    End Sub

    <Fact>
    Public Sub GenerateButton_RaisesGenerateRequested()
        RunSta(Sub()
                   Using p As New XfaXmlPreview()
                       ' Butonul trăiește pe suprafața „document lipsă"; îl facem vizibil ca
                       ' PerformClick (care cere CanSelect = vizibil + activat) să ridice Click.
                       p.ShowDocument("x.pdf", exists:=False)
                       Dim raised As Boolean = False
                       AddHandler p.GenerateRequested, Sub(s, e) raised = True
                       Dim btn = DirectCast(FindByName(p, "btnGenereaza"), Button)
                       btn.PerformClick()
                       Assert.True(raised)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Clear_ReturnsToMessageState()
        RunSta(Sub()
                   Using p As New XfaXmlPreview()
                       p.ShowDocument("x.pdf", exists:=False)
                       Assert.True(PanelVisible(p, "pnlMissing"))
                       p.Clear()
                       Assert.True(PanelVisible(p, "lblMessage"))
                       Assert.False(PanelVisible(p, "pnlMissing"))
                   End Using
               End Sub)
    End Sub

End Class
