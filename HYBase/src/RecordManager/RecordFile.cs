using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using HYBase.Utils;
using System.Linq;
using HYBase.BufferManager;

[assembly: InternalsVisibleTo("test")]
namespace HYBase.RecordManager
{
    [StructLayout(LayoutKind.Sequential)]
    struct RecordFileHeader
    {
        public int recordSize;
        public int numberPages;
        public int FirstFree;
        public int increaseKey;
    }

    public class RecordFile
    {

        internal RecordFileHeader fileHeader;
        internal PagedFile file;
        public RecordFile(PagedFile f, int recordSize)
        {
            fileHeader = new RecordFileHeader
            {
                recordSize = recordSize,
                numberPages = 0,
                FirstFree = -1
            };
            file = f;
        }
        public RecordFile(PagedFile f)
        {
            fileHeader = Utils.Utils.ByteArrayToStructure<RecordFileHeader>(f.GetHeader());

            file = f;
        }
        internal void WriteHeader()
        {
            file.SetHeader(Utils.Utils.StructureToByteArray(fileHeader));
        }

        public void Close()
        {
            WriteHeader();
            file.Close();
        }
        public int IncreaseKey()
        {
            return fileHeader.increaseKey;
        }
        public Record GetRec(RID rid)
        {
            var page = GetPage(rid.PageID);
            var rec = new Record(page.Data.Get(rid.SlotID).ToArray(), rid);
            UnPin(page);
            return rec;
        }
        internal RecordFilePage AllocatePage()
        {
            var page = RecordFilePage.AllocateEmpty(fileHeader.recordSize);
            var pageNum = file.AllocatePage();
            page.pageNum = pageNum;
            page.Raw = file.GetPageData(page.pageNum);

            fileHeader.numberPages++;

            return page;
        }
        internal void SetPage(in RecordFilePage page)
        {
            page.WriteBack(fileHeader.recordSize);
            file.MarkDirty(page.pageNum);
        }
        internal RecordFilePage GetPage(int page)
        {
            var node = RecordFilePage.AllocateEmpty(fileHeader.recordSize);
            node.pageNum = page;
            RecordFilePage.FromByteArray(file.GetPageData(page), fileHeader.recordSize, ref node);
            return node;
        }
        public RID InsertRec(byte[] data)
        {
            fileHeader.increaseKey++;
            if (fileHeader.FirstFree == -1)
            {
                var page = AllocatePage();
                fileHeader.FirstFree = page.pageNum;
                page.Valid[0] = true;
                page.RecordNum++;
                page.Data.Set(0, data.AsSpan());
                SetPage(page);
                UnPin(page);
                return new RID(page.pageNum, 0);
            }
            else
            {
                var page = GetPage(fileHeader.FirstFree);
                for (int i = 0; i < page.Valid.Length; i++)
                {
                    if (!page.Valid[i])
                    {
                        page.Valid[i] = true;
                        page.Data.Set(i, data.AsSpan());
                        page.RecordNum++;
                        SetPage(page);
                        UnPin(page);
                        if (page.RecordNum == page.Valid.Length)
                        {
                            fileHeader.FirstFree = page.NextFree;
                        }
                        return new RID(page.pageNum, i);
                    }
                }
            }
            throw new NotImplementedException();

        }
        internal void UnPin(in RecordFilePage page)
        {
            file.UnPin(page.pageNum);
        }

        public void DeleteRec(RID rid)
        {
            var page = GetPage(rid.PageID);
            if (page.Valid[rid.SlotID])
            {
                page.Valid[rid.SlotID] = false;
                page.RecordNum--;
                page.NextFree = fileHeader.FirstFree;
                fileHeader.FirstFree = page.pageNum;
                SetPage(page);
                UnPin(page);
            }
        }

        public void UpdateRec(Record rec)
        {
            var page = GetPage(rec.Rid.PageID);
            if (page.Valid[rec.Rid.SlotID])
            {
                page.Data.Set(rec.Rid.SlotID, rec.Data.AsSpan());
            }
            SetPage(page);
            UnPin(page);
        }
        public void ForcePages(int pageNum)
        {
            WriteHeader();
            file.ForcePages();
        }
    }
}