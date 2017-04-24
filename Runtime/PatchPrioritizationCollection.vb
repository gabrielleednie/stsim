﻿'*********************************************************************************************
' ST-Sim: A SyncroSim Module for the ST-Sim State-and-Transition Model.
'
' Copyright © 2007-2017 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.
'
'*********************************************************************************************

Imports System.Collections.ObjectModel

''' <summary>
''' Patch Prioritization Collection
''' </summary>
''' <remarks></remarks>
Friend Class PatchPrioritizationCollection
    Inherits KeyedCollection(Of Integer, PatchPrioritization)

    Protected Overrides Function GetKeyForItem(item As PatchPrioritization) As Integer
        Return item.PatchPrioritizationId
    End Function

End Class
