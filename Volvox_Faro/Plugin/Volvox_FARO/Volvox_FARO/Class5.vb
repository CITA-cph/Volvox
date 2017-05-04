'Imports System.Runtime.InteropServices.ComTypes
'Imports IQOPENLib
'Imports LSSDKLib


'Public Class Class5
'    Implements _IScanCtrlSDKEvents

'    Dim cookie As Integer
'    Dim icp As IConnectionPoint = Nothing
'    Dim licsdk As IiQLicensedInterfaceIf = New ScanCtrlSDKClass()

'    Public Sub startScan()

'        Dim license As String = "FARO Open Runtime License" & vbLf + "Key:Q44ELPNKTAKXFM6T83ZUSZTPL" & vbLf & vbLf + "The software is the registered property of FARO Scanner Production GmbH, Stuttgart, Germany." & vbLf + "All rights reserved." & vbLf & "This software may only be used with written permission of FARO Scanner Production GmbH, Stuttgart, Germany."
'        licsdk.License = license

'        Dim sctrl As IScanCtrlSDK = DirectCast(licsdk, IScanCtrlSDK)

'        sctrl.ScannerIP = "192.168.10.87"
'        sctrl.connect()

'        If Not sctrl.Connected Then MsgBox("Can't connect") : Return

'        Threading.Thread.Sleep(5000)

'        sctrl.HorizontalAngleMin = 10
'        sctrl.HorizontalAngleMax = 20
'        sctrl.VerticalAngleMin = 10
'        sctrl.VerticalAngleMax = 20

'        sctrl.StorageMode = StorageMode.SMRemote
'        sctrl.RemoteScanStoragePath = "C:/tmp"
'        sctrl.ScanBaseName = "Remote"
'        sctrl.Resolution = 32
'        sctrl.syncParam()

'        Threading.Thread.Sleep(5000)

'        Dim icpc As IConnectionPointContainer = DirectCast(sctrl, IConnectionPointContainer)

'        Dim g As Guid = GetType(_IScanCtrlSDKEvents).GUID

'        icpc.FindConnectionPoint(g, icp)
'        icp.Advise(Me, cookie)

'        Threading.Thread.Sleep(5000)

'        sctrl.startScan()

'    End Sub

'    Public Sub scanCompleted() Implements _IScanCtrlSDKEvents.scanCompleted
'        icp.Unadvise(cookie)
'        MsgBox("complete finally")
'    End Sub

'End Class
