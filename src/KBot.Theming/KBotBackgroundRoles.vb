Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Public Enum ThemeBackgroundRole
    Automatic
    Surface
    SurfaceAlt
    Inherit
End Enum

''' <summary>Exposes an optional background role on layout surfaces in the WinForms designer.</summary>
<ProvideProperty("BackgroundRole", GetType(Control))>
<ToolboxItemFilter("System.Windows.Forms")>
Public Class KBotBackgroundRoles
    Inherits Component
    Implements IExtenderProvider

    Private NotInheritable Class RoleSetting
        Public Role As ThemeBackgroundRole
    End Class

    Private Shared ReadOnly _roles As New ConditionalWeakTable(Of Control, RoleSetting)()

    Public Sub New()
    End Sub

    Public Sub New(container As IContainer)
        container.Add(Me)
    End Sub

    Public Function CanExtend(extendee As Object) As Boolean Implements IExtenderProvider.CanExtend
        Return TypeOf extendee Is Form OrElse TypeOf extendee Is Panel OrElse TypeOf extendee Is UserControl
    End Function

    <Category("K-BOT Theme")>
    <Description("Chooses the theme background. Inherit uses the parent's background unless this control has its own designer BackColor.")>
    <DefaultValue(ThemeBackgroundRole.Automatic)>
    Public Function GetBackgroundRole(ctrl As Control) As ThemeBackgroundRole
        Return RoleFor(ctrl)
    End Function

    Public Sub SetBackgroundRole(ctrl As Control, value As ThemeBackgroundRole)
        If value = ThemeBackgroundRole.Automatic Then
            _roles.Remove(ctrl)
        Else
            _roles.GetValue(ctrl, Function(key) New RoleSetting()).Role = value
        End If
    End Sub

    Friend Shared Function RoleFor(ctrl As Control) As ThemeBackgroundRole
        Dim setting As RoleSetting = Nothing
        Return If(_roles.TryGetValue(ctrl, setting), setting.Role, ThemeBackgroundRole.Automatic)
    End Function
End Class
