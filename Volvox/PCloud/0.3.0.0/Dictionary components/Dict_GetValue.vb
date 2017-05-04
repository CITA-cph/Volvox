Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Cloud

Public Class Dict_GetValue
    Inherits GH_Component

    Sub New()
        MyBase.New("Get Value", "GetValue", "Get data value stored in a cloud.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("0734a51c-ed49-41ae-8f20-d041c78a9d7f")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_GetDataItem
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to manipulate", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Indices", "I", "List of indices", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddNumberParameter("Values", "V", "User data values", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim strdc As String = Nothing
        Dim pc As GH_Cloud = Nothing
        Dim il As New List(Of Integer)


        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, strdc) Then Return
        If Not DA.GetDataList(2, il) Then Return

        Dim nl As New List(Of Double)
        For i As Integer = 0 To il.Count - 1 Step 1
            nl.Add(pc.Value.UserDictionary(strdc)(il(i)))
        Next
        DA.SetDataList(0, nl)
    End Sub

End Class
