using System;

namespace src
{
    public abstract class Either<T, U>
    {
        public static Either<T, U> Succeed(U value) =>
            new Right<T, U>() {
                Value = value
            };

        public static Either<T, U> Fail(T value) =>
            new Left<T, U>() {
                Value = value
            };

        public abstract Either<T, V> Select<V>(Func<U, V> f);

        public abstract Either<T, V> Then<V>(Func<U, Either<T, V>> f);

        public abstract V Extract<V>(Func<T, V> f, Func<U, V> g);

        private class Right<T, U> : Either<T, U>
        {
            public U Value;

            public override Either<T, V> Select<V>(Func<U, V> f) =>
                new Right<T, V>() {
                    Value = f(Value)
                };

            public override Either<T, V> Then<V>(Func<U, Either<T, V>> f) =>
                f(Value);

            public override V Extract<V>(Func<T, V> f, Func<U, V> g) =>
                g(Value);

        }

        private class Left<T, U> : Either<T, U>
        {
            public T Value;

            public override Either<T, V> Select<V>(Func<U, V> f) =>
                new Left<T, V>() {
                    Value = Value
                };

            public override Either<T, V> Then<V>(Func<U, Either<T, V>> f) =>
                new Left<T, V>() {
                    Value = Value
                };

            public override V Extract<V>(Func<T, V> f, Func<U, V> g) =>
                f(Value);

        }

    }

}
