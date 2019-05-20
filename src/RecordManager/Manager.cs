using System;
using System.IO;

namespace HYBase.RecordManager
{

    class RecordManager
    {
        public RecordManager(BufferManager.BufferManager bufferManager)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Create the record file 
        /// </summary>
        /// <param name="filename">the file name</param>
        /// <returns>the filestream</returns>
        public FileStream CreateFile(String filename)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// This method should retrieve the record with identifier `rid` from the `file`.
        /// </summary>
        /// <param name="file">the record file</param>
        /// <param name="rid">the id of record</param>
        /// <returns>the returned record</returns>
        public Record GetRec(FileStream file, RID rid)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// This method should insert the data pointed to by `data` as a new record in the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <param name="rid"></param>
        public void InsertRec(FileStream file, byte[] data, RID rid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method should delete the record with identifier rid from the file. If the page containing the record becomes empty after the deletion, 
        /// you can choose either to dispose of the page or keep the page in the file for use in the future,
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rid"></param>
        public void DeleteRec(FileStream file, RID rid)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// This method should update the contents of the record in the file that is associated with rec (see the Record class description below). 
        /// This method should replace the existing contents of the record in the file with the current contents of `rec`.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rec"></param>
        public void UpdateRec(FileStream file, Record rec)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// This method should call the corresponding method ForcePages in order to copy the contents of one or 
        /// all dirty pages of the file from the buffer pool to disk.
        /// </summary>
        /// <param name="pageNum"></param>
        public void ForcePages(int pageNum)
        {
            throw new NotImplementedException();
        }
    }
}