Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud
Imports Rhino.Geometry

Public Class Util_Average_OBSOLETE
    Inherits GH_Component

    Sub New()
        MyBase.New("Average", "Average", "Get the average point from a cloud.", "Volvox", "Analysis")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("192e5ca1-7a31-4ba5-a94b-ff5a15680576")
        End Get
    End Property


    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Average
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to calculate average on", GH_ParamAccess.item)
        Dim thisparam As Param_Cloud = pManager.Param(0)
        thisparam.Hidden = True
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
