using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    public class ByFieldValuesEqualityComparer<T>: IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return GetEqualityComponents(x).ToArray().SequenceEqual(GetEqualityComponents(y).ToArray());
        }

        public int GetHashCode(T obj)
        {
            unchecked
            {
                return GetEqualityComponents(obj).Aggregate(17, (currentHash, currentObj) => currentHash * 23 + (currentObj?.GetHashCode() ?? 0));
            }
        }

        private IEnumerable<object> GetEqualityComponents(object obj)
        {
            var type = obj.GetType();
            if (type.IsValueType || type.IsPrimitive || type == typeof(string))
            {
                yield return obj;
            }
            else
            {
                foreach (var value in TestHelper.GetProperties(obj).Select(property => property.GetValue(obj)))
                {
                    if (value == null)
                    {
                        yield return null;
                    }
                    else
                    {
                        var valueType = value.GetType();
                        if (valueType.IsAssignableFromGenericList())
                        {
                            foreach (var equalityComponent in ((IEnumerable) value).Cast<object>().SelectMany(GetEqualityComponents))
                            {
                                yield return equalityComponent;
                            }
                        }
                        else
                        {
                            foreach (var equalityComponent in GetEqualityComponents(value))
                            {
                                yield return equalityComponent;
                            }
                        }
                    }
                }
            }
        }
    }
}
