namespace InventoryDir.Items
{
    [System.Serializable]
    public class ItemEffect
    {
        public EffectType type;
        public float value;
        public float duration; // 0 = instant/permanent
    }

    public enum EffectType
    {
        HealthRestore,
        HealthBoost,
        SpeedBoost,
        JumpBoost,
        StaminaRestore,
        StaminaBoost,
        ScaleIncrease,
        ScaleDecrease,
        GravityReduction
    }
}