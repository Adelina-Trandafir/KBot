Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports KBot.Common

' Descoperă prin reflecție toate clasele care implementează IHarnessTest și au
' constructor fără parametri. Implicit scanează assembly-ul harness-ului.
Public NotInheritable Class HarnessTestDiscovery
    Public Shared Function Discover(Optional assemblies As IEnumerable(Of Assembly) = Nothing) As List(Of IHarnessTest)
        Dim asms As IEnumerable(Of Assembly) =
            If(assemblies, New Assembly() {GetType(HarnessTestDiscovery).Assembly})

        Dim result As New List(Of IHarnessTest)()
        For Each asm As Assembly In asms
            Dim types As Type()
            Try
                types = asm.GetTypes()
            Catch rtle As ReflectionTypeLoadException
                ' Nu înghiți: loghează complet, apoi continuă cu tipurile încărcabile.
                GlobalErrorLog.Write("HarnessTestDiscovery.GetTypes(" & asm.GetName().Name & ")", rtle)
                types = rtle.Types.Where(Function(x) x IsNot Nothing).ToArray()
            End Try

            For Each t As Type In types
                If t.IsClass AndAlso Not t.IsAbstract AndAlso GetType(IHarnessTest).IsAssignableFrom(t) Then
                    Dim ctor As ConstructorInfo = t.GetConstructor(Type.EmptyTypes)
                    If ctor IsNot Nothing Then
                        Try
                            Dim instance As IHarnessTest = TryCast(Activator.CreateInstance(t), IHarnessTest)
                            If instance IsNot Nothing Then
                                result.Add(instance)
                            End If
                        Catch ex As Exception
                            ' Un test cu ctor defect nu blochează harness-ul; eroarea e suprafațată.
                            GlobalErrorLog.Write("HarnessTestDiscovery.CreateInstance(" & t.FullName & ")", ex)
                        End Try
                    End If
                End If
            Next
        Next
        Return result.OrderBy(Function(x) x.Category).ThenBy(Function(x) x.Name).ToList()
    End Function
End Class
