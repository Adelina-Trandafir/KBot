Option Strict On
Imports KBot.Common

''' <summary>
''' The director's K-BOT (operator, 30.10.2026). A login whose role is «Director» opens the SAME main window as everybody
''' else, narrowed to the FOREXE part of the application and, inside it, to the three documents the director reads and
''' signs: DDF, ORD and the CAB correction notes. Nothing else is shown, and nothing in this mode talks to FOREXE (the robot
''' is the accountant's job). More documents are added here, in one place, when they are asked for.
'''
''' <para>This class only HOLDS the rules; the window that applies them is the one that owns the thing to narrow: the shell
''' (<c>KbotForm.Views</c>: the left navigation and the write commands), the main window (<c>AvacontMain</c>: the ribbon) and
''' the FOREXE coordinator (<c>ForexeController</c>: no connection).</para>
''' </summary>
Friend NotInheritable Class DirectorMode

    Private Sub New()
    End Sub

    ''' <summary>The views of the shell's left navigation a director sees, in the order they are offered.</summary>
    Friend Shared ReadOnly ViewKeys As String() = {"ddf", "ord", "notecab"}

    ''' <summary>
    ''' The ribbon buttons a director keeps. A tab with none of them goes out of sight with everything else; the pinned
    ''' «Inchide» stays so a page can be closed.
    ''' </summary>
    Friend Shared ReadOnly RibbonKeys As String() = {"pagina_angajamente", RibbonKeyToSign, "pagina_inchide", "setari", "istoric_versiuni"}

    ''' <summary>The director-only ribbon button that opens the list of documents waiting for the signature (slice 0081-06).</summary>
    Friend Const RibbonKeyToSign As String = "director_de_semnat"

    ''' <summary>The only signature role a director may add to a document (a signature in any other field is not uploaded).</summary>
    Friend Shared ReadOnly SigningRoles As String() = {"Ordonator"}

    ''' <summary>Is the logged-in role a director's? Read live: a re-login can change the role of the session.</summary>
    Friend Shared Function IsActive(k_session As SessionContext) As Boolean
        Return DirectorForm.EsteDirector(k_session)
    End Function

    ''' <summary>
    ''' What a director is told when something would drive FOREXE or change a document. The signing itself is not a command of
    ''' the shell: it runs inside the document view, so this message does not stand in its way.
    ''' </summary>
    Friend Const ReadOnlyMessage As String = "Cu rolul «Director» documentele se citesc și se semnează. Modificările și lucrul cu FOREXE se fac din K-BOT-ul contabilului."

End Class
