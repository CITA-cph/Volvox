Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Util_3dGrid
    Inherits GH_Component

    Sub New()
        MyBase.New("Cloud Grid", "CGrid", "Create a point cloud 3d grid.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_Util3dGrid
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Grid
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddPlaneParameter("Plane", "P", "Base plane for grid", GH_ParamAccess.item, Plane.WorldXY)
        pManager.AddNumberParameter("Size", "S", "Size of grid cells", GH_ParamAccess.item, 1)
        pManager.AddIntegerParameter("Extent X", "Ex", "Number of grid points in base plane X direction", GH_ParamAccess.item, 10)
        pManager.AddIntegerParameter("Extent Y", "Ey", "Number of grid points in base plane Y direction", GH_ParamAccess.item, 15)
        pManager.AddIntegerParameter("Extent Z", "Ez", "Number of grid points in base plane Z direction", GH_ParamAccess.item, 20)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim ex As Integer
        Dim ey As Integer
        Dim ez As Integer
        Dim sz As Double
        Dim pl As Plane = Plane.WorldXY

        If Not (DA.GetData(0, pl)) Then Return
        If Not (DA.GetData(1, sz)) Then Return
        If Not (DA.GetData(2, ex)) Then Return
        If Not (DA.GetData(3, ey)) Then Return
        If Not (DA.GetData(4, ez)) Then Return

        If ex < 1 Or ey < 1 Or ez < 1 Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid extents cannot be less than 1.")
            Return
        End If

        Dim pc As New PointCloud

        For i As Integer = 0 To ez - 1 Step 1
            For j As Integer = 0 To ey - 1 Step 1
                For k As Integer = 0 To ex - 1 Step 1
                    Dim np As New Point3d(k * sz, j * sz, i * sz)
                    pc.Add(np)
                Next
            Next
        Next

        pc.Transform(Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, pl))

        DA.SetData(0, pc)

    End Sub
End Class
