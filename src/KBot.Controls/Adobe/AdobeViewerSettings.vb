Option Strict On
Imports KBot.Common

''' <summary>Ce a ieșit dintr-o citire a setării: valoarea folosită și, dacă e cazul, avertismentul.</summary>
Public NotInheritable Class AdobeSettingRead(Of T)

    Public ReadOnly Property Value As T
    ''' <summary>Text românesc pentru jurnal când valoarea stocată nu a putut fi interpretată; gol altfel.</summary>
    Public ReadOnly Property Warning As String

    Public Sub New(value As T, warning As String)
        Me.Value = value
        Me.Warning = If(warning, "")
    End Sub

    Public ReadOnly Property HasWarning As Boolean
        Get
            Return Warning.Length > 0
        End Get
    End Property

End Class

''' <summary>
''' Citirea și scrierea celor două setări ale gazdei Adobe, peste
''' <see cref="KBot.Common.KBotPaths"/> (<c>&lt;AppDir&gt;\kbot_paths.json</c>).
'''
''' Enumerările trăiesc aici, în KBot.Controls, fiindcă tot aici trăiește codul care le APLICĂ;
''' KBot.Common păstrează doar textul, altfel ar trebui să refere KBot.Controls (ciclu).
'''
''' REGULA DE CĂDERE, aceeași pentru ambele setări: valoare lipsă, goală sau nerecunoscută -&gt;
''' «Auto», cu un avertisment pentru jurnal. NICIODATĂ o excepție: o setare stricată nu are voie
''' să împiedice deschiderea unui document.
''' </summary>
Public NotInheritable Class AdobeViewerSettings

    Private Sub New()
    End Sub

    Public Const ModeAutoText As String = "Auto"
    Public Const ModeModernText As String = "Modern"
    Public Const ModeClassicText As String = "Classic"

    Public Const NewInstanceAutoText As String = "Auto"
    Public Const NewInstanceYesText As String = "Da"
    Public Const NewInstanceNoText As String = "Nu"

    ''' <summary>Textul stocat -&gt; profilul cerut. Necunoscut -&gt; Auto + avertisment.</summary>
    Public Shared Function ParseMode(stored As String) As AdobeSettingRead(Of AdobeViewerMode)
        If String.IsNullOrWhiteSpace(stored) Then
            Return New AdobeSettingRead(Of AdobeViewerMode)(AdobeViewerMode.Auto, "")
        End If
        Select Case stored.Trim().ToLowerInvariant()
            Case "auto", "automat"
                Return New AdobeSettingRead(Of AdobeViewerMode)(AdobeViewerMode.Auto, "")
            Case "modern"
                Return New AdobeSettingRead(Of AdobeViewerMode)(AdobeViewerMode.Modern, "")
            Case "classic", "clasic"
                Return New AdobeSettingRead(Of AdobeViewerMode)(AdobeViewerMode.Classic, "")
            Case Else
                Return New AdobeSettingRead(Of AdobeViewerMode)(
                    AdobeViewerMode.Auto,
                    $"Setarea «{KBotPathsKeys.AdobeViewerMode}» are valoarea nerecunoscută «{stored}» — " &
                    "se folosește «Auto». Valori acceptate: Auto, Modern, Classic.")
        End Select
    End Function

    ''' <summary>Textul stocat -&gt; forțarea «/n». Necunoscut -&gt; Auto + avertisment.</summary>
    Public Shared Function ParseNewInstance(stored As String) As AdobeSettingRead(Of AdobeNewInstanceMode)
        If String.IsNullOrWhiteSpace(stored) Then
            Return New AdobeSettingRead(Of AdobeNewInstanceMode)(AdobeNewInstanceMode.Auto, "")
        End If
        Select Case stored.Trim().ToLowerInvariant()
            Case "auto", "automat"
                Return New AdobeSettingRead(Of AdobeNewInstanceMode)(AdobeNewInstanceMode.Auto, "")
            Case "da", "yes", "true", "1"
                Return New AdobeSettingRead(Of AdobeNewInstanceMode)(AdobeNewInstanceMode.Da, "")
            Case "nu", "no", "false", "0"
                Return New AdobeSettingRead(Of AdobeNewInstanceMode)(AdobeNewInstanceMode.Nu, "")
            Case Else
                Return New AdobeSettingRead(Of AdobeNewInstanceMode)(
                    AdobeNewInstanceMode.Auto,
                    $"Setarea «{KBotPathsKeys.AdobeNewInstance}» are valoarea nerecunoscută «{stored}» — " &
                    "se folosește «Auto». Valori acceptate: Auto, Da, Nu.")
        End Select
    End Function

    ''' <summary>Valoarea de scris în fișier pentru un profil.</summary>
    Public Shared Function ModeToText(mode As AdobeViewerMode) As String
        Select Case mode
            Case AdobeViewerMode.Modern : Return ModeModernText
            Case AdobeViewerMode.Classic : Return ModeClassicText
            Case Else : Return ModeAutoText
        End Select
    End Function

    ''' <summary>Valoarea de scris în fișier pentru forțarea «/n».</summary>
    Public Shared Function NewInstanceToText(mode As AdobeNewInstanceMode) As String
        Select Case mode
            Case AdobeNewInstanceMode.Da : Return NewInstanceYesText
            Case AdobeNewInstanceMode.Nu : Return NewInstanceNoText
            Case Else : Return NewInstanceAutoText
        End Select
    End Function

    ''' <summary>Eticheta românească din combo, pentru fiecare profil.</summary>
    Public Shared Function ModeLabel(mode As AdobeViewerMode) As String
        Return AdobeProfileChoice.ModeLabel(mode)
    End Function

    ''' <summary>Eticheta românească din combo, pentru forțarea «/n».</summary>
    Public Shared Function NewInstanceLabel(mode As AdobeNewInstanceMode) As String
        Select Case mode
            Case AdobeNewInstanceMode.Da : Return "Da"
            Case AdobeNewInstanceMode.Nu : Return "Nu"
            Case Else : Return "Automat"
        End Select
    End Function

    ''' <summary>Profilul cerut acum, din setările încărcate.</summary>
    Public Shared Function CurrentMode() As AdobeSettingRead(Of AdobeViewerMode)
        Return ParseMode(KBotPaths.Current.AdobeViewerMode)
    End Function

    ''' <summary>Forțarea «/n» cerută acum, din setările încărcate.</summary>
    Public Shared Function CurrentNewInstance() As AdobeSettingRead(Of AdobeNewInstanceMode)
        Return ParseNewInstance(KBotPaths.Current.AdobeNewInstance)
    End Function

    ''' <summary>
    ''' Persistă ambele setări. Întoarce False când fișierul nu a putut fi scris (setarea rămâne
    ''' activă pentru sesiunea curentă — vezi <see cref="KBot.Common.KBotPaths.Save"/>).
    ''' </summary>
    Public Shared Function Persist(mode As AdobeViewerMode, newInstance As AdobeNewInstanceMode) As Boolean
        Dim current As KBotPaths = KBotPaths.Current
        current.AdobeViewerMode = ModeToText(mode)
        current.AdobeNewInstance = NewInstanceToText(newInstance)
        Return current.Save()
    End Function

End Class

''' <summary>Numele cheilor JSON, pentru mesaje care trebuie să numească exact setarea stricată.</summary>
Public NotInheritable Class KBotPathsKeys

    Private Sub New()
    End Sub

    Public Const AdobeViewerMode As String = "AdobeViewerMode"
    Public Const AdobeNewInstance As String = "AdobeNewInstance"

End Class
