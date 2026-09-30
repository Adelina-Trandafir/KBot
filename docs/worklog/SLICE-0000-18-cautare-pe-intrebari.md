# SLICE-0000-18 — Search that understands a typed question (no model)

Plan: `docs/PLAN_help_assistant.md` § 0000-18.

## What changed and why

The old `HelpLibrary.Search` needed EVERY word of the query in a topic, matched whole topics
only and knew no word endings, so a real question («cum trimit un DDF?») found nothing. The
search now:

1. **Drops filler words** — one list, `HelpSearch.StopWords` (folded Romanian: cum, se, un, o,
   la, de, pot, sa, unde, ce, care, pentru, ...). Words of one letter are dropped too.
2. **Matches stems** — folded (lower case, no diacritics, one character for one character),
   a common Romanian ending dropped (`-urilor`, `-ilor`, `-ului`, `-ii`, `-ea`, `-a`, `-e`,
   `-i`...), then cut to 5 letters: «trimit» and «trimiterea» both become «trimi», «plăți» and
   «plata» both «plat». A second, looser key (the first 4 letters of the stem) scores half, so
   «semnez» still meets «semnare».
3. **Scores by how many words match**, not «all or nothing»: score = words matched × 100 +
   weight (capped at 99). Weights per word: title 10 / 5, keywords 6 / 3, section heading 8 / 4,
   section text 2 / 1 (full stem / first four letters). With three words or more a section must
   hold at least half of them; with one or two, one is enough.
4. **Searches per SECTION** — the text before the first `## ` and each `## ` heading with its
   text. A hit = topic + section heading + a snippet (~170 characters around the first match)
   + the section's anchor. A topic gives at most 3 hits, and its extra hits only for sections
   that match on their own (not just through the topic title).
5. `keywords:` is unchanged as the place for synonyms (content in 0000-19 / 0000-22).
6. **Done once at load**: `HelpSearch.Index` (called from `HelpLibrary.Parse`) splits each
   topic into sections, folds and stems title, keywords, headings and text into hash sets. A
   query only folds its own words and looks them up. The reading order used to break ties is
   worked out once per part.

Result model: `HelpHit(TopicId, SectionAnchor, Title, SectionTitle, Snippet, Score)` + `Key`
(`topicId#anchor`, the id the question log will store in 0000-21).

**Anchors.** `HelpSearch.AnchorFor(heading)` = folded words of the heading joined by dashes
(`pagina-forexe-certificatul-memorat`), unique inside the topic (`-2`, `-3`). The topic page
gives every level-2 heading the same id (`HelpHtml.AddSectionAnchors`, from the heading's
text, same order), so both sides agree without storing anything. The printed manual gets no
section ids (they would clash between topics).

**Help window.** `HelpForm.ShowTopic(id, sectionAnchor)`; page keys may be `t:<id>#<anchor>`
(history and «Înapoi» keep the section); the section is scrolled into view when the page has
loaded (`DocumentCompleted`), carried with the pending page when the browser is busy. The
results page (`HelpHtml.SearchPage`) lists one entry per section: «Topic › Section» + snippet,
link `topic:<id>#<anchor>`. `topic:id#anchor` links inside topics work too.

## Files touched

- `src/KBot.App/Help/HelpSearch.vb` (new): `HelpHit`, `HelpTermSet`, `HelpQueryTerm`,
  `HelpSection`, `HelpSearch` (fold, words, stop words, stem, plain text, anchors, index, search,
  snippet)
- `src/KBot.App/Help/HelpTopic.vb`: `Sections`, `TitleTerms`, `KeywordTerms` (Friend)
- `src/KBot.App/Help/HelpLibrary.vb`: `Parse` indexes; `Search` returns `List(Of HelpHit)`;
  old `Fold` removed (only the old search used it)
- `src/KBot.App/Help/HelpHtml.vb`: `SearchPage(query, hits)`, heading anchors, `.hit` style,
  `topic:id#anchor` links
- `src/KBot.App/Help/HelpForm.vb`: section in the page key, scroll to the section

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** Coverage lists
  `RobotQueueForm` (slice 0098, another thread's window; already noted in 0000-17).
- No tests, no app run (operator's rule).

## Left unverified or deferred

- The ranking was never run on real questions: the weights are a first guess. 0000-21 records
  every question and the hit opened, so they can be tuned from data.
- The scroll to a section relies on `HtmlElement.ScrollIntoView` in the WebBrowser control;
  not seen on screen.
- The help text describing the search is updated in 0000-22 (planned there, tag `0000-18`).
