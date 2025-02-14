﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System;
using System.Data;
using SyncroSim.Core;
using System.Diagnostics;
using System.Globalization;

namespace SyncroSim.STSim
{
    public partial class STSimTransformer
    {
        private void ChangeCellAgeForProbabilisticTransition(Cell simulationCell, int iteration, int timestep, Transition transition)
        {
            simulationCell.Age = this.DetermineTargetAgeProbabilistic(
                simulationCell.Age, 
                simulationCell.StratumId, 
                simulationCell.StateClassId, 
                iteration, 
                timestep, 
                transition);
        }

        public int DetermineTargetAgeProbabilistic(int currentCellAge, int? destinationStratumId, int destinationStateClassId, int iteration, int timestep, Transition transition)
        {
            return this.InternalDetermineTargetAgeProbabilistic(
                currentCellAge, 
                destinationStratumId, 
                destinationStateClassId, 
                iteration, 
                timestep, 
                transition);
        }

        private int InternalDetermineTargetAgeProbabilistic(int currentCellAge, int? destinationStratumId, int destinationStateClassId, int iteration, int timestep, Transition transition)
        {
            DeterministicTransition dt = this.GetDeterministicTransition(destinationStratumId, destinationStateClassId, iteration, timestep);

            int AgeMin = 0;
            int AgeMax = 0;

            if (dt == null)
            {
                AgeMin = 0;
                AgeMax = int.MaxValue;
            }
            else
            {
                AgeMin = dt.AgeMinimum;
                AgeMax = dt.AgeMaximum;
            }

            int NewAge = 0;

            if (!(transition.AgeReset))
            {
                int TargetSimCellAge = (currentCellAge + transition.AgeRelative);

                if (TargetSimCellAge < AgeMin)
                {
                    NewAge = AgeMin;
                }
                else if (TargetSimCellAge > AgeMax)
                {
                    NewAge = AgeMax;
                }
                else
                {
                    NewAge = TargetSimCellAge;
                }
            }
            else
            {
                int TargetSimCellAge = Math.Max(AgeMin, AgeMin + transition.AgeRelative);

                if (TargetSimCellAge > AgeMax)
                {
                    TargetSimCellAge = AgeMax;
                }

                NewAge = TargetSimCellAge;
            }

            if (NewAge < 0)
            {
                NewAge = 0;
            }

            return NewAge;
        }

        private void InitializeCellAge(Cell simulationCell, int stratumId, int stateClassId, int minimumAge, int maximumAge, int iteration, int timestep)
        {
            DeterministicTransition dt = null;

            if (this.m_DeterministicTransitionMap != null)
            {
                dt = this.GetDeterministicTransition(stratumId, stateClassId, iteration, timestep);
            }

            Debug.Assert(minimumAge != int.MaxValue);

            if (dt == null)
            {
                if (maximumAge == int.MaxValue)
                {
                    simulationCell.Age = minimumAge;
                }
                else
                {
                    simulationCell.Age = this.m_RandomGenerator.GetNextInteger(minimumAge, maximumAge + 1);
                }
            }
            else
            {
                int AgeMinOut = 0;
                int AgeMaxOut = 0;

                GetAgeMinMax(dt, minimumAge, maximumAge, ref AgeMinOut, ref AgeMaxOut);

                if (AgeMaxOut == int.MaxValue)
                {
                    simulationCell.Age = AgeMinOut;
                }
                else
                {
                    simulationCell.Age = this.m_RandomGenerator.GetNextInteger(AgeMinOut, AgeMaxOut + 1);
                }
            }
        }

        /// <summary>
        /// Initializes the age reporting helpers
        /// </summary>
        /// <remarks></remarks>
        private void InitializeAgeReportingHelpers()
        {
            Debug.Assert(this.m_AgeReportingHelperSC == null);
            Debug.Assert(this.m_AgeReportingHelperTR == null);
            Debug.Assert(this.m_AgeReportingHelperSA == null);
            Debug.Assert(this.m_AgeReportingHelperTA == null);

            this.m_AgeReportingHelperSC = new ClassBinHelper(false, 0, 0);
            this.m_AgeReportingHelperTR = new ClassBinHelper(false, 0, 0);
            this.m_AgeReportingHelperSA = new ClassBinHelper(false, 0, 0);
            this.m_AgeReportingHelperTA = new ClassBinHelper(false, 0, 0);

            //If not reporting any ages then all age helpers are disabled
            //-----------------------------------------------------------

            if (
                !this.m_SummaryStateClassOutputAges && 
                !this.m_SummaryTransitionOutputAges && 
                !this.m_SummaryStateAttributeOutputAges && 
                !this.m_SummaryTransitionAttributeOutputAges)
            {
                return;
            }

            //If ages are not configured then all age helpers are disabled
            //------------------------------------------------------------

            DataRow dr = this.Project.GetDataSheet(Strings.DATASHEET_AGE_TYPE_NAME).GetDataRow();

            if (dr == null)
            {
                return;
            }

            //If the age configuration is invalid then all age helpers are disabled
            //---------------------------------------------------------------------

            if (dr[Strings.DATASHEET_AGE_TYPE_FREQUENCY_COLUMN_NAME] != DBNull.Value)
            {
                if (dr[Strings.DATASHEET_AGE_TYPE_MAXIMUM_COLUMN_NAME] == DBNull.Value)
                {
                    this.RecordStatus(StatusType.Warning, 
                        "Age reporting freqency set without age reporting maximum.  Not reporting ages.");

                    return;
                }
            }

            if (dr[Strings.DATASHEET_AGE_TYPE_MAXIMUM_COLUMN_NAME] != DBNull.Value)
            {
                if (dr[Strings.DATASHEET_AGE_TYPE_FREQUENCY_COLUMN_NAME] == DBNull.Value)
                {
                    this.RecordStatus(StatusType.Warning, 
                        "Age reporting maximum set without age reporting frequency.  Not reporting ages.");

                    return;
                }
            }

            int f = Convert.ToInt32(dr[Strings.DATASHEET_AGE_TYPE_FREQUENCY_COLUMN_NAME], CultureInfo.InvariantCulture);
            int m = Convert.ToInt32(dr[Strings.DATASHEET_AGE_TYPE_MAXIMUM_COLUMN_NAME], CultureInfo.InvariantCulture);

            if (m < f)
            {
                this.RecordStatus(StatusType.Warning, 
                    "Age reporting maximum is less than age reporting frequency.  Not reporting ages.");

                return;
            }

            //If the user has requested age reporting for state attributes but has not enabled them for 
            //state classes then we need a warning because the attribute age output cannot be generated
            //without also having it for state classes.

            if (this.m_SummaryStateAttributeOutputAges && !this.m_SummaryStateClassOutputAges)
            {
                this.RecordStatus(StatusType.Warning, 
                    "Age reporting for State Attributes requires age reporting for State Classes.  Not generating ages for State Attributes.");
            }

            //Only enable an age helper if that type of output is enabled

            if (this.m_SummaryStateClassOutputAges)
            {
                this.m_AgeReportingHelperSC = new ClassBinHelper(true, f, m);
            }

            if (this.m_SummaryTransitionOutputAges)
            {
                this.m_AgeReportingHelperTR = new ClassBinHelper(true, f, m);
            }

            if (this.m_SummaryStateAttributeOutputAges)
            {
                this.m_AgeReportingHelperSA = new ClassBinHelper(true, f, m);
            }

            if (this.m_SummaryTransitionAttributeOutputAges)
            {
                this.m_AgeReportingHelperTA = new ClassBinHelper(true, f, m);
            }           
        }

        /// <summary>
        /// Gets the age min and age max for the specified deterministic transition and age values
        /// </summary>
        /// <param name="dt">The deterministic transition</param>
        /// <param name="ageMinIn">The minimum age IN value</param>
        /// <param name="ageMaxIn">The maximum age IN value</param>
        /// <param name="ageMinOut">The minimum age OUT value</param>
        /// <param name="ageMaxOut">The maximum age OUT value</param>
        /// <remarks></remarks>
        internal static void GetAgeMinMax(DeterministicTransition dt, int ageMinIn, int ageMaxIn, ref int ageMinOut, ref int ageMaxOut)
        {
            //Normalize
            int dtagemin = Math.Min(dt.AgeMinimum, dt.AgeMaximum);
            int dtagemax = Math.Max(dt.AgeMinimum, dt.AgeMaximum);

            //This should already be normalized
            Debug.Assert(ageMinIn <= ageMaxIn);

            //If any age value is outside of the dt value, set the age value equal to the nearest dt value.
            //For example, if ageMinIn > dt.agemax the set ageMinIn = dt.agemax.

            if (ageMinIn < dtagemin)
            {
                ageMinIn = dtagemin;
            }

            if (ageMaxIn < dtagemin)
            {
                ageMaxIn = dtagemin;
            }

            if (ageMinIn > dtagemax)
            {
                ageMinIn = dtagemax;
            }

            if (ageMaxIn > dtagemax)
            {
                ageMaxIn = dtagemax;
            }

            ageMinOut = ageMinIn;
            ageMaxOut = ageMaxIn;

            Debug.Assert(ageMinOut <= ageMaxOut);
        }
    }
}
