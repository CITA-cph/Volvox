Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports Rhino.Geometry

Public Class Util_GetRange
    Inherits GH_Component

    Sub New()
        MyBase.New("Sub Cloud", "SubCloud", "Get a part of a cloud.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_UtilGetRange
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SubCloud
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddIntervalParameter("Range", "R", "Range of points to get", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cl As GH_Cloud = Nothing
        If Not DA.GetData(0, cl) Then Return
        If Not cl.IsValid Then Return

        Dim nl As New Interval
        If Not DA.GetData(1, nl) Then Return

        Dim outpc As New PointCloud

        For i As Integer = nl.Min To nl.Max Step 1
            outpc.AppendNew()
            outpc(outpc.Count - 1).Location = cl.Value(i).Location
            If cl.Value.ContainsColors Then outpc(outpc.Count - 1).Color = cl.Value(i).Color
            If cl.Value.ContainsNormals Then outpc(outpc.Count - 1).Normal = cl.Value(i).Normal
        Next

        DA.SetData(0, outpc)

    End Sub
End Class
