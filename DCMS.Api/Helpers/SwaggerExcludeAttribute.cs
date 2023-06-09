﻿using System;

namespace DCMS.Api.Helpers
{
    /// <summary>
    /// Properties with this attribute will be ignored by Swagger
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExcludeAttribute : Attribute
    {
    }
}
