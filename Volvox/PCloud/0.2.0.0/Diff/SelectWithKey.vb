Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Types

Public Class SelectWithKey
    Inherits GH_Component
    Sub New()
        MyBase.New("GUIDSelect", "GUIDSelect", "Pick objects by GUID", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("baef8147-20e8-4e6b-ba7b-921f3d04b739")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddGenericParameter("Data", "D", "Data to choose from", GH_ParamAccess.list)
        pManager.AddTextParameter("GUIDs", "G", "GUIDs list", GH_ParamAccess.list)
        pManager.AddTextParameter("Pick", "P", "Pick values with those GUIDs", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddGenericParameter("Data", "D", "Data to choose from", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Indices", "I", "Indices of the choosen elements", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim p As New List(Of String)
        If Not DA.GetDataList(2, p) Then Return

        Dim g As New List(Of IGH_Goo)
        If Not DA.GetDataList(0, g) Then Return

        Dim k As New List(Of String)
        If Not DA.GetDataList(1, k) Then Return

        If k.Count <> g.Count Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Key.count <> Data.count")
            Return
        End If

        Dim outi As New List(Of Integer)
        Dim outd As New List(Of IGH_Goo)

        For i As Integer = 0 To p.Count - 1 Step 1
            For j As Integer = 0 To k.Count - 1 Step 1
                If k(j) = p(i) Then
                    outi.Add(j)
                    outd.Add(g(j))
                End If
            Next
        Next

        DA.SetDataList(0, outd)
        DA.SetDataList(1, outi)


    End Sub
End Class
