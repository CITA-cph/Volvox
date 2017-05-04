Imports Volvox_Instr
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types

Public Class Instr_RandomSub
    Inherits Instr_BaseReporting

    Private Property Percent As Double = 0.5
    Private Property Seed As Integer = 2

    Sub New()

    End Sub

    Sub New(perc As Double, rndseed As Integer)
        If perc < 0 Then perc = 0
        If perc > 1 Then perc = 1
        Me.Percent = perc
        Me.Seed = rndseed
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return GuidsRelease1.Instr_RandomSub
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Random Subsampling " & Math.Round(Percent * 100, 2) & "% " & "Seed:" & Seed
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

        Dim ni As New Instr_RandomSub(Percent, Seed)
        ni.GlobalCloud = Nothing
        ni.NewCloud = Nothing
        ni.ThreadList = New List(Of Threading.Thread)
        ni.CloudPieces = Nothing
        ni.PointCounter = 0
        ni.LastPercentReported = 0

        Return ni

    End Function

    Public Overrides Function Execute(ByRef PointCloud As PointCloud) As Boolean
        If Percent = 1 Then Return True

        LastPercentReported = 0
        PointCounter = 0
        ThreadList.Clear()
        NewCloud = New PointCloud
        CloudPieces = Nothing

        ReDim CloudPieces(ProcCount - 1)

        GlobalCloud = PointCloud

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf RandomAction)
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


        'Dim pctemp As New PointCloud()
        'Dim rnd As New Random(Seed)

        'Dim counter As Integer = 0
        'Dim totc As Double = 1 / PointCloud.Count
        'Dim lastpercent As Integer = 0

        'For i As Integer = 0 To PointCloud.Count - 1 Step 1

        '    counter += 1

        '    If lastpercent < ((counter * totc) * 100) Then
        '        lastpercent = 5 * Math.Ceiling((counter * totc) * 20)
        '        Me.ReportPercent = lastpercent
        '    End If

        '    Select Case rnd.NextDouble() + Percent
        '        Case Is > 1
        '            pctemp.AppendNew()
        '            pctemp.Item(pctemp.Count - 1).Location = PointCloud.Item(i).Location
        '            If PointCloud.ContainsColors Then pctemp.Item(pctemp.Count - 1).Color = PointCloud.Item(i).Color
        '            If PointCloud.ContainsNormals Then pctemp.Item(pctemp.Count - 1).Normal = PointCloud.Item(i).Normal
        '    End Select


        'Next

        'PointCloud.Dispose()
        'PointCloud = pctemp

        'Return True
    End Function

    Sub RandomAction(Myindex As Integer)

        Dim i0 As Integer = Myindex * Math.Ceiling(GlobalCloud.Count / ProcCount)
        Dim i1 As Integer = Math.Min((Myindex + 1) * Math.Ceiling(GlobalCloud.Count / ProcCount) - 1, GlobalCloud.Count - 1)

        Dim totc As Double = 1 / GlobalCloud.Count
        Dim rnd As New Random(Seed + Myindex)

        Dim pctemp As New PointCloud()

        For i As Integer = i0 To i1 Step 1

            PointCounter += 1

            If LastPercentReported < ((PointCounter * totc) * 100) Then
                LastPercentReported = 5 * Math.Ceiling((PointCounter * totc) * 20)
                Me.ReportPercent = LastPercentReported
            End If

            Select Case Rnd.NextDouble() + Percent
                Case Is > 1
                    pctemp.AppendNew()
                    pctemp.Item(pctemp.Count - 1).Location = GlobalCloud.Item(i).Location
                    If GlobalCloud.ContainsColors Then pctemp.Item(pctemp.Count - 1).Color = GlobalCloud.Item(i).Color
                    If GlobalCloud.ContainsNormals Then pctemp.Item(pctemp.Count - 1).Normal = GlobalCloud.Item(i).Normal
            End Select

        Next

        CloudPieces(Myindex) = pctemp

        'Dim rnd As New Random(Seed)

        'Dim counter As Integer = 0
        'Dim totc As Double = 1 / PointCloud.Count
        'Dim lastpercent As Integer = 0

        'For i As Integer = 0 To PointCloud.Count - 1 Step 1

        '    counter += 1

        '    If lastpercent < ((counter * totc) * 100) Then
        '        lastpercent = 5 * Math.Ceiling((counter * totc) * 20)
        '        Me.ReportPercent = lastpercent
        '    End If

        '    Select Case rnd.NextDouble() + Percent
        '        Case Is > 1
        '            pctemp.AppendNew()
        '            pctemp.Item(pctemp.Count - 1).Location = PointCloud.Item(i).Location
        '            If PointCloud.ContainsColors Then pctemp.Item(pctemp.Count - 1).Color = PointCloud.Item(i).Color
        '            If PointCloud.ContainsNormals Then pctemp.Item(pctemp.Count - 1).Normal = PointCloud.Item(i).Normal
        '    End Select


        'Next

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
