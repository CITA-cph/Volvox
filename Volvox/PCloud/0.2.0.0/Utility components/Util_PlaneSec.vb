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

    Dim GlobalCloud As PointCloud = Nothing
    Dim NewCloud As PointCloud = Nothing
    Dim ThreadList As New List(Of Threading.Thread)
    Dim CloudPieces() As PointCloud = Nothing
    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim CutPlane As Plane = Nothing
    Dim Tolerance As Double

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim proj As Boolean = False
        If Not DA.GetData(3, proj) Then Return
        Dim pc As GH_Cloud = Nothing
        If Not DA.GetData(0, pc) Then Return
        Dim pl As Rhino.Geometry.Plane
        If Not DA.GetData(1, pl) Then Return
        Dim tol As Double
        If Not DA.GetData(2, tol) Then Return

        CutPlane = pl
        Tolerance = tol

        ThreadList.Clear()
        NewCloud = New PointCloud
        CloudPieces = Nothing

        ReDim CloudPieces(ProcCount - 1)

        GlobalCloud = pc.Value

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf SecAction)
            nt.IsBackground = True
            ThreadList.Add(nt)
        Next

        For i As Integer = 0 To ProcCount - 1 Step 1
            ThreadList(i).Start(i)
        Next

        For Each t As Threading.Thread In ThreadList
            t.Join()
        Next

        GlobalCloud = Nothing

        For Each pcl As PointCloud In CloudPieces
            If pcl IsNot Nothing Then NewCloud.Merge(pcl)
        Next

        CloudPieces = Nothing
        ThreadList.Clear()

        If proj Then NewCloud.Transform(Rhino.Geometry.Transform.PlanarProjection(pl))
        DA.SetData(0, NewCloud)

    End Sub

    Sub SecAction(MyIndex As Integer)

        Dim eq() As Double = CutPlane.GetPlaneEquation
        Dim denom As Double = 1 / Math.Sqrt(eq(0) * eq(0) + eq(1) * eq(1) + eq(2) * eq(2))

        Dim MyCloud As New PointCloud

        Dim i0 As Integer = MyIndex * Math.Ceiling(GlobalCloud.Count / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(GlobalCloud.Count / ProcCount) - 1, GlobalCloud.Count - 1)

        Dim totc As Integer = GlobalCloud.Count

        For i As Integer = i0 To i1 Step 1

            Dim GlobalCloudItem As PointCloudItem = GlobalCloud.Item(i)
            If Math.Abs(Volvox.FastPlaneToPt(denom, eq(0), eq(1), eq(2), eq(3), GlobalCloudItem.Location)) <= Tolerance Then

                MyCloud.AppendNew()
                Dim MyCloudItem As PointCloudItem = MyCloud.Item(MyCloud.Count - 1)
                MyCloudItem.Location = GlobalCloudItem.Location
                If GlobalCloud.ContainsColors Then MyCloudItem.Color = GlobalCloudItem.Color
                If GlobalCloud.ContainsNormals Then MyCloudItem.Normal = GlobalCloudItem.Normal

            End If

        Next

        CloudPieces(MyIndex) = MyCloud

    End Sub

End Class
