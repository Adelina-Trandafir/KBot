Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Controls
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0062-02: <see cref="KBotTableLayoutPanel"/> -- fixed rows/columns that follow the theme
''' and the K-BOT scale from an authored snapshot, and never from their own grown value.
'''
''' The scale tests use <c>ScalingMode.Manual</c>: a headless control reports 96 dpi, so the ratio
''' between our factor and the platform's is exactly the manual factor. <c>AppScaling</c> is process
''' state; every test puts it back.
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

    ''' <summary>Two columns (80 Absolute, 100 Percent) x two rows (40 Absolute, 100 Percent).</summary>
    Private Shared Function NewTable() As KBotTableLayoutPanel
        Dim t As New KBotTableLayoutPanel() With {.Width = 400, .Height = 300}
        t.ColumnCount = 2
        t.RowCount = 2
        t.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        t.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        t.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        t.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        Return t
    End Function

    Private Shared Sub Manual(factor As Single)
        AppScaling.LoadFrom(ScalingMode.Manual, factor, False, 1.0F)
    End Sub

    ' ── Snapshot ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Snapshot_IsTakenOnce_AtTheFirstThemeOrScaleHook()
        Using t = NewTable()
            Assert.False(t.HasStyleBaseline)
            t.ApplyTheme(BuiltInSchemes.Classic())
            Assert.True(t.HasStyleBaseline)
            Assert.Equal(80F, t.DebugAuthoredColumn(0), 2)
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)

            ' A grown value must never become the snapshot.
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
            Assert.Equal(80F, t.DebugAuthoredColumn(0), 2)
        End Using
    End Sub

    ' ── Scale ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Manual150_AbsoluteStyles_AreBaseTimes1_5()
        Using t = NewTable()
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Assert.Equal(120F, t.ColumnStyles(0).Width, 2)
            Assert.Equal(60F, t.RowStyles(0).Height, 2)
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
        End Using
    End Sub

    <Fact>
    Public Sub BackToAutomatic_IsExactlyTheBase()
        Using t = NewTable()
            Manual(1.5F)
            t.RefreshDpiMetrics()
            AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
            t.RefreshDpiMetrics()
            Assert.Equal(80F, t.ColumnStyles(0).Width)
            Assert.Equal(40F, t.RowStyles(0).Height)
        End Using
    End Sub

    <Fact>
    Public Sub ScaleAbsoluteStylesOff_TouchesNoStyle()
        Using t = NewTable()
            t.ScaleAbsoluteStyles = False
            Manual(1.5F)
            t.RefreshDpiMetrics()
            Assert.Equal(80F, t.ColumnStyles(0).Width)
            Assert.Equal(40F, t.RowStyles(0).Height)
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
        End Using
    End Sub

    ' ── Fit to content ─────────────────────────────────────────────────────────

    ' A leaf whose preferred size is whatever the test says.
    Private NotInheritable Class Wanting
        Inherits Control
        Public Wants As Size
        Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
            Return Wants
        End Function
    End Class

    <Fact>
    Public Sub FixedRow_GrowsBySurplus_AndReturnsToAuthored()
        Using t = NewTable()
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
    Public Sub FixedRow_SurplusIsMeasuredFromTheSnapshot_NotFromTheGrownValue()
        Using t = NewTable()
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
            ' A panel echoes its bounds: were it measured, the row would stick at 40 forever
            ' (fine) or at whatever it once grew to (not fine). It must ask for nothing.
            Dim p As New Panel() With {.Dock = DockStyle.Fill, .Margin = Padding.Empty}
            t.Controls.Add(p, 0, 0)
            t.RowStyles(0).Height = 90F      ' pretend a previous pass grew it
            t.ApplyTheme(BuiltInSchemes.Modern())
            ' The first hook takes the snapshot from the live value (90): what matters is that
            ' the panel adds NO surplus on top of it.
            Assert.Equal(90F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub AutoFitOff_NoSurplusIsAdded()
        Using t = NewTable()
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
            Dim b As New Wanting() With {.Dock = DockStyle.Fill, .Wants = New Size(50, 56), .Margin = Padding.Empty}
            t.Controls.Add(b, 0, 0)
            ' Column 0 (Absolute 80) holds 50: 80. Column 1 (Percent) is empty: 0.
            ' Row 0 (Absolute 40) holds 56: 56. Row 1 (Percent) is empty: 0. No gaps, no padding.
            Dim before As Size = t.GetPreferredSize(Size.Empty)
            Assert.Equal(New Size(80, 56), before)

            ' After the fit the row IS 56, so the surplus is 0 and the answer is the same 56 --
            ' the base answer now, not an addition on top of it.
            t.AutoFitToTheme = True
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(56F, t.RowStyles(0).Height, 2)
            Assert.Equal(before, t.GetPreferredSize(Size.Empty))
        End Using
    End Sub

    ' ── ResetStyleBaseline ─────────────────────────────────────────────────────

    <Fact>
    Public Sub ResetStyleBaseline_AfterAManualWrite_TheNewValueIsTheAuthoredOne()
        Using t = NewTable()
            t.ApplyTheme(BuiltInSchemes.Classic())
            Assert.Equal(40F, t.DebugAuthoredRow(0), 2)

            ' Somebody collapses the band (slice 0049-02) and says so.
            t.RowStyles(0).Height = 0F
            t.ResetStyleBaseline()
            Assert.Equal(0F, t.DebugAuthoredRow(0), 2)

            ' The next theme switch does NOT revive the band.
            t.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(0F, t.RowStyles(0).Height, 2)
        End Using
    End Sub

    <Fact>
    Public Sub ResetStyleBaseline_LeavesOurOwnFittedValueAlone()
        Using t = NewTable()
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
        Using f As New Form() With {.AutoScaleMode = AutoScaleMode.None, .ClientSize = New Size(300, 200)}
            Using t = NewTable()
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
        End Using
    End Sub

End Class
