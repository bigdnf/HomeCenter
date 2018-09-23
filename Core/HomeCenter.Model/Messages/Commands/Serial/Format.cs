﻿using System;

namespace HomeCenter.Model.Commands.Serial
{
    public class Format
    {
        public Type ValueType;
        public string ValueName;
        public int Lp;

        public Format(int lp, Type valueType, string valueName)
        {
            Lp = lp;
            ValueType = valueType;
            ValueName = valueName;
        }
    }
}