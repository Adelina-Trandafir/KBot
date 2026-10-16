Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slices KE05..KE07 -- the FOREXE robot, seen from the e-invoice views. KBot.EFactura cannot reach the shell's robot (it lives in KBot.App),
''' so the shell hands it over through <see cref="KeRobot.Current"/> when it starts. Nothing there = the views say the robot is not available.
''' </summary>
Public Interface IKeRobot

    ''' <summary>
    ''' Writes the reception on the site (adds a new one or changes an existing one), then reads the angajament back and takes the reception into K-BOT.
    ''' Never throws for a refusal: the answer says what happened and why.
    ''' </summary>
    Function SalveazaReceptieAsync(k_cerere As KeReceptieDeSalvat, ct As CancellationToken) As Task(Of KeReceptieSalvata)

End Interface

''' <summary>The one place the shell puts its robot for the e-invoice views (set once at start, read when a button is pressed).</summary>
Public NotInheritable Class KeRobot

    Private Sub New()
    End Sub

    ''' <summary>The shell's robot, or Nothing while it is not wired.</summary>
    Public Shared Property Current As IKeRobot

End Class
