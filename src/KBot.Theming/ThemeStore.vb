Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.Json.Serialization
Imports KBot.Common

''' <summary>
''' Persistență + descoperire scheme (JSON în %AppData%\AVACONT). Stochează DOAR
''' alegerea (numele schemei active), nu schema întreagă. Schemele utilizator (pentru
''' editorul viitor) trăiesc ca fișiere complete în …\AVACONT\Themes\*.json. Orice I/O
''' e învelit în Try/Catch care LOGHează (niciodată catch gol) și cade elegant.
''' </summary>
Public Module ThemeStore

    Private Const AppFolderName As String = "AVACONT"
    Private Const ThemesSubfolder As String = "Themes"
    Private Const ActiveFileName As String = "theme.json"

    Private ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .PropertyNameCaseInsensitive = True
    }

    ' Rădăcină alternativă pentru teste (înlocuiește %AppData%). Nothing în producție.
    Private _overrideRoot As String = Nothing

    ''' <summary>Doar pentru teste: redirijează rădăcina AVACONT către un director temporar.</summary>
    Friend Property OverrideRootForTests As String
        Get
            Return _overrideRoot
        End Get
        Set(value As String)
            _overrideRoot = value
        End Set
    End Property

    ''' <summary>…\AVACONT (sau rădăcina de test, dacă e setată)</summary>
    Public ReadOnly Property AppDataFolder As String
        Get
            Dim root As String = If(_overrideRoot,
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
            Return Path.Combine(root, AppFolderName)
        End Get
    End Property

    ''' <summary>…\AVACONT\theme.json</summary>
    Public ReadOnly Property ActiveFilePath As String
        Get
            Return Path.Combine(AppDataFolder, ActiveFileName)
        End Get
    End Property

    ''' <summary>…\AVACONT\Themes</summary>
    Public ReadOnly Property ThemesFolder As String
        Get
            Return Path.Combine(AppDataFolder, ThemesSubfolder)
        End Get
    End Property

    ''' <summary>
    ''' Citește theme.json ÎNTREG. Fișierul ține de la felia 0036 două lucruri fără legătură între
    ''' ele — schema activă și setările de scalare — iar cele două se scriu din locuri diferite;
    ''' de aceea fiecare scriere trece prin citire-modificare-scriere, ca <c>SaveActive</c> să nu
    ''' calce peste scalare și invers. Nothing = fișier lipsă sau corupt.
    ''' </summary>
    Private Function LoadConfig() As ActiveConfig
        Try
            If Not File.Exists(ActiveFilePath) Then Return Nothing
            Dim json As String = File.ReadAllText(ActiveFilePath)
            Return JsonSerializer.Deserialize(Of ActiveConfig)(json, _jsonOptions)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.LoadConfig", ex)
            Return Nothing
        End Try
    End Function

    Private Sub SaveConfig(cfg As ActiveConfig)
        Try
            Directory.CreateDirectory(AppDataFolder)
            Dim json As String = JsonSerializer.Serialize(cfg, _jsonOptions)
            File.WriteAllText(ActiveFilePath, json)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.SaveConfig", ex)
        End Try
    End Sub

    ''' <summary>Salvează numele schemei active, PĂSTRÂND setările de scalare. Eșecul se loghează, nu propagă.</summary>
    Public Sub SaveActive(schemeName As String)
        Dim cfg As ActiveConfig = If(LoadConfig(), New ActiveConfig())
        cfg.ActiveScheme = schemeName
        SaveConfig(cfg)
    End Sub

    ''' <summary>
    ''' Citește numele schemei active persistate; Nothing dacă lipsește/corupt
    ''' (apelantul cade pe schema default documentată = Classic).
    ''' </summary>
    Public Function LoadActiveName() As String
        Dim cfg As ActiveConfig = LoadConfig()
        Return If(cfg IsNot Nothing, cfg.ActiveScheme, Nothing)
    End Function

    ''' <summary>Salvează setările de scalare, PĂSTRÂND numele schemei active.</summary>
    Public Sub SaveScaling(mode As ScalingMode, manualFactor As Single, dpiUnaware As Boolean,
                           textScale As Single)
        Dim cfg As ActiveConfig = If(LoadConfig(), New ActiveConfig())
        cfg.ScalingMode = CInt(mode)
        cfg.ScalingFactor = manualFactor
        cfg.DpiUnaware = dpiUnaware
        cfg.TextScale = textScale
        SaveConfig(cfg)
    End Sub

    ''' <summary>
    ''' Duce setările de scalare persistate în <see cref="AppScaling"/>. Un fișier lipsă sau
    ''' corupt lasă implicitele (automat, factor 1, conștient de DPI) — adică EXACT
    ''' comportamentul dinaintea feliei 0036, care e și cel corect pentru un operator care n-a
    ''' atins niciodată setarea.
    ''' </summary>
    Public Sub LoadScaling()
        Dim cfg As ActiveConfig = LoadConfig()
        If cfg Is Nothing Then Return
        Dim mode As ScalingMode = ScalingMode.Automatic
        If [Enum].IsDefined(GetType(ScalingMode), cfg.ScalingMode) Then
            mode = CType(cfg.ScalingMode, ScalingMode)
        Else
            GlobalErrorLog.Write("ThemeStore.LoadScaling",
                New InvalidDataException($"Mod de scalare necunoscut în theme.json: {cfg.ScalingMode}. Se folosește «automat»."))
        End If
        AppScaling.LoadFrom(mode, cfg.ScalingFactor, cfg.DpiUnaware, cfg.TextScale)
    End Sub

    ''' <summary>
    ''' Scrie o schemă ÎNTREAGĂ în …\AVACONT\Themes\&lt;Nume&gt;.json. Așa se persistă și editarea
    ''' unei scheme built-in: fișierul are numele ei, iar <c>ThemeManager</c> îl pune PESTE cea
    ''' compilată la pornire (vezi <c>MergeSchemes</c>). Ștergerea fișierului readuce implicitul —
    ''' de aceea nu se scrie niciodată nimic peste codul sursă.
    '''
    ''' Metodă de frontieră (I/O): loghează ȘI aruncă — cel care a apăsat «Salvează» trebuie să
    ''' afle dacă n-a mers.
    ''' </summary>
    Public Sub SaveScheme(scheme As ThemeScheme)
        If scheme Is Nothing Then Throw New ArgumentNullException(NameOf(scheme))
        If String.IsNullOrWhiteSpace(scheme.Name) Then Throw New ArgumentException(
            "O schemă fără nume nu poate fi salvată — numele e cheia fișierului.", NameOf(scheme))
        Try
            Directory.CreateDirectory(ThemesFolder)
            Dim json As String = JsonSerializer.Serialize(scheme, _jsonOptions)
            File.WriteAllText(SchemeFilePath(scheme.Name), json)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.SaveScheme", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Șterge fișierul unei scheme. Întoarce True dacă a existat ceva de șters — un False nu e o
    ''' eroare, e răspunsul la «readu implicitul» pentru o schemă care n-a fost niciodată editată.
    ''' </summary>
    Public Function DeleteScheme(schemeName As String) As Boolean
        If String.IsNullOrWhiteSpace(schemeName) Then Return False
        Try
            Dim path As String = SchemeFilePath(schemeName)
            If Not File.Exists(path) Then Return False
            File.Delete(path)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.DeleteScheme", ex)
            Throw
        End Try
    End Function

    ''' <summary>…\AVACONT\Themes\&lt;Nume&gt;.json — numele curățat de ce n-are voie într-un nume de fișier.</summary>
    Public Function SchemeFilePath(schemeName As String) As String
        Dim safe As String = schemeName.Trim()
        For Each bad As Char In Path.GetInvalidFileNameChars()
            safe = safe.Replace(bad, "_"c)
        Next
        Return Path.Combine(ThemesFolder, safe & ".json")
    End Function

    ''' <summary>
    ''' Loads every user scheme from …\AVACONT\Themes\*.json. A malformed file is SKIPPED and
    ''' logged; it never blocks startup and never contaminates the rest.
    '''
    ''' <para><b><paramref name="builtInFor"/> is what keeps an old file from freezing the
    ''' application in the past</b> (slice 0049). When a stored file carries the name of a built-in
    ''' scheme it REPLACES that scheme — that is how editing «Modern» is persisted. But a file
    ''' written by an older build knows nothing about any key added since, so a straight
    ''' deserialize hands every one of them the type's default: a saved «Modern» from before this
    ''' slice silently switched off every card, the shipped font and all four row heights, and the
    ''' application looked *exactly* as it had before — with no error anywhere to say why.</para>
    '''
    ''' <para>So the stored file is OVERLAID on the compiled scheme instead: what the file actually
    ''' says wins, key by key, and everything it does not mention keeps the value compiled in.
    ''' The operator's edits survive; keys invented after they saved arrive with their new
    ''' defaults. Pass <c>Nothing</c> to get the old replace-wholesale behaviour.</para>
    ''' </summary>
    ''' <param name="builtInFor">
    ''' Returns the compiled scheme for a name, or <c>Nothing</c> when the name is not a built-in
    ''' (a scheme the operator invented has nothing to be overlaid on, so it is read as it stands).
    ''' </param>
    Public Function LoadUserSchemes(Optional builtInFor As Func(Of String, ThemeScheme) = Nothing) As List(Of ThemeScheme)
        Dim result As New List(Of ThemeScheme)()
        Try
            If Not Directory.Exists(ThemesFolder) Then Return result
            For Each filePath As String In Directory.EnumerateFiles(ThemesFolder, "*.json")
                Try
                    Dim json As String = File.ReadAllText(filePath)
                    Dim scheme As ThemeScheme = JsonSerializer.Deserialize(Of ThemeScheme)(json, _jsonOptions)
                    If scheme Is Nothing OrElse String.IsNullOrWhiteSpace(scheme.Name) Then
                        GlobalErrorLog.Write("ThemeStore.LoadUserSchemes",
                            New InvalidDataException($"Schemă utilizator invalidă (nume gol): {filePath}"))
                        Continue For
                    End If

                    Dim compiled As ThemeScheme = If(builtInFor IsNot Nothing, builtInFor(scheme.Name), Nothing)
                    If compiled IsNot Nothing Then
                        Dim merged As ThemeScheme = OverlayOnto(compiled, json)
                        If merged IsNot Nothing Then scheme = merged
                    End If

                    result.Add(scheme)
                Catch exFile As Exception
                    ' Un fișier corupt nu blochează restul; logăm și continuăm.
                    GlobalErrorLog.Write($"ThemeStore.LoadUserSchemes({Path.GetFileName(filePath)})", exFile)
                End Try
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.LoadUserSchemes(enumerate)", ex)
        End Try
        Return result
    End Function

    ''' <summary>
    ''' The compiled scheme with the stored file's values written over it, key by key and nesting
    ''' level by nesting level. Anything the file does not mention keeps the compiled value.
    '''
    ''' <para>Deliberately generic — it walks the JSON rather than naming properties — so a key
    ''' added to <see cref="ThemePalette"/> or <see cref="ThemeStyleOptions"/> in some future slice
    ''' is carried through without anybody remembering to come back here.</para>
    '''
    ''' <para>Returns <c>Nothing</c> if the overlay cannot be built, so the caller keeps whatever
    ''' it already had rather than losing the operator's file over a parsing problem.</para>
    ''' </summary>
    Friend Function OverlayOnto(compiled As ThemeScheme, storedJson As String) As ThemeScheme
        If compiled Is Nothing OrElse String.IsNullOrWhiteSpace(storedJson) Then Return Nothing
        Try
            Dim baseNode As JsonObject = TryCast(JsonSerializer.SerializeToNode(compiled, _jsonOptions), JsonObject)
            Dim storedNode As JsonObject = TryCast(JsonNode.Parse(storedJson), JsonObject)
            If baseNode Is Nothing OrElse storedNode Is Nothing Then Return Nothing

            Overlay(baseNode, storedNode)
            Return baseNode.Deserialize(Of ThemeScheme)(_jsonOptions)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.OverlayOnto", ex)
            Return Nothing
        End Try
    End Function

    ' Writes source over target in place. An object meets an object => descend; anything else =>
    ' the source value replaces the target one outright. A JsonNode cannot have two parents, hence
    ' the DeepClone on every value that crosses over.
    Private Sub Overlay(target As JsonObject, source As JsonObject)
        For Each pair As KeyValuePair(Of String, JsonNode) In source
            Dim key As String = MatchingKey(target, pair.Key)
            Dim sourceObj As JsonObject = TryCast(pair.Value, JsonObject)
            Dim targetObj As JsonObject = If(key Is Nothing, Nothing, TryCast(target(key), JsonObject))

            If sourceObj IsNot Nothing AndAlso targetObj IsNot Nothing Then
                Overlay(targetObj, sourceObj)
            Else
                target(If(key, pair.Key)) = If(pair.Value Is Nothing, Nothing, pair.Value.DeepClone())
            End If
        Next
    End Sub

    ' The key as TARGET spells it, so an overlay never ends up with "surface" beside "Surface".
    ' Deserialization is case-insensitive, but two spellings of one key in the same object is a
    ' coin toss over which one lands.
    Private Function MatchingKey(target As JsonObject, key As String) As String
        If target.ContainsKey(key) Then Return key
        For Each pair As KeyValuePair(Of String, JsonNode) In target
            If String.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase) Then Return pair.Key
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Contract JSON pentru theme.json: alegerea activă + scalarea (felia 0036).
    '''
    ''' Scalarea stă AICI, lângă schema activă, nu în <c>ThemeStyleOptions</c>: e o proprietate a
    ''' ECRANULUI pe care lucrează operatorul, nu a temei. Pusă în schemă, o trecere de la
    ''' «Modern» la «Întunecat» ar redimensiona tăcut toată aplicația.
    '''
    ''' Valorile implicite sunt exact comportamentul dinaintea feliei: automat, factor 1,
    ''' conștient de DPI. Un theme.json vechi (fără câmpurile noi) le primește pe astea.
    ''' </summary>
    Private NotInheritable Class ActiveConfig
        <JsonPropertyName("activeScheme")>
        Public Property ActiveScheme As String

        <JsonPropertyName("scalingMode")>
        Public Property ScalingMode As Integer = 0

        <JsonPropertyName("scalingFactor")>
        Public Property ScalingFactor As Single = 1.0F

        <JsonPropertyName("dpiUnaware")>
        Public Property DpiUnaware As Boolean = False

        ''' <summary>Mărimea textului și a controalelor (1 = 100%). Felia 0036-01.</summary>
        <JsonPropertyName("textScale")>
        Public Property TextScale As Single = 1.0F
    End Class

End Module
