Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Slice 00EF-09 -- what the invoice window asks of a PDF viewer. The viewer itself (the embedded Adobe surface of the DDF
''' «Document» page, <c>ReaderHostPreview</c>) lives in KBot.App, which this project cannot reference, so the shell hands the
''' window a factory and the window only talks to this contract. A window made without a factory shows a plain note where the
''' viewer would be.
''' </summary>
Public Interface IFacturaPdfViewer

    ''' <summary>The control the window puts in its «document» page (it is docked to fill).</summary>
    ReadOnly Property Surface As Control

    ''' <summary>Shows the PDF at <paramref name="k_path"/> (an existing file).</summary>
    Sub ShowDocument(k_path As String)

    ''' <summary>A plain line instead of a document («Se descarcă…», or why there is none); lets go of the document on screen.</summary>
    Sub ShowNotice(k_message As String)

    ''' <summary>Lets go of the document on screen.</summary>
    Sub Clear()

End Interface
