using System;

namespace HYBase.RecordManager
{
    /*
    class RM_Manager {
  public:
       RM_Manager  (PF_Manager &pfm);            // Constructor
       ~RM_Manager ();                           // Destructor
    RC CreateFile  (const char *fileName, int recordSize);  
                                                 // Create a new file
    RC DestroyFile (const char *fileName);       // Destroy a file
    RC OpenFile    (const char *fileName, RM_FileHandle &fileHandle);
                                                 // Open a file
    RC CloseFile   (RM_FileHandle &fileHandle);  // Close a file
};
     */
    class Manager
    {
        public Manager(BufferManager.Manager bufferManager)
        {
            throw new NotImplementedException();
        }
        public void CreateFile(String fileName, int recordSize)
        {
            throw new NotImplementedException();
        }
        public void DestroyFile(String fileName)
        {
            throw new NotImplementedException();
        }
        public FileHandle OpenFile(String fileName)
        {
            throw new NotImplementedException();
        }
        void CloseFile(FileHandle fileHandle)
        {
            throw new NotImplementedException();
        }
    }
}