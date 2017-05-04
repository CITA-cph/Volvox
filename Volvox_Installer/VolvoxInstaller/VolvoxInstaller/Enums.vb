Public Structure Files

    Public Shared Function Libs(path As String) As List(Of String)
        Dim f As New List(Of String)

        f.Add(path & "\Licence.txt")

        f.Add(path & "\Volvox_Cloud.dll")
        f.Add(path & "\Volvox_Instr.dll")
        f.Add(path & "\Volvox_Python.py")
        f.Add(path & "\Volvox.gha")
        f.Add(path & "\VolvoxIcon.ico")

        f.Add(path & "\Crc32C.NET.dll")
        f.Add(path & "\Crc32C.NET.xml")

        f.Add(path & "\E57LibCommon.dll")
        f.Add(path & "\E57LibReader.dll")
        f.Add(path & "\E57LibWriter.dll")

        Return f
    End Function

    Private Shared Function Examples() As List(Of String)
        Dim f As New List(Of String)

        f.Add("Advanced scripting with Volvox.gh")
        f.Add("Clipping planes.gh")
        f.Add("EngineX vs Engine.gh")
        f.Add("Load E57.gh")
        f.Add("LoadAndSave.gh")
        f.Add("Scanner Position.gh")
        f.Add("Selection.gh")
        f.Add("UserData.gh")
        f.Add("Basic Operations.gh")
        f.Add("Expression Dictionary.gh")

        Return f
    End Function

    Public Shared Function ExamplesBytes(ByRef Names As List(Of String)) As List(Of Byte())
        Dim b As New List(Of Byte())
        Names.AddRange(Files.Examples)

        b.Add(My.Resources.Advanced_scripting_with_Volvox)
        b.Add(My.Resources.Clipping_planes)
        b.Add(My.Resources.EngineX_vs_Engine)
        b.Add(My.Resources.Load_E57)

        b.Add(My.Resources.LoadAndSave)
        b.Add(My.Resources.Scanner_Position)
        b.Add(My.Resources.Selection)
        b.Add(My.Resources.UserData)

        b.Add(My.Resources.Basic_Operations)
        b.Add(My.Resources.Expression_Dictionary)

        Return b
    End Function
End Structure
