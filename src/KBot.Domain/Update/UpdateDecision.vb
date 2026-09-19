Option Strict On

''' <summary>What the client should do about the published version (slice 0067).</summary>
Public Enum UpdateDecision
    ''' <summary>Nothing newer is published; carry on.</summary>
    UpToDate = 0
    ''' <summary>A newer version exists; the operator may postpone it ("Mai târziu").</summary>
    Available = 1
    ''' <summary>This client is below the published minimum; it cannot proceed without updating.</summary>
    Required = 2
End Enum
