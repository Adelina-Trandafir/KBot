Imports System

Public NotInheritable Class HarnessTestResult
    Public ReadOnly Property Outcome As HarnessTestOutcome
    Public ReadOnly Property Message As String
    Public ReadOnly Property Details As String

    Private Sub New(outcome As HarnessTestOutcome, message As String, details As String)
        Me.Outcome = outcome
        Me.Message = message
        Me.Details = details
    End Sub

    Public Shared Function Passed(Optional message As String = "OK") As HarnessTestResult
        Return New HarnessTestResult(HarnessTestOutcome.Passed, message, Nothing)
    End Function

    Public Shared Function Failed(message As String, Optional details As String = Nothing) As HarnessTestResult
        Return New HarnessTestResult(HarnessTestOutcome.Failed, message, details)
    End Function

    Public Shared Function Skipped(message As String) As HarnessTestResult
        Return New HarnessTestResult(HarnessTestOutcome.Skipped, message, Nothing)
    End Function

    Public Shared Function Errored(ex As Exception) As HarnessTestResult
        Return New HarnessTestResult(HarnessTestOutcome.[Error], ex.Message, ex.ToString())
    End Function
End Class
