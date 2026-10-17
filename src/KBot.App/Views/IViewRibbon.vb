Option Strict On

''' <summary>How a view's button looks in the ribbon's view group (slice 0114-15).</summary>
Public Enum ViewButtonState
    ''' <summary>Not shown: there is nothing for the operator to do with it now.</summary>
    Hidden = 0
    ''' <summary>Shown and dimmed: it exists for this view, but not for what is on screen now.</summary>
    Dimmed = 1
    ''' <summary>Shown and usable.</summary>
    Enabled = 2
End Enum

''' <summary>
''' A view of the angajament shell that has buttons in the ribbon's VIEW group (the group at the right, left of the form group;
''' slice 0114-15). The buttons themselves are made in the designer of <c>AvacontMain</c> with their keys; the view only says which
''' of them it wants now and what each one does. The window shows or hides the buttons while the view is on screen and sets the
''' group's caption. A view that has no ribbon buttons does not implement this.
''' </summary>
Public Interface IViewRibbon

    ''' <summary>The caption of the view group while this view is on screen («Rezervări», «Recepții»).</summary>
    ReadOnly Property RibbonCaption As String

    ''' <summary>The keys of the view group's buttons that belong to this view (every other button is hidden while it is on screen).</summary>
    ReadOnly Property RibbonKeys As IReadOnlyList(Of String)

    ''' <summary>How the button with this key looks now. Called for every key in <see cref="RibbonKeys"/>.</summary>
    Function RibbonButtonState(k_key As String) As ViewButtonState

    ''' <summary>The text of the button when it is not the one written in the designer (it follows the state of the view); empty = keep it.</summary>
    Function RibbonButtonText(k_key As String) As String

    ''' <summary>The operator pressed the button with this key. The view logs and tells the operator what it must; nothing is thrown.</summary>
    Sub RunRibbonButton(k_key As String)

    ''' <summary>Raised when what <see cref="RibbonButtonState"/> or <see cref="RibbonButtonText"/> answer has changed.</summary>
    Event RibbonStateChanged As EventHandler

End Interface
