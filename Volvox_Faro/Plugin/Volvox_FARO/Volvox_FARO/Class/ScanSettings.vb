Imports System.Runtime.InteropServices.ComTypes
Imports IQOPENLib
Imports LSSDKLib

Public Class ScanSettings

    Private m_rate As MeasurmentRate = 1
    Private m_res As ScanResolution = 8
    Private m_nois As NoiseCompression = 1

    Private m_hmin As Integer = 0
    Private m_hmax As Integer = 360
    Private m_vmin As Integer = -62.5
    Private m_vmax As Integer = 90

    Sub New()

    End Sub

    Sub New(other As ScanSettings)
        Me.m_rate = other.m_rate
        Me.m_res = other.m_res
        Me.m_nois = other.m_nois
        Me.m_hmin = other.m_hmin
        Me.m_hmax = other.m_hmax
        Me.m_vmin = other.m_vmin
        Me.m_vmax = other.m_vmax
    End Sub

    Public Function Duplicate() As ScanSettings
        Return New ScanSettings(Me)
    End Function

    Public Property Rate As MeasurmentRate
        Get
            Return m_rate
        End Get
        Set(value As MeasurmentRate)
            m_rate = value
        End Set
    End Property

    Public Property Resolution As ScanResolution
        Get
            Return m_res
        End Get
        Set(value As ScanResolution)
            m_res = value
        End Set
    End Property

    Public Property Compression As NoiseCompression
        Get
            Return m_nois
        End Get
        Set(value As NoiseCompression)
            m_nois = value
        End Set
    End Property

    Public Property HorizontalAngleMin As Integer
        Get
            Return m_hmin
        End Get
        Set(value As Integer)
            m_hmin = value
        End Set
    End Property

    Public Property HorizontalAngleMax As Integer
        Get
            Return m_hmax
        End Get
        Set(value As Integer)
            m_hmax = value
        End Set
    End Property

    Public Property VerticalAngleMin As Integer
        Get
            Return m_vmin
        End Get
        Set(value As Integer)
            m_vmin = value
        End Set
    End Property

    Public Property VerticalAngleMax As Integer
        Get
            Return m_vmax
        End Get
        Set(value As Integer)
            m_vmax = value
        End Set
    End Property

    Public Function ApplyTo(ByRef ScanControl As IScanCtrlSDK) As Boolean
        If Not AreCorrect Then Return False

        ScanControl.MeasurementRate = m_rate
        ScanControl.NoiseCompression = m_nois
        ScanControl.Resolution = m_res

        ScanControl.HorizontalAngleMin = m_hmin
        ScanControl.HorizontalAngleMax = m_hmax
        ScanControl.VerticalAngleMin = m_vmin
        ScanControl.VerticalAngleMax = m_vmax
        Return True
    End Function

    Public Enum ScanResolution
        Resolution_1 = 1
        Resolution_2 = 2
        Resolution_4 = 4
        Resolution_5 = 5
        Resolution_8 = 8
        Resolution_10 = 10
        Resolution_16 = 16
        Resolution_20 = 20
        Resolution_32 = 32
    End Enum

    Public Enum MeasurmentRate
        Rate_1 = 1
        Rate_2 = 2
        Rate_4 = 4
        Rate_8 = 8
    End Enum

    Public Enum NoiseCompression
        NoCompression = 1
        Reduce_4 = 2
        Reduce_16 = 4
    End Enum

    Public ReadOnly Property IsValid As Boolean
        Get
            Dim ok As Boolean = True

            If Not AreCorrect Then ok = False
            If Not (Me.HorizontalAngleMin <= Me.HorizontalAngleMax) Then ok = False
            If Not (Me.VerticalAngleMin <= Me.VerticalAngleMax) Then ok = False

            If (Me.HorizontalAngleMin < 0) Then ok = False
            If (Me.HorizontalAngleMax > 360) Then ok = False

            If (Me.VerticalAngleMin < -62.5) Then ok = False
            If (Me.VerticalAngleMax > 90) Then ok = False

            Return ok
        End Get
    End Property

    Private ReadOnly Property AreCorrect() As Boolean
        Get
            '1
            If m_res = 1 AndAlso m_rate = 8 AndAlso m_nois = 1 Then Return True
            If m_res = 1 AndAlso m_rate = 4 AndAlso m_nois = 1 Then Return True
            If m_res = 1 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 1 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True

            '1/2
            If m_res = 2 AndAlso m_rate = 8 AndAlso m_nois = 1 Then Return True
            If m_res = 2 AndAlso m_rate = 4 AndAlso m_nois = 1 Then Return True
            If m_res = 2 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 2 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 1 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True

            '1/4
            If m_res = 4 AndAlso m_rate = 8 AndAlso m_nois = 1 Then Return True
            If m_res = 4 AndAlso m_rate = 4 AndAlso m_nois = 1 Then Return True
            If m_res = 4 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 4 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 2 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True
            If m_res = 1 AndAlso m_rate = 1 AndAlso m_nois = 4 Then Return True

            '1/5
            If m_res = 5 AndAlso m_rate = 4 AndAlso m_nois = 1 Then Return True
            If m_res = 5 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 5 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True

            '1/8
            If m_res = 8 AndAlso m_rate = 4 AndAlso m_nois = 1 Then Return True
            If m_res = 8 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 8 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 4 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True
            If m_res = 2 AndAlso m_rate = 1 AndAlso m_nois = 4 Then Return True

            '1/10
            If m_res = 10 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 10 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 5 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True
            If m_res = 5 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True

            '1/16
            If m_res = 16 AndAlso m_rate = 2 AndAlso m_nois = 1 Then Return True
            If m_res = 16 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 8 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True
            If m_res = 4 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True

            '1/20
            If m_res = 20 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 10 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True
            If m_res = 5 AndAlso m_rate = 1 AndAlso m_nois = 4 Then Return True

            '1/32
            If m_res = 32 AndAlso m_rate = 1 AndAlso m_nois = 1 Then Return True
            If m_res = 16 AndAlso m_rate = 1 AndAlso m_nois = 2 Then Return True
            If m_res = 8 AndAlso m_rate = 1 AndAlso m_nois = 4 Then Return True

            Return False
        End Get
    End Property

End Class
