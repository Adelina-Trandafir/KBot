Option Strict On
Imports System.Collections.Generic
Imports System.Globalization

''' <summary>One row of the server's <c>Setari</c> table as <c>GET /api/setari</c> answers it (slice 0100-02). POCO.</summary>
Public NotInheritable Class ServerSettingRow
    ''' <summary>The setting key («Multithread»). ASCII, read by code.</summary>
    Public Property Key As String = String.Empty
    ''' <summary>The text the operator may see for it.</summary>
    Public Property Text As String = String.Empty
    ''' <summary>Which kind <see cref="Value"/> is: «int», «date» or «text».</summary>
    Public Property Kind As String = String.Empty
    ''' <summary>The value as invariant text (dates ISO, whole numbers digits only); Nothing = none / did not fit its kind.</summary>
    Public Property Value As String
End Class

''' <summary>
''' The settings the SERVER decides (slice 0100-02, table <c>Setari</c> of the connected database): read
''' once after login and again after a change of unit, held here for the whole application. Not a file:
''' nothing of it is saved on the operator's computer, so a change made on the server is the one that
''' counts at the next login.
'''
''' <para><b>No row = off.</b> A database with no <c>Setari</c> table, a missing key or a value that does
''' not fit its kind reads as the safe value (<see cref="MultithreadAllowed"/> False,
''' <see cref="MultithreadMax"/> 1): a server-controlled feature is never on by accident.</para>
''' </summary>
Public NotInheritable Class ServerSettings

    ''' <summary>0 = multi-thread downloads are off for this database, 1 = allowed.</summary>
    Public Const KeyMultithread As String = "Multithread"
    ''' <summary>The most FOREXE tabs a download may use at once.</summary>
    Public Const KeyMultithreadMax As String = "Multithread_Max"
    ''' <summary>Slice 0104: 1 = the unit also runs the Access application, 0 (or no row) = it does not.</summary>
    Public Const KeyAccess As String = "Access"

    ' Replaced as a whole on every Apply / Clear, so a reader never sees half a refresh.
    Private Shared _rows As IReadOnlyDictionary(Of String, ServerSettingRow) =
        New Dictionary(Of String, ServerSettingRow)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Raised after <see cref="Apply"/> / <see cref="Clear"/> when a setting changed. May fire on any thread.</summary>
    Public Shared Event Changed As EventHandler

    Private Sub New()
    End Sub

    ''' <summary>The server allows multi-thread downloads (Multithread is a number other than 0).</summary>
    Public Shared ReadOnly Property MultithreadAllowed As Boolean
        Get
            Return GetInt(KeyMultithread, 0) <> 0
        End Get
    End Property

    ''' <summary>The server's limit of tabs at once, at least 1 (the application's own ceiling is applied by AppSettings).</summary>
    Public Shared ReadOnly Property MultithreadMax As Integer
        Get
            Return Math.Max(1, GetInt(KeyMultithreadMax, 1))
        End Get
    End Property

    ''' <summary>
    ''' Slice 0104: the connected unit has the Access application (Access is a number other than 0). The
    ''' Access features are shown only while this holds -- see <see cref="AccessFeature"/>.
    ''' </summary>
    Public Shared ReadOnly Property AccessEnabled As Boolean
        Get
            Return GetInt(KeyAccess, 0) <> 0
        End Get
    End Property

    ''' <summary>The whole number stored under <paramref name="key"/>, else <paramref name="fallback"/>.</summary>
    Public Shared Function GetInt(key As String, fallback As Integer) As Integer
        Dim raw As String = RawValue(key)
        Dim n As Integer
        If raw IsNot Nothing AndAlso Integer.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, n) Then Return n
        Return fallback
    End Function

    ''' <summary>The date stored under <paramref name="key"/>, else Nothing.</summary>
    Public Shared Function GetDate(key As String) As Date?
        Dim raw As String = RawValue(key)
        Dim d As Date
        If raw IsNot Nothing AndAlso Date.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d
        Return Nothing
    End Function

    ''' <summary>The text stored under <paramref name="key"/>, else <paramref name="fallback"/>.</summary>
    Public Shared Function GetText(key As String, fallback As String) As String
        Dim raw As String = RawValue(key)
        Return If(raw, fallback)
    End Function

    ''' <summary>
    ''' Replaces everything with what the server just answered. Raises <see cref="Changed"/> when the
    ''' settings the application acts on differ from before.
    ''' </summary>
    Public Shared Sub Apply(rows As IEnumerable(Of ServerSettingRow))
        Dim wasAllowed As Boolean = MultithreadAllowed
        Dim wasMax As Integer = MultithreadMax
        Dim wasAccess As Boolean = AccessEnabled
        Dim fresh As New Dictionary(Of String, ServerSettingRow)(StringComparer.OrdinalIgnoreCase)
        If rows IsNot Nothing Then
            For Each r As ServerSettingRow In rows
                If r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.Key) Then fresh(r.Key.Trim()) = r
            Next
        End If
        _rows = fresh
        If wasAllowed <> MultithreadAllowed OrElse wasMax <> MultithreadMax OrElse wasAccess <> AccessEnabled Then
            RaiseEvent Changed(Nothing, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>Forgets everything (log out / a unit that could not be read): every setting is off again.</summary>
    Public Shared Sub Clear()
        Apply(Nothing)
    End Sub

    Private Shared Function RawValue(key As String) As String
        Dim row As ServerSettingRow = Nothing
        If key IsNot Nothing AndAlso _rows.TryGetValue(key, row) Then Return row.Value
        Return Nothing
    End Function

End Class
