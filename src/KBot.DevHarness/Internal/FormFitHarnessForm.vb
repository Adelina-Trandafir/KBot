Option Strict On
Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The bench of slice 0062 -- everything the slice added, on one screen, with numbers next to
''' every effect so the verdict is not a guess:
''' <list type="bullet">
''' <item><b>The window grows to its theme and never below its base</b> (<see cref="ThemeFormFit"/>):
''' switch the scheme, drag the text-size slider, and read client vs. base vs. demand in the
''' journal. Turn «fereastra crește la temă» off to see the same switches WITHOUT the fit.</item>
''' <item><b>What the base means</b> (<see cref="ThemeFormFit.Baseline"/>): the two radios flip
''' between «follows the scale» and «raw pixels»; every open themed form refits on the spot.</item>
''' <item><b>Where windows land</b> (<see cref="AppScreen"/>): the three probe buttons open a
''' framed dialog, a CenterParent dialog with no owner (WinForms' mouse-monitor fallback) and a
''' borderless shell; each reports the screen it landed on versus the reference and the mouse.
''' Move THIS window to another monitor, make it the reference, and open them again.</item>
''' <item><b><see cref="KBotTableLayoutPanel"/></b>: the log viewer's filter row, authored SHORT
''' (32px rows), with themed cell lines. Under Modern the rows must grow and under Classic come
''' back; the label column must keep step with the label as the text grows; collapsing the band
''' through <c>SetRowCollapsed</c> must survive the next scheme switch.</item>
''' </list>
''' Scheme, text size and base are put back on close: a bench never leaves a setting behind.
''' </summary>
Public NotInheritable Class FormFitHarnessForm

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _originalScheme As ThemeScheme
    Private ReadOnly _originalTextScale As Single
    Private ReadOnly _originalBaseline As FormFitBaseline
    Private _suppress As Boolean

    Public Sub New(log As Action(Of String))
        _log = log
        _originalScheme = ThemeManager.Current
        _originalTextScale = AppScaling.TextScale
        _originalBaseline = ThemeFormFit.Baseline
        InitializeComponent()
        _suppress = True
        Try
            trkScale.Value = Math.Max(trkScale.Minimum, Math.Min(trkScale.Maximum, CInt(Math.Round(AppScaling.TextScale * 100))))
            lblScaleValue.Text = trkScale.Value & "%"
            rdoScaled.Checked = (ThemeFormFit.Baseline = FormFitBaseline.Scaled)
            rdoRaw.Checked = (ThemeFormFit.Baseline = FormFitBaseline.DesignerRaw)
            chkAutoFit.Checked = AutoFitToTheme
            chkThemedBorder.Checked = tblProbe.ThemedCellBorder
            chkScaleStyles.Checked = tblProbe.ScaleAbsoluteStyles
            chkTableAutoFit.Checked = tblProbe.AutoFitToTheme
        Finally
            _suppress = False
        End Try
    End Sub

    ' ── Lifecycle ────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)   ' theme, fit, placement -- all done by the base
        Try
            AddHandler AppScaling.ScalingChanged, AddressOf OnScalingChangedReadout
            Note("deschis")
            Readout("la deschidere")
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            RemoveHandler AppScaling.ScalingChanged, AddressOf OnScalingChangedReadout
            ' Put back what the bench moved. Scheme and text size persist, so the file ends as it began.
            If _originalScheme IsNot Nothing AndAlso Not ReferenceEquals(ThemeManager.Current, _originalScheme) Then
                ThemeManager.SetScheme(_originalScheme)
            End If
            If Math.Abs(AppScaling.TextScale - _originalTextScale) > 0.0001F Then AppScaling.SetTextScale(_originalTextScale)
            If ThemeFormFit.Baseline <> _originalBaseline Then ThemeFormFit.Baseline = _originalBaseline
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.OnFormClosed", ex)
        Finally
            MyBase.OnFormClosed(e)
        End Try
    End Sub

    ' The base refits AFTER OnThemeChanged (same handler, next statement); the readout is posted
    ' so it reads the size the fit produced, not the one before it.
    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        Try
            lblActive.Text = "activ: " & ThemeManager.Current.Name
            If IsHandleCreated Then BeginInvoke(Sub() Readout("după schema " & ThemeManager.Current.Name))
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub OnScalingChangedReadout(sender As Object, e As EventArgs)
        Try
            If IsHandleCreated Then BeginInvoke(Sub() Readout("după scalare " & CInt(Math.Round(AppScaling.TextScale * 100)) & "%"))
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.OnScalingChangedReadout", ex)
        End Try
    End Sub

    ' ── Scheme ───────────────────────────────────────────────────────────────────

    Private Sub OnClassic(sender As Object, e As EventArgs) Handles btnClassic.Click
        Switch(BuiltInSchemes.Classic())
    End Sub

    Private Sub OnDark(sender As Object, e As EventArgs) Handles btnDark.Click
        Switch(BuiltInSchemes.Dark())
    End Sub

    Private Sub OnModern(sender As Object, e As EventArgs) Handles btnModern.Click
        Switch(BuiltInSchemes.Modern())
    End Sub

    Private Sub OnColorful(sender As Object, e As EventArgs) Handles btnColorful.Click
        Switch(BuiltInSchemes.Colorful())
    End Sub

    Private Sub Switch(scheme As ThemeScheme)
        Try
            Note("comută schema → " & scheme.Name)
            ThemeManager.SetScheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.Switch", ex)
            Note("EROARE la comutare: " & ex.Message)
        End Try
    End Sub

    ' ── Text size and base ───────────────────────────────────────────────────────

    Private Sub trkScale_ValueChanged(sender As Object, e As EventArgs) Handles trkScale.ValueChanged
        lblScaleValue.Text = trkScale.Value & "%"
    End Sub

    ' Applied at the END of the gesture, like the options form: applying rescales this very
    ' window, and the slider would move under the finger.
    Private Sub trkScale_GestureDone(sender As Object, e As EventArgs) Handles trkScale.MouseUp, trkScale.KeyUp, trkScale.Leave
        Try
            If _suppress Then Return
            Dim wanted As Single = trkScale.Value / 100.0F
            If Math.Abs(wanted - AppScaling.TextScale) < 0.0001F Then Return
            Note("mărime text → " & trkScale.Value & "%")
            AppScaling.SetTextScale(wanted)
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.trkScale_GestureDone", ex)
            Note("EROARE la scalare: " & ex.Message)
        End Try
    End Sub

    Private Sub rdoBaseline_CheckedChanged(sender As Object, e As EventArgs) Handles rdoScaled.CheckedChanged, rdoRaw.CheckedChanged
        Try
            If _suppress Then Return
            Dim rdo As RadioButton = TryCast(sender, RadioButton)
            If rdo Is Nothing OrElse Not rdo.Checked Then Return
            Dim wanted As FormFitBaseline = If(rdo Is rdoRaw, FormFitBaseline.DesignerRaw, FormFitBaseline.Scaled)
            If wanted = ThemeFormFit.Baseline Then Return
            Note("baza ferestrei → " & wanted.ToString())
            ThemeFormFit.Baseline = wanted   ' refits every open themed form, this one included
            Readout("după schimbarea bazei")
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.rdoBaseline_CheckedChanged", ex)
            Note("EROARE la schimbarea bazei: " & ex.Message)
        End Try
    End Sub

    Private Sub chkAutoFit_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoFit.CheckedChanged
        If _suppress Then Return
        AutoFitToTheme = chkAutoFit.Checked
        Note("fereastra crește la temă: " & If(AutoFitToTheme, "DA", "NU"))
        If AutoFitToTheme Then RefitToTheme()
        Readout("după comutatorul de potrivire")
    End Sub

    Private Sub btnRefit_Click(sender As Object, e As EventArgs) Handles btnRefit.Click
        Try
            Dim before As Size = ClientSize
            RefitToTheme()
            Note("repotrivire cerută: " & SizeText(before) & " → " & SizeText(ClientSize))
            Readout("după repotrivire")
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnRefit_Click", ex)
            Note("EROARE la repotrivire: " & ex.Message)
        End Try
    End Sub

    ' ── Where windows land ───────────────────────────────────────────────────────

    Private Sub btnDialog_Click(sender As Object, e As EventArgs) Handles btnDialog.Click
        Try
            Note("deschide dialog cu chenar, CenterScreen (mouse-ul e pe " & Screen.FromPoint(Control.MousePosition).DeviceName & ")")
            Using d As New FormFitProbeDialog(False, FormStartPosition.CenterScreen, AddressOf Note)
                d.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnDialog_Click", ex)
            Note("EROARE la deschidere: " & ex.Message)
        End Try
    End Sub

    ' Modeless and ownerless on purpose: exactly the case where WinForms' CenterParent falls back
    ' to CenterToScreen, i.e. to the monitor under the mouse. WinForms disposes a modeless form
    ' on close, so no Using.
    Private Sub btnDialogParent_Click(sender As Object, e As EventArgs) Handles btnDialogParent.Click
        Try
            Note("deschide dialog CenterParent FĂRĂ owner (mouse-ul e pe " & Screen.FromPoint(Control.MousePosition).DeviceName & ")")
            Dim d As New FormFitProbeDialog(False, FormStartPosition.CenterParent, AddressOf Note)
            d.Show()
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnDialogParent_Click", ex)
            Note("EROARE la deschidere: " & ex.Message)
        End Try
    End Sub

    Private Sub btnShell_Click(sender As Object, e As EventArgs) Handles btnShell.Click
        Try
            Note("deschide shell fără chenar, CenterScreen (mouse-ul e pe " & Screen.FromPoint(Control.MousePosition).DeviceName & ")")
            Using d As New FormFitProbeDialog(True, FormStartPosition.CenterScreen, AddressOf Note)
                d.ShowDialog(Me)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnShell_Click", ex)
            Note("EROARE la deschidere: " & ex.Message)
        End Try
    End Sub

    Private Sub btnSetRef_Click(sender As Object, e As EventArgs) Handles btnSetRef.Click
        Try
            AppScreen.SetReference(Me)
            Note("referința = fereastra asta, pe " & Screen.FromHandle(Handle).DeviceName)
            Readout("după schimbarea referinței")
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnSetRef_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    ' ── The table ────────────────────────────────────────────────────────────────

    Private Sub chkThemedBorder_CheckedChanged(sender As Object, e As EventArgs) Handles chkThemedBorder.CheckedChanged
        If _suppress Then Return
        tblProbe.ThemedCellBorder = chkThemedBorder.Checked
        Note("linii de celulă din temă: " & If(tblProbe.ThemedCellBorder, "DA", "NU (culorile sistemului)"))
    End Sub

    Private Sub chkScaleStyles_CheckedChanged(sender As Object, e As EventArgs) Handles chkScaleStyles.CheckedChanged
        If _suppress Then Return
        tblProbe.ScaleAbsoluteStyles = chkScaleStyles.Checked
        Note("coloanele fixe urmează scara K-BOT: " & If(tblProbe.ScaleAbsoluteStyles, "DA", "NU"))
        Readout("după comutatorul de scară al tabelului")
    End Sub

    Private Sub chkTableAutoFit_CheckedChanged(sender As Object, e As EventArgs) Handles chkTableAutoFit.CheckedChanged
        If _suppress Then Return
        tblProbe.AutoFitToTheme = chkTableAutoFit.Checked
        Note("rândurile fixe cresc la conținut: " & If(tblProbe.AutoFitToTheme, "DA", "NU"))
        Readout("după comutatorul de potrivire al tabelului")
    End Sub

    ''' <summary>
    ''' The slice 0049-02 move, through the table's own API since 0066: the band is collapsed with
    ''' <c>SetRowCollapsed</c>, which the next scheme or scale pass honours on its own -- the
    ''' authored height stays in the table, in logical pixels. Visible is WRITTEN here, never read.
    ''' </summary>
    Private Sub btnCollapse_Click(sender As Object, e As EventArgs) Handles btnCollapse.Click
        Try
            lblBanda.Visible = False
            chkBanda.Visible = False
            tblProbe.SetRowCollapsed(1, True)
            Note("banda strânsă prin SetRowCollapsed (autorat: " & tblProbe.DebugAuthoredRow(1).ToString("0.#") & ", acum " & tblProbe.RowStyles(1).Height.ToString("0.#") & ")")
            Readout("după strângerea benzii")
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnCollapse_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    Private Sub btnExpand_Click(sender As Object, e As EventArgs) Handles btnExpand.Click
        Try
            If Not tblProbe.IsRowCollapsed(1) Then
                Note("banda nu a fost strânsă -- nimic de desfăcut")
                Return
            End If
            tblProbe.SetRowCollapsed(1, False)
            lblBanda.Visible = True
            chkBanda.Visible = True
            Note("banda desfăcută prin SetRowCollapsed (autorat: " & tblProbe.DebugAuthoredRow(1).ToString("0.#") & ", acum " & tblProbe.RowStyles(1).Height.ToString("0.#") & ")")
            Readout("după desfacerea benzii")
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.btnExpand_Click", ex)
            Note("EROARE: " & ex.Message)
        End Try
    End Sub

    ' ── Readout ──────────────────────────────────────────────────────────────────

    ''' <summary>One journal line per fact: the window, the reference, the table. Every number the
    ''' slice promises is here, next to the state that produced it.</summary>
    Private Sub Readout(moment As String)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            Dim captured As Size = ThemeFormFit.CapturedClientSize(Me)
            Dim demand As Size = If(AutoFitToTheme, ThemeFormFit.Demand(Me, FitRoot), Size.Empty)
            Note("[" & moment & "] fereastră: client " & SizeText(ClientSize) & " · baza " & SizeText(captured) &
                 " (scară la captură " & ThemeFormFit.CapturedScale(Me).ToString("0.00") & ", acum " & AppScaling.FactorFor(Me).ToString("0.00") &
                 ", " & ThemeFormFit.Baseline.ToString() & ")" &
                 If(AutoFitToTheme, " · cerere " & SizeText(demand), " · potrivire OPRITĂ") &
                 " · font " & Font.Size.ToString("0.##") & "pt · ecran " & Screen.FromHandle(Handle).DeviceName)

            Dim refForm As Form = AppScreen.ReferenceForm()
            lblReference.Text = "referință: " & If(refForm Is Nothing, "(niciuna)", refForm.Name) & " pe " & AppScreen.Reference(Me).DeviceName &
                                " · mouse pe " & Screen.FromPoint(Control.MousePosition).DeviceName

            Dim cols As New Text.StringBuilder()
            For i As Integer = 0 To tblProbe.ColumnStyles.Count - 1
                If tblProbe.ColumnStyles(i).SizeType <> SizeType.Absolute Then Continue For
                If cols.Length > 0 Then cols.Append("  ")
                cols.Append(i).Append(":").Append(tblProbe.DebugAuthoredColumn(i).ToString("0")).Append("→").Append(tblProbe.ColumnStyles(i).Width.ToString("0"))
            Next
            Dim rows As String = "r0 " & tblProbe.DebugAuthoredRow(0).ToString("0") & "→" & tblProbe.RowStyles(0).Height.ToString("0") &
                                 "  r1 " & tblProbe.DebugAuthoredRow(1).ToString("0") & "→" & tblProbe.RowStyles(1).Height.ToString("0")
            Dim lbl As String = "lblCauta " & lblCauta.GetPreferredSize(Size.Empty).Width & "px în coloana " & tblProbe.ColumnStyles(0).Width.ToString("0")
            lblTableInfo.Text = "coloane autorat→acum: " & cols.ToString() & "   rânduri: " & rows & "   " & lbl &
                                "   cerere tabel " & SizeText(tblProbe.GetPreferredSize(Size.Empty))
            Note("[" & moment & "] tabel: " & lblTableInfo.Text)
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.Readout", ex)
        End Try
    End Sub

    Private Sub Note(text As String)
        Try
            _log(text)
            lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") & "  " & text)
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitHarnessForm.Note", ex)
        End Try
    End Sub

    Private Shared Function SizeText(s As Size) As String
        Return s.Width & "×" & s.Height
    End Function

End Class
