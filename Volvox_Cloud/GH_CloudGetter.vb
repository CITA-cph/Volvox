Imports Rhino.Input
Imports Rhino.Input.Custom

Public NotInheritable Class GH_CloudGetter

    Private Shared m_reference As Boolean = True

    Sub New()
        m_reference = True
    End Sub

    Public Shared Function GetCloud() As GH_Cloud
        Dim go As GetObject = Nothing
        Dim getResult As Rhino.Input.GetResult
        While True
            go = New GetObject()
            If (Not m_reference) Then
                go.SetCommandPrompt("Cloud to copy")
                go.AddOption("Mode", "Copy")
            Else
                go.SetCommandPrompt("Cloud to reference")
                go.AddOption("Mode", "Reference")
            End If
            go.GeometryFilter = Rhino.DocObjects.ObjectType.PointSet
            getResult = go.Get()
            If (getResult <> 3) Then
                Exit While
            End If
            m_reference = Not m_reference
        End While
        If (getResult <> 12) Then
            Return Nothing
        End If
        If (m_reference) Then
            Return New GH_Cloud(go.Object(0).ObjectId())
        End If
        Return New GH_Cloud(go.Object(0).PointCloud())

    End Function

    Public Shared Function GetClouds() As List(Of GH_Cloud)
        Dim go As GetObject = Nothing
        Dim multiple As GetResult
        While True
            go = New GetObject()
            If (Not m_reference) Then
                go.SetCommandPrompt("Clouds to copy")
                go.AddOption("Mode", "Copy")
            Else
                go.SetCommandPrompt("Clouds to reference")
                go.AddOption("Mode", "Reference")
            End If
            go.GeometryFilter = Rhino.DocObjects.ObjectType.PointSet
            multiple = go.GetMultiple(1, 0)
            If (multiple <> 3) Then
                Exit While
            End If
            m_reference = Not m_reference
        End While
        If (multiple <> 12) Then
            Return Nothing
        End If
        Dim clouds As List(Of GH_Cloud) = New List(Of GH_Cloud)()
        Dim objectCount As Integer = go.ObjectCount - 1
        Dim i As Integer = 0
        Do
            If (Not m_reference) Then
                clouds.Add(New GH_Cloud(go.Object(i).PointCloud()))
            Else
                clouds.Add(New GH_Cloud(go.Object(i).ObjectId()))
            End If
            i = i + 1
        Loop While i <= objectCount
        Return clouds

    End Function

End Class

