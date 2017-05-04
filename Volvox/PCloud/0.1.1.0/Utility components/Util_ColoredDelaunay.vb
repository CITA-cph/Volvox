Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports System.Drawing
Imports Rhino.Geometry

Public Class Util_ColoredDelaunay
    Inherits GH_Component

    Sub New()
        MyBase.New("Delaunay Colored", "CDel", "Delaunay triangulation on Point Cloud, with colors.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_ColoredDelaunay
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Delaunay
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud to process", GH_ParamAccess.item)
        pManager.AddPlaneParameter("Plane", "Pl", "Projection plane", GH_ParamAccess.item, Plane.WorldXY)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddMeshParameter("Mesh", "M", "Resulting mesh", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pc As GH_Cloud = Nothing
        If Not (DA.GetData(0, pc)) Then Return

        Dim pl As Plane = Plane.WorldXY
        If Not (DA.GetData(1, pl)) Then Return

        Dim nodes As New Grasshopper.Kernel.Geometry.Node2List

        Dim trans As Rhino.Geometry.Transform = Rhino.Geometry.Transform.PlaneToPlane(pl, Plane.WorldXY)
        Dim transback As Rhino.Geometry.Transform = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, pl)

        For i As Integer = 0 To pc.Value.Count - 1 Step 1
            Dim thisp As Point3d = pc.Value(i).Location
            thisp.Transform(trans)
            nodes.Append(New Geometry.Node2(thisp.X, thisp.Y))
        Next

        Dim decomp As Mesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, Nothing)

        Dim delm As New Mesh
        delm.Vertices.AddVertices(pc.Value.GetPoints)
        delm.Faces.AddFaces(decomp.Faces)

        For i As Integer = 0 To delm.Vertices.Count - 1 Step 1
            delm.VertexColors.Add(pc.Value(i).Color)
        Next

        DA.SetData(0, delm)

    End Sub
End Class
