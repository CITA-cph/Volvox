Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud
Imports System.Drawing

Public Class Util_PlaneSec
    Inherits GH_Component

    Public Sub New()
        MyBase.New("Cloud | Plane", "Sec", "Solve intersection events for a Cloud and a Plane.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_PlaneSec
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Section
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Base Cloud", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddPlaneParameter("Plane", "P", "Section plane", GH_ParamAccess.item)
        pManager.AddNumberParameter("Tolerance", "T", "Maximal distance from point to plane", GH_ParamAccess.item, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
        pManager.AddBooleanParameter("Project", "B", "Project points back on the plane", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Section", "S", "Points on plane", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim proj As Boolean = False

        If Not DA.GetData(3, proj) Then Return

        Dim pc As GH_Cloud = Nothing
        If Not DA.GetData(0, pc) Then Return

        Dim pl As Rhino.Geometry.Plane
        If Not DA.GetData(1, pl) Then Return

        Dim tol As Double
        If Not DA.GetData(2, tol) Then Return

        Dim outpc As New Rhino.Geometry.PointCloud

        Dim eq() As Double = pl.GetPlaneEquation()

        Dim denom As Double = 1 / Math.Sqrt(eq(0) * eq(0) + eq(1) * eq(1) + eq(2) * eq(2))

        For Each it As PointCloudItem In pc.Value
            If Math.Abs(Volvox.FastPlaneToPt(denom, eq(0), eq(1), eq(2), eq(3), it.Location)) <= tol Then
                outpc.AppendNew()
                outpc.Item(outpc.Count - 1).Location = it.Location
                If pc.Value.ContainsColors Then outpc.Item(outpc.Count - 1).Color = it.Color
                If pc.Value.ContainsNormals Then outpc.Item(outpc.Count - 1).Normal = it.Normal
            End If
        Next

        If proj Then outpc.Transform(Rhino.Geometry.Transform.PlanarProjection(pl))
        DA.SetData(0, outpc)

    End Sub

End Class
