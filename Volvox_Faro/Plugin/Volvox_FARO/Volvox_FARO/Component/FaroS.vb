Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports IQOPENLib

Public Class FaroS

    Private Shared libRef As IiQLibIf
    Private Shared initialized As Boolean = False

    Sub New()

    End Sub

    Public Shared Function Initialize() As Result
        If libRef IsNot Nothing Then
            initialized = True
            Return Result.Success
        End If

        Dim licLibIf As IiQLicensedInterfaceIf = New iQLibIfClass()
        licLibIf.License = "FARO Open Runtime License" & vbLf + "Key:Q44ELPNKTAKXFM6T83ZUSZTPL" & vbLf & vbLf + "The software is the registered property of FARO Scanner Production GmbH, Stuttgart, Germany." & vbLf + "All rights reserved." & vbLf & "This software may only be used with written permission of FARO Scanner Production GmbH, Stuttgart, Germany."
        libRef = DirectCast(licLibIf, IiQLibIf)

        ' need to find some way to check for errors, i.e. see if libRef was correctly constructed, etc.
        If libRef Is Nothing Then
            Return Result.Failure
        End If
        initialized = True
        Return Result.Success
    End Function

    Public Shared Function IsInitialized() As Result
        If initialized Then
            Return Result.Success
        End If
        Return Result.Failure
    End Function

    Public Shared Function Uninitialize() As Result
        UnloadAll()
        libRef = Nothing
        initialized = False

        GC.Collect()
        GC.WaitForPendingFinalizers()
        Return Result.Success
    End Function

    ' load scan or workspace
    Public Shared Function Load(path As String) As Result
        If Not initialized Then
            Return Result.Failure
        End If
        Return DirectCast(libRef.load(path), Result)
    End Function

    ' unload scan
    Public Shared Function Unload(scan_id As Integer) As Result
        If Not initialized Then
            Return Result.Failure
        End If
        If (libRef.getNumScans() > scan_id) AndAlso (scan_id >= 0) Then
            libRef.unloadScan(scan_id)
        End If

        Return Result.Success
    End Function

    ' unload all scans
    Public Shared Function UnloadAll() As Result
        If Not initialized Then
            Return Result.Failure
        End If

        Dim num As Integer = libRef.getNumScans()

        For i As Integer = num - 1 To 0 Step -1
            libRef.unloadScan(i)
        Next
        Return Result.Success
    End Function

    ' get points and reflections
    Public Shared Function GetXYZPoints(scan_id As Integer, ByRef points As Double(), ByRef color As Integer(), Optional [step] As Integer = 1) As Result
        If Not initialized Then
            points = New Double(0) {}
            color = New Integer(0) {}
            Return Result.Failure
        End If

        Dim nc As Integer = libRef.getScanNumCols(scan_id)
        Dim nr As Integer = libRef.getScanNumRows(scan_id)

        Dim pos() As Double = Nothing
        Dim refl() As Int32 = Nothing

        Dim p_temp As New List(Of Double)()
        Dim c_temp As New List(Of Integer)()

        Dim col As Integer = 0
        While col < nc
            libRef.getXYZScanPoints2(scan_id, 0, col, nr, pos, refl)
            Dim row As Integer = 0
            While row < nr
                p_temp.Add(CDbl(pos.GetValue(3 * row)))
                p_temp.Add(CDbl(pos.GetValue(3 * row + 1)))
                p_temp.Add(CDbl(pos.GetValue(3 * row + 2)))
                c_temp.Add(CInt(refl.GetValue(row)))
                row += [step]
            End While
            col += [step]
        End While

        points = p_temp.ToArray()
        color = c_temp.ToArray()
        Return Result.Success
    End Function

    ' get scanner position
    Public Shared Function GetScanPosition(scan_id As Integer, ByRef position As Double()) As Result
        position = New Double(2) {}
        If Not initialized Then
            Return Result.Failure
        End If
        Return DirectCast(libRef.getScanPosition(scan_id, position(0), position(1), position(2)), Result)
    End Function

    ' get scanner position
    Public Shared Function GetScanPosition(scan_id As Integer, ByRef x As Double, ByRef y As Double, ByRef z As Double) As Result
        If Not initialized Then
            x = InlineAssignHelper(y, InlineAssignHelper(z, 0.0))
            Return Result.Failure
        End If
        Return DirectCast(libRef.getScanPosition(scan_id, x, y, z), Result)
    End Function

    ' get orientation of scanner in axis-angle notation
    Public Shared Function GetScanOrientation(scan_id As Integer, ByRef x As Double, ByRef y As Double, ByRef z As Double, ByRef angle As Double) As Result
        If Not initialized Then
            x = InlineAssignHelper(y, InlineAssignHelper(z, InlineAssignHelper(angle, 0.0)))
            Return Result.Failure
        End If
        Return DirectCast(libRef.getScanOrientation(scan_id, x, y, z, angle), Result)
    End Function

    ' get orientation of scanner in axis-angle notation
    Public Shared Function GetScanOrientation(scan_id As Integer, ByRef aa As Double()) As Result
        aa = New Double(3) {}
        If Not initialized Then
            Return Result.Failure
        End If
        Return DirectCast(libRef.getScanOrientation(scan_id, aa(0), aa(1), aa(2), aa(3)), Result)
    End Function

    ' get SDK version info
    Public Shared Function Info() As String
        Return [String].Format("FARO SDK version: " + libRef.getLibVersion())
    End Function

    ' get columnCount
    Public Shared Function columnCount(scan_id As Integer) As Integer
        Return libRef.getScanNumCols(scan_id)
    End Function

    ' get rowCount
    Public Shared Function rowCount(scan_id As Integer) As Integer
        Return libRef.getScanNumRows(scan_id)

    End Function

    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function

    Public Enum Result
        Success
        Failure
    End Enum
End Class
