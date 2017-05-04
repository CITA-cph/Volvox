Imports System.IO
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters

Public Class RdfGUIDs
    Inherits GH_Component

    Sub New()
        MyBase.New("RdfGUIDs", "RdfGUIDs", "Get all GUIDS from an association .rdf file.", "Volvox", "Diff")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("ea715570-cabc-4127-8a23-237b93151239")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "Rdf", "R", "Association .rdf file", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("GUIDS", "G", "GUIDS", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim rdf As String = String.Empty
        If Not DA.GetData(0, rdf) Then Return


        Dim gs As New List(Of String)

        Using str As StreamReader = New StreamReader(rdf)

            Dim thisname As String = String.Empty
            Dim thisguid As String = String.Empty

            While Not str.EndOfStream
                Dim s As String = str.ReadLine

                If s.Contains("<rel:subsetRepOf>") Then
                    gs.Add(GetGuid(s))
                End If

            End While

        End Using

        DA.SetDataList(0, gs)

    End Sub

    Function GetGuid(s As String) As String
        '  <rel:subsetRepOf> "11dNe1_A9DlhgcQ6g03dGj"^^<xsd:string> .
        s = s.Split(Chr(34))(1)
        Dim ss() As String = s.Split(" ")

        Return ss(0)
    End Function

End Class
