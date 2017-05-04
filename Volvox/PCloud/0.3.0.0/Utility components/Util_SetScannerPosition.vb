Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Util_SetScannerPosition
    Inherits GH_Component

    Sub New()
        MyBase.New("Set Position", "Set Pose", "Set the scanner position.", "Volvox", "Cloud")
    End Sub

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("5a4f511d-d930-44de-96ee-d712e420f016")
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SetPosition
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Cloud to modify", GH_ParamAccess.item)
        pManager.AddPlaneParameter("Position", "P", "Scanner position", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud(), "Cloud", "C", "Modified Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pcin As PointCloud = Nothing
        Dim pl As New Plane
        If Not DA.GetData(0, pcin) Then Return
        If Not DA.GetData(1, pl) Then Return

        Dim npci As New GH_Cloud(pcin.Duplicate)
        npci.ScannerPosition = pl

        DA.SetData(0, npci)
    End Sub

End Class
