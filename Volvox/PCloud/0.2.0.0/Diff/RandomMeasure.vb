Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class RandomMeasure
    Inherits GH_Component

    Sub New()
        MyBase.New("IfcCompare", "IfcCompare", "Compare N points with geometry", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("eb310e29-f16d-4eca-9c01-286875312cb6")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list)
        pManager.AddTextParameter("Mesh GUID", "Gm", "Mesh GUIDS", GH_ParamAccess.list)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Clouds to compare", GH_ParamAccess.list)
        pManager.AddTextParameter("Cloud GUID", "Gc", "Cloud GUIDS", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Samples", "S", "Number of samples", GH_ParamAccess.item, 500)
        pManager.AddIntegerParameter("Seed", "R", "Randomness seed", GH_ParamAccess.item, 2)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddNumberParameter("Distances", "D", "List of distance values", GH_ParamAccess.tree)
        pManager.AddTextParameter("GUIDs", "G", "GUIDs", GH_ParamAccess.tree)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim rnds As Integer
        Dim samp As Integer

        If Not DA.GetData(4, samp) Then Return
        If Not DA.GetData(5, rnds) Then Return

        Dim pc As New List(Of GH_Cloud)
        If Not DA.GetDataList(2, pc) Then Return

        Dim msh As New List(Of Mesh)
        If Not DA.GetDataList(0, msh) Then Return

        Dim mg As New List(Of String)
        If Not DA.GetDataList(1, mg) Then Return

        Dim cg As New List(Of String)
        If Not DA.GetDataList(3, cg) Then Return

        If mg.Count <> msh.Count Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh and Mesh GUIDs lists are not the same length.")
        End If

        If pc.Count <> cg.Count Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cloud and Cloud GUIDs lists are not the same length.")
        End If



        Dim data As New SortedList(Of String, List(Of Double))

        For i As Integer = 0 To cg.Count - 1 Step 1
            For j As Integer = 0 To mg.Count - 1 Step 1
                If cg(i) = mg(j) Then
                    Dim thisc As GH_Cloud = pc(i)
                    Dim thism As Mesh = msh(j)
                    Dim thisd As New List(Of Double)
                    Dim rnd As New Random(rnds)

                    For k As Integer = 0 To samp - 1 Step 1
                        Dim rndp As Point3d = pc(i).Value(rnd.Next(0, pc(i).Value.Count)).Location
                        thisd.Add(msh(j).ClosestPoint(rndp).DistanceTo(rndp))
                    Next

                    If data.ContainsKey(cg(i)) Then
                        data(cg(i)).AddRange(thisd)
                    Else
                        data.Add(cg(i), thisd)
                    End If

                End If
            Next
        Next

        Dim dtd As New DataTree(Of Double)
        Dim dtg As New DataTree(Of String)

        For i As Integer = 0 To data.Keys.Count - 1 Step 1
            dtg.Add(data.Keys(i), New GH_Path(i))
            dtd.AddRange(data(data.Keys(i)), New GH_Path(i))
        Next

        DA.SetDataTree(0, dtd)
        DA.SetDataTree(1, dtg)
    End Sub
End Class
