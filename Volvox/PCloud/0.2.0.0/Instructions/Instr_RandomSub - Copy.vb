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

        Dim pctemp As New PointCloud()
        Dim rnd As New Random(Seed)

        Dim counter As Integer = 0
        Dim totc As Double = 1 / PointCloud.Count
        Dim lastpercent As Integer = 0

        For i As Integer = 0 To PointCloud.Count - 1 Step 1

            counter += 1

            If lastpercent < ((counter * totc) * 100) Then
                lastpercent = 5 * Math.Ceiling((counter * totc) * 20)
                Me.ReportPercent = lastpercent
            End If

            Select Case rnd.NextDouble() + Percent
                Case Is > 1
                    pctemp.AppendNew()
                    pctemp.Item(pctemp.Count - 1).Location = PointCloud.Item(i).Location
                    If PointCloud.ContainsColors Then pctemp.Item(pctemp.Count - 1).Color = PointCloud.Item(i).Color
                    If PointCloud.ContainsNormals Then pctemp.Item(pctemp.Count - 1).Normal = PointCloud.Item(i).Normal
            End Select


        Next

        PointCloud.Dispose()
        PointCloud = pctemp

        Return True
    End Function

End Class
