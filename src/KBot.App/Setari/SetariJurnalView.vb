Option Strict On
Imports System.IO
Imports System.Text
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Jurnal» (slice 0072-01): the log viewer as a page of the settings window -- the files on
''' the left, the entries in the grid, the raw block of the selected entry underneath, and the
''' filters above everything (level chips, text, date range).
'''
''' <para>Until this slice the same surface was <c>LogViewerForm</c>, a window of its own opened
''' from the shell's options menu. The operator asked for it as a page (20.09.2026); the window
''' still exists for the harness and the standalone «Jurnale» launcher, but it now only hosts
''' this control. Everything about READING and UNDERSTANDING a log stays in
''' <c>KBot.Common\Logging</c> (slice 0031-01) and is pure: <c>LogPaths</c>,
''' <c>LogFileLoader</c>, <c>LogFilter</c>, <c>ServerClock</c>. This page parses nothing -- it
''' only ties that core to the house controls.</para>
'''
''' <para><b>Newest first.</b> The grid is sorted by the corrected timestamp, descending: what
''' the operator opens the page for is what just happened, and it should be the first row, not
''' the last one after a scroll. Entries with no timestamp cannot be placed and go at the end,
''' in file order.</para>
'''
''' <para><b>The message is not a column.</b> A stack trace does not fit a cell; the «Mesaj»
''' column showed its first line and the rest was cut. The grid keeps the identifying columns
''' (time, level, origin, file, source) and the whole raw block is read in <c>txtDetaliu</c>,
''' a <see cref="KBotTextBox"/> with both scroll bars.</para>
'''
''' <para><b>Reading never sits on the UI thread.</b> Files are read and parsed on a background
''' thread (a log can be 5 MB); FILTERING is in memory and synchronous -- it never re-reads a
''' file and never asks the server again. That is why the search has a 250 ms timer.</para>
'''
''' <para><b>Which journal: <c>CmbTipJurnal</c> (slice 0089).</b> «Jurnale locale» = every
''' local file. «Server FOREXE» and «Timpi FOREXE» = the caller's OWN lines of the server's
''' <c>api_server.log</c> / <c>forexe_timing.log</c>, from their last N logins (N from
''' <c>cmbSesiuni</c>, 3 by default). The server does the choosing (<c>PYTHON/routes/logs.py</c>):
''' the user comes from the bearer session, so a client can never read another operator's lines,
''' and only <c>[forexe]</c> / <c>[forexe.xxx]</c> lines come back -- the rest is the old Access
''' path. The text is parsed here by the same <c>LogFileLoader</c> as a local file, so the level
''' chips split it into errors / warnings / information like any other journal.</para>
'''
''' <para><b>The server is optional.</b> Without <c>IApiClient</c> (the harness) the two server
''' choices do not exist; with it, a failure is written into <c>noticeGol</c> and the page keeps
''' working. A dead API is not allowed to take the page down with it.</para>
'''
''' <para><b>What it does NOT do:</b> no live tailing (no <c>FileSystemWatcher</c>), nothing is
''' written to the server (the routes are read-only), and nothing is deleted outside the one
''' explicit path in <see cref="btnGoleste_Click"/>, which asks twice.</para>
''' </summary>
Public Class SetariJurnalView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    ' -- Navigation keys --------------------------------------------------------
    Private Const KEY_TOATE As String = "toate"
    Private Const PREFIX_LOCAL As String = "loc:"

    ' -- Journal kinds (CmbTipJurnal, slice 0089) ----------------------------------
    ' Key, the text the operator reads, the route (Nothing = local files) and the file name the
    ' text is parsed under -- LogFileLoader picks the parser by that name.
    Private Shared ReadOnly TIPURI As (Key As String, Text As String, Route As String, FileName As String)() = {
        ("local", "Jurnale locale", Nothing, Nothing),
        ("server", "Server FOREXE", "/api/logs/server", "api_server.log"),
        ("timing", "Timpi FOREXE", "/api/logs/timing", "forexe_timing.log")}

    ' How many of the operator's last logins a server journal covers. The first one is the default.
    Private Shared ReadOnly SESIUNI As Integer() = {3, 5, 10, 20, 50}

    ' Chip keys = level names, so translating the checked chips into a set of KBotLogLevel is a
    ' single lookup, not a map kept by heart in two places.
    Private Shared ReadOnly NIVELURI As (Key As String, Text As String, Level As KBotLogLevel)() = {
        ("err", "Erori", KBotLogLevel.[Error]),
        ("wrn", "Avertismente", KBotLogLevel.Warn),
        ("inf", "Informații", KBotLogLevel.Info),
        ("dbg", "Depanare", KBotLogLevel.Debug),
        ("trc", "Urmărire", KBotLogLevel.Trace),
        ("unk", "Necunoscut", KBotLogLevel.Unknown)}

    ''' <summary>
    ''' The API client for server logs. Nothing = local files only: the «Server» group does
    ''' not appear at all (not empty and disabled -- a group that cannot work must not exist).
    '''
    ''' <para>A property, not a constructor argument, so the designer can build the page with
    ''' the parameterless constructor (the hosting form declares it in its .Designer.vb, per
    ''' the house rule) and the host sets the client right after. It is read at the first
    ''' <see cref="Activated"/> and on every server request, so it must be set before then.</para>
    ''' </summary>
    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ApiClient As IApiClient
        Get
            Return _api
        End Get
        Set(value As IApiClient)
            _api = value
        End Set
    End Property
    Private _api As IApiClient

    ' Entries loaded for the current selection (before filtering), newest first.
    Private _incarcate As New List(Of LogEntry)()
    ' The last server failure (or «nothing of yours»), shown in noticeGol while the grid is empty.
    Private _eroareServer As String = String.Empty
    ' The sessions summary of the last server load, for the status line.
    Private _rezumatSesiuni As String = String.Empty
    ' How many bytes were read and whether any file was cut at the read window.
    Private _octetiCititi As Long
    Private _taiat As Boolean
    ' The current selection in the list (nav key).
    Private _selectie As String = KEY_TOATE
    ' While the controls are being filled from code their events must not re-filter.
    Private _suprimaEvenimente As Boolean
    ' One load at a time: the second one cancels the first (the operator changed the file).
    Private _cts As CancellationTokenSource
    ' The first activation loads the list; later ones only refresh what is on screen.
    Private _pornit As Boolean

    Public Sub New()
        InitializeComponent()
        ' The chips are built HERE, not at activation: the filter takes its levels from them,
        ' and a page without chips would filter on the empty set -- that is, show nothing
        ' (LogFilter: «empty set = nothing»). This way every caller gets a coherent page.
        ConstruiesteJetoane()
        ConstruiesteTipuri()
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "jurnal"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    ' =====================================================================
    ' ACTIVATION
    ' =====================================================================

    ''' <summary>
    ''' First activation builds the file list and loads «Toate fisierele»; every later one
    ''' re-reads the current selection, because the logs kept growing while another page was
    ''' on screen.
    ''' </summary>
    Public Sub Activated() Implements ISetariView.Activated
        Try
            AplicaCulorileJetoanelor()
            PotrivesteTipuriCuClientul()
            ArataCombinatiileDeTip(_api IsNot Nothing AndAlso AppSettings.Current.AdvancedOptions)
            If Not _pornit Then
                _pornit = True
                ConstruiesteListaFisiere()
                navFisiere.SelectedKey = KEY_TOATE      ' raises SelectionChanged -> loads
            Else
                Reincarca()
            End If
        Catch ex As Exception
            ' UI boundary (activation): a throw would break the page switch.
            GlobalErrorLog.Write("SetariJurnalView.Activated", ex)
            lblStare.Text = "Vizualizatorul nu a putut porni. Detalii în jurnalul de erori."
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return

            ButtonStyles.ApplyTrans(btnDeschideDosar, ThemeManager.Current)
            ButtonStyles.ApplyTrans(btnCopiaza, ThemeManager.Current)
            ButtonStyles.ApplyTrans(btnExporta, ThemeManager.Current)
            ButtonStyles.ApplyTrans(btnGoleste, ThemeManager.Current)
            ButtonStyles.ApplyTrans(btnReimprospateaza, ThemeManager.Current)
            btnDeschideDosar.Padding = Padding.Empty
            btnGoleste.Padding = Padding.Empty
            btnCopiaza.Padding = Padding.Empty
            btnExporta.Padding = Padding.Empty

            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            tlyMain.BackColor = p.SurfaceAltColor
            tlyFilter.BackColor = p.SurfaceAltColor
            tlyFilterActual.BackColor = p.SurfaceAltColor
            tlyFooter.BackColor = p.SurfaceAltColor

            For Each caption As Label In New Label() {lblCauta, lblDeLa, lblPanaLa, lblStare}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ' The ERRORS / WARNINGS chip colours come from the palette and are given from HERE
            ' (the control names no colour), so they must be given AGAIN on a scheme switch. The
            ' grid repaints itself through RowFormatting, which reads the palette per row.
            AplicaCulorileJetoanelor()
            grila.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub AplicaCulorileJetoanelor()
        Try
            Dim p As ThemePalette = ThemeManager.Current.Palette
            For Each c As KBotChip In chipNiveluri.Chips
                Select Case c.Key
                    Case "err" : c.AccentOverride = p.ErrorColor
                    Case "wrn" : c.AccentOverride = p.WarningColor
                    Case Else : c.AccentOverride = Color.Empty
                End Select
            Next
            chipNiveluri.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.AplicaCulorileJetoanelor", ex)
        End Try
    End Sub

    ' Every level checked at start: the page opens showing EVERYTHING, and the operator takes
    ' out what does not interest them. The other way round would be a page that opens empty.
    Private Sub ConstruiesteJetoane()
        _suprimaEvenimente = True
        Try
            chipNiveluri.Chips.Clear()
            For Each n In NIVELURI
                chipNiveluri.AddChip(n.Key, n.Text, True)
            Next
        Finally
            _suprimaEvenimente = False
        End Try
    End Sub

    ''' <summary>
    ''' The list on the left: «Toate fisierele» pinned first, then the LOCAL files (the files in
    ''' <c>LogPaths.LogsDirectory()</c>, freshest first, archives marked as such). Server journals
    ''' are not files here: they are chosen in <c>CmbTipJurnal</c>.
    ''' </summary>
    Private Sub ConstruiesteListaFisiere()
        _suprimaEvenimente = True
        Try
            navFisiere.Items.Clear()
            navFisiere.AddItem(KEY_TOATE, "Toate fișierele")
            navFisiere.AddSeparator()

            For Each f As FileInfo In FisiereLocale()
                navFisiere.AddItem(PREFIX_LOCAL & f.Name, EtichetaFisier(f))
            Next
        Finally
            _suprimaEvenimente = False
        End Try
    End Sub

    ''' <summary>
    ''' The local log files, most recently written first. A missing directory is not an error:
    ''' it is an installation that has not written a log yet.
    ''' </summary>
    Private Shared Function FisiereLocale() As List(Of FileInfo)
        Try
            Dim dir As New DirectoryInfo(LogPaths.LogsDirectory())
            If Not dir.Exists Then Return New List(Of FileInfo)()
            Dim toate As New List(Of FileInfo)()
            For Each tipar As String In {"*.log", "*.log.1", "*.log.2", "*.log.3", "*.log.4", "*.log.5", "log_*.txt"}
                toate.AddRange(dir.GetFiles(tipar))
            Next
            Return toate.
                GroupBy(Function(f) f.Name, StringComparer.OrdinalIgnoreCase).
                Select(Function(g) g.First()).
                OrderByDescending(Function(f) f.LastWriteTime).
                ToList()
        Catch ex As Exception
            ' I/O boundary called from list building (a UI boundary already wrapped): log and
            ' return empty, so the page opens and says «no file».
            GlobalErrorLog.Write("SetariJurnalView.FisiereLocale", ex)
            Return New List(Of FileInfo)()
        End Try
    End Function

    ' «harness_errors.log» / «harness_errors.log.2 (arhiva)» -- the archive shows in the label.
    Private Shared Function EticheteazaArhiva(nume As String) As Boolean
        Dim ext As String = Path.GetExtension(nume)
        Dim gen As Integer
        Return ext.Length > 1 AndAlso Integer.TryParse(ext.AsSpan(1), gen) AndAlso gen >= 1 AndAlso gen <= 5
    End Function

    Private Shared Function EtichetaFisier(f As FileInfo) As String
        If EticheteazaArhiva(f.Name) Then Return f.Name & " (arhivă)"
        Return f.Name
    End Function

    ' =====================================================================
    ' LOADING
    ' =====================================================================

    Private Sub navFisiere_SelectionChanged(key As String) Handles navFisiere.SelectionChanged
        If _suprimaEvenimente Then Return
        Try
            _selectie = key
            Dim ignorat2 As Task = IncarcaSelectiaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.navFisiere_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub btnReimprospateaza_Click(sender As Object, e As EventArgs) Handles btnReimprospateaza.Click
        Try
            Reincarca()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.btnReimprospateaza_Click", ex)
        End Try
    End Sub

    ' Refresh rebuilds the file list too: rotation may have created a new archive meanwhile.
    Private Sub Reincarca()
        Dim curenta As String = _selectie
        ConstruiesteListaFisiere()
        _suprimaEvenimente = True
        Try
            navFisiere.SelectedKey = If(navFisiere.Items.Any(Function(i) String.Equals(i.Key, curenta, StringComparison.Ordinal)),
                                        curenta, KEY_TOATE)
            _selectie = navFisiere.SelectedKey
        Finally
            _suprimaEvenimente = False
        End Try
        ' A refresh keeps the days the operator opened: they are reading one of them.
        Dim ignorat As Task = IncarcaSelectiaAsync(strangeZilele:=False)
    End Sub

    ''' <summary>
    ''' Loads the current selection: reading and parsing on a background thread, filling on the
    ''' UI thread. A new load cancels the one in flight -- the operator changed the file, the
    ''' old answer has no place in the grid.
    '''
    ''' <para><paramref name="strangeZilele"/>: a DIFFERENT journal (file, kind, session count)
    ''' opens with every day collapsed; the group level's CollapsedByDefault alone would do that
    ''' only for days never seen before. A refresh passes False.</para>
    ''' </summary>
    Private Async Function IncarcaSelectiaAsync(Optional strangeZilele As Boolean = True) As Task
        _cts?.Cancel()
        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim ct As CancellationToken = _cts.Token
        Dim cerut As String = _selectie
        Dim tip As Integer = TipCurent()
        Dim sesiuni As Integer = SesiuniCerute()

        SeteazaOcupat(True)
        lblStare.Text = "Se încarcă…"
        noticeGol.Clear()
        noticeGol.Visible = False
        _eroareServer = String.Empty
        _rezumatSesiuni = String.Empty
        MarcheazaNoticeServer(False)
        Try
            Dim rezultat As IncarcareRezultat
            If TIPURI(tip).Route IsNot Nothing Then
                rezultat = Await IncarcaJurnalServerAsync(tip, sesiuni, ct)
            Else
                Dim cai As List(Of String) = CaiPentru(cerut)
                rezultat = Await Task.Run(Function() CitesteFisiere(cai, ct), ct)
            End If

            If ct.IsCancellationRequested OrElse
               Not String.Equals(cerut, _selectie, StringComparison.Ordinal) OrElse
               tip <> TipCurent() OrElse sesiuni <> SesiuniCerute() Then Return

            _incarcate = OrdoneazaCeleMaiNoiPrimele(rezultat.Entries)
            _octetiCititi = rezultat.Bytes
            _taiat = rezultat.Truncated
            ActualizeazaBadgeuri()
            AplicaFiltrul()
            If strangeZilele Then grila.CollapseAllGroups()
        Catch ex As OperationCanceledException
            ' Cancellation is the normal path (a second selection), not an error.
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.IncarcaSelectiaAsync", ex)
            _incarcate = New List(Of LogEntry)()
            grila.ClearRows()
            ArataGol("Jurnalul nu a putut fi citit: " & ex.Message)
            lblStare.Text = "Încărcare eșuată."
        Finally
            SeteazaOcupat(False)
        End Try
    End Function

    ' The page's own busy bar AND the window's band: the band is what the operator watches
    ' while another page is loading, so it should tell the same story here.
    Private Sub SeteazaOcupat(ocupat As Boolean)
        busy.Running = ocupat
        RaiseEvent BusyChanged(ocupat)
    End Sub

    ''' <summary>Which files a nav key covers: one, or every local one.</summary>
    Private Function CaiPentru(key As String) As List(Of String)
        If key.StartsWith(PREFIX_LOCAL, StringComparison.Ordinal) Then
            Return New List(Of String) From {LogPaths.Combine(key.Substring(PREFIX_LOCAL.Length))}
        End If
        ' «Toate fisierele» = every LOCAL one. Server journals never mix in here: they are their own
        ' choice in CmbTipJurnal (slice 0089).
        Return FisiereLocale().Select(Function(f) f.FullName).ToList()
    End Function

    ''' <summary>What came out of a load: the entries plus the numbers for the status line.</summary>
    Private NotInheritable Class IncarcareRezultat
        Public Property Entries As List(Of LogEntry)
        Public Property Bytes As Long
        Public Property Truncated As Boolean
    End Class

    ''' <summary>
    ''' Background thread: reads and parses each file and stitches them into one list. Order is
    ''' settled afterwards by <see cref="OrdoneazaCeleMaiNoiPrimele"/>, on the UI side, so the
    ''' server path and the test hook go through the same sort.
    ''' </summary>
    Private Shared Function CitesteFisiere(cai As List(Of String), ct As CancellationToken) As IncarcareRezultat
        Dim toate As New List(Of LogEntry)()
        Dim octeti As Long = 0
        Dim taiat As Boolean = False

        For Each cale As String In cai
            ct.ThrowIfCancellationRequested()
            Try
                Dim r As LogLoadResult = LogFileLoader.LoadFile(cale)
                octeti += r.FileLengthBytes
                taiat = taiat OrElse r.WasTruncated
                toate.AddRange(r.Entries)
            Catch ex As IOException
                ' A file that cannot be read (deleted between listing and reading, locked
                ' exclusively) does NOT stop the rest. It is logged, so its absence is not silent.
                GlobalErrorLog.Write("SetariJurnalView.CitesteFisiere(" & cale & ")", ex)
            Catch ex As UnauthorizedAccessException
                GlobalErrorLog.Write("SetariJurnalView.CitesteFisiere(" & cale & ")", ex)
            End Try
        Next

        Return New IncarcareRezultat With {.Entries = toate, .Bytes = octeti, .Truncated = taiat}
    End Function

    ''' <summary>
    ''' Newest first, on the CORRECTED timestamp. Entries without a timestamp stay in file
    ''' order, at the end -- a date invented for them would be a sortable lie. The sort is
    ''' stable, so two entries written in the same millisecond keep the order of the file.
    ''' </summary>
    Friend Shared Function OrdoneazaCeleMaiNoiPrimele(entries As IEnumerable(Of LogEntry)) As List(Of LogEntry)
        If entries Is Nothing Then Return New List(Of LogEntry)()
        Dim cuData As New List(Of LogEntry)()
        Dim faraData As New List(Of LogEntry)()
        For Each en As LogEntry In entries
            If en Is Nothing Then Continue For
            If ServerClock.ToClientLocal(en).HasValue Then cuData.Add(en) Else faraData.Add(en)
        Next
        Dim ordonate As List(Of LogEntry) = cuData.
            OrderByDescending(Function(en) ServerClock.ToClientLocal(en).Value).
            ToList()
        ordonate.AddRange(faraData)
        Return ordonate
    End Function

    ' =====================================================================
    ' JOURNAL KIND AND SERVER JOURNALS (slice 0089)
    ' =====================================================================

    ''' <summary>
    ''' Fills the two combos. «Jurnale locale» is chosen and the sessions combo is off until a
    ''' server journal is picked.
    ''' </summary>
    Private Sub ConstruiesteTipuri()
        _suprimaEvenimente = True
        Try
            cmbSesiuni.Items.Clear()
            For Each n As Integer In SESIUNI
                cmbSesiuni.Items.Add(n.ToString(Globalization.CultureInfo.InvariantCulture) & " sesiuni")
            Next
            cmbSesiuni.SelectedIndex = 0
        Finally
            _suprimaEvenimente = False
        End Try
        PotrivesteTipuriCuClientul()
    End Sub

    ''' <summary>
    ''' Lists the journal kinds this page can serve. Without an API client only «Jurnale locale»
    ''' exists: a choice that cannot work must not be offered. The client arrives after the
    ''' constructor (designer-built page), so this runs again at activation.
    '''
    ''' <para>The server journals also hang off the advanced options switch
    ''' (<see cref="AppSettings.AdvancedOptions"/>, operator): with it off they do not exist, and
    ''' with only «Jurnale locale» left the two combos (kind and sessions) have nothing to choose,
    ''' so they are hidden and their columns collapse (<see cref="ArataCombinatiileDeTip"/>, called
    ''' from <see cref="Activated"/> only: the column widths are read AFTER the designer's DPI
    ''' scaling, never in the constructor). The switch lives on another page, so it can only have
    ''' changed while this one was away -- activation is where it is read.</para>
    ''' </summary>
    Private Sub PotrivesteTipuriCuClientul()
        Dim serverPermis As Boolean = _api IsNot Nothing AndAlso AppSettings.Current.AdvancedOptions
        Dim dorite As Integer = If(serverPermis, TIPURI.Length, 1)
        If CmbTipJurnal.Items.Count = dorite AndAlso CmbTipJurnal.SelectedIndex >= 0 Then Return
        _suprimaEvenimente = True
        Try
            CmbTipJurnal.Items.Clear()
            For i As Integer = 0 To dorite - 1
                CmbTipJurnal.Items.Add(TIPURI(i).Text)
            Next
            CmbTipJurnal.SelectedIndex = 0
            AplicaTipul()
        Finally
            _suprimaEvenimente = False
        End Try
    End Sub

    ' The widths the designer gave the first two filter columns, remembered the first time they
    ' are collapsed (they are device pixels, already scaled), so showing them again restores them.
    Private _latimeColTip As Single = -1
    Private _latimeColSesiuni As Single = -1

    Private Sub ArataCombinatiileDeTip(arata As Boolean)
        If _latimeColTip < 0 Then
            _latimeColTip = tlyFilterActual.ColumnStyles(0).Width
            _latimeColSesiuni = tlyFilterActual.ColumnStyles(1).Width
        End If
        CmbTipJurnal.Visible = arata
        cmbSesiuni.Visible = arata
        tlyFilterActual.ColumnStyles(0).Width = If(arata, _latimeColTip, 0F)
        tlyFilterActual.ColumnStyles(1).Width = If(arata, _latimeColSesiuni, 0F)
    End Sub

    ''' <summary>Index into <see cref="TIPURI"/> of the chosen kind (the combo lists them in that order).</summary>
    Private Function TipCurent() As Integer
        Dim i As Integer = CmbTipJurnal.SelectedIndex
        Return If(i < 0 OrElse i >= TIPURI.Length, 0, i)
    End Function

    Private Function SesiuniCerute() As Integer
        Dim i As Integer = cmbSesiuni.SelectedIndex
        Return If(i < 0 OrElse i >= SESIUNI.Length, SESIUNI(0), SESIUNI(i))
    End Function

    ' The controls that depend on the kind: the sessions count means something only on the
    ' server; «Deschide dosarul» and «Goleste» act on LOCAL files only.
    Private Sub AplicaTipul()
        Dim server As Boolean = TIPURI(TipCurent()).Route IsNot Nothing
        cmbSesiuni.Enabled = server
        btnDeschideDosar.Enabled = Not server
        btnGoleste.Enabled = Not server
    End Sub

    Private Sub CmbTipJurnal_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbTipJurnal.SelectedIndexChanged
        If _suprimaEvenimente Then Return
        Try
            AplicaTipul()
            Dim ignorat As Task = IncarcaSelectiaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.CmbTipJurnal_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub cmbSesiuni_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSesiuni.SelectedIndexChanged
        If _suprimaEvenimente Then Return
        Try
            If TIPURI(TipCurent()).Route Is Nothing Then Return
            Dim ignorat As Task = IncarcaSelectiaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.cmbSesiuni_SelectedIndexChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Loads one server journal and runs its text through the same parser as a local file. A
    ''' failure does NOT throw: it is kept in <see cref="_eroareServer"/> (shown in
    ''' <c>noticeGol</c>) and an empty result comes back, so the page stays usable.
    ''' </summary>
    Private Async Function IncarcaJurnalServerAsync(tip As Integer, sesiuni As Integer,
                                                    ct As CancellationToken) As Task(Of IncarcareRezultat)
        Dim raspuns As ServerJournalResponse = Await AduJurnalServerAsync(tip, sesiuni, ct)
        If raspuns Is Nothing Then Return New IncarcareRezultat With {.Entries = New List(Of LogEntry)()}

        Dim text As String = If(raspuns.Text, String.Empty)
        Dim r As LogLoadResult = LogFileLoader.LoadText(text, TIPURI(tip).FileName, Date.Today,
                                                        LogOrigin.Server, raspuns.Truncated,
                                                        Encoding.UTF8.GetByteCount(text))
        _rezumatSesiuni = RezumatSesiuni(raspuns)
        If r.Entries.Count = 0 Then
            _eroareServer = "Serverul nu are nicio intrare de-a dumneavoastră în ultimele " &
                            sesiuni.ToString(Globalization.CultureInfo.InvariantCulture) &
                            " sesiuni. Se înregistrează doar ce s-a lucrat după actualizarea serverului."
        End If
        Return New IncarcareRezultat With {.Entries = r.Entries.ToList(),
                                           .Bytes = r.FileLengthBytes,
                                           .Truncated = r.WasTruncated}
    End Function

    ''' <summary>
    ''' The request itself. Nothing on failure -- the reason is in <see cref="_eroareServer"/>
    ''' and the failure flag is up. Cancellation is re-thrown: it is the normal path.
    ''' </summary>
    Private Async Function AduJurnalServerAsync(tip As Integer, sesiuni As Integer,
                                                ct As CancellationToken) As Task(Of ServerJournalResponse)
        If _api Is Nothing OrElse TIPURI(tip).Route Is Nothing Then Return Nothing
        Try
            Dim raspuns As ServerJournalResponse = Await _api.GetAsync(Of ServerJournalResponse)(
                TIPURI(tip).Route & "?sessions=" & sesiuni.ToString(Globalization.CultureInfo.InvariantCulture), ct)

            If raspuns?.ServerTime IsNot Nothing Then
                Dim st As DateTimeOffset
                If DateTimeOffset.TryParse(raspuns.ServerTime, Globalization.CultureInfo.InvariantCulture,
                                           Globalization.DateTimeStyles.None, st) Then ServerClock.Update(st)
            End If
            Return raspuns
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.AduJurnalServerAsync", ex)
            _eroareServer = "Jurnalul serverului nu s-a putut aduce: " & ex.Message &
                            " Jurnalele locale funcționează mai departe."
            MarcheazaNoticeServer(True)
            Return Nothing
        End Try
    End Function

    ''' <summary>«3 sesiuni: 28.09 09:09–10:51 (014_SCSV), ...» -- newest first, as the server sends them.</summary>
    Private Shared Function RezumatSesiuni(raspuns As ServerJournalResponse) As String
        If raspuns?.Sessions Is Nothing OrElse raspuns.Sessions.Count = 0 Then Return String.Empty
        Dim parti As New List(Of String)()
        For Each s As ServerJournalSession In raspuns.Sessions
            If s Is Nothing Then Continue For
            Dim p As String = FormatStamp(s.First, "dd.MM HH:mm") & "–" & FormatStamp(s.Last, "HH:mm")
            If Not String.IsNullOrEmpty(s.Dc) Then p &= " (" & s.Dc & ")"
            parti.Add(p)
        Next
        Return parti.Count.ToString(Globalization.CultureInfo.InvariantCulture) &
               If(parti.Count = 1, " sesiune: ", " sesiuni: ") & String.Join(", ", parti)
    End Function

    ' "2026-09-28 09:09:29" (server time, as the route sends it) in the given format.
    ' Unreadable text is shown as it came.
    Private Shared Function FormatStamp(text As String, format As String) As String
        Dim d As Date
        If Date.TryParseExact(If(text, String.Empty), "yyyy-MM-dd HH:mm:ss", Globalization.CultureInfo.InvariantCulture,
                              Globalization.DateTimeStyles.None, d) Then
            Return d.ToString(format, Globalization.CultureInfo.InvariantCulture)
        End If
        Return If(text, String.Empty)
    End Function

    ''' <summary>Body of <c>GET /api/logs/server</c> and <c>/api/logs/timing</c> (PYTHON/routes/logs.py).</summary>
    Private NotInheritable Class ServerJournalResponse
        Public Property Kind As String
        Public Property Text As String
        Public Property Truncated As Boolean
        Public Property Sessions As List(Of ServerJournalSession)
        <System.Text.Json.Serialization.JsonPropertyName("sessions_requested")>
        Public Property SessionsRequested As Integer
        <System.Text.Json.Serialization.JsonPropertyName("server_time")>
        Public Property ServerTime As String
    End Class

    Private NotInheritable Class ServerJournalSession
        Public Property Session As String
        Public Property Dc As String
        Public Property First As String
        Public Property Last As String
        Public Property Entries As Integer
        Public Property Errors As Integer
        Public Property Warnings As Integer
    End Class

    ' =====================================================================
    ' FILTERING
    ' =====================================================================

    Private Sub chipNiveluri_CheckedChanged(chipKey As String) Handles chipNiveluri.CheckedChanged
        If _suprimaEvenimente Then Return
        Try
            AplicaFiltrul()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.chipNiveluri_CheckedChanged", ex)
        End Try
    End Sub

    ' The search does NOT re-filter on every key: the timer restarts, filtering comes after quiet.
    Private Sub txtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        If _suprimaEvenimente Then Return
        tmrCautare.Stop()
        tmrCautare.Start()
    End Sub

    Private Sub tmrCautare_Tick(sender As Object, e As EventArgs) Handles tmrCautare.Tick
        Try
            tmrCautare.Stop()
            AplicaFiltrul()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.tmrCautare_Tick", ex)
        End Try
    End Sub

    Private Sub txtInterval_TextChanged(sender As Object, e As EventArgs) Handles txtDeLa.TextChanged, txtPanaLa.TextChanged
        If _suprimaEvenimente Then Return
        tmrCautare.Stop()
        tmrCautare.Start()
    End Sub

    ''' <summary>
    ''' Filters IN MEMORY (never a re-read) and fills the grid. The row carries the entry in
    ''' <c>Tag</c>, so the selection fills the detail panel without another lookup. The order of
    ''' <c>_incarcate</c> (newest first) is preserved by the filter.
    ''' </summary>
    Private Sub AplicaFiltrul()
        Dim filtru As New LogFilter() With {
            .Levels = NiveluriBifate(),
            .Text = txtCauta.Text,
            .FromDate = DataDin(txtDeLa.Text, False),
            .ToDate = DataDin(txtPanaLa.Text, True)}

        Dim rezultat As LogFilterResult = filtru.Apply(_incarcate)

        grila.BeginUpdate()
        Try
            grila.ClearRows()
            For Each en As LogEntry In rezultat.Entries
                Dim r As KBotDataRow = grila.AddRow()
                r.Tag = en
                Dim stamp As Date? = ServerClock.ToClientLocal(en)
                r("ora") = If(stamp.HasValue, stamp.Value.ToString("dd.MM.yyyy HH:mm:ss.fff"), String.Empty)
                r("nivel") = TextNivel(en.Level)
                r("sursa") = If(en.Origin = LogOrigin.Server, "server", "local")
                r("fisier") = en.FileName
                r("detaliu") = en.Source
            Next
        Finally
            grila.EndUpdate()
        End Try

        ActualizeazaDetaliu(Nothing)   ' ClearRows does not raise SelectionChanged
        ActualizeazaStare(rezultat)

        If rezultat.ShownCount = 0 AndAlso _incarcate.Count = 0 AndAlso Not String.IsNullOrEmpty(_eroareServer) Then
            noticeGol.Show(_eroareServer, If(_noticeServerAfisat, NoticeKind.[Error], NoticeKind.Warning))
            noticeGol.Visible = True
        ElseIf rezultat.ShownCount = 0 Then
            ArataGol(If(_incarcate.Count = 0,
                        "Niciun jurnal de arătat. Fie nu s-a scris încă nimic, fie fișierul e gol.",
                        "Niciun rând nu trece de filtrele curente."))
        Else
            noticeGol.Visible = False
            noticeGol.Clear()
        End If
    End Sub

    Private Sub ArataGol(mesaj As String)
        noticeGol.Show(mesaj, NoticeKind.Warning)
        noticeGol.Visible = True
    End Sub

    ' The level set the chips ask for. The bar guarantees at least one checked chip
    ' (MinimumRequiredChecked = 1), so the set cannot come out empty by accident.
    Private Function NiveluriBifate() As ISet(Of KBotLogLevel)
        Dim nivele As New HashSet(Of KBotLogLevel)()
        For Each n In NIVELURI
            If chipNiveluri.ContainsChip(n.Key) AndAlso chipNiveluri.IsChecked(n.Key) Then nivele.Add(n.Level)
        Next
        Return nivele
    End Function

    ''' <summary>
    ''' The date at one end of the range, in the operator's format (dd.MM.yyyy). Empty = no
    ''' bound. The UPPER bound goes to the end of the day: «until 14.08» means the whole 14th,
    ''' not midnight -- otherwise the filter would cut exactly the day the operator asked for.
    ''' Invalid text behaves as «no bound» too (it is typed letter by letter).
    ''' </summary>
    Private Shared Function DataDin(text As String, sfarsitDeZi As Boolean) As Date?
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim d As Date
        If Not Date.TryParseExact(text.Trim(), "dd.MM.yyyy", Globalization.CultureInfo.InvariantCulture,
                                  Globalization.DateTimeStyles.None, d) Then Return Nothing
        Return If(sfarsitDeZi, d.Date.AddDays(1).AddTicks(-1), d.Date)
    End Function

    Private Shared Function TextNivel(level As KBotLogLevel) As String
        Select Case level
            Case KBotLogLevel.[Error] : Return "EROARE"
            Case KBotLogLevel.Warn : Return "AVERT."
            Case KBotLogLevel.Info : Return "INFO"
            Case KBotLogLevel.Debug : Return "DEPAN."
            Case KBotLogLevel.Trace : Return "URM."
            Case Else : Return "?"
        End Select
    End Function

    ' Each chip's badge = how many entries of that level were LOADED (not how many show): the
    ' number must say what is in the file, otherwise it would change under the filtering finger.
    Private Sub ActualizeazaBadgeuri()
        For Each n In NIVELURI
            If Not chipNiveluri.ContainsChip(n.Key) Then Continue For
            ' Enumerable.Count(predicate), not List.Count -- the property shadows the method in VB.
            Dim nivel As KBotLogLevel = n.Level
            chipNiveluri.SetBadge(n.Key, Enumerable.Count(_incarcate, Function(en) en.Level = nivel))
        Next
    End Sub

    ''' <summary>
    ''' The status line: how many entries, how many shown, how much was read, whether the window
    ''' was cut, the server clock offset and -- mandatory -- how many entries were excluded for
    ''' having no date. A silent exclusion looks exactly like a defect. The same line goes to
    ''' the window's band.
    ''' </summary>
    Private Sub ActualizeazaStare(rezultat As LogFilterResult)
        Dim sb As New StringBuilder()
        sb.Append(rezultat.TotalCount.ToString("N0")).Append(" intrări · ")
        sb.Append(rezultat.ShownCount.ToString("N0")).Append(" afișate")
        If _octetiCititi > 0 Then
            sb.Append(" · ").Append((_octetiCititi / 1024.0 / 1024.0).ToString("N1")).Append(" MB")
        End If
        If _taiat Then sb.Append(" · doar coada fișierului")
        If Not String.IsNullOrEmpty(_rezumatSesiuni) Then sb.Append(" · ").Append(_rezumatSesiuni)
        If ServerClock.HasReading AndAlso ServerClock.Offset <> TimeSpan.Zero Then
            sb.Append(" · ceas server ").Append(ServerClock.OffsetText())
        End If
        If rezultat.ExcludedWithoutTimestamp > 0 Then
            sb.Append(" · ").Append(rezultat.ExcludedWithoutTimestamp.ToString("N0")).
               Append(" fără dată, excluse de filtrul de timp")
        End If
        lblStare.Text = sb.ToString()
        RaiseEvent StatusChanged(lblStare.Text)
    End Sub

    ' =====================================================================
    ' GRID AND DETAIL
    ' =====================================================================

    ''' <summary>
    ''' Row colour by level -- FROM THE PALETTE, never a colour written here (plan 1.1). Info
    ''' gets nothing: it is the ordinary row, and painting it would make an exception of normal.
    '''
    ''' The args instance is REUSED by the grid for every row: it is not kept.
    ''' </summary>
    Private Sub grila_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles grila.RowFormatting
        Try
            Dim en As LogEntry = TryCast(e.Row?.Tag, LogEntry)
            If en Is Nothing Then Return
            Dim p As ThemePalette = ThemeManager.Current.Palette
            Select Case en.Level
                Case KBotLogLevel.[Error] : e.ForeColor = p.ErrorColor
                Case KBotLogLevel.Warn : e.ForeColor = p.WarningColor
                Case KBotLogLevel.Debug : e.ForeColor = p.TextDimColor
                Case KBotLogLevel.Trace : e.ForeColor = p.DisabledTextColor
                Case KBotLogLevel.Unknown : e.ForeColor = p.TextDimColor
            End Select
        Catch ex As Exception
            ' Paint boundary: a throw from here would take the process down.
            GlobalErrorLog.Write("SetariJurnalView.grila_RowFormatting", ex)
        End Try
    End Sub

    Private Sub grila_SelectionChanged(sender As Object, e As EventArgs) Handles grila.SelectionChanged
        Try
            ActualizeazaDetaliu(TryCast(grila.CurrentRow?.Tag, LogEntry))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.grila_SelectionChanged", ex)
        End Try
    End Sub

    ' The detail panel shows the RAW, UNCONVERTED block: the time in it is the time written in
    ' the file, even if the «Ora» column shows it corrected. Whoever reads a stack trace must see
    ' exactly what was written, not a version helped by us. It is also the only place the message
    ' is shown at all -- the grid has no message column.
    Private Sub ActualizeazaDetaliu(en As LogEntry)
        txtDetaliu.Text = If(en Is Nothing, String.Empty, en.Raw)
    End Sub

    ' =====================================================================
    ' ACTIONS
    ' =====================================================================

    ''' <summary>Copies the selected row, or -- with no selection -- everything shown now.</summary>
    Private Sub btnCopiaza_Click(sender As Object, e As EventArgs) Handles btnCopiaza.Click
        Try
            Dim text As String = TextulAfisat()
            If String.IsNullOrEmpty(text) Then
                lblStare.Text = "Nimic de copiat."
                Return
            End If
            Clipboard.SetText(text)
            lblStare.Text = "Copiat în clipboard."
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.btnCopiaza_Click", ex)
            KBotMessage.Show(FindForm(), "Copierea nu a reușit: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Function TextulAfisat() As String
        Dim curent As LogEntry = TryCast(grila.CurrentRow?.Tag, LogEntry)
        If curent IsNot Nothing Then Return curent.Raw
        Dim sb As New StringBuilder()
        For i As Integer = 0 To grila.RowCount - 1
            Dim en As LogEntry = TryCast(grila.Rows(i).Tag, LogEntry)
            If en IsNot Nothing Then sb.AppendLine(en.Raw)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Exports what the operator is LOOKING AT: the rows that pass the filters AND sit in an
    ''' expanded day (slice 0089). The days start collapsed, so opening a day is how the operator
    ''' picks what goes into the file; no day open = nothing exported, and the dialog is not
    ''' even shown. UTF-8 with BOM (Notepad wants them that way).
    ''' </summary>
    Private Sub btnExporta_Click(sender As Object, e As EventArgs) Handles btnExporta.Click
        Try
            Dim sb As New StringBuilder()
            For i As Integer = 0 To grila.RowCount - 1
                If Not grila.IsRowShown(i) Then Continue For
                Dim en As LogEntry = TryCast(grila.Rows(i).Tag, LogEntry)
                If en IsNot Nothing Then sb.AppendLine(en.Raw)
            Next
            If sb.Length = 0 Then
                lblStare.Text = If(grila.RowCount = 0,
                                   "Nimic de exportat.",
                                   "Nimic de exportat: deschideți zilele pe care vreți să le exportați.")
                Return
            End If
            Using dlg As New SaveFileDialog()
                dlg.Filter = "Fișier text (*.txt)|*.txt|Toate fișierele (*.*)|*.*"
                dlg.FileName = "jurnal_" & Date.Now.ToString("yyyyMMdd_HHmmss") & ".txt"
                If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                File.WriteAllText(dlg.FileName, sb.ToString(), New UTF8Encoding(True))
                lblStare.Text = "Exportat în " & dlg.FileName
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.btnExporta_Click", ex)
            KBotMessage.Show(FindForm(), "Exportul nu a reușit: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub btnDeschideDosar_Click(sender As Object, e As EventArgs) Handles btnDeschideDosar.Click
        Try
            Dim dir As String = LogPaths.EnsureLogsDirectory()
            Diagnostics.Process.Start("explorer.exe", """" & dir & """")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.btnDeschideDosar_Click", ex)
            KBotMessage.Show(FindForm(), "Dosarul nu s-a putut deschide: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' The only path that DELETES. Local files only -- server logs are never deleted from the
    ''' client, and the routes are and stay read-only. Two steps, because there is no «back».
    ''' </summary>
    Private Sub btnGoleste_Click(sender As Object, e As EventArgs) Handles btnGoleste.Click
        Try
            Using dlg As New LogClearDialog
                If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                lblStare.Text = dlg.Rezumat
            End Using
            ' The files changed under us: list and content start over.
            ConstruiesteListaFisiere()
            _suprimaEvenimente = True
            Try
                navFisiere.SelectedKey = KEY_TOATE
                _selectie = KEY_TOATE
            Finally
                _suprimaEvenimente = False
            End Try
            Dim ignorat As Task = IncarcaSelectiaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.btnGoleste_Click", ex)
            KBotMessage.Show(FindForm(), "Golirea nu a reușit: " & ex.Message, "Jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' =====================================================================
    ' FRIEND HOOKS FOR TESTS (headless, no screen and no disk)
    ' =====================================================================
    ' The page is never shown in tests, so Control.Visible of a child always returns False (no
    ' visible parent) -- that is why the notice state is also kept in a flag of its own, the
    ' only thing a test can honestly ask.

    Private _noticeServerAfisat As Boolean

    ' One place raises/lowers the server failure flag. The text itself is shown by AplicaFiltrul
    ' in noticeGol (the file panel that used to hold a notice of its own is hidden).
    Private Sub MarcheazaNoticeServer(afisat As Boolean)
        _noticeServerAfisat = afisat
    End Sub

    ''' <summary>Friend test hook: loads ready-made entries, skipping disk and network. Goes through the same sort as a real load.</summary>
    Friend Sub DebugIncarcaIntrari(entries As IEnumerable(Of LogEntry))
        _incarcate = OrdoneazaCeleMaiNoiPrimele(entries)
        ActualizeazaBadgeuri()
        AplicaFiltrul()
    End Sub

    ''' <summary>Friend test hook: sends a row down the REAL colouring path.</summary>
    Friend Sub DebugFormateazaRand(e As KBotRowFormattingEventArgs)
        grila_RowFormatting(grila, e)
    End Sub

    ''' <summary>Friend test hook: the text in the detail panel.</summary>
    Friend Function DebugTextDetaliu() As String
        Return txtDetaliu.Text
    End Function

    ''' <summary>Friend test hook: selects a row on the real path (grid -> detail).</summary>
    Friend Sub DebugSelecteazaRand(index As Integer)
        grila.CurrentRowIndex = index
        grila_SelectionChanged(grila, EventArgs.Empty)
    End Sub

    ''' <summary>Friend test hook: how many rows pass the current filters.</summary>
    Friend Function DebugNumarRanduri() As Integer
        Return grila.RowCount
    End Function

    ''' <summary>Friend test hook: the entry behind a grid row, in the order shown.</summary>
    Friend Function DebugIntrareaRandului(index As Integer) As LogEntry
        Return TryCast(grila.Rows(index).Tag, LogEntry)
    End Function

    ''' <summary>Friend test hook: asks the server journal on the real path (failure included); the grid is left alone.</summary>
    Friend Function DebugAduListaServerAsync() As Task
        Return AduJurnalServerAsync(1, SESIUNI(0), CancellationToken.None)
    End Function

    ''' <summary>Friend test hook: is the server notice shown?</summary>
    Friend Function DebugNoticeServerAfisat() As Boolean
        Return _noticeServerAfisat
    End Function

    ' The window went away under a load in flight: cancel it, so the background read does not
    ' come back to fill a grid that no longer has a handle. (Dispose belongs to the designer
    ' file; the handle is what the fill needs anyway.)
    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        Try
            _cts?.Cancel()
            _cts?.Dispose()
            _cts = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("SetariJurnalView.OnHandleDestroyed", ex)
        End Try
        MyBase.OnHandleDestroyed(e)
    End Sub
End Class
