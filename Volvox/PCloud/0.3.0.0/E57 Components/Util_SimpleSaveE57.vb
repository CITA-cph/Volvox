Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class Util_SimpleSaveE57
    Inherits GH_Component

    Sub New()
        MyBase.New("Save E57", "SaveE57", "Save E57 file.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("eb706e4b-6329-4093-87ae-25eeaf8a0fa6")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_SaveE57())
        pManager.AddParameter(New Param_Cloud(), "Scans", "S", "Point clouds to save in the file", GH_ParamAccess.list)
        pManager.AddBooleanParameter("Spherical", "O", "Save as spherical coordinates", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Run", "R", "Save the file", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_SaveE57
        End Get
    End Property

    Private Function RetrieveData(ByRef Da As IGH_DataAccess) As Boolean
        If Not (Da.GetData(0, X.fpath)) Then Return False

        X.fpath = RemoveIllegal(X.fpath)
        X.fpath = ValidatePath(X.fpath)

        If Utils_IO.IsFileLocked(New FileInfo(X.fpath)) Then Return False
        If Not (Da.GetData(2, X.spher)) Then Return False

        Dim ghc As New List(Of GH_Cloud)

        If Not (Da.GetDataList(1, ghc)) Then Return False

        For i As Integer = 0 To ghc.Count - 1 Step 1
            X.fpclo.Add(ghc(i).Value)
            X.spose.Add(ghc(i).ScannerPosition)
        Next

        For i As Integer = 0 To X.fpclo.Count - 1 Step 1
            If X.fpclo(i) Is Nothing Then Return False
        Next

        For i As Integer = 0 To X.fpclo.Count - 1 Step 1
            X.sguid.Add(Guid.NewGuid.ToString)
        Next

        Return True
    End Function

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
    End Sub

    Private run As Boolean = False
    Private abort As Boolean = False
    Private WithEvents X As E57_CloudExporter = Nothing
    Private done As Boolean = False

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        If Not (DA.GetData(3, run)) Then Return
        If Not (DA.GetData(4, abort)) Then Return

        If X Is Nothing Then
            If run Then
                done = False
                X = New E57_CloudExporter(Me)
                If RetrieveData(DA) Then
                    Me.Message = "Saving..."
                    X.SimpleStart()
                Else
                    Me.Message = "Failed to retrieve the data"
                    X.Dispose()
                    X = Nothing
                End If
            End If
        Else
            If X.IsRunning Then
                If abort Then
                    done = False
                    X.Abort()
                    X.Dispose()
                    X = Nothing
                    Me.Message = "Aborted"
                End If
            Else
                X.Dispose()
                X = Nothing
                done = False
                Me.Message = ""
            End If
        End If

        If done Then
            If X IsNot Nothing Then
                X.Dispose()
                X = Nothing
            End If
            done = False
        End If

    End Sub

    Dim Expire As Action = AddressOf ExpireComponent

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

    Private Sub XFinished() Handles X.FinishedLoading
        done = True
        Rhino.RhinoApp.MainApplicationWindow.Invoke(Expire)
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of String), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add("")
        Next
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of Boolean), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add(False)
        Next
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of Date), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add(Date.Now)
        Next
    End Sub

    Private Sub AddEmpty(ByRef L As List(Of Double), Count As Integer)
        For i As Integer = 0 To Count - 1 Step 1
            L.Add(Double.NaN)
        Next
    End Sub



End Class
