Option Strict On
Imports System.Collections.Generic
Imports System.Data
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 0104: the <see cref="IAccessBridge"/> of the Access component. KBot.Common.AccessBridge finds this
''' class by its interface (the only one in the assembly), so its name is free; it must have a public
''' parameterless constructor.
''' </summary>
Public NotInheritable Class AccessBridgeImpl
    Implements IAccessBridge

    Public Function ResolveUnitFile(registryPath As String, idUnitate As Integer) As String _
        Implements IAccessBridge.ResolveUnitFile
        Return AccessBudgetSender.ResolveUnitFile(registryPath, idUnitate)
    End Function

    Public Function ReadRegistry(registryPath As String, an As Integer, sursa As String) As DataTable _
        Implements IAccessBridge.ReadRegistry
        Return AccessBudgetSender.ReadRegistry(registryPath, an, sursa)
    End Function

    Public Function SendBudget(unitFile As String, idClsfAcc As Integer, budget As BudgetVersion,
                               corrections As IEnumerable(Of RectificareBugetara)) As AccessSendResult _
        Implements IAccessBridge.SendBudget
        Return AccessBudgetSender.Send(unitFile, idClsfAcc, budget, corrections)
    End Function

End Class
