Imports Volvox_Instr
Imports Rhino.Geometry
Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel.Types
Imports System.Threading

Public Class Instr_LoadMulti
    Inherits Instr_BaseReporting

    Private Property MyFilePath As String
    Private Property MyMask As String
    Private Property MyPercent As Double
    Private Property MySeed As Integer

    Sub New(Filepath As String, Mask As String, Percent As Double, Seed As Integer)
        MyFilePath = Filepath
        MyMask = Mask
        MyPercent = MyPercent
        MySeed = Seed
    End Sub

    Sub New()
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return New Guid("f95e16fc-8e16-4f47-8708-230494c1210c")
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Load Cloud"
        End Get
    End Property

    Public Overrides Function Duplicate() As IGH_Goo

        Dim ni As New Instr_LoadMulti(MyFilePath, MyMask, MyPercent, MySeed)

        ni.LinesCounter = 0
        ni.ThreaderList = New List(Of Threading_Load)

        Return ni

    End Function

    Dim LinesCounter As Integer = 0
    Dim ThreaderList As New List(Of Threading_Load)

    Public Overrides Function Execute(ByRef PointCloud As PointCloud) As Boolean

        MyFilePath = PointCloud.UserDictionary.GetString("VolvoxFilePath")
        MyMask = PointCloud.UserDictionary.GetString("VolvoxFileMask")
        MyPercent = PointCloud.UserDictionary.GetDouble("VolvoxFilePercent")
        MySeed = PointCloud.UserDictionary.GetInteger("VolvoxFileSeed")

        If Not File.Exists(MyFilePath) Then
            Me.ReportCustom = "File doesn't exists"
            Return False
        End If

        Dim pc As PointCloud = LoadPoints()

        If pc Is Nothing Then Return False

        PointCloud.Dispose()
        PointCloud = pc.Duplicate
        pc.Dispose()
        Return True

    End Function

    Function LoadPoints() As PointCloud

        Dim seedcounter As Integer = 0

        Thread.Sleep(10)

        If Utils_IO.IsFileLocked(New FileInfo(MyFilePath)) Then
            Return Nothing
        End If

        Using sread As StreamReader = New StreamReader(MyFilePath)
            Dim count As Integer
            Dim templist As New List(Of String)

            Do
                If seedcounter >= Int32.MaxValue - 1000 Then seedcounter = -seedcounter
                seedcounter += 321


                Dim templine As String = sread.ReadLine
                If templine Is Nothing Then
                    If templist.Count > 0 Then
                        Dim nth As New Threading_Load(New List(Of String)(templist))
                        SyncLock ThreaderList
                            ThreaderList.Add(nth)
                            ThreaderList(ThreaderList.Count - 1).Start(MyMask, MyPercent, MySeed + seedcounter)
                        End SyncLock
                        templist.Clear()
                        count = 0
                    End If

                    Exit Do
                Else

                    templist.Add(templine)
                    count += 1

                    If count = 10000 Then
                        Dim nth As New Threading_Load(New List(Of String)(templist))
                        SyncLock ThreaderList
                            ThreaderList.Add(nth)
                            ThreaderList(ThreaderList.Count - 1).Start(MyMask, MyPercent, MySeed + seedcounter)
                        End SyncLock

                        LinesCounter += 10000
                        Me.ReportCustom = LinesCounter & " pts loaded"

                        templist.Clear()
                        count = 0
                    End If

                End If
            Loop

        End Using

        SyncLock ThreaderList
            For Each t As Threading_Load In ThreaderList
                t.t.Join()
            Next
        End SyncLock

        Dim newpointcloud As New PointCloud

        For Each t As Threading_Load In ThreaderList
            newpointcloud.Merge(t.outcloud)
        Next

        Return newpointcloud

    End Function

    Public Overrides Sub Abort()

        For i As Integer = 0 To ThreaderList.Count - 1 Step 1
            Dim ThisThread As Threading_Load = ThreaderList(i)
            If Not ThisThread Is Nothing Then
                ThisThread.Abort()
            End If
        Next

    End Sub

End Class
