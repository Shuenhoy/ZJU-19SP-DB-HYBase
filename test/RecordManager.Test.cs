using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using HYBase.RecordManager;
using HYBase.BufferManager;
using HYBase.IndexManager;
using static HYBase.Utils.Utils;
using HYBase.Utils;
using System.Runtime.InteropServices;
using System.Text;
namespace HYBase.UnitTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct record
    {
        public int a, b;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string str;
    }
    public class RecordTest
    {
        PagedFileManager pagedFileManager;
        RecordManager.RecordManager recordManager;
        static Random rand = new Random();
        public RecordTest()
        {
            pagedFileManager = new PagedFileManager();
            recordManager = new RecordManager.RecordManager(pagedFileManager);
        }

        [Fact]
        void FileScanTest()
        {
            MemoryStream m1 = new MemoryStream();
            var record = recordManager.CreateFile(m1, 14);
            var lists = Enumerable.Range(0, 10000).Select(x => (x, rand.Next(), Utils.RandomString(5))).Filter(x => rand.NextDouble() > 0.7 ? true : false).ToList();
            var lists1 = lists.ToList();
            lists1.Shuffle();

            foreach (var (p, s, str) in lists)
            {
                record.InsertRec(StructureToByteArray(new record { a = p, b = s, str = str }));
            }


            FileScan scan = new FileScan();
            {
                byte[] pred = BitConverter.GetBytes(lists[1000].x);

                scan.OpenScan(record, 4, 0, AttrType.Int, CompOp.LT, pred);
                var l3 = new List<int>();
                while (true)
                {
                    var x = scan.GetNextRecord();
                    if (x == null)
                    {
                        break;
                    }
                    var rec = ByteArrayToStructure<record>(x.Data);

                    l3.Add(rec.a);
                }
                Assert.Equal(1000, l3.Length());

                l3.Sort();

                Assert.Equal(lists.Take(1000).Select(x => x.x).ToList(), l3);
            }
            {
                lists.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                byte[] pred = BitConverter.GetBytes(lists[1000].Item2);

                scan.OpenScan(record, 4, 4, AttrType.Int, CompOp.LT, pred);
                var l3 = new List<int>();
                /*
                
                
                 */
                while (true)
                {
                    var x = scan.GetNextRecord();
                    if (x == null)
                    {
                        break;
                    }
                    var rec = ByteArrayToStructure<record>(x.Data);

                    l3.Add(rec.b);
                }

                l3.Sort();

                Assert.Equal(lists.Take(1000).Select(x => x.Item2).ToList(), l3);
                Assert.Equal(1000, l3.Length());

            }

            {
                lists.Sort((a, b) => a.Item3.CompareTo(b.Item3));
                byte[] pred = Encoding.UTF8.GetBytes(lists[1000].Item3);

                scan.OpenScan(record, 6, 8, AttrType.String, CompOp.LT, pred);
                var l3 = new List<string>();

                while (true)
                {
                    var x = scan.GetNextRecord();
                    if (x == null)
                    {
                        break;
                    }
                    var rec = ByteArrayToStructure<record>(x.Data);

                    l3.Add(rec.str);
                }

                l3.Sort();

                Assert.Equal(lists.Take(1000).Select(x => x.Item3).ToList(), l3);
                Assert.Equal(1000, l3.Length());

            }

        }
    }
}