﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2019 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.

using System.Collections.ObjectModel;

namespace SyncroSim.STSim
{
    internal class TransitionSpreadDistributionCollection : KeyedCollection<int, TransitionSpreadDistribution>
    {
        protected override int GetKeyForItem(TransitionSpreadDistribution item)
        {
            return item.TransitionSpreadDistributionId;
        }
    }
}
