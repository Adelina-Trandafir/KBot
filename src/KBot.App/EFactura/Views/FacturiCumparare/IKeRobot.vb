Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slices KE05..KE07 -- the FOREXE robot, seen from the e-invoice views. The robot belongs to the shell (<c>KbotForm</c>) and the views know only
''' this contract; the shell hands the robot over through <see cref="KeRobot.Current"/> when it starts. Nothing there = the views say the robot is not available.
''' </summary>
Public Interface IKeRobot

    ''' <summary>
    ''' Writes the reception on the site (adds a new one or changes an existing one), then reads the angajament back and takes the reception into K-BOT.
    ''' Never throws for a refusal: the answer says what happened and why.
    ''' </summary>
    Function SalveazaReceptieAsync(k_cerere As KeReceptieDeSalvat, ct As CancellationToken) As Task(Of KeReceptieSalvata)

    ''' <summary>
    ''' Slice 02EF-12: opens the shell's window «Adauga rezervare» (the DDF editor, with the existing indicators) with the value of each indicator filled in
    ''' from the reception; saving is refused while a value cannot cover the reception. Returns when the window has closed.
    ''' </summary>
    Function CreeazaRezervareAsync(k_cerere As KeRezervareDeCreat) As Task(Of KeRezervareCreata)

End Interface

''' <summary>The one place the shell puts its robot for the e-invoice views (set once at start, read when a button is pressed).</summary>
Public NotInheritable Class KeRobot

    Private Sub New()
    End Sub

    ''' <summary>The shell's robot, or Nothing while it is not wired.</summary>
    Public Shared Property Current As IKeRobot

    ''' <summary>
    ''' Slice 02EF-12: the reservations or the receptions of an angajament changed (a DDF saved, a download taken into K-BOT). Every open page that shows their
    ''' figures asks the server again. Raised on the UI thread by the shell; the argument is the angajament code.
    ''' </summary>
    Public Shared Event DateAngajamentSchimbate As EventHandler(Of String)

    ''' <summary>Tells every open page that the figures of this angajament changed (see <see cref="DateAngajamentSchimbate"/>). UI boundary: a listener that fails is logged.</summary>
    Public Shared Sub AnuntaDateSchimbate(k_cod As String)
        Try
            RaiseEvent DateAngajamentSchimbate(Nothing, If(k_cod, String.Empty))
        Catch ex As Exception
            KBot.Common.GlobalErrorLog.Write("KeRobot.AnuntaDateSchimbate", ex)
        End Try
    End Sub

End Class
