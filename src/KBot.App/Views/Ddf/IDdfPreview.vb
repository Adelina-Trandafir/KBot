Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Contractul unei suprafețe de previzualizare a documentului DDF (felia 0020-03). Vederea
''' (DdfView) nu știe nimic despre Adobe, XFA sau handle-uri de fereastră — vorbește doar cu
''' această interfață, iar <see cref="DdfPreviewFactory"/> alege implementarea la COMPILARE.
'''
''' Două implementări există (planul §11), amândouă construite, niciuna cod mort:
'''   * <see cref="XfaXmlPreview"/>   — IMPLICITĂ: redă din XML-ul XFA (siblingul .xml sau
'''                                     PdfXmlExtractor pe PDF), o suprafață WinForms tematizată;
'''   * <see cref="ReaderHostPreview"/> — REZERVĂ: găzduiește fereastra proprie a Adobe Reader.
''' Comutarea se face schimbând o singură constantă și recompilând — deliberat FĂRĂ comutator
''' la rulare (planul §11).
''' </summary>
Public Interface IDdfPreview

    ''' <summary>Controlul de găzduit în panoul de previzualizare al vederii.</summary>
    ReadOnly Property Surface As Control

    ''' <summary>
    ''' Afișează documentul de la <paramref name="pdfPath"/>. Când fișierul lipsește
    ''' (<paramref name="exists"/> = False) implementarea trebuie să redea starea
    ''' „document lipsă" (mesaj + butonul «Generează documentul») — NU să arunce.
    ''' </summary>
    Sub ShowDocument(pdfPath As String, exists As Boolean)

    ''' <summary>Golește suprafața (nicio revizie selectată / o rădăcină de lună).</summary>
    Sub Clear()

    ''' <summary>
    ''' Ridicat de butonul «Generează documentul» de pe suprafața „document lipsă".
    ''' DdfView îl tratează apelând felia 05 (generarea PDF-ului).
    ''' </summary>
    Event GenerateRequested As EventHandler

End Interface

''' <summary>Care implementare de previzualizare se folosește. Se schimbă în cod, nu la rulare.</summary>
Public Enum DdfPreviewMode
    ''' <summary>Implicit: redă din XML-ul XFA (fără proces extern).</summary>
    XfaXml = 0
    ''' <summary>Rezervă: găzduiește fereastra Adobe Reader.</summary>
    AdobeReader = 1
End Enum

''' <summary>
''' Alege implementarea de previzualizare. O singură constantă și un singur Select Case,
''' exact ca în planul §11. NU există comutator la rulare — se schimbă <see cref="Mode"/>
''' și se recompilează.
''' </summary>
Public NotInheritable Class DdfPreviewFactory

    Private Sub New()
    End Sub

    ' Schimbă această constantă și recompilează. Deliberat fără comutator la rulare.
    Private Const Mode As DdfPreviewMode = DdfPreviewMode.XfaXml

    ''' <summary>Creează suprafața de previzualizare aleasă la compilare.</summary>
    Public Shared Function Create() As IDdfPreview
        Select Case Mode
            Case DdfPreviewMode.AdobeReader
                Return New ReaderHostPreview()
            Case Else
                Return New XfaXmlPreview()
        End Select
    End Function

    ''' <summary>
    ''' English (slice 0025-05): same choice, but on the DEFAULT path it hands back the instance
    ''' the DESIGNER already created and parented, instead of building a second one.
    '''
    ''' Why it exists: `DdfView` now declares its preview surface in `DdfView.Designer.vb`, so the
    ''' operator can see the page on the design surface instead of an empty panel. Building another
    ''' `XfaXmlPreview` here would leave the designer's copy parented and unused, sitting behind the
    ''' one actually driven — two grids, one of them permanently blank.
    '''
    ''' On the Adobe path the compile-time switch still wins and a fresh `ReaderHostPreview` is
    ''' returned; the caller mounts it and hides the designer's surface (see `DdfView.MountPreview`).
    ''' </summary>
    Public Shared Function Create(designerDefault As XfaXmlPreview) As IDdfPreview
        If designerDefault Is Nothing Then Throw New ArgumentNullException(NameOf(designerDefault))
        Select Case Mode
            Case DdfPreviewMode.AdobeReader
                Return New ReaderHostPreview()
            Case Else
                Return designerDefault
        End Select
    End Function

End Class
