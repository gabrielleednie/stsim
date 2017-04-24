﻿'*********************************************************************************************
' ST-Sim: A SyncroSim Module for the ST-Sim State-and-Transition Model.
'
' Copyright © 2007-2017 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.
'
'*********************************************************************************************

''' <summary>
''' Transition filter criteria class
''' </summary>
''' <remarks></remarks>
Class TransitionFilterCriteria

    Public IncludeDeterministic As Boolean = True
    Public IncludeProbabilistic As Boolean = True
    Public TransitionGroups As New Dictionary(Of Integer, Boolean)

End Class
