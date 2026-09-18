Option Strict On
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Drawing
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
''' <para><b>What the platform does and what we do (slice 0066-02).</b> Every form is
''' <c>AutoScaleMode.Dpi</c>: WinForms multiplies EVERY rectangle by itself (Bounds, Margin,
''' Padding, the fixed styles of the tables, MinimumSize) by <c>DeviceDpi / design dpi</c> -- the
''' same number on both axes, exactly. Until 0066-02 the forms were <c>AutoScaleMode.Font</c>,
''' which multiplies by the ratio of two INTEGER font metrics (the rounded average character
''' width and the line height): MEASURED 1.29 on X and 1.47 on Y at 150%, 0.86 x 0.93 at 100%
''' with the text at 99% -- never <c>DeviceDpi / 96</c>, never the same on both axes, and
''' different at every DPI. That is why the same window looked different at 100%, 125% and
''' 150%, and why the tree, the grid and the table (which drew correctly, at
''' <c>DeviceDpi / 96</c>) never matched what stood around them. On top of the platform's scale
''' we put ONE uniform zoom, <see cref="ZoomFor"/> -- the text size, plus, under
''' <see cref="ScalingMode.Fixed100"/> / <see cref="ScalingMode.Manual"/>, the difference between
''' the scale asked for and the screen's -- through <c>Control.Scale</c>, and we write the fonts
''' with the SAME number. So geometry and text sit on a single ruler in all three modes: "Fix
''' 100%" at 150% is the window of a 100% screen, text included. <see cref="DpiUnaware"/> stays
''' for whoever prefers Windows' bitmap stretch.</para>
'''
''' <para><b>At design time the scale is the surface's</b>, i.e. still <c>DeviceDpi / 96</c>.
''' The VS 2022 designer for .NET runs DPI-aware: on a 150% screen it draws at 144 dpi and stamps
''' screen pixels into .Designer.vb, with <c>AutoScaleDimensions = (144, 144)</c> next to them
''' (before 0066-02 the font pair of the same dpi, (9, 22) for Calibri 9). If OUR measures stayed
''' at 1 in the designer, half of the drawing would be at 150% and the other half (rows, bands,
''' paddings) at 100% -- exactly the difference seen between designer and runtime. What stays 1
''' at design time is only our ZOOM (<see cref="ZoomFor"/>): the text size is the operator's
''' setting, not the screen's, and the designer does not read theme.json.</para>
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

    ''' <summary>
    ''' The FONT size for a given control -- 1 at design time, where exactly what was authored
    ''' must show. The same number as <see cref="ZoomFor"/>, because geometry and text sit on a
    ''' single ruler (slice 0066-02): a font is in points, so the screen's DPI already scales it,
    ''' and on top of that we put exactly what we put on the rectangles -- the text size, and
    ''' under Fixed100 / Manual the difference to the screen. Under
    ''' <see cref="ScalingMode.Automatic"/> it is exactly <see cref="TextScale"/>.
    ''' </summary>
    Public Function TextFactorFor(ctrl As Control) As Single
        Return ZoomFor(ctrl)
    End Function

    ''' <summary>
    ''' Varianta MĂRITĂ a unui font pe care îl ține un control al nostru — fontul unei coloane, al
    ''' unei benzi, al unei etichete desenate de noi.
    '''
    ''' <para><b>De ce e nevoie de ea.</b> <see cref="ApplyTextScale"/> mărește doar
    ''' <c>Control.Font</c>; un font ținut într-o PROPRIETATE proprie (<c>KBotDataColumn.ColumnFont</c>,
    ''' <c>KBotDataView.HeaderFont</c>, fonturile de bandă derivate din schemă) nu trece pe acolo și
    ''' rămâne la mărimea autorită. Exact așa arăta grila: rândurile creșteau cu cursorul, iar textul
    ''' din celule nu — fiindcă toate coloanele aveau <c>ColumnFont</c> pus în designer.</para>
    '''
    ''' <para>Proprietatea publică rămâne cea AUTORITĂ (designerul serializează 9, nu 13,5) — se
    ''' scalează la FOLOSIRE, ca la lățimile de coloană din felia 0035.</para>
    '''
    ''' <para>Fonturile derivate se păstrează într-un cache golit la fiecare schimbare de mărime și
    ''' NU se eliberează — un <c>Font</c> pe care tocmai îl folosește o pictare nu are voie să
    ''' dispară sub ea (aceeași alegere ca în <see cref="FontBaseline"/>).</para>
    ''' </summary>
    Public Function ScaledFont(ctrl As Control, source As Font) As Font
        If source Is Nothing Then Return Nothing
        Try
            Dim factor As Single = TextFactorFor(ctrl)
            If Math.Abs(factor - 1.0F) < 0.0001F Then Return source

            Dim key As Tuple(Of Font, Single) = Tuple.Create(source, factor)
            Dim gata As Font = Nothing
            If _scaledFonts.TryGetValue(key, gata) Then Return gata

            Dim marime As Single = source.Size * factor
            If marime <= 0F Then Return source
            gata = New Font(source.FontFamily, marime, source.Style, source.Unit,
                            source.GdiCharSet, source.GdiVerticalFont)
            _scaledFonts(key) = gata
            Return gata
        Catch ex As Exception
            ' Predicat de pictură: o familie stricată nu are voie să arunce dintr-un OnPaint.
            GlobalErrorLog.Write("AppScaling.ScaledFont", ex)
            Return source
        End Try
    End Function

    ' Fonturile derivate, pe fontul-sursă. Se golește la fiecare schimbare de mărime.
    Private ReadOnly _scaledFonts As New Dictionary(Of Tuple(Of Font, Single), Font)()

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
            ' Fonturile derivate sunt bune doar pentru mărimea cu care au fost făcute.
            _scaledFonts.Clear()
            ThemeStore.SaveScaling(_mode, _manualFactor, _dpiUnaware, _textScale)
            Broadcast()
            RaiseEvent ScalingChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.SetTextScale", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Carries the text size to a control and all its children -- FIRST the geometric zoom
    ''' (<see cref="ApplyZoom"/>, once per root), then the fonts. <c>ThemeManager.Apply</c> calls
    ''' it LAST, after the theme wrote its fonts -- the order matters: the other way round the
    ''' "Colorat" scheme would restore the unscaled font over the enlargement and it would vanish
    ''' on one scheme only, which would have looked like a defect without a cause.
    ''' </summary>
    Public Sub ApplyTextScale(root As Control)
        If root Is Nothing Then Return
        Try
            If KBotDesignTime.IsDesignTime(root) Then Return   ' în designer se vede ce s-a autorit
            ScaleTree(root)
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.ApplyTextScale", ex)
            Throw
        End Try
    End Sub

    ' The geometric zoom on the root (moves the rectangles, once for the whole tree), then the
    ' fonts, control by control. Under AutoScaleMode.Dpi writing a font no longer triggers any
    ' platform autoscale, so the order of the two does not matter any more.
    Private Sub ScaleTree(root As Control)
        ApplyZoom(root)
        ScaleFontsRecursive(root)
    End Sub

    ' Children with a font of their own are scaled individually; the rest inherit from the form.
    Private Sub ScaleFontsRecursive(ctrl As Control)
        FontBaseline.ApplyScale(ctrl, TextFactorFor(ctrl))

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
            _scaledFonts.Clear()   ' under Fixed100 / Manual the derived fonts depend on the mode
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
        _scaledFonts.Clear()
    End Sub

    ''' <summary>
    ''' Scara pentru controlul dat: scara de ECRAN (modul ales) înmulțită cu MĂRIMEA cerută de
    ''' operator. Sunt două lucruri diferite adunate într-un singur număr, și trebuie să fie
    ''' împreună: la 150% pe un ecran, cu textul pus pe 125%, un rând trebuie să fie de 1,875 ori
    ''' cel de la 96 dpi — nu de 1,5 și nici de 1,25.
    '''
    ''' Pentru un control fără handle, unde <c>DeviceDpi</c> minte cu 96, răspunsul e 1. La design
    ''' time NU se scurtcircuitează: suprafața designerului e conștientă de DPI și desenează la
    ''' scara ecranului, deci și măsurile noastre trebuie să meargă la aceeași scară — vezi
    ''' rezumatul modulului. Mărirea textului e 1 acolo oricum (designerul nu citește theme.json).
    ''' </summary>
    Public Function FactorFor(ctrl As Control) As Single
        Try
            Return EcranFactor(ctrl) * _textScale
        Catch
            ' Predicat de pictură: „nu știu” înseamnă 1, niciodată o excepție dintr-un OnPaint.
            Return 1.0F
        End Try
    End Function

    ''' <summary>
    ''' Only what the PLATFORM does by itself for a control: <c>DeviceDpi / 96</c>, in every mode
    ''' -- the scale <c>AutoScaleMode.Dpi</c> has already multiplied every rectangle by (slice
    ''' 0066-02). 1 for a missing control. <see cref="ThemeFormFit"/> keeps it at capture, to know
    ''' by how much the client size it photographs is already multiplied.
    ''' </summary>
    Public Function PlatformFactorFor(ctrl As Control) As Single
        Try
            If ctrl Is Nothing Then Return 1.0F
            Return CSng(ctrl.DeviceDpi / 96.0)
        Catch
            Return 1.0F
        End Try
    End Function

    ''' <summary>
    ''' OUR zoom over the platform's scale: <see cref="FactorFor"/> divided by
    ''' <see cref="PlatformFactorFor"/>. Under <see cref="ScalingMode.Automatic"/> it is exactly
    ''' <see cref="TextScale"/>; under Fixed100 at 150% it is <c>96 / 144 x text</c>; under Manual
    ''' it is <c>factor x 96 / DeviceDpi x text</c>. This number goes on the rectangles through
    ''' <see cref="ApplyZoom"/> (<c>Control.Scale</c>) AND on the fonts through
    ''' <see cref="TextFactorFor"/>, so there is a single ruler. 1 at design time.
    ''' </summary>
    Public Function ZoomFor(ctrl As Control) As Single
        Try
            If ctrl IsNot Nothing AndAlso KBotDesignTime.IsDesignTime(ctrl) Then Return 1.0F
            Return FactorFor(ctrl) / PlatformFactorFor(ctrl)
        Catch
            Return 1.0F
        End Try
    End Function

    ' The zoom already put on each root (a form or a designed UserControl), so the next pass
    ' writes only the DIFFERENCE -- Control.Scale multiplies what is there now, not what was
    ' authored. ConditionalWeakTable: a closed window is not kept alive by its entry.
    Private ReadOnly _zoomApplied As New ConditionalWeakTable(Of Control, StrongBox(Of Single))()

    ''' <summary>
    ''' The zoom the given root has been multiplied by so far through <see cref="ApplyZoom"/>;
    ''' 1 if never. A reading seam for the bench and the tests.
    ''' </summary>
    Public Function AppliedZoomOf(root As Control) As Single
        Dim box As StrongBox(Of Single) = Nothing
        If root Is Nothing OrElse Not _zoomApplied.TryGetValue(root, box) Then Return 1.0F
        Return box.Value
    End Function

    ''' <summary>
    ''' Puts <see cref="ZoomFor"/> on the geometry of a root -- a form or a designed
    ''' <c>UserControl</c> -- through <c>Control.Scale</c>, which multiplies Bounds, Margin,
    ''' Padding, MinimumSize and the fixed styles of the tables uniformly, on it and on all its
    ''' children. Writes only the difference to the zoom already applied, so it is idempotent. The
    ''' designed roots below it (the views in the main form) are marked with the same zoom, so a
    ''' later pass on one of them does not multiply it a second time. Returns <c>True</c> when it
    ''' moved something. Nothing at design time and nothing on a control that is not a designed
    ''' root.
    ''' </summary>
    Public Function ApplyZoom(root As Control) As Boolean
        If root Is Nothing Then Return False
        Try
            If Not (TypeOf root Is Form OrElse TypeOf root Is UserControl) Then Return False
            If KBotDesignTime.IsDesignTime(root) Then Return False
            Dim want As Single = ZoomFor(root)
            Dim have As Single = AppliedZoomOf(root)
            Dim delta As Single = want / have
            If Math.Abs(delta - 1.0F) < 0.0001F Then Return False
            root.Scale(New SizeF(delta, delta))
            MarkZoom(root, want)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AppScaling.ApplyZoom", ex)
            Throw
        End Try
    End Function

    ' Marks the root and every designed root below it with the zoom just applied.
    Private Sub MarkZoom(ctrl As Control, zoom As Single)
        If TypeOf ctrl Is Form OrElse TypeOf ctrl Is UserControl Then
            Dim box As StrongBox(Of Single) = Nothing
            If _zoomApplied.TryGetValue(ctrl, box) Then
                box.Value = zoom
            Else
                _zoomApplied.Add(ctrl, New StrongBox(Of Single)(zoom))
            End If
        End If
        For Each child As Control In ctrl.Controls
            MarkZoom(child, zoom)
        Next
    End Sub

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
                ' The zoom and the fonts FIRST: Control.Scale moves the rectangles, so our own
                ' measures must be recomputed AFTER it, not before.
                ScaleTree(f)
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
