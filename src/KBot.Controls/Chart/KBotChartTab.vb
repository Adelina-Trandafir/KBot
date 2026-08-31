Option Strict On
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports KBot.Common

''' <summary>
''' One button of the band that a <see cref="KBotChartView"/> draws above its plot — the strip
''' that stands in for a tab control.
''' </summary>
''' <remarks>
''' <para>The band is SINGLE-SELECT, which is what separates it from <c>KBotChipBar</c>: exactly
''' one tab is current at any moment, because the plot below can only show one thing at a time.
''' Pressing the tab that is already current does nothing and raises nothing.</para>
''' <para>The tab does not carry the data it switches to. It raises
''' <c>KBotChartView.TabSelected</c> with its key and the host refills the series — the chart never
''' guesses what a tab means.</para>
''' </remarks>
Public NotInheritable Class KBotChartTab

    ''' <summary>Parameterless constructor — required by the designer collection dialog.</summary>
    Public Sub New()
    End Sub

    ''' <summary>Convenience for code: a tab with a key and a caption.</summary>
    Public Sub New(key As String, text As String)
        ' «Me.» is MANDATORY: VB is case-insensitive, so a parameter shadows the property of the
        ' same name and an unqualified assignment would write the parameter into itself.
        Me.Key = key
        Me.Text = If(text, String.Empty)
    End Sub

    <Category("K-BOT Chart Tab")>
    <Description("Identifier reported by TabSelected and accepted by SelectTab. Must be non-empty and unique.")>
    Public Property Key As String

    <Category("K-BOT Chart Tab")>
    <Description("The caption written on the button. This is read by the operator, so it is written in the operator's language.")>
    Public Property Text As String = String.Empty

    <Category("K-BOT Chart Tab")>
    <Description("Icon drawn to the left of the caption. Nothing = caption only.")>
    <DefaultValue(CType(Nothing, Image))>
    Public Property Icon As Image

    <Category("K-BOT Chart Tab")>
    <Description("False => the button is drawn faded and cannot be pressed, but still takes up its place in the band.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    <Category("K-BOT Chart Tab")>
    <Description("False => the button takes up no place at all and is skipped by the keyboard.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

    <Category("K-BOT Chart Tab")>
    <Description("Floating label shown while the pointer rests on the button (multiple lines; accepts the rich-text markup of KBotToolTip). Empty = no label.")>
    <Editor(GetType(MultilineStringEditor), GetType(UITypeEditor))>
    <DefaultValue("")>
    Public Property Tooltip As String = String.Empty

    ''' <summary>Where the last layout pass put this button, in client pixels.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Bounds As Rectangle = Rectangle.Empty

    Public Overrides Function ToString() As String
        Return $"{If(Key, String.Empty)}: {If(Text, String.Empty)}"
    End Function
End Class

''' <summary>
''' The ordered buttons of the header band. Every mutation invalidates the chart's layout.
''' </summary>
''' <remarks>
''' <b>Key validation does NOT live here</b>, for the same reason as in
''' <c>KBotChartSeriesCollection</c>: the collection dialog inserts an empty tab the moment Add is
''' pressed. The contract is enforced in <c>KBotChartView.EndInit</c> and in the runtime methods.
''' </remarks>
Public NotInheritable Class KBotChartTabCollection
    Inherits Collection(Of KBotChartTab)

    ''' <summary>The chart that owns this collection (Nothing for a free-standing instance).</summary>
    Friend Property Owner As KBotChartView

    ' Entry points: log and RE-THROW (see the twin note in KBotChartSeriesCollection).
    Protected Overrides Sub InsertItem(index As Integer, item As KBotChartTab)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartTabCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotChartTab)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.SetItem(index, item)
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartTabCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            MyBase.RemoveItem(index)
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartTabCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            MyBase.ClearItems()
            Owner?.InvalidateChartLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChartTabCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' An error file written from inside devenv.exe is noise, not diagnostics (see KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub
End Class
