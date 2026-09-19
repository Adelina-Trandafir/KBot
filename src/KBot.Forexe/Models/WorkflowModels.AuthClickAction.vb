Namespace WorkflowModels

    ''' <summary>
    ''' AuthClick action - similar cu Click, dar cu un timeout mai mare și opțiunea de a aștepta o anumită URL
    ''' după click pentru a confirma că autentificarea a reușit.
    ''' </summary>
    Public Class AuthClickAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "AuthClick"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        Public Property AuthTimeout As Integer = 120
        Public Property ExpectedUrlAfterAuth As String = String.Empty
        Public Property WaitNavigation As Boolean = True

        ' Slice 0071: failure sentinels watched DURING the wait for ExpectedUrlAfterAuth, not
        ' after it. When the certificate is unavailable or refused, FOREXE does not leave the
        ' page hanging -- it redirects to a logout page (.../vdesk/hangup.php3, laid out as
        ' <table id="main_table" class="logout_page">). Without these the step sat the whole
        ' AuthTimeout on a page that had already said no. Either sentinel ends the step at
        ' once with FailMessage. Both optional; FailUrl takes Playwright's glob syntax like
        ' ExpectedUrlAfterAuth, FailSelector is a locator.
        Public Property FailUrl As String = String.Empty
        Public Property FailSelector As String = String.Empty
        Public Property FailMessage As String = String.Empty
    End Class

End Namespace
