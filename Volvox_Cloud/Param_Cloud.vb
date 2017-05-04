Imports Grasshopper.Kernel
Imports Rhino
Imports Rhino.DocObjects
Imports Rhino.Geometry
Imports System.Drawing


Public Class Param_Cloud

    Inherits GH_PersistentGeometryParam(Of GH_Cloud)
    Implements IGH_BakeAwareObject, IGH_PreviewObject

    ' Fields
    Private m_hidden As Boolean
    Public Property Hidden As Boolean Implements IGH_PreviewObject.Hidden
    WithEvents DispContr As New Control_Display

    ' Methods
    Public Sub New()
        MyBase.New(New GH_InstanceDescription("Cloud", "Clo", "Contains a cloud", "Params", "Geometry"))
        Me.m_hidden = False
    End Sub

    Public Sub BakeGeometry(ByVal doc As RhinoDoc, ByVal obj_ids As List(Of Guid)) Implements IGH_BakeAwareObject.BakeGeometry
        Me.BakeGeometry(doc, Nothing, obj_ids)
    End Sub

    Public Sub BakeGeometry(ByVal doc As RhinoDoc, ByVal att As ObjectAttributes, ByVal obj_ids As List(Of Guid)) Implements IGH_BakeAwareObject.BakeGeometry

        Dim enumerator As IEnumerator = Nothing
        If (att Is Nothing) Then
            att = doc.CreateDefaultAttributes
        End If
        Try
            enumerator = MyBase.m_data.GetEnumerator
            Do While enumerator.MoveNext
                Dim guid As Guid
                Dim current As IGH_BakeAwareData = DirectCast(enumerator.Current, IGH_BakeAwareData)
                If ((Not current Is Nothing) AndAlso current.BakeGeometry(doc, att, guid)) Then
                    obj_ids.Add(guid)
                End If
            Loop
        Finally
            If TypeOf enumerator Is IDisposable Then
                TryCast(enumerator, IDisposable).Dispose()
            End If
        End Try
    End Sub

    Public Overrides Sub AppendAdditionalMenuItems(menu As Windows.Forms.ToolStripDropDown)
        MyBase.AppendAdditionalMenuItems(menu)
        GH_DocumentObject.Menu_AppendSeparator(menu)

        If Settings_Global.DisplayDynamic Then
            DispContr.DispButBack = Color.FromArgb(255, 196, 225, 255)
            DispContr.DispButFrame = Color.FromArgb(255, 51, 153, 255)
        Else
            DispContr.DispButBack = Color.White
            DispContr.DispButFrame = Color.FromArgb(255, 189, 189, 189)
        End If

        GH_DocumentObject.Menu_AppendCustomItem(menu, DispContr)
        GH_DocumentObject.Menu_AppendItem(menu, "Show scanner position", AddressOf PositionSwitch, True, Settings_Global.DisplayPositions)

    End Sub

    Private Sub PositionSwitch()
        Settings_Global.DisplayPositions = Not Settings_Global.DisplayPositions
        Me.ExpirePreview(True)
    End Sub

    Private Sub DynamicSwitch() Handles DispContr.DynamicClicked
        Settings_Global.DisplayDynamic = Not Settings_Global.DisplayDynamic
        If Settings_Global.DisplayDynamic Then
            DispContr.DispButBack = Color.FromArgb(255, 196, 225, 255)
            DispContr.DispButFrame = Color.FromArgb(255, 51, 153, 255)
        Else
            DispContr.DispButBack = Color.White
            DispContr.DispButFrame = Color.FromArgb(255, 189, 189, 189)
        End If
    End Sub

    Private Sub IncreaseRadius() Handles DispContr.PlusClicked
        Settings_Global.DisplayRadius += 1
        Me.ExpirePreview(True)
    End Sub

    Private Sub DecreaseRadius() Handles DispContr.MinusClicked
        If Settings_Global.DisplayRadius > 1 Then Settings_Global.DisplayRadius -= 1
        Me.ExpirePreview(True)
    End Sub

    Public Sub DrawViewportMeshes(ByVal args As IGH_PreviewArgs) Implements IGH_PreviewObject.DrawViewportMeshes
    End Sub

    Public Sub DrawViewportWires(ByVal args As IGH_PreviewArgs) Implements IGH_PreviewObject.DrawViewportWires
        Me.Preview_DrawWires(args)
    End Sub

    Protected Overrides Function InstantiateT() As GH_Cloud
        Return New GH_Cloud()
    End Function

    Protected Overrides Function PreferredCast(ByVal data As Object) As GH_Cloud
        If (Not TypeOf data Is PointCloud) Then
            Return Nothing
        End If
        Return New GH_Cloud(DirectCast(data, PointCloud))
    End Function

    Protected Overrides Function Prompt_Singular(ByRef value As GH_Cloud) As GH_GetterResult
        value = GH_CloudGetter.GetCloud()
        If (value Is Nothing) Then
            Return GH_GetterResult.cancel
        End If
        Return GH_GetterResult.success
    End Function

    Protected Overrides Function Prompt_Plural(ByRef values As List(Of GH_Cloud)) As GH_GetterResult
        values = GH_CloudGetter.GetClouds
        If (values IsNot Nothing AndAlso values.Count <> 0) Then
            Return GH_GetterResult.success
        End If
        Return GH_GetterResult.cancel
    End Function

    ' Properties
    Public ReadOnly Property ClippingBox As BoundingBox Implements IGH_PreviewObject.ClippingBox
        Get
            Return Me.Preview_ComputeClippingBox
        End Get
    End Property

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("{E285577D-197D-42AB-9FA8-E639FB1DDDDD}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_CloudParam
        End Get
    End Property

    Public ReadOnly Property IsBakeCapable As Boolean Implements IGH_BakeAwareObject.IsBakeCapable
        Get
            Return Not MyBase.m_data.IsEmpty
        End Get
    End Property

    Public ReadOnly Property IsPreviewCapable As Boolean Implements IGH_PreviewObject.IsPreviewCapable
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property TypeName As String
        Get
            Return "Cloud"
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return "Cloud"
    End Function

End Class



