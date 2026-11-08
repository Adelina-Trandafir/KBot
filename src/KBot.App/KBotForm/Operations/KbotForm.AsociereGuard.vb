Imports System.Globalization
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' The association guard (operator, 30.09.2026): no FOREXE refresh of an angajament and no new
''' ORD while one of its receptions has a chain that does not close on its value.
''' </summary>
''' <remarks>
''' <para>Why: after a download the server refuses the whole save when a touched chain's newest
''' snapshot has another total than the reception (F15, <c>valideaza_plasarile</c>). A snapshot
''' linked to the wrong reception earlier -- the anytime editor only warns -- therefore made
''' every later download of that angajament fail at the very end, after the robot had run. An
''' ORD built on such a chain uses wrong reception figures. So the check runs FIRST, and the
''' operator is offered the association window to fix the links.</para>
''' <para>The check is <see cref="AsociereStare.LanturiNeinchise"/>, over the same GET the
''' association window reads. When it cannot be read, the operation does not start either: a
''' refresh or an ORD needs the server anyway, and letting it through would hide the error.</para>
''' </remarks>
Partial Public Class KbotForm

    Private Shared ReadOnly _roCultureGuard As CultureInfo = CultureInfo.GetCultureInfo("ro-RO")

    ''' <summary>
    ''' True when <paramref name="operatie"/> may start for <paramref name="cod"/>. Otherwise
    ''' the operator was told why and offered the association window; after it closes the
    ''' links are read again, so a fixed angajament goes on without a second click.
    ''' </summary>
    Private Async Function AsocierePermiteAsync(cod As String, operatie As String) As Task(Of Boolean)
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return False
            Do
                Dim stare As AsociereStare
                busyBar.Running = True
                Try
                    stare = Await WithReauth(Of AsociereStare)(
                        Function() _apiClient.GetAsociereAsync(cod, CancellationToken.None))
                Finally
                    busyBar.Running = False
                End Try
                If stare Is Nothing Then Throw New InvalidOperationException("GET asociere returned nothing.")

                Dim probleme As List(Of LantNeinchis) = stare.LanturiNeinchise()
                If probleme.Count = 0 Then Return True

                Dim r As DialogResult = KBotMessage.Show(Me, TextLanturiNeinchise(cod, operatie, probleme),
                                                         $"Asocierea recepțiilor «{cod}»",
                                                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If r <> DialogResult.Yes Then Return False
                If Not Await DeschideLegaturileReceptiilorAsync(cod) Then Return False
            Loop
        Catch ex As Exception
            ' UI flow: logged and shown; the operation does not start.
            GlobalErrorLog.Write("MainForm.AsocierePermiteAsync", ex)
            KBotMessage.Show(Me, $"Asocierea recepțiilor lui «{cod}» nu a putut fi verificată, deci «{operatie}» " &
                            "nu pornește. Detalii în jurnalul de erori.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ''' <summary>The sentence the operator reads: which receptions, and what to do.</summary>
    Private Shared Function TextLanturiNeinchise(cod As String, operatie As String,
                                                 probleme As List(Of LantNeinchis)) As String
        Const MaxAfisate As Integer = 6
        Dim sb As New Text.StringBuilder()
        sb.AppendLine($"«{operatie}» nu poate porni: în asocierea recepțiilor lui «{cod}» există " &
                      If(probleme.Count = 1, "o recepție al cărei ultim instantaneu nu are valoarea recepției:",
                                             $"{probleme.Count} recepții al căror ultim instantaneu nu are valoarea recepției:"))
        sb.AppendLine()
        For Each p As LantNeinchis In probleme.Take(MaxAfisate)
            sb.AppendLine($"• Recepția din {p.Receptie.DataR:dd.MM.yyyy} ({Suma(p.Receptie.SumaAntet)} lei): " &
                          $"ultimul instantaneu, din {p.Ultimul.DataH:dd.MM.yyyy HH:mm}, are {Suma(p.Ultimul.Total)} lei.")
        Next
        If probleme.Count > MaxAfisate Then sb.AppendLine($"• … și încă {probleme.Count - MaxAfisate}.")
        sb.AppendLine()
        sb.AppendLine("De obicei un instantaneu stă pe altă recepție decât a lui. Mutați-l pe recepția corectă " &
                      "(sau în coș) și salvați legăturile.")
        sb.AppendLine()
        sb.Append("Deschid acum fereastra de asociere?")
        Return sb.ToString()
    End Function

    Private Shared Function Suma(v As Double) As String
        Return v.ToString("N2", _roCultureGuard)
    End Function

End Class
