namespace ErrorRelics.ErrorRelicsCode.Fragments;

/// <summary>
/// Generation-only tags for Effects that can open blocking reward/card-selection
/// flows or permanently mutate a player's run inventory/deck. These Effects are
/// valid, but repeated ERROR triggers can interrupt multiplayer flow while peers
/// wait on player choices, so players may opt them out of future generation.
///
/// This tag is intentionally independent from Ancient-source metadata. An Effect
/// may belong to both sets; generation applies both enabled filters together.
/// </summary>
public static class ErrorEffectGenerationMetadata
{
    public static bool MayAffectMultiplayerFlow(ErrorEffectId effectId) => effectId switch
    {
        // Select cards to transform / upgrade / remove.
        ErrorEffectId.E008_Transform3AndUpgrade => true,             // Astrolabe
        ErrorEffectId.E090_EmptyCageRemove2FromDeck => true,         // Empty Cage
        ErrorEffectId.E091_PomanderUpgrade1FromDeck => true,         // Pomander

        // Select cards to enchant.
        ErrorEffectId.E009_Enchant1CardRoyallyApproved => true,      // Royal Stamp
        ErrorEffectId.E102_GnarledHammerEnchantUpTo3Sharp3 => true,  // Gnarled Hammer
        ErrorEffectId.E103_KifudaEnchantUpTo3Adroit3 => true,        // Kifuda
        ErrorEffectId.E104_PunchDaggerEnchant1Momentum5 => true,     // Punch Dagger
        ErrorEffectId.E105_TriBoomerangEnchant3Instinct1 => true,    // Tri-Boomerang

        // Select a deck card to duplicate.
        ErrorEffectId.E101_DollysMirrorDuplicate1NonQuestCard => true, // Dolly's Mirror

        // Open reward/choice flows during a run. Relic rewards are the main
        // reported multiplayer interruption; card-reward/grid flows are tagged
        // for the same reason: they block on a player's choice before returning.
        ErrorEffectId.E098_CallingBellOfferThreeRelicRewards => true, // Calling Bell
        ErrorEffectId.E099_ToyBoxOfferFiveWaxRelics => true,          // Toy Box
        ErrorEffectId.E106_OrreryOffer5CardRewards => true,           // Orrery
        ErrorEffectId.E107_GlassEyeOffer5RarityCardRewards => true,   // Glass Eye
        ErrorEffectId.E108_SmallCapsuleOffer1RelicReward => true,     // Small Capsule
        ErrorEffectId.E109_ChoicesParadoxChoose1Of5RetainToHand => true, // Choices Paradox

        _ => false
    };
}
