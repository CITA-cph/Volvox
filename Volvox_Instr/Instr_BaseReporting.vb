Imports Rhino.Geometry

Public Class Instr_BaseReporting

    Inherits Volvox_Instr.Instr_Base

    Private Percent As Integer
    Private Custom As String
    Public PercArgs As New ReportingPercentArgs(Percent)
    Public CustArgs As New ReportingCustomArgs(Custom)

    ''' <summary>
    ''' Set this value from 0 to 100 so the Engine can display the completed percent of instruction.
    ''' </summary>
    ''' <returns>Current percent completed</returns>
    Public Property ReportPercent As Integer
        Get
            Return Percent
        End Get
        Set(value As Integer)
            Percent = value
            PercArgs.Percent = Percent
            RaiseEvent ReportingPercent(Me, PercArgs)
        End Set
    End Property

    ''' <summary>
    ''' Set this string to any message you would like to display.
    ''' </summary>
    ''' <returns></returns>
    Public Property ReportCustom As String
        Get
            Return Custom
        End Get
        Set(value As String)
            Custom = value
            CustArgs.Custom = Custom
            RaiseEvent ReportingCustom(Me, CustArgs)
        End Set
    End Property

    ''' <summary>
    ''' Don't use it if you don't know what you're doing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Event ReportingPercent(ByVal sender As Object, ByVal e As ReportingPercentArgs)

    ''' <summary>
    ''' Don't use it if you don't know what you're doing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Event ReportingCustom(ByVal sender As Object, ByVal e As ReportingCustomArgs)

    ''' <summary>
    ''' Reporting percent event arguments.
    ''' </summary>
    Public Class ReportingPercentArgs
        Inherits System.EventArgs

        Public Property Percent As Integer

        Sub New(PercentReported As Integer)
            Percent = PercentReported
        End Sub
    End Class

    ''' <summary>
    ''' Reporting custom string event arguments.
    ''' </summary>
    Public Class ReportingCustomArgs
        Inherits System.EventArgs

        Public Property Custom As String

        Sub New(CustomReported As String)
            Custom = CustomReported
        End Sub
    End Class


    ''' <summary>
    ''' For multithreaded instructions-you have to abort each thread you've created.
    ''' </summary>
    Public Overridable Sub Abort()

    End Sub

End Class
