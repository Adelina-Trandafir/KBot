Option Strict On
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Windows.Forms

''' <summary>
''' English (slice 0025): "is this instance living inside the Visual Studio designer rather than
''' in the running application?".
'''
''' <c>Control.DesignMode</c> on its own is not enough and never was: it is False while the
''' constructor runs (the site is attached afterwards) and it is False for a control NESTED
''' inside another control (only the sited, top-level one is told). Four independent signals are
''' consulted, any one of which is decisive.
'''
''' PUBLIC on purpose: <c>KBot.Controls</c> is a different assembly and needs the same answer
''' (it already references <c>KBot.Theming</c>). Hidden from the property grid and pushed out of
''' IntelliSense's common list so it does not look like part of the theming API.
''' </summary>
<EditorBrowsable(EditorBrowsableState.Advanced)>
Public NotInheritable Class KBotDesignTime

    ' Static helper only — never instantiated.
    Private Sub New()
    End Sub

    ' Host processes that run designers. VS2022 hosts the WinForms designer OUT of process
    ' (DesignToolsServer.exe), so "devenv" alone would miss exactly the case that matters on
    ' net8.0-windows. Computed once: the process name cannot change while we run.
    Private Shared ReadOnly _designerProcess As Boolean = DetectDesignerProcess()

    ''' <summary>
    ''' Returns True when <paramref name="c"/> is being hosted by a designer.
    ''' </summary>
    Public Shared Function IsDesignTime(c As Control) As Boolean
        ' No logging in here, deliberately: this predicate exists precisely so that the paint /
        ' mouse / keyboard handlers do NOT write log files from inside Visual Studio. A throw
        ' from a reflection-free predicate is not realistic; if one ever happens, "no, we are
        ' not in a designer" is the safe answer (the runtime path is the strict one).
        Try
            If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then Return True

            ' Walk up: a nested control is not sited, its top-level container is.
            Dim cur As Control = c
            Dim guard As Integer = 0
            While cur IsNot Nothing AndAlso guard < 64
                If cur.Site IsNot Nothing AndAlso cur.Site.DesignMode Then Return True
                cur = cur.Parent
                guard += 1
            End While

            Return _designerProcess
        Catch
            Return False
        End Try
    End Function

    Private Shared Function DetectDesignerProcess() As Boolean
        Try
            Dim name As String = Process.GetCurrentProcess().ProcessName
            If String.IsNullOrEmpty(name) Then Return False
            For Each host As String In {"devenv",
                                        "DesignToolsServer",
                                        "Microsoft.VisualStudio.DesignTools.DesignToolsServer",
                                        "XDesProc",
                                        "Blend"}
                If String.Equals(name, host, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
            Return False
        Catch
            Return False
        End Try
    End Function

End Class
