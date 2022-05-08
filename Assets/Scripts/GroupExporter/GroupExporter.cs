using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using System.Text;
using System.IO;
using System;
using System.Runtime.InteropServices;

public class GroupExporter
{
    public enum ExportFileType
    {
        NONE,
        OBJ,
        STL
    }

    public static void ExportGroupAsFile(GroupBehaviour group, string fileName, ExportFileType fileType)
    {
        if (fileType == ExportFileType.OBJ)
            OBJExporter.ExportGroupAsOBJFile(group, fileName);
        else if (fileType == ExportFileType.STL)
            STLExporter.ExportGroupAsSTLFile(group, fileName);
    }
}
