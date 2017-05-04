Imports Volvox_Instr
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types

Public Class Instr_BoxCrop

    Inherits Instr_BaseReporting

    Private Property BoxCrop As New List(Of Rhino.Geometry.Box)

    Sub New(B As List(Of Rhino.Geometry.Box))
        Dim Boxes() As Box = B.ToArray
        Dim Volumes(Boxes.Length - 1) As Double
        For i As Integer = 0 To Boxes.GetUpperBound(0) Step 1
            Volumes(i) = Boxes(i).Volume
        Next

        Array.Sort(Volumes, Boxes)
        Array.Reverse(Boxes)
        BoxCrop.AddRange(Boxes)
    End Sub

    Sub New()
        BoxCrop = New List(Of Rhino.Geometry.Box)
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return GuidsRelease1.Instr_BoxCrop
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Box Crop"
        End Get
    End Property

    Dim GlobalCloud As PointCloud = Nothing
    Dim NewCloud As PointCloud = Nothing
    Dim ThreadList As New List(Of Threading.Thread)
    Dim CloudPieces() As PointCloud = Nothing
    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim PointCounter As Integer = 0
    Dim LastPercentReported As Integer = 0

    Public Overrides Function Duplicate() As IGH_Goo

        Dim nl As New List(Of Box)(BoxCrop)

        Dim ni As New Instr_BoxCrop(nl)
        ni.GlobalCloud = Nothing
        ni.NewCloud = Nothing
        ni.ThreadList = New List(Of Threading.Thread)
        ni.CloudPieces = Nothing
        ni.PointCounter = 0
        ni.LastPercentReported = 0

        Return ni

    End Function

    Public Overrides Function Execute(ByRef PointCloud As PointCloud) As Boolean

        LastPercentReported = 0
        PointCounter = 0
        ThreadList.Clear()
        NewCloud = New PointCloud
        CloudPieces = Nothing

        ReDim CloudPieces(ProcCount - 1)

        GlobalCloud = PointCloud

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf CropAction)
            nt.IsBackground = True
            ThreadList.Add(nt)
        Next

        For i As Integer = 0 To ProcCount - 1 Step 1
            ThreadList(i).Start(i)
        Next

        For Each t As Threading.Thread In ThreadList
            t.Join()
        Next

        GlobalCloud.Dispose()
        PointCloud.Dispose()

        For Each pc As PointCloud In CloudPieces
            If pc IsNot Nothing Then NewCloud.Merge(pc)
        Next

        PointCloud = NewCloud.Duplicate
        CloudPieces = Nothing
        NewCloud.Dispose()
        ThreadList.Clear()

        Return True

    End Function

    Sub CropAction(MyIndex As Integer)

        Dim MyCloud As New PointCloud

        Dim i0 As Integer = MyIndex * Math.Ceiling(GlobalCloud.Count / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(GlobalCloud.Count / ProcCount) - 1, GlobalCloud.Count - 1)

        Dim totc As Double = 1 / GlobalCloud.Count

        For i As Integer = i0 To i1 Step 1

            PointCounter += 1

            If LastPercentReported < ((PointCounter * totc) * 100) Then
                LastPercentReported = 5 * Math.Ceiling((PointCounter * totc) * 20)
                Me.ReportPercent = LastPercentReported
            End If

            For j As Integer = 0 To BoxCrop.Count - 1 Step 1
                Dim GlobalCloudItem As PointCloudItem = GlobalCloud.Item(i)
                If Math_Utils.IsInBox(GlobalCloudItem.Location, BoxCrop(j)) Then
                    MyCloud.AppendNew()
                    Dim MyCloudItem As PointCloudItem = MyCloud.Item(MyCloud.Count - 1)
                    MyCloudItem.Location = GlobalCloudItem.Location
                    If GlobalCloud.ContainsColors Then MyCloudItem.Color = GlobalCloudItem.Color
                    If GlobalCloud.ContainsNormals Then MyCloudItem.Normal = GlobalCloudItem.Normal
                    Exit For
                End If
            Next
        Next

        CloudPieces(MyIndex) = MyCloud

    End Sub

    Public Overrides Sub Abort()

        For i As Integer = 0 To ThreadList.Count - 1 Step 1
            Dim ThisThread As Threading.Thread = ThreadList(i)
            If Not ThisThread Is Nothing Then
                ThisThread.Abort()
            End If
        Next

        CloudPieces = Nothing
        GlobalCloud = Nothing
        ThreadList.Clear()
        If NewCloud IsNot Nothing Then NewCloud.Dispose()

    End Sub

End Class
