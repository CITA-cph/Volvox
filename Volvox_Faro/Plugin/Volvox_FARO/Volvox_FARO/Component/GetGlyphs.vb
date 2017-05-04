Imports System.Collections.Generic
Imports System.IO
Imports AForge
Imports AForge.Imaging
Imports AForge.Vision.GlyphRecognition
Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Rhino.Geometry
Imports Volvox_Cloud
Imports Grasshopper.Kernel.Parameters

Public Class GetGlyphs
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the MyComponent1 class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Glyph Detection", "GetGly",
                    "Extracts Glyphs and outputs Points and Information on Glypfs",
                    "Volvox", "Glyph")
    End Sub

    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to find Glyphs in. ", GH_ParamAccess.item)
        pManager.AddParameter(New Param_FilePath, "File path", "F", "Image file path", GH_ParamAccess.item)

        pManager.AddTextParameter("Glyph", "G", "Glyphs to search for.", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Max Glyphs", "M", "Maximum number of Glyphs to search for.", GH_ParamAccess.item, 10)

        pManager.AddIntegerParameter("Step", "S", "Subsampling step", GH_ParamAccess.item, 1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_Component.GH_OutputParamManager)
        pManager.AddIntegerParameter("ID", "I", "Glyph ID", GH_ParamAccess.list)
        pManager.AddNumberParameter("Confidence", "C", "Recognition confidence", GH_ParamAccess.list)
        pManager.AddPointParameter("Glyph Points", "P", "Glyph corner points.", GH_ParamAccess.tree)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pth As String = ""
        Dim cloud As GH_Cloud = Nothing
        Dim inGlyphs As New List(Of String)
        Dim maxGlyph As Integer = Nothing
        Dim stp As Integer = 0

        If Not DA.GetData(0, cloud) Then Return
        If Not DA.GetData(4, stp) Then Return
        If Not DA.GetData(1, pth) Then Return
        If Not DA.GetDataList(2, inGlyphs) Then Return
        If Not DA.GetData(3, maxGlyph) Then Return

        Dim pts As New DataTree(Of Point3d)
        Dim conf As New List(Of Double)
        Dim ids As New List(Of Integer)

        GetGlyphs(cloud, inGlyphs, maxGlyph, pth, stp, conf, ids, pts)

        DA.SetDataList(0, ids)
        DA.SetDataList(1, conf)
        DA.SetDataTree(2, pts)
    End Sub

    Private Function EncodeGlyph(text As String, id As Integer) As Glyph

        Dim rows() As String = text.Split(" ")

        Dim arr(rows.Length + 1, rows.Length + 1) As Byte

        For i As Integer = 0 To rows.Length - 1 Step 1
            For j As Integer = 0 To rows.Length - 1 Step 1
                arr(i + 1, j + 1) = Convert.ToByte(If(rows(i).Chars(j) = "0", 0, 1))
            Next
        Next

        Return New Glyph(id.ToString, arr)
    End Function

    Sub GetGlyphs(Cloud As GH_Cloud, searchGlyphs As List(Of String), maxGlyph As Integer, fpath As String, stp As Integer, ByRef Confidence As List(Of Double), ByRef Ids As List(Of Integer), ByRef pts As DataTree(Of Point3d))
        Dim pc As PointCloud = Cloud.Value

        ' import Image
        Dim bitmap As System.Drawing.Bitmap = Image.FromFile(fpath)

        Dim glyphDatabase As New GlyphDatabase(EncodeGlyph(searchGlyphs(0), 0).Size)

        For i As Integer = 0 To searchGlyphs.Count - 1 Step 1
            glyphDatabase.Add(EncodeGlyph(searchGlyphs(i), i))
        Next

        Dim recognizer As New GlyphRecognizer(glyphDatabase)
        recognizer.MaxNumberOfGlyphsToSearch = maxGlyph

        Dim glyphs As List(Of ExtractedGlyphData) = recognizer.FindGlyphs(bitmap)
        Dim height As Integer = bitmap.Height
        Dim counter As Integer = 0

        For Each glyphData As ExtractedGlyphData In glyphs
            If glyphData.RecognizedGlyph IsNot Nothing Then
                Ids.Add(glyphData.RecognizedGlyph.Name)
                Confidence.Add(glyphData.Confidence)

                Dim corners As List(Of IntPoint) = Nothing
                corners = glyphData.RecognizedQuadrilateral
                For Each pt As IntPoint In corners
                    Dim idx As Integer = System.Math.Floor(pt.X / stp) * height + System.Math.Floor(pt.Y / stp)
                    Dim point As Point3d = pc(idx).Location
                    pts.Add(point, New GH_Path(counter))
                Next
                counter += 1
            End If
        Next

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{2b65d4ea-a6bf-4996-9e64-7b39f8db9ff6}")
        End Get
    End Property
End Class