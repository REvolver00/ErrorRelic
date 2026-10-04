# Campfire Gift Relic — 0.93.1 experimental integration

Co-op only. Adds a normal campfire action that lets the acting player choose one of their relics and a teammate. The source relic is serialized before removal, removed with `RelicCmd.Remove`, reconstructed with `RelicModel.FromSerializable`, and immediately obtained by the recipient through `RelicCmd.Obtain`.

This deliberately fires the recipient's normal `AfterObtained` lifecycle. There is no recipient accept/decline step and no reward screen. ERROR SavedProperty state (H/E identity and discovery flags) travels with the serialized relic. ERROR PROOF is excluded because it owns run activation/discovery charge semantics.

Networking follows the reference campfire-trade mod's key invariants: the rest-site option is present at the same index on every peer, a `PlayerChoiceSynchronizer` choice id is reserved/synced for the campfire action, and a reliable mod-defined network message makes every peer execute the same source-index -> target transfer. The sender applies locally before broadcasting; remote peers apply on receipt.

This feature requires real co-op testing before release. In particular test interactive `AfterObtained` Effects, repeated A->B->C->D gifting, cancel paths, leaving campfire, and 3/4-player sessions.
