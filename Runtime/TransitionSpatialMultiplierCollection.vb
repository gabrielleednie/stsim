﻿'*********************************************************************************************
' ST-Sim: A SyncroSim Module for the ST-Sim State-and-Transition Model.
'
' Copyright © 2007-2017 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.
'
'*********************************************************************************************

Imports System.Collections.ObjectModel

Friend Class TransitionSpatialMultiplierCollection
    Inherits KeyedCollection(Of Integer, TransitionSpatialMultiplier)

    Protected Overrides Function GetKeyForItem(ByVal item As TransitionSpatialMultiplier) As Integer
        Return item.TransitionSpatialMultiplierId
    End Function

End Class

