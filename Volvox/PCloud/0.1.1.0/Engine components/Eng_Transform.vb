Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Instr

Public Class Eng_Transform
    Inherits GH_Component

    Sub New()
        MyBase.New("Transform", "Transform", "Apply transformation to the Point Cloud.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Transform
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Transform
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddTransformParameter("Transformation", "T", "Transformation matrix to apply", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim tx As New Rhino.Geometry.Transform

        If Not DA.GetData(0, tx) Then Return

        DA.SetData(0, New Instr_Transform(tx))
    End Sub
End Class
