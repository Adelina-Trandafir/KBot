Option Strict On

''' <summary>
''' THE base of every modal window of the application (the DDF / ORD editors, the reception links, the CAB notes, the
''' reservations chart, the groups, the message catalog): a borderless, resizable window with its own caption bar that
''' <c>AvacontMain.ShowModalAsync</c> opens inside the area of the pages. One type, so one behaviour: the same opening
''' size, the same resize band, and the same place when maximised (under the ribbon, beside the side panel).
''' A window the operator must answer before going on inherits this, never <see cref="KBotShellForm"/> or
''' <see cref="KBotThemedForm"/> directly; <c>ShowModalAsync</c> accepts nothing else.
''' </summary>
Public Class KBotModalForm
    Inherits KBotShellForm

End Class
