// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// 指向性ページ範囲
    /// </summary>
    public class PageDirectionalRange
    {
        #region Constructors

        public PageDirectionalRange()
        {
            this.Position = new PagePosition();
            this.Direction = 1;
            this.PartSize = 1;
        }

        public PageDirectionalRange(PagePosition position, int direction, int pageSize)
        {
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            this.Position = position;
            this.Direction = direction;

            var last = new PagePosition(position.Index + direction * (pageSize - 1), direction > 0 ? 1 : 0);
            this.PartSize = Math.Abs(last.Value - position.Value) + 1; 
        }

        public PageDirectionalRange(PagePosition p0, PagePosition p1)
        {
            this.Position = p0;
            this.Direction = p1 < p0 ? -1 : 1;
            this.PartSize = Math.Abs(p1.Value - p0.Value) + 1;
        }

        public PageDirectionalRange(IEnumerable<PagePart> parts, int direction)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            int count = 0;

            PagePosition p0 = new PagePosition();
            PagePosition p1 = new PagePosition();

            foreach (var part in parts)
            {
                if (part.PartSize <= 0) continue;

                var a0 = part.Position;
                var a1 = part.Position + part.PartSize - 1;

                if (count == 0)
                {
                    p0 = a0;
                    p1 = a1;
                }
                else
                {
                    if (a0.Value < p0.Value) p0 = a0;
                    if (a1.Value > p1.Value) p1 = a1;
                }

                count++;
            }

            this.Position = direction > 0 ? p0 : p1;
            this.Direction = direction;
            this.PartSize = Math.Abs(p1.Value - p0.Value) + 1;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 範囲開始
        /// </summary>
        public PagePosition Position { get; }

        /// <summary>
        /// 範囲終了
        /// </summary>
        public PagePosition Last => Position + Direction * (PartSize - 1);

        public PagePosition Min => Direction > 0 ? Position : Last;
        public PagePosition Max => Direction > 0 ? Last : Position;

        /// <summary>
        /// 方向
        /// </summary>
        public int Direction { get; }

        /// <summary>
        /// パーツサイズ
        /// </summary>
        public int PartSize { get; }

        /// <summary>
        /// ページサイズ
        /// </summary>
        public int PageSize => Position.Index - Last.Index + 1; // 未使用？

        #endregion

        #region Medhods

        //
        public override string ToString()
        {
            return $"{Position},{Last}";
        }

        //
        public bool IsContains(PagePosition position)
        {
            if (this.Direction > 0)
            {
                return this.Position <= position && position <= this.Last;
            }
            else
            {
                return this.Last <= position && position <= this.Position;
            }
        }

        //
        public PagePosition Next()
        {
            return Next(this.Direction);
        }

        //
        public PagePosition Next(int direction)
        {
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var pos = (this.Direction != direction) ? this.Position : this.Last;
            return pos + direction;
        }

        //
        public PagePosition Move(int delta)
        {
            int direction = delta < 0 ? -1 : 1;
            var pos = (this.Direction != direction) ? this.Last : this.Position;
            var position = new PagePosition(pos.Index + delta, direction > 0 ? 0 : 1);

            if (Math.Abs(delta) == 1)
            {
                var next = Next(direction);
                return Math.Abs(pos.Value - next.Value) < Math.Abs(pos.Value - position.Value) ? next : position;
            }
            else
            {
                return position;
            }
        }
        
        #endregion
    }
}
