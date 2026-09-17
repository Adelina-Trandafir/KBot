Option Strict On
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0062: a themed form never ends up smaller than its base and never smaller than what its
''' themed content asks for -- and a second pass measures against the SAME base, so nothing
''' compounds. The pure rule (<see cref="ThemeFormFit.Fit"/>) is tested on numbers; the
''' capture/apply path on a real, never-shown form.
'''
''' AVACONT is redirected to a temp folder: writing <see cref="ThemeFormFit.Baseline"/> persists.
''' </summary>
Public Class ThemeFormFitTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_formfit_test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_tempRoot)
        ThemeStore.OverrideRootForTests = _tempRoot
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ThemeFormFit.Baseline = FormFitBaseline.Scaled
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        ThemeStore.OverrideRootForTests = Nothing
        Try
            If Directory.Exists(_tempRoot) Then Directory.Delete(_tempRoot, True)
        Catch
        End Try
    End Sub

    ' ── Fit: the pure rule ─────────────────────────────────────────────────────

    <Fact>
    Public Sub Fit_DemandBelowBaseline_ReturnsBaseline()
        Assert.Equal(New Size(800, 600),
                     ThemeFormFit.Fit(New Size(800, 600), New Size(500, 400), Size.Empty, Size.Empty))
    End Sub

    <Fact>
    Public Sub Fit_DemandAboveBaseline_ReturnsDemand()
        Assert.Equal(New Size(900, 700),
                     ThemeFormFit.Fit(New Size(800, 600), New Size(900, 700), Size.Empty, Size.Empty))
    End Sub

    <Fact>
    Public Sub Fit_ResultBelowMinimum_ReturnsMinimum()
        Assert.Equal(New Size(1000, 800),
                     ThemeFormFit.Fit(New Size(800, 600), New Size(500, 400), New Size(1000, 800), Size.Empty))
    End Sub

    <Fact>
    Public Sub Fit_ResultAboveWorkArea_IsClampedToWorkArea()
        Assert.Equal(New Size(1920, 1040),
                     ThemeFormFit.Fit(New Size(800, 600), New Size(2500, 1500), Size.Empty, New Size(1920, 1040)))
    End Sub

    <Fact>
    Public Sub Fit_PerAxis_MixesTheRules()
        ' Width comes from demand, height from the baseline, then the height hits the minimum.
        Assert.Equal(New Size(900, 650),
                     ThemeFormFit.Fit(New Size(800, 600), New Size(900, 500), New Size(100, 650), New Size(1920, 1040)))
    End Sub

    <Fact>
    Public Sub Fit_IsIdempotent()
        ' The test that catches composition: feeding the result back as the demand changes nothing.
        Dim baseline As New Size(800, 600)
        Dim minimum As New Size(300, 200)
        Dim work As New Size(1920, 1040)
        For Each demand As Size In {New Size(500, 400), New Size(900, 700), New Size(2500, 1500)}
            Dim once As Size = ThemeFormFit.Fit(baseline, demand, minimum, work)
            Dim twice As Size = ThemeFormFit.Fit(baseline, once, minimum, work)
            Assert.Equal(once, twice)
        Next
    End Sub

    ' ── ScaledBaseline: what the base means when the scale moves ───────────────

    <Fact>
    Public Sub ScaledBaseline_Scaled_DoublesWithTheFactor()
        Assert.Equal(New Size(1600, 1200),
                     ThemeFormFit.ScaledBaseline(New Size(800, 600), 1.0F, 2.0F, FormFitBaseline.Scaled))
        ' Captured at 1.5, now at 1.0: the base follows DOWN too.
        Assert.Equal(New Size(800, 600),
                     ThemeFormFit.ScaledBaseline(New Size(1200, 900), 1.5F, 1.0F, FormFitBaseline.Scaled))
    End Sub

    <Fact>
    Public Sub ScaledBaseline_DesignerRaw_IgnoresTheFactor()
        Assert.Equal(New Size(800, 600),
                     ThemeFormFit.ScaledBaseline(New Size(800, 600), 1.0F, 2.0F, FormFitBaseline.DesignerRaw))
    End Sub

    <Fact>
    Public Sub ScaledBaseline_AbsurdCaptureScale_CountsAsOne()
        Assert.Equal(New Size(1600, 1200),
                     ThemeFormFit.ScaledBaseline(New Size(800, 600), 0F, 2.0F, FormFitBaseline.Scaled))
    End Sub

    ' ── Baseline setting: persistence and the pre-slice default ────────────────

    <Fact>
    Public Sub Baseline_RoundTripsThroughThemeJson_AndKeepsTheActiveScheme()
        ThemeStore.SaveActive("Modern")
        ThemeFormFit.Baseline = FormFitBaseline.DesignerRaw
        Assert.Equal(FormFitBaseline.DesignerRaw, ThemeStore.LoadFormFitBaseline())
        Assert.Equal("Modern", ThemeStore.LoadActiveName())

        ' Writing the scheme afterwards must not lose the key either (read-modify-write).
        ThemeStore.SaveActive("Dark")
        Assert.Equal(FormFitBaseline.DesignerRaw, ThemeStore.LoadFormFitBaseline())
    End Sub

    <Fact>
    Public Sub Baseline_MissingKey_DefaultsToScaled()
        ' A theme.json written before the slice has no key; it must land on Scaled.
        Directory.CreateDirectory(ThemeStore.AppDataFolder)
        File.WriteAllText(ThemeStore.ActiveFilePath, "{""activeScheme"":""Classic"",""textScale"":1.25}")
        Assert.Equal(FormFitBaseline.Scaled, ThemeStore.LoadFormFitBaseline())
    End Sub

    <Fact>
    Public Sub Baseline_UnknownValue_Throws()
        Assert.Throws(Of ArgumentException)(Sub() ThemeFormFit.Baseline = CType(7, FormFitBaseline))
    End Sub

    ' ── Capture + Apply on a real form (never shown) ───────────────────────────

    ''' <summary>A themed form with one Fill panel; the panel's preferred size is the "content".</summary>
    Private NotInheritable Class ProbeForm
        Inherits KBotThemedForm

        Public ReadOnly Root As New DemandingPanel() With {.Dock = DockStyle.Fill}
        Public ReadOnly Band As New Panel() With {.Dock = DockStyle.Top, .Height = 40}

        Public Sub New()
            SuspendLayout()
            Controls.Add(Root)
            Controls.Add(Band)
            AutoScaleMode = AutoScaleMode.None
            FormBorderStyle = FormBorderStyle.None
            StartPosition = FormStartPosition.Manual
            ClientSize = New Size(400, 300)
            ResumeLayout(True)
        End Sub

        Public Function FitRootForTest() As Control
            Return FitRoot
        End Function
    End Class

    ' A panel whose preferred size is whatever the test says -- the stand-in for "themed content".
    Private NotInheritable Class DemandingPanel
        Inherits Panel
        Public Wants As Size
        Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
            Return Wants
        End Function
    End Class

    <Fact>
    Public Sub Capture_StoresTheBase_Once()
        Using f As New ProbeForm()
            Assert.False(ThemeFormFit.HasSnapshot(f))
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            Assert.True(ThemeFormFit.HasSnapshot(f))
            Assert.Equal(New Size(400, 300), ThemeFormFit.CapturedClientSize(f))

            ' A later size change does not move the base.
            f.ClientSize = New Size(700, 500)
            ThemeFormFit.Capture(f)
            Assert.Equal(New Size(400, 300), ThemeFormFit.CapturedClientSize(f))
        End Using
    End Sub

    ''' <summary>
    ''' The automatic hook. The form is shown off-screen for the shortest possible moment (a
    ''' snapshot needs the Load path; nothing else does). What it must NOT capture is the
    ''' transient client size a borderless window reports at OnHandleCreated (400x300 reads
    ''' as 378x244 there) -- the reason the hook is OnCreateControl and not OnHandleCreated.
    ''' </summary>
    <Fact>
    Public Sub ShowingTheForm_CapturesTheRealBase_BeforeLoad()
        Using f As New ProbeForm()
            f.ShowInTaskbar = False
            f.Location = New Point(-32000, -32000)
            f.Show()
            Try
                Assert.True(ThemeFormFit.HasSnapshot(f))
                Assert.Equal(New Size(400, 300), ThemeFormFit.CapturedClientSize(f))
            Finally
                f.Hide()
            End Try
        End Using
    End Sub

    <Fact>
    Public Sub Apply_WithoutCapture_Throws()
        Using f As New Form() With {.ClientSize = New Size(400, 300)}
            Dim root As New Panel() With {.Dock = DockStyle.Fill}
            f.Controls.Add(root)
            Assert.Throws(Of InvalidOperationException)(Sub() ThemeFormFit.Apply(f, root))
        End Using
    End Sub

    <Fact>
    Public Sub Apply_GrowsToTheContent_AndCountsTheBandsAroundTheRoot()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            ' The root asks for 500x350 of its own; around it sits a 40px band, which must be
            ' added on top -- the content does not shrink to make room for the band.
            f.Root.Wants = New Size(500, 350)
            Assert.True(ThemeFormFit.Apply(f, f.Root))
            Assert.Equal(New Size(500, 350 + f.Band.Height), f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_ShrunkBelowTheBase_ComesBackToTheBase()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            f.ClientSize = New Size(250, 180)   ' what a smaller scheme font would leave behind
            Assert.True(ThemeFormFit.Apply(f, f.Root))
            Assert.Equal(New Size(400, 300), f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_SecondPass_DoesNotCompound()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            f.Root.Wants = New Size(500, 350)

            ThemeFormFit.Apply(f, f.Root)
            Dim once As Size = f.ClientSize
            Assert.False(ThemeFormFit.Apply(f, f.Root))
            Assert.Equal(once, f.ClientSize)
            Assert.False(ThemeFormFit.Apply(f, f.Root))
            Assert.Equal(once, f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_NeverShrinksAnOperatorEnlargedWindow()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            f.ClientSize = New Size(700, 500)   ' the operator dragged it larger
            Assert.False(ThemeFormFit.Apply(f, f.Root))
            Assert.Equal(New Size(700, 500), f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_MinimumSizeWins_OverBaseAndDemand()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            f.MinimumSize = New Size(600, 450)   ' borderless: minimum is in client pixels too
            ThemeFormFit.Apply(f, f.Root)
            Assert.Equal(New Size(600, 450), f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_ScaledBaseline_FollowsTheManualFactor_DesignerRawDoesNot()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            Assert.Equal(1.0F, ThemeFormFit.CapturedScale(f), 3)

            ' Scale doubles (manual factor, so it does not depend on the machine's DPI).
            AppScaling.LoadFrom(ScalingMode.Manual, 2.0F, False, 1.0F)

            ThemeFormFit.Baseline = FormFitBaseline.DesignerRaw
            ThemeFormFit.Apply(f, f.Root)
            Assert.Equal(New Size(400, 300), f.ClientSize)

            ThemeFormFit.Baseline = FormFitBaseline.Scaled
            ThemeFormFit.Apply(f, f.Root)
            Assert.Equal(New Size(800, 600), f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Capture_StoresTheScreenFactorOnly_SoTextScaleCannotShrinkTheBase()
        ' The text size is applied by the theme AFTER the capture, so it is not in the captured
        ' client size; captured at 125% text and asked at 100%, the base must be the captured
        ' size itself -- not captured / 1.25.
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.25F)
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            Assert.Equal(1.0F, ThemeFormFit.CapturedScale(f), 3)

            AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
            f.ClientSize = New Size(320, 240)   ' what the platform leaves after the font shrinks
            ThemeFormFit.Apply(f, f.Root)
            Assert.Equal(New Size(400, 300), f.ClientSize)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_LargerThanTheScreen_IsClampedNotThrown()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            f.Root.Wants = New Size(20000, 20000)
            ThemeFormFit.Apply(f, f.Root)
            Dim area As Rectangle = Screen.FromHandle(h).WorkingArea
            Assert.True(f.ClientSize.Width <= area.Width)
            Assert.True(f.ClientSize.Height <= area.Height)
        End Using
    End Sub

    <Fact>
    Public Sub Apply_NotNormalWindow_DoesNothing()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            f.WindowState = FormWindowState.Maximized
            Assert.False(ThemeFormFit.Apply(f, f.Root))
        End Using
    End Sub

    <Fact>
    Public Sub Forget_DropsTheSnapshot()
        Using f As New ProbeForm()
            Dim h As IntPtr = f.Handle : ThemeFormFit.Capture(f)
            ThemeFormFit.Forget(f)
            Assert.False(ThemeFormFit.HasSnapshot(f))
        End Using
    End Sub

    ' ── ContentDemand: measuring through panels ────────────────────────────────

    <Fact>
    Public Sub ContentDemand_WalksThePanel_BandsPlusFillPlusPadding()
        ' A plain Panel's GetPreferredSize ignores a non-AutoSize child (measured: padding only).
        ' The walk must see the band, the Fill child's content and the padding.
        Using card As New Panel() With {.Padding = New Padding(12), .Size = New Size(100, 100)}
            card.Controls.Add(New DemandingPanel() With {.Dock = DockStyle.Fill, .Wants = New Size(500, 350)})
            card.Controls.Add(New Panel() With {.Dock = DockStyle.Top, .Height = 40})
            Dim d As Size = ThemeFormFit.ContentDemand(card)
            Assert.Equal(New Size(500 + 24, 40 + 350 + 24), d)
        End Using
    End Sub

    <Fact>
    Public Sub ContentDemand_IgnoresChildrenAnchoredToTheFarEdge_CountsFixedOnes()
        Using card As New Panel() With {.Size = New Size(100, 100)}
            ' Follows the container: says nothing.
            card.Controls.Add(New Panel() With {.Bounds = New Rectangle(0, 0, 900, 900),
                                                .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom})
            ' Authored position: its extent counts.
            card.Controls.Add(New Panel() With {.Bounds = New Rectangle(10, 20, 200, 100),
                                                .Anchor = AnchorStyles.Top Or AnchorStyles.Left})
            Assert.Equal(New Size(210, 120), ThemeFormFit.ContentDemand(card))
        End Using
    End Sub

    <Fact>
    Public Sub ContentDemand_LeafAnswersForItself_ContainerNeverEchoesItsBounds()
        Using leaf As New DemandingPanel() With {.Size = New Size(900, 900), .Wants = New Size(30, 20)}
            Assert.Equal(New Size(30, 20), ThemeFormFit.ContentDemand(leaf))
        End Using
        Using empty As New Panel() With {.Size = New Size(900, 900)}
            Assert.Equal(Size.Empty, ThemeFormFit.ContentDemand(empty))
        End Using
    End Sub

    ' ── FitRoot: the default and its refusal to guess ──────────────────────────

    <Fact>
    Public Sub FitRoot_Default_IsTheSingleFillChild()
        Using f As New ProbeForm()
            Assert.Same(f.Root, f.FitRootForTest())
        End Using
    End Sub

    <Fact>
    Public Sub FitRoot_TwoFillChildren_Throws()
        Using f As New ProbeForm()
            f.Controls.Add(New Panel() With {.Dock = DockStyle.Fill})
            Assert.Throws(Of InvalidOperationException)(Function() f.FitRootForTest())
        End Using
    End Sub

    <Fact>
    Public Sub FitRoot_NoFillChild_Throws()
        Using f As New ProbeForm()
            f.Root.Dock = DockStyle.None
            Assert.Throws(Of InvalidOperationException)(Function() f.FitRootForTest())
        End Using
    End Sub

    <Fact>
    Public Sub Switches_DefaultTrue_AndNotSerializedWhenUntouched()
        Using f As New ProbeForm()
            Assert.True(f.CenterOnScreen)
            Assert.True(f.AutoFitToTheme)
            Dim props = System.ComponentModel.TypeDescriptor.GetProperties(f)
            Assert.False(props(NameOf(KBotThemedForm.CenterOnScreen)).ShouldSerializeValue(f))
            Assert.False(props(NameOf(KBotThemedForm.AutoFitToTheme)).ShouldSerializeValue(f))
            f.AutoFitToTheme = False
            Assert.True(props(NameOf(KBotThemedForm.AutoFitToTheme)).ShouldSerializeValue(f))
        End Using
    End Sub

End Class
