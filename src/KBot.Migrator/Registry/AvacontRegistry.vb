Imports System.Collections.Generic
Imports System.IO
Imports Microsoft.Win32
Imports KBot.Common

''' <summary>
''' O unitate așa cum e înregistrată de AVACONT în registrul Windows. POCO.
''' </summary>
Public NotInheritable Class AvacontDc

    ''' <summary>Numele cheii = numele bazei de unitate: 000_DEMO, 005_CEVM…</summary>
    Public Property Dc As String

    Public Property NumeUnitate As String
    Public Property CodFiscal As String

    ''' <summary>«CaleUnitate» — rădăcina instalării (de regulă C:\AVACONT).</summary>
    Public Property CaleUnitate As String

    ''' <summary>Anii declarați în subcheile de tip ISJ / LOCAL / REPUBLICAN, reuniți.</summary>
    Public ReadOnly Property Ani As New List(Of String)()

    Public Overrides Function ToString() As String
        If String.IsNullOrWhiteSpace(NumeUnitate) Then Return Dc
        Return Dc & " — " & NumeUnitate
    End Function

End Class

''' <summary>
''' Citește unitățile din
''' <c>HKCU\Software\VB and VBA Program Settings\AVACONT</c>, ramura pe care o
''' scriu aplicațiile VB6/VBA prin <c>SaveSetting</c>.
'''
''' De ce de aici și nu dintr-un fișier: e singurul loc în care ordinea DC-urilor
''' instalate pe stația operatorului chiar există. Migratorul nu scrie NIMIC în
''' registru — doar citește.
'''
''' Metodă de graniță (registru): logăm și RE-ARUNCĂM.
''' </summary>
Public Module AvacontRegistry

    Public Const RootPath As String = "Software\VB and VBA Program Settings\AVACONT"

    ''' <summary>Subcheile care nu descriu o bază de date, ci altceva.</summary>
    Private ReadOnly NonDataKeys As String() = {"Tokens"}

    ''' <summary>
    ''' Unitățile găsite, sortate după DC. Lista goală e un răspuns valid (AVACONT
    ''' nu e instalat pe stația asta), nu o eroare.
    ''' </summary>
    Public Function ReadDcs() As List(Of AvacontDc)
        Try
            Dim result As New List(Of AvacontDc)()

            Using root As RegistryKey = Registry.CurrentUser.OpenSubKey(RootPath, False)
                If root Is Nothing Then Return result

                For Each name As String In root.GetSubKeyNames()
                    Using unit As RegistryKey = root.OpenSubKey(name, False)
                        If unit Is Nothing Then Continue For

                        Dim dc As New AvacontDc() With {
                            .Dc = name,
                            .NumeUnitate = ReadString(unit, "NumeUnitate"),
                            .CodFiscal = ReadString(unit, "CodFiscal"),
                            .CaleUnitate = ReadString(unit, "CaleUnitate")
                        }

                        ' Anii stau in subchei numite dupa sursa de finantare
                        ' (ISJ, LOCAL, REPUBLICAN...), nu intr-un loc fix.
                        For Each child As String In unit.GetSubKeyNames()
                            If Array.IndexOf(NonDataKeys, child) >= 0 Then Continue For
                            Using branch As RegistryKey = unit.OpenSubKey(child, False)
                                If branch Is Nothing Then Continue For
                                For Each an As String In SplitAni(ReadString(branch, "Ani"))
                                    If Not dc.Ani.Contains(an) Then dc.Ani.Add(an)
                                Next
                            End Using
                        Next

                        dc.Ani.Sort()
                        result.Add(dc)
                    End Using
                Next
            End Using

            result.Sort(Function(a, b) String.Compare(a.Dc, b.Dc, StringComparison.OrdinalIgnoreCase))
            Return result

        Catch ex As Exception
            GlobalErrorLog.Write("AvacontRegistry.ReadDcs", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Unde se așteaptă să stea fișierul FOREXE al anului, pentru unitatea dată:
    ''' <c>&lt;CaleUnitate&gt;\forexe\FX_&lt;an&gt;.accdb</c>.
    '''
    ''' E o SUGESTIE, nu un adevăr: operatorul o poate schimba în formular. Calea
    ''' reală poate diferi (fișiere per unitate de tipul FX_2026_gr35.accdb).
    ''' </summary>
    Public Function SuggestFxPath(dc As AvacontDc, an As String) As String
        Try
            If dc Is Nothing OrElse String.IsNullOrWhiteSpace(dc.CaleUnitate) Then Return ""
            Return Path.Combine(dc.CaleUnitate, "forexe", "FX_" & an & ".accdb")
        Catch ex As Exception
            GlobalErrorLog.Write("AvacontRegistry.SuggestFxPath", ex)
            Throw
        End Try
    End Function

    Private Function ReadString(key As RegistryKey, name As String) As String
        Dim value As Object = key.GetValue(name, Nothing)
        Return If(value Is Nothing, "", value.ToString().Trim())
    End Function

    ''' <summary>«2025;2026» → {"2025","2026"}. Separatorul e cel scris de VB6.</summary>
    Private Function SplitAni(raw As String) As List(Of String)
        Dim ani As New List(Of String)()
        If String.IsNullOrWhiteSpace(raw) Then Return ani
        Dim parsed As Integer
        For Each part As String In raw.Split(";"c, ","c)
            Dim an As String = part.Trim()
            If an.Length = 4 AndAlso Integer.TryParse(an, parsed) Then ani.Add(an)
        Next
        Return ani
    End Function

End Module
