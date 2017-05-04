Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports Rhino.Geometry

Public Class Util_Average
    Inherits GH_Component

    Sub New()
        MyBase.New("Average", "Average", "Get the average point from a cloud.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_UtilAverage
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Average
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to calculate average on", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
        pManager.AddNumberParameter("Percent", "%", "Percent of points to average", GH_ParamAccess.item, 1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddPointParameter("Point", "P", "Point", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cl As GH_Cloud = Nothing
        If Not DA.GetData(0, cl) Then Return
        If Not cl.IsValid Then Return

        Dim X As Double
        Dim Y As Double
        Dim Z As Double


        For i As Integer = 0 To cl.Value.Count - 1 Step 1
            X += cl.Value(i).X
            Y += cl.Value(i).Y
            Z += cl.Value(i).Z
        Next

        X = X / cl.Value.Count
        Y = Y / cl.Value.Count
        Z = Z / cl.Value.Count

        Dim pt As New Point3d(X, Y, Z)

        DA.SetData(0, pt)

    End Sub
End Class
