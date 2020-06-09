﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2019 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System;
using System.Diagnostics;
using System.Globalization;
using SyncroSim.StochasticTime;
using System.Collections.Generic;
using SyncroSim.Core;
using SyncroSim.Common;

namespace SyncroSim.STSim
{
    public sealed partial class STSimTransformer : StochasticTimeTransformer
    {
        private double m_TotalAmount;
        private bool m_CalcNumCellsFromDist;
        private RandomGenerator m_RandomGenerator = new RandomGenerator();
        private STSimDistributionProvider m_DistributionProvider;
        private string m_TimestepUnitsLower = "timestep";
        private AgeHelper m_AgeReportingHelperSC;
        private AgeHelper m_AgeReportingHelperTR;
        private AgeHelper m_AgeReportingHelperSA;
        private AgeHelper m_AgeReportingHelperTA;
        private SizeClassHelper m_SizeClassHelper;
        private double m_AmountPerCell;
        private int m_TotalIterations;
        private int m_TimestepZero;
        private bool m_IsSpatial;

        public event EventHandler<CellEventArgs> CellInitialized;
        public event EventHandler<CellEventArgs> CellsInitialized;
        public event EventHandler<CellChangeEventArgs> ChangingCellProbabilistic;
        public event EventHandler<CellChangeEventArgs> ChangingCellDeterministic;
        public event EventHandler<CellChangeEventArgs> CellBeforeTransitions;
        public event EventHandler<SpatialTransitionEventArgs> ApplyingSpatialTransitions;
        public event EventHandler<MultiplierEventArgs> ApplyingTransitionMultipliers;
        public event EventHandler<MultiplierEventArgs> ApplyingSpatialMultipliers;
        public event EventHandler<EventArgs> BeginModelRun;
        public event EventHandler<EventArgs> ModelRunComplete;
        public event EventHandler<EventArgs> BeginNormalSpatialMerge;
        public event EventHandler<EventArgs> NormalSpatialMergeComplete;

        /// <summary>
        /// Gets whether this should be a spatial run
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsSpatial
        {
            get
            {
                return this.m_IsSpatial;
            }
        }

        /// <summary>
        /// Gets the value for Timestep Zero
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int TimestepZero
        {
            get
            {
                return this.m_TimestepZero;
            }
        }

        /// <summary>
        /// Gets the amount per cell
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double AmountPerCell
        {
            get
            {
                return this.m_AmountPerCell;
            }
        }

        /// <summary>
        /// Collection of simulation cells
        /// </summary>
        /// <remarks></remarks>
        public CellCollection Cells
        {
            get
            {
                return this.m_Cells;
            }
        }

        /// <summary>
        /// Collection of Transition Groups
        /// </summary>
        public TransitionGroupCollection TransitionGroups
        {
            get
            {
                return this.m_TransitionGroups;
            }
        }

        /// <summary>
        /// Collection of Transition Types
        /// </summary>
        public TransitionTypeCollection TransitionTypes
        {
            get
            {
                return this.m_TransitionTypes;
            }
        }

        /// <summary>
        /// Gets the input rasters array
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public InputRasters InputRasters
        {
            get
            {
                return this.m_InputRasters;
            }
        }

        /// <summary>
        /// Gets the distribution provider
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public STSimDistributionProvider DistributionProvider
        {
            get
            {
                return this.m_DistributionProvider;
            }
        }

        public ParallelJobConfig STSimLoadConfigurationFile()
        {
            return this.LoadConfigurationFile();
        }

        public int STSimGetJobIdFromFolder(string folderName)
        {
            return ParallelTransformer.GetJobIdFromFolder(folderName);
        }

        public int STSimGetMergeScenarioId(DataStore store)
        {
            return ParallelTransformer.GetMergeScenarioId(store);
        }

        public string STSimGetJobOutputScenarioFolderName(string libraryFileName, int scenarioId, bool create)
        {
            return ParallelTransformer.GetJobOutputScenarioFolderName(libraryFileName, scenarioId, create);
        }

        public double? GetAttributeValueNoAge(
            int stateAttributeTypeId, int stratumId, int? secondaryStratumId, int? tertiaryStratumId, int stateClassId, int iteration, int timestep)
        {
            return this.m_StateAttributeValueMapNoAges.GetAttributeValueNoAge(
                stateAttributeTypeId, stratumId, secondaryStratumId, tertiaryStratumId, stateClassId, iteration, timestep);
        }

        public double? GetAttributeValueByAge(
            int stateAttributeTypeId, int stratumId, int? secondaryStratumId, int? tertiaryStratumId, int stateClassId, int iteration, int timestep, int age)
        {
            return this.m_StateAttributeValueMapAges.GetAttributeValueByAge(
                stateAttributeTypeId, stratumId, secondaryStratumId, tertiaryStratumId, stateClassId, iteration, timestep, age);
        }

        /// <summary>
        /// Compresses the specified raster to match the cell collection
        /// </summary>
        /// <param name="raster"></param>
        public void CompressRasterForCellCollection(StochasticTimeRaster raster)
        {
            if (raster.DataType == RasterDataType.DTInteger)
            {
                int[] arr = new int[this.Cells.Count];

                foreach (Cell c in this.Cells)
                {
                    arr[c.CollectionIndex] = raster.IntCells[c.CellId];
                }

                raster.IntCells = arr;
            }
            else
            {
                double[] arr = new double[this.Cells.Count];

                foreach (Cell c in this.Cells)
                {
                    arr[c.CollectionIndex] = raster.DblCells[c.CellId];
                }

                raster.DblCells = arr;
            }
        }

        /// <summary>
        /// Overrides Configure
        /// </summary>
        /// <remarks></remarks>
        public override void Configure()
        {
            this.InternalConfigure();
        }

        /// <summary>
        /// Overrides Initialize
        /// </summary>
        public override void Initialize()
        {
            this.InternalInitialize();
        }

        /// <summary>
        /// Overrides Transform
        /// </summary>
        /// <remarks></remarks>
        public override void Transform()
        {
            this.InternalTransform();
        }

        /// <summary>
        /// Overrides OnBeforeIteration
        /// </summary>
        /// <param name="iteration"></param>
        /// <remarks></remarks>
        protected override void OnBeforeIteration(int iteration)
        {
            base.OnBeforeIteration(iteration);
            this.InternalOnBeforeIteration(iteration);
        }

        /// <summary>
        /// Overrides OnIteration
        /// </summary>
        /// <param name="iteration"></param>
        /// <remarks></remarks>
        protected override void OnIteration(int iteration)
        {
            this.InternalOnIteration(iteration);
        }

        /// <summary>
        /// Overrides OnBeforeTimestep
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <remarks></remarks>
        protected override void OnBeforeTimestep(int iteration, int timestep)
        {
            base.OnBeforeTimestep(iteration, timestep);
            this.InternalOnBeforeTimestep(iteration, timestep);
        }

        /// <summary>
        /// Overrides OnTimestep
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <remarks></remarks>
        protected override void OnTimestep(int iteration, int timestep)
        {
            this.InternalOnTimestep(iteration, timestep);
        }

        /// <summary>
        /// Called when external data has been appended to the specified data sheet
        /// </summary>
        /// <param name="dataSheet"></param>
        /// <remarks></remarks>
        protected override void OnExternalDataReady(DataSheet dataSheet)
        {
            this.STSimExternalDataReady(dataSheet);
        }

        private void InternalConfigure()
        {
            base.Configure();

            this.ConfigureIsSpatialRunFlag();
            this.ConfigureTimestepUnits();
            this.NormalizeRunControl();
            this.NormalizeOutputOptions();
            this.NormalizeMapIds();
            this.NormalizeInitialConditions();

            if (this.IsSpatial)
            {
                this.NormalizeColorData();
            }
        }

        private void InternalInitialize()
        {
            this.SetStatusMessage("Initializing");

            this.ConfigureTimestepUnits();
            this.ConfigureIsSpatialRunFlag();
            this.ConfigureTimestepsAndIterations();

            if (this.IsSpatial)
            {
                this.FillInitialConditionsSpatialCollectionAndMap();
                this.InitializeRasterData(this.MinimumIteration);
            }
            else
            {
                this.FillInitialConditionsDistributionCollectionAndMap();
            }

            this.InitializeOutputOptions();
            this.InitializeDistributionProvider();
            this.InitializeAgeReportingHelpers();
            this.InitializeSizeClassHelper();
            this.InitializeModelCollections();
            this.NormalizeForUserDistributions();
            this.InitializeDistributionValues();
            this.InitializeOutputDataTables();
            this.InitializeModel();
        }

        private void InternalTransform()
        {
            BeginModelRun?.Invoke(this, new EventArgs());

            //Run the main loop

            base.RunStochasticLoop();

            //We process spatial averaging output after the rest of the model has completed because
            //these calculations must be done across the entire data set.

            this.WriteAvgStateClassRasters();
            this.WriteAvgAgeRasters();
            this.WriteAvgStateAttributeRasters();
            this.WriteAvgTransitionAttributeRasters();
            this.WriteAvgTransitionProbabiltyRasters();

            ModelRunComplete?.Invoke(this, new EventArgs());
        }

        public override void PostProcess()
        {
            base.PostProcess();

            //After the entire transformation is complete we must update the age classes

            using (SyncroSimTransactionScope scope = Session.CreateTransactionScope())
            {
                using (DataStore store = this.Library.CreateDataStore())
                {
                    AgeUtilities.UpdateAgeClassWork(store, this.ResultScenario);
                }

                scope.Complete();
            }
        }

        private void InternalOnBeforeIteration(int iteration)
        {
            this.ResampleExternalVariableValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleDistributionValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleStateAttributeValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionAttributeValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionTargetValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionAttributeTargetValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionMultiplierValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionDirectionMultiplierValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionSlopeMultiplierValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
            this.ResampleTransitionAdjacencyMultiplierValues(iteration, this.MinimumTimestep, DistributionFrequency.Iteration);
        }

        private void InternalOnBeforeTimestep(int iteration, int timestep)
        {
            this.ResampleExternalVariableValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleDistributionValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleStateAttributeValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionAttributeValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionTargetValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionAttributeTargetValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionMultiplierValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionDirectionMultiplierValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionSlopeMultiplierValues(iteration, timestep, DistributionFrequency.Timestep);
            this.ResampleTransitionAdjacencyMultiplierValues(iteration, timestep, DistributionFrequency.Timestep);
        }

        private void InternalOnIteration(int iteration)
        {
            base.OnIteration(iteration);

            this.m_ProportionAccumulatorMap = new ProportionAccumulatorMap(this.m_AmountPerCell);

            if (this.IsSpatial)
            {
                this.ResetStrataCellCollections();
                this.InitializeRasterData(iteration);
                this.InitializeCellsRaster(iteration);
                this.ResetTransitionSpreadGroupCells();
            }
            else
            {
                this.InitializeCellsNonRaster(iteration);
            }

            this.WriteStratumAmountTabularData(iteration, this.m_TimestepZero);
            this.WriteStratumRaster(iteration, this.m_TimestepZero);
            this.WriteStateClassRaster(iteration, this.m_TimestepZero);
            this.WriteAgeRaster(iteration, this.m_TimestepZero);
            this.WriteTSTRasters(iteration, this.m_TimestepZero);
            this.WriteStateAttributeRasters(iteration, this.m_TimestepZero);
            this.ProcessTransitionAdjacencyStateAttributeOutput(iteration, this.m_TimestepZero);
        }

        private void InternalOnTimestep(int iteration, int timestep)
        {
            base.OnTimestep(iteration, timestep);

            this.Simulate(iteration, timestep);
            this.GenerateStateClassAttributes();

            //Tabular data
            this.WriteStratumAmountTabularData(iteration, timestep);
            this.WriteSummaryStateClassTabularData(this.m_OutputStratumStateTable, iteration, timestep);
            this.WriteSummaryTransitionTabularData(timestep, this.m_OutputStratumTransitionTable);
            this.WriteSummaryTransitionStateTabularData(this.m_OutputStratumTransitionStateTable);
            this.WriteSummaryStateAttributeTabularData(this.m_OutputStateAttributeTable);
            this.WriteSummaryTransitionAttributeTabularData(this.m_OutputTransitionAttributeTable);

            //Spatial data
            this.WriteStratumRaster(iteration, timestep);
            this.WriteStateClassRaster(iteration, timestep);
            this.WriteAgeRaster(iteration, timestep);
            this.WriteTSTRasters(iteration, timestep);
            this.WriteStateAttributeRasters(iteration, timestep);

            //Other data
            this.ProcessTransitionAdjacencyStateAttributeOutput(iteration, timestep);

            //Record averaging data
            this.RecordAvgRasterStateClassData(timestep);
            this.RecordAvgRasterAgeData(timestep);
            this.RecordAvgRasterStateAttributeData(iteration, timestep);

            Debug.Assert(this.m_SummaryTransitionAttributeResults.Count == 0);
        }

        /// <summary>
        /// Simulates for the specified iteration and timestep
        /// </summary>
        private void Simulate(int iteration, int timestep)
        {
            this.ResetTransitionsForCells(iteration, timestep);
            this.ResetTransitionAttributeRemainingTargetAmounts();
            this.AssignPatchPrioritizations(iteration, timestep);
            this.ApplyTransitions(iteration, timestep);
        }

        /// <summary>
        /// Applies transitions for all cells
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <remarks></remarks>
        private void ApplyTransitions(int iteration, int timestep)
        {
            this.ReorderShufflableTransitionGroups(iteration, timestep);

            foreach (Cell simulationCell in this.m_Cells)
            {
                DeterministicTransition dt = this.GetDeterministicTransition(simulationCell.StratumId, simulationCell.StateClassId, iteration, timestep);

                if (dt != null)
                {
                    CellBeforeTransitions?.Invoke(this, new CellChangeEventArgs(simulationCell, iteration, timestep, dt, null));
                }
            }

            if (this.IsSpatial)
            {
                Dictionary<int, double[]> RasterTransitionAttrValues = CreateRasterTransitionAttributeArrays(timestep);
                Dictionary<int, int[]> dictTransitionedPixels = CreateTransitionGroupTransitionedPixels();
                Dictionary<int, int[]> dictTransitionedEventPixels = CreateTransitionGroupTransitionedPixels();

                ApplyingSpatialTransitions?.Invoke(this, new SpatialTransitionEventArgs(iteration, timestep));

                //Apply spatial probabilistic transitions
                this.ApplyProbabilisticTransitionsRaster(iteration, timestep, RasterTransitionAttrValues, dictTransitionedPixels, dictTransitionedEventPixels);

                //Apply spatial transition spread
                this.ApplyTransitionSpread(iteration, timestep, RasterTransitionAttrValues, dictTransitionedPixels);

                //Write/record spatial probabilistic transition data
                this.WriteTransitionGroupRasters(iteration, timestep, dictTransitionedPixels); 
                this.RecordAvgRasterTransitionProbabilityData(timestep, dictTransitionedPixels); 
                         
                //Write transition event data                   
                this.WriteTransitionEventRasters(iteration, timestep, dictTransitionedEventPixels);

                //Apply deterministic transitions
                foreach (Cell simulationCell in this.m_Cells)
                {
                    this.ApplyDeterministicTransitions(simulationCell, iteration, timestep);
                }

                //Write/record transition attribute data
                this.WriteTransitionAttributeRasters(iteration, timestep, RasterTransitionAttrValues);
                this.RecordAvgRasterTransitionAttributeData(timestep, RasterTransitionAttrValues);
            }
            else
            {
                Dictionary<int, TransitionGroup> RemainingTransitionGroups = new Dictionary<int, TransitionGroup>();

                foreach (TransitionGroup tg in this.m_ShufflableTransitionGroups)
                {
                    RemainingTransitionGroups.Add(tg.TransitionGroupId, tg);
                }

                foreach (TransitionGroup TransitionGroup in this.m_ShufflableTransitionGroups)
                {
                    if (TransitionGroup.PrimaryTransitionTypes.Count == 0)
                    {
                        continue;
                    }

                    MultiLevelKeyMap1<Dictionary<int, TransitionAttributeTarget>> tatMap = new MultiLevelKeyMap1<Dictionary<int, TransitionAttributeTarget>>();

                    this.ResetTransitionTargetMultipliers(iteration, timestep, TransitionGroup);
                    this.ResetTransitionAttributeTargetMultipliers(iteration, timestep, RemainingTransitionGroups, tatMap, TransitionGroup);

                    RemainingTransitionGroups.Remove(TransitionGroup.TransitionGroupId);

                    foreach (Cell simulationCell in this.m_Cells)
                    {
                        this.ApplyProbabilisticTransitionsByCell(simulationCell, iteration, timestep, TransitionGroup, null, null);
                    }
                }

                foreach (Cell simulationCell in this.m_Cells)
                {
                    this.ApplyDeterministicTransitions(simulationCell, iteration, timestep);
                }
            }
        }

        /// <summary>
        /// Applies deterministic transitions for the specified cell
        /// </summary>
        /// <param name="simulationCell">The simulation cell</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="timestep">The current timestep</param>
        /// <remarks></remarks>
        private void ApplyDeterministicTransitions(Cell simulationCell, int iteration, int timestep)
        {
            if (simulationCell.StratumId == 0 || simulationCell.StateClassId == 0)
            {
                return;
            }

            DeterministicTransition dt = this.GetDeterministicTransition(simulationCell.StratumId, simulationCell.StateClassId, iteration, timestep);

            if (dt != null)
            {
				if (ChangingCellDeterministic != null)
                    ChangingCellDeterministic(this, new CellChangeEventArgs(simulationCell, iteration, timestep, dt, null));
            }

            bool IncrementAge = true;

            if (dt != null)
            {
                if (simulationCell.Age >= dt.AgeMaximum)
                {
                    if (!(dt.StateClassIdSource == dt.StateClassIdDestination))
                    {
                        this.ChangeCellForDeterministicTransition(simulationCell, dt, iteration, timestep);
                    }

                    IncrementAge = false;
                }
            }

            if (IncrementAge)
            {
                Debug.Assert(simulationCell.Age != int.MaxValue);
                simulationCell.Age += 1;
            }

            //Record output here because Tst should not be incremented until after the output has been recorded.
            this.RecordSummaryStateClassOutput(simulationCell, iteration, timestep);
            this.RecordSummaryStateAttributeOutput(simulationCell, iteration, timestep);

            //If there are Tst values then increment them by one.
            if (simulationCell.TstValues.Count > 0)
            {
                Debug.Assert(this.m_TstTransitionGroupMap.HasItems);

                foreach (Tst t in simulationCell.TstValues)
                {
                    t.TstValue += 1;
                }
            }
        }

        /// <summary>
        /// Applies probabilistic transitions for the specified cell in non-raster mode
        /// </summary>
        /// <param name="simulationCell"></param>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <param name="TransitionGroup"></param>
        /// <param name="transitionedPixels"></param>
        /// <param name="rasterTransitionAttrValues"></param>
        /// <remarks></remarks>
        private void ApplyProbabilisticTransitionsByCell(
            Cell simulationCell, int iteration, int timestep, TransitionGroup TransitionGroup, 
            int[] transitionedPixels, Dictionary<int, double[]> rasterTransitionAttrValues)
        {
            if (simulationCell.StratumId == 0 || simulationCell.StateClassId == 0)
            {
                return;
            }

            double CumulativeProbability = 0.0;
            double RandomNextDouble = this.m_RandomGenerator.GetNextDouble();

            foreach (Transition tr in simulationCell.Transitions)
            {
                if (TransitionGroup.PrimaryTransitionTypes.Contains(tr.TransitionTypeId))
                {
                    double multiplier = this.GetTransitionMultiplier(tr.TransitionTypeId, iteration, timestep, simulationCell);
                    multiplier *= this.GetExternalTransitionMultipliers(tr.TransitionTypeId, iteration, timestep, simulationCell);
                    TransitionType tt = this.m_TransitionTypes[tr.TransitionTypeId];

                    foreach (TransitionGroup tg in tt.TransitionGroups)
                    {
                        TransitionTarget target = this.m_TransitionTargetMap.GetTransitionTarget(
                            tg.TransitionGroupId, simulationCell.StratumId, simulationCell.SecondaryStratumId,
                            simulationCell.TertiaryStratumId, iteration, timestep);

                        bool TargetPrioritizationMultiplierApplied = false;

                        if (target != null && !target.IsDisabled && target.HasPrioritizations)
                        {
                            TransitionTargetPrioritization pri = target.GetPrioritization(
                                simulationCell.StratumId, simulationCell.SecondaryStratumId,
                                simulationCell.TertiaryStratumId, simulationCell.StateClassId, 
                                iteration, timestep);

                            if (pri != null && pri.ProbabilityOverride.HasValue)
                            {
                                Debug.Assert(pri.ProbabilityOverride.Value == 1.0 || pri.ProbabilityOverride.Value == 0.0);

                                if (pri.ProbabilityOverride.Value == 1.0)
                                {
                                    Transition SelectedTransition = this.SelectTransitionPathway(
                                        simulationCell, TransitionGroup.TransitionGroupId, iteration, timestep);

                                    if (SelectedTransition != null)
                                    {
                                        this.InvokeProbabilisticTransitionForCell(
                                        simulationCell, SelectedTransition, iteration, timestep, transitionedPixels, rasterTransitionAttrValues);

                                        return;
                                    }
                                }
                                else if (pri.ProbabilityOverride.Value == 0.0)
                                {
                                    multiplier *= 0.0;
                                    TargetPrioritizationMultiplierApplied = true;
                                }
                            }
                            else
                            {
                                multiplier *= pri.ProbabilityMultiplier;
                                TargetPrioritizationMultiplierApplied = true;
                            }
                        }

                        if (!TargetPrioritizationMultiplierApplied)
                        {
                            multiplier *= this.GetTransitionTargetMultiplier(
                                tg.TransitionGroupId, simulationCell.StratumId, simulationCell.SecondaryStratumId, 
                                simulationCell.TertiaryStratumId, iteration, timestep); 
                        }                                
                    }

                    if (this.m_TransitionAttributeTargets.Count > 0)
                    {
                        double? ProbOverride = this.GetAttributeTargetProbabilityOverride(tt, simulationCell, iteration, timestep);

                        if (ProbOverride.HasValue && ProbOverride.Value == 1.0)
                        {
                            Transition SelectedTransition = this.SelectTransitionPathway(
                                simulationCell, TransitionGroup.TransitionGroupId, iteration, timestep);

                            if (SelectedTransition != null)
                            {
                                this.InvokeProbabilisticTransitionForCell(
                                    simulationCell, SelectedTransition, iteration, timestep, transitionedPixels, rasterTransitionAttrValues);

                                return;
                            }
                        }

                        multiplier = this.ModifyMultiplierForTransitionAttributeTarget(multiplier, tt, simulationCell, iteration, timestep);
                    }

                    if (this.IsSpatial)
                    {
                        multiplier *= this.GetTransitionSpatialMultiplier(simulationCell, tr.TransitionTypeId, iteration, timestep);

                        foreach (TransitionGroup tg in tt.TransitionGroups)
                        {
                            multiplier *= this.GetTransitionAdjacencyMultiplier(tg.TransitionGroupId, iteration, timestep, simulationCell);
                            multiplier *= this.GetExternalSpatialMultipliers(simulationCell, iteration, timestep, tg.TransitionGroupId);
                        }
                    }

                    CumulativeProbability += (tr.Probability * tr.Proportion * multiplier);

                    if (CumulativeProbability > RandomNextDouble)
                    {
                        this.InvokeProbabilisticTransitionForCell(simulationCell, tr, iteration, timestep, transitionedPixels, rasterTransitionAttrValues);
                        return;
                    }
                }
            }
        }

        private void InvokeProbabilisticTransitionForCell(
            Cell simulationCell, Transition tr, int iteration, int timestep, 
            int[] transitionedPixels, Dictionary<int, double[]> rasterTransitionAttrValues)
        {
            this.RecordSummaryTransitionOutput(simulationCell, tr, iteration, timestep, null);
            this.RecordSummaryTransitionByStateClassOutput(simulationCell, tr, iteration, timestep);

            this.ChangeCellForProbabilisticTransition(simulationCell, tr, iteration, timestep, rasterTransitionAttrValues);
            this.FillProbabilisticTransitionsForCell(simulationCell, iteration, timestep);

            if (this.IsSpatial)
            {
                this.UpdateTransitionedPixels(simulationCell, tr.TransitionTypeId, transitionedPixels);
            }
        }

        /// <summary>
        /// Fills the transition lists with transitions that apply to the specified cell
        /// </summary>
        /// <param name="simulationCell"></param>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <remarks></remarks>
        private void FillProbabilisticTransitionsForCell(Cell simulationCell, int iteration, int timestep)
        {
            //DEVTODO: would it make sense to check for zero multipliers (temporal or spatial or area targets) to 
            //shorten this list based on excluding transitions that are impossible based on these?

            //Always clear the transition list for the cell
            simulationCell.Transitions.Clear();

            //Get the list of transitions that are applicable to this cell.  If there are none then exit now.
            TransitionCollection trlist = this.GetTransitionCollection(simulationCell, iteration, timestep);

            if (trlist == null)
            {
                return;
            }

            //But if there are transitions, add them to the cell's transition list
            foreach (Transition tr in trlist)
            {
                if (simulationCell.Age < tr.AgeMinimum)
                {
                    continue;
                }

                if (simulationCell.Age > tr.AgeMaximum)
                {
                    continue;
                }

                if (!this.CompareTstValues(simulationCell, tr))
                {
                    continue;
                }

                Debug.Assert(!simulationCell.Transitions.Contains(tr));

#if DEBUG
                if (tr.StratumIdSource.HasValue)
                {
                    Debug.Assert(this.m_Strata.Contains(tr.StratumIdSource.Value));
                }
#endif

                simulationCell.Transitions.Add(tr);
            }

            //We need to randomize the order of the list in case the sum of the probabilities exceeds 1 so that we don't 
            //bias up the probability of transitions at the top of the list.

            for (int i = simulationCell.Transitions.Count - 1; i >= 0; i--)
            {
                int n = this.m_RandomGenerator.GetNextInteger(i + 1);
                Transition t = simulationCell.Transitions[i];

                simulationCell.Transitions[i] = simulationCell.Transitions[n];
                simulationCell.Transitions[n] = t;
            }
        }

        /// <summary>
        /// Changes a simulation cell's for a deterministic transition
        /// </summary>
        /// <param name="simulationCell">The simulation cell to change</param>
        /// <param name="dt">The deterministic transition</param>
        /// <remarks></remarks>
        private void ChangeCellForDeterministicTransition(Cell simulationCell, DeterministicTransition dt, int iteration, int timestep)
        {
            this.UpdateCellStratum(simulationCell, dt.StratumIdDestination, iteration, timestep);
            this.UpdateCellStateClass(simulationCell, dt.StateClassIdDestination, iteration, timestep);

            DeterministicTransition dtdest = this.GetDeterministicTransition(simulationCell.StratumId, simulationCell.StateClassId, iteration, timestep);

            if (dtdest == null)
            {
                simulationCell.Age = 0;
            }
            else
            {
                //DEVTODO: allow to class to be null and in this case set the simulationCell.Age to dtdest.AgeMaximum
                simulationCell.Age = dtdest.AgeMinimum;
            }

            Debug.Assert(this.m_Strata.Contains(simulationCell.StratumId));
        }

        /// <summary>
        /// Changes a simulation cell's for a probabilistic transition
        /// </summary>
        /// <param name="simulationCell">The cell that is changing</param>
        /// <param name="tr">The transition</param>
        /// <param name="iteration">The iteration</param>
        /// <param name="timestep">The timestep</param>
        /// <remarks></remarks>
        private void ChangeCellForProbabilisticTransition(Cell simulationCell, Transition tr, int iteration, int timestep, Dictionary<int, double[]> rasterTransitionAttrValues)
        {
			if (ChangingCellProbabilistic != null)
                ChangingCellProbabilistic(this, new CellChangeEventArgs(simulationCell, iteration, timestep, null, tr));

            this.GenerateTransitionAttributes(simulationCell, tr, iteration, timestep, rasterTransitionAttrValues);

            this.UpdateCellStratum(simulationCell, tr.StratumIdDestination, iteration, timestep);
            this.UpdateCellStateClass(simulationCell, tr.StateClassIdDestination, iteration, timestep);

            this.ChangeCellAgeForProbabilisticTransition(simulationCell, iteration, timestep, tr);
            this.ChangeCellTstForProbabilisticTransition(simulationCell, tr);

            Debug.Assert(this.m_Strata.Contains(simulationCell.StratumId));
            Debug.Assert(simulationCell.StateClassId != 0);
        }

        /// <summary>
        /// Updates the stratum for the specified cell using the specified destination stratum
        /// </summary>
        /// <param name="simulationCell"></param>
        /// <param name="destinationStratumId"></param>
        /// <remarks></remarks>
        private void UpdateCellStratum(Cell simulationCell, int? destinationStratumId, int iteration, int timestep)
        {
            Debug.Assert(simulationCell.StratumId != 0);
            Debug.Assert(simulationCell.StateClassId != 0);

            if (destinationStratumId.HasValue)
            {
                if (destinationStratumId.Value != simulationCell.StratumId)
                {
                    Stratum stold = this.m_Strata[simulationCell.StratumId];
                    Stratum stnew = this.m_Strata[destinationStratumId.Value];

                    if (this.IsSpatial)
                    {
                        Debug.Assert(stold.Cells.ContainsKey(simulationCell.CellId));
                        Debug.Assert(!stnew.Cells.ContainsKey(simulationCell.CellId));

                        stold.Cells.Remove(simulationCell.CellId);
                        stnew.Cells.Add(simulationCell.CellId, simulationCell);
                    }

                    simulationCell.StratumId = destinationStratumId.Value;

                    if (this.IsSpatial)
                    {
                        this.UpdateTransitionsSpreadGroupMembership(simulationCell, iteration, timestep);
                    }

                    this.m_ProportionAccumulatorMap.Decrement(stold.StratumId, simulationCell.SecondaryStratumId, simulationCell.TertiaryStratumId);
                    this.m_ProportionAccumulatorMap.AddOrIncrement(stnew.StratumId, simulationCell.SecondaryStratumId, simulationCell.TertiaryStratumId);
                }
            }
        }

        /// <summary>
        /// Updates the stratum for the specified cell using the specified destination state class
        /// </summary>
        /// <param name="simulationCell"></param>
        /// <param name="destinationStateClassId"></param>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <remarks></remarks>
        private void UpdateCellStateClass(Cell simulationCell, int? destinationStateClassId, int iteration, int timestep)
        {
            Debug.Assert(simulationCell.StratumId != 0);
            Debug.Assert(simulationCell.StateClassId != 0);

            if (destinationStateClassId.HasValue)
            {
                if (destinationStateClassId != simulationCell.StateClassId)
                {
                    simulationCell.StateClassId = destinationStateClassId.Value;

                    if (this.IsSpatial)
                    {
                        this.UpdateTransitionsSpreadGroupMembership(simulationCell, iteration, timestep);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the applicable transitions for all cells
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="timestep"></param>
        /// <remarks></remarks>
        private void ResetTransitionsForCells(int iteration, int timestep)
        {
            foreach (Cell simulationCell in this.m_Cells)
            {
                this.FillProbabilisticTransitionsForCell(simulationCell, iteration, timestep);
            }

#if DEBUG

            foreach (Cell c in this.m_Cells)
            {
                this.VALIDATE_CELL_TRANSITIONS(c, iteration, timestep);
            }

#endif
        }

        private Transition SelectTransitionPathway(Cell simulationCell, int transitionGroupId, int iteration, int timestep)
        {
            double SumProbability = 0.0;
            TransitionCollection Transitions = new TransitionCollection();

            foreach (Transition tr in simulationCell.Transitions)
            {
                TransitionType tt = this.m_TransitionTypes[tr.TransitionTypeId];

                if (tt.PrimaryTransitionGroups.Contains(transitionGroupId))
                {
                    double multiplier = this.GetTransitionMultiplier(tr.TransitionTypeId, iteration, timestep, simulationCell);

                    multiplier *= this.GetExternalTransitionMultipliers(tr.TransitionTypeId, iteration, timestep, simulationCell);
                    multiplier *= this.GetTransitionTargetMultiplier(transitionGroupId, simulationCell.StratumId, simulationCell.SecondaryStratumId, simulationCell.TertiaryStratumId, iteration, timestep);

                    if (this.m_IsSpatial)
                    {
                        multiplier *= this.GetTransitionSpatialMultiplier(simulationCell, tr.TransitionTypeId, iteration, timestep);

                        foreach (TransitionGroup tg in tt.TransitionGroups)
                        {
                            multiplier *= this.GetTransitionAdjacencyMultiplier(tg.TransitionGroupId, iteration, timestep, simulationCell);

                            if (this.m_IsSpatial)
                            {

                                multiplier *= this.GetExternalSpatialMultipliers(simulationCell, iteration, timestep, tg.TransitionGroupId);
                            }
                        }
                    }

                    if (this.m_TransitionAttributeTargets.Count > 0)
                    {
                        multiplier = this.ModifyMultiplierForTransitionAttributeTarget(multiplier, tt, simulationCell, iteration, timestep);
                    }

                    SumProbability += tr.Probability * tr.Proportion * multiplier;
                    Transitions.Add(tr);
                }
            }

            double CumulativeProbability = 0.0;
            double Rand = this.m_RandomGenerator.GetNextDouble();

            foreach (Transition tr in Transitions)
            {
                TransitionType tt = this.m_TransitionTypes[tr.TransitionTypeId];

                double multiplier = this.GetTransitionMultiplier(tr.TransitionTypeId, iteration, timestep, simulationCell);

                multiplier *= this.GetExternalTransitionMultipliers(tr.TransitionTypeId, iteration, timestep, simulationCell);
                multiplier *= this.GetTransitionTargetMultiplier(transitionGroupId, simulationCell.StratumId, simulationCell.SecondaryStratumId, simulationCell.TertiaryStratumId, iteration, timestep);

                if (this.m_IsSpatial)
                {
                    multiplier *= this.GetTransitionSpatialMultiplier(simulationCell, tr.TransitionTypeId, iteration, timestep);

                    foreach (TransitionGroup tg in tt.TransitionGroups)
                    {
                        multiplier *= this.GetTransitionAdjacencyMultiplier(tg.TransitionGroupId, iteration, timestep, simulationCell);
                        multiplier *= this.GetExternalSpatialMultipliers(simulationCell, iteration, timestep, tg.TransitionGroupId);
                    }
                }

                if (this.m_TransitionAttributeTargets.Count > 0)
                {
                    multiplier = this.ModifyMultiplierForTransitionAttributeTarget(multiplier, tt, simulationCell, iteration, timestep);
                }

                CumulativeProbability += ((tr.Probability * tr.Proportion * multiplier) / SumProbability);

                if (CumulativeProbability > Rand)
                {
                    return tr;
                }
            }

            return null;
        }

        private void STSimExternalDataReady(DataSheet dataSheet)
        {
            if (dataSheet.Name == Strings.DATASHEET_PT_NAME)
            {
                this.m_Transitions.Clear();
                this.FillProbabilisticTransitionsCollection();
                this.m_TransitionMap = new TransitionMap(this.ResultScenario, this.m_Transitions);
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_TARGET_NAME)
            {
                this.m_TransitionTargets.Clear();
                this.FillTransitionTargetCollection();
                this.InitializeTransitionTargetDistributionValues();
                this.InitializeTransitionTargetPrioritizations();
                this.m_TransitionTargetMap = new TransitionTargetMap(this.ResultScenario, this.m_TransitionTargets);
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_MULTIPLIER_VALUE_NAME)
            {
                this.m_TransitionMultiplierValues.Clear();
                this.FillTransitionMultiplierValueCollection();
                this.InitializeTransitionMultiplierDistributionValues();

                foreach (TransitionMultiplierType tmt in this.m_TransitionMultiplierTypes)
                {
                    tmt.ClearMultiplierValueMap();
                }

                foreach (TransitionMultiplierValue sm in this.m_TransitionMultiplierValues)
                {
                    TransitionMultiplierType mt = this.GetTransitionMultiplierType(sm.TransitionMultiplierTypeId);
                    mt.AddTransitionMultiplierValue(sm);
                }

                foreach (TransitionMultiplierType tmt in this.m_TransitionMultiplierTypes)
                {
                    tmt.CreateMultiplierValueMap();
                }
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_SPATIAL_MULTIPLIER_NAME)
            {
                if (this.m_IsSpatial)
                {
                    this.m_TransitionSpatialMultipliers.Clear();
                    this.m_TransitionSpatialMultiplierRasters.Clear();
                    this.FillTransitionSpatialMultiplierCollection();

                    foreach (TransitionMultiplierType tmt in this.m_TransitionMultiplierTypes)
                    {
                        tmt.ClearSpatialMultiplierMap();
                    }

                    foreach (TransitionSpatialMultiplier sm in this.m_TransitionSpatialMultipliers)
                    {
                        TransitionMultiplierType mt = this.GetTransitionMultiplierType(sm.TransitionMultiplierTypeId);
                        mt.AddTransitionSpatialMultiplier(sm);
                    }

                    foreach (TransitionMultiplierType tmt in this.m_TransitionMultiplierTypes)
                    {
                        tmt.CreateSpatialMultiplierMap();
                    }
                }
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_SPATIAL_INITIATION_MULTIPLIER_NAME)
            {
                if (this.m_IsSpatial)
                {
                    this.m_TransitionSpatialInitiationMultipliers.Clear();
                    this.m_TransitionSpatialInitiationMultiplierRasters.Clear();
                    this.FillTransitionSpatialInitiationMultiplierCollection();

                    foreach (TransitionMultiplierType tmt in this.m_TransitionMultiplierTypes)
                    {
                        tmt.ClearSpatialInitiationMultiplierMap();
                    }

                    foreach (TransitionSpatialInitiationMultiplier sm in this.m_TransitionSpatialInitiationMultipliers)
                    {
                        TransitionMultiplierType mt = this.GetTransitionMultiplierType(sm.TransitionMultiplierTypeId);
                        mt.AddTransitionSpatialInitiationMultiplier(sm);
                    }

                    foreach (TransitionMultiplierType tmt in this.m_TransitionMultiplierTypes)
                    {
                        tmt.CreateSpatialInitiationMultiplierMap();
                    }
                }
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_ORDER_NAME)
            {
                this.m_TransitionOrders.Clear();
                this.FillTransitionOrderCollection();
                this.m_TransitionOrderMap = new TransitionOrderMap(this.m_TransitionOrders);
            }
            else if (dataSheet.Name == Strings.DATASHEET_STATE_ATTRIBUTE_VALUE_NAME)
            {
                this.m_StateAttributeValues.Clear();
                this.FillStateAttributeValueCollection();
                this.m_StateAttributeTypeIdsAges = null;
                this.m_StateAttributeTypeIdsNoAges = null;
                this.m_StateAttributeValueMapAges = null;
                this.m_StateAttributeValueMapNoAges = null;
                this.InitializeStateAttributes();
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_ATTRIBUTE_VALUE_NAME)
            {
                this.m_TransitionAttributeValues.Clear();
                this.FillTransitionAttributeValueCollection();
                this.m_TransitionAttributeValueMap = null;
                this.m_TransitionAttributeTypeIds = null;
                this.InitializeTransitionAttributes();
            }
            else if (dataSheet.Name == Strings.DATASHEET_TRANSITION_ATTRIBUTE_TARGET_NAME)
            {
                this.m_TransitionAttributeTargets.Clear();
                this.FillTransitionAttributeTargetCollection();
                this.InitializeTransitionAttributeTargetDistributionValues();
                this.InitializeTransitionAttributeTargetPrioritizations();
                this.m_TransitionAttributeTargetMap = new TransitionAttributeTargetMap(this.ResultScenario, this.m_TransitionAttributeTargets);
            }
            else
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "External data is not supported for: {0}", dataSheet.Name);
                throw new TransformerFailedException(msg);
            }
        }

        /// <summary>
        /// Resets the remaining amounts for transition attribute targets
        /// </summary>
        /// <remarks></remarks>
        private void ResetTransitionAttributeRemainingTargetAmounts()
        {
            foreach (TransitionAttributeTarget t in this.m_TransitionAttributeTargets)
            {
                if (!t.IsDisabled)
                {
                    t.TargetRemaining = t.CurrentValue.Value;
                }
            }
        }

        /// <summary>
        /// Resets all strata cell collections
        /// </summary>
        /// <remarks></remarks>
        private void ResetStrataCellCollections()
        {
            foreach (Stratum s in this.m_Strata)
            {
#if DEBUG
                foreach (Cell c in s.Cells.Values)
                {
                    Debug.Assert(c.StratumId != 0);
                    Debug.Assert(c.StateClassId != 0);
                }
#endif

                s.Cells.Clear();
            }
        }

        private DeterministicTransition GetDeterministicTransition(Cell simulationCell, int iteration, int timestep)
        {
            return this.GetDeterministicTransition(simulationCell.StratumId, simulationCell.StateClassId, iteration, timestep);
        }

        private DeterministicTransition GetDeterministicTransition(int? stratumId, int stateClassId, int iteration, int timestep)
        {
            return this.m_DeterministicTransitionMap.GetDeterministicTransition(stratumId, stateClassId, iteration, timestep);
        }

        private TransitionCollection GetTransitionCollection(Cell simulationCell, int iteration, int timestep)
        {
            return this.GetTransitionCollection(simulationCell.StratumId, simulationCell.StateClassId, iteration, timestep);
        }

        private TransitionCollection GetTransitionCollection(int stratumId, int stateClassId, int iteration, int timestep)
        {
            return this.m_TransitionMap.GetTransitions(stratumId, stateClassId, iteration, timestep);
        }
    }
}
