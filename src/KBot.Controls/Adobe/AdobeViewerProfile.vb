Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' Which Adobe host profile the operator wants. Three values, not two: <see cref="Auto"/> detects
''' the installed viewer generation from the window tree and picks for itself, the other two force.
''' Stored per installation in <c>&lt;AppDir&gt;\kbot_paths.json</c> — see
''' <c>docs\SETARI_UTILIZATOR.md</c>.
''' </summary>
Public Enum AdobeViewerMode
    ''' <summary>Detect after embedding and use the matching profile (default).</summary>
    Auto = 0
    ''' <summary>Force the modern profile (Acrobat with bEnableAv2 = 1).</summary>
    Modern = 1
    ''' <summary>Force the classic profile (Acrobat with bEnableAv2 = 0).</summary>
    Classic = 2
End Enum

''' <summary>
''' Which surface renders the PDF on the DDF «Document» tab.
'''
''' Two genuinely different mechanisms, not two settings of one: <see cref="WindowHost"/> starts
''' Adobe as a separate process and reparents its window; <see cref="ActiveX"/> loads the AcroPDF
''' control INSIDE this process. They cannot both show the same document at once — Adobe is
''' effectively single-instance and the control is served by the same engine — so this is a choice,
''' never a combination.
''' </summary>
Public Enum AdobePreviewEngine
    ''' <summary>Reparent a real Adobe window (the default, and the only one ever run in the app).</summary>
    WindowHost = 0
    ''' <summary>The in-process AcroPDF ActiveX control.</summary>
    ActiveX = 1
End Enum

''' <summary>
''' Whether Adobe is launched with «/n» (a NEW instance). Separate from
''' <see cref="AdobeViewerMode"/> because it is the one profile value with a consequence OUTSIDE
''' the preview: with <c>/n</c> off, Adobe may hand the document to an instance the operator opened
''' themselves, and K-BOT then reparents a window it did not create.
''' </summary>
Public Enum AdobeNewInstanceMode
    ''' <summary>Whatever the resolved profile says (default).</summary>
    Auto = 0
    ''' <summary>Always launch with «/n», whatever the profile says.</summary>
    Da = 1
    ''' <summary>Never launch with «/n», whatever the profile says.</summary>
    Nu = 2
End Enum

''' <summary>
''' One complete recipe for hosting Adobe: how to launch it, which /A open parameters to pass, and
''' how to place the window once it is embedded.
'''
''' THE NUMBERS ARE MEASURED, NOT CHOSEN. Both profiles are transcriptions of bench states saved by
''' <c>AdobeReaderHarnessForm</c> on 04.08.2026 (20:06 modern, 20:10 classic) against Acrobat
''' 26.1.21771.0. They are pinned by <c>AdobeViewerProfileTests</c> precisely so an accidental edit
''' fails a test instead of quietly breaking the preview on the operator's machine.
''' </summary>
Public NotInheritable Class AdobeViewerProfile

    ''' <summary>Short name used in log lines and in the documentation.</summary>
    Public ReadOnly Property Name As String
    ''' <summary>Launch with «/n» (a new Adobe instance).</summary>
    Public ReadOnly Property NewInstance As Boolean
    ''' <summary>Launch with «/s» (no splash screen).</summary>
    Public ReadOnly Property NoSplash As Boolean
    ''' <summary>The /A open parameters, already formed («toolbar=0»), in the order Adobe gets them.</summary>
    Public ReadOnly Property OpenParameters As IReadOnlyList(Of String)
    Public ReadOnly Property ClipEnabled As Boolean
    Public ReadOnly Property ClipRight As Integer
    Public ReadOnly Property ClipTop As Integer
    Public ReadOnly Property Dx As Integer
    Public ReadOnly Property Dy As Integer
    Public ReadOnly Property Dw As Integer
    Public ReadOnly Property Dh As Integer
    ''' <summary>
    ''' Hide the floating <c>AVL_AVPopup</c> badge that Adobe puts over its own window. True on both
    ''' profiles: it is not expressible in a saved bench state, so it can only come from the watcher.
    ''' </summary>
    Public ReadOnly Property HidePopups As Boolean

    Public Sub New(name As String, newInstance As Boolean, noSplash As Boolean,
                   openParameters As IEnumerable(Of String),
                   clipEnabled As Boolean, clipRight As Integer, clipTop As Integer,
                   dx As Integer, dy As Integer, dw As Integer, dh As Integer,
                   hidePopups As Boolean)
        Me.Name = name
        Me.NewInstance = newInstance
        Me.NoSplash = noSplash
        Me.OpenParameters = If(openParameters Is Nothing, New List(Of String)(), openParameters.ToList())
        Me.ClipEnabled = clipEnabled
        Me.ClipRight = clipRight
        Me.ClipTop = clipTop
        Me.Dx = dx
        Me.Dy = dy
        Me.Dw = dw
        Me.Dh = dh
        Me.HidePopups = hidePopups
    End Sub

    ''' <summary>The «/A "a&amp;b"» payload, or an empty string when the profile has none.</summary>
    Public Function OpenParametersText() As String
        Return String.Join("&", OpenParameters)
    End Function

    ''' <summary>
    ''' The same profile with <see cref="NewInstance"/> forced by the operator's setting. Returns
    ''' Me when the setting is <see cref="AdobeNewInstanceMode.Auto"/>, so «Auto» costs nothing and
    ''' the identity comparison in the log stays meaningful.
    ''' </summary>
    Public Function WithNewInstance(mode As AdobeNewInstanceMode) As AdobeViewerProfile
        Dim wanted As Boolean
        Select Case mode
            Case AdobeNewInstanceMode.Da : wanted = True
            Case AdobeNewInstanceMode.Nu : wanted = False
            Case Else : Return Me
        End Select
        If wanted = NewInstance Then Return Me
        Return New AdobeViewerProfile(Name, wanted, NoSplash, OpenParameters,
                                      ClipEnabled, ClipRight, ClipTop, Dx, Dy, Dw, Dh, HidePopups)
    End Function

    ''' <summary>One line for the log: everything that will be applied, in one place.</summary>
    Public Function Describe() As String
        Dim op As String = OpenParametersText()
        Return $"profil={Name} · /n={If(NewInstance, "da", "nu")} /s={If(NoSplash, "da", "nu")} · " &
               $"/A=«{If(op.Length = 0, "(niciun parametru)", op)}» · " &
               $"decupare={If(ClipEnabled, $"dreapta {ClipRight} sus {ClipTop}", "inactivă")} · " &
               $"poziție=dx {Dx} dy {Dy} dw {Dw} dh {Dh} · popup ascuns={If(HidePopups, "da", "nu")}"
    End Function

End Class

''' <summary>
''' The two measured profiles and the rule that picks between them.
''' </summary>
Public NotInheritable Class AdobeViewerProfiles

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Acrobat's modern UI (<c>bEnableAv2 = 1</c>), from the bench state of 04.08.2026 20:06.
    '''
    ''' TWO THINGS CARRIED ACROSS AS MEASURED, deliberately not "fixed":
    '''  * <c>newInstance = False</c>. In production that means Adobe may reuse an instance the
    '''    operator already had open, so the window K-BOT reparents can belong to THEIR document.
    '''    <see cref="AdobeReaderHost"/> logs loudly when the embedded window was not created by the
    '''    process it launched; the operator can force «/n» with the «Instanță nouă Adobe» setting.
    '''  * NO open parameters at all. The /A switches have no effect on this UI, and the classic
    '''    ones are deliberately NOT added here.
    ''' </summary>
    Public Shared ReadOnly Property Modern As AdobeViewerProfile
        Get
            Return _modern
        End Get
    End Property

    ''' <summary>Acrobat's classic UI (<c>bEnableAv2 = 0</c>), from the bench state of 04.08.2026 20:10.</summary>
    Public Shared ReadOnly Property Classic As AdobeViewerProfile
        Get
            Return _classic
        End Get
    End Property

    Private Shared ReadOnly _modern As New AdobeViewerProfile(
        name:="Modern", newInstance:=False, noSplash:=False,
        openParameters:=New String() {},
        clipEnabled:=True, clipRight:=230, clipTop:=152,
        dx:=-130, dy:=0, dw:=0, dh:=0,
        hidePopups:=True)

    Private Shared ReadOnly _classic As New AdobeViewerProfile(
        name:="Clasic", newInstance:=True, noSplash:=True,
        openParameters:=New String() {"toolbar=0", "navpanes=0"},
        clipEnabled:=False, clipRight:=0, clipTop:=0,
        dx:=0, dy:=0, dw:=0, dh:=0,
        hidePopups:=True)

    ''' <summary>
    ''' The profile for a detected viewer generation. An UNRECOGNISED tree falls back to
    ''' <see cref="Classic"/> — the conservative choice, because the classic profile neither clips
    ''' nor moves the window, so a wrong guess shows too much chrome rather than a blank rectangle.
    ''' </summary>
    Public Shared Function [For](generation As AdobeUiGeneration) As AdobeViewerProfile
        If generation = AdobeUiGeneration.Modern Then Return Modern
        Return Classic
    End Function

    ''' <summary>
    ''' The profile a mode + a detection produce together. A forced mode wins over the detection;
    ''' the detection still ran, and <see cref="AdobeProfileChoice.Mismatch"/> says whether the tree
    ''' disagreed — that line is what explains a broken preview after an Adobe update.
    ''' </summary>
    Public Shared Function Resolve(mode As AdobeViewerMode, detection As AdobeUiDetection) As AdobeProfileChoice
        Dim detected As AdobeUiGeneration =
            If(detection Is Nothing, AdobeUiGeneration.Unknown, detection.Generation)
        Select Case mode
            Case AdobeViewerMode.Modern
                Return New AdobeProfileChoice(Modern, mode, detected,
                                              mismatch:=detected = AdobeUiGeneration.Classic)
            Case AdobeViewerMode.Classic
                Return New AdobeProfileChoice(Classic, mode, detected,
                                              mismatch:=detected = AdobeUiGeneration.Modern)
            Case Else
                Return New AdobeProfileChoice([For](detected), mode, detected, mismatch:=False)
        End Select
    End Function

End Class

''' <summary>Which profile was chosen, from which setting, against which detection.</summary>
Public NotInheritable Class AdobeProfileChoice

    Public ReadOnly Property Profile As AdobeViewerProfile
    Public ReadOnly Property Mode As AdobeViewerMode
    Public ReadOnly Property Detected As AdobeUiGeneration
    ''' <summary>True when a FORCED mode contradicts what the window tree actually shows.</summary>
    Public ReadOnly Property Mismatch As Boolean

    Public Sub New(profile As AdobeViewerProfile, mode As AdobeViewerMode,
                   detected As AdobeUiGeneration, mismatch As Boolean)
        Me.Profile = profile
        Me.Mode = mode
        Me.Detected = detected
        Me.Mismatch = mismatch
    End Sub

    ''' <summary>The Romanian name of a mode, as shown in the ComboBox and written to the log.</summary>
    Public Shared Function ModeLabel(mode As AdobeViewerMode) As String
        Select Case mode
            Case AdobeViewerMode.Modern : Return "Modern"
            Case AdobeViewerMode.Classic : Return "Clasic"
            Case Else : Return "Automat"
        End Select
    End Function

End Class
