Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' SCALAREA LA DPI a măsurilor proprii ale arborelui (felia 0035).
'''
''' <para><b>Ce era stricat.</b> Un control desenat de noi are două feluri de măsuri: literele și
''' geometria. Literele se scalează SINGURE — un font e în puncte, iar la 150% același font se
''' pictează cu 1,5× mai mulți pixeli. Geometria, în schimb, era în pixeli bruți:
''' <c>ItemHeight = 22</c> rămânea 22 de pixeli și la 100%, și la 150%. Rezultatul e exact ce se
''' vedea pe ecran: la 100% un rând înalt cu litere mici și mult aer în jur, iar la 150% un rând
''' care nu mai încape textul. Aceeași boală o avea și grila (vezi
''' <c>KBotDataView.Dpi.vb</c>) — de aceea cele două ecrane arătau prost în AceLAȘI fel.</para>
'''
''' <para><b>Leacul e cel al platformei, nu unul inventat aici.</b> WinForms cheamă
''' <see cref="Control.ScaleControl"/> pe fiecare copil când formularul își face autoscalarea
''' (<c>AutoScaleMode.Font</c>) și din nou la fiecare schimbare de DPI (aplicația e
''' <c>PerMonitorV2</c> — vezi <c>Program.Main</c>). Un control obișnuit își scalează acolo
''' <c>Bounds</c>/<c>Padding</c>; noi ne scalăm, în plus, măsurile NOASTRE. Nimic nou de chemat
''' din gazdă, nimic de ținut minte.</para>
'''
''' <para><b>Două valori pentru fiecare măsură.</b> Proprietatea publică rămâne LOGICĂ (px la
''' 96 dpi): asta a scris operatorul în designer, asta îi întoarce getter-ul, asta se
''' serializează în <c>.Designer.vb</c>. Câmpul intern, cel cu care se pictează, e cel SCALAT.
''' Dacă getter-ul ar întoarce valoarea scalată, designerul ar reciti 33 acolo unde s-a scris 22
''' și ar îngheța 33 — iar la următoarea deschidere s-ar scala încă o dată. E aceeași capcană pe
''' care o descrie regula casei despre <c>ShouldSerialize</c>, doar că pe numere.</para>
'''
''' <para><b>În designer nu se scalează nimic.</b> Suprafața de proiectare a Visual Studio
''' desenează la 96 dpi (<c>DesignToolsServer</c>), deci ce se vede acolo e valoarea logică —
''' adică exact ce s-a tastat. <see cref="KBotDesignTime"/> e cel care ne spune unde suntem.</para>
''' </summary>
Partial Public Class AdvancedTreeControl
    ' Declarat AICI, în partiala care se ocupă de scară: interfața n-are decât un membru, iar
    ' acela e chiar mai jos. VB acceptă un Implements într-o partială oarecare a clasei.
    Implements IDpiScaledControl

    ' Scara măsurilor proprii. 1 = 96 dpi. Sursa e DeviceDpi / 96, NU factorul primit de la
    ' WinForms: `AutoScaleMode.Font` dă raportul înălțimilor de font (~1,45 la 150%, nu 1,5), iar
    ' constantele din pictură trec deja prin `ThemeShapes.ScaleDpi`, care citește DeviceDpi. Două
    ' surse ar însemna măsuri care nu se potrivesc între ele cu câțiva pixeli.
    '
    ' `ScaleControl` rămâne DECLANȘATORUL (WinForms îl cheamă la autoscalarea formularului și la
    ' fiecare schimbare de DPI), nu sursa — așa nu se acumulează rotunjiri la apeluri repetate.
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

    ''' <summary>Drumul invers: px de pe ecran → valoare logică (folosit de înălțimea automată).</summary>
    Friend Function UnscaleY(device As Integer) As Integer
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
            GlobalErrorLog.Write("AdvancedTreeControl.ScaleControl", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Recitește scara și reface măsurile dacă s-a schimbat ceva. O cheamă
    ''' <c>OnHandleCreated</c> (partiala .Overrides): handle-ul e prima clipă în care
    ''' <c>DeviceDpi</c> spune adevărul — până atunci întoarce 96 chiar și pe un ecran la 150%.
    ''' Fără el, un arbore care nu trece printr-o autoscalare (unul construit din cod, nu din
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

    ' Mutarea pe un monitor cu altă scalare — semnalul sigur, indiferent dacă părintele rescalează
    ' sau nu copiii.
    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        Try
            MyBase.OnDpiChangedAfterParent(e)
            If RefreshDpiScale() Then ApplyMetricScale()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Reface TOATE măsurile scalate din perechile lor logice. Se cheamă după fiecare schimbare
    ''' de scară; e idempotentă (nu compune, ci recalculează din logic).
    ''' </summary>
    Private Sub ApplyMetricScale()
        Try
            _itemHeight = SY(_itemHeightLogic)
            _headerHeight = SY(_headerHeightLogic)
            _footerHeight = SY(_footerHeightLogic)
            m_ExpanderSize = SX(_expanderSizeLogic)
            m_Indent = SX(_indentLogic)
            _checkBoxSize = SX(_checkBoxSizeLogic)
            _leftIconSize = New Size(SX(_leftIconSizeLogic.Width), SY(_leftIconSizeLogic.Height))
            _rightIconSize = New Size(SX(_rightIconSizeLogic.Width), SY(_rightIconSizeLogic.Height))
            _minimumCollapsedWidth = SX(_minimumCollapsedWidthLogic)

            RefreshSearchBarMetrics()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ApplyMetricScale", ex)
        End Try
    End Sub

End Class
