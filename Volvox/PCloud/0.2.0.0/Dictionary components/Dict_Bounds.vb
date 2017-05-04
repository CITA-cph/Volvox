Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Dict_Bounds

    Inherits GH_Component

    Sub New()
        MyBase.New("Bounds", "Bounds", "Get user data bounds.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictBounds
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_bounds
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to manipulate", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddIntervalParameter("Bounds", "B", "Data bounds", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim vals As New List(Of Double)
        Dim strdc As String = Nothing
        Dim pc As GH_Cloud = Nothing

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, strdc) Then Return

        If strdc.Length < 1 Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Key has to have at least one character.")
            Return
        End If

        Select Case pc.Value.UserDictionary(strdc).GetType
            Case GetType(List(Of Integer)), GetType(Integer())

                Dim nl As New List(Of Integer)
                nl.AddRange(pc.Value.UserDictionary(strdc))

                Dim minv As Integer = nl(0)
                Dim maxv As Integer = nl(0)

                For Each n As Integer In nl
                    If n < minv Then minv = n
                    If n > maxv Then maxv = n
                Next

                Dim itv As New Interval(minv, maxv)

                DA.SetData(0, itv)

            Case GetType(List(Of Double)), GetType(Double())

                Dim nl As New List(Of Double)
                nl.AddRange(pc.Value.UserDictionary(strdc))

                Dim minv As Double = nl(0)
                Dim maxv As Double = nl(0)

                For Each n As Double In nl
                    If n < minv Then minv = n
                    If n > maxv Then maxv = n
                Next

                Dim itv As New Interval(minv, maxv)

                DA.SetData(0, itv)

        End Select

    End Sub


End Class
