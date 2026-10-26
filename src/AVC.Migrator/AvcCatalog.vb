''' <summary>
''' The migrations AVC.Migrator knows, in the order they run. The order follows the porting stages (stage 1 first) and what
''' points into what: a step only appears after the steps it depends on.
''' </summary>
Friend NotInheritable Class AvcCatalog

    Private Sub New()
    End Sub

    ''' <summary>A fresh set of steps (they keep what they read, so every file read starts from new instances).</summary>
    Public Shared Function Create() As List(Of AvcStep)
        Return New List(Of AvcStep) From {
            New AvcSubunitsStep(),
            New AvcPlanContStep(),
            New AvcParteneriSiStep(),
            New AvcJurnaleStep()
        }
    End Function

End Class
