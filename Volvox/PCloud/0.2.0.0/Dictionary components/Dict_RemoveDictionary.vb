'Public Class Dict_RemoveDictionary

'End Class

Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Dict_RemoveDictionary
    Inherits GH_Component

    Sub New()
        MyBase.New("Remove Data", "RemKey", "Remove data stored in a cloud.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictRemoveDictionary
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_RemoveData
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "User data keys, leave empty list to remove all of them.", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Modified cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pc As PointCloud = Nothing
        If Not DA.GetData(0, pc) Then Return
        pc = pc.Duplicate
        Dim nl As New List(Of String)

        If Not DA.GetDataList(1, nl) Then
            pc.UserDictionary.Clear()
        Else
            For Each thiskey As String In nl
                If pc.UserDictionary.Keys.Contains(thiskey) Then
                    pc.UserDictionary.Remove(thiskey)
                End If
            Next
        End If

        DA.SetData(0, pc)

    End Sub

End Class