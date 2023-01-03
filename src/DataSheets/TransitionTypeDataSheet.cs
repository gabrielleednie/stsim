﻿// stsim: A SyncroSim Package for developing state-and-transition simulation models using ST-Sim.
// Copyright © 2007-2023 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.

using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using SyncroSim.Core;

namespace SyncroSim.STSim
{
    internal class TransitionTypeDataSheet : DataSheet
    {
        private DataTable m_TTData;
        private DataSheet m_TGDataSheet;
        private DataTable m_TGData;
        private DataSheet m_TTGDataSheet;
        private DataTable m_TTGData;
        private Dictionary<int, string> m_PrevNames = new Dictionary<int, string>();
        private bool m_InTTGRowsAdded;

        protected override void OnDataFeedsRefreshed()
        {
            base.OnDataFeedsRefreshed();

            this.m_TTData = this.GetData();

            this.m_TGDataSheet = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_GROUP_NAME);
            this.m_TGData = this.m_TGDataSheet.GetData();

            this.m_TTGDataSheet = this.Project.GetDataSheet(Strings.DATASHEET_TRANSITION_TYPE_GROUP_NAME);
            this.m_TTGData = this.m_TTGDataSheet.GetData();

            this.m_TTGDataSheet.RowsAdded += this.OnTTGRowsAdded;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (!this.IsDisposed))
            {
                if (this.m_TTGDataSheet != null)
                {
                    this.m_TTGDataSheet.RowsAdded -= this.OnTTGRowsAdded;
                }
            }

            base.Dispose(disposing);
        }

        private void OnTTGRowsAdded(object sender, DataSheetRowEventArgs e)
        {
            if (this.m_InTTGRowsAdded)
            {
                return;
            }

            this.m_InTTGRowsAdded = true;
            Dictionary<string, bool> MissingTypeGroups = new Dictionary<string, bool>();
            Dictionary<string, bool> ExistingTypeGroups = CreateTTG_ID_Dictionary();
            string IdColName = this.PrimaryKeyColumn.Name;

            using (DataStore store = this.Library.CreateDataStore())
            {
                foreach (DataRow dr in this.m_TTData.Rows)
                {
                    if (dr.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    string AutoGroupName = GetAutoGeneratedGroupName(dr);

                    if (!this.m_TGDataSheet.ValidationTable.ContainsValue(AutoGroupName))
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    int ttid = Convert.ToInt32(dr[IdColName], CultureInfo.InvariantCulture);
                    var tgid = this.m_TGDataSheet.ValidationTable.GetValue(AutoGroupName);
                    var Key = Create_TT_TG_Key(ttid, tgid);

                    if (ExistingTypeGroups.ContainsKey(Key))
                    {
                        continue;
                    }

                    Debug.Assert(!MissingTypeGroups.ContainsKey(Key));

                    if (!MissingTypeGroups.ContainsKey(Key))
                    {
                        MissingTypeGroups.Add(Key, true);
                    }
                }

                if (MissingTypeGroups.Count > 0)
                {
                    this.m_TTGDataSheet.BeginAddRows();

                    foreach (string Key in MissingTypeGroups.Keys)
                    {
                        string[] s = Key.Split('-');

                        this.CreateTransitionTypeGroup(
                            int.Parse(s[0], CultureInfo.InvariantCulture), 
                            int.Parse(s[1], CultureInfo.InvariantCulture));
                    }

                    this.m_TTGDataSheet.EndAddRows();
                }
            }

            this.m_InTTGRowsAdded = false;
        }

        protected override void OnRowsAdded(object sender, DataSheetRowEventArgs e)
        {
            Dictionary<int, string> AutoGroups = new Dictionary<int, string>();
            Dictionary<int, int> AutoTypeGroups = new Dictionary<int, int>();
            string IdColName = this.PrimaryKeyColumn.Name;

            using (DataStore store = this.Library.CreateDataStore())
            {
                foreach (DataRow dr in this.m_TTData.Rows)
                {
                    if (dr.RowState != DataRowState.Added)
                    {
                        continue;
                    }

                    int ThisId = Convert.ToInt32(dr[IdColName], CultureInfo.InvariantCulture);
                    string AutoGroupName = GetAutoGeneratedGroupName(dr);

                    if (this.m_TGDataSheet.ValidationTable.ContainsValue(AutoGroupName))
                    {
                        continue;
                    }

                    int AutoGroupId = Library.GetNextSequenceId(store);

                    AutoGroups.Add(AutoGroupId, AutoGroupName);
                    AutoTypeGroups.Add(ThisId, AutoGroupId);
                }
            }

            Debug.Assert(AutoTypeGroups.Count == AutoGroups.Count);

            if (AutoGroups.Count > 0)
            {
                this.m_TGDataSheet.BeginAddRows();
                this.m_TTGDataSheet.BeginAddRows();

                foreach (int gid in AutoGroups.Keys)
                {
                    this.CreateTransitionGroup(gid, AutoGroups[gid]);
                }

                using (DataStore store = this.Library.CreateDataStore())
                {
                    foreach (int tid in AutoTypeGroups.Keys)
                    {
                        this.CreateTransitionTypeGroup(tid, AutoTypeGroups[tid]);
                    }
                }

                this.m_TGDataSheet.EndAddRows();
                this.m_TTGDataSheet.EndAddRows();
            }

            base.OnRowsAdded(sender, e);
        }

        public override void DeleteRows(IEnumerable<DataRow> rows)
        {
            List<DataRow> DeleteRows = new List<DataRow>();
            Dictionary<string, DataRow> GroupRows = this.CreateTGRowDictionary();

            foreach (DataRow dr in rows)
            {
                string AutoGroupName = GetAutoGeneratedGroupName(dr);

                if (!GroupRows.ContainsKey(AutoGroupName))
                {
                    continue;
                }

                Debug.Assert(DataTableUtilities.GetDataBool(GroupRows[AutoGroupName], Strings.IS_AUTO_COLUMN_NAME));
                DeleteRows.Add(GroupRows[AutoGroupName]);
            }

            if (DeleteRows.Count > 0)
            {
                ((TransitionGroupDataSheet)this.m_TGDataSheet).DeleteAutoGeneratedRows(DeleteRows);
            }

            base.DeleteRows(rows);
        }

        protected override void OnModifyingRows(object sender, DataSheetRowEventArgs e)
        {
            this.m_PrevNames.Clear();

            string IdColName = this.PrimaryKeyColumn.Name;

            foreach (DataRow dr in this.m_TTData.Rows)
            {
                if (dr.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                int TypeId = Convert.ToInt32(dr[IdColName], CultureInfo.InvariantCulture);
                string TypeName = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME, DataRowVersion.Current], CultureInfo.InvariantCulture);

                this.m_PrevNames.Add(TypeId, TypeName);
            }

            base.OnModifyingRows(sender, e);
        }

        protected override void OnRowsModified(object sender, DataSheetRowEventArgs e)
        {
            List<DataRow> ModifyRows = new List<DataRow>();
            string IdColName = this.PrimaryKeyColumn.Name;
            Dictionary<string, DataRow> GroupRows = this.CreateTGRowDictionary();
            Dictionary<string, bool> ExistingNames = new Dictionary<string, bool>();

            foreach (string k in GroupRows.Keys)
            {
                ExistingNames.Add(k, true);
            }

            foreach (DataRow dr in this.m_TTData.Rows)
            {
                if (dr.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                int id = Convert.ToInt32(dr[IdColName], CultureInfo.InvariantCulture);

                if (!this.m_PrevNames.ContainsKey(id))
                {
                    continue;
                }

                string OldName = this.m_PrevNames[id];
                string OldAutoGroupName = GetAutoGeneratedGroupName(OldName);

                if (!GroupRows.ContainsKey(OldAutoGroupName))
                {
                    continue;
                }

                string NewName = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture);

                Debug.Assert(DataTableUtilities.GetDataBool(GroupRows[OldAutoGroupName], Strings.IS_AUTO_COLUMN_NAME));

                if (OldName != NewName)
                {
                    ModifyRows.Add(dr);
                }
            }

            if (ModifyRows.Count > 0)
            {
                this.m_TGDataSheet.BeginModifyRows();

                foreach (DataRow dr in ModifyRows)
                {
                    string OldName = this.m_PrevNames[Convert.ToInt32(dr[IdColName], CultureInfo.InvariantCulture)];
                    string NewName = Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture);

                    Debug.Assert(OldName != NewName);
                    Debug.Assert(!GroupRows.ContainsKey(GetAutoGeneratedGroupName(NewName)));

                    string OldAutoGroupName = GetAutoGeneratedGroupName(OldName);
                    string NewAutoGroupName = GetAutoGeneratedGroupName(NewName);

                    GroupRows[OldAutoGroupName][Strings.DATASHEET_NAME_COLUMN_NAME] = NewAutoGroupName;
                }

                this.m_TGDataSheet.EndModifyRows();
            }

            base.OnRowsModified(sender, e);
        }

        private Dictionary<string, DataRow> CreateTGRowDictionary()
        {
            Dictionary<string, DataRow> d = new Dictionary<string, DataRow>();

            foreach (DataRow dr in this.m_TGData.Rows)
            {
                if (dr.RowState != DataRowState.Deleted)
                {
                    d.Add(Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture), dr);
                }
            }

            return d;
        }

        private Dictionary<string, bool> CreateTTG_ID_Dictionary()
        {
            Dictionary<string, bool> d = new Dictionary<string, bool>();

            foreach (DataRow dr in this.m_TTGData.Rows)
            {
                if (dr.RowState != DataRowState.Deleted)
                {
                    string k = Create_TT_TG_Key(
                        Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_TYPE_GROUP_TYPE_COLUMN_NAME], CultureInfo.InvariantCulture), 
                        Convert.ToInt32(dr[Strings.DATASHEET_TRANSITION_TYPE_GROUP_GROUP_COLUMN_NAME], CultureInfo.InvariantCulture));

                    Debug.Assert(!d.ContainsKey(k));

                    if (!d.ContainsKey(k))
                    {
                        d.Add(k, true);
                    }
                }
            }

            return d;
        }

        private static string Create_TT_TG_Key(int ttid, int tgid)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", ttid, tgid);
        }

        private void CreateTransitionGroup(int id, string name)
        {
            DataRow dr = this.m_TGData.NewRow();

            dr[this.m_TGDataSheet.PrimaryKeyColumn.Name] = id;
            dr[Strings.DATASHEET_NAME_COLUMN_NAME] = name;
            dr[Strings.IS_AUTO_COLUMN_NAME] = Booleans.BoolToInt(true);

            this.m_TGData.Rows.Add(dr);
        }

        private void CreateTransitionTypeGroup(int typeId, int groupId)
        {
            DataRow dr = this.m_TTGData.NewRow();

            dr[Strings.DATASHEET_TRANSITION_TYPE_ID_COLUMN_NAME] = typeId;
            dr[Strings.DATASHEET_TRANSITION_GROUP_ID_COLUMN_NAME] = groupId;
            dr[Strings.IS_AUTO_COLUMN_NAME] = Booleans.BoolToInt(true);

            this.m_TTGData.Rows.Add(dr);
        }

        private static string GetAutoGeneratedGroupName(DataRow dr)
        {
            return GetAutoGeneratedGroupName(Convert.ToString(dr[Strings.DATASHEET_NAME_COLUMN_NAME], CultureInfo.InvariantCulture));
        }

        private static string GetAutoGeneratedGroupName(string typeName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", typeName, Strings.AUTO_COLUMN_SUFFIX);
        }
    }
}
