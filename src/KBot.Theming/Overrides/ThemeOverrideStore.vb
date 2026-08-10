Option Strict On
Imports System.IO
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode
Imports KBot.Common

''' <summary>
''' Citirea/scrierea fișierelor de suprascrieri (<see cref="ThemeOverrideSet"/>) — sora lui
''' <see cref="ThemeStore"/>, dar pentru stiluri PE CONTROL, nu pentru alegerea schemei.
''' Locul implicit: …\AVACONT\Overrides\*.json, lângă …\AVACONT\Themes\.
'''
''' FELIA 0028 SCRIE, NU CITEȘTE la pornire: <see cref="LoadAll"/> și <see cref="LoadFile"/>
''' există și sunt testate, dar nimeni nu le apelează în calea de pornire a aplicației. Aplicarea
''' la rulare, app-wide, e explicit lăsată pe felia următoare (cerința operatorului: «later, not
''' in this slice»).
'''
''' Toate operațiile de I/O sunt metode-frontieră: logează prin <c>GlobalErrorLog</c> ȘI
''' rearuncă — apelantul (editorul) trebuie să poată spune operatorului că salvarea a eșuat.
''' </summary>
Public Module ThemeOverrideStore

    Private Const OverridesSubfolder As String = "Overrides"

    ''' <summary>
    ''' <c>UnsafeRelaxedJsonEscaping</c> peste latina extinsă: fără el, System.Text.Json scrie
    ''' diacriticele românești ca „ș”. Regula casei cere diacritice LITERALE — fișierul e
    ''' menit să fie deschis și citit de om.
    ''' </summary>
    Private ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .PropertyNameCaseInsensitive = True,
        .DefaultIgnoreCondition = Serialization.JsonIgnoreCondition.WhenWritingNull,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    ''' <summary>…\AVACONT\Overrides (rădăcina o dă <see cref="ThemeStore.AppDataFolder"/>,
    ''' deci redirijarea pentru teste e comună cu a schemelor).</summary>
    Public ReadOnly Property OverridesFolder As String
        Get
            Return Path.Combine(ThemeStore.AppDataFolder, OverridesSubfolder)
        End Get
    End Property

    ''' <summary>Calea implicită pentru o gazdă: …\AVACONT\Overrides\{Scope}.json.</summary>
    Public Function DefaultPathFor(scope As String) As String
        Dim safeName As String = SanitizeFileName(If(scope, String.Empty))
        If String.IsNullOrWhiteSpace(safeName) Then safeName = "stiluri"
        Return Path.Combine(OverridesFolder, safeName & ".json")
    End Function

    ''' <summary>
    ''' Scrie setul la calea dată (creând directorul). Curăță întâi intrările goale — un fișier
    ''' trebuie să conțină alegeri, nu inventarul ierarhiei.
    ''' </summary>
    Public Sub Save(styleSet As ThemeOverrideSet, filePath As String)
        If styleSet Is Nothing Then Throw New ArgumentNullException(NameOf(styleSet))
        If String.IsNullOrWhiteSpace(filePath) Then Throw New ArgumentException("Cale de fișier goală.", NameOf(filePath))
        Try
            styleSet.Prune()
            styleSet.SavedUtc = DateTime.UtcNow.ToString("o", Globalization.CultureInfo.InvariantCulture)

            Dim folder As String = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(folder) Then Directory.CreateDirectory(folder)

            File.WriteAllText(filePath, JsonSerializer.Serialize(styleSet, _jsonOptions), Text.Encoding.UTF8)
        Catch ex As Exception
            ' Frontieră de I/O: logăm ȘI rearuncăm — editorul trebuie să afișeze eșecul.
            GlobalErrorLog.Write("ThemeOverrideStore.Save", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Citește un set de la o cale explicită. Fișier inexistent ⇒ Nothing.</summary>
    Public Function LoadFile(filePath As String) As ThemeOverrideSet
        Try
            If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then Return Nothing
            Return JsonSerializer.Deserialize(Of ThemeOverrideSet)(File.ReadAllText(filePath), _jsonOptions)
        Catch ex As Exception
            GlobalErrorLog.Write($"ThemeOverrideStore.LoadFile({Path.GetFileName(If(filePath, String.Empty))})", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Toate seturile din …\AVACONT\Overrides. Un fișier corupt e SĂRIT + logat (nu rearuncă):
    ''' aceeași regulă ca la <c>ThemeStore.LoadUserSchemes</c> — un fișier stricat nu are voie să
    ''' facă restul invizibil.
    ''' </summary>
    Public Function LoadAll() As List(Of ThemeOverrideSet)
        Dim result As New List(Of ThemeOverrideSet)()
        Try
            If Not Directory.Exists(OverridesFolder) Then Return result
            For Each filePath As String In Directory.EnumerateFiles(OverridesFolder, "*.json")
                Try
                    Dim loaded As ThemeOverrideSet = JsonSerializer.Deserialize(Of ThemeOverrideSet)(
                        File.ReadAllText(filePath), _jsonOptions)
                    If loaded IsNot Nothing Then result.Add(loaded)
                Catch exFile As Exception
                    GlobalErrorLog.Write($"ThemeOverrideStore.LoadAll({Path.GetFileName(filePath)})", exFile)
                End Try
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeOverrideStore.LoadAll(enumerate)", ex)
        End Try
        Return result
    End Function

    ''' <summary>Numele de fișier fără caractere interzise de Windows.</summary>
    Public Function SanitizeFileName(raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then Return String.Empty
        Dim sb As New Text.StringBuilder()
        Dim invalid As Char() = Path.GetInvalidFileNameChars()
        For Each ch As Char In raw.Trim()
            sb.Append(If(Array.IndexOf(invalid, ch) >= 0, "_"c, ch))
        Next
        Return sb.ToString()
    End Function

End Module
