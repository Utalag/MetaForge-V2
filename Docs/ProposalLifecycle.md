# Proposal Lifecycle

Účel: Zavést jednotný životní cyklus návrhů v MetaForge tak, aby se nemíchaly volné nápady, kandidátní návrhy a aktivně schválené implementační položky.

## Stavový model

- Idea — volný nápad nebo směr, ještě bez závazku.
- Candidate — kandidátní návrh zapsaný v `PROPOSALS_NEXT.md` a rozpracovaný v detailním `Docs/Plans/PROP-xxx-*.md`.
- Approved — kandidát ručně vybraný do aktivního pořadí v `PROPOSALS.md`.
- In Progress — návrh je aktivně implementován nebo rozpracován.
- Done — návrh byl dokončen a uzavřen v akceptovaném rozsahu.
- Dropped — návrh byl vědomě zavržen, zastaral nebo nahrazen.

## Povolené přechody

- Idea -> Candidate
- Idea -> Dropped
- Idea -> Open Question
- Candidate -> Approved
- Candidate -> Dropped
- Approved -> In Progress
- Approved -> Dropped
- In Progress -> Done
- In Progress -> Dropped

## Governance pravidla

- `PROPOSALS.md` je pouze master checklist aktivních a prioritizovaných návrhů.
- `PROPOSALS_NEXT.md` je kandidátní backlog.
- Detail každého kandidáta žije v `Docs/Plans/PROP-xxx-*.md`.
- `Idea` záznamy se ukládají do `.github/ideas/IDEA-xxx-*.md` podle šablony `temp-idea.md`.
- `Open Question` záznamy se ukládají do `Docs/OpenQuestions/OQ-xxx_PROP-xxx_name.md`.
- Pokud chybí základní architektonické rozhodnutí, nevzniká hotový návrh, ale `Open Question`.
- Planning agent nesmí zapisovat přímo do `PROPOSALS.md`.
- Přesun `Candidate -> Approved` je vždy vědomé rozhodnutí ownera nebo orchestrátoru.

## Stavové značky v PROPOSALS.md

- `[ ]` Schváleno (Approved), ještě nezačato
- `[-]` Rozpracováno (In Progress)
- `[x]` Dokončeno (Done)
- `[>]` Odloženo na později (Dropped s možným návratem)
- `[!]` Blokováno otevřenou otázkou nebo závislostí

## Role Planning Agenta

Planning agent:
- analyzuje nápad nebo problém,
- rozlišuje návrh, follow-up a otevřenou otázku,
- vytváří detailní `PROP` dokument podle šablony,
- zapisuje kandidátní položku do `PROPOSALS_NEXT.md`,
- upozorňuje na duplicity, blokace a chybějící rozhodnutí.

Planning agent není schvalovací autorita.