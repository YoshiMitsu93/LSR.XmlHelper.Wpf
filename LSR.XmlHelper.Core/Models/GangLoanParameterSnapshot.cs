namespace LSR.XmlHelper.Core.Models
{
    public sealed class GangLoanParameterSnapshot
    {
        public GangLoanParameterSnapshot(
            string resepectLevel,
            string rate,
            string maxPeriods,
            string minAmount,
            string maxAmount)
        {
            ResepectLevel = resepectLevel;
            Rate = rate;
            MaxPeriods = maxPeriods;
            MinAmount = minAmount;
            MaxAmount = maxAmount;
        }

        public string ResepectLevel { get; }
        public string Rate { get; }
        public string MaxPeriods { get; }
        public string MinAmount { get; }
        public string MaxAmount { get; }
    }
}
