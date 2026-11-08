Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 0086-02 -- what a flow that left <see cref="KbotForm"/> still asks of the main window.
''' Only BEHAVIOURS the form owns (the re-login net, the busy bar, the tree reload, the modal
''' host); the objects a flow works with (controller, API client, queue, state) are handed to its
''' constructor, not reached through here. Keep it that small: a member that is not needed by two
''' flows does not belong here.
''' </summary>
Friend Interface IShellHost

    ''' <summary>Runs an authenticated call behind the re-login net (see <c>KbotForm.WithReauth</c>).</summary>
    Function WithReauth(Of T)(k_action As Func(Of Task(Of T))) As Task(Of T)

    ''' <summary>Starts or stops the busy bar of the main window.</summary>
    Sub SetBusy(k_running As Boolean)

    ''' <summary>Reloads the angajamente tree; <paramref name="k_cod"/> selects and reveals that code.</summary>
    Function LoadTreeAsync(k_keepSelection As Boolean, k_cod As String) As Task

    ''' <summary>Shows a form as a modal window of the main window; True when it was shown.</summary>
    Function ShowModalAsync(k_form As KBotModalForm, k_ref As MachetaRef) As Task(Of Boolean)

    ''' <summary>The reference of a window of one angajament, in the year in force.</summary>
    Function CodRef(k_typeKey As String, k_cod As String) As MachetaRef

    ''' <summary>The angajament's name as the tree has it, or empty.</summary>
    Function AngajamentName(k_cod As String) As String

    ''' <summary>Marks a download as «no receptii question» when another FOREXE action is under way.</summary>
    Sub MarkDownloadWithoutQuestion(k_cod As String)

    ''' <summary>The association guard: True when the angajament's links allow the operation.</summary>
    Function AssociationAllowsAsync(k_cod As String, k_operation As String) As Task(Of Boolean)

End Interface
