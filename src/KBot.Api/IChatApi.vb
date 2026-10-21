Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The shared chat (slice CHAT-08): <c>/api/chat/*</c>, the same routes the web pages use, reached
''' here with the K-BOT bearer. Kept out of <see cref="IApiClient"/> like <see cref="IMarcajApi"/>:
''' one caller (the chat service). Every call throws <see cref="ApiException"/> on a non-2xx.
''' </summary>
Public Interface IChatApi

    ''' <summary>Identity, preferences and unread counts. Cheap: this is what the closed chat polls.</summary>
    Function GetChatStateAsync(ct As CancellationToken) As Task(Of ChatState)

    ''' <summary>
    ''' Messages of the applications the caller shows. <paramref name="after"/> = newer than that id
    ''' (oldest first); <paramref name="before"/> = older than that id; neither = the latest ones.
    ''' Only one of the two may be given.
    ''' </summary>
    Function GetChatMessagesAsync(before As String, after As String, limit As Integer, ct As CancellationToken) As Task(Of ChatMessagePage)

    Function SendChatMessageAsync(request As ChatSendRequest, ct As CancellationToken) As Task(Of ChatSendResult)

    ''' <summary>
    ''' Saves the shown name, the pseudonym choice and/or the applications whose messages are shown (any of V, A, K; at
    ''' least one); a Nothing argument is left as it is.
    ''' </summary>
    Function SaveChatPreferencesAsync(displayName As String, anonymous As Boolean?, applications As IReadOnlyList(Of String),
                                      ct As CancellationToken) As Task(Of ChatState)

    ''' <summary>Marks the messages of <paramref name="application"/> up to <paramref name="lastId"/> as read.</summary>
    Function MarkChatReadAsync(application As String, lastId As String, ct As CancellationToken) As Task(Of ChatState)

End Interface
