Imports System.IO
Imports Xunit

''' <summary>
''' Slice 000T: a typed question finds the tutorial (stems, filler words and diacritics do not matter).
''' Pure: the library is read from a temporary folder; no window is created.
''' </summary>
Public Class TutorialSearchTests

    Private Const FlowText As String =
        "---" & vbLf & "id: revizie" & vbLf & "title: Adaugă o revizie pe baza unei rezervări existente" & vbLf &
        "part: contabil" & vbLf & "keywords: ddf document fundamentare plus" & vbLf & "---" & vbLf &
        "## Alege un angajament" & vbLf & "wait: manual" & vbLf & "Text." & vbLf &
        "## Salvează documentul" & vbLf & "wait: manual" & vbLf & "Text."

    Private Const OtherText As String =
        "---" & vbLf & "id: partener" & vbLf & "title: Asociază un partener la un document" & vbLf &
        "part: contabil" & vbLf & "---" & vbLf & "## Deschide Sumar" & vbLf & "wait: manual" & vbLf & "Text."

    Private Shared Function Library() As HelpLibrary
        Dim k_root As String = Path.Combine(Path.GetTempPath(), "kbot-tutorial-search-" & Guid.NewGuid().ToString("N"))
        Dim k_folder As String = Path.Combine(k_root, "tutorials")
        Directory.CreateDirectory(k_folder)
        File.WriteAllText(Path.Combine(k_folder, "revizie.md"), FlowText, New Text.UTF8Encoding(False))
        File.WriteAllText(Path.Combine(k_folder, "partener.md"), OtherText, New Text.UTF8Encoding(False))
        Try
            Return HelpLibrary.Load(k_root)
        Finally
            Directory.Delete(k_root, True)
        End Try
    End Function

    Private Shared ReadOnly Contabil As HelpPart() = {HelpPart.Contabil}

    <Fact>
    Public Sub Load_ReadsTheTutorialsFolder()
        Assert.Equal(2, Library().Tutorials.Count)
    End Sub

    <Theory>
    <InlineData("cum adaug o revizie pe baza unei rezervari existente")>
    <InlineData("revizie rezervare")>
    <InlineData("Adaugă revizie din rezervări")>
    Public Sub SearchTutorials_FindsTheRevisionTutorialFirst(k_question As String)
        Dim k_hits As List(Of TutorialFlow) = Library().SearchTutorials(k_question, Contabil)
        Assert.NotEmpty(k_hits)
        Assert.Equal("revizie", k_hits(0).Id)
    End Sub

    <Fact>
    Public Sub SearchTutorials_MatchesKeywordsToo()
        Dim k_hits As List(Of TutorialFlow) = Library().SearchTutorials("fundamentare", Contabil)
        Assert.Equal({"revizie"}, k_hits.Select(Function(f) f.Id).ToArray())
    End Sub

    <Fact>
    Public Sub SearchTutorials_NothingForAnUnrelatedQuestionOrAPartTheLoginCannotRead()
        Assert.Empty(Library().SearchTutorials("imprimanta culoare", Contabil))
        Assert.Empty(Library().SearchTutorials("revizie rezervare", {HelpPart.Director}))
        Assert.Empty(Library().SearchTutorials("   ", Contabil))
    End Sub

End Class
