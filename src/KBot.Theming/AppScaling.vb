Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Modul de calcul al scării pentru măsurile K-BOT.
''' </summary>
Public Enum ScalingMode
    ''' <summary>Scara vine din DPI-ul ecranului (<c>DeviceDpi / 96</c>). Comportamentul dintotdeauna.</summary>
    Automatic = 0
    ''' <summary>Scara e fixată la 1 — geometria desenată rămâne cea de la 96 dpi, adică EXACT cea din designer.</summary>
    Fixed100 = 1
    ''' <summary>Scara e cea tastată de operator (<see cref="AppScaling.ManualFactor"/>), aceeași pe orice ecran.</summary>
    Manual = 2
End Enum

''' <summary>
''' SURSA UNICĂ a scării cu care se desenează măsurile proprii ale controalelor K-BOT (felia 0036).
'''
''' <para><b>De ce există.</b> Până acum scara era calculată în trei locuri, toate din
''' <c>DeviceDpi / 96</c>: <see cref="ThemeShapes.ScaleDpi"/> (constantele din pictură, ~157 de
''' locuri), plus câte un <c>_dpiScale</c> în <c>AdvancedTreeControl</c> și în <c>KBotDataView</c>.
''' Formula era una singură, dar nu exista niciun loc din care s-o POȚI SCHIMBA. Modulul ăsta e
''' acel loc: cele trei drumuri întreabă acum aici, deci o alegere a operatorului ajunge peste tot
''' deodată sau nicăieri — niciodată pe jumătate.</para>
'''
''' <para><b>Ce rezolvă pentru operator.</b> Proiectarea se face la 100%; rulată la 125% sau 150%,
''' aceeași fereastră arăta altfel. <see cref="ScalingMode.Fixed100"/> pune geometria DESENATĂ
''' înapoi pe valorile din designer, iar <see cref="ScalingMode.Manual"/> îi dă un singur număr pe
''' care îl poate potrivi cu ochii.</para>
'''
''' <para><b>Onestitate — ce NU face.</b> Aici se decide DOAR scara măsurilor NOASTRE. Fonturile
''' (care sunt în puncte) și <c>Bounds</c>-urile controalelor obișnuite le scalează în continuare
''' WinForms prin <c>AutoScaleMode.Font</c>, iar acela nu poate fi oprit din afară. Deci pe
''' «Fix 100%» la 150% textul va fi în continuare mai mare decât geometria din jurul lui — e
''' compromisul modului, nu o scăpare. Singurul comutator care dă proporții IDENTICE cu
''' proiectarea e <see cref="DpiUnaware"/>, fiindcă acolo întinde Windows toată fereastra ca
''' bitmap; costul e textul mai moale. De aceea sunt două setări, nu una.</para>
'''
''' <para><b>La design time scara e mereu 1</b>, indiferent de mod: suprafața Visual Studio
''' desenează la 96 dpi, deci acolo trebuie să se vadă chiar valoarea tastată. Un factor manual
''' aplicat în designer ar face ca ce vezi să nu mai fie ce ai scris.</para>
''' </summary>
Public Module AppScaling

    ''' <summary>Limita de jos a factorului manual — sub ea nu mai încape textul în nimic.</summary>
    Public Const MinManualFactor As Single = 0.5F

    ''' <summary>Limita de sus a factorului manual.</summary>
    Public Const MaxManualFactor As Single = 4.0F

    ''' <summary>Limitele măririi textului. Mai jos de 75% nu se mai citește; peste 200% nu mai încape.</summary>
    Public Const MinTextScale As Single = 0.75F

    ''' <summary>Vezi <see cref="MinTextScale"/>.</summary>
    Public Const MaxTextScale As Single = 2.0F

    Private _mode As ScalingMode = ScalingMode.Automatic
    Private _manualFactor As Single = 1.0F
    Private _dpiUnaware As Boolean = False
    Private _textScale As Single = 1.0F

    ''' <summary>
    ''' Ridicat după ce s-a schimbat modul sau factorul. <see cref="Broadcast"/> a rulat deja,
    ''' deci ferestrele deschise s-au remăsurat — evenimentul e pentru ce e ÎN PLUS față de asta.
    ''' </summary>
    Public Event ScalingChanged As EventHandler

    ''' <summary>Modul de scalare activ. Scrie prin <see cref="Configure"/>, ca să se și difuzeze.</summary>
    Public ReadOnly Property Mode As ScalingMode
        Get
            Return _mode
        End Get
    End Property

    ''' <summary>Factorul folosit în <see cref="ScalingMode.Manual"/> (1 = 96 dpi).</summary>
    Public ReadOnly Property ManualFactor As Single
        Get
            Return _manualFactor
        End Get
    End Property

    ''' <summary>
    ''' «Windows să întindă fereastra, nu noi să scalăm.» Citit O SINGURĂ DATĂ la pornire, în
    ''' <c>Program.Main</c>, unde decide <c>HighDpiMode.DpiUnaware</c> în loc de
    ''' <c>PerMonitorV2</c> — de aceea schimbarea lui cere repornirea aplicației: modul DPI al
    ''' unui proces nu se mai poate schimba după ce s-a creat prima fereastră.
    ''' </summary>
    Public Property DpiUnaware As Boolean
        Get
            Return _dpiUnaware
        End Get
        Set(value As Boolean)
            If value = _dpiUnaware Then Return
            _dpiUnaware = value
            ThemeStore.SaveScaling(_mode, _manualFactor, _dpiUnaware, _textScale)
        End Set
    End Property

    ''' <summary>
    ''' MĂRIMEA TEXTULUI ȘI A CONTROALELOR, ca fracție (1 = 100%). E lucrul cerut de operator prin
    ''' «un buton sau un cursor pentru text mai mare sau mai mic».
    '''
    ''' <para><b>Cum devine dintr-un font o mărire a întregii ferestre.</b> Toate formularele sunt
    ''' <c>AutoScaleMode.Font</c>: când li se schimbă fontul, WinForms rulează singur
    ''' <c>PerformAutoScale</c> și rescalează dreptunghiurile copiilor. Deci scriind fontul
    ''' formularului (din baza lui, prin <see cref="FontBaseline"/>) se măresc și literele, și
    ''' controalele — platforma face partea grea. Peste asta, factorul intră și în
    ''' <see cref="FactorFor"/>, ca măsurile pe care le desenăm NOI (înălțimea de rând din arbore
    ''' și din grilă, constantele din pictură) să crească în același pas; altfel textul ar crește
    ''' într-un rând care nu crește.</para>
    '''
    ''' <para>Scrie prin <see cref="SetTextScale"/>, ca să se și aplice.</para>
    ''' </summary>
    Public ReadOnly Property TextScale As Single
        Get
            Return _textScale
        End Get
    End Property

    ''' <summary>Aduce un factor între limite. Valorile absurde (0, negative) cad pe 1.</summary>
    Public Function ClampFactor(value As Single) As Single
        If value <= 0F OrElse Single.IsNaN(value) OrElse Single.IsInfinity(value) Then Return 1.0F
        If value < MinManualFactor Then Return MinManualFactor
        If value > MaxManualFactor Then Return MaxManualFactor
        Return value
    End Function

    ''' <summary>Aduce mărimea textului între limite. Valorile absurde cad pe 1 (100%).</summary>
    Public Function ClampTextScale(value As Single) As Single
        If value <= 0F OrElse Single.IsNaN(value) OrElse Single.IsInfinity(value) Then Return 1.0F
        If value < MinTextScale Then Return MinTextScale
        If value > MaxTextScale Then Return MaxTextScale
        Return value
    End Function

    ''' <summary>
    ''' Așază mărimea textului, o persistă și o duce pe ecran. Punct UNIC de scriere, ca și
    ''' <see cref="Configure"/>.
    ''' </summary>
    Public Sub SetTextScale(value As Single)
        Try
            Dim nou As Single = ClampTextScale(value)
            If nou = _textScale Then Return
            _textScale = nou
            ThemeStore.SaveScaling(_mode, _manualFactor, _dpiUnaware, _textScale)
            Broadcast()
            RaiseEvent ScalingChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.SetTextScale", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Duce mărimea textului la un control și la toți copiii lui. O cheamă
    ''' <c>ThemeManager.Apply</c> la SFÂRȘIT, după ce tema și-a scris fonturile — ordinea contează:
    ''' invers, schema «Colorat» ar restaura fontul nescalat peste mărire și aceasta ar dispărea
    ''' pe o singură schemă, ceea ce ar fi arătat ca un defect fără cauză.
    ''' </summary>
    Public Sub ApplyTextScale(root As Control)
        If root Is Nothing Then Return
        Try
            If KBotDesignTime.IsDesignTime(root) Then Return   ' în designer se vede ce s-a autorit
            ScaleFontsRecursive(root)
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.ApplyTextScale", ex)
            Throw
        End Try
    End Sub

    ' Formularul PRIMUL: scrierea fontului lui declanșează autoscalarea WinForms, care mută
    ' dreptunghiurile copiilor. Copiii cu font propriu se scalează după aceea, individual.
    Private Sub ScaleFontsRecursive(ctrl As Control)
        FontBaseline.ApplyScale(ctrl, _textScale)

        Dim sc As SplitContainer = TryCast(ctrl, SplitContainer)
        If sc IsNot Nothing Then
            For Each child As Control In sc.Panel1.Controls
                ScaleFontsRecursive(child)
            Next
            For Each child As Control In sc.Panel2.Controls
                ScaleFontsRecursive(child)
            Next
            Return
        End If

        Dim tc As TabControl = TryCast(ctrl, TabControl)
        If tc IsNot Nothing Then
            For Each tp As TabPage In tc.TabPages
                ScaleFontsRecursive(tp)
            Next
            Return
        End If

        For Each child As Control In ctrl.Controls
            ScaleFontsRecursive(child)
        Next
    End Sub

    ''' <summary>
    ''' Așază modul + factorul, le persistă și le duce la ferestrele deschise. Punctul UNIC de
    ''' scriere: nu există cale prin care scara să se schimbe fără ca ecranul să afle.
    ''' </summary>
    Public Sub Configure(mode As ScalingMode, manualFactor As Single)
        Try
            _mode = mode
            _manualFactor = ClampFactor(manualFactor)
            ThemeStore.SaveScaling(_mode, _manualFactor, _dpiUnaware, _textScale)
            Broadcast()
            RaiseEvent ScalingChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.Configure", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Așază valorile citite din fișier FĂRĂ să persiste și fără să difuzeze — o folosește
    ''' <c>ThemeManager.Initialize</c>, înainte să existe vreo fereastră. Separată de
    ''' <see cref="Configure"/> tocmai ca încărcarea să nu rescrie fișierul din care tocmai a citit.
    ''' </summary>
    Friend Sub LoadFrom(mode As ScalingMode, manualFactor As Single, dpiUnaware As Boolean,
                        textScale As Single)
        _mode = mode
        _manualFactor = ClampFactor(manualFactor)
        _dpiUnaware = dpiUnaware
        _textScale = ClampTextScale(textScale)
    End Sub

    ''' <summary>
    ''' Scara pentru controlul dat: scara de ECRAN (modul ales) înmulțită cu MĂRIMEA cerută de
    ''' operator. Sunt două lucruri diferite adunate într-un singur număr, și trebuie să fie
    ''' împreună: la 150% pe un ecran, cu textul pus pe 125%, un rând trebuie să fie de 1,875 ori
    ''' cel de la 96 dpi — nu de 1,5 și nici de 1,25.
    '''
    ''' La design time — și pentru un control fără handle, unde <c>DeviceDpi</c> minte cu 96 —
    ''' răspunsul e 1.
    ''' </summary>
    Public Function FactorFor(ctrl As Control) As Single
        Try
            If ctrl IsNot Nothing AndAlso KBotDesignTime.IsDesignTime(ctrl) Then Return 1.0F
            Return EcranFactor(ctrl) * _textScale
        Catch
            ' Predicat de pictură: „nu știu” înseamnă 1, niciodată o excepție dintr-un OnPaint.
            Return 1.0F
        End Try
    End Function

    ' Doar partea de ECRAN a scării — fără mărirea cerută de operator.
    Private Function EcranFactor(ctrl As Control) As Single
        Select Case _mode
            Case ScalingMode.Fixed100
                Return 1.0F
            Case ScalingMode.Manual
                Return _manualFactor
            Case Else
                If ctrl Is Nothing Then Return 1.0F
                Return CSng(ctrl.DeviceDpi / 96.0)
        End Select
    End Function

    ''' <summary>Valoare logică (px @96dpi) → px de ecran, la scara controlului dat.</summary>
    Public Function Scale(ctrl As Control, logical As Integer) As Integer
        Return CInt(Math.Round(logical * FactorFor(ctrl)))
    End Function

    ''' <summary>
    ''' Duce scara nouă la toate ferestrele deschise: controalele care își țin măsuri proprii
    ''' (<see cref="IDpiScaledControl"/>) le refac, restul se repictează — constantele lor trec
    ''' prin <see cref="ThemeShapes.ScaleDpi"/> la fiecare pictare, deci le ajunge o invalidare.
    ''' </summary>
    Public Sub Broadcast()
        Try
            ' Copie a listei: scrierea fontului formularului declanșează autoscalarea WinForms,
            ' iar aceea poate deschide/închide ferestre prin evenimentele de layout — o enumerare
            ' directă peste OpenForms ar crăpa atunci cu «colecția s-a modificat».
            Dim ferestre As New List(Of Form)()
            For Each f As Form In Application.OpenForms
                If f IsNot Nothing AndAlso Not f.IsDisposed Then ferestre.Add(f)
            Next

            For Each f As Form In ferestre
                If f.IsDisposed Then Continue For
                ' Fonturile ÎNTÂI: autoscalarea pe care o declanșează mută dreptunghiurile, deci
                ' măsurile proprii trebuie recalculate DUPĂ ea, nu înainte.
                ScaleFontsRecursive(f)
                RefreshTree(f)
                f.PerformLayout()
                f.Invalidate(True)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.Broadcast", ex)
            Throw
        End Try
    End Sub

    ' Recursie completă: un control cu măsuri proprii poate sta oricât de adânc (grila dintr-o
    ' vedere, arborele din panoul unui SplitContainer). Spre deosebire de ThemeManager.Traverse,
    ' aici NU se oprește la IThemedControl — scara nu e o culoare, nu are cum să strice
    ' un copil intern.
    Private Sub RefreshTree(ctrl As Control)
        Dim scaled As IDpiScaledControl = TryCast(ctrl, IDpiScaledControl)
        If scaled IsNot Nothing Then scaled.RefreshDpiMetrics()

        Dim sc As SplitContainer = TryCast(ctrl, SplitContainer)
        If sc IsNot Nothing Then
            For Each child As Control In sc.Panel1.Controls
                RefreshTree(child)
            Next
            For Each child As Control In sc.Panel2.Controls
                RefreshTree(child)
            Next
            Return
        End If

        Dim tc As TabControl = TryCast(ctrl, TabControl)
        If tc IsNot Nothing Then
            For Each tp As TabPage In tc.TabPages
                RefreshTree(tp)
            Next
            Return
        End If

        For Each child As Control In ctrl.Controls
            RefreshTree(child)
        Next
    End Sub

End Module
