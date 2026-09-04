Option Strict On
Imports KBot.Domain

''' <summary>
''' The write commands of the fundamentation document (slice 0051), which <c>DdfView</c> asks
''' for and <c>MainForm</c> executes.
'''
''' <para><b>Why they go through the shell instead of happening in the view:</b> each one
''' needs the re-authentication net on one or more response shapes, and <c>WithReauth</c> is
''' private and generic in <c>MainForm</c>. The view is handed a single action, so the
''' re-login policy stays where it always is -- in one place.</para>
'''
''' <para><see cref="Adauga"/> and <see cref="AdaugaRevizieInitiala"/> are asked for from the
''' RESERVATIONS tree, not from the DDF tree: the "+" icon on a reservation leaf is the
''' trigger, exactly where Access put it (<c>mcTree_RightIconClick</c> in
''' <c>frmFX_MAIN_REZ</c> -&gt; <c>fxRezervari_AdaugaRevizie</c> -&gt;
''' <c>FX_Adaugare_DDF</c>). They had no caller when slice 0051 shipped (decision D20).
''' There is still deliberately NO "add" entry in <c>DdfView</c>'s context menu: one trigger,
''' in the one place the operator already knows.</para>
''' </summary>
Public Enum DdfActiune
    ''' <summary>A new revision on an angajament that already has a document. Asked for by the
    ''' "+" icon on a reservation that is NOT the initial one.</summary>
    Adauga = 0
    ''' <summary>The very first revision, which also creates the document. Asked for by the
    ''' "+" icon on an INITIAL reservation.</summary>
    AdaugaRevizieInitiala = 1
    ''' <summary>Open the editor on the selected revision.</summary>
    Modifica = 2
    ''' <summary>Delete the selected revision.</summary>
    StergeRevizie = 3
    ''' <summary>Delete the whole document, with everything hanging off it.</summary>
    Sterge = 4
    ''' <summary>Delete every revision of one month. When that is all of them, the DOCUMENT
    ''' goes instead -- the server decides and says which it did.</summary>
    StergeLuna = 5
End Enum

''' <summary>
''' One write command, carrying everything needed to execute it outside the view.
''' POCO -&gt; no Try/Catch.
''' </summary>
Public NotInheritable Class DdfComanda
    Public ReadOnly Property Actiune As DdfActiune
    Public ReadOnly Property Cod As String

    ''' <summary>The revision the command targets; <c>Nothing</c> for the two «add» actions
    ''' and for <see cref="DdfActiune.StergeLuna"/>.</summary>
    Public ReadOnly Property Revizie As RevizieRow

    ''' <summary>The document key. Taken from the revision when there is one; carried
    ''' separately for <see cref="DdfActiune.StergeLuna"/>, whose node is a month, not a
    ''' revision.</summary>
    Public ReadOnly Property Iddf As Integer

    ''' <summary>The month's year; 0 when the command is not about a month.</summary>
    Public ReadOnly Property An As Integer
    ''' <summary>The month, 1-12; 0 when the command is not about a month.</summary>
    Public ReadOnly Property Luna As Integer

    Public Sub New(actiune As DdfActiune, cod As String, Optional revizie As RevizieRow = Nothing)
        Me.Actiune = actiune
        Me.Cod = If(cod, String.Empty)
        Me.Revizie = revizie
        Me.Iddf = If(revizie Is Nothing, 0, revizie.Iddf)
    End Sub

    Private Sub New(cod As String, iddf As Integer, an As Integer, luna As Integer)
        Me.Actiune = DdfActiune.StergeLuna
        Me.Cod = If(cod, String.Empty)
        Me.Iddf = iddf
        Me.An = an
        Me.Luna = luna
    End Sub

    ''' <summary>
    ''' The month root's command -- the port of <c>FX_Stergere_Revizii(IDDF, LunaAn)</c>.
    ''' </summary>
    Public Shared Function PeLuna(cod As String, iddf As Integer, an As Integer,
                                  luna As Integer) As DdfComanda
        Return New DdfComanda(cod, iddf, an, luna)
    End Function
End Class
