﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System.Collections.ObjectModel;

namespace SyncroSim.STSim
{
    /// <summary>
    /// Collection of simulation cells keyed by CellID
    /// </summary>
    public class CellCollection : KeyedCollection<int, Cell>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Prevent public constructor
        /// </remarks>
        public CellCollection()
        {
            return;
        }

        /// <summary>
        /// Gets the key for the specified collection item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override int GetKeyForItem(Cell item)
        {
            return item.CellId;
        }
    }

}
