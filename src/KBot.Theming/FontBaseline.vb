Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Memoria fontului NESCALAT al fiecărui control, pentru mărirea textului (felia 0036-01).
'''
''' <para><b>De ce nu se refolosește <see cref="DesignerBaseline"/>.</b> Acela reține valorile din
''' designer ca să le PUNĂ LA LOC — exact ce face schema «Colorat», prin
''' <c>PreserveDesignerColors</c>. Dacă mărirea textului s-ar sprijini pe el, «Colorat» ar
''' restaura fontul nescalat și mărirea ar dispărea tăcut pe o singură schemă. Sunt două memorii
''' fiindcă răspund la două întrebări diferite: «ce a autorit operatorul» și «de la ce pornesc
''' când înmulțesc».</para>
'''
''' <para><b>Baza se re-așază când tema scrie peste ea — dar i se SPUNE, nu ghicește.</b> Nu se
''' poate reține o dată și gata: <c>ThemeManager.ApplyBaseFont</c> schimbă fontul formularului la
''' fiecare comutare de schemă («Modern» cere Segoe UI Variable Text 9), iar «Colorat» îl
''' restaurează pe cel din designer — deci baza de ieri nu mai e baza de azi. Cele două locuri
''' cheamă <see cref="Rebase"/> imediat după ce scriu.</para>
'''
''' <para><b>De ce nu se ghicește.</b> Prima formă compara REFERINȚA fontului scris de noi cu cea
''' de pe control, ca să vadă dacă l-a rescris altcineva. Nu ține pe un <c>Form</c>: fiindcă
''' formularele sunt <c>AutoScaleMode.Font</c>, scrierea fontului declanșează
''' <c>PerformAutoScale</c>, iar autoscalarea își face PROPRIA instanță de <c>Font</c> — deci
''' obiectul de pe control nu mai era niciodată cel scris de noi, tocmai în cazul care contează.
''' Rezultatul: baza se muta la fiecare pas și mărimile se compuneau (10 → 15 → 30 în loc de 20).
''' Un test a prins-o; semnalul explicit e leacul, fiindcă nu depinde de ce face WinForms în
''' interior.</para>
'''
''' <para><b>Ce NU se atinge.</b> Doar formularele și controalele cu font PROPRIU (setat în
''' designer). Un control care moștenește fontul ambiental nu se scalează aici — i-ar fi FIXAT
''' fontul, adică l-ar rupe de formular pentru totdeauna. El se mărește oricum, prin moștenire,
''' când crește fontul formularului.</para>
'''
''' <para><b>Fonturile create nu se eliberează</b>, deliberat. Un <c>Font</c> pe care l-a luat un
''' control se poate afla în chiar acel moment într-o pictură, iar copiii care moștenesc fontul
''' ambiental împart ACEEAȘI instanță cu formularul; un <c>Dispose</c> pus la momentul greșit dă
''' <c>ObjectDisposedException</c> dintr-un <c>OnPaint</c>, adică exact felul de cădere pe care
''' regulile casei îl evită peste tot. Costul e o mână de obiecte per schimbare de mărime —
''' colectorul le ia oricum când controlul moare.</para>
''' </summary>
Public Module FontBaseline

    Private NotInheritable Class Snapshot
        ''' <summary>Fontul de la 100% — punctul din care se înmulțește.</summary>
        Public BaseFont As Font
        ''' <summary>Am scris vreodată pe controlul ăsta? (decide dacă revenirea la 100% are ce întoarce)</summary>
        Public HasApplied As Boolean
        ''' <summary>Controlul are font propriu (autorit), deci merită scalat individual.</summary>
        Public HasOwnFont As Boolean
    End Class

    Private ReadOnly _snapshots As New ConditionalWeakTable(Of Control, Snapshot)()

    ''' <summary>
    ''' «Fontul de pe control E acum baza.» O cheamă cele două locuri care scriu fonturi peste ale
    ''' noastre — <c>ThemeManager.ApplyBaseFont</c> (fontul schemei) și
    ''' <c>DesignerBaseline.Restore</c> (schema «Colorat», care repune fontul din designer).
    '''
    ''' Trebuie chemată ÎNAINTE ca mărirea să fie aplicată din nou; ordinea e păzită de
    ''' <c>ThemeManager.Apply</c>, care rulează mărirea la sfârșit de tot.
    ''' </summary>
    Public Sub Rebase(ctrl As Control)
        If ctrl Is Nothing Then Return
        Try
            Dim snap As Snapshot = Ensure(ctrl)
            snap.BaseFont = ctrl.Font
            snap.HasOwnFont = IsSerialized(ctrl, NameOf(Control.Font))
            snap.HasApplied = False
        Catch ex As Exception
            GlobalErrorLog.Write("FontBaseline.Rebase", ex)
        End Try
    End Sub

    ' Instantaneul controlului; prima dată, fontul de acum devine baza.
    Private Function Ensure(ctrl As Control) As Snapshot
        Dim snap As Snapshot = Nothing
        If _snapshots.TryGetValue(ctrl, snap) Then Return snap

        snap = New Snapshot With {
            .BaseFont = ctrl.Font,
            .HasOwnFont = IsSerialized(ctrl, NameOf(Control.Font)),
            .HasApplied = False
        }
        _snapshots.Add(ctrl, snap)
        Return snap
    End Function

    ''' <summary>
    ''' Scrie pe control fontul de bază înmulțit cu <paramref name="factor"/>. Un factor de 1 pune
    ''' înapoi baza (și uită că am scris vreodată), ca revenirea la 100% să fie exactă, nu o
    ''' înmulțire cu inversul — aceea ar lăsa erori de rotunjire la fiecare drum dus-întors.
    '''
    ''' Întoarce True dacă fontul controlului chiar s-a schimbat.
    ''' </summary>
    Public Function ApplyScale(ctrl As Control, factor As Single) As Boolean
        If ctrl Is Nothing Then Return False
        Try
            Dim snap As Snapshot = Ensure(ctrl)
            If snap.BaseFont Is Nothing Then Return False

            ' Doar formularele și controalele cu font propriu. Restul moștenesc — vezi rezumatul.
            If TypeOf ctrl IsNot Form AndAlso Not snap.HasOwnFont Then Return False

            If Math.Abs(factor - 1.0F) < 0.0001F Then
                If Not snap.HasApplied Then Return False   ' n-am scris niciodată: nimic de întors
                ctrl.Font = snap.BaseFont
                snap.HasApplied = False
                Return True
            End If

            Dim marime As Single = snap.BaseFont.Size * factor
            If marime <= 0F Then Return False

            ctrl.Font = New Font(snap.BaseFont.FontFamily, marime, snap.BaseFont.Style,
                                 snap.BaseFont.Unit, snap.BaseFont.GdiCharSet, snap.BaseFont.GdiVerticalFont)
            snap.HasApplied = True
            Return True
        Catch ex As Exception
            ' Un font lipsă sau o familie stricată nu are voie să oprească aplicarea temei pe
            ' restul ferestrei — controlul rămâne pe fontul lui.
            GlobalErrorLog.Write("FontBaseline.ApplyScale", ex)
            Return False
        End Try
    End Function

    ''' <summary>Uită ce știm despre un control (folosit de teste).</summary>
    Public Sub Forget(ctrl As Control)
        If ctrl Is Nothing Then Return
        _snapshots.Remove(ctrl)
    End Sub

    ''' <summary>Fontul de la 100% al controlului, sau Nothing dacă nu l-am văzut niciodată.</summary>
    Public Function BaseFontOf(ctrl As Control) As Font
        If ctrl Is Nothing Then Return Nothing
        Dim snap As Snapshot = Nothing
        If Not _snapshots.TryGetValue(ctrl, snap) Then Return Nothing
        Return snap.BaseFont
    End Function

    ' Calea pe care merge Visual Studio când decide ce scrie în .Designer.vb — NU ShouldSerializeX
    ' apelat direct (regula casei din CLAUDE.md).
    Private Function IsSerialized(ctrl As Control, propName As String) As Boolean
        Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(ctrl)(propName)
        If pd Is Nothing Then Return False
        Return pd.ShouldSerializeValue(ctrl)
    End Function

End Module
