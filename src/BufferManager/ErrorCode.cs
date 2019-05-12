namespace HYBase.BufferManager
{
    public enum ErrorCode
    {
        EOF,             // end of file
        PAGEPINNED,      // page pinned in buffer
        PAGENOTINBUF,    // page to be unpinned is not in buffer
        PAGEUNPINNED,    // page already unpinned
        PAGEFREE,        // page already free
        INVALIDPAGE,     // invalid page number
        FILEOPEN,        // file handle already open
        CLOSEDFILE,      // file is closed
    }
}