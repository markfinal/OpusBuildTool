﻿// <copyright file="TypeUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class TypeUtilities
    {
        public static void CheckTypeImplementsInterface(System.Type type, System.Type interfaceType)
        {
            if (!interfaceType.IsAssignableFrom(type))
            {
                throw new Exception("Type '{0}' does not implement the interface {1}", type.ToString(), interfaceType.ToString());
            }
        }

        public static void CheckTypeIsNotAbstract(System.Type type)
        {
            if (type.IsAbstract)
            {
                throw new Exception("Type '{0}' is abstract", type.ToString());
            }
        }
    }
}
