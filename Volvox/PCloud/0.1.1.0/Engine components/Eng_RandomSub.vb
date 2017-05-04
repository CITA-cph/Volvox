Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Instr

Public Class Eng_RandomSub
    Inherits GH_Component

    Sub New()
        MyBase.New("Random Subsampling", "RSub", "Apply random subsampling to the Point Cloud.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_RandomSub
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_RandomSub
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to leave, 0 to 1 domain", GH_ParamAccess.item, 0.5)
        pManager.AddIntegerParameter("Seed", "S", "Random seed", GH_ParamAccess.item, 123)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim perc As Double
        Dim seed As Integer

        If Not DA.GetData(0, perc) Then Return
        If Not DA.GetData(1, seed) Then Return

        DA.SetData(0, New Instr_RandomSub(perc, seed))
    End Sub

End Class
