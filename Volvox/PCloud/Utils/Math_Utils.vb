Imports Rhino.Geometry

Module Math_Utils


    ''' <summary>
    ''' Moves the origin to the  Point3d.Origin first.
    ''' </summary>
    ''' <param name="P"></param>
    ''' <returns></returns>
    Public Function PlaneToQuaternion(P As Plane) As Quaternion
        P.Origin = Point3d.Origin
        Dim t As Rhino.Geometry.Transform = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, P)

        Dim tr As Double = t.M00 + t.M11 + t.M22

        Dim qw As Double
        Dim qx As Double
        Dim qy As Double
        Dim qz As Double

        Dim s As Double

        If (tr > 0) Then
            s = Math.Sqrt(tr + 1.0) * 2
            qw = 0.25 * s
            qx = (t.M21 - t.M12) / s
            qy = (t.M02 - t.M20) / s
            qz = (t.M10 - t.M01) / s
        ElseIf (t.M00 > t.M11) And (t.M00 > t.M22) Then
            s = Math.Sqrt(1.0 + t.M00 - t.M11 - t.M22) * 2
            qw = (t.M21 - t.M12) / s
            qx = 0.25 * s
            qy = (t.M01 + t.M10) / s
            qz = (t.M02 + t.M20) / s
        ElseIf (t.M11 > t.M22) Then
            s = Math.Sqrt(1.0 + t.M11 - t.M00 - t.M22) * 2
            qw = (t.M02 - t.M20) / s
            qx = (t.M01 + t.M10) / s
            qy = 0.25 * s
            qz = (t.M12 + t.M21) / s
        Else
            s = Math.Sqrt(1.0 + t.M22 - t.M00 - t.M11) * 2
            qw = (t.M10 - t.M01) / s
            qx = (t.M02 + t.M20) / s
            qy = (t.M12 + t.M21) / s
            qz = 0.25 * s
        End If

        Dim q As New Quaternion(qw, qx, qy, qz)
        q.Unitize()
        Return q

    End Function

    ''' <summary>
    ''' Just cause OCD... this is exactly box.contains from Rhino.
    ''' </summary>
    ''' <param name="Point"></param>
    ''' <param name="TestBox"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function IsInBox(Point As Point3d, TestBox As Box) As Boolean
        Return TestBox.Contains(Point, False)
    End Function

    ''' <summary>
    ''' Checks if point lies within a sphere.
    ''' </summary>
    ''' <param name="Point">Point to test</param>
    ''' <param name="Center">Sphere center</param>
    ''' <param name="Radius">Sphere radius</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function IsInSphere(Point As Point3d, Center As Point3d, Radius As Double) As Boolean

        Dim RadiusSqr As Double = Radius * Radius
        Dim Distance As Double = FastDistCheck(Point, Center)

        If Distance <= RadiusSqr Then Return True
        Return False
    End Function


    ''' <summary>
    ''' Converts XYZ point to spherical coordinates.
    ''' </summary>
    ''' <param name="Point">Point to convert</param>
    ''' <param name="Origin">Origin plane</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function SphericalPoint(Point As Point3d, Origin As Plane) As Point3d

        Dim t As Rhino.Geometry.Transform = Rhino.Geometry.Transform.PlaneToPlane(Origin, Plane.WorldXY)
        Point.Transform(t)

        Dim nx As Double
        Dim ny As Double
        Dim nz As Double

        nx = Math.Sqrt(Point.X ^ 2 + Point.Y ^ 2 + Point.Z ^ 2)
        ny = Math.Acos(Point.Z / nx)
        nz = Math.Atan(Point.Y / Point.X)

        Return New Point3d(nx, ny, nz)

    End Function


    ''' <summary>
    ''' Distance between 2 points without the Sqrt part.
    ''' </summary>
    ''' <param name="A">First point</param>
    ''' <param name="B">Second point</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function FastDistCheck(A As Point3d, B As Point3d) As Double
        Return (A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y) + (A.Z - B.Z) * (A.Z - B.Z)
    End Function


    ''' <summary>
    ''' Just a bit faster than Rhino.Geometry.Plane.DistanceTo
    ''' </summary>
    ''' <param name="denom">Equation denominator = 1/math.sqrt(a^2+b^2+c^2+d^2)</param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="c"></param>
    ''' <param name="d"></param>
    ''' <param name="Pt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function FastPlaneToPt(Denom As Double, a As Double, b As Double, c As Double, d As Double, Pt As Point3d) As Double
        Return ((a * Pt.X + b * Pt.Y + c * Pt.Z + d) * Denom)
    End Function

    Function FastPtAbovePlane(a As Double, b As Double, c As Double, d As Double, pt As Point3d) As Boolean
        If (pt.X * a + pt.Y * b + pt.Z * c + d) >= 0 Then Return True
        Return False
    End Function



End Module
