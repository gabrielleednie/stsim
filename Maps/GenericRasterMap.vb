﻿'*********************************************************************************************
' ST-Sim: A SyncroSim Module for the ST-Sim State-and-Transition Model.
'
' Copyright © 2007-2017 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.
'
'*********************************************************************************************

Imports System.Globalization
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports SyncroSim.Core
Imports SyncroSim.Core.Forms
Imports SyncroSim.StochasticTime.Forms

<ObfuscationAttribute(Exclude:=True, ApplyToMembers:=False)>
Class GenericRasterMap
    Inherits StochasticTimeExportTransformer

    ReadOnly fileFilterRegex As String = String.Format(CultureInfo.CurrentCulture, FILE_FILTER_ID_REGEX, SPATIAL_MAP_AVG_ANNUAL_TRANSITION_PROBABILITY_VARIABLE_PREFIX)

    Protected Overrides Sub Export(location As String, exportType As ExportType)
        StochasticTimeExportTransformer.CopyRasterFiles(Me.GetActiveResultScenarios(), fileFilterRegex, location, AddressOf CreateExportFilename)
    End Sub

    ''' <summary>
    ''' Rename the Spatial Export File, from the short form name generated during Model run, to the user friendly Export name  
    ''' </summary>
    ''' <param name="sourceName">The short form filename, as created during Model run spatial output</param>
    ''' <returns>The long form user-friendly Export filename</returns>
    ''' <remarks></remarks>
    Private Function CreateExportFilename(sourceName As String) As String
        Return CreateExportFilenameInternal(sourceName)
    End Function

    ''' <summary>
    ''' Create an Export filename using the export filename convention. This involves replacing the Id (in available) with the 
    ''' appropriate Group ID Name, and the File Prefix with the Display Name is it appears in the DataSheet column definition</summary>
    ''' <param name="filename">The name of the file as it appears in the internal filenaming convention</param>
    ''' <returns>The filename as it appears in the external filenaming convention</returns>
    ''' <remarks>Example Internal file convention is It0000-Tsy-tgap-z.tif. External convention is ...Tg-TransitionGroup.tif</remarks>
    Private Function CreateExportFilenameInternal(ByVal filename As String,ds As DataSheet, fileColName As String) As String

        ' We need datasheet name
        ' We need FileColName
        ' We need BandColName ?
        ' We need bandFilterName
        ' From bandFilterName we can determine filter datasheet, which we can use to look up Name for ID

        ' Pull the Id out of the filename, and convert a name
        Dim m As Match = Regex.Match(filename, fileFilterRegex)
        If Not m.Success Or m.Groups.Count <> 4 Then
            ' Something wrong here, so just return the original filename
            Debug.Assert(False, String.Format(CultureInfo.InvariantCulture, "Error parsing the internal filename '{0}'.",filename))
            Return filename
        End If

        Dim id As Integer = CInt(m.Groups(2).Value)
        Dim name As String = id.ToString(CultureInfo.InvariantCulture)

        Dim ds As DataSheet = Me.Project.GetDataSheet(DATASHEET_TRANSITION_GROUP_NAME)

        For Each dr As DataRow In ds.GetData.Rows
            If CInt(dr(ds.PrimaryKeyColumn.Name)) = id Then
                name = CStr(dr("NAME"))
                Exit For
            End If
        Next

        Return String.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}.{3}", m.Groups(1), SPATIAL_MAP_EXPORT_AVG_ANNUAL_TRANSITION_PROBABILITY_VARIABLE_PREFIX, name, m.Groups(3))

    End Function

End Class