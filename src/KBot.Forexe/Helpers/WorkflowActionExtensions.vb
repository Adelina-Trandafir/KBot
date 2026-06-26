Imports System.Runtime.CompilerServices
Imports WorkflowModels

''' <summary>
''' Extensii pentru IWorkflowAction care nu necesită modificarea modelelor sau a interfeței.
''' ConditionalWeakTable atașează metadata la instanțe existente fără să le modifice —
''' intrările dispar automat când acțiunea e garbage-collected.
''' </summary>
Friend Module WorkflowActionExtensions

    Private ReadOnly _skipIdle As New _
        Runtime.CompilerServices.ConditionalWeakTable(Of IWorkflowAction, StrongBox(Of Boolean))()

    ''' <summary>Marchează acțiunea cu valoarea skipIdleWait citită din WFL.</summary>
    <Extension>
    Friend Sub SetSkipIdleWait(action As IWorkflowAction, value As Boolean)
        _skipIdle.GetOrCreateValue(action).Value = value
    End Sub

    ''' <summary>Returnează True dacă acțiunea are skipIdleWait="true" în WFL.</summary>
    <Extension>
    Friend Function GetSkipIdleWait(action As IWorkflowAction) As Boolean
        Dim box As StrongBox(Of Boolean) = Nothing
        Return _skipIdle.TryGetValue(action, box) AndAlso box.Value
    End Function

End Module