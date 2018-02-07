// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;




namespace NeeView
{
    public class PlaceString
    {
        private string _path;

        public PlaceString(string path)
        {
            _path = path;
        }

        public override string ToString()
        {
            if (_path == null)
            {
                return "このPC";
            }
            else if (_path.StartsWith("http://") || _path.StartsWith("https://"))
            {
                return new Uri(_path).Host;
            }
            else if (_path.StartsWith("data:"))
            {
                return "HTML埋め込み画像";
            }
            else
            {
                return LoosePath.GetFileName(_path);
            }
        }
    }
}


