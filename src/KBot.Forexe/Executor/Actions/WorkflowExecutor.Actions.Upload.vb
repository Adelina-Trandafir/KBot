Imports System.IO
Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteUploadAsync(action As UploadAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"Încarc fișier în: {parsedSelector}")
        If Not File.Exists(action.Path) Then Throw New FileNotFoundException(action.Path)

        Dim locator = _page.Locator(parsedSelector)

        ' Așteptăm ca input-ul să fie prezent în DOM cu timeout
        Await locator.WaitForAsync(New LocatorWaitForOptions With {
        .State = WaitForSelectorState.Attached,
        .Timeout = action.Timeout * 1000
    })

        Await locator.SetInputFilesAsync(action.Path)
    End Function

End Class
