Imports Volvox_Instr
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports System.IO
Imports E57LibWriter
Imports System.Drawing

Public Class Instr_SaveXYZ
    Inherits Instr_Base

    Friend Property FPath As String
    Friend Property FPrec As Double
    Friend Property FMask As String
    Friend Property Dqout As Boolean

    Sub New(FilePath As String, Precision As Double, Quotes As Boolean, Mask As String)
        FPath = FilePath
        FPrec = Precision
        Dqout = Quotes
    End Sub

    Public Overrides ReadOnly Property InstructionGUID As Guid
        Get
            Return New Guid("7c6c2b90-a4be-4847-babf-e23167997635")
        End Get
    End Property

    Public Overrides ReadOnly Property InstructionType As String
        Get
            Return "Save XYZ"
        End Get
    End Property

    Public Overrides Function duplicate() As IGH_Goo
        Return New Instr_SaveXYZ(FPath, FPrec, Dqout, FMask)
    End Function

    Public Overrides Function Execute(ByRef pc As PointCloud) As Boolean

        Try
            If Not File.Exists(FPath) Then File.Create(FPath).Dispose()
        Catch ex As Exception
            Return False
        End Try

        If Utils_IO.IsFileLocked(New FileInfo(FPath)) Then Return False

        Using w As StreamWriter = New StreamWriter(FPath)
            Dim nparse As New Parse_Save(FMask, FPrec, Dqout)
            For i As Integer = 0 To pc.Count - 1 Step 1
                w.WriteLine(nparse.PCloudItemToText(pc, i))
            Next
        End Using

        Return True
    End Function

End Class
