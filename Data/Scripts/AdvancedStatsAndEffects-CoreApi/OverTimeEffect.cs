namespace AdvancedStatsAndEffects
{
    public class OverTimeEffect
    {

        public UniqueEntityId Id { get; set; }
        public string Target { get; set; }
        public OverTimeProperty CurrentValue { get; set; }

        public PlayerData.OverTimeEffectData GetSaveData()
        {
            return new PlayerData.OverTimeEffectData()
            {
                Id = Id.DefinitionId,
                Target = Target,
                CurrentValue = CurrentValue.GetSaveData()
            };
        }

        public static OverTimeEffect FromSaveData(PlayerData.OverTimeEffectData data)
        {
            return new OverTimeEffect()
            {
                Id = new UniqueEntityId(data.Id),
                Target = data.Target,
                CurrentValue = OverTimeProperty.FromSaveData(data.CurrentValue)
            };
        }

    }

}
