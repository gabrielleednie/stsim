﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using SyncroSim.Core;
using System.Collections.Generic;

namespace SyncroSim.STSim
{
    internal class TransitionTargetPrioritizationItemMap : STSimMapBase4<TransitionTargetPrioritization>
    {
        public TransitionTargetPrioritizationItemMap(
            List<TransitionTargetPrioritization> collection,
            Scenario scenario) : base(scenario)
        {
            foreach (TransitionTargetPrioritization item in collection)
            {
                this.AddItem(
                    item.StratumId,
                    item.SecondaryStratumId,
                    item.TertiaryStratumId,
                    item.StateClassId,
                    item.Iteration,
                    item.Timestep,
                    item);
            }
        }

        public TransitionTargetPrioritization GetPrioritization(
            int? stratumId,
            int? secondaryStratumId,
            int? tertiaryStratumId,
            int? stateClassId,
            int? iteration,
            int? timestep)
        {
            return this.GetItem(
                stratumId,
                secondaryStratumId,
                tertiaryStratumId,
                stateClassId,
                iteration,
                timestep);
        }
    }
}
