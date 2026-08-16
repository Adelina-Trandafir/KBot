Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Mouse-ul și TASTATURA meniului — două drumuri egale către același rând, ca la un meniu de
''' sistem. O singură evidențiere le deservește pe amândouă: survolarea o mută, săgețile o mută,
''' Enter alege ce e evidențiat.
'''
''' <para><b>De ce tot ce ține de taste trece prin <c>ProcessCmdKey</c>.</b> Un formular fără
''' controale-copil nu primește săgețile în <c>OnKeyDown</c> — navigarea de dialog le înghite
''' înainte. <c>ProcessCmdKey</c> vede mesajul brut (și <c>WM_SYSKEYDOWN</c>, adică Alt+literă)
''' înaintea oricărei alte prelucrări, deci e singurul loc unde săgețile, Enter, Esc și literele
''' de acces pot fi tratate la fel.</para>
''' </summary>
Partial Public Class CustomPopup

    ''' <summary>
    ''' Frontieră de UI (tastatură): se loghează și se înghite — un throw de aici ar ieși prin
    ''' bucla de mesaje. «Am tratat tasta?» pe eroare e Nu, ca mesajul să-și urmeze drumul normal.
    ''' </summary>
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Try
            Dim code As Keys = keyData And Keys.KeyCode

            ' Windows aprinde sublinierile de acces la prima tastă apăsată în meniu.
            RevealMnemonics()

            Select Case code
                Case Keys.Escape
                    CloseWith(Nothing, -1)
                    Return True
                Case Keys.Up
                    MoveSelection(-1)
                    Return True
                Case Keys.Down
                    MoveSelection(1)
                    Return True
                Case Keys.Left
                    ' Săgețile orizontale sunt ale CURSORULUI evidențiat. Fără unul, tasta își
                    ' vede de drum: un meniu în care stânga/dreapta nu fac nimic e mai bun decât
                    ' unul în care fac altceva decât se așteaptă operatorul.
                    If NudgeSelectedSlider(-SliderKeyStep) Then Return True
                Case Keys.Right
                    If NudgeSelectedSlider(SliderKeyStep) Then Return True
                Case Keys.Home
                    ' Pe un cursor, Home/End sunt capetele ȘINEI, nu capetele meniului — asta
                    ' așteaptă oricine a mai folosit un cursor.
                    If IsSliderRow(SelectedIndex) Then
                        SetSliderValue(SelectedIndex, Items(SelectedIndex).SliderMinimum)
                        Return True
                    End If
                    SelectEdge(True)
                    Return True
                Case Keys.End
                    If IsSliderRow(SelectedIndex) Then
                        SetSliderValue(SelectedIndex, Items(SelectedIndex).SliderMaximum)
                        Return True
                    End If
                    SelectEdge(False)
                    Return True
                Case Keys.Enter, Keys.Space
                    ActivateItem(SelectedIndex)
                    Return True
            End Select

            ' Litera de acces. Ctrl+literă NU e o literă de acces (e o scurtătură a aplicației și
            ' trebuie să-și vadă de drum); Alt+literă e, și ajunge tot aici, prin WM_SYSKEYDOWN.
            If (keyData And Keys.Control) = Keys.None Then
                Dim ch As Char = KeyToChar(code)
                If ch <> PopupMnemonic.None AndAlso HandleMnemonic(ch) Then Return True
            End If

            Return MyBase.ProcessCmdKey(msg, keyData)
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.ProcessCmdKey", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Ridicarea tastei încheie un gest de cursor pornit din săgeți / Home / End. Săgețile sosesc
    ''' una câte una și se REPETĂ cât ții tasta apăsată, deci predarea la fiecare apăsare ar fi
    ''' comandat lucrul greu de zeci de ori — exact ce se întâmpla cu mouse-ul înainte de trecerea
    ''' asta.
    '''
    ''' Frontieră de UI (tastatură): logăm și înghițim.
    ''' </summary>
    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        Try
            MyBase.OnKeyUp(e)
            CommitKeyboardSlider()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.OnKeyUp", ex)
        End Try
    End Sub

    ''' <summary>Tasta ca literă/cifră de acces, sau <see cref="PopupMnemonic.None"/>.</summary>
    Friend Shared Function KeyToChar(code As Keys) As Char
        If code >= Keys.A AndAlso code <= Keys.Z Then Return ChrW(CInt(code))
        If code >= Keys.D0 AndAlso code <= Keys.D9 Then Return ChrW(CInt(code))
        If code >= Keys.NumPad0 AndAlso code <= Keys.NumPad9 Then
            Return ChrW(CInt(code) - CInt(Keys.NumPad0) + AscW("0"c))
        End If
        Return PopupMnemonic.None
    End Function

    ''' <summary>
    ''' Regula Windows pentru litera de acces, cap-coadă:
    ''' o singură potrivire ⇒ se alege pe loc; mai multe ⇒ evidențierea trece la următoarea,
    ''' ciclic, fără să aleagă nimic (a doua apăsare merge mai departe, Enter confirmă).
    ''' Elementele dezactivate și separatorii n-au literă de acces deloc (vezi
    ''' <c>CustomPopupItem.Mnemonic</c>), deci nu apar niciodată printre potriviri.
    ''' Întoarce False dacă litera nu e a nimănui — atunci tasta își vede de drum.
    ''' </summary>
    Friend Function HandleMnemonic(ch As Char) As Boolean
        Dim potriviri As New List(Of Integer)()
        For i As Integer = 0 To Items.Count - 1
            If Items(i).Mnemonic = ch Then potriviri.Add(i)
        Next

        If potriviri.Count = 0 Then Return False
        If potriviri.Count = 1 Then
            ActivateItem(potriviri(0))
            Return True
        End If

        Dim urmator As Integer = potriviri(0)
        For Each idx As Integer In potriviri
            If idx > SelectedIndex Then
                urmator = idx
                Exit For
            End If
        Next
        SelectedIndex = urmator
        Return True
    End Function

    ''' <summary>Mută evidențierea cu un pas, sărind separatorii și rândurile dezactivate; se învârte în cerc.</summary>
    Friend Sub MoveSelection(delta As Integer)
        Dim n As Integer = Items.Count
        If n = 0 OrElse delta = 0 Then Return
        ' Fără nicio evidențiere, «jos» pornește dinaintea primului rând și «sus» de după ultimul.
        Dim start As Integer = If(SelectedIndex >= 0, SelectedIndex, If(delta > 0, -1, n))
        For pas As Integer = 1 To n
            Dim idx As Integer = (((start + delta * pas) Mod n) + n) Mod n
            If IsSelectable(idx) Then
                SelectedIndex = idx
                Return
            End If
        Next
    End Sub

    ''' <summary>Home / End: primul, respectiv ultimul rând care se poate alege.</summary>
    Friend Sub SelectEdge(primul As Boolean)
        If primul Then
            For i As Integer = 0 To Items.Count - 1
                If IsSelectable(i) Then
                    SelectedIndex = i
                    Return
                End If
            Next
        Else
            For i As Integer = Items.Count - 1 To 0 Step -1
                If IsSelectable(i) Then
                    SelectedIndex = i
                    Return
                End If
            Next
        End If
    End Sub

    ' =====================================================================
    ' MOUSE
    ' =====================================================================

    ''' <summary>
    ''' Apăsarea contează doar pentru CURSOARE: acolo tragerea trebuie să înceapă pe apăsare, nu
    ''' pe ridicare. Rândurile obișnuite se aleg în continuare pe ridicare (vezi
    ''' <see cref="OnMouseUp"/>) — regula meniurilor de sistem, care apără deschiderea însăși de a
    ''' fi luată drept alegere.
    ''' </summary>
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim i As Integer = HitTest(e.Location)
            If IsSliderRow(i) AndAlso Items(i).Enabled Then
                SelectedIndex = i
                BeginSliderDrag(i, e.X)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.OnMouseDown", ex)
        End Try
    End Sub

    ''' <summary>Survolarea MUTĂ evidențierea — la un meniu nu există «hover» separat de selecție.</summary>
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            ' Cât se trage un cursor, mouse-ul e AL LUI: mișcarea nu mai mută evidențierea, nici
            ' dacă degetul iese din rând. Altfel o tragere lungă ar «scăpa» pe rândul vecin și
            ' valoarea ar rămâne în urmă la jumătatea drumului.
            If IsDraggingSlider Then
                UpdateSliderDrag(e.X)
                Return
            End If

            Dim i As Integer = HitTest(e.Location)
            If IsSelectable(i) Then SelectedIndex = i
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.OnMouseMove", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Alegerea se face la RIDICAREA butonului, ca la meniurile de sistem: dacă meniul s-a deschis
    ''' pe apăsare, apăsarea aceea nu are voie să aleagă rândul de sub cursor.
    ''' </summary>
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            If e.Button <> MouseButtons.Left Then Return

            ' Ridicarea care încheie o tragere NU alege nimic — degetul tocmai a lăsat șina.
            If IsDraggingSlider Then
                EndSliderDrag()
                Return
            End If

            ActivateItem(HitTest(e.Location))
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.OnMouseUp", ex)
        End Try
    End Sub

    ''' <summary>Rotița derulează, când meniul e mai înalt decât zona de lucru a ecranului.</summary>
    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            If MaxScroll <= 0 Then Return
            Dim pasi As Integer = -e.Delta \ SystemInformation.MouseWheelScrollDelta
            ' MouseWheelScrollLines e -1 când sistemul e pus pe «o pagină per pas»; aici asta ar
            ' însemna derulare inversă, deci cădem pe cele trei rânduri obișnuite.
            Dim randuri As Integer = SystemInformation.MouseWheelScrollLines
            If randuri <= 0 Then randuri = 3
            ScrollBy(pasi * EffectiveRowHeight() * randuri)
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.OnMouseWheel", ex)
        End Try
    End Sub

End Class
