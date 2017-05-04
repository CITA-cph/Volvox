Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports E57LibReader
Imports System.IO
Imports System.Drawing

Public Class E57_Metadata_OBSOLETE
    Inherits GH_Component

    Sub New()
        MyBase.New("E57 Metadata", "E57Meta", "Read E57 file metadata.", "Volvox", "I/O")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_E57Metadata
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
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
        pManager.AddTextParameter("Date", "Date", "Date and time that the file was created.", GH_ParamAccess.item)
        pManager.AddTextParameter("Metadata", "Metadata", "Information describing the Coordinate Reference System to be used for the
file.", GH_ParamAccess.item)
        pManager.AddTextParameter("Scans", "Scans", "Scans' names.", GH_ParamAccess.list)
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
        DA.SetData(4, doc.e57LibraryVersion.Text)
        ''missing date
        DA.SetData(6, doc.CoordinateMetaData.Text)
        Dim names As New List(Of String)
        For i As Integer = 0 To doc.ScanCount - 1 Step 1
            names.Add(doc.Scans(i).Name)
        Next
        DA.SetDataList(7, names)
        DA.SetData(8, doc.ScanCount)
    End Sub
End Class
