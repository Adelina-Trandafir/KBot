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
''' The settings that only matter for the HOSTED Adobe window are NOT on the page
''' (slice 0072-01): choosing «Fereastră găzduită» opens <see cref="AdobeGazduireForm"/>,
''' and the «Opțiuni…» button under the combo reopens it later.</para>
'''
''' <para><b>Switches save on change; folders save on the button.</b> A switch is one value
''' and its effect is immediate. The folder grid is a set edited cell by cell, validated at
''' STARTUP (a path that cannot be written stops the launch), so it is written once, on
''' purpose, and the page says a restart is needed.</para>
'''
''' <para><b>The «Generale» tab is grouped by purpose (slice 0100):</b> «Fereastra principală» (how the
''' main window starts and its menu), «FOREXE» (the console, the browser button, the reception picker),
''' «Avansat» (the advanced options and what hangs off them) and -- only with the advanced options on --
''' «Descărcări multiple» (several FOREXE tabs at once: the switch, how many, the update of the old
''' angajamente on connection, and «actualizează implicit toate recepțiile»). Regrouping changed no behaviour.</para>
'''
''' <para><b>Three tabs (slice 0777).</b> A horizontal <see cref="KBotNavList"/> on top, like the
''' vertical one of the settings window: «Generale» (the switches), «Documente» (PDF / Excel)
''' and «KBOT» (the main tree: its order and, per order, the CODANGAJAMENT / SURSE columns --
''' the same keys the menu of the tree header's icon writes). The page follows
''' <see cref="AppSettings.Changed"/> so a choice made in that menu shows here at once.</para>
''' </summary>
Public Class SetariAplicatieView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    ' Guards the change handlers while the page fills its controls from the stores.
    Private _suppress As Boolean

    ' Tab keys of navPagini (authored in the designer).
    Private Const PAGE_GENERALE As String = "generale"
    Private Const PAGE_DOCUMENTE As String = "documente"
    Private Const PAGE_KBOT As String = "kbot"

    Public Sub New()
        InitializeComponent()
        BuildCombos()
        ' Raises SelectionChanged, which shows the first tab.
        navPagini.SelectedKey = PAGE_GENERALE
    End Sub

    ' ---------------- tabs ----------------

    Private Sub NavPagini_SelectionChanged(key As String) Handles navPagini.SelectionChanged
        Try
            Dim page As Control
            Select Case key
                Case PAGE_GENERALE : page = tlyGenerale
                Case PAGE_DOCUMENTE : page = tlyPaginaDocumente
                Case PAGE_KBOT : page = tlyPaginaKbot
                Case Else
                    ' No silent no-ops: a tab added in the designer and forgotten here must show.
                    Throw New ArgumentException("Pagină necunoscută în setările aplicației: «" & key & "».")
            End Select
            For Each p As Control In New Control() {tlyGenerale, tlyPaginaDocumente, tlyPaginaKbot}
                p.Visible = ReferenceEquals(p, page)
            Next
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("SetariAplicatieView.NavPagini_SelectionChanged", ex)
        End Try
    End Sub

    ' The shared store can change under the page (the tree header's menu): follow it while
    ' the page has a window. AppSettings is static, so the subscription ends with the handle.
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        AddHandler AppSettings.Changed, AddressOf AppSettings_Changed
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        RemoveHandler AppSettings.Changed, AddressOf AppSettings_Changed
        MyBase.OnHandleDestroyed(e)
    End Sub

    Private Sub AppSettings_Changed(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf UrmeazaSetarile))
            Else
                UrmeazaSetarile()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.AppSettings_Changed", ex)
        End Try
    End Sub

    ' What follows the shared store: the tree tab and the advanced-options switch.
    Private Sub UrmeazaSetarile()
        IncarcaArborele()
        Dim before As Boolean = _suppress
        _suppress = True
        Try
            chkAvansate.Checked = AppSettings.Current.AdvancedOptions
            AplicaOptiunileAvansate(AppSettings.Current.AdvancedOptions)
            ' Slice 0097-02: the tour switches itself off when it was seen to the end.
            chkTurInitial.Checked = AppSettings.Current.ShowInitialTour
        Finally
            _suppress = before
        End Try
    End Sub

    ' ---------------- advanced options ----------------

    ' The documents tab is an advanced page (operator, 24.09.2026). Hidden while it is the
    ' selected tab -> back to the switches, so the page never shows a tab without its button.
    Private Sub AplicaOptiunileAvansate(shown As Boolean)
        navPagini.SetItemVisible(PAGE_DOCUMENTE, shown)
        chkCapturi.Visible = shown   ' slice 0000-02: help capture mode lives under the advanced options
        ' Slice 0100: the whole multi-download group is an advanced one (operator, 01.10.2026).
        For Each c As Control In New Control() {lblGrupDescarcari, chkMultiThread, lblFire, txtFire,
                                                chkAutoVechi, lblZile, txtZile, chkToateReceptiile}
            c.Visible = shown
        Next
        If Not shown AndAlso String.Equals(navPagini.SelectedKey, PAGE_DOCUMENTE, StringComparison.Ordinal) Then
            navPagini.SelectedKey = PAGE_GENERALE
        End If
    End Sub

    ' Ticking asks for the password; a wrong or abandoned prompt puts the tick back. Unticking
    ' needs nothing. The settings window follows AppSettings.Changed for its own pages.
    Private Sub ChkAvansate_CheckedChanged(sender As Object, e As EventArgs) Handles chkAvansate.CheckedChanged
        Try
            If _suppress Then Return
            If chkAvansate.Checked Then
                Dim ok As Boolean
                Using dlg As New ParolaAvansataForm()
                    ok = dlg.ShowDialog(FindForm()) = DialogResult.OK
                End Using
                If Not ok Then
                    _suppress = True
                    Try
                        chkAvansate.Checked = False
                    Finally
                        _suppress = False
                    End Try
                    RaiseEvent StatusChanged("Opțiunile avansate au rămas oprite.")
                    Return
                End If
            End If
            SalveazaComutator(Sub(s) s.AdvancedOptions = chkAvansate.Checked,
                              If(chkAvansate.Checked, "Opțiunile avansate sunt active.", "Opțiunile avansate sunt ascunse."))
            AplicaOptiunileAvansate(AppSettings.Current.AdvancedOptions)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.ChkAvansate_CheckedChanged", ex)
        End Try
    End Sub

    ' Slice 0000-02: the help screenshot mode (menu entry + «Capturi pentru ajutor» window).
    Private Sub ChkCapturi_CheckedChanged(sender As Object, e As EventArgs) Handles chkCapturi.CheckedChanged
        SalveazaComutator(Sub(s) s.HelpCaptureMode = chkCapturi.Checked,
                          If(chkCapturi.Checked, "Modul de capturi pentru ajutor e pornit.", "Modul de capturi pentru ajutor e oprit."))
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
            IncarcaArborele()
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

            ' The plain ActiveX engine is no longer offered (the code behind it stays). Only the Read Mode one is.
            For Each g As AdobePreviewEngine In New AdobePreviewEngine() {AdobePreviewEngine.WindowHost, AdobePreviewEngine.ActiveXReadMode}
                cboAdobeMotor.Items.Add(New AdobeEngineItem(g))
            Next
            For Each r As ExcelRibbonMode In New ExcelRibbonMode() {ExcelRibbonMode.HideDockWindow, ExcelRibbonMode.Excel4Macro}
                cboExcelRibbon.Items.Add(New RibbonItem(r))
            Next

            cboSortare.Items.Add(New SortItem(AppSettings.TreeSortName, "După nume"))
            cboSortare.Items.Add(New SortItem(AppSettings.TreeSortDate, "După data creării"))
            ' Index 0 = ascending, 1 = descending (read back by index in IncarcaArborele).
            cboOrdine.Items.Add("Crescătoare")
            cboOrdine.Items.Add("Descrescătoare")
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
            chkStartMaximized.Checked = s.StartMaximized
            chkTurInitial.Checked = s.ShowInitialTour
            chkAvansate.Checked = s.AdvancedOptions
            chkCapturi.Checked = s.HelpCaptureMode
            chkMultiThread.Checked = s.MultiThreadDownloads
            txtFire.Text = s.DownloadThreadsInEffect.ToString(Globalization.CultureInfo.InvariantCulture)
            chkAutoVechi.Checked = s.AutoUpdateOnConnect
            txtZile.Text = s.AutoUpdateDaysInEffect.ToString(Globalization.CultureInfo.InvariantCulture)
            chkToateReceptiile.Checked = s.UpdateAllReceptiiByDefault
            ActualizeazaDisponibilitateaDescarcarilor()
            AplicaOptiunileAvansate(s.AdvancedOptions)
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
            IncarcaArborele()
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
                          If(chkLogViewer.Checked, "Rândul «Jurnal activitate» este oferit.", "Rândul «Jurnal activitate» este ascuns."))
    End Sub

    Private Sub ChkShowBrowser_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowBrowser.CheckedChanged
        SalveazaComutator(Sub(s) s.ShowBrowserButton = chkShowBrowser.Checked,
                          If(chkShowBrowser.Checked, "Butonul «Arată browserul» este oferit.", "Butonul «Arată browserul» este ascuns."))
    End Sub

    Private Sub ChkReceptii_CheckedChanged(sender As Object, e As EventArgs) Handles chkReceptii.CheckedChanged
        SalveazaComutator(Sub(s) s.ReceptiiCheckedOnOpen = chkReceptii.Checked,
                          If(chkReceptii.Checked, "Selectorul de recepții pornește cu tot bifat.", "Selectorul de recepții pornește gol."))
    End Sub

    ' Slice 0097-02: the main window opens maximized. Read once, when the window opens.
    Private Sub ChkStartMaximized_CheckedChanged(sender As Object, e As EventArgs) Handles chkStartMaximized.CheckedChanged
        SalveazaComutator(Sub(s) s.StartMaximized = chkStartMaximized.Checked,
                          If(chkStartMaximized.Checked,
                             "Fereastra principală va porni mărită pe tot ecranul. Se aplică de la următoarea pornire.",
                             "Fereastra principală va porni la mărimea ei obișnuită. Se aplică de la următoarea pornire."))
    End Sub

    ' Slice 0097-02: the main window's tour at every start, until seen to the end or switched off.
    Private Sub ChkTurInitial_CheckedChanged(sender As Object, e As EventArgs) Handles chkTurInitial.CheckedChanged
        SalveazaComutator(Sub(s) s.ShowInitialTour = chkTurInitial.Checked,
                          If(chkTurInitial.Checked,
                             "Turul ferestrei principale va porni singur la următoarea pornire a K-BOT.",
                             "Turul ferestrei principale nu mai pornește singur. Îl găsești oricând la «?»."))
    End Sub

    ' ---------------- multiple downloads (slice 0100) ----------------

    ' What hangs off the main switch is enabled only while the switch is on; the days field also
    ' needs its own box. Kept visible (not hidden) so the operator sees what the switch brings.
    Private Sub ActualizeazaDisponibilitateaDescarcarilor()
        Dim pornit As Boolean = chkMultiThread.Checked
        lblFire.Enabled = pornit
        txtFire.Enabled = pornit
        chkAutoVechi.Enabled = pornit
        chkToateReceptiile.Enabled = pornit
        lblZile.Enabled = pornit AndAlso chkAutoVechi.Checked
        txtZile.Enabled = pornit AndAlso chkAutoVechi.Checked
    End Sub

    Private Sub ChkMultiThread_CheckedChanged(sender As Object, e As EventArgs) Handles chkMultiThread.CheckedChanged
        ActualizeazaDisponibilitateaDescarcarilor()
        SalveazaComutator(Sub(s) s.MultiThreadDownloads = chkMultiThread.Checked,
                          If(chkMultiThread.Checked,
                             "Descărcarea pe mai multe taburi FOREXE e pornită (cel mult " & AppSettings.Current.DownloadThreadsInEffect & " deodată).",
                             "Descărcările merg din nou una câte una."))
    End Sub

    Private Sub ChkAutoVechi_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoVechi.CheckedChanged
        ActualizeazaDisponibilitateaDescarcarilor()
        SalveazaComutator(Sub(s) s.AutoUpdateOnConnect = chkAutoVechi.Checked,
                          If(chkAutoVechi.Checked,
                             "La conectare se actualizează angajamentele neactualizate de " & AppSettings.Current.AutoUpdateDaysInEffect & " zile.",
                             "La conectare nu se mai actualizează nimic singur."))
    End Sub

    Private Sub ChkToateReceptiile_CheckedChanged(sender As Object, e As EventArgs) Handles chkToateReceptiile.CheckedChanged
        SalveazaComutator(Sub(s) s.UpdateAllReceptiiByDefault = chkToateReceptiile.Checked,
                          If(chkToateReceptiile.Checked,
                             "Cât timp descărcarea pe mai multe taburi e pornită, se citesc toate recepțiile, fără întrebare.",
                             "Alegerea recepțiilor se face ca până acum."))
    End Sub

    ' The two numbers save when the field is left or on Enter -- not on every keystroke.
    Private Sub TxtFire_Leave(sender As Object, e As EventArgs) Handles txtFire.Leave
        SalveazaNumarul(txtFire, "numărul de taburi", AppSettings.DownloadThreadsMin, AppSettings.DownloadThreadsMax,
                        Function(s) s.DownloadThreadsInEffect, Sub(s, n) s.DownloadThreads = n,
                        "Se descarcă cel mult {0} angajamente deodată.")
    End Sub

    Private Sub TxtZile_Leave(sender As Object, e As EventArgs) Handles txtZile.Leave
        SalveazaNumarul(txtZile, "numărul de zile", 1, AppSettings.AutoUpdateDaysMax,
                        Function(s) s.AutoUpdateDaysInEffect, Sub(s, n) s.AutoUpdateDays = n,
                        "La conectare se actualizează angajamentele neactualizate de {0} zile.")
    End Sub

    Private Sub TxtDescarcari_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtFire.FieldKeyDown, txtZile.FieldKeyDown
        Try
            If e.KeyCode <> Keys.Enter Then Return
            e.SuppressKeyPress = True
            If ReferenceEquals(sender, txtFire) Then
                TxtFire_Leave(sender, EventArgs.Empty)
            Else
                TxtZile_Leave(sender, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.TxtDescarcari_FieldKeyDown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Validates one whole-number field of the multiple-downloads group and saves it. Out of range
    ''' or not a number -> the band says why and the field goes back to the stored value.
    ''' </summary>
    Private Sub SalveazaNumarul(field As KBotTextField, ce As String, minim As Integer, maxim As Integer,
                                read As Func(Of AppSettings, Integer), write As Action(Of AppSettings, Integer),
                                mesajFormat As String)
        Try
            If _suppress Then Return
            Dim stored As Integer = read(AppSettings.Current)
            Dim raw As String = If(field.Text, String.Empty).Trim()
            Dim asked As Integer
            If Not Integer.TryParse(raw, Globalization.NumberStyles.None,
                                    Globalization.CultureInfo.InvariantCulture, asked) OrElse
               asked < minim OrElse asked > maxim Then
                RaiseEvent StatusChanged("Pentru " & ce & " trebuie un număr între " & minim & " și " & maxim &
                                         ". A rămas " & stored & ".")
                IncarcaComutatoarele()
                Return
            End If
            If asked = stored Then Return
            SalveazaComutator(Sub(s) write(s, asked), String.Format(mesajFormat, asked))
        Catch ex As Exception
            ' Reached from Leave / KeyDown only: UI boundary, log and swallow.
            GlobalErrorLog.Write("SetariAplicatieView.SalveazaNumarul", ex)
        End Try
    End Sub

    ' ---------------- documents ----------------

    Private Sub IncarcaDocumentele()
        _suppress = True
        Try
            SelecteazaMotor(AdobeViewerSettings.CurrentEngine().Value)
            SelecteazaPanglica(OfficeHostSettings.CurrentExcelRibbon().Value)
            chkAcroTrace.Checked = AcroPdfTraceLog.SwitchedOn
            chkAcroNou.Checked = AppSettings.Current.AcroPdfFreshControl
            chkAdobeClasic.Checked = AppSettings.Current.AdobeClassicUi
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

    ' The «Opțiuni…» button is for the HOSTED WINDOW only; on ActiveX the settings behind
    ' it do nothing, and a button that opens a dialog which changes nothing would look like it
    ' acts (same rule as DdfDocumentPage). The reverse holds for the ActiveX rows and the script
    ' messages button: they are shown only while an ActiveX engine is the one in use.
    Private Sub ActualizeazaDisponibilitateaAdobe()
        Dim activeX As Boolean = MotorulCurent() <> AdobePreviewEngine.WindowHost
        btnAdobeGazduire.Enabled = Not activeX
        chkAcroTrace.Visible = activeX
        chkAcroNou.Visible = activeX
        btnMesajeAdobe.Visible = activeX
    End Sub

    ' The engine picked in the combo; with nothing picked (the stored engine is one the combo no
    ' longer offers) the stored one.
    Private Function MotorulCurent() As AdobePreviewEngine
        Dim motor As AdobeEngineItem = TryCast(cboAdobeMotor.SelectedItem, AdobeEngineItem)
        Return If(motor Is Nothing, AdobeViewerSettings.CurrentEngine().Value, motor.Engine)
    End Function

    Private Function MotorulEsteFereastra() As Boolean
        Return MotorulCurent() = AdobePreviewEngine.WindowHost
    End Function

    ''' <summary>
    ''' The engine saves into kbot_paths.json next to the two values it does not own (Persist
    ''' writes all three; they are read back from the store, untouched). Choosing the hosted
    ''' window then opens the dialog with its settings -- the operator's request: they
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

    ' The list of Adobe script alerts closed automatically (AdobeMesajeForm writes it itself).
    Private Sub BtnMesajeAdobe_Click(sender As Object, e As EventArgs) Handles btnMesajeAdobe.Click
        Try
            Using dlg As New AdobeMesajeForm()
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then RaiseEvent StatusChanged(dlg.Rezumat)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.BtnMesajeAdobe_Click", ex)
            RaiseEvent StatusChanged("Lista mesajelor Adobe nu a putut fi deschisă: " & ex.Message)
        End Try
    End Sub

    ' Modal, owned by the settings window; the dialog writes its own stores and hands back one
    ' line for the band. Abandoned = nothing changed, and the band says nothing.
    Private Sub DeschideOptiunileGazduirii()
        Using dlg As New AdobeGazduireForm()
            If dlg.ShowDialog(FindForm()) = DialogResult.OK Then RaiseEvent StatusChanged(dlg.Rezumat)
        End Using
    End Sub

    ' The ActiveX viewer's exhaustive trace (AcroPdfTraceLog). NOT saved: held in memory for this
    ' run only, so every start of the application begins with it off (operator, 24.09.2026).
    ' It records from the next document load until that document is open or a blocking error.
    Private Sub ChkAcroTrace_CheckedChanged(sender As Object, e As EventArgs) Handles chkAcroTrace.CheckedChanged
        Try
            If _suppress Then Return
            AcroPdfTraceLog.SwitchedOn = chkAcroTrace.Checked
            RaiseEvent StatusChanged(If(chkAcroTrace.Checked,
                "Jurnalul de diagnostic ActiveX este pornit până la închiderea aplicației: Logs\" &
                AcroPdfTraceLog.FileNameOnly & ", de la următoarea deschidere de document.",
                "Jurnalul de diagnostic ActiveX este oprit."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.ChkAcroTrace_CheckedChanged", ex)
        End Try
    End Sub

    ' Adobe starts in its classic interface (slice 0078-10). Saved; AdobeUiPreference writes the
    ' registry value when the next document opens, and puts it back at exit. Switching it off
    ' puts the operator's own value back at once.
    Private Sub ChkAdobeClasic_CheckedChanged(sender As Object, e As EventArgs) Handles chkAdobeClasic.CheckedChanged
        If _suppress Then Return
        SalveazaComutator(Sub(s) s.AdobeClassicUi = chkAdobeClasic.Checked,
                          If(chkAdobeClasic.Checked,
                             "Adobe va porni în interfața clasică, de la următorul document deschis.",
                             "Adobe nu mai este pus pe interfața clasică. Valoarea ta din Adobe a fost pusă la loc."))
        If Not chkAdobeClasic.Checked Then AdobeUiPreference.Restore(Nothing)
    End Sub

    ' ActiveX: a new AcroPDF control for every document asked for (DDF and ORD). Saved.
    Private Sub ChkAcroNou_CheckedChanged(sender As Object, e As EventArgs) Handles chkAcroNou.CheckedChanged
        If _suppress Then Return
        SalveazaComutator(Sub(s) s.AcroPdfFreshControl = chkAcroNou.Checked,
                          If(chkAcroNou.Checked,
                             "ActiveX: fiecare document nou se deschide într-un control Adobe nou.",
                             "ActiveX: documentele se încarcă în același control Adobe."))
    End Sub

    Private Sub CboExcelRibbon_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboExcelRibbon.SelectedIndexChanged
        Dim item As RibbonItem = TryCast(cboExcelRibbon.SelectedItem, RibbonItem)
        If item Is Nothing Then Return
        SalveazaComutator(Sub(s) s.ExcelRibbon = OfficeHostSettings.ExcelRibbonToText(item.Mode),
                          "Panglica Excel: " & item.ToString() & ". Se aplică documentului următor.")
    End Sub

    ' ---------------- KBOT: the main tree (slice 0777) ----------------

    ' Also reached from AppSettings.Changed, i.e. possibly from inside SalveazaComutator's
    ' own Save: the previous suppress state is put back, not forced to False.
    Private Sub IncarcaArborele()
        Dim before As Boolean = _suppress
        _suppress = True
        Try
            Dim s As AppSettings = AppSettings.Current
            Dim wanted As String = If(s.TreeSortIsDate, AppSettings.TreeSortDate, AppSettings.TreeSortName)
            For i As Integer = 0 To cboSortare.Items.Count - 1
                If DirectCast(cboSortare.Items(i), SortItem).Value = wanted Then cboSortare.SelectedIndex = i : Exit For
            Next
            cboOrdine.SelectedIndex = If(s.TreeSortDescending, 1, 0)
            chkNumeCod.Checked = s.TreeNameShowCod
            chkNumeSurse.Checked = s.TreeNameShowSurse
            chkDataCod.Checked = s.TreeDateShowCod
            chkDataSurse.Checked = s.TreeDateShowSurse
            txtLatimeCod.Text = s.TreeCodColumnWidth.ToString(Globalization.CultureInfo.InvariantCulture)
            txtLatimeSurse.Text = s.TreeSurseColumnWidth.ToString(Globalization.CultureInfo.InvariantCulture)
        Finally
            _suppress = before
        End Try
    End Sub

    Private Sub CboOrdine_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboOrdine.SelectedIndexChanged
        If cboOrdine.SelectedIndex < 0 Then Return
        Dim descending As Boolean = (cboOrdine.SelectedIndex = 1)
        SalveazaComutator(Sub(s) s.TreeSortDescending = descending,
                          "Arborele de angajamente se ordonează " & If(descending, "descrescător", "crescător") & ".")
    End Sub

    Private Sub CboSortare_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSortare.SelectedIndexChanged
        Dim item As SortItem = TryCast(cboSortare.SelectedItem, SortItem)
        If item Is Nothing Then Return
        SalveazaComutator(Sub(s) s.TreeSort = item.Value,
                          "Arborele de angajamente se sortează " & item.ToString().ToLowerInvariant() & ".")
    End Sub

    Private Sub ChkNumeCod_CheckedChanged(sender As Object, e As EventArgs) Handles chkNumeCod.CheckedChanged
        SalveazaComutator(Sub(s) s.TreeNameShowCod = chkNumeCod.Checked,
                          ColumnMessage("CODANGAJAMENT", "după nume", chkNumeCod.Checked))
    End Sub

    Private Sub ChkNumeSurse_CheckedChanged(sender As Object, e As EventArgs) Handles chkNumeSurse.CheckedChanged
        SalveazaComutator(Sub(s) s.TreeNameShowSurse = chkNumeSurse.Checked,
                          ColumnMessage("SURSE", "după nume", chkNumeSurse.Checked))
    End Sub

    Private Sub ChkDataCod_CheckedChanged(sender As Object, e As EventArgs) Handles chkDataCod.CheckedChanged
        SalveazaComutator(Sub(s) s.TreeDateShowCod = chkDataCod.Checked,
                          ColumnMessage("CODANGAJAMENT", "după dată", chkDataCod.Checked))
    End Sub

    Private Sub ChkDataSurse_CheckedChanged(sender As Object, e As EventArgs) Handles chkDataSurse.CheckedChanged
        SalveazaComutator(Sub(s) s.TreeDateShowSurse = chkDataSurse.Checked,
                          ColumnMessage("SURSE", "după dată", chkDataSurse.Checked))
    End Sub

    ' The widths save when the field is left or on Enter -- not on every keystroke, where
    ' «1» on the way to «140» would already re-lay the tree with a 1 px column.
    Private Sub TxtLatimeCod_Leave(sender As Object, e As EventArgs) Handles txtLatimeCod.Leave
        SalveazaLatimea(txtLatimeCod, "CODANGAJAMENT",
                        Function(s) s.TreeCodColumnWidth, Sub(s, w) s.TreeCodColumnWidth = w)
    End Sub

    Private Sub TxtLatimeSurse_Leave(sender As Object, e As EventArgs) Handles txtLatimeSurse.Leave
        SalveazaLatimea(txtLatimeSurse, "SURSE",
                        Function(s) s.TreeSurseColumnWidth, Sub(s, w) s.TreeSurseColumnWidth = w)
    End Sub

    Private Sub TxtLatime_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtLatimeCod.FieldKeyDown, txtLatimeSurse.FieldKeyDown
        Try
            If e.KeyCode <> Keys.Enter Then Return
            e.SuppressKeyPress = True
            If ReferenceEquals(sender, txtLatimeCod) Then
                TxtLatimeCod_Leave(sender, EventArgs.Empty)
            Else
                TxtLatimeSurse_Leave(sender, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.TxtLatime_FieldKeyDown", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Validates one width field and saves it. Not a whole number in range -> the band says
    ''' why and the field goes back to the stored value. Unchanged -> nothing is written.
    ''' </summary>
    Private Sub SalveazaLatimea(field As KBotTextField, column As String,
                                read As Func(Of AppSettings, Integer),
                                write As Action(Of AppSettings, Integer))
        Try
            If _suppress Then Return
            Dim stored As Integer = read(AppSettings.Current)
            Dim raw As String = If(field.Text, String.Empty).Trim()
            Dim asked As Integer
            If Not Integer.TryParse(raw, Globalization.NumberStyles.None,
                                    Globalization.CultureInfo.InvariantCulture, asked) OrElse
               Not AppSettings.IsValidTreeColumnWidth(asked) Then
                RaiseEvent StatusChanged("Lățimea coloanei " & column & " trebuie să fie un număr între " &
                                         AppSettings.TreeColumnWidthMin & " și " & AppSettings.TreeColumnWidthMax &
                                         ". A rămas " & stored & ".")
                UrmeazaSetarile()
                Return
            End If
            If asked = stored Then Return
            SalveazaComutator(Sub(s) write(s, asked),
                              "Coloana " & column & " are acum " & asked & " px.")
        Catch ex As Exception
            ' Reached from Leave / KeyDown only: UI boundary, log and swallow.
            GlobalErrorLog.Write("SetariAplicatieView.SalveazaLatimea", ex)
        End Try
    End Sub

    Private Shared Function ColumnMessage(column As String, sortText As String, shown As Boolean) As String
        Return "Coloana " & column & " este " & If(shown, "afișată", "ascunsă") &
               " la sortarea " & sortText & "."
    End Function

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            For Each t As Control In New Control() {tlyGenerale, tlyComutatoare, tlyPaginaDocumente, tlyDocumente,
                                                   tlyPaginaKbot, tlyArbore}
                t.BackColor = p.SurfaceAltColor
            Next
            For Each caption As Label In New Label() {lblVerbose, lblFire, lblZile, lblAdobeMotor, lblExcelRibbon,
                                                      lblSortare, lblOrdine, lblColoaneNume, lblColoaneData,
                                                      lblLatimeCod, lblLatimeSurse}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnAdobeGazduire, scheme)
            ButtonStyles.ApplySecondary(btnMesajeAdobe, scheme)
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

    Private NotInheritable Class SortItem
        Public ReadOnly Property Value As String
        Private ReadOnly _label As String

        Public Sub New(value As String, label As String)
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

''' <summary>O intrare din combo-ul «Motor previzualizare». POCO -&gt; fără Try/Catch.</summary>
Friend NotInheritable Class AdobeEngineItem
    Public ReadOnly Property Engine As AdobePreviewEngine

    Public Sub New(engine As AdobePreviewEngine)
        Me.Engine = engine
    End Sub

    Public Overrides Function ToString() As String
        Return AdobeViewerSettings.EngineLabel(Engine)
    End Function
End Class

''' <summary>O intrare din combo-ul «Instanță nouă Adobe». POCO -&gt; fără Try/Catch.</summary>
Friend NotInheritable Class AdobeNewInstanceItem
    Public ReadOnly Property Mode As AdobeNewInstanceMode

    Public Sub New(mode As AdobeNewInstanceMode)
        Me.Mode = mode
    End Sub

    Public Overrides Function ToString() As String
        Return AdobeViewerSettings.NewInstanceLabel(Mode)
    End Function
End Class
