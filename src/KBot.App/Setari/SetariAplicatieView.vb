Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Aplicație» (slice 0072): the global switches, how documents open, and the folders.
'''
''' <para><b>Three stores, one page.</b> The switches and the Excel option go to
''' <see cref="AppSettings"/> (<c>app_settings.json</c>, per user); the PDF engine keeps
''' living in <c>kbot_paths.json</c> (per machine -- what Adobe is installed there) through
''' <see cref="AdobeViewerSettings.Persist"/>, the same call the DDF view's combos make; the
''' folders keep living in <c>settings.json</c> through <see cref="SetariFoldere.Salveaza"/>.
''' The four settings that only matter for the HOSTED Adobe window are NOT on the page
''' (slice 0072-01): choosing «Fereastră găzduită» opens <see cref="AdobeGazduireForm"/>,
''' and the «Opțiuni…» button under the combo reopens it later.</para>
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
            RichTextBoxLogger.VerboseLogging = If(item.Value, buildDefault)
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

    Private Sub SelecteazaPanglica(mode As ExcelRibbonMode)
        For i As Integer = 0 To cboExcelRibbon.Items.Count - 1
            If DirectCast(cboExcelRibbon.Items(i), RibbonItem).Mode = mode Then cboExcelRibbon.SelectedIndex = i : Return
        Next
    End Sub

    ' The «Opțiuni…» button is for the HOSTED WINDOW only; on ActiveX the four settings behind
    ' it do nothing, and a button that opens a dialog which changes nothing would look like it
    ' acts (same rule as DdfDocumentPage).
    Private Sub ActualizeazaDisponibilitateaAdobe()
        btnAdobeGazduire.Enabled = MotorulEsteFereastra()
    End Sub

    Private Function MotorulEsteFereastra() As Boolean
        Dim motor As AdobeEngineItem = TryCast(cboAdobeMotor.SelectedItem, AdobeEngineItem)
        Return motor Is Nothing OrElse motor.Engine = AdobePreviewEngine.WindowHost
    End Function

    ''' <summary>
    ''' The engine saves into kbot_paths.json next to the two values it does not own (Persist
    ''' writes all three; they are read back from the store, untouched). Choosing the hosted
    ''' window then opens the dialog with its four settings -- the operator's request: they
    ''' appear the moment that engine is picked, not as rows sitting on the page.
    ''' </summary>
    Private Sub CboAdobeMotor_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAdobeMotor.SelectedIndexChanged
        Try
            If _suppress Then Return
            Dim motor As AdobeEngineItem = TryCast(cboAdobeMotor.SelectedItem, AdobeEngineItem)
            If motor Is Nothing Then Return
            ActualizeazaDisponibilitateaAdobe()

            Dim salvat As Boolean = AdobeViewerSettings.Persist(AdobeViewerSettings.CurrentMode().Value,
                                                                AdobeViewerSettings.CurrentNewInstance().Value,
                                                                motor.Engine)
            RaiseEvent StatusChanged(If(salvat,
                "Motorul PDF a fost salvat (kbot_paths.json). Documentul următor îl folosește.",
                "Motorul PDF s-a aplicat pentru sesiunea curentă, dar nu a putut fi salvat. Detalii în jurnalul de erori."))

            If motor.Engine = AdobePreviewEngine.WindowHost Then DeschideOptiunileGazduirii()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.CboAdobeMotor_SelectedIndexChanged", ex)
            RaiseEvent StatusChanged("Setările Adobe nu au putut fi salvate: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnAdobeGazduire_Click(sender As Object, e As EventArgs) Handles btnAdobeGazduire.Click
        Try
            DeschideOptiunileGazduirii()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.BtnAdobeGazduire_Click", ex)
            RaiseEvent StatusChanged("Fereastra de opțiuni nu a putut fi deschisă: " & ex.Message)
        End Try
    End Sub

    ' Modal, owned by the settings window; the dialog writes its own stores and hands back one
    ' line for the band. Abandoned = nothing changed, and the band says nothing.
    Private Sub DeschideOptiunileGazduirii()
        Using dlg As New AdobeGazduireForm()
            If dlg.ShowDialog(FindForm()) = DialogResult.OK Then RaiseEvent StatusChanged(dlg.Rezumat)
        End Using
    End Sub

    Private Sub CboExcelRibbon_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboExcelRibbon.SelectedIndexChanged
        Dim item As RibbonItem = TryCast(cboExcelRibbon.SelectedItem, RibbonItem)
        If item Is Nothing Then Return
        SalveazaComutator(Sub(s) s.ExcelRibbon = OfficeHostSettings.ExcelRibbonToText(item.Mode),
                          "Panglica Excel: " & item.ToString() & ". Se aplică documentului următor.")
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
            For Each caption As Label In New Label() {lblVerbose, lblAdobeMotor, lblExcelRibbon}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnAdobeGazduire, scheme)
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
