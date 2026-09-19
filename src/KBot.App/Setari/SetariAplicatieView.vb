Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Aplicație» (slice 0072): the global switches, how documents open, and the folders.
'''
''' <para><b>Three stores, one page.</b> The switches and the two host options go to
''' <see cref="AppSettings"/> (<c>app_settings.json</c>, per user); the three Adobe viewer
''' settings keep living in <c>kbot_paths.json</c> (per machine -- what Adobe is installed
''' there) through <see cref="AdobeViewerSettings.Persist"/>, the same call the DDF view's
''' combos make; the folders keep living in <c>settings.json</c> through
''' <see cref="SetariFoldere.Salveaza"/>.</para>
'''
''' <para><b>Switches save on change; folders save on the button.</b> A switch is one value
''' and its effect is immediate. The folder grid is a set edited cell by cell, validated at
''' STARTUP (a path that cannot be written stops the launch), so it is written once, on
''' purpose, and the page says a restart is needed.</para>
''' </summary>
Public Class SetariAplicatieView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    ' Guards the change handlers while the page fills its controls from the stores.
    Private _suppress As Boolean

    Public Sub New()
        InitializeComponent()
        BuildCombos()
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "aplicatie"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            IncarcaComutatoarele()
            IncarcaDocumentele()
            IncarcaFolderele()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.Activated", ex)
        End Try
    End Sub

    ' ---------------- combos (labels only; values come at Activated) ----------------

    Private Sub BuildCombos()
        Try
            cboVerbose.Items.Add(New VerboseItem(Nothing, "Implicit (pornit pe Debug, oprit pe Release)"))
            cboVerbose.Items.Add(New VerboseItem(True, "Pornit — tot ce scrie robotul"))
            cboVerbose.Items.Add(New VerboseItem(False, "Oprit — doar <Log> și erorile"))

            For Each g As AdobePreviewEngine In New AdobePreviewEngine() {AdobePreviewEngine.WindowHost, AdobePreviewEngine.ActiveX}
                cboAdobeMotor.Items.Add(New AdobeEngineItem(g))
            Next
            For Each m As AdobeViewerMode In New AdobeViewerMode() {AdobeViewerMode.Auto, AdobeViewerMode.Modern, AdobeViewerMode.Classic}
                cboAdobeMod.Items.Add(New AdobeModeItem(m))
            Next
            For Each n As AdobeNewInstanceMode In New AdobeNewInstanceMode() {AdobeNewInstanceMode.Auto, AdobeNewInstanceMode.Da, AdobeNewInstanceMode.Nu}
                cboAdobeInst.Items.Add(New AdobeNewInstanceItem(n))
            Next
            For Each d As AdobeDetachMode In New AdobeDetachMode() {AdobeDetachMode.KillProcess, AdobeDetachMode.CloseWindow}
                cboAdobeDetach.Items.Add(New DetachItem(d))
            Next
            For Each r As ExcelRibbonMode In New ExcelRibbonMode() {ExcelRibbonMode.HideDockWindow, ExcelRibbonMode.Excel4Macro}
                cboExcelRibbon.Items.Add(New RibbonItem(r))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.BuildCombos", ex)
            Throw
        End Try
    End Sub

    ' ---------------- switches ----------------

    Private Sub IncarcaComutatoarele()
        _suppress = True
        Try
            Dim s As AppSettings = AppSettings.Current
            SelecteazaVerbose(s.VerboseLogging)
            chkLogViewer.Checked = s.LogViewerEnabled
            chkShowBrowser.Checked = s.ShowBrowserButton
            chkReceptii.Checked = s.ReceptiiCheckedOnOpen
        Finally
            _suppress = False
        End Try
    End Sub

    Private Sub SelecteazaVerbose(value As Boolean?)
        For i As Integer = 0 To cboVerbose.Items.Count - 1
            Dim item As VerboseItem = DirectCast(cboVerbose.Items(i), VerboseItem)
            If Nullable.Equals(item.Value, value) Then
                cboVerbose.SelectedIndex = i
                Return
            End If
        Next
        cboVerbose.SelectedIndex = 0
    End Sub

    ''' <summary>
    ''' One saver for every switch: copies the current store, applies the change, writes it
    ''' (which makes it Current and raises Changed), and reports on the band. A write that
    ''' fails is said, and the control is put back to what the store still holds.
    ''' </summary>
    Private Sub SalveazaComutator(aplica As Action(Of AppSettings), mesaj As String)
        Try
            If _suppress Then Return
            Dim copie As AppSettings = AppSettings.Current.Clone()
            aplica(copie)
            copie.Save()
            RaiseEvent StatusChanged(mesaj)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.SalveazaComutator", ex)
            RaiseEvent StatusChanged("Setarea nu a putut fi salvată: " & ex.Message)
            IncarcaComutatoarele()
            IncarcaDocumentele()
        End Try
    End Sub

    Private Sub CboVerbose_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboVerbose.SelectedIndexChanged
        Dim item As VerboseItem = TryCast(cboVerbose.SelectedItem, VerboseItem)
        If item Is Nothing Then Return
        SalveazaComutator(Sub(s) s.VerboseLogging = item.Value,
                          "Consola FOREXE: " & item.ToString() & ".")
        ' Effect at once on every open console (slice 0071 re-renders from its buffer).
        ' «Implicit» puts the build's own default back: on in Debug, off in Release.
        If Not _suppress Then
            Dim buildDefault As Boolean
#If DEBUG Then
            buildDefault = True
#Else
            buildDefault = False
#End If
            RichTextBoxLogger.VerboseLogging = If(item.Value.HasValue, item.Value.Value, buildDefault)
        End If
    End Sub

    Private Sub ChkLogViewer_CheckedChanged(sender As Object, e As EventArgs) Handles chkLogViewer.CheckedChanged
        SalveazaComutator(Sub(s) s.LogViewerEnabled = chkLogViewer.Checked,
                          If(chkLogViewer.Checked, "Rândul «Arată jurnal» este oferit.", "Rândul «Arată jurnal» este ascuns."))
    End Sub

    Private Sub ChkShowBrowser_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowBrowser.CheckedChanged
        SalveazaComutator(Sub(s) s.ShowBrowserButton = chkShowBrowser.Checked,
                          If(chkShowBrowser.Checked, "Butonul «Arată browserul» este oferit.", "Butonul «Arată browserul» este ascuns."))
    End Sub

    Private Sub ChkReceptii_CheckedChanged(sender As Object, e As EventArgs) Handles chkReceptii.CheckedChanged
        SalveazaComutator(Sub(s) s.ReceptiiCheckedOnOpen = chkReceptii.Checked,
                          If(chkReceptii.Checked, "Selectorul de recepții pornește cu tot bifat.", "Selectorul de recepții pornește gol."))
    End Sub

    ' ---------------- documents ----------------

    Private Sub IncarcaDocumentele()
        _suppress = True
        Try
            SelecteazaMotor(AdobeViewerSettings.CurrentEngine().Value)
            SelecteazaMod(AdobeViewerSettings.CurrentMode().Value)
            SelecteazaInstanta(AdobeViewerSettings.CurrentNewInstance().Value)
            SelecteazaDetach(AdobeHostSettings.CurrentDetachMode().Value)
            chkAdobePopup.Checked = AdobeHostSettings.CurrentPopupWatch()
            SelecteazaPanglica(OfficeHostSettings.CurrentExcelRibbon().Value)
            ActualizeazaDisponibilitateaAdobe()
        Finally
            _suppress = False
        End Try
    End Sub

    Private Sub SelecteazaMotor(engine As AdobePreviewEngine)
        For i As Integer = 0 To cboAdobeMotor.Items.Count - 1
            If DirectCast(cboAdobeMotor.Items(i), AdobeEngineItem).Engine = engine Then cboAdobeMotor.SelectedIndex = i : Return
        Next
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

    Private Sub SelecteazaPanglica(mode As ExcelRibbonMode)
        For i As Integer = 0 To cboExcelRibbon.Items.Count - 1
            If DirectCast(cboExcelRibbon.Items(i), RibbonItem).Mode = mode Then cboExcelRibbon.SelectedIndex = i : Return
        Next
    End Sub

    ' «Mod» and «instanță nouă» describe the HOSTED WINDOW; on ActiveX they do nothing, and
    ' the combos say so instead of looking like they act (same rule as DdfDocumentPage).
    Private Sub ActualizeazaDisponibilitateaAdobe()
        Dim motor As AdobeEngineItem = TryCast(cboAdobeMotor.SelectedItem, AdobeEngineItem)
        Dim peFereastra As Boolean = motor Is Nothing OrElse motor.Engine = AdobePreviewEngine.WindowHost
        cboAdobeMod.Enabled = peFereastra
        cboAdobeInst.Enabled = peFereastra
        cboAdobeDetach.Enabled = peFereastra
        chkAdobePopup.Enabled = peFereastra
        lblAdobeMod.Enabled = peFereastra
        lblAdobeInst.Enabled = peFereastra
        lblAdobeDetach.Enabled = peFereastra
    End Sub

    ' The three viewer combos save together into kbot_paths.json (they describe one surface).
    Private Sub AdobeViewer_Changed(sender As Object, e As EventArgs) _
        Handles cboAdobeMotor.SelectedIndexChanged, cboAdobeMod.SelectedIndexChanged, cboAdobeInst.SelectedIndexChanged
        Try
            If _suppress Then Return
            Dim motor As AdobeEngineItem = TryCast(cboAdobeMotor.SelectedItem, AdobeEngineItem)
            Dim mode As AdobeModeItem = TryCast(cboAdobeMod.SelectedItem, AdobeModeItem)
            Dim inst As AdobeNewInstanceItem = TryCast(cboAdobeInst.SelectedItem, AdobeNewInstanceItem)
            If motor Is Nothing OrElse mode Is Nothing OrElse inst Is Nothing Then Return
            ActualizeazaDisponibilitateaAdobe()

            Dim salvat As Boolean = AdobeViewerSettings.Persist(mode.Mode, inst.Mode, motor.Engine)
            RaiseEvent StatusChanged(If(salvat,
                "Setările Adobe au fost salvate (kbot_paths.json). Documentul următor le folosește.",
                "Setările Adobe s-au aplicat pentru sesiunea curentă, dar nu au putut fi salvate. Detalii în jurnalul de erori."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.AdobeViewer_Changed", ex)
            RaiseEvent StatusChanged("Setările Adobe nu au putut fi salvate: " & ex.Message)
        End Try
    End Sub

    Private Sub CboAdobeDetach_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAdobeDetach.SelectedIndexChanged
        Dim item As DetachItem = TryCast(cboAdobeDetach.SelectedItem, DetachItem)
        If item Is Nothing Then Return
        SalveazaComutator(Sub(s) s.AdobeDetachMode = AdobeHostSettings.DetachModeToText(item.Mode),
                          "Eliberarea ferestrei Adobe: " & item.ToString() & ". Se aplică documentului următor.")
    End Sub

    Private Sub ChkAdobePopup_CheckedChanged(sender As Object, e As EventArgs) Handles chkAdobePopup.CheckedChanged
        SalveazaComutator(Sub(s) s.AdobePopupWatch = chkAdobePopup.Checked,
                          If(chkAdobePopup.Checked, "Fereastra plutitoare Adobe se ascunde.", "Fereastra plutitoare Adobe rămâne vizibilă.") &
                          " Se aplică documentului următor.")
    End Sub

    Private Sub CboExcelRibbon_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboExcelRibbon.SelectedIndexChanged
        Dim item As RibbonItem = TryCast(cboExcelRibbon.SelectedItem, RibbonItem)
        If item Is Nothing Then Return
        SalveazaComutator(Sub(s) s.ExcelRibbon = OfficeHostSettings.ExcelRibbonToText(item.Mode),
                          "Panglica Excel: " & item.ToString() & ". Se aplică documentului următor.")
    End Sub

    ' ---------------- folders ----------------

    ''' <summary>
    ''' One row per folder setting, in the order <see cref="SetariFoldere.Toate"/> declares
    ''' them. The RAW operator value goes in the editable column (empty = default), exactly
    ''' what <see cref="SetariFoldere.Bruta"/> exists for; the resolved path is not shown
    ''' because it would look editable and is not.
    ''' </summary>
    Private Sub IncarcaFolderele()
        _suppress = True
        Try
            Dim foldere As SetariFoldere = SetariFoldere.Incarca()
            gridFoldere.BeginUpdate()
            Try
                gridFoldere.ClearRows()
                For Each setare As SetariFoldere.Setare In SetariFoldere.Toate
                    Dim rand As KBotDataRow = gridFoldere.AddRow()
                    rand("cheie") = setare.Cheie
                    rand("descriere") = setare.Descriere
                    rand("implicit") = setare.Implicit
                    rand("cale") = If(foldere.Bruta(setare.Cheie), String.Empty)
                Next
            Finally
                gridFoldere.EndUpdate()
            End Try
            gridFoldere.ClearDirty()
            btnSalveazaFoldere.Enabled = False
            lblFoldereStare.Text = If(foldere.Probleme.Count = 0,
                                      "Fișier: " & SetariFoldere.CaleSetari(),
                                      String.Join(" ", foldere.Probleme))
        Finally
            _suppress = False
        End Try
    End Sub

    Private Sub GridFoldere_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridFoldere.CellValueChanged
        Try
            If _suppress Then Return
            btnSalveazaFoldere.Enabled = True
            lblFoldereStare.Text = "Modificări nesalvate — apasă «Salvează folderele»."
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.GridFoldere_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub BtnSalveazaFoldere_Click(sender As Object, e As EventArgs) Handles btnSalveazaFoldere.Click
        Try
            Dim valori As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = 0 To gridFoldere.RowCount - 1
                Dim cheie As String = Convert.ToString(gridFoldere("cheie", i), Globalization.CultureInfo.InvariantCulture)
                Dim cale As String = Convert.ToString(gridFoldere("cale", i), Globalization.CultureInfo.InvariantCulture)
                valori(cheie) = cale
            Next
            SetariFoldere.Salveaza(valori)
            gridFoldere.ClearDirty()
            btnSalveazaFoldere.Enabled = False
            lblFoldereStare.Text = "Salvat. Folderele se verifică la următoarea pornire a aplicației."
            RaiseEvent StatusChanged("Folderele au fost salvate în settings.json — au efect la următoarea pornire.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.BtnSalveazaFoldere_Click", ex)
            lblFoldereStare.Text = "Salvarea a eșuat: " & ex.Message
            RaiseEvent StatusChanged("Folderele nu au putut fi salvate.")
        End Try
    End Sub

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyComutatoare.BackColor = p.SurfaceAltColor
            tlyDocumente.BackColor = p.SurfaceAltColor
            tlyFoldereButoane.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblVerbose, lblAdobeMotor, lblAdobeMod, lblAdobeInst,
                                                      lblAdobeDetach, lblExcelRibbon, lblFoldereHint, lblFoldereStare}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplyPrimary(btnSalveazaFoldere, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.ApplyTheme", ex)
        End Try
    End Sub

    ' ---------------- combo items (POCOs, no Try/Catch) ----------------

    Private NotInheritable Class VerboseItem
        Public ReadOnly Property Value As Boolean?
        Private ReadOnly _label As String

        Public Sub New(value As Boolean?, label As String)
            Me.Value = value
            _label = label
        End Sub

        Public Overrides Function ToString() As String
            Return _label
        End Function
    End Class

    Private NotInheritable Class DetachItem
        Public ReadOnly Property Mode As AdobeDetachMode

        Public Sub New(mode As AdobeDetachMode)
            Me.Mode = mode
        End Sub

        Public Overrides Function ToString() As String
            Return AdobeHostSettings.DetachModeLabel(Mode)
        End Function
    End Class

    Private NotInheritable Class RibbonItem
        Public ReadOnly Property Mode As ExcelRibbonMode

        Public Sub New(mode As ExcelRibbonMode)
            Me.Mode = mode
        End Sub

        Public Overrides Function ToString() As String
            Return OfficeHostSettings.ExcelRibbonLabel(Mode)
        End Function
    End Class

End Class
