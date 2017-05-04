Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class CC_XYZCommand_OBSOLETE
    Inherits GH_Component

    Public Overrides Sub AddedToDocument(document As GH_Document)
        If MySettings.PyScripts Is Nothing Then CC_Python.LoadPython()
        MyBase.AddedToDocument(document)
    End Sub

    Public Sub New()
        MyBase.New("Command Line .xyz", "Command .xyz", "Cloud Compare command line with xyz file." & vbCrLf & "http://cloudcompare.org/" & vbCrLf & "See commands at http://www.danielgm.net/cc/doc/wiki/index.php5?title=CommandLine", "Volvox", "CloudCompare")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("12bda3df-6728-4858-8326-41386d07e7f3")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_CommandXYZ
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddTextParameter("File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddTextParameter("Commands", "C", "Cloud Compare commands to execute", GH_ParamAccess.list)
        pManager.AddTextParameter("Name", "N", "File name", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Run", "R", "Run script", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("File path", "F", "New file path", GH_ParamAccess.item)
    End Sub

    Dim outfile As New String("")

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim filepath As New String("")
        Dim commands As New List(Of String)
        Dim outname As New String("")
        Dim runbool As Boolean

        If Not DA.GetData(0, filepath) Then Return
        If Not DA.GetDataList(1, commands) Then Return
        If Not DA.GetData(2, outname) Then Return
        If Not DA.GetData(3, runbool) Then Return

        If runbool Then
            outfile = MySettings.PyScripts.CloudCompare.CCcommandXYZ(filepath, commands, outname)
        End If

        DA.SetData(0, outfile)
    End Sub

End Class
