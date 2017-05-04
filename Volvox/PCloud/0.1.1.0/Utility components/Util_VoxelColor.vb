Imports System.Drawing
Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Volvox_Cloud
Imports Rhino.Display
Imports Rhino.Geometry

Public Class Util_VoxelColor
    Inherits GH_Component

    Sub New()
        MyBase.New("Voxel Colored", "VoxelC", "Voxelize a PointCloud with colors.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_VoxelColor
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_VoxelColored
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to voxelize", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddNumberParameter("Voxel size", "V", "Voxel size", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddMeshParameter("Mesh", "M", "Resulting mesh", GH_ParamAccess.item)
    End Sub

    Private aver As Boolean = True

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pc As PointCloud = Nothing
        If Not DA.GetData(0, pc) Then Return
        Dim vxs As Double = 1
        If Not DA.GetData(1, vxs) Then Return

        Dim voxmesh As Mesh = Voxelize(pc, vxs)

        pc.Dispose()
        DA.SetData(0, voxmesh)
    End Sub

    Function Voxelize(pts As PointCloud, vx As Double) As Mesh
        voxm = Nothing
        voxm = New Mesh

        ReDim trees(ProcCount - 1)

        Dim scale As Double = 1 / vx
        pts.Scale(scale)

        GlobalCloud = pts

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim t As New Threading.Thread(AddressOf GetDataTrees)
            Thl.Add(t)
            Thl(Thl.Count - 1).Start(i)
        Next

        For Each t As Threading.Thread In Thl
            t.Join()
        Next

        Dim vxdt As New DataTree(Of Color)

        For Each d As DataTree(Of Color) In trees
            For i As Integer = 0 To d.BranchCount - 1 Step 1
                vxdt.AddRange(d.Branch(i), d.Paths(i))
            Next
        Next

        trees = Nothing
        Thl.Clear()
        GlobalCloud = Nothing
        globtree = vxdt

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim t As New Threading.Thread(AddressOf CreateBox)
            Thl.Add(t)
            Thl(Thl.Count - 1).Start(i)
        Next

        For Each t As Threading.Thread In Thl
            t.Join()
        Next

        voxm = WeldBetter(voxm)
        voxm.Scale(vx)

        globtree = Nothing
        Return voxm

    End Function

    Dim trees() As DataTree(Of Color)
    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim GlobalCloud As PointCloud = Nothing
    Dim Thl As New List(Of Threading.Thread)
    Dim voxm As New Mesh
    Dim globtree As DataTree(Of Color)

    Sub GetDataTrees(MyIndex As Integer)

        Dim i0 As Integer = MyIndex * Math.Ceiling(GlobalCloud.Count / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(GlobalCloud.Count / ProcCount) - 1, GlobalCloud.Count - 1)

        Dim vxdt As New DataTree(Of Color)

        For i As Integer = i0 To i1 Step 1
            Dim thispt As PointCloudItem = GlobalCloud(i)
            Dim thisp As New GH_Path(CInt(thispt.X - 0.5), CInt(thispt.Y - 0.5), CInt(thispt.Z - 0.5))
            vxdt.Add(thispt.Color, thisp)
        Next

        trees(MyIndex) = vxdt

    End Sub

    Function WeldBetter(msh As Mesh) As Mesh
        Dim outm As New Mesh

        Dim colf(msh.Faces.Count - 1) As Color

        Dim counter As Integer = 0
        For Each mf As MeshFace In msh.Faces
            colf(counter) = msh.VertexColors(mf.A)
            counter += 1
        Next

        outm = msh.Duplicate
        outm.Vertices.CombineIdentical(True, True)

        Dim col(outm.Vertices.Count - 1) As List(Of Color)

        For i As Integer = 0 To col.Length - 1 Step 1
            col(i) = New List(Of Color)
        Next

        counter = 0
        For Each mf As MeshFace In outm.Faces
            col(mf.A).Add(colf(counter))
            col(mf.B).Add(colf(counter))
            col(mf.C).Add(colf(counter))
            col(mf.D).Add(colf(counter))
            counter += 1
        Next

        'globcol = col
        'ReDim globcolstore(outm.Vertices.Count - 1)
        'Dim thl As New List(Of Threading.Thread)

        ''   For i As Integer = 0 To ProcCount - 1 Step 1
        'Dim t As New Threading.Thread(AddressOf AverageC)
        '    thl.Add(t)
        'thl(thl.Count - 1).Start(0)
        '' Next

        'For Each tt As Threading.Thread In thl
        '    tt.Join()
        'Next

        For Each c As List(Of Color) In col
            outm.VertexColors.Add(AverageColors(c))
        Next

        ' outm.VertexColors.AppendColors(globcolstore)

        globcolstore = Nothing
        globcol = Nothing
        Return outm
    End Function

    Dim globcol() As List(Of Color) = Nothing
    Dim globcolstore() As Color = Nothing

    Sub AverageC(MyIndex As Integer)

        Dim i0 As Integer = MyIndex * Math.Ceiling(globcol.Length / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(globcol.Length / ProcCount) - 1, globcol.Length - 1)

        Dim totc As Integer = globcol.Length

        For i As Integer = i0 To i1 Step 1
            globcolstore(i) = (AverageColors(globcol(i)))
        Next

    End Sub

    Function AverageColors(col As List(Of Color)) As Color
        Dim a As Integer = 0
        Dim r As Integer = 0
        Dim g As Integer = 0
        Dim b As Integer = 0

        If col.Count = 0 Then Return Color.Black

        Dim countdenom As Double = 1 / col.Count

        For Each thisc As Color In col
            a += thisc.A
            r += thisc.R
            g += thisc.G
            b += thisc.B
        Next

        a *= countdenom
        r *= countdenom
        g *= countdenom
        b *= countdenom

        Return Color.FromArgb(a, r, g, b)
    End Function

    Sub CreateBox(MyIndex As Integer)

        Dim i0 As Integer = MyIndex * Math.Ceiling(globtree.BranchCount / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(globtree.BranchCount / ProcCount) - 1, globtree.BranchCount - 1)

        Dim vxdt As New DataTree(Of Color)
        Dim localmesh As New Mesh

        For i As Integer = i0 To i1 Step 1

            Dim nm As New Mesh
            Dim pth As GH_Path = globtree.Paths(i)
            Dim pt As Point3d = New Point3d(pth.Indices(0), pth.Indices(1), pth.Indices(2))

            nm.Vertices.Add(pt.X, pt.Y, pt.Z)
            nm.Vertices.Add(pt.X + 1, pt.Y, pt.Z)
            nm.Vertices.Add(pt.X + 1, pt.Y + 1, pt.Z)
            nm.Vertices.Add(pt.X, pt.Y + 1, pt.Z)

            nm.Vertices.Add(pt.X, pt.Y, pt.Z + 1)
            nm.Vertices.Add(pt.X + 1, pt.Y, pt.Z + 1)
            nm.Vertices.Add(pt.X + 1, pt.Y + 1, pt.Z + 1)
            nm.Vertices.Add(pt.X, pt.Y + 1, pt.Z + 1)

            Dim avercolor As Color = AverageColors(globtree.Branch(pth).ToList)
            For j As Integer = 0 To 7 Step 1
                nm.VertexColors.Add(avercolor)
            Next

            For j As Integer = -1 To 1 Step 2
                Dim p1 As New GH_Path(pth.Indices(0) + j, pth.Indices(1), pth.Indices(2))
                Dim p2 As New GH_Path(pth.Indices(0), pth.Indices(1) + j, pth.Indices(2))
                Dim p3 As New GH_Path(pth.Indices(0), pth.Indices(1), pth.Indices(2) + j)

                Select Case j
                    Case -1
                        If Not globtree.PathExists(p1) Then nm.Faces.AddFace(3, 0, 4, 7)
                        If Not globtree.PathExists(p2) Then nm.Faces.AddFace(0, 1, 5, 4)
                        If Not globtree.PathExists(p3) Then nm.Faces.AddFace(3, 2, 1, 0)
                    Case 1
                        If Not globtree.PathExists(p1) Then nm.Faces.AddFace(1, 2, 6, 5)
                        If Not globtree.PathExists(p2) Then nm.Faces.AddFace(7, 6, 2, 3)
                        If Not globtree.PathExists(p3) Then nm.Faces.AddFace(6, 7, 4, 5)
                End Select
            Next

            localmesh.Append(nm)

        Next

        SyncLock voxm
            voxm.Append(localmesh)
        End SyncLock

    End Sub

End Class
