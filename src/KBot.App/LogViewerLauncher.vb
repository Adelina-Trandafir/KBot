Option Strict On
#If DEBUG Then
Imports System.Windows.Forms
Imports Microsoft.Extensions.DependencyInjection

''' <summary>
''' Capătul dinspre aplicație al punții <c>ILogViewerLauncher</c> (felia 0031-04): bancul de probă
''' cere o fereastră de jurnale, o primește de aici, construită prin DI ca oricare alta.
'''
''' <b>Doar pe Debug</b>, ca și referința către <c>KBot.DevHarness</c> — în Release nu există nici
''' interfața, nici clasa asta, iar <c>LogViewerForm</c> rămâne întreg (e o fereastră a shell-ului,
''' nu una a bancului).
''' </summary>
Friend NotInheritable Class LogViewerLauncher
    Implements Global.KBot.DevHarness.ILogViewerLauncher

    Private ReadOnly _provider As IServiceProvider

    Public Sub New(provider As IServiceProvider)
        _provider = provider
    End Sub

    ' Instanță nouă la fiecare cerere: proba vizuală o arată modal și o eliberează, deci una
    ' refolosită ar fi deja eliberată la a doua rulare.
    Public Function CreateLogViewer() As Form Implements Global.KBot.DevHarness.ILogViewerLauncher.CreateLogViewer
        Return _provider.GetRequiredService(Of LogViewerForm)()
    End Function

End Class
#End If
