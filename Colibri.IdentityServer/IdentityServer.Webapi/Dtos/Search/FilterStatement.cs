﻿using ExpressionBuilder.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Webapi.Dtos.Search
{
    public class FilterStatement
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        //public OperationBase Operation { get; set; } // temporary
    }
}
