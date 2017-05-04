Imports Grasshopper.Kernel


Public Class Stats
    Inherits GH_Component

    Sub New()
        MyBase.New("Statistics", "Stats", "Data statistics", "Volvox", "Diff")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("422bf971-45ae-4c56-9f34-9e9f59a959ce")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddNumberParameter("Data", "D", "Data", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddNumberParameter("Average", "A", "Average", GH_ParamAccess.item)
        pManager.AddNumberParameter("Standard", "S", "Standard deviation", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim nl As New List(Of Double)

        If Not DA.GetDataList(0, nl) Then Return

        DA.SetData(0, AverageData(nl))
        DA.SetData(1, StandardData(nl))

    End Sub

    Public Function AverageData(d As List(Of Double)) As Double
        Dim av As Double = 0
        For Each n As Double In d
            av += n
        Next
        Return av / d.Count
    End Function

    Public Function VarianceData(d As List(Of Double)) As Double
        Dim av As Double = AverageData(d)

        Dim v As Double
        For i As Integer = 0 To d.Count - 1 Step 1
            v += (d(i) - av) ^ 2
        Next

        Return v / d.Count
    End Function

    Public Function StandardData(d As List(Of Double)) As Double
        Return Math.Sqrt(VarianceData(d))
    End Function
End Class
