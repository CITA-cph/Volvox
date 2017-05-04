Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Dict_AddDictionary
    Inherits GH_Component

    Sub New()
        MyBase.New("Add Data", "AddData", "Add user data to a cloud.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictAddDictionary
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_AddData
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to manipulate", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
        pManager.AddNumberParameter("Data", "D", "User data", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Modified cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim vals As New List(Of Double)
        Dim strdc As String = Nothing
        Dim pc As PointCloud = Nothing

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, strdc) Then Return
        If Not DA.GetDataList(2, vals) Then Return

        pc = pc.Duplicate

        If strdc.Length < 1 Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Key has to have at least one character.")
            Return
        End If

        If vals.Count < 1 Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Data has to have at least one value.")
            Return
        End If

        pc.UserDictionary.Set(strdc, vals)
        DA.SetData(0, pc)

    End Sub

End Class
