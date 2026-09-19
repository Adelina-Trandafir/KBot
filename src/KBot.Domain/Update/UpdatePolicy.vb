Option Strict On

''' <summary>
''' The one comparison behind every update prompt (slice 0067): pure, testable, no I/O.
'''
''' <para>Two numbers come from the server: <c>version</c> (the latest package) and
''' <c>minimum</c> (the lowest version still allowed to run). A client below
''' <c>minimum</c> gets <see cref="UpdateDecision.Required"/> even if it skipped several
''' optional releases in between -- that is the whole point of carrying a minimum instead
''' of a per-release flag.</para>
''' </summary>
Public NotInheritable Class UpdatePolicy

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Decides for <paramref name="current"/> against a published <paramref name="latest"/> /
    ''' <paramref name="minimum"/> pair. Throws on a version string that does not parse: a
    ''' server that publishes garbage must be seen, not treated as "up to date".
    ''' </summary>
    Public Shared Function Decide(current As Version, latest As String, minimum As String) As UpdateDecision
        If current Is Nothing Then Throw New ArgumentNullException(NameOf(current))
        Dim latestV As Version = ParseVersion(latest, NameOf(latest))
        Dim minimumV As Version = ParseVersion(minimum, NameOf(minimum))
        Return Decide(current, latestV, minimumV)
    End Function

    Public Shared Function Decide(current As Version, latest As Version, minimum As Version) As UpdateDecision
        If current Is Nothing Then Throw New ArgumentNullException(NameOf(current))
        If latest Is Nothing Then Throw New ArgumentNullException(NameOf(latest))
        If minimum Is Nothing Then Throw New ArgumentNullException(NameOf(minimum))

        Dim cur As Version = Normalize(current)
        Dim lat As Version = Normalize(latest)
        Dim min As Version = Normalize(minimum)

        If cur < min Then Return UpdateDecision.Required
        If cur < lat Then Return UpdateDecision.Available
        Return UpdateDecision.UpToDate
    End Function

    ''' <summary>
    ''' <c>System.Version</c> treats a missing part as -1, so "1.0.30" &lt; "1.0.30.0". The
    ''' server may publish either shape; pad to four parts before comparing.
    ''' </summary>
    Public Shared Function Normalize(v As Version) As Version
        If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))
        Return New Version(Math.Max(v.Major, 0), Math.Max(v.Minor, 0),
                           Math.Max(v.Build, 0), Math.Max(v.Revision, 0))
    End Function

    Public Shared Function ParseVersion(text As String, paramName As String) As Version
        Dim parsed As Version = Nothing
        If String.IsNullOrWhiteSpace(text) OrElse Not Version.TryParse(text.Trim(), parsed) Then
            Throw New ArgumentException("Versiune invalidă de la server: «" & If(text, "<nimic>") & "».", paramName)
        End If
        Return parsed
    End Function
End Class
