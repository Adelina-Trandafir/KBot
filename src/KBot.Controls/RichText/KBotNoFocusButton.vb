Option Strict On
Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

''' <summary>
''' A toolbar button that NEVER takes the focus.
'''
''' <para>That is the whole point of it, and it is not cosmetic. A formatting toolbar acts on
''' the SELECTION in the editor next to it; an ordinary button takes the focus when clicked,
''' the editor loses its selection highlight, and "make this bold" ends up applying to a
''' caret rather than to the words the operator had picked. Setting
''' <c>ControlStyles.Selectable</c> to <c>False</c> leaves the selection where it was.</para>
'''
''' <para>Ported from <c>CustomControls.NoFocusButton</c> in the <c>VBA_DDF_INFO</c> project,
''' minus its hardcoded colours -- those come from the theme now
''' (<see cref="KBotRichTextEditor"/> paints its own toolbar).</para>
'''
''' <para><b>It also draws its own picture</b> whenever <see cref="ImageLayout"/> asks for
''' anything other than <see cref="RichTextImageLayout.Original"/>. A <c>Button</c> can only
''' put an image down at its own pixel size, so stretching, fitting and tiling are ours; the
''' untouched case stays the framework's, which keeps the toolbar's old look bit for bit.</para>
'''
''' <para>Not an <c>IThemedControl</c>: it owns no child controls and holds no colours of its
''' own. Its host sets them, which is what keeps a "pressed" button distinguishable from an
''' unpressed one under every scheme.</para>
''' </summary>
<ToolboxItem(True)>
Public Class KBotNoFocusButton
    Inherits Button

    ' The picture lives HERE and not in MyBase.Image, because the base class draws whatever it
    ' holds and there is no hook that skips just that step. MyBase.Image is handed the picture
    ' only in the Original layout -- then, and only then, the framework does the drawing.
    Private _picture As Image = Nothing
    Private _ownsPicture As Boolean = False
    Private _imageLayout As RichTextImageLayout = RichTextImageLayout.Original

    Public Sub New()
        SetStyle(ControlStyles.Selectable, False)
        FlatStyle = FlatStyle.Flat
        FlatAppearance.BorderSize = 1
        TabStop = False
    End Sub

    ''' <summary>
    ''' The picture on the button. Shadowed so the two layouts can share one storage: the host
    ''' writes it exactly as it would write <c>Button.Image</c>.
    '''
    ''' <para>Out of the property grid (house rule for a shadowed member, and the editor
    ''' publishes <c>BoldImage</c>, <c>BoldImageKey</c> and their siblings for that).</para>
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property Image As Image
        Get
            Return _picture
        End Get
        Set(value As Image)
            SetPicture(value, owned:=False)
        End Set
    End Property

    ''' <summary>
    ''' How <see cref="Image"/> meets the button's rectangle. Written by the host from its own
    ''' single <c>ButtonImageLayout</c>, so the whole band answers to one property.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ImageLayout As RichTextImageLayout
        Get
            Return _imageLayout
        End Get
        Set(value As RichTextImageLayout)
            If Not [Enum].IsDefined(GetType(RichTextImageLayout), value) Then
                Throw New ArgumentException("Mod de desenare necunoscut pentru pictograma butonului.", NameOf(value))
            End If
            If _imageLayout = value Then Return
            _imageLayout = value
            SyncBaseImage()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' The picture together with WHO OWNS IT -- the host's own object, or a copy handed out by
    ''' an <c>ImageList</c>.
    '''
    ''' <para>The distinction is not pedantry: <c>ImageList.Images(i)</c> returns a NEW bitmap on
    ''' every read, so the one put down on the previous pass belongs to us and nobody else will
    ''' ever free it. Since the editor re-resolves its keys whenever the list changes or the
    ''' control is created, dropping that copy on the floor would leak a bitmap per button per
    ''' pass.</para>
    ''' </summary>
    Friend Sub SetPicture(picture As Image, owned As Boolean)
        If ReferenceEquals(_picture, picture) Then
            _ownsPicture = owned
            Return
        End If
        Dim previous As Image = _picture
        Dim previousOwned As Boolean = _ownsPicture
        _picture = picture
        _ownsPicture = owned
        SyncBaseImage()
        Invalidate()
        ' Freed LAST: the base class must have let go of it first (SyncBaseImage), or the next
        ' paint would draw a disposed bitmap.
        If previousOwned AndAlso previous IsNot Nothing Then previous.Dispose()
    End Sub

    ''' <summary>The framework gets the picture only when it is the one drawing it.</summary>
    Private Sub SyncBaseImage()
        MyBase.Image = If(_imageLayout = RichTextImageLayout.Original, _picture, Nothing)
    End Sub

    ''' <summary>Fixed by the constructor -> not the operator's choice, not serialised.</summary>
    Public Shadows Function ShouldSerializeFlatStyle() As Boolean
        Return False
    End Function

    ''' <summary>Fixed by the constructor -> not serialised.</summary>
    Public Shadows Function ShouldSerializeTabStop() As Boolean
        Return False
    End Function

    ''' <summary>
    ''' The background, the flat border and the text are still the base class's; only the
    ''' scaled or repeated picture is ours, and it goes on last so it sits over the fill.
    '''
    ''' <para>UI boundary: a broken picture must not take the whole form down (C7).</para>
    ''' </summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Try
            If _picture Is Nothing OrElse _imageLayout = RichTextImageLayout.Original Then Return

            Dim box As Rectangle = ContentBox()
            If box.Width <= 0 OrElse box.Height <= 0 Then Return

            Select Case _imageLayout
                Case RichTextImageLayout.Stretch
                    DrawPicture(e.Graphics, box)
                Case RichTextImageLayout.Zoom
                    DrawPicture(e.Graphics, ZoomBox(box))
                Case RichTextImageLayout.Tile
                    TilePicture(e.Graphics, box)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNoFocusButton.OnPaint", ex)
        End Try
    End Sub

    ''' <summary>The rectangle left after the flat border and the button's own padding.</summary>
    Private Function ContentBox() As Rectangle
        Dim border As Integer = Math.Max(0, FlatAppearance.BorderSize)
        Dim r As Rectangle = ClientRectangle
        r.Inflate(-border, -border)
        Return New Rectangle(r.Left + Padding.Left,
                             r.Top + Padding.Top,
                             r.Width - Padding.Horizontal,
                             r.Height - Padding.Vertical)
    End Function

    ''' <summary>The largest rectangle with the picture's proportions that fits, centred.</summary>
    Private Function ZoomBox(box As Rectangle) As Rectangle
        If _picture.Width <= 0 OrElse _picture.Height <= 0 Then Return box
        Dim scale As Double = Math.Min(box.Width / CDbl(_picture.Width), box.Height / CDbl(_picture.Height))
        Dim w As Integer = Math.Max(1, CInt(Math.Round(_picture.Width * scale)))
        Dim h As Integer = Math.Max(1, CInt(Math.Round(_picture.Height * scale)))
        Return New Rectangle(box.Left + (box.Width - w) \ 2, box.Top + (box.Height - h) \ 2, w, h)
    End Function

    ''' <summary>
    ''' The picture into <paramref name="dest"/>, greyed while the button is disabled -- the
    ''' framework does that for us in the Original layout and the toolbar goes disabled the
    ''' moment the description is read-only, so a full-colour icon there would read as live.
    ''' </summary>
    Private Sub DrawPicture(g As Graphics, dest As Rectangle)
        g.InterpolationMode = InterpolationMode.NearestNeighbor
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        If Enabled Then
            g.DrawImage(_picture, dest)
            Return
        End If
        Using attrs As ImageAttributes = DisabledAttributes()
            g.DrawImage(_picture, dest, 0, 0, _picture.Width, _picture.Height, GraphicsUnit.Pixel, attrs)
        End Using
    End Sub

    ''' <summary>The picture repeated from the top-left corner of <paramref name="box"/>.</summary>
    Private Sub TilePicture(g As Graphics, box As Rectangle)
        Dim source As New Rectangle(0, 0, _picture.Width, _picture.Height)
        If source.Width <= 0 OrElse source.Height <= 0 Then Return

        If Enabled Then
            Using brush As New TextureBrush(_picture, WrapMode.Tile)
                ' Without the shift the pattern starts at the CONTROL's origin, so the first
                ' tile is cut by the border instead of sitting against it.
                brush.TranslateTransform(box.Left, box.Top)
                g.FillRectangle(brush, box)
            End Using
            Return
        End If

        Using attrs As ImageAttributes = DisabledAttributes()
            Using brush As New TextureBrush(_picture, source, attrs)
                brush.TranslateTransform(box.Left, box.Top)
                g.FillRectangle(brush, box)
            End Using
        End Using
    End Sub

    ''' <summary>Grey and half-transparent: the disabled look, for any destination size.</summary>
    Private Shared Function DisabledAttributes() As ImageAttributes
        Dim m As New ColorMatrix(New Single()() {
            New Single() {0.299F, 0.299F, 0.299F, 0F, 0F},
            New Single() {0.587F, 0.587F, 0.587F, 0F, 0F},
            New Single() {0.114F, 0.114F, 0.114F, 0F, 0F},
            New Single() {0F, 0F, 0F, 0.55F, 0F},
            New Single() {0F, 0F, 0F, 0F, 1F}})
        Dim attrs As New ImageAttributes()
        attrs.SetColorMatrix(m)
        Return attrs
    End Function

    ''' <summary>Going grey (or coming back) changes what we paint, not what the base holds.</summary>
    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        If _picture IsNot Nothing AndAlso _imageLayout <> RichTextImageLayout.Original Then Invalidate()
    End Sub

    ''' <summary>A copy this button owns dies with it -- see <see cref="SetPicture"/>.</summary>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso _ownsPicture AndAlso _picture IsNot Nothing Then
                MyBase.Image = Nothing
                _picture.Dispose()
                _picture = Nothing
                _ownsPicture = False
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
End Class
