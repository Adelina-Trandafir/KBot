Option Strict On

''' <summary>
''' What one marker of a <c>KBotLaneView</c> stands for, and therefore how it is drawn.
''' </summary>
''' <remarks>
''' <para>The styles are FACTS about the marker, not decoration. Each one exists because the
''' operator has to be able to tell that marker apart from an ordinary one without opening its
''' label — on a surface holding twenty lanes of twenty markers, a label per marker is not a
''' reading, it is an interrogation.</para>
''' <para>Note what is NOT here: "disabled". A marker that cannot be moved is
''' <see cref="Locked"/>, drawn with a padlock and in FULL colour. Greying it out was tried on
''' the chart in slice 0048-06 and destroyed the colour pairing between a mark and its row —
''' where most of a chain is locked, everything went grey and the surface stopped saying
''' anything at all.</para>
''' </remarks>
Public Enum KBotLaneMarkerStyle

    ''' <summary>An ordinary marker: a filled disc in the lane's colour.</summary>
    Normal = 0

    ''' <summary>
    ''' The marker that CLOSES a chain (F21 of <c>docs/FUNDAMENT_Asociere_Receptii.md</c>) — the
    ''' record that the thing was deleted. Drawn as a cross cap, so the end of a lane reads as an
    ''' end rather than as one more entry.
    ''' </summary>
    Deletion = 1

    ''' <summary>
    ''' A save that recorded nothing (F17). Drawn hollow with an "=" through it, because the
    ''' alternative — an ordinary marker carrying the same number as the one before it — reads as
    ''' a duplicate the operator then has to explain to themselves.
    ''' </summary>
    NoChange = 2

    ''' <summary>
    ''' Something the server will refuse to move. Drawn with a padlock and in FULL colour: see the
    ''' remark on this enum for why it is not greyed.
    ''' </summary>
    Locked = 3

    ''' <summary>
    ''' Not placed on anything yet. Drawn as a diamond, which is the only shape here that is not a
    ''' variation on the disc — it belongs to a different lane and it should look like it.
    ''' </summary>
    Loose = 4
End Enum

''' <summary>
''' The mark at the closed end of a lane: does the lane finish where it should?
''' </summary>
''' <remarks>
''' This is F15 made visible — "the chain end must equal the reception's current value" — and F15
''' is deliberately a SIGN, never a refusal. A lane whose end does not match is not an error the
''' control knows how to fix; it is something the operator has to see and decide about.
''' </remarks>
Public Enum KBotLaneEndMark

    ''' <summary>Nothing is claimed. An empty lane, or one the host has no opinion about.</summary>
    None = 0

    ''' <summary>The lane closes where it should.</summary>
    Ok = 1

    ''' <summary>The lane does not close where it should. A sign, not a refusal.</summary>
    Warning = 2
End Enum
