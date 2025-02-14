﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using SyncroSim.StochasticTime;
using System.Collections.Generic;
using SyncroSim.Core;

namespace SyncroSim.STSim
{
    public partial class STSimTransformer
    {
        private CellCollection m_Cells = new CellCollection();
        private StratumCollection m_Strata = new StratumCollection();
        private StratumCollection m_SecondaryStrata = new StratumCollection();
        private StratumCollection m_TertiaryStrata = new StratumCollection();
        private StateClassCollection m_StateClasses = new StateClassCollection();
        private TransitionTypeCollection m_TransitionTypes = new TransitionTypeCollection();
        private TransitionGroupCollection m_TransitionGroups = new TransitionGroupCollection();
        private TransitionGroupCollection m_PrimaryTransitionGroups = new TransitionGroupCollection();
        private TransitionGroupCollection m_TransitionSimulationGroups = new TransitionGroupCollection();
        private List<TransitionGroup> m_ShufflableTransitionGroups = new List<TransitionGroup>();
        private List<TransitionGroup> m_TransitionSpreadGroups = new List<TransitionGroup>();
        private StateAttributeTypeCollection m_StateAttributeTypes = new StateAttributeTypeCollection();
        private TransitionAttributeTypeCollection m_TransitionAttributeTypes = new TransitionAttributeTypeCollection();
        private TransitionMultiplierTypeCollection m_TransitionMultiplierTypes = new TransitionMultiplierTypeCollection();
        private PatchPrioritizationCollection m_PatchPrioritizations = new PatchPrioritizationCollection();
        private InitialConditionsDistributionCollection m_InitialConditionsDistributions = new InitialConditionsDistributionCollection();
        private InitialConditionsSpatialCollection m_InitialConditionsSpatialValues = new InitialConditionsSpatialCollection();
        private TransitionCollection m_Transitions = new TransitionCollection();
        private DeterministicTransitionCollection m_DeterministicTransitions = new DeterministicTransitionCollection();
        private TransitionMultiplierValueCollection m_TransitionMultiplierValues = new TransitionMultiplierValueCollection();
        private TransitionSpatialMultiplierCollection m_TransitionSpatialMultipliers = new TransitionSpatialMultiplierCollection();
        private Dictionary<string, StochasticTimeRaster> m_TransitionSpatialMultiplierRasters = new Dictionary<string, StochasticTimeRaster>();
        private TransitionSpatialInitiationMultiplierCollection m_TransitionSpatialInitiationMultipliers = new TransitionSpatialInitiationMultiplierCollection();
        private Dictionary<string, StochasticTimeRaster> m_TransitionSpatialInitiationMultiplierRasters = new Dictionary<string, StochasticTimeRaster>();
        private TransitionTargetCollection m_TransitionTargets = new TransitionTargetCollection();
        private TransitionTargetPrioritizationCollection m_TransitionTargetPrioritizations = new TransitionTargetPrioritizationCollection();
        private TransitionAttributeTargetCollection m_TransitionAttributeTargets = new TransitionAttributeTargetCollection();
        private TransitionAttributeTargetPrioritizationCollection m_TransitionAttributeTargetPrioritizations = new TransitionAttributeTargetPrioritizationCollection();
        private TransitionOrderCollection m_TransitionOrders = new TransitionOrderCollection();
        private TransitionSizeDistributionCollection m_TransitionSizeDistributions = new TransitionSizeDistributionCollection();
        private TransitionSpreadDistributionCollection m_TransitionSpreadDistributions = new TransitionSpreadDistributionCollection();
        private TransitionPatchPrioritizationCollection m_TransitionPatchPrioritizations = new TransitionPatchPrioritizationCollection();
        private TransitionSizePrioritizationCollection m_TransitionSizePrioritizations = new TransitionSizePrioritizationCollection();
        private TransitionDirectionMultiplierCollection m_TransitionDirectionMultipliers = new TransitionDirectionMultiplierCollection();
        private TransitionSlopeMultiplierCollection m_TransitionSlopeMultipliers = new TransitionSlopeMultiplierCollection();
        private TransitionAdjacencySettingCollection m_TransitionAdjacencySettings = new TransitionAdjacencySettingCollection();
        private TransitionAdjacencyMultiplierCollection m_TransitionAdjacencyMultipliers = new TransitionAdjacencyMultiplierCollection();
        private TransitionPathwayAutoCorrelationCollection m_TransitionPathwayAutoCorrelations = new TransitionPathwayAutoCorrelationCollection();
        private StateAttributeValueCollection m_StateAttributeValues = new StateAttributeValueCollection();
        private TransitionAttributeValueCollection m_TransitionAttributeValues = new TransitionAttributeValueCollection();
        private InitialTSTSpatialCollection m_InitialTSTSpatialRecords = new InitialTSTSpatialCollection();
        private TSTTransitionGroupCollection m_TSTTransitionGroups = new TSTTransitionGroupCollection();
        private TSTRandomizeCollection m_TSTRandomizeRecords = new TSTRandomizeCollection();
        private InputRasters m_InputRasters = new InputRasters();
        private Dictionary<int, bool> m_TransitionAttributeTypesWithTarget = new Dictionary<int, bool>();
        private OutputFilterTransitionGroupCollection m_OutputFilterTransitionGroups = new OutputFilterTransitionGroupCollection();
        private OutputFilterAttributeCollection m_OutputFilterStateAttributes = new OutputFilterAttributeCollection();
        private OutputFilterAttributeCollection m_OutputFilterTransitionAttributes = new OutputFilterAttributeCollection();

#if DEBUG
        private bool TRANSITION_TYPES_FILLED;
        private bool TRANSITION_GROUPS_FILLED;
        private bool TRANSITION_SIM_GROUPS_FILLED;
        private bool STATE_ATTRIBUTE_TYPES_FILLED;
        private bool TRANSITION_ATTRIBUTE_TYPES_FILLED;
        private bool IC_DISTRIBUTIONS_FILLED;
        private bool TRANSITION_MULTIPLIERS_FILLED;
        private bool INITIAL_TST_SPATIAL_FILLED;
        private bool STATE_ATTRIBUTES_FILLED;
        private bool TRANSITION_ATTRIBUTES_FILLED;
        private bool TST_TRANSITION_GROUPS_FILLED;
        private bool TST_RANDOMIZE_FILLED;
#endif

        /// <summary>
        /// Fills the cell collection
        /// </summary>
        /// <param name="numCells">The number of cells to create</param>
        /// <remarks>
        /// Note that the cell ID and the collection index are not necessarily the same
        /// since for spatial runs a NO DATA cell is not created.
        /// </remarks>
        private void FillCellCollection(int numCells)
        {
            Debug.Assert(numCells > 0);

            int CollectionIndex = 0;

            for (int CellId = 0; CellId < numCells; CellId++)
            {
                if (this.IsSpatial)
                {
                    //Only create a Cell in the Collection if Stratum or StateClass <>0, to conserve memory.
                    if (this.m_InputRasters.SClassCells[CellId] == 0 || this.m_InputRasters.PrimaryStratumCells[CellId] == 0)
                    {
                        continue;
                    }
                }

                Cell SimulationCell = new Cell(CellId, CollectionIndex);
                this.m_Cells.Add(SimulationCell);

                CollectionIndex++;
            }
        }

        /// <summary>
        /// Fills the Cell Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillCellCollection()
        {
            Debug.Assert(this.m_Cells.Count == 0);

            int NumCells = 0;

            if (this.IsSpatial)
            {
                NumCells = this.m_InputRasters.NumberCells;
            }
            else
            {
                DataRow dr = this.ResultScenario.GetDataSheet(Strings.DATASHEET_NSIC_NAME).GetDataRow();
                NumCells = Convert.ToInt32(dr[Strings.DATASHEET_NSIC_NUM_CELLS_COLUMN_NAME], CultureInfo.InvariantCulture);
            }

            this.FillCellCollection(NumCells);
        }

        /// <summary>
        /// Fills the model's stratum collection
        /// </summary>
        /// <remarks></remarks>
        private void FillStratumCollection()
        {
            Debug.Assert(this.m_Strata.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_STRATA_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int id = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                string name = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture);
                this.m_Strata.Add(new Stratum(id, name));
            }
        }

        /// <summary>
        /// Fills the model's secondary stratum collection
        /// </summary>
        /// <remarks></remarks>
        private void FillSecondaryStratumCollection()
        {
            Debug.Assert(this.m_SecondaryStrata.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_SECONDARY_STRATA_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int id = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                string name = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture);
                this.m_SecondaryStrata.Add(new Stratum(id, name));
            }
        }

        /// <summary>
        /// Fills the model's tertiary stratum collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTertiaryStratumCollection()
        {
            Debug.Assert(this.m_TertiaryStrata.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TERTIARY_STRATA_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int id = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                string name = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture);
                this.m_TertiaryStrata.Add(new Stratum(id, name));
            }
        }

        /// <summary>
        /// Fills the model's state class collection
        /// </summary>
        /// <remarks></remarks>
        private void FillStateClassCollection()
        {
            Debug.Assert(this.m_StateClasses.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_STATECLASS_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int id = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int slxid = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_STATE_LABEL_X_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int slyid = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_STATE_LABEL_Y_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                string name = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture);

                this.m_StateClasses.Add(new StateClass(id, slxid, slyid, name));
            }
        }

        /// <summary>
        /// Fills the model's transition type collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionTypeCollection()
        {
            Debug.Assert(this.m_TransitionTypes.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_TYPE_NAME);

            bool AtLeastOne = false;

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionTypeId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                string DisplayName = Convert.ToString(dr[ds.DisplayMember], CultureInfo.InvariantCulture);
                int? MapId = null;

                if (dr[Strings.DATASHEET_MAPID_COLUMN_NAME] != DBNull.Value)
                {
                    MapId = Convert.ToInt32(dr[Strings.DATASHEET_MAPID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (MapId.HasValue)
                {
                    AtLeastOne = true;
                }

                this.m_TransitionTypes.Add(new TransitionType(TransitionTypeId, DisplayName, MapId));
            }

            if (this.m_IsSpatial && (!AtLeastOne) && this.m_CreateRasterTransitionOutput)
            {
                this.RecordStatus(StatusType.Warning, "Spatial transition type output requested but no IDs specified for Transition Types.");
            }

#if DEBUG
            TRANSITION_TYPES_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the model's transition group collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionGroupCollection()
        {
            Debug.Assert(this.m_TransitionGroups.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_GROUP_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                this.m_TransitionGroups.Add(
                    new TransitionGroup(
                        Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture), 
                        Convert.ToString(dr["NAME"], CultureInfo.InvariantCulture), 
                        DataTableUtilities.GetDataBool(dr, Strings.IS_AUTO_COLUMN_NAME)));
            }

#if DEBUG
            TRANSITION_GROUPS_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the transition simulation group collection
        /// </summary>
        private void FillTransitionSimulationGroupCollection()
        {
            Debug.Assert(this.m_TransitionSimulationGroups.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_SIMULATION_GROUP_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int tgid = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                TransitionGroup tg = this.m_TransitionGroups[tgid];

                Debug.Assert(!this.m_TransitionSimulationGroups.Contains(tg));

                if (!this.m_TransitionSimulationGroups.Contains(tg))
                {
                    this.m_TransitionSimulationGroups.Add(tg);
                }
            }

#if DEBUG
            TRANSITION_SIM_GROUPS_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the groups for each transition type
        /// </summary>
        /// <remarks>
        /// This function must not be called before the transition group, simulation group, 
        /// and transition type collections have been filled
        /// </remarks>
        private void FillGroupsForTransitionTypes()
        {
#if DEBUG
            Debug.Assert(TRANSITION_TYPES_FILLED);
            Debug.Assert(TRANSITION_GROUPS_FILLED);
            Debug.Assert(TRANSITION_SIM_GROUPS_FILLED);
#endif

            DataSheet DSTypeGroup = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_TYPE_GROUP_NAME);

            //For every transition type...

            foreach (TransitionType TType in this.m_TransitionTypes)
            {
                Debug.Assert(TType.TransitionGroups.Count == 0);

                //Find the groups it belongs to...

                string query = string.Format(CultureInfo.InvariantCulture, 
                    "{0}={1}", Strings.DATASHEET_TRANSITION_TYPE_ID_COLUMN_NAME, TType.TransitionTypeId);

                DataRow[] TGroupRows = DSTypeGroup.GetData().Select(query);
                bool TypeHasNonAutoPrimaryGroup = false;

                //When adding the primary transition groups, we only want to add an auto-generated
                //group if there is not already a user-defined one. To ensure this, we loop over the 
                //groups twice with the user-defined ones taking precedcence.

                //User Defined

                foreach (DataRow TGroupRow in TGroupRows)
                {
                    int tgid = Convert.ToInt32(TGroupRow[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                    TransitionGroup tg = this.m_TransitionGroups[tgid];

                    if (!tg.IsAuto) 
                    {
                        Debug.Assert(!TType.TransitionGroups.Contains(tgid));
                        TType.TransitionGroups.Add(tg);

                        if (this.m_TransitionSimulationGroups.Contains(tg))  //If it's primary
                        {
                            TType.PrimaryTransitionGroups.Add(tg);                        
                            TypeHasNonAutoPrimaryGroup = true;

                            if (!this.m_PrimaryTransitionGroups.Contains(tg))  //Global primary group list
                            {
                                this.m_PrimaryTransitionGroups.Add(tg);
                            }
                        }
                    }
                }

                //Auto Generated
               
                foreach (DataRow TGroupRow in TGroupRows)
                {
                    int tgid = Convert.ToInt32(TGroupRow[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                    TransitionGroup tg = this.m_TransitionGroups[tgid];

                    if (tg.IsAuto)
                    {
                        Debug.Assert(!TType.TransitionGroups.Contains(tgid));

                        TType.TransitionGroups.Add(tg);

                        if (!TypeHasNonAutoPrimaryGroup)
                        {
                            TType.PrimaryTransitionGroups.Add(tg);

                            if (!this.m_PrimaryTransitionGroups.Contains(tg))  //Global primary group list
                            {
                                this.m_PrimaryTransitionGroups.Add(tg);
                            }
                        }
                    }
                }
                               
                if (TType.PrimaryTransitionGroups.Count > 1)
                {
                    string msg = string.Format(CultureInfo.InvariantCulture,
                        "The transition type '{0}' has more than one transition simulation group.", TType.DisplayName);

                    this.RecordStatus(StatusType.Warning, msg);
                }

                Debug.Assert(TType.PrimaryTransitionGroups.Count > 0);
            }
        }

        /// <summary>
        /// Fills the types for each transition group
        /// </summary>
        /// <remarks>
        /// This function must not be called before the transition group, simulation group, 
        /// and transition type collections have been filled
        /// </remarks>
        private void FillTypesForTransitionGroups()
        {
#if DEBUG
            Debug.Assert(TRANSITION_TYPES_FILLED);
            Debug.Assert(TRANSITION_GROUPS_FILLED);
            Debug.Assert(TRANSITION_SIM_GROUPS_FILLED);
#endif

            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_TYPE_GROUP_NAME);

            //We only want to add a primary type to an auto-generated group if that type is not already
            //a primary type in a user-defined group.  To ensure this, we loop over the groups
            //twice with the user-defined ones taking precedcence.

            Dictionary<int, bool> PrimaryTypesAdded = new Dictionary<int, bool>();

            //User defined

            foreach (TransitionGroup TGroup in this.m_TransitionGroups)
            {
                if (!TGroup.IsAuto) 
                {
                    Debug.Assert(TGroup.TransitionTypes.Count == 0); 
                                   
                    string query = string.Format(CultureInfo.InvariantCulture, 
                        "{0}={1}", Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME, TGroup.TransitionGroupId);

                    DataRow[] TTypeRows = ds.GetData().Select(query);
                    bool IsSimulationGroup = this.m_TransitionSimulationGroups.Contains(TGroup);

                    foreach (DataRow TTypeRow in TTypeRows)
                    {
                        int ttid = Convert.ToInt32(TTypeRow[Strings.DATASHEET_TRANSITION_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                        TransitionType TType = this.m_TransitionTypes[ttid];

                        Debug.Assert(!TGroup.TransitionTypes.Contains(ttid));
                        Debug.Assert(!TGroup.PrimaryTransitionTypes.Contains(ttid));

                        TGroup.TransitionTypes.Add(TType);

                        if (IsSimulationGroup)
                        {
                            TGroup.PrimaryTransitionTypes.Add(TType);

                            if (!PrimaryTypesAdded.ContainsKey(TType.TransitionTypeId))
                            {
                                PrimaryTypesAdded.Add(TType.TransitionTypeId, true);
                            }                           
                        }
                    }
                }
            }

            //Auto Generated

            foreach (TransitionGroup TGroup in this.m_TransitionGroups)
            {
                if (TGroup.IsAuto) 
                {
                    Debug.Assert(TGroup.TransitionTypes.Count == 0);

                    string query = string.Format(CultureInfo.InvariantCulture, 
                        "{0}={1}", Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME, TGroup.TransitionGroupId);

                    DataRow[] TTypeRows = ds.GetData().Select(query);

                    foreach (DataRow TTypeRow in TTypeRows)
                    {
                        int ttid = Convert.ToInt32(TTypeRow[Strings.DATASHEET_TRANSITION_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                        TransitionType TType = this.m_TransitionTypes[ttid];

                        Debug.Assert(!TGroup.TransitionTypes.Contains(ttid));
                        Debug.Assert(!TGroup.PrimaryTransitionTypes.Contains(ttid));

                        TGroup.TransitionTypes.Add(TType);

                        if (!PrimaryTypesAdded.ContainsKey(TType.TransitionTypeId))
                        {
                            TGroup.PrimaryTransitionTypes.Add(TType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fills the Transition Multiplier Type Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionMultiplierTypeCollection()
        {
            Debug.Assert(this.m_TransitionMultiplierTypes.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_NAME);

            //Always add type with a Null Id because transition multipliers can have null types.
            this.m_TransitionMultiplierTypes.Add(new TransitionMultiplierType(null, this.ResultScenario, this.m_DistributionProvider));

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionMultiplierTypeId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);

                this.m_TransitionMultiplierTypes.Add(new TransitionMultiplierType
                    (TransitionMultiplierTypeId, this.ResultScenario, this.m_DistributionProvider));
            }
        }

        /// <summary>
        /// Fills the State Attribute Type collection
        /// </summary>
        /// <remarks></remarks>
        private void FillStateAttributeTypeCollection()
        {
            Debug.Assert(this.m_StateAttributeTypes.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_STATE_ATTRIBUTE_TYPE_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int StateAttributeTypeId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                this.m_StateAttributeTypes.Add(new StateAttributeType(StateAttributeTypeId));
            }

#if DEBUG
            STATE_ATTRIBUTE_TYPES_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the Transition Attribute Type collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionAttributeTypeCollection()
        {
            Debug.Assert(this.m_TransitionAttributeTypes.Count == 0);
            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_ATTRIBUTE_TYPE_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionAttributeTypeId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                this.m_TransitionAttributeTypes.Add(new TransitionAttributeType(TransitionAttributeTypeId));
            }

#if DEBUG
            TRANSITION_ATTRIBUTE_TYPES_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the Patch Prioritization Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillPatchPrioritizationCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_PatchPrioritizations.Count == 0);

            DataSheet ds = this.Project.GetDataSheet(Strings.DATASHEET_PATCH_PRIORITIZATION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int Id = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                string Name = Convert.ToString(dr[ds.DisplayMember], CultureInfo.InvariantCulture);

                PatchPrioritizationType t = 0;

                if (Name == Strings.PATCH_PRIORITIZATION_SMALLEST)
                {
                    t = PatchPrioritizationType.Smallest;
                }
                else if (Name == Strings.PATCH_PRIORITIZATION_SMALLEST_EDGES_ONLY)
                {
                    t = PatchPrioritizationType.SmallestEdgesOnly;
                }
                else if (Name == Strings.PATCH_PRIORITIZATION_LARGEST)
                {
                    t = PatchPrioritizationType.Largest;
                }
                else if (Name == Strings.PATCH_PRIORITIZATION_LARGEST_EDGES_ONLY)
                {
                    t = PatchPrioritizationType.LargestEdgesOnly;
                }
                else
                {
                    ExceptionUtils.ThrowInvalidOperationException("The patch prioritization '{0}' is not valid", Name);
                }

                PatchPrioritization pp = new PatchPrioritization(Id, t);
                this.m_PatchPrioritizations.Add(pp);
            }
        }

        /// <summary>
        /// Fills the initial conditions distribution collection and creates the map
        /// </summary>
        /// <remarks></remarks>
        private void FillInitialConditionsDistributionCollectionAndMap()
        {
            this.m_InitialConditionsDistributions.Clear();

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_NSIC_DISTRIBUTION_NAME);

            if (ds.GetData().Rows.Count == 0)
            {
                throw new ArgumentException(MessageStrings.ERROR_NO_INITIAL_CONDITIONS_DISTRIBUTION_RECORDS);
            }

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int AgeMin = 0;
                int AgeMax = int.MaxValue;
                int? TSTGroupId = null;
                int? TSTMin = null;
                int? TSTMax = null;
                double RelativeAmount = Convert.ToDouble(dr[Strings.DATASHEET_NSIC_DISTRIBUTION_RELATIVE_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMin = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMax = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TSTGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TST_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMin = Convert.ToInt32(dr[Strings.DATASHEET_TST_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMax = Convert.ToInt32(dr[Strings.DATASHEET_TST_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                InitialConditionsDistribution InitialStateRecord = new InitialConditionsDistribution(
                    StratumId, 
                    Iteration, 
                    SecondaryStratumId, 
                    TertiaryStratumId, 
                    StateClassId, 
                    AgeMin, 
                    AgeMax, 
                    TSTGroupId,
                    TSTMin,
                    TSTMax,
                    RelativeAmount);

                this.m_InitialConditionsDistributions.Add(InitialStateRecord);
            }

            this.m_InitialConditionsDistributionMap = 
                new InitialConditionsDistributionMap(this.m_InitialConditionsDistributions);

#if DEBUG
            this.IC_DISTRIBUTIONS_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the initial conditions spatial collection and creates the map
        /// </summary>
        /// <remarks></remarks>
        private void FillInitialConditionsSpatialCollectionAndMap()
        {
            this.m_InitialConditionsSpatialValues.Clear();

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_SPIC_NAME);

            if (ds.GetData().Rows.Count == 0)
            {
                throw new ArgumentException(MessageStrings.ERROR_NO_INITIAL_CONDITIONS_SPATIAL_RECORDS);
            }

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                string PrimaryStratumName = null;
                string SecondaryStratumName = null;
                string TertiaryStratumName = null;
                string StateClassName = null;
                string AgeName = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                PrimaryStratumName = dr[Strings.DATASHEET_SPIC_STRATUM_FILE_COLUMN_NAME].ToString();
                SecondaryStratumName = dr[Strings.DATASHEET_SPIC_SECONDARY_STRATUM_FILE_COLUMN_NAME].ToString();
                TertiaryStratumName = dr[Strings.DATASHEET_SPIC_TERTIARY_STRATUM_FILE_COLUMN_NAME].ToString();
                StateClassName = dr[Strings.DATASHEET_SPIC_STATE_CLASS_FILE_COLUMN_NAME].ToString();
                AgeName = dr[Strings.DATASHEET_SPIC_AGE_FILE_COLUMN_NAME].ToString();

                InitialConditionsSpatial InitialStateRecord = new InitialConditionsSpatial(
                    Iteration, PrimaryStratumName, SecondaryStratumName, TertiaryStratumName, StateClassName, AgeName);

                this.m_InitialConditionsSpatialValues.Add(InitialStateRecord);
            }

            this.m_InitialConditionsSpatialMap = new InitialConditionsSpatialMap(this.m_InitialConditionsSpatialValues);
        }

        /// <summary>
        /// Fills the Deterministic Transitions Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillDeterministicTransitionsCollection()
        {
            Debug.Assert(this.m_DeterministicTransitions.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_DT_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                int? Timestep = null;
                int? StratumIdSource = null;
                int StateClassIdSource = Convert.ToInt32(dr[Strings.DATASHEET_DT_STATECLASSIDSOURCE_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StratumIdDest = null;
                int? StateClassIdDest = StateClassIdSource;
                int AgeMin = 0;
                int AgeMax = int.MaxValue;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DT_STRATUMIDSOURCE_COLUMN_NAME] != DBNull.Value)
                {
                    StratumIdSource = Convert.ToInt32(dr[Strings.DATASHEET_DT_STRATUMIDSOURCE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DT_STRATUMIDDEST_COLUMN_NAME] != DBNull.Value)
                {
                    StratumIdDest = Convert.ToInt32(dr[Strings.DATASHEET_DT_STRATUMIDDEST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DT_STATECLASSIDDEST_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassIdDest = Convert.ToInt32(dr[Strings.DATASHEET_DT_STATECLASSIDDEST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMin = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMax = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                DeterministicTransition dt = new DeterministicTransition(
                    Iteration, Timestep, StratumIdSource, StateClassIdSource, StratumIdDest, 
                    StateClassIdDest, AgeMin, AgeMax);

                this.m_DeterministicTransitions.Add(dt);
            }
        }

        /// <summary>
        /// Fills the Probabilistic Transitions Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillProbabilisticTransitionsCollection()
        {
            Debug.Assert(this.m_Transitions.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_PT_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                int? Timestep = null;
                int? StratumIdSource = null;
                int StateClassIdSource = Convert.ToInt32(dr[Strings.DATASHEET_PT_STATECLASSIDSOURCE_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StratumIdDest = null;
                int? StateClassIdDest = StateClassIdSource;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int TransitionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                double Probability = Convert.ToDouble(dr[Strings.DATASHEET_PT_PROBABILITY_COLUMN_NAME], CultureInfo.InvariantCulture);
                double Proportion = 1.0;
                int AgeMinimum = 0;
                int AgeMaximum = int.MaxValue;
                int AgeRelative = 0;
                bool AgeReset = true;
                int TstMinimum = 0;
                int TstMaximum = int.MaxValue;
                int TstRelative = (-int.MaxValue);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_STRATUMIDSOURCE_COLUMN_NAME] != DBNull.Value)
                {
                    StratumIdSource = Convert.ToInt32(dr[Strings.DATASHEET_PT_STRATUMIDSOURCE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_STRATUMIDDEST_COLUMN_NAME] != DBNull.Value)
                {
                    StratumIdDest = Convert.ToInt32(dr[Strings.DATASHEET_PT_STRATUMIDDEST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_STATECLASSIDDEST_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassIdDest = Convert.ToInt32(dr[Strings.DATASHEET_PT_STATECLASSIDDEST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_PROPORTION_COLUMN_NAME] != DBNull.Value)
                {
                    Proportion = Convert.ToDouble(dr[Strings.DATASHEET_PT_PROPORTION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMinimum = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMaximum = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_AGE_RELATIVE_COLUMN_NAME] != DBNull.Value)
                {
                    AgeRelative = Convert.ToInt32(dr[Strings.DATASHEET_PT_AGE_RELATIVE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_AGE_RESET_COLUMN_NAME] != DBNull.Value)
                {
                    AgeReset = Convert.ToBoolean(dr[Strings.DATASHEET_PT_AGE_RESET_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_TST_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    TstMinimum = Convert.ToInt32(dr[Strings.DATASHEET_PT_TST_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_TST_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    TstMaximum = Convert.ToInt32(dr[Strings.DATASHEET_PT_TST_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_PT_TST_RELATIVE_COLUMN_NAME] != DBNull.Value)
                {
                    TstRelative = Convert.ToInt32(dr[Strings.DATASHEET_PT_TST_RELATIVE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                Transition pt = new Transition(
                    Iteration, Timestep, StratumIdSource, StateClassIdSource, StratumIdDest, 
                    StateClassIdDest, SecondaryStratumId, TertiaryStratumId, TransitionTypeId, Probability, Proportion,
                    AgeMinimum, AgeMaximum, AgeRelative, AgeReset, TstMinimum, TstMaximum, TstRelative);

                this.m_Transitions.Add(pt);
            }
        }

        /// <summary>
        /// Fills the output filter transition group collection
        /// </summary>
        private void FillOutputFilterTransitionGroupCollection()
        {
#if DEBUG
            Debug.Assert(TRANSITION_GROUPS_FILLED);
#endif
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                this.m_OutputFilterTransitionGroups.Add(
                    new OutputFilterTransitionGroup(
                        Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_SUMMARY_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_SUMMARY_BY_STATE_CLASS_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_TST_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_SPATIAL_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_SPATIAL_EVENTS_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_SPATIAL_TST_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_SPATIAL_PROB_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_GROUPS_AVG_SPATIAL_TST_COLUMN_NAME])));
            }

            foreach (TransitionGroup g in this.m_TransitionGroups)
            {
                OutputFilterFlagTransitionGroup f = OutputFilterFlagTransitionGroup.None;

                if (this.FilterIncludesSummaryForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.Summary;
                if (this.FilterIncludesSummaryByStateClassForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.SummaryByStateClass;
                if (this.FilterIncludesTSTForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.SummaryTST;
                if (this.FilterIncludesSpatialForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.Spatial;
                if (this.FilterIncludesSpatialEventsForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.SpatialEvents;
                if (this.FilterIncludesSpatialTSTForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.SpatialTST;
                if (this.FilterIncludesSpatialProbabilityForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.SpatialProbability;
                if (this.FilterIncludesAvgSpatialTSTForTG(g.TransitionGroupId)) f |= OutputFilterFlagTransitionGroup.AvgSpatialTST;

                g.OutputFilter = f;
            }

            foreach (TransitionType tt in this.m_TransitionTypes)
            {
                OutputFilterFlagTransitionGroup f = OutputFilterFlagTransitionGroup.None;

                if (this.FilterIncludesSummaryByStateClassForTT(tt.TransitionTypeId)) f |= OutputFilterFlagTransitionGroup.SummaryByStateClass;

                tt.OutputFilter = f;
            }
        }

        /// <summary>
        /// Fills the output filter state attribute collection
        /// </summary>
        private void FillOutputFilterStateAttributeCollection()
        {
#if DEBUG
            Debug.Assert(STATE_ATTRIBUTE_TYPES_FILLED);
#endif
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_OUTPUT_FILTER_STATE_ATTRIBUTES);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                this.m_OutputFilterStateAttributes.Add(
                    new OutputFilterAttribute(
                        Convert.ToInt32(dr[Strings.DATASHEET_STATE_ATTRIBUTE_TYPE_ID_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_STATE_ATTRIBUTES_SUMMARY_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_STATE_ATTRIBUTES_SPATIAL_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_STATE_ATTRIBUTES_AVG_SPATIAL_COLUMN_NAME])));
            }


            foreach (StateAttributeType t in this.m_StateAttributeTypes)
            {
                OutputFilterFlagAttribute f = OutputFilterFlagAttribute.None;

                if (this.FilterIncludesSummaryForSAT(t.Id)) f |= OutputFilterFlagAttribute.Summary;
                if (this.FilterIncludesSpatialForForSAT(t.Id)) f |= OutputFilterFlagAttribute.Spatial;
                if (this.FilterIncludesAvgSpatialForForSAT(t.Id)) f |= OutputFilterFlagAttribute.AvgSpatial;

                t.OutputFilter = f;
            }
        }

        /// <summary>
        /// Fills the output filter transition attribute collection
        /// </summary>
        private void FillOutputFilterTransitionAttributeCollection()
        {
#if DEBUG
            Debug.Assert(TRANSITION_ATTRIBUTE_TYPES_FILLED);
#endif
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_ATTRIBUTES);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                this.m_OutputFilterTransitionAttributes.Add(
                    new OutputFilterAttribute(
                        Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_ATTRIBUTE_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_ATTRIBUTES_SUMMARY_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_ATTRIBUTES_SPATIAL_COLUMN_NAME]),
                        Booleans.BoolFromValue(dr[Strings.DATASHEET_OUTPUT_FILTER_TRANSITION_ATTRIBUTES_AVG_SPATIAL_COLUMN_NAME])));
            }

            foreach (TransitionAttributeType t in this.m_TransitionAttributeTypes)
            {
                OutputFilterFlagAttribute f = OutputFilterFlagAttribute.None;

                if (this.FilterIncludesSummaryForTAT(t.TransitionAttributeId)) f |= OutputFilterFlagAttribute.Summary;
                if (this.FilterIncludesSpatialForTAT(t.TransitionAttributeId)) f |= OutputFilterFlagAttribute.Spatial;
                if (this.FilterIncludesAvgSpatialForTAT(t.TransitionAttributeId)) f |= OutputFilterFlagAttribute.AvgSpatial;

                t.OutputFilter = f;
            }
        }

        /// <summary>
        /// Fills the State Attribute Value Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillStateAttributeValueCollection()
        {
            Debug.Assert(this.m_StateAttributeValues.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_STATE_ATTRIBUTE_VALUE_NAME);
            bool StratumOrStateClassWarningIssued = false;

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int StateAttributeTypeId = Convert.ToInt32(dr[Strings.DATASHEET_STATE_ATTRIBUTE_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? Iteration = null;
                int? Timestep = null;
                int? StateClassId = null;
                int AgeMin = 0;
                int AgeMax = int.MaxValue;
                int? TSTGroupId = null;
                int? TSTMin = null;
                int? TSTMax = null;
                double? Value = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMin = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMax = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TSTGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TST_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMin = Convert.ToInt32(dr[Strings.DATASHEET_TST_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMax = Convert.ToInt32(dr[Strings.DATASHEET_TST_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATE_ATTRIBUTE_VALUE_VALUE_COLUMN_NAME] != DBNull.Value)
                {
                    Value = Convert.ToDouble(dr[Strings.DATASHEET_STATE_ATTRIBUTE_VALUE_VALUE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (!StratumId.HasValue && !StateClassId.HasValue)
                {
                    if (!StratumOrStateClassWarningIssued)
                    {
                        this.RecordStatus(StatusType.Information, "At least one State Attribute Value had neither a stratum nor a state class.");
                        StratumOrStateClassWarningIssued = true;
                    }
                }

                try
                {
                    StateAttributeValue Item = new StateAttributeValue(
                        StateAttributeTypeId,
                        StratumId,
                        SecondaryStratumId,
                        TertiaryStratumId,
                        Iteration,
                        Timestep,
                        StateClassId,
                        AgeMin,
                        AgeMax,
                        TSTGroupId,
                        TSTMin,
                        TSTMax,
                        Value,
                        DistributionTypeId,
                        DistributionFrequency,
                        DistributionSD,
                        DistributionMin,
                        DistributionMax);

                    Item.IsDisabled = (!Item.DistributionValue.HasValue && !Item.DistributionTypeId.HasValue);

                    if (Item.IsDisabled)
                    {
                        throw new ArgumentException("A State Attribute must have a Value or a Distribution Type.");
                    }
                    else
                    {
                        this.m_DistributionProvider.Validate(
                            Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);
                    }

                    this.m_StateAttributeValues.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }         
            }
#if DEBUG
            this.STATE_ATTRIBUTES_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the Transition Attribute Value Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionAttributeValueCollection()
        {
            Debug.Assert(this.m_TransitionAttributeValues.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_ATTRIBUTE_VALUE_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionAttributeTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_ATTRIBUTE_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? Iteration = null;
                int? Timestep = null;
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StateClassId = null;
                int AgeMin = 0;
                int AgeMax = int.MaxValue;
                int? TSTGroupId = null;
                int? TSTMin = null;
                int? TSTMax = null;
                double? Value = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMin = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMax = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TSTGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TST_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMin = Convert.ToInt32(dr[Strings.DATASHEET_TST_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMax = Convert.ToInt32(dr[Strings.DATASHEET_TST_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_ATTRIBUTE_VALUE_VALUE_COLUMN_NAME] != DBNull.Value)
                {
                    Value = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_ATTRIBUTE_VALUE_VALUE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionAttributeValue Item = new TransitionAttributeValue(
                        TransitionAttributeTypeId,
                        StratumId,
                        SecondaryStratumId,
                        TertiaryStratumId,
                        Iteration, Timestep,
                        TransitionGroupId,
                        StateClassId,
                        AgeMin,
                        AgeMax,
                        TSTGroupId,
                        TSTMin,
                        TSTMax,
                        Value,
                        DistributionTypeId,
                        DistributionFrequency,
                        DistributionSD,
                        DistributionMin,
                        DistributionMax);

                    Item.IsDisabled = (!Item.DistributionValue.HasValue && !Item.DistributionTypeId.HasValue);

                    if (Item.IsDisabled)
                    {
                        throw new ArgumentException("A Transition Attribute must have a Value or a Distribution Type.");
                    }
                    else
                    {
                        this.m_DistributionProvider.Validate(
                            Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);
                    }

                    this.m_TransitionAttributeValues.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }              
            }

#if DEBUG
            this.TRANSITION_ATTRIBUTES_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the TST Transition Group collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillTstTransitionGroupCollection()
        {
            Debug.Assert(this.m_TstTransitionGroupMap == null);

            this.m_TstTransitionGroupMap = new TstTransitionGroupMap(this.ResultScenario);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TST_GROUP_VALUE_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int TransitionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TstTransitionGroup Item = new TstTransitionGroup(TransitionGroupId);

                this.m_TSTTransitionGroups.Add(Item);

                this.m_TstTransitionGroupMap.AddGroup(
                    TransitionTypeId, StratumId, SecondaryStratumId, TertiaryStratumId, Item);
            }

#if DEBUG
            this.TST_TRANSITION_GROUPS_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the Randomize TST collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillTstRandomizeCollection()
        {
            Debug.Assert(this.m_TstRandomizeMap == null);

            this.m_TstRandomizeMap = new TstRandomizeMap(this.ResultScenario);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TST_RANDOMIZE_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? TransitionGroupId = null;
                int? StateClassId = null;
                int? Iteration = null;
                int MinInitialTST = 0;
                int MaxInitialTST = 0;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TST_RANDOMIZE_MIN_INITIAL_TST_COLUMN_NAME] != DBNull.Value)
                {
                    MinInitialTST = Convert.ToInt32(dr[Strings.DATASHEET_TST_RANDOMIZE_MIN_INITIAL_TST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }
                else
                {
                    MinInitialTST = 0;
                }

                if (dr[Strings.DATASHEET_TST_RANDOMIZE_MAX_INITIAL_TST_COLUMN_NAME] != DBNull.Value)
                {
                    MaxInitialTST = Convert.ToInt32(dr[Strings.DATASHEET_TST_RANDOMIZE_MAX_INITIAL_TST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }
                else
                {
                    MaxInitialTST = int.MaxValue;
                }

                TstRandomize Item = new TstRandomize(MinInitialTST, MaxInitialTST, TransitionGroupId);

                this.m_TSTRandomizeRecords.Add(Item);

                this.m_TstRandomizeMap.AddTstRandomize(
                    TransitionGroupId, StratumId, SecondaryStratumId, 
                    TertiaryStratumId, StateClassId, Iteration, Item);
            }

#if DEBUG
            this.TST_RANDOMIZE_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the Initial TST Spatial Collection and map
        /// </summary>
        private void FillInitialTSTSpatialCollectionAndMap()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_InitialTSTSpatialRecords.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_INITIAL_TST_SPATIAL_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null; 
                int? TransitionGroupId = null;
         
                string FileName = Convert.ToString(
                    dr[Strings.DATASHEET_INITIAL_TST_SPATIAL_FILE_COLUMN_NAME], 
                    CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                InitialTSTSpatial Item = new InitialTSTSpatial(Iteration, TransitionGroupId, FileName);
                this.m_InitialTSTSpatialRecords.Add(Item);
            }

            Debug.Assert(this.m_InitialTstSpatialMap == null);

            this.m_InitialTstSpatialMap = new InitialTSTSpatialMap(
                this.m_InitialTSTSpatialRecords, this.ResultScenario, this.m_InputRasters);

#if DEBUG
            this.INITIAL_TST_SPATIAL_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the TransitionOrder collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionOrderCollection()
        {
            Debug.Assert(this.m_TransitionOrders.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_ORDER_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                double? Order = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_ORDER_ORDER_COLUMN_NAME] != DBNull.Value)
                {
                    Order = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_ORDER_ORDER_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                this.m_TransitionOrders.Add(new TransitionOrder(TransitionGroupId, Iteration, Timestep, Order));
            }
        }

        /// <summary>
        /// Fills the transition target collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionTargetCollection()
        {
            Debug.Assert(this.m_TransitionTargets.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_TARGET_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                double? TargetAmount = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] != DBNull.Value)
                {
                    TargetAmount = Convert.ToDouble(dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionTarget Item = new TransitionTarget(
                        Iteration, Timestep, StratumId, SecondaryStratumId, TertiaryStratumId, TransitionGroupId, TargetAmount, 
                        DistributionTypeId, DistributionFrequency, DistributionSD, DistributionMin, DistributionMax, this.ResultScenario);

                    Item.IsDisabled = (!Item.DistributionValue.HasValue && !Item.DistributionTypeId.HasValue);

                    if (!Item.IsDisabled)
                    {
                        this.m_DistributionProvider.Validate(
                            Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);
                    }

                    this.m_TransitionTargets.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Fills the transition target prioritization collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionTargetPrioritizationCollection()
        {
            Debug.Assert(this.m_TransitionTargetPrioritizations.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_TARGET_PRIORITIZATION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                int? Timestep = null;
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? StateClassId = null;
                double Priority = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_TARGET_PRIORITIZATION_PRIORITY_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionTargetPrioritization Item = new TransitionTargetPrioritization(
                    Iteration, 
                    Timestep, 
                    TransitionGroupId, 
                    StratumId, 
                    SecondaryStratumId, 
                    TertiaryStratumId, 
                    StateClassId, 
                    Priority);

                this.m_TransitionTargetPrioritizations.Add(Item);
            }
        }

        /// <summary>
        /// Fills the transition attribute target collection model
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionAttributeTargetCollection()
        {
            Debug.Assert(this.m_TransitionAttributeTargets.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_ATTRIBUTE_TARGET_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionAttributeTargetId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int TransitionAttributeTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_ATTRIBUTE_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                double? TargetAmount = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] != DBNull.Value)
                {
                    TargetAmount = Convert.ToDouble(dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionAttributeTarget Item = new TransitionAttributeTarget(
                        TransitionAttributeTargetId, Iteration, Timestep, StratumId, SecondaryStratumId, TertiaryStratumId, TransitionAttributeTypeId, 
                        TargetAmount, DistributionTypeId, DistributionFrequency, DistributionSD, DistributionMin, DistributionMax, this.ResultScenario);

                    Item.IsDisabled = (!Item.DistributionValue.HasValue && !Item.DistributionTypeId.HasValue);

                    if (!Item.IsDisabled)
                    {
                        this.m_DistributionProvider.Validate(
                            Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);
                    }

                    this.m_TransitionAttributeTargets.Add(Item);

                    if (!(this.m_TransitionAttributeTypesWithTarget.ContainsKey(TransitionAttributeTypeId)))
                    {
                        this.m_TransitionAttributeTypesWithTarget.Add(TransitionAttributeTypeId, true);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Fills the transition attribute target prioritization collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionAttributeTargetPrioritizationCollection()
        {
            Debug.Assert(this.m_TransitionAttributeTargetPrioritizations.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_ATTRIBUTE_TARGET_PRIORITIZATION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                int? Timestep = null;
                int TransitionAttributeTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_ATTRIBUTE_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? TransitionGroupId = null;
                int? StateClassId = null;
                double Priority = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_TARGET_PRIORITIZATION_PRIORITY_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionAttributeTargetPrioritization Item = new TransitionAttributeTargetPrioritization(
                    Iteration,
                    Timestep,
                    TransitionAttributeTypeId,
                    StratumId,
                    SecondaryStratumId,
                    TertiaryStratumId,
                    TransitionGroupId,
                    StateClassId,
                    Priority);

                this.m_TransitionAttributeTargetPrioritizations.Add(Item);
            }
        }

        /// <summary>
        /// Fills the Transition Multiplier Value Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionMultiplierValueCollection()
        {
            Debug.Assert(this.m_TransitionMultiplierValues.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? StateClassId = null;
                int AgeMin = 0;
                int AgeMax = int.MaxValue;
                int? TSTGroupId = null;
                int TSTMin = 0;
                int TSTMax = int.MaxValue;
                int? TransitionMultiplierTypeId = null;
                double? MultiplierAmount = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;
                bool TSTWild = false;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMin = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    AgeMax = Convert.ToInt32(dr[Strings.DATASHEET_AGE_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_GROUP_COLUMN_NAME] == DBNull.Value)
                {
                    if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_MIN_COLUMN_NAME] != DBNull.Value ||
                        dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_MAX_COLUMN_NAME] != DBNull.Value)
                    {
                        TSTWild = true;
                    }
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_GROUP_COLUMN_NAME] != DBNull.Value)
                {
                    TSTGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_GROUP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_MIN_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMin = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_MIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_MAX_COLUMN_NAME] != DBNull.Value)
                {
                    TSTMax = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_TST_MAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionMultiplierTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] != DBNull.Value)
                {
                    MultiplierAmount = Convert.ToDouble(dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionMultiplierValue Item = new TransitionMultiplierValue(
                        TransitionGroupId,
                        Iteration,
                        Timestep,
                        StratumId,
                        SecondaryStratumId,
                        TertiaryStratumId,
                        StateClassId,
                        AgeMin,
                        AgeMax,
                        TSTGroupId,
                        TSTMin,
                        TSTMax,
                        TSTWild,
                        TransitionMultiplierTypeId,
                        MultiplierAmount,
                        DistributionTypeId,
                        DistributionFrequency,
                        DistributionSD,
                        DistributionMin,
                        DistributionMax);

                    this.m_DistributionProvider.Validate(
                        Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);

                    this.m_TransitionMultiplierValues.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }
            }

#if DEBUG
            this.TRANSITION_MULTIPLIERS_FILLED = true;
#endif
        }

        /// <summary>
        /// Fills the Transition Spatial Multiplier Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionSpatialMultiplierCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionSpatialMultipliers.Count == 0);
            Debug.Assert(this.m_TransitionSpatialMultiplierRasters.Count == 0);

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_SPATIAL_MULTIPLIER_NAME);
            bool highResScenario = false;

            if (this.ResultScenario.DisplayName == Constants.STSIMRESOLUTION_SCENARIO_NAME)
            {
                highResScenario = true;
            }

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionSpatialMultiplierId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? TransitionMultiplierTypeId = null;
                int? Iteration = null;
                int? Timestep = null;
                string FileName = Convert.ToString(dr[Strings.DATASHEET_TRANSITION_SPATIAL_MULTIPLIER_FILE_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionMultiplierTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionSpatialMultiplier Multiplier = new TransitionSpatialMultiplier(
                    TransitionSpatialMultiplierId, TransitionGroupId, TransitionMultiplierTypeId, Iteration, Timestep, FileName);

                string tsmFilename = Spatial.GetSpatialInputFileName(ds, FileName, false);
                StochasticTimeRaster rastTSM = new StochasticTimeRaster(tsmFilename, RasterDataType.DTDouble);
                string compareMsg = "";

                //Compare the TSM raster metadata to that of the Initial Condition raster files

                var cmpRes = this.m_InputRasters.CompareMetadata(rastTSM, ref compareMsg);

                if (cmpRes == CompareMetadataResult.RowColumnMismatch)
                {
                    if (highResScenario)
                    {
                        return; // do not apply transition spatial multiplier for now
                    }

                    string msg = string.Format(CultureInfo.InvariantCulture, MessageStrings.STATUS_SPATIAL_FILE_TSM_ROW_COLUMN_MISMATCH, tsmFilename);
                    ExceptionUtils.ThrowArgumentException(msg);
                }
                else
                {
                    if (cmpRes == CompareMetadataResult.UnimportantDifferences)
                    {
                        string msg = string.Format(CultureInfo.InvariantCulture, MessageStrings.STATUS_SPATIAL_FILE_TSM_METADATA_INFO, tsmFilename, compareMsg);
                        RecordStatus(StatusType.Information, msg);
                    }

                    this.m_TransitionSpatialMultipliers.Add(Multiplier);

                    //We only want to store a single copy of each unique TSM raster file to conserve memory

                    if (!m_TransitionSpatialMultiplierRasters.ContainsKey(FileName))
                    {
                        this.CompressRasterForCellCollection(rastTSM);
                        this.m_TransitionSpatialMultiplierRasters.Add(FileName, rastTSM);
                    }
                }
            }
        }

        /// <summary>
        /// Fills the Transition Spatial Initiation Multiplier Collection for the model
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionSpatialInitiationMultiplierCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionSpatialInitiationMultipliers.Count == 0);
            Debug.Assert(this.m_TransitionSpatialInitiationMultiplierRasters.Count == 0);

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_SPATIAL_INITIATION_MULTIPLIER_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionSpatialInitiationMultiplierId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? TransitionMultiplierTypeId = null;
                int? Iteration = null;
                int? Timestep = null;
                string FileName = Convert.ToString(dr[Strings.DATASHEET_TRANSITION_SPATIAL_INITIATION_MULTIPLIER_FILE_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionMultiplierTypeId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_MULTIPLIER_TYPE_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionSpatialInitiationMultiplier Multiplier = new TransitionSpatialInitiationMultiplier(
                    TransitionSpatialInitiationMultiplierId, TransitionGroupId, TransitionMultiplierTypeId, Iteration, Timestep, FileName);

                string tsimFilename = Spatial.GetSpatialInputFileName(ds, FileName, false);
                StochasticTimeRaster rastTSIM = new StochasticTimeRaster(tsimFilename, RasterDataType.DTDouble);
                string cmpMsg = "";

                //Compare the TSIM raster metadata to that of the Initial Condition raster files
                var cmpRes = this.m_InputRasters.CompareMetadata(rastTSIM, ref cmpMsg);

                if (cmpRes == STSim.CompareMetadataResult.RowColumnMismatch)
                {
                    string msg = string.Format(CultureInfo.InvariantCulture, MessageStrings.STATUS_SPATIAL_FILE_TSIM_ROW_COLUMN_MISMATCH, tsimFilename);
                    ExceptionUtils.ThrowArgumentException(msg);
                }
                else
                {
                    if (cmpRes == STSim.CompareMetadataResult.UnimportantDifferences)
                    {
                        string msg = string.Format(CultureInfo.InvariantCulture, MessageStrings.STATUS_SPATIAL_FILE_TSIM_METADATA_INFO, tsimFilename, cmpMsg);
                        RecordStatus(StatusType.Information, msg);
                    }

                    this.m_TransitionSpatialInitiationMultipliers.Add(Multiplier);

                    //We only want to store a single copy of each unique TSIM raster file to conserve memory

                    if (!m_TransitionSpatialInitiationMultiplierRasters.ContainsKey(FileName))
                    {
                        this.CompressRasterForCellCollection(rastTSIM);
                        this.m_TransitionSpatialInitiationMultiplierRasters.Add(FileName, rastTSIM);
                    }
                }
            }
        }

        /// <summary>
        /// Fills the transition size distribution collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionSizeDistributionCollection()
        {
#if DEBUG

            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionSizeDistributions.Count == 0);

            Debug.Assert(TRANSITION_TYPES_FILLED);
            Debug.Assert(TRANSITION_GROUPS_FILLED);

#endif

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_SIZE_DISTRIBUTION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionSizeDistributionId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int? StratumId = null;
                int? Iteration = null;
                int? Timestep = null;
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                double MaximumSize = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_SIZE_DISTRIBUTION_MAXIMUM_AREA_COLUMN_NAME], CultureInfo.InvariantCulture);
                double RelativeAmount = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_SIZE_DISTRIBUTION_RELATIVE_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionSizeDistribution tsd = new TransitionSizeDistribution(
                    TransitionSizeDistributionId, StratumId, Iteration, Timestep, TransitionGroupId, MaximumSize, RelativeAmount);

                this.m_TransitionSizeDistributions.Add(tsd);
                this.m_TransitionGroups[TransitionGroupId].HasSizeDistribution = true;
            }
        }

        /// <summary>
        /// Fills the transition spread distribution collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionSpreadDistributionCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionSpreadDistributions.Count == 0);

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_SPREAD_DISTRIBUTION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionSpreadDistributionId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int? StratumId = null;
                int? Iteration = null;
                int? Timestep = null;
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int StateClassId = Convert.ToInt32(dr[Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                double MaximumDistance = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_SPREAD_DISTRIBUTION_MAXIMUM_DISTANCE_COLUMN_NAME], CultureInfo.InvariantCulture);
                double RelativeAmount = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_SPREAD_DISTRIBUTION_RELATIVE_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionSpreadDistribution tsd = new TransitionSpreadDistribution(
                    TransitionSpreadDistributionId, StratumId, Iteration, Timestep, TransitionGroupId, 
                    StateClassId, MaximumDistance, RelativeAmount);

                this.m_TransitionSpreadDistributions.Add(tsd);
            }
        }

        /// <summary>
        /// Fills the Transition Patch Prioritization Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionPatchPrioritizationCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionPatchPrioritizations.Count == 0);

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_PATCH_PRIORITIZATION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionPatchPrioritizationId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int PatchPrioritizationId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_PATCH_PRIORITIZATION_PP_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                Debug.Assert(this.m_PatchPrioritizations.Contains(PatchPrioritizationId));

                TransitionPatchPrioritization pp = new TransitionPatchPrioritization(
                    TransitionPatchPrioritizationId, Iteration, Timestep, TransitionGroupId, PatchPrioritizationId);

                this.m_TransitionPatchPrioritizations.Add(pp);
            }
        }

        /// <summary>
        /// Fills the Transition Size Prioritization Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionSizePrioritizationCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionSizePrioritizations.Count == 0);

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionSizePrioritizationId = Convert.ToInt32(dr[ds.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? TransitionGroupId = null;
                SizePrioritization PriorityType = SizePrioritization.Largest;
                bool MaximizeFidelityToDistribution = true;
                bool MaximizeFidelityToTotalArea = false;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_PRIORITY_TYPE_COLUMN_NAME] != DBNull.Value)
                {
                    PriorityType = (SizePrioritization)(long)dr[Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_PRIORITY_TYPE_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_MFDIST_COLUMN_NAME] != DBNull.Value)
                {
                    MaximizeFidelityToDistribution = Convert.ToBoolean(dr[Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_MFDIST_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_MFAREA_COLUMN_NAME] != DBNull.Value)
                {
                    MaximizeFidelityToTotalArea = Convert.ToBoolean(dr[Strings.DATASHEET_TRANSITION_SIZE_PRIORITIZATION_MFAREA_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionSizePrioritization Item = new TransitionSizePrioritization(
                    TransitionSizePrioritizationId, Iteration, Timestep, StratumId, TransitionGroupId, PriorityType, 
                    MaximizeFidelityToDistribution, MaximizeFidelityToTotalArea);

                this.m_TransitionSizePrioritizations.Add(Item);
            }
        }

        /// <summary>
        /// Fills the Transition Pathway Auto-Correlation Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionPathwayAutoCorrelationCollection()
        {
            Debug.Assert(this.IsSpatial);
            Debug.Assert(this.m_TransitionPathwayAutoCorrelations.Count == 0);

            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_PATHWAY_AUTO_CORRELATION_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int? TransitionGroupId = null;
                bool AutoCorrelation = DataTableUtilities.GetDataBool(dr, Strings.DATASHEET_TRANSITION_PATHWAY_AUTO_CORRELATION_COLUMN_NAME);
                AutoCorrelationSpread SpreadTo = AutoCorrelationSpread.ToAnyCell;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_PATHWAY_SPREAD_TO_COLUMN_NAME] != DBNull.Value)
                {
                    SpreadTo = (AutoCorrelationSpread)Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_PATHWAY_SPREAD_TO_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                TransitionPathwayAutoCorrelation Item = new TransitionPathwayAutoCorrelation(
                    Iteration, Timestep, StratumId, SecondaryStratumId, TertiaryStratumId, TransitionGroupId, AutoCorrelation, SpreadTo);

                this.m_TransitionPathwayAutoCorrelations.Add(Item);
            }
        }

        /// <summary>
        /// Fills the Transition Direction Multiplier Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionDirectionMultiplierCollection()
        {
            Debug.Assert(this.m_TransitionDirectionMultipliers.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_DIRECTION_MULTIPLER_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;

                CardinalDirection CardinalDirection = (CardinalDirection)Convert.ToInt32(dr[
                    Strings.DATASHEET_TRANSITION_DIRECTION_MULTIPLER_CARDINAL_DIRECTION_COLUMN_NAME], CultureInfo.InvariantCulture);

                double? MultiplierAmount = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] != DBNull.Value)
                {
                    MultiplierAmount = Convert.ToDouble(dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionDirectionMultiplier Item = new TransitionDirectionMultiplier(
                        TransitionGroupId, Iteration, Timestep, StratumId, SecondaryStratumId, TertiaryStratumId, CardinalDirection, MultiplierAmount,
                        DistributionTypeId, DistributionFrequency, DistributionSD, DistributionMin, DistributionMax);

                    this.m_DistributionProvider.Validate(
                        Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);

                    this.m_TransitionDirectionMultipliers.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Fills the Transition Slope Multiplier Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionSlopeMultiplierCollection()
        {
            Debug.Assert(this.m_TransitionSlopeMultipliers.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_SLOPE_MULTIPLIER_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                int Slope = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_SLOPE_MULTIPLIER_SLOPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                double? MultiplierAmount = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] != DBNull.Value)
                {
                    MultiplierAmount = Convert.ToDouble(dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionSlopeMultiplier Item = new TransitionSlopeMultiplier(
                        TransitionGroupId, Iteration, Timestep, StratumId, SecondaryStratumId, TertiaryStratumId, Slope, MultiplierAmount, 
                        DistributionTypeId, DistributionFrequency, DistributionSD, DistributionMin, DistributionMax);

                    this.m_DistributionProvider.Validate(
                        Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);

                    this.m_TransitionSlopeMultipliers.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }
            }
        }

        private bool TRANSITION_ADJACENCY_SETTINGS_FILLED;
        private bool TRANSITION_ADJACENCY_MULTIPLIERS_FILLED;

        private double GetCellSizeSafe()
        {
            DataRow SpatialICPropsRow = this.ResultScenario.GetDataSheet(Strings.DATASHEET_SPPIC_NAME).GetDataRow();

            if (SpatialICPropsRow == null || SpatialICPropsRow[Strings.DATASHEET_SPPIC_CELL_SIZE_COLUMN_NAME] == DBNull.Value)
            {
                DataRow NonSpatialICRow = this.ResultScenario.GetDataSheet(Strings.DATASHEET_NSIC_NAME).GetDataRow();
                double TotalAmount = Convert.ToDouble(NonSpatialICRow[Strings.DATASHEET_NSIC_TOTAL_AMOUNT_COLUMN_NAME]);
                int NumCells = Convert.ToInt32(NonSpatialICRow[Strings.DATASHEET_NSIC_NUM_CELLS_COLUMN_NAME]);

                return (TotalAmount / NumCells);
            }
            else
            {
                return Convert.ToDouble(SpatialICPropsRow[Strings.DATASHEET_SPPIC_CELL_SIZE_COLUMN_NAME]);
            }
        }

        private double GetDefaultNeighborhoodRadius()
        {
            double CellSize = this.GetCellSizeSafe();
            double Radius = Math.Sqrt(2 * (Math.Pow(CellSize, 2)));

            return Math.Ceiling(Radius);
        }

        /// <summary>
        /// Fills the Transition Adjacency Setting Collection
        /// </summary>
        /// <remarks></remarks>
        private void FillTransitionAdjacencySettingCollection()
        {
            Debug.Assert(this.m_TransitionAdjacencySettings.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_ADJACENCY_SETTING_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? StateClassId = DataTableUtilities.GetNullableInt(dr, Strings.DATASHEET_STATECLASS_ID_COLUMN_NAME);
                int? StateAttributeTypeId = DataTableUtilities.GetNullableInt(dr, Strings.DATASHEET_STATE_ATTRIBUTE_TYPE_ID_COLUMN_NAME);
                double? NeighborhoodRadius = DataTableUtilities.GetNullableDouble(dr, Strings.DATASHEET_TRANSITION_ADJACENCY_SETTING_NBR_COLUMN_NAME);
                int? UpdateFrequency = DataTableUtilities.GetNullableInt(dr, Strings.DATASHEET_TRANSITION_ADJACENCY_SETTING_UF_COLUMN_NAME);

                if (!NeighborhoodRadius.HasValue)
                {
                    NeighborhoodRadius = this.GetDefaultNeighborhoodRadius();
                }

                if (!StateClassId.HasValue && !StateAttributeTypeId.HasValue)
                {
                    throw new ArgumentException(
                        "Transition adjacency settings: you must specify either a " + 
                        "adjacent state class or an adjacent state attribute type.");
                }

                this.m_TransitionAdjacencySettings.Add(
                    new TransitionAdjacencySetting(
                        TransitionGroupId, 
                        StateClassId,
                        StateAttributeTypeId, 
                        NeighborhoodRadius.Value, 
                        UpdateFrequency));
            }

            this.TRANSITION_ADJACENCY_SETTINGS_FILLED = true;
        }

        /// <summary>
        /// Fills the Transition Adjacency Multiplier Collection
        /// </summary>
        /// <remarks>
        /// The Transition Adjacency Setting Collection must be filled before this collection is filled so
        /// we can validate that they have the same transition groups.
        /// </remarks>
        private void FillTransitionAdjacencyMultiplierCollection()
        {
            Debug.Assert(this.m_TransitionAdjacencyMultipliers.Count == 0);
            DataSheet ds = this.ResultScenario.GetDataSheet(Strings.DATASHEET_TRANSITION_ADJACENCY_MULTIPLIER_NAME);

            foreach (DataRow dr in ds.GetData().Rows)
            {
                int TransitionGroupId = Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                int? Iteration = null;
                int? Timestep = null;
                int? StratumId = null;
                int? SecondaryStratumId = null;
                int? TertiaryStratumId = null;
                double AttributeValue = Constants.EIGHT_DIV_NINE;
                double? MultiplierAmount = null;
                int? DistributionTypeId = null;
                DistributionFrequency? DistributionFrequency = null;
                double? DistributionSD = null;
                double? DistributionMin = null;
                double? DistributionMax = null;

                if (dr[Strings.DATASHEET_ITERATION_COLUMN_NAME] != DBNull.Value)
                {
                    Iteration = Convert.ToInt32(dr[Strings.DATASHEET_ITERATION_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME] != DBNull.Value)
                {
                    Timestep = Convert.ToInt32(dr[Strings.DATASHEET_TIMESTEP_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    StratumId = Convert.ToInt32(dr[Strings.DATASHEET_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    SecondaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_SECONDARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME] != DBNull.Value)
                {
                    TertiaryStratumId = Convert.ToInt32(dr[Strings.DATASHEET_TERTIARY_STRATUM_ID_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_TRANSITION_ADJACENCY_MULTIPLIER_ATTRIBUTE_VALUE_COLUMN_NAME] != DBNull.Value)
                {
                    AttributeValue = Convert.ToDouble(dr[Strings.DATASHEET_TRANSITION_ADJACENCY_MULTIPLIER_ATTRIBUTE_VALUE_COLUMN_NAME]);
                }

                if (dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] != DBNull.Value)
                {
                    MultiplierAmount = Convert.ToDouble(dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionTypeId = Convert.ToInt32(dr[Strings.DATASHEET_DISTRIBUTIONTYPE_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionFrequency = (DistributionFrequency)(long)dr[Strings.DATASHEET_DISTRIBUTION_FREQUENCY_COLUMN_NAME];
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionSD = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONSD_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMin = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMIN_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                if (dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME] != DBNull.Value)
                {
                    DistributionMax = Convert.ToDouble(dr[Strings.DATASHEET_DISTRIBUTIONMAX_COLUMN_NAME], CultureInfo.InvariantCulture);
                }

                try
                {
                    TransitionAdjacencyMultiplier Item = new TransitionAdjacencyMultiplier(
                        TransitionGroupId, Iteration, Timestep, StratumId, SecondaryStratumId, TertiaryStratumId, AttributeValue, MultiplierAmount, 
                        DistributionTypeId, DistributionFrequency, DistributionSD, DistributionMin, DistributionMax);

                    this.m_DistributionProvider.Validate(
                        Item.DistributionTypeId, Item.DistributionValue, Item.DistributionSD, Item.DistributionMin, Item.DistributionMax);

                    this.m_TransitionAdjacencyMultipliers.Add(Item);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ds.DisplayName + " -> " + ex.Message);
                }
            }

            //The initial collection has been filled

            this.TRANSITION_ADJACENCY_MULTIPLIERS_FILLED = true;

            //Add default records for any transition groups that appear in settings
            //but not in multiplier values.

            this.AddRecordsForMissingTransitionGroups(ds);

            //Warn if transition groups still not identical - specifically, if a transition
            //group appears in the multiplier values but not in settings.

            if (!this.AdjacencyMultiplierGroupsIdentical())
            {
                this.RecordStatus(StatusType.Warning, 
                    "Transition adjacency settings and multipliers do not have identical transition groups.  " + 
                    "Some multipliers may not be applied.");
            }
        }

        private Dictionary<int, TransitionAdjacencySetting> CreateAdjacencySettingDictionaryByTGID()
        {
            Debug.Assert(TRANSITION_ADJACENCY_SETTINGS_FILLED == true);
            Dictionary<int, TransitionAdjacencySetting> d = new Dictionary<int, TransitionAdjacencySetting>();

            foreach (TransitionAdjacencySetting s in this.m_TransitionAdjacencySettings)
            {
                if (!d.ContainsKey(s.TransitionGroupId))
                {
                    d.Add(s.TransitionGroupId, s);
                }
            }

            return d;
        }

        private Dictionary<int, TransitionAdjacencyMultiplier> CreateAdjacencyMultiplierDictionaryByTGID()
        {
            Debug.Assert(TRANSITION_ADJACENCY_MULTIPLIERS_FILLED == true);
            Dictionary<int, TransitionAdjacencyMultiplier> d = new Dictionary<int, TransitionAdjacencyMultiplier>();

            foreach (TransitionAdjacencyMultiplier m in this.m_TransitionAdjacencyMultipliers)
            {
                if (!d.ContainsKey(m.TransitionGroupId))
                {
                    d.Add(m.TransitionGroupId, m);
                }
            }

            return d;
        }

        private bool AdjacencyMultiplierGroupsIdentical()
        {
            Dictionary<int, TransitionAdjacencySetting> Settings = this.CreateAdjacencySettingDictionaryByTGID();
            Dictionary<int, TransitionAdjacencyMultiplier> Multipliers = this.CreateAdjacencyMultiplierDictionaryByTGID();

            if (Settings.Count != Multipliers.Count)
            {
                return false;
            }

            foreach (int tg in Settings.Keys)
            {
                if (!Multipliers.ContainsKey(tg))
                {
                    return false;
                }
            }

            foreach (int tg in Multipliers.Keys)
            {
                if (!Settings.ContainsKey(tg))
                {
                    return false;
                }
            }

            return true;
        }

        private void AddRecordsForMissingTransitionGroups(DataSheet ds)
        {
            DataTable dt = ds.GetData();
            Dictionary<int, TransitionAdjacencySetting> Settings = this.CreateAdjacencySettingDictionaryByTGID();
            Dictionary<int, TransitionAdjacencyMultiplier> Multipliers = this.CreateAdjacencyMultiplierDictionaryByTGID();

            foreach (int tg in Settings.Keys)
            {
                if (!Multipliers.ContainsKey(tg))
                {
                    DataRow dr = dt.NewRow();
                    dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] = tg;
                    dr[Strings.DATASHEET_AMOUNT_COLUMN_NAME] = 1.0;
                    dt.Rows.Add(dr);

                    this.m_TransitionAdjacencyMultipliers.Add(
                        new TransitionAdjacencyMultiplier(
                            tg, null, null, null, null, null, Constants.EIGHT_DIV_NINE, 1.0,
                            null, null, null, null, null));
                }
            }
        }

        /// <summary>
        /// ValidateSpatialPrimaryGroups
        /// </summary>
        /// <remarks>
        /// If the run is spatial and Area targets, patch prioritization, direction or slope multipliers have been defined
        /// for groups that have no records in the types by group table where primary = true OR NULL then show a warning.
        /// </remarks>
        private void ValidateSpatialPrimaryGroups()
        {
#if DEBUG
            Debug.Assert(this.m_IsSpatial);
            Debug.Assert(this.TRANSITION_TYPES_FILLED);
            Debug.Assert(this.TRANSITION_GROUPS_FILLED);
#endif

            //Then, verify that each collection has at least one primary transition group

            bool TransitionTargetsGroupFound = true;
            bool TransitionPatchPrioritizationGroupFound = true;
            bool TransitionDirectionMultipliersGroupFound = true;
            bool TransitionSlopeMultipliersGroupFound = true;

            foreach (TransitionTarget t in this.m_TransitionTargets)
            {
                if (!this.m_PrimaryTransitionGroups.Contains(t.TransitionGroupId))
                {
                    TransitionTargetsGroupFound = false;
                    break;
                }
            }

            foreach (TransitionPatchPrioritization t in this.m_TransitionPatchPrioritizations)
            {
                if (!this.m_PrimaryTransitionGroups.Contains(t.TransitionGroupId))
                {
                    TransitionPatchPrioritizationGroupFound = false;
                    break;
                }
            }

            foreach (TransitionDirectionMultiplier t in this.m_TransitionDirectionMultipliers)
            {
                if (!this.m_PrimaryTransitionGroups.Contains(t.TransitionGroupId))
                {
                    TransitionDirectionMultipliersGroupFound = false;
                    break;
                }
            }

            foreach (TransitionSlopeMultiplier t in this.m_TransitionSlopeMultipliers)
            {
                if (!this.m_PrimaryTransitionGroups.Contains(t.TransitionGroupId))
                {
                    TransitionSlopeMultipliersGroupFound = false;
                    break;
                }
            }

            if (!TransitionTargetsGroupFound)
            {
                this.RecordStatus(StatusType.Warning, "At least one Transition Target has been defined with a non-primary Transition Group.");
            }

            if (!TransitionPatchPrioritizationGroupFound)
            {
                this.RecordStatus(StatusType.Warning, "At least one Transition Patch Prioritization has been defined with a non-primary Transition Group.");
            }

            if (!TransitionDirectionMultipliersGroupFound)
            {
                this.RecordStatus(StatusType.Warning, "At least one Transition Direction Multiplier has been defined with a non-primary Transition Group.");
            }

            if (!TransitionSlopeMultipliersGroupFound)
            {
                this.RecordStatus(StatusType.Warning, "At least one Transition Slope Multiplier has been defined with a non-primary Transition Group.");
            }
        }
    }
}
