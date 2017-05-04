Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class CC_Command_OBSOLETE
    Inherits GH_Component

    Public Overrides Sub AddedToDocument(document As GH_Document)
        If MySettings.PyScripts Is Nothing Then CC_Python.LoadPython()
        MyBase.AddedToDocument(document)
    End Sub

    Public Sub New()
        MyBase.New("Command Line", "Command", "CloudCompare command line." & vbCrLf & "http://cloudcompare.org/" & vbCrLf & "See commands at http://www.danielgm.net/cc/doc/wiki/index.php5?title=CommandLine", "Volvox", "CloudCompare")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("8b40ec5f-88f7-4831-87de-5dc4603c13c3")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Command
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddTextParameter("Commands", "C", "Cloud Compare commands to execute", GH_ParamAccess.list)
        pManager.AddBooleanParameter("Run", "R", "Run script", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("OutMessage", "Out", "Message", GH_ParamAccess.item)
    End Sub

    Dim OutMessage As New String("")

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim commands As New List(Of String)
        Dim runbool As Boolean

        If Not DA.GetDataList(0, commands) Then Return
        If Not DA.GetData(1, runbool) Then Return

        If runbool Then
            OutMessage = MySettings.PyScripts.CloudCompare.CCcommand(commands)
        End If

        DA.SetData(0, OutMessage)
    End Sub

End Class
