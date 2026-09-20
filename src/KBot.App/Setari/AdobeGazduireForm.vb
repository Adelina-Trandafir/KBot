Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The extra settings of the HOSTED Adobe window (slice 0072-01): viewer profile, /n switch,
''' how the window is released when the document changes, and the floating-badge watcher.
'''
''' <para><b>Why a dialog and not four rows on the page.</b> The four settings mean something
''' only while the PDF engine is «Fereastră găzduită»; on ActiveX they did nothing and sat
''' there disabled, looking like they act. The operator asked (20.09.2026) that they leave the
''' page and appear in a small window the moment that engine is chosen. The page keeps one
''' button («Opțiuni…») for coming back to them later.</para>
'''
''' <para><b>Two stores, saved together on «Salvează».</b> Profile and /n live in
''' <c>kbot_paths.json</c> (per machine) through <see cref="AdobeViewerSettings.Persist"/>;
''' release mode and the watcher live in <see cref="AppSettings"/> (per user). Nothing is
''' written before the button -- unlike the page, the dialog is a unit the operator confirms
''' or abandons. <see cref="Rezumat"/> carries one line for the page's status band.</para>
''' </summary>
Public Class AdobeGazduireForm

    ''' <summary>One line for the status band after a save; empty when the dialog was abandoned.</summary>
    Public ReadOnly Property Rezumat As String
        Get
            Return _rezumat
        End Get
    End Property
    Private _rezumat As String = String.Empty

    Public Sub New()
        InitializeComponent()
        Try
            For Each m As AdobeViewerMode In New AdobeViewerMode() {AdobeViewerMode.Auto, AdobeViewerMode.Modern, AdobeViewerMode.Classic}
                cboAdobeMod.Items.Add(New AdobeModeItem(m))
            Next
            For Each n As AdobeNewInstanceMode In New AdobeNewInstanceMode() {AdobeNewInstanceMode.Auto, AdobeNewInstanceMode.Da, AdobeNewInstanceMode.Nu}
                cboAdobeInst.Items.Add(New AdobeNewInstanceItem(n))
            Next
            For Each d As AdobeDetachMode In New AdobeDetachMode() {AdobeDetachMode.KillProcess, AdobeDetachMode.CloseWindow}
                cboAdobeDetach.Items.Add(New DetachItem(d))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeGazduireForm.New", ex)
            Throw
        End Try
    End Sub

    ' The values are read at Load, not in the constructor: what the dialog shows is what the
    ' stores hold the moment it opens, including a change made by the DDF view's own combos.
    Private Sub AdobeGazduireForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            SelecteazaMod(AdobeViewerSettings.CurrentMode().Value)
            SelecteazaInstanta(AdobeViewerSettings.CurrentNewInstance().Value)
            SelecteazaDetach(AdobeHostSettings.CurrentDetachMode().Value)
            chkAdobePopup.Checked = AdobeHostSettings.CurrentPopupWatch()
            ActiveControl = cboAdobeMod
        Catch ex As Exception
            ' UI boundary (Load): log and swallow -- a throw would take the opening down.
            GlobalErrorLog.Write("AdobeGazduireForm.AdobeGazduireForm_Load", ex)
        End Try
    End Sub

    Private Sub SelecteazaMod(mode As AdobeViewerMode)
        For i As Integer = 0 To cboAdobeMod.Items.Count - 1
            If DirectCast(cboAdobeMod.Items(i), AdobeModeItem).Mode = mode Then cboAdobeMod.SelectedIndex = i : Return
        Next
    End Sub

    Private Sub SelecteazaInstanta(mode As AdobeNewInstanceMode)
        For i As Integer = 0 To cboAdobeInst.Items.Count - 1
            If DirectCast(cboAdobeInst.Items(i), AdobeNewInstanceItem).Mode = mode Then cboAdobeInst.SelectedIndex = i : Return
        Next
    End Sub

    Private Sub SelecteazaDetach(mode As AdobeDetachMode)
        For i As Integer = 0 To cboAdobeDetach.Items.Count - 1
            If DirectCast(cboAdobeDetach.Items(i), DetachItem).Mode = mode Then cboAdobeDetach.SelectedIndex = i : Return
        Next
    End Sub

    ''' <summary>
    ''' Writes both stores. A failed kbot_paths.json write is not fatal (the setting stays
    ''' active for the session -- see <c>KBotPaths.Save</c>) and is said in the summary; a
    ''' failed app_settings.json write is shown and keeps the dialog open.
    ''' </summary>
    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Dim mode As AdobeModeItem = TryCast(cboAdobeMod.SelectedItem, AdobeModeItem)
            Dim inst As AdobeNewInstanceItem = TryCast(cboAdobeInst.SelectedItem, AdobeNewInstanceItem)
            Dim detach As DetachItem = TryCast(cboAdobeDetach.SelectedItem, DetachItem)
            If mode Is Nothing OrElse inst Is Nothing OrElse detach Is Nothing Then
                KBotMessage.Show(Me, "Alege o valoare în fiecare listă.", "Fereastră găzduită Adobe",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' The engine is not this dialog's business, but Persist writes all three; the page
            ' opened us BECAUSE the engine is the hosted window, so that is what goes back.
            Dim salvatPaths As Boolean = AdobeViewerSettings.Persist(mode.Mode, inst.Mode, AdobePreviewEngine.WindowHost)

            Dim copie As AppSettings = AppSettings.Current.Clone()
            copie.AdobeDetachMode = AdobeHostSettings.DetachModeToText(detach.Mode)
            copie.AdobePopupWatch = chkAdobePopup.Checked
            copie.Save()

            _rezumat = If(salvatPaths,
                          "Setările ferestrei găzduite Adobe au fost salvate. Se aplică documentului următor.",
                          "Setările Adobe s-au aplicat pentru sesiunea curentă, dar kbot_paths.json nu a putut fi scris. Detalii în jurnalul de erori.")
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeGazduireForm.BtnSalveaza_Click", ex)
            KBotMessage.Show(Me, "Setările nu au putut fi salvate: " & ex.Message, "Fereastră găzduită Adobe",
                             MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyMain.BackColor = p.SurfaceAltColor
            tlyCampuri.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblIntro, lblAdobeMod, lblAdobeInst, lblAdobeDetach}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeGazduireForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class

''' <summary>A row of the «La schimbarea documentului» combo: the value + its Romanian label. POCO, no Try/Catch.</summary>
Friend NotInheritable Class DetachItem
    Public ReadOnly Property Mode As AdobeDetachMode

    Public Sub New(mode As AdobeDetachMode)
        Me.Mode = mode
    End Sub

    Public Overrides Function ToString() As String
        Return AdobeHostSettings.DetachModeLabel(Mode)
    End Function
End Class
