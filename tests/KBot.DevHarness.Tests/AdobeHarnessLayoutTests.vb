Option Strict On
Imports System
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.DevHarness
Imports Xunit

' Layout regression guard for AdobeReaderHarnessForm (slice 0023).
'
' WHY THIS EXISTS: the options panel shipped as a TableLayoutPanel with AutoScroll plus a Percent
' filler row. A Percent row absorbs whatever space is left, so the table always reported that its
' content fitted, never showed a scrollbar, and silently CLIPPED every section past the fold —
' «Preferințe Adobe (HKCU)» and «Politici Adobe (HKLM)» were unreachable on screen. Nothing in the
' build or the other 475 tests noticed. These tests assert what the operator actually needs: the
' panel scrolls, and every section (the two registry ones especially) can be scrolled to.
'
' Everything runs on a dedicated STA thread — same pattern as SumarViewTests/HarnessTestsRunTest.
Public Class AdobeHarnessLayoutTests

    ' Ruleaza corpul testului pe un fir STA si propaga orice esec inapoi.
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

    ' Builds the form and forces a real layout pass without launching Adobe (the ctor only probes
    ' the registry for the Adobe path; no process is started until a PDF is chosen).
    Private Shared Function NewForm() As AdobeReaderHarnessForm
        Dim f As New AdobeReaderHarnessForm(Sub(m)
                                                ' log sink: discarded
                                            End Sub)
        f.StartPosition = FormStartPosition.Manual
        f.Location = New Point(0, 0)
        f.Show()
        Application.DoEvents()
        f.PerformLayout()
        Application.DoEvents()
        Return f
    End Function

    Private Shared Function OptionsPanel(f As AdobeReaderHarnessForm) As FlowLayoutPanel
        Return f.Controls.Find("flowOptions", searchAllChildren:=True).
                 OfType(Of FlowLayoutPanel)().Single()
    End Function

    Private Shared Function SectionNamed(panel As FlowLayoutPanel, name As String) As GroupBox
        Return panel.Controls.OfType(Of GroupBox)().Single(Function(g) g.Name = name)
    End Function

    <Fact>
    Public Sub OptionsPanel_IsAScrollingFlowPanel_NotATableLayoutPanel()
        ' A TableLayoutPanel here is the exact shape that produced the clipping defect.
        RunSta(Sub()
                   Using f = NewForm()
                       Dim panel = OptionsPanel(f)
                       Assert.True(panel.AutoScroll, "flowOptions trebuie să aibă AutoScroll")
                       Assert.Equal(FlowDirection.TopDown, panel.FlowDirection)
                       Assert.False(panel.WrapContents, "secțiunile nu au voie să se împacheteze pe coloane")
                       Assert.IsNotType(Of TableLayoutPanel)(panel)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AllElevenSections_ArePresent_InOrder()
        RunSta(Sub()
                   Using f = NewForm()
                       Dim names = OptionsPanel(f).Controls.OfType(Of GroupBox)().
                                     Select(Function(g) g.Name).ToArray()
                       Assert.Equal(New String() {
                           "grpLaunch", "grpChrome", "grpFile", "grpProbe", "grpScenario",
                           "grpClip", "grpChildren", "grpKeys", "grpUser", "grpMachine", "grpCmd"},
                           names)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ContentIsTallerThanThePanel_SoAVerticalScrollbarMustExist()
        ' If this ever fails because the content got shorter, fine. If it fails because the
        ' scrollbar vanished while the content is still tall, the clipping defect is back.
        RunSta(Sub()
                   Using f = NewForm()
                       Dim panel = OptionsPanel(f)
                       Dim contentBottom As Integer =
                           panel.Controls.OfType(Of Control)().Max(Function(c) c.Bottom)
                       Assert.True(contentBottom > panel.ClientSize.Height,
                                   $"conținutul ({contentBottom}px) ar trebui să depășească panoul ({panel.ClientSize.Height}px)")
                       Assert.True(panel.VerticalScroll.Visible,
                                   "panoul de opțiuni TREBUIE să aibă bară de derulare verticală")
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub BothRegistrySections_CanBeScrolledTo()
        ' The two sections the operator could not reach. Scroll to the bottom and require that each
        ' one lands fully inside the visible client area.
        RunSta(Sub()
                   Using f = NewForm()
                       Dim panel = OptionsPanel(f)
                       For Each name As String In New String() {"grpUser", "grpMachine"}
                           Dim section = SectionNamed(panel, name)
                           panel.ScrollControlIntoView(section)
                           Application.DoEvents()
                           ' Top/Bottom are already scroll-relative after ScrollControlIntoView.
                           Assert.True(section.Top >= 0 AndAlso section.Bottom <= panel.ClientSize.Height,
                                       $"«{name}» nu e complet vizibilă după derulare (top={section.Top}, bottom={section.Bottom}, client={panel.ClientSize.Height})")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RegistryControls_AreAllReachableInsideTheirSections()
        RunSta(Sub()
                   Using f = NewForm()
                       Dim panel = OptionsPanel(f)
                       Dim expected As String() = {
                           "cboHive", "chkExpandRhp", "chkRhpSticky", "chkRhpCollapsed",
                           "chkClassicViewer", "btnApplyUser", "btnRestoreUser", "chkRestoreOnClose",
                           "cboProduct", "chkSuppressUpsell", "chkDisableServices",
                           "btnApplyMachine", "btnRevertMachine"}
                       For Each name As String In expected
                           Dim hits = f.Controls.Find(name, searchAllChildren:=True)
                           Assert.True(hits.Length = 1, $"controlul «{name}» lipsește din formular")
                           Assert.True(hits(0).Width > 0 AndAlso hits(0).Height > 0,
                                       $"controlul «{name}» are dimensiune nulă (secțiune colapsată)")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SectionsTrackThePanelWidth_AndDoNotForceHorizontalScrolling()
        ' The sections are width-tracked in code (a FlowLayoutPanel ignores Dock on its children).
        ' If that handler breaks, captions get truncated and a horizontal scrollbar appears.
        RunSta(Sub()
                   Using f = NewForm()
                       Dim panel = OptionsPanel(f)
                       Dim usable As Integer = panel.ClientSize.Width - panel.Padding.Horizontal
                       For Each g In panel.Controls.OfType(Of GroupBox)()
                           Assert.True(g.Width <= usable,
                                       $"«{g.Name}» ({g.Width}px) depășește lățimea utilă ({usable}px)")
                           Assert.True(g.Width > usable \ 2,
                                       $"«{g.Name}» ({g.Width}px) e mult prea îngustă față de panou ({usable}px)")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SplitterIsWideEnoughForTheOptionCaptions()
        ' 320px truncated every caption at the operator's DPI; their own designer file measured
        ' chkNewInstance at 427 and chkDisableServices at 412.
        RunSta(Sub()
                   Using f = NewForm()
                       Dim split = f.Controls.Find("splitMain", searchAllChildren:=True).
                                     OfType(Of SplitContainer)().Single()
                       Assert.False(split.IsSplitterFixed, "splitter-ul trebuie să poată fi tras")
                       Dim widest As Integer = f.Controls.Find("chkNewInstance", searchAllChildren:=True).
                                                 Single().PreferredSize.Width
                       Assert.True(split.SplitterDistance >= widest,
                                   $"panoul stâng ({split.SplitterDistance}px) e mai îngust decât cea mai lată opțiune ({widest}px)")
                   End Using
               End Sub)
    End Sub

End Class
