Public Module Maths
    'Check the degenerate cases later
    Public Function CartesianToCylindrical(X As Double, Y As Double, Z As Double) As Double()
        Dim result(2) As Double
        result(0) = Math.Sqrt(X * X + Y * Y)
        result(1) = Math.Atan2(Y, Z)
        result(2) = Z
        If X = Y Then result(0) = 0
        Return result
    End Function

    Public Function CylindricalToCartesian(Radius As Double, Angle As Double, Height As Double) As Double()
        Dim result(2) As Double
        result(0) = Radius * (Math.Cos(Angle))
        result(1) = Radius * (Math.Sin(Angle))
        result(2) = Height
        Return result
    End Function

    ''' <summary>
    ''' Returns Range, Azimuth, Elevation in that order
    ''' </summary>
    ''' <param name="X"></param>
    ''' <param name="Y"></param>
    ''' <param name="Z"></param>
    ''' <returns></returns>
    Public Function CartesianToSpherical(X As Double, Y As Double, Z As Double) As Double()
        Dim result(2) As Double
        If X = 0 And Y = 0 And Z = 0 Then
            result(0) = 0
            result(1) = 0
            result(2) = 0
            Return result
        End If

        If X = 0 And Y = 0 Then
            result(0) = Z
            result(1) = 0
        Else
            result(0) = Math.Sqrt(X * X + Y * Y + Z * Z)
            result(1) = Math.Atan2(Y, X)
        End If

        result(2) = Math.Asin(Z / result(0))

        Return result
    End Function

    Public Function SphericalToCartesian(Range As Double, Azimuth As Double, Elevation As Double) As Double()
        Dim result(2) As Double
        Dim cosElevation As Double = Math.Cos(Elevation)
        result(0) = Range * (cosElevation * Math.Cos(Azimuth))
        result(1) = Range * (cosElevation * Math.Sin(Azimuth))
        result(2) = Range * Math.Sin(Elevation)
        Return result
    End Function

    Public Function GPSTime(time As DateTime) As Double
        Dim gpsStartDate As New DateTime(1980, 1, 6, 0, 0, 0) 'the gps start time is 6th January
        Return (time - gpsStartDate).TotalSeconds
    End Function

    Public Function GPSTime(time As Double) As Date
        Dim gpsStartDate As New DateTime(1980, 1, 6, 0, 0, 0) 'the gps start time is 6th January
        Return gpsStartDate.AddSeconds(time)
    End Function

End Module

