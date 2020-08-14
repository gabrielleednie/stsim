﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2019 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using SyncroSim.Core;
using System.Diagnostics;
using System.Collections.Generic;

namespace SyncroSim.STSim
{
    internal class StateAttributeValueMap : STSimMapBase5<List<AttributeValueRecord>>
    {
        private STSimDistributionProvider m_DistributionProvider;

        internal StateAttributeValueMap(
            Scenario scenario, 
            STSimDistributionProvider provider,
            StateAttributeValueCollection items) : base(scenario)
        {
            this.m_DistributionProvider = provider;

            foreach (StateAttributeValue item in items)
            {
                this.AddAttributeValue(item);
            }
        }

        public double? GetAttributeValue(
            int stateAttributeTypeId, int stratumId, int? secondaryStratumId, int? tertiaryStratumId, 
            int stateClassId, int iteration, int timestep, int age)
        {
            List<AttributeValueRecord> cm = this.GetItem(
                stateAttributeTypeId, stratumId, secondaryStratumId, tertiaryStratumId, 
                stateClassId, iteration, timestep);

            if (cm != null)
            {
                return AttributeValueRecord.GetAttributeRecordValue(cm,
                    iteration, timestep, this.m_DistributionProvider, age);
            }
            else
            {
                return null;
            }
        }

        private void AddAttributeValue(StateAttributeValue item)
        {
            List<AttributeValueRecord> l = this.GetItemExact(
                item.StateAttributeTypeId, item.StratumId, item.SecondaryStratumId, item.TertiaryStratumId,
                item.StateClassId, item.Iteration, item.Timestep);

            if (l == null)
            {
                l = new List<AttributeValueRecord>();

                this.AddItem(
                    item.StateAttributeTypeId, item.StratumId, item.SecondaryStratumId, item.TertiaryStratumId, 
                    item.StateClassId, item.Iteration, item.Timestep, l);
            }

            AttributeValueRecord.AddAttributeRecord(l, item.MinimumAge, item.MaximumAge, item);
            Debug.Assert(this.HasItems);
        }
    }
}
