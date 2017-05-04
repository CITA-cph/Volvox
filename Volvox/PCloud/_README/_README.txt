README

1. OBEY THE NAMING CONVENTION

PREFIX explanation : 

Cloud_ File_		MAIN engine components and stuff related directly to them

Eng_				Grasshopper components which create INSTRUCTIONS

GH_					Grasshopper types

Instr_ Instruction itself, has to override those subs, functions and properties :
					Sub New
					Public Overrides ReadOnly Property InstructionGUID As Guid
					Public Overrides ReadOnly Property InstructionType As String
					Public Overrides Function Execute(ByRef PointClouds As List(Of GH_Cloud)) As Boolean

Param_				Grasshopper parameters

Util_				Grasshopper components which are not related to the engine

GUID_				Guid tables

Icon_				Icons

Control_			Windows Forms controls

Asmbl_				Related to assembly (assembly description for instance)

Math_				Mathematical functions etc.

Settings_			Internal settings (point preview radius etc.)

Other_				Other stuff which can't be classified to any other category


2.Changelog
