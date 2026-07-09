Imports System.Runtime.CompilerServices

' Testele văd membrii Friend (ex. ThemeStore.OverrideRootForTests) ca să poată
' rula round-trip-ul de persistență împotriva unui director temporar.
<Assembly: InternalsVisibleTo("KBot.Theming.Tests")>
