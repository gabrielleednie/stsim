﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2019 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using SyncroSim.Core;
using System.Diagnostics;
using System.Collections.Generic;

namespace SyncroSim.STSim
{
    internal class TransitionAttributeValueMap : STSimMapBase6<List<AttributeValueRecord>>
    {
        private STSimDistributionProvider m_DistributionProvider;

        private Dictionary<int, Dictionary<int, bool>> m_TypeGroupMap = 
            new Dictionary<int, Dictionary<int, bool>>();

        public TransitionAttributeValueMap(
            Scenario scenario, 
            STSimDistributionProvider provider,
            TransitionAttributeValueCollection transitionAttributes) : base(scenario)
        {
            this.m_DistributionProvider = provider;

            foreach (TransitionAttributeValue ta in transitionAttributes)
            {
                this.AddAttributeValue(ta);
            }
        }

        public Dictionary<int, Dictionary<int, bool>> TypeGroupMap
        {
            get
            {
                return this.m_TypeGroupMap;
            }
        }

        public double? GetAttributeValue(
            int transitionAttributeTypeId, int transitionGroupId, 
            int stratumId, int? secondaryStratumId, int? tertiaryStratumId, 
            int stateClassId, int iteration, int timestep, int age)
        {
            List<AttributeValueRecord> cm = this.GetItem(
                transitionAttributeTypeId, transitionGroupId, 
                stratumId, secondaryStratumId, tertiaryStratumId, 
                stateClassId, iteration, timestep);

            if (cm != null)
            {
                return AttributeValueRecord.GetAttributeRecordValue(
                    cm, iteration, timestep, this.m_DistributionProvider, age);
            }
            else
            {
                return null;
            }
        }

        private void AddAttributeValue(TransitionAttributeValue item)
        {
            List<AttributeValueRecord> l = this.GetItemExact(
                item.TransitionAttributeTypeId, item.TransitionGroupId, 
                item.StratumId, item.SecondaryStratumId, item.TertiaryStratumId, 
                item.StateClassId, item.Iteration, item.Timestep);

            if (l == null)
            {
                l = new List<AttributeValueRecord>();

                this.AddItem(
                    item.TransitionAttributeTypeId, item.TransitionGroupId, 
                    item.StratumId, item.SecondaryStratumId, item.TertiaryStratumId, 
                    item.StateClassId, item.Iteration, item.Timestep, l);
            }

            AttributeValueRecord.AddAttributeRecord(l, item.MinimumAge, item.MaximumAge, item);

            if (!this.m_TypeGroupMap.ContainsKey(item.TransitionGroupId))
            {
                this.m_TypeGroupMap.Add(item.TransitionGroupId, new Dictionary<int, bool>());
            }

            Dictionary<int, bool> d = TypeGroupMap[item.TransitionGroupId];

            if (!d.ContainsKey(item.TransitionAttributeTypeId))
            {
                d.Add(item.TransitionAttributeTypeId, true);
            }

            Debug.Assert(this.HasItems);
        }
    }
}
