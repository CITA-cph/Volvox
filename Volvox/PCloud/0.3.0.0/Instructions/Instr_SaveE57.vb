Imports Volvox_Instr
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports System.IO
Imports E57LibWriter
Imports System.Drawing

Public Class Instr_SaveE57
    Inherits Instr_BaseReporting

    Private Property FPath As String

    Sub New(FilePath As String)
        FPath = FilePath
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return New Guid("ec01aa1c-fa17-4e75-8a99-073f6861fa95")
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Save E57"
        End Get
    End Property

    Public Overrides Function duplicate() As IGH_Goo
        Return New Instr_SaveE57(FPath)
    End Function

    Public Overrides Function Execute(ByRef pc As PointCloud) As Boolean

        'Try
        If Not Directory.Exists(Path.GetDirectoryName(Me.FPath)) Then
            Dim sec As New Security.AccessControl.DirectorySecurity()
            Dim dir As DirectoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(Me.FPath))
        End If
        '    File.Create(FPath).Dispose()
        'Catch ex As Exception
        '    Me.ReportCustom = ex.Message
        '    Return False
        'End Try

        If Utils_IO.IsFileLocked(New FileInfo(FPath)) Then Return False

        Dim doc As New e57Document()
        doc.Date = Date.Now
        doc.GenerateGuid()

        Dim sc As New Scan(doc)
        sc.StartDate = Date.Now
        sc.EndDate = Date.Now
        sc.GenerateGuid()

        Dim xl As New List(Of Double)
        Dim yl As New List(Of Double)
        Dim zl As New List(Of Double)

        Dim rl As List(Of Int64) = Nothing
        Dim gl As List(Of Int64) = Nothing
        Dim bl As List(Of Int64) = Nothing

        If pc.ContainsColors Then
            rl = New List(Of Int64)
            gl = New List(Of Int64)
            bl = New List(Of Int64)
        End If

        For j As Integer = 0 To pc.Count - 1 Step 1
            Dim p As Point3d = pc(j).Location
            xl.Add(p.X)
            yl.Add(p.Y)
            zl.Add(p.Z)
        Next

        sc.AppendData(E57LibCommon.ElementName.CartesianX, xl)
        sc.AppendData(E57LibCommon.ElementName.CartesianY, yl)
        sc.AppendData(E57LibCommon.ElementName.CartesianZ, zl)

        If pc.ContainsColors Then
            For j As Integer = 0 To pc.Count - 1 Step 1
                Dim col As Color = pc(j).Color
                rl.Add(col.R)
                gl.Add(col.G)
                bl.Add(col.B)
            Next
            sc.AppendData(E57LibCommon.ElementName.ColorRed, rl)
            sc.AppendData(E57LibCommon.ElementName.ColorGreen, gl)
            sc.AppendData(E57LibCommon.ElementName.ColorBlue, bl)
        End If

        Dim good As Boolean = False

        doc.Scans.Add(sc)
        good = doc.Save(FPath)
        doc.Scans.Clear()
        doc = Nothing

        Return good
    End Function



End Class
