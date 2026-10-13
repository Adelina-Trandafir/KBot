Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

''' <summary>
''' The window the operator really sees, for the code that talks to Windows about it (foreground,
''' enabled, activation, minimise, close). <c>Control.FindForm()</c> stops at the first
''' <c>Form</c> up the parents, which is the hosted shell (a non-top-level form docked in
''' <c>AvacontMain</c>) -- not a window of its own: it is never the foreground window and its
''' handle is a child window. The hosted viewers must ask the top-level one.
''' </summary>
Friend Module AdobeTopForm

    ''' <summary>The top-level form that contains the control; Nothing if it is not on a form.</summary>
    <Extension>
    Friend Function TopLevelForm(k_control As Control) As Form
        Dim k_form As Form = k_control?.FindForm()
        While k_form IsNot Nothing AndAlso Not k_form.TopLevel AndAlso k_form.Parent IsNot Nothing
            Dim k_up As Form = k_form.Parent.FindForm()
            If k_up Is Nothing OrElse k_up Is k_form Then Exit While
            k_form = k_up
        End While
        Return k_form
    End Function
End Module
