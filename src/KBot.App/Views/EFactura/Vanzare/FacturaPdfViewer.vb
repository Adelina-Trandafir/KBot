Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.EFactura

''' <summary>
''' Slice 00EF-09 -- the PDF viewer of the E-Factura invoice window: the same embedded Adobe surface the DDF «Document» page uses
''' (<see cref="ReaderHostPreview"/>), behind the small contract the window knows (<see cref="IFacturaPdfViewer"/>). The window lives in
''' KBot.EFactura, which cannot reference this project, so the shell hands it a factory of these.
'''
''' <para>Nothing is signed or uploaded here: no signing session and no print-count target are set, so a print is only logged and a save
''' stays on the file shown -- unless <c>k_userChoosesSave</c> (slice 00EF-31, the received invoices): then Adobe's Save asks the
''' operator whether to save and where. The files are in the PDF work area (<see cref="TempPdfStore"/>), which is emptied when K-BOT starts.</para>
''' </summary>
Friend NotInheritable Class FacturaPdfViewer
    Implements IFacturaPdfViewer

    Private ReadOnly _preview As New ReaderHostPreview()

    Public Sub New(Optional k_userChoosesSave As Boolean = False)
        _preview.UserChoosesSave = k_userChoosesSave
        _preview.ShowNotice("Alegeți o factură.")
    End Sub

    Public ReadOnly Property Surface As Control Implements IFacturaPdfViewer.Surface
        Get
            Return _preview
        End Get
    End Property

    Public Sub ShowDocument(k_path As String) Implements IFacturaPdfViewer.ShowDocument
        _preview.ShowDocument(k_path, True)
    End Sub

    Public Sub ShowNotice(k_message As String) Implements IFacturaPdfViewer.ShowNotice
        _preview.ShowNotice(k_message)
    End Sub

    Public Sub Clear() Implements IFacturaPdfViewer.Clear
        _preview.Clear()
    End Sub

End Class
