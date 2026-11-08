Option Strict On
Imports System.IO
Imports KBot.Domain

''' <summary>
''' Logica pură de nume a PDF-urilor ORD (felia 0033). Fără WinForms: se poate testa.
''' Sora lui <see cref="DdfPdfLocator"/>, cu convenția citită din <c>mdl_FX_ORD_PDF</c>
''' (NU ghicită — vezi worklog-ul feliei):
'''   &lt;root&gt;\&lt;partener | GENERAL&gt;\ORD_NR_{NrORD}_{CodAngajament}.PDF
''' Folderul partenerului se folosește doar când documentul e legat de partener; numele lui
''' e <c>NumePartener</c> normalizat (\W+ -&gt; «_»), aceeași regulă ca la DDF — de altfel
''' VBA-ul citește chiar <c>FX_DDF.PartAng/NumePartener</c> prin <c>FX_ORD.IDDF</c>, iar
''' serverul le trimite pe rândul ordonanțării.
'''
''' O CIUDĂȚENIE ACCESS de reținut (vizibilă în datele exportate: căi «ORD_NR_0_…»):
''' <c>mdl_FX_ORD_PDF</c> ia numărul dintr-un dicționar populat DOAR pe ramura „toate
''' documentele lunii"; pe ramura „un singur document" dicționarul e gol, deci numărul iese
''' 0 și fișierul se naște cu numele greșit. Aici calea AȘTEPTATĂ se compune întotdeauna cu
''' <c>NrORD</c>-ul real — nu reproducem defectul și nu ghicim un al doilea nume. Consecința
''' onestă: pentru documentele vechi salvate cu «_0_», vederea va spune „nu există PDF".
''' </summary>
Public NotInheritable Class OrdPdfLocator

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Calea AȘTEPTATĂ a PDF-ului unei ordonanțări, sub rădăcina dată (de regulă
    ''' <c>KBotPaths.Current.OrdPdfRoot</c>). Întoarce <c>Nothing</c> fără antet sau fără cod —
    ''' apelantul tratează asta ca „nu avem ce căuta", nu ca o eroare.
    ''' </summary>
    Public Shared Function ExpectedPath(root As String, ordonantare As OrdHeaderRow, codAngajament As String) As String
        If ordonantare Is Nothing Then Return Nothing
        If String.IsNullOrWhiteSpace(codAngajament) Then Return Nothing
        Dim folder As String = ordonantare.FolderPdf              ' partener normalizat sau «GENERAL»
        Dim fileName As String = $"ORD_NR_{ordonantare.NrOrd}_{codAngajament}.PDF"
        Return Path.Combine(NormalizeRoot(root), folder, fileName)
    End Function

    Private Shared Function NormalizeRoot(root As String) As String
        Return If(String.IsNullOrEmpty(root), String.Empty, root)
    End Function

End Class
