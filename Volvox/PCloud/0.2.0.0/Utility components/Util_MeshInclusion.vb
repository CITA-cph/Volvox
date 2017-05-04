Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Drawing
Imports Volvox_Cloud

Public Class Util_MeshInclusion
    Inherits GH_Component

    Sub New()
        MyBase.New("Mesh Include", "MInclude", "Cull points outside/inside of a mesh.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_UtilMeshInclusion
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_MeshCull
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to compare", GH_ParamAccess.item)
        pManager.AddMeshParameter("Mesh", "M", "Mesh to test inclusion", GH_ParamAccess.list)
        pManager.AddBooleanParameter("Leave", "L", "Leave points outside the mesh", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim Incl() As Boolean = Nothing
    Dim GlobalCloud As PointCloud = Nothing
    Dim MeshToCheck As List(Of Mesh)

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pc As GH_Cloud = Nothing
        Dim m As New List(Of Mesh)
        Dim it As Boolean

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetDataList(1, m) Then Return
        If Not DA.GetData(2, it) Then Return

        For Each msh As Mesh In m
            msh.FaceNormals.ComputeFaceNormals()
        Next

        GlobalCloud = pc.Value
        MeshToCheck = m
        ReDim Incl(pc.Value.Count - 1)

        Dim ThreadList As New List(Of Threading.Thread)

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf Inclusion)
            nt.IsBackground = True
            ThreadList.Add(nt)
        Next

        For i As Integer = 0 To ProcCount - 1 Step 1
            ThreadList(i).Start(i)
        Next

        For Each t As Threading.Thread In ThreadList
            t.Join()
        Next

        Dim pco As New PointCloud

        If it Then
            For i As Integer = 0 To pc.Value.Count - 1 Step 1
                If Not Incl(i) Then CopyToCloud(pc.Value, pco, i)
            Next
        Else
            For i As Integer = 0 To pc.Value.Count - 1 Step 1
                If Incl(i) Then CopyToCloud(pc.Value, pco, i)
            Next
        End If

        Incl = Nothing
        MeshToCheck = Nothing
        GlobalCloud = Nothing

        DA.SetData(0, pco)
    End Sub

    Dim halfpi As Double = Math.PI / 2

    Sub Inclusion(MyIndex As Integer)

        Dim i0 As Integer = MyIndex * Math.Ceiling(Incl.Count / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(Incl.Count / ProcCount) - 1, Incl.Count - 1)

        For i As Integer = i0 To i1 Step 1
            Incl(i) = False
            Dim GlobalCloudItem As PointCloudItem = GlobalCloud.Item(i)
            Dim p3 As Point3d = GlobalCloudItem.Location
            For Each m As Mesh In MeshToCheck
                If m.IsPointInside(p3, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, False) Then
                    Incl(i) = True
                    Exit For
                End If
            Next
        Next

    End Sub

End Class
