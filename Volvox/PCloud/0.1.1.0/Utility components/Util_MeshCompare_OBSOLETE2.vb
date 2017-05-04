Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Drawing
Imports Volvox_Cloud

Public Class Util_MeshCompare_OBSOLETE2
    Inherits GH_Component

    Sub New()
        MyBase.New("Mesh Compare", "MCompare", "Display distance to mesh.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_MeshCompare
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Compare
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to compare", GH_ParamAccess.item)
        pManager.AddMeshParameter("Mesh", "M", "Mesh to compare", GH_ParamAccess.item)
        pManager.AddIntervalParameter("Interval", "I", "Interval", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Colored cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pc As PointCloud = Nothing
        Dim m As Mesh = Nothing
        Dim it As Interval = Nothing

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, m) Then Return
        If Not DA.GetData(2, it) Then Return

        For Each p As PointCloudItem In pc

            Dim p3 As Point3d = p.Location
            Dim pm As New Point3d
            Dim pv As New Vector3d

            m.ClosestPoint(p3, pm, pv, 0)
            Dim normalPlane As New Plane(pm, pv)
            Dim d As Double = normalPlane.DistanceTo(p3)
            Dim ncol As New Rhino.Display.ColorHSL(1, 0.5, 1, 0.5)

            Dim col As New Color
            Select Case it.T0 < it.T1
                Case True
                    Select Case d
                        Case Is < it.T0
                            col = Color.Black
                        Case Is > it.T1
                            col = Color.White
                        Case Else
                            ncol.H = it.NormalizedParameterAt(d) * 0.333
                            col = ncol.ToArgbColor
                    End Select
                Case False
                    Select Case d
                        Case Is < it.T1
                            col = Color.White
                        Case Is > it.T0
                            col = Color.Black
                        Case Else
                            ncol.H = it.NormalizedParameterAt(d) * 0.333
                            col = ncol.ToArgbColor
                    End Select
            End Select


            p.Color = col
        Next

        DA.SetData(0, pc)
    End Sub

End Class
