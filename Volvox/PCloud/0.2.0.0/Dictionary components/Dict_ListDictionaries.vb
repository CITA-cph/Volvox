Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Dict_ListDictionaries
    Inherits GH_Component

    Sub New()
        MyBase.New("List Keys", "KeyList", "List all keys stored in a cloud.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictListDictionary
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Listkeys
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("Keys", "K", "List of keys", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pc As GH_Cloud = Nothing
        If Not DA.GetData(0, pc) Then Return
        Dim nl As New List(Of String)
        Dim pcv As PointCloud = pc.Value
        DA.SetDataList(0, pcv.UserDictionary.Keys)

    End Sub

End Class
