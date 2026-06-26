Imports System.IO
Imports System.IO.Pipes
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports GeneralClasses

Partial Public Class KBOT_IPC
    ' =========================
    ' DEFINIȚII WINAPI & CONSTANTE
    ' =========================
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function RegisterWindowMessage(ByVal lpString As String) As Integer
    End Function

    ' Delegat pentru callback-ul de enumerare
    Private Delegate Function EnumWindowsProc(ByVal hWnd As IntPtr, ByVal lParam As IntPtr) As Boolean

    ' EnumChildWindows: Cauta recursiv toti copiii unui parinte
    <DllImport("user32.dll")>
    Private Shared Function EnumChildWindows(ByVal hWndParent As IntPtr, ByVal lpEnumFunc As EnumWindowsProc, ByVal lParam As IntPtr) As Boolean
    End Function

    ' GetWindowText: Citeste titlul ferestrei
    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function GetWindowText(ByVal hWnd As IntPtr, ByVal lpString As StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function GetWindowTextLength(ByVal hWnd As IntPtr) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function IsWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function
    ' =========================
    ' VARIABILE LOCALE
    ' =========================
    Private WithEvents _executor As WorkflowExecutor

    ' =========================
    ' WNDPROC - COMUNICARE CU ACCESS (RECEIVE ONLY)
    ' =========================
    ' Comenzi convenite cu Access (WM_USER + X)
    Private Const CMD_HELLO As Integer = &H400 + 101      ' WM_USER + 101
    Private Const CMD_START_JOB As Integer = &H400 + 102      ' WM_USER + 102
    Private Const CMD_STOP_JOB As Integer = &H400 + 103           ' WM_USER + 103
    Private Const CMD_RECONNECT As Integer = &H400 + 104       ' WM_USER + 104
    Private Const CMD_EXIT As Integer = &H400 + 105            ' WM_USER + 105
    Private Const CMD_SAVE_LOG As Integer = &H400 + 106   ' WM_USER + 106
    Private Const CMD_SHOW_BROWSER As Integer = &H400 + 107 ' WM_USER + 107
    Private Const CMD_SHOW_HISTORY As Integer = &H400 + 108 ' WM_USER + 108

    Private Const CMD_VBA_READY As Integer = &H400 + 200 ' WM_USER + 200

    Private Const CMD_GET_EXTRASE As Integer = &H400 + 300 ' WM_USER + 300

    ' Declaram Listener-ul stabil
    Private _ipcListener As IPCListener
    Private Class QueuedMessage
        Public Property JsonContent As String
        Public Property RequiresAck As Boolean
    End Class

    Private _jobFilePath As String
    Private Property _jobFolderPath As String
    Private _logger As RichTextBoxLogger
    Private _cancellationTokenSource As CancellationTokenSource
    Private Property _isServer As Boolean = False
    Private Property _hwndWatcher As IntPtr = IntPtr.Zero

    ' Monitorizare
    Private _accessWatchdog As System.Windows.Forms.Timer
    Private Property _folderWatcher As FileSystemWatcher
    Private _notifyIcon As NotifyIcon
    Private _currentJobConfig As RobotJob
    Private Property _isStandbyMode As Boolean = False
    Private _isBusy As Boolean = False
    Private Property _deleteJobFileAfterRead As Boolean = True

    ' =========================
    ' VARIABILE PIPE (LOGICA STRICTA)
    ' =========================
    Private _pipeClient As NamedPipeClientStream = Nothing
    Private _isConnecting As Boolean = False
    Private _isConnected As Boolean = False
    Private _currentTaskId As Integer = 0
    Private _currentJobName As String = ""

    ' =========================
    ' COADA DE MESAJE (Stop-and-Wait Protocol)
    ' ========================================
    Private _outgoingQueue As New Queue(Of QueuedMessage)()
    Private _waitingForVbaAck As Boolean = False
    Private ReadOnly _queueLock As New Object()

    ' =========================
    ' CERTIFICATE CACHING
    ' =========================
    Private _cachedCert As X509Certificate2
    Private _cachedPin As String = String.Empty
    Private _cacheExpirationTime As DateTime = DateTime.MinValue
    Private Const CERTIFICATE_CACHE_MINUTES As Integer = 15

    ' Flag pentru a distinge între apăsarea X (minimizare) și Ieșire din Meniu (închidere reală)
    Private _forceExit As Boolean = False

    ' Timer pentru actualizarea Tooltip-ului
    Private _trayTimer As System.Windows.Forms.Timer ' Timer pentru Tooltip

    ' Meniul de context pentru Tray
    Private _ctxTrayMenu As ContextMenuStrip

    ' Flag pentru a evita lansarea multiplă a watcher-ului PIN
    Private _isPinWatcherActive As Boolean = False

    ' Obiect care stocheza variabilele trimise catre vba. Se reseteaza dupa fiecare job.
    Private _vbaVariables As Object

    Private _wasLogonCompleted As Boolean = False

    ''' <summary>
    ''' True când aplicația rulează fără token — executorul este dezactivat,
    ''' doar Resend este disponibil. Se resetează automat la selectarea unui certificat valid.
    ''' </summary>
    Private _isLightMode As Boolean = False

    ' lista de editoare deschise, pentru a le putea actualiza in timp real
    Private _openEditors As New List(Of WorkflowEditorForm)()

    ' Delegat pentru callback-ul de log message (pentru a actualiza editorul in timp real)
    Private _onLogMessageHandler As WorkflowExecutor.OnLogMessageEventHandler = Nothing

    Private _isConsoleVisible As Boolean = False

    Private _originalConsoleHeight As Integer = 0

    Private _wicketMonitorForm As WicketMonitorForm = Nothing
End Class
