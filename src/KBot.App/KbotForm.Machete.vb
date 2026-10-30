Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' The shell's part of the macheta registry (see <c>MachetaRegistry</c>): it registers the kinds it
''' owns and knows how to open them again. Every macheta -- a page, an editor, a nomenclature, the
''' E-Factura list -- reports to the history. Pages (the lists, the nomenclatures, the settings, the help, E-Factura)
''' are shown in the work area of the main window (<c>AvacontMain</c>) and stay open when another one is asked
''' for (slice 0113-01); the editors and the windows the operator must answer (DDF / ORD, the reception links, the
''' uncorrelated operations, the reservations chart, the groups) are modal windows of their own. Small dialogs
''' (a choice, a confirmation, a file picker) are not machete.
''' </summary>
''' <remarks>
''' Reopening: the pages and windows open as before. A view of the left navigation, and the editors /
''' child windows that hang from a view, are reopened by bringing the shell to the front, switching
''' the year when the macheta is from another one, selecting its angajament in the tree and opening
''' the view -- the editors reopen to the DDF / ORD view of that angajament, from where the operator
''' opens the document again (a draft is not something a history can resume).
''' </remarks>
Partial Public Class KbotForm

    Friend Const MachetaExtrase As String = "extrase"
    Friend Const MachetaGrupe As String = "grupe"
    Friend Const MachetaSetari As String = "setari"
    Friend Const MachetaJurnal As String = "jurnal"
    Friend Const MachetaClasificatii As String = "clasificatii"
    Friend Const MachetaParteneri As String = "parteneri"
    Friend Const MachetaPlanCont As String = "plan_cont"
    Friend Const MachetaSabloane As String = "sabloane"
    Friend Const MachetaDdfEdit As String = "ddf.edit"
    Friend Const MachetaOrdEdit As String = "ord.edit"
    Friend Const MachetaAsociere As String = "asociere"
    Friend Const MachetaCabNote As String = "cabnote"
    Friend Const MachetaCabOmise As String = "cabnote.omise"
    Friend Const MachetaGrafic As String = "grafic.rezervari"
    Friend Const MachetaMesaje As String = "mesaje"
    Friend Const MachetaAjutor As String = "ajutor"
    Friend Const MachetaAngajamente As String = "angajamente"
    Private Const ViewPrefix As String = "view."

    ''' <summary>The registry this shell reports to (set by <see cref="RegisterMachete"/>); Nothing when it runs on its own.</summary>
    Friend Property Machete As MachetaRegistry

    ''' <summary>Registers the kinds of machete of the shell. Called once by the main window.</summary>
    Friend Sub RegisterMachete(k_registry As MachetaRegistry)
        ArgumentNullException.ThrowIfNull(k_registry)
        Machete = k_registry

        ' Pages and windows: opened by the code that already opens them (which reports to the history itself).
        k_registry.Register(New MachetaType(MachetaExtrase, "Extrase de cont", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideExtrasele)))
        k_registry.Register(New MachetaType(MachetaGrupe, "Grupe de angajamente", "folder_open", Function(k_ref) OpenPage(AddressOf DeschideEditorulGrupe), Nothing, False))
        k_registry.Register(New MachetaType(MachetaSetari, "Configurare K-BOT", "settings", Function(k_ref) OpenPage(AddressOf ShowSettings)))
        k_registry.Register(New MachetaType(MachetaJurnal, "Jurnal activitate", "folder", Function(k_ref) OpenPage(AddressOf ShowLog)))
        k_registry.Register(New MachetaType(MachetaClasificatii, "Clasificații cheltuieli", "cells", Function(k_ref) OpenPage(AddressOf DeschideClasificatiile),
                                            Function(k_ref) If(k_ref.Arg("an").Length > 0, "Clasificații cheltuieli · " & k_ref.Arg("an"), "Clasificații cheltuieli")))
        k_registry.Register(New MachetaType(MachetaParteneri, "Parteneri", "binvoice", Function(k_ref) OpenPage(AddressOf DeschidePartenerii)))
        k_registry.Register(New MachetaType(MachetaPlanCont, "Plan de conturi", "cells", Function(k_ref) OpenPage(AddressOf DeschidePlanCont)))
        k_registry.Register(New MachetaType(MachetaEFacturaVanzare, "E-Factura Vânzare", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideEFactura)))
        k_registry.Register(New MachetaType(MachetaEFacturaCumparare, "E-Factura Cumpărare", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideEFacturaCumparare)))
        k_registry.Register(New MachetaType(MachetaEFacturaReceptii, "E-Factura Recepții", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideEFacturaReceptii)))
        k_registry.Register(New MachetaType(MachetaEFacturaVercon, "E-Factura Trimite la Vercon", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideEFacturaVercon)))
        k_registry.Register(New MachetaType(MachetaAdechitRapoarte, "ADECHIT Rapoarte", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideAdechitRapoarte)))
        k_registry.Register(New MachetaType(MachetaVerconRapoarte, "VERCON Rapoarte", "binvoice", Function(k_ref) OpenPage(AddressOf DeschideVerconRapoarte)))
        k_registry.Register(New MachetaType(MachetaIstoricVersiuni,"Istoric versiuni", "istoric", Function(k_ref) OpenPage(AddressOf DeschideIstoricVersiuni)))
        k_registry.Register(New MachetaType(MachetaAjutor, "Ajutor", "folder", AddressOf OpenHelpMachetaAsync))
        k_registry.Register(New MachetaType(MachetaAngajamente, "Lista angajamentelor", "folder_open", AddressOf OpenListaMachetaAsync))
        k_registry.Register(New MachetaType(MachetaCabNote, "Operațiuni necorelate", "FX_RED_16", AddressOf OpenCabNotesMachetaAsync, Nothing, False))
        k_registry.Register(New MachetaType(MachetaCabOmise, "Operațiuni omise", "FX_RED_16", AddressOf OpenCabSkippedMachetaAsync, Nothing, False))
        k_registry.Register(New MachetaType(MachetaMesaje, "Catalog de mesaje", "folder", Function(k_ref) OpenPage(AddressOf DeschideCatalogulDeMesaje), Nothing, False))

        ' Editors and child windows of one angajament.
        k_registry.Register(New MachetaType(MachetaDdfEdit, "Editor DDF", "ddf", ReopenInView("ddf"), TitleWithCod("Editor DDF"), False))
        k_registry.Register(New MachetaType(MachetaOrdEdit, "Editor ORD", "ord", ReopenInView("ord"), TitleWithCod("Editor ORD"), False))
        k_registry.Register(New MachetaType(MachetaAsociere, "Legăturile recepțiilor", "receptii", AddressOf OpenAsociereMachetaAsync, TitleWithCod("Legăturile recepțiilor"), False))
        k_registry.Register(New MachetaType(MachetaGrafic, "Graficul rezervărilor", "rezervari", ReopenInView("rezervari"), TitleWithCod("Graficul rezervărilor"), False))

        ' One kind per view of the left navigation.
        For Each k_item As KBotNavItem In navViews.Items
            If k_item.IsSeparator OrElse String.IsNullOrEmpty(k_item.Key) Then Continue For
            Dim k_text As String = k_item.Text
            k_registry.Register(New MachetaType(ViewPrefix & k_item.Key, k_text, k_item.Key,
                                                AddressOf OpenViewMachetaAsync, TitleWithCod(k_text), False))   ' slice 0113-01: a view is not a line of «Machete deschise»
        Next
    End Sub

    ' A page opens through the code that already opens it; that code reports to the history itself. Shown = it did.
    Private Function OpenPage(k_open As Action) As Task(Of Boolean)
        Dim k_notesBefore As Integer = Machete.NoteCount
        k_open()
        Return Task.FromResult(Machete.NoteCount <> k_notesBefore)
    End Function

    ''' <summary>
    ''' Every opening of a macheta from a menu or a ribbon button goes through the registry, which records it. Without a
    ''' registry (the shell running alone) the opener runs directly.
    ''' </summary>
    Friend Async Sub OpenRegistered(k_typeKey As String, k_direct As Action)
        Try
            If Machete Is Nothing Then
                k_direct()
            Else
                Await Machete.OpenAsync(MachetaRef.Create(k_typeKey)).ConfigureAwait(True)
            End If
        Catch ex As Exception
            ' UI boundary (async Sub from a menu / ribbon click): log and swallow; the opener told the operator what it had to.
            GlobalErrorLog.Write("KbotForm.OpenRegistered", ex)
        End Try
    End Sub

    ' «Title · cod» (or the title alone when the reference has no angajament).
    Private Shared Function TitleWithCod(k_title As String) As Func(Of MachetaRef, String)
        Return Function(k_ref) If(k_ref.Arg("cod").Length > 0, k_title & " · " & k_ref.Arg("cod"), k_title)
    End Function

    ' The opener of a macheta that hangs from a view: the view of that angajament.
    Private Function ReopenInView(k_viewKey As String) As Func(Of MachetaRef, Task(Of Boolean))
        Return Function(k_ref) OpenViewMachetaAsync(MachetaRef.Create(ViewPrefix & k_viewKey, "an", k_ref.Arg("an"), "cod", k_ref.Arg("cod")))
    End Function

    ''' <summary>The reference of an editor or child window of one angajament, in the year in force.</summary>
    Friend Function CodRef(k_typeKey As String, k_cod As String) As MachetaRef
        Return MachetaRef.Create(k_typeKey, "an", _session.An.ToString(CultureInfo.InvariantCulture), "cod", If(k_cod, String.Empty))
    End Function

    ''' <summary>
    ''' Shows a form as a modal window of the main window (its own caption bar, smaller than the work area, centred on the
    ''' main window) and returns when it has closed -- ShowDialog, for the code that opened it. The page or shell on screen
    ''' is put away first (slice 0113-01).
    ''' </summary>
    Friend Function ShowModalAsync(k_form As KBotModalForm, k_ref As MachetaRef) As Task(Of Boolean)
        Return AvacontMain.ShowModalAsync(Me, k_form, Recordable(k_ref))
    End Function

    ''' <summary>Shows a modeless form as a page of the main window (or beside the shell when it runs alone). The page stays open when another one is asked for.</summary>
    Friend Function ShowMachetaPage(k_form As Form, k_ref As MachetaRef) As Boolean
        Dim k_main As AvacontMain = TryCast(HostForm, AvacontMain)
        If k_main Is Nothing Then
            k_form.Show(Me)
            Return True
        End If
        Return k_main.ShowPage(k_form, Recordable(k_ref))
    End Function

    ' A macheta of an angajament that does not exist yet (its code starts with «!»: a new one, not saved) cannot be
    ' opened again from the history, so it is not recorded.
    Private Shared Function Recordable(k_ref As MachetaRef) As MachetaRef
        If k_ref IsNot Nothing AndAlso k_ref.Arg("cod").StartsWith("!", StringComparison.Ordinal) Then Return Nothing
        Return k_ref
    End Function

    Private Function OpenHelpMachetaAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Dim k_service As HelpService = TryCast(KBotHelp.Provider, HelpService)
        If k_service Is Nothing Then Return Task.FromResult(False)
        Return Task.FromResult(k_service.ShowHelpPage(TryCast(HostForm, AvacontMain)))
    End Function

    Private Function OpenListaMachetaAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Dim k_main As AvacontMain = TryCast(HostForm, AvacontMain)
        Return Task.FromResult(k_main Is Nothing OrElse k_main.ShowShell())
    End Function

    Private Function ViewRef(k_viewKey As String, k_cod As String) As MachetaRef
        Return CodRef(ViewPrefix & k_viewKey, k_cod)
    End Function

    ' The operator opened a view of the nav on the selected angajament: the history hears it.
    Private Sub NoteViewOpened(k_viewKey As String)
        If Machete Is Nothing OrElse _currentInfo Is Nothing Then Return
        If _currentInfo.CodAngajament.StartsWith("!", StringComparison.Ordinal) Then Return   ' not saved yet
        If Not Machete.IsRegistered(ViewPrefix & k_viewKey) Then Return
        Machete.Note(ViewRef(k_viewKey, _currentInfo.CodAngajament))
    End Sub

    ''' <summary>Selects an angajament in the tree like a click on its row does. False = it is not in the tree now.</summary>
    Friend Function SelectAngajament(k_cod As String) As Boolean
        Dim k_node As AdvancedTreeControl.TreeItem = Nothing
        If Not _treeNodes.TryGetValue(k_cod, k_node) Then Return False
        Dim k_info As AngajamentTreeInfo = Nothing
        If Not _treeInfos.TryGetValue(TryCast(k_node.Tag, String), k_info) Then Return False
        _currentInfo = k_info
        tree.SelectAndReveal(k_node)
        ApplyViewGating(k_info)
        _activeView?.SetContext(k_info)
        RefreshInfoForm()
        Return True
    End Function

    ' Same steps as the year drop-down of the caption bar (CapBar_PeriodChanged), but awaitable.
    Private Async Function ChangeYearAsync(k_an As Integer) As Task(Of Boolean)
        If _periods Is Nothing OrElse Not _periods.Any(Function(k_p) k_p.AN = k_an) Then Return False
        capBar.SetSelectorKey(KBotCaptionBar.SelectorYear, k_an.ToString(CultureInfo.InvariantCulture))
        LoadSsForSelectedYear()
        Await LoadTreeAsync()
        Return True
    End Function

    ' Brings the shell to the front, in the year of the macheta, with its angajament selected. False = it told the
    ' operator why that cannot be done.
    Private Async Function PrepareAngajamentAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Const k_caption As String = "Machete deschise"
        Dim k_cod As String = k_ref.Arg("cod")
        Dim k_an As Integer
        Integer.TryParse(k_ref.Arg("an"), NumberStyles.Integer, CultureInfo.InvariantCulture, k_an)

        ' The shell must be what is on screen: the tree and the views live in it.
        Dim k_main As AvacontMain = TryCast(HostForm, AvacontMain)
        If k_main IsNot Nothing AndAlso Not k_main.ShowShell() Then Return False

        If k_an > 0 AndAlso _session.An <> k_an Then
            If Not Await ChangeYearAsync(k_an) Then
                KBotMessage.Show(Me, "Anul " & k_an.ToString(CultureInfo.InvariantCulture) & " nu mai este disponibil pentru această unitate.",
                                 k_caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return False
            End If
        End If
        If k_cod.Length > 0 AndAlso Not SelectAngajament(k_cod) Then
            KBotMessage.Show(Me, "Angajamentul «" & k_cod & "» nu este în lista curentă (poate este ascuns sau din alt sector).",
                             k_caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return True
    End Function

    Private Async Function OpenViewMachetaAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Const k_caption As String = "Machete deschise"
        Try
            Dim k_viewKey As String = k_ref.TypeKey.Substring(ViewPrefix.Length)
            If Not Await PrepareAngajamentAsync(k_ref) Then Return False
            Dim k_item As KBotNavItem = navViews.Items.FirstOrDefault(Function(k_i) String.Equals(k_i.Key, k_viewKey, StringComparison.Ordinal))
            If k_item Is Nothing OrElse Not k_item.Enabled OrElse Not k_item.Visible Then
                KBotMessage.Show(Me, "Pagina «" & If(k_item Is Nothing, k_viewKey, k_item.Text) & "» nu este disponibilă pentru acest angajament.",
                                 k_caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return False
            End If
            navViews.SelectedKey = k_viewKey
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.OpenViewMachetaAsync", ex)
            Throw
        End Try
    End Function

    Private Async Function OpenAsociereMachetaAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Try
            If Not Await PrepareAngajamentAsync(k_ref) Then Return False
            Await DeschideLegaturileReceptiilorAsync(k_ref.Arg("cod"))
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.OpenAsociereMachetaAsync", ex)
            Throw
        End Try
    End Function

    Private Async Function OpenCabNotesMachetaAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Try
            Await ShowUncorrelatedAsync(onlyIds:=Nothing)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.OpenCabNotesMachetaAsync", ex)
            Throw
        End Try
    End Function

    Private Async Function OpenCabSkippedMachetaAsync(k_ref As MachetaRef) As Task(Of Boolean)
        Try
            Await ShowSkippedAsync()
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.OpenCabSkippedMachetaAsync", ex)
            Throw
        End Try
    End Function
End Class
