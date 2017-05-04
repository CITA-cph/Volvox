Imports System.Drawing
Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Volvox_Cloud
Imports Rhino.Geometry

Public Class Util_DisjointClusters
    Inherits GH_Component

    Sub New()
        MyBase.New("Disjoint Cloud", "Disjoint", "Disjoint cloud based on voxel topology.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_Disjoint
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Disjoint
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddNumberParameter("Voxel size", "V", "Voxel size", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.list)
    End Sub


    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pc As GH_Cloud = Nothing
        If Not DA.GetData(0, pc) Then Return
        Dim vxs As Double = 1
        If Not DA.GetData(1, vxs) Then Return

        Dim pclist() As PointCloud = VoxelSplit(pc.Value, vxs)
        DA.SetDataList(0, pclist)
    End Sub

    Function Voxelize(pts As PointCloud, vx As Double) As Mesh
        Dim voxm As New Mesh

        Dim scale As Double = 1 / vx
        pts.Scale(scale)

        Dim vxdt As New DataTree(Of Boolean)

        For i As Integer = 0 To pts.Count - 1 Step 1
            Dim thispt As Point3d = pts(i).Location
            Dim thisp As New GH_Path(Math.Floor(thispt.X), Math.Floor(thispt.Y), Math.Floor(thispt.Z))
            vxdt.Add(True, thisp)
        Next

        For i As Integer = 0 To vxdt.Paths.Count - 1 Step 1
            voxm.Append(CreateBox(vxdt.Path(i), vxdt))
        Next

        voxm.Vertices.CombineIdentical(True, True)

        pts.Scale(vx)
        voxm.Scale(vx)
        Return voxm
    End Function

    Function VoxelSplit(pc As PointCloud, vsize As Double) As Rhino.Geometry.PointCloud()

        Dim st As New System.Diagnostics.Stopwatch
        st.Start()
        Dim mes As New String("")
        mes &= st.ElapsedMilliseconds & vbCrLf

        Dim nmfirst As Mesh = Voxelize(pc, vsize)
        Dim vx() As Mesh = nmfirst.SplitDisjointPieces
        Dim vxcount(vx.GetUpperBound(0)) As Integer

        mes &= st.ElapsedMilliseconds & vbCrLf

        For i As Integer = 0 To vx.Length - 1 Step 1
            vxcount(i) = vx(i).Faces.Count
        Next

        mes &= st.ElapsedMilliseconds & vbCrLf

        Array.Sort(vxcount, vx)
        Array.Reverse(vx)

        Dim nm As New Mesh
        For Each m As Mesh In vx
            nm.Append(m)
        Next

        mes &= st.ElapsedMilliseconds & vbCrLf

        Dim intl(vx.Length - 1) As Interval

        Dim sss As Integer = 0
        For i As Integer = 0 To vx.Count - 1 Step 1
            Dim ni As New Interval(sss, sss + vx(i).Faces.Count - 1)
            sss += vx(i).Faces.Count
            intl(i) = ni
        Next

        mes &= st.ElapsedMilliseconds & vbCrLf

        Dim glist As New List(Of Integer)

        ReDim globlist(pc.Count - 1)
        globmesh = nm
        GlobalCloud = pc
        globintl = intl.ToArray

        Dim thl As New List(Of Threading.Thread)
        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim t As New Threading.Thread(AddressOf ClosestMesh)
            thl.Add(t)
            thl(thl.Count - 1).Start(i)
        Next

        For Each t As Threading.Thread In thl
            t.Join()
        Next

        glist.AddRange(globlist)

        globmesh = Nothing
        GlobalCloud = Nothing
        globintl = Nothing
        globlist = Nothing

        mes &= st.ElapsedMilliseconds & vbCrLf

        Dim outclouds(intl.Count - 1) As PointCloud
        For i As Integer = 0 To intl.Count - 1 Step 1
            outclouds(i) = New PointCloud
        Next

        mes &= st.ElapsedMilliseconds & vbCrLf

        For i As Integer = 0 To glist.Count - 1 Step 1
            Dim thisi As Integer = glist(i)
            outclouds(thisi).AppendNew()
            outclouds(thisi)(outclouds(thisi).Count - 1).Location = pc(i).Location
            If pc.ContainsColors Then outclouds(thisi)(outclouds(thisi).Count - 1).Color = pc(i).Color
            If pc.ContainsNormals Then outclouds(thisi)(outclouds(thisi).Count - 1).Normal = pc(i).Normal
        Next

        mes &= st.ElapsedMilliseconds & vbCrLf
        ' MsgBox(mes)
        Return outclouds
    End Function

    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim globlist() As Integer = Nothing
    Dim globmesh As Mesh = Nothing
    Dim GlobalCloud As PointCloud = Nothing
    Dim globintl() As Interval = Nothing

    Sub ClosestMesh(myindex As Integer)

        Dim i0 As Integer = myindex * Math.Ceiling(GlobalCloud.Count / ProcCount)
        Dim i1 As Integer = Math.Min((myindex + 1) * Math.Ceiling(GlobalCloud.Count / ProcCount) - 1, GlobalCloud.Count - 1)

        For i As Integer = 0 To i1 Step 1
            Dim thisp As Point3d = GlobalCloud(i).Location
            Dim thisi As Integer = globmesh.ClosestMeshPoint(thisp, 0).FaceIndex

            For j As Integer = 0 To globintl.Count - 1 Step 1
                If globintl(j).IncludesParameter(thisi) Then
                    globlist(i) = j
                    Exit For
                End If
            Next
        Next

    End Sub



    Function CreateBox(pth As GH_Path, ByRef dt As DataTree(Of Boolean)) As Mesh
        Dim nm As New Mesh

        Dim pt As Point3d = New Point3d(pth.Indices(0), pth.Indices(1), pth.Indices(2))

        nm.Vertices.Add(pt.X, pt.Y, pt.Z)
        nm.Vertices.Add(pt.X + 1, pt.Y, pt.Z)
        nm.Vertices.Add(pt.X + 1, pt.Y + 1, pt.Z)
        nm.Vertices.Add(pt.X, pt.Y + 1, pt.Z)

        nm.Vertices.Add(pt.X, pt.Y, pt.Z + 1)
        nm.Vertices.Add(pt.X + 1, pt.Y, pt.Z + 1)
        nm.Vertices.Add(pt.X + 1, pt.Y + 1, pt.Z + 1)
        nm.Vertices.Add(pt.X, pt.Y + 1, pt.Z + 1)

        For i As Integer = -1 To 1 Step 2
            Dim p1 As New GH_Path(pth.Indices(0) + i, pth.Indices(1), pth.Indices(2))
            Dim p2 As New GH_Path(pth.Indices(0), pth.Indices(1) + i, pth.Indices(2))
            Dim p3 As New GH_Path(pth.Indices(0), pth.Indices(1), pth.Indices(2) + i)

            Select Case i
                Case -1
                    If Not dt.PathExists(p1) Then nm.Faces.AddFace(3, 0, 4, 7)
                    If Not dt.PathExists(p2) Then nm.Faces.AddFace(0, 1, 5, 4)
                    If Not dt.PathExists(p3) Then nm.Faces.AddFace(3, 2, 1, 0)
                Case 1
                    If Not dt.PathExists(p1) Then nm.Faces.AddFace(1, 2, 6, 5)
                    If Not dt.PathExists(p2) Then nm.Faces.AddFace(7, 6, 2, 3)
                    If Not dt.PathExists(p3) Then nm.Faces.AddFace(6, 7, 4, 5)
            End Select
        Next

        Return nm
    End Function

End Class
