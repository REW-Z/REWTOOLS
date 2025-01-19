using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace REWTOOLS
{

    public struct Union4B
    {
        public byte b0;
        public byte b1;
        public byte b2;
        public byte b3;

        public int Int32
        {
            get
            {
                byte[] data = new byte[] { b0, b1, b2, b3 };
                return System.BitConverter.ToInt32(data, 0);
            }
            set
            {
                byte[] data = System.BitConverter.GetBytes(value);
                b0 = data[0];
                b1 = data[1];
                b2 = data[2];
                b3 = data[3];
            }
        }
        public float Single
        {
            get
            {
                byte[] data = new byte[] { b0, b1, b2, b3 };
                return System.BitConverter.ToSingle(data, 0);
            }
            set
            {
                byte[] data = System.BitConverter.GetBytes(value);
                b0 = data[0];
                b1 = data[1];
                b2 = data[2];
                b3 = data[3];
            }
        }
    }


    ///// <summary>
    ///// 静态链表
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public class StaticLinkedList<T> : IEnumerable<T>, IEnumerable
    //{
    //    //节点
    //    public struct Node
    //    {
    //        public T value;
    //        public int next;
    //        public int prev;
    //    }
    //    public Node[] nodes;

    //    //指针
    //    private int firstPtr;
    //    private int lastPtr;
    //    private int emptyFirstPtr;
    //    private int emptyLastPtr;



    //    //标记
    //    private bool hasItem;//not empty mark  
    //    private int count;

    //    //数据
    //    private int datalength;
    //    private T defaultValue;

    //    // ---------------------------- Public Interface -------------------------------

    //    public StaticLinkedList(int initLength, T defaultValue = default)
    //    {
    //        this.datalength = initLength;
    //        this.defaultValue = defaultValue;

    //        nodes = new Node[this.datalength];
    //        for (int i = 0; i < nodes.Length; ++i)
    //        {
    //            nodes[i] = new Node() { value = this.defaultValue, next = -1, prev = -1 };
    //        }

    //        this.firstPtr = 0;
    //        this.lastPtr = 0;
    //        this.count = 0;

    //        hasItem = false;
    //    }


    //    public int Count => this.count;


    //    public void AddFirst(T newItem)
    //    {
    //        AddFirstInternal(newItem);
    //    }

    //    public void Remove(T item)
    //    {
    //        for (int i = 0; i < nodes.Length; ++i)
    //        {
    //            if (object.Equals(nodes[i].value, item))
    //            {
    //                RemoveAtInternal(i);
    //                return;
    //            }
    //        }
    //    }


    //    public void RemoveAt(int idx)
    //    {
    //        RemoveAtInternal(idx);
    //    }


    //    public void Clear()
    //    {
    //        nodes = new Node[this.datalength];
    //        for (int i = 0; i < nodes.Length; ++i)
    //        {
    //            nodes[i] = new Node() { value = this.defaultValue, next = -1, prev = -1 };
    //        }

    //        this.firstPtr = 0;
    //        this.lastPtr = 0;
    //        this.count = 0;

    //        hasItem = false;
    //    }

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        if (!hasItem) yield break;

    //        int currentIdx = firstPtr;
    //        while (currentIdx > -1 && currentIdx < nodes.Length)
    //        {
    //            yield return nodes[currentIdx].value;
    //            currentIdx = nodes[currentIdx].next;
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        if (!hasItem) yield break;

    //        int currentIdx = firstPtr;
    //        while (currentIdx > -1 && currentIdx < nodes.Length)
    //        {
    //            yield return nodes[currentIdx].value;
    //            currentIdx = nodes[currentIdx].next;
    //        }
    //    }





    //    // ---------------------------- Private -------------------------------
    //    private void AddFirstInternal(T newItem)
    //    {
    //        if (!hasItem)//空表
    //        {
    //            nodes[firstPtr] = new Node() { value = newItem, prev = -1, next = -1 };
    //        }
    //        else//有至少一个元素
    //        {
    //            //查找空位
    //            int emptyIdx = FindEmpty();

    //            //填充新的首节点
    //            nodes[emptyIdx] = new Node() { value = newItem, prev = -1, next = firstPtr }; //???

    //            //修改原来的首节点
    //            nodes[firstPtr] = new Node() { value = nodes[firstPtr].value, prev = emptyIdx, next = nodes[firstPtr].next };

    //            //首节点指针
    //            firstPtr = emptyIdx;
    //        }

    //        count += 1;//元素数量+1
    //        hasItem = true;//标记为非空
    //    }
    //    private void RemoveAtInternal(int idx)
    //    {
    //        if (!hasItem) return;//空表
    //        if (idx < 0 || idx > nodes.Length - 1) return;//越界

    //        int thisPrev = nodes[idx].prev;
    //        int thisNext = nodes[idx].next;

    //        if(firstPtr == lastPtr)//单个元素
    //        {
    //            if(idx == firstPtr)
    //            {
    //                //清空
    //                nodes[idx] = new Node() { value = defaultValue, next = -1, prev = -1 };

    //                //数量-1
    //                count -= 1; if (count != 0) { throw new Exception("Error count at static linked list!"); }//数量-1
    //                hasItem = false;//标记为空
    //            }
    //            else
    //            {
    //                //要删除的元素不存在  
    //            }
    //        }
    //        else//多个元素
    //        {
    //            //前置处理
    //            if (thisPrev == -1)//是第一个节点
    //            {
    //                firstPtr = thisNext;
    //            }
    //            else
    //            {
    //                nodes[thisPrev] = new Node() { value = nodes[thisPrev].value, prev = nodes[thisPrev].prev, next = thisNext };
    //            }

    //            //后继处理
    //            if (thisNext == -1)//是最后一个节点
    //            {
    //                lastPtr = thisPrev;
    //            }
    //            else
    //            {
    //                nodes[thisNext] = new Node() { value = nodes[thisNext].value, prev = thisPrev, next = nodes[thisNext].next };
    //            }

    //            //清空
    //            nodes[idx] = new Node() { value = this.defaultValue, next = -1, prev = -1 };
    //            //数量-1
    //            count -= 1; if (count == 0) { throw new Exception("Error count at static linked list!"); }//数量-1
    //        }
    //    }

    //    private int FindEmpty()
    //    {
    //        int emptyNodeIdx = -1;
    //        for (int i = 0; i < nodes.Length; ++i)
    //        {
    //            if (IsEmpty(i))
    //            {
    //                emptyNodeIdx = i;
    //                break;
    //            }
    //        }
    //        if (emptyNodeIdx == -1)
    //        {
    //            int oldLength = nodes.Length;
    //            Extend();
    //            emptyNodeIdx = oldLength - 1 + 1;
    //        }

    //        return emptyNodeIdx;
    //    }
    //    private bool IsEmpty(int idx)
    //    {
    //        if (!(idx > -1 && idx < nodes.Length))
    //            throw new Exception("static linked list out of index!");

    //        if (idx == firstPtr && firstPtr == lastPtr)//单个元素或者无元素  且  查找的是起始位置
    //        {
    //            if (hasItem)
    //            {
    //                return false;
    //            }
    //            else
    //            {
    //                return true;
    //            }   
    //        }
    //        else
    //        {
    //            bool j1 = (nodes[idx].next == -1 && nodes[idx].prev == -1);
    //            bool j2 = (nodes[idx].next == -1 && nodes[idx].prev == -1 && object.Equals(nodes[idx].value, defaultValue));
    //            return (j1);
    //        }
    //    }

    //    private void Extend()
    //    {
    //        Debug.LogAssertion("StaticLinkedList Extend...");
    //        //X2
    //        var newArray = new Node[nodes.Length * 2];
    //        for (int i = 0; i < newArray.Length; ++i)
    //        {
    //            if (i < nodes.Length)
    //            {
    //                newArray[i] = nodes[i];
    //            }
    //            else
    //            {
    //                newArray[i] = new Node() { value = defaultValue, prev = -1, next = -1 };
    //            }
    //        }

    //        this.nodes = newArray;
    //    }

    //}





    /// <summary>
    /// 静态链表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StaticLinkedList<T> : IEnumerable<T>, IEnumerable
    {
        //节点
        public struct Node
        {
            public T value;
            public int next;
            public int prev;
        }
        public Node[] nodes;

        //指针
        private int firstPtr;
        private int lastPtr;
        private int emptyFirstPtr;
        private int emptyLastPtr;



        //标记
        private int count;

        //数据
        private int datalength;
        private T defaultValue;

        // ---------------------------- Public Interface -------------------------------

        public StaticLinkedList(int initLength, T defaultValue = default)
        {
            this.datalength = initLength;
            this.defaultValue = defaultValue;

            Clear();
        }


        public int Count => this.count;


        public int AddLast(T newItem)
        {
            return AddLastInternal(newItem);
        }

        public void Remove(T item)
        {
            if (object.Equals(item, this.defaultValue)) return;


            for (int i = 0; i < nodes.Length; ++i)
            {
                if (object.Equals(nodes[i].value, item))
                {
                    RemoveAtInternal(i);
                    return;
                }
            }
        }


        public void RemoveAt(int indexInNodes)
        {
            RemoveAtInternal(indexInNodes);
        }

        public void Clear()
        {
            nodes = new Node[this.datalength];

            this.firstPtr = 0;
            this.lastPtr = 0;
            this.emptyFirstPtr = 0;
            this.emptyLastPtr = this.datalength - 1;

            for (int i = 0; i < nodes.Length; ++i)
            {
                int nodePrev = (i - 1) > -1 ? (i - 1) : -1;
                int nodeNext = (i + 1) < (nodes.Length) ? (i + 1) : -1;
                nodes[i] = new Node() { value = this.defaultValue, next = nodeNext, prev = nodePrev };
            }

            this.count = 0;
        }

        public void SetValueAt(int indexInNodes, T newvalue)
        {
            var node = this.nodes[indexInNodes];

            node.value = newvalue;

            this.nodes[indexInNodes] = node;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int current = firstPtr;
            while (current != emptyFirstPtr && current != -1)
            {
                if (object.Equals(nodes[current].value, defaultValue))
                {
                    Debug.LogAssertion("遍历到空元素：打印链表：");
                    Log();
                }

                yield return nodes[current].value;

                if (current == lastPtr) break;

                current = nodes[current].next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int current = firstPtr;
            while (current != emptyFirstPtr && current != -1)
            {
                if (object.Equals(nodes[current].value, defaultValue))
                {
                    Debug.LogAssertion("遍历到空元素：打印链表：");
                    Log();
                }

                yield return nodes[current].value;

                if (current == lastPtr) break;

                current = nodes[current].next;
            }
        }





        // ---------------------------- Private -------------------------------
        private int AddLastInternal(T newItem)
        {
            if (object.Equals(newItem, defaultValue))
            {
                Debug.LogAssertion("正在往表中添加默认值！");
            }

            //不足则扩充
            if (emptyFirstPtr == emptyLastPtr)
            {
                Extend();
            }
            //获取空位
            int target = emptyFirstPtr;

            //首空位指针更新
            int newEmptyFirst = this.nodes[emptyFirstPtr].next;
            if (newEmptyFirst < 0 || newEmptyFirst > this.nodes.Length - 1)
            {
                Debug.LogAssertion("空节点列表指针越界。打印：");
                Log();
            }
            emptyFirstPtr = newEmptyFirst;

            //旧的末数据节点更新
            nodes[lastPtr] = new Node() { value = nodes[lastPtr].value, prev = nodes[lastPtr].prev, next = target };

            //新的末数据节点设置
            nodes[target] = new Node() { value = newItem, prev = lastPtr, next = -1 };

            //末数据位指针更新
            lastPtr = target;

            count += 1;



            return target;
        }
        private void RemoveAtInternal(int idx)
        {
            if (idx < 0 || idx > (this.nodes.Length - 1)) return;//越界
            if (object.Equals(this.nodes[idx].value, this.defaultValue)) return;//空闲节点不能删除
            if (firstPtr == lastPtr && lastPtr == emptyFirstPtr) return; //空表  



            if (idx == firstPtr && idx == lastPtr)//唯一节点  
            {
                FreeNodeInternal(idx);
            }
            else if (idx == firstPtr)//首节点
            {
                int thisNext = nodes[idx].next;
                nodes[thisNext] = new Node() { value = nodes[thisNext].value, prev = -1, next = nodes[thisNext].next };
                firstPtr = thisNext;

                FreeNodeInternal(idx);
            }
            else if (idx == lastPtr)//末节点
            {
                int thisPrev = nodes[idx].prev;
                nodes[thisPrev] = new Node() { value = nodes[thisPrev].value, prev = nodes[thisPrev].prev, next = -1 };
                lastPtr = thisPrev;

                FreeNodeInternal(idx);
            }
            else//中间节点
            {
                int thisPrev = nodes[idx].prev;
                int thisNext = nodes[idx].next;
                if (thisPrev > -1)
                {
                    nodes[thisPrev] = new Node() { value = nodes[thisPrev].value, prev = nodes[thisPrev].prev, next = nodes[idx].next };
                }
                if (thisNext > -1)
                {
                    nodes[thisNext] = new Node() { value = nodes[thisNext].value, prev = nodes[idx].prev, next = nodes[thisNext].next };
                }

                FreeNodeInternal(idx);
            }

            count -= 1;
        }
        private void FreeNodeInternal(int idx)
        {
            if (idx < 0 || idx > this.nodes.Length - 1)
            {
                Debug.LogAssertion("Free Idx越界");
                Log();
            }

            int nextEmpty = nodes[emptyFirstPtr].next;
            if (emptyFirstPtr == emptyLastPtr)
                nextEmpty = emptyLastPtr;

            nodes[idx] = new Node() { value = defaultValue, prev = -1, next = emptyFirstPtr };
            nodes[emptyFirstPtr] = new Node() { value = defaultValue, prev = idx, next = nextEmpty };
            emptyFirstPtr = idx;
        }

        private void Extend()
        {
            int oldLength = this.datalength;

            //创建新的数组并拷贝数据 
            this.datalength *= 2;
            var newDataNodes = new Node[this.datalength];
            for (int i = 0; i < newDataNodes.Length; ++i)
            {
                if (i < this.nodes.Length)
                {
                    newDataNodes[i] = this.nodes[i];
                }
                else
                {
                    int nodePrev = (i - 1) > -1 ? (i - 1) : -1;
                    int nodeNext = (i + 1) < (newDataNodes.Length) ? (i + 1) : -1;
                    newDataNodes[i] = new Node() { value = this.defaultValue, next = nodeNext, prev = nodePrev };
                }
            }
            //指向新数组
            this.nodes = newDataNodes;

            //首尾空节点指针设置  
            this.nodes[emptyLastPtr] = new Node() { value = this.defaultValue, prev = this.nodes[emptyLastPtr].prev, next = (oldLength - 1 + 1) };
            this.nodes[(oldLength - 1 + 1)] = new Node() { value = this.defaultValue, prev = emptyLastPtr, next = this.nodes[(oldLength - 1 + 1)].next };
            emptyLastPtr = this.nodes.Length - 1;

        }

        public void Log()
        {
            System.Text.StringBuilder strb = new System.Text.StringBuilder();

            strb.Append("************ StaticLinkedList(size:" + this.nodes.Length + ") *************");

            for (int i = 0; i < this.nodes.Length; ++i)
            {
                strb.Append("\n");

                if (i == firstPtr && i == lastPtr)
                {
                    strb.Append("<color=#FF0000>first/last  -></color>\t");
                }
                else if (i == firstPtr)
                {
                    strb.Append("<color=#FF0000>first       -></color>\t");
                }
                else if (i == lastPtr)
                {
                    strb.Append("<color=#FF0000>last        -></color>\t");
                }
                else
                {
                    strb.Append("<color=#FF0000>--------------</color>\t");
                }


                strb.Append("<color=#AAAA00>【");
                strb.Append(i);
                strb.Append("】</color>");

                strb.Append("(prev:" + this.nodes[i].prev + ")");

                strb.Append("<b>:" + this.nodes[i].value.ToString() + "</b>");

                strb.Append("(next:" + this.nodes[i].next + ")");


                if (i == emptyFirstPtr && i == emptyLastPtr)
                {
                    strb.Append("<color=#00FF00>  ->  empty first/last</color>");
                }
                if (i == emptyFirstPtr)
                {
                    strb.Append("<color=#00FF00>  ->  empty first</color>");
                }
                else if (i == emptyLastPtr)
                {
                    strb.Append("<color=#00FF00>  ->   empty last</color>");
                }
            }

            Debug.Log(strb.ToString());
        }

    }
}