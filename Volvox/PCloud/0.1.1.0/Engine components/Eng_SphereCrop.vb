Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Instr
Imports Rhino.Geometry

Public Class Eng_SphereCrop
    Inherits GH_Component

    Sub New()
        MyBase.New("Sphere Crop", "SCrop", "Cull points outsite of the sphere.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_SphereCrop
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SphereCrop
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Center", "C", "Center of the sphere.", GH_ParamAccess.list)
        pManager.AddNumberParameter("Radius", "R", "Radius of the sphere.", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim c As New List(Of Point3d)
        Dim r As New List(Of Double)

        If Not DA.GetDataList(0, c) Then Return
        If Not DA.GetDataList(1, r) Then Return

        If c.Count > 1 And r.Count = 1 Then
            For i As Integer = 1 To c.Count - 1 Step 1
                r.Add(r(0))
            Next
        End If

        If r.Count > 1 And c.Count = 1 Then
            For i As Integer = 1 To r.Count - 1 Step 1
                c.Add(c(0))
            Next
        End If

        DA.SetData(0, New Instr_SphereCrop(c, r))

    End Sub

End Class
