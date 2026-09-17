Option Strict On

''' <summary>
''' A self-theming CONTAINER whose children belong to the host form (slice 0062).
'''
''' <para><see cref="IThemedControl"/> tells <c>ThemeManager.Traverse</c> two things at once:
''' "I paint myself" and "do not recurse into me with the generic rules". The second half is
''' right for a control that OWNS its children (the text box inside <c>KBotTextField</c>) and
''' wrong for a layout container: a <c>TableLayoutPanel</c> full of the form's own labels,
''' buttons and text fields must still have those children themed by the generic per-type
''' rules, exactly as when it was a plain <c>TableLayoutPanel</c>.</para>
'''
''' <para>For an <see cref="IThemedContainer"/> the traversal therefore recurses into the
''' children FIRST, with the generic rules, and calls <see cref="IThemedControl.ApplyTheme"/> on
''' the container AFTER them -- so a container that measures its content (fixed rows grown to the
''' themed buttons) measures the children as the scheme left them, not as the designer did.</para>
''' </summary>
Public Interface IThemedContainer
    Inherits IThemedControl
End Interface
