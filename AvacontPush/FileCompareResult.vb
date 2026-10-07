' State of one local file relative to the server.
Public Enum FileState
    NewOnServer   ' remote copy is missing        -> "NOU pe server"
    Modified      ' local is newer than remote     -> "MODIFICAT"
    Identical     ' remote is same/newer than local -> "IDENTIC"
End Enum

' One row in the file list.
Public NotInheritable Class FileCompareResult
    Public Property RelativePath As String        ' remote-style, forward slashes
    Public Property LocalFullPath As String
    Public Property RemotePath As String
    Public Property State As FileState
    Public Property LocalMtimeUtc As DateTime
    Public Property RemoteMtimeUtc As DateTime    ' DateTime.MinValue when remote is missing
End Class
