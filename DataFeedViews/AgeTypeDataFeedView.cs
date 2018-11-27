﻿// A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2018 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.

using System.Windows.Forms;

namespace SyncroSim.STSim
{
    internal partial class AgeTypeDataFeedView
    {
        public AgeTypeDataFeedView()
        {
            InitializeComponent();
        }

        public override void LoadDataFeed(Core.DataFeed dataFeed)
        {
            base.LoadDataFeed(dataFeed);

            this.SetTextBoxBinding(this.TextBoxFrequency, Strings.DATASHEET_AGE_TYPE_FREQUENCY_COLUMN_NAME);
            this.SetTextBoxBinding(this.TextBoxMaximum, Strings.DATASHEET_AGE_TYPE_MAXIMUM_COLUMN_NAME);

            this.RefreshBoundControls();
            this.AddStandardCommands();
        }

        protected override bool OnBoundTextBoxValidating(TextBox textBox, string columnName, string proposedValue)
        {
            if (!base.OnBoundTextBoxValidating(textBox, columnName, proposedValue))
            {
                return false;
            }

            if (!AgeUtilities.HasAgeClassUpdateTag(this.Project))
            {
                if (MessageBox.Show(MessageStrings.PROMPT_AGE_TYPE_CHANGE, "Age Type", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
