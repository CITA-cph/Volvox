Imports System.IO
Imports Grasshopper.Kernel

Public Class ProjectFolder
    Inherits GH_Component

    Sub New()
        MyBase.New("Project Folder", "ProjectFolder", "Create project subfolder", "Volvox", "Diff")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("0d974d3e-b7b7-4d92-a8f7-b1b00c57e0af")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddTextParameter("Directory", "D", "Projects direcotory", GH_ParamAccess.item)
        pManager.AddTextParameter("Project", "P", "Project name", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTextParameter("Project folder", "F", "Current project folder", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim str As String = String.Empty
        Dim pro As String = String.Empty
        If Not DA.GetData(0, str) Then Return
        If Not DA.GetData(1, pro) Then Return

        Dim target As String = str & "\" & pro

        If Not Directory.Exists(target) Then Directory.CreateDirectory(target)

        DA.SetData(0, target)


    End Sub
End Class
