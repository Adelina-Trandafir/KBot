Imports System.Collections.Generic

Namespace KBot.Forexe
    Public Class JobRequest
        Public Property WorkflowName As String = String.Empty
        Public Property WflPath As String = String.Empty
        Public Property Parameters As New Dictionary(Of String, String)

        ''' <summary>
        ''' Browserul se vede cât rulează job-ul? IMPLICIT NU — exact ca în KBOT_IPC, unde
        ''' <c>isStealth = Not jobToRun.ShowBrowser</c>. Ascuns înseamnă stealth în executor:
        ''' fereastra pleacă off-screen (--window-position=-3000,0) și iese din Taskbar/Alt-Tab.
        ''' Se poate aduce oricând la vedere din consolă (ShowBrowserAsync).
        ''' Contează doar la RunAsync (conectarea), fiindcă acolo se CREEAZĂ fereastra;
        ''' un job următor rulează pe fereastra deja deschisă, în starea în care a lăsat-o.
        ''' </summary>
        Public Property ShowBrowser As Boolean = False
    End Class

    Public Class JobResult
        Public Property Success As Boolean
        Public Property Message As String = String.Empty
        ' Variabilele plate ale executorului la finalul job-ului (nume -> valoare).
        ' Consumatorii existenți (dicționar plat) rămân neatinși.
        Public Property Data As New Dictionary(Of String, String)
        ' Îmbogățire aditivă: rezultatele tabelare (ex. ScrapeTable) sparte pe
        ' variabilă -> listă de rânduri (coloană -> valoare). Populat de RunJobAsync
        ' pentru orice variabilă care conține un JSON array de obiecte.
        Public Property Tables As New Dictionary(Of String, List(Of Dictionary(Of String, String)))
    End Class
End Namespace
