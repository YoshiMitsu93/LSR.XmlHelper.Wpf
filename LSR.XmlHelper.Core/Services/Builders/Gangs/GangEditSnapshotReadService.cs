using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangEditSnapshotReadService
    {
        public GangEditSnapshot? TryGet(string rootFolderPath, string gangId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return null;

            if (!Directory.Exists(rootFolderPath))
                return null;

            if (string.IsNullOrWhiteSpace(gangId))
                return null;

            var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
            var resolvedPath = resolver.ResolveGangFile(rootFolderPath, gangId);

            if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
                return null;

            XDocument doc;

            try
            {
                doc = XDocument.Load(resolvedPath, LoadOptions.None);
            }
            catch
            {
                return null;
            }

            var gang = doc
                .Descendants("Gang")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), gangId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (gang is null)
                return null;

            string ReadValue(string elementName)
            {
                return ((string?)gang.Element(elementName) ?? "").Trim();
            }

            var fullName = ReadValue("FullName");

            var peopleGroupId = ReadValue("PeopleGroupID");
            if (string.IsNullOrWhiteSpace(peopleGroupId))
                peopleGroupId = ReadValue("PersonnelID");

            var vehicleGroupId = ReadValue("VehicleGroupID");
            if (string.IsNullOrWhiteSpace(vehicleGroupId))
                vehicleGroupId = ReadValue("VehiclesID");

            var dealerMenuGroupId = ReadValue("DealerMenuGroupID");
            if (string.IsNullOrWhiteSpace(dealerMenuGroupId))
                dealerMenuGroupId = ReadValue("DealerMenuGroup");

            var meleeWeaponsId = ReadValue("MeleeWeaponsID");
            var sideArmsId = ReadValue("SideArmsID");
            var longGunsId = ReadValue("LongGunsID");
            var colorPrefix = ReadValue("ColorPrefix");
            var colorString = ReadValue("ColorString");

            var loanParams = new List<GangLoanParameterSnapshot>();

            var loanParameterNodes = gang
                .Descendants("LoanParameter")
                .ToList();

            foreach (var node in loanParameterNodes)
            {
                var resepectLevel = ((string?)node.Element("ResepectLevel") ?? "").Trim();
                var rate = ((string?)node.Element("Rate") ?? "").Trim();
                var maxPeriods = ((string?)node.Element("MaxPeriods") ?? "").Trim();
                var minAmount = ((string?)node.Element("MinAmount") ?? "").Trim();
                var maxAmount = ((string?)node.Element("MaxAmount") ?? "").Trim();

                if (string.IsNullOrWhiteSpace(resepectLevel)
                    && string.IsNullOrWhiteSpace(rate)
                    && string.IsNullOrWhiteSpace(maxPeriods)
                    && string.IsNullOrWhiteSpace(minAmount)
                    && string.IsNullOrWhiteSpace(maxAmount))
                    continue;

                loanParams.Add(new GangLoanParameterSnapshot(
                    resepectLevel,
                    rate,
                    maxPeriods,
                    minAmount,
                    maxAmount));
            }

            return new GangEditSnapshot(
                gangId.Trim(),
                fullName,
                peopleGroupId,
                vehicleGroupId,
                dealerMenuGroupId,
                meleeWeaponsId,
                sideArmsId,
                longGunsId,
                colorPrefix,
                colorString,
                ReadValue("MinimumRep"),
                ReadValue("MaximumRep"),
                ReadValue("StartingRep"),
                ReadValue("HostileRepLevel"),
                ReadValue("NeutralRepLevel"),
                ReadValue("FriendlyRepLevel"),
                ReadValue("MemberOfferRepLevel"),
                ReadValue("HitSquadRep"),
                ReadValue("PickupPaymentMin"),
                ReadValue("PickupPaymentMax"),
                ReadValue("TheftPaymentMin"),
                ReadValue("TheftPaymentMax"),
                ReadValue("HitPaymentMin"),
                ReadValue("HitPaymentMax"),
                ReadValue("DeliveryPaymentMin"),
                ReadValue("DeliveryPaymentMax"),
                ReadValue("WheelmanPaymentMin"),
                ReadValue("WheelmanPaymentMax"),
                ReadValue("ImpoundTheftPaymentMin"),
                ReadValue("ImpoundTheftPaymentMax"),
                ReadValue("BodyDisposalPaymentMin"),
                ReadValue("BodyDisposalPaymentMax"),
                ReadValue("CopHitPaymentMin"),
                ReadValue("CopHitPaymentMax"),
                ReadValue("AmbushPaymentMin"),
                ReadValue("AmbushPaymentMax"),
                ReadValue("BriberyPaymentMin"),
                ReadValue("BriberyPaymentMax"),
                ReadValue("ArsonPaymentMin"),
                ReadValue("ArsonPaymentMax"),
                ReadValue("FightPercentage"),
                ReadValue("FightPolicePercentage"),
                ReadValue("AlwaysFightPolicePercentage"),
                ReadValue("DrugDealerPercentage"),
                ReadValue("AmbientMemberMoneyMin"),
                ReadValue("AmbientMemberMoneyMax"),
                ReadValue("DealerMemberMoneyMin"),
                ReadValue("DealerMemberMoneyMax"),
                ReadValue("CostToPayoffGangScalar"),
                ReadValue("PercentageTrustingOfPlayer"),
                ReadValue("PercentageWithLongGuns"),
                ReadValue("PercentageWithSidearms"),
                ReadValue("PercentageWithMelee"),
                ReadValue("VehicleSpawnPercentage"),
                ReadValue("PedestrianSpawnPercentageAroundDen"),
                ReadValue("MemberKickUpDays"),
                ReadValue("MemberKickUpAmount"),
                ReadValue("MemberKickUpMissLimit"),
                loanParams);
        }
    }
}
