﻿'*********************************************************************************************
' ST-Sim: A SyncroSim Module for the ST-Sim State-and-Transition Model.
'
' Copyright © 2007-2017 Apex Resource Management Solution Ltd. (ApexRMS). All rights reserved.
'
'*********************************************************************************************

''' <summary>
''' Time-since-transition value
''' </summary>
Friend Class Tst

    Private m_TstValue As Integer
    Private m_TransitionGroupId As Integer

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="transitionGroupId">The transition group ID</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal transitionGroupId As Integer)
        Me.m_TransitionGroupId = transitionGroupId
    End Sub

    ''' <summary>
    ''' Gets or sets the TST value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TstValue As Integer
        Get
            Return Me.m_TstValue
        End Get
        Set(ByVal value As Integer)
            Me.m_TstValue = value
        End Set
    End Property

    ''' <summary>
    ''' The transition group for this TST
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TransitionGroupId As Integer
        Get
            Return Me.m_TransitionGroupId
        End Get
    End Property

End Class

