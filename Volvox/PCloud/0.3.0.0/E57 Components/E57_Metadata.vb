Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports E57LibReader
Imports System.IO
Imports System.Drawing
Imports Rhino.Geometry

Public Class E57_Metadata
    Inherits GH_Component

    Sub New()
        MyBase.New("E57 Metadata", "E57Meta", "Read E57 file metadata.", "Volvox", "I/O")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("5eb18521-aa54-475e-b721-e3ac76438c76")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_E57Metadata
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "FilePath", "F", "E57 File Path", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("Format Name", "Format", "Shall contain the String ""ASTM E57 3D Imaging Data File""", GH_ParamAccess.item)
        pManager.AddTextParameter("File Guid", "Guid", "File Guid", GH_ParamAccess.item)
        pManager.AddTextParameter("Version Major", "Major", "The major version number of the file format.", GH_ParamAccess.item)
        pManager.AddTextParameter("Version Minor", "Minor", "The minor version number of the file format.", GH_ParamAccess.item)
        pManager.AddTextParameter("Library Version", "Library", "The version identifier for the E57 file format library that wrote the file.", GH_ParamAccess.item)
        pManager.AddTimeParameter("Date", "Date", "Date and time that the file was created.", GH_ParamAccess.item)
        pManager.AddTextParameter("Metadata", "Metadata", "Information describing the Coordinate Reference System to be used for the file.", GH_ParamAccess.item)
        pManager.AddTextParameter("Scans", "Scans", "Scans' names.", GH_ParamAccess.list)
        pManager.AddPlaneParameter("Poses", "Poses", "Poses", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Scan Count", "Count", "Number of scans.", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim fp As String = String.Empty
        If Not DA.GetData(0, fp) Then Return

        If Not File.Exists(fp) Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File doesn't exist.")
        End If

        Dim doc As New e57Document(fp, False)

        DA.SetData(0, doc.FormatName.Text)
        DA.SetData(1, doc.Guid.Text)
        DA.SetData(2, doc.VersionMajor.Value)
        DA.SetData(3, doc.VersionMinor.Value)
        If doc.e57LibraryVersion IsNot Nothing Then DA.SetData(4, doc.e57LibraryVersion.Text)
        If doc.Date IsNot Nothing Then DA.SetData(5, E57LibCommon.Maths.GPSTime(doc.Date.Value))
        If doc.CoordinateMetaData IsNot Nothing Then DA.SetData(6, doc.CoordinateMetaData.Text)

        Dim names As New List(Of String)
        Dim poses As New List(Of Plane)

        For i As Integer = 0 To doc.ScanCount - 1 Step 1
            names.Add(doc.Scans(i).Name)
            poses.Add(GetPlane(doc.Scans(i)))
        Next

        DA.SetDataList(7, names)
        DA.SetDataList(8, poses)


        DA.SetData(9, doc.ScanCount)
    End Sub

    Function GetPlane(s As Scan) As Plane

        Dim trans() As Double = s.Translation
        Dim rot() As Double = s.Rotation

        Dim rq As New Quaternion(rot(0), rot(1), rot(2), rot(3))

        Dim ang As Double
        Dim axi As New Vector3d
        rq.GetRotation(ang, axi)

        Dim thistrans As Rhino.Geometry.Transform = Rhino.Geometry.Transform.Translation(trans(0), trans(1), trans(2))
        Dim thisrotation As Rhino.Geometry.Transform = Rhino.Geometry.Transform.Rotation(ang, axi, New Point3d(0, 0, 0))
        If Not thisrotation.IsValid Then thisrotation = Transform.Identity

        Dim pos As New Plane(Plane.WorldXY)

        pos.Transform(thisrotation)
        pos.Transform(thistrans)

        Return pos

    End Function


End Class
