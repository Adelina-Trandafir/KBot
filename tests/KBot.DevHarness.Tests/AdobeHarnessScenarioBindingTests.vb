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
    Public Sub Loading_SetsOnlyTheRegistryRowsTheScenarioNames()
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Assert.Equal("1", Ctl(Of ComboBox)(f, "cboRhpSticky").Text)
                       ' Untouched is NOT «0» — a value the file is silent about stays alone.
                       Assert.Equal(PrefRowSelection.Untouched, Ctl(Of ComboBox)(f, "cboExpandRhp").Text)
                       Assert.Equal(PrefRowSelection.Untouched, Ctl(Of ComboBox)(f, "cboRhpViewMode").Text)
                       Assert.Equal(PrefRowSelection.Untouched, Ctl(Of ComboBox)(f, "cboEnableAv2").Text)
                       Assert.False(Ctl(Of CheckBox)(f, "chkRestoreOnClose").Checked)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_ShowsTheScenarioValueVerbatim_EvenWhenItIsOne()
        ' A file asking for bEnableAv2 = 1 must show 1 in the row. The old panel had a
        ' «bEnableAv2 = 0» checkbox, so 1 was inexpressible and the write was clamped on 04.08.
        Const modernUi As String = "{
  ""schema"": 1,
  ""userPrefs"": { ""values"": { ""bEnableAv2"": 1 } },
  ""scenario"": [ ""applyUserPrefs"" ]
}"
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, modernUi)
                       Assert.Equal("1", Ctl(Of ComboBox)(f, "cboEnableAv2").Text)

                       ' …and the grid shows what was really asked for.
                       Dim grid = Ctl(Of DataGridView)(f, "gridPrefs")
                       Dim row = grid.Rows.Cast(Of DataGridViewRow)().
                                 Single(Function(r) CStr(r.Cells(0).Value) = "bEnableAv2")
                       Assert.Equal("1", CStr(row.Cells(1).Value))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_ShowsAZeroAsAZero_NotAsUntouched()
        Const classicUi As String = "{
  ""schema"": 1,
  ""userPrefs"": { ""values"": { ""bEnableAv2"": 0 } },
  ""scenario"": [ ""applyUserPrefs"" ]
}"
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, classicUi)
                       Assert.Equal("0", Ctl(Of ComboBox)(f, "cboEnableAv2").Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_ShowsANullAsADeletion()
        ' JSON null is the third state the checkboxes could never express.
        Const wipe As String = "{
  ""schema"": 1,
  ""userPrefs"": { ""values"": { ""bRHPSticky"": null } },
  ""scenario"": [ ""applyUserPrefs"" ]
}"
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, wipe)
                       Assert.Equal(PrefRowSelection.DeleteText, Ctl(Of ComboBox)(f, "cboRhpSticky").Text)
                       Dim grid = Ctl(Of DataGridView)(f, "gridPrefs")
                       Dim row = grid.Rows.Cast(Of DataGridViewRow)().
                                 Single(Function(r) CStr(r.Cells(0).Value) = "bRHPSticky")
                       Assert.Equal("(șters)", CStr(row.Cells(1).Value))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_ShowsAStringValueVerbatim_IncludingOneThePanelNeverListed()
        Const odd As String = "{
  ""schema"": 1,
  ""userPrefs"": { ""values"": { ""aDefaultRHPViewMode_L"": ""Docked"" } },
  ""scenario"": [ ""applyUserPrefs"" ]
}"
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, odd)
                       Assert.Equal("Docked", Ctl(Of ComboBox)(f, "cboRhpViewMode").Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_AScenarioSilentAboutHkcu_ClearsRowsLeftByThePreviousOne()
        Const silent As String = "{ ""schema"": 1, ""scenario"": [ ""launch"" ] }"
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       Assert.Equal("1", Ctl(Of ComboBox)(f, "cboRhpSticky").Text)
                       LoadScenario(f, silent)
                       Assert.Equal(PrefRowSelection.Untouched, Ctl(Of ComboBox)(f, "cboRhpSticky").Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_FillsTheMoveSpinnersFromTheScenario()
        Const move As String = "{
  ""schema"": 1,
  ""move"": { ""dx"": -120, ""dy"": -90, ""dw"": 120, ""dh"": 90 },
  ""scenario"": [ ""applyMove"" ]
}"
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, move)
                       Assert.Equal(-120D, Ctl(Of NumericUpDown)(f, "numDx").Value)
                       Assert.Equal(-90D, Ctl(Of NumericUpDown)(f, "numDy").Value)
                       Assert.Equal(120D, Ctl(Of NumericUpDown)(f, "numDw").Value)
                       Assert.Equal(90D, Ctl(Of NumericUpDown)(f, "numDh").Value)
                       ' A non-zero delta arriving from a FILE must enable «Readu la zero» exactly
                       ' as one typed into the spinner does — caught by rendering the panel.
                       Assert.True(Ctl(Of Button)(f, "btnResetMove").Enabled)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Loading_AScenarioWithoutAMoveSection_LeavesTheSpinnersAtZero()
        RunSta(Sub()
                   Using f = NewForm()
                       LoadScenario(f, SettingsJson)
                       For Each name As String In New String() {"numDx", "numDy", "numDw", "numDh"}
                           Assert.Equal(0D, Ctl(Of NumericUpDown)(f, name).Value)
                       Next
                       Assert.False(Ctl(Of Button)(f, "btnResetMove").Enabled)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnUntouchedPanel_AsksForNothing()
        ' The default state must write NOTHING — the grid is empty until something is requested.
        RunSta(Sub()
                   Using f = NewForm()
                       Assert.Empty(Ctl(Of DataGridView)(f, "gridPrefs").Rows.Cast(Of DataGridViewRow)())
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
