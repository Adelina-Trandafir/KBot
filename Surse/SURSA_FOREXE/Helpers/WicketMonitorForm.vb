Imports System.Drawing
Imports System.Windows.Forms
Imports WorkflowModels

' =============================================================================
'  WicketMonitorForm — fereastra modeless de monitorizare Wicket
'
'  Stream-uri afisate:
'    [WFL]    — actiuni WFL la momentul pornirii (OnActionStart)
'    [WICKET] — schimbari brute #statlogo / #animlogo (Source=WICKET)
'    IDLE     — confirmare idle stabil, vizual distinct (Source=IDLE)
'    [CLICK]  — click utilizator: tag.cls  props per tip  <- parinte
'    [KEY]    — primul input intr-un element nou: tag.cls  props per tip
'
'  Controalele sunt declarate in WicketMonitorForm.Designer.vb.
'  Culorile structurale → KBotTheme.ApplyTheme(Me).
'  Culorile semantice → proprietati theme-aware (KBotTheme.IsDark).
' =============================================================================
Public Class WicketMonitorForm

    Private _attachedExecutor As WorkflowExecutor = Nothing

    ' =========================================================================
    '  Culori semantice theme-aware
    ' =========================================================================
    Private ReadOnly Property ClrTimestamp As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(110, 110, 110),
                      Color.FromArgb(150, 150, 150))
        End Get
    End Property

    Private ReadOnly Property ClrWflTag As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(86, 156, 214),
                      Color.FromArgb(0, 90, 170))
        End Get
    End Property

    Private ReadOnly Property ClrWflText As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(170, 205, 240),
                      Color.FromArgb(0, 60, 130))
        End Get
    End Property

    Private ReadOnly Property ClrWicketTag As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(220, 130, 50),
                      Color.FromArgb(180, 90, 0))
        End Get
    End Property

    Private ReadOnly Property ClrWicketLoad As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(220, 155, 80),
                      Color.FromArgb(190, 110, 20))
        End Get
    End Property

    Private ReadOnly Property ClrWicketNone As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(80, 200, 120),
                      Color.FromArgb(0, 140, 60))
        End Get
    End Property

    ' Verde saturat — rezervat exclusiv pentru IDLE CONFIRMAT
    Private ReadOnly Property ClrIdle As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(40, 230, 100),
                      Color.FromArgb(0, 175, 65))
        End Get
    End Property

    Private ReadOnly Property ClrElementName As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(185, 185, 185),
                      Color.FromArgb(70, 70, 70))
        End Get
    End Property

    ' ── CLICK — violet ────────────────────────────────────────────────────────
    Private ReadOnly Property ClrClickTag As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(180, 120, 220),
                      Color.FromArgb(120, 60, 160))
        End Get
    End Property

    Private ReadOnly Property ClrClickInfo As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(210, 180, 240),
                      Color.FromArgb(90, 50, 130))
        End Get
    End Property

    ' ── KEY — teal ───────────────────────────────────────────────────────────
    Private ReadOnly Property ClrKeyTag As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(80, 210, 210),
                      Color.FromArgb(0, 140, 140))
        End Get
    End Property

    Private ReadOnly Property ClrKeyInfo As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(160, 235, 235),
                      Color.FromArgb(0, 105, 105))
        End Get
    End Property

    ' =========================================================================
    '  Constructor
    ' =========================================================================
    Public Sub New()
        InitializeComponent()
        KBotTheme.ApplyTheme(Me)
    End Sub

    ' =========================================================================
    '  AttachExecutor / DetachExecutor
    ' =========================================================================
    Public Sub AttachExecutor(executor As WorkflowExecutor)
        DetachExecutor()
        _attachedExecutor = executor
        AddHandler executor.OnWicketStateChange, AddressOf HandleWicketStateChange
        AddHandler executor.OnActionStart, AddressOf HandleActionStart
    End Sub

    Public Sub DetachExecutor()
        If _attachedExecutor Is Nothing Then Return
        RemoveHandler _attachedExecutor.OnWicketStateChange, AddressOf HandleWicketStateChange
        RemoveHandler _attachedExecutor.OnActionStart, AddressOf HandleActionStart
        _attachedExecutor = Nothing
    End Sub

    ' =========================================================================
    '  Handlere eveniment executor
    ' =========================================================================
    Private Sub HandleWicketStateChange(entry As WicketMonitorEntry)
        AppendEntry(entry)
    End Sub

    Private Sub HandleActionStart(action As IWorkflowAction)
        Dim description As String
        If Not String.IsNullOrEmpty(action.LogValue) Then
            description = action.LogValue
        Else
            Dim selProp = action.GetType().GetProperty("Selector")
            Dim sel As String = Nothing
            If selProp IsNot Nothing Then sel = TryCast(selProp.GetValue(action), String)
            description = If(Not String.IsNullOrEmpty(sel),
                             $"{action.ActionType}: {sel}",
                             action.ActionType)
        End If

        AppendEntry(New WicketMonitorEntry With {
            .Timestamp = DateTime.Now,
            .Source = "WFL",
            .Element = action.ActionType,
            .State = description,
            .PageUrl = ""
        })
    End Sub

    ' =========================================================================
    '  AppendEntry — thread-safe
    ' =========================================================================
    Private Sub AppendEntry(entry As WicketMonitorEntry)
        If Me.IsDisposed Then Return
        If Me.InvokeRequired Then
            Me.Invoke(Sub() AppendEntry(entry))
            Return
        End If

        Dim ts As String = entry.Timestamp.ToString("HH:mm:ss.fff")

        Select Case entry.Source

            Case "WFL"
                AppendColored($"[{ts}]  ", ClrTimestamp)
                AppendColored("[WFL]    ", ClrWflTag)
                AppendColored($"{entry.State}{Environment.NewLine}", ClrWflText)

            Case "WICKET"
                AppendColored($"[{ts}]  ", ClrTimestamp)
                AppendColored("[WICKET]  ", ClrWicketTag)
                AppendColored(entry.Element, ClrElementName)

                Dim isNone As Boolean = String.Equals(entry.State, "none",
                                                      StringComparison.OrdinalIgnoreCase)
                Dim stateColor As Color = If(isNone, ClrWicketNone, ClrWicketLoad)
                AppendColored($" → {entry.State}{Environment.NewLine}", stateColor)

            Case "IDLE"
                Dim sep As New String("─"c, 54)
                AppendColored($"{sep}{Environment.NewLine}", ClrIdle)
                AppendColored($"  ✦  {entry.Element}  {entry.State}{Environment.NewLine}", ClrIdle)
                AppendColored($"{sep}{Environment.NewLine}", ClrIdle)

            Case "CLICK"
                ' [CLICK]   input.form-control  name:cod  type:text  value:(empty)  ← div.well  text:Cauta
                AppendColored($"[{ts}]  ", ClrTimestamp)
                AppendColored("[CLICK]   ", ClrClickTag)
                AppendColored(entry.Element, ClrElementName)
                AppendColored($"  {entry.State}{Environment.NewLine}", ClrClickInfo)

            Case "KEY"
                ' [KEY]     input.form-control  name:cod  type:text  value:(empty)
                AppendColored($"[{ts}]  ", ClrTimestamp)
                AppendColored("[KEY]     ", ClrKeyTag)
                AppendColored(entry.Element, ClrElementName)
                AppendColored($"  {entry.State}{Environment.NewLine}", ClrKeyInfo)

        End Select

        rtbMonitor.SelectionStart = rtbMonitor.Text.Length
        rtbMonitor.ScrollToCaret()
    End Sub

    Private Sub AppendColored(text As String, color As Color)
        rtbMonitor.SelectionStart = rtbMonitor.TextLength
        rtbMonitor.SelectionLength = 0
        rtbMonitor.SelectionColor = color
        rtbMonitor.AppendText(text)
        rtbMonitor.SelectionColor = rtbMonitor.ForeColor
    End Sub

    ' =========================================================================
    '  Event handlers UI
    ' =========================================================================
    Private Sub BtnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        rtbMonitor.Clear()
    End Sub

    Private Sub WicketMonitorForm_FormClosing(sender As Object, e As FormClosingEventArgs) _
        Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
        End If
    End Sub

    Private Sub PnlBottom_Resize(sender As Object, e As EventArgs) Handles pnlBottom.Resize
        btnClear.Left = pnlBottom.Width - btnClear.Width - 8
    End Sub

    ' =========================================================================
    '  Dispose
    ' =========================================================================
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            DetachExecutor()
            If components IsNot Nothing Then components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class