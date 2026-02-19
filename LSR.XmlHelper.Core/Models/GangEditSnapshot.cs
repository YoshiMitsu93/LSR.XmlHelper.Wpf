using System.Collections.Generic;

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
            string longGunsId,
            string colorPrefix,
            string colorString,
            string minimumRep,
            string maximumRep,
            string startingRep,
            string hostileRepLevel,
            string neutralRepLevel,
            string friendlyRepLevel,
            string memberOfferRepLevel,
            string hitSquadRep,
            string pickupPaymentMin,
            string pickupPaymentMax,
            string theftPaymentMin,
            string theftPaymentMax,
            string hitPaymentMin,
            string hitPaymentMax,
            string deliveryPaymentMin,
            string deliveryPaymentMax,
            string wheelmanPaymentMin,
            string wheelmanPaymentMax,
            string impoundTheftPaymentMin,
            string impoundTheftPaymentMax,
            string bodyDisposalPaymentMin,
            string bodyDisposalPaymentMax,
            string copHitPaymentMin,
            string copHitPaymentMax,
            string ambushPaymentMin,
            string ambushPaymentMax,
            string briberyPaymentMin,
            string briberyPaymentMax,
            string arsonPaymentMin,
            string arsonPaymentMax,
            string fightPercentage,
            string fightPolicePercentage,
            string alwaysFightPolicePercentage,
            string drugDealerPercentage,
            string ambientMemberMoneyMin,
            string ambientMemberMoneyMax,
            string dealerMemberMoneyMin,
            string dealerMemberMoneyMax,
            string costToPayoffGangScalar,
            string percentageTrustingOfPlayer,
            string percentageWithLongGuns,
            string percentageWithSidearms,
            string percentageWithMelee,
            string vehicleSpawnPercentage,
            string pedestrianSpawnPercentageAroundDen,
            string memberKickUpDays,
            string memberKickUpAmount,
            string memberKickUpMissLimit,
            IReadOnlyList<GangLoanParameterSnapshot> loanParameters)
        {
            GangId = gangId;
            FullName = fullName;
            PeopleGroupId = peopleGroupId;
            VehicleGroupId = vehicleGroupId;
            DealerMenuGroupId = dealerMenuGroupId;
            MeleeWeaponsId = meleeWeaponsId;
            SideArmsId = sideArmsId;
            LongGunsId = longGunsId;
            ColorPrefix = colorPrefix;
            ColorString = colorString;

            MinimumRep = minimumRep;
            MaximumRep = maximumRep;
            StartingRep = startingRep;
            HostileRepLevel = hostileRepLevel;
            NeutralRepLevel = neutralRepLevel;
            FriendlyRepLevel = friendlyRepLevel;
            MemberOfferRepLevel = memberOfferRepLevel;
            HitSquadRep = hitSquadRep;

            PickupPaymentMin = pickupPaymentMin;
            PickupPaymentMax = pickupPaymentMax;
            TheftPaymentMin = theftPaymentMin;
            TheftPaymentMax = theftPaymentMax;
            HitPaymentMin = hitPaymentMin;
            HitPaymentMax = hitPaymentMax;
            DeliveryPaymentMin = deliveryPaymentMin;
            DeliveryPaymentMax = deliveryPaymentMax;
            WheelmanPaymentMin = wheelmanPaymentMin;
            WheelmanPaymentMax = wheelmanPaymentMax;
            ImpoundTheftPaymentMin = impoundTheftPaymentMin;
            ImpoundTheftPaymentMax = impoundTheftPaymentMax;
            BodyDisposalPaymentMin = bodyDisposalPaymentMin;
            BodyDisposalPaymentMax = bodyDisposalPaymentMax;
            CopHitPaymentMin = copHitPaymentMin;
            CopHitPaymentMax = copHitPaymentMax;
            AmbushPaymentMin = ambushPaymentMin;
            AmbushPaymentMax = ambushPaymentMax;
            BriberyPaymentMin = briberyPaymentMin;
            BriberyPaymentMax = briberyPaymentMax;
            ArsonPaymentMin = arsonPaymentMin;
            ArsonPaymentMax = arsonPaymentMax;

            FightPercentage = fightPercentage;
            FightPolicePercentage = fightPolicePercentage;
            AlwaysFightPolicePercentage = alwaysFightPolicePercentage;
            DrugDealerPercentage = drugDealerPercentage;

            AmbientMemberMoneyMin = ambientMemberMoneyMin;
            AmbientMemberMoneyMax = ambientMemberMoneyMax;
            DealerMemberMoneyMin = dealerMemberMoneyMin;
            DealerMemberMoneyMax = dealerMemberMoneyMax;
            CostToPayoffGangScalar = costToPayoffGangScalar;

            PercentageTrustingOfPlayer = percentageTrustingOfPlayer;
            PercentageWithLongGuns = percentageWithLongGuns;
            PercentageWithSidearms = percentageWithSidearms;
            PercentageWithMelee = percentageWithMelee;

            VehicleSpawnPercentage = vehicleSpawnPercentage;
            PedestrianSpawnPercentageAroundDen = pedestrianSpawnPercentageAroundDen;

            MemberKickUpDays = memberKickUpDays;
            MemberKickUpAmount = memberKickUpAmount;
            MemberKickUpMissLimit = memberKickUpMissLimit;

            LoanParameters = loanParameters ?? new List<GangLoanParameterSnapshot>();
        }

        public string GangId { get; }
        public string FullName { get; }
        public string PeopleGroupId { get; }
        public string VehicleGroupId { get; }
        public string DealerMenuGroupId { get; }
        public string MeleeWeaponsId { get; }
        public string SideArmsId { get; }
        public string LongGunsId { get; }
        public string ColorPrefix { get; }
        public string ColorString { get; }

        public string MinimumRep { get; }
        public string MaximumRep { get; }
        public string StartingRep { get; }
        public string HostileRepLevel { get; }
        public string NeutralRepLevel { get; }
        public string FriendlyRepLevel { get; }
        public string MemberOfferRepLevel { get; }
        public string HitSquadRep { get; }

        public string PickupPaymentMin { get; }
        public string PickupPaymentMax { get; }
        public string TheftPaymentMin { get; }
        public string TheftPaymentMax { get; }
        public string HitPaymentMin { get; }
        public string HitPaymentMax { get; }
        public string DeliveryPaymentMin { get; }
        public string DeliveryPaymentMax { get; }
        public string WheelmanPaymentMin { get; }
        public string WheelmanPaymentMax { get; }
        public string ImpoundTheftPaymentMin { get; }
        public string ImpoundTheftPaymentMax { get; }
        public string BodyDisposalPaymentMin { get; }
        public string BodyDisposalPaymentMax { get; }
        public string CopHitPaymentMin { get; }
        public string CopHitPaymentMax { get; }
        public string AmbushPaymentMin { get; }
        public string AmbushPaymentMax { get; }
        public string BriberyPaymentMin { get; }
        public string BriberyPaymentMax { get; }
        public string ArsonPaymentMin { get; }
        public string ArsonPaymentMax { get; }

        public string FightPercentage { get; }
        public string FightPolicePercentage { get; }
        public string AlwaysFightPolicePercentage { get; }
        public string DrugDealerPercentage { get; }

        public string AmbientMemberMoneyMin { get; }
        public string AmbientMemberMoneyMax { get; }
        public string DealerMemberMoneyMin { get; }
        public string DealerMemberMoneyMax { get; }
        public string CostToPayoffGangScalar { get; }

        public string PercentageTrustingOfPlayer { get; }
        public string PercentageWithLongGuns { get; }
        public string PercentageWithSidearms { get; }
        public string PercentageWithMelee { get; }

        public string VehicleSpawnPercentage { get; }
        public string PedestrianSpawnPercentageAroundDen { get; }

        public string MemberKickUpDays { get; }
        public string MemberKickUpAmount { get; }
        public string MemberKickUpMissLimit { get; }

        public IReadOnlyList<GangLoanParameterSnapshot> LoanParameters { get; }
    }
}
