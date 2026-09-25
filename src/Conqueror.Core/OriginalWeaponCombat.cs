namespace Conqueror.Core;

// FMT-ASSAULT-002 combat rows and RULE-ASSAULT-023 blow resolution.
public static class OriginalWeaponCombat
{
    public const int CombatRowCount = 25;
    public const int HitRollRange = 200;
    public const int BaseHitThreshold = 100;
    public const int PositionalHitBonus = 30;
    private static readonly int[] CombatRowsByItemId =
    [
        4, 5, 12, 14, 6, 13, 11, 7, 9, 8, 10, 16,
        17, 18, 15, 3, 0, 1, 2, 19, 20, 21, 22
    ];

    // RULE-ASSAULT-031: a miss breaks the weapon with odds set by 200 + 50 * this rating.
    private static readonly int[] BreakRatingsByCombatRow =
    [
        4, 5, 6, 3, 6, 5, 3, 4, 2, 6, 0, 2,
        1, 0, 1, 3, 2, 1, 1, 3, 1, 3, 3, 6, 6
    ];

    // FMT-ASSAULT-002: dice, sides and penetration, columns 0-2 of each 28-byte row.
    private static readonly int[] DiceCountsByCombatRow =
    [
        1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        2, 2, 2, 2, 2, 1, 3, 3, 1, 1, 2, 2
    ];

    private static readonly int[] DieSidesByCombatRow =
    [
        6, 5, 5, 4, 9, 8, 9, 8, 5, 7, 9, 8, 8,
        8, 7, 7, 6, 6, 12, 4, 4, 16, 12, 6, 7
    ];

    private static readonly int[] ArmorPenetrationByCombatRow =
    [
        4, 5, 6, 3, 6, 5, 3, 4, 2, 6, 0, 2, 1,
        0, 1, 3, 2, 1, 1, 3, 1, 3, 3, 6, 6
    ];

    // FMT-ASSAULT-002 column 3. RULE-ASSAULT-026: signed foreground velocity
    // is (screen displacement << 9) / this divisor.
    private static readonly int[] ForegroundMotionDivisorsByCombatRow =
    [
        400, 400, 380, 380, 400, 420, 460, 450, 470, 480, 500, 440, 480,
        480, 480, 520, 500, 500, 500, 520, 530, 520, 530, 900, 1000
    ];

    // FMT-ASSAULT-002 column 4 (reach). RULE-ASSAULT-005: the player's strike
    // adds 0x40 to it before comparing the range.
    private static readonly int[] ContactDistancesByCombatRow =
    [
        350, 350, 350, 350, 450, 440, 430, 420, 420, 420, 420, 420, 420,
        420, 420, 400, 400, 400, 400, 380, 380, 360, 360, 7000, 8192
    ];

    public static int CombatRowFor(int itemId)
    {
        if ((uint)itemId < (uint)CombatRowsByItemId.Length) return CombatRowsByItemId[itemId];
        return itemId switch
        {
            43 => 23,
            44 => 24,
            _ => throw new ArgumentOutOfRangeException(nameof(itemId))
        };
    }

    public static int? BreakRollRangeFor(int itemId)
    {
        var row = CombatRowFor(itemId);
        return row == 0 ? null : checked(200 + BreakRatingsByCombatRow[row] * 50);
    }

    public static int ContactDistanceFor(int itemId) => ContactDistanceForCombatRow(CombatRowFor(itemId));

    public static int GridReachFor(int itemId) => Math.Max(1, ContactDistanceFor(itemId) >> 8);

    public static int ContactDistanceForCombatRow(int combatRow)
    {
        if ((uint)combatRow >= (uint)CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        return checked(ContactDistancesByCombatRow[combatRow] + 0x40);
    }

    // RULE-ASSAULT-020: actor mode 11 compares the ray distance with the reach
    // column itself, without the player's 0x40 allowance.
    public static int ActorContactDistanceForCombatRow(int combatRow)
    {
        if ((uint)combatRow >= (uint)CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        return ContactDistancesByCombatRow[combatRow];
    }

    public static int GridReachForCombatRow(int combatRow) =>
        Math.Max(1, ContactDistanceForCombatRow(combatRow) >> 8);

    public static int ForegroundMotionDivisorForCombatRow(int combatRow)
    {
        if ((uint)combatRow >= (uint)CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        return ForegroundMotionDivisorsByCombatRow[combatRow];
    }

    public static int ForegroundVelocityForCombatRow(int combatRow, int screenDisplacement) =>
        checked((int)(((long)screenDisplacement << 9) / ForegroundMotionDivisorForCombatRow(combatRow)));

    // RULE-ASSAULT-024: the player's attack skill comes from strength,
    // dexterity and sword experience.
    public static int PlayerAttackSkill(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return checked(player.Stats.Strength + player.Stats.Dexterity + player.SwordExperience * 2);
    }

    // RULE-ASSAULT-024: the player's health comes from strength, stamina and honor.
    public static int PlayerHealth(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return checked(player.Stats.Strength + player.Stats.Stamina + player.Stats.Honor);
    }

    // RULE-ASSAULT-023: a blow hits when random(200) is below this threshold.
    public static int HitThreshold(int attackerSkill, int defenderSkill, bool closeRanged, bool behindDefender)
    {
        if (attackerSkill < 0) throw new ArgumentOutOfRangeException(nameof(attackerSkill));
        if (defenderSkill < 0) throw new ArgumentOutOfRangeException(nameof(defenderSkill));
        return checked(BaseHitThreshold + attackerSkill - defenderSkill / 2
            + (closeRanged ? PositionalHitBonus : 0)
            + (behindDefender ? PositionalHitBonus : 0));
    }

    public static bool Hits(int attackerSkill, int defenderSkill, bool closeRanged, bool behindDefender, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return random.Next(HitRollRange) < HitThreshold(attackerSkill, defenderSkill, closeRanged, behindDefender);
    }

    public static int DamageFor(int itemId, int targetArmor, Random random)
        => DamageForCombatRow(CombatRowFor(itemId), targetArmor, random);

    public static int DamageForCombatRow(int combatRow, int targetArmor, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if ((uint)combatRow >= (uint)CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        if (targetArmor < 0) throw new ArgumentOutOfRangeException(nameof(targetArmor));
        var rolled = 0;
        for (var die = 0; die < DiceCountsByCombatRow[combatRow]; die++)
            rolled += random.Next(DieSidesByCombatRow[combatRow]) + 1;
        var mitigation = Math.Max(targetArmor - ArmorPenetrationByCombatRow[combatRow], 0);
        return Math.Max(rolled - mitigation, 0);
    }
}
