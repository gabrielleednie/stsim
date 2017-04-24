﻿'************************************************************************************
' ST-Sim: A .NET class library for developing state-and-transition simulation models.
'
' Copyright © 2007-2017 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.
'
'************************************************************************************

Imports System.IO
Imports SyncroSim.Core

Partial Class STSimUpdates

    ''' <summary>
    ''' UpdateSTSimTables_SSIM_V_1
    ''' </summary>
    ''' <param name="store"></param>
    ''' <remarks>Updates the ST-Sim tables for SyncroSim version 1</remarks>
    Private Shared Sub UpdateSTSimTables_SSIM_V_1(ByVal store As DataStore)

        'Stratum
        store.ExecuteNonQuery("CREATE TABLE STSim_Stratum(StratumID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, ID INTEGER, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_Stratum(StratumID, ProjectID, Name, ID, Description) SELECT ST_StratumID, ProjectID, Name, ID, Description FROM ST_Stratum")
        store.ExecuteNonQuery("DROP TABLE ST_Stratum")

        'Secondary Stratum
        store.ExecuteNonQuery("CREATE TABLE STSim_SecondaryStratum(SecondaryStratumID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, ID INTEGER, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_SecondaryStratum(SecondaryStratumID, ProjectID, Name, ID, Description) SELECT ST_SecondaryStratumID, ProjectID, Name, ID, Description FROM ST_SecondaryStratum")
        store.ExecuteNonQuery("DROP TABLE ST_SecondaryStratum")

        'State Label X
        store.ExecuteNonQuery("CREATE TABLE STSim_StateLabelX(StateLabelXID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_StateLabelX(StateLabelXID, ProjectID, Name, Description) SELECT ST_StateLabelXID, ProjectID, Name, Description FROM ST_StateLabelX")
        store.ExecuteNonQuery("DROP TABLE ST_StateLabelX")

        'State Label Y
        store.ExecuteNonQuery("CREATE TABLE STSim_StateLabelY(StateLabelYID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_StateLabelY(StateLabelYID, ProjectID, Name, Description) SELECT ST_StateLabelYID, ProjectID, Name, Description FROM ST_StateLabelY")
        store.ExecuteNonQuery("DROP TABLE ST_StateLabelY")

        'State Class
        store.ExecuteNonQuery("CREATE TABLE STSim_StateClass(StateClassID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, StateLabelXID INTEGER, StateLabelYID INTEGER, ID INTEGER, Color TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_StateClass(StateClassID, ProjectID, Name, StateLabelXID, StateLabelYID, ID, Color, Description) SELECT ST_StateClassID, ProjectID, Name, StateLabelXID, StateLabelYID, ID, Color, Description FROM ST_StateClass")
        store.ExecuteNonQuery("DROP TABLE ST_StateClass")

        'Transition Group
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionGroup(TransitionGroupID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionGroup(TransitionGroupID, ProjectID, Name, Description) SELECT ST_TransitionGroupID, ProjectID, Name, Description FROM ST_TransitionGroup")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionGroup")

        'Transition Type
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionType(TransitionTypeID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, ID INTEGER, Color TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionType(TransitionTypeID, ProjectID, Name, ID, Color, Description) SELECT ST_TransitionTypeID, ProjectID, Name, ID, Color, Description FROM ST_TransitionType")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionType")

        'Transition Type Group
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionTypeGroup(TransitionTypeGroupID INTEGER PRIMARY KEY AUTOINCREMENT, ProjectID INTEGER, TransitionTypeID INTEGER, TransitionGroupID INTEGER, IsPrimary INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionTypeGroup(ProjectID, TransitionTypeID, TransitionGroupID, IsPrimary) SELECT ProjectID, TransitionTypeID, TransitionGroupID, IsPrimary FROM ST_TransitionTypeGroup")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionTypeGroup")

        'Transition Multiplier Type
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionMultiplierType(TransitionMultiplierTypeID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionMultiplierType(TransitionMultiplierTypeID, ProjectID, Name) SELECT ST_TransitionMultiplierTypeID, ProjectID, Name FROM ST_TransitionMultiplierType")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionMultiplierType")

        'Age Type
        If (store.TableExists("ST_AgeType")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_AgeType(AgeTypeID INTEGER PRIMARY KEY AUTOINCREMENT, ProjectID INTEGER, Frequency INTEGER, MaximumAge INTEGER)")
            store.ExecuteNonQuery("INSERT INTO STSim_AgeType(ProjectID, Frequency, MaximumAge) SELECT ProjectID, Frequency, MaximumAge FROM ST_AgeType")
            store.ExecuteNonQuery("DROP TABLE ST_AgeType")

        End If

        'Age Group
        If (store.TableExists("ST_AgeGroup")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_AgeGroup(AgeGroupID INTEGER PRIMARY KEY AUTOINCREMENT, ProjectID INTEGER, MaximumAge INTEGER, ID INTEGER, Color TEXT)")
            store.ExecuteNonQuery("INSERT INTO STSim_AgeGroup(ProjectID, MaximumAge, ID, Color) SELECT ProjectID, MaximumAge, ID, Color FROM ST_AgeGroup")
            store.ExecuteNonQuery("DROP TABLE ST_AgeGroup")

        End If

        'Attribute Group
        store.ExecuteNonQuery("CREATE TABLE STSim_AttributeGroup(AttributeGroupID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_AttributeGroup(AttributeGroupID, ProjectID, Name, Description) SELECT ST_AttributeGroupID, ProjectID, Name, Description FROM ST_AttributeGroup")
        store.ExecuteNonQuery("DROP TABLE ST_AttributeGroup")

        'State Attribute Type
        store.ExecuteNonQuery("CREATE TABLE STSim_StateAttributeType(StateAttributeTypeID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, AttributeGroupID INTEGER, Units TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_StateAttributeType(StateAttributeTypeID, ProjectID, Name, AttributeGroupID, Units, Description) SELECT ST_StateAttributeTypeID, ProjectID, Name, AttributeGroupID, Units, Description FROM ST_StateAttributeType")
        store.ExecuteNonQuery("DROP TABLE ST_StateAttributeType")

        'Transition Attribute Type
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionAttributeType(TransitionAttributeTypeID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT, AttributeGroupID INTEGER, Units TEXT, Description TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionAttributeType(TransitionAttributeTypeID, ProjectID, Name, AttributeGroupID, Units, Description) SELECT ST_TransitionAttributeTypeID, ProjectID, Name, AttributeGroupID, Units, Description FROM ST_TransitionAttributeType")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionAttributeType")

        'Terminology
        store.ExecuteNonQuery("CREATE TABLE STSim_Terminology(TerminologyID INTEGER PRIMARY KEY AUTOINCREMENT, ProjectID INTEGER, AmountLabel TEXT, AmountUnits INTEGER, StateLabelX TEXT, StateLabelY TEXT, PrimaryStratumLabel TEXT, SecondaryStratumLabel TEXT, TimestepUnits TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_Terminology(ProjectID, AmountLabel, AmountUnits, StateLabelX, StateLabelY, PrimaryStratumLabel, SecondaryStratumLabel, TimestepUnits) SELECT ProjectID, AmountLabel, AmountUnits, StateLabelX, StateLabelY, PrimaryStratumLabel, SecondaryStratumLabel, TimestepUnits FROM ST_Terminology")
        store.ExecuteNonQuery("DROP TABLE ST_Terminology")

        'Patch prioritization
        store.ExecuteNonQuery("CREATE TABLE STSim_PatchPrioritization(PatchPrioritizationID INTEGER PRIMARY KEY, ProjectID INTEGER, Name TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_PatchPrioritization(PatchPrioritizationID, ProjectID, Name) SELECT ST_PatchPrioritizationID, ProjectID, Name FROM ST_PatchPrioritization")
        store.ExecuteNonQuery("DROP TABLE ST_PatchPrioritization")

        'Run Control
        store.ExecuteNonQuery("CREATE TABLE STSim_RunControl(RunControlID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, MinimumIteration INTEGER, MaximumIteration INTEGER, MinimumTimestep INTEGER, MaximumTimestep INTEGER, IsSpatial INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_RunControl(ScenarioID, MinimumIteration, MaximumIteration, MinimumTimestep, MaximumTimestep, IsSpatial) SELECT ScenarioID, MinimumIteration, MaximumIteration, MinimumTimestep, MaximumTimestep, RunSpatially FROM ST_RunControl")
        store.ExecuteNonQuery("DROP TABLE ST_RunControl")

        'Initial conditions non spatial
        store.ExecuteNonQuery("CREATE TABLE STSim_InitialConditionsNonSpatial(InitialConditionsNonSpatialID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, TotalAmount DOUBLE, NumCells INTEGER, CalcFromDist INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_InitialConditionsNonSpatial(ScenarioID, TotalAmount, NumCells, CalcFromDist) SELECT ScenarioID, TotalAmount, NumCells, CalcFromDist FROM ST_InitialConditionsNonSpatial")
        store.ExecuteNonQuery("DROP TABLE ST_InitialConditionsNonSpatial")

        'Initial conditions non spatial distribution
        store.ExecuteNonQuery("CREATE TABLE STSim_InitialConditionsNonSpatialDistribution(InitialConditionsNonSpatialDistributionID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, StateClassID INTEGER, AgeMin INTEGER, AgeMax INTEGER, RelativeAmount DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_InitialConditionsNonSpatialDistribution(ScenarioID, StratumID, SecondaryStratumID, StateClassID, AgeMin, AgeMax, RelativeAmount) SELECT ScenarioID, StratumID, SecondaryStratumID, StateClassID, AgeMin, AgeMax, RelativeAmount From ST_InitialConditionsNonSpatialDistribution")
        store.ExecuteNonQuery("DROP TABLE ST_InitialConditionsNonSpatialDistribution")

        'Initial conditions spatial
        store.ExecuteNonQuery("CREATE TABLE STSim_InitialConditionsSpatial(InitialConditionsSpatialID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, NumRows INTEGER, NumColumns INTEGER, NumCells INTEGER, CellSize SINGLE, CellSizeUnits TEXT, CellArea DOUBLE, CellAreaOverride INTEGER, XLLCorner SINGLE, YLLCorner SINGLE, SRS TEXT, StratumFileName TEXT, SecondaryStratumFileName TEXT, StateClassFileName TEXT, AgeFileName TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_InitialConditionsSpatial(ScenarioID, NumRows, NumColumns, NumCells, CellSize, CellSizeUnits, CellArea, CellAreaOverride, XLLCorner, YLLCorner, SRS, StratumFileName, SecondaryStratumFileName, StateClassFileName, AgeFileName) SELECT ScenarioID, NumRows, NumColumns, NumCells, CellSize, CellSizeUnits, CellArea, CellAreaOverride, XLLCorner, YLLCorner, SRS, StratumFileName, SecondaryStratumFileName, StateClassFileName, AgeFileName FROM ST_InitialConditionsSpatial")
        store.ExecuteNonQuery("DROP TABLE ST_InitialConditionsSpatial")

        'Deterministic Transition
        store.ExecuteNonQuery("CREATE TABLE STSim_DeterministicTransition(DeterministicTransitionID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, StratumIDSource INTEGER, StateClassIDSource INTEGER, StratumIDDest INTEGER, StateClassIDDest INTEGER, AgeMin INTEGER, AgeMax INTEGER, Location TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_DeterministicTransition(ScenarioID, StratumIDSource, StateClassIDSource, StratumIDDest, StateClassIDDest, AgeMin, AgeMax, Location) SELECT ScenarioID, StratumIDSource, StateClassIDSource, StratumIDDest, StateClassIDDest, AgeMin, AgeMax, Location FROM ST_DeterministicTransition")
        store.ExecuteNonQuery("DROP TABLE ST_DeterministicTransition")

        'Probabilistic Transition
        store.ExecuteNonQuery("CREATE TABLE STSim_Transition(TransitionID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, StratumIDSource INTEGER, StateClassIDSource INTEGER, StratumIDDest INTEGER, StateClassIDDest INTEGER, TransitionTypeID INTEGER, Probability DOUBLE, Proportion DOUBLE, AgeMin INTEGER, AgeMax INTEGER, AgeRelative INTEGER, AgeReset INTEGER, TSTMin INTEGER, TSTMax INTEGER, TSTRelative INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_Transition(ScenarioID, StratumIDSource, StateClassIDSource, StratumIDDest, StateClassIDDest, TransitionTypeID, Probability, Proportion, AgeMin, AgeMax, AgeRelative, AgeReset, TSTMin, TSTMax, TSTRelative) SELECT ScenarioID, StratumIDSource, StateClassIDSource, StratumIDDest, StateClassIDDest, TransitionTypeID, Probability, Proportion, AgeMin, AgeMax, AgeRelative, AgeReset, TSTMin, TSTMax, TSTRelative FROM ST_Transition")
        store.ExecuteNonQuery("DROP TABLE ST_Transition")

        'Output Options
        store.ExecuteNonQuery("CREATE TABLE STSim_OutputOptions(OutputOptionsID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, SummaryOutputSC INTEGER, SummaryOutputSCTimesteps INTEGER, SummaryOutputSCZeroValues INTEGER, SummaryOutputTR INTEGER, SummaryOutputTRTimesteps INTEGER, SummaryOutputTRIntervalMean INTEGER, SummaryOutputTRSC INTEGER, SummaryOutputTRSCTimesteps INTEGER, SummaryOutputSA INTEGER, SummaryOutputSATimesteps INTEGER, SummaryOutputTA INTEGER, SummaryOutputTATimesteps INTEGER, RasterOutputSC INTEGER, RasterOutputSCTimesteps INTEGER, RasterOutputTR INTEGER, RasterOutputTRTimesteps INTEGER, RasterOutputAge INTEGER, RasterOutputAgeTimesteps INTEGER, RasterOutputTST INTEGER, RasterOutputTSTTimesteps INTEGER, RasterOutputST INTEGER, RasterOutputSTTimesteps INTEGER, RasterOutputSA INTEGER, RasterOutputSATimesteps INTEGER, RasterOutputTA INTEGER, RasterOutputTATimesteps INTEGER, RasterOutputAATP INTEGER, RasterOutputAATPTimesteps INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_OutputOptions(ScenarioID, SummaryOutputSC, SummaryOutputSCTimesteps, SummaryOutputSCZeroValues, SummaryOutputTR, SummaryOutputTRTimesteps, SummaryOutputTRIntervalMean, SummaryOutputTRSC, SummaryOutputTRSCTimesteps, SummaryOutputSA, SummaryOutputSATimesteps, SummaryOutputTA, SummaryOutputTATimesteps, RasterOutputSC, RasterOutputSCTimesteps, RasterOutputTR, RasterOutputTRTimesteps, RasterOutputAge, RasterOutputAgeTimesteps, RasterOutputTST, RasterOutputTSTTimesteps, RasterOutputST, RasterOutputSTTimesteps, RasterOutputSA, RasterOutputSATimesteps, RasterOutputTA, RasterOutputTATimesteps, RasterOutputAATP, RasterOutputAATPTimesteps) SELECT ScenarioID, SummaryOutputSC, SummaryOutputSCTimesteps, SummaryOutputSCZeroValues, SummaryOutputTR, SummaryOutputTRTimesteps, SummaryOutputTRIntervalMean, SummaryOutputTRSC, SummaryOutputTRSCTimesteps, SummaryOutputSA, SummaryOutputSATimesteps, SummaryOutputTA, SummaryOutputTATimesteps, RasterOutputSC, RasterOutputSCTimesteps, RasterOutputTR, RasterOutputTRTimesteps, RasterOutputAge, RasterOutputAgeTimesteps, RasterOutputTST, RasterOutputTSTTimesteps, RasterOutputST, RasterOutputSTTimesteps, RasterOutputSA, RasterOutputSATimesteps, RasterOutputTA, RasterOutputTATimesteps, RasterOutputAATP, RasterOutputAATPTimesteps FROM ST_OutputOptions")
        store.ExecuteNonQuery("DROP TABLE ST_OutputOptions")

        'Transition Targets
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionTarget(TransitionTargetID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionGroupID INTEGER, Amount DOUBLE, DistributionType INTEGER, DistributionSD DOUBLE, DistributionMin DOUBLE, DistributionMax DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionTarget(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax FROM ST_TransitionTarget")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionTarget")

        'Transition multiplier value
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionMultiplierValue(TransitionMultiplierValueID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, StateClassID INTEGER, TransitionGroupID INTEGER, TransitionMultiplierTypeID INTEGER, Amount DOUBLE, DistributionType INTEGER, DistributionSD DOUBLE, DistributionMin DOUBLE, DistributionMax DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionMultiplierValue(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, StateClassID, TransitionGroupID, TransitionMultiplierTypeID, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, StateClassID, TransitionGroupID, TransitionMultiplierTypeID, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax FROM ST_TransitionMultiplierValue")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionMultiplierValue")

        'Transition size distribution
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionSizeDistribution(TransitionSizeDistributionID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, TransitionGroupID INTEGER, MaximumArea DOUBLE, RelativeAmount DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionSizeDistribution(ScenarioID, Iteration, Timestep, StratumID, TransitionGroupID, MaximumArea, RelativeAmount) SELECT ScenarioID, Iteration, Timestep, StratumID, TransitionGroupID, MaximumArea, RelativeAmount FROM ST_TransitionSizeDistribution")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionSizeDistribution")

        'Transition spread distribution
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionSpreadDistribution(TransitionSpreadDistributionID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, TransitionGroupID INTEGER, StateClassID INTEGER, MaximumDistance DOUBLE,RelativeAmount DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionSpreadDistribution(ScenarioID, Iteration, Timestep, StratumID, TransitionGroupID, StateClassID, MaximumDistance, RelativeAmount) SELECT ScenarioID, Iteration, Timestep, StratumID, TransitionGroupID, StateClassID, MaximumDistance, RelativeAmount FROM ST_TransitionSpreadDistribution")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionSpreadDistribution")

        'Transition patch prioritization
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionPatchPrioritization(TransitionPatchPrioritizationID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, TransitionGroupID INTEGER, PatchPrioritizationID INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionPatchPrioritization(ScenarioID, Iteration, Timestep, TransitionGroupID, PatchPrioritizationID) SELECT ScenarioID, Iteration, Timestep, TransitionGroupID, PatchPrioritization FROM ST_TransitionPatchPrioritization")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionPatchPrioritization")

        'Spatial multipliers
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionSpatialMultiplier(TransitionSpatialMultiplierID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, TransitionGroupID INTEGER, MultiplierFileName TEXT)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionSpatialMultiplier(ScenarioID, Iteration, Timestep, TransitionGroupID, MultiplierFileName) SELECT ScenarioID, Iteration, Timestep, TransitionGroupID, MultiplierFilename FROM ST_TransitionSpatialMultiplier")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionSpatialMultiplier")

        'Direction multipliers
        If (store.TableExists("ST_TransitionDirectionMultiplier")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_TransitionDirectionMultiplier(TransitionDirectionMultiplierID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionGroupID INTEGER, CardinalDirection INTEGER, Amount DOUBLE, DistributionType INTEGER, DistributionSD DOUBLE, DistributionMin DOUBLE, DistributionMax DOUBLE)")
            store.ExecuteNonQuery("INSERT INTO STSim_TransitionDirectionMultiplier(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, CardinalDirection, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, CardinalDirection, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax FROM ST_TransitionDirectionMultiplier")
            store.ExecuteNonQuery("DROP TABLE ST_TransitionDirectionMultiplier")

        End If

        'Digital elevation model
        If (store.TableExists("ST_DigitalElevationModel")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_DigitalElevationModel(DigitalElevationModelID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, DigitalElevationModelFileName TEXT)")
            store.ExecuteNonQuery("INSERT INTO STSim_DigitalElevationModel(ScenarioID, DigitalElevationModelFileName) SELECT ScenarioID, DigitalElevationModelFilename FROM ST_DigitalElevationModel")
            store.ExecuteNonQuery("DROP TABLE ST_DigitalElevationModel")

        End If

        'Slope multipliers
        If (store.TableExists("ST_TransitionSlopeMultiplier")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_TransitionSlopeMultiplier(TransitionSlopeMultiplierID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionGroupID INTEGER, Slope INTEGER, Amount DOUBLE, DistributionType INTEGER, DistributionSD DOUBLE, DistributionMin DOUBLE, DistributionMax DOUBLE)")
            store.ExecuteNonQuery("INSERT INTO STSim_TransitionSlopeMultiplier(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, Slope, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, Slope, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax FROM ST_TransitionSlopeMultiplier")
            store.ExecuteNonQuery("DROP TABLE ST_TransitionSlopeMultiplier")

        End If

        'Transition adjacency setting
        If (store.TableExists("ST_TransitionAdjacencySetting")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_TransitionAdjacencySetting(TransitionAdjacencySettingID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, TransitionGroupID INTEGER, StateAttributeTypeID INTEGER, NeighborhoodRadius DOUBLE, UpdateFrequency INTEGER)")
            store.ExecuteNonQuery("INSERT INTO STSim_TransitionAdjacencySetting(ScenarioID, TransitionGroupID, StateAttributeTypeID, NeighborhoodRadius, UpdateFrequency) SELECT ScenarioID, TransitionGroupID, StateAttributeTypeID, NeighborhoodRadius, UpdateFrequency FROM ST_TransitionAdjacencySetting")
            store.ExecuteNonQuery("DROP TABLE ST_TransitionAdjacencySetting")

        End If

        'Transition adjacency multiplier
        If (store.TableExists("ST_TransitionAdjacencyMultiplier")) Then

            store.ExecuteNonQuery("CREATE TABLE STSim_TransitionAdjacencyMultiplier(TransitionAdjacencyMultiplierID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionGroupID INTEGER, AttributeValue DOUBLE, Amount DOUBLE, DistributionType INTEGER, DistributionSD DOUBLE, DistributionMin DOUBLE, DistributionMax DOUBLE)")
            store.ExecuteNonQuery("INSERT INTO STSim_TransitionAdjacencyMultiplier(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, AttributeValue, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, AttributeValue, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax FROM ST_TransitionAdjacencyMultiplier")
            store.ExecuteNonQuery("DROP TABLE ST_TransitionAdjacencyMultiplier")

        End If

        'TST Group
        store.ExecuteNonQuery("CREATE TABLE STSim_TimeSinceTransitionGroup(TimeSinceTransitionGroupID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionTypeID INTEGER, TransitionGroupID INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_TimeSinceTransitionGroup(ScenarioID, StratumID, SecondaryStratumID, TransitionTypeID, TransitionGroupID) SELECT ScenarioID, StratumID, SecondaryStratumID, TransitionTypeID, TransitionGroupID FROM ST_TimeSinceTransitionGroup")
        store.ExecuteNonQuery("DROP TABLE ST_TimeSinceTransitionGroup")

        'TST Randomize
        store.ExecuteNonQuery("CREATE TABLE STSim_TimeSinceTransitionRandomize(TimeSinceTransitionRandomizeID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionGroupID INTEGER, StateClassID INTEGER, MinInitialTST INTEGER, MaxInitialTST INTEGER)")
        store.ExecuteNonQuery("INSERT INTO STSim_TimeSinceTransitionRandomize(ScenarioID, StratumID, SecondaryStratumID, TransitionGroupID, StateClassID, MinInitialTST, MaxInitialTST) SELECT ScenarioID, StratumID, SecondaryStratumID, TransitionGroupID, StateClassID, MinInitialTST, MaxInitialTST FROM ST_TimeSinceTransitionRandomize")
        store.ExecuteNonQuery("DROP TABLE ST_TimeSinceTransitionRandomize")

        'State Attribute Value
        store.ExecuteNonQuery("CREATE TABLE STSim_StateAttributeValue(StateAttributeValueID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, StateClassID INTEGER, StateAttributeTypeID INTEGER, AgeMin INTEGER, AgeMax INTEGER, Value DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_StateAttributeValue(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, StateClassID, StateAttributeTypeID, AgeMin, AgeMax, Value) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, StateClassID, StateAttributeTypeID, AgeMin, AgeMax, Value FROM ST_StateAttributeValue")
        store.ExecuteNonQuery("DROP TABLE ST_StateAttributeValue")

        'Transition Attribute Value
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionAttributeValue(TransitionAttributeValueID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionGroupID INTEGER, StateClassID INTEGER, TransitionAttributeTypeID INTEGER, AgeMin INTEGER, AgeMax INTEGER, Value DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionAttributeValue(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, StateClassID, TransitionAttributeTypeID, AgeMin, AgeMax, Value) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionGroupID, StateClassID, TransitionAttributeTypeID, AgeMin, AgeMax, Value FROM ST_TransitionAttributeValue")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionAttributeValue")

        'Transition Attribute Target
        store.ExecuteNonQuery("CREATE TABLE STSim_TransitionAttributeTarget(TransitionAttributeTargetID INTEGER PRIMARY KEY AUTOINCREMENT, ScenarioID INTEGER, Iteration INTEGER, Timestep INTEGER, StratumID INTEGER, SecondaryStratumID INTEGER, TransitionAttributeTypeID INTEGER, Amount DOUBLE, DistributionType INTEGER, DistributionSD DOUBLE, DistributionMin DOUBLE, DistributionMax DOUBLE)")
        store.ExecuteNonQuery("INSERT INTO STSim_TransitionAttributeTarget(ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionAttributeTypeID, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax) SELECT ScenarioID, Iteration, Timestep, StratumID, SecondaryStratumID, TransitionAttributeTypeID, Amount, DistributionType, DistributionSD, DistributionMin, DistributionMax FROM ST_TransitionAttributeTarget")
        store.ExecuteNonQuery("DROP TABLE ST_TransitionAttributeTarget")

        'Processing
        If (store.TableExists("ST_Processing")) Then
            store.ExecuteNonQuery("DROP TABLE ST_Processing")
        End If

        'Output tables - these just need to be renamed

        If (store.TableExists("ST_OutputStratumAmount")) Then
            store.ExecuteNonQuery("ALTER TABLE ST_OutputStratumAmount RENAME TO STSim_OutputStratumAmount")
        End If

        store.ExecuteNonQuery("ALTER TABLE ST_OutputStateAttribute RENAME TO STSim_OutputStateAttribute")
        store.ExecuteNonQuery("ALTER TABLE ST_OutputStratumState RENAME TO STSim_OutputStratumState")
        store.ExecuteNonQuery("ALTER TABLE ST_OutputStratumTransition RENAME TO STSim_OutputStratumTransition")
        store.ExecuteNonQuery("ALTER TABLE ST_OutputStratumTransitionState RENAME TO STSim_OutputStratumTransitionState")
        store.ExecuteNonQuery("ALTER TABLE ST_OutputTransitionAttribute RENAME TO STSim_OutputTransitionAttribute")

    End Sub

    ''' <summary>
    ''' Renames input directories for SSIM version 1
    ''' </summary>
    ''' <param name="store"></param>
    ''' <remarks></remarks>
    Private Shared Sub RenameInputDirectories_SSIM_V_1(ByVal store As DataStore)

        Dim base As String = GetCurrentInputFolderBase(store)

        If (Not Directory.Exists(base)) Then
            Return
        End If

        For Each d1 As String In Directory.GetDirectories(base)

            For Each d2 As String In Directory.GetDirectories(d1)

                Dim di As New DirectoryInfo(d2)

                If (di.Name = "ST_InitialConditionsSpatial") Then
                    Directory.Move(d2, Path.Combine(d1, "STSim_InitialConditionsSpatial"))
                ElseIf (di.Name = "ST_DigitalElevationModel") Then
                    Directory.Move(d2, Path.Combine(d1, "STSim_DigitalElevationModel"))
                ElseIf (di.Name = "ST_TransitionSpatialMultiplier") Then
                    Directory.Move(d2, Path.Combine(d1, "STSim_TransitionSpatialMultiplier"))
                End If

            Next

        Next

    End Sub

    Private Shared Function GetCurrentInputFolderBase(ByVal store As DataStore) As String

        Const FOLDER_NAME As String = "InputFolderName"

        Dim dt As DataTable = Nothing

        'Unfortunately, at some point, the core moves the columns from the Folders table
        'to the Files table.  At this time in the developement of SyncroSim, though, we do
        'not have a perfectly stable 1.0 API (and this function is only called to update
        'very old 'legacy' databases) so we are just going to check for the existence of
        'the table we need...

        If (store.TableExists("SSim_Folders")) Then
            dt = store.CreateDataTable("SSim_Folders")
        Else
            dt = store.CreateDataTable("SSim_Files")
        End If

        Dim dr As DataRow = dt.Rows(0)

        Debug.Assert(dt.Rows.Count = 1 Or dt.Rows.Count = 0)

        Dim p As String = Nothing

        If (dr IsNot Nothing) AndAlso (dr(FOLDER_NAME) IsNot DBNull.Value) Then
            p = CStr(dr(FOLDER_NAME))
        Else
            p = store.DataStoreConnection.ConnectionString & ".input"
        End If

        Return p

    End Function

    Private Shared Function GetVersionTableValue(ByVal store As DataStore, ByVal tableName As String) As Integer

        Dim dt As DataTable = store.CreateDataTable(tableName)

        If (dt.Rows.Count <> 1) Then

            ExceptionUtils.ThrowArgumentException(
                "The version table '{0}' is corrupt.  Cannot continue.", tableName)

        End If

        Return CInt(dt.Rows(0)("Version"))

    End Function

End Class
