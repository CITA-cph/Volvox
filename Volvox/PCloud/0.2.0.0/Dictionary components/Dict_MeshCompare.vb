Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Drawing
Imports Volvox_Cloud

Public Class Dict_MeshCompare
    Inherits GH_Component

    Sub New()
        MyBase.New("Mesh Compare", "MCompare", "Compute distance to a mesh.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictMeshCompare
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Compare
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to compare", GH_ParamAccess.item)
        pManager.AddMeshParameter("Mesh", "M", "Mesh to compare with", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud with UserData", GH_ParamAccess.item)
    End Sub

    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim Dist() As Double = Nothing
    Dim GlobalCloud As PointCloud = Nothing
    Dim MeshToCheck As Mesh = Nothing

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pc As PointCloud = Nothing
        Dim m As Mesh = Nothing
        Dim it As String = Nothing

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, m) Then Return
        If Not DA.GetData(2, it) Then Return

        pc = pc.Duplicate

        m.FaceNormals.ComputeFaceNormals()

        GlobalCloud = pc
        MeshToCheck = m
        ReDim Dist(pc.Count - 1)

        Dim ThreadList As New List(Of Threading.Thread)

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf Distances)
            nt.IsBackground = True
            ThreadList.Add(nt)
        Next

        For i As Integer = 0 To ProcCount - 1 Step 1
            ThreadList(i).Start(i)
        Next

        For Each t As Threading.Thread In ThreadList
            t.Join()
        Next

        pc.UserDictionary.Set(it, Dist)
        Dist = Nothing
        MeshToCheck = Nothing
        GlobalCloud = Nothing

        DA.SetData(0, pc)
    End Sub

    Dim halfpi As Double = Math.PI / 2

    Sub Distances(MyIndex As Integer)

        Dim i0 As Integer = MyIndex * Math.Ceiling(Dist.Count / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(Dist.Count / ProcCount) - 1, Dist.Count - 1)

        Dim totc As Integer = Dist.Count

        For i As Integer = i0 To i1 Step 1
            Dim GlobalCloudItem As PointCloudItem = GlobalCloud.Item(i)

            Dim p3 As Point3d = GlobalCloudItem.Location
            Dim pm As New Point3d
            Dim pv As New Vector3d

            MeshToCheck.ClosestPoint(p3, pm, pv, 0)
            Dim d As Double = p3.DistanceTo(pm)

            If Vector3d.VectorAngle(pv, New Vector3d(pm - p3)) < halfpi Then d = -d
            Dist(i) = d
        Next



    End Sub

End Class
