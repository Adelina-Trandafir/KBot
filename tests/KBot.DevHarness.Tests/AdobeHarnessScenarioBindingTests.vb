Option Strict On
Imports System
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.DevHarness
Imports Xunit

' What loading a scenario must actually DO (slice 0023, operator clarification):
' a scenario carries SETTINGS, never a document. Loading one PRE-SETS the controls in the left
' panel; the PDF is chosen separately with «Deschide PDF…» and then opens under those settings.
' These tests drive the real form through its private load path and check the controls afterwards.
Public Class AdobeHarnessScenarioBindingTests

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

    Private Shared Function NewForm() As AdobeReaderHarnessForm
        Dim f As New AdobeReaderHarnessForm(Sub(m)
                                            End Sub)
        f.StartPosition = FormStartPosition.Manual
        f.Location = New Point(0, 0)
        f.Show()
        Application.DoEvents()
        Return f
    End Function

    ' Drives the form's own load path (private by design — nothing outside the bench loads files).
    Private Shared Sub LoadScenario(f As AdobeReaderHarnessForm, json As String)
        ' NOT named `path` — VB identifiers are case-insensitive, so it would shadow System.IO.Path.
        Dim filePath As String = Path.Combine(Path.GetTempPath(),
                                              "kbot_scn_" & Guid.NewGuid().ToString("N") & ".json")
        File.WriteAllText(filePath, json)
        Try
            Dim m As MethodInfo = GetType(AdobeReaderHarnessForm).GetMethod(
                "LoadScenarioFile", BindingFlags.NonPublic Or BindingFlags.Instance)
            Assert.NotNull(m)
            m.Invoke(f, New Object() {filePath})
            Application.DoEvents()
        Finally
            Try
                File.Delete(filePath)
            Catch
            End Try
        End Try
    End Sub

    Private Shared Function Ctl(Of T As Control)(f As Form, name As String) As T
        Return DirectCast(f.Controls.Find(name, searchAllChildren:=True).Single(), T)
    End Function

    Private Const SettingsJson As String = "{
  ""schema"": 1,
  ""name"": ""test"",
  ""launch"": { ""newInstance"": false, ""noSplash"": false },
  ""openParameters"": { ""toolbar"": 0, ""navpanes"": 1 },
  ""clip"": { ""enabled"": true, ""right"": 250, ""top"": 30 },
  ""userPrefs"": { ""values"": { ""bRHPSticky"": 1 }, ""restoreOnClose"": false },
  ""scenario"": [ ""launch"" ]
}"

    <Fact>
    Public Sub Loading_SetsTheLaunchAndChromeSwitchesInThePanel()
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Assert.False(Ctl(Of CheckBox)(f, "chkNewInstance").Checked)
                       Assert.False(Ctl(Of CheckBox)(f, "chkNoSplash").Checked)
                       ' toolbar=0 means "hide it" -> the box is ticked; navpanes=1 means leave it.
                       Assert.True(Ctl(Of CheckBox)(f, "chkToolbar").Checked)
                       Assert.False(Ctl(Of CheckBox)(f, "chkNavpanes").Checked)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_SetsTheClipSpinners()
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Assert.True(Ctl(Of CheckBox)(f, "chkClip").Checked)
                       Assert.Equal(250D, Ctl(Of NumericUpDown)(f, "numClipRight").Value)
                       Assert.Equal(30D, Ctl(Of NumericUpDown)(f, "numClipTop").Value)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_TicksOnlyTheRegistryValuesTheScenarioNames()
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Assert.True(Ctl(Of CheckBox)(f, "chkRhpSticky").Checked)
                       Assert.False(Ctl(Of CheckBox)(f, "chkExpandRhp").Checked)
                       Assert.False(Ctl(Of CheckBox)(f, "chkRhpCollapsed").Checked)
                       Assert.False(Ctl(Of CheckBox)(f, "chkClassicViewer").Checked)
                       Assert.False(Ctl(Of CheckBox)(f, "chkRestoreOnClose").Checked)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_DoesNotTouchTheDocument()
        ' The whole point: the PDF comes from «Deschide PDF…», never from the scenario.
        RunSta(Sub()
                   Using f = NewForm()
                       Dim before As String = Ctl(Of Label)(f, "lblFile").Text
                       LoadScenario(f, SettingsJson)
                       Assert.Equal(before, Ctl(Of Label)(f, "lblFile").Text)
                       Assert.Contains("alege un PDF", Ctl(Of TextBox)(f, "txtCmd").Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub CommandPreview_ReflectsTheScenarioSwitches_AfterLoading()
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Dim cmd As String = Ctl(Of TextBox)(f, "txtCmd").Text
                       Assert.Contains("toolbar=0", cmd)          ' ticked by the scenario
                       Assert.DoesNotContain("navpanes=0", cmd)   ' navpanes:1 left it unticked
                       Assert.DoesNotContain("/n", cmd)           ' newInstance:false
                       Assert.DoesNotContain("/s", cmd)           ' noSplash:false
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AfterLoading_ThePanelIsTheSourceOfTruth_NotTheScenario()
        ' The operator can still adjust after loading, and the adjustment must win.
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Assert.Contains("toolbar=0", Ctl(Of TextBox)(f, "txtCmd").Text)

                       Ctl(Of CheckBox)(f, "chkToolbar").Checked = False
                       Ctl(Of CheckBox)(f, "chkNavpanes").Checked = True
                       Application.DoEvents()

                       Dim cmd As String = Ctl(Of TextBox)(f, "txtCmd").Text
                       Assert.DoesNotContain("toolbar=0", cmd)
                       Assert.Contains("navpanes=0", cmd)
                   End Using
               End Sub)
    End Sub

End Class
