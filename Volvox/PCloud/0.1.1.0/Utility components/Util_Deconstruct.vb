Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports System.Drawing

Public Class Util_GetPs
    Inherits GH_Component

    Public Sub New()
        MyBase.New("Deconstruct Cloud", "DeCloud", "Get points out of Cloud.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Explode
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Deconstruct
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list)
        pManager.AddVectorParameter("Normals", "N", "Normals", GH_ParamAccess.list)
        pManager.AddColourParameter("Colors", "C", "Colors", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim cl As GH_Cloud = Nothing
        If Not DA.GetData(0, cl) Then Return

        If Not cl.IsValid Then Return

        DA.SetDataList(0, cl.Value.GetPoints)

        If cl.Value.ContainsNormals Then
            DA.SetDataList(1, cl.Value.GetNormals)
        End If

        If cl.Value.ContainsColors Then
            DA.SetDataList(2, cl.Value.GetColors)
        End If

    End Sub

End Class
