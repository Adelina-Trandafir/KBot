Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' The header (and footer) strip of <see cref="KBotRichTextEditor"/>: a flat panel that paints
''' its own background plus ONE edge separator.
'''
''' <para><b>Why a class and not a plain Panel.</b> The band has to behave like the grid's header
''' band -- a solid strip closed by a baseline of its own colour and width
''' (<c>KBotDataView.HeaderSeparatorColor</c> / <c>HeaderSeparatorWidth</c>) -- and a
''' <c>Panel</c> gives neither the line nor a flicker-free repaint while the operator types into
''' the editor below it. Both are two lines of paint code, so they live here once instead of in
''' two Paint handlers on the host.</para>
'''
''' <para><b>Not an <c>IThemedControl</c>, and not in the Toolbox.</b> It holds no palette of its
''' own: <see cref="KBotRichTextEditor.ApplyTheme"/> writes its colours, exactly as it writes the
''' toolbar buttons'. Outside that editor it has no job.</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
Public Class KBotRichTextBand
    Inherits Panel

    Private _separatorColor As Color = Color.Empty
    Private _separatorWidth As Integer = 1
    Private _separatorAtTop As Boolean = False

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw, True)
        Margin = New Padding(0)
    End Sub

    ''' <summary>The baseline colour. <c>Empty</c> = no line at all.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SeparatorColor As Color
        Get
            Return _separatorColor
        End Get
        Set(value As Color)
            If _separatorColor = value Then Return
            _separatorColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>The baseline thickness in DEVICE pixels -- the host scales before it assigns.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SeparatorWidth As Integer
        Get
            Return _separatorWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If _separatorWidth = clamped Then Return
            _separatorWidth = clamped
            Invalidate()
        End Set
    End Property

    ''' <summary>True = the line closes the TOP edge (the footer), False = the bottom (the header).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SeparatorAtTop As Boolean
        Get
            Return _separatorAtTop
        End Get
        Set(value As Boolean)
            If _separatorAtTop = value Then Return
            _separatorAtTop = value
            Invalidate()
        End Set
    End Property

    ''' <summary>UI boundary: a broken colour must not take the whole form down (C7).</summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Try
            If _separatorWidth <= 0 OrElse _separatorColor.IsEmpty Then Return
            Dim y As Integer = If(_separatorAtTop, 0, Height - _separatorWidth)
            Using b As New SolidBrush(_separatorColor)
                e.Graphics.FillRectangle(b, 0, y, Width, _separatorWidth)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextBand.OnPaint", ex)
        End Try
    End Sub
End Class
