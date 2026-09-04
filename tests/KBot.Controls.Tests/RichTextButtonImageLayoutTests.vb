Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' <c>KBotRichTextEditor.ButtonImageLayout</c> — ONE property that says how the pictures meet
''' the six buttons of the header band.
'''
''' The rule worth pinning: <c>Original</c> leaves the drawing to the framework (the toolbar's
''' old look, to the pixel) and every other layout moves it into
''' <see cref="KBotNoFocusButton"/>, which is what lets a 64x64 icon fit a 30x30 button instead
''' of being cropped by it. Both paths read the SAME picture, so nothing is lost by switching.
''' </summary>
Public Class RichTextButtonImageLayoutTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    ' 64 px into a 30 px button: the size that makes the four layouts tell each other apart.
    ' Not disposed here on purpose — the ImageList holds the original bitmap until its native
    ' handle exists, and the test owns the list.
    Private Shared Function NewImageList() As ImageList
        Dim il As New ImageList() With {.ImageSize = New Size(64, 64), .ColorDepth = ColorDepth.Depth32Bit}
        Dim bmp As New Bitmap(64, 64)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            Using b As New SolidBrush(Color.SteelBlue)
                g.FillEllipse(b, 0, 0, 63, 63)
            End Using
            Using p As New Pen(Color.Firebrick, 6)
                g.DrawLine(p, 0, 0, 63, 63)
            End Using
        End Using
        il.Images.Add("pictograma", bmp)
        Return il
    End Function

    Private Shared Function NewEditor(list As ImageList, layout As RichTextImageLayout) As KBotRichTextEditor
        Dim ed As New KBotRichTextEditor() With {.Size = New Size(700, 120), .FooterVisible = False}
        ed.Images = list
        ed.BoldImageKey = "pictograma"
        ed.ItalicImageKey = "pictograma"
        ed.UnderlineImageKey = "pictograma"
        ed.TextColorImageKey = "pictograma"
        ed.HighlightImageKey = "pictograma"
        ed.CollapseExpandedImageKey = "pictograma"
        ed.ButtonImageLayout = layout
        Return ed
    End Function

    ''' <summary>The band as the operator sees it, painted into a bitmap.</summary>
    Private Shared Function Render(ed As KBotRichTextEditor) As Bitmap
        ed.PerformLayout()
        Dim bmp As New Bitmap(ed.Width, ed.Height)
        ed.DrawToBitmap(bmp, New Rectangle(0, 0, ed.Width, ed.Height))
        Return bmp
    End Function

    Private Shared Function SamePixels(a As Bitmap, b As Bitmap) As Boolean
        If a.Width <> b.Width OrElse a.Height <> b.Height Then Return False
        For y As Integer = 0 To a.Height - 1
            For x As Integer = 0 To a.Width - 1
                If a.GetPixel(x, y) <> b.GetPixel(x, y) Then Return False
            Next
        Next
        Return True
    End Function

    <Fact>
    Public Sub The_default_is_the_untouched_picture_drawn_by_the_framework()
        RunSta(Sub()
                   Dim list As ImageList = NewImageList()
                   Using ed As KBotRichTextEditor = NewEditor(list, RichTextImageLayout.Original)
                       Assert.Equal(RichTextImageLayout.Original, ed.ButtonImageLayout)
                       ' Seen as a plain Button, the picture is the BASE one — so the framework
                       ' is the one putting it down, exactly as before this property existed.
                       Dim asButton As Button = ed.btnBold
                       Assert.NotNull(asButton.Image)
                       Assert.Same(ed.btnBold.Image, asButton.Image)
                   End Using
                   list.Dispose()
               End Sub)
    End Sub

    <Fact>
    Public Sub A_scaled_layout_keeps_the_picture_but_takes_it_off_the_framework()
        RunSta(Sub()
                   Dim list As ImageList = NewImageList()
                   Using ed As KBotRichTextEditor = NewEditor(list, RichTextImageLayout.Zoom)
                       ' The button still HAS the picture...
                       Assert.NotNull(ed.btnBold.Image)
                       ' ...but the base class has nothing to draw, because the button draws it.
                       Dim asButton As Button = ed.btnBold
                       Assert.Null(asButton.Image)
                   End Using
                   list.Dispose()
               End Sub)
    End Sub

    <Fact>
    Public Sub One_property_reaches_every_button_of_the_band()
        RunSta(Sub()
                   Dim list As ImageList = NewImageList()
                   Using ed As KBotRichTextEditor = NewEditor(list, RichTextImageLayout.Original)
                       ed.CollapseButton = True
                       ed.ButtonImageLayout = RichTextImageLayout.Tile
                       For Each btn As KBotNoFocusButton In New KBotNoFocusButton() {
                           ed.btnBold, ed.btnItalic, ed.btnUnderline,
                           ed.btnTextColor, ed.btnHighlight, ed.btnCollapse}
                           Assert.Equal(RichTextImageLayout.Tile, btn.ImageLayout)
                       Next
                   End Using
                   list.Dispose()
               End Sub)
    End Sub

    <Fact>
    Public Sub Each_layout_paints_a_different_band()
        RunSta(Sub()
                   Dim list As ImageList = NewImageList()
                   Dim shots As New Dictionary(Of RichTextImageLayout, Bitmap)()
                   Try
                       For Each layout As RichTextImageLayout In [Enum].GetValues(GetType(RichTextImageLayout))
                           Using ed As KBotRichTextEditor = NewEditor(list, layout)
                               shots.Add(layout, Render(ed))
                           End Using
                       Next
                       ' Four layouts, four different toolbars: a property that changed nothing
                       ' on screen would pass every other test in this file.
                       For Each first As RichTextImageLayout In shots.Keys
                           For Each second As RichTextImageLayout In shots.Keys
                               If first = second Then Continue For
                               Assert.False(SamePixels(shots(first), shots(second)),
                                            $"{first} and {second} painted the same band.")
                           Next
                       Next
                   Finally
                       For Each bmp As Bitmap In shots.Values
                           bmp.Dispose()
                       Next
                   End Try
                   list.Dispose()
               End Sub)
    End Sub

    <Fact>
    Public Sub Switching_back_to_original_restores_the_framework_drawing()
        RunSta(Sub()
                   Dim list As ImageList = NewImageList()
                   Dim before As Bitmap = Nothing
                   Dim after As Bitmap = Nothing
                   Try
                       Using ed As KBotRichTextEditor = NewEditor(list, RichTextImageLayout.Original)
                           before = Render(ed)
                           ed.ButtonImageLayout = RichTextImageLayout.Stretch
                           ed.ButtonImageLayout = RichTextImageLayout.Original
                           after = Render(ed)
                       End Using
                       Assert.True(SamePixels(before, after), "The trip through Stretch changed the untouched look.")
                   Finally
                       If before IsNot Nothing Then before.Dispose()
                       If after IsNot Nothing Then after.Dispose()
                   End Try
                   list.Dispose()
               End Sub)
    End Sub

    <Fact>
    Public Sub An_unknown_layout_is_refused()
        RunSta(Sub()
                   Using ed As New KBotRichTextEditor()
                       Assert.Throws(Of ArgumentException)(Sub() ed.ButtonImageLayout = CType(42, RichTextImageLayout))
                       Assert.Equal(RichTextImageLayout.Original, ed.ButtonImageLayout)
                   End Using
                   Using btn As New KBotNoFocusButton()
                       Assert.Throws(Of ArgumentException)(Sub() btn.ImageLayout = CType(42, RichTextImageLayout))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub A_freshly_dropped_editor_writes_no_layout_line()
        RunSta(Sub()
                   Using ed As New KBotRichTextEditor()
                       Dim p As PropertyDescriptor = TypeDescriptor.GetProperties(ed)("ButtonImageLayout")
                       Assert.NotNull(p)
                       Assert.False(p.ShouldSerializeValue(ed))
                       ed.ButtonImageLayout = RichTextImageLayout.Zoom
                       Assert.True(p.ShouldSerializeValue(ed))
                   End Using
               End Sub)
    End Sub
End Class
