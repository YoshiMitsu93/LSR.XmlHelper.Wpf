namespace LSR.XmlHelper.Core.Models
{
    public sealed class GangEditSnapshot
    {
        public GangEditSnapshot(
            string gangId,
            string fullName,
            string peopleGroupId,
            string vehicleGroupId,
            string dealerMenuGroupId,
            string meleeWeaponsId,
            string sideArmsId,
            string longGunsId)
        {
            GangId = gangId;
            FullName = fullName;
            PeopleGroupId = peopleGroupId;
            VehicleGroupId = vehicleGroupId;
            DealerMenuGroupId = dealerMenuGroupId;
            MeleeWeaponsId = meleeWeaponsId;
            SideArmsId = sideArmsId;
            LongGunsId = longGunsId;
        }

        public string GangId { get; }
        public string FullName { get; }
        public string PeopleGroupId { get; }
        public string VehicleGroupId { get; }
        public string DealerMenuGroupId { get; }
        public string MeleeWeaponsId { get; }
        public string SideArmsId { get; }
        public string LongGunsId { get; }
    }
}
