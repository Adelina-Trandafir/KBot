''' <summary>
''' Where a read, a verification or a migration stands: the phase, the step it is on (n of N) with the table being processed inside it,
''' and how much of the whole is done. Built by <see cref="AvcRunner"/> and shown by the console; carries no logic of its own.
''' </summary>
Friend NotInheritable Class AvcProgressInfo

    Public Sub New(k_phase As String, k_stepNo As Integer, k_stepCount As Integer, k_title As String, k_detail As String, k_fraction As Double)
        Phase = k_phase
        StepNo = k_stepNo
        StepCount = k_stepCount
        Title = k_title
        Detail = k_detail
        Fraction = Math.Max(0.0, Math.Min(1.0, k_fraction))
    End Sub

    ''' <summary>Citire / Verificare / Scriere.</summary>
    Public ReadOnly Property Phase As String
    Public ReadOnly Property StepNo As Integer
    Public ReadOnly Property StepCount As Integer
    Public ReadOnly Property Title As String
    ''' <summary>The table inside the step, with its own count (e.g. <c>«TZ_Oper»: documente 1200 din 5400</c>); empty between tables.</summary>
    Public ReadOnly Property Detail As String
    ''' <summary>The share of the whole operation done, 0..1.</summary>
    Public ReadOnly Property Fraction As Double

    ''' <summary>The line the operator reads, without the clock (the console adds elapsed and remaining time).</summary>
    Public Function Describe() As String
        Dim k_text = Phase
        If StepCount > 0 AndAlso StepNo > 0 Then k_text &= $" {StepNo}/{StepCount}"
        If Title.Length > 0 Then k_text &= $" · {Title}"
        If Detail.Length > 0 Then k_text &= $" · {Detail}"
        Return k_text
    End Function

End Class
