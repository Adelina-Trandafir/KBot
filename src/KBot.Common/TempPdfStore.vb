Option Strict On
Imports System.IO

''' <summary>
''' Zona de lucru a PDF-urilor NESEMNATE (felia 0041): <c>&lt;AppDir&gt;\TempPdf\</c>.
'''
''' DOUĂ ZONE, DOUĂ REGULI — deosebirea contează:
'''   * PDF-urile SEMNATE stau în cache-ul persistent (<c>KBotPaths.DdfPdfRoot</c> /
'''     <c>OrdPdfRoot</c>, cu numele lor obișnuite) și se validează prin SHA-256 față de
'''     server. Ele NU se șterg niciodată în bloc — un fișier semnat se înlocuiește doar când
'''     suma lui nu mai corespunde.
'''   * PDF-urile NESEMNATE sunt artefacte DERIVATE: se regenerează prin <c>XfaWriter</c> ori
'''     de câte ori operatorul cere să le vadă, nu se încarcă niciodată pe server și nu au
'''     sumă de urmărit. Ele stau AICI, iar folderul se golește la fiecare pornire — vechea
'''     regulă „temporar, șters la fiecare deschidere" trăiește exclusiv în această zonă.
'''
''' <see cref="Wipe"/> este o graniță de PORNIRE: nu are voie să arunce niciodată. Un fișier
''' blocat (Adobe îl ține deschis dintr-o sesiune anterioară) se sare cu un avertisment în
''' jurnal — aplicația pornește oricum.
''' </summary>
Public NotInheritable Class TempPdfStore

    ''' <summary>Numele folderului, lângă executabil.</summary>
    Public Const FolderName As String = "TempPdf"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Rădăcina zonei de lucru: <c>&lt;AppDir&gt;\TempPdf\</c>. Nu creează nimic — vezi
    ''' <see cref="EnsureRoot"/>.
    ''' </summary>
    Public Shared ReadOnly Property Root As String
        Get
            Return KBotPaths.FolderPdfTemporar
        End Get
    End Property

    ''' <summary>
    ''' Rădăcina, creată dacă lipsește. Graniță de I/O: loghează și rearuncă — fără folder nu
    ''' există unde scrie documentul regenerat, iar apelantul trebuie să vadă asta.
    ''' </summary>
    Public Shared Function EnsureRoot() As String
        Try
            Dim dir As String = Root
            Directory.CreateDirectory(dir)
            Return dir
        Catch ex As Exception
            GlobalErrorLog.Write("TempPdfStore.EnsureRoot", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Calea din zona de lucru pentru un nume de fișier dat. Numele vine din aceleași convenții
    ''' ca la cache-ul persistent, dar aici NU există subfoldere de partener: zona e plată și
    ''' oricum se golește la pornire.
    ''' </summary>
    Public Shared Function PathFor(fileName As String) As String
        If String.IsNullOrWhiteSpace(fileName) Then Return Nothing
        Return Path.Combine(Root, Path.GetFileName(fileName))
    End Function

    ''' <summary>
    ''' Golește zona de lucru. Se cheamă O DATĂ, la pornirea aplicației.
    '''
    ''' NU aruncă niciodată: un fișier care nu se poate șterge (ținut deschis de un Adobe rămas
    ''' din sesiunea anterioară) se sare, se loghează și se merge mai departe — o pornire care
    ''' cade din cauza unui fișier temporar ar fi mult mai rea decât un temporar rămas pe disc.
    ''' Întoarce numărul de fișiere efectiv șterse.
    ''' </summary>
    Public Shared Function Wipe() As Integer
        Dim sterse As Integer = 0
        Try
            Dim dir As String = Root
            If Not Directory.Exists(dir) Then Return 0

            For Each full As String In Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
                Try
                    ' Un fișier marcat read-only ar refuza ștergerea; îl deblocăm întâi.
                    Dim attrs As FileAttributes = File.GetAttributes(full)
                    If (attrs And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then
                        File.SetAttributes(full, attrs And Not FileAttributes.ReadOnly)
                    End If
                    File.Delete(full)
                    sterse += 1
                Catch exFisier As Exception
                    ' Blocat / fără drepturi: îl lăsăm și continuăm cu următorul.
                    GlobalErrorLog.Write("TempPdfStore.Wipe(" & Path.GetFileName(full) & ")", exFisier)
                End Try
            Next
            Return sterse
        Catch ex As Exception
            ' Graniță de pornire: logăm și înghițim (enumerarea însăși a căzut).
            GlobalErrorLog.Write("TempPdfStore.Wipe", ex)
            Return sterse
        End Try
    End Function

End Class
