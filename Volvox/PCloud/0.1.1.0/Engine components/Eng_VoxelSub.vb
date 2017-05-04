Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Instr

Public Class Eng_VoxelSub
    Inherits GH_Component

    Sub New()
        MyBase.New("Voxel Subsampling", "VSub", "Apply voxel based spatial subsampling to the Point Cloud.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_VoxelSub
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_VoxelSub
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddNumberParameter("Size", "S", "Voxel size", GH_ParamAccess.item, -1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim size As Double

        If Not DA.GetData(0, size) Then Return

        DA.SetData(0, New Instr_VoxelSub(size))

    End Sub

End Class
