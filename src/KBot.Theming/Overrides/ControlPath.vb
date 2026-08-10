Option Strict On
Imports System.Text
Imports System.Windows.Forms

''' <summary>
''' Identitatea unui control în ierarhia gazdei sale, ca șir stabil între rulări: numele
''' controalelor de pe lanțul rădăcină→control, separate prin „/”. Rădăcina NU intră în cale
''' (ea e <c>ThemeOverrideSet.Scope</c>), deci calea unui control direct pe formular e chiar
''' numele lui.
'''
''' De ce nume și nu indici: indicii se schimbă la orice reordonare din designer, numele nu —
''' iar în K-BOT fiecare control e declarat în .Designer.vb (regula casei), deci are nume. Pentru
''' controalele fără nume (copii interni, creați în cod) cădem pe „{Tip}[{index}]”, care e stabil
''' cât timp nu se schimbă ordinea de adăugare; sunt oricum cazuri pe care editorul nu le arată.
'''
''' Pur funcțional, fără stare — testabil fără UI.
''' </summary>
Public Module ControlPath

    Public Const Separator As Char = "/"c

    ''' <summary>
    ''' Calea lui <paramref name="ctrl"/> relativ la <paramref name="root"/>. String gol dacă
    ''' e chiar rădăcina; Nothing dacă nu e descendentul ei.
    ''' </summary>
    Public Function Build(root As Control, ctrl As Control) As String
        If root Is Nothing OrElse ctrl Is Nothing Then Return Nothing
        If ReferenceEquals(root, ctrl) Then Return String.Empty

        Dim segments As New List(Of String)()
        Dim node As Control = ctrl
        While node IsNot Nothing AndAlso Not ReferenceEquals(node, root)
            segments.Add(SegmentOf(node))
            node = node.Parent
        End While

        ' Am ieșit fără să dăm de rădăcină ⇒ controlul nu e sub ea.
        If node Is Nothing Then Return Nothing

        segments.Reverse()
        Dim sb As New StringBuilder()
        For i As Integer = 0 To segments.Count - 1
            If i > 0 Then sb.Append(Separator)
            sb.Append(segments(i))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Drumul invers: controlul de la calea dată sub <paramref name="root"/>, sau Nothing.
    ''' Îl folosește cititorul de la rulare (felia următoare) ca să lege fișierul de ierarhie.
    ''' </summary>
    Public Function Resolve(root As Control, path As String) As Control
        If root Is Nothing Then Return Nothing
        If String.IsNullOrEmpty(path) Then Return root

        Dim node As Control = root
        For Each segment As String In path.Split(Separator)
            Dim [next] As Control = FindChild(node, segment)
            If [next] Is Nothing Then Return Nothing
            node = [next]
        Next
        Return node
    End Function

    ''' <summary>
    ''' Segmentul unui control: numele lui, sau „{Tip}[{index}]” dacă nu are nume. Ghilimelele
    ''' pătrate fac forma de fallback imposibil de confundat cu un nume real.
    ''' </summary>
    Public Function SegmentOf(ctrl As Control) As String
        If ctrl Is Nothing Then Return String.Empty
        If Not String.IsNullOrWhiteSpace(ctrl.Name) Then Return ctrl.Name

        Dim parent As Control = ctrl.Parent
        Dim idx As Integer = If(parent Is Nothing, 0, parent.Controls.IndexOf(ctrl))
        Return $"{ctrl.GetType().Name}[{idx}]"
    End Function

    Private Function FindChild(parent As Control, segment As String) As Control
        If parent Is Nothing OrElse String.IsNullOrEmpty(segment) Then Return Nothing
        For Each child As Control In parent.Controls
            If String.Equals(SegmentOf(child), segment, StringComparison.Ordinal) Then Return child
        Next
        Return Nothing
    End Function

End Module
