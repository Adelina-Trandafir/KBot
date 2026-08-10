Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Memoria valorilor AUTORITE ÎN DESIGNER pentru fiecare control atins de motorul de teme.
'''
''' De ce există: o temă SCRIE <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c> pe control. După
''' prima aplicare, valoarea pusă de operator în designer nu mai există nicăieri în proces — s-a
''' pierdut. Schema «Colorful» (felia 0028) promite exact invers: să păstreze culorile din
''' designer. Ca să poată, cineva trebuie să le fi reținut ÎNAINTE ca prima temă să scrie peste
''' ele. Ăsta e locul.
'''
''' Contractul, în trei puncte:
'''  1. <see cref="Capture"/> e apelată de <c>ThemeManager.Traverse</c> ca PRIMĂ operație pe
'''     fiecare control, deci instantaneul e luat înainte de orice scriere a temei. E idempotentă:
'''     al doilea apel pe același control nu suprascrie primul instantaneu.
'''  2. Nu reținem doar VALOAREA, ci și dacă proprietatea a fost cu adevărat SETATĂ — prin
'''     <c>TypeDescriptor…ShouldSerializeValue</c>, exact calea pe care merge și Visual Studio
'''     când decide ce scrie în .Designer.vb. Distincția e obligatorie pentru <c>Font</c>: un
'''     control care moștenește fontul ambiant raportează un Font valid, dar a-l scrie înapoi
'''     l-ar FIXA și l-ar face surd la <c>ApplyBaseFont</c>. Nesetat ⇒ restaurăm prin
'''     <c>Reset*</c>, nu prin atribuire.
'''  3. Tabelul e <see cref="ConditionalWeakTable(Of TKey, TValue)"/>: un control închis (vedere
'''     aruncată, formular închis) nu e ținut în viață de temă.
''' </summary>
Public Module DesignerBaseline

    ''' <summary>Cele trei proprietăți ambientale + steagul «chiar a fost setată».</summary>
    Private NotInheritable Class Snapshot
        Public BackColor As Color
        Public ForeColor As Color
        Public Font As Font
        Public HasBackColor As Boolean
        Public HasForeColor As Boolean
        Public HasFont As Boolean
    End Class

    Private ReadOnly _snapshots As New ConditionalWeakTable(Of Control, Snapshot)()

    ''' <summary>
    ''' Reține valorile curente ale controlului, dacă nu au fost deja reținute. Apelată ÎNAINTE
    ''' de orice stilizare — vezi punctul 1 din rezumatul de clasă.
    ''' </summary>
    Public Sub Capture(ctrl As Control)
        If ctrl Is Nothing Then Return
        Try
            Dim existing As Snapshot = Nothing
            If _snapshots.TryGetValue(ctrl, existing) Then Return

            Dim snap As New Snapshot With {
                .BackColor = ctrl.BackColor,
                .ForeColor = ctrl.ForeColor,
                .Font = ctrl.Font,
                .HasBackColor = IsSerialized(ctrl, NameOf(Control.BackColor)),
                .HasForeColor = IsSerialized(ctrl, NameOf(Control.ForeColor)),
                .HasFont = IsSerialized(ctrl, NameOf(Control.Font))
            }
            _snapshots.Add(ctrl, snap)
        Catch ex As Exception
            ' Instantaneul e un ajutor, nu o precondiție: dacă nu se poate lua, tema merge înainte.
            GlobalErrorLog.Write("DesignerBaseline.Capture", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Pune la loc valorile reținute. Întoarce False dacă nu există instantaneu pentru control
    ''' (nu a trecut niciodată prin traversare) — apelantul poate atunci să-l stilizeze normal.
    ''' </summary>
    Public Function Restore(ctrl As Control) As Boolean
        If ctrl Is Nothing Then Return False
        Try
            Dim snap As Snapshot = Nothing
            If Not _snapshots.TryGetValue(ctrl, snap) Then Return False

            ' Setat în designer ⇒ atribuim valoarea. Nesetat ⇒ Reset*, ca proprietatea să redevină
            ' moștenită (ambientală) în loc să rămână fixată cu ce a scris ultima temă.
            If snap.HasBackColor Then
                ctrl.BackColor = snap.BackColor
            Else
                ctrl.ResetBackColor()
            End If

            If snap.HasForeColor Then
                ctrl.ForeColor = snap.ForeColor
            Else
                ctrl.ResetForeColor()
            End If

            If snap.HasFont Then
                ctrl.Font = snap.Font
            Else
                ctrl.ResetFont()
            End If

            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("DesignerBaseline.Restore", ex)
            Return False
        End Try
    End Function

    ''' <summary>Există un instantaneu pentru controlul dat? (folosit de teste și de editor)</summary>
    Public Function HasSnapshot(ctrl As Control) As Boolean
        If ctrl Is Nothing Then Return False
        Dim snap As Snapshot = Nothing
        Return _snapshots.TryGetValue(ctrl, snap)
    End Function

    ''' <summary>
    ''' Uită instantaneul unui control. Folosit de editorul de stiluri: după ce operatorul
    ''' schimbă intenționat o culoare, NOUA valoare devine baza pe care o păstrează «Colorful».
    ''' </summary>
    Public Sub Forget(ctrl As Control)
        If ctrl Is Nothing Then Return
        _snapshots.Remove(ctrl)
    End Sub

    ' Calea pe care merge Visual Studio când decide dacă scrie proprietatea în .Designer.vb —
    ' NU ShouldSerializeX apelat direct (vezi regula casei din CLAUDE.md).
    Private Function IsSerialized(ctrl As Control, propName As String) As Boolean
        Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(ctrl)(propName)
        If pd Is Nothing Then Return False
        Return pd.ShouldSerializeValue(ctrl)
    End Function

End Module
