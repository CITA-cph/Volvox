Imports Grasshopper.Kernel
Imports GH_IO
Imports Rhino.Geometry
Imports System.Drawing
Imports Volvox_Cloud

Public Class Util_Create
    Inherits GH_Component

    Public Sub New()
        MyBase.New("Construct Cloud", "Cloud", "Construct a Cloud.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Create
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Construct
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list)
        pManager.AddVectorParameter("Normals", "N", "Normals", GH_ParamAccess.list)
        pManager.AddColourParameter("Colors", "C", "Colors", GH_ParamAccess.list)
        pManager.Param(1).Optional = True
        pManager.Param(2).Optional = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pts As New List(Of Point3d)
        Dim vec As New List(Of Vector3d)
        Dim col As New List(Of Color)

        If Not DA.GetDataList(0, pts) Then Return

        Dim addcol As Boolean = False
        Dim addvec As Boolean = False

        If DA.GetDataList(1, vec) Then
            If vec.Count = pts.Count Then
                addvec = True
            Else
                If vec.Count > 0 Then Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "You have to provide the same amount of normals and points.")
            End If
        End If

        If DA.GetDataList(2, col) Then
            If col.Count = pts.Count Then
                addcol = True
            Else
                If col.Count > 0 Then Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "You have to provide the same amount of colors and points.")
            End If
        End If

        Dim nc As New Rhino.Geometry.PointCloud(pts)

        If addcol Or addvec Then
            For Each it As PointCloudItem In nc
                If addcol Then it.Color = col(it.Index)
                If addvec Then it.Normal = vec(it.Index)
            Next
        End If

        DA.SetData(0, nc)

    End Sub

End Class
