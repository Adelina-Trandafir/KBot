Option Strict On
Imports System.Collections.Generic

' SLICE-CHAT-08. The shared chat of the web pages (PYTHON/routes/chat), as K-BOT.app reads it.
' Property names are the wire names, exactly as routes/chat/contract.py writes them (ASCII).

''' <summary>Who the caller is in the chat: the pseudonym the server made, and the name they chose to show.</summary>
Public NotInheritable Class ChatSelf
    Public Property pseudonym As String = String.Empty
    Public Property display_name As String = String.Empty
End Class

''' <summary>What the caller keeps between visits.</summary>
Public NotInheritable Class ChatPreferences
    ''' <summary>The applications whose messages are shown: any of V (VERCON), A (ADECHIT), K (K-BOT).</summary>
    Public Property applications As List(Of String) = New List(Of String)()
    ''' <summary>True = new messages go out under the pseudonym.</summary>
    Public Property anonymous As Boolean = True
End Class

''' <summary>The caller's identity, preferences and unread counts (<c>GET /api/chat/state</c>).</summary>
Public NotInheritable Class ChatState
    Public Property title As String = String.Empty
    Public Property self As ChatSelf = New ChatSelf()
    Public Property preferences As ChatPreferences = New ChatPreferences()
    ''' <summary>Unread messages in the applications that are shown.</summary>
    Public Property unread As Integer
    Public Property unread_by_application As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()
    ''' <summary>Per application, the id of the last message the caller has read (a decimal string).</summary>
    Public Property read_by_application As Dictionary(Of String, String) = New Dictionary(Of String, String)()
End Class

''' <summary>The message a message answers, cut short by the server.</summary>
Public NotInheritable Class ChatReply
    Public Property id As String = String.Empty
    Public Property application As String = String.Empty
    Public Property anonymous As Boolean
    Public Property author As String = String.Empty
    Public Property text As String = String.Empty
End Class

''' <summary>One message. <c>id</c> is a decimal string (ids go past what a double holds exactly).</summary>
Public NotInheritable Class ChatMessage
    Public Property id As String = String.Empty
    Public Property application As String = String.Empty
    Public Property anonymous As Boolean
    Public Property author As String = String.Empty
    Public Property text As String = String.Empty
    ''' <summary>True for the caller's own messages.</summary>
    Public Property mine As Boolean
    ''' <summary>UTC, ISO 8601 with a trailing Z.</summary>
    Public Property created_at As String = String.Empty
    Public Property reply As ChatReply
End Class

''' <summary>A slice of the history plus the state (<c>GET /api/chat/messages</c>).</summary>
Public NotInheritable Class ChatMessagePage
    Public Property messages As List(Of ChatMessage) = New List(Of ChatMessage)()
    Public Property has_more As Boolean
    Public Property state As ChatState = New ChatState()
End Class

''' <summary>What <c>POST /api/chat/messages</c> takes. <c>client_key</c> makes a retry harmless.</summary>
Public NotInheritable Class ChatSendRequest
    Public Property text As String = String.Empty
    Public Property application As String = "K"
    Public Property anonymous As Boolean = True
    Public Property display_name As String = String.Empty
    ''' <summary>A new GUID per message; sending the same one twice returns the first message.</summary>
    Public Property client_key As String = String.Empty
    ''' <summary>The id of the message answered, or Nothing.</summary>
    Public Property reply_to As String
End Class

''' <summary>The answer to a send.</summary>
Public NotInheritable Class ChatSendResult
    Public Property message As ChatMessage
    ''' <summary>False when the server already had this <c>client_key</c> (a retry).</summary>
    Public Property created As Boolean
    Public Property state As ChatState = New ChatState()
End Class
