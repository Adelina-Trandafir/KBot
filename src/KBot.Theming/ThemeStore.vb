Imports System.IO
Imports System.Text.Json
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

    ''' <summary>Salvează numele schemei active. Eșecul se loghează, nu propagă.</summary>
    Public Sub SaveActive(schemeName As String)
        Try
            Directory.CreateDirectory(AppDataFolder)
            Dim cfg As New ActiveConfig With {.ActiveScheme = schemeName}
            Dim json As String = JsonSerializer.Serialize(cfg, _jsonOptions)
            File.WriteAllText(ActiveFilePath, json)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.SaveActive", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Citește numele schemei active persistate; Nothing dacă lipsește/corupt
    ''' (apelantul cade pe schema default documentată = Classic).
    ''' </summary>
    Public Function LoadActiveName() As String
        Try
            If Not File.Exists(ActiveFilePath) Then Return Nothing
            Dim json As String = File.ReadAllText(ActiveFilePath)
            Dim cfg As ActiveConfig = JsonSerializer.Deserialize(Of ActiveConfig)(json, _jsonOptions)
            Return If(cfg IsNot Nothing, cfg.ActiveScheme, Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("ThemeStore.LoadActiveName", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Încarcă toate schemele utilizator din …\AVACONT\Themes\*.json. Un fișier
    ''' malformat e SĂRIT + logat, nu oprește pornirea și nu contaminează restul.
    ''' </summary>
    Public Function LoadUserSchemes() As List(Of ThemeScheme)
        Dim result As New List(Of ThemeScheme)()
        Try
            If Not Directory.Exists(ThemesFolder) Then Return result
            For Each filePath As String In Directory.EnumerateFiles(ThemesFolder, "*.json")
                Try
                    Dim json As String = File.ReadAllText(filePath)
                    Dim scheme As ThemeScheme = JsonSerializer.Deserialize(Of ThemeScheme)(json, _jsonOptions)
                    If scheme IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(scheme.Name) Then
                        result.Add(scheme)
                    Else
                        GlobalErrorLog.Write("ThemeStore.LoadUserSchemes",
                            New InvalidDataException($"Schemă utilizator invalidă (nume gol): {filePath}"))
                    End If
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

    ''' <summary>Contract JSON minimal pentru theme.json — doar alegerea activă.</summary>
    Private NotInheritable Class ActiveConfig
        <JsonPropertyName("activeScheme")>
        Public Property ActiveScheme As String
    End Class

End Module
