using System;
using System.Collections.Generic;
using System.Linq;
using KolonyTools;
using UnityEngine;
using Contracts;
using ContractConfigurator;


class CaravanEndPointParameterFactory : ContractConfigurator.ReachStateFactory
{
    public static CelestialBody originBody, destinationBody;
    public static CaravanParameters.Situation originSituation, destinationSituation;

    public override bool Load(ConfigNode configNode)
    {
        var res = base.Load(configNode);
        return res;
    }

    public override ContractParameter Generate(Contract contract)
    {
        if (name == "BeAtOrigin")
            return new CaravanEndPointParameter(originBody, originSituation);
        else if (name == "BeAtDestination")
            return new CaravanEndPointParameter(destinationBody, destinationSituation);
        else
            throw new Exception(String.Format("Invalid name: {0}", name));
    }

}

class CaravanEndPointParameter : ContractConfigurator.Parameters.ReachState
{
    public CaravanEndPointParameter() {}

    public CaravanEndPointParameter(CelestialBody b, CaravanParameters.Situation s)
        : base(
            new List<CelestialBody>{ b }, "", CaravanParameters.GetSituationList(s),
            float.MinValue, float.MaxValue, 0.0f, float.MaxValue, 0.0d, double.MaxValue, double.MinValue, double.MaxValue, 0.0f, float.MaxValue, "")
    {
    }
}

class HasPartWithModuleFactory : ContractConfigurator.ParameterFactory
{
    protected string moduleName;

    public override bool Load(ConfigNode configNode)
    {
        if (!base.Load(configNode))
            return false;

        if (configNode.HasValue("moduleName"))
        {
            return ConfigNodeUtil.ParseValue<string>(configNode, "moduleName", n => moduleName = n, this, "");
        }
        else
        {
            return false;
        }
    }

    public override ContractParameter Generate(Contract contract)
    {
        return new HasPartWithModule(moduleName);
    }
}

class HasPartWithModule : ContractConfigurator.Parameters.PartValidation
{
    public HasPartWithModule() {}

    public HasPartWithModule(string moduleName)
        : base(GetFilters(moduleName))
    {
    }

    private static List<ContractConfigurator.Parameters.PartValidation.Filter> GetFilters(string moduleName)
    {
        var filter = new ContractConfigurator.Parameters.PartValidation.Filter();
        filter.partModules.Add(moduleName);
        return new List<ContractConfigurator.Parameters.PartValidation.Filter>{filter};
    }
}
    
namespace KolonyTools
{

    public class WolfContractHandling
    {
        private static string _contractTypeName = "MKS_Caravan";

        public static IEnumerable<CaravanParameters> GetActiveCaravanContracts()
        {
            foreach (var c in ContractSystem.Instance.GetCurrentActiveContracts<ConfiguredContract>(c => c.contractType.name == _contractTypeName))
            {
                yield return ExtractParameters(c);
            }
            yield break;
        }

        private static CaravanParameters ExtractParameters(ConfiguredContract contract)
        {
            var origin = contract.GetParameter("VesselParameterGroupCatch1").GetParameter("BeAtOrigin") as ContractConfigurator.Parameters.ReachState;
            var destination = contract.GetParameter("VesselParameterGroupCatch2").GetParameter("BeAtDestination") as ContractConfigurator.Parameters.ReachState;
            return new CaravanParameters(
                origin.targetBodies.First(),
                ReadSituation(origin.SituationList()),
                destination.targetBodies.First(),
                ReadSituation(destination.SituationList()));
        }

        private static Vessel ExtractVessel(ConfiguredContract contract)
        {
            var destination = contract.GetParameter("VesselParameterGroupCatch2") as ContractConfigurator.Parameters.VesselParameterGroup;
            return destination.TrackedVessel;
        }

        public static void MakeContractFor(CaravanParameters parameters)
        {
            SetupFactoriesFor(parameters);
            var contract = SpawnContract();
            AddAndAccept(contract);
        }

        public static void ContractCompleted(Contract contract)
        {
            var configuredContract = contract as ConfiguredContract;
            if (configuredContract == null)
                return;
            if (configuredContract.contractType.name != _contractTypeName)
                return;
            WolfCaravanScenario.AddCompletedRoute(ExtractParameters(configuredContract), ExtractVessel(configuredContract));
        }

        private static void SetupFactoriesFor(CaravanParameters p)
        {
            CaravanEndPointParameterFactory.originBody = p._fromBody;
            CaravanEndPointParameterFactory.originSituation = p._fromSituation;
            CaravanEndPointParameterFactory.destinationBody = p._toBody;
            CaravanEndPointParameterFactory.destinationSituation = p._toSituation;
        }

        private static ConfiguredContract SpawnContract()
        {
            ConfiguredContract contract = Contract.Generate(typeof(ConfiguredContract), Contract.ContractPrestige.Trivial, 0, Contract.State.Withdrawn) as ConfiguredContract;
            ContractType contractType = ContractType.GetContractType(_contractTypeName);
            contract.Initialize(contractType);
            return contract;
        }

        private static void AddAndAccept(ConfiguredContract contract)
        {
            ContractSystem.Instance.Contracts.Add(contract);
            contract.ContractState = Contract.State.Offered;
            contract.Accept();
        }

        private static CaravanParameters.Situation ReadSituation(string situationList)
        {
            if (situationList.ToLower().StartsWith("orbit"))
                return CaravanParameters.Situation.Orbit;
            else
                return CaravanParameters.Situation.Surface;
        }


    }
   
}
