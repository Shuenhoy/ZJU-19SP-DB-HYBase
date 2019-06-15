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

    public class CatalogTest
    {
        PagedFileManager pagedFileManager;
        RecordManager.RecordManager recordManager;
        CatalogManager.CatalogManager catalogManager;
        static Random rand = new Random();
        public CatalogTest()
        {
            pagedFileManager = new PagedFileManager();
            recordManager = new RecordManager.RecordManager(pagedFileManager);
            catalogManager = new CatalogManager.CatalogManager();
            catalogManager.Init(recordManager, new MemoryStream(), new MemoryStream(), new MemoryStream());
        }

        [Fact]
        public void CatalogManagerTest()
        {
            Assert.False(catalogManager.TableExist("test"));
            catalogManager.CreateTable("test", new[] { new CatalogManager.AttributeInfo(AttrType.Int, "id", 4) });
            Assert.True(catalogManager.TableExist("test"));
            Assert.True(catalogManager.ColumnExist("test", "id"));
            catalogManager.DropTable("test");
            Assert.False(catalogManager.TableExist("test"));
            catalogManager.CreateTable("test", new[] {
                new CatalogManager.AttributeInfo(AttrType.Int, "id2", 4) ,
                new CatalogManager.AttributeInfo(AttrType.String, "name", 8) ,

                });
            catalogManager.CreateTable("test2", new[] {
                new CatalogManager.AttributeInfo(AttrType.Int, "id3", 4) ,
                new CatalogManager.AttributeInfo(AttrType.String, "name4", 8) ,

                });
            Assert.True(catalogManager.TableExist("test"));
            Assert.True(catalogManager.ColumnExist("test", "id2"));
            Assert.True(catalogManager.ColumnExist("test", "name"));

            Assert.False(catalogManager.ColumnExist("test", "id3"));
            Assert.False(catalogManager.ColumnExist("test", "name4"));

            Assert.True(catalogManager.ColumnExist("test2", "id3"));
            Assert.True(catalogManager.ColumnExist("test2", "name4"));

            Assert.False(catalogManager.ColumnExist("test", "id"));


            var attrs1 = catalogManager.GetAttributes("test");
            Assert.Equal(2, attrs1.Length);


        }




    }
}