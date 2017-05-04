Imports Grasshopper.Kernel

Public Class Eng_Preview
    Inherits GH_Component

    Sub New()
        MyBase.New("Preview", "Preview", "Custom preview cloud", "Cloud", "Instructions")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Preview
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddColourParameter("Color", "C", "Preview color", GH_ParamAccess.item, Drawing.Color.Black)
        pManager.AddIntegerParameter("Size", "S", "Point size", GH_ParamAccess.item, 1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim c As System.Drawing.Color
        Dim s As Integer
        DA.GetData(0, c)
        DA.GetData(1, s)

        ' DA.SetData(0, New Instr_Preview(c, s))
    End Sub

End Class