Imports System.Collections.Generic

' Answer of POST /api/auth/units seen as the login window needs it: either the units the
' account may open, or "the password is the initial one - change it first" with the short-lived
' restricted token that only reaches the password-change routes (security plan S1).
Public NotInheritable Class UnitsOutcome
    Public Sub New(units As IReadOnlyList(Of UnitInfo), passwordChangeRequired As Boolean, restrictedToken As String)
        Me.Units = If(units, Array.Empty(Of UnitInfo)())
        Me.PasswordChangeRequired = passwordChangeRequired
        Me.RestrictedToken = restrictedToken
    End Sub

    Public ReadOnly Property Units As IReadOnlyList(Of UnitInfo)
    Public ReadOnly Property PasswordChangeRequired As Boolean
    Public ReadOnly Property RestrictedToken As String
End Class
