Imports System.Drawing
Imports Rhino.Geometry

Module PointCloud_Tools

    Sub CopyToCloud(ByRef Source As PointCloud, ByRef Target As PointCloud, Index As Integer)

        Dim P As PointCloudItem = Source(Index)
        Target.AppendNew()
        Target.Item(Target.Count - 1).Location = P.Location
        If Source.ContainsColors Then Target.Item(Target.Count - 1).Color = P.Color
        If Source.ContainsNormals Then Target.Item(Target.Count - 1).Normal = P.Normal

    End Sub


End Module
