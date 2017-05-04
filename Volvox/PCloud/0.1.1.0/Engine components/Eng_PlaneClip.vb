Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Instr
Imports Rhino.Geometry

Public Class Eng_PlaneClip
    Inherits GH_Component

    Sub New()
        MyBase.New("Plane Clip", "PClip", "Cull points below the plane.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_PlaneClip
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_PlaneClip
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddPlaneParameter("Plane", "P", "Clipping plane", GH_ParamAccess.item, Plane.WorldXY)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cplane As New Rhino.Geometry.Plane

        If Not DA.GetData(0, cplane) Then Return

        DA.SetData(0, New Instr_PlaneClip(cplane))

    End Sub

End Class
