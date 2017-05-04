Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class CC_E57RandomSub_OBSOLETE
    Inherits GH_Component

    Public Overrides Sub AddedToDocument(document As GH_Document)
        If MySettings.PyScripts Is Nothing Then CC_Python.LoadPython()
        MyBase.AddedToDocument(document)
    End Sub

    Public Sub New()
        MyBase.New("Random Subsampling", "RandomCC", "Run CloudCompare E57 random subsampling." & vbCrLf & "http://cloudcompare.org/", "Volvox", "CloudCompare")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("a1ff5050-3905-4160-b1f9-da8a72e0e2ac")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_RandomCC
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddTextParameter("File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Amount", "A", "Amount of points to keep", GH_ParamAccess.item, 1000)
        pManager.AddTextParameter("Name", "N", "File name", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Run", "R", "Run script", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("File path", "F", "New file path", GH_ParamAccess.item)
    End Sub

    Dim outfile As New String("")

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim filepath As New String("")
        Dim amount As Integer
        Dim outname As New String("")
        Dim runbool As Boolean

        If Not DA.GetData(0, filepath) Then Return
        If Not DA.GetData(1, amount) Then Return
        If Not DA.GetData(2, outname) Then Return
        If Not DA.GetData(3, runbool) Then Return

        If runbool Then
            outfile = MySettings.PyScripts.CloudCompare.SubsampleRandom(filepath, amount, outname)
        End If

        DA.SetData(0, outfile)
    End Sub

End Class
