﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // BookMementoCollectionChangedイベントの種類
    public enum BookMementoCollectionChangedType
    {
        Load,
        Clear,
        Add,
        Update,
        Remove,
    }

    // BookMementoCollectionChangedイベントの引数
    public class BookMementoCollectionChangedArgs
    {
        public BookMementoCollectionChangedType HistoryChangedType { get; set; }
        public string Key { get; set; }

        public BookMementoCollectionChangedArgs(BookMementoCollectionChangedType type, string key)
        {
            HistoryChangedType = type;
            Key = key;
        }
    }


    /// <summary>
    /// BookMementoUnit用ノード
    /// </summary>
    public class BookMementoUnitNode
    {
        public BookMementoUnit Value { get; set; }

        public BookMementoUnitNode(BookMementoUnit value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// 高速検索用BookMemento辞書
    /// 履歴、ブックマーク共有の辞書です
    /// </summary>
    public class BookMementoUnit
    {
        // 履歴用リンク
        public LinkedListNode<BookMementoUnit> HistoryNode { get; set; }

        // ブックマーク用リンク
        public BookMementoUnitNode BookmarkNode { get; set; }

        // 本体
        public Book.Memento Memento { get; set; }

        //
        public override string ToString()
        {
            return Memento?.Place ?? base.ToString();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class BookMementoCollection
    {
        public Dictionary<string, BookMementoUnit> Items { get; set; } = new Dictionary<string, BookMementoUnit>();

        //
        public void Add(BookMementoUnit unit)
        {
            Debug.Assert(unit != null);
            Debug.Assert(unit.Memento != null);
            Debug.Assert(unit.Memento.Place != null);
            Debug.Assert(unit.HistoryNode != null || unit.BookmarkNode != null);

            Items.Add(unit.Memento.Place, unit);
        }

        //
        public BookMementoUnit Find(string place)
        {
            BookMementoUnit unit;
            Items.TryGetValue(place, out unit);
            return unit;
        }
    }
}