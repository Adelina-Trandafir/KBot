Option Strict On

''' <summary>
''' Slice 000T-10: a window that can say whether what a tutorial requires (its <c>requires:</c> header key) is
''' true now. The main window answers <c>forexe</c> (a live FOREXE session, or a Debug build). A tutorial whose
''' requirement is not met is neither listed in the «?» popup nor found by a typed question, and cannot be
''' started (not even from a link of another tutorial): it says what is missing instead.
''' </summary>
Public Interface ITutorialRequirements

    ''' <summary>True when <paramref name="k_name"/> holds. An unknown name is False (and logged by the implementer).</summary>
    Function TutorialRequirementMet(k_name As String) As Boolean

    ''' <summary>The sentence the operator reads when <paramref name="k_name"/> does not hold.</summary>
    Function TutorialRequirementText(k_name As String) As String

End Interface
