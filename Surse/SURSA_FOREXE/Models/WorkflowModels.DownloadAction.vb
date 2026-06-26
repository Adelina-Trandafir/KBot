Namespace WorkflowModels

    ''' <summary>
    ''' Download action - Handles file downloads and optional remote parsing
    ''' </summary>
    Public Class DownloadAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Download"
            End Get
        End Property

        Public Property Timeout As Integer = 60 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequiredOneOf("wheretosave")> Public Property SaveFolder As String = String.Empty
        Public Property FileName As String = String.Empty
        Public Property OpenFile As Boolean = False

        <WflRequiredOneOf("wheretosave")> Public Property SaveTo As String = String.Empty ' Variabila în care salvăm JSON-ul rezultat (ca la ScrapeTable)
        Public Property ParseExcel As Boolean = False   ' Dacă True -> Trimite la API Python pentru conversie în JSON
        Public Property HeaderRows As Integer = 1  ' Ii spune procesorului python pe cate randuri este header-ul. Implicit 1
        Public Property SkipFirstNRows As Integer = 0    ' Ii spune procesorului python cate randuri sa sara de la inceput. Implicit 0
        Public Property SkipLastNRows As Integer = 0     ' Ii spune procesorului python cate randuri sa sara de la sfarsit. Implicit 0
        Public Property SkipFirstNColumns As Integer = 0  ' Ii spune procesorului python cate coloane sa sara de la inceput. Implicit 0
        Public Property SkipLastNColumns As Integer = 0   ' Ii spune procesorului python cate coloane sa sara de la sfarsit. Implicit 0
        Public Property FilterColumn As String = String.Empty ' Numele coloanei pe care vrem sa filtram
        Public Property Filter As String = String.Empty  ' Valoarea pe care vrem sa o gasim in coloana de filtrare
        Public Property ComplexFilter As String = String.Empty ' Filtru complex care accepta operatori: =, !=, >, <, >=, <= , REGEX si AND intre conditii (daca e aplicat anuleaza Filter simplu)
        Public Property ApiUrl As String = "http://adcredit.avatarsoft.ro:5008/api/tools/process_excel" ' Poate fi suprascris din XML
        Public Property ApiKey As String = "Ad3l1na1i1ub3st3P310ana5iRazvan2026!@#"        ' Cheia API (hardcodată sau din XML)
    End Class

End Namespace
