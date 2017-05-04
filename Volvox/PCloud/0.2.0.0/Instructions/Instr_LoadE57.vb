Imports Volvox_Instr
Imports Rhino.Geometry
Imports System.IO
Imports Grasshopper.Kernel.Types
Imports System.Threading

Public Class Instr_LoadE57
    Inherits Instr_BaseReporting

    Private Property MyFilePath As String
    Private Property MyPercent As Double
    Private Property MyCheck As Boolean

    Sub New(Filepath As String, Percent As Double, Check As Boolean)
        MyFilePath = Filepath
        MyPercent = Percent
        MyCheck = Check
    End Sub

    Sub New()
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return New Guid("99bf91c4-2f2c-4c85-ba0f-3905e82b975c")
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Load E57"
        End Get
    End Property

    Public Overrides Function Duplicate() As IGH_Goo
        Dim ni As New Instr_LoadE57(MyFilePath, MyPercent, MyCheck)
        Return ni
    End Function

    WithEvents Reconstructor As E57_CloudConstructor = Nothing

    Public Overrides Function Execute(ByRef PointCloud As PointCloud) As Boolean
        MyFilePath = PointCloud.UserDictionary.GetString("VolvoxFilePath")
        MyPercent = PointCloud.UserDictionary.GetDouble("VolvoxFilePercent")
        MyCheck = PointCloud.UserDictionary.GetBool("VolvoxFileCheck")

        If Not File.Exists(MyFilePath) Then
            Me.ReportCustom = "File doesn't exists"
            Return False
        End If

        Dim pc As PointCloud = LoadPoints()

        If pc Is Nothing Then Return False

        PointCloud.Dispose()

        If pc IsNot Nothing Then
            PointCloud = pc.Duplicate
            pc.Dispose()
        End If

        If Reconstructor IsNot Nothing Then
            Reconstructor.Dispose()
            Reconstructor = Nothing
        End If
        Return True
    End Function


    Function LoadPoints() As PointCloud

        Dim seedcounter As Integer = 0

        Thread.Sleep(10)

        If Utils_IO.IsFileLocked(New FileInfo(MyFilePath)) Then
            Return Nothing
        End If

        Reconstructor = Nothing
        Reconstructor = New E57_CloudConstructor(MyFilePath)
        Dim nl As New List(Of Integer)
        nl.Add(-1)
        Reconstructor.Indices = nl
        Reconstructor.Percent = MyPercent
        Reconstructor.Start(MyCheck)
        Reconstructor.Join()

        Dim pc As New PointCloud
        For i As Integer = 0 To Reconstructor.Clouds.Count - 1 Step 1
            If Reconstructor.Clouds(i) IsNot Nothing Then pc.Merge(Reconstructor.Clouds(i).Value)
        Next

        Return pc
    End Function

    Public Overrides Sub Abort()
        If Reconstructor IsNot Nothing Then
            Reconstructor.Dispose()
            Reconstructor = Nothing
        End If
    End Sub

    Public Sub ReportLoading(sender As Object, e As E57MessageEventArgs) Handles Reconstructor.Message
        Select Case e.Type
            Case E57MessageEventArgs.MessageType.SubPercentReport
                Me.ReportPercent = e.Message
            Case Else
                Me.ReportCustom = e.Message
        End Select

    End Sub


End Class
