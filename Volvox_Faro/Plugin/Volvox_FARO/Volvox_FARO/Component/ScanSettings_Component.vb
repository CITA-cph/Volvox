Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class ScanSettings_Component
    Inherits GH_Component

    Sub New()
        MyBase.New("FARO Settings", "FARO Set", "FARO scan settings", "Volvox", "FARO")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("{C5F3C2C3-E6EF-498B-A006-8229D6510308}")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddIntegerParameter("Resolution", "R", "Resolution as 1/Nth of the full resolution", GH_ParamAccess.item, 1)
        pManager.AddIntegerParameter("Rate", "M", "Measurment rate", GH_ParamAccess.item, 8)
        pManager.AddIntegerParameter("Compression", "C", "Noise compression", GH_ParamAccess.item, 1)

        pManager.AddIntervalParameter("Horizontal", "H", "Horizontal angles", GH_ParamAccess.item, New Interval(0, 360))
        pManager.AddIntervalParameter("Vertical", "V", "Vertical angles", GH_ParamAccess.item, New Interval(-62.5, 90))
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_ScanSettings)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim res As Integer
        Dim rate As Integer
        Dim nois As Integer
        Dim hor As New Interval
        Dim ver As New Interval

        If Not DA.GetData(0, res) Then Return
        If Not DA.GetData(1, rate) Then Return
        If Not DA.GetData(2, nois) Then Return
        If Not DA.GetData(3, hor) Then Return
        If Not DA.GetData(4, ver) Then Return

        Dim sets As New ScanSettings

        With sets
            .Resolution = res
            .Rate = rate
            .Compression = nois
            .HorizontalAngleMin = hor.Min
            .HorizontalAngleMax = hor.Max
            .VerticalAngleMin = ver.Min
            .VerticalAngleMax = ver.Max
        End With

        If Not sets.IsValid Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Wrong settings, please refer to the SDK manual." & vbCrLf & "Please search for ""FARO LS SDK 5.5 Manual EN"" at http://www.faro.com/download-centre")
            Return
        End If

        DA.SetData(0, New GH_ScanSettings(sets))

    End Sub
End Class
