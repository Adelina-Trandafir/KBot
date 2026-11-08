Option Strict On
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>SLICE-02EF-14: partea comună din dreapta a editorului de recepții.</summary>
Public Class PrimiteReceptiiEditorView

    Friend ReadOnly pgNoua As PrimiteReceptieNouaView
    Friend ReadOnly pgExistente As PrimiteReceptiiExistenteView
    Friend ReadOnly pgAsociate As PrimiteReceptiiAsociateView

    Public Sub New()
        InitializeComponent()
        Dim backgroundRoles As New KBotBackgroundRoles(components)
        pgNoua = New PrimiteReceptieNouaView With {.Name = NameOf(pgNoua)}
        pgExistente = New PrimiteReceptiiExistenteView With {.Name = NameOf(pgExistente)}
        pgAsociate = New PrimiteReceptiiAsociateView With {.Name = NameOf(pgAsociate)}
        ' Keep ownership of the inactive views while they are detached from the panel.
        components.Add(pgNoua)
        components.Add(pgExistente)
        components.Add(pgAsociate)
        ' SLICE-0129-02: the runtime views share the form's background unless they specify their own.
        For Each k_surface As Control In New Control() {pgNoua, pgNoua.tlyNoua, pgExistente, pgExistente.pnlMod, pgAsociate}
            backgroundRoles.SetBackgroundRole(k_surface, ThemeBackgroundRole.Inherit)
        Next
        backgroundRoles.SetBackgroundRole(pnlRecViewHost, ThemeBackgroundRole.Inherit)
        ShowPage(pgNoua)
    End Sub

    ' SLICE-KE20-06: mount only the selected view and keep the other instances for the next switch.
    Friend Sub ShowPage(k_view As Control)
        If k_view.Parent Is pnlRecViewHost Then Return
        pnlRecViewHost.SuspendLayout()
        Try
            pnlRecViewHost.Controls.Clear()
            k_view.Dock = DockStyle.Fill
            pnlRecViewHost.Controls.Add(k_view)
            ThemeManager.Apply(k_view)
        Finally
            pnlRecViewHost.ResumeLayout(True)
        End Try
    End Sub

    Friend Sub SetPageEnabled(k_enabled As Boolean)
        pnlRecViewHost.Enabled = k_enabled
    End Sub

End Class
