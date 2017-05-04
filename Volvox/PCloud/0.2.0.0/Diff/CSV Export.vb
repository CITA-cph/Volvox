
Imports System.IO
Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types

Public Class CSV_Export
    Inherits GH_Component
    Sub New()
        MyBase.New("CSV Export", "CSV", "Export data tree as .csv file", "Volvox", "Diff")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("ed3638c7-0332-4ab0-a5c3-e57c3bff641e")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddTextParameter("Project folder", "F", "Project folder", GH_ParamAccess.item)
        pManager.AddTextParameter("File name", "N", "Data name", GH_ParamAccess.item)
        pManager.AddTextParameter("Data", "D", "Data to export", GH_ParamAccess.tree)
        pManager.AddBooleanParameter("Swap", "S", "Swap columns with rows", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Run", "R", "Run", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)

    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim pf As String = String.Empty
        Dim fn As String = String.Empty
        Dim dt As New GH_Structure(Of GH_String)
        Dim run As Boolean
        Dim swap As Boolean

        If Not DA.GetData(4, run) Then Return
        If run = False Then Return
        If Not DA.GetDataTree(2, dt) Then Return
        If Not DA.GetData(0, pf) Then Return
        If Not DA.GetData(1, fn) Then Return
        If Not DA.GetData(3, swap) Then Return

        Dim megastr As String = String.Empty

        If swap Then

            For Each b As List(Of GH_String) In dt.Branches
                Dim raw As New List(Of String)
                For Each ghs As GH_String In b
                    raw.Add(ghs.Value)
                Next

                Dim thisrow As String = String.Empty
                For Each s As String In raw
                    thisrow &= s & ","
                Next

                megastr &= thisrow & vbCrLf
            Next

        Else

            Dim alll As New List(Of List(Of String))
            Dim maxcount As Integer = 0

            For Each b As List(Of GH_String) In dt.Branches
                Dim raw As New List(Of String)
                For Each ghs As GH_String In b
                    raw.Add(ghs.Value)
                Next
                alll.Add(raw)
                If raw.Count > maxcount Then maxcount = raw.Count
            Next

            For i As Integer = 0 To maxcount - 1 Step 1
                Dim thisrow As String = String.Empty
                For j As Integer = 0 To alll.Count - 1 Step 1
                    If i <= alll(j).Count - 1 Then
                        thisrow &= alll(j)(i) & ","
                    Else
                        thisrow &= " ,"
                    End If
                Next
                megastr &= thisrow & vbCrLf
            Next

        End If

        If megastr <> String.Empty Then

            Dim filedir As String = pf & "\SpreadSheets\" & fn & ".csv"
            Directory.CreateDirectory(Path.GetDirectoryName(filedir))
            File.Create(filedir).Dispose()

            Using str As StreamWriter = New StreamWriter(filedir)
                str.Write(megastr)
            End Using

        End If


    End Sub

End Class
