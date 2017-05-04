Imports System.Drawing
Imports System.Windows.Forms
Imports GH_IO.Serialization
Imports GH_IO.Types
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Util_Selection
    Inherits GH_Component

    Sub New()
        MyBase.New("Selection", "Selection", "Create selection.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_UtilSelection
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Selection
        End Get
    End Property

    Public Overrides Sub CreateAttributes()
        m_attributes = New Util_SelectionAtt(Me)
    End Sub

    Public Overrides Function Write(writer As GH_IWriter) As Boolean

        writer.SetInt32("line_count", lines.Count)

        For i As Integer = 0 To lines.Count - 1 Step 1
            Dim thisline As Line = lines(i)
            Dim s As New Point3d(thisline.From)
            Dim e As New Point3d(thisline.To)
            writer.SetLine("line " & i, New GH_IO.Types.GH_Line(New GH_Point3D(s.X, s.Y, s.Z), New GH_Point3D(e.X, e.Y, e.Z)))
        Next

        writer.SetInt32("mesh_count", meshes.Count)

        For i As Integer = 0 To meshes.Count - 1 Step 1
            For j As Integer = 0 To 7 Step 1
                Dim thisv As Point3d = meshes(i).Vertices(j)
                writer.SetPoint3D("meshvertex " & i & " index " & j, New GH_Point3D(thisv.X, thisv.Y, thisv.Z))
            Next
        Next

        Return MyBase.Write(writer)
    End Function

    Public Overrides Function Read(reader As GH_IReader) As Boolean

        Dim linec As Integer = reader.GetInt32("line_count")
        lines.Clear()

        For i As Integer = 0 To linec - 1 Step 1
            Dim thisline As GH_IO.Types.GH_Line = reader.GetLine("line " & i)
            Dim pa As GH_Point3D = thisline.A
            Dim pb As GH_Point3D = thisline.B
            lines.Add(New Line(pa.x, pa.y, pa.z, pb.x, pb.y, pb.z))
        Next

        Dim thisi As Integer = reader.GetInt32("mesh_count")
        meshes.Clear()

        For i As Integer = 0 To thisi - 1 Step 1
            Dim thism As New Mesh
            For j As Integer = 0 To 7 Step 1
                Dim thisp As GH_Point3D = reader.GetPoint3D("meshvertex " & i & " index " & j)
                thism.Vertices.Add(thisp.x, thisp.y, thisp.z)
            Next

            thism.Faces.AddFace(3, 0, 4, 7)
            thism.Faces.AddFace(0, 1, 5, 4)
            thism.Faces.AddFace(3, 2, 1, 0)
            thism.Faces.AddFace(1, 2, 6, 5)
            thism.Faces.AddFace(7, 6, 2, 3)
            thism.Faces.AddFace(6, 7, 4, 5)

            meshes.Add(thism)
        Next

        Return MyBase.Read(reader)
    End Function

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddBoxParameter("Bounding Box", "B", "Optional bounding box", GH_ParamAccess.tree)
        Me.Params.Input(0).Optional = True
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddMeshParameter("Selection", "S", "Selection mesh", GH_ParamAccess.list)
    End Sub

    Public Overrides Sub AppendAdditionalMenuItems(menu As ToolStripDropDown)
        MyBase.AppendAdditionalMenuItems(menu)
        Menu_AppendItem(menu, "Create Selection", AddressOf RunSelection)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        selectingnow = False

        Dim bbox As New GH_Structure(Of GH_Box)
        Dim bboxin As New List(Of BoundingBox)


        If DA.GetDataTree(0, bbox) Then
            For Each obj As GH_Box In bbox.AllData(True)
                bboxin.Add(obj.Value.BoundingBox)
            Next
        End If

        If bboxin.Count > 0 Then

            bb = bboxin(0)
            For Each b As BoundingBox In bboxin
                bb.Union(b)
            Next

            bboxes.Clear()
            RebuildMeshes()

        End If

        For Each m As Mesh In meshes
            m.FaceNormals.ComputeFaceNormals()
        Next

        DA.SetDataList(0, meshes)
    End Sub

    Dim bb As New BoundingBox

    Dim lines As New List(Of Line)
    Dim meshes As New List(Of Mesh)
    Dim selectingnow As Boolean = False

    Dim bboxes As New List(Of Box)

    Friend Sub RunSelection()

        selectingnow = True
        Dim complete As Boolean = False
        lines.Clear()
        meshes.Clear()

        Me.OnPreviewExpired(True)

        While Not complete

            Dim rect As New Rectangle
            Dim view As Rhino.Display.RhinoView = Nothing

            Rhino.RhinoApp.WriteLine("Draw selection rectangle")
            Dim getrect As Rhino.Commands.Result = Rhino.Input.RhinoGet.Get2dRectangle(True, rect, view)

            If getrect = Rhino.Commands.Result.Success Then

                Dim ffar As New Plane
                Dim fnear As New Plane

                view.ActiveViewport.GetFrustumFarPlane(ffar)
                view.ActiveViewport.GetFrustumNearPlane(fnear)

                Dim ball As Sphere = Sphere.FitSphereToPoints(bb.GetCorners)

                ball.Radius += Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 10

                ffar.Origin = ball.ClosestPoint(ffar.ClosestPoint(ball.Center))

                Dim l1 As New Line
                Dim l2 As New Line
                Dim l3 As New Line
                Dim l4 As New Line

                view.ActiveViewport.GetFrustumLine(rect.X, rect.Y, l1)
                l1 = ClipLine(fnear, ffar, l1)
                lines.Add(l1)

                view.ActiveViewport.GetFrustumLine(rect.X + rect.Width, rect.Y, l2)
                l2 = ClipLine(fnear, ffar, l2)
                lines.Add(l2)

                view.ActiveViewport.GetFrustumLine(rect.X + rect.Width, rect.Y + rect.Height, l3)
                l3 = ClipLine(fnear, ffar, l3)
                lines.Add(l3)

                view.ActiveViewport.GetFrustumLine(rect.X, rect.Y + rect.Height, l4)
                l4 = ClipLine(fnear, ffar, l4)
                lines.Add(l4)

                lines.Add(New Line(l1.From, l2.From))
                lines.Add(New Line(l2.From, l3.From))
                lines.Add(New Line(l3.From, l4.From))
                lines.Add(New Line(l4.From, l1.From))

                lines.Add(New Line(l1.To, l2.To))
                lines.Add(New Line(l2.To, l3.To))
                lines.Add(New Line(l3.To, l4.To))
                lines.Add(New Line(l4.To, l1.To))

                Dim nm As New Mesh

                nm.Vertices.Add(l1.From)
                nm.Vertices.Add(l2.From)
                nm.Vertices.Add(l3.From)
                nm.Vertices.Add(l4.From)
                nm.Vertices.Add(l1.To)
                nm.Vertices.Add(l2.To)
                nm.Vertices.Add(l3.To)
                nm.Vertices.Add(l4.To)

                nm.Faces.AddFace(3, 2, 1, 0)
                nm.Faces.AddFace(3, 0, 4, 7)
                nm.Faces.AddFace(0, 1, 5, 4)
                nm.Faces.AddFace(1, 2, 6, 5)
                nm.Faces.AddFace(7, 6, 2, 3)
                nm.Faces.AddFace(6, 7, 4, 5)

                nm.UnifyNormals()
                meshes.Add(nm)

                Me.OnPreviewExpired(True)

                Rhino.RhinoApp.WriteLine(rect.ToString)
            ElseIf getrect = Rhino.Commands.Result.Cancel
                Exit While
            End If

        End While

        Rhino.RhinoApp.WriteLine("Selection created")
        Grasshopper.Instances.DocumentEditorFadeIn()
        selectingnow = False
        Me.ExpireSolution(True)
    End Sub

    Public Overrides Sub DrawViewportWires(args As IGH_PreviewArgs)
        If Not Me.Locked Then args.Display.DrawLines(lines, Color.Blue)
    End Sub

    Dim meshmat As New Rhino.Display.DisplayMaterial(Color.Blue, 0.95)

    Public Overrides Sub DrawViewportMeshes(args As IGH_PreviewArgs)
        If selectingnow Or Me.Locked Then Return

        For Each m As Mesh In meshes
            If Me.Attributes.Selected Then
                args.Display.DrawMeshShaded(m, args.ShadeMaterial_Selected)
            Else
                args.Display.DrawMeshShaded(m, meshmat)
            End If
        Next

    End Sub

    Public Overrides ReadOnly Property ClippingBox As BoundingBox
        Get

            Dim bbox As BoundingBox = BoundingBox.Empty

            For Each l As Line In lines
                bbox.Union(l.BoundingBox)
            Next

            For Each m As Mesh In meshes
                bbox.Union(m.GetBoundingBox(False))
            Next

            Return bbox

        End Get
    End Property


    Sub RebuildMeshes()

        Dim ball As Sphere = Sphere.FitSphereToPoints(bb.GetCorners)
        ball.Radius += Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 10

        lines.Clear()

        For Each m As Mesh In meshes

            Dim f5 As MeshFace = m.Faces(5)
            Dim f0 As MeshFace = m.Faces(0)

            Dim ffar As New Plane(m.Vertices(f5.A), m.Vertices(f5.B), m.Vertices(f5.D))
            Dim fnear As New Plane(m.Vertices(f0.A), m.Vertices(f0.B), m.Vertices(f0.D))

            Dim nbb As New Box(ffar, ball.BoundingBox.GetCorners)
            bboxes.Add(nbb)

            ffar = New Plane(nbb.GetCorners(4), nbb.GetCorners(5), nbb.GetCorners(7)) 'ball.ClosestPoint(ffar.ClosestPoint(ball.Center))

            Dim l1 As New Line(m.Vertices(0), m.Vertices(4))
            Dim l2 As New Line(m.Vertices(1), m.Vertices(5))
            Dim l3 As New Line(m.Vertices(2), m.Vertices(6))
            Dim l4 As New Line(m.Vertices(3), m.Vertices(7))

            Dim par1 As Double
            Dim par2 As Double
            Dim par3 As Double
            Dim par4 As Double

            Rhino.Geometry.Intersect.Intersection.LinePlane(l1, ffar, par1)
            Rhino.Geometry.Intersect.Intersection.LinePlane(l2, ffar, par2)
            Rhino.Geometry.Intersect.Intersection.LinePlane(l3, ffar, par3)
            Rhino.Geometry.Intersect.Intersection.LinePlane(l4, ffar, par4)

            If Not (par1 < 0 Or par2 < 0 Or par3 < 0 Or par4 < 0) Then

                Dim p1 As Point3d = l1.PointAt(par1)
                Dim p2 As Point3d = l2.PointAt(par2)
                Dim p3 As Point3d = l3.PointAt(par3)
                Dim p4 As Point3d = l4.PointAt(par4)

                Dim pf1 As New Point3f(p1.X, p1.Y, p1.Z)
                Dim pf2 As New Point3f(p2.X, p2.Y, p2.Z)
                Dim pf3 As New Point3f(p3.X, p3.Y, p3.Z)
                Dim pf4 As New Point3f(p4.X, p4.Y, p4.Z)

                m.Vertices(4) = pf1
                m.Vertices(5) = pf2
                m.Vertices(6) = pf3
                m.Vertices(7) = pf4

            End If

            For i As Integer = 0 To m.TopologyEdges.Count - 1 Step 1
                lines.Add(m.TopologyEdges.EdgeLine(i))
            Next

        Next



    End Sub

    Private Function ClipLine(NearPlane As Plane, FarPlane As Plane, l As Line) As Line
        Dim lo As New Line
        Dim par1 As Double
        Dim par2 As Double

        Rhino.Geometry.Intersect.Intersection.LinePlane(l, NearPlane, par1)
        Rhino.Geometry.Intersect.Intersection.LinePlane(l, FarPlane, par2)

        Return New Line(l.PointAt(par1), l.PointAt(par2))
    End Function
End Class
