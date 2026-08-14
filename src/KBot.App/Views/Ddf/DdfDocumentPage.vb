Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Pagina «Document» a vederii DDF: PDF-ul REAL (<see cref="ReaderHostPreview"/>), distinct de
''' «Vizualizare» (reconstrucția din XML XFA). Aduce cu ea banda de setări Adobe (felia 0024) și
''' banda de butoane de jos.
'''
''' ÎNCĂRCARE LENEȘĂ, gardă pe perechea (cale, existență): <see cref="SetContext"/> doar REȚINE
''' ținta; încorporarea — singurul loc de unde poate porni Adobe — se face abia când pagina
''' devine vizibilă. De aceea garda nu poate fi pe cale singură: după o generare, calea rămâne
''' aceeași și doar existența se întoarce din False în True, iar o gardă pe cale ar sări exact
''' re-încorporarea care trebuia făcută.
''' </summary>
Public Class DdfDocumentPage
    Implements IDdfPage, IThemedControl

    ' Ținta cerută (reținută) și ce e efectiv încorporat acum — perechea (cale, existență).
    Private _pendingPath As String
    Private _pendingExists As Boolean
    Private _shownPath As String
    Private _shownExists As Boolean
    ' Se populează combo-urile de setări chiar acum? Atunci o selecție programatică nu are voie
    ' să declanșeze o salvare.
    Private _suppressAdobeComboEvent As Boolean

    Public Event GenerateRequested As EventHandler Implements IDdfPage.GenerateRequested
    ' Pagina «Document» nu listează fișiere -> nu ridică niciodată acest eveniment. Rămâne
    ' declarat ca gazda să se poată abona uniform la toate paginile.
    Public Event FileActivated As EventHandler(Of String) Implements IDdfPage.FileActivated

    Public Sub New()
        InitializeComponent()
        BuildAdobeCombos()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfPage.PageKey
        Get
            Return "document"
        End Get
    End Property

    ''' <summary>
    ''' Reține ținta PDF a nodului curent. Nimic selectat / o rădăcină de lună -&gt; ținta se
    ''' golește. NU încorporează nimic cât timp pagina e ascunsă (vezi nota clasei).
    ''' </summary>
    Public Sub SetContext(ctx As DdfPageContext) Implements IDdfPage.SetContext
        Try
            ' O rădăcină de lună ajunge aici cu PdfPath gol — părintele nu compune cale decât
            ' pentru o frunză (sau pentru fișierul ales din listă), deci nu mai verificăm IsRoot.
            If ctx Is Nothing OrElse String.IsNullOrEmpty(ctx.PdfPath) Then
                _pendingPath = Nothing
                _pendingExists = False
            Else
                _pendingPath = ctx.PdfPath
                _pendingExists = ctx.PdfExists
            End If
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Graniță UI: loghează și înghite. Aici — și DOAR aici — poate porni Adobe.
    Private Sub DdfDocumentPage_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Try
            MountIfVisible()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.VisibleChanged", ex)
        End Try
    End Sub

    ' Încorporează ținta reținută, dar numai dacă pagina e pe ecran ȘI perechea (cale, existență)
    ' s-a schimbat față de ce e afișat — ca să nu re-încorporăm (și să nu relansăm Adobe) la
    ' fiecare comutare de pagină.
    Private Sub MountIfVisible()
        If Not Visible Then Return
        If String.Equals(_shownPath, _pendingPath, StringComparison.Ordinal) AndAlso
           _shownExists = _pendingExists Then Return

        _shownPath = _pendingPath
        _shownExists = _pendingExists
        If String.IsNullOrEmpty(_pendingPath) Then
            previewPdf.Clear()
        Else
            previewPdf.ShowDocument(_pendingPath, _pendingExists)
        End If
    End Sub

    ' Trivial: ridică mai departe cererea de generare spre gazdă (DdfView).
    Private Sub previewPdf_GenerateRequested(sender As Object, e As EventArgs) _
        Handles previewPdf.GenerateRequested
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ' ── Setările gazdei Adobe (felia 0024) ───────────────────────────────────
    ' Combo-urile benzii de sus. Se umplu din setările salvate; o schimbare se PERSISTĂ imediat
    ' și se aplică documentului afișat acum, ca operatorul să vadă efectul fără să repornească.
    Private Sub BuildAdobeCombos()
        Try
            _suppressAdobeComboEvent = True
            Try
                For Each m As AdobeViewerMode In New AdobeViewerMode() {
                    AdobeViewerMode.Auto, AdobeViewerMode.Modern, AdobeViewerMode.Classic}
                    cboAdobeMod.Items.Add(New AdobeModeItem(m))
                Next
                For Each n As AdobeNewInstanceMode In New AdobeNewInstanceMode() {
                    AdobeNewInstanceMode.Auto, AdobeNewInstanceMode.Da, AdobeNewInstanceMode.Nu}
                    cboAdobeInst.Items.Add(New AdobeNewInstanceItem(n))
                Next
                For Each g As AdobePreviewEngine In New AdobePreviewEngine() {
                    AdobePreviewEngine.WindowHost, AdobePreviewEngine.ActiveX}
                    cboAdobeMotor.Items.Add(New AdobeEngineItem(g))
                Next
                ' Listele de mai sus sunt etichete pure, deci se pot construi oriunde. Citirea
                ' setărilor SALVATE, în schimb, nu se face în DESIGNER: acolo `AppDir` e folderul
                ' lui devenv.exe, deci `kbot_paths.json` lipsește oricum, iar singurul efect ar fi
                ' o selecție căzută pe «Automat» care nu spune nimic despre mașina operatorului.
                ' (Aceeași grijă ca în constructorul lui ReaderHostPreview — până la felia 0032,
                ' această metodă rula doar din constructorul de RULARE al vederii DdfView.)
                If KBotDesignTime.IsDesignTime(Me) Then Return
                SelectAdobeMode(AdobeViewerSettings.CurrentMode().Value)
                SelectAdobeNewInstance(AdobeViewerSettings.CurrentNewInstance().Value)
                SelectAdobeEngine(AdobeViewerSettings.CurrentEngine().Value)
            Finally
                _suppressAdobeComboEvent = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.BuildAdobeCombos", ex)
            Throw
        End Try
    End Sub

    Private Sub SelectAdobeMode(mode As AdobeViewerMode)
        For i As Integer = 0 To cboAdobeMod.Items.Count - 1
            Dim item As AdobeModeItem = TryCast(cboAdobeMod.Items(i), AdobeModeItem)
            If item IsNot Nothing AndAlso item.Mode = mode Then
                cboAdobeMod.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    Private Sub SelectAdobeNewInstance(mode As AdobeNewInstanceMode)
        For i As Integer = 0 To cboAdobeInst.Items.Count - 1
            Dim item As AdobeNewInstanceItem = TryCast(cboAdobeInst.Items(i), AdobeNewInstanceItem)
            If item IsNot Nothing AndAlso item.Mode = mode Then
                cboAdobeInst.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    Private Sub SelectAdobeEngine(engine As AdobePreviewEngine)
        For i As Integer = 0 To cboAdobeMotor.Items.Count - 1
            Dim item As AdobeEngineItem = TryCast(cboAdobeMotor.Items(i), AdobeEngineItem)
            If item IsNot Nothing AndAlso item.Engine = engine Then
                cboAdobeMotor.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    ' Graniță UI: loghează și înghite. Toate trei combo-urile intră aici — setările se salvează
    ' împreună, fiindcă toate descriu aceeași suprafață de previzualizare.
    Private Sub AdobeSetting_Changed(sender As Object, e As EventArgs) _
        Handles cboAdobeMod.SelectedIndexChanged, cboAdobeInst.SelectedIndexChanged,
                cboAdobeMotor.SelectedIndexChanged
        Try
            If _suppressAdobeComboEvent Then Return
            Dim mode As AdobeModeItem = TryCast(cboAdobeMod.SelectedItem, AdobeModeItem)
            Dim inst As AdobeNewInstanceItem = TryCast(cboAdobeInst.SelectedItem, AdobeNewInstanceItem)
            Dim motor As AdobeEngineItem = TryCast(cboAdobeMotor.SelectedItem, AdobeEngineItem)
            If mode Is Nothing OrElse inst Is Nothing OrElse motor Is Nothing Then Return

            ' «Mod» și «instanță nouă» descriu FEREASTRA găzduită; pe motorul ActiveX nu au efect,
            ' iar combo-urile o spun în loc să pară că fac ceva.
            Dim onActiveX As Boolean = motor.Engine = AdobePreviewEngine.ActiveX
            cboAdobeMod.Enabled = Not onActiveX
            cboAdobeInst.Enabled = Not onActiveX

            Dim saved As Boolean = AdobeViewerSettings.Persist(mode.Mode, inst.Mode, motor.Engine)
            ' Setarea rămâne activă pentru sesiune, dar nu s-a putut scrie: o spunem pe banda de
            ' aviz a paginii, nu o ascundem — altfel operatorul o va regăsi schimbată la
            ' următoarea pornire. (Până la felia 0032 mesajul mergea în `lblEmpty` al vederii,
            ' care a rămas la părinte odată cu arborele.)
            lblAvizSetari.Visible = Not saved
            If Not saved Then
                lblAvizSetari.Text = "Setarea Adobe s-a aplicat, dar nu a putut fi salvată. Detalii în jurnalul de erori."
            End If
            ' Se aplică pe documentul afișat ACUM (geometria); parametrii de lansare («/n», «/s»,
            ' /A) intră în vigoare la documentul următor — nu se poate reporni un proces în curs.
            previewPdf.ReapplySettings()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.AdobeSetting_Changed", ex)
        End Try
    End Sub

    ''' <summary>Cascadă: chrome-ul paginii + suprafața PDF (care se auto-temează).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            pnlAdobe.BackColor = p.SurfaceAltColor
            lblAdobeMod.ForeColor = p.TextDimColor
            lblAdobeMod.BackColor = Color.Transparent
            lblAdobeInst.ForeColor = p.TextDimColor
            lblAdobeInst.BackColor = Color.Transparent
            lblAdobeMotor.ForeColor = p.TextDimColor
            lblAdobeMotor.BackColor = Color.Transparent
            cboAdobeMod.BackColor = p.InputBackColor
            cboAdobeMod.ForeColor = p.InputTextColor
            cboAdobeInst.BackColor = p.InputBackColor
            cboAdobeInst.ForeColor = p.InputTextColor
            cboAdobeMotor.BackColor = p.InputBackColor
            cboAdobeMotor.ForeColor = p.InputTextColor
            lblAvizSetari.ForeColor = p.ErrorColor
            lblAvizSetari.BackColor = p.SurfaceAltColor
            pnlBottomButtons.BackColor = p.SurfaceAltColor

            previewPdf.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfDocumentPage.ApplyTheme", ex)
        End Try
    End Sub

End Class

''' <summary>
''' O intrare din combo-ul «Mod vizualizator Adobe»: valoarea + eticheta românească. Există ca să
''' NU se compare texte de interfață când se citește selecția. POCO -&gt; fără Try/Catch.
''' </summary>
Friend NotInheritable Class AdobeModeItem
    Public ReadOnly Property Mode As AdobeViewerMode

    Public Sub New(mode As AdobeViewerMode)
        Me.Mode = mode
    End Sub

    Public Overrides Function ToString() As String
        Return AdobeViewerSettings.ModeLabel(Mode)
    End Function
End Class

''' <summary>O intrare din combo-ul «Motor previzualizare». POCO -&gt; fără Try/Catch.</summary>
Friend NotInheritable Class AdobeEngineItem
    Public ReadOnly Property Engine As AdobePreviewEngine

    Public Sub New(engine As AdobePreviewEngine)
        Me.Engine = engine
    End Sub

    Public Overrides Function ToString() As String
        Return AdobeViewerSettings.EngineLabel(Engine)
    End Function
End Class

''' <summary>O intrare din combo-ul «Instanță nouă Adobe». POCO -&gt; fără Try/Catch.</summary>
Friend NotInheritable Class AdobeNewInstanceItem
    Public ReadOnly Property Mode As AdobeNewInstanceMode

    Public Sub New(mode As AdobeNewInstanceMode)
        Me.Mode = mode
    End Sub

    Public Overrides Function ToString() As String
        Return AdobeViewerSettings.NewInstanceLabel(Mode)
    End Function
End Class
