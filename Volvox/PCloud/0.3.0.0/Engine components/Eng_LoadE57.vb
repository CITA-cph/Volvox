Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Parameters

Public Class Eng_LoadE57

    Inherits GH_Component

    Sub New()
        MyBase.New("Load E57", "Load E57", "Load E57 within the engine.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease3.Instr_LoadE57
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quinary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_LoadEngineE57
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load", GH_ParamAccess.item, 1)
        pManager.AddBooleanParameter("Check", "C", "Check if file is corrupted", GH_ParamAccess.item, True)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to load", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As New String("")
        Dim fperc As Double
        Dim fcheck As Boolean = False

        If Not DA.GetData(0, fpath) Then Return
        If Not DA.GetData(1, fperc) Then Return
        If Not DA.GetData(2, fcheck) Then Return

        Dim pc As New PointCloud()

        pc.UserDictionary.Set("VolvoxFilePath", fpath)
        pc.UserDictionary.Set("VolvoxFilePercent", fperc)
        pc.UserDictionary.Set("VolvoxFileCheck", fcheck)

        DA.SetData(0, pc)

    End Sub

End Class
