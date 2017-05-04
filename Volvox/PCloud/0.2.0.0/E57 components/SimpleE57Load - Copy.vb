Imports System.Drawing
Imports e57Lib
Imports e57Lib.ScanData
Imports e57Lib.XmlReflection
Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Parameters
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class SimpleE57Load
    Inherits GH_Component

    Sub New()
        MyBase.New("Simple Load E57", "SimpleE57", "Loads selected scans from E57 file", "Volvox", "E57")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("b8490fc4-d3f9-47d9-b7c6-10125f17772e")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "FilePath", "F", "E57 File Path", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Index", "I", "Scan index, set to -1 to load all scans from the file", GH_ParamAccess.list, -1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Loaded Cloud", GH_ParamAccess.list)
        pManager.AddPlaneParameter("Position", "P", "Scan pose", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As New String("")
        If Not DA.GetData(0, fpath) Then Return

        Dim il As New List(Of Integer)
        If Not DA.GetDataList(1, il) Then Return

        Dim doc As New e57Document(fpath, False)

        For Each int As Integer In il
            If int > doc.Scans.Count - 1 Then
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Supplied index too high.")
                Return
            End If
        Next

        Dim pcl As New List(Of GH_Cloud)

        If il.Contains(-1) And il.Count = 1 Then
            il.Clear()
            For i As Integer = 0 To doc.ScanCount - 1 Step 1
                il.Add(i)
            Next
        ElseIf il.Contains(-1) And il.Count > 1 Then

            Dim tempil As New List(Of Integer)
            For i As Integer = 0 To il.Count - 1 Step 1
                If il(i) <> -1 Then tempil.Add(il(i))
            Next

            il.Clear()
            il.AddRange(tempil)
        End If

        Dim posiL As New List(Of Plane)

        For Each int As Integer In il
            Dim thisScan As Scan = doc.Scans(int)

            thisScan.ReadScanData()

            Dim data As ScanData = thisScan.Data

            Dim pc As New PointCloud

            'coordinates
            If data.HasCartesian Then

                Dim xl As New List(Of Double)
                Dim yl As New List(Of Double)
                Dim zl As New List(Of Double)

                data.GetList(ElementName.CartesianX, xl)
                data.GetList(ElementName.CartesianY, yl)
                data.GetList(ElementName.CartesianZ, zl)

                If (xl.Count <> yl.Count) Or (xl.Count <> zl.Count) Then
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File might be corrupted.")
                    Return
                End If

                For i As Integer = 0 To xl.Count - 1 Step 1
                    pc.Add(New Point3d(xl(i), yl(i), zl(i)))
                Next

            ElseIf data.HasSpherical Then

                Dim azimuth As New List(Of Double)
                Dim range As New List(Of Double)
                Dim elevation As New List(Of Double)

                data.GetList(ElementName.SphericalAzimuth, azimuth)
                data.GetList(ElementName.SphericalRange, range)
                data.GetList(ElementName.SphericalElevation, elevation)

                If (azimuth.Count <> range.Count) Or (azimuth.Count <> elevation.Count) Then
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File might be corrupted.")
                    Return
                End If

                For i As Integer = 0 To azimuth.Count - 1 Step 1
                    Dim sp() As Double = e57Lib.Maths.SphericalToCartesian(range(i), azimuth(i), elevation(i))
                    pc.Add(New Point3d(sp(0), sp(1), sp(2)))
                Next

            End If

            'color 
            If data.HasColor Then


                Dim redl As New List(Of Double)
                Dim greenl As New List(Of Double)
                Dim bluel As New List(Of Double)

                data.GetList(ElementName.ColorRed, redl)
                data.GetList(ElementName.ColorGreen, greenl)
                data.GetList(ElementName.ColorBlue, bluel)

                If (redl.Count <> greenl.Count) Or (redl.Count <> bluel.Count) Then
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File might be corrupted.")
                    Return
                End If

                Dim redint As New Interval(thisScan.ColorLimits(0), thisScan.ColorLimits(1))
                Dim greenint As New Interval(thisScan.ColorLimits(2), thisScan.ColorLimits(3))
                Dim blueint As New Interval(thisScan.ColorLimits(4), thisScan.ColorLimits(5))

                For i As Integer = 0 To redl.Count - 1 Step 1
                    Dim thiscolor As Color = Color.FromArgb(redint.NormalizedParameterAt(redl(i)) * 255,
                                                            greenint.NormalizedParameterAt(greenl(i)) * 255,
                                                            blueint.NormalizedParameterAt(bluel(i)) * 255)

                    pc(i).Color = thiscolor
                Next

            End If

            'intensity 
            If data.HasIntensity Then
                Dim intens As New List(Of Double)
                If data.GetList(ElementName.Intensity, intens) Then
                    pc.UserDictionary.Set(ElementName.Intensity, intens)
                    If data.HasColor = 0 Then
                        Dim intensitv As New Interval(thisScan.IntensityLimits(0), thisScan.IntensityLimits(1))

                        For i As Integer = 0 To intens.Count - 1 Step 1
                            Dim thiscolor As Color = Color.FromArgb(intensitv.NormalizedParameterAt(intens(i)) * 255,
                                                                    intensitv.NormalizedParameterAt(intens(i)) * 255,
                                                                    intensitv.NormalizedParameterAt(intens(i)) * 255)
                            pc(i).Color = thiscolor
                        Next
                    End If
                End If
            End If

            'time stamp 
            If data.HasTimeStamp Then
                Dim ts As New List(Of Double)
                If data.GetList(ElementName.TimeStamp, ts) Then
                    pc.UserDictionary.Set(ElementName.TimeStamp, ts)
                End If
            End If

            AddToDictionary(pc, ElementName.CartesianInvalidState, data)
            AddToDictionary(pc, ElementName.ColumnIndex, data)
            AddToDictionary(pc, ElementName.IsColorInvalid, data)
            AddToDictionary(pc, ElementName.IsIntensityInvalid, data)
            AddToDictionary(pc, ElementName.IsTimeStampInvalid, data)
            AddToDictionary(pc, ElementName.ReturnCount, data)
            AddToDictionary(pc, ElementName.ReturnIndex, data)
            AddToDictionary(pc, ElementName.RowIndex, data)


            Dim trans() As Double = thisScan.Translation
            Dim rot() As Double = thisScan.Rotation

            Dim rq As New Quaternion(rot(0), rot(1), rot(2), rot(3))

            Dim ang As Double
            Dim axi As New Vector3d
            rq.GetRotation(ang, axi)

            Dim thistrans As Rhino.Geometry.Transform = Rhino.Geometry.Transform.Translation(trans(0), trans(1), trans(2))
            Dim thisrotation As Rhino.Geometry.Transform = Rhino.Geometry.Transform.Rotation(ang, axi, New Point3d(0, 0, 0))
            If Not thisrotation.IsValid Then thisrotation = Transform.Identity

            Dim ghc As New GH_Cloud(pc)
            Dim pos As New Plane(Plane.WorldXY)

            pos.Transform(thisrotation)
            pos.Transform(thistrans)

            posiL.Add(pos)

            ghc.Transform(thisrotation)
            ghc.Transform(thistrans)

            pcl.Add(ghc)
        Next

        DA.SetDataList(0, pcl)
        DA.SetDataList(1, posiL)

    End Sub

    Function AddToDictionary(ByRef Cloud As PointCloud, ElementName As String, ByRef Data As ScanData) As Boolean
        If Data.HasData(ElementName) = ElementType.Empty Then Return False

        Select Case Data.TypeOfElement(ElementName)
            Case ElementType.Double
                Dim templ As New List(Of Double)
                Data.GetList(ElementName, templ)
                Cloud.UserDictionary.Set(ElementName, templ)
            Case ElementType.Integer
                Dim templ As New List(Of Int64)
                Data.GetList(ElementName, templ)
                Dim smaller As New List(Of Integer)
                For Each number As Int64 In templ
                    smaller.Add(number)
                Next
                Cloud.UserDictionary.Set(ElementName, smaller)
        End Select

        Return True
    End Function

End Class
