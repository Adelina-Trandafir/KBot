Option Strict On
Imports System.Collections.ObjectModel

''' <summary>
''' Colecția ORDONATĂ din spatele <see cref="KBotDataView.Groups"/> (slice 0029) — sora lui
''' <see cref="KBotDataColumnCollection"/>, cu aceleași reguli.
'''
''' <para><b>Ordinea e ierarhia:</b> elementul 0 e nivelul dinafară, ultimul e cel mai dinăuntru.
''' Mutarea unui element în dialogul de colecție al designerului rearanjează gruparea, exact ca
''' mutarea unei linii în fereastra «Sorting and Grouping» a unui raport Access.</para>
'''
''' <para>Un nivel cu cheie VIDĂ se sare tăcut la construcția benzilor (designerul inserează un
''' element gol în clipa în care apeși «Add»); o cheie NECUNOSCUTĂ e o greșeală de model și se
''' verifică zgomotos la <c>EndInit</c>, la fel ca la coloane.</para>
''' </summary>
Public NotInheritable Class KBotGroupLevelCollection
    Inherits Collection(Of KBotGroupLevel)

    ''' <summary>Grila care deține colecția (Nothing pentru o instanță liberă).</summary>
    Friend Property Owner As KBotDataView

    ' Cei patru mutatori sunt PUNCTE DE INTRARE (designerul îi cheamă din InitializeComponent,
    ' apelantul îi cheamă direct), deci nu există deasupra lor niciun boundary deja împachetat la
    ' care să se logheze — vezi KBotDataColumnCollection pentru aceeași hotărâre. Clasificare:
    ' boundary => loghează și RE-ARUNCĂ. O grupare pierdută în tăcere ar arăta ca o eroare de date.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotGroupLevel)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            item.Owner = Owner
            Owner?.OnGroupLevelsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotGroupLevelCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotGroupLevel)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            Dim inlocuit As KBotGroupLevel = Me(index)
            MyBase.SetItem(index, item)
            If inlocuit IsNot Nothing Then inlocuit.Owner = Nothing
            item.Owner = Owner
            Owner?.OnGroupLevelsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotGroupLevelCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            Dim scos As KBotGroupLevel = Me(index)
            MyBase.RemoveItem(index)
            If scos IsNot Nothing Then scos.Owner = Nothing
            Owner?.OnGroupLevelsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotGroupLevelCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            For Each nivel As KBotGroupLevel In Me
                nivel.Owner = Nothing
            Next
            MyBase.ClearItems()
            Owner?.OnGroupLevelsChanged()
        Catch ex As Exception
            LogUnlessDesignTime("KBotGroupLevelCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' Un fișier de erori scris din interiorul devenv.exe e, în cel mai bun caz, zgomot.
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub

End Class
