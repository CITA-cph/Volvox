Friend Module GuidsRelease1

    'components
    Public Property Comp_Engine As New Guid("{FE1DDC47-064C-4854-9453-9735F2F069D0}")
    Public Property Comp_FileEngine As New Guid("070150b6-b69c-494b-8734-dd22de18c6ac")

    Public Property Comp_Explode As New Guid("{F953056C-F6D5-47AD-B517-C6B48088698C}")
    Public Property Comp_Create As New Guid("{2DFC6AF7-CF4A-4BF4-9539-D12AE41D292C}")
    Public Property Comp_Transform As New Guid("{0EF7A8B8-64FE-4A29-99D3-B6132F21C845}")
    Public Property Comp_SphereCrop As New Guid("{D2C222CB-5C4E-4676-9A5B-20E8B88AB6E8}")
    Public Property Comp_RandomSub As New Guid("{EB7B1042-B9C9-4790-B2C1-A26A80E4AF97}")
    Public Property Comp_Preview As New Guid("{508C3D1D-3B59-44B2-8071-199AC2E4817A}")
    Public Property Comp_Merge As New Guid("{B117FC52-4234-4EE3-B1D2-04EE3F8BC91E}")
    Public Property Comp_BoxCrop As New Guid("{D09257F4-4CDF-4CD1-BDE1-FC2DD18455A9}")
    Public Property Comp_VoxelSub As New Guid("{FFE4B849-24FE-4D70-BCFC-5082425BB46F}")
    Public Property Comp_PlaneClip As New Guid("{2CAE2F79-F6EC-49C9-9012-40C79AA216F5}")
    Public Property Comp_PlaneSec As New Guid("{71203A2F-53E1-4CD2-B37A-CC6502934F1C}")
    Public Property Comp_Distance As New Guid("{FF2F239B-1F62-475C-9E17-0B61AFD5812F}")
    Public Property Comp_MeshCompare As New Guid("a9408e0d-f777-4ed6-adce-395f59067ede")
    Public Property Comp_Setup As New Guid("37f366ad-e8ea-4e6f-885c-240c7f25c57a")
    Public Property Comp_LoadFile As New Guid("0a057baa-84af-411c-83ec-6fcad3e4eaf1")
    Public Property Comp_SaveFile As New Guid("20e20b88-cbb5-483a-8889-036c3c5f5760")
    Public Property Comp_ClosestPoint As New Guid("ea8db7c9-dc9e-4c32-a7f9-0e9e26c3c5f6")
    Public Property Comp_GetPoints As New Guid("8e4f6927-9226-4ab3-bbbc-c39b2daa8267")
    Public Property Comp_ColoredDelaunay As New Guid("de6f6e0f-057c-42fa-94f9-46b0f4beab0b")
    Public Property Comp_Disjoint As New Guid("c0829c5b-a2a4-4f50-8f6b-9ffb770f0d5d")
    Public Property Comp_VoxelColor As New Guid("07984c11-0643-43aa-a4e0-b9da0c1c12fc")
    Public Property Comp_Description As New Guid("2c2d7295-6225-4e20-bd10-613cd352d61a")

    'instructions
    Public Property Instr_Transform As New Guid("{F9580DFD-5C55-4748-A7AF-2E32A63ED67D}")
    Public Property Instr_SphereCrop As New Guid("{CB1D7B2F-D1A8-4713-BD3F-0995666368A7}")
    Public Property Instr_SpatialSub As New Guid("{9E78B301-17A4-4FFC-AC10-58E25089861E}")
    Public Property Instr_RandomSub As New Guid("{D07DC6F2-C41B-4AE9-A05D-711F3B2867CF}")
    Public Property Instr_Merge As New Guid("{0C4BA2F1-4DFD-4954-B13C-04184D37DAAA}")
    Public Property Instr_BoxCrop As New Guid("{BC25E158-4388-40CC-BED1-FD200F343295}")
    Public Property Instr_PlaneClip As New Guid("{B1E4E107-C97E-4FFC-9DE3-0D071614BDE8}")
    Public Property Instr_BBox As New Guid("{95FC71E2-1C8D-4AB4-8523-0527B1E5D488}")

    'parameters
    'Public Property Param_Instruction As New Guid("{CE3C9BB4-E0EB-4A8A-A25B-8D78E4318CAD}")
    '  Public Property Param_GHCloud As New Guid("{E285577D-197D-42AB-9FA8-E639FB1DDDDD}")
    Public Property Param_CloudLocal As New Guid("fbb5c848-6b0b-4bd9-8599-8732073e0f5f")

End Module

Friend Module GuidsRelease2

    'How to pass on the same guid for next release
    'Public Property CreateComponent As Guid = GuidsRelease1.CreateComponent

    Public Property Instr_LoadXYZ As New Guid("e678ce22-80e5-4f4c-9186-87ef26167145")
    Public Property Instr_LoadE57 As New Guid("1769540f-6e9f-40df-8c2f-bd2a2e7cdc31")

    Public Property Comp_EngineX As New Guid("c47ad36b-c05b-4e85-aa58-80e38d22b53b")
    Public Property Comp_MeshCompare As New Guid("7d6c5652-1f04-4a24-af32-7313eaac2d01")
    Public Property Comp_Disjoint As New Guid("b366e861-8ca8-451e-b6c7-25879677663b")
    Public Property Comp_Random As New Guid("4dde2415-8375-4479-abaa-e9d200952d84")
    Public Property Comp_MultiMerge As New Guid("8238afb9-8fa1-405d-bb67-407b15bff784")

    Public Property CC_Command As New Guid("dda63b42-b05d-4cf3-8b75-f2c8aa1d1696")
    Public Property CC_E57Command As New Guid("bfc7c825-558a-42b7-bd44-0221be0662b1")
    Public Property CC_E57RandomSub As New Guid("62f7f90b-097e-448d-8d60-254c1ff47d3a")
    Public Property CC_E57SpatialSub As New Guid("0cdc04a3-3fea-4e16-bc8a-83238c2b088f")
    Public Property CC_Open As New Guid("96b90d62-9d58-4dab-a8a0-6368a07aca46")
    Public Property CC_toXYZ As New Guid("d523ce6c-9ef9-46bd-89f4-120e3ffb9c2a")
    Public Property CC_XYZCommand As New Guid("95b369f4-5839-4ea8-b575-fccfb463f7e9")

    Public Property Comp_DictAddDictionary As New Guid("fa6d15d6-33b2-454e-ada6-82f453dab372")
    Public Property Comp_DictBounds As New Guid("006e224a-fa88-472a-96c2-01b3d4d98391")
    Public Property Comp_DictColorDictionary As New Guid("5c4c0d91-6ca2-42d9-9726-be558f6642a1")
    Public Property Comp_DictCullCloud As New Guid("41fddcdf-384c-466e-8dde-38e1346ca657")
    Public Property Comp_DictExpression As New Guid("5bf0a400-1f14-4db6-bb51-5218c0b158bc")
    Public Property Comp_DictGetDictionary As New Guid("9a700e90-ce95-49e3-b2b2-ba7ffed449eb")
    Public Property Comp_DictListDictionary As New Guid("549cbc51-9884-4e15-bd79-1bd9b044c2f2")
    Public Property Comp_DictMeshCompare As New Guid("864e08dc-77c5-4565-9bce-2828b50ceab3")
    Public Property Comp_DictRemoveDictionary As New Guid("a2108f99-a238-41d3-bca4-713f5801fe1e")

    Public Property Comp_E57Load As New Guid("b8490fc4-d3f9-47d9-b7c6-10125f17772e")
    Public Property Comp_E57LoadEx As New Guid("55a997f8-5787-44e6-80c1-36e0b1f2fa31")
    Public Property Comp_E57Metadata As New Guid("f84686a1-5c3e-4d8d-9d77-3d527fdb0091")
    Public Property Comp_xyzHint As New Guid("94dfb12d-f46c-42f0-bace-fdcc2e31646c")

    Public Property Comp_Util3dGrid As New Guid("23d40355-1f4d-4b70-b2c1-6ed334fffc99")
    Public Property Comp_UtilAverage As New Guid("f7bb4cc4-5247-498e-a9af-f2717ca08def")

    Public Property Comp_UtilClippingPlane As New Guid("211153d5-5bc2-41c9-acaa-caf2cfc90123")
    Public Property Comp_UtilGetRange As New Guid("cab2d0a7-4288-4e16-bce5-65be545cea8d")
    Public Property Comp_IOLoadXYZEx As New Guid("e3d25833-6d3d-4611-9cd3-d14dea2dd9cd")
    Public Property Comp_UtilMeshInclusion As New Guid("9667b7db-d8d4-42db-ad3e-45844e0c19ad")
    Public Property Comp_UtilRemoveColors As New Guid("b6c3bbbb-994b-49fa-b00b-4e7602ef4413")
    Public Property Comp_IOSaveXYZ As New Guid("83173be0-69d2-433f-aa84-1536b7fe1950")
    Public Property Comp_UtilSelection As New Guid("36eb34a1-51de-4caa-9bc2-438f1f7e0855")
    Public Property Comp_UtilStatistics As New Guid("acd09064-095d-4de8-8d4f-c15807a7a126")


End Module

Friend Module GuidsRelease3

    Public Property Instr_LoadE57 As New Guid("0008cf69-0f5f-4cd6-9b85-d081101dd7c6")

End Module