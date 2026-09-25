Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>
''' The operator's application switches (slice 0072): what the settings window
''' («Setări») reads and writes. One JSON file per Windows user,
''' <c>%APPDATA%\AVACONT\KBot\app_settings.json</c>, next to <c>settings.json</c>
''' (folders) and <c>last_login.json</c>.
'''
''' <para><b>Why a new file and not <c>settings.json</c>.</b> <see cref="SetariFoldere"/>
''' reports every key it does not know as a problem at startup, on purpose: a typo in a
''' folder key must be seen. Putting booleans in there would either weaken that rule or
''' fill the startup log with «unknown key» lines. <c>kbot_paths.json</c> is per MACHINE
''' (what Adobe is installed) and stays where it is.</para>
'''
''' <para><b>Missing = default.</b> A file that does not exist, is empty or is broken
''' yields the defaults and logs (broken only); it never throws at startup. Every default
''' is the behaviour the application had before this slice, so an operator who never opens
''' the window gets exactly what they had.</para>
'''
''' <para><b>Effect at once.</b> <see cref="Save"/> makes the saved instance
''' <see cref="Current"/> and raises <see cref="Changed"/>, so surfaces that show a switch
''' (the FOREXE band, the options menu) can follow without a restart.</para>
''' </summary>
Public NotInheritable Class AppSettings

    ''' <summary>File name inside <see cref="SetariFoldere.DirectorSetari"/>.</summary>
    Public Const FileName As String = "app_settings.json"

    ''' <summary>Stored text for <see cref="AdobeDetachMode"/>: end the Adobe process we started.</summary>
    Public Const DetachKillProcess As String = "KillProcess"
    ''' <summary>Stored text for <see cref="AdobeDetachMode"/>: close only the hosted window.</summary>
    Public Const DetachCloseWindow As String = "CloseWindow"

    ''' <summary>Stored text for <see cref="ExcelRibbon"/>: hide the ribbon through the Excel 4 macro.</summary>
    Public Const RibbonExcel4Macro As String = "Excel4Macro"
    ''' <summary>Stored text for <see cref="ExcelRibbon"/>: hide the ribbon's own window (Word's method).</summary>
    Public Const RibbonHideDockWindow As String = "HideDockWindow"

    ''' <summary>Stored text for <see cref="TreeSort"/>: by name (Descriere).</summary>
    Public Const TreeSortName As String = "Name"
    ''' <summary>Stored text for <see cref="TreeSort"/>: by DataCreare.</summary>
    Public Const TreeSortDate As String = "Date"

    ' ── Journals ─────────────────────────────────────────────────────────

    ''' <summary>
    ''' The FOREXE console shows every diagnostic line (steps, waits, docking, stacks), not
    ''' only <c>&lt;Log&gt;</c> and errors. <c>Nothing</c> = the build's own default (on in
    ''' Debug, off in Release) -- see <c>RichTextBoxLogger.VerboseLogging</c>, slice 0071.
    ''' </summary>
    Public Property VerboseLogging As Boolean?

    ''' <summary>The «Arată jurnal» row of the shell's options menu is offered.</summary>
    Public Property LogViewerEnabled As Boolean = True

    ' ── FOREXE ───────────────────────────────────────────────────────────

    ''' <summary>The «Arată browserul» button of the FOREXE band is offered while connected.</summary>
    Public Property ShowBrowserButton As Boolean = True

    ''' <summary>
    ''' While the browser is docked in the viewer, its own toolbar (tabs, address bar) stays
    ''' out of the panel. Applied to every executor at job start.
    ''' </summary>
    Public Property ForexeHideBrowserChrome As Boolean = True

    ''' <summary>The reception picker opens with every reception already ticked.</summary>
    Public Property ReceptiiCheckedOnOpen As Boolean = True

    ''' <summary>
    ''' The advanced settings pages are shown: the documents tab of the application page, the
    ''' FOREXE page styles, the theme and the file paths. Switching it on asks for a password
    ''' (operator, 24.09.2026);
    ''' off by default, so an operator sees only the everyday pages.
    ''' </summary>
    Public Property AdvancedOptions As Boolean = False

    ''' <summary>
    ''' The browser's developer tools (F12, Ctrl+Shift+I / J / C, the context menu's
    ''' «Inspect») stay reachable in the FOREXE page. Off by default: the page script
    ''' swallows those keys and the context menu (operator, 21.09.2026).
    ''' </summary>
    Public Property ForexeDevToolsAllowed As Boolean = False

    ''' <summary>
    ''' The operator's CSS rules for the FOREXE page, written into every load and refresh.
    ''' Missing from the file = <see cref="PageStyleRule.Defaults"/>; an empty list in the
    ''' file means the operator removed them all and stays empty.
    ''' </summary>
    Public Property ForexePageStyles As List(Of PageStyleRule) = PageStyleRule.Defaults()

    ' ── Documents ────────────────────────────────────────────────────────

    ''' <summary>
    ''' How a hosted Adobe window is let go when the document changes: «KillProcess» (A) or
    ''' «CloseWindow» (B). Text, because the enum lives in KBot.Controls (cycle otherwise).
    ''' </summary>
    Public Property AdobeDetachMode As String = DetachKillProcess

    ''' <summary>Hide Adobe's floating popup (AVL_AVPopup) while a document is hosted.</summary>
    Public Property AdobePopupWatch As Boolean = True

    ''' <summary>
    ''' ActiveX viewer (DDF and ORD): destroy the AcroPDF control whenever another document is
    ''' asked for and create a new one for it, instead of loading into the same control. A reused
    ''' control sometimes stays empty after LoadFile; a fresh one built at once in every client
    ''' trace (operator, 24.09.2026). Off by default.
    ''' </summary>
    Public Property AcroPdfFreshControl As Boolean = False

    ''' <summary>
    ''' Adobe script alerts («Warning: JavaScript Window») that K-BOT closes by itself: one
    ''' regular expression per entry, matched (case-insensitive, anywhere in the text) against the
    ''' alert's message. An alert that matches none is LEFT ON SCREEN for the operator -- the forms
    ''' also use alerts to tell the operator something («Validarea s-a terminat cu succes!...»),
    ''' and closing those left the operator not knowing what happened (operator, 24.09.2026).
    ''' Missing from the file = <see cref="DefaultAdobeTrappedAlerts"/>; an empty list stays empty
    ''' (nothing is closed automatically).
    ''' </summary>
    Public Property AdobeTrappedAlerts As List(Of String) = DefaultAdobeTrappedAlerts()

    ''' <summary>
    ''' The alerts closed automatically before the list existed, as seen in the working logs up to
    ''' 24.09.2026: «GeneralError / Operation failed.» and «TypeError: sumCell5.toFixed is not a
    ''' function», raised by the DDF / ORD scripts on open and changing nothing in the file.
    ''' </summary>
    Public Shared Function DefaultAdobeTrappedAlerts() As List(Of String)
        Return New List(Of String) From {"GeneralError", "Operation failed", "TypeError"}
    End Function

    ''' <summary>
    ''' How the Excel ribbon is taken down in the hosted preview: «Excel4Macro» or
    ''' «HideDockWindow». Text, for the same reason as <see cref="AdobeDetachMode"/>.
    ''' </summary>
    Public Property ExcelRibbon As String = RibbonHideDockWindow

    ' ── Main tree (slice 0777) ──────────────────────────────────────────────

    ''' <summary>
    ''' The order of the main tree: <see cref="TreeSortName"/> (by the angajament's name,
    ''' the Descriere) or <see cref="TreeSortDate"/> (by DataCreare; rows without a date go
    ''' last, by name). Text, so a value this build does not know falls back to the default
    ''' instead of breaking the load.
    ''' </summary>
    Public Property TreeSort As String = TreeSortName

    ''' <summary>
    ''' The direction of <see cref="TreeSort"/>: False = ascending (A..Z, oldest first -- the
    ''' default, operator 23.09.2026), True = descending. Rows without a date stay last either way.
    ''' </summary>
    Public Property TreeSortDescending As Boolean = False

    ''' <summary>Sorted by name: the CODANGAJAMENT column is shown.</summary>
    Public Property TreeNameShowCod As Boolean = True
    ''' <summary>Sorted by name: the SURSE column is shown.</summary>
    Public Property TreeNameShowSurse As Boolean = False
    ''' <summary>Sorted by date: the CODANGAJAMENT column is shown (off by default, operator 23.09.2026).</summary>
    Public Property TreeDateShowCod As Boolean = False
    ''' <summary>Sorted by date: the SURSE column is shown (on by default, operator 23.09.2026).</summary>
    Public Property TreeDateShowSurse As Boolean = True

    ''' <summary>Smallest / largest column width the settings page accepts, logical px (96 dpi).</summary>
    Public Const TreeColumnWidthMin As Integer = 30
    Public Const TreeColumnWidthMax As Integer = 600

    ''' <summary>Width of the CODANGAJAMENT column, logical px (96 dpi). Same under both sorts.</summary>
    Public Property TreeCodColumnWidth As Integer = 100
    ''' <summary>Width of the SURSE column, logical px (96 dpi). Same under both sorts.</summary>
    Public Property TreeSurseColumnWidth As Integer = 70

    ''' <summary>True when <paramref name="width"/> is inside [TreeColumnWidthMin, TreeColumnWidthMax].</summary>
    Public Shared Function IsValidTreeColumnWidth(width As Integer) As Boolean
        Return width >= TreeColumnWidthMin AndAlso width <= TreeColumnWidthMax
    End Function

    ''' <summary>True when <see cref="TreeSort"/> asks for the date order; anything else is by name.</summary>
    Public ReadOnly Property TreeSortIsDate As Boolean
        Get
            Return String.Equals(TreeSort, TreeSortDate, StringComparison.OrdinalIgnoreCase)
        End Get
    End Property

    ''' <summary>The CODANGAJAMENT column is shown under the sort in force.</summary>
    Public ReadOnly Property TreeShowCod As Boolean
        Get
            Return If(TreeSortIsDate, TreeDateShowCod, TreeNameShowCod)
        End Get
    End Property

    ''' <summary>The SURSE column is shown under the sort in force.</summary>
    Public ReadOnly Property TreeShowSurse As Boolean
        Get
            Return If(TreeSortIsDate, TreeDateShowSurse, TreeNameShowSurse)
        End Get
    End Property

    ' ── Bank statements (slice 0080-02) ──────────────────────────────────

    ''' <summary>
    ''' The columns of the four statement grids, in order (<see cref="ExtraseGrid"/>). Nothing =
    ''' the defaults of <see cref="ExtraseColumns.Defaults"/>; read them through
    ''' <see cref="ExtraseColumnsFor"/>, which also drops keys this build does not know.
    ''' </summary>
    Public Property ExtraseViewHeaderColumns As List(Of String)
    Public Property ExtraseViewOperationColumns As List(Of String)
    Public Property ExtraseWindowHeaderColumns As List(Of String)
    Public Property ExtraseWindowOperationColumns As List(Of String)

    ''' <summary>The columns in force for <paramref name="grid"/>: the stored ones, or the defaults.</summary>
    Public Function ExtraseColumnsFor(grid As ExtraseGrid) As List(Of String)
        Return ExtraseColumns.Normalize(grid, StoredExtraseColumns(grid))
    End Function

    ''' <summary>Stores <paramref name="keys"/> for <paramref name="grid"/>; Nothing goes back to the defaults.</summary>
    Public Sub SetExtraseColumns(grid As ExtraseGrid, keys As IEnumerable(Of String))
        Dim value As List(Of String) = If(keys Is Nothing, Nothing, ExtraseColumns.Normalize(grid, keys))
        Select Case grid
            Case ExtraseGrid.ViewHeaders : ExtraseViewHeaderColumns = value
            Case ExtraseGrid.ViewOperations : ExtraseViewOperationColumns = value
            Case ExtraseGrid.WindowHeaders : ExtraseWindowHeaderColumns = value
            Case ExtraseGrid.WindowOperations : ExtraseWindowOperationColumns = value
            Case Else
                Throw New ArgumentException($"Grilă de extrase necunoscută: '{grid}'.", NameOf(grid))
        End Select
    End Sub

    Private Function StoredExtraseColumns(grid As ExtraseGrid) As List(Of String)
        Select Case grid
            Case ExtraseGrid.ViewHeaders : Return ExtraseViewHeaderColumns
            Case ExtraseGrid.ViewOperations : Return ExtraseViewOperationColumns
            Case ExtraseGrid.WindowHeaders : Return ExtraseWindowHeaderColumns
            Case ExtraseGrid.WindowOperations : Return ExtraseWindowOperationColumns
            Case Else
                Throw New ArgumentException($"Grilă de extrase necunoscută: '{grid}'.", NameOf(grid))
        End Select
    End Function

    ' ── Login ────────────────────────────────────────────────────────────

    ''' <summary>The login form pre-fills the last user name that got in.</summary>
    Public Property RememberLastLogin As Boolean = True

    ''' <summary>The login form pre-selects the unit that user picked last time.</summary>
    Public Property RememberLastUnit As Boolean = True

    ' ── Store ────────────────────────────────────────────────────────────

    Private Shared ReadOnly _gate As New Object()
    Private Shared _current As AppSettings

    ''' <summary>Raised after <see cref="Save"/> replaced <see cref="Current"/>.</summary>
    Public Shared Event Changed As EventHandler

    ''' <summary>Full path of the store file, in the given folder or the per-user default.</summary>
    Public Shared Function FilePath(Optional dir As String = Nothing) As String
        Return Path.Combine(If(String.IsNullOrEmpty(dir), SetariFoldere.DirectorSetari(), dir), FileName)
    End Function

    ''' <summary>The settings in force, loaded once. Thread-safe.</summary>
    Public Shared ReadOnly Property Current As AppSettings
        Get
            If _current Is Nothing Then
                SyncLock _gate
                    If _current Is Nothing Then _current = Load()
                End SyncLock
            End If
            Return _current
        End Get
    End Property

    ''' <summary>
    ''' Reads the file. Missing / empty = defaults, silently; broken = defaults + log.
    ''' Never throws: this runs at startup, before any window is up.
    ''' </summary>
    Public Shared Function Load(Optional dir As String = Nothing) As AppSettings
        Dim cale As String = FilePath(dir)
        Try
            If Not File.Exists(cale) Then Return New AppSettings()
            Dim json As String = File.ReadAllText(cale)
            If String.IsNullOrWhiteSpace(json) Then Return New AppSettings()

            Dim dto As AppSettingsDto = JsonSerializer.Deserialize(Of AppSettingsDto)(json, JsonOptions)
            If dto Is Nothing Then Return New AppSettings()
            Return FromDto(dto)
        Catch ex As Exception
            GlobalErrorLog.Write("AppSettings.Load", ex)
            Return New AppSettings()
        End Try
    End Function

    ''' <summary>
    ''' Writes this instance and makes it <see cref="Current"/> (an explicit <paramref name="dir"/>
    ''' writes there only -- tests and tools -- and leaves the singleton alone).
    ''' </summary>
    ''' <remarks>I/O boundary: logs and rethrows. A form that believes it saved when it did not
    ''' is worse than one that complains.</remarks>
    Public Sub Save(Optional dir As String = Nothing)
        Dim isDefaultDir As Boolean = String.IsNullOrEmpty(dir)
        Try
            Dim target As String = FilePath(dir)
            Directory.CreateDirectory(Path.GetDirectoryName(target))
            File.WriteAllText(target, JsonSerializer.Serialize(ToDto(), JsonOptions), New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("AppSettings.Save", ex)
            Throw
        End Try
        If isDefaultDir Then MakeCurrent()
    End Sub

    ''' <summary>A copy the settings window can edit without touching <see cref="Current"/>.</summary>
    Public Function Clone() As AppSettings
        Return FromDto(ToDto())
    End Function

    Private Sub MakeCurrent()
        SyncLock _gate
            _current = Me
        End SyncLock
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub

    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .PropertyNameCaseInsensitive = True,
        .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull}

    Private Function ToDto() As AppSettingsDto
        Return New AppSettingsDto With {
            .VerboseLogging = VerboseLogging,
            .LogViewerEnabled = LogViewerEnabled,
            .ShowBrowserButton = ShowBrowserButton,
            .ForexeHideBrowserChrome = ForexeHideBrowserChrome,
            .ReceptiiCheckedOnOpen = ReceptiiCheckedOnOpen,
            .AdvancedOptions = AdvancedOptions,
            .ForexeDevToolsAllowed = ForexeDevToolsAllowed,
            .ForexePageStyles = ForexePageStyles?.Select(Function(r) New PageStyleRuleDto With {
                .Enabled = r.Enabled, .Selector = r.Selector, .Css = r.Css, .Note = r.Note, .Page = r.Page}).ToList(),
            .AdobeDetachMode = AdobeDetachMode,
            .AdobePopupWatch = AdobePopupWatch,
            .AcroPdfFreshControl = AcroPdfFreshControl,
            .AdobeTrappedAlerts = AdobeTrappedAlerts?.ToList(),
            .ExcelRibbon = ExcelRibbon,
            .TreeSort = TreeSort,
            .TreeSortDescending = TreeSortDescending,
            .TreeNameShowCod = TreeNameShowCod,
            .TreeNameShowSurse = TreeNameShowSurse,
            .TreeDateShowCod = TreeDateShowCod,
            .TreeDateShowSurse = TreeDateShowSurse,
            .TreeCodColumnWidth = TreeCodColumnWidth,
            .TreeSurseColumnWidth = TreeSurseColumnWidth,
            .RememberLastLogin = RememberLastLogin,
            .RememberLastUnit = RememberLastUnit,
            .ExtraseViewHeaderColumns = ExtraseViewHeaderColumns?.ToList(),
            .ExtraseViewOperationColumns = ExtraseViewOperationColumns?.ToList(),
            .ExtraseWindowHeaderColumns = ExtraseWindowHeaderColumns?.ToList(),
            .ExtraseWindowOperationColumns = ExtraseWindowOperationColumns?.ToList()}
    End Function

    ' A key missing from the file keeps its default: every DTO member is nullable.
    Private Shared Function FromDto(dto As AppSettingsDto) As AppSettings
        Dim s As New AppSettings()
        s.VerboseLogging = dto.VerboseLogging
        If dto.LogViewerEnabled.HasValue Then s.LogViewerEnabled = dto.LogViewerEnabled.Value
        If dto.ShowBrowserButton.HasValue Then s.ShowBrowserButton = dto.ShowBrowserButton.Value
        If dto.ForexeHideBrowserChrome.HasValue Then s.ForexeHideBrowserChrome = dto.ForexeHideBrowserChrome.Value
        If dto.ReceptiiCheckedOnOpen.HasValue Then s.ReceptiiCheckedOnOpen = dto.ReceptiiCheckedOnOpen.Value
        If dto.AdvancedOptions.HasValue Then s.AdvancedOptions = dto.AdvancedOptions.Value
        If dto.ForexeDevToolsAllowed.HasValue Then s.ForexeDevToolsAllowed = dto.ForexeDevToolsAllowed.Value
        If dto.ForexePageStyles IsNot Nothing Then
            s.ForexePageStyles = dto.ForexePageStyles.
                Where(Function(r) r IsNot Nothing).
                Select(Function(r) New PageStyleRule(r.Note, r.Selector, r.Css, r.Page) With {
                    .Enabled = If(r.Enabled, True)}).ToList()
        End If
        If Not String.IsNullOrWhiteSpace(dto.AdobeDetachMode) Then s.AdobeDetachMode = dto.AdobeDetachMode.Trim()
        If dto.AdobePopupWatch.HasValue Then s.AdobePopupWatch = dto.AdobePopupWatch.Value
        If dto.AcroPdfFreshControl.HasValue Then s.AcroPdfFreshControl = dto.AcroPdfFreshControl.Value
        If dto.AdobeTrappedAlerts IsNot Nothing Then
            s.AdobeTrappedAlerts = dto.AdobeTrappedAlerts.
                Where(Function(p) Not String.IsNullOrWhiteSpace(p)).
                Select(Function(p) p.Trim()).ToList()
        End If
        If Not String.IsNullOrWhiteSpace(dto.ExcelRibbon) Then s.ExcelRibbon = dto.ExcelRibbon.Trim()
        If Not String.IsNullOrWhiteSpace(dto.TreeSort) Then s.TreeSort = dto.TreeSort.Trim()
        If dto.TreeSortDescending.HasValue Then s.TreeSortDescending = dto.TreeSortDescending.Value
        If dto.TreeNameShowCod.HasValue Then s.TreeNameShowCod = dto.TreeNameShowCod.Value
        If dto.TreeNameShowSurse.HasValue Then s.TreeNameShowSurse = dto.TreeNameShowSurse.Value
        If dto.TreeDateShowCod.HasValue Then s.TreeDateShowCod = dto.TreeDateShowCod.Value
        If dto.TreeDateShowSurse.HasValue Then s.TreeDateShowSurse = dto.TreeDateShowSurse.Value
        ' A width out of range in the file (hand-edited) keeps the default instead of drawing
        ' a column of 0 or 5000 px.
        If dto.TreeCodColumnWidth.HasValue AndAlso IsValidTreeColumnWidth(dto.TreeCodColumnWidth.Value) Then
            s.TreeCodColumnWidth = dto.TreeCodColumnWidth.Value
        End If
        If dto.TreeSurseColumnWidth.HasValue AndAlso IsValidTreeColumnWidth(dto.TreeSurseColumnWidth.Value) Then
            s.TreeSurseColumnWidth = dto.TreeSurseColumnWidth.Value
        End If
        If dto.RememberLastLogin.HasValue Then s.RememberLastLogin = dto.RememberLastLogin.Value
        If dto.RememberLastUnit.HasValue Then s.RememberLastUnit = dto.RememberLastUnit.Value
        ' A list missing from the file stays Nothing (= defaults); a present one is kept as the
        ' operator saved it and cleaned only when read (ExtraseColumnsFor).
        s.ExtraseViewHeaderColumns = dto.ExtraseViewHeaderColumns?.ToList()
        s.ExtraseViewOperationColumns = dto.ExtraseViewOperationColumns?.ToList()
        s.ExtraseWindowHeaderColumns = dto.ExtraseWindowHeaderColumns?.ToList()
        s.ExtraseWindowOperationColumns = dto.ExtraseWindowOperationColumns?.ToList()
        Return s
    End Function

End Class

''' <summary>Wire DTO. The property name IS the JSON key. POCO, no Try/Catch.</summary>
Friend NotInheritable Class AppSettingsDto
    Public Property VerboseLogging As Boolean?
    Public Property LogViewerEnabled As Boolean?
    Public Property ShowBrowserButton As Boolean?
    Public Property ForexeHideBrowserChrome As Boolean?
    Public Property ReceptiiCheckedOnOpen As Boolean?
    Public Property AdvancedOptions As Boolean?
    Public Property ForexeDevToolsAllowed As Boolean?
    Public Property ForexePageStyles As List(Of PageStyleRuleDto)
    Public Property AdobeDetachMode As String
    Public Property AdobePopupWatch As Boolean?
    Public Property AcroPdfFreshControl As Boolean?
    Public Property AdobeTrappedAlerts As List(Of String)
    Public Property ExcelRibbon As String
    Public Property TreeSort As String
    Public Property TreeSortDescending As Boolean?
    Public Property TreeNameShowCod As Boolean?
    Public Property TreeNameShowSurse As Boolean?
    Public Property TreeDateShowCod As Boolean?
    Public Property TreeDateShowSurse As Boolean?
    Public Property TreeCodColumnWidth As Integer?
    Public Property TreeSurseColumnWidth As Integer?
    Public Property RememberLastLogin As Boolean?
    Public Property RememberLastUnit As Boolean?
    Public Property ExtraseViewHeaderColumns As List(Of String)
    Public Property ExtraseViewOperationColumns As List(Of String)
    Public Property ExtraseWindowHeaderColumns As List(Of String)
    Public Property ExtraseWindowOperationColumns As List(Of String)
End Class

''' <summary>Wire shape of one page style rule. POCO.</summary>
Friend NotInheritable Class PageStyleRuleDto
    Public Property Enabled As Boolean?
    Public Property Selector As String
    Public Property Css As String
    Public Property Note As String
    Public Property Page As String
End Class
