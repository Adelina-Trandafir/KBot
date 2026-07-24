Option Strict On
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Previzualizare de REZERVĂ a documentului DDF (felia 0020-03, opțiunea A a planului §6):
''' lansează PDF-ul cu handler-ul implicit, găsește fereastra Adobe Reader și o reparentează
''' (SetParent) într-un panou-gazdă, curățând stilurile WS_POPUP / WS_CAPTION / WS_THICKFRAME
''' și re-așezând-o la redimensionare.
'''
''' Se codează DEFENSIV, fiindcă fiecare dintre aceste puncte a mușcat deja tiparul (planul §6):
'''   * Reader e practic mono-instanță -> Process.MainWindowHandle e adesea Zero sau arată spre
'''     o fereastră preexistentă. Căutăm cu EnumWindows o fereastră de nivel superior a cărei
'''     clasă aparține Reader-ului ȘI al cărei titlu conține numele de bază al fișierului, cu
'''     timeout mărginit, și renunțăm curat (mesaj românesc) în loc să prindem fereastra greșită.
'''   * La dispose și la fiecare schimbare de document restaurăm părintele original înainte de
'''     a închide, ca să nu lăsăm un proces Adobe orfan.
'''   * Focus-ul și tastatura peste granița de proces NU se comportă ca la un copil nativ. Nu
'''     încercăm să reparăm asta; o documentăm.
'''   * Dacă fereastra nu apare în timeout, cădem pe Process.Start pe fișier fără găzduire și
'''     o spunem în zona de mesaj.
'''
''' AVERTISMENT (planul §8 ter): NU invoca semnarea (Adobe) cât timp această suprafață ține o
''' fereastră găzduită — lansarea unui mod de semnare peste același proces cere probleme.
''' </summary>
Public Class ReaderHostPreview
    Implements IDdfPreview, IThemedControl

    ' Timeout la căutarea ferestrei Reader (ms) și pasul de polling.
    Private Const FIND_TIMEOUT_MS As Integer = 8000
    Private Const FIND_POLL_MS As Integer = 150

    Public Event GenerateRequested As EventHandler Implements IDdfPreview.GenerateRequested

    ' Fereastra Adobe găzduită acum + părintele ei original (pentru restaurare la detach).
    Private _hostedWindow As IntPtr = IntPtr.Zero
    Private _originalParent As IntPtr = IntPtr.Zero
    Private _originalStyle As IntPtr = IntPtr.Zero
    ' Procesul Reader pe care l-am pornit noi (dacă l-am pornit) — ca să nu lăsăm orfani.
    Private _readerProcess As Process

    Public Sub New()
        InitializeComponent()
        ShowMessage("Selectați o revizie din arbore.")
    End Sub

    Public ReadOnly Property Surface As Control Implements IDdfPreview.Surface
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Afișează documentul. Fișier lipsă -> suprafața „document lipsă" (contract IDdfPreview,
    ''' niciodată o excepție). La orice document nou detașăm mai întâi fereastra găzduită curent.
    ''' </summary>
    Public Sub ShowDocument(pdfPath As String, exists As Boolean) Implements IDdfPreview.ShowDocument
        Try
            DetachReader()

            If String.IsNullOrWhiteSpace(pdfPath) Then
                ShowMessage("Selectați o revizie din arbore.")
                Return
            End If
            If Not exists Then
                ShowMissing()
                Return
            End If

            ShowHost()
            EmbedReader(pdfPath)
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.ShowDocument", ex)
            ShowMessage("Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' Pornește Reader pe fișier, îi găsește fereastra și o reparentează în pnlHost. Dacă nu
    ' o găsește în timeout, cade pe deschiderea simplă (fără găzduire) și spune asta.
    Private Sub EmbedReader(pdfPath As String)
        ' Pornim documentul cu handler-ul implicit (adesea Adobe deja rulează).
        Try
            _readerProcess = Process.Start(New ProcessStartInfo(pdfPath) With {.UseShellExecute = True})
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.EmbedReader", ex)
            ShowMessage("Documentul nu a putut fi deschis cu aplicația implicită.")
            Return
        End Try

        Dim baseName As String = Path.GetFileNameWithoutExtension(pdfPath)
        Dim hwnd As IntPtr = FindReaderWindow(baseName)
        If hwnd = IntPtr.Zero Then
            ' Nu am găsit fereastra — o lăsăm deschisă separat și anunțăm operatorul.
            ShowMessage("Documentul s-a deschis în Adobe Reader (fără încorporare).")
            Return
        End If

        HostWindow(hwnd)
    End Sub

    ' Caută o fereastră de nivel superior a Reader-ului al cărei titlu conține numele fișierului,
    ' cu timeout mărginit. Nothing (Zero) dacă nu apare — apelantul cade pe deschiderea simplă.
    Private Function FindReaderWindow(baseName As String) As IntPtr
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(FIND_TIMEOUT_MS)
        Do
            Dim found As IntPtr = IntPtr.Zero
            EnumWindows(
                Function(h, l)
                    If Not IsWindowVisible(h) Then Return True
                    Dim cls As String = GetClass(h)
                    ' Clasele ferestrei principale Adobe (Reader/Acrobat, versiuni diferite).
                    If cls.IndexOf("Acrobat", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
                       cls.IndexOf("AdobeAcrobat", StringComparison.OrdinalIgnoreCase) < 0 Then Return True
                    Dim title As String = GetTitle(h)
                    If title.IndexOf(baseName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                        found = h
                        Return False   ' oprim enumerarea
                    End If
                    Return True
                End Function, IntPtr.Zero)

            If found <> IntPtr.Zero Then Return found
            Threading.Thread.Sleep(FIND_POLL_MS)
        Loop While DateTime.UtcNow < deadline
        Return IntPtr.Zero
    End Function

    ' Reparentează fereastra găsită în pnlHost, curățând stilurile de fereastră de sine
    ' stătătoare (titlu / margine / popup) și așezând-o pe tot panoul.
    Private Sub HostWindow(hwnd As IntPtr)
        _hostedWindow = hwnd
        _originalParent = GetParent(hwnd)
        _originalStyle = GetWindowLongPtrSafe(hwnd, GWL_STYLE)

        Dim style As Long = _originalStyle.ToInt64()
        style = style And Not (WS_CAPTION Or WS_THICKFRAME Or WS_POPUP)
        style = style Or WS_CHILD
        SetWindowLongPtrSafe(hwnd, GWL_STYLE, New IntPtr(style))

        SetParent(hwnd, pnlHost.Handle)
        LayoutHostedWindow()
    End Sub

    Private Sub LayoutHostedWindow()
        If _hostedWindow = IntPtr.Zero Then Return
        MoveWindow(_hostedWindow, 0, 0, pnlHost.ClientSize.Width, pnlHost.ClientSize.Height, True)
    End Sub

    Private Sub pnlHost_SizeChanged(sender As Object, e As EventArgs) Handles pnlHost.SizeChanged
        Try
            LayoutHostedWindow()
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.pnlHost_SizeChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Restaurează părintele/stilul original al ferestrei găzduite și închide procesul Reader
    ''' pe care l-am pornit — ca să nu rămână un proces orfan. Apelat la fiecare schimbare de
    ''' document și la dispose. „Very safe" prin construcție: fiecare pas e best-effort.
    ''' </summary>
    Friend Sub DetachReader()
        Try
            If _hostedWindow <> IntPtr.Zero Then
                ' Restaurăm stilul și părintele înainte de a închide.
                If _originalStyle <> IntPtr.Zero Then
                    SetWindowLongPtrSafe(_hostedWindow, GWL_STYLE, _originalStyle)
                End If
                SetParent(_hostedWindow, _originalParent)
                _hostedWindow = IntPtr.Zero
                _originalParent = IntPtr.Zero
                _originalStyle = IntPtr.Zero
            End If

            If _readerProcess IsNot Nothing Then
                Try
                    If Not _readerProcess.HasExited Then _readerProcess.CloseMainWindow()
                Catch
                    ' Best-effort — nu propagăm.
                End Try
                _readerProcess.Dispose()
                _readerProcess = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.DetachReader", ex)
        End Try
    End Sub

    Public Sub Clear() Implements IDdfPreview.Clear
        Try
            DetachReader()
            ShowMessage("Selectați o revizie din arbore.")
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.Clear", ex)
        End Try
    End Sub

    Private Sub btnGenereaza_Click(sender As Object, e As EventArgs) Handles btnGenereaza.Click
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ' ── Stări ─────────────────────────────────────────────────────────────────
    Private Sub ShowHost()
        pnlHost.Visible = True
        pnlMissing.Visible = False
        lblMessage.Visible = False
    End Sub

    Private Sub ShowMissing()
        pnlHost.Visible = False
        pnlMissing.Visible = True
        lblMessage.Visible = False
    End Sub

    Private Sub ShowMessage(message As String)
        lblMessage.Text = message
        pnlHost.Visible = False
        pnlMissing.Visible = False
        lblMessage.Visible = True
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            pnlHost.BackColor = p.SurfaceColor
            pnlMissing.BackColor = p.SurfaceAltColor
            tblMissing.BackColor = p.SurfaceAltColor
            lblMissing.ForeColor = p.TextDimColor
            lblMissing.BackColor = Color.Transparent
            lblMessage.ForeColor = p.TextDimColor
            lblMessage.BackColor = p.SurfaceAltColor
            btnGenereaza.BackColor = p.AccentColor
            btnGenereaza.ForeColor = p.AccentTextColor
            btnGenereaza.FlatAppearance.BorderColor = p.AccentColor
        Catch ex As Exception
            GlobalErrorLog.Write("ReaderHostPreview.ApplyTheme", ex)
        End Try
    End Sub

    ' ── Win32 interop ─────────────────────────────────────────────────────────
    Private Const GWL_STYLE As Integer = -16
    Private Const WS_CHILD As Long = &H40000000L
    Private Const WS_POPUP As Long = &H80000000L
    Private Const WS_CAPTION As Long = &HC00000L
    Private Const WS_THICKFRAME As Long = &H40000L

    Private Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function EnumWindows(callback As EnumWindowsProc, extra As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindowVisible(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetParent(hWndChild As IntPtr, hWndNewParent As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetParent(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function MoveWindow(hWnd As IntPtr, x As Integer, y As Integer,
                                       w As Integer, h As Integer, repaint As Boolean) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetClassName(hWnd As IntPtr, lpClassName As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetWindowText(hWnd As IntPtr, lpString As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    ' GetWindowLongPtr/SetWindowLongPtr nu există pe 32-bit; alegem varianta potrivită la rulare.
    <DllImport("user32.dll", EntryPoint:="GetWindowLongPtrW")>
    Private Shared Function GetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="GetWindowLongW")>
    Private Shared Function GetWindowLong32(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongPtrW")>
    Private Shared Function SetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongW")>
    Private Shared Function SetWindowLong32(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    Private Shared Function GetWindowLongPtrSafe(hWnd As IntPtr, nIndex As Integer) As IntPtr
        If IntPtr.Size = 8 Then Return GetWindowLongPtr64(hWnd, nIndex)
        Return New IntPtr(GetWindowLong32(hWnd, nIndex))
    End Function

    Private Shared Function SetWindowLongPtrSafe(hWnd As IntPtr, nIndex As Integer, val As IntPtr) As IntPtr
        If IntPtr.Size = 8 Then Return SetWindowLongPtr64(hWnd, nIndex, val)
        Return New IntPtr(SetWindowLong32(hWnd, nIndex, val.ToInt32()))
    End Function

    Private Shared Function GetClass(hWnd As IntPtr) As String
        Dim sb As New StringBuilder(256)
        GetClassName(hWnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

    Private Shared Function GetTitle(hWnd As IntPtr) As String
        Dim sb As New StringBuilder(512)
        GetWindowText(hWnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

End Class
