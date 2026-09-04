Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' THE ONE PLACE the application's base font is decided (slice 0052).
'''
''' <para><b>Why a single place matters more than the choice itself.</b> Every form is
''' <c>AutoScaleMode.Font</c>, which means WinForms reads the form's font, compares it with the
''' <c>AutoScaleDimensions</c> stamped in the designer file, and multiplies every child rectangle
''' and the form's own <c>ClientSize</c> by the ratio. So the font is not a decoration — it is one
''' half of a measurement, and the designer file is the other half. If the two halves disagree,
''' every window opens at a size nobody drew.</para>
'''
''' <para>They did disagree. The designer surface laid out against the WinForms default (Segoe UI
''' 9), while the «Modern» scheme wrote «Segoe UI Variable Text» 9 over it at runtime. Measured on
''' a 150% screen: Segoe UI 9 stamps (10, 25) and Segoe UI Variable Text 9 measures (10, 24), so
''' every window in the application was squashed vertically by 4% at launch, width unchanged, with
''' nothing in the designer showing it. That is the defect this file removes — the font is now
''' assigned BEFORE <c>InitializeComponent</c>, from the base types, so the designer surface and
''' the running window are looking at the same font.</para>
'''
''' <para><b>Calibri, 9pt.</b> The operator's choice, and already the de-facto one: 242 of the
''' fonts named across the .Designer.vb files were Calibri (grid columns, tree fonts, header
''' fonts) against 28 Segoe UI. The ambient font was the odd one out.</para>
'''
''' <para><b>The font is never disposed</b>, deliberately, and for the same reason
''' <see cref="FontBaseline"/> gives: children that inherit the ambient font share this very
''' instance with the form, so a <c>Dispose</c> at the wrong moment throws
''' <c>ObjectDisposedException</c> out of somebody's <c>OnPaint</c>. One process-lifetime object
''' is the cheaper side of that trade.</para>
''' </summary>
Public Module KBotFonts

    ''' <summary>The face the application is authored in. Not configurable — see the class summary.</summary>
    Public Const BaseFontName As String = "Calibri"

    ''' <summary>Base size in points. Per-control Bold / Italic / 10pt / 12pt stay as authored.</summary>
    Public Const BaseFontSize As Single = 9.0F

    ''' <summary>
    ''' What the operator is told when the face is missing. Shown ONCE at startup, by
    ''' <c>Program.Main</c> — not from here: a message box raised out of a field initializer would
    ''' fire before there is a message loop to own it.
    ''' </summary>
    Public Const MissingFontMessage As String =
        "Fontul Calibri nu este instalat pe acest calculator. Aplicația va folosi fontul " &
        "implicit al sistemului, iar unele ferestre pot arăta ușor diferit."

    ''' <summary>Title for <see cref="MissingFontMessage"/>.</summary>
    Public Const MissingFontCaption As String = "Font lipsă"

    Private ReadOnly _base As Font = Resolve()
    Private _isFallback As Boolean

    ''' <summary>
    ''' Calibri 9 — or the system default, if Calibri is not installed. Assigned by the base types
    ''' (<see cref="KBotThemedForm"/>, <see cref="KBotShellForm"/>,
    ''' <see cref="KBotThemedUserControl"/>) before their <c>InitializeComponent</c> runs.
    ''' </summary>
    Public ReadOnly Property Base As Font
        Get
            Return _base
        End Get
    End Property

    ''' <summary>
    ''' True when <see cref="Base"/> is NOT Calibri because the machine does not have it. Read by
    ''' <c>Program.Main</c> to decide whether to tell the operator.
    ''' </summary>
    Public ReadOnly Property IsFallback As Boolean
        Get
            Dim unused As Font = _base   ' force the field initializer, which is what sets the flag
            Return _isFallback
        End Get
    End Property

    ''' <summary>
    ''' Builds the base font, and CHECKS it. Constructing a <see cref="Font"/> with a missing face
    ''' does not throw and does not report failure — GDI substitutes silently and hands back a
    ''' perfectly valid object whose <c>Name</c> is somebody else's. So success is decided by
    ''' comparing the resolved name, never by the constructor returning.
    ''' </summary>
    Private Function Resolve() As Font
        Try
            Dim candidate As New Font(BaseFontName, BaseFontSize, FontStyle.Regular)
            If String.Equals(candidate.Name, BaseFontName, StringComparison.OrdinalIgnoreCase) Then
                _isFallback = False
                Return candidate
            End If

            ' Substituted. Keep the application running on the system font and say why, once.
            candidate.Dispose()
            _isFallback = True
            GlobalErrorLog.Write("KBotFonts.Resolve",
                New InvalidOperationException(
                    $"Font «{BaseFontName}» not installed; GDI substituted another face. " &
                    "Falling back to SystemFonts.DefaultFont. Form layout may differ from the designer."))
            Return New Font(SystemFonts.DefaultFont, FontStyle.Regular)
        Catch ex As Exception
            ' A broken font table must not take the process down before the first window exists.
            _isFallback = True
            GlobalErrorLog.Write("KBotFonts.Resolve", ex)
            Return SystemFonts.DefaultFont
        End Try
    End Function

End Module
