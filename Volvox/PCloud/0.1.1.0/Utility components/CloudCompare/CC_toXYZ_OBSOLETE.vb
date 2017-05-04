Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class CC_toXYZ_OBSOLETE
    Inherits GH_Component

    Public Overrides Sub AddedToDocument(document As GH_Document)
        If MySettings.PyScripts Is Nothing Then CC_Python.LoadPython()
        MyBase.AddedToDocument(document)
    End Sub

    Public Sub New()
        MyBase.New("Convert .xyz", "Convert", "CloudCompare convert to XYZ." & vbCrLf & "http://cloudcompare.org/", "Volvox", "CloudCompare")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("31642049-ab6b-4f3f-b4d5-3102619fdca7")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Convert
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddTextParameter("File Path", "F", "File to convert with CloudCompare to XYZ format.", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Run", "R", "Run script", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("File path", "F", "New file path", GH_ParamAccess.item)
    End Sub

    Dim outfile As New String("")

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim filepath As New String("")
        Dim outname As New String("")
        Dim runbool As Boolean

        If Not DA.GetData(0, filepath) Then Return
        If Not DA.GetData(1, runbool) Then Return

        If runbool Then
            outfile = MySettings.PyScripts.CloudCompare.toXYZ(filepath)
        End If

        DA.SetData(0, outfile)
    End Sub

End Class
