Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports Rhino.Geometry

Public Class Screenshot
    Inherits GH_Component
    Sub New()
        MyBase.New("Screenshot", "Screenshot", "Save screenshot to the project folder", "Volvox", "Diff")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("fdb29f13-ff74-4a4a-8aab-b22a056f89ab")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddTextParameter("Project Folder", "F", "Project folder", GH_ParamAccess.item)
        pManager.AddGeometryParameter("BoundingBox", "B", "Geometry to generate bounding box from", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Width", "W", "Width", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Height", "H", "Height", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Run", "R", "Run", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("Filename", "F", "Filename", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim dorun As Boolean
        Dim projectfolder As String = String.Empty
        Dim bmpw As Integer
        Dim bmph As Integer
        Dim geom As New List(Of GeometryBase)

        If Not DA.GetData(0, projectfolder) Then Return
        If Not DA.GetDataList(1, geom) Then Return
        If Not DA.GetData(2, bmpw) Then Return
        If Not DA.GetData(3, bmph) Then Return
        If Not DA.GetData(4, dorun) Then Return

        Dim cumul As New BoundingBox
        If geom.Count = 0 Then Return

        For Each g As GeometryBase In geom
            cumul.Union(g.GetBoundingBox(False))
        Next

        If Not dorun Then Return

        Dim target As String = projectfolder & "/Screenshots"

        If Not Directory.Exists(target) Then Directory.CreateDirectory(target)

        Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.ZoomBoundingBox(cumul)
        Dim b As Bitmap = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.CaptureToBitmap(New Size(bmpw, bmph), False, False, False)
        Dim finalpath As String = target & "/Screenshot" & Directory.EnumerateFiles(target).Count & ".png"
        b.Save(finalpath)

        DA.SetData(0, finalpath)
    End Sub



End Class
