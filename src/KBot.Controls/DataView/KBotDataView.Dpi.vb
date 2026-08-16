Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' SCALAREA LA DPI a măsurilor proprii ale grilei (felia 0035) — perechea lui
''' <c>AdvancedTreeControl.Dpi.vb</c>, cu aceeași boală și același leac.
'''
''' <para><b>Ce se vedea.</b> Rândul, banda de antet și cea de subsol erau în pixeli bruți
''' (<c>RowHeight = 28</c>), în timp ce textul din ele se scala singur, fiind în puncte. La 100%
''' ieșea un rând înalt cu litere mici — mult aer în jurul lor, degeaba; la 150% textul ajungea
''' să nu mai încapă în rândul care nu crescuse deloc. Nu era o alegere de aspect, era o măsură
''' care lipsea.</para>
'''
''' <para><b>Leacul.</b> WinForms cheamă <see cref="Control.ScaleControl"/> pe fiecare copil la
''' autoscalarea formularului (<c>AutoScaleMode.Font</c>) și la fiecare schimbare de DPI
''' (aplicația e <c>PerMonitorV2</c>). Acolo ne scalăm măsurile noastre, ca orice control care se
''' respectă. Proprietățile publice rămân LOGICE (px la 96 dpi) — ce a scris operatorul, ce
''' serializează designerul; câmpurile cu care se pictează sunt cele scalate.</para>
'''
''' <para><b>Ce NU s-a atins.</b> Lățimile de coloană: cele auto-dimensionate se scalează deja
''' singure (se măsoară din textul real, în pixeli de ecran, plus spații trecute prin
''' <c>ScaleDpi</c>), iar cele fixate de operator au rămas logice — o coloană cu lățime pusă cu
''' mâna e o alegere pe care nu vrem s-o rescriem fără s-o vedem întâi pe ecran. E de urmărit
''' la prima verificare vizuală a grilei.</para>
''' </summary>
Partial Class KBotDataView
    ' Declarat AICI, în partiala care se ocupă de scară: interfața n-are decât un membru, iar
    ' acela e chiar mai jos. VB acceptă un Implements într-o partială oarecare a clasei.
    Implements IDpiScaledControl

    ' Scara măsurilor proprii. 1 = 96 dpi.
    '
    ' Sursa e DeviceDpi / 96, NU factorul primit de la WinForms. Sunt aproape la fel, dar nu
    ' identice: `AutoScaleMode.Font` dă raportul dintre înălțimile de font, care la 150% iese
    ' ~1,45, nu 1,5. Și tot restul picturii grilei folosește deja `ScaleDpi` (DeviceDpi) pentru
    ' constantele ei. Dacă lățimile ar veni din factor iar spațiile dintre pictograme din
    ' DeviceDpi, podeaua de lățime și desenul ar ieși din DOUĂ formule și coloana s-ar putea
    ' strâmta cu câțiva pixeli sub ce se pictează efectiv — chiar scăparea descrisă în nota de
    ' DPI din KBotDataView.HeaderIcons. O singură sursă, deci.
    '
    ' `ScaleControl` rămâne DECLANȘATORUL (WinForms îl cheamă la autoscalare și la fiecare
    ' schimbare de DPI), nu sursa. Așa nici nu se acumulează rotunjiri dacă e chemat de două ori.
    Private _dpiScale As Single = 1.0F

    ''' <summary>Scara curentă (1 = 96 dpi). Diagnostic, nu setare.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DpiScaleX As Single
        Get
            Return _dpiScale
        End Get
    End Property

    ''' <summary>Aceeași scară pe verticală — DeviceDpi nu are două axe.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DpiScaleY As Single
        Get
            Return _dpiScale
        End Get
    End Property

    ''' <summary>Valoare logică (px @96dpi) → px pe orizontală, la scara curentă.</summary>
    Friend Function SX(logical As Integer) As Integer
        Return CInt(Math.Round(logical * _dpiScale))
    End Function

    ''' <summary>Valoare logică (px @96dpi) → px pe verticală, la scara curentă.</summary>
    Friend Function SY(logical As Integer) As Integer
        Return CInt(Math.Round(logical * _dpiScale))
    End Function

    ''' <summary>
    ''' Drumul invers pe orizontală: px de ecran → valoare logică. Îl folosește tragerea de
    ''' margine a coloanei, singura scriere de lățime care pornește din pixeli de ecran.
    ''' </summary>
    Friend Function UnscaleX(device As Integer) As Integer
        If _dpiScale <= 0 Then Return device
        Return CInt(Math.Round(device / _dpiScale))
    End Function

    ' Recitește scara. Din felia 0036 răspunsul vine din AppScaling — sursa unică — fiindcă
    ' operatorul o poate fixa la 100% sau pune un factor al lui; pe modul automat e exact
    ' DeviceDpi / 96, adică fix ce se calcula aici. Design time rămâne 1, tratat acolo.
    Private Function RefreshDpiScale() As Boolean
        Dim noua As Single = AppScaling.FactorFor(Me)
        If noua <= 0 OrElse noua = _dpiScale Then Return False
        _dpiScale = noua
        Return True
    End Function

    Protected Overrides Sub ScaleControl(factor As SizeF, specified As BoundsSpecified)
        Try
            MyBase.ScaleControl(factor, specified)
            If RefreshDpiScale() Then ApplyMetricScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ScaleControl", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Recitește scara și reface măsurile dacă s-a schimbat ceva. O cheamă
    ''' <c>OnHandleCreated</c> (partiala .Theming): handle-ul e prima clipă în care
    ''' <c>DeviceDpi</c> spune adevărul — până atunci întoarce 96 chiar și pe un ecran la 150%.
    ''' Fără el, o grilă care nu trece printr-o autoscalare (una construită din cod, nu din
    ''' designer) ar rămâne la scara 1.
    ''' </summary>
    Friend Sub SyncDpiScale()
        If RefreshDpiScale() Then ApplyMetricScale()
    End Sub

    ''' <summary>
    ''' <see cref="IDpiScaledControl.RefreshDpiMetrics"/> — poarta prin care
    ''' <c>AppScaling.Broadcast</c> ajunge la măsurile noastre când operatorul schimbă modul de
    ''' scalare. Aceeași treabă ca <see cref="SyncDpiScale"/>, doar că vizibilă din
    ''' <c>KBot.Theming</c>: aceea e <c>Friend</c>, deci nu poate implementa o interfață publică.
    ''' </summary>
    Public Sub RefreshDpiMetrics() Implements IDpiScaledControl.RefreshDpiMetrics
        SyncDpiScale()
    End Sub

    ' Mutarea pe un monitor cu altă scalare. `ScaleControl` vine și el, dar numai dacă părintele
    ' chiar rescalează copiii; ăsta e semnalul sigur.
    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        Try
            MyBase.OnDpiChangedAfterParent(e)
            If RefreshDpiScale() Then ApplyMetricScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Reface măsurile scalate din perechile lor logice. Idempotentă: recalculează din valoarea
    ''' logică, nu compune peste cea scalată.
    ''' </summary>
    Private Sub ApplyMetricScale()
        Try
            _rowHeight = SY(_rowHeightLogic)
            _headerHeight = SY(_headerHeightLogic)
            _footerHeight = SY(_footerHeightLogic)
            ' Lățimile de coloană: fiecare își reface lățimea PICTATĂ din cea cerută de operator.
            ' Nu se scalează lățimea curentă (aceea poate fi rezultatul unei umpleri sau al unei
            ' strâmtări) — se pornește de fiecare dată de la valoarea cerută, exact ca trecerea de
            ' layout, care începe tot cu RestoreAuthoredWidth.
            For Each c In _columns
                c.RefreshWidthScale()
            Next
            InvalidateHeaderHeight()
            InvalidateBands()
            LayoutChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ApplyMetricScale", ex)
        End Try
    End Sub

End Class
