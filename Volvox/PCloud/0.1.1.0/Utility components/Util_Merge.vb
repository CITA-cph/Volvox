Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Drawing
Imports Volvox_Cloud

Public Class Util_Merge
    Inherits GH_Component

    Public Sub New()
        MyBase.New("Merge Clouds", "MCloud", "Merge multiple clouds into one.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Merge
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Merge
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Clouds to merge", GH_ParamAccess.list)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cl As New List(Of GH_Cloud)
        If Not DA.GetDataList(0, cl) Then Return

        Dim outcl As New PointCloud

        For Each pc As GH_Cloud In cl
            outcl.Merge(pc.Value)
        Next

        DA.SetData(0, outcl)

    End Sub

End Class
