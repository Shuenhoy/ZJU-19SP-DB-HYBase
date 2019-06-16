using HYBase.RecordManager;
using System;

namespace HYBase.IndexManager
{
    enum ScanState
    {
        Stop,
        Forward,
        Backward
    }
    public class IndexScan
    {
        Index index = null;
        CompOp op = 0;
        byte[] compValue;
        LeafNode l;
        int id;
        ScanState state = ScanState.Stop;
        bool satisfied(ReadOnlySpan<byte> value)
        {
            switch (op)
            {
                case CompOp.EQ:
                    return BytesComp.Comp(value, compValue.AsSpan(), index.fileHeader.AttributeType) == 0;
                case CompOp.GE:
                    return BytesComp.Comp(value, compValue.AsSpan(), index.fileHeader.AttributeType) >= 0;
                case CompOp.GT:
                    return BytesComp.Comp(value, compValue.AsSpan(), index.fileHeader.AttributeType) > 0;
                case CompOp.LE:
                    return BytesComp.Comp(value, compValue.AsSpan(), index.fileHeader.AttributeType) <= 0;
                case CompOp.LT:
                    return BytesComp.Comp(value, compValue.AsSpan(), index.fileHeader.AttributeType) < 0;
                case CompOp.NE:
                    return BytesComp.Comp(value, compValue.AsSpan(), index.fileHeader.AttributeType) != 0;
                default: throw new NotImplementedException();
            }
        }
        public void CloseScan()
        {
            state = ScanState.Stop;
            index.UnPin(l);
        }
        void Backward()
        {
            id--;
            if (id < 0)
            {
                index.UnPin(l);

                if (l.Prev == -1)
                {
                    if (op == CompOp.NE)
                    {
                        state = ScanState.Forward;
                        (l, id) = index.FindLast(compValue).Value;
                        if (id == -1) CloseScan();
                        else if (BytesComp.Comp(l.Data.Get(id), compValue.AsSpan(), index.fileHeader.AttributeType) == 0) Forward();
                    }
                    else
                    {
                        CloseScan();
                    }
                }
                else
                {
                    l = index.GetLeafNode(l.Prev);
                    id = l.ChildrenNumber - 1;
                }
            }
        }
        void Forward()
        {
            id++;
            if (id >= l.ChildrenNumber)
            {
                index.UnPin(l);
                id = 0;
                if (l.Next == -1)
                {
                    CloseScan();
                }
                else
                {
                    l = index.GetLeafNode(l.Next);

                }
            }
        }
        public void OpenScan(Index i, CompOp compOp, byte[] value)
        {
            index = i;
            op = compOp;
            compValue = value;
            if (op == CompOp.GE || op == CompOp.LT || op == CompOp.NE)
            {
                var find = index.FindFirst(value);
                (l, id) = find.Value;

                if (op == CompOp.GE)
                {
                    state = ScanState.Forward;
                }
                else
                {
                    state = ScanState.Backward;
                    if (id < 0)
                    {
                        index.UnPin(l);

                        if (l.Prev == -1)
                        {
                            if (op == CompOp.NE)
                            {
                                state = ScanState.Forward;
                                (l, id) = index.FindLast(compValue).Value;
                                if (BytesComp.Comp(l.Data.Get(id), compValue.AsSpan(), index.fileHeader.AttributeType) == 0)
                                    Forward();
                            }
                            else
                            {
                                CloseScan();
                            }
                        }
                        else
                        {
                            l = index.GetLeafNode(l.Prev);
                            id = l.ChildrenNumber - 1;
                        }
                    }
                    else if (BytesComp.Comp(l.Data.Get(id), compValue.AsSpan(), i.fileHeader.AttributeType) == 0) Backward();


                }
            }
            else
            {
                state = ScanState.Forward;
                (l, id) = index.FindLast(value).Value;
                if (id < 0) CloseScan();
                else if (op == CompOp.GT)
                {
                    if (BytesComp.Comp(l.Data.Get(id), compValue.AsSpan(), i.fileHeader.AttributeType) == 0) Forward();
                }

            }

        }
        public bool NextEntry(out RID r)
        {
            var tmp = GetNextEntry();
            if (tmp == null)
            {
                r = new RID { };

                return false;
            }
            r = tmp.Value.Item2;
            return true;
        }
        public (byte[], RID)? GetNextEntry()
        {
            if (state == ScanState.Stop) return null;
            var value = l.Data.Get(id).ToArray();
            if (!satisfied(value))
            {
                CloseScan();
                return null;
            }
            var rid = new RID(l.ridPage[id], l.ridSlot[id]);

            switch (state)
            {
                case ScanState.Backward:
                    Backward();
                    break;
                case ScanState.Forward:
                    Forward();
                    break;
                default: throw new NotImplementedException();
            }

            return (value, rid);
        }
    }
}