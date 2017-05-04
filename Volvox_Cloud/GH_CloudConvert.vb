Imports Rhino
Imports Rhino.Geometry
Imports Grasshopper
Imports Grasshopper.Kernel
Imports GH_IO
Imports System.Drawing
Imports Grasshopper.Kernel.Types
Imports Rhino.DocObjects
Imports System.Runtime.CompilerServices

Module GH_CloudConvert

    Public Function ToGHCloud(ByVal data As Object, ByVal conversion_level As GH_Conversion, ByRef target As GH_Cloud) As Boolean
        Select Case conversion_level
            Case GH_Conversion.Primary
                Return ToGHCloud_Primary(RuntimeHelpers.GetObjectValue(data), target)
            Case GH_Conversion.Secondary
                Return ToGHCloud_Secondary(RuntimeHelpers.GetObjectValue(data), target)
            Case GH_Conversion.Both
                If Not ToGHCloud_Primary(RuntimeHelpers.GetObjectValue(data), target) Then
                    Return ToGHCloud_Secondary(RuntimeHelpers.GetObjectValue(data), target)
                End If
                Return True
        End Select
        Return False
    End Function

    Public Function ToGHCloud_Primary(ByVal data As Object, ByRef target As GH_Cloud) As Boolean

        Dim pcg As New Guid("3977cd01-cd46-3cff-82e5-83559c2e75c8")

        If (data Is Nothing) Then
            Return False
        End If
        Dim gUID As Guid = data.GetType.GUID

        If (gUID = pcg) Then
            If (target Is Nothing) Then
                target = New GH_Cloud(DirectCast(data, PointCloud))
            Else
                target.ReferenceID = Guid.Empty
                target.Value = DirectCast(data, PointCloud)
            End If
            Return True
        End If
        If Not (gUID = pcg) Then
            Return False
        End If
        If (target Is Nothing) Then
            target = DirectCast(data, GH_Cloud)
            Return True
        End If
        target.Value = DirectCast(data, GH_Cloud).Value
        target.ReferenceID = DirectCast(data, GH_Cloud).ReferenceID
        Return True
    End Function

    Public Function ToGHCloud_Secondary(ByVal data As Object, ByRef target As GH_Cloud) As Boolean
        Dim guid As Guid
        If (data Is Nothing) Then
            Return False
        End If

        If GH_Convert.ToGUID_Primary(RuntimeHelpers.GetObjectValue(data), guid) Then
            If (target Is Nothing) Then
                target = New GH_Cloud(guid)
            Else
                target.ReferenceID = guid
            End If
            target.ClearCaches()
            target.LoadGeometry()
            Return target.IsValid
        End If
        Dim destination As String = Nothing

        If GH_Convert.ToString_Primary(RuntimeHelpers.GetObjectValue(data), destination) Then
            Dim obj2 As RhinoObject = GH_Convert.FindRhinoObjectByNameAndType(destination, ObjectType.PointSet)
            If (Not obj2 Is Nothing) Then
                If (target Is Nothing) Then
                    target = New GH_Cloud
                End If
                target.ReferenceID = obj2.Id
                target.ClearCaches()
                target.LoadGeometry()
                Return target.IsValid
            End If
        End If
        Dim rpc As PointCloud = Nothing

        If Not GH_CloudConvert.ToCloud(RuntimeHelpers.GetObjectValue(data), rpc) Then
            Return False
        End If
        If (target Is Nothing) Then
            target = New GH_Cloud(rpc)
        Else
            target.Value = rpc
            target.ReferenceID = Guid.Empty
        End If
        Return True
    End Function

    Public Function ToCloud(ByVal data As Object, ByRef rpc As PointCloud) As Boolean

        Dim pcg As New Guid("3977cd01-cd46-3cff-82e5-83559c2e75c8")

        If (Not data Is Nothing) Then
            Dim gUID As Guid = data.GetType.GUID

            If (gUID = pcg) Then
                rpc = DirectCast(data, PointCloud)
                Return True
            End If

            If (gUID = GH_TypeLib.id_gh_curve) Then
                rpc = DirectCast(data, GH_Cloud).Value
                Return True
            End If

            If TypeOf data Is Curve Then
                rpc = DirectCast(data, PointCloud)
                Return True
            End If

        End If
        Return False
    End Function

End Module
