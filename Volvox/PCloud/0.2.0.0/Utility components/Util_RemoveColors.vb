Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Util_CleanCloud
    Inherits GH_Component

    Sub New()
        MyBase.New("Clean Cloud", "CleanCloud", "Cleans the cloud out of colors.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_CleanCloud
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_UtilRemoveColors
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pcin As PointCloud = Nothing
        If Not DA.GetData(0, pcin) Then Return
        Dim npci As New PointCloud(pcin.GetPoints)
        DA.SetData(0, npci)
    End Sub
End Class
