Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports Rhino.Geometry
Imports System.Drawing

Public Class Util_ClosestPoint
    Inherits GH_Component


    Sub New()
        MyBase.New("Closest Point", "CP", "Find closest point in the point cloud.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_ClosestPoint
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_ClosestPoint
        End Get
    End Property


    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud to process", GH_ParamAccess.item)
        pManager.AddPointParameter("Point", "P", "Point to search from", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddPointParameter("Point", "P", "Closest point in cloud", GH_ParamAccess.item)
        pManager.AddVectorParameter("Normal", "N", "Normal at point", GH_ParamAccess.item)
        pManager.AddColourParameter("Color", "C", "Color at point", GH_ParamAccess.item)
        pManager.AddNumberParameter("Distance", "D", "Distance to closest point", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Index", "I", "Closest point index", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pc As GH_Cloud = Nothing
        If Not (DA.GetData(0, pc)) Then Return

        Dim p As Point3d = Nothing
        If Not (DA.GetData(1, p)) Then Return

        Dim idx As Integer = pc.Value.ClosestPoint(p)

        DA.SetData(4, idx)
        DA.SetData(0, pc.Value(idx).Location)
        If pc.Value.ContainsNormals Then DA.SetData(1, pc.Value(idx).Normal)
        If pc.Value.ContainsColors Then DA.SetData(2, pc.Value(idx).Color)
        DA.SetData(3, pc.Value(idx).Location.DistanceTo(p))
    End Sub

End Class
