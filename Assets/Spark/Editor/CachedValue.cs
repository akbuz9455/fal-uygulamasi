using System;

namespace LeastSquares.Spark
{
    public class CachedValue<T>
    {
        private T _cache;
        private object[] _lastDependencies;
        
        public T Get(Func<T> generator, params object[] dependencies)
        {
            if (dependencies != _lastDependencies)
            {
                _cache = generator();
                _lastDependencies = dependencies;
            }

            return _cache;
        }
    }
}