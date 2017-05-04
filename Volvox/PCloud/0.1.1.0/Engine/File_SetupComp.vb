'Imports System.IO
'Imports Grasshopper.Kernel

'Public Class File_SetupComp

'    Inherits GH_Component

'    Sub New()
'        MyBase.New("Setup", "Setup", "Creates setup to be used with File Engine", "Cloud", "Engine")
'    End Sub

'    Public Overrides ReadOnly Property ComponentGuid As Guid
'        Get
'            Return GuidsRelease1.Comp_Setup
'        End Get
'    End Property

'    Public Overrides ReadOnly Property Exposure As GH_Exposure
'        Get
'            Return GH_Exposure.primary
'        End Get
'    End Property

'    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
'        pManager.AddTextParameter("File path", "F", "File to operate on", GH_ParamAccess.item)
'        pManager.AddTextParameter("Save path", "S", "Place to store the new file", GH_ParamAccess.item)
'        pManager.AddNumberParameter("Display count", "D", "Number of points to display, set to -1 to display all", GH_ParamAccess.item, 1000)
'        pManager.AddIntegerParameter("Chunck size", "C", "Chunk size", GH_ParamAccess.item, 10000)
'        pManager.AddTextParameter("Mask", "M", "Source file mask", GH_ParamAccess.item, "x,y,z")
'        pManager.AddTextParameter("Separator", "S", "Separator", GH_ParamAccess.item, " ")
'    End Sub

'    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
'        pManager.AddParameter(New Param_Setup)
'    End Sub

'    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

'        Dim fp As String = String.Empty
'        Dim sf As String = String.Empty
'        Dim di As Double = 1000
'        Dim ch As Integer = 10000
'        Dim ma As String = String.Empty
'        Dim se As String = String.Empty

'        If Not DA.GetData(0, fp) Then Return
'        If Not DA.GetData(1, sf) Then Return
'        If Not DA.GetData(2, di) Then Return
'        If Not DA.GetData(3, ch) Then Return
'        If Not DA.GetData(4, ma) Then Return
'        If Not DA.GetData(5, se) Then Return

'        If Not File.Exists(fp) Then Return

'        DA.SetData(0, New File_Setup(fp, sf, di, ch, ma, se))

'    End Sub

'End Class
