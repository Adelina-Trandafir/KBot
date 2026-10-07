Option Strict On
Imports System.ComponentModel
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-09 -- the shared body of the three document views of the invoice window («Factură PDF», «Factură ANAF», «Eroare ANAF»):
''' one page, one embedded PDF viewer. Each view is its own page (its own control, its own viewer, its own designer file); this class
''' only holds what the three have in common. The viewer itself is the embedded Adobe surface of the DDF «Document» page, which lives
''' in KBot.App (this project cannot reference it), so the shell hands the window a factory and the window hands it on to the pages.
'''
''' <para>The page does not decide WHAT it shows: the window prepares the file (or the line of text) and calls
''' <see cref="ShowDocument"/> / <see cref="ShowNotice"/>. The viewer is made the first time it is needed, so Adobe does not start
''' with the window.</para>
''' </summary>
Public Class VanzarePdfPage
    Implements IThemedContainer

    Private _viewer As IFacturaPdfViewer

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Makes the viewer (set by the window before the page is first shown); Nothing = the page says there is none.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ViewerFactory As Func(Of IFacturaPdfViewer)

    ''' <summary>What the viewer shows now ("invoice|state|total|date|lines"): the same again is not opened twice.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Stamp As String

    ''' <summary>Shows the PDF at <paramref name="k_path"/> (an existing file).</summary>
    Public Sub ShowDocument(k_path As String)
        If EnsureViewer() Then _viewer.ShowDocument(k_path)
    End Sub

    ''' <summary>A plain line instead of a document («Se descarcă…», or why there is none).</summary>
    Public Sub ShowNotice(k_message As String)
        Stamp = Nothing
        If EnsureViewer() Then _viewer.ShowNotice(k_message)
    End Sub

    ''' <summary>Lets go of the document on screen.</summary>
    Public Sub ReleaseDocument()
        Stamp = Nothing
        _viewer?.Clear()
    End Sub

    ' The embedded viewer, made the first time it is needed. False = there is none (the page says so).
    Private Function EnsureViewer() As Boolean
        If _viewer IsNot Nothing Then Return True
        If ViewerFactory Is Nothing Then
            lblNota.Visible = True
            Return False
        End If
        _viewer = ViewerFactory.Invoke()
        Dim k_surface As Control = _viewer.Surface
        k_surface.Dock = DockStyle.Fill
        Controls.Add(k_surface)
        ThemeManager.Apply(k_surface)
        lblNota.Visible = False
        Return True
    End Function

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            lblNota.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("VanzarePdfPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
