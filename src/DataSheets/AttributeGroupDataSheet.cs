﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System;
using System.Data;
using SyncroSim.Core;
using System.Globalization;
using System.Collections.Generic;

namespace SyncroSim.STSim
{
    internal class AttributeGroupDataSheet : DataSheet
    {
        public override void DeleteRows(IEnumerable<DataRow> rows)
        {
            DataSheet dssa = this.Project.GetDataSheet(Strings.DATASHEET_STATE_ATTRIBUTE_TYPE_NAME);
            DataSheet dsta = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_ATTRIBUTE_TYPE_NAME);

            dssa.BeginModifyRows();
            dsta.BeginModifyRows();

            foreach (DataRow ParentRow in rows)
            {
                int GroupId = Convert.ToInt32(ParentRow[this.PrimaryKeyColumn.Name], CultureInfo.InvariantCulture);

                FixupChildReferences(dssa, GroupId);
                FixupChildReferences(dsta, GroupId);
            }

            dssa.EndModifyRows();
            dsta.EndModifyRows();

            base.DeleteRows(rows);
        }

        private static void FixupChildReferences(DataSheet dataSheet, int groupId)
        {
            DataTable dt = dataSheet.GetData();

            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (dr[Strings.DATASHEET_ATTRIBUTE_GROUP_ID_COLUMN_NAME] == DBNull.Value)
                {
                    continue;
                }

                int id = Convert.ToInt32(dr[Strings.DATASHEET_ATTRIBUTE_GROUP_ID_COLUMN_NAME], CultureInfo.InvariantCulture);

                if (id == groupId)
                {
                    dr[Strings.DATASHEET_ATTRIBUTE_GROUP_ID_COLUMN_NAME] = DBNull.Value;
                }
            }
        }
    }
}
