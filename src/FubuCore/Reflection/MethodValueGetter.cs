﻿using System;
using System.Reflection;

namespace FubuCore.Reflection
{
    public class MethodValueGetter : IValueGetter
    {
        private readonly MethodInfo _methodInfo;
        private readonly object _firstArgument;

        public MethodValueGetter(MethodInfo methodInfo, object firstArgument)
        {
            _methodInfo = methodInfo;
            _firstArgument = firstArgument;
        }

        public object GetValue(object target)
        {
            return _methodInfo.Invoke(target, new[] { _firstArgument });
        }

        public string Name
        {
            get { return "[{0}]".ToFormat(_firstArgument); }
        }

        public Type DeclaringType
        {
            get { return _methodInfo.DeclaringType; }
        }

        public bool Equals(MethodValueGetter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return _methodInfo.Equals(other._methodInfo) && _firstArgument == other._firstArgument;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MethodValueGetter)) return false;
            return Equals((MethodValueGetter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_methodInfo != null ? _methodInfo.GetHashCode() : 0) * 397) ^ (_firstArgument != null ? _firstArgument.GetHashCode() : 0);
            }
        }
    }
}