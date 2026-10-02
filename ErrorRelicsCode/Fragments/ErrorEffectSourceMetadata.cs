namespace ErrorRelics.ErrorRelicsCode.Fragments;

/// <summary>
/// Mechanical source metadata for ERROR Effects. This is deliberately based on
/// the source relic's vanilla rarity, not a subjective power rating.
/// </summary>
public static class ErrorEffectSourceMetadata
{
    public static bool IsAncientSource(ErrorEffectId effectId) => effectId switch
    {
        ErrorEffectId.E006_Add3Apparitions => true,                 // Distinguished Cape
        ErrorEffectId.E008_Transform3AndUpgrade => true,            // Astrolabe
        ErrorEffectId.E010_Add2RandomCurses => true,                // Distinguished Cape
        ErrorEffectId.E011_EnchantBasicStrikesTezcatarasEmber => true, // Nutritious Soup
        ErrorEffectId.E012_EnchantAllGoopyEligible => true,         // Pael's Claw
        ErrorEffectId.E013_Upgrade6RandomCards => true,             // Sand Castle
        ErrorEffectId.E016_UpgradeBasicStrikeAndDefend => true,     // Neow's Talisman
        ErrorEffectId.E017_Add2Relax => true,                       // Pael's Horn
        ErrorEffectId.E018_AddNeowsFury => true,                    // Neow's Torment
        ErrorEffectId.E019_AddBrightestFlame => true,               // Storybook
        ErrorEffectId.E020_AddApotheosis => true,                   // Jewelry Box
        ErrorEffectId.E021_AddWhistle => true,                      // Tanx's Whistle
        ErrorEffectId.E069_VeryHotCocoaGainEnergy4 => true,         // Very Hot Cocoa
        ErrorEffectId.E073_LoomingFruitGainMaxHp31 => true,         // Looming Fruit
        ErrorEffectId.E074_NutritiousOysterGainMaxHp11 => true,     // Nutritious Oyster
        ErrorEffectId.E075_GoldenPearlGainGold150 => true,          // Golden Pearl
        ErrorEffectId.E076_SignetRingGainGold888 => true,           // Signet Ring
        ErrorEffectId.E078_SaiGainBlock7 => true,                   // Sai
        ErrorEffectId.E085_IronClubDraw1 => true,                   // Iron Club
        ErrorEffectId.E090_EmptyCageRemove2FromDeck => true,        // Empty Cage
        ErrorEffectId.E091_PomanderUpgrade1FromDeck => true,        // Pomander
        ErrorEffectId.E094_RadiantPearlAdd1LuminesceToHand => true, // Radiant Pearl
        ErrorEffectId.E097_CallingBellAddCurseOfTheBell => true,    // Calling Bell
        ErrorEffectId.E098_CallingBellOfferThreeRelicRewards => true, // Calling Bell
        ErrorEffectId.E099_ToyBoxOfferFiveWaxRelics => true,        // Toy Box
        ErrorEffectId.E100_ToyBoxMeltLeftmostWax => true,           // Toy Box
        ErrorEffectId.E105_TriBoomerangEnchant3Instinct1 => true,   // Tri-Boomerang
        ErrorEffectId.E107_GlassEyeOffer5RarityCardRewards => true, // Glass Eye
        ErrorEffectId.E108_SmallCapsuleOffer1RelicReward => true,   // Small Capsule
        ErrorEffectId.E109_ChoicesParadoxChoose1Of5RetainToHand => true, // Choices Paradox
        _ => false
    };
}
