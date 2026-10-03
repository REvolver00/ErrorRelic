# H104 / E109 expansion — focused test plan

This build intentionally leaves risky reward/UI effects in the random pool. The goal is to find hard runtime/multiplayer failures first; balance, no-op and absurd power are not failures.

## 1. Fast smoke tests

Use the existing `relicspawn Hxxx Exxx` command where available.

- `H102 + E097`: Calling Bell pickup Hook → add Curse of the Bell.
- `H102 + E098`: Calling Bell pickup Hook → Common/Uncommon/Rare relic rewards.
- `H103 + E099`: Toy Box pickup Hook → 5 Wax relic rewards.
- `H104 + E100`: every 3 completed combats → melt leftmost unmelted Wax; stop after 15 combats / 5 activations.
- `H087 + E101`: Dolly's Mirror deck selection, clone, add to Deck Bottom, preview.
- `H089 + E102`: Gnarled Hammer up-to-3 Sharp 3 selector.
- `H090 + E103`: Kifuda up-to-3 Adroit 3 selector.
- `H091 + E104`: Punch Dagger Momentum 5 selector.
- `H092 + E105`: Tri-Boomerang Instinct 1 selector + enchant VFX.
- `H098 + E106`: Orrery 5 card rewards.
- `H099 + E107`: Glass Eye 2 Common / 2 Uncommon / 1 Rare reward sequence.
- `H100 + E108`: Small Capsule one random relic reward.
- `H101 + E109`: native Choices Paradox timing, 5 Retain candidates → choose 1 to hand.

## 2. Reward-screen stress matrix

For E098 / E099 / E106 / E107 / E108, force each under at least these Hook families:

- AfterObtained
- combat start / first turn
- AfterCardPlayed
- AfterCardExhausted
- enemy-death / combat-event Hook already present in ERROR
- AfterCombatEnd

Failure criteria to record:

- screen cannot close;
- action queue stops advancing after closing;
- reward can be claimed twice;
- reward appears only on one peer but state changes on both/vice versa;
- different peers consume different relic/card RNG positions;
- save/reload changes already-selected reward outcome;
- one peer enters an unrecoverable screen/state.

A reward being absurd, recursively granting more ERROR relics, or producing a long but recoverable cascade is **not** by itself a failure.

## 3. Calling Bell cascade tests

Especially test:

- `AfterObtained + E098`
- `AfterObtained + E108`

A newly obtained ERROR relic can itself have an AfterObtained Hook. Deliberately allow reward cascades and nested acquisition. Only treat crashes, softlocks, broken screen stack, save corruption or multiplayer divergence as blockers.

## 4. Toy Box / Wax multiplayer tests

Test host and client after E099:

- both peers see the same five concrete relics;
- selected relic order matches;
- each selected relic preserves `IsWax=true` after acquisition;
- save/reload preserves Wax state and inventory order;
- H104 counter survives save/reload;
- combat 3/6/9/12/15 melts exactly the leftmost currently unmelted Wax relic;
- both peers agree on `IsMelted` state;
- no sixth activation after 15 combats.

Also test mixing E099 with pre-existing Wax relics: E100 should follow `Owner.Relics.FirstOrDefault(IsWax && !IsMelted)` exactly, not “the Wax relics created by this ERROR relic.”

## 5. Choice/deck selector tests

E090/E091/E101–E105 should be tested:

- out of combat (normal expected use);
- during combat from a rewired Hook;
- with very small decks / few eligible cards;
- multiplayer host/client selection;
- save/reload after the selected mutation.

For Tri-Boomerang specifically verify selection behavior when fewer than three enchantable cards exist. The build intentionally preserves the source selector rather than silently changing “choose 3” to “up to 3.” If the source UI can softlock in a rewired deck state, that is a candidate for an E002/E007-style temporary blacklist, not for weakening the Effect.

## 6. Choices Paradox context matrix

E109:

- works normally under Hooks that supply a real `PlayerChoiceContext`;
- is combat-only;
- safely no-ops under Hooks that use `ThrowingPlayerChoiceContext` rather than manufacturing an unsynchronized replacement context;
- candidate cards all receive Retain before selection;
- only the selected card is added to Hand Bottom;
- if no candidate cards are generated, returns without opening a selector (matching vanilla's anti-softlock intent).

## 7. Persistent counters

- H081 Iron Club: card count persists across combats and save/reload; triggers every 4 owned cards played.
- H104 Toy Box: `CombatsSeen` persists across combats and save/reload; increments only until 15.
- H062/H063 Shuriken/Kunai: counters reset at owner side-turn start and combat reset, not persisted.
- H085 Permafrost: resets once per combat.

## 8. Pass/fail philosophy

Allowed:

- extreme power;
- negative outcomes;
- no-op caused by wrong context;
- bizarre nested rewards;
- huge run-state changes that still serialize and synchronize correctly.

Block/blacklist candidate:

- crash;
- hard/soft lock;
- corrupted save;
- deterministic multiplayer divergence;
- a client trapped in an unrecoverable UI/action state.
