Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Controls
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0066 (over 0062): <see cref="KBotTableLayoutPanel"/> -- fixed rows, fixed columns and
''' the table's own padding are authored in LOGICAL pixels and computed for the screen from
''' <c>AppScaling.FactorFor</c>, never from a live value and never from the platform's factor.
'''
''' <para>The machine's DPI must not decide a test: <c>Fixed100</c> makes the scale exactly 1,
''' <c>Manual</c> makes it exactly the number given, whatever monitor the runner sits on. The
''' few tests that stay on <c>Automatic</c> express their expectations through
''' <c>DpiScale</c>. <c>AppScaling</c> is process state; every test puts it back.</para>
''' </summary>
Public Class KBotTableLayoutPanelTests
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
    End Sub

    ' Minimal site with DesignMode = True -- the one signal KBotDesignTime reads here.
    Private NotInheritable Class FakeDesignSite
        Implements ISite
        Public Property Name As String Implements ISite.Name
        Public ReadOnly Property Component As IComponent Implements ISite.Component
            Get
                Return Nothing
            End Get
        End Property
        Public ReadOnly Property Container As IContainer Implements ISite.Container
            Get
                Return Nothing
            End Get
        End Property
        Public ReadOnly Property DesignMode As Boolean Implements ISite.DesignMode
            Get
                Return True
            End Get
        End Property
        Public Function GetService(serviceType As Type) As Object Implements IServiceProvider.GetService
            Return Nothing
        End Function
    End Class

    ''' <summary>Two columns (80 Absolute, 100 Percent) x two rows (40 Absolute, 100 Percent), padding 8.</summary>
    Private Shared Function NewTable() As KBotTableLayoutPanel
        Dim t As New KBotTableLayoutPanel() With {.Width = 400, .Height = 300}
        t.ColumnCount = 2
        t.RowCount = 2
        t.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        t.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        t.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        t.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        t.Padding = New Padding(8)
        Return t
    End Function

    Private Shared Sub Manual(factor As Single)
        AppScaling.LoadFrom(ScalingMode.Manual, factor, False, 1.0F)
    End Sub

    ''' <summary>Scale exactly 1 on any monitor.</summary>
    Private Shared Sub Fixed()
        AppScaling.LoadFrom(ScalingMode.Fixed100, 1.0F, False, 1.0F)
    End Sub

    ' A leaf whose preferred size is whatever the test says.
    Private NotInheritable Class Wanting
        Inherits Control
        Public Wants As Size
        Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
            Return Wants
        End Function
    End Class

    ' ── Snapshot ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Snapshot_IsTakenOnce_AtTheFirstHook_AndIsLogical()
        Using t = NewTable()
            Fixed()
            Assert.False(t.HasStyleBaseline)
            t.ApplyTheme(BuiltInSchemes.Classic())
            Assert.True(t.HasStyleBaseline)
            Assert.Equal(80F, t.DebugAuthoredColumn(0), 2)
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)

            ' A computed value must never become the snapshot.
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
            Assert.Equal(80F, t.DebugAuthoredColumn(0), 2)
        End Using
    End Sub

    ' ── Scale ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Manual150_StylesAndPadding_AreLogicalTimes1_5()
        Using t = NewTable()
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Assert.Equal(1.5F, t.DpiScale, 3)
            Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
            Assert.Equal(60F, t.RowStyles(0).Height, 2)
            Assert.Equal(New Padding(8), t.Padding)          ' the public value stays logical
            Assert.Equal(New Padding(12), t.PaddingPx)       ' the layout engine reads the device value
            Assert.Equal(New Padding(12), DirectCast(t, Control).Padding)
        End Using
    End Sub

    <Fact>
    Public Sub Scale_RoundsToWholePixels()
        Using t = NewTable()
            Manual(1.25F)
            t.SetRowHeight(0, 33F)   ' 41.25 -> 41
            Assert.Equal(41F, t.RowStyles(0).Height, 2)
            Assert.Equal(New Padding(10), t.PaddingPx)
        End Using
    End Sub

    <Fact>
    Public Sub SecondRescale_DoesNotCompound()
        Using t = NewTable()
            Manual(1.5F)
            t.RefreshDpiMetrics()
            t.RefreshDpiMetrics()
            t.ApplyTheme(BuiltInSchemes.Dark())
            Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
            Assert.Equal(60F, t.RowStyles(0).Height, 2)
            Assert.Equal(New Padding(12), t.PaddingPx)
        End Using
    End Sub

    <Fact>
    Public Sub Fixed100_IsExactlyTheLogicalValue()
        Using t = NewTable()
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Fixed()
            t.RefreshDpiMetrics()
            Assert.Equal(80F, t.ColumnStyles(0).Width)
            Assert.Equal(40F, t.RowStyles(0).Height)
            Assert.Equal(New Padding(8), t.PaddingPx)
        End Using
    End Sub

    <Fact>
    Public Sub Automatic_IsDeviceDpiOver96_TheSameFactorTheTreeUses()
        Using t = NewTable()
            AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
            t.RefreshDpiMetrics()
            Dim k As Single = AppScaling.FactorFor(t)
            Assert.Equal(k, t.DpiScale, 3)
            Assert.Equal(CSng(Math.Round(80 * k)), t.ColumnStyles(0).Width, 2)
            Assert.Equal(CSng(Math.Round(40 * k)), t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub TextScale_IsPartOfTheFactor()
        Using t = NewTable()
            AppScaling.LoadFrom(ScalingMode.Fixed100, 1.0F, False, 1.25F)
            t.RefreshDpiMetrics()
            Assert.Equal(100F, t.ColumnStyles(0).Width, 2)
            Assert.Equal(50F, t.RowStyles(0).Height, 2)
            Assert.Equal(New Padding(10), t.PaddingPx)
        End Using
    End Sub

    <Fact>
    Public Sub ScaleAbsoluteStylesOff_KeepsTheLogicalPixels()
        Using t = NewTable()
            t.ScaleAbsoluteStyles = False
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Assert.Equal(80F, t.ColumnStyles(0).Width)
            Assert.Equal(40F, t.RowStyles(0).Height)
            Assert.Equal(New Padding(8), t.PaddingPx)
        End Using
    End Sub

    <Fact>
    Public Sub PercentAndAutoSizeStyles_AreNeverTouched()
        Using t = NewTable()
            t.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            t.RowCount = 3
            Manual(2.0F)
            t.RefreshDpiMetrics()
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(SizeType.Percent, t.ColumnStyles(1).SizeType)
            Assert.Equal(100F, t.ColumnStyles(1).Width)
            Assert.Equal(SizeType.Percent, t.RowStyles(1).SizeType)
            Assert.Equal(100F, t.RowStyles(1).Height)
            Assert.Equal(SizeType.AutoSize, t.RowStyles(2).SizeType)
        End Using
    End Sub

    <Fact>
    Public Sub DesignTime_NoScaling()
        Using t = NewTable()
            t.Site = New FakeDesignSite()
            Manual(1.5F)
            t.RefreshDpiMetrics()
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(80F, t.ColumnStyles(0).Width)
            Assert.Equal(40F, t.RowStyles(0).Height)
            Assert.Equal(New Padding(8), t.PaddingPx)
        End Using
    End Sub

    ''' <summary>
    ''' The platform's font autoscale multiplies the live styles and the padding by ITS factor
    ''' (the font ratio). It must never survive: after the pass the live values are the logical
    ''' ones times OUR factor. The snapshot taken at that first ScaleControl must be the authored
    ''' value, not the platform's product.
    ''' </summary>
    <Fact>
    Public Sub PlatformAutoscale_IsATriggerNotASource()
        Manual(1.5F)
        Using f As New Form() With {.AutoScaleMode = AutoScaleMode.Font, .Font = New Font("Segoe UI", 9.0F)}
            Using t = NewTable()
                t.Dock = DockStyle.Fill
                f.SuspendLayout()
                f.Controls.Add(t)
                ' A stamp taken on a smaller font than the current one: the platform WILL scale.
                f.AutoScaleDimensions = New SizeF(6.0F, 13.0F)
                f.ResumeLayout(True)
                f.PerformLayout()

                Assert.True(t.HasStyleBaseline)
                Assert.Equal(80F, t.DebugAuthoredColumn(0), 2)
                Assert.Equal(40F, t.DebugAuthoredRow(0), 2)
                Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
                Assert.Equal(60F, t.RowStyles(0).Height, 2)
                Assert.Equal(New Padding(12), t.PaddingPx)

                ' A font change scales again; the answer is the same function of the logical values.
                f.Font = New Font("Segoe UI", 13.5F)
                f.PerformLayout()
                Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
                Assert.Equal(60F, t.RowStyles(0).Height, 2)
                Assert.Equal(New Padding(12), t.PaddingPx)
            End Using
        End Using
    End Sub

    ' ── Fit to content ─────────────────────────────────────────────────────────

    <Fact>
    Public Sub FixedRow_GrowsBySurplus_AndReturnsToAuthored()
        Using t = NewTable()
            Fixed()
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 56), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)

            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(56F, t.RowStyles(0).Height, 2)     ' 40 authored + 16 surplus
            Assert.Equal(80F, t.ColumnStyles(0).Width, 2)   ' 50 fits in 80: no surplus

            b.Wants = New Size(50, 30)                       ' the scheme left; the surplus is gone
            t.ApplyTheme(BuiltInSchemes.Classic())
            Assert.Equal(40F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub Surplus_IsMeasuredOverTheScaledAuthoredMeasure()
        Using t = NewTable()
            Manual(1.5F)
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 70), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(70F, t.RowStyles(0).Height, 2)     ' 60 scaled + 10 surplus
            b.Wants = New Size(50, 30)
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(60F, t.RowStyles(0).Height, 2)     ' back to 40 x 1.5
        End Using
    End Sub

    <Fact>
    Public Sub FixedRow_SurplusIsMeasuredFromTheSnapshot_NotFromTheGrownValue()
        Using t = NewTable()
            Fixed()
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 56), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)
            t.ApplyTheme(BuiltInSchemes.Modern())
            t.ApplyTheme(BuiltInSchemes.Modern())
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(56F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub FillDockedContainer_DoesNotAskOnTheDockedAxis()
        Using t = NewTable()
            Fixed()
            ' A panel echoes its bounds: were it measured, the row would stick at whatever it once
            ' grew to. It must ask for nothing.
            Dim p As New Panel() With {.Dock = DockStyle.Fill, .Margin = Padding.Empty}
            t.Controls.Add(p, 0, 0)
            t.RowStyles(0).Height = 90F      ' written before any hook: this IS the authored value
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(90F, t.RowStyles(0).Height, 2)
            Assert.Equal(90F, t.DebugAuthoredRow(0), 2)
        End Using
    End Sub

    <Fact>
    Public Sub FillDockedButton_DoesNotEchoItsCell()
        ' Measured in 0066: a docked Button answers GetPreferredSize with its bounds. A row
        ' holding one must still come back to its authored measure.
        Using t = NewTable()
            Fixed()
            Dim b As New Button() With {.Dock = DockStyle.Fill, .Margin = Padding.Empty, .Text = "OK"}
            t.Controls.Add(b, 0, 0)
            t.RowStyles(0).Height = 90F
            t.ApplyTheme(BuiltInSchemes.Classic())
            Assert.Equal(90F, t.RowStyles(0).Height, 2)
            t.SetRowHeight(0, 40F)
            ' 40 plus what the button's text needs over 40 -- with a 9pt font that is nothing,
            ' and certainly not the 90 the cell had.
            Assert.True(t.RowStyles(0).Height < 90F, "row: " & t.RowStyles(0).Height)
        End Using
    End Sub

    <Fact>
    Public Sub AutoFitOff_NoSurplusIsAdded()
        Using t = NewTable()
            Fixed()
            t.AutoFitToTheme = False
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 56), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(40F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub GetPreferredSize_ReportsWhatADockedChildStillLacks()
        ' The 0030 failure: a 56px button in a 40px cell reported 40. Before any fit, the panel
        ' must already say it needs 16 more; after the fit, the base answer is enough.
        Using t = NewTable()
            Fixed()
            t.Padding = Padding.Empty
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 56), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)
            ' Column 0 (Absolute 80) holds 50: 80. Column 1 (Percent) is empty: 0.
            ' Row 0 (Absolute 40) holds 56: 56. Row 1 (Percent) is empty: 0. No gaps, no padding.
            Dim before As Size = t.GetPreferredSize(Size.Empty)
            Assert.Equal(New Size(80, 56), before)

            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(56F, t.RowStyles(0).Height, 2)
            Assert.Equal(before, t.GetPreferredSize(Size.Empty))
        End Using
    End Sub

    <Fact>
    Public Sub GetPreferredSize_IncludesTheDevicePadding_AndSkipsCollapsedLines()
        Using t = NewTable()
            Manual(2.0F)
            t.RefreshDpiMetrics()
            ' Padding 8 logical = 16 device on each side; column 0 = 160, row 0 = 80.
            Assert.Equal(New Size(160 + 32, 80 + 32), t.GetPreferredSize(Size.Empty))
            t.SetRowCollapsed(0, True)
            Assert.Equal(New Size(160 + 32, 32), t.GetPreferredSize(Size.Empty))
        End Using
    End Sub

    ' ── Runtime API ────────────────────────────────────────────────────────────

    <Fact>
    Public Sub SetRowCollapsed_WritesZero_AndSurvivesScaleAndTheme()
        Using t = NewTable()
            Manual(1.5F)
            t.SetRowCollapsed(0, True)
            Assert.True(t.IsRowCollapsed(0))
            Assert.Equal(0F, t.RowStyles(0).Height, 2)
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)     ' the authored measure is kept

            t.ApplyTheme(BuiltInSchemes.Modern())
            Manual(2.0F)
            t.RefreshDpiMetrics()
            Assert.Equal(0F, t.RowStyles(0).Height, 2)

            t.SetRowCollapsed(0, False)
            Assert.False(t.IsRowCollapsed(0))
            Assert.Equal(80F, t.RowStyles(0).Height, 2)     ' 40 x 2, at the scale in force NOW
        End Using
    End Sub

    <Fact>
    Public Sub SetColumnCollapsed_WritesZero_AndComesBack()
        Using t = NewTable()
            Manual(1.5F)
            t.SetColumnCollapsed(0, True)
            Assert.True(t.IsColumnCollapsed(0))
            Assert.Equal(0F, t.ColumnStyles(0).Width, 2)
            t.SetColumnCollapsed(0, False)
            Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
        End Using
    End Sub

    <Fact>
    Public Sub Collapse_RefusesAPercentLine_AndABadIndex()
        Using t = NewTable()
            Assert.Throws(Of ArgumentException)(Sub() t.SetRowCollapsed(1, True))
            Assert.Throws(Of ArgumentException)(Sub() t.SetColumnCollapsed(1, True))
            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() t.SetRowCollapsed(7, True))
            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() t.SetRowHeight(-1, 10F))
        End Using
    End Sub

    <Fact>
    Public Sub SetRowHeight_IsLogical_MakesTheRowAbsolute_AndLiftsACollapse()
        Using t = NewTable()
            Manual(1.5F)
            t.SetRowCollapsed(0, True)
            t.SetRowHeight(0, 60F)
            Assert.False(t.IsRowCollapsed(0))
            Assert.Equal(60F, t.DebugAuthoredRow(0), 2)
            Assert.Equal(90F, t.RowStyles(0).Height, 2)

            ' The Percent row becomes a fixed one.
            t.SetRowHeight(1, 20F)
            Assert.Equal(SizeType.Absolute, t.RowStyles(1).SizeType)
            Assert.Equal(30F, t.RowStyles(1).Height, 2)

            ' Negative is clamped to 0, not thrown (C3).
            t.SetColumnWidth(0, -5F)
            Assert.Equal(0F, t.ColumnStyles(0).Width, 2)
        End Using
    End Sub

    <Fact>
    Public Sub Padding_IsLogicalInAndDeviceOut_AndSurvivesTheScale()
        Using t = NewTable()
            Manual(2.0F)
            t.RefreshDpiMetrics()
            t.Padding = New Padding(3, 4, 5, 6)
            Assert.Equal(New Padding(3, 4, 5, 6), t.Padding)
            Assert.Equal(New Padding(6, 8, 10, 12), t.PaddingPx)
            Fixed()
            t.RefreshDpiMetrics()
            Assert.Equal(New Padding(3, 4, 5, 6), t.PaddingPx)
        End Using
    End Sub

    <Fact>
    Public Sub StylesAddedAtRuntime_AreReadFromTheirLiveValue_Unscaled()
        Using t = NewTable()
            Manual(2.0F)
            t.RefreshDpiMetrics()
            ' Code appends a fixed row and writes it in DEVICE pixels, like any live style.
            t.RowStyles.Add(New RowStyle(SizeType.Absolute, 100F))
            t.RowCount = 3
            t.RefreshDpiMetrics()
            Assert.Equal(50F, t.DebugAuthoredRow(2), 2)
            Assert.Equal(100F, t.RowStyles(2).Height, 2)
            ' The rows that were there keep their logical measure.
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)
            Assert.Equal(80F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    ' ── ResetStyleBaseline (the escape hatch) ──────────────────────────────────

    <Fact>
    Public Sub ResetStyleBaseline_AdoptsAnOutsideWrite_AsTheLogicalValue()
        Using t = NewTable()
            Manual(2.0F)
            t.ApplyTheme(BuiltInSchemes.Classic())
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)

            ' Somebody writes the style directly, in device pixels, and says so.
            t.RowStyles(0).Height = 100F
            t.ResetStyleBaseline()
            Assert.Equal(50F, t.DebugAuthoredRow(0), 2)

            ' The next theme switch keeps it.
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(100F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub ResetStyleBaseline_LeavesOurOwnComputedValueAlone()
        Using t = NewTable()
            Fixed()
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 56), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(56F, t.RowStyles(0).Height, 2)

            ' A reset with nothing written from outside must not turn 56 into the authored value.
            t.ResetStyleBaseline()
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)
        End Using
    End Sub

    <Fact>
    Public Sub ResetStyleBaseline_BeforeAnySnapshot_IsHarmless()
        Using t = NewTable()
            t.ResetStyleBaseline()
            Assert.False(t.HasStyleBaseline)
        End Using
    End Sub

    ' ── Themed cell border ─────────────────────────────────────────────────────

    <Fact>
    Public Sub ThemedCellBorder_PromotesNoneToSingle_AndRefusesNoneAfterwards()
        Using t = NewTable()
            Assert.Equal(TableLayoutPanelCellBorderStyle.None, t.CellBorderStyle)
            t.ThemedCellBorder = True
            Assert.Equal(TableLayoutPanelCellBorderStyle.Single, t.CellBorderStyle)
            Assert.Throws(Of ArgumentException)(Sub() t.CellBorderStyle = TableLayoutPanelCellBorderStyle.None)
        End Using
    End Sub

    <Fact>
    Public Sub CellBorderColor_EmptyMeansTheme_PinnedWins()
        Using t = NewTable()
            t.ApplyTheme(BuiltInSchemes.Dark())
            Assert.Equal(BuiltInSchemes.Dark().Palette.BorderColor, t.EffectiveCellBorderColor)
            t.CellBorderColor = Color.Red
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(Color.Red, t.EffectiveCellBorderColor)
            t.ResetCellBorderColor()
            Assert.Equal(BuiltInSchemes.Modern().Palette.BorderColor, t.EffectiveCellBorderColor)
        End Using
    End Sub

    <Fact>
    Public Sub ThemedCellBorder_PaintsTheGridInTheChosenColour()
        Fixed()
        Using f As New Form() With {.AutoScaleMode = AutoScaleMode.None, .ClientSize = New Size(300, 200)}
            Using t = NewTable()
                t.Padding = Padding.Empty
                t.Dock = DockStyle.Fill
                t.BackColor = Color.White
                t.ThemedCellBorder = True
                t.CellBorderColor = Color.Red
                f.Controls.Add(t)
                Dim h As IntPtr = f.Handle
                f.PerformLayout()
                Using bmp As New Bitmap(t.Width, t.Height)
                    t.DrawToBitmap(bmp, New Rectangle(Point.Empty, t.Size))
                    Dim widths As Integer() = t.GetColumnWidths()
                    Dim heights As Integer() = t.GetRowHeights()
                    ' Frame, and the line after the first column / first row, all red.
                    Assert.Equal(Color.Red.ToArgb(), bmp.GetPixel(0, 20).ToArgb())
                    Assert.Equal(Color.Red.ToArgb(), bmp.GetPixel(widths(0), 20).ToArgb())
                    Assert.Equal(Color.Red.ToArgb(), bmp.GetPixel(bmp.Width - 1, 20).ToArgb())
                    Assert.Equal(Color.Red.ToArgb(), bmp.GetPixel(20, heights(0)).ToArgb())
                    Assert.Equal(Color.Red.ToArgb(), bmp.GetPixel(20, bmp.Height - 1).ToArgb())
                    ' And the cell interior stays the background.
                    Assert.Equal(Color.White.ToArgb(), bmp.GetPixel(20, 20).ToArgb())
                End Using
            End Using
        End Using
    End Sub

    ' ── Theming of the children (IThemedContainer) ─────────────────────────────

    <Fact>
    Public Sub ThemeManagerTraverse_StillReachesTheChildren()
        ' A plain IThemedControl stops the traversal at itself; a themed CONTAINER must not --
        ' the label inside belongs to the host form and takes the generic rules. The designer
        ' snapshot is taken by Traverse on every control it visits, so it is the witness.
        Using f As New Form()
            Using t = NewTable()
                Dim lbl As New Label() With {.Text = "Caută:", .AutoSize = True}
                t.Controls.Add(lbl, 0, 0)
                f.Controls.Add(t)
                Assert.False(DesignerBaseline.HasSnapshot(lbl))
                ThemeManager.Apply(f)
                Assert.True(DesignerBaseline.HasSnapshot(lbl))
                Assert.True(t.HasStyleBaseline, "the container itself must have been themed too")
            End Using
        End Using
    End Sub

    ' ── Designer serialization (C4) ────────────────────────────────────────────

    <Fact>
    Public Sub FreshControl_SerializesNoneOfItsProperties()
        Using t As New KBotTableLayoutPanel()
            Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(t)
            For Each name As String In {NameOf(KBotTableLayoutPanel.AutoFitToTheme),
                                        NameOf(KBotTableLayoutPanel.ScaleAbsoluteStyles),
                                        NameOf(KBotTableLayoutPanel.ThemedCellBorder),
                                        NameOf(KBotTableLayoutPanel.CellBorderColor),
                                        NameOf(KBotTableLayoutPanel.CellBorderStyle),
                                        NameOf(KBotTableLayoutPanel.Padding),
                                        NameOf(KBotTableLayoutPanel.BackColor),
                                        NameOf(KBotTableLayoutPanel.ForeColor),
                                        NameOf(KBotTableLayoutPanel.Font)}
                Assert.False(props(name).ShouldSerializeValue(t), name & " must not be serialized on a fresh control (value: " & props(name).GetValue(t)?.ToString() & ")")
            Next
            ' A theme pass does not turn a theme colour into an operator choice.
            t.ApplyTheme(BuiltInSchemes.Dark())
            Assert.False(props(NameOf(KBotTableLayoutPanel.BackColor)).ShouldSerializeValue(t))
            ' An explicit choice does.
            t.BackColor = Color.Yellow
            Assert.True(props(NameOf(KBotTableLayoutPanel.BackColor)).ShouldSerializeValue(t))
            t.ResetBackColor()
            Assert.False(props(NameOf(KBotTableLayoutPanel.BackColor)).ShouldSerializeValue(t))
            ' The padding serializes as the LOGICAL value the operator typed.
            Manual(2.0F)
            t.Padding = New Padding(8)
            Assert.True(props(NameOf(KBotTableLayoutPanel.Padding)).ShouldSerializeValue(t))
            Assert.Equal(New Padding(8), props(NameOf(KBotTableLayoutPanel.Padding)).GetValue(t))
            t.ResetPadding()
            Assert.False(props(NameOf(KBotTableLayoutPanel.Padding)).ShouldSerializeValue(t))
        End Using
    End Sub

End Class
