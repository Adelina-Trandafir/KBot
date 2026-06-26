Namespace WorkflowModels

    ''' <summary>
    ''' SwitchTab action — Comută _page pe un tab deja deschis în browser.
    ''' Criteriile de selecție sunt evaluate în ordine: tabIndex → urlEquals → urlContains → urlPattern.
    ''' Dacă niciun tab nu corespunde, se aruncă excepție (eroare critică).
    ''' Dacă expectedUrl e setat, rulează Children dacă URL-ul conține valoarea, ElseChildren dacă nu.
    ''' </summary>
    Public Class SwitchTabAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "SwitchTab"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        ' ── Criterii de identificare tab (cel puțin unul obligatoriu) ──────────
        <WflRequiredOneOf("tabSel")> Public Property TabIndex As Integer = -1
        <WflRequiredOneOf("tabSel")> Public Property UrlEquals As String = String.Empty
        <WflRequiredOneOf("tabSel")> Public Property UrlContains As String = String.Empty
        <WflRequiredOneOf("tabSel")> Public Property UrlPattern As String = String.Empty

        ' ── Comportament ────────────────────────────────────────────────────────
        Public Property Reload As Boolean = False
        Public Property SavePreviousTabTo As String = String.Empty
        Public Property SaveCurrentUrlTo As String = String.Empty
        Public Property CloseTabWhenDone As Boolean = False

        ' ── Verificare post-switch (activează Children/ElseChildren) ────────────
        Public Property ExpectedUrl As String = String.Empty
        Public Property Children As New List(Of IWorkflowAction)
        Public Property ElseChildren As New List(Of IWorkflowAction)
    End Class

End Namespace
