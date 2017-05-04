Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud

Public Class Util_Statistics
    Inherits GH_Component

    Sub New()
        MyBase.New("Cloud Statistics", "CloudStats", "Basic information about cloud.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_UtilStatistics
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Stats
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddIntegerParameter("Count", "P", "Number of points", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Colors", "C", "Contains colors", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Colors", "N", "Contains normals", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cl As GH_Cloud = Nothing
        If Not DA.GetData(0, cl) Then Return
        If Not cl.IsValid Then Return

        DA.SetData(0, cl.Value.Count)
        DA.SetData(1, cl.Value.ContainsColors)
        DA.SetData(2, cl.Value.ContainsNormals)

    End Sub
End Class
