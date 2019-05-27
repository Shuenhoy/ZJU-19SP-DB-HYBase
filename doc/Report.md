# MiniSQL - HYBase报告（草稿）



[TOC]



##  Buffer Manager（已完成）



Buffer Manager是整个数据库系统中最底层的部分.

### 分页文件(Paged File)

数据库系统中的文件一般较大，我们一般无法将整个文件一次性读取到内存中。因此我们引入分页文件的概念，即：一个完整的文件被分成了若干**页（Page）**，每次从硬盘读取一个页的数据。对于传统的机械硬盘，整个硬盘被分割成为了若干个大小为4096字节的扇区，程序在进行磁盘IO时，主要的时间开销在于磁盘探头转动到对应的扇区所在的位置，因此读取一个扇区与读取一个字节的时间相差无几。

基于这样的事实，我们将每页的大小规定为4096字节并将每页的起始地址对齐到4096的整数倍，这样我们就可以实现快速读入整页数据的功能。

与分页文件相关的类及其方法如下：

#### PagedFileManager

负责创建、打开分页文件

```c#
namespace HYBase.BufferManager
    class PagedFileManager
    {
        // 创建一个分页文件
        public PagedFile CreateFile(Stream file);
        // 打开一个分页文件
        public PagedFile OpenFile(Stream file);
    }
}
```

#### PagedFile

通过`PagedFileManager`打开或创建分页文件后，我们会得到`PagedFile`对象

```c#
namespace HYBase.BufferManager{
    public class PagedFile
    {
        // 设置某页的数据
        public void SetPageData(int pageNum, byte[] data);
        // 获取某页的数据。注：可以直接修改返回的数组
        public byte[] GetPageData(int pageNum);
        // 销毁回收某页
        public void DeallocatePage(int pageNum);
        // 销毁回收所有的页
        public void DeallocatePages();
        // 分配某页，返回页的编号
        public int AllocatePage();
        // 获取文件头部数据（注释 to XY: 暂时还没实现）
        public byte[] GetHeader();
        // 修改文件头部数据（注释 to XY: 暂时还没实现）
        public void SetHeader(byte[] header);
        // 将文件头写回文件
        public void WriteHeader();
        // 关闭该文件
        public void Close();
    }
}
```

### 缓存(Buffer)机制

尽管我们通过分页的方式加块了磁盘IO的速度，但是与内存IO相比，磁盘IO的效率仍然非常低。根据由于我们对文件的不同部分访问的频率是不同的，我们可以考虑将常访问的部分留在内存中，只有在有必要的时候才将他们输出到硬盘中，这样可以大大提高操作的效率。

具体的做法是，维护一个能容纳固定数量元素的缓存区，每次要从硬盘中获取数据时，先通过哈希表检查缓存区中是否已经存在对应的值，若不存在再从硬盘读取；同样，要向硬盘写入内容时，我们也先在缓存区检查。

缓存区中维护的元素应当是我们访问的比较多的，这里我们采用了简单的LRU策略。即维护一个队列，队首的为访问时间距离现在最远的，队尾的为我们最近访问的元素。当我们访问一个元素时，如果它不存在于队列中，则将它置于队尾；若它已经存在于队列中，则将其从队列中取出放至队尾。若队列中元素数量已经达到最大限制，且有新的元素需要入队时，则将队首元素弹出，如有需要写回硬盘，并从缓存区清除。

同时，我们引入了钉(Pin)的机制，缓存区中一些被频繁访问的元素可以被显式钉住一次或多次，一个被钉住次数(PinCount)大于0的元素将不会被从缓存区中清除。

由于我们的硬盘IO是以页为单位的，所以我们的缓存区中的对象也为页。

判断，一个缓存区中元素是否需要被写回硬盘的依据是，如果某个缓存区中的元素被修改过，我们就将其标记为Dirty。Dirty的元素是需要被写回硬盘的。

与缓存区相关的接口均被`PagedFile`封装

#### PagedFile(续)

```C#
namespace HYBase.BufferManager{
    public class PagedFile
    {
        // 手动标记某页为Dirty，
        // 用于修改了GetPageData返回的数组的情况
        public void MarkDirty(int pageNum);
        // 将所有页的数据强制写回硬盘
        public void ForcePages();
        // 将某页的数据强制写回硬盘
        public void ForcePage(int pageNum);
        // 将所有没有被钉住(PinCount==0)的元素写回硬盘
        // 并从缓存区中清除
        public void FlushPages();
		// 取消一次钉住(PinCount--)        
        public void UnPin(int pageNum);
    }
}
```





## Index Manager（存储部分完成）



在没有索引的情况下，我们要找到需要的记录需要的时间为$O(n)$。所谓的索引就是特殊的数据结构，可以让我们以更快的查找元素。当我们具有对某个字段的索引时，我们在Index中，通过该字段可以快速查找到对应的记录的编号(RID)。

在内存中，我们可以使用各类平衡二叉树（红黑树，AVL树，Splay树等）来实现对元素的快速查找。但是这些二叉树难以被以页的形式组织，因此并不适合在硬盘中使用。我们采用方便在硬盘存储的B+树实现索引。

### B+树的结构

一个$m$阶B+树由内部节点(Internal Node)与叶子节点(Leaf Node)构成：

* 每个内部节点包含$k(\left\lceil\frac{m}{2}\right\rceil\le k\le m)$个子节点，和若干个键$a_1<a_2<\cdots<a_{k-2}$。第$i$个子节点中所包含的任何值$v_{ij}$都满足$a_{i-1}<v_{ij}<a_i$
* 每个叶子节点都包含若干元素，为实际存储的值
* 所有叶子节点的深度都相同

这样，当我们要查询某个值时，只需要从根节点开始确定我们要查询的键所在的区间，并不停递归向下。不难得出，查询的复杂度为$O(\log N)$。

我们可以简单的将他们按照下面的方式定义

```c#
namespace HYBase.IndexManager {
    struct InternalNode
    {
        public int Father;         // 该节点的父节点
        public int ChildrenNumber; // 该节点的孩子节点数

        public bool[] Valid;       // 某个孩子节点是否有效
        public int[] Children;     // 孩子节点的page编号
        public byte[] Values;      // 键部分
    }
    struct LeafNode
    {
        public int Father; // 该节点的父节点
        public int Prev;   // 该节点的上一个相邻叶子节点
        public int Next;   // 该节点的下一个相邻子节点
        public int ChildrenNumber; // 该节点包含的元素个数

        public bool[] Valid; // 某个元素是否有效
        public byte[] Data;  // 数据部分
        public RID[]  RIDs;  // 记录编号
    }
}
```



### B+树的磁盘存储

由于磁盘IO的速度非常慢，因此我们需要尽量减少磁盘IO的数目。对于B+树而言，由于B+树为多叉树，我们可以尽量增加B+树的内部节点的孩子数量，这样，树的高度就会降低。

我们令B+树的一个节点占据一整个页，然后每页包含的孩子或数据的个数为使得整页大小不超过4096字节的，尽量大的，4的倍数。这样，每个节点都会有非常多的孩子，整棵树的高度将很低。

与B+树的磁盘存储有关的函数如下

```c#
namespace HYBase.IndexManager
{
    class Index
    {
       	// 强制将所有数据写入文件
        public void ForcePages();
        void UnPin(in InternalNode node);
        void UnPin(in LeafNode node);
        
        // 将对某个节点的修改提交到数据结构
        void SetInternalNode(in InternalNode node);
        
		// 将对某个节点的修改提交到数据结构
        void SetLeafNode(in LeafNode node);
        
      	// 销毁某个节点
        internal void DeallocateNode(int pageNum);
        
        // 分配一个新的内部节点
        internal InternalNode AllocateInternalNode();

        // 分配一个新的叶子节点
        internal LeafNode AllocateLeafNode();

        // 获取编号为 `id` 的内部节点，会增加对应页的PinCount
        // 使用完毕后务必使用UnPin
        internal InternalNode GetInternalNode(int id);
        
        // 获取编号为 `id` 的叶子节点，会增加对应页的PinCount
        // 使用完毕后务必使用UnPin
        internal LeafNode GetLeafNode(int id);
    }
}
```

### B+树的插入

* 找到对应的叶子节点
* 如果该节点没满
  * 遍历叶子节点的所有元素，找到插入位
  * 在插入位插入新元素，并将所有元素后移
* 如果该节点已满
  * 分配一个新的叶子节点，将原节点中一半的元素挪到新节点中，并将新节点插入为原节点父节点的孩子，该过程同样类似于上述在子节点的插入，且是递归向上的

### B+树的删除

我们采用惰性删除的方式

* 找到对应的叶子节点
* 将对应元素的`Valid`置为`false`
* 如果一个节点所有的元素或子节点都被删除，则移除该节点，并在它的父节点中重复上述惰性删除的过程。

### Index上的查询

我们采用`IndexScan`类来实施在Index上的查询操作。

```c#
namespace HYBase.IndexManager
{
    class IndexScan
    {
        // 启动扫描，寻找index中与value比较满足compOp的值
        void OpenScan(Index index, CompOp compOp, object value);
        // 下一个满足条件的值的Record ID
        RID GetNextEntry();
        void CloseScan();
    }
}
```



## Record Manager

### Record的存储

Record同样被存储在分页文件中。为了便于实现，我们不考虑有跨页的Record的情况。

首先我们在文件头存储每条记录的字节数`inrecordSize`，每页的最大记录数`int numberRecordsOnPage`，页数`int numberPages`与第一个未满的页的编号`firstNotFull`四项数据。

对于每页，实际上与上述的`Index`中每页的内容是类似的，包含该页的记录数`int numberRecords`，下一个没满的页的编号`nextNotFull`，每个记录是否有效`bool[] Valid`与实际的数据`byte[] data`三项。

对于每条记录，都由RID(Record ID)确定，RID规定了Record所在的Page编号(PageID)以及它在Page中的位置(SlotID)。

### Record的插入

当我们要插入一个记录时，只需要找到`firstNotFull`对应的页，并扫描该页的每个位置，将要插入的元素放在第一个遇到的`Valid`为`false`的元素即可。如果在插入后，该页已满，则将`firstNotFull`更新为`nextNotFull`

### Record的删除

当我们要删除一个记录时，只需要将其对应的`Valid`置为`false`，并交换`firstNotFull`与`nextNotFull`

### Record的扫描

类似于Index的扫描，只是扫描方式为顺序扫描整个文件

```C#
namespace HYBase.RecordManager
{
    class FileScan
    {
      	// 启动扫描，寻找记录中偏移量为attrOffset，
        // 长度为attrLength字节的值与value比较满足compOp的记录
        void OpenScan(RecordFile file,
            int attrLength,
            int attrOffset,
            CompOp compOp,
            object value);

        Record NextRecord();
        void CloseScan()
        {
            throw new NotImplementedException();
        }
    }
}
```

## CatalogManager

Catalog文件负责保存表信息、索引信息等。为了提高系统的复用率，我们将Catalog信息保存在若干Record文件中。

### 表信息的存储(RelationCatalog)

表信息保存在`relcatalog` Record文件中，其中的每条记录包含`string relationName`，字段个数`attrCount`和索引个数`indexCount`三个字段。

### 字段信息的存储(AttributeCatalog)

字段信息保存在`attrcatalog` Record文件中，其中的每条记录包含该字段所属的表名`string relationName`，该字段的字段名`string attributeName`，该字段在记录中的偏移量`int offset`，该字段的字节数目`int attributeLength`，该字段的索引的编号`int indexNo`(-1表示无索引)。

### 索引信息的存储(IndexCatalog)

索引信息保存在`indexcatalog` Record文件中，其中的每条记录包含该索引所属的表明`string relationName`，该索引的索引名`string indexName`，该索引的编号`int indexID`三个字段。



## Interpreter （已基本完成）