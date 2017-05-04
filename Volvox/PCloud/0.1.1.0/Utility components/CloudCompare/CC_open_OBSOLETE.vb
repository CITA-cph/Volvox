Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class CC_open_OBSOLETE
    Inherits GH_Component

    Public Overrides Sub AddedToDocument(document As GH_Document)
        If MySettings.PyScripts Is Nothing Then CC_Python.LoadPython()
        MyBase.AddedToDocument(document)
    End Sub

    Public Sub New()
        MyBase.New("Open File", "OpenCC", "Open file in CloudCompare." & vbCrLf & "http://cloudcompare.org/", "Volvox", "CloudCompare")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("f1b3cdda-8c79-454f-83fb-eba02954363b")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_OpenCC
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddTextParameter("File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Run", "R", "Run script", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)

    End Sub

    Dim outfile As New String("")

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim filepath As New String("")
        Dim runbool As Boolean

        If Not DA.GetData(0, filepath) Then Return
        If Not DA.GetData(1, runbool) Then Return


        If runbool Then
            outfile = MySettings.PyScripts.CloudCompare.open(filepath)
        End If

    End Sub

End Class
