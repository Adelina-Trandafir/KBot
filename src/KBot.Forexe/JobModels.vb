Imports System.Collections.Generic
Imports KBot.Domain      ' CelulaTabel / RandTabel / TabelRezultat (decizia D-N).

Namespace KBot.Forexe
    Public Class JobRequest
        Public Property WorkflowName As String = String.Empty
        Public Property WflPath As String = String.Empty
        Public Property Parameters As New Dictionary(Of String, String)

        ' There is no ShowBrowser switch any more (slice 0070). KBOT_IPC had one
        ' (`isStealth = Not jobToRun.ShowBrowser`) and a job could ask for a visible Chromium
        ' window; here the window is ALWAYS born hidden, and the operator sees the page only
        ' docked into a K-BOT form («Arată browserul» in the console). A free window has a
        ' close button, and closing it kills the session.
    End Class

    Public Class JobResult
        Public Property Success As Boolean
        Public Property Message As String = String.Empty
        ' Variabilele plate ale executorului la finalul job-ului (nume -> valoare).
        ' Consumatorii existenți (dicționar plat) rămân neatinși.
        Public Property Data As New Dictionary(Of String, String)
        ' Îmbogățire aditivă: rezultatele tabelare (ex. ScrapeTable) sparte pe
        ' variabilă -> listă de rânduri (coloană -> celulă). Populat de RunJobAsync
        ' pentru orice variabilă care conține un JSON array de obiecte.
        '
        ' CELULA E `CelulaTabel`, NU `String`, DIN 26.08.2026 (decizia D-N). Un
        ' `ForEachVar` al cărui `collectFields` numește un câmp pe care un `ScrapeTable`
        ' interior îl scrie cu `saveTo` produce o celulă IMBRICATĂ, iar executorul o
        ' păstrează ca atare (`BuildCollectedRow` face `JToken.Parse`). Aici se turtea
        ' înapoi în text cu `.ToString()`, și serverul trebuia să țină vie o a doua cale
        ' de citire pentru ea. Structura călătorește; nimic nu se aplatizează.
        Public Property Tables As New Dictionary(Of String, TabelRezultat)
    End Class
End Namespace
