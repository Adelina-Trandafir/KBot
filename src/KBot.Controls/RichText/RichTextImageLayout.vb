Option Strict On

''' <summary>
''' How a toolbar button of <see cref="KBotRichTextEditor"/> paints the picture bound to it.
'''
''' <para><b>Why the editor needs one at all.</b> A <c>Button</c> draws its <c>Image</c> at the
''' picture's own pixel size and nothing else: a 16x16 icon leaves a ring of background inside a
''' 30x30 button, and a 64x64 one is cropped by its edges. The operator picks the icon set, so
''' the operator also has to be able to say how it meets the button.</para>
'''
''' <para>The names follow <c>System.Windows.Forms.ImageLayout</c> where they mean the same
''' thing, so nobody learns a second vocabulary -- but the default here is
''' <see cref="Original"/> (centred, untouched), not the framework's top-left corner, because
''' that is what the toolbar has always looked like.</para>
''' </summary>
Public Enum RichTextImageLayout
    ''' <summary>The picture at its own size, placed by the button's <c>ImageAlign</c>
    ''' (the toolbar sets that to centre). Drawn by the framework, exactly as before.</summary>
    Original = 0
    ''' <summary>Pulled to fill the whole button, aspect ratio ignored.</summary>
    Stretch = 1
    ''' <summary>Grown or shrunk to the largest size that still fits, aspect ratio kept.</summary>
    Zoom = 2
    ''' <summary>Repeated from the top-left corner until the button is covered.</summary>
    Tile = 3
End Enum
