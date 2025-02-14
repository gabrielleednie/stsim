﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System;
using System.Data;
using System.Globalization;
using System.Collections.Generic;
using SyncroSim.Core;

namespace SyncroSim.STSim
{
    internal class TransitionGroupDataSheet : DataSheet
    {
        public override void Validate(object proposedValue, string columnName)
        {
            base.Validate(proposedValue, columnName);

            if (columnName == Strings.DATASHEET_NAME_COLUMN_NAME)
            {
                ValidateName(Convert.ToString(proposedValue, CultureInfo.InvariantCulture));
            }
        }

        public override void Validate(DataRow proposedRow, DataTransferMethod transferMethod)
        {
            base.Validate(proposedRow, transferMethod);
            ValidateName(Convert.ToString(proposedRow[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture));
        }

        public override void Validate(DataTable proposedData, DataTransferMethod transferMethod)
        {
            base.Validate(proposedData, transferMethod);

            foreach (DataRow dr in proposedData.Rows)
            {
                if (!DataTableUtilities.GetDataBool(dr, Strings.IS_AUTO_COLUMN_NAME))
                {
                    ValidateName(Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture));
                }
            }
        }

        public override void DeleteRows(IEnumerable<DataRow> rows)
        {
            List<DataRow> l = new List<DataRow>();

            foreach (DataRow dr in rows)
            {
                if (!DataTableUtilities.GetDataBool(dr, Strings.IS_AUTO_COLUMN_NAME))
                {
                    l.Add(dr);
                }
            }

            if (l.Count > 0)
            {
                base.DeleteRows(l);
            }
        }

        internal void DeleteAutoGeneratedRows(IEnumerable<DataRow> rows)
        {
            //Normally when deleting rows we don't want to delete "Auto-Generated" records.
            //These should only be deleted when you delete their associated type.  So, we
            //override DeleteRows() above to avoid this.  However, we also need a way to delete
            //Auto-Generated groups when told to do so by the Transition Type property
            //which is what this function does.  However, we can't just delete the rows.
            //We need to call the base class DeleteRows() function to ensure that we delete
            //any rows in "downstream" datasheets which reference these groups.

            base.DeleteRows(rows);
        }

        private static void ValidateName(string name)
        {
            if (name.EndsWith(Strings.AUTO_COLUMN_SUFFIX, StringComparison.Ordinal))
            {
                string msg = string.Format(CultureInfo.InvariantCulture, 
                    "The transition group name cannot have the suffix: '{0}'.", 
                    Strings.AUTO_COLUMN_SUFFIX);

                throw new DataException(msg);
            }
        }
    }
}
