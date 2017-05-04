Imports Grasshopper.Kernel
Imports Microsoft.Scripting.Hosting
Imports IronPython.Hosting
Imports System.Windows.Forms
Imports System.IO

Public Class Asmbl_Priority
    Inherits GH_AssemblyPriority

    'Public Overrides Function PriorityLoad() As GH_LoadingInstruction

    '    Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    '    path += "\Grasshopper\Libraries\"

    '    For Each filename As String In IO.Directory.GetFiles(path, "*", IO.SearchOption.AllDirectories)
    '        If IO.Path.GetFileName(filename) = "Volvox_Python.py" Then
    '            Dim ThisScript As String = SomePythonPart() & File.ReadAllText(filename)
    '            Dim dir As String = IO.Path.GetDirectoryName(filename)

    '            Dim newfile As String = dir & "\Volvox_" & Process.GetCurrentProcess.Id

    '            Using fstr As TextWriter = New StreamWriter(newfile)
    '                Dim tempstr() As String = ThisScript.Split(vbCrLf)

    '                For Each l As String In tempstr
    '                    fstr.WriteLine(l)
    '                Next
    '            End Using

    '            MySettings.PyScripts = Python.CreateEngine.ExecuteFile(newfile)
    '            File.Delete(newfile)

    '            Exit For
    '        End If
    '    Next

    '    Return GH_LoadingInstruction.Proceed

    'End Function

    'Function SomePythonPart() As String

    '    Dim intro As New String("")

    '    intro &= "import sys" & vbCrLf
    '    intro &= "import System.IO" & vbCrLf
    '    intro &= "sys.path.append('C:\Program Files\Rhinoceros 5.0 (64-bit)\Plug-ins\IronPython\Lib')" & vbCrLf
    '    intro &= "import subprocess" & vbCrLf
    '    intro &= "import os" & vbCrLf
    '    intro &= "import shutil" & vbCrLf
    '    intro &= "import glob" & vbCrLf
    '    intro &= "import clr" & vbCrLf
    '    intro &= "clr.AddReferenceToFileAndPath('" & Application.StartupPath & "RhinoCommon.dll')" & vbCrLf
    '    intro &= "import Rhino as rc" & vbCrLf & vbCrLf

    '    Return intro

    'End Function

End Class
