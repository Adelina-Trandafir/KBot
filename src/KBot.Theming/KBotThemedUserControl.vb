Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Base type for the application's user controls, and it exists for exactly one reason: a
''' <see cref="UserControl"/> is a <c>ContainerControl</c>, so it carries its own
''' <c>AutoScaleDimensions</c> stamp and runs its own <c>PerformAutoScale</c> — independently of
''' the form hosting it.
'''
''' <para><b>What goes wrong without it.</b> Visual Studio renders a user control against the
''' WinForms default font and stamps the pair it measured there. At runtime the control is
''' constructed, measures itself against that same default, and only THEN gets added to a form
''' whose font is Calibri — at which point the ambient font changes underneath it and it rescales
''' a second time. Two scalings, each with its own rounding, on a control that was drawn once.
''' Assigning the base font in the constructor makes the first measurement already correct, so
''' the ratio is 1 and the second scaling has nothing left to do.</para>
'''
''' <para><b>Why it lives in KBot.Theming</b> and not with the controls: it is a base type in the
''' same sense <see cref="KBotThemedForm"/> and <see cref="KBotShellForm"/> are, and splitting
''' the three across two assemblies would hide the fact that they say the same thing. The house
''' rule in CLAUDE.md was widened to «base forms and base user controls» rather than bent.</para>
'''
''' <para><b>It deliberately adds no theming.</b> Views that need the theme implement
''' <c>IThemedControl</c> and are handed the scheme by <c>ThemeManager.Traverse</c>, exactly as
''' before; this type does not join that contract and does not change how any control is painted.
''' It sets a font, and nothing else.</para>
''' </summary>
Public Class KBotThemedUserControl
    Inherits UserControl

    ''' <summary>See the class summary: before <c>InitializeComponent</c>, or it is pointless.</summary>
    Public Sub New()
        Font = KBotFonts.Base
    End Sub

End Class
