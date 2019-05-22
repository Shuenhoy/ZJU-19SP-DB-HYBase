using System.Runtime.InteropServices;

namespace HYBase.CatalogManager
{

    [StructLayout(LayoutKind.Sequential)]
    struct RelationCatalog
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        String relationName;
        int attrCount;
        int indexCount;
    }
    struct AttributeCatalog
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        String relationName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        String attributeName;

    }
}