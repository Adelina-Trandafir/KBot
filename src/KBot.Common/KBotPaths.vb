Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.Json

''' <summary>
''' Setările locale ale K-BOT (felia 0020-04, extinsă în felia 0024). Backing store: un fișier JSON
''' lângă executabil (<c>&lt;AppDir&gt;\kbot_paths.json</c>). Un fișier lipsă sau stricat CADE pe
''' implicit și loghează; NU aruncă la pornire.
'''
''' Conține:
'''   * <see cref="DdfPdfRoot"/> — rădăcina PDF-urilor DDF (felia 0020-04);
'''   * <see cref="AdobeViewerMode"/> / <see cref="AdobeNewInstance"/> — cum se comportă gazda
'''     Adobe din fila «Document» a vederii DDF (felia 0024). Se schimbă LA RULARE, deci fișierul
'''     se și SCRIE, nu doar se citește — vezi <see cref="Save"/> și <c>docs\SETARI_UTILIZATOR.md</c>.
'''
''' De ce AICI și nu într-un fișier nou: acesta ESTE mecanismul de setări al soluției. Alternativa
''' luată în calcul a fost <c>ThemeStore</c> (%AppData%\AVACONT\theme.json, per utilizator), dar
''' modul vizualizatorului descrie ce Adobe e INSTALAT pe mașină, nu o preferință a persoanei: un
''' magazin care urmează utilizatorul pe altă mașină ar duce acolo o valoare greșită.
'''
''' ATENȚIE: aceasta NU este o reînviere a lui <c>AppConfig</c>. `AppConfig` a fost retras fiindcă
''' ținea ADRESA SERVERULUI, iar adresa serverului rămâne hardcodată în <c>ApiOptions</c>.
''' </summary>
Public NotInheritable Class KBotPaths

    ''' <summary>Valoarea implicită a rădăcinii PDF-urilor DDF (planul, decizia 13).</summary>
    Public Const DefaultDdfPdfRoot As String = "C:\AVACONT\FOREXE\PDF\DDF\"

    ''' <summary>Valoarea implicită a modului vizualizatorului Adobe.</summary>
    Public Const DefaultAdobeViewerMode As String = "Auto"

    ''' <summary>Valoarea implicită a comutatorului «instanță nouă Adobe».</summary>
    Public Const DefaultAdobeNewInstance As String = "Auto"

    ''' <summary>
    ''' Motorul implicit de previzualizare DDF: fereastra Adobe găzduită.
    '''
    ''' Rămâne implicitul fiindcă e singurul care a rulat vreodată în aplicație. Ruta ActiveX arată
    ''' mai bine pe banc — randează XFA, schimbă documente, colapsează panourile cu un click — dar
    ''' asta s-a măsurat DOAR pe banc, deci se alege explicit, nu implicit.
    ''' </summary>
    Public Const DefaultAdobePreviewEngine As String = "Fereastra"

    ''' <summary>Numele fișierului de configurare, lângă executabil.</summary>
    Public Const FileName As String = "kbot_paths.json"

    ''' <summary>Rădăcina în care se caută PDF-urile DDF (recursiv). Nu e niciodată gol.</summary>
    Public Property DdfPdfRoot As String = DefaultDdfPdfRoot

    ''' <summary>
    ''' Profilul gazdei Adobe: «Auto» (detectează), «Modern» sau «Classic». Se păstrează ca TEXT,
    ''' fiindcă enumerarea trăiește în KBot.Controls, care referă KBot.Common (invers ar fi ciclu).
    ''' O valoare necunoscută NU e o eroare aici — apelantul cade pe «Auto» și avertizează.
    ''' </summary>
    Public Property AdobeViewerMode As String = DefaultAdobeViewerMode

    ''' <summary>Forțarea comutatorului «/n» la pornirea Adobe: «Auto», «Da» sau «Nu».</summary>
    Public Property AdobeNewInstance As String = DefaultAdobeNewInstance

    ''' <summary>
    ''' Cum se afișează PDF-ul pe fila «Document» a DDF: «Fereastra» (fereastra Adobe reparentată,
    ''' implicit) sau «ActiveX» (controlul AcroPDF, în proces). Text, din același motiv ca celelalte.
    ''' </summary>
    Public Property AdobePreviewEngine As String = DefaultAdobePreviewEngine

    Private Shared ReadOnly _gate As New Object()
    Private Shared _current As KBotPaths

    ''' <summary>
    ''' Instanța curentă, încărcată o singură dată din <c>&lt;AppDir&gt;\kbot_paths.json</c>.
    ''' Thread-safe; cade pe implicit dacă fișierul lipsește sau e stricat.
    ''' </summary>
    Public Shared ReadOnly Property Current As KBotPaths
        Get
            If _current Is Nothing Then
                SyncLock _gate
                    If _current Is Nothing Then _current = Load()
                End SyncLock
            End If
            Return _current
        End Get
    End Property

    ''' <summary>
    ''' Încarcă din directorul dat (implicit <see cref="AppContext.BaseDirectory"/>). Fișier
    ''' lipsă/gol/stricat -&gt; valori implicite (+ log pe stricat). NU aruncă.
    ''' </summary>
    Public Shared Function Load(Optional dir As String = Nothing) As KBotPaths
        Dim baseDir As String = If(String.IsNullOrEmpty(dir), AppContext.BaseDirectory, dir)
        Dim result As New KBotPaths()
        Dim filePath As String = Path.Combine(baseDir, FileName)

        Try
            If Not File.Exists(filePath) Then Return result   ' lipsă -> implicit, fără log

            Dim json As String = File.ReadAllText(filePath)
            If String.IsNullOrWhiteSpace(json) Then Return result   ' gol -> implicit

            Dim dto As KBotPathsDto = JsonSerializer.Deserialize(Of KBotPathsDto)(json)
            If dto IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(dto.DdfPdfRoot) Then result.DdfPdfRoot = dto.DdfPdfRoot.Trim()
                If Not String.IsNullOrWhiteSpace(dto.AdobeViewerMode) Then result.AdobeViewerMode = dto.AdobeViewerMode.Trim()
                If Not String.IsNullOrWhiteSpace(dto.AdobeNewInstance) Then result.AdobeNewInstance = dto.AdobeNewInstance.Trim()
                If Not String.IsNullOrWhiteSpace(dto.AdobePreviewEngine) Then result.AdobePreviewEngine = dto.AdobePreviewEngine.Trim()
            End If
            Return result
        Catch ex As Exception
            ' Stricat (JSON nevalid / drepturi) -> implicit + log; niciodată o excepție la pornire.
            GlobalErrorLog.Write("KBotPaths.Load", ex)
            Return New KBotPaths()
        End Try
    End Function

    ''' <summary>
    ''' Scrie setările curente în <c>&lt;dir&gt;\kbot_paths.json</c> și le face instanța
    ''' <see cref="Current"/>, ca o schimbare făcută la rulare să fie vizibilă imediat în tot
    ''' procesul, nu abia după repornire.
    '''
    ''' Întoarce False (+ log) când scrierea eșuează — de exemplu instalare într-un folder fără drept
    ''' de scriere. Setarea rămâne activă pentru SESIUNEA curentă; apelantul spune operatorului că
    ''' nu s-a putut salva. Niciodată o excepție: aceasta e o graniță de UI.
    ''' </summary>
    Public Function Save(Optional dir As String = Nothing) As Boolean
        ' Un director EXPLICIT înseamnă „scrie acolo" (teste, unelte) și NU atinge singleton-ul —
        ' altfel un test ar contamina starea procesului pentru tot ce vine după el.
        Dim isAppDir As Boolean = String.IsNullOrEmpty(dir)
        Dim baseDir As String = If(isAppDir, AppContext.BaseDirectory, dir)
        Dim filePath As String = Path.Combine(baseDir, FileName)
        Try
            Dim dto As New KBotPathsDto With {
                .DdfPdfRoot = DdfPdfRoot,
                .AdobeViewerMode = AdobeViewerMode,
                .AdobeNewInstance = AdobeNewInstance,
                .AdobePreviewEngine = AdobePreviewEngine}
            Dim json As String = JsonSerializer.Serialize(dto, New JsonSerializerOptions With {.WriteIndented = True})
            Directory.CreateDirectory(baseDir)
            File.WriteAllText(filePath, json, New UTF8Encoding(False))
            If isAppDir Then MakeCurrent()
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotPaths.Save", ex)
            ' Chiar dacă fișierul nu s-a scris, setarea din memorie devine cea curentă: operatorul
            ' a cerut-o acum, iar o sesiune care o ignoră ar fi mai derutantă decât una care nu o
            ' ține minte după repornire.
            If isAppDir Then MakeCurrent()
            Return False
        End Try
    End Function

    ''' <summary>Face din această instanță <see cref="Current"/> (schimbare vizibilă imediat).</summary>
    Private Sub MakeCurrent()
        SyncLock _gate
            _current = Me
        End SyncLock
    End Sub

End Class

''' <summary>DTO de fir pentru JSON. Numele proprietății E cheia JSON. POCO -> fără Try/Catch.</summary>
Friend NotInheritable Class KBotPathsDto
    Public Property DdfPdfRoot As String
    Public Property AdobeViewerMode As String
    Public Property AdobeNewInstance As String
    Public Property AdobePreviewEngine As String
End Class
