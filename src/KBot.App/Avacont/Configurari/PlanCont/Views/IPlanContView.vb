Option Strict On
Imports System.Threading.Tasks

''' <summary>
''' A view of the «Plan de conturi» window (slice AV01-04): the accounts with their tree and fields, or the opening balances in
''' a grid. The window shows one at a time, hands it the shared <see cref="PlanContContext"/> and asks it to settle what it has
''' pending before the other view, another subunit or the closing takes over.
''' </summary>
Public Interface IPlanContView

    ''' <summary>A line for the window's status bar.</summary>
    Event StatusMessage As Action(Of String)

    ''' <summary>The view changed the chart (saved or deleted an account): the window reloads it and binds the views again. Argument = the account to select.</summary>
    Event PlanChanged As Action(Of Integer?)

    ''' <summary>The view is waiting for the server (True) or is free again (False).</summary>
    Event BusyChanged As Action(Of Boolean)

    ''' <summary>Shows the context's chart; <paramref name="k_selectCont"/> = the account to select (Nothing = none).</summary>
    Sub Bind(k_context As PlanContContext, k_selectCont As Integer?)

    ''' <summary>The account on screen (selected in the tree / current row of the grid); Nothing = none or an unsaved new one.</summary>
    ReadOnly Property CurrentContId As Integer?

    ''' <summary>True when something is unsaved or an editor is still open.</summary>
    ReadOnly Property HasPending As Boolean

    ''' <summary>
    ''' Settles the pending work: the accounts view asks (Yes saves, No drops, Cancel stays), the balances view saves its edited
    ''' rows by itself. True = the caller may go on.
    ''' </summary>
    Function CommitAsync() As Task(Of Boolean)

End Interface
