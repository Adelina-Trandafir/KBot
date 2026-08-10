Option Strict On
Imports System.Text.Json.Serialization

''' <summary>
''' Un fișier de suprascrieri: TOATE alegerile operatorului pentru o singură gazdă (un formular
''' sau o vedere), sub forma unei liste de <see cref="ControlStyleOverride"/> indexată pe cale.
'''
''' <see cref="Scope"/> e numele TIPULUI gazdei („MainForm”, „SumarView”), nu numele instanței:
''' vederile se creează leneș și se recreează, deci numele de instanță nu e stabil, dar tipul e.
''' Cititorul de la rulare (felia următoare — AICI NU SE CITEȘTE NIMIC LA RULARE) va potrivi
''' fișierul cu gazda pe <c>Scope</c>, apoi fiecare intrare pe <c>Path</c>.
''' </summary>
Public NotInheritable Class ThemeOverrideSet

    ''' <summary>Nume prietenos, ales de operator la salvare.</summary>
    Public Property Name As String = "Stiluri"

    ''' <summary>Numele tipului gazdei căreia i se aplică setul („MainForm”, „SumarView”…).</summary>
    Public Property Scope As String = String.Empty

    ''' <summary>Numele schemei active la momentul autorării — context, nu constrângere.</summary>
    Public Property BaseScheme As String = String.Empty

    ''' <summary>Momentul salvării, ISO 8601 UTC. Pur informativ.</summary>
    Public Property SavedUtc As String = String.Empty

    ''' <summary>Intrările, câte una pe control atins.</summary>
    Public Property Entries As List(Of ControlStyleOverride) = New List(Of ControlStyleOverride)()

    ''' <summary>Intrarea pentru calea dată, sau Nothing.</summary>
    Public Function Find(path As String) As ControlStyleOverride
        If String.IsNullOrEmpty(path) OrElse Entries Is Nothing Then Return Nothing
        For Each e In Entries
            If String.Equals(e.Path, path, StringComparison.Ordinal) Then Return e
        Next
        Return Nothing
    End Function

    ''' <summary>Intrarea pentru calea dată, creată dacă lipsește.</summary>
    Public Function GetOrCreate(path As String, typeName As String) As ControlStyleOverride
        Dim found As ControlStyleOverride = Find(path)
        If found IsNot Nothing Then Return found
        found = New ControlStyleOverride With {.Path = path, .TypeName = typeName}
        Entries.Add(found)
        Return found
    End Function

    ''' <summary>
    ''' Aruncă intrările fără nicio alegere. Rulează înainte de salvare: un fișier cu 200 de
    ''' intrări goale n-ar spune nimic despre ce a vrut operatorul.
    ''' </summary>
    Public Sub Prune()
        If Entries Is Nothing Then Return
        For i As Integer = Entries.Count - 1 To 0 Step -1
            If Entries(i) Is Nothing OrElse Entries(i).IsEmpty Then Entries.RemoveAt(i)
        Next
    End Sub

    ''' <summary>Câte controale au cel puțin o alegere.</summary>
    <JsonIgnore>
    Public ReadOnly Property TouchedCount As Integer
        Get
            If Entries Is Nothing Then Return 0
            Dim n As Integer = 0
            For Each e In Entries
                If e IsNot Nothing AndAlso Not e.IsEmpty Then n += 1
            Next
            Return n
        End Get
    End Property

End Class
