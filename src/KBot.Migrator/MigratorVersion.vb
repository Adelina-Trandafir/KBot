Imports System.Reflection

''' <summary>
''' The migrator's own FileVersion (the one bumped by hand in KBot.Migrator.vbproj), shown in
''' the title bar and written into every journal so the operator can tell which build ran.
''' </summary>
''' <remarks>
''' Read from the assembly attribute, not from the .exe on disk: a single-file publish has no
''' Assembly.Location to hand to FileVersionInfo.
''' </remarks>
Public NotInheritable Class MigratorVersion

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property Text As String
        Get
            Dim attr = GetType(MigratorVersion).Assembly.GetCustomAttribute(Of AssemblyFileVersionAttribute)()
            Return If(attr Is Nothing OrElse String.IsNullOrEmpty(attr.Version), "?", attr.Version)
        End Get
    End Property

End Class
