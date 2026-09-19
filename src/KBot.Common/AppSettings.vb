Option Strict On
Imports System.IO
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

    ' ── Documents ────────────────────────────────────────────────────────

    ''' <summary>
    ''' How a hosted Adobe window is let go when the document changes: «KillProcess» (A) or
    ''' «CloseWindow» (B). Text, because the enum lives in KBot.Controls (cycle otherwise).
    ''' </summary>
    Public Property AdobeDetachMode As String = DetachKillProcess

    ''' <summary>Hide Adobe's floating popup (AVL_AVPopup) while a document is hosted.</summary>
    Public Property AdobePopupWatch As Boolean = True

    ''' <summary>
    ''' How the Excel ribbon is taken down in the hosted preview: «Excel4Macro» or
    ''' «HideDockWindow». Text, for the same reason as <see cref="AdobeDetachMode"/>.
    ''' </summary>
    Public Property ExcelRibbon As String = RibbonHideDockWindow

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
            .AdobeDetachMode = AdobeDetachMode,
            .AdobePopupWatch = AdobePopupWatch,
            .ExcelRibbon = ExcelRibbon,
            .RememberLastLogin = RememberLastLogin,
            .RememberLastUnit = RememberLastUnit}
    End Function

    ' A key missing from the file keeps its default: every DTO member is nullable.
    Private Shared Function FromDto(dto As AppSettingsDto) As AppSettings
        Dim s As New AppSettings()
        s.VerboseLogging = dto.VerboseLogging
        If dto.LogViewerEnabled.HasValue Then s.LogViewerEnabled = dto.LogViewerEnabled.Value
        If dto.ShowBrowserButton.HasValue Then s.ShowBrowserButton = dto.ShowBrowserButton.Value
        If dto.ForexeHideBrowserChrome.HasValue Then s.ForexeHideBrowserChrome = dto.ForexeHideBrowserChrome.Value
        If dto.ReceptiiCheckedOnOpen.HasValue Then s.ReceptiiCheckedOnOpen = dto.ReceptiiCheckedOnOpen.Value
        If Not String.IsNullOrWhiteSpace(dto.AdobeDetachMode) Then s.AdobeDetachMode = dto.AdobeDetachMode.Trim()
        If dto.AdobePopupWatch.HasValue Then s.AdobePopupWatch = dto.AdobePopupWatch.Value
        If Not String.IsNullOrWhiteSpace(dto.ExcelRibbon) Then s.ExcelRibbon = dto.ExcelRibbon.Trim()
        If dto.RememberLastLogin.HasValue Then s.RememberLastLogin = dto.RememberLastLogin.Value
        If dto.RememberLastUnit.HasValue Then s.RememberLastUnit = dto.RememberLastUnit.Value
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
    Public Property AdobeDetachMode As String
    Public Property AdobePopupWatch As Boolean?
    Public Property ExcelRibbon As String
    Public Property RememberLastLogin As Boolean?
    Public Property RememberLastUnit As Boolean?
End Class
