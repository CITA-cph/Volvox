Imports GH_IO.Serialization
Imports Grasshopper.Kernel
Imports Volvox_FARO

Public Class Param_ScanSettings
    Inherits GH_PersistentParam(Of GH_ScanSettings)

    Sub New()
        MyBase.New(New GH_InstanceDescription("Settings", "S", "FARO scan settings", "Volvox", "FARO"))
    End Sub

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.hidden
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("{4347DB1C-F0B5-44AE-88C4-464953525C8A}")
        End Get
    End Property

    Protected Overrides Function Prompt_Plural(ByRef values As List(Of GH_ScanSettings)) As GH_GetterResult
        Return GH_GetterResult.cancel
    End Function

    Protected Overrides Function Prompt_Singular(ByRef value As GH_ScanSettings) As GH_GetterResult
        Return GH_GetterResult.cancel
    End Function
End Class
