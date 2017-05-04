Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Parameters

Public Class Eng_LoadCloud

    Inherits GH_Component

    Sub New()
        MyBase.New("Load", "Load", "Load cloud within the engine.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Instr_LoadXYZ
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.senary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_loadengine
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddTextParameter("Mask", "M", "Mask", GH_ParamAccess.item, "x y z")
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load", GH_ParamAccess.item, 1)
        pManager.AddIntegerParameter("Seed", "S", "Random seed", GH_ParamAccess.item, 123)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to load", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As New String("")
        Dim fmask As New String("")
        Dim fperc As Double
        Dim fseed As Integer

        If Not DA.GetData(0, fpath) Then Return
        If Not DA.GetData(1, fmask) Then Return
        If Not DA.GetData(2, fperc) Then Return
        If Not DA.GetData(3, fseed) Then Return

        Dim pc As New PointCloud()

        pc.UserDictionary.Set("VolvoxFilePath", fpath)
        pc.UserDictionary.Set("VolvoxFileMask", fmask)
        pc.UserDictionary.Set("VolvoxFilePercent", fperc)
        pc.UserDictionary.Set("VolvoxFileSeed", fseed)

        DA.SetData(0, pc)

    End Sub

End Class
