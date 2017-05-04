Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud

Public Class Dict_GetDictionary
    Inherits GH_Component

    Sub New()
        MyBase.New("Get Data", "GetData", "Get data set stored in a cloud.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictGetDictionary
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_GetData
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to manipulate", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddNumberParameter("Values", "V", "User data values", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim strdc As String = Nothing
        Dim pc As GH_Cloud = Nothing

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, strdc) Then Return

        Dim nl As New List(Of Double)

        nl.AddRange(pc.Value.UserDictionary(strdc))

        DA.SetDataList(0, nl)

    End Sub

End Class
