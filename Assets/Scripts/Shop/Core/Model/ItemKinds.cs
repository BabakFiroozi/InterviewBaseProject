namespace Shop.Model
{
    /// <summary>
    /// Well-known values for <see cref="ShopItemConfig.kind"/>. Kinds are plain strings so that new
    /// ones can be introduced from JSON alone. Every kind except <see cref="PowerUp"/> is "equippable":
    /// it is bought once, and one item per kind (the "slot") can be equipped at a time.
    /// </summary>
    public static class ItemKinds
    {
        public const string PowerUp = "powerup";
        public const string PlayerTheme = "playerTheme";
        public const string WorldTheme = "worldTheme";
    }
}
