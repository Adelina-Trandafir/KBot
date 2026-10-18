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

    ' ---- Slice 00EF-43: the received e-invoices in the Access store -------------------------------------

    Public Function ResolveEFacturaStore(registryPath As String, an As Integer) As String _
        Implements IAccessBridge.ResolveEFacturaStore
        Return AccessEfStore.ResolveStore(registryPath, an)
    End Function

    Public Function ListEFacturaMessageIds(storeFile As String) As List(Of String) _
        Implements IAccessBridge.ListEFacturaMessageIds
        Return AccessEfStore.ListMessageIds(storeFile)
    End Function

    Public Function ListEFacturaUnitMessageIds(storeFile As String, cuiNormalizat As String) As List(Of String) _
        Implements IAccessBridge.ListEFacturaUnitMessageIds
        Return AccessEfStore.ListUnitMessageIds(storeFile, cuiNormalizat)
    End Function

    Public Function ReadEFacturaMessages(storeFile As String, idSol As IEnumerable(Of String)) As List(Of AccessEfMessage) _
        Implements IAccessBridge.ReadEFacturaMessages
        Return AccessEfStore.ReadMessages(storeFile, idSol)
    End Function

    Public Function WriteEFacturaMessages(storeFile As String, messages As IEnumerable(Of AccessEfMessage)) As AccessEfWriteResult _
        Implements IAccessBridge.WriteEFacturaMessages
        Return AccessEfStore.WriteMessages(storeFile, messages)
    End Function

End Class
