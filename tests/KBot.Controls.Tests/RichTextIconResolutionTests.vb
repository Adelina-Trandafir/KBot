Option Strict On
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

''' <summary>
''' WHEN the toolbar's keys are turned into pictures.
'''
''' <para>The bug these lock down: a generated <c>.Designer.vb</c> creates the
''' <c>ImageList</c> EMPTY, writes the editor's properties in alphabetical order (so
''' <c>BoldImageKey</c> lands before <c>Images</c>, and <c>Images</c> itself while the list is
''' still empty), and only afterwards loads the pictures and names the keys. Resolving once, at
''' set time, therefore left the running form showing B / I / U / A / ▨ on a page whose designer
''' had shown the icons.</para>
''' </summary>
Public Class RichTextIconResolutionTests

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

    Private Shared Function NewIcon(fill As Color) As Bitmap
        Dim bmp As New Bitmap(24, 24)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            Using b As New SolidBrush(fill)
                g.FillEllipse(b, 1, 1, 22, 22)
            End Using
        End Using
        Return bmp
    End Function

    ''' <summary>The five keys the DDF description page binds, filled the way a resx would.</summary>
    Private Shared Sub FillList(il As ImageList)
        il.ColorDepth = ColorDepth.Depth32Bit
        il.ImageSize = New Size(24, 24)
        il.Images.Add(NewIcon(Color.SteelBlue))
        il.Images.Add(NewIcon(Color.Firebrick))
        il.Images.Add(NewIcon(Color.SeaGreen))
        il.Images.Add(NewIcon(Color.Goldenrod))
        il.Images.Add(NewIcon(Color.MediumPurple))
        il.Images.SetKeyName(0, "text_backcolor")
        il.Images.SetKeyName(1, "text_forecolor")
        il.Images.SetKeyName(2, "bold")
        il.Images.SetKeyName(3, "italic")
        il.Images.SetKeyName(4, "underline")
    End Sub

    Private Shared Function CommandButtons(ed As KBotRichTextEditor) As KBotNoFocusButton()
        Return New KBotNoFocusButton() {ed.btnBold, ed.btnItalic, ed.btnUnderline,
                                        ed.btnTextColor, ed.btnHighlight}
    End Function

    <Fact>
    Public Sub The_designer_order_still_ends_up_with_pictures()
        RunSta(Sub()
                   Using host As New Form() With {.ClientSize = New Size(900, 300)}
                       Dim ed As New KBotRichTextEditor() With {.Dock = DockStyle.Fill}
                       Dim il As New ImageList()

                       ' EXACTLY what InitializeComponent emits: the keys, then the still-empty
                       ' list, and only afterwards the pictures behind those keys.
                       ed.BoldImageKey = "bold"
                       ed.HighlightImageKey = "text_backcolor"
                       ed.Images = il
                       ed.ItalicImageKey = "italic"
                       ed.TextColorImageKey = "text_forecolor"
                       ed.UnderlineImageKey = "underline"
                       host.Controls.Add(ed)
                       FillList(il)

                       For Each b As KBotNoFocusButton In CommandButtons(ed)
                           Assert.Null(b.Image)      ' nothing to resolve yet -- this is the bug's moment
                       Next

                       ' Asking for the handle is what a Show -- or a DrawToBitmap -- does first.
                       Dim ignored As IntPtr = ed.Handle

                       For Each b As KBotNoFocusButton In CommandButtons(ed)
                           Assert.NotNull(b.Image)
                           Assert.Equal(String.Empty, b.Text)   ' the letter is gone
                       Next
                       il.Dispose()
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Filling_the_list_wholesale_reaches_the_buttons()
        RunSta(Sub()
                   Dim first As New ImageList()
                   FillList(first)
                   Using ed As New KBotRichTextEditor()
                       ed.Images = first
                       ed.BoldImageKey = "bold"
                       Assert.NotNull(ed.btnBold.Image)

                       ' A list whose contents are replaced raises RecreateHandle; the editor
                       ' follows it instead of holding a picture from the previous set.
                       Dim before As Image = ed.btnBold.Image
                       first.ImageSize = New Size(32, 32)
                       Assert.NotSame(before, ed.btnBold.Image)
                   End Using
                   first.Dispose()
               End Sub)
    End Sub

    <Fact>
    Public Sub A_disposed_list_puts_the_letters_back()
        RunSta(Sub()
                   Dim il As New ImageList()
                   FillList(il)
                   Using ed As New KBotRichTextEditor()
                       ed.Images = il
                       ed.BoldImageKey = "bold"
                       Assert.NotNull(ed.btnBold.Image)

                       il.Dispose()
                       ' The source is gone: the button reads as a toolbar again, not as a
                       ' blank square holding a picture nobody owns.
                       Assert.Null(ed.Images)
                       Assert.Null(ed.btnBold.Image)
                       Assert.Equal("B", ed.btnBold.Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub A_picture_the_host_owns_is_never_freed_by_the_button()
        RunSta(Sub()
                   Using mine As Bitmap = NewIcon(Color.Teal)
                       Using ed As New KBotRichTextEditor()
                           ed.BoldImage = mine
                           Assert.Same(mine, ed.btnBold.Image)
                           ' Re-resolving (here: a layout change) must not throw the host's own
                           ' object away -- only copies handed out by an ImageList are ours.
                           ed.ButtonImageLayout = RichTextImageLayout.Zoom
                           ed.ButtonImageLayout = RichTextImageLayout.Original
                       End Using
                       Assert.Equal(24, mine.Width)     ' still alive after the editor died
                   End Using
               End Sub)
    End Sub
End Class
