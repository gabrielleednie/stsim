﻿// A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2018 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.

using System.Collections.ObjectModel;

namespace SyncroSim.STSim
{
    internal class TransitionPatchPrioritizationCollection : KeyedCollection<int, TransitionPatchPrioritization>
    {
        protected override int GetKeyForItem(TransitionPatchPrioritization item)
        {
            return item.TransitionPatchPrioritizationId;
        }
    }
}
