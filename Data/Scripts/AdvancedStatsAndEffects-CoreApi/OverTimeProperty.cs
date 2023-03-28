namespace AdvancedStatsAndEffects
{
    public class OverTimeProperty
    {

        public float Max { get; set; }
        public float Current { get; set; }
        public float ConsumeRate { get; set; }
        public bool IsPositive { get; set; }

        public OverTimeProperty()
        {

        }

        public OverTimeProperty(float max, float consumeRate)
        {
            Max = max;
            Current = max;
            ConsumeRate = consumeRate;
            IsPositive = max > 0;
        }

        public void AddAmmount(float ammount)
        {
            Max += ammount;
            Current += ammount;
        }

        public PlayerData.OverTimePropertyData GetSaveData()
        {
            return new PlayerData.OverTimePropertyData()
            {
                Max = Max,
                Current = Current,
                ConsumeRate = ConsumeRate,
                IsPositive = IsPositive
            };
        }

        public PlayerData.OverTimeNamedPropertyData GetSaveData(string name)
        {
            return new PlayerData.OverTimeNamedPropertyData()
            {
                Name = name,
                Max = Max,
                Current = Current,
                ConsumeRate = ConsumeRate,
                IsPositive = IsPositive
            };
        }

        public static OverTimeProperty FromSaveData(PlayerData.OverTimePropertyData data)
        {
            return new OverTimeProperty()
            {
                Max = data.Max,
                Current = data.Current,
                ConsumeRate = data.ConsumeRate,
                IsPositive = data.IsPositive
            };
        }

    }

}
