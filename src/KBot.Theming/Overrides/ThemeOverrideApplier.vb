Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Aplică un <see cref="ThemeOverrideSet"/> peste o ierarhie de controale. E singurul loc care
''' știe CUM se traduce un slot din fișier într-o proprietate reală de control — atât editorul
''' (previzualizare live, butonul «Încarcă»), cât și viitorul cititor de la pornire trec pe aici,
''' ca să nu existe două tabele de corespondență care se pot desincroniza.
'''
''' <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c> există pe orice <see cref="Control"/>. Restul
''' sloturilor (hover, contur, accent, selecție) NU: sunt proprietăți ale controalelor K-BOT, cu
''' nume care diferă de la o familie la alta (<c>HoverColor</c> la KBotComboBox,
''' <c>HoverBackColor</c> la AdvancedTreeControl). De aceea potrivirea se face pe o LISTĂ de
''' nume candidate, în ordinea preferinței, prin <see cref="TypeDescriptor"/> — aceeași reflexie
''' pe care o folosește și PropertyGrid-ul, deci ce vede editorul e ce se aplică.
'''
''' Un slot fără proprietate potrivită pe controlul-țintă rămâne în fișier fără efect. Deliberat:
''' un Button system de azi poate deveni un KBot* mâine, iar ștergerea alegerii ar fi o pierdere
''' tăcută de date.
'''
''' NOTĂ DE FELIE (0028): nimic din modulul ăsta nu e apelat în calea de pornire a aplicației.
''' Aplicarea app-wide la rulare e felia următoare, prin cerința explicită a operatorului.
''' </summary>
Public Module ThemeOverrideApplier

    ''' <summary>Nume candidate pentru «culoarea de hover», în ordinea preferinței.</summary>
    Public ReadOnly HoverColorNames As String() = {"HoverColor", "HoverBackColor"}

    ''' <summary>Nume candidate pentru «culoarea conturului».</summary>
    Public ReadOnly BorderColorNames As String() = {"BorderColor"}

    ''' <summary>Nume candidate pentru «culoarea de accent».</summary>
    Public ReadOnly AccentColorNames As String() = {"AccentColor", "SelectedBorderColor"}

    ''' <summary>Nume candidate pentru «fundalul selecției».</summary>
    Public ReadOnly SelectionBackColorNames As String() = {"SelectionBackColor", "SelectedBackColor"}

    ''' <summary>Nume candidate pentru «textul selecției».</summary>
    Public ReadOnly SelectionForeColorNames As String() = {"SelectionForeColor", "SelectedForeColor"}

    ''' <summary>
    ''' Aplică toate intrările setului sub <paramref name="root"/>. Întoarce câte intrări au găsit
    ''' un control (cele care nu-l găsesc sunt sărite tăcut: ierarhia se poate să fi evoluat de la
    ''' salvare, iar asta nu e o eroare de rulare).
    ''' </summary>
    Public Function Apply(root As Control, styleSet As ThemeOverrideSet) As Integer
        If root Is Nothing OrElse styleSet Is Nothing OrElse styleSet.Entries Is Nothing Then Return 0
        Dim applied As Integer = 0
        Try
            root.SuspendLayout()
            Try
                For Each entry As ControlStyleOverride In styleSet.Entries
                    If entry Is Nothing Then Continue For
                    Dim target As Control = ControlPath.Resolve(root, entry.Path)
                    If target Is Nothing Then Continue For
                    ApplyEntry(target, entry)
                    applied += 1
                Next
            Finally
                root.ResumeLayout(True)
            End Try
        Catch ex As Exception
            ' Frontieră: cine cere aplicarea trebuie să afle că a eșuat (editorul afișează mesajul).
            GlobalErrorLog.Write("ThemeOverrideApplier.Apply", ex)
            Throw
        End Try
        Return applied
    End Function

    ''' <summary>Aplică o singură intrare pe un control. Sloturile neatinse nu se scriu.</summary>
    Public Sub ApplyEntry(ctrl As Control, entry As ControlStyleOverride)
        If ctrl Is Nothing OrElse entry Is Nothing Then Return

        Dim back As Color = ControlStyleOverride.ToColor(entry.BackColor)
        If back <> Color.Empty Then ctrl.BackColor = back

        Dim fore As Color = ControlStyleOverride.ToColor(entry.ForeColor)
        If fore <> Color.Empty Then ctrl.ForeColor = fore

        Dim f As Font = entry.ToFont()
        If f IsNot Nothing Then ctrl.Font = f

        TrySetColor(ctrl, HoverColorNames, ControlStyleOverride.ToColor(entry.HoverColor))
        TrySetColor(ctrl, BorderColorNames, ControlStyleOverride.ToColor(entry.BorderColor))
        TrySetColor(ctrl, AccentColorNames, ControlStyleOverride.ToColor(entry.AccentColor))
        TrySetColor(ctrl, SelectionBackColorNames, ControlStyleOverride.ToColor(entry.SelectionBackColor))
        TrySetColor(ctrl, SelectionForeColorNames, ControlStyleOverride.ToColor(entry.SelectionForeColor))

        ctrl.Invalidate()
    End Sub

    ''' <summary>
    ''' Prima proprietate <see cref="Color"/> scriibilă cu unul dintre numele date, sau Nothing
    ''' dacă niciunul nu există pe control.
    ''' </summary>
    Public Function FindColorProperty(ctrl As Control, candidates As String()) As PropertyDescriptor
        If ctrl Is Nothing OrElse candidates Is Nothing Then Return Nothing
        Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(ctrl)
        For Each name As String In candidates
            Dim pd As PropertyDescriptor = props(name)
            If pd IsNot Nothing AndAlso Not pd.IsReadOnly AndAlso pd.PropertyType Is GetType(Color) Then
                Return pd
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Citește slotul de pe control (<c>Color.Empty</c> dacă nu-l expune) — folosit de editor ca
    ''' să arate valoarea curentă, nu una inventată.
    ''' </summary>
    Public Function ReadColor(ctrl As Control, candidates As String()) As Color
        Dim pd As PropertyDescriptor = FindColorProperty(ctrl, candidates)
        If pd Is Nothing Then Return Color.Empty
        Dim value As Object = pd.GetValue(ctrl)
        If value Is Nothing Then Return Color.Empty
        Return CType(value, Color)
    End Function

    ''' <summary>
    ''' Scrie slotul dacă există și culoarea nu e goală. Întoarce numele proprietății scrise, sau
    ''' Nothing dacă nu s-a scris nimic — apelantul poate spune operatorului ce s-a aplicat.
    ''' </summary>
    Public Function TrySetColor(ctrl As Control, candidates As String(), value As Color) As String
        If value = Color.Empty Then Return Nothing
        Dim pd As PropertyDescriptor = FindColorProperty(ctrl, candidates)
        If pd Is Nothing Then Return Nothing
        pd.SetValue(ctrl, value)
        Return pd.Name
    End Function

End Module
