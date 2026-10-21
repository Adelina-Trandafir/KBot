Option Strict On

''' <summary>
''' What every view shares (slice ME01): the server client, the database chosen in the left tree
''' (empty = «all databases»), the list of databases, and the two ways a view talks back to the
''' main window (the status line and the busy bar).
''' </summary>
Public NotInheritable Class AdminContext

    ''' <summary>The text the tree shows for the first root, and the value «all databases» takes in the UI.</summary>
    Public Const AllDatabasesCaption As String = "<TOATE BAZELE>"

    Private ReadOnly _client As AdminClient
    Private _selectedDb As String = String.Empty
    Private _databases As IReadOnlyList(Of DatabaseInfo) = New List(Of DatabaseInfo)()
    Private _busyCount As Integer

    Public Event StatusChanged(k_text As String)
    Public Event BusyChanged(k_busy As Boolean)
    Public Event RefreshRequested()

    Public Sub New(k_client As AdminClient)
        If k_client Is Nothing Then Throw New ArgumentNullException(NameOf(k_client))
        _client = k_client
    End Sub

    Public ReadOnly Property Client As AdminClient
        Get
            Return _client
        End Get
    End Property

    ''' <summary>The database chosen in the tree; empty means all of them.</summary>
    Public Property SelectedDb As String
        Get
            Return _selectedDb
        End Get
        Set(k_value As String)
            _selectedDb = If(k_value, String.Empty)
        End Set
    End Property

    ''' <summary>The account (e-mail) chosen in the accounts tree; empty when none.</summary>
    Public Property SelectedAccount As String = String.Empty

    ''' <summary>The databases ticked in the tree (ME01-06); empty when none. Choosing a row (the clicked one) is <see cref="SelectedDb"/>; this is the ticks.</summary>
    Public Property CheckedDatabases As IReadOnlyList(Of String) = New List(Of String)()

    ''' <summary>The accounts ticked in the tree (ME01-06); empty when none.</summary>
    Public Property CheckedAccounts As IReadOnlyList(Of String) = New List(Of String)()

    ''' <summary>The local backup store once the operator has opened it in this run (slice ME02); Nothing until then.</summary>
    Public Property LocalStore As BackupStore

    Public ReadOnly Property IsAll As Boolean
        Get
            Return _selectedDb = String.Empty
        End Get
    End Property

    ''' <summary>The value to hand to admin_cli's <c>--db</c>: the database name, or ALL.</summary>
    Public ReadOnly Property DbArgument As String
        Get
            Return If(IsAll, "ALL", _selectedDb)
        End Get
    End Property

    Public Property Databases As IReadOnlyList(Of DatabaseInfo)
        Get
            Return _databases
        End Get
        Set(k_value As IReadOnlyList(Of DatabaseInfo))
            _databases = If(k_value, New List(Of DatabaseInfo)())
        End Set
    End Property

    Public Sub Say(k_text As String)
        RaiseEvent StatusChanged(k_text)
    End Sub

    Public Sub SetBusy(k_on As Boolean)
        _busyCount = Math.Max(0, _busyCount + If(k_on, 1, -1))
        RaiseEvent BusyChanged(_busyCount > 0)
    End Sub

    ''' <summary>Asks the main window to reload the databases and accounts (after a change).</summary>
    Public Sub RequestRefresh()
        RaiseEvent RefreshRequested()
    End Sub

    ''' <summary>
    ''' The single database the view needs, or empty after telling the operator to choose one
    ''' (when «all databases» is selected).
    ''' </summary>
    Public Function RequireSingleDb(k_what As String) As String
        If Not IsAll Then Return _selectedDb
        KBotMessage.Show(k_what & " se cere pe o singură bază. Alegeți o bază din lista din stânga.",
                         "KBotAdmin", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Return String.Empty
    End Function

End Class

''' <summary>A page of the right-hand panel.</summary>
Public Interface IAdminView

    Sub Attach(k_context As AdminContext)

    ''' <summary>The page was opened (first time or again): load what it needs.</summary>
    Function ActivatedAsync() As Task

    ''' <summary>The database chosen in the tree changed while this page is the open one.</summary>
    Sub SelectionChanged()

End Interface

''' <summary>The guard every UI handler runs its work in: busy bar on, errors shown to the operator and logged.</summary>
Public Module UiRun

    Public Async Function Guard(k_source As String, k_context As AdminContext, k_work As Func(Of Task)) As Task
        k_context.SetBusy(True)
        Try
            Await k_work().ConfigureAwait(True)
        Catch ex As AdminException
            GlobalErrorLog.Write(k_source, ex)
            k_context.Say(ex.Message)
            KBotMessage.Show(ex.Message, "KBotAdmin", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write(k_source, ex)
            k_context.Say(ex.Message)
            KBotMessage.Show(ex.Message, "KBotAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            k_context.SetBusy(False)
        End Try
    End Function

    ''' <summary>A yes/no question; the answer is True only on «Da» (default button is «Nu»).</summary>
    Public Function Confirm(k_text As String, k_caption As String) As Boolean
        Return KBotMessage.Show(k_text, k_caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) = DialogResult.Yes
    End Function

    Public Function FormatBytes(k_bytes As Long) As String
        Dim k_value As Double = k_bytes
        Dim k_units As String() = {"B", "KB", "MB", "GB", "TB"}
        Dim k_i As Integer = 0
        Do While k_value >= 1024.0 AndAlso k_i < k_units.Length - 1
            k_value /= 1024.0
            k_i += 1
        Loop
        Return k_value.ToString(If(k_i = 0, "0", "0.#"), Globalization.CultureInfo.InvariantCulture) & " " & k_units(k_i)
    End Function

End Module
